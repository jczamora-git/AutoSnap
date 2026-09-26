using System.Drawing;
using AutoSnap.Capture;
using AutoSnap.Models;
using AutoSnap.Services;

namespace AutoSnap.Forms;

public partial class SourcePickerForm : Form
{
    private readonly IWindowEnumerationService _windowService;
    private readonly IMonitorEnumerationService _monitorService;
    private readonly ImageList _windowImageList;

    public ICaptureSource? SelectedSource { get; private set; }

    public SourcePickerForm(
        IWindowEnumerationService windowService,
        IMonitorEnumerationService monitorService,
        CaptureSourceType initialTab = CaptureSourceType.Window)
    {
        _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
        _monitorService = monitorService ?? throw new ArgumentNullException(nameof(monitorService));

        _windowImageList = new ImageList
        {
            ImageSize = new Size(24, 24),
            ColorDepth = ColorDepth.Depth32Bit
        };

        InitializeComponent();

        listViewWindows.SmallImageList = _windowImageList;

        if (initialTab == CaptureSourceType.Monitor)
        {
            tabControlSources.SelectedTab = tabMonitors;
        }
        else
        {
            tabControlSources.SelectedTab = tabWindows;
        }

        RefreshWindows();
        RefreshMonitors();
    }

    private void RefreshWindows()
    {
        listViewWindows.BeginUpdate();
        listViewWindows.Items.Clear();
        _windowImageList.Images.Clear();

        string filter = txtSearch.Text.Trim();
        var windows = _windowService.GetOpenWindows();

        int imageIndex = 0;
        foreach (var win in windows)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                bool matches = win.Title.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                win.ProcessName.Contains(filter, StringComparison.OrdinalIgnoreCase);
                if (!matches) continue;
            }

            if (win.Icon != null)
            {
                _windowImageList.Images.Add(win.Icon);
            }
            else
            {
                _windowImageList.Images.Add(SystemIcons.Application);
            }

            var item = new ListViewItem(win.Title, imageIndex++)
            {
                Tag = win
            };
            item.SubItems.Add(win.ProcessName);
            item.SubItems.Add($"0x{win.Hwnd.ToInt64():X}");
            item.SubItems.Add($"{win.Bounds.Width} × {win.Bounds.Height}");

            listViewWindows.Items.Add(item);
        }

        listViewWindows.EndUpdate();
        lblWindowCount.Text = $"{listViewWindows.Items.Count} window(s) found";
    }

    private void RefreshMonitors()
    {
        listViewMonitors.BeginUpdate();
        listViewMonitors.Items.Clear();

        var monitors = _monitorService.GetMonitors();
        foreach (var mon in monitors)
        {
            var item = new ListViewItem(mon.DisplayName)
            {
                Tag = mon
            };
            item.SubItems.Add($"{mon.Bounds.Width} × {mon.Bounds.Height}");
            item.SubItems.Add(mon.IsPrimary ? "Yes" : "No");
            item.SubItems.Add($"({mon.Bounds.X}, {mon.Bounds.Y})");

            listViewMonitors.Items.Add(item);
        }

        listViewMonitors.EndUpdate();
    }

    private void btnRefresh_Click(object sender, EventArgs e)
    {
        if (tabControlSources.SelectedTab == tabWindows)
        {
            RefreshWindows();
        }
        else
        {
            RefreshMonitors();
        }
    }

    private void txtSearch_TextChanged(object sender, EventArgs e)
    {
        RefreshWindows();
    }

    private void btnSelect_Click(object sender, EventArgs e)
    {
        ConfirmSelection();
    }

    private void listViewWindows_DoubleClick(object sender, EventArgs e)
    {
        ConfirmSelection();
    }

    private void listViewMonitors_DoubleClick(object sender, EventArgs e)
    {
        ConfirmSelection();
    }

    private void ConfirmSelection()
    {
        if (tabControlSources.SelectedTab == tabWindows)
        {
            if (listViewWindows.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "Please select an application window.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (listViewWindows.SelectedItems[0].Tag is WindowInfo win)
            {
                SelectedSource = new WindowCaptureSource(win.Hwnd, win.Title, win.ProcessName);
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        else if (tabControlSources.SelectedTab == tabMonitors)
        {
            if (listViewMonitors.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "Please select a monitor / display.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (listViewMonitors.SelectedItems[0].Tag is MonitorInfo mon)
            {
                SelectedSource = new MonitorCaptureSource(mon);
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
