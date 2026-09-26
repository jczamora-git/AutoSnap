namespace AutoSnap.Models;

public class VideoSettings
{
    public bool UseCustomFFmpeg { get; set; } = false;
    public string? CustomFFmpegPath { get; set; }
    public string? CustomFFprobePath { get; set; }
    public TimeSpan DefaultSnapshotInterval { get; set; } = TimeSpan.FromMinutes(1);
    public ImageFormatType DefaultFormat { get; set; } = ImageFormatType.Jpg;
    public int DefaultJpegQuality { get; set; } = 90;
}
