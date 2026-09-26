namespace AutoSnap.Transcription;

public sealed class TranscriptSegment
{
    public TimeSpan Start { get; init; }
    public TimeSpan End { get; init; }
    public string Text { get; set; } = string.Empty;
    public float Confidence { get; init; } = 1.0f;
    public string? Language { get; init; }

    public string FormattedTimeRange =>
        $"{Start:hh\\:mm\\:ss} --> {End:hh\\:mm\\:ss}";
}
