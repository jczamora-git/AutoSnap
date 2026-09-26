using AutoSnap.Capture;
using AutoSnap.Models;

namespace AutoSnap.Services;

public class SnapshotService
{
    private readonly StorageService _storageService;

    public SnapshotService(StorageService storageService)
    {
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    }

    public async Task<SnapshotRecord?> ProcessAndSaveFrameAsync(
        CapturedFrame? frame,
        CaptureSettings settings,
        CancellationToken cancellationToken = default)
    {
        if (frame == null)
            return null;

        try
        {
            return await _storageService.SaveSnapshotAsync(frame, settings, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }
}
