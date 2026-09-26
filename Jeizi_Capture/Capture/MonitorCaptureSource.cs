using System.Drawing;
using System.Drawing.Imaging;
using AutoSnap.Models;

namespace AutoSnap.Capture;

public class MonitorCaptureSource : ICaptureSource
{
    public MonitorInfo Monitor { get; }
    public CaptureSourceType SourceType => CaptureSourceType.Monitor;

    public string DisplayName => $"{Monitor.DisplayName} ({Monitor.Bounds.Width} × {Monitor.Bounds.Height})";

    public bool IsAvailable => Screen.AllScreens.Any(s => s.DeviceName == Monitor.DeviceName);

    public string StatusDescription => IsAvailable ? "Ready" : "Selected display is no longer connected.";

    public MonitorCaptureSource(MonitorInfo monitor)
    {
        Monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
    }

    public Task<CapturedFrame?> CaptureAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Refresh current screen coordinates in case display setup shifted
        var currentScreen = Screen.AllScreens.FirstOrDefault(s => s.DeviceName == Monitor.DeviceName)
                            ?? (Monitor.Index < Screen.AllScreens.Length ? Screen.AllScreens[Monitor.Index] : null);

        if (currentScreen == null)
        {
            throw new InvalidOperationException("Selected display is no longer available.");
        }

        Rectangle bounds = currentScreen.Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return Task.FromResult<CapturedFrame?>(null);
        }

        Bitmap bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
        try
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                // CopyFromScreen handles negative coordinates accurately for multi-monitors
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

            string sourceName = $"Display_{Monitor.Index + 1}";
            return Task.FromResult<CapturedFrame?>(new CapturedFrame(bmp, sourceName));
        }
        catch
        {
            bmp.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
