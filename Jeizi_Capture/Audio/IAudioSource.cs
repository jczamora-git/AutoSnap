using NAudio.Wave;

namespace AutoSnap.Audio;

public interface IAudioSource : IDisposable
{
    event EventHandler<AudioChunk>? AudioDataAvailable;
    event EventHandler<Exception>? ErrorOccurred;
    event EventHandler? Stopped;

    string SourceName { get; }
    bool IsCapturing { get; }
    WaveFormat Format { get; }

    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync();
}
