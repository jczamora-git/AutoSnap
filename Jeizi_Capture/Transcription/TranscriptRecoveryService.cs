using System.Text.Json;

namespace AutoSnap.Transcription;

public class TranscriptRecoveryService
{
    private readonly string _recoveryDirectory;

    public TranscriptRecoveryService(string? customDirectory = null)
    {
        _recoveryDirectory = !string.IsNullOrWhiteSpace(customDirectory)
            ? customDirectory
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AutoSnap", "Recovery");

        Directory.CreateDirectory(_recoveryDirectory);
    }

    public async Task AutoSaveSessionAsync(TranscriptDocument document, string? customOutputDirectory = null, CancellationToken cancellationToken = default)
    {
        string dir = !string.IsNullOrWhiteSpace(customOutputDirectory) ? customOutputDirectory : _recoveryDirectory;
        Directory.CreateDirectory(dir);

        string filePath = Path.Combine(dir, "session.autosave.json");
        string json = JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });

        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    public List<TranscriptDocument> FindRecoverableSessions()
    {
        var recoverable = new List<TranscriptDocument>();
        if (!Directory.Exists(_recoveryDirectory))
            return recoverable;

        foreach (var file in Directory.GetFiles(_recoveryDirectory, "*.autosave.json", SearchOption.AllDirectories))
        {
            try
            {
                string json = File.ReadAllText(file);
                var doc = JsonSerializer.Deserialize<TranscriptDocument>(json);
                if (doc != null && doc.Segments.Count > 0)
                {
                    recoverable.Add(doc);
                }
            }
            catch { }
        }

        return recoverable;
    }

    public void DeleteRecoveryFile(string? outputDirectory = null)
    {
        string dir = !string.IsNullOrWhiteSpace(outputDirectory) ? outputDirectory : _recoveryDirectory;
        string filePath = Path.Combine(dir, "session.autosave.json");
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch { }
    }
}
