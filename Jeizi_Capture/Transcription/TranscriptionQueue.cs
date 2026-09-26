using System.Collections.Concurrent;
using System.Diagnostics;
using AutoSnap.Audio;

namespace AutoSnap.Transcription;

public enum LiveTranscriptionPerformanceState
{
    Excellent,       // >= 1.5x real time
    RealTime,        // 1.0x - 1.5x real time
    Borderline,      // 0.75x - 1.0x real time
    FallingBehind,   // 0.25x - 0.75x real time
    SeverelyBehind   // < 0.25x real time
}

public record TranscriptionFlushProgress(
    TimeSpan ProcessedAudio,
    TimeSpan TotalAudio,
    TimeSpan RemainingAudio,
    double SpeedMultiplier,
    TimeSpan EstimatedRemainingTime);

public class TranscriptionQueue : IDisposable
{
    private readonly ITranscriptionService _transcriptionService;
    private readonly AudioBuffer _audioBuffer;
    private readonly ConcurrentQueue<AudioChunk> _chunkQueue = new();
    private CancellationTokenSource? _workerCts;
    private Task? _workerTask;
    private bool _isRunning;
    private bool _disposed;

    private readonly List<TranscriptSegment> _allSegments = new();
    private readonly object _segmentsLock = new();

    private TimeSpan _processedTime = TimeSpan.Zero;
    private readonly Stopwatch _liveStopwatch = new();

    // Rolling RTF & Speed tracking
    private readonly Queue<double> _recentRtf = new();
    private readonly object _metricsLock = new();
    private double _currentRtf = 0.5;
    private double _currentSpeed = 2.0;

    // Performance Probe on first chunk
    private bool _probeFired;

    // Backpressure limits
    public static readonly TimeSpan MaxLiveBacklogDuration = TimeSpan.FromSeconds(60);
    public static readonly TimeSpan HardSafetyCeiling = TimeSpan.FromMinutes(5);

    public event EventHandler<TranscriptSegment>? SegmentProduced;
    public event EventHandler<(TimeSpan LiveTime, TimeSpan ProcessedTime, TimeSpan BacklogTime, double RealTimeFactor, double SpeedMultiplier, LiveTranscriptionPerformanceState State)>? LatencyUpdated;
    public event EventHandler<double>? PerformanceProbeAlert; // Fires if first chunk speed < 0.5x
    public event EventHandler<string>? BacklogCeilingReached;
    public event EventHandler<Exception>? ErrorOccurred;

    public TranscriptionLanguageMode LanguageMode { get; set; } = TranscriptionLanguageMode.Taglish;
    public TimeSpan LiveTime => _liveStopwatch.Elapsed;
    public TimeSpan ProcessedTime => _processedTime;
    public TimeSpan BacklogTime => LiveTime > _processedTime ? LiveTime - _processedTime : TimeSpan.Zero;
    public double DelaySeconds => BacklogTime.TotalSeconds;
    public double RealTimeFactor => _currentRtf;
    public double SpeedMultiplier => _currentSpeed;

    public LiveTranscriptionPerformanceState PerformanceState
    {
        get
        {
            if (_currentSpeed >= 1.5) return LiveTranscriptionPerformanceState.Excellent;
            if (_currentSpeed >= 1.0) return LiveTranscriptionPerformanceState.RealTime;
            if (_currentSpeed >= 0.75) return LiveTranscriptionPerformanceState.Borderline;
            if (_currentSpeed >= 0.25) return LiveTranscriptionPerformanceState.FallingBehind;
            return LiveTranscriptionPerformanceState.SeverelyBehind;
        }
    }

    public int QueuedChunksCount => _chunkQueue.Count;

    public TimeSpan QueuedDuration
    {
        get
        {
            long totalTicks = 0;
            foreach (var chunk in _chunkQueue)
            {
                totalTicks += chunk.Duration.Ticks;
            }
            return TimeSpan.FromTicks(totalTicks);
        }
    }

    public IReadOnlyList<TranscriptSegment> Segments
    {
        get
        {
            lock (_segmentsLock)
            {
                return _allSegments.ToList();
            }
        }
    }

