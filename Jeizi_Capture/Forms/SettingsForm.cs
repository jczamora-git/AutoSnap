using System.Diagnostics;
using AutoSnap.Chrome;
using AutoSnap.Models;
using AutoSnap.Transcription;
using AutoSnap.Video;

namespace AutoSnap.Forms;

public partial class SettingsForm : Form
{
    private readonly AppSettings _settings;
    private readonly WhisperModelManager _modelManager;

    public SettingsForm(AppSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _modelManager = new WhisperModelManager(_settings.Transcription.CustomModelsDirectory);
        InitializeComponent();
        InitializeExtendedTabs();
        LoadSettingsToUi();
    }

    // Extended controls for Video and Transcription tabs
    private TabPage tabVideo = null!;
    private TabPage tabTranscription = null!;

    // Video Tab controls
    private Label lblManagedStatus = null!;
    private Label lblManagedLocation = null!;
    private Button btnInstallManagedFFmpeg = null!;
    private Button btnOpenFFmpegFolder = null!;
    private Button btnTestFFmpeg = null!;
    private ProgressBar progressFFmpeg = null!;
    private Label lblFFmpegProgress = null!;

    private CheckBox chkUseCustomFFmpeg = null!;
    private TextBox txtFFmpegPath = null!;
    private TextBox txtFFprobePath = null!;
    private Button btnBrowseFFmpeg = null!;
    private Button btnBrowseFFprobe = null!;
    private Label lblCustomStatus = null!;

    // Transcription Tab controls
    private ComboBox cmbDefaultLanguage = null!;
    private ComboBox cmbDefaultModel = null!;
    private TextBox txtModelsDir = null!;
    private Button btnBrowseModelsDir = null!;
    private Button btnManageModels = null!;
    private NumericUpDown numChunkDuration = null!;
    private NumericUpDown numOverlapDuration = null!;
    private CheckBox chkAutosave = null!;
    private NumericUpDown numAutosaveInterval = null!;
    private CheckBox chkKeepAudio = null!;

    private void InitializeExtendedTabs()
    {
        // 1. Video / FFmpeg Tab
        tabVideo = new TabPage("Video / FFmpeg")
        {
            Padding = new Padding(12),
            UseVisualStyleBackColor = true,
            AutoScroll = true
        };

        // Managed FFmpeg Card
        var grpManaged = new GroupBox
        {
            Text = "AutoSnap-Managed FFmpeg & FFprobe (Recommended)",
            Dock = DockStyle.Top,
            Height = 195,
            Padding = new Padding(12),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41)
        };

        lblManagedStatus = new Label
        {
            Text = "Status: Checking...",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(108, 117, 125),
            Location = new Point(14, 26),
            AutoSize = true
        };

        lblManagedLocation = new Label
        {
            Text = $"Location: {FFmpegManager.GetManagedDirectory()}",
            Font = new Font("Segoe UI", 8F),
            ForeColor = Color.FromArgb(108, 117, 125),
            Location = new Point(14, 50),
            Size = new Size(470, 20)
        };

