using System.Drawing;

namespace AutoSnap.Models;

public class SnapshotRecord
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public Size Resolution { get; set; }
}
