using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using AutoSnap.Models;

namespace AutoSnap.BrowserCapture;

public class BrowserCaptureSession : IDisposable
{
    private readonly WebSocket _webSocket;
    private readonly CancellationTokenSource _cts = new();
    private Task? _receiveTask;
    private readonly ConcurrentDictionary<int, TaskCompletionSource<byte[]>> _pendingFrameRequests = new();
    private int _requestIdCounter;
    private bool _disposed;

    public bool IsConnected => _webSocket.State == WebSocketState.Open;
    public bool HasActiveStream { get; private set; }
    public string StreamTitle { get; private set; } = "Chrome Tab";
    public int StreamWidth { get; private set; }
    public int StreamHeight { get; private set; }

    public event EventHandler<(string Title, int Width, int Height)>? StreamStarted;
    public event EventHandler? StreamEnded;
    public event EventHandler? Disconnected;

    public BrowserCaptureSession(WebSocket webSocket)
    {
        _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
    }

    public void Start()
    {
        _receiveTask = Task.Run(() => ReceiveLoopAsync(_cts.Token));
    }

    public async Task<byte[]> RequestFrameAsync(
        ImageFormatType format = ImageFormatType.Jpg,
        int jpegQuality = 90,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!IsConnected)
        {
            throw new InvalidOperationException("Browser capture session is not connected.");
        }

        if (!HasActiveStream)
        {
            throw new InvalidOperationException("No active Chrome tab media stream available.");
        }

        int requestId = Interlocked.Increment(ref _requestIdCounter);
        var tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingFrameRequests[requestId] = tcs;

        var requestMsg = new BrowserCaptureMessage
        {
            Type = "requestFrame",
            RequestId = requestId,
            Format = format == ImageFormatType.Png ? "png" : "jpeg",
            Quality = Math.Clamp(jpegQuality / 100.0, 0.1, 1.0)
        };

        string json = JsonSerializer.Serialize(requestMsg);
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        try
        {
            using var reg = cancellationToken.Register(() =>
            {
                if (_pendingFrameRequests.TryRemove(requestId, out var removedTcs))
                {
                    removedTcs.TrySetCanceled(cancellationToken);
                }
            });

            await _webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                cancellationToken);

            return await tcs.Task;
        }
        catch
        {
            _pendingFrameRequests.TryRemove(requestId, out _);
            throw;
        }
    }

    public async Task StopStreamAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected)
        {
            var msg = new BrowserCaptureMessage { Type = "stopStream" };
            string json = JsonSerializer.Serialize(msg);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            try
            {
                await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
            }
            catch
            {
                // Ignore send error on closing
            }
        }
        HasActiveStream = false;
        StreamEnded?.Invoke(this, EventArgs.Empty);
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * 1024];
        using var ms = new MemoryStream();

        try
        {
            while (!cancellationToken.IsCancellationRequested && _webSocket.State == WebSocketState.Open)
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
                    break;
                }

                if (ms.Length > 0)
                {
                    ms.Position = 0;
                    string json = Encoding.UTF8.GetString(ms.ToArray());
                    ProcessMessage(json);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on cancellation
        }
        catch
        {
            // Disconnection or socket failure
        }
        finally
        {
            HandleDisconnect();
        }
    }

    private void ProcessMessage(string json)
    {
        try
        {
            var msg = JsonSerializer.Deserialize<BrowserCaptureMessage>(json);
            if (msg == null) return;

            switch (msg.Type)
            {
                case "streamStarted":
                    HasActiveStream = true;
                    StreamTitle = string.IsNullOrWhiteSpace(msg.Title) ? "Chrome Tab" : msg.Title;
                    StreamWidth = msg.Width ?? 1280;
                    StreamHeight = msg.Height ?? 720;
                    StreamStarted?.Invoke(this, (StreamTitle, StreamWidth, StreamHeight));
                    break;

                case "streamEnded":
                    HasActiveStream = false;
                    StreamEnded?.Invoke(this, EventArgs.Empty);
                    break;

                case "frameData":
                    if (msg.RequestId.HasValue && _pendingFrameRequests.TryRemove(msg.RequestId.Value, out var tcs))
                    {
                        if (!string.IsNullOrWhiteSpace(msg.Data))
                        {
                            byte[] rawBytes = Convert.FromBase64String(msg.Data);
                            tcs.TrySetResult(rawBytes);
                        }
                        else
                        {
                            tcs.TrySetException(new InvalidOperationException("Received empty frame data from browser."));
                        }
                    }
                    break;

                case "frameError":
                    if (msg.RequestId.HasValue && _pendingFrameRequests.TryRemove(msg.RequestId.Value, out var errorTcs))
                    {
                        errorTcs.TrySetException(new InvalidOperationException(msg.Message ?? "Failed to capture frame in browser."));
                    }
                    break;
            }
        }
        catch
        {
            // Ignore malformed message
        }
    }

    private void HandleDisconnect()
    {
        HasActiveStream = false;
        foreach (var kvp in _pendingFrameRequests)
        {
            kvp.Value.TrySetException(new InvalidOperationException("Browser capture session disconnected."));
        }
        _pendingFrameRequests.Clear();
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _cts.Cancel();
            _cts.Dispose();
            _webSocket.Dispose();

            foreach (var kvp in _pendingFrameRequests)
            {
                kvp.Value.TrySetCanceled();
            }
            _pendingFrameRequests.Clear();
        }
    }
}
