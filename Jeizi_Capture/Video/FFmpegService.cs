using System.Diagnostics;
using System.Globalization;
using AutoSnap.Models;

namespace AutoSnap.Video;

public class FFmpegService
{
    private readonly string? _customPath;
    private readonly bool _useCustomPath;

    public FFmpegService(string? customPath = null, bool useCustomPath = false)
    {
        _customPath = customPath;
        _useCustomPath = useCustomPath;
    }

    public static string? FindExecutable(string? customPath = null, bool useCustom = false)
    {
        return FFmpegManager.FindFFmpeg(customPath, useCustom);
    }

    public bool IsAvailable => FindExecutable(_customPath, _useCustomPath) != null;

    public async Task ExtractFrameAsync(
        string videoPath,
        TimeSpan timestamp,
        string outputPath,
        ImageFormatType format = ImageFormatType.Jpg,
        int jpegQuality = 90,
        CancellationToken cancellationToken = default)
    {
        string? ffmpegExe = FindExecutable(_customPath, _useCustomPath);
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
        string? ffmpegExe = FindExecutable(_customPath, _useCustomPath);
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
