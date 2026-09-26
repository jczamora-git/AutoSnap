using AutoSnap.Transcription;

namespace AutoSnap.Models;

public class TranscriptionSettings
{
    public TranscriptionLanguageMode DefaultLanguageMode { get; set; } = TranscriptionLanguageMode.Taglish;
    public string DefaultModelName { get; set; } = "Small";
    public string? CustomModelsDirectory { get; set; }
    public int ChunkDurationSeconds { get; set; } = 15;
    public int OverlapDurationSeconds { get; set; } = 2;
    public bool EnableAutosave { get; set; } = true;
    public int AutosaveIntervalSeconds { get; set; } = 30;
    public bool KeepCapturedAudio { get; set; } = false;
}
