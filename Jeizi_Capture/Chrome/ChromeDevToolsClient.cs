using System.Collections.Concurrent;
using System.Drawing;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using AutoSnap.Models;

namespace AutoSnap.Chrome;

public sealed class ChromeDevToolsClient : IDisposable
{
    private readonly Uri _webSocketUri;
    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cts;
    private Task? _receiveTask;
    private int _requestIdCounter;
    private readonly ConcurrentDictionary<int, TaskCompletionSource<JsonElement>> _pendingRequests = new();
    private bool _disposed;

    public bool IsConnected => _webSocket != null && _webSocket.State == WebSocketState.Open;

    public event EventHandler<string>? Disconnected;
    public event EventHandler<(string Method, JsonElement Params)>? EventReceived;

    public ChromeDevToolsClient(string webSocketDebuggerUrl)
    {
        if (string.IsNullOrWhiteSpace(webSocketDebuggerUrl))
            throw new ArgumentException("WebSocket debugger URL cannot be empty.", nameof(webSocketDebuggerUrl));

        _webSocketUri = new Uri(webSocketDebuggerUrl);
    }

    public ChromeDevToolsClient(Uri webSocketUri)
    {
        _webSocketUri = webSocketUri ?? throw new ArgumentNullException(nameof(webSocketUri));
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (IsConnected)
            return;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        _webSocket?.Dispose();
        _webSocket = new ClientWebSocket();
        _webSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(15);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);
        await _webSocket.ConnectAsync(_webSocketUri, linkedCts.Token);

        _receiveTask = Task.Run(() => ReceiveLoopAsync(_cts.Token));

        // Enable Page domain to ensure screencapture events and lifecycle are initialized
        try
        {
            await SendCommandAsync("Page.enable", null, cancellationToken);
        }
        catch
        {
            // Ignore if Page domain is already active
        }
    }

    public async Task<JsonElement> SendCommandAsync(string method, object? parameters = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!IsConnected || _webSocket == null)
            throw new InvalidOperationException("Not connected to Chrome DevTools.");

        int id = Interlocked.Increment(ref _requestIdCounter);
        var tcs = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingRequests[id] = tcs;

        var requestObj = new Dictionary<string, object?>
        {
            ["id"] = id,
            ["method"] = method
        };

        if (parameters != null)
        {
            requestObj["params"] = parameters;
        }

        string json = JsonSerializer.Serialize(requestObj);
        byte[] buffer = Encoding.UTF8.GetBytes(json);

        try
        {
            using var reg = cancellationToken.Register(() =>
            {
                if (_pendingRequests.TryRemove(id, out var removedTcs))
                {
                    removedTcs.TrySetCanceled(cancellationToken);
                }
            });

            await _webSocket.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                true,
                cancellationToken);

            return await tcs.Task;
        }
        catch (Exception)
        {
            _pendingRequests.TryRemove(id, out _);
            throw;
        }
    }

    public async Task<Bitmap> CaptureScreenshotAsync(
        ImageFormatType format = ImageFormatType.Jpg,
        int jpegQuality = 90,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object>
        {
            ["format"] = format == ImageFormatType.Png ? "png" : "jpeg",
            ["fromSurface"] = true,
            ["captureBeyondViewport"] = false
        };

        if (format != ImageFormatType.Png)
        {
            parameters["quality"] = Math.Clamp(jpegQuality, 1, 100);
        }

        var result = await SendCommandAsync("Page.captureScreenshot", parameters, cancellationToken);

        if (!result.TryGetProperty("data", out var dataProp))
        {
            throw new InvalidOperationException("CDP screenshot response did not contain image data.");
        }

        string? base64 = dataProp.GetString();
        if (string.IsNullOrWhiteSpace(base64))
        {
            throw new InvalidOperationException("Received empty image data from Chrome.");
        }

        byte[] imageBytes = Convert.FromBase64String(base64);
        using var ms = new MemoryStream(imageBytes);
        using var tempImg = Image.FromStream(ms);
        return new Bitmap(tempImg);
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * 1024];
        using var ms = new MemoryStream();

        try
        {
            while (!cancellationToken.IsCancellationRequested && _webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                ms.SetLength(0);

                do
                {
                    result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                    ms.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await HandleDisconnectAsync("WebSocket closed by remote endpoint.");
                    break;
                }

                if (ms.Length > 0)
                {
                    ms.Position = 0;
                    ProcessReceivedMessage(ms.ToArray());
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on cancellation
        }
        catch (Exception ex)
        {
            await HandleDisconnectAsync(ex.Message);
        }
    }

    private void ProcessReceivedMessage(byte[] utf8Json)
    {
        try
        {
            using var doc = JsonDocument.Parse(utf8Json);
            var root = doc.RootElement;

            if (root.TryGetProperty("id", out var idProp) && idProp.TryGetInt32(out int id))
            {
                if (_pendingRequests.TryRemove(id, out var tcs))
                {
                    if (root.TryGetProperty("error", out var errorProp))
                    {
                        string message = errorProp.TryGetProperty("message", out var msgProp) ? msgProp.GetString() ?? "Unknown error" : "Unknown error";
                        int code = errorProp.TryGetProperty("code", out var codeProp) && codeProp.TryGetInt32(out int c) ? c : -1;
                        tcs.TrySetException(new InvalidOperationException($"CDP Error ({code}): {message}"));
                    }
                    else if (root.TryGetProperty("result", out var resultProp))
                    {
                        tcs.TrySetResult(resultProp.Clone());
                    }
                    else
                    {
                        tcs.TrySetResult(root.Clone());
                    }
                }
            }
            else if (root.TryGetProperty("method", out var methodProp))
            {
                string method = methodProp.GetString() ?? string.Empty;
                JsonElement paramsEl = root.TryGetProperty("params", out var p) ? p.Clone() : default;
                EventReceived?.Invoke(this, (method, paramsEl));
            }
        }
        catch
        {
            // Ignore malformed messages
        }
    }

    private async Task HandleDisconnectAsync(string reason)
    {
        foreach (var kvp in _pendingRequests)
        {
            kvp.Value.TrySetException(new InvalidOperationException($"Connection closed: {reason}"));
        }
        _pendingRequests.Clear();

        try
        {
            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                await _webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
        }
        catch
        {
            // Ignore close errors
        }

        Disconnected?.Invoke(this, reason);
    }

    public async Task DisconnectAsync()
    {
        if (_cts != null)
        {
            _cts.Cancel();
        }

        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            try
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", CancellationToken.None);
            }
            catch
            {
                // Ignore disconnect errors
            }
        }

        foreach (var kvp in _pendingRequests)
        {
            kvp.Value.TrySetCanceled();
        }
        _pendingRequests.Clear();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _cts?.Cancel();
            _cts?.Dispose();
            _webSocket?.Dispose();

            foreach (var kvp in _pendingRequests)
            {
                kvp.Value.TrySetCanceled();
            }
            _pendingRequests.Clear();
        }
    }
}
