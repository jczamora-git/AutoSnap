using System.Drawing;
using System.Drawing.Imaging;
using AutoSnap.Models;
using AutoSnap.Native;

namespace AutoSnap.Capture;

public class WindowCaptureSource : ICaptureSource
{
    public IntPtr Hwnd { get; }
    public string Title { get; }
    public string ProcessName { get; }
    public CaptureSourceType SourceType => CaptureSourceType.Window;

    public string DisplayName => string.IsNullOrWhiteSpace(Title)
        ? $"{ProcessName} (Window)"
        : $"{Title} ({ProcessName})";

    public bool IsAvailable => NativeMethods.IsWindow(Hwnd) && !NativeMethods.IsWindowCloaked(Hwnd);

    public string StatusDescription
    {
        get
        {
            if (!NativeMethods.IsWindow(Hwnd))
                return "Selected window is no longer available (closed).";
            if (NativeMethods.IsIconic(Hwnd))
                return "Selected window is minimized.";
            if (NativeMethods.IsWindowCloaked(Hwnd))
                return "Selected window is cloaked or hidden.";
            return "Ready";
        }
    }

    public WindowCaptureSource(IntPtr hwnd, string title, string processName)
    {
        Hwnd = hwnd;
        Title = title;
        ProcessName = processName;
    }

    public Task<CapturedFrame?> CaptureAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!NativeMethods.IsWindow(Hwnd))
        {
            throw new InvalidOperationException("Selected window is no longer available.");
        }

        if (NativeMethods.IsIconic(Hwnd))
        {
            // Window is minimized - cannot capture full contents without restoring
            return Task.FromResult<CapturedFrame?>(null);
        }

        Rectangle bounds = NativeMethods.GetWindowBounds(Hwnd);
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return Task.FromResult<CapturedFrame?>(null);
        }

        Bitmap? bmp = null;
        IntPtr hdcWindow = IntPtr.Zero;
        IntPtr hdcMem = IntPtr.Zero;
        IntPtr hBitmap = IntPtr.Zero;
        IntPtr hOldBitmap = IntPtr.Zero;

        try
        {
            hdcWindow = NativeMethods.GetWindowDC(Hwnd);
            if (hdcWindow != IntPtr.Zero)
            {
                hdcMem = NativeMethods.CreateCompatibleDC(hdcWindow);
                hBitmap = NativeMethods.CreateCompatibleBitmap(hdcWindow, bounds.Width, bounds.Height);
                hOldBitmap = NativeMethods.SelectObject(hdcMem, hBitmap);

                bool success = NativeMethods.PrintWindow(Hwnd, hdcMem, NativeMethods.PW_RENDERFULLCONTENT);

                if (!success)
                {
                    // Fallback to standard PrintWindow without PW_RENDERFULLCONTENT
                    success = NativeMethods.PrintWindow(Hwnd, hdcMem, 0);
                }

                if (!success)
                {
                    // Fallback to BitBlt from Window DC
                    success = NativeMethods.BitBlt(hdcMem, 0, 0, bounds.Width, bounds.Height, hdcWindow, 0, 0, NativeMethods.SRCCOPY);
                }

                if (success)
                {
                    using (Bitmap tempBmp = Image.FromHbitmap(hBitmap))
                    {
                        // Clone to detach from GDI handle so we can safely delete GDI objects
                        bmp = new Bitmap(tempBmp.Width, tempBmp.Height, PixelFormat.Format32bppArgb);
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.DrawImage(tempBmp, 0, 0);
                        }
                    }
                }
            }
        }
        catch
        {
            bmp?.Dispose();
            throw;
        }
        finally
        {
            if (hOldBitmap != IntPtr.Zero && hdcMem != IntPtr.Zero)
            {
                NativeMethods.SelectObject(hdcMem, hOldBitmap);
            }
            if (hBitmap != IntPtr.Zero)
            {
                NativeMethods.DeleteObject(hBitmap);
            }
            if (hdcMem != IntPtr.Zero)
            {
                NativeMethods.DeleteDC(hdcMem);
            }
            if (hdcWindow != IntPtr.Zero)
            {
                NativeMethods.ReleaseDC(Hwnd, hdcWindow);
            }
        }

        if (bmp == null)
        {
            // Final fallback: Screen copy bounded by window rectangle if visible on screen
            bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            }
        }

        string sourceName = string.IsNullOrWhiteSpace(ProcessName) ? "Window" : ProcessName;
        return Task.FromResult<CapturedFrame?>(new CapturedFrame(bmp, sourceName));
    }

    public void Dispose()
    {
        // No persistent unmanaged resources to dispose
        GC.SuppressFinalize(this);
    }
}
