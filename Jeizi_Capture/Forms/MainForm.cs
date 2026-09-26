using System.Diagnostics;
using System.Drawing;
using AutoSnap.BrowserCapture;
using AutoSnap.Capture;
using AutoSnap.Models;
using AutoSnap.Services;

namespace AutoSnap.Forms;

public partial class MainForm : Form
{
    private readonly AppSettings _appSettings;
    private readonly SettingsService _settingsService;
    private readonly IWindowEnumerationService _windowService;
    private readonly IMonitorEnumerationService _monitorService;
    private readonly BrowserCaptureServer _browserCaptureServer;
    private readonly StorageService _storageService;
    private readonly SnapshotService _snapshotService;
    private readonly SnapshotScheduler _scheduler;

    private ICaptureSource? _selectedSource;
    private readonly System.Windows.Forms.Timer _previewTimer;
    private bool _isPreviewBusy;
    private bool _isExplicitExit;

    public MainForm()
    {
        InitializeComponent();

        _settingsService = new SettingsService();
        _appSettings = _settingsService.LoadSettings();

        _windowService = new WindowEnumerationService();
        _monitorService = new MonitorEnumerationService();
        _browserCaptureServer = new BrowserCaptureServer();
        _storageService = new StorageService();
        _snapshotService = new SnapshotService(_storageService);
        _scheduler = new SnapshotScheduler(_snapshotService);

        _scheduler.StateChanged += Scheduler_StateChanged;
        _scheduler.SnapshotSaved += Scheduler_SnapshotSaved;
        _scheduler.CaptureError += Scheduler_CaptureError;
        _scheduler.StatusTicked += Scheduler_StatusTicked;

        _browserCaptureServer.StreamStarted += BrowserCaptureServer_StreamStarted;
        _browserCaptureServer.StreamEnded += BrowserCaptureServer_StreamEnded;
        _browserCaptureServer.SessionConnected += BrowserCaptureServer_SessionConnected;
        _browserCaptureServer.SessionDisconnected += BrowserCaptureServer_SessionDisconnected;

        _previewTimer = new System.Windows.Forms.Timer
        {
            Interval = 800
        };
        _previewTimer.Tick += PreviewTimer_Tick;

        ApplySettingsToUi();
        UpdateStateUi(CaptureSessionState.Idle);
        SelectDefaultSource();
    }

    private void ApplySettingsToUi()
    {
        var cap = _appSettings.Capture;

        // Set interval combobox
        int intervalSec = (int)cap.Interval.TotalSeconds;
        switch (intervalSec)
        {
            case 1: cmbInterval.SelectedIndex = 0; break;
            case 5: cmbInterval.SelectedIndex = 1; break;
            case 10: cmbInterval.SelectedIndex = 2; break;
            case 30: cmbInterval.SelectedIndex = 3; break;
            case 60: cmbInterval.SelectedIndex = 4; break;
            case 300: cmbInterval.SelectedIndex = 5; break;
            case 600: cmbInterval.SelectedIndex = 6; break;
            case 1800: cmbInterval.SelectedIndex = 7; break;
            case 3600: cmbInterval.SelectedIndex = 8; break;
            default:
                cmbInterval.SelectedIndex = 9; // Custom
                numCustomInterval.Value = Math.Max(1, decimal.TryParse(cap.CustomIntervalText, out var v) ? v : 5);
                cmbCustomUnit.SelectedItem = cap.CustomIntervalUnit;
                break;
        }

        lblOutputDir.Text = _appSettings.Capture.OutputFolder;
    }

    private void SelectDefaultSource()
    {
        // Default to Primary Monitor
        var monitors = _monitorService.GetMonitors();
        var primary = monitors.FirstOrDefault(m => m.IsPrimary) ?? monitors.FirstOrDefault();
        if (primary != null)
        {
            SetSource(new MonitorCaptureSource(primary));
            rbSourceMonitor.Checked = true;
        }
        else
        {
            rbSourceWindow.Checked = true;
        }

        _previewTimer.Start();
    }

