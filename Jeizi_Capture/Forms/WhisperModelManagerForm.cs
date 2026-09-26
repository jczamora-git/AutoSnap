using System.Diagnostics;
using AutoSnap.Hardware;
using AutoSnap.Transcription;

namespace AutoSnap.Forms;

public class WhisperModelManagerForm : Form
{
    private readonly WhisperModelManager _modelManager;
    private readonly SystemHardwareInfo _hardwareInfo;
    private readonly ListView _lvModels;
    private readonly Button _btnDownload;
    private readonly Button _btnDelete;
    private readonly Button _btnClose;
    private readonly ProgressBar _progressBar;
    private readonly Label _lblProgress;

    // Details panel controls
    private readonly Label _lblDetailName;
    private readonly Label _lblDetailSize;
    private readonly Label _lblDetailQuality;
    private readonly Label _lblDetailSpeed;
    private readonly Label _lblDetailRating;
    private readonly Label _lblDetailEstimatedUse;
    private readonly Label _lblDetailDescription;

    private CancellationTokenSource? _downloadCts;
    private Task? _downloadTask;
    private bool _closeRequested;
    private bool _allowClose;

    private bool CanUpdateUi => !IsDisposed && !Disposing && IsHandleCreated;

    public WhisperModelManagerForm(WhisperModelManager modelManager)
    {
        _modelManager = modelManager ?? throw new ArgumentNullException(nameof(modelManager));
        _hardwareInfo = SystemHardwareService.GetSystemHardwareInfo();

        Text = "Manage Whisper Transcription Models";
        Size = new Size(760, 640);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        BackColor = Color.FromArgb(248, 249, 250);

        // 1. Top System Information Card
        var pnlSystem = new Panel
        {
            Location = new Point(16, 12),
            Size = new Size(712, 105),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(10)
        };

        var lblSysHeader = new Label
        {
            Text = "YOUR SYSTEM HARDWARE & RECOMMENDATIONS",
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            ForeColor = Color.FromArgb(108, 117, 125),
            Location = new Point(10, 8),
            AutoSize = true
        };

        var lblCpu = new Label
        {
            Text = $"CPU: {_hardwareInfo.CpuName} ({_hardwareInfo.LogicalCoreCount} logical cores)",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(33, 37, 41),
            Location = new Point(10, 30),
            AutoSize = true
        };

        var lblRam = new Label
        {
            Text = $"Memory: {_hardwareInfo.TotalMemoryFormatted} Physical RAM",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(33, 37, 41),
            Location = new Point(10, 52),
            AutoSize = true
        };

        var lblGpu = new Label
        {
            Text = $"GPU: {_hardwareInfo.GpuName} ({_hardwareInfo.DedicatedVramFormatted})",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(33, 37, 41),
            Location = new Point(360, 30),
            AutoSize = true
        };

        var lblBackend = new Label
        {
            Text = $"Whisper Backend: {_hardwareInfo.WhisperBackend}",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(108, 117, 125),
            Location = new Point(360, 52),
            AutoSize = true
        };

        string recommendedModel = SystemHardwareService.GetPrimaryRecommendedModelName(_hardwareInfo);
        var lblRecBadge = new Label
        {
            Text = $"★ Recommended Model for Your System: {recommendedModel}",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(21, 87, 36),
            BackColor = Color.FromArgb(212, 237, 218),
            Padding = new Padding(6, 3, 6, 3),
            Location = new Point(10, 76),
            AutoSize = true
        };

        pnlSystem.Controls.Add(lblSysHeader);
        pnlSystem.Controls.Add(lblCpu);
        pnlSystem.Controls.Add(lblRam);
        pnlSystem.Controls.Add(lblGpu);
        pnlSystem.Controls.Add(lblBackend);
        pnlSystem.Controls.Add(lblRecBadge);

        // 2. Model List View
        _lvModels = new ListView
        {
            Location = new Point(16, 126),
            Size = new Size(712, 175),
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            MultiSelect = false,
            HideSelection = false
        };

        _lvModels.Columns.Add("Model", 130);
        _lvModels.Columns.Add("Download Size", 95);
        _lvModels.Columns.Add("Status", 100);
        _lvModels.Columns.Add("System Rating", 140);
        _lvModels.Columns.Add("Quality / Speed", 150);

        _lvModels.SelectedIndexChanged += (s, e) => UpdateDetails();

        // 3. Selected Model Details Card
        var pnlDetails = new Panel
        {
            Location = new Point(16, 308),
            Size = new Size(712, 150),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(12)
        };

        _lblDetailName = new Label
        {
            Text = "Select a model to view details",
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41),
            Location = new Point(12, 10),
            AutoSize = true
        };

