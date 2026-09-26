using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using AutoSnap.Chrome;
using AutoSnap.Models;

namespace AutoSnap.BrowserCapture;

public class BrowserCaptureServer : IDisposable
{
    private HttpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _listenerTask;
    private BrowserCaptureSession? _activeSession;
    private readonly object _lock = new();
    private bool _disposed;

    public int Port { get; private set; }
    public string Url => $"http://127.0.0.1:{Port}/";
    public bool IsRunning => _listener != null && _listener.IsListening;
    public bool HasActiveStream => _activeSession != null && _activeSession.HasActiveStream;
    public bool HasAudioTrack => _activeSession != null && _activeSession.HasAudioTrack;
    public BrowserCaptureSession? ActiveSession => _activeSession;

    public event EventHandler<BrowserCaptureSession>? SessionConnected;
    public event EventHandler? SessionDisconnected;
    public event EventHandler<(string Title, int Width, int Height)>? StreamStarted;
    public event EventHandler? StreamEnded;
    public event EventHandler<bool>? AudioTrackStatusChanged;
    public event EventHandler<byte[]>? AudioDataReceived;

    private readonly SemaphoreSlim _serverLock = new(1, 1);

    public void Start(int preferredPort = 0)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _serverLock.Wait();
        try
        {
            if (IsRunning)
                return;

            Port = preferredPort > 0 ? preferredPort : FindAvailablePort();

            if (_cts != null || _listenerTask != null)
            {
                var oldCts = Interlocked.Exchange(ref _cts, null);
                var oldTask = Interlocked.Exchange(ref _listenerTask, null);
                if (oldCts != null)
                {
                    try
                    {
                        oldCts.Cancel();
                        oldTask?.GetAwaiter().GetResult();
                    }
                    catch { }
                    finally
                    {
                        oldCts.Dispose();
                    }
                }
            }

            var cts = new CancellationTokenSource();
            _cts = cts;

            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://127.0.0.1:{Port}/");
            _listener.Start();

            Debug.WriteLine($"[BrowserCaptureServer] Server started on port {Port}");
            _listenerTask = Task.Run(() => ListenerLoopAsync(cts.Token), cts.Token);
        }
        finally
        {
            _serverLock.Release();
        }
    }

    public static int FindAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private async Task ListenerLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _listener != null && _listener.IsListening)
        {
            try
            {
                var context = await _listener.GetContextAsync().ConfigureAwait(false);
                _ = HandleRequestAsync(context);
            }
            catch (HttpListenerException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // Continue listening
            }
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        try
        {
            if (context.Request.IsWebSocketRequest && context.Request.Url?.AbsolutePath == "/ws")
            {
                var wsContext = await context.AcceptWebSocketAsync(null);
                var session = new BrowserCaptureSession(wsContext.WebSocket);

                lock (_lock)
                {
                    _activeSession?.Dispose();
                    _activeSession = session;
                }

                session.StreamStarted += (s, e) => StreamStarted?.Invoke(this, e);
                session.StreamEnded += (s, e) => StreamEnded?.Invoke(this, EventArgs.Empty);
                session.AudioTrackStatusChanged += (s, e) => AudioTrackStatusChanged?.Invoke(this, e);
                session.AudioDataReceived += (s, e) => AudioDataReceived?.Invoke(this, e);
                session.Disconnected += (s, e) =>
                {
                    lock (_lock)
                    {
                        if (_activeSession == session)
                        {
                            _activeSession = null;
                        }
                    }
                    SessionDisconnected?.Invoke(this, EventArgs.Empty);
                };

                session.Start();
                SessionConnected?.Invoke(this, session);
                return;
            }

            // HTTP GET request for the capture page
            string html = BrowserCapturePage.GetHtml();
            byte[] bytes = Encoding.UTF8.GetBytes(html);

            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = bytes.Length;
            context.Response.StatusCode = 200;

            await context.Response.OutputStream.WriteAsync(bytes);
            context.Response.OutputStream.Close();
        }
        catch
        {
            try
            {
                context.Response.StatusCode = 500;
                context.Response.Close();
            }
            catch
            {
                // Ignore failure on error response
            }
        }
    }

    public void OpenCapturePageInBrowser(string? customChromeExePath = null)
    {
        if (!IsRunning)
        {
            Start();
        }

        string? chromeExe = ChromeService.FindChromeExecutable(customChromeExePath);

        try
        {
            if (!string.IsNullOrWhiteSpace(chromeExe) && File.Exists(chromeExe))
            {
                // Open new tab in user's existing Chrome session
                Process.Start(new ProcessStartInfo
                {
                    FileName = chromeExe,
                    Arguments = $"\"{Url}\"",
                    UseShellExecute = false
                });
            }
            else
            {
                // Fallback to default browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = Url,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Could not open capture page in browser: {ex.Message}", ex);
        }
    }

    public async Task<byte[]> RequestFrameAsync(
        ImageFormatType format = ImageFormatType.Jpg,
        int jpegQuality = 90,
        CancellationToken cancellationToken = default)
    {
        BrowserCaptureSession? session;
        lock (_lock)
        {
            session = _activeSession;
        }

        if (session == null || !session.IsConnected)
        {
            throw new InvalidOperationException("No browser capture tab is currently connected.");
        }

        if (!session.HasActiveStream)
        {
            throw new InvalidOperationException("Waiting for user to select a Chrome tab.");
        }

        return await session.RequestFrameAsync(format, jpegQuality, cancellationToken);
    }

    public async Task StopStreamAsync(CancellationToken cancellationToken = default)
    {
        BrowserCaptureSession? session;
        lock (_lock)
        {
            session = _activeSession;
        }

        if (session != null)
        {
            await session.StopStreamAsync(cancellationToken);
        }
    }

    public void Stop()
    {
        _serverLock.Wait();
        try
        {
            var cts = Interlocked.Exchange(ref _cts, null);
            var task = Interlocked.Exchange(ref _listenerTask, null);

            Debug.WriteLine("[BrowserCaptureServer] Stop requested");

            if (_listener != null)
            {
                try
                {
                    _listener.Stop();
                    _listener.Close();
                }
                catch { }
                _listener = null;
            }

            if (cts != null)
            {
                try
                {
                    cts.Cancel();
                    task?.GetAwaiter().GetResult();
                }
                catch { }
                finally
                {
                    cts.Dispose();
                    Debug.WriteLine("[BrowserCaptureServer] CTS disposed");
                }
            }

            lock (_lock)
            {
                _activeSession?.Dispose();
                _activeSession = null;
            }
        }
        finally
        {
            _serverLock.Release();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            Stop();
            _serverLock.Dispose();
        }
    }
}
