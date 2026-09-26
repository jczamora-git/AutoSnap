namespace AutoSnap.Video;

public class VideoInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName => Path.GetFileName(FilePath);
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;
    public int Width { get; set; }
    public int Height { get; set; }
    public double FrameRate { get; set; }
    public string Codec { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public bool HasAudio { get; set; }
    public string AudioCodec { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }

    public string FormattedDuration => Duration.ToString(Duration.TotalHours >= 1 ? @"hh\:mm\:ss" : @"mm\:ss");
    public string ResolutionString => Width > 0 && Height > 0 ? $"{Width} × {Height}" : "Unknown";
    public string FrameRateString => FrameRate > 0 ? $"{FrameRate:0.##} FPS" : "Unknown";
}
