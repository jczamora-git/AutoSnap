using System.Diagnostics;
using AutoSnap.Models;
using AutoSnap.Transcription;
using AutoSnap.Video;

namespace AutoSnap.Forms;

public class VideoProcessingControl : UserControl
{
    private readonly FFmpegService _ffmpegService;
    private readonly FFprobeService _ffprobeService;
    private readonly WhisperTranscriptionService _transcriptionService;
    private readonly WhisperModelManager _modelManager;
    private readonly TranscriptRecoveryService _recoveryService;
    private readonly AppSettings _appSettings;

    private VideoInfo? _selectedVideo;
    private CancellationTokenSource? _processingCts;

    // Controls
    private Label _lblFileVal = null!;
    private Label _lblDurationVal = null!;
    private Label _lblResolutionVal = null!;
    private Label _lblFpsVal = null!;
    private Label _lblCodecVal = null!;
    private Label _lblAudioVal = null!;

    private CheckBox _chkExtractSnapshots = null!;
    private ComboBox _cmbInterval = null!;
    private Label _lblEstimatedCount = null!;
    private ComboBox _cmbFormat = null!;

    private CheckBox _chkGenerateTranscript = null!;
    private ComboBox _cmbLanguage = null!;
    private ComboBox _cmbModel = null!;
    private Button _btnManageModels = null!;

    private Button _btnBrowse = null!;
    private Button _btnProcess = null!;
    private Button _btnCancel = null!;
    private Button _btnOpenFolder = null!;

    private ProgressBar _progressBar = null!;
    private Label _lblProgressStatus = null!;
    private Label _lblProgressCount = null!;
    private Label _lblProgressTime = null!;
    private TextBox _txtTranscriptPreview = null!;
    private Panel _pnlProgress = null!;

    public VideoProcessingControl(
        FFmpegService ffmpegService,
        FFprobeService ffprobeService,
        WhisperTranscriptionService transcriptionService,
        WhisperModelManager modelManager,
        TranscriptRecoveryService recoveryService,
        AppSettings appSettings)
    {
        _ffmpegService = ffmpegService;
        _ffprobeService = ffprobeService;
        _transcriptionService = transcriptionService;
        _modelManager = modelManager;
        _recoveryService = recoveryService;
        _appSettings = appSettings;

        InitializeUi();
    }

    private void InitializeUi()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(248, 249, 250);
        AutoScroll = true;
        Padding = new Padding(20);

