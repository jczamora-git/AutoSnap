using System.Diagnostics;
using AutoSnap.Audio;
using AutoSnap.BrowserCapture;
using AutoSnap.Models;
using AutoSnap.Transcription;
using AutoSnap.Video;
using NAudio.CoreAudioApi;

namespace AutoSnap.Forms;

public class TranscriptionControl : UserControl
{
    private readonly WhisperTranscriptionService _transcriptionService;
    private readonly WhisperModelManager _modelManager;
    private readonly TranscriptRecoveryService _recoveryService;
    private readonly FFmpegService _ffmpegService;
    private readonly BrowserCaptureServer _browserCaptureServer;
    private readonly AppSettings _appSettings;

    private TranscriptionSession? _activeSession;
    private CancellationTokenSource? _sessionCts;

    // Controls
    private ComboBox _cmbSourceType = null!;
    private ComboBox _cmbAudioDevice = null!;
    private Button _btnSelectFile = null!;
    private Label _lblSelectedFile = null!;
    private Button _btnBrowserHelper = null!;
    private Label _lblBrowserStatus = null!;

    private ComboBox _cmbLanguage = null!;
    private ComboBox _cmbModel = null!;
    private Button _btnManageModels = null!;

    private Button _btnStartStop = null!;
    private Label _lblStatusBadge = null!;
    private Label _lblMetrics = null!;

    private TextBox _txtTranscript = null!;
    private Button _btnExportTxt = null!;
    private Button _btnExportSrt = null!;
    private Button _btnExportVtt = null!;
    private Button _btnOpenFolder = null!;

    private string? _localMediaFilePath;
    private BrowserAudioSource? _activeBrowserAudioSource;

    public TranscriptionControl(
        WhisperTranscriptionService transcriptionService,
        WhisperModelManager modelManager,
        TranscriptRecoveryService recoveryService,
        FFmpegService ffmpegService,
        BrowserCaptureServer browserCaptureServer,
        AppSettings appSettings)
    {
        _transcriptionService = transcriptionService;
        _modelManager = modelManager;
        _recoveryService = recoveryService;
        _ffmpegService = ffmpegService;
        _browserCaptureServer = browserCaptureServer;
        _appSettings = appSettings;

        InitializeUi();
        PopulateAudioDevices();
    }

    private void InitializeUi()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(248, 249, 250);
        AutoScroll = true;
        Padding = new Padding(20);

