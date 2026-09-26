using System.Drawing;
using AutoSnap.Capture;
using AutoSnap.Models;

namespace AutoSnap.BrowserCapture;

public class BrowserCaptureSource : ICaptureSource
{
    private readonly BrowserCaptureServer _server;
    private string _streamTitle = "Chrome Tab";
    private bool _disposed;

    public CaptureSourceType SourceType => CaptureSourceType.ChromeTab;

    public string DisplayName => string.IsNullOrWhiteSpace(_streamTitle) || _streamTitle == "Chrome Tab"
        ? "Chrome Tab"
        : $"{_streamTitle} (Chrome Tab)";

    public bool IsAvailable => _server.HasActiveStream;

    public string StatusDescription
    {
        get
        {
            if (!_server.IsRunning)
                return "Capture server is not running.";
            if (_server.ActiveSession == null || !_server.ActiveSession.IsConnected)
                return "Waiting for capture page connection...";
            if (!_server.HasActiveStream)
                return "Waiting for user to choose Chrome tab...";
            return $"Streaming: {_streamTitle}";
        }
    }

    public string StreamTitle => _streamTitle;

    public event EventHandler? StreamUpdated;

    public BrowserCaptureSource(BrowserCaptureServer server)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _server.StreamStarted += Server_StreamStarted;
        _server.StreamEnded += Server_StreamEnded;
        _server.SessionDisconnected += Server_SessionDisconnected;

        if (_server.HasActiveStream && _server.ActiveSession != null)
        {
            _streamTitle = _server.ActiveSession.StreamTitle;
        }
    }

    private void Server_StreamStarted(object? sender, (string Title, int Width, int Height) e)
    {
        _streamTitle = e.Title;
        StreamUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void Server_StreamEnded(object? sender, EventArgs e)
    {
        _streamTitle = "Chrome Tab";
        StreamUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void Server_SessionDisconnected(object? sender, EventArgs e)
    {
        _streamTitle = "Chrome Tab";
        StreamUpdated?.Invoke(this, EventArgs.Empty);
    }

    public async Task<CapturedFrame?> CaptureAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_server.HasActiveStream)
        {
            throw new InvalidOperationException("No Chrome tab is currently being shared.");
        }

        try
        {
            byte[] rawBytes = await _server.RequestFrameAsync(
                ImageFormatType.Jpg,
                90,
                cancellationToken);

            using var ms = new MemoryStream(rawBytes);
            using var tempImg = Image.FromStream(ms);
            var bmp = new Bitmap(tempImg);

            string sourceName = string.IsNullOrWhiteSpace(_streamTitle) ? "ChromeTab" : _streamTitle;
            return new CapturedFrame(bmp, sourceName);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException($"Failed to capture frame from Chrome tab: {ex.Message}", ex);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _server.StreamStarted -= Server_StreamStarted;
            _server.StreamEnded -= Server_StreamEnded;
            _server.SessionDisconnected -= Server_SessionDisconnected;
            GC.SuppressFinalize(this);
        }
    }
}
