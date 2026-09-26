using NAudio.Wave;

namespace AutoSnap.Audio;

public class BrowserAudioSource : IAudioSource
{
    private bool _isCapturing;
    private bool _disposed;
    private readonly object _lock = new();

    public event EventHandler<AudioChunk>? AudioDataAvailable;
    public event EventHandler<Exception>? ErrorOccurred;
    public event EventHandler? Stopped;

    public string SourceName => "Browser Tab Audio";
    public bool IsCapturing => _isCapturing;
    public WaveFormat Format { get; } = new WaveFormat(16000, 16, 1);

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _isCapturing = true;
        }
        return Task.CompletedTask;
    }

    public void PushPcmData(byte[] pcmData, int offset, int count)
    {
        if (!_isCapturing || count <= 0) return;

        try
        {
            byte[] chunkData = new byte[count];
            Array.Copy(pcmData, offset, chunkData, 0, count);

            float rms = AudioLevelAnalyzer.CalculateRms16BitMono(chunkData, 0, count);

            var chunk = new AudioChunk
            {
                Data = chunkData,
                Length = count,
                Timestamp = DateTime.Now.TimeOfDay,
                Duration = TimeSpan.FromSeconds((double)count / 32000),
                Format = Format,
                PeakRms = rms,
                IsSilent = AudioLevelAnalyzer.IsSilent(rms)
            };

            AudioDataAvailable?.Invoke(this, chunk);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
        }
    }

    public Task StopAsync()
    {
        lock (_lock)
        {
            if (_isCapturing)
            {
                _isCapturing = false;
                Stopped?.Invoke(this, EventArgs.Empty);
            }
        }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            StopAsync().GetAwaiter().GetResult();
        }
    }
}
