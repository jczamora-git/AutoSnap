using NAudio.Wave;

namespace AutoSnap.Audio;

public class AudioBuffer
{
    private readonly MemoryStream _stream = new();
    private readonly object _lock = new();
    private readonly TimeSpan _chunkDuration;
    private readonly TimeSpan _overlapDuration;
    private readonly WaveFormat _format = new(16000, 16, 1);
    private TimeSpan _currentStreamTime = TimeSpan.Zero;

    private readonly int _bytesPerSecond;
    private readonly int _chunkBytes;
    private readonly int _stepBytes;

    public AudioBuffer(TimeSpan? chunkDuration = null, TimeSpan? overlapDuration = null)
    {
        _chunkDuration = chunkDuration ?? TimeSpan.FromSeconds(15);
        _overlapDuration = overlapDuration ?? TimeSpan.FromSeconds(2);

        _bytesPerSecond = _format.AverageBytesPerSecond; // 32000
        _chunkBytes = (int)(_chunkDuration.TotalSeconds * _bytesPerSecond);
        int overlapBytes = (int)(_overlapDuration.TotalSeconds * _bytesPerSecond);
        _stepBytes = Math.Max(_bytesPerSecond, _chunkBytes - overlapBytes);
    }

    public void AddAudio(byte[] data, int offset, int count)
    {
        if (count <= 0) return;
        lock (_lock)
        {
            _stream.Write(data, offset, count);
        }
    }

    public List<AudioChunk> ExtractReadyChunks(bool isFinalizing = false)
    {
        var readyChunks = new List<AudioChunk>();

        lock (_lock)
        {
            byte[] allBytes = _stream.ToArray();

            while (allBytes.Length >= _chunkBytes)
            {
                byte[] chunkData = new byte[_chunkBytes];
                Array.Copy(allBytes, 0, chunkData, 0, _chunkBytes);

                float rms = AudioLevelAnalyzer.CalculateRms16BitMono(chunkData, 0, _chunkBytes);
                bool isSilent = AudioLevelAnalyzer.IsSilent(rms);

                readyChunks.Add(new AudioChunk
                {
                    Data = chunkData,
                    Length = _chunkBytes,
                    Timestamp = _currentStreamTime,
                    Duration = _chunkDuration,
                    Format = _format,
                    PeakRms = rms,
                    IsSilent = isSilent
                });

                // Advance by step
                _currentStreamTime += TimeSpan.FromSeconds((double)_stepBytes / _bytesPerSecond);

                int remaining = allBytes.Length - _stepBytes;
                byte[] nextBytes = new byte[remaining];
                Array.Copy(allBytes, _stepBytes, nextBytes, 0, remaining);
                allBytes = nextBytes;
            }

            // If finalizing, extract any remaining buffer that is at least 0.5s long
            if (isFinalizing && allBytes.Length >= _bytesPerSecond / 2)
            {
                float rms = AudioLevelAnalyzer.CalculateRms16BitMono(allBytes, 0, allBytes.Length);
                readyChunks.Add(new AudioChunk
                {
                    Data = allBytes,
                    Length = allBytes.Length,
                    Timestamp = _currentStreamTime,
                    Duration = TimeSpan.FromSeconds((double)allBytes.Length / _bytesPerSecond),
                    Format = _format,
                    PeakRms = rms,
                    IsSilent = AudioLevelAnalyzer.IsSilent(rms)
                });
                allBytes = Array.Empty<byte>();
            }

            // Replace stream contents with remaining bytes
            _stream.SetLength(0);
            if (allBytes.Length > 0)
            {
                _stream.Write(allBytes, 0, allBytes.Length);
            }
        }

        return readyChunks;
    }

    public void Reset()
    {
        lock (_lock)
        {
            _stream.SetLength(0);
            _currentStreamTime = TimeSpan.Zero;
        }
    }
}