    public TranscriptionQueue(
        ITranscriptionService transcriptionService,
        TimeSpan? chunkDuration = null,
        TimeSpan? overlapDuration = null)
    {
        _transcriptionService = transcriptionService;
        _audioBuffer = new AudioBuffer(chunkDuration, overlapDuration);
    }

    public void Start()
    {
        if (_disposed || _isRunning) return;

        if (_workerCts != null || _workerTask != null)
        {
            var oldCts = Interlocked.Exchange(ref _workerCts, null);
            var oldTask = Interlocked.Exchange(ref _workerTask, null);
            if (oldCts != null)
            {
                try
                {
                    oldCts.Cancel();
                    oldTask?.GetAwaiter().GetResult();
                }
                catch { }
                finally
                {
                    oldCts.Dispose();
                }
            }
        }

        _isRunning = true;
        _probeFired = false;
        _liveStopwatch.Restart();

        var cts = new CancellationTokenSource();
        _workerCts = cts;
        Debug.WriteLine("[TranscriptionQueue] Worker loop started on background thread");
        _workerTask = Task.Run(() => WorkerLoopAsync(cts.Token), cts.Token);
    }

    public void EnqueueAudio(byte[] pcmData, int offset, int count)
    {
        if (!_isRunning || count <= 0) return;

        // Check hard safety ceiling to prevent unbounded memory growth
        if (BacklogTime > HardSafetyCeiling)
        {
            BacklogCeilingReached?.Invoke(this, $"Live audio backlog exceeded {HardSafetyCeiling.TotalMinutes:0} minutes safety limit. Pausing buffer accumulation.");
            return;
        }

        _audioBuffer.AddAudio(pcmData, offset, count);
        var readyChunks = _audioBuffer.ExtractReadyChunks();

        foreach (var chunk in readyChunks)
        {
            _chunkQueue.Enqueue(chunk);
        }
    }

