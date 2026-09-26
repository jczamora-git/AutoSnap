namespace AutoSnap.Transcription;

public class TranscriptDocument
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Source { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public TranscriptionLanguageMode LanguageMode { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public List<TranscriptSegment> Segments { get; set; } = new();

    public string FullText => string.Join("\n\n", Segments.Select(s => $"[{s.Start:hh\\:mm\\:ss}]\n{s.Text}"));
}
