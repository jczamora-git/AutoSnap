namespace AutoSnap.Models;

public class ChromeSettings
{
    public string? CustomChromeExecutablePath { get; set; }
    public string ProfileDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AutoSnap",
        "ChromeProfile");
    public bool AutoLaunchChrome { get; set; } = false;
    public bool CloseChromeOnExit { get; set; } = true;
    public int DebugPort { get; set; } = 0;
}
