namespace AutoSnap.Models;

public enum CaptureSessionState
{
    Idle,
    Starting,
    Capturing,
    Paused,
    Stopping,
    ProcessingVideo,
    Error
}
