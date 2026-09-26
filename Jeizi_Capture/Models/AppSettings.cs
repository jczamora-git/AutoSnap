namespace AutoSnap.Models;

public class AppSettings
{
    public CaptureSettings Capture { get; set; } = new();
    public ChromeSettings Chrome { get; set; } = new();
    public VideoSettings Video { get; set; } = new();
    public TranscriptionSettings Transcription { get; set; } = new();
    public bool MinimizeToTray { get; set; } = true;
    public bool StartMinimized { get; set; } = false;
    public bool ContinueCapturingWhenMinimized { get; set; } = true;
    public string? CustomFfmpegPath { get; set; }
    public bool EnableStorageManagement { get; set; } = false;
    public long MaxStorageSizeMb { get; set; } = 5000;
    public int DeleteOlderThanDays { get; set; } = 30;
}
