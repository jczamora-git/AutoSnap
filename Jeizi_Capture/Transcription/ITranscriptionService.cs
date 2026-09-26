namespace AutoSnap.Transcription;

public interface ITranscriptionService : IDisposable
{
    bool IsModelLoaded { get; }
    string? LoadedModelPath { get; }

    Task LoadModelAsync(string modelPath, CancellationToken cancellationToken = default);

    Task<List<TranscriptSegment>> TranscribeStreamAsync(
        Stream waveStream,
        TranscriptionLanguageMode languageMode = TranscriptionLanguageMode.Taglish,
        IProgress<TranscriptSegment>? segmentProgress = null,
        CancellationToken cancellationToken = default);

    Task<List<TranscriptSegment>> TranscribeAudioBytesAsync(
        byte[] pcm16Bit16KhzMono,
        TranscriptionLanguageMode languageMode = TranscriptionLanguageMode.Taglish,
        TimeSpan timestampOffset = default,
        CancellationToken cancellationToken = default);
}
