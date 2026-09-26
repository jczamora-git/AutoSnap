using NAudio.Wave;

namespace AutoSnap.Audio;

public class AudioChunk
{
    public byte[] Data { get; init; } = Array.Empty<byte>();
    public int Length { get; init; }
    public TimeSpan Timestamp { get; init; }
    public TimeSpan Duration { get; init; }
    public WaveFormat Format { get; init; } = new WaveFormat(16000, 16, 1);
    public float PeakRms { get; init; }
    public bool IsSilent { get; init; }
}