        _lblDetailSize = new Label
        {
            Text = "Size: —",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(73, 80, 87),
            Location = new Point(14, 38),
            AutoSize = true
        };

        _lblDetailQuality = new Label
        {
            Text = "Quality: —",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(73, 80, 87),
            Location = new Point(160, 38),
            AutoSize = true
        };

        _lblDetailSpeed = new Label
        {
            Text = "Speed: —",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(73, 80, 87),
            Location = new Point(310, 38),
            AutoSize = true
        };

        _lblDetailRating = new Label
        {
            Text = "Your System: —",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(40, 167, 69),
            Location = new Point(480, 36),
            AutoSize = true
        };

        _lblDetailDescription = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(33, 37, 41),
            Location = new Point(14, 64),
            Size = new Size(680, 36)
        };

        _lblDetailEstimatedUse = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
            ForeColor = Color.FromArgb(108, 117, 125),
            Location = new Point(14, 112),
            Size = new Size(680, 24)
        };

        pnlDetails.Controls.Add(_lblDetailName);
        pnlDetails.Controls.Add(_lblDetailSize);
        pnlDetails.Controls.Add(_lblDetailQuality);
        pnlDetails.Controls.Add(_lblDetailSpeed);
        pnlDetails.Controls.Add(_lblDetailRating);
        pnlDetails.Controls.Add(_lblDetailDescription);
        pnlDetails.Controls.Add(_lblDetailEstimatedUse);

        // 4. Progress bar and label
        _progressBar = new ProgressBar
        {
            Location = new Point(16, 468),
            Size = new Size(712, 16),
            Visible = false
        };

        _lblProgress = new Label
        {
            Location = new Point(16, 488),
            Size = new Size(712, 20),
            Text = "",
            ForeColor = Color.FromArgb(73, 80, 87),
            Visible = false
        };

        // 5. Action Buttons
        _btnDownload = new Button
        {
            Text = "📥 Download Model",
            Location = new Point(16, 520),
            Size = new Size(150, 34),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Enabled = false
        };
        _btnDownload.FlatAppearance.BorderSize = 0;
        _btnDownload.Click += BtnDownload_Click;

        _btnDelete = new Button
        {
            Text = "🗑 Delete Model",
            Location = new Point(176, 520),
            Size = new Size(130, 34),
            BackColor = Color.FromArgb(248, 215, 218),
            ForeColor = Color.FromArgb(114, 28, 36),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F),
            Enabled = false
        };
        _btnDelete.FlatAppearance.BorderSize = 0;
        _btnDelete.Click += BtnDelete_Click;

        _btnClose = new Button
        {
            Text = "Close",
            Location = new Point(628, 520),
            Size = new Size(100, 34),
            BackColor = Color.FromArgb(233, 236, 239),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F)
        };
        _btnClose.FlatAppearance.BorderSize = 0;
        _btnClose.Click += (s, e) => Close();

        Controls.Add(pnlSystem);
        Controls.Add(_lvModels);
        Controls.Add(pnlDetails);
        Controls.Add(_progressBar);
        Controls.Add(_lblProgress);
        Controls.Add(_btnDownload);
        Controls.Add(_btnDelete);
        Controls.Add(_btnClose);

        RefreshModelList();
    }

    private WhisperModelInfo? GetSelectedModel()
    {
        if (_lvModels.SelectedItems.Count == 0) return null;
        return _lvModels.SelectedItems[0].Tag as WhisperModelInfo;
    }

    private void RefreshModelList()
    {
        if (IsDisposed || Disposing) return;

        _lvModels.BeginUpdate();
        try
        {
            _lvModels.Items.Clear();
            foreach (var model in WhisperModelManager.AvailableModels)
            {
                bool installed = _modelManager.IsModelInstalled(model.Name);
                var rec = SystemHardwareService.GetRecommendation(model, _hardwareInfo);

                var item = new ListViewItem(model.Name);
                item.SubItems.Add(model.SizeFormatted);
                item.SubItems.Add(installed ? "● Installed" : "○ Available");
                item.SubItems.Add(rec.RatingText);
                item.SubItems.Add($"{model.QualityTier} / {model.SpeedTier}");
                item.Tag = model;

                if (installed)
                {
                    item.ForeColor = Color.FromArgb(21, 87, 36);
                }
                else if (rec.Rating == ModelRating.NotRecommended)
                {
                    item.ForeColor = Color.FromArgb(108, 117, 125);
                }

                _lvModels.Items.Add(item);
            }

            if (_lvModels.Items.Count > 0 && _lvModels.SelectedItems.Count == 0)
            {
                string recName = SystemHardwareService.GetPrimaryRecommendedModelName(_hardwareInfo);
                ListViewItem? target = null;
                foreach (ListViewItem item in _lvModels.Items)
                {
                    if (item.Tag is WhisperModelInfo m && m.Name.Equals(recName, StringComparison.OrdinalIgnoreCase))
                    {
                        target = item;
                        break;
                    }
                }
                target ??= _lvModels.Items[0];
                target.Selected = true;
            }
        }
        finally
        {
            _lvModels.EndUpdate();
        }

        UpdateDetails();
    }

    private void UpdateDetails()
    {
        if (IsDisposed || Disposing) return;

        if (_downloadTask is { IsCompleted: false })
        {
            // During download, keep UI state locked
            return;
        }

        var model = GetSelectedModel();
        if (model == null)
        {
            _btnDownload.Enabled = false;
            _btnDelete.Enabled = false;
            _lblDetailName.Text = "Select a model from the list";
            _lblDetailSize.Text = "Size: —";
            _lblDetailQuality.Text = "Quality: —";
            _lblDetailSpeed.Text = "Speed: —";
            _lblDetailRating.Text = "";
            _lblDetailDescription.Text = "";
            _lblDetailEstimatedUse.Text = "";
            return;
        }

        bool installed = _modelManager.IsModelInstalled(model.Name);
        var rec = SystemHardwareService.GetRecommendation(model, _hardwareInfo);

        _lblDetailName.Text = $"{model.Name} Model";
        _lblDetailSize.Text = $"Download Size: {model.SizeFormatted}";
        _lblDetailQuality.Text = $"Quality: {model.QualityTier}";
        _lblDetailSpeed.Text = $"Speed: {model.SpeedTier}";

        _lblDetailRating.Text = $"Your System: {rec.LiveRatingText}";
        _lblDetailRating.ForeColor = rec.LiveRatingColor;

        _lblDetailDescription.Text = $"{model.Description}\nLive: {rec.LiveGuidance} | Offline: {rec.OfflineGuidance}";
        _lblDetailEstimatedUse.Text = $"Performance: {rec.EstimatedUseSummary}";

        _btnDownload.Enabled = !installed;
        _btnDownload.Text = "📥 Download Model";
        _btnDelete.Enabled = installed;
    }

    private void SetDownloadingUi(bool downloading)
    {
        if (IsDisposed || Disposing) return;

        if (downloading)
        {
            _btnDownload.Text = "⏹ Cancel Download";
            _btnDownload.Enabled = true;
            _btnDelete.Enabled = false;
            _btnClose.Enabled = false;
            _lvModels.Enabled = false;
            _progressBar.Visible = true;
            _lblProgress.Visible = true;
            _progressBar.Value = 0;
            _lblProgress.Text = "Starting download...";
        }
        else
        {
            _btnDownload.Text = "📥 Download Model";
            _btnClose.Enabled = true;
            _lvModels.Enabled = true;
            _progressBar.Visible = false;
            _lblProgress.Visible = false;
            UpdateDetails();
        }
    }

    private void RequestDownloadCancellation()
    {
        var cts = _downloadCts;
        if (cts == null) return;

        try
        {
            cts.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Defensive race guard
        }
    }

    private async void BtnDownload_Click(object? sender, EventArgs e)
    {
        // If download is actively running, this button acts as Cancel
        if (_downloadTask is { IsCompleted: false })
        {
            RequestDownloadCancellation();
            if (CanUpdateUi)
            {
                _btnDownload.Enabled = false;
                _lblProgress.Text = "Cancelling download...";
            }
            return;
        }

        var model = GetSelectedModel();
        if (model == null) return;

        var rec = SystemHardwareService.GetRecommendation(model, _hardwareInfo);
        if (rec.Rating == ModelRating.NotRecommended)
        {
            var warn = MessageBox.Show(
                this,
                $"'{model.Name}' is demanding for your system ({_hardwareInfo.TotalMemoryFormatted} RAM, {_hardwareInfo.LogicalCoreCount} cores).\n{rec.GuidanceText}\n\nDo you want to proceed with downloading anyway?",
                "Demanding Model Notice",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (warn != DialogResult.Yes) return;
        }

        var cts = new CancellationTokenSource();
        _downloadCts = cts;

        SetDownloadingUi(true);

        // Progress<T> is created on the WinForms UI thread and captures the SynchronizationContext.
        // Therefore callbacks execute directly on the UI thread without needing BeginInvoke.
        var progress = new Progress<(long BytesDownloaded, long TotalBytes, double Percent, double SpeedMbPerSec)>(p =>
        {
            if (!CanUpdateUi) return;
            UpdateProgress(p.BytesDownloaded, p.TotalBytes, p.Percent, p.SpeedMbPerSec);
        });

        try
        {
            _downloadTask = _modelManager.DownloadModelAsync(model, progress, cts.Token);
            await _downloadTask;

            if (!_closeRequested && CanUpdateUi)
            {
                RefreshModelList();
                MessageBox.Show(
                    this,
                    $"Model '{model.Name}' successfully downloaded and ready for use.",
                    "Download Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        catch (OperationCanceledException)
        {
            if (!_closeRequested && CanUpdateUi)
            {
                MessageBox.Show(
                    this,
                    "Model download was cancelled.",
                    "Cancelled",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            if (!_closeRequested && CanUpdateUi)
            {
                MessageBox.Show(
                    this,
                    $"Failed to download model: {ex.Message}",
                    "Download Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        finally
        {
            if (ReferenceEquals(_downloadCts, cts))
            {
                _downloadCts = null;
            }

            _downloadTask = null;
            cts.Dispose();

            if (CanUpdateUi)
            {
                SetDownloadingUi(false);
                RefreshModelList();
            }

            if (_closeRequested && !IsDisposed && !Disposing)
            {
                _allowClose = true;
                Close();
            }
        }
    }

    private void UpdateProgress(long bytes, long total, double percent, double speed)
    {
        if (!CanUpdateUi) return;

        _progressBar.Value = Math.Clamp((int)percent, 0, 100);
        double downloadedMb = bytes / (1024.0 * 1024.0);
        double totalMb = total / (1024.0 * 1024.0);
        _lblProgress.Text = $"Downloading: {downloadedMb:0.0} / {totalMb:0.0} MB ({percent:0.0}%) — {speed:0.0} MB/s";
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (!CanUpdateUi) return;

        var model = GetSelectedModel();
        if (model == null) return;

        if (_modelManager.IsModelInUse(model.Name))
        {
            MessageBox.Show(
                this,
                $"Model '{model.Name}' is currently in use by an active transcription session.",
                "Model In Use",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            this,
            $"Are you sure you want to delete the '{model.Name}' model file ({model.SizeFormatted})?",
            "Confirm Delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm == DialogResult.Yes)
        {
            try
            {
                _modelManager.DeleteModel(model);
                RefreshModelList();
            }
            catch (Exception ex)
            {
                if (CanUpdateUi)
                {
                    MessageBox.Show(
                        this,
                        $"Failed to delete model: {ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (!IsDisposed && !Disposing)
        {
            RefreshModelList();
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // If an active download task is in progress, delay close until it stops cleanly
        if (!_allowClose && _downloadTask is { IsCompleted: false })
        {
            e.Cancel = true;
            _closeRequested = true;

            RequestDownloadCancellation();

            if (CanUpdateUi)
            {
                _btnClose.Enabled = false;
                _btnDownload.Enabled = false;
                _btnDelete.Enabled = false;
                _lblProgress.Text = "Cancelling download...";
            }

            return;
        }

        base.OnFormClosing(e);
    }
}