        var tblTop = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 1
        };
        tblTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tblTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

        // 1. Source Card
        var grpSource = new GroupBox
        {
            Text = "Transcription Audio Source",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41),
            Dock = DockStyle.Fill,
            Height = 160,
            Padding = new Padding(12)
        };

        var lblSrc = new Label { Text = "Source:", Location = new Point(14, 28), AutoSize = true, Font = new Font("Segoe UI", 8.5F) };
        _cmbSourceType = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(70, 24),
            Size = new Size(240, 24),
            Font = new Font("Segoe UI", 8.5F)
        };
        _cmbSourceType.Items.AddRange(new object[] { "System Audio (WASAPI Output)", "Browser Tab Audio (Chrome)", "Local Media File (Audio/Video)" });
        _cmbSourceType.SelectedIndex = 0;
        _cmbSourceType.SelectedIndexChanged += (s, e) => UpdateSourceTypeVisibility();

        // System Audio Device Sub-panel
        var lblDev = new Label { Text = "Device:", Location = new Point(14, 62), AutoSize = true, Font = new Font("Segoe UI", 8.5F) };
        _cmbAudioDevice = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(70, 58),
            Size = new Size(240, 24),
            Font = new Font("Segoe UI", 8.5F)
        };

        // Local Media File controls
        _btnSelectFile = new Button
        {
            Text = "📁 Browse File...",
            Location = new Point(70, 58),
            Size = new Size(110, 26),
            Font = new Font("Segoe UI", 8F),
            BackColor = Color.FromArgb(233, 236, 239),
            FlatStyle = FlatStyle.Flat,
            Visible = false
        };
        _btnSelectFile.FlatAppearance.BorderSize = 0;
        _btnSelectFile.Click += BtnSelectFile_Click;

        _lblSelectedFile = new Label
        {
            Text = "No file selected",
            Location = new Point(70, 90),
            AutoSize = true,
            Font = new Font("Segoe UI", 8F, FontStyle.Italic),
            ForeColor = Color.FromArgb(108, 117, 125),
            Visible = false
        };

        // Browser Tab Audio controls
        _btnBrowserHelper = new Button
        {
            Text = "🌐 Open Chrome Sharing Helper",
            Location = new Point(70, 58),
            Size = new Size(200, 26),
            Font = new Font("Segoe UI", 8F),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Visible = false
        };
        _btnBrowserHelper.FlatAppearance.BorderSize = 0;
        _btnBrowserHelper.Click += (s, e) => _browserCaptureServer.OpenCapturePageInBrowser(_appSettings.Chrome.CustomChromeExecutablePath);

        _lblBrowserStatus = new Label
        {
            Text = "Tab audio stream status: Disconnected",
            Location = new Point(70, 90),
            AutoSize = true,
            Font = new Font("Segoe UI", 8F),
            ForeColor = Color.FromArgb(108, 117, 125),
            Visible = false
        };

        _browserCaptureServer.AudioTrackStatusChanged += (s, hasAudio) =>
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    _lblBrowserStatus.Text = hasAudio
                        ? "Tab audio stream: Connected and streaming"
                        : "Tab audio not shared (re-select tab and check Share Audio)";
                    _lblBrowserStatus.ForeColor = hasAudio ? Color.FromArgb(21, 87, 36) : Color.FromArgb(114, 28, 36);
                }));
            }
        };

        grpSource.Controls.Add(lblSrc);
        grpSource.Controls.Add(_cmbSourceType);
        grpSource.Controls.Add(lblDev);
        grpSource.Controls.Add(_cmbAudioDevice);
        grpSource.Controls.Add(_btnSelectFile);
        grpSource.Controls.Add(_lblSelectedFile);
        grpSource.Controls.Add(_btnBrowserHelper);
        grpSource.Controls.Add(_lblBrowserStatus);

        // 2. Language & Model Card
        var grpConfig = new GroupBox
        {
            Text = "Transcription Configuration",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41),
            Dock = DockStyle.Fill,
            Height = 160,
            Padding = new Padding(12)
        };

        var lblLang = new Label { Text = "Language:", Location = new Point(14, 28), AutoSize = true, Font = new Font("Segoe UI", 8.5F) };
        _cmbLanguage = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(85, 24),
            Size = new Size(160, 24),
            Font = new Font("Segoe UI", 8.5F)
        };
        _cmbLanguage.Items.AddRange(new object[] { "Taglish (Recommended)", "English", "Filipino / Tagalog", "Auto Detect" });
        _cmbLanguage.SelectedIndex = 0;

        var lblMod = new Label { Text = "Model:", Location = new Point(14, 62), AutoSize = true, Font = new Font("Segoe UI", 8.5F) };
        _cmbModel = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(85, 58),
            Size = new Size(160, 24),
            Font = new Font("Segoe UI", 8.5F)
        };
        _cmbModel.Items.AddRange(new object[] { "Small", "Base", "Tiny", "Medium" });
        _cmbModel.SelectedIndex = 0;

        _btnManageModels = new Button
        {
            Text = "Manage Models...",
            Location = new Point(255, 57),
            Size = new Size(115, 26),
            Font = new Font("Segoe UI", 8F),
            BackColor = Color.FromArgb(233, 236, 239),
            FlatStyle = FlatStyle.Flat
        };
        _btnManageModels.FlatAppearance.BorderSize = 0;
        _btnManageModels.Click += (s, e) =>
        {
            using var dlg = new WhisperModelManagerForm(_modelManager);
            dlg.ShowDialog(this);
        };

        grpConfig.Controls.Add(lblLang);
        grpConfig.Controls.Add(_cmbLanguage);
        grpConfig.Controls.Add(lblMod);
        grpConfig.Controls.Add(_cmbModel);
        grpConfig.Controls.Add(_btnManageModels);

        tblTop.Controls.Add(grpSource, 0, 0);
        tblTop.Controls.Add(grpConfig, 1, 0);

        // 3. Controls & Metrics Bar
        var pnlBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            Padding = new Padding(0, 8, 0, 8)
        };

        _btnStartStop = new Button
        {
            Text = "🎙 Start Transcription",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            BackColor = Color.FromArgb(40, 167, 69),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(180, 34),
            Location = new Point(0, 8)
        };
        _btnStartStop.FlatAppearance.BorderSize = 0;
        _btnStartStop.Click += BtnStartStop_Click;

        _lblStatusBadge = new Label
        {
            Text = "● Idle",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(108, 117, 125),
            Location = new Point(190, 16),
            AutoSize = true
        };

        _lblMetrics = new Label
        {
            Text = "Live: 00:00:00 | Processed: 00:00:00 | Delay: 0s",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(73, 80, 87),
            Location = new Point(270, 17),
            AutoSize = true
        };

        pnlBar.Controls.Add(_btnStartStop);
        pnlBar.Controls.Add(_lblStatusBadge);
        pnlBar.Controls.Add(_lblMetrics);

        // 4. Live Transcript Editor & Exporter
        var grpTranscript = new GroupBox
        {
            Text = "Live Transcript (Editable)",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41),
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            Height = 320
        };

        _txtTranscript = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Segoe UI", 9.5F),
            BackColor = Color.White
        };

        var pnlExport = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 6, 0, 0)
        };

        _btnExportTxt = new Button { Text = "📄 Export TXT", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Size = new Size(110, 30), BackColor = Color.FromArgb(233, 236, 239), FlatStyle = FlatStyle.Flat };
        _btnExportTxt.FlatAppearance.BorderSize = 0;
        _btnExportTxt.Click += async (s, e) => await ExportCurrentAsync("txt");

        _btnExportSrt = new Button { Text = "🎬 Export SRT", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Size = new Size(110, 30), BackColor = Color.FromArgb(233, 236, 239), FlatStyle = FlatStyle.Flat };
        _btnExportSrt.FlatAppearance.BorderSize = 0;
        _btnExportSrt.Click += async (s, e) => await ExportCurrentAsync("srt");

        _btnExportVtt = new Button { Text = "🌐 Export VTT", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Size = new Size(110, 30), BackColor = Color.FromArgb(233, 236, 239), FlatStyle = FlatStyle.Flat };
        _btnExportVtt.FlatAppearance.BorderSize = 0;
        _btnExportVtt.Click += async (s, e) => await ExportCurrentAsync("vtt");

        _btnOpenFolder = new Button { Text = "📂 Open Transcripts Folder", Font = new Font("Segoe UI", 8.5F), Size = new Size(170, 30), BackColor = Color.FromArgb(233, 236, 239), FlatStyle = FlatStyle.Flat };
        _btnOpenFolder.FlatAppearance.BorderSize = 0;
        _btnOpenFolder.Click += (s, e) =>
        {
            string dir = Path.Combine(_appSettings.Capture.OutputFolder, DateTime.Now.ToString("yyyy-MM-dd"), "Transcripts");
            Directory.CreateDirectory(dir);
            Process.Start(new ProcessStartInfo { FileName = dir, UseShellExecute = true });
        };

        pnlExport.Controls.Add(_btnExportTxt);
        pnlExport.Controls.Add(_btnExportSrt);
        pnlExport.Controls.Add(_btnExportVtt);
        pnlExport.Controls.Add(_btnOpenFolder);

        grpTranscript.Controls.Add(_txtTranscript);
        grpTranscript.Controls.Add(pnlExport);

        Controls.Add(grpTranscript);
        Controls.Add(pnlBar);
        Controls.Add(tblTop);
    }

    private void PopulateAudioDevices()
    {
        _cmbAudioDevice.Items.Clear();
        _cmbAudioDevice.Items.Add("Default Windows Audio Endpoint");

        var devices = SystemAudioSource.GetRenderDevices();
        foreach (var dev in devices)
        {
            _cmbAudioDevice.Items.Add(dev.FriendlyName);
        }

        _cmbAudioDevice.SelectedIndex = 0;
    }

    private void UpdateSourceTypeVisibility()
    {
        int idx = _cmbSourceType.SelectedIndex;
        _cmbAudioDevice.Visible = (idx == 0);
        _btnSelectFile.Visible = (idx == 2);
        _lblSelectedFile.Visible = (idx == 2);
        _btnBrowserHelper.Visible = (idx == 1);
        _lblBrowserStatus.Visible = (idx == 1);
    }

    private void BtnSelectFile_Click(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Select Audio or Video File for Transcription",
            Filter = "Media Files (*.mp4;*.mkv;*.mov;*.avi;*.webm;*.wav;*.mp3;*.m4a;*.flac;*.ogg)|*.mp4;*.mkv;*.mov;*.avi;*.webm;*.wav;*.mp3;*.m4a;*.flac;*.ogg|All Files (*.*)|*.*"
        };

        if (ofd.ShowDialog(this) == DialogResult.OK)
        {
            _localMediaFilePath = ofd.FileName;
            _lblSelectedFile.Text = Path.GetFileName(_localMediaFilePath);
            _lblSelectedFile.ForeColor = Color.FromArgb(33, 37, 41);
        }
    }

    private async void BtnStartStop_Click(object? sender, EventArgs e)
    {
        if (_activeSession != null)
        {
            await StopTranscriptionAsync();
        }
        else
        {
            await StartTranscriptionAsync();
        }
    }

    private async Task StartTranscriptionAsync()
    {
        string modelName = _cmbModel.SelectedItem?.ToString() ?? "Small";
        if (!_modelManager.IsModelInstalled(modelName))
        {
            var ask = MessageBox.Show(
                this,
                $"The selected Whisper model '{modelName}' is not installed.\nWould you like to open the Model Manager to download it?",
                "Model Not Installed",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (ask == DialogResult.Yes)
            {
                using var dlg = new WhisperModelManagerForm(_modelManager);
                dlg.ShowDialog(this);
                if (!_modelManager.IsModelInstalled(modelName)) return;
            }
            else return;
        }

        string modelPath = _modelManager.GetModelPath(modelName);
        await _transcriptionService.LoadModelAsync(modelPath);

        IAudioSource audioSource;
        int srcType = _cmbSourceType.SelectedIndex;

        if (srcType == 0)
        {
            // System Audio
            audioSource = new SystemAudioSource();
        }
        else if (srcType == 1)
        {
            // Browser Tab Audio
            if (!_browserCaptureServer.IsRunning)
            {
                _browserCaptureServer.Start();
            }

            var browserSrc = new BrowserAudioSource();
            _activeBrowserAudioSource = browserSrc;
            _browserCaptureServer.AudioDataReceived += (s, data) => browserSrc.PushPcmData(data, 0, data.Length);

            if (!_browserCaptureServer.HasActiveStream)
            {
                _browserCaptureServer.OpenCapturePageInBrowser(_appSettings.Chrome.CustomChromeExecutablePath);
            }

            audioSource = browserSrc;
        }
        else
        {
            // Local File
            if (string.IsNullOrEmpty(_localMediaFilePath) || !File.Exists(_localMediaFilePath))
            {
                MessageBox.Show(this, "Please select an audio or video file first.", "No File Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            audioSource = new MediaFileAudioSource(_localMediaFilePath, _ffmpegService);
        }

        string outputDir = Path.Combine(
            !string.IsNullOrWhiteSpace(_appSettings.Capture.OutputFolder) ? _appSettings.Capture.OutputFolder : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "AutoSnap"),
            DateTime.Now.ToString("yyyy-MM-dd"),
            "Transcripts");

        _sessionCts = new CancellationTokenSource();
        _activeSession = new TranscriptionSession(
            audioSource,
            _transcriptionService,
            _recoveryService,
            modelName,
            (TranscriptionLanguageMode)_cmbLanguage.SelectedIndex,
            outputDir);

        _activeSession.StateChanged += (s, state) =>
        {
            if (InvokeRequired) BeginInvoke(new Action(() => UpdateStateBadge(state)));
            else UpdateStateBadge(state);
        };

        _activeSession.LatencyUpdated += (s, metrics) =>
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    _lblMetrics.Text = $"Live: {metrics.LiveTime:hh\\:mm\\:ss} | Processed: {metrics.ProcessedTime:hh\\:mm\\:ss} | Delay: {metrics.DelaySeconds:0}s";
                }));
            }
            else
            {
                _lblMetrics.Text = $"Live: {metrics.LiveTime:hh\\:mm\\:ss} | Processed: {metrics.ProcessedTime:hh\\:mm\\:ss} | Delay: {metrics.DelaySeconds:0}s";
            }
        };

        _activeSession.SegmentProduced += (s, seg) =>
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    _txtTranscript.AppendText($"[{seg.Start:hh\\:mm\\:ss}]\r\n{seg.Text}\r\n\r\n");
                }));
            }
            else
            {
                _txtTranscript.AppendText($"[{seg.Start:hh\\:mm\\:ss}]\r\n{seg.Text}\r\n\r\n");
            }
        };

        _btnStartStop.Text = "⏹ Stop Transcription";
        _btnStartStop.BackColor = Color.FromArgb(220, 53, 69);
        _cmbSourceType.Enabled = false;
        _cmbLanguage.Enabled = false;
        _cmbModel.Enabled = false;

        await _activeSession.StartAsync(_sessionCts.Token);
    }

    private void UpdateStateBadge(TranscriptionState state)
    {
        switch (state)
        {
            case TranscriptionState.Listening:
                _lblStatusBadge.Text = "● Listening / Live";
                _lblStatusBadge.ForeColor = Color.FromArgb(40, 167, 69);
                break;
            case TranscriptionState.Preparing:
                _lblStatusBadge.Text = "● Preparing...";
                _lblStatusBadge.ForeColor = Color.FromArgb(255, 193, 7);
                break;
            case TranscriptionState.Processing:
                _lblStatusBadge.Text = "● Processing Audio";
                _lblStatusBadge.ForeColor = Color.FromArgb(0, 123, 255);
                break;
            case TranscriptionState.Finalizing:
                _lblStatusBadge.Text = "● Finalizing...";
                _lblStatusBadge.ForeColor = Color.FromArgb(108, 117, 125);
                break;
            case TranscriptionState.Completed:
                _lblStatusBadge.Text = "● Completed";
                _lblStatusBadge.ForeColor = Color.FromArgb(21, 87, 36);
                break;
            default:
                _lblStatusBadge.Text = "● Idle";
                _lblStatusBadge.ForeColor = Color.FromArgb(108, 117, 125);
                break;
        }
    }

    private async Task StopTranscriptionAsync()
    {
        if (_activeSession == null) return;

        _btnStartStop.Enabled = false;
        try
        {
            await _activeSession.StopAsync();
            UpdateStateBadge(TranscriptionState.Completed);
        }
        finally
        {
            _activeSession.Dispose();
            _activeSession = null;
            _sessionCts?.Dispose();
            _sessionCts = null;

            _btnStartStop.Text = "🎙 Start Transcription";
            _btnStartStop.BackColor = Color.FromArgb(40, 167, 69);
            _btnStartStop.Enabled = true;
            _cmbSourceType.Enabled = true;
            _cmbLanguage.Enabled = true;
            _cmbModel.Enabled = true;
        }
    }

    private async Task ExportCurrentAsync(string format)
    {
        if (string.IsNullOrWhiteSpace(_txtTranscript.Text))
        {
            MessageBox.Show(this, "No transcript available to export.", "Empty Transcript", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Title = $"Export Transcript as .{format.ToUpperInvariant()}",
            Filter = format switch
            {
                "srt" => "SubRip Subtitle (*.srt)|*.srt",
                "vtt" => "WebVTT Subtitle (*.vtt)|*.vtt",
                _ => "Text Document (*.txt)|*.txt"
            },
            FileName = $"transcript_{DateTime.Now:yyyyMMdd_HHmmss}.{format}"
        };

        if (sfd.ShowDialog(this) == DialogResult.OK)
        {
            string content = _txtTranscript.Text;
            if (_activeSession != null && _activeSession.Document.Segments.Count > 0)
            {
                content = format switch
                {
                    "srt" => TranscriptExporter.ToSrt(_activeSession.Document),
                    "vtt" => TranscriptExporter.ToVtt(_activeSession.Document),
                    _ => TranscriptExporter.ToTxt(_activeSession.Document)
                };
            }

            await File.WriteAllTextAsync(sfd.FileName, content, System.Text.Encoding.UTF8);
            MessageBox.Show(this, $"Transcript saved to:\n{sfd.FileName}", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
