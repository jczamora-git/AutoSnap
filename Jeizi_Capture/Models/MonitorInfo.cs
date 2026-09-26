using System.Drawing;

namespace AutoSnap.Models;

public class MonitorInfo
{
    public int Index { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public Rectangle Bounds { get; set; }
    public Rectangle WorkingArea { get; set; }
    public bool IsPrimary { get; set; }

    public override string ToString()
    {
        return $"{DisplayName} ({Bounds.Width} × {Bounds.Height}){(IsPrimary ? " [Primary]" : string.Empty)}";
    }
}
