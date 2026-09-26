using System.Diagnostics;
using AutoSnap.Capture;
using AutoSnap.Models;

namespace AutoSnap.Services;

public class SnapshotScheduler : IDisposable
{
    private readonly SnapshotService _snapshotService;
    private readonly SemaphoreSlim _captureLock = new(1, 1);

    private CancellationTokenSource? _sessionCts;
    private Task? _schedulerTask;
    private ICaptureSource? _currentSource;
    private CaptureSettings? _currentSettings;

    private readonly object _stateLock = new();
    private CaptureSessionState _state = CaptureSessionState.Idle;
    private bool _isPaused;
    private DateTime _sessionStartTime;
    private DateTime _nextScheduledCaptureTime;
    private int _capturedCount;
    private bool _disposed;

    public CaptureSessionState State
    {
        get { lock (_stateLock) return _state; }
        private set
        {
            lock (_stateLock)
            {
                if (_state == value) return;
                _state = value;
            }
            StateChanged?.Invoke(this, value);
        }
    }

    public int SnapshotsCapturedCount => _capturedCount;
    public TimeSpan SessionDuration => State == CaptureSessionState.Idle
        ? TimeSpan.Zero
        : DateTime.Now - _sessionStartTime;

    public event EventHandler<CaptureSessionState>? StateChanged;
    public event EventHandler<SnapshotRecord>? SnapshotSaved;
    public event EventHandler<(string Message, Exception? Exception)>? CaptureError;
    public event EventHandler<(TimeSpan Remaining, TimeSpan Duration, int TotalCaptured)>? StatusTicked;

    public SnapshotScheduler(SnapshotService snapshotService)
    {
        _snapshotService = snapshotService ?? throw new ArgumentNullException(nameof(snapshotService));
    }

    public async Task StartAsync(ICaptureSource source, CaptureSettings settings)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (settings == null) throw new ArgumentNullException(nameof(settings));
        if (settings.Interval <= TimeSpan.Zero)
            throw new ArgumentException("Interval must be greater than zero.", nameof(settings));

