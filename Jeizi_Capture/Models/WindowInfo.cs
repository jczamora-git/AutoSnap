using System.Drawing;

namespace AutoSnap.Models;

public class WindowInfo
{
    public IntPtr Hwnd { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public int ProcessId { get; set; }
    public Icon? Icon { get; set; }
    public Rectangle Bounds { get; set; }

    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Title) ? ProcessName : $"{Title} ({ProcessName})";
    }
}
