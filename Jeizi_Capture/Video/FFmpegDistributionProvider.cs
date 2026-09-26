namespace AutoSnap.Video;

public static class FFmpegDistributionProvider
{
    public const string ProviderName = "Gyan.dev FFmpeg Release Essentials";
    public const string ProviderHomepage = "https://www.gyan.dev/ffmpeg/builds/";
    public const string OfficialLinkDocumentation = "Official Windows binary provider recommended by https://ffmpeg.org/download.html";

    /// <summary>
    /// Direct URL to the official Gyan.dev essentials release zip build.
    /// This build contains up-to-date Windows x64 binaries for ffmpeg.exe and ffprobe.exe.
    /// </summary>
    public const string DownloadUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip";

    /// <summary>
    /// Approximate download size in bytes (~95 MB).
    /// </summary>
    public const long ApproximateSizeBytes = 95 * 1024 * 1024;
}
