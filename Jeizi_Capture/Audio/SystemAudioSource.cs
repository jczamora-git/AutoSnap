using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace AutoSnap.Audio;

public class SystemAudioSource : IAudioSource
{
    private WasapiLoopbackCapture? _capture;
    private AudioResampler? _resampler;
    private readonly MMDevice? _device;
    private readonly byte[] _readBuffer = new byte[16384];
    private bool _isCapturing;
    private bool _disposed;
    private readonly object _lock = new();

    public event EventHandler<AudioChunk>? AudioDataAvailable;
    public event EventHandler<Exception>? ErrorOccurred;
    public event EventHandler? Stopped;

    public string SourceName => _device?.FriendlyName ?? "Default System Audio";
    public bool IsCapturing => _isCapturing;
    public WaveFormat Format { get; } = new WaveFormat(16000, 16, 1);

    public SystemAudioSource(MMDevice? device = null)
    {
        _device = device;
    }

    public static List<MMDevice> GetRenderDevices()
    {
        var list = new List<MMDevice>();
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            foreach (var dev in devices)
            {
                list.Add(dev);
            }
        }
        catch { }
        return list;
    }

    public static MMDevice? GetDefaultRenderDevice()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            return enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }
        catch
        {
            return null;
        }
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_isCapturing) return Task.CompletedTask;

            try
            {
                _capture = _device != null
                    ? new WasapiLoopbackCapture(_device)
                    : new WasapiLoopbackCapture();

                _resampler = new AudioResampler(_capture.WaveFormat, 16000, 1);

                _capture.DataAvailable += OnDataAvailable;
                _capture.RecordingStopped += OnRecordingStopped;

                _capture.StartRecording();
                _isCapturing = true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
                throw;
            }
        }

        return Task.CompletedTask;
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (!_isCapturing || _resampler == null || e.BytesRecorded == 0)
            return;

        try
        {
            _resampler.AddSamples(e.Buffer, 0, e.BytesRecorded);

            int bytesRead;
            while ((bytesRead = _resampler.ReadResampledBytes(_readBuffer, 0, _readBuffer.Length)) > 0)
            {
                byte[] chunkBytes = new byte[bytesRead];
                Array.Copy(_readBuffer, 0, chunkBytes, 0, bytesRead);

                float rms = AudioLevelAnalyzer.CalculateRms16BitMono(chunkBytes, 0, bytesRead);

                var chunk = new AudioChunk
                {
                    Data = chunkBytes,
                    Length = bytesRead,
                    Timestamp = DateTime.Now.TimeOfDay,
                    Duration = TimeSpan.FromSeconds((double)bytesRead / 32000),
                    Format = Format,
                    PeakRms = rms,
                    IsSilent = AudioLevelAnalyzer.IsSilent(rms)
                };

                AudioDataAvailable?.Invoke(this, chunk);
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
        }
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        _isCapturing = false;
        if (e.Exception != null)
        {
            ErrorOccurred?.Invoke(this, e.Exception);
        }
        Stopped?.Invoke(this, EventArgs.Empty);
    }

    public Task StopAsync()
    {
        lock (_lock)
        {
            if (!_isCapturing) return Task.CompletedTask;

            try
            {
                _isCapturing = false;
                _capture?.StopRecording();
            }
            catch { }
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            try
            {
                StopAsync().GetAwaiter().GetResult();
                _capture?.Dispose();
                _resampler?.Dispose();
            }
            catch { }
        }
    }
}
