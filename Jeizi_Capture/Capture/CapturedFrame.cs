using System.Drawing;

namespace AutoSnap.Capture;

public sealed class CapturedFrame : IDisposable
{
    private bool _disposed;

    public Image Image { get; }
    public DateTime Timestamp { get; }
    public string SourceName { get; }
    public Size Resolution => Image?.Size ?? Size.Empty;

    public CapturedFrame(Image image, string sourceName, DateTime? timestamp = null)
    {
        Image = image ?? throw new ArgumentNullException(nameof(image));
        SourceName = string.IsNullOrWhiteSpace(sourceName) ? "Snapshot" : sourceName;
        Timestamp = timestamp ?? DateTime.Now;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Image.Dispose();
            _disposed = true;
        }
    }
}