    private async Task WorkerLoopAsync(CancellationToken cancellationToken)
    {
        var inferSw = new Stopwatch();

        while (!cancellationToken.IsCancellationRequested && _isRunning)
        {
            try
            {
                if (_chunkQueue.TryDequeue(out var chunk))
                {
                    if (!chunk.IsSilent && chunk.Data.Length > 0)
                    {
                        inferSw.Restart();

                        var segments = await _transcriptionService.TranscribeAudioBytesAsync(
                            chunk.Data,
                            LanguageMode,
                            chunk.Timestamp,
                            cancellationToken);

                        inferSw.Stop();

                        double audioSec = chunk.Duration.TotalSeconds;
                        double inferSec = inferSw.Elapsed.TotalSeconds;

                        if (audioSec > 0)
                        {
                            double rtf = inferSec / audioSec;
                            double speed = inferSec > 0 ? audioSec / inferSec : 1.0;

                            lock (_metricsLock)
                            {
                                _recentRtf.Enqueue(rtf);
                                while (_recentRtf.Count > 5) _recentRtf.Dequeue();
                                _currentRtf = _recentRtf.Average();
                                _currentSpeed = _currentRtf > 0 ? 1.0 / _currentRtf : 1.0;
                            }

                            // Performance probe check on first valid speech chunk
                            if (!_probeFired && audioSec >= 5)
                            {
                                _probeFired = true;
                                if (speed < 0.5)
                                {
                                    PerformanceProbeAlert?.Invoke(this, speed);
                                }
                            }
                        }

                        foreach (var segment in segments)
                        {
                            AddSegmentWithOverlapSuppression(segment);
                        }
                    }

                    _processedTime = chunk.Timestamp + chunk.Duration;
                    LatencyUpdated?.Invoke(this, (LiveTime, _processedTime, BacklogTime, _currentRtf, _currentSpeed, PerformanceState));
                }
                else
                {
                    await Task.Delay(100, cancellationToken);
                    LatencyUpdated?.Invoke(this, (LiveTime, _processedTime, BacklogTime, _currentRtf, _currentSpeed, PerformanceState));
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
            }
        }
    }

    private void AddSegmentWithOverlapSuppression(TranscriptSegment newSegment)
    {
        if (string.IsNullOrWhiteSpace(newSegment.Text))
            return;

        lock (_segmentsLock)
        {
            if (_allSegments.Count > 0)
            {
                var last = _allSegments[^1];

                string lastText = last.Text.Trim();
                string newText = newSegment.Text.Trim();

                if (string.Equals(lastText, newText, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                string[] lastWords = lastText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string[] newWords = newText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                int maxOverlap = Math.Min(5, Math.Min(lastWords.Length, newWords.Length));
                int matchedWords = 0;

                for (int matchLen = maxOverlap; matchLen >= 2; matchLen--)
                {
                    var lastSuffix = string.Join(" ", lastWords[^matchLen..]);
                    var newPrefix = string.Join(" ", newWords[..matchLen]);

                    if (string.Equals(lastSuffix, newPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        matchedWords = matchLen;
                        break;
                    }
                }

                if (matchedWords > 0)
                {
                    var cleanNewWords = newWords[matchedWords..];
                    if (cleanNewWords.Length == 0) return;
                    newText = string.Join(" ", cleanNewWords);
                }

                newSegment.Text = newText;
            }

            _allSegments.Add(newSegment);
            SegmentProduced?.Invoke(this, newSegment);
        }
    }

    /// <summary>
    /// Processes all remaining queued audio in background while reporting progress and estimated time remaining.
    /// </summary>
    public async Task FlushRemainingAsync(IProgress<TranscriptionFlushProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        var remainingChunks = _audioBuffer.ExtractReadyChunks(isFinalizing: true);
        foreach (var chunk in remainingChunks)
        {
            _chunkQueue.Enqueue(chunk);
        }

        TimeSpan initialTotalAudio = LiveTime;

        while (!_chunkQueue.IsEmpty && !cancellationToken.IsCancellationRequested)
        {
            if (_chunkQueue.TryDequeue(out var chunk))
            {
                if (!chunk.IsSilent && chunk.Data.Length > 0)
                {
                    var sw = Stopwatch.StartNew();
                    var segments = await _transcriptionService.TranscribeAudioBytesAsync(
                        chunk.Data,
                        LanguageMode,
                        chunk.Timestamp,
                        cancellationToken);
                    sw.Stop();

                    double audioSec = chunk.Duration.TotalSeconds;
                    double inferSec = sw.Elapsed.TotalSeconds;
                    if (audioSec > 0 && inferSec > 0)
                    {
                        lock (_metricsLock)
                        {
                            double rtf = inferSec / audioSec;
                            _recentRtf.Enqueue(rtf);
                            while (_recentRtf.Count > 5) _recentRtf.Dequeue();
                            _currentRtf = _recentRtf.Average();
                            _currentSpeed = 1.0 / _currentRtf;
                        }
                    }

                    foreach (var segment in segments)
                    {
                        AddSegmentWithOverlapSuppression(segment);
                    }
                }

                _processedTime = chunk.Timestamp + chunk.Duration;

                TimeSpan remaining = initialTotalAudio > _processedTime ? initialTotalAudio - _processedTime : TimeSpan.Zero;
                double speed = _currentSpeed > 0 ? _currentSpeed : 1.0;
                TimeSpan eta = TimeSpan.FromSeconds(remaining.TotalSeconds / speed);

                progress?.Report(new TranscriptionFlushProgress(_processedTime, initialTotalAudio, remaining, speed, eta));
            }
        }
    }

    /// <summary>
    /// Immediately halts transcription worker and clears pending queued audio without losing completed segments.
    /// </summary>
    public async Task StopNowAsync()
    {
        _isRunning = false;
        _liveStopwatch.Stop();

        var cts = Interlocked.Exchange(ref _workerCts, null);
        var task = Interlocked.Exchange(ref _workerTask, null);

        if (cts != null)
        {
            try
            {
                cts.Cancel();
                if (task != null)
                {
                    try
                    {
                        await task.ConfigureAwait(false);
                    }
                    catch { }
                }
            }
            finally
            {
                cts.Dispose();
            }
        }

        // Drain / clear unprocessed audio
        while (_chunkQueue.TryDequeue(out _)) { }
    }

    public async Task StopAsync()
    {
        await StopNowAsync();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _isRunning = false;
            var cts = Interlocked.Exchange(ref _workerCts, null);
            var task = Interlocked.Exchange(ref _workerTask, null);
            if (cts != null)
            {
                try
                {
                    cts.Cancel();
                    task?.GetAwaiter().GetResult();
                }
                catch { }
                finally
                {
                    cts.Dispose();
                }
            }
        }
    }
}
