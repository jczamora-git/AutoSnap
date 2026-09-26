using AutoSnap.Video;
using NAudio.Wave;

namespace AutoSnap.Audio;

public class MediaFileAudioSource : IAudioSource
{
    private readonly string _filePath;
    private readonly FFmpegService _ffmpegService;
    private string? _tempWavPath;
    private bool _isCapturing;
    private bool _disposed;
    private readonly object _lock = new();

    public event EventHandler<AudioChunk>? AudioDataAvailable;
    public event EventHandler<Exception>? ErrorOccurred;
    public event EventHandler? Stopped;

    public string SourceName => Path.GetFileName(_filePath);
    public string FilePath => _filePath;
    public bool IsCapturing => _isCapturing;
    public WaveFormat Format { get; } = new WaveFormat(16000, 16, 1);

    public MediaFileAudioSource(string filePath, FFmpegService ffmpegService)
    {
        _filePath = filePath;
        _ffmpegService = ffmpegService;
    }

    public async Task<string> PrepareAudioWavAsync(CancellationToken cancellationToken = default)
    {
        string tempDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AutoSnap", "Temp");
        Directory.CreateDirectory(tempDir);

        _tempWavPath = Path.Combine(tempDir, $"audio_{Guid.NewGuid():N}.wav");

        await _ffmpegService.ExtractAudioAsync(_filePath, _tempWavPath, cancellationToken);
        return _tempWavPath;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_isCapturing) return;
            _isCapturing = true;
        }

        try
        {
            if (string.IsNullOrEmpty(_tempWavPath) || !File.Exists(_tempWavPath))
            {
                await PrepareAudioWavAsync(cancellationToken);
            }

            // Stream chunks from wav file
            await Task.Run(() =>
            {
                using var reader = new WaveFileReader(_tempWavPath!);
                byte[] buffer = new byte[32000]; // 1 second chunks
                int bytesRead;
                TimeSpan currentTime = TimeSpan.Zero;

                while (_isCapturing && (bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    byte[] chunkBytes = new byte[bytesRead];
                    Array.Copy(buffer, 0, chunkBytes, 0, bytesRead);

                    float rms = AudioLevelAnalyzer.CalculateRms16BitMono(chunkBytes, 0, bytesRead);

                    var chunk = new AudioChunk
                    {
                        Data = chunkBytes,
                        Length = bytesRead,
                        Timestamp = currentTime,
                        Duration = TimeSpan.FromSeconds((double)bytesRead / 32000),
                        Format = Format,
                        PeakRms = rms,
                        IsSilent = AudioLevelAnalyzer.IsSilent(rms)
                    };

                    currentTime += chunk.Duration;
                    AudioDataAvailable?.Invoke(this, chunk);
                }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
        }
        finally
        {
            lock (_lock)
            {
                _isCapturing = false;
            }
            Stopped?.Invoke(this, EventArgs.Empty);
        }
    }

    public Task StopAsync()
    {
        lock (_lock)
        {
            _isCapturing = false;
        }
        return Task.CompletedTask;
    }

    public void CleanupTempFile()
    {
        try
        {
            if (!string.IsNullOrEmpty(_tempWavPath) && File.Exists(_tempWavPath))
            {
                File.Delete(_tempWavPath);
                _tempWavPath = null;
            }
        }
        catch { }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            StopAsync().GetAwaiter().GetResult();
            CleanupTempFile();
        }
    }
}
