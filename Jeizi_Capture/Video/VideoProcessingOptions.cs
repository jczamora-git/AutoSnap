using AutoSnap.Models;
using AutoSnap.Transcription;

namespace AutoSnap.Video;

public class VideoProcessingOptions
{
    public bool ExtractSnapshots { get; set; } = true;
    public TimeSpan SnapshotInterval { get; set; } = TimeSpan.FromMinutes(1);
    public ImageFormatType Format { get; set; } = ImageFormatType.Jpg;
    public int JpegQuality { get; set; } = 90;

    public bool GenerateTranscript { get; set; } = false;
    public TranscriptionLanguageMode LanguageMode { get; set; } = TranscriptionLanguageMode.Taglish;
    public string WhisperModelType { get; set; } = "Small";

    public string? OutputBaseDirectory { get; set; }
}
