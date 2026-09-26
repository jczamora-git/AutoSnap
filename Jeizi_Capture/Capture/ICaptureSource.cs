using AutoSnap.Models;

namespace AutoSnap.Capture;

public interface ICaptureSource : IDisposable
{
    string DisplayName { get; }
    CaptureSourceType SourceType { get; }
    bool IsAvailable { get; }
    string StatusDescription { get; }

    Task<CapturedFrame?> CaptureAsync(CancellationToken cancellationToken = default);
}
