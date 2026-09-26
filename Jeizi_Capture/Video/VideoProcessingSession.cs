using System.Diagnostics;
using AutoSnap.Audio;
using AutoSnap.Transcription;

namespace AutoSnap.Video;

public class VideoProcessingSession
{
    private readonly FFmpegService _ffmpegService;
    private readonly VideoSnapshotExtractor _snapshotExtractor;
    private readonly WhisperTranscriptionService _transcriptionService;
    private readonly WhisperModelManager _modelManager;
    private readonly TranscriptRecoveryService _recoveryService;

    public event EventHandler<string>? StatusMessageChanged;
    public event EventHandler<(int Current, int Total, double Percent, TimeSpan CurrentTime, TimeSpan TotalDuration)>? SnapshotProgressChanged;
    public event EventHandler<TranscriptSegment>? TranscriptSegmentProduced;

    public VideoProcessingSession(
        FFmpegService ffmpegService,
        WhisperTranscriptionService transcriptionService,
        WhisperModelManager modelManager,
        TranscriptRecoveryService recoveryService)
    {
        _ffmpegService = ffmpegService;
        _snapshotExtractor = new VideoSnapshotExtractor(ffmpegService);
        _transcriptionService = transcriptionService;
        _modelManager = modelManager;
        _recoveryService = recoveryService;

        _snapshotExtractor.ProgressChanged += (s, e) => SnapshotProgressChanged?.Invoke(this, e);
    }

    public async Task<VideoProcessingResult> ProcessVideoAsync(
        VideoInfo videoInfo,
        VideoProcessingOptions options,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new VideoProcessingResult();

        string baseDir = !string.IsNullOrWhiteSpace(options.OutputBaseDirectory)
            ? options.OutputBaseDirectory
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "AutoSnap");

        string dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
        string videoFolder = Path.GetFileNameWithoutExtension(videoInfo.FilePath);
        string sessionDir = Path.Combine(baseDir, dateFolder, videoFolder);
        Directory.CreateDirectory(sessionDir);

        try
        {
            // 1. Snapshot extraction
            if (options.ExtractSnapshots)
            {
                StatusMessageChanged?.Invoke(this, "Extracting video snapshots...");
                string snapshotsDir = Path.Combine(sessionDir, "Snapshots");
                Directory.CreateDirectory(snapshotsDir);

                int count = await _snapshotExtractor.ExtractSnapshotsAsync(
                    videoInfo,
                    options.SnapshotInterval,
                    snapshotsDir,
                    options.Format,
                    options.JpegQuality,
                    cancellationToken);

                result.SnapshotCount = count;
                result.SnapshotsDirectory = snapshotsDir;
            }

            // 2. Transcription
            if (options.GenerateTranscript && videoInfo.HasAudio)
            {
                StatusMessageChanged?.Invoke(this, "Preparing audio for transcription...");
                string transcriptDir = Path.Combine(sessionDir, "Transcript");
                Directory.CreateDirectory(transcriptDir);

                string modelPath = _modelManager.GetModelPath(options.WhisperModelType);
                if (!File.Exists(modelPath))
                {
                    throw new InvalidOperationException($"Whisper model '{options.WhisperModelType}' is not installed. Please download it first in Model Manager.");
                }

                await _transcriptionService.LoadModelAsync(modelPath, cancellationToken);

                using var audioSource = new MediaFileAudioSource(videoInfo.FilePath, _ffmpegService);
                string wavPath = await audioSource.PrepareAudioWavAsync(cancellationToken);

                StatusMessageChanged?.Invoke(this, "Transcribing audio with Whisper...");

                using var stream = File.OpenRead(wavPath);
                var progress = new Progress<TranscriptSegment>(seg => TranscriptSegmentProduced?.Invoke(this, seg));

                var segments = await _transcriptionService.TranscribeStreamAsync(
                    stream,
                    options.LanguageMode,
                    progress,
                    cancellationToken);

                var doc = new TranscriptDocument
                {
                    Source = videoInfo.FileName,
                    SourceType = "Video",
                    CreatedAt = DateTime.Now,
                    LanguageMode = options.LanguageMode,
                    ModelName = options.WhisperModelType,
                    Duration = videoInfo.Duration,
                    Segments = segments
                };

                await TranscriptExporter.ExportAllFormatsAsync(doc, transcriptDir, "transcript", cancellationToken);
                result.TranscriptPath = Path.Combine(transcriptDir, "transcript.txt");

                audioSource.CleanupTempFile();
            }

            stopwatch.Stop();
            result.Success = true;
            result.ProcessingDuration = stopwatch.Elapsed;
            StatusMessageChanged?.Invoke(this, "Processing completed successfully.");
            return result;
        }
        catch (OperationCanceledException)
        {
            result.Success = false;
            result.ErrorMessage = "Video processing was cancelled by user.";
            StatusMessageChanged?.Invoke(this, "Processing cancelled.");
            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            StatusMessageChanged?.Invoke(this, $"Error: {ex.Message}");
            return result;
        }
    }
}
