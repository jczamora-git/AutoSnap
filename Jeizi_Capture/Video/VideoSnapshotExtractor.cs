using System.Diagnostics;
using AutoSnap.Models;

namespace AutoSnap.Video;

public class VideoSnapshotExtractor
{
    private readonly FFmpegService _ffmpegService;

    public event EventHandler<(int Current, int Total, double Percent, TimeSpan CurrentTime, TimeSpan TotalDuration)>? ProgressChanged;

    public VideoSnapshotExtractor(FFmpegService ffmpegService)
    {
        _ffmpegService = ffmpegService;
    }

    public async Task<int> ExtractSnapshotsAsync(
        VideoInfo videoInfo,
        TimeSpan interval,
        string outputDirectory,
        ImageFormatType format = ImageFormatType.Jpg,
        int jpegQuality = 90,
        CancellationToken cancellationToken = default)
    {
        if (videoInfo.Duration <= TimeSpan.Zero)
            throw new ArgumentException("Video duration must be greater than zero.", nameof(videoInfo));

        if (interval <= TimeSpan.Zero)
            throw new ArgumentException("Interval must be greater than zero.", nameof(interval));

        Directory.CreateDirectory(outputDirectory);

        // Generate timestamps: 00:00:00, interval, 2*interval, ... < Duration
        var timestamps = new List<TimeSpan>();
        TimeSpan cur = TimeSpan.Zero;
        while (cur < videoInfo.Duration)
        {
            timestamps.Add(cur);
            cur += interval;
        }

        if (timestamps.Count == 0)
            timestamps.Add(TimeSpan.Zero);

        int total = timestamps.Count;
        string ext = format == ImageFormatType.Png ? ".png" : ".jpg";

        for (int i = 0; i < total; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var ts = timestamps[i];
            string filename = $"{ts:hh\\-mm\\-ss}{ext}";
            string outputPath = Path.Combine(outputDirectory, filename);

            await _ffmpegService.ExtractFrameAsync(
                videoInfo.FilePath,
                ts,
                outputPath,
                format,
                jpegQuality,
                cancellationToken);

            double percent = (double)(i + 1) / total * 100.0;
            ProgressChanged?.Invoke(this, (i + 1, total, percent, ts, videoInfo.Duration));
        }

        return total;
    }
}
