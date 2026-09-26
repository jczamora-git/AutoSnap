namespace AutoSnap.Transcription;

public enum TranscriptionState
{
    Idle,
    Preparing,
    Listening,
    Processing,
    Paused,
    Finalizing,
    Completed,
    Cancelled,
    Error
}
