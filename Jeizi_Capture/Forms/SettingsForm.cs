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
    private TextBox txtFFmpegPath = null!;
    private TextBox txtFFprobePath = null!;
    private Button btnBrowseFFmpeg = null!;
    private Button btnBrowseFFprobe = null!;
    private Button btnTestFFmpeg = null!;
    private Label lblFFmpegStatus = null!;

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
        // Video Tab
        tabVideo = new TabPage("Video / FFmpeg")
        {
            Padding = new Padding(12),
            UseVisualStyleBackColor = true
        };

        var grpFFmpeg = new GroupBox
        {
            Text = "FFmpeg & FFprobe Configuration",
            Dock = DockStyle.Top,
            Height = 220,
            Padding = new Padding(12)
        };

        var lblFFmpeg = new Label { Text = "Custom FFmpeg Path (optional if on system PATH):", Location = new Point(14, 26), AutoSize = true };
        txtFFmpegPath = new TextBox { Location = new Point(14, 46), Size = new Size(380, 24), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        btnBrowseFFmpeg = new Button { Text = "Browse...", Location = new Point(404, 44), Size = new Size(85, 27), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        btnBrowseFFmpeg.Click += (s, e) =>
        {
            using var ofd = new OpenFileDialog { Title = "Locate ffmpeg.exe", Filter = "ffmpeg.exe|ffmpeg.exe|All Executables (*.exe)|*.exe" };
            if (ofd.ShowDialog(this) == DialogResult.OK) txtFFmpegPath.Text = ofd.FileName;
        };

        var lblFFprobe = new Label { Text = "Custom FFprobe Path (optional if on system PATH):", Location = new Point(14, 82), AutoSize = true };
        txtFFprobePath = new TextBox { Location = new Point(14, 102), Size = new Size(380, 24), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        btnBrowseFFprobe = new Button { Text = "Browse...", Location = new Point(404, 100), Size = new Size(85, 27), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        btnBrowseFFprobe.Click += (s, e) =>
        {
            using var ofd = new OpenFileDialog { Title = "Locate ffprobe.exe", Filter = "ffprobe.exe|ffprobe.exe|All Executables (*.exe)|*.exe" };
            if (ofd.ShowDialog(this) == DialogResult.OK) txtFFprobePath.Text = ofd.FileName;
        };

        btnTestFFmpeg = new Button { Text = "🔍 Test FFmpeg & FFprobe", Location = new Point(14, 140), Size = new Size(180, 30) };
        btnTestFFmpeg.Click += BtnTestFFmpeg_Click;

        lblFFmpegStatus = new Label
        {
            Text = "Status: Not tested",
            Location = new Point(205, 146),
            AutoSize = true,
            ForeColor = Color.FromArgb(108, 117, 125)
        };

        var lblNote = new Label
        {
            Text = "Note: AutoSnap will check PATH, Chocolatey, Scoop, and app folders automatically.",
            Location = new Point(14, 180),
            AutoSize = true,
            ForeColor = Color.FromArgb(108, 117, 125),
            Font = new Font("Segoe UI", 8F, FontStyle.Italic)
        };

        grpFFmpeg.Controls.Add(lblFFmpeg);
        grpFFmpeg.Controls.Add(txtFFmpegPath);
        grpFFmpeg.Controls.Add(btnBrowseFFmpeg);
        grpFFmpeg.Controls.Add(lblFFprobe);
        grpFFmpeg.Controls.Add(txtFFprobePath);
        grpFFmpeg.Controls.Add(btnBrowseFFprobe);
        grpFFmpeg.Controls.Add(btnTestFFmpeg);
        grpFFmpeg.Controls.Add(lblFFmpegStatus);
        grpFFmpeg.Controls.Add(lblNote);

        tabVideo.Controls.Add(grpFFmpeg);

        // Transcription Tab
        tabTranscription = new TabPage("Transcription")
        {
            Padding = new Padding(12),
            UseVisualStyleBackColor = true,
            AutoScroll = true
        };

        var grpWhisper = new GroupBox
        {
            Text = "Local Whisper AI Settings",
            Dock = DockStyle.Top,
            Height = 150,
            Padding = new Padding(12)
        };

        var lblLang = new Label { Text = "Default Language:", Location = new Point(14, 26), AutoSize = true };
        cmbDefaultLanguage = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(135, 22), Size = new Size(160, 24) };
        cmbDefaultLanguage.Items.AddRange(new object[] { "Taglish (Recommended)", "English", "Filipino / Tagalog", "Auto Detect" });

        var lblModel = new Label { Text = "Default Model:", Location = new Point(14, 58), AutoSize = true };
        cmbDefaultModel = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(135, 54), Size = new Size(160, 24) };
        cmbDefaultModel.Items.AddRange(new object[] { "Small", "Base", "Tiny", "Medium" });

        btnManageModels = new Button { Text = "📥 Manage Models...", Location = new Point(310, 53), Size = new Size(140, 27) };
        btnManageModels.Click += (s, e) =>
        {
            using var dlg = new WhisperModelManagerForm(_modelManager);
            dlg.ShowDialog(this);
        };

        var lblDir = new Label { Text = "Models Directory:", Location = new Point(14, 90), AutoSize = true };
        txtModelsDir = new TextBox { Location = new Point(135, 86), Size = new Size(260, 24), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        btnBrowseModelsDir = new Button { Text = "Browse...", Location = new Point(404, 84), Size = new Size(85, 27), Anchor = AnchorStyles.Top | AnchorStyles.Right };
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
            Margin = new Padding(0, 10, 0, 0)
        };

        var lblChunk = new Label { Text = "Chunk Duration (sec):", Location = new Point(14, 26), AutoSize = true };
        numChunkDuration = new NumericUpDown { Location = new Point(145, 24), Size = new Size(70, 24), Minimum = 5, Maximum = 60, Value = 15 };

        var lblOverlap = new Label { Text = "Overlap (sec):", Location = new Point(240, 26), AutoSize = true };
        numOverlapDuration = new NumericUpDown { Location = new Point(330, 24), Size = new Size(60, 24), Minimum = 0, Maximum = 10, Value = 2 };

        chkAutosave = new CheckBox { Text = "Auto-save transcript every (sec):", Location = new Point(14, 60), AutoSize = true, Checked = true };
        numAutosaveInterval = new NumericUpDown { Location = new Point(230, 58), Size = new Size(60, 24), Minimum = 10, Maximum = 300, Value = 30 };

        chkKeepAudio = new CheckBox { Text = "Keep recorded temporary audio after transcription", Location = new Point(14, 94), AutoSize = true, Checked = false };

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
        txtFFmpegPath.Text = _settings.Video.CustomFFmpegPath ?? string.Empty;
        txtFFprobePath.Text = _settings.Video.CustomFFprobePath ?? string.Empty;

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
        string? ffmpeg = FFmpegService.FindExecutable(txtFFmpegPath.Text.Trim());
        string? ffprobe = FFprobeService.FindExecutable(txtFFprobePath.Text.Trim());

        if (ffmpeg != null && ffprobe != null)
        {
            lblFFmpegStatus.Text = "Status: ✓ FFmpeg and FFprobe found!";
            lblFFmpegStatus.ForeColor = Color.FromArgb(21, 87, 36);
            MessageBox.Show(this, $"FFmpeg found at:\n{ffmpeg}\n\nFFprobe found at:\n{ffprobe}", "FFmpeg Detected", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            lblFFmpegStatus.Text = $"Status: ⚠ {(ffmpeg == null ? "FFmpeg missing" : "FFprobe missing")}";
            lblFFmpegStatus.ForeColor = Color.FromArgb(114, 28, 36);
            MessageBox.Show(this, "Could not locate FFmpeg or FFprobe.\nPlease install FFmpeg or specify the exact executable paths above.", "FFmpeg Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
