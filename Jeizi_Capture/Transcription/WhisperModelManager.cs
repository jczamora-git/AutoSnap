using System.Diagnostics;

namespace AutoSnap.Transcription;

public class WhisperModelInfo
{
    public string Name { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string DownloadUrl { get; init; } = string.Empty;
    public long ApproximateSizeBytes { get; init; }
    public string Description { get; init; } = string.Empty;
    public string QualityTier { get; init; } = "Good";
    public string SpeedTier { get; init; } = "Fast";
    public bool IsRecommended { get; init; }

    public string SizeFormatted
    {
        get
        {
            if (ApproximateSizeBytes >= 1024L * 1024 * 1024)
            {
                return $"{ApproximateSizeBytes / (1024.0 * 1024.0 * 1024.0):0.0} GB";
            }
            return $"{ApproximateSizeBytes / (1024.0 * 1024.0):0} MB";
        }
    }
}

public class WhisperModelManager
{
    private readonly string _modelsDirectory;
    private readonly HttpClient _httpClient;
    private readonly HashSet<string> _activeModelsInUse = new();
    private readonly object _lock = new();

    public static readonly List<WhisperModelInfo> AvailableModels = new()
    {
        new WhisperModelInfo
        {
            Name = "Tiny",
            FileName = "ggml-tiny.bin",
            DownloadUrl = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-tiny.bin",
            ApproximateSizeBytes = 75L * 1024 * 1024,
            Description = "Fastest / lowest resource use. Best for quick rough transcription.",
            QualityTier = "Basic",
            SpeedTier = "Ultra Fast"
        },
        new WhisperModelInfo
        {
            Name = "Base",
            FileName = "ggml-base.bin",
            DownloadUrl = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.bin",
            ApproximateSizeBytes = 142L * 1024 * 1024,
            Description = "Lightweight and faster than Small. Useful for low-end systems.",
            QualityTier = "Moderate",
            SpeedTier = "Very Fast"
        },
        new WhisperModelInfo
        {
            Name = "Small",
            FileName = "ggml-small.bin",
            DownloadUrl = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-small.bin",
            ApproximateSizeBytes = 466L * 1024 * 1024,
            Description = "Recommended balanced model. Good for English, Filipino, and Taglish.",
            QualityTier = "High",
            SpeedTier = "Fast",
            IsRecommended = true
        },
        new WhisperModelInfo
        {
            Name = "Medium",
            FileName = "ggml-medium.bin",
            DownloadUrl = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-medium.bin",
            ApproximateSizeBytes = 1536L * 1024 * 1024,
            Description = "Higher accuracy but slower and more memory intensive.",
            QualityTier = "Very High",
            SpeedTier = "Moderate"
        },
        new WhisperModelInfo
        {
            Name = "Large v3",
            FileName = "ggml-large-v3.bin",
            DownloadUrl = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-large-v3.bin",
            ApproximateSizeBytes = 2900L * 1024 * 1024,
            Description = "Highest accuracy option. Very demanding, especially for live CPU transcription.",
            QualityTier = "Maximum",
            SpeedTier = "Slow"
        },
        new WhisperModelInfo
        {
            Name = "Large v3 Turbo",
            FileName = "ggml-large-v3-turbo.bin",
            DownloadUrl = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-large-v3-turbo.bin",
            ApproximateSizeBytes = 1536L * 1024 * 1024,
            Description = "High accuracy with significantly faster inference than full Large v3. Recommended for powerful systems.",
            QualityTier = "Maximum",
            SpeedTier = "Fast",
            IsRecommended = true
        },
        new WhisperModelInfo
        {
            Name = "Large v3 Turbo Q5",
            FileName = "ggml-large-v3-turbo-q5_0.bin",
            DownloadUrl = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-large-v3-turbo-q5_0.bin",
            ApproximateSizeBytes = 547L * 1024 * 1024,
            Description = "Quantized Turbo model. Lower storage/RAM requirement with strong speed/quality balance.",
            QualityTier = "High",
            SpeedTier = "Very Fast",
            IsRecommended = true
        }
    };

    public string ModelsDirectory => _modelsDirectory;

