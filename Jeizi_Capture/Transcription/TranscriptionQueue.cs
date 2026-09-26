using System.Collections.Concurrent;
using System.Diagnostics;
using AutoSnap.Audio;

namespace AutoSnap.Transcription;

public class TranscriptionQueue : IDisposable
{
    private readonly ITranscriptionService _transcriptionService;
    private readonly AudioBuffer _audioBuffer;
    private readonly ConcurrentQueue<AudioChunk> _chunkQueue = new();
    private readonly CancellationTokenSource _cts = new();
    private Task? _workerTask;
    private bool _isRunning;
    private bool _disposed;

    private readonly List<TranscriptSegment> _allSegments = new();
    private readonly object _segmentsLock = new();

    private TimeSpan _liveCaptureTime = TimeSpan.Zero;
    private TimeSpan _processedTime = TimeSpan.Zero;
    private readonly Stopwatch _liveStopwatch = new();

    public event EventHandler<TranscriptSegment>? SegmentProduced;
    public event EventHandler<(TimeSpan LiveTime, TimeSpan ProcessedTime, double DelaySeconds)>? LatencyUpdated;
    public event EventHandler<Exception>? ErrorOccurred;

    public TranscriptionLanguageMode LanguageMode { get; set; } = TranscriptionLanguageMode.Taglish;
    public TimeSpan LiveTime => _liveStopwatch.Elapsed;
    public TimeSpan ProcessedTime => _processedTime;
    public double DelaySeconds => Math.Max(0, (_liveStopwatch.Elapsed - _processedTime).TotalSeconds);

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
        if (_isRunning) return;
        _isRunning = true;
        _liveStopwatch.Restart();
        _workerTask = Task.Run(() => WorkerLoopAsync(_cts.Token));
    }

    public void EnqueueAudio(byte[] pcmData, int offset, int count)
    {
        if (!_isRunning || count <= 0) return;

        _audioBuffer.AddAudio(pcmData, offset, count);
        var readyChunks = _audioBuffer.ExtractReadyChunks();

        foreach (var chunk in readyChunks)
        {
            _chunkQueue.Enqueue(chunk);
        }
    }

    private async Task WorkerLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _isRunning)
        {
            try
            {
                if (_chunkQueue.TryDequeue(out var chunk))
                {
                    if (!chunk.IsSilent && chunk.Data.Length > 0)
                    {
                        var segments = await _transcriptionService.TranscribeAudioBytesAsync(
                            chunk.Data,
                            LanguageMode,
                            chunk.Timestamp,
                            cancellationToken);

                        foreach (var segment in segments)
                        {
                            AddSegmentWithOverlapSuppression(segment);
                        }
                    }

                    _processedTime = chunk.Timestamp + chunk.Duration;
                    LatencyUpdated?.Invoke(this, (LiveTime, _processedTime, DelaySeconds));
                }
                else
                {
                    await Task.Delay(100, cancellationToken);
                    LatencyUpdated?.Invoke(this, (LiveTime, _processedTime, DelaySeconds));
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

                // Check for duplicate or overlapping prefix text
                string lastText = last.Text.Trim();
                string newText = newSegment.Text.Trim();

                if (string.Equals(lastText, newText, StringComparison.OrdinalIgnoreCase))
                {
                    // Exact duplicate from overlap window
                    return;
                }

                // Check if last segment ends with start of new segment (suffix/prefix overlap)
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

    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        // Extract remaining audio from buffer
        var remainingChunks = _audioBuffer.ExtractReadyChunks(isFinalizing: true);
        foreach (var chunk in remainingChunks)
        {
            _chunkQueue.Enqueue(chunk);
        }

        while (!_chunkQueue.IsEmpty)
        {
            if (_chunkQueue.TryDequeue(out var chunk))
            {
                if (!chunk.IsSilent && chunk.Data.Length > 0)
                {
                    var segments = await _transcriptionService.TranscribeAudioBytesAsync(
                        chunk.Data,
                        LanguageMode,
                        chunk.Timestamp,
                        cancellationToken);

                    foreach (var segment in segments)
                    {
                        AddSegmentWithOverlapSuppression(segment);
                    }
                }
                _processedTime = chunk.Timestamp + chunk.Duration;
            }
        }
    }

    public async Task StopAsync()
    {
        _isRunning = false;
        _liveStopwatch.Stop();
        _cts.Cancel();

        if (_workerTask != null)
        {
            try
            {
                await _workerTask;
            }
            catch { }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _isRunning = false;
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
