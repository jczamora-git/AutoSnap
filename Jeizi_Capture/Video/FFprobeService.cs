using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace AutoSnap.Video;

public class FFprobeService
{
    private readonly string? _customPath;

    public FFprobeService(string? customPath = null)
    {
        _customPath = customPath;
    }

    public static string? FindExecutable(string? customPath = null)
    {
        if (!string.IsNullOrWhiteSpace(customPath) && File.Exists(customPath))
            return customPath;

        // Check app directory
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string localProbe = Path.Combine(baseDir, "ffprobe.exe");
        if (File.Exists(localProbe)) return localProbe;

        localProbe = Path.Combine(baseDir, "ffmpeg", "ffprobe.exe");
        if (File.Exists(localProbe)) return localProbe;

        localProbe = Path.Combine(baseDir, "ffmpeg", "bin", "ffprobe.exe");
        if (File.Exists(localProbe)) return localProbe;

        // Check LocalAppData
        string localAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AutoSnap", "ffmpeg", "bin", "ffprobe.exe");
        if (File.Exists(localAppData)) return localAppData;

        // Check PATH
        string? pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathEnv))
        {
            foreach (var path in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    string candidate = Path.Combine(path.Trim(), "ffprobe.exe");
                    if (File.Exists(candidate)) return candidate;
                }
                catch
                {
                    // Ignore invalid path characters
                }
            }
        }

        // Check common chocolatey/scoop/winget default paths
        string[] commonPaths =
        {
            @"C:\ffmpeg\bin\ffprobe.exe",
            @"C:\ProgramData\chocolatey\bin\ffprobe.exe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "scoop", "shims", "ffprobe.exe")
        };

        foreach (var path in commonPaths)
        {
            if (File.Exists(path)) return path;
        }

        return null;
    }

    public bool IsAvailable => FindExecutable(_customPath) != null;

    public async Task<VideoInfo> GetVideoInfoAsync(string videoPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(videoPath))
            throw new FileNotFoundException("Video file not found.", videoPath);

        string? ffprobeExe = FindExecutable(_customPath);
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
            Arguments = $"-v error -show_entries format=duration,size:stream=index,codec_name,codec_type,width,height,r_frame_rate -of json \"{videoPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        string output = await outputTask;
        string error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"FFprobe failed with exit code {process.ExitCode}: {error}");
        }

        try
        {
            using var doc = JsonDocument.Parse(output);
            var root = doc.RootElement;

            if (root.TryGetProperty("format", out var formatProp))
            {
                if (formatProp.TryGetProperty("duration", out var durationProp))
                {
                    if (double.TryParse(durationProp.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double durationSeconds))
                    {
                        videoInfo.Duration = TimeSpan.FromSeconds(durationSeconds);
                    }
                }
            }

            if (root.TryGetProperty("streams", out var streamsProp) && streamsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var stream in streamsProp.EnumerateArray())
                {
                    string codecType = stream.TryGetProperty("codec_type", out var typeProp) ? typeProp.GetString() ?? "" : "";
                    string codecName = stream.TryGetProperty("codec_name", out var codecProp) ? codecProp.GetString() ?? "" : "";

                    if (codecType.Equals("video", StringComparison.OrdinalIgnoreCase))
                    {
                        videoInfo.Codec = codecName;
                        if (stream.TryGetProperty("width", out var widthProp))
                            videoInfo.Width = widthProp.GetInt32();
                        if (stream.TryGetProperty("height", out var heightProp))
                            videoInfo.Height = heightProp.GetInt32();

                        if (stream.TryGetProperty("r_frame_rate", out var rFrameRateProp))
                        {
                            string? rateStr = rFrameRateProp.GetString();
                            if (!string.IsNullOrEmpty(rateStr) && rateStr.Contains('/'))
                            {
                                var parts = rateStr.Split('/');
                                if (parts.Length == 2 &&
                                    double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double num) &&
                                    double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double den) &&
                                    den > 0)
                                {
                                    videoInfo.FrameRate = num / den;
                                }
                            }
                        }
                    }
                    else if (codecType.Equals("audio", StringComparison.OrdinalIgnoreCase))
                    {
                        videoInfo.HasAudio = true;
                        videoInfo.AudioCodec = codecName;
                    }
                }
            }
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse FFprobe output: {ex.Message}", ex);
        }

        return videoInfo;
    }
}