    private void SetSource(ICaptureSource? source)
    {
        _selectedSource?.Dispose();
        _selectedSource = source;

        if (_selectedSource != null)
        {
            lblSelectedSource.Text = _selectedSource.DisplayName;
            lblSelectedSource.ForeColor = Color.FromArgb(30, 30, 30);
            btnStart.Enabled = _scheduler.State == CaptureSessionState.Idle && _selectedSource.IsAvailable;
            btnTakeSnapshot.Enabled = _selectedSource.IsAvailable;

            if (_selectedSource is BrowserCaptureSource browserSource)
            {
                UpdateChromeTabUi(browserSource);
            }
            else
            {
                lblSelectedSubtext.Text = _selectedSource is WindowCaptureSource ? "Application Window" : "Display / Monitor";
                lblChromeStatusBadge.Visible = false;
                btnOpenChrome.Visible = false;
                btnStopChrome.Visible = false;
                btnSelectSource.Text = "Change...";
            }
        }
        else
        {
            lblSelectedSource.Text = "None (Click 'Change...' to choose)";
            lblSelectedSource.ForeColor = Color.Gray;
            lblSelectedSubtext.Text = string.Empty;
            lblChromeStatusBadge.Visible = false;
            btnOpenChrome.Visible = false;
            btnStopChrome.Visible = false;
            btnStart.Enabled = false;
            btnTakeSnapshot.Enabled = false;
            btnSelectSource.Text = "Select...";
        }

        _ = UpdatePreviewAsync();
    }

    private void UpdateChromeTabUi(BrowserCaptureSource browserSource)
    {
        lblChromeStatusBadge.Visible = true;

        if (_browserCaptureServer.HasActiveStream)
        {
            lblSelectedSource.Text = browserSource.StreamTitle;
            lblSelectedSubtext.Text = "Chrome Tab (MediaStream)";
            lblChromeStatusBadge.Text = "● Connected";
            lblChromeStatusBadge.ForeColor = Color.SeaGreen;
            btnSelectSource.Text = "Change Tab...";
            btnOpenChrome.Visible = false;
            btnStopChrome.Visible = true;
            btnStart.Enabled = _scheduler.State == CaptureSessionState.Idle;
            btnTakeSnapshot.Enabled = true;
        }
        else
        {
            lblSelectedSource.Text = "No tab selected";
            lblSelectedSubtext.Text = "Use a tab from your existing Chrome session.";
            lblChromeStatusBadge.Text = _browserCaptureServer.ActiveSession != null ? "Waiting for source..." : "Not Connected";
            lblChromeStatusBadge.ForeColor = Color.DarkOrange;
            btnSelectSource.Text = "Choose Chrome Tab";
            btnOpenChrome.Visible = true;
            btnStopChrome.Visible = false;
            btnStart.Enabled = false;
            btnTakeSnapshot.Enabled = false;
        }
    }

    private TimeSpan GetSelectedInterval()
    {
        return cmbInterval.SelectedIndex switch
        {
            0 => TimeSpan.FromSeconds(1),
            1 => TimeSpan.FromSeconds(5),
            2 => TimeSpan.FromSeconds(10),
            3 => TimeSpan.FromSeconds(30),
            4 => TimeSpan.FromMinutes(1),
            5 => TimeSpan.FromMinutes(5),
            6 => TimeSpan.FromMinutes(10),
            7 => TimeSpan.FromMinutes(30),
            8 => TimeSpan.FromHours(1),
            _ => CalculateCustomInterval()
        };
    }

    private TimeSpan CalculateCustomInterval()
    {
        double val = (double)numCustomInterval.Value;
        string unit = cmbCustomUnit.SelectedItem?.ToString() ?? "Seconds";

        return unit switch
        {
            "Minutes" => TimeSpan.FromMinutes(val),
            "Hours" => TimeSpan.FromHours(val),
            _ => TimeSpan.FromSeconds(val)
        };
    }

