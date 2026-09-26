using AutoSnap.Models;

namespace AutoSnap.Services;

public interface IMonitorEnumerationService
{
    IReadOnlyList<MonitorInfo> GetMonitors();
}

public class MonitorEnumerationService : IMonitorEnumerationService
{
    public IReadOnlyList<MonitorInfo> GetMonitors()
    {
        var monitors = new List<MonitorInfo>();
        var screens = Screen.AllScreens;

        for (int i = 0; i < screens.Length; i++)
        {
            var screen = screens[i];
            monitors.Add(new MonitorInfo
            {
                Index = i,
                DeviceName = screen.DeviceName,
                DisplayName = $"Display {i + 1}",
                Bounds = screen.Bounds,
                WorkingArea = screen.WorkingArea,
                IsPrimary = screen.Primary
            });
        }

        return monitors;
    }
}
