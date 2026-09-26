using Whisper.net;

namespace AutoSnap.Transcription;

public class WhisperTranscriptionService : ITranscriptionService
{
    private WhisperFactory? _factory;
    private string? _loadedModelPath;
    private readonly object _lock = new();
    private bool _disposed;

    public bool IsModelLoaded => _factory != null;
    public string? LoadedModelPath => _loadedModelPath;

    public Task LoadModelAsync(string modelPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(modelPath))
            throw new FileNotFoundException("Whisper model file not found.", modelPath);

        lock (_lock)
        {
            if (_loadedModelPath == modelPath && _factory != null)
                return Task.CompletedTask;

            _factory?.Dispose();
            _factory = WhisperFactory.FromPath(modelPath);
            _loadedModelPath = modelPath;
        }

        return Task.CompletedTask;
    }

    private static string GetLanguageCode(TranscriptionLanguageMode mode) => mode switch
    {
        TranscriptionLanguageMode.English => "en",
        TranscriptionLanguageMode.Filipino => "tl",
        TranscriptionLanguageMode.Taglish => "tl",
        _ => "auto"
    };

    public async Task<List<TranscriptSegment>> TranscribeStreamAsync(
        Stream waveStream,
        TranscriptionLanguageMode languageMode = TranscriptionLanguageMode.Taglish,
        IProgress<TranscriptSegment>? segmentProgress = null,
        CancellationToken cancellationToken = default)
    {
        WhisperFactory? factory;
        lock (_lock)
        {
            factory = _factory;
        }

        if (factory == null)
            throw new InvalidOperationException("No Whisper model is currently loaded. Please select and load a model first.");

        string lang = GetLanguageCode(languageMode);

        using var processor = factory.CreateBuilder()
            .WithLanguage(lang)
            .Build();

        var segments = new List<TranscriptSegment>();

        await foreach (var segment in processor.ProcessAsync(waveStream, cancellationToken))
        {
            if (string.IsNullOrWhiteSpace(segment.Text))
                continue;

            var transcriptSegment = new TranscriptSegment
            {
                Start = segment.Start,
                End = segment.End,
                Text = segment.Text.Trim(),
                Language = lang,
                Confidence = segment.MinProbability
            };

            segments.Add(transcriptSegment);
            segmentProgress?.Report(transcriptSegment);
        }

        return segments;
    }

    public async Task<List<TranscriptSegment>> TranscribeAudioBytesAsync(
        byte[] pcm16Bit16KhzMono,
        TranscriptionLanguageMode languageMode = TranscriptionLanguageMode.Taglish,
        TimeSpan timestampOffset = default,
        CancellationToken cancellationToken = default)
    {
        if (pcm16Bit16KhzMono.Length == 0)
            return new List<TranscriptSegment>();

        // Create in-memory standard 16kHz 16-bit mono WAV stream with 44-byte header
        using var ms = new MemoryStream();
        WriteWavHeader(ms, pcm16Bit16KhzMono.Length, 16000, 1, 16);
        ms.Write(pcm16Bit16KhzMono, 0, pcm16Bit16KhzMono.Length);
        ms.Position = 0;

        var rawSegments = await TranscribeStreamAsync(ms, languageMode, null, cancellationToken);

        // Adjust segments by timestampOffset
        if (timestampOffset > TimeSpan.Zero)
        {
            return rawSegments.Select(s => new TranscriptSegment
            {
                Start = s.Start + timestampOffset,
                End = s.End + timestampOffset,
                Text = s.Text,
                Confidence = s.Confidence,
                Language = s.Language
            }).ToList();
        }

        return rawSegments;
    }

    private static void WriteWavHeader(Stream stream, int pcmDataLength, int sampleRate, short channels, short bitsPerSample)
    {
        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true);
        int byteRate = sampleRate * channels * (bitsPerSample / 8);
        short blockAlign = (short)(channels * (bitsPerSample / 8));

        // RIFF chunk
        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + pcmDataLength);
        writer.Write("WAVE"u8.ToArray());

        // fmt subchunk
        writer.Write("fmt "u8.ToArray());
        writer.Write(16); // Subchunk1Size
        writer.Write((short)1); // AudioFormat (PCM)
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write(blockAlign);
        writer.Write(bitsPerSample);

        // data subchunk
        writer.Write("data"u8.ToArray());
        writer.Write(pcmDataLength);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            lock (_lock)
            {
                _factory?.Dispose();
                _factory = null;
            }
        }
    }
}