    private async void PreviewTimer_Tick(object? sender, EventArgs e)
    {
        if (_isPreviewBusy || _selectedSource == null)
            return;

        // In minimized state, avoid computing preview to save CPU
        if (WindowState == FormWindowState.Minimized)
            return;

        await UpdatePreviewAsync();
    }

    private async Task UpdatePreviewAsync()
    {
        if (_selectedSource == null)
        {
            ClearPreview("No source selected");
            return;
        }

        if (!_selectedSource.IsAvailable)
        {
            string status = _selectedSource.StatusDescription;
            ClearPreview(status);
            return;
        }

        _isPreviewBusy = true;
        try
        {
            using var frame = await _selectedSource.CaptureAsync();
            if (frame?.Image != null)
            {
                var oldImg = picPreview.Image;
                picPreview.Image = new Bitmap(frame.Image);
                oldImg?.Dispose();
                lblPreviewStatus.Visible = false;
            }
        }
        catch
        {
            if (_selectedSource is BrowserCaptureSource && !_selectedSource.IsAvailable)
            {
                ClearPreview("Waiting for Chrome tab selection...");
            }
        }
        finally
        {
            _isPreviewBusy = false;
        }
    }

    private void ClearPreview(string message)
    {
        var oldImg = picPreview.Image;
        picPreview.Image = null;
        oldImg?.Dispose();

        lblPreviewStatus.Text = message;
        lblPreviewStatus.Visible = true;
    }

