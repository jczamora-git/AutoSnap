using System.Diagnostics;
using System.Globalization;
using AutoSnap.Models;

namespace AutoSnap.Video;

public class FFmpegService
{
    private readonly string? _customPath;

    public FFmpegService(string? customPath = null)
    {
        _customPath = customPath;
    }

    public static string? FindExecutable(string? customPath = null)
    {
        if (!string.IsNullOrWhiteSpace(customPath) && File.Exists(customPath))
            return customPath;

        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string localProbe = Path.Combine(baseDir, "ffmpeg.exe");
        if (File.Exists(localProbe)) return localProbe;

        localProbe = Path.Combine(baseDir, "ffmpeg", "ffmpeg.exe");
        if (File.Exists(localProbe)) return localProbe;

        localProbe = Path.Combine(baseDir, "ffmpeg", "bin", "ffmpeg.exe");
        if (File.Exists(localProbe)) return localProbe;

        string localAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AutoSnap", "ffmpeg", "bin", "ffmpeg.exe");
        if (File.Exists(localAppData)) return localAppData;

        string? pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathEnv))
        {
            foreach (var path in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    string candidate = Path.Combine(path.Trim(), "ffmpeg.exe");
                    if (File.Exists(candidate)) return candidate;
                }
                catch
                {
                    // Ignore invalid characters
                }
            }
        }

        string[] commonPaths =
        {
            @"C:\ffmpeg\bin\ffmpeg.exe",
            @"C:\ProgramData\chocolatey\bin\ffmpeg.exe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "scoop", "shims", "ffmpeg.exe")
        };

        foreach (var path in commonPaths)
        {
            if (File.Exists(path)) return path;
        }

        return null;
    }

    public bool IsAvailable => FindExecutable(_customPath) != null;

    public async Task ExtractFrameAsync(
        string videoPath,
        TimeSpan timestamp,
        string outputPath,
        ImageFormatType format = ImageFormatType.Jpg,
        int jpegQuality = 90,
        CancellationToken cancellationToken = default)
    {
        string? ffmpegExe = FindExecutable(_customPath);
        if (string.IsNullOrEmpty(ffmpegExe))
            throw new InvalidOperationException("ffmpeg.exe was not found. Please install FFmpeg or configure its path in Settings.");

        string timeStr = timestamp.ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture);

        string qualityArgs = format == ImageFormatType.Jpg
            ? $"-q:v {Math.Max(1, Math.Min(31, 31 - (int)(jpegQuality * 0.3)))}"
            : "";

        var startInfo = new ProcessStartInfo
        {
            FileName = ffmpegExe,
            Arguments = $"-ss {timeStr} -i \"{videoPath}\" -frames:v 1 {qualityArgs} -y \"{outputPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                    process.Kill(true);
            }
            catch { }
            throw;
        }

        if (process.ExitCode != 0)
        {
            string error = await process.StandardError.ReadToEndAsync(CancellationToken.None);
            throw new InvalidOperationException($"FFmpeg frame extraction failed with code {process.ExitCode}: {error}");
        }
    }

    public async Task ExtractAudioAsync(
        string mediaPath,
        string outputWavPath,
        CancellationToken cancellationToken = default)
    {
        string? ffmpegExe = FindExecutable(_customPath);
        if (string.IsNullOrEmpty(ffmpegExe))
            throw new InvalidOperationException("ffmpeg.exe was not found. Please install FFmpeg or configure its path in Settings.");

        // Extract as 16kHz 16-bit mono PCM WAV for Whisper
        var startInfo = new ProcessStartInfo
        {
            FileName = ffmpegExe,
            Arguments = $"-i \"{mediaPath}\" -vn -acodec pcm_s16le -ar 16000 -ac 1 -y \"{outputWavPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                    process.Kill(true);
            }
            catch { }
            throw;
        }

        if (process.ExitCode != 0)
        {
            string error = await process.StandardError.ReadToEndAsync(CancellationToken.None);
            throw new InvalidOperationException($"FFmpeg audio extraction failed with code {process.ExitCode}: {error}");
        }
    }
}
