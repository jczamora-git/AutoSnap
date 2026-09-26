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
    public event EventHandler<(TimeSpan LiveTime, TimeSpan ProcessedTime, TimeSpan BacklogTime, double RealTimeFactor, double SpeedMultiplier, LiveTranscriptionPerformanceState State)>? LatencyUpdated;
    public event EventHandler<double>? PerformanceProbeAlert;
    public event EventHandler<string>? BacklogCeilingReached;
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
    public TranscriptionQueue Queue => _queue;
    public TimeSpan LiveTime => _queue.LiveTime;
    public TimeSpan ProcessedTime => _queue.ProcessedTime;
    public TimeSpan BacklogTime => _queue.BacklogTime;
    public double DelaySeconds => _queue.DelaySeconds;
    public double RealTimeFactor => _queue.RealTimeFactor;
    public double SpeedMultiplier => _queue.SpeedMultiplier;
    public LiveTranscriptionPerformanceState PerformanceState => _queue.PerformanceState;

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
        _queue.PerformanceProbeAlert += (s, speed) => PerformanceProbeAlert?.Invoke(this, speed);
        _queue.BacklogCeilingReached += (s, msg) => BacklogCeilingReached?.Invoke(this, msg);
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

    private readonly SemaphoreSlim _sessionLock = new(1, 1);

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _sessionLock.WaitAsync(cancellationToken);
        try
        {
            if (State != TranscriptionState.Idle)
                return;

            State = TranscriptionState.Preparing;

            _queue.Start();
            await _audioSource.StartAsync(cancellationToken);

            State = TranscriptionState.Listening;
            _autosaveTimer.Start();
        }
        finally
        {
            _sessionLock.Release();
        }
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

    /// <summary>
    /// Immediately stops audio capture, cancels remaining queued audio, and saves existing segments (sub-second completion).
    /// </summary>
    public async Task StopNowAsync()
    {
        await _sessionLock.WaitAsync();
        try
        {
            if (State == TranscriptionState.Completed || State == TranscriptionState.Idle)
                return;

            State = TranscriptionState.Finalizing;
            _autosaveTimer.Stop();

            await _audioSource.StopAsync();
            await _queue.StopNowAsync();

            _document.Duration = _queue.ProcessedTime;
            await TriggerAutosaveAsync();

            State = TranscriptionState.Completed;
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    /// <summary>
    /// Finishes processing remaining queued audio in the background while reporting progress.
    /// </summary>
    public async Task FinishRemainingAsync(IProgress<TranscriptionFlushProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        await _sessionLock.WaitAsync(cancellationToken);
        try
        {
            if (State == TranscriptionState.Completed || State == TranscriptionState.Idle)
                return;

            State = TranscriptionState.Finalizing;
            _autosaveTimer.Stop();

            await _audioSource.StopAsync();
            await _queue.FlushRemainingAsync(progress, cancellationToken);
            await _queue.StopNowAsync();

            _document.Duration = _queue.ProcessedTime;
            await TriggerAutosaveAsync();

            State = TranscriptionState.Completed;
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_queue.BacklogTime <= TimeSpan.FromSeconds(30))
        {
            await FinishRemainingAsync(null, cancellationToken);
        }
        else
        {
            await StopNowAsync();
        }
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
            try
            {
                StopAsync().GetAwaiter().GetResult();
            }
            catch { }

            _audioSource.AudioDataAvailable -= OnAudioDataAvailable;
            _audioSource.Stopped -= OnAudioSourceStopped;

            _autosaveTimer.Dispose();
            _queue.Dispose();
            _audioSource.Dispose();
            _sessionLock.Dispose();
        }
    }
}
