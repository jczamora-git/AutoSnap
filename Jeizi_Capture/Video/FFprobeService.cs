using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace AutoSnap.Video;

public class FFprobeService
{
    private readonly string? _customPath;
    private readonly bool _useCustomPath;

    public FFprobeService(string? customPath = null, bool useCustomPath = false)
    {
        _customPath = customPath;
        _useCustomPath = useCustomPath;
    }

    public static string? FindExecutable(string? customPath = null, bool useCustom = false)
    {
        return FFmpegManager.FindFFprobe(customPath, useCustom);
    }

    public bool IsAvailable => FindExecutable(_customPath, _useCustomPath) != null;

    public async Task<VideoInfo> GetVideoInfoAsync(string videoPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(videoPath))
            throw new FileNotFoundException("Video file not found.", videoPath);

        string? ffprobeExe = FindExecutable(_customPath, _useCustomPath);
        if (string.IsNullOrEmpty(ffprobeExe))
        {
            throw new InvalidOperationException("ffprobe.exe was not found. Please install FFmpeg/FFprobe or configure its path in Settings.");
        }

        var fileInfo = new FileInfo(videoPath);
        var videoInfo = new VideoInfo
        {
            FilePath = videoPath,
            FileSizeBytes = fileInfo.Length
        };

        var startInfo = new ProcessStartInfo
        {
            FileName = ffprobeExe,
            Arguments = $"-v quiet -print_format json -show_format -show_streams \"{videoPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        string jsonOutput = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(jsonOutput))
        {
            throw new InvalidOperationException($"ffprobe failed with exit code {process.ExitCode}.");
        }

        ParseFfprobeJson(jsonOutput, videoInfo);
        return videoInfo;
    }

    private static void ParseFfprobeJson(string json, VideoInfo videoInfo)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Parse format
        if (root.TryGetProperty("format", out var formatProp))
        {
            if (formatProp.TryGetProperty("format_name", out var fn))
                videoInfo.Format = fn.GetString() ?? "Unknown";

            if (formatProp.TryGetProperty("duration", out var dur) &&
                double.TryParse(dur.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double durationSec))
            {
                videoInfo.Duration = TimeSpan.FromSeconds(durationSec);
            }
        }

        // Parse streams
        if (root.TryGetProperty("streams", out var streamsProp) && streamsProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var stream in streamsProp.EnumerateArray())
            {
                string codecType = stream.TryGetProperty("codec_type", out var ct) ? ct.GetString() ?? "" : "";

                if (codecType == "video" && videoInfo.Width == 0 && videoInfo.Height == 0)
                {
                    int width = stream.TryGetProperty("width", out var w) ? w.GetInt32() : 0;
                    int height = stream.TryGetProperty("height", out var h) ? h.GetInt32() : 0;
                    if (width > 0 && height > 0)
                    {
                        videoInfo.Width = width;
                        videoInfo.Height = height;
                    }

                    if (stream.TryGetProperty("codec_name", out var cn))
                    {
                        videoInfo.Codec = cn.GetString() ?? "Unknown";
                    }

                    if (stream.TryGetProperty("r_frame_rate", out var rfr))
                    {
                        string rfrStr = rfr.GetString() ?? "";
                        var parts = rfrStr.Split('/');
                        if (parts.Length == 2 &&
                            double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double num) &&
                            double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double den) &&
                            den > 0)
                        {
                            videoInfo.FrameRate = num / den;
                        }
                    }

                    if (videoInfo.Duration == TimeSpan.Zero &&
                        stream.TryGetProperty("duration", out var streamDur) &&
                        double.TryParse(streamDur.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double sDurSec))
                    {
                        videoInfo.Duration = TimeSpan.FromSeconds(sDurSec);
                    }
                }
                else if (codecType == "audio")
                {
                    videoInfo.HasAudio = true;
                    if (stream.TryGetProperty("codec_name", out var acn))
                    {
                        videoInfo.AudioCodec = acn.GetString() ?? "Unknown";
                    }
                }
            }
        }
    }
}
