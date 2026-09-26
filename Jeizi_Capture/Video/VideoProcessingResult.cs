namespace AutoSnap.Video;

public class VideoProcessingResult
{
    public bool Success { get; set; }
    public int SnapshotCount { get; set; }
    public string? SnapshotsDirectory { get; set; }
    public string? TranscriptPath { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan ProcessingDuration { get; set; }
}
