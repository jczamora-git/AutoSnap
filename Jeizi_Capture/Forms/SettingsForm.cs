using AutoSnap.Chrome;
using AutoSnap.Models;

namespace AutoSnap.Forms;

public partial class SettingsForm : Form
{
    private readonly AppSettings _settings;

    public SettingsForm(AppSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        InitializeComponent();
        LoadSettingsToUi();
    }

    private void LoadSettingsToUi()
    {
        txtOutputFolder.Text = _settings.Capture.OutputFolder;
        chkMinimizeToTray.Checked = _settings.MinimizeToTray;
        chkStartMinimized.Checked = _settings.StartMinimized;
        chkContinueWhileMinimized.Checked = _settings.ContinueCapturingWhenMinimized;

        if (_settings.Capture.Format == ImageFormatType.Png)
        {
            rbFormatPng.Checked = true;
            trackQuality.Enabled = false;
            numQuality.Enabled = false;
        }
        else
        {
            rbFormatJpg.Checked = true;
            trackQuality.Enabled = true;
            numQuality.Enabled = true;
        }

        trackQuality.Value = Math.Clamp(_settings.Capture.JpegQuality, 1, 100);
        numQuality.Value = trackQuality.Value;

        switch (_settings.Capture.Scale)
        {
            case ImageScaleType.Scale75:
                cmbScale.SelectedIndex = 1;
                break;
            case ImageScaleType.Scale50:
                cmbScale.SelectedIndex = 2;
                break;
            default:
                cmbScale.SelectedIndex = 0;
                break;
        }

        // Chrome executable settings
        txtChromeExe.Text = _settings.Chrome.CustomChromeExecutablePath ?? string.Empty;

        // Storage settings
        chkStorageCleanup.Checked = _settings.EnableStorageManagement;
        numMaxStorage.Value = Math.Max(100, _settings.MaxStorageSizeMb);
        numDeleteDays.Value = Math.Max(1, _settings.DeleteOlderThanDays);
    }

    private void btnBrowseOutput_Click(object sender, EventArgs e)
    {
        using var fbd = new FolderBrowserDialog();
        fbd.Description = "Select Screenshot Output Directory";
        fbd.SelectedPath = Directory.Exists(txtOutputFolder.Text)
            ? txtOutputFolder.Text
            : Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

        if (fbd.ShowDialog(this) == DialogResult.OK)
        {
            txtOutputFolder.Text = fbd.SelectedPath;
        }
    }

    private void rbFormat_CheckedChanged(object sender, EventArgs e)
    {
        bool isJpg = rbFormatJpg.Checked;
        trackQuality.Enabled = isJpg;
        numQuality.Enabled = isJpg;
    }

    private void trackQuality_Scroll(object sender, EventArgs e)
    {
        numQuality.Value = trackQuality.Value;
    }

    private void numQuality_ValueChanged(object sender, EventArgs e)
    {
        trackQuality.Value = (int)numQuality.Value;
    }

    private void btnBrowseChromeExe_Click(object sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog();
        ofd.Title = "Locate Google Chrome Executable";
        ofd.Filter = "Executable Files (chrome.exe)|chrome.exe|All Executable Files (*.exe)|*.exe";
        if (ofd.ShowDialog(this) == DialogResult.OK)
        {
            txtChromeExe.Text = ofd.FileName;
        }
    }

    private void btnAutoDetectChrome_Click(object sender, EventArgs e)
    {
        string? found = ChromeService.FindChromeExecutable();
        if (!string.IsNullOrWhiteSpace(found))
        {
            txtChromeExe.Text = found;
            MessageBox.Show(this, $"Found Chrome at:\n{found}", "Chrome Detected", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show(this, "Could not automatically locate Google Chrome. Please browse to chrome.exe manually.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        string path = txtOutputFolder.Text.Trim();
        if (string.IsNullOrWhiteSpace(path))
        {
            MessageBox.Show(this, "Please specify a valid output folder.", "Invalid Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to access or create directory: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _settings.Capture.OutputFolder = path;
        _settings.MinimizeToTray = chkMinimizeToTray.Checked;
        _settings.StartMinimized = chkStartMinimized.Checked;
        _settings.ContinueCapturingWhenMinimized = chkContinueWhileMinimized.Checked;

        _settings.Capture.Format = rbFormatPng.Checked ? ImageFormatType.Png : ImageFormatType.Jpg;
        _settings.Capture.JpegQuality = (int)numQuality.Value;

        _settings.Capture.Scale = cmbScale.SelectedIndex switch
        {
            1 => ImageScaleType.Scale75,
            2 => ImageScaleType.Scale50,
            _ => ImageScaleType.Original
        };

        // Save Chrome settings
        string chromeExe = txtChromeExe.Text.Trim();
        _settings.Chrome.CustomChromeExecutablePath = string.IsNullOrWhiteSpace(chromeExe) ? null : chromeExe;

        // Save Storage settings
        _settings.EnableStorageManagement = chkStorageCleanup.Checked;
        _settings.MaxStorageSizeMb = (long)numMaxStorage.Value;
        _settings.DeleteOlderThanDays = (int)numDeleteDays.Value;

        DialogResult = DialogResult.OK;
        Close();
    }
}
