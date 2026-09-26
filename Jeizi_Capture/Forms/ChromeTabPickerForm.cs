using System.Drawing;
using AutoSnap.Chrome;
using AutoSnap.Models;

namespace AutoSnap.Forms;

public partial class ChromeTabPickerForm : Form
{
    private readonly ChromeService _chromeService;
    private readonly ChromeSettings _chromeSettings;
    private List<ChromeTabInfo> _loadedTabs = new();

    public ChromeTabCaptureSource? SelectedSource { get; private set; }

    public ChromeTabPickerForm(ChromeService chromeService, ChromeSettings chromeSettings)
    {
        _chromeService = chromeService ?? throw new ArgumentNullException(nameof(chromeService));
        _chromeSettings = chromeSettings ?? throw new ArgumentNullException(nameof(chromeSettings));

        InitializeComponent();
        UpdateConnectionStatusUi();

        _chromeService.ConnectionStateChanged += ChromeService_ConnectionStateChanged;
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);

        if (!_chromeService.IsConnected)
        {
            // If Chrome is already running with debug port or auto-launch is enabled, try connecting/launching
            if (_chromeSettings.AutoLaunchChrome)
            {
                await LaunchOrConnectChromeAsync();
            }
            else
            {
                // Try connecting to existing debug port first if specified
                int port = _chromeSettings.DebugPort > 0 ? _chromeSettings.DebugPort : 9222;
                try
                {
                    await _chromeService.ConnectToExistingAsync(port);
                }
                catch
                {
                    // Not connected yet, user can click Launch
                }
            }
        }

        if (_chromeService.IsConnected)
        {
            await RefreshTabsAsync();
        }
    }

    private void ChromeService_ConnectionStateChanged(object? sender, ChromeConnectionState state)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(UpdateConnectionStatusUi));
            return;
        }

        UpdateConnectionStatusUi();
    }

    private void UpdateConnectionStatusUi()
    {
        switch (_chromeService.State)
        {
            case ChromeConnectionState.Connected:
                lblStatusDot.ForeColor = Color.SeaGreen;
                lblStatusText.Text = $"Chrome: Connected (Port {_chromeService.CurrentConnection?.DebuggingPort})";
                lblStatusText.ForeColor = Color.FromArgb(40, 140, 40);
                btnLaunchChrome.Text = "🚀 Relaunch Chrome";
                break;

            case ChromeConnectionState.Starting:
                lblStatusDot.ForeColor = Color.Orange;
                lblStatusText.Text = "Chrome: Starting dedicated instance...";
                lblStatusText.ForeColor = Color.FromArgb(180, 120, 0);
                break;

            case ChromeConnectionState.Connecting:
                lblStatusDot.ForeColor = Color.Orange;
                lblStatusText.Text = "Chrome: Connecting to DevTools endpoint...";
                lblStatusText.ForeColor = Color.FromArgb(180, 120, 0);
                break;

            case ChromeConnectionState.ConnectionLost:
                lblStatusDot.ForeColor = Color.Crimson;
                lblStatusText.Text = "Chrome: Connection Lost. Please relaunch.";
                lblStatusText.ForeColor = Color.Crimson;
                btnLaunchChrome.Text = "🚀 Launch Capture Chrome";
                break;

            case ChromeConnectionState.Error:
                lblStatusDot.ForeColor = Color.Crimson;
                lblStatusText.Text = "Chrome: Connection Error";
                lblStatusText.ForeColor = Color.Crimson;
                btnLaunchChrome.Text = "🚀 Launch Capture Chrome";
                break;

            default:
                lblStatusDot.ForeColor = Color.Gray;
                lblStatusText.Text = "Chrome: Not Connected";
                lblStatusText.ForeColor = Color.FromArgb(100, 100, 100);
                btnLaunchChrome.Text = "🚀 Launch Capture Chrome";
                break;
        }
    }

    private async Task LaunchOrConnectChromeAsync()
    {
        btnLaunchChrome.Enabled = false;
        try
        {
            await _chromeService.LaunchCaptureChromeAsync(_chromeSettings);
            await RefreshTabsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to start Chrome: {ex.Message}", "Chrome Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            btnLaunchChrome.Enabled = true;
        }
    }

    private async Task RefreshTabsAsync()
    {
        if (!_chromeService.IsConnected)
        {
            listViewTabs.Items.Clear();
            lblTabCount.Text = "Chrome not connected";
            return;
        }

        btnRefresh.Enabled = false;
        try
        {
            _loadedTabs = await _chromeService.GetTabsAsync();
            PopulateTabsList();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not load Chrome tabs: {ex.Message}", "Error Loading Tabs", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnRefresh.Enabled = true;
        }
    }

    private void PopulateTabsList()
    {
        listViewTabs.BeginUpdate();
        listViewTabs.Items.Clear();

        string filter = txtSearch.Text.Trim();

        foreach (var tab in _loadedTabs)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                bool matches = (!string.IsNullOrEmpty(tab.Title) && tab.Title.Contains(filter, StringComparison.OrdinalIgnoreCase)) ||
                               (!string.IsNullOrEmpty(tab.Url) && tab.Url.Contains(filter, StringComparison.OrdinalIgnoreCase));
                if (!matches) continue;
            }

            var item = new ListViewItem(string.IsNullOrWhiteSpace(tab.Title) ? "(Untitled)" : tab.Title)
            {
                Tag = tab
            };
            item.SubItems.Add(tab.Url);
            item.SubItems.Add(tab.Type);

            listViewTabs.Items.Add(item);
        }

        listViewTabs.EndUpdate();
        lblTabCount.Text = $"{listViewTabs.Items.Count} tab(s) available";

        if (listViewTabs.Items.Count > 0 && listViewTabs.SelectedIndices.Count == 0)
        {
            listViewTabs.Items[0].Selected = true;
        }
    }

    private async void btnLaunchChrome_Click(object sender, EventArgs e)
    {
        await LaunchOrConnectChromeAsync();
    }

    private async void btnRefresh_Click(object sender, EventArgs e)
    {
        if (!_chromeService.IsConnected)
        {
            await LaunchOrConnectChromeAsync();
        }
        else
        {
            await RefreshTabsAsync();
        }
    }

    private void txtSearch_TextChanged(object sender, EventArgs e)
    {
        PopulateTabsList();
    }

    private void btnSelect_Click(object sender, EventArgs e)
    {
        ConfirmSelection();
    }

    private void listViewTabs_DoubleClick(object sender, EventArgs e)
    {
        ConfirmSelection();
    }

    private void ConfirmSelection()
    {
        if (listViewTabs.SelectedItems.Count == 0)
        {
            MessageBox.Show(this, "Please select a Chrome tab from the list.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (listViewTabs.SelectedItems[0].Tag is ChromeTabInfo tab)
        {
            try
            {
                SelectedSource = new ChromeTabCaptureSource(tab);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to create tab capture source: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _chromeService.ConnectionStateChanged -= ChromeService_ConnectionStateChanged;
        base.OnFormClosed(e);
    }
}
