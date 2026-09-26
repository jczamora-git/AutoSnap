using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace AutoSnap.Audio;

public class AudioResampler : IDisposable
{
    private readonly WaveFormat _targetFormat;
    private readonly BufferedWaveProvider _inputBuffer;
    private readonly IWaveProvider _resampledProvider;
    private bool _disposed;

    public AudioResampler(WaveFormat sourceFormat, int targetSampleRate = 16000, int targetChannels = 1)
    {
        _targetFormat = new WaveFormat(targetSampleRate, 16, targetChannels);
        _inputBuffer = new BufferedWaveProvider(sourceFormat)
        {
            DiscardOnBufferOverflow = true,
            BufferLength = sourceFormat.AverageBytesPerSecond * 20
        };

        ISampleProvider sampleProvider;
        if (sourceFormat.Encoding == WaveFormatEncoding.IeeeFloat)
        {
            sampleProvider = new WaveToSampleProvider(_inputBuffer);
        }
        else if (sourceFormat.Encoding == WaveFormatEncoding.Pcm)
        {
            if (sourceFormat.BitsPerSample == 16)
                sampleProvider = new Pcm16BitToSampleProvider(_inputBuffer);
            else if (sourceFormat.BitsPerSample == 24)
                sampleProvider = new Pcm24BitToSampleProvider(_inputBuffer);
            else if (sourceFormat.BitsPerSample == 32)
                sampleProvider = new Pcm32BitToSampleProvider(_inputBuffer);
            else if (sourceFormat.BitsPerSample == 8)
                sampleProvider = new Pcm8BitToSampleProvider(_inputBuffer);
            else
                sampleProvider = _inputBuffer.ToSampleProvider();
        }
        else
        {
            sampleProvider = _inputBuffer.ToSampleProvider();
        }

        // Downmix to mono if needed
        if (sourceFormat.Channels != targetChannels)
        {
            if (targetChannels == 1 && sourceFormat.Channels == 2)
            {
                sampleProvider = new StereoToMonoSampleProvider(sampleProvider)
                {
                    LeftVolume = 0.5f,
                    RightVolume = 0.5f
                };
            }
            else if (targetChannels == 1 && sourceFormat.Channels > 2)
            {
                sampleProvider = new MultiplexingSampleProvider(new[] { sampleProvider }, 1);
            }
        }

        // Resample rate if needed
        if (sourceFormat.SampleRate != targetSampleRate)
        {
            sampleProvider = new WdlResamplingSampleProvider(sampleProvider, targetSampleRate);
        }

        // Convert back to 16-bit PCM
        _resampledProvider = new SampleToWaveProvider16(sampleProvider);
    }

    public void AddSamples(byte[] buffer, int offset, int count)
    {
        if (_disposed || count == 0) return;
        _inputBuffer.AddSamples(buffer, offset, count);
    }

    public int ReadResampledBytes(byte[] destBuffer, int offset, int count)
    {
        if (_disposed) return 0;
        return _resampledProvider.Read(destBuffer, offset, count);
    }

    public void Clear()
    {
        _inputBuffer.ClearBuffer();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _inputBuffer.ClearBuffer();
        }
    }
}