    private void btnSelectSource_Click(object sender, EventArgs e)
    {
        if (rbSourceChrome.Checked)
        {
            try
            {
                if (!_browserCaptureServer.IsRunning)
                {
                    _browserCaptureServer.Start();
                }

                _browserCaptureServer.OpenCapturePageInBrowser(_appSettings.Chrome.CustomChromeExecutablePath);

                if (_selectedSource is not BrowserCaptureSource)
                {
                    SetSource(new BrowserCaptureSource(_browserCaptureServer));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to open capture page: {ex.Message}", "Chrome Capture", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return;
        }

        var initialTab = rbSourceMonitor.Checked
            ? CaptureSourceType.Monitor
            : CaptureSourceType.Window;

        using var srcPicker = new SourcePickerForm(_windowService, _monitorService, initialTab);
        if (srcPicker.ShowDialog(this) == DialogResult.OK && srcPicker.SelectedSource != null)
        {
            if (srcPicker.SelectedSource.SourceType == CaptureSourceType.Window)
            {
                rbSourceWindow.Checked = true;
            }
            else
            {
                rbSourceMonitor.Checked = true;
            }

            SetSource(srcPicker.SelectedSource);
        }
    }

    private void btnOpenChrome_Click(object sender, EventArgs e)
    {
        btnSelectSource_Click(sender, e);
    }

    private async void btnStopChrome_Click(object sender, EventArgs e)
    {
        try
        {
            await _browserCaptureServer.StopStreamAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to stop stream: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void rbSourceType_CheckedChanged(object sender, EventArgs e)
    {
        if (sender is RadioButton { Checked: true } rb)
        {
            if (rb == rbSourceVideo)
            {
                MessageBox.Show(this, "Local Video frame extraction will be enabled in Phase 2.", "AutoSnap", MessageBoxButtons.OK, MessageBoxIcon.Information);
                rbSourceWindow.Checked = true;
                return;
            }

            if (rb == rbSourceChrome)
            {
                if (!_browserCaptureServer.IsRunning)
                {
                    _browserCaptureServer.Start();
                }

                var browserSource = new BrowserCaptureSource(_browserCaptureServer);
                SetSource(browserSource);

                if (!_browserCaptureServer.HasActiveStream)
                {
                    btnSelectSource_Click(this, EventArgs.Empty);
                }
            }
            else if (rb == rbSourceWindow)
            {
                if (_selectedSource is not WindowCaptureSource)
                {
                    btnSelectSource_Click(this, EventArgs.Empty);
                }
            }
            else if (rb == rbSourceMonitor)
            {
                if (_selectedSource is not MonitorCaptureSource)
                {
                    btnSelectSource_Click(this, EventArgs.Empty);
                }
            }
        }
    }

    private void BrowserCaptureServer_StreamStarted(object? sender, (string Title, int Width, int Height) e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => BrowserCaptureServer_StreamStarted(sender, e)));
            return;
        }

        if (_selectedSource is BrowserCaptureSource browserSource)
        {
            UpdateChromeTabUi(browserSource);
            _ = UpdatePreviewAsync();
        }
    }

    private void BrowserCaptureServer_StreamEnded(object? sender, EventArgs e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => BrowserCaptureServer_StreamEnded(sender, e)));
            return;
        }

        if (_selectedSource is BrowserCaptureSource browserSource)
        {
            UpdateChromeTabUi(browserSource);
            ClearPreview("The selected Chrome tab is no longer available.");

            if (_scheduler.State == CaptureSessionState.Capturing)
            {
                _ = _scheduler.StopAsync();
                lblLastSaved.Text = "The selected Chrome tab is no longer available.";
                lblLastSaved.ForeColor = Color.Crimson;
            }
        }
    }

    private void BrowserCaptureServer_SessionConnected(object? sender, BrowserCaptureSession e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => BrowserCaptureServer_SessionConnected(sender, e)));
            return;
        }

        if (_selectedSource is BrowserCaptureSource browserSource)
        {
            UpdateChromeTabUi(browserSource);
        }
    }

    private void BrowserCaptureServer_SessionDisconnected(object? sender, EventArgs e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => BrowserCaptureServer_SessionDisconnected(sender, e)));
            return;
        }

        if (_selectedSource is BrowserCaptureSource browserSource)
        {
            UpdateChromeTabUi(browserSource);
        }
    }

    private void cmbInterval_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool isCustom = cmbInterval.SelectedIndex == 9;
        pnlCustomInterval.Visible = isCustom;

        _appSettings.Capture.Interval = GetSelectedInterval();
    }

    private async void btnStart_Click(object sender, EventArgs e)
    {
        if (_selectedSource == null)
        {
            MessageBox.Show(this, "Please select a capture source first.", "Source Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!_selectedSource.IsAvailable)
        {
            MessageBox.Show(this, _selectedSource.StatusDescription, "Source Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _appSettings.Capture.Interval = GetSelectedInterval();
        _appSettings.Capture.CustomIntervalText = numCustomInterval.Value.ToString();
        _appSettings.Capture.CustomIntervalUnit = cmbCustomUnit.SelectedItem?.ToString() ?? "Seconds";

        try
        {
            await _scheduler.StartAsync(_selectedSource, _appSettings.Capture);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to start capture: {ex.Message}", "Capture Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnPause_Click(object sender, EventArgs e)
    {
        if (_scheduler.State == CaptureSessionState.Capturing)
        {
            _scheduler.Pause();
        }
        else if (_scheduler.State == CaptureSessionState.Paused)
        {
            _scheduler.Resume();
        }
    }

    private async void btnStop_Click(object sender, EventArgs e)
    {
        await _scheduler.StopAsync();
    }

    private async void btnTakeSnapshot_Click(object sender, EventArgs e)
    {
        if (_selectedSource == null)
        {
            MessageBox.Show(this, "Please select a capture source first.", "Source Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!_selectedSource.IsAvailable)
        {
            MessageBox.Show(this, _selectedSource.StatusDescription, "Source Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnTakeSnapshot.Enabled = false;
        try
        {
            var record = await _scheduler.TriggerManualSnapshotAsync(_selectedSource, _appSettings.Capture);
            if (record != null)
            {
                lblLastSaved.Text = $"Manual snapshot saved: {record.FileName}";
                lblLastSaved.ForeColor = Color.FromArgb(0, 120, 215);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Snapshot failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnTakeSnapshot.Enabled = true;
        }
    }

    private void btnSettings_Click(object sender, EventArgs e)
    {
        using var settingsForm = new SettingsForm(_appSettings);
        if (settingsForm.ShowDialog(this) == DialogResult.OK)
        {
            _settingsService.SaveSettings(_appSettings);
            ApplySettingsToUi();
        }
    }

    private void btnOpenFolder_Click(object sender, EventArgs e)
    {
        try
        {
            string folder = _appSettings.Capture.OutputFolder;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = folder,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not open folder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Scheduler_StateChanged(object? sender, CaptureSessionState state)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => UpdateStateUi(state)));
            return;
        }
        UpdateStateUi(state);
    }

    private void UpdateStateUi(CaptureSessionState state)
    {
        switch (state)
        {
            case CaptureSessionState.Idle:
                lblStatusBadge.Text = "Idle";
                lblStatusBadge.BackColor = Color.FromArgb(230, 230, 230);
                lblStatusBadge.ForeColor = Color.FromArgb(80, 80, 80);
                btnStart.Enabled = _selectedSource != null && _selectedSource.IsAvailable;
                btnPause.Enabled = false;
                btnPause.Text = "Pause";
                btnStop.Enabled = false;
                btnSelectSource.Enabled = true;
                cmbInterval.Enabled = true;
                pnlCustomInterval.Enabled = true;
                lblCountdownVal.Text = "--:--:--";
                lblDurationVal.Text = "00:00:00";
                break;

            case CaptureSessionState.Capturing:
                lblStatusBadge.Text = "● Capturing";
                lblStatusBadge.BackColor = Color.FromArgb(212, 237, 218);
                lblStatusBadge.ForeColor = Color.FromArgb(21, 87, 36);
                btnStart.Enabled = false;
                btnPause.Enabled = true;
                btnPause.Text = "Pause";
                btnStop.Enabled = true;
                btnSelectSource.Enabled = false;
                cmbInterval.Enabled = false;
                pnlCustomInterval.Enabled = false;
                break;

            case CaptureSessionState.Paused:
                lblStatusBadge.Text = "❚❚ Paused";
                lblStatusBadge.BackColor = Color.FromArgb(255, 243, 205);
                lblStatusBadge.ForeColor = Color.FromArgb(133, 100, 4);
                btnStart.Enabled = false;
                btnPause.Enabled = true;
                btnPause.Text = "Resume";
                btnStop.Enabled = true;
                break;

            case CaptureSessionState.Stopping:
                lblStatusBadge.Text = "Stopping...";
                lblStatusBadge.BackColor = Color.FromArgb(248, 215, 218);
                lblStatusBadge.ForeColor = Color.FromArgb(114, 28, 36);
                btnStart.Enabled = false;
                btnPause.Enabled = false;
                btnStop.Enabled = false;
                break;

            case CaptureSessionState.Error:
                lblStatusBadge.Text = "⚠ Error";
                lblStatusBadge.BackColor = Color.FromArgb(248, 215, 218);
                lblStatusBadge.ForeColor = Color.FromArgb(114, 28, 36);
                btnStart.Enabled = true;
                btnPause.Enabled = false;
                btnStop.Enabled = true;
                break;
        }

        UpdateTrayMenuItems(state);
    }

    private void Scheduler_SnapshotSaved(object? sender, SnapshotRecord record)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() =>
            {
                lblCapturedCountVal.Text = _scheduler.SnapshotsCapturedCount.ToString();
                lblLastSaved.Text = $"Last saved: {record.FileName} ({record.Resolution.Width}×{record.Resolution.Height})";
                lblLastSaved.ForeColor = Color.FromArgb(40, 140, 40);
            }));
            return;
        }

        lblCapturedCountVal.Text = _scheduler.SnapshotsCapturedCount.ToString();
        lblLastSaved.Text = $"Last saved: {record.FileName} ({record.Resolution.Width}×{record.Resolution.Height})";
        lblLastSaved.ForeColor = Color.FromArgb(40, 140, 40);
    }

    private void Scheduler_CaptureError(object? sender, (string Message, Exception? Exception) e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() =>
            {
                lblLastSaved.Text = $"Error: {e.Message}";
                lblLastSaved.ForeColor = Color.Crimson;

                if (e.Message.Contains("no longer available", StringComparison.OrdinalIgnoreCase) ||
                    e.Message.Contains("stream ended", StringComparison.OrdinalIgnoreCase))
                {
                    ClearPreview("The selected Chrome tab is no longer available.");
                    _ = _scheduler.StopAsync();
                }
            }));
            return;
        }

        lblLastSaved.Text = $"Error: {e.Message}";
        lblLastSaved.ForeColor = Color.Crimson;

        if (e.Message.Contains("no longer available", StringComparison.OrdinalIgnoreCase) ||
            e.Message.Contains("stream ended", StringComparison.OrdinalIgnoreCase))
        {
            ClearPreview("The selected Chrome tab is no longer available.");
            _ = _scheduler.StopAsync();
        }
    }

    private void Scheduler_StatusTicked(object? sender, (TimeSpan Remaining, TimeSpan Duration, int TotalCaptured) e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() =>
            {
                lblCountdownVal.Text = e.Remaining.ToString(@"hh\:mm\:ss");
                lblDurationVal.Text = e.Duration.ToString(@"hh\:mm\:ss");
                lblCapturedCountVal.Text = e.TotalCaptured.ToString();
            }));
            return;
        }

        lblCountdownVal.Text = e.Remaining.ToString(@"hh\:mm\:ss");
        lblDurationVal.Text = e.Duration.ToString(@"hh\:mm\:ss");
        lblCapturedCountVal.Text = e.TotalCaptured.ToString();
    }

    #region Tray Icon Handlers

    private void UpdateTrayMenuItems(CaptureSessionState state)
    {
        trayMenuPauseResume.Text = state == CaptureSessionState.Paused ? "Resume Capture" : "Pause Capture";
        trayMenuPauseResume.Enabled = state == CaptureSessionState.Capturing || state == CaptureSessionState.Paused;
        trayMenuStop.Enabled = state == CaptureSessionState.Capturing || state == CaptureSessionState.Paused;
    }

    private void notifyIcon_DoubleClick(object sender, EventArgs e)
    {
        RestoreFromTray();
    }

    private void trayMenuOpen_Click(object sender, EventArgs e)
    {
        RestoreFromTray();
    }

    private void trayMenuSnapshot_Click(object sender, EventArgs e)
    {
        btnTakeSnapshot_Click(sender, e);
    }

    private void trayMenuPauseResume_Click(object sender, EventArgs e)
    {
        btnPause_Click(sender, e);
    }

    private async void trayMenuStop_Click(object sender, EventArgs e)
    {
        await _scheduler.StopAsync();
    }

    private async void trayMenuExit_Click(object sender, EventArgs e)
    {
        _isExplicitExit = true;
        await _scheduler.StopAsync();
        Close();
        Application.Exit();
    }

    private void RestoreFromTray()
    {
        Show();
        WindowState = FormWindowState.Normal;
        BringToFront();
        Activate();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!_isExplicitExit && _appSettings.MinimizeToTray)
        {
            e.Cancel = true;
            Hide();
            if (_scheduler.State == CaptureSessionState.Capturing)
            {
                notifyIcon.ShowBalloonTip(2000, "AutoSnap", "Capture continues in the background. Double-click tray icon to open.", ToolTipIcon.Info);
            }
            return;
        }

        _previewTimer.Stop();
        _previewTimer.Dispose();
        _scheduler.Dispose();
        _selectedSource?.Dispose();
        _settingsService.SaveSettings(_appSettings);

        // Stop capture server cleanly
        _browserCaptureServer.Stop();
        _browserCaptureServer.Dispose();

        base.OnFormClosing(e);
    }

    #endregion
}
