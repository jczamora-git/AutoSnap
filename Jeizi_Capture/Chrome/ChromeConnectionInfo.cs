namespace AutoSnap.Chrome;

public sealed class ChromeConnectionInfo
{
    public string ExecutablePath { get; init; } = string.Empty;
    public string ProfileDirectory { get; init; } = string.Empty;
    public int DebuggingPort { get; init; }
    public string DebuggingEndpoint => $"http://127.0.0.1:{DebuggingPort}";
    public int? ProcessId { get; init; }
    public bool StartedByAutoSnap { get; init; }
}
