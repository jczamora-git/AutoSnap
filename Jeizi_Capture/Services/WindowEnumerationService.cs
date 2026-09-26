using System.Diagnostics;
using System.Drawing;
using System.Text;
using AutoSnap.Models;
using AutoSnap.Native;

namespace AutoSnap.Services;

public interface IWindowEnumerationService
{
    IReadOnlyList<WindowInfo> GetOpenWindows();
}

public class WindowEnumerationService : IWindowEnumerationService
{
    private static readonly HashSet<string> ExcludedProcessNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "ShellExperienceHost",
        "SearchHost",
        "StartMenuExperienceHost",
        "TextInputHost",
        "ApplicationFrameHost",
        "SystemSettings",
        "LockApp"
    };

    public IReadOnlyList<WindowInfo> GetOpenWindows()
    {
        var windows = new List<WindowInfo>();
        int currentProcessId = Environment.ProcessId;

        NativeMethods.EnumWindows((hWnd, lParam) =>
        {
            if (!NativeMethods.IsWindowVisible(hWnd))
                return true;

            if (NativeMethods.IsWindowCloaked(hWnd))
                return true;

            int style = NativeMethods.GetWindowLong(hWnd, NativeMethods.GWL_STYLE);
            int exStyle = NativeMethods.GetWindowLong(hWnd, NativeMethods.GWL_EXSTYLE);

            // Exclude tool windows that shouldn't appear in task switcher
            if ((exStyle & NativeMethods.WS_EX_TOOLWINDOW) != 0 && (exStyle & NativeMethods.WS_EX_APPWINDOW) == 0)
                return true;

            int textLength = NativeMethods.GetWindowTextLength(hWnd);
            if (textLength <= 0)
                return true;

            var sb = new StringBuilder(textLength + 1);
            NativeMethods.GetWindowText(hWnd, sb, sb.Capacity);
            string title = sb.ToString().Trim();

            if (string.IsNullOrWhiteSpace(title) ||
                title == "Program Manager" ||
                title == "Windows Input Experience")
            {
                return true;
            }

            NativeMethods.GetWindowThreadProcessId(hWnd, out uint processId);
            if (processId == 0 || processId == currentProcessId)
                return true;

            string processName = "Unknown";
            Icon? icon = null;

            try
            {
                using var proc = Process.GetProcessById((int)processId);
                processName = proc.ProcessName;

                if (ExcludedProcessNames.Contains(processName))
                    return true;

                // Try to extract window icon, or fall back to process icon
                IntPtr hIcon = NativeMethods.SendMessage(hWnd, NativeMethods.WM_GETICON, (IntPtr)NativeMethods.ICON_SMALL2, IntPtr.Zero);
                if (hIcon == IntPtr.Zero)
                    hIcon = NativeMethods.SendMessage(hWnd, NativeMethods.WM_GETICON, (IntPtr)NativeMethods.ICON_SMALL, IntPtr.Zero);
                if (hIcon == IntPtr.Zero)
                    hIcon = NativeMethods.SendMessage(hWnd, NativeMethods.WM_GETICON, (IntPtr)NativeMethods.ICON_BIG, IntPtr.Zero);

                if (hIcon != IntPtr.Zero)
                {
                    try { icon = (Icon)Icon.FromHandle(hIcon).Clone(); } catch { }
                }

                if (icon == null)
                {
                    try
                    {
                        string? mainModule = proc.MainModule?.FileName;
                        if (!string.IsNullOrEmpty(mainModule) && File.Exists(mainModule))
                        {
                            icon = Icon.ExtractAssociatedIcon(mainModule);
                        }
                    }
                    catch
                    {
                        // Some system processes deny access to MainModule
                    }
                }
            }
            catch
            {
                // Process may have exited or access denied
            }

            Rectangle bounds = NativeMethods.GetWindowBounds(hWnd);
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return true;

            windows.Add(new WindowInfo
            {
                Hwnd = hWnd,
                Title = title,
                ProcessName = processName,
                ProcessId = (int)processId,
                Icon = icon,
                Bounds = bounds
            });

            return true;
        }, IntPtr.Zero);

        return windows.OrderBy(w => w.Title).ToList();
    }
}
