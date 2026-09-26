using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace AutoSnap.Video;

public class FFmpegManager
{
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromHours(1) };

    public static string GetManagedDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AutoSnap",
            "Tools",
            "FFmpeg");
    }

    public static string GetManagedFFmpegPath()
    {
        return Path.Combine(GetManagedDirectory(), "ffmpeg.exe");
    }

    public static string GetManagedFFprobePath()
    {
        return Path.Combine(GetManagedDirectory(), "ffprobe.exe");
    }

    public static bool IsManagedInstalled()
    {
        string ffmpeg = GetManagedFFmpegPath();
        string ffprobe = GetManagedFFprobePath();

        return File.Exists(ffmpeg) && File.Exists(ffprobe) &&
               new FileInfo(ffmpeg).Length > 1024 * 1024 &&
               new FileInfo(ffprobe).Length > 1024 * 1024;
    }

    public static string? FindFFmpeg(string? customPath = null, bool useCustom = false)
    {
        // 1. User custom override
        if (useCustom && !string.IsNullOrWhiteSpace(customPath) && File.Exists(customPath))
            return customPath;

        // 2. AutoSnap-managed tool directory
        string managed = GetManagedFFmpegPath();
        if (File.Exists(managed) && new FileInfo(managed).Length > 1024 * 1024)
            return managed;

        // 3. Application local directory
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string[] localProbes =
        {
            Path.Combine(baseDir, "ffmpeg.exe"),
            Path.Combine(baseDir, "ffmpeg", "ffmpeg.exe"),
            Path.Combine(baseDir, "ffmpeg", "bin", "ffmpeg.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AutoSnap", "ffmpeg", "bin", "ffmpeg.exe")
        };

        foreach (var probe in localProbes)
        {
            if (File.Exists(probe)) return probe;
        }

        // 4. System PATH & Common package manager paths
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
                catch { }
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

    public static string? FindFFprobe(string? customPath = null, bool useCustom = false)
    {
        // 1. User custom override
        if (useCustom && !string.IsNullOrWhiteSpace(customPath) && File.Exists(customPath))
            return customPath;

        // 2. AutoSnap-managed tool directory
        string managed = GetManagedFFprobePath();
        if (File.Exists(managed) && new FileInfo(managed).Length > 1024 * 1024)
            return managed;

        // 3. Application local directory
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string[] localProbes =
        {
            Path.Combine(baseDir, "ffprobe.exe"),
            Path.Combine(baseDir, "ffmpeg", "ffprobe.exe"),
            Path.Combine(baseDir, "ffmpeg", "bin", "ffprobe.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AutoSnap", "ffmpeg", "bin", "ffprobe.exe")
        };

        foreach (var probe in localProbes)
        {
            if (File.Exists(probe)) return probe;
        }

        // 4. System PATH & Common package manager paths
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
                catch { }
            }
        }

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

    public static async Task<string?> GetVersionAsync(string executablePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(executablePath)) return null;

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = "-version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                var match = Regex.Match(output, @"ffmpeg\s+version\s+([^\s]+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }

                // First line fallback
                string firstLine = output.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                return firstLine;
            }
        }
        catch { }

        return null;
    }

    public static async Task InstallManagedFFmpegAsync(
        IProgress<(long BytesDownloaded, long TotalBytes, double Percent, double SpeedMbPerSec, string Stage)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        string toolsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AutoSnap",
            "Tools");

        Directory.CreateDirectory(toolsDir);

        string zipPartPath = Path.Combine(toolsDir, "ffmpeg_download.zip.part");
        string stagingDir = Path.Combine(toolsDir, "ffmpeg_staging");

        try
        {
            if (File.Exists(zipPartPath))
            {
                try { File.Delete(zipPartPath); } catch { }
            }

            if (Directory.Exists(stagingDir))
            {
                try { Directory.Delete(stagingDir, true); } catch { }
            }

            // Check disk space (require at least 300MB free)
            var drive = new DriveInfo(Path.GetPathRoot(toolsDir)!);
            if (drive.AvailableFreeSpace < 300L * 1024 * 1024)
            {
                throw new InvalidOperationException($"Insufficient disk space on drive {drive.Name}. At least 300 MB of free space is required to install FFmpeg.");
            }

            // 1. Download
            progress?.Report((0, FFmpegDistributionProvider.ApproximateSizeBytes, 0, 0, "Connecting to download server..."));

            using var response = await _httpClient.GetAsync(
                FFmpegDistributionProvider.DownloadUrl,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            long totalBytes = response.Content.Headers.ContentLength ?? FFmpegDistributionProvider.ApproximateSizeBytes;

            using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
            using (var fileStream = new FileStream(zipPartPath, FileMode.Create, FileAccess.Write, FileShare.None, 65536, true))
            {
                byte[] buffer = new byte[65536];
                long totalRead = 0;
                int bytesRead;
                var stopwatch = Stopwatch.StartNew();

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    totalRead += bytesRead;

                    double elapsedSec = stopwatch.Elapsed.TotalSeconds;
                    double speedMb = elapsedSec > 0 ? (totalRead / (1024.0 * 1024.0)) / elapsedSec : 0;
                    double percent = totalBytes > 0 ? (double)totalRead / totalBytes * 100.0 : 0;

                    progress?.Report((totalRead, totalBytes, percent, speedMb, "Downloading FFmpeg & FFprobe archive..."));
                }

                await fileStream.FlushAsync(cancellationToken);
            }

            // 2. Extract
            progress?.Report((totalBytes, totalBytes, 100, 0, "Extracting binaries from archive..."));

            Directory.CreateDirectory(stagingDir);
            ZipFile.ExtractToDirectory(zipPartPath, stagingDir, true);

            // Locate ffmpeg.exe and ffprobe.exe inside extracted staging directory
            string[] foundFfmpeg = Directory.GetFiles(stagingDir, "ffmpeg.exe", SearchOption.AllDirectories);
            string[] foundFfprobe = Directory.GetFiles(stagingDir, "ffprobe.exe", SearchOption.AllDirectories);

            if (foundFfmpeg.Length == 0 || foundFfprobe.Length == 0)
            {
                throw new InvalidOperationException("The downloaded archive did not contain valid ffmpeg.exe and ffprobe.exe executables.");
            }

            string stagedFfmpeg = foundFfmpeg[0];
            string stagedFfprobe = foundFfprobe[0];

            // 3. Verify execution before copying to final location
            progress?.Report((totalBytes, totalBytes, 100, 0, "Verifying executable binaries..."));

            string? ver = await GetVersionAsync(stagedFfmpeg, cancellationToken);
            if (string.IsNullOrWhiteSpace(ver))
            {
                throw new InvalidOperationException("Downloaded ffmpeg.exe failed verification test.");
            }

            // 4. Deploy to final managed directory
            string targetManagedDir = GetManagedDirectory();
            Directory.CreateDirectory(targetManagedDir);

            string targetFfmpeg = GetManagedFFmpegPath();
            string targetFfprobe = GetManagedFFprobePath();

            if (File.Exists(targetFfmpeg)) File.Delete(targetFfmpeg);
            if (File.Exists(targetFfprobe)) File.Delete(targetFfprobe);

            File.Copy(stagedFfmpeg, targetFfmpeg, true);
            File.Copy(stagedFfprobe, targetFfprobe, true);

            progress?.Report((totalBytes, totalBytes, 100, 0, "FFmpeg installed successfully!"));
        }
        finally
        {
            // Cleanup part and staging
            try
            {
                if (File.Exists(zipPartPath)) File.Delete(zipPartPath);
            }
            catch { }

            try
            {
                if (Directory.Exists(stagingDir)) Directory.Delete(stagingDir, true);
            }
            catch { }
        }
    }
}