        var tblMain = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 4
        };
        tblMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tblMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

        // 1. Video Selection & Info Card
        var grpInfo = new GroupBox
        {
            Text = "Selected Video File",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41),
            Dock = DockStyle.Fill,
            Padding = new Padding(14),
            Height = 220
        };

        _btnBrowse = new Button
        {
            Text = "📁 Browse Video File...",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(170, 32),
            Location = new Point(16, 26)
        };
        _btnBrowse.FlatAppearance.BorderSize = 0;
        _btnBrowse.Click += BtnBrowse_Click;

        var pnlMeta = new TableLayoutPanel
        {
            Location = new Point(16, 68),
            Size = new Size(380, 140),
            ColumnCount = 2,
            RowCount = 6
        };
        pnlMeta.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
        pnlMeta.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        void AddMetaRow(string label, out Label valLabel, int row)
        {
            var lbl = new Label { Text = label, ForeColor = Color.FromArgb(108, 117, 125), Font = new Font("Segoe UI", 8.5F), AutoSize = true, Margin = new Padding(0, 2, 0, 2) };
            valLabel = new Label { Text = "—", ForeColor = Color.FromArgb(33, 37, 41), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 2, 0, 2) };
            pnlMeta.Controls.Add(lbl, 0, row);
            pnlMeta.Controls.Add(valLabel, 1, row);
        }

        AddMetaRow("File:", out _lblFileVal, 0);
        AddMetaRow("Duration:", out _lblDurationVal, 1);
        AddMetaRow("Resolution:", out _lblResolutionVal, 2);
        AddMetaRow("Frame Rate:", out _lblFpsVal, 3);
        AddMetaRow("Codec:", out _lblCodecVal, 4);
        AddMetaRow("Audio:", out _lblAudioVal, 5);

        grpInfo.Controls.Add(_btnBrowse);
        grpInfo.Controls.Add(pnlMeta);

        // 2. Processing Options Card
        var grpOptions = new GroupBox
        {
            Text = "Processing Options",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41),
            Dock = DockStyle.Fill,
            Padding = new Padding(14),
            Height = 220
        };

        _chkExtractSnapshots = new CheckBox
        {
            Text = "Extract Snapshots",
            Checked = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Location = new Point(16, 26),
            AutoSize = true
        };
        _chkExtractSnapshots.CheckedChanged += (s, e) => UpdateOptionsState();

        var lblInt = new Label { Text = "Interval:", Location = new Point(36, 54), AutoSize = true, Font = new Font("Segoe UI", 8.5F) };
        _cmbInterval = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(90, 50),
            Size = new Size(110, 24),
            Font = new Font("Segoe UI", 8.5F)
        };
        _cmbInterval.Items.AddRange(new object[] { "1 second", "5 seconds", "10 seconds", "30 seconds", "1 minute", "5 minutes", "10 minutes" });
        _cmbInterval.SelectedIndex = 4; // 1 minute
        _cmbInterval.SelectedIndexChanged += (s, e) => UpdateEstimatedSnapshots();

        _lblEstimatedCount = new Label
        {
            Text = "Est. snapshots: —",
            Location = new Point(210, 54),
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
            ForeColor = Color.FromArgb(108, 117, 125)
        };

        var lblFmt = new Label { Text = "Format:", Location = new Point(36, 84), AutoSize = true, Font = new Font("Segoe UI", 8.5F) };
        _cmbFormat = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(90, 80),
            Size = new Size(110, 24),
            Font = new Font("Segoe UI", 8.5F)
        };
        _cmbFormat.Items.AddRange(new object[] { "JPEG (90%)", "PNG (Lossless)" });
        _cmbFormat.SelectedIndex = 0;

        _chkGenerateTranscript = new CheckBox
        {
            Text = "Generate Transcript",
            Checked = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Location = new Point(16, 115),
            AutoSize = true
        };
        _chkGenerateTranscript.CheckedChanged += (s, e) => UpdateOptionsState();

        var lblLang = new Label { Text = "Language:", Location = new Point(36, 143), AutoSize = true, Font = new Font("Segoe UI", 8.5F) };
        _cmbLanguage = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(105, 140),
            Size = new Size(140, 24),
            Font = new Font("Segoe UI", 8.5F)
        };
        _cmbLanguage.Items.AddRange(new object[] { "Taglish (Recommended)", "English", "Filipino / Tagalog", "Auto Detect" });
        _cmbLanguage.SelectedIndex = 0;

        var lblMod = new Label { Text = "Model:", Location = new Point(36, 173), AutoSize = true, Font = new Font("Segoe UI", 8.5F) };
        _cmbModel = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(105, 170),
            Size = new Size(140, 24),
            Font = new Font("Segoe UI", 8.5F)
        };
        _cmbModel.Items.AddRange(new object[] { "Small", "Base", "Tiny", "Medium", "Large v3", "Large v3 Turbo", "Large v3 Turbo Q5" });
        _cmbModel.SelectedIndex = 0;

        _btnManageModels = new Button
        {
            Text = "Manage...",
            Location = new Point(255, 169),
            Size = new Size(80, 26),
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

        grpOptions.Controls.Add(_chkExtractSnapshots);
        grpOptions.Controls.Add(lblInt);
        grpOptions.Controls.Add(_cmbInterval);
        grpOptions.Controls.Add(_lblEstimatedCount);
        grpOptions.Controls.Add(lblFmt);
        grpOptions.Controls.Add(_cmbFormat);
        grpOptions.Controls.Add(_chkGenerateTranscript);
        grpOptions.Controls.Add(lblLang);
        grpOptions.Controls.Add(_cmbLanguage);
        grpOptions.Controls.Add(lblMod);
        grpOptions.Controls.Add(_cmbModel);
        grpOptions.Controls.Add(_btnManageModels);

        tblMain.Controls.Add(grpInfo, 0, 0);
        tblMain.Controls.Add(grpOptions, 1, 0);

        // 3. Action Buttons Panel
        var pnlActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0, 12, 0, 12)
        };

        _btnProcess = new Button
        {
            Text = "▶ Process Video",
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            BackColor = Color.FromArgb(40, 167, 69),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(160, 38),
            Enabled = false
        };
        _btnProcess.FlatAppearance.BorderSize = 0;
        _btnProcess.Click += BtnProcess_Click;

        _btnCancel = new Button
        {
            Text = "⏹ Cancel",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            BackColor = Color.FromArgb(220, 53, 69),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(110, 38),
            Visible = false
        };
        _btnCancel.FlatAppearance.BorderSize = 0;
        _btnCancel.Click += (s, e) => _processingCts?.Cancel();

        _btnOpenFolder = new Button
        {
            Text = "📂 Open Output Folder",
            Font = new Font("Segoe UI", 9F),
            BackColor = Color.FromArgb(233, 236, 239),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(160, 38)
        };
        _btnOpenFolder.FlatAppearance.BorderSize = 0;
        _btnOpenFolder.Click += (s, e) =>
        {
            string baseDir = !string.IsNullOrWhiteSpace(_appSettings.Capture.OutputFolder)
                ? _appSettings.Capture.OutputFolder
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "AutoSnap");
            if (Directory.Exists(baseDir))
            {
                Process.Start(new ProcessStartInfo { FileName = baseDir, UseShellExecute = true });
            }
        };

        pnlActions.Controls.Add(_btnProcess);
        pnlActions.Controls.Add(_btnCancel);
        pnlActions.Controls.Add(_btnOpenFolder);

        // 4. Progress & Transcript Preview Panel
        _pnlProgress = new Panel
        {
            Dock = DockStyle.Top,
            Height = 260,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(16),
            Margin = new Padding(0, 10, 0, 10)
        };

        _lblProgressStatus = new Label
        {
            Text = "Ready to process video.",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41),
            Location = new Point(16, 12),
            AutoSize = true
        };

        _progressBar = new ProgressBar
        {
            Location = new Point(16, 38),
            Size = new Size(740, 20),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        _lblProgressCount = new Label
        {
            Text = "0 / 0 snapshots (0%)",
            Location = new Point(16, 64),
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(108, 117, 125)
        };

        _lblProgressTime = new Label
        {
            Text = "Current: 00:00:00 / 00:00:00",
            Location = new Point(240, 64),
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(108, 117, 125)
        };

        var lblPreviewTitle = new Label
        {
            Text = "Live Transcript Preview:",
            Location = new Point(16, 92),
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(73, 80, 87)
        };

        _txtTranscriptPreview = new TextBox
        {
            Location = new Point(16, 114),
            Size = new Size(740, 125),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 9F),
            BackColor = Color.FromArgb(248, 249, 250)
        };

        _pnlProgress.Controls.Add(_lblProgressStatus);
        _pnlProgress.Controls.Add(_progressBar);
        _pnlProgress.Controls.Add(_lblProgressCount);
        _pnlProgress.Controls.Add(_lblProgressTime);
        _pnlProgress.Controls.Add(lblPreviewTitle);
        _pnlProgress.Controls.Add(_txtTranscriptPreview);

        Controls.Add(_pnlProgress);
        Controls.Add(pnlActions);
        Controls.Add(tblMain);
    }

    private void UpdateOptionsState()
    {
        bool hasVideo = _selectedVideo != null;
        _btnProcess.Enabled = hasVideo && (_chkExtractSnapshots.Checked || _chkGenerateTranscript.Checked);
        _cmbInterval.Enabled = _chkExtractSnapshots.Checked;
        _cmbFormat.Enabled = _chkExtractSnapshots.Checked;
        _cmbLanguage.Enabled = _chkGenerateTranscript.Checked;
        _cmbModel.Enabled = _chkGenerateTranscript.Checked;
    }

    private void UpdateEstimatedSnapshots()
    {
        if (_selectedVideo == null || _selectedVideo.Duration <= TimeSpan.Zero)
        {
            _lblEstimatedCount.Text = "Est. snapshots: —";
            return;
        }

        var interval = GetSelectedInterval();
        if (interval > TimeSpan.Zero)
        {
            int count = (int)Math.Ceiling(_selectedVideo.Duration.TotalSeconds / interval.TotalSeconds);
            _lblEstimatedCount.Text = $"Est. snapshots: ~{count}";
        }
    }

    private TimeSpan GetSelectedInterval() => _cmbInterval.SelectedIndex switch
    {
        0 => TimeSpan.FromSeconds(1),
        1 => TimeSpan.FromSeconds(5),
        2 => TimeSpan.FromSeconds(10),
        3 => TimeSpan.FromSeconds(30),
        4 => TimeSpan.FromMinutes(1),
        5 => TimeSpan.FromMinutes(5),
        6 => TimeSpan.FromMinutes(10),
        _ => TimeSpan.FromMinutes(1)
    };

    private async void BtnBrowse_Click(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Select Local Video File",
            Filter = "Video Files (*.mp4;*.mkv;*.mov;*.avi;*.webm;*.m4v)|*.mp4;*.mkv;*.mov;*.avi;*.webm;*.m4v|All Files (*.*)|*.*"
        };

        if (ofd.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                _lblProgressStatus.Text = "Reading video metadata...";
                _selectedVideo = await _ffprobeService.GetVideoInfoAsync(ofd.FileName);

                _lblFileVal.Text = _selectedVideo.FileName;
                _lblDurationVal.Text = _selectedVideo.FormattedDuration;
                _lblResolutionVal.Text = _selectedVideo.ResolutionString;
                _lblFpsVal.Text = _selectedVideo.FrameRateString;
                _lblCodecVal.Text = !string.IsNullOrEmpty(_selectedVideo.Codec) ? _selectedVideo.Codec : "Unknown";
                _lblAudioVal.Text = _selectedVideo.HasAudio ? $"Yes ({_selectedVideo.AudioCodec})" : "No audio stream";

                _chkGenerateTranscript.Enabled = _selectedVideo.HasAudio;
                if (!_selectedVideo.HasAudio) _chkGenerateTranscript.Checked = false;

                UpdateEstimatedSnapshots();
                UpdateOptionsState();
                _lblProgressStatus.Text = "Ready to process video.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not inspect video: {ex.Message}", "FFprobe Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private async void BtnProcess_Click(object? sender, EventArgs e)
    {
        if (_selectedVideo == null) return;

        if (_chkGenerateTranscript.Checked)
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
        }

        var cts = new CancellationTokenSource();
        _processingCts = cts;
        CancellationToken token = cts.Token;

        _btnProcess.Visible = false;
        _btnCancel.Visible = true;
        _btnBrowse.Enabled = false;
        _txtTranscriptPreview.Clear();
        _progressBar.Value = 0;

        var options = new VideoProcessingOptions
        {
            ExtractSnapshots = _chkExtractSnapshots.Checked,
            SnapshotInterval = GetSelectedInterval(),
            Format = _cmbFormat.SelectedIndex == 1 ? ImageFormatType.Png : ImageFormatType.Jpg,
            GenerateTranscript = _chkGenerateTranscript.Checked,
            LanguageMode = (TranscriptionLanguageMode)_cmbLanguage.SelectedIndex,
            WhisperModelType = _cmbModel.SelectedItem?.ToString() ?? "Small",
            OutputBaseDirectory = _appSettings.Capture.OutputFolder
        };

        var session = new VideoProcessingSession(_ffmpegService, _transcriptionService, _modelManager, _recoveryService);

        session.StatusMessageChanged += (s, msg) =>
        {
            if (InvokeRequired) BeginInvoke(new Action(() => _lblProgressStatus.Text = msg));
            else _lblProgressStatus.Text = msg;
        };

        session.SnapshotProgressChanged += (s, p) =>
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    _progressBar.Value = Math.Clamp((int)p.Percent, 0, 100);
                    _lblProgressCount.Text = $"{p.Current} / {p.Total} snapshots ({p.Percent:0.0}%)";
                    _lblProgressTime.Text = $"Current: {p.CurrentTime:hh\\:mm\\:ss} / {p.TotalDuration:hh\\:mm\\:ss}";
                }));
            }
            else
            {
                _progressBar.Value = Math.Clamp((int)p.Percent, 0, 100);
                _lblProgressCount.Text = $"{p.Current} / {p.Total} snapshots ({p.Percent:0.0}%)";
                _lblProgressTime.Text = $"Current: {p.CurrentTime:hh\\:mm\\:ss} / {p.TotalDuration:hh\\:mm\\:ss}";
            }
        };

        session.TranscriptSegmentProduced += (s, seg) =>
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    _txtTranscriptPreview.AppendText($"[{seg.Start:hh\\:mm\\:ss}] {seg.Text}\r\n\r\n");
                }));
            }
            else
            {
                _txtTranscriptPreview.AppendText($"[{seg.Start:hh\\:mm\\:ss}] {seg.Text}\r\n\r\n");
            }
        };

        try
        {
            var result = await session.ProcessVideoAsync(_selectedVideo, options, token);
            if (result.Success)
            {
                _progressBar.Value = 100;
                MessageBox.Show(
                    this,
                    $"Video processing completed in {result.ProcessingDuration:mm\\:ss}!\n\nSnapshots: {result.SnapshotCount}\nLocation: {result.SnapshotsDirectory ?? result.TranscriptPath}",
                    "Processing Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(this, result.ErrorMessage ?? "Processing failed.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Error processing video: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnProcess.Visible = true;
            _btnCancel.Visible = false;
            _btnBrowse.Enabled = true;
            var oldCts = Interlocked.Exchange(ref _processingCts, null);
            oldCts?.Dispose();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            var oldCts = Interlocked.Exchange(ref _processingCts, null);
            oldCts?.Cancel();
            oldCts?.Dispose();
        }
        base.Dispose(disposing);
    }
}