    public WhisperModelManager(string? customDirectory = null, HttpClient? httpClient = null)
    {
        _modelsDirectory = !string.IsNullOrWhiteSpace(customDirectory)
            ? customDirectory
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AutoSnap", "Models", "Whisper");

        Directory.CreateDirectory(_modelsDirectory);
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromHours(2) };
    }

    public string GetModelPath(string modelNameOrFileName)
    {
        var model = AvailableModels.FirstOrDefault(m =>
            m.Name.Equals(modelNameOrFileName, StringComparison.OrdinalIgnoreCase) ||
            m.FileName.Equals(modelNameOrFileName, StringComparison.OrdinalIgnoreCase) ||
            (modelNameOrFileName.Equals("Turbo Q5", StringComparison.OrdinalIgnoreCase) && m.Name.Contains("Q5", StringComparison.OrdinalIgnoreCase)));

        string fileName = model != null ? model.FileName : modelNameOrFileName;
        if (!fileName.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
            fileName += ".bin";

        return Path.Combine(_modelsDirectory, fileName);
    }

    public bool IsModelInstalled(string modelNameOrFileName)
    {
        string path = GetModelPath(modelNameOrFileName);
        if (!File.Exists(path)) return false;

        var info = new FileInfo(path);
        return info.Length > 1024 * 1024; // At least 1MB
    }

    public List<WhisperModelInfo> GetInstalledModels()
    {
        return AvailableModels.Where(m => IsModelInstalled(m.Name)).ToList();
    }

    public void MarkModelInUse(string modelName)
    {
        lock (_lock)
        {
            _activeModelsInUse.Add(modelName);
        }
    }

    public void UnmarkModelInUse(string modelName)
    {
        lock (_lock)
        {
            _activeModelsInUse.Remove(modelName);
        }
    }

    public bool IsModelInUse(string modelName)
    {
        lock (_lock)
        {
            return _activeModelsInUse.Contains(modelName);
        }
    }

    public async Task DownloadModelAsync(
        WhisperModelInfo model,
        IProgress<(long BytesDownloaded, long TotalBytes, double Percent, double SpeedMbPerSec)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Check free disk space
        var drive = new DriveInfo(Path.GetPathRoot(_modelsDirectory)!);
        long requiredSpace = model.ApproximateSizeBytes + (50L * 1024 * 1024);
        if (drive.AvailableFreeSpace < requiredSpace)
        {
            double reqMb = requiredSpace / (1024.0 * 1024.0);
            double freeMb = drive.AvailableFreeSpace / (1024.0 * 1024.0);
            throw new InvalidOperationException($"Not enough disk space on drive {drive.Name}. Required: ~{reqMb:0} MB, Available: {freeMb:0} MB.");
        }

        string finalPath = GetModelPath(model.FileName);
        string partPath = finalPath + ".part";

        if (File.Exists(partPath))
        {
            try { File.Delete(partPath); } catch { }
        }

        using var response = await _httpClient.GetAsync(model.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        long totalBytes = response.Content.Headers.ContentLength ?? model.ApproximateSizeBytes;

        using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var fileStream = new FileStream(partPath, FileMode.Create, FileAccess.Write, FileShare.None, 65536, true);

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

            progress?.Report((totalRead, totalBytes, percent, speedMb));
        }

        await fileStream.FlushAsync(cancellationToken);
        fileStream.Close();

        // Validate
        var fi = new FileInfo(partPath);
        if (fi.Length < 1024 * 1024)
        {
            try { File.Delete(partPath); } catch { }
            throw new InvalidOperationException("Downloaded model file is invalid or incomplete.");
        }

        if (File.Exists(finalPath))
        {
            File.Delete(finalPath);
        }

        File.Move(partPath, finalPath);
    }

    public bool DeleteModel(WhisperModelInfo model)
    {
        lock (_lock)
        {
            if (_activeModelsInUse.Contains(model.Name) || _activeModelsInUse.Contains(model.FileName))
            {
                throw new InvalidOperationException($"Cannot delete model '{model.Name}' because it is currently in use by an active session.");
            }
        }

        string path = GetModelPath(model.FileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            return true;
        }

        return false;
    }
}
