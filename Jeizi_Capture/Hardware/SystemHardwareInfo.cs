namespace AutoSnap.Hardware;

public class SystemHardwareInfo
{
    public string CpuName { get; init; } = "Unknown CPU";
    public int LogicalCoreCount { get; init; } = 4;
    public int PhysicalCoreCount { get; init; } = 2;
    public long TotalPhysicalMemoryBytes { get; init; } = 8L * 1024 * 1024 * 1024;
    public long AvailablePhysicalMemoryBytes { get; init; } = 4L * 1024 * 1024 * 1024;
    public string GpuName { get; init; } = "Unknown / Integrated";
    public long DedicatedVramBytes { get; init; }
    public string OsVersion { get; init; } = "Windows";
    public string Architecture { get; init; } = "x64";
    public string WhisperBackend { get; init; } = "CPU (Whisper.net runtime)";

    public string TotalMemoryFormatted => $"{Math.Round(TotalPhysicalMemoryBytes / (1024.0 * 1024.0 * 1024.0))} GB";
    public string DedicatedVramFormatted => DedicatedVramBytes > 0
        ? $"{Math.Round(DedicatedVramBytes / (1024.0 * 1024.0 * 1024.0))} GB"
        : "Shared / Integrated";
}