        btnInstallManagedFFmpeg = new Button
        {
            Text = "📥 Install FFmpeg",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Location = new Point(14, 76),
            Size = new Size(150, 32),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnInstallManagedFFmpeg.FlatAppearance.BorderSize = 0;
        btnInstallManagedFFmpeg.Click += BtnInstallManagedFFmpeg_Click;

        btnOpenFFmpegFolder = new Button
        {
            Text = "📂 Open Folder",
            Font = new Font("Segoe UI", 8.5F),
            Location = new Point(172, 76),
            Size = new Size(110, 32),
            BackColor = Color.FromArgb(233, 236, 239),
            FlatStyle = FlatStyle.Flat
        };
        btnOpenFFmpegFolder.FlatAppearance.BorderSize = 0;
        btnOpenFFmpegFolder.Click += (s, e) =>
        {
            string dir = FFmpegManager.GetManagedDirectory();
            Directory.CreateDirectory(dir);
            Process.Start(new ProcessStartInfo { FileName = dir, UseShellExecute = true });
        };

        btnTestFFmpeg = new Button
        {
            Text = "🔍 Test FFmpeg",
            Font = new Font("Segoe UI", 8.5F),
            Location = new Point(290, 76),
            Size = new Size(110, 32),
            BackColor = Color.FromArgb(233, 236, 239),
            FlatStyle = FlatStyle.Flat
        };
        btnTestFFmpeg.FlatAppearance.BorderSize = 0;
        btnTestFFmpeg.Click += BtnTestFFmpeg_Click;

        progressFFmpeg = new ProgressBar
        {
            Location = new Point(14, 118),
            Size = new Size(470, 16),
            Visible = false
        };

        lblFFmpegProgress = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 8F),
            ForeColor = Color.FromArgb(73, 80, 87),
            Location = new Point(14, 138),
            Size = new Size(470, 20),
            Visible = false
        };

        var lblManagedNote = new Label
        {
            Text = "Downloads Windows x64 essentials from official Gyan.dev build. No admin rights or system PATH modification required.",
            Font = new Font("Segoe UI", 7.5F, FontStyle.Italic),
            ForeColor = Color.FromArgb(108, 117, 125),
            Location = new Point(14, 162),
            Size = new Size(470, 24)
        };

        grpManaged.Controls.Add(lblManagedStatus);
        grpManaged.Controls.Add(lblManagedLocation);
        grpManaged.Controls.Add(btnInstallManagedFFmpeg);
        grpManaged.Controls.Add(btnOpenFFmpegFolder);
        grpManaged.Controls.Add(btnTestFFmpeg);
        grpManaged.Controls.Add(progressFFmpeg);
        grpManaged.Controls.Add(lblFFmpegProgress);
        grpManaged.Controls.Add(lblManagedNote);

        // Custom Installation Card
        var grpCustom = new GroupBox
        {
            Text = "Advanced: Custom Installation Override",
            Dock = DockStyle.Top,
            Height = 180,
            Padding = new Padding(12),
            Margin = new Padding(0, 10, 0, 0),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41)
        };

        chkUseCustomFFmpeg = new CheckBox
        {
            Text = "Use Custom FFmpeg / FFprobe binaries",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Location = new Point(14, 24),
            AutoSize = true
        };
        chkUseCustomFFmpeg.CheckedChanged += (s, e) => UpdateCustomFFmpegUiState();

        var lblFFmpeg = new Label { Text = "Custom ffmpeg.exe Path:", Location = new Point(14, 52), AutoSize = true, Font = new Font("Segoe UI", 8F) };
        txtFFmpegPath = new TextBox { Location = new Point(14, 70), Size = new Size(380, 23), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Font = new Font("Segoe UI", 8.5F) };
        btnBrowseFFmpeg = new Button { Text = "Browse...", Location = new Point(402, 69), Size = new Size(85, 25), Anchor = AnchorStyles.Top | AnchorStyles.Right, Font = new Font("Segoe UI", 8F) };
        btnBrowseFFmpeg.Click += (s, e) =>
        {
            using var ofd = new OpenFileDialog { Title = "Locate ffmpeg.exe", Filter = "ffmpeg.exe|ffmpeg.exe|All Executables (*.exe)|*.exe" };
            if (ofd.ShowDialog(this) == DialogResult.OK) txtFFmpegPath.Text = ofd.FileName;
        };

        var lblFFprobe = new Label { Text = "Custom ffprobe.exe Path:", Location = new Point(14, 100), AutoSize = true, Font = new Font("Segoe UI", 8F) };
        txtFFprobePath = new TextBox { Location = new Point(14, 118), Size = new Size(380, 23), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Font = new Font("Segoe UI", 8.5F) };
        btnBrowseFFprobe = new Button { Text = "Browse...", Location = new Point(402, 117), Size = new Size(85, 25), Anchor = AnchorStyles.Top | AnchorStyles.Right, Font = new Font("Segoe UI", 8F) };
        btnBrowseFFprobe.Click += (s, e) =>
        {
            using var ofd = new OpenFileDialog { Title = "Locate ffprobe.exe", Filter = "ffprobe.exe|ffprobe.exe|All Executables (*.exe)|*.exe" };
            if (ofd.ShowDialog(this) == DialogResult.OK) txtFFprobePath.Text = ofd.FileName;
        };

        lblCustomStatus = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 8F, FontStyle.Italic),
            ForeColor = Color.FromArgb(108, 117, 125),
            Location = new Point(14, 148),
            AutoSize = true
        };

        grpCustom.Controls.Add(chkUseCustomFFmpeg);
        grpCustom.Controls.Add(lblFFmpeg);
        grpCustom.Controls.Add(txtFFmpegPath);
        grpCustom.Controls.Add(btnBrowseFFmpeg);
        grpCustom.Controls.Add(lblFFprobe);
        grpCustom.Controls.Add(txtFFprobePath);
        grpCustom.Controls.Add(btnBrowseFFprobe);
        grpCustom.Controls.Add(lblCustomStatus);

        tabVideo.Controls.Add(grpCustom);
        tabVideo.Controls.Add(grpManaged);

        // 2. Transcription Tab
        tabTranscription = new TabPage("Transcription [Beta]")
        {
            Padding = new Padding(12),
            UseVisualStyleBackColor = true,
            AutoScroll = true
        };

        var grpWhisper = new GroupBox
        {
            Text = "Local Whisper AI Settings — Status: Experimental",
            Dock = DockStyle.Top,
            Height = 150,
            Padding = new Padding(12),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41)
        };

        var lblLang = new Label { Text = "Default Language:", Location = new Point(14, 26), AutoSize = true, Font = new Font("Segoe UI", 8.5F) };
        cmbDefaultLanguage = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(135, 22), Size = new Size(160, 24), Font = new Font("Segoe UI", 8.5F) };
        cmbDefaultLanguage.Items.AddRange(new object[] { "Taglish (Recommended)", "English", "Filipino / Tagalog", "Auto Detect" });

        var lblModel = new Label { Text = "Default Model:", Location = new Point(14, 58), AutoSize = true, Font = new Font("Segoe UI", 8.5F) };
        cmbDefaultModel = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(135, 54), Size = new Size(160, 24), Font = new Font("Segoe UI", 8.5F) };
        cmbDefaultModel.Items.AddRange(new object[] { "Small", "Base", "Tiny", "Medium", "Large v3", "Large v3 Turbo", "Large v3 Turbo Q5" });

        btnManageModels = new Button { Text = "📥 Manage Models...", Location = new Point(310, 53), Size = new Size(140, 27), Font = new Font("Segoe UI", 8.5F) };
        btnManageModels.Click += (s, e) =>
        {
            using var dlg = new WhisperModelManagerForm(_modelManager);
            dlg.ShowDialog(this);
        };

        var lblDir = new Label { Text = "Models Directory:", Location = new Point(14, 90), AutoSize = true, Font = new Font("Segoe UI", 8.5F) };
        txtModelsDir = new TextBox { Location = new Point(135, 86), Size = new Size(260, 24), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Font = new Font("Segoe UI", 8.5F) };
        btnBrowseModelsDir = new Button { Text = "Browse...", Location = new Point(404, 84), Size = new Size(85, 27), Anchor = AnchorStyles.Top | AnchorStyles.Right, Font = new Font("Segoe UI", 8.5F) };
        btnBrowseModelsDir.Click += (s, e) =>
        {
            using var fbd = new FolderBrowserDialog { SelectedPath = txtModelsDir.Text };
            if (fbd.ShowDialog(this) == DialogResult.OK) txtModelsDir.Text = fbd.SelectedPath;
        };

        grpWhisper.Controls.Add(lblLang);
        grpWhisper.Controls.Add(cmbDefaultLanguage);
        grpWhisper.Controls.Add(lblModel);
        grpWhisper.Controls.Add(cmbDefaultModel);
        grpWhisper.Controls.Add(btnManageModels);
        grpWhisper.Controls.Add(lblDir);
        grpWhisper.Controls.Add(txtModelsDir);
        grpWhisper.Controls.Add(btnBrowseModelsDir);

        var grpStreaming = new GroupBox
        {
            Text = "Live Streaming & Recovery Settings",
            Dock = DockStyle.Top,
            Height = 160,
            Padding = new Padding(12),
            Margin = new Padding(0, 10, 0, 0),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41)
        };

        var lblChunk = new Label { Text = "Chunk Duration (sec):", Location = new Point(14, 26), AutoSize = true, Font = new Font("Segoe UI", 8.5F) };
        numChunkDuration = new NumericUpDown { Location = new Point(145, 24), Size = new Size(70, 24), Minimum = 5, Maximum = 60, Value = 15, Font = new Font("Segoe UI", 8.5F) };

        var lblOverlap = new Label { Text = "Overlap (sec):", Location = new Point(240, 26), AutoSize = true, Font = new Font("Segoe UI", 8.5F) };
        numOverlapDuration = new NumericUpDown { Location = new Point(330, 24), Size = new Size(60, 24), Minimum = 0, Maximum = 10, Value = 2, Font = new Font("Segoe UI", 8.5F) };

        chkAutosave = new CheckBox { Text = "Auto-save transcript every (sec):", Location = new Point(14, 60), AutoSize = true, Checked = true, Font = new Font("Segoe UI", 8.5F) };
        numAutosaveInterval = new NumericUpDown { Location = new Point(230, 58), Size = new Size(60, 24), Minimum = 10, Maximum = 300, Value = 30, Font = new Font("Segoe UI", 8.5F) };

        chkKeepAudio = new CheckBox { Text = "Keep recorded temporary audio after transcription", Location = new Point(14, 94), AutoSize = true, Checked = false, Font = new Font("Segoe UI", 8.5F) };

        grpStreaming.Controls.Add(lblChunk);
        grpStreaming.Controls.Add(numChunkDuration);
        grpStreaming.Controls.Add(lblOverlap);
        grpStreaming.Controls.Add(numOverlapDuration);
        grpStreaming.Controls.Add(chkAutosave);
        grpStreaming.Controls.Add(numAutosaveInterval);
        grpStreaming.Controls.Add(chkKeepAudio);

        tabTranscription.Controls.Add(grpStreaming);
        tabTranscription.Controls.Add(grpWhisper);

        tabControlSettings.TabPages.Add(tabVideo);
        tabControlSettings.TabPages.Add(tabTranscription);

        RefreshFFmpegStatus();
    }

    private void UpdateCustomFFmpegUiState()
    {
        bool useCustom = chkUseCustomFFmpeg.Checked;
        txtFFmpegPath.Enabled = useCustom;
        btnBrowseFFmpeg.Enabled = useCustom;
        txtFFprobePath.Enabled = useCustom;
        btnBrowseFFprobe.Enabled = useCustom;
    }

    private async void RefreshFFmpegStatus()
    {
        bool managedInstalled = FFmpegManager.IsManagedInstalled();
        if (managedInstalled)
        {
            string path = FFmpegManager.GetManagedFFmpegPath();
            string? ver = await FFmpegManager.GetVersionAsync(path);
            lblManagedStatus.Text = $"Status: ● Installed {(ver != null ? $"({ver})" : "")}";
            lblManagedStatus.ForeColor = Color.FromArgb(21, 87, 36);
            btnInstallManagedFFmpeg.Text = "🔄 Reinstall FFmpeg";
        }
        else
        {
            lblManagedStatus.Text = "Status: ○ Not Installed (AutoSnap-managed)";
            lblManagedStatus.ForeColor = Color.FromArgb(114, 28, 36);
            btnInstallManagedFFmpeg.Text = "📥 Install FFmpeg";
        }
    }

    private async void BtnInstallManagedFFmpeg_Click(object? sender, EventArgs e)
    {
        btnInstallManagedFFmpeg.Enabled = false;
        btnTestFFmpeg.Enabled = false;
        progressFFmpeg.Visible = true;
        lblFFmpegProgress.Visible = true;
        progressFFmpeg.Value = 0;

        var progress = new Progress<(long Bytes, long Total, double Percent, double SpeedMb, string Stage)>(p =>
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    progressFFmpeg.Value = Math.Clamp((int)p.Percent, 0, 100);
                    lblFFmpegProgress.Text = $"{p.Stage} ({p.Percent:0.0}%)";
                }));
            }
            else
            {
                progressFFmpeg.Value = Math.Clamp((int)p.Percent, 0, 100);
                lblFFmpegProgress.Text = $"{p.Stage} ({p.Percent:0.0}%)";
            }
        });

        try
        {
            await FFmpegManager.InstallManagedFFmpegAsync(progress);
            MessageBox.Show(this, "FFmpeg and FFprobe successfully installed and verified!", "Installation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to install FFmpeg: {ex.Message}", "Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnInstallManagedFFmpeg.Enabled = true;
            btnTestFFmpeg.Enabled = true;
            progressFFmpeg.Visible = false;
            lblFFmpegProgress.Visible = false;
            RefreshFFmpegStatus();
        }
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

        // Video Settings
        chkUseCustomFFmpeg.Checked = _settings.Video.UseCustomFFmpeg;
        txtFFmpegPath.Text = _settings.Video.CustomFFmpegPath ?? string.Empty;
        txtFFprobePath.Text = _settings.Video.CustomFFprobePath ?? string.Empty;
        UpdateCustomFFmpegUiState();

        // Transcription Settings
        cmbDefaultLanguage.SelectedIndex = (int)_settings.Transcription.DefaultLanguageMode;
        cmbDefaultModel.SelectedItem = _settings.Transcription.DefaultModelName;
        if (cmbDefaultModel.SelectedIndex == -1) cmbDefaultModel.SelectedIndex = 0;
        txtModelsDir.Text = _settings.Transcription.CustomModelsDirectory ?? _modelManager.ModelsDirectory;
        numChunkDuration.Value = Math.Clamp(_settings.Transcription.ChunkDurationSeconds, 5, 60);
        numOverlapDuration.Value = Math.Clamp(_settings.Transcription.OverlapDurationSeconds, 0, 10);
        chkAutosave.Checked = _settings.Transcription.EnableAutosave;
        numAutosaveInterval.Value = Math.Clamp(_settings.Transcription.AutosaveIntervalSeconds, 10, 300);
        chkKeepAudio.Checked = _settings.Transcription.KeepCapturedAudio;

        // Storage settings
        chkStorageCleanup.Checked = _settings.EnableStorageManagement;
        numMaxStorage.Value = Math.Max(100, _settings.MaxStorageSizeMb);
        numDeleteDays.Value = Math.Max(1, _settings.DeleteOlderThanDays);
    }

    private void BtnTestFFmpeg_Click(object? sender, EventArgs e)
    {
        bool useCustom = chkUseCustomFFmpeg.Checked;
        string? ffmpeg = FFmpegManager.FindFFmpeg(txtFFmpegPath.Text.Trim(), useCustom);
        string? ffprobe = FFmpegManager.FindFFprobe(txtFFprobePath.Text.Trim(), useCustom);

        if (ffmpeg != null && ffprobe != null)
        {
            lblManagedStatus.Text = "Status: ✓ FFmpeg and FFprobe active and ready!";
            lblManagedStatus.ForeColor = Color.FromArgb(21, 87, 36);
            MessageBox.Show(this, $"FFmpeg found at:\n{ffmpeg}\n\nFFprobe found at:\n{ffprobe}", "FFmpeg Detected", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            lblManagedStatus.Text = $"Status: ⚠ {(ffmpeg == null ? "FFmpeg missing" : "FFprobe missing")}";
            lblManagedStatus.ForeColor = Color.FromArgb(114, 28, 36);
            MessageBox.Show(this, "Could not locate FFmpeg or FFprobe.\nClick [Install FFmpeg] to auto-install managed binaries, or specify custom paths.", "FFmpeg Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
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

        // Chrome settings
        string chromeExe = txtChromeExe.Text.Trim();
        _settings.Chrome.CustomChromeExecutablePath = string.IsNullOrWhiteSpace(chromeExe) ? null : chromeExe;

        // Video settings
        _settings.Video.UseCustomFFmpeg = chkUseCustomFFmpeg.Checked;
        string ffmpegPath = txtFFmpegPath.Text.Trim();
        string ffprobePath = txtFFprobePath.Text.Trim();
        _settings.Video.CustomFFmpegPath = string.IsNullOrWhiteSpace(ffmpegPath) ? null : ffmpegPath;
        _settings.Video.CustomFFprobePath = string.IsNullOrWhiteSpace(ffprobePath) ? null : ffprobePath;

        // Transcription settings
        _settings.Transcription.DefaultLanguageMode = (TranscriptionLanguageMode)cmbDefaultLanguage.SelectedIndex;
        _settings.Transcription.DefaultModelName = cmbDefaultModel.SelectedItem?.ToString() ?? "Small";
        string modelsDir = txtModelsDir.Text.Trim();
        _settings.Transcription.CustomModelsDirectory = string.IsNullOrWhiteSpace(modelsDir) ? null : modelsDir;
        _settings.Transcription.ChunkDurationSeconds = (int)numChunkDuration.Value;
        _settings.Transcription.OverlapDurationSeconds = (int)numOverlapDuration.Value;
        _settings.Transcription.EnableAutosave = chkAutosave.Checked;
        _settings.Transcription.AutosaveIntervalSeconds = (int)numAutosaveInterval.Value;
        _settings.Transcription.KeepCapturedAudio = chkKeepAudio.Checked;

        // Storage settings
        _settings.EnableStorageManagement = chkStorageCleanup.Checked;
        _settings.MaxStorageSizeMb = (long)numMaxStorage.Value;
        _settings.DeleteOlderThanDays = (int)numDeleteDays.Value;

        DialogResult = DialogResult.OK;
        Close();
    }
}
