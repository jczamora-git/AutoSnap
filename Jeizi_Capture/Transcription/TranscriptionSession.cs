using AutoSnap.Audio;

namespace AutoSnap.Transcription;

public class TranscriptionSession : IDisposable
{
    private readonly IAudioSource _audioSource;
    private readonly ITranscriptionService _transcriptionService;
    private readonly TranscriptRecoveryService _recoveryService;
    private readonly TranscriptionQueue _queue;
    private readonly System.Timers.Timer _autosaveTimer;
    private TranscriptionState _state = TranscriptionState.Idle;
    private readonly string _outputDirectory;
    private readonly TranscriptDocument _document;
    private bool _disposed;

    public event EventHandler<TranscriptionState>? StateChanged;
    public event EventHandler<TranscriptSegment>? SegmentProduced;
    public event EventHandler<(TimeSpan LiveTime, TimeSpan ProcessedTime, double DelaySeconds)>? LatencyUpdated;
    public event EventHandler<Exception>? ErrorOccurred;

    public TranscriptionState State
    {
        get => _state;
        private set
        {
            if (_state != value)
            {
                _state = value;
                StateChanged?.Invoke(this, _state);
            }
        }
    }

    public TranscriptDocument Document => _document;
    public TimeSpan LiveTime => _queue.LiveTime;
    public TimeSpan ProcessedTime => _queue.ProcessedTime;
    public double DelaySeconds => _queue.DelaySeconds;

    public TranscriptionSession(
        IAudioSource audioSource,
        ITranscriptionService transcriptionService,
        TranscriptRecoveryService recoveryService,
        string modelName,
        TranscriptionLanguageMode languageMode,
        string outputDirectory,
        TimeSpan? autosaveInterval = null)
    {
        _audioSource = audioSource;
        _transcriptionService = transcriptionService;
        _recoveryService = recoveryService;
        _outputDirectory = outputDirectory;

        _document = new TranscriptDocument
        {
            Source = audioSource.SourceName,
            SourceType = audioSource.GetType().Name,
            CreatedAt = DateTime.Now,
            LanguageMode = languageMode,
            ModelName = modelName
        };

        _queue = new TranscriptionQueue(transcriptionService)
        {
            LanguageMode = languageMode
        };

        _queue.SegmentProduced += OnSegmentProduced;
        _queue.LatencyUpdated += (s, e) => LatencyUpdated?.Invoke(this, e);
        _queue.ErrorOccurred += (s, e) => ErrorOccurred?.Invoke(this, e);

        _audioSource.AudioDataAvailable += OnAudioDataAvailable;
        _audioSource.ErrorOccurred += (s, e) => ErrorOccurred?.Invoke(this, e);
        _audioSource.Stopped += OnAudioSourceStopped;

        var interval = autosaveInterval ?? TimeSpan.FromSeconds(30);
        _autosaveTimer = new System.Timers.Timer(interval.TotalMilliseconds)
        {
            AutoReset = true
        };
        _autosaveTimer.Elapsed += async (s, e) => await TriggerAutosaveAsync();
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (State != TranscriptionState.Idle)
            return;

        State = TranscriptionState.Preparing;

        _queue.Start();
        await _audioSource.StartAsync(cancellationToken);

        State = TranscriptionState.Listening;
        _autosaveTimer.Start();
    }

    private void OnAudioDataAvailable(object? sender, AudioChunk chunk)
    {
        if (State != TranscriptionState.Listening && State != TranscriptionState.Processing)
            return;

        _queue.EnqueueAudio(chunk.Data, 0, chunk.Length);
    }

    private void OnSegmentProduced(object? sender, TranscriptSegment segment)
    {
        _document.Segments.Add(segment);
        SegmentProduced?.Invoke(this, segment);
    }

    private async void OnAudioSourceStopped(object? sender, EventArgs e)
    {
        if (State == TranscriptionState.Listening || State == TranscriptionState.Processing)
        {
            await FinalizeAsync();
        }
    }

    private async Task TriggerAutosaveAsync()
    {
        if (_document.Segments.Count == 0) return;
        try
        {
            await _recoveryService.AutoSaveSessionAsync(_document, _outputDirectory);
        }
        catch { }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (State == TranscriptionState.Completed || State == TranscriptionState.Finalizing)
            return;

        State = TranscriptionState.Finalizing;
        _autosaveTimer.Stop();

        await _audioSource.StopAsync();
        await _queue.FlushAsync(cancellationToken);
        await _queue.StopAsync();

        _document.Duration = _queue.ProcessedTime;

        // Final autosave / export
        await TriggerAutosaveAsync();

        State = TranscriptionState.Completed;
    }

    private async Task FinalizeAsync()
    {
        await StopAsync();
    }

    public async Task ExportFilesAsync(string targetDirectory, string? baseFileName = null, CancellationToken cancellationToken = default)
    {
        string name = !string.IsNullOrWhiteSpace(baseFileName) ? baseFileName : "transcript";
        await TranscriptExporter.ExportAllFormatsAsync(_document, targetDirectory, name, cancellationToken);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _autosaveTimer.Dispose();
            _queue.Dispose();
            _audioSource.Dispose();
        }
    }
}
