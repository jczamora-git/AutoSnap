using System.Text;

namespace AutoSnap.Transcription;

public static class TranscriptExporter
{
    public static string ToTxt(TranscriptDocument document)
    {
        return ToTxt(document.Segments);
    }

    public static string ToTxt(IEnumerable<TranscriptSegment> segments)
    {
        var sb = new StringBuilder();
        foreach (var seg in segments)
        {
            if (string.IsNullOrWhiteSpace(seg.Text)) continue;
            sb.AppendLine($"[{seg.Start:hh\\:mm\\:ss}]");
            sb.AppendLine(seg.Text.Trim());
            sb.AppendLine();
        }
        return sb.ToString().TrimEnd();
    }

    public static string ToSrt(TranscriptDocument document)
    {
        return ToSrt(document.Segments);
    }

    public static string ToSrt(IEnumerable<TranscriptSegment> segments)
    {
        var sb = new StringBuilder();
        int index = 1;

        foreach (var seg in segments)
        {
            if (string.IsNullOrWhiteSpace(seg.Text)) continue;

            string start = FormatSrtTime(seg.Start);
            string end = FormatSrtTime(seg.End > seg.Start ? seg.End : seg.Start + TimeSpan.FromSeconds(2));

            sb.AppendLine(index.ToString());
            sb.AppendLine($"{start} --> {end}");
            sb.AppendLine(seg.Text.Trim());
            sb.AppendLine();

            index++;
        }

        return sb.ToString().TrimEnd();
    }

    public static string ToVtt(TranscriptDocument document)
    {
        return ToVtt(document.Segments);
    }

    public static string ToVtt(IEnumerable<TranscriptSegment> segments)
    {
        var sb = new StringBuilder();
        sb.AppendLine("WEBVTT");
        sb.AppendLine();

        foreach (var seg in segments)
        {
            if (string.IsNullOrWhiteSpace(seg.Text)) continue;

            string start = FormatVttTime(seg.Start);
            string end = FormatVttTime(seg.End > seg.Start ? seg.End : seg.Start + TimeSpan.FromSeconds(2));

            sb.AppendLine($"{start} --> {end}");
            sb.AppendLine(seg.Text.Trim());
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    public static async Task ExportAllFormatsAsync(TranscriptDocument document, string targetDirectory, string baseFileName, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(targetDirectory);

        string txtPath = Path.Combine(targetDirectory, $"{baseFileName}.txt");
        string srtPath = Path.Combine(targetDirectory, $"{baseFileName}.srt");
        string vttPath = Path.Combine(targetDirectory, $"{baseFileName}.vtt");

        await File.WriteAllTextAsync(txtPath, ToTxt(document), Encoding.UTF8, cancellationToken);
        await File.WriteAllTextAsync(srtPath, ToSrt(document), Encoding.UTF8, cancellationToken);
        await File.WriteAllTextAsync(vttPath, ToVtt(document), Encoding.UTF8, cancellationToken);
    }

    private static string FormatSrtTime(TimeSpan time)
    {
        return $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00},{time.Milliseconds:000}";
    }

    private static string FormatVttTime(TimeSpan time)
    {
        return $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}.{time.Milliseconds:000}";
    }
}
