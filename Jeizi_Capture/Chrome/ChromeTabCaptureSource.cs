using System.Drawing;
using System.Text.Json;
using AutoSnap.Capture;
using AutoSnap.Models;

namespace AutoSnap.Chrome;

public class ChromeTabCaptureSource : ICaptureSource
{
    private readonly ChromeDevToolsClient _client;
    private bool _isAvailable = true;
    private string _statusDescription = "Ready";
    private bool _disposed;

    public ChromeTabInfo TabInfo { get; private set; }
    public CaptureSourceType SourceType => CaptureSourceType.ChromeTab;

    public string DisplayName => string.IsNullOrWhiteSpace(TabInfo.Title)
        ? (string.IsNullOrWhiteSpace(TabInfo.Url) ? "Chrome Tab" : TabInfo.Url)
        : TabInfo.Title;

    public bool IsAvailable => _isAvailable && _client.IsConnected;

    public string StatusDescription => _statusDescription;

    public ChromeTabCaptureSource(ChromeTabInfo tabInfo)
    {
        TabInfo = tabInfo ?? throw new ArgumentNullException(nameof(tabInfo));

        if (string.IsNullOrWhiteSpace(tabInfo.WebSocketDebuggerUrl))
        {
            throw new ArgumentException("Tab does not have a valid WebSocketDebuggerUrl.", nameof(tabInfo));
        }

        _client = new ChromeDevToolsClient(tabInfo.WebSocketDebuggerUrl);
        _client.Disconnected += Client_Disconnected;
        _client.EventReceived += Client_EventReceived;
    }

    public async Task EnsureConnectedAsync(CancellationToken cancellationToken = default)
    {
        if (!_client.IsConnected)
        {
            try
            {
                await _client.ConnectAsync(cancellationToken);
                _isAvailable = true;
                _statusDescription = "Ready";
            }
            catch (Exception ex)
            {
                _isAvailable = false;
                _statusDescription = $"Failed to connect to Chrome tab: {ex.Message}";
                throw;
            }
        }
    }

    public async Task<CapturedFrame?> CaptureAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_isAvailable)
        {
            throw new InvalidOperationException("The selected Chrome tab is no longer available.");
        }

        try
        {
            if (!_client.IsConnected)
            {
                await EnsureConnectedAsync(cancellationToken);
            }

            var bitmap = await _client.CaptureScreenshotAsync(
                ImageFormatType.Jpg,
                90,
                cancellationToken);

            string sourceName = string.IsNullOrWhiteSpace(TabInfo.Title) ? "ChromeTab" : TabInfo.Title;
            return new CapturedFrame(bitmap, sourceName);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _isAvailable = false;
            _statusDescription = "The selected Chrome tab is no longer available.";
            throw new InvalidOperationException("The selected Chrome tab is no longer available.", ex);
        }
    }

    private void Client_Disconnected(object? sender, string reason)
    {
        _isAvailable = false;
        _statusDescription = "The selected Chrome tab is no longer available.";
    }

    private void Client_EventReceived(object? sender, (string Method, JsonElement Params) e)
    {
        if (e.Method == "Page.frameNavigated" || e.Method == "Page.navigatedWithinDocument")
        {
            try
            {
                if (e.Params.TryGetProperty("frame", out var frameProp) && frameProp.TryGetProperty("url", out var urlProp))
                {
                    string? newUrl = urlProp.GetString();
                    if (!string.IsNullOrWhiteSpace(newUrl))
                    {
                        TabInfo = new ChromeTabInfo
                        {
                            Id = TabInfo.Id,
                            Title = TabInfo.Title,
                            Url = newUrl,
                            Type = TabInfo.Type,
                            WebSocketDebuggerUrl = TabInfo.WebSocketDebuggerUrl,
                            FaviconUrl = TabInfo.FaviconUrl
                        };
                    }
                }
            }
            catch
            {
                // Ignore parsing errors for navigation events
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _client.Disconnected -= Client_Disconnected;
            _client.EventReceived -= Client_EventReceived;
            _client.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