        await _captureLock.WaitAsync();
        try
        {
            if (State == CaptureSessionState.Capturing || State == CaptureSessionState.Starting || State == CaptureSessionState.Paused)
            {
                return; // Already running
            }

            // Ensure any lingering task is cleanly stopped before starting a new session
            if (_sessionCts != null || _schedulerTask != null)
            {
                var oldCts = Interlocked.Exchange(ref _sessionCts, null);
                var oldTask = Interlocked.Exchange(ref _schedulerTask, null);
                if (oldCts != null)
                {
                    try
                    {
                        oldCts.Cancel();
                        if (oldTask != null) await oldTask.ConfigureAwait(false);
                    }
                    catch { }
                    finally { oldCts.Dispose(); }
                }
            }

            _currentSource = source;
            _currentSettings = settings.Clone();
            _capturedCount = 0;
            _isPaused = false;
            _sessionStartTime = DateTime.Now;

            var cts = new CancellationTokenSource();
            _sessionCts = cts;
            CancellationToken token = cts.Token;

            State = CaptureSessionState.Starting;
            Debug.WriteLine("[SnapshotScheduler] Session started");

            // Take initial snapshot immediately on start
            _nextScheduledCaptureTime = DateTime.Now.Add(_currentSettings.Interval);
            State = CaptureSessionState.Capturing;

            _ = PerformCaptureAndSaveAsync(_currentSource, _currentSettings, isScheduled: true);

            _schedulerTask = Task.Run(() => SchedulerLoopAsync(token), token);
        }
        finally
        {
            _captureLock.Release();
        }
    }

    public void Pause()
    {
        lock (_stateLock)
        {
            if (_state != CaptureSessionState.Capturing) return;
            _isPaused = true;
            State = CaptureSessionState.Paused;
            Debug.WriteLine("[SnapshotScheduler] Session paused");
        }
    }

    public void Resume()
    {
        lock (_stateLock)
        {
            if (_state != CaptureSessionState.Paused) return;
            _isPaused = false;
            // Shift next capture time from current moment
            if (_currentSettings != null)
            {
                _nextScheduledCaptureTime = DateTime.Now.Add(_currentSettings.Interval);
            }
            State = CaptureSessionState.Capturing;
            Debug.WriteLine("[SnapshotScheduler] Session resumed");
        }
    }

    public async Task StopAsync()
    {
        await _captureLock.WaitAsync();
        try
        {
            if (State == CaptureSessionState.Idle || State == CaptureSessionState.Stopping)
                return;

            State = CaptureSessionState.Stopping;
            _isPaused = false;

            var cts = Interlocked.Exchange(ref _sessionCts, null);
            var task = Interlocked.Exchange(ref _schedulerTask, null);

            if (cts != null)
            {
                try
                {
                    cts.Cancel();
                    Debug.WriteLine("[SnapshotScheduler] Cancellation requested");

                    if (task != null)
                    {
                        try
                        {
                            await task.ConfigureAwait(false);
                            Debug.WriteLine("[SnapshotScheduler] Worker task completed");
                        }
                        catch (OperationCanceledException)
                        {
                            // Expected during normal shutdown.
                        }
                        catch (Exception ex)
                        {
                            CaptureError?.Invoke(this, ("Scheduler stopped with error.", ex));
                        }
                    }
                }
                finally
                {
                    cts.Dispose();
                    Debug.WriteLine("[SnapshotScheduler] CTS disposed");
                }
            }

            State = CaptureSessionState.Idle;
        }
        finally
        {
            _captureLock.Release();
        }
    }

    public async Task<SnapshotRecord?> TriggerManualSnapshotAsync(ICaptureSource? fallbackSource = null, CaptureSettings? fallbackSettings = null)
    {
        ICaptureSource? source = _currentSource ?? fallbackSource;
        CaptureSettings? settings = _currentSettings ?? fallbackSettings;

        if (source == null || settings == null)
            return null;

        // Perform capture asynchronously without interrupting or resetting scheduled timer cadence
        return await PerformCaptureAndSaveAsync(source, settings, isScheduled: false);
    }

    private async Task SchedulerLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(200, cancellationToken);

                if (_isPaused || State != CaptureSessionState.Capturing || _currentSettings == null || _currentSource == null)
                {
                    continue;
                }

                DateTime now = DateTime.Now;
                TimeSpan remaining = _nextScheduledCaptureTime > now
                    ? _nextScheduledCaptureTime - now
                    : TimeSpan.Zero;

                TimeSpan duration = now - _sessionStartTime;
                StatusTicked?.Invoke(this, (remaining, duration, _capturedCount));

                if (now >= _nextScheduledCaptureTime)
                {
                    // Advance next scheduled interval
                    _nextScheduledCaptureTime = now.Add(_currentSettings.Interval);
                    _ = PerformCaptureAndSaveAsync(_currentSource, _currentSettings, isScheduled: true);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                CaptureError?.Invoke(this, ($"Error during scheduler loop: {ex.Message}", ex));
            }
        }
    }

    private async Task<SnapshotRecord?> PerformCaptureAndSaveAsync(
        ICaptureSource source,
        CaptureSettings settings,
        bool isScheduled)
    {
        CapturedFrame? frame = null;
        try
        {
            if (!source.IsAvailable)
            {
                string msg = source.StatusDescription;
                CaptureError?.Invoke(this, (msg, null));
                if (isScheduled && State == CaptureSessionState.Capturing)
                {
                    State = CaptureSessionState.Error;
                }
                return null;
            }

            frame = await source.CaptureAsync();
            if (frame == null)
            {
                // Window might be minimized
                return null;
            }

            var record = await _snapshotService.ProcessAndSaveFrameAsync(frame, settings);
            if (record != null)
            {
                Interlocked.Increment(ref _capturedCount);
                SnapshotSaved?.Invoke(this, record);
                return record;
            }
        }
        catch (Exception ex)
        {
            CaptureError?.Invoke(this, ($"Capture failed: {ex.Message}", ex));
            if (isScheduled && !source.IsAvailable)
            {
                State = CaptureSessionState.Error;
            }
        }
        finally
        {
            frame?.Dispose();
        }

        return null;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            var cts = Interlocked.Exchange(ref _sessionCts, null);
            var task = Interlocked.Exchange(ref _schedulerTask, null);
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
            _captureLock.Dispose();
        }
    }
}
