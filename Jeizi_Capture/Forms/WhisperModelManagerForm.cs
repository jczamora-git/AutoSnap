using AutoSnap.Transcription;

namespace AutoSnap.Forms;

public class WhisperModelManagerForm : Form
{
    private readonly WhisperModelManager _modelManager;
    private readonly ListView _lvModels;
    private readonly Button _btnDownload;
    private readonly Button _btnDelete;
    private readonly Button _btnClose;
    private readonly ProgressBar _progressBar;
    private readonly Label _lblProgress;
    private CancellationTokenSource? _downloadCts;

    public WhisperModelManagerForm(WhisperModelManager modelManager)
    {
        _modelManager = modelManager ?? throw new ArgumentNullException(nameof(modelManager));

        Text = "Manage Whisper Transcription Models";
        Size = new Size(680, 440);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        BackColor = Color.FromArgb(248, 249, 250);

        var lblHeader = new Label
        {
            Text = "Whisper Speech Recognition Models",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = Color.FromArgb(33, 37, 41),
            Location = new Point(20, 16),
            AutoSize = true
        };

        var lblDesc = new Label
        {
            Text = "Download local Whisper AI models for offline speech-to-text. Models are saved in %LocalAppData%\\AutoSnap\\Models\\Whisper.",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = Color.FromArgb(108, 117, 125),
            Location = new Point(20, 42),
            Size = new Size(620, 32)
        };

        _lvModels = new ListView
        {
            Location = new Point(20, 80),
            Size = new Size(624, 210),
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            MultiSelect = false,
            HideSelection = false
        };

        _lvModels.Columns.Add("Model", 100);
        _lvModels.Columns.Add("Size", 80);
        _lvModels.Columns.Add("Status", 110);
        _lvModels.Columns.Add("Description", 310);

        _lvModels.SelectedIndexChanged += (s, e) => UpdateButtons();

        _progressBar = new ProgressBar
        {
            Location = new Point(20, 305),
            Size = new Size(624, 18),
            Visible = false
        };

        _lblProgress = new Label
        {
            Location = new Point(20, 328),
            Size = new Size(624, 20),
            Text = "",
            ForeColor = Color.FromArgb(73, 80, 87),
            Visible = false
        };

        _btnDownload = new Button
        {
            Text = "Download Model",
            Location = new Point(20, 355),
            Size = new Size(130, 32),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Enabled = false
        };
        _btnDownload.FlatAppearance.BorderSize = 0;
        _btnDownload.Click += BtnDownload_Click;

        _btnDelete = new Button
        {
            Text = "Delete Model",
            Location = new Point(160, 355),
            Size = new Size(110, 32),
            BackColor = Color.FromArgb(248, 215, 218),
            ForeColor = Color.FromArgb(114, 28, 36),
            FlatStyle = FlatStyle.Flat,
            Enabled = false
        };
        _btnDelete.FlatAppearance.BorderColor = Color.FromArgb(245, 198, 203);
        _btnDelete.Click += BtnDelete_Click;

        _btnClose = new Button
        {
            Text = "Close",
            Location = new Point(534, 355),
            Size = new Size(110, 32),
            BackColor = Color.FromArgb(233, 236, 239),
            ForeColor = Color.FromArgb(33, 37, 41),
            FlatStyle = FlatStyle.Flat,
            DialogResult = DialogResult.OK
        };
        _btnClose.FlatAppearance.BorderSize = 0;

        Controls.Add(lblHeader);
        Controls.Add(lblDesc);
        Controls.Add(_lvModels);
        Controls.Add(_progressBar);
        Controls.Add(_lblProgress);
        Controls.Add(_btnDownload);
        Controls.Add(_btnDelete);
        Controls.Add(_btnClose);

        RefreshModelList();
    }

    public void RefreshModelList()
    {
        _lvModels.Items.Clear();

        foreach (var model in WhisperModelManager.AvailableModels)
        {
            bool isInstalled = _modelManager.IsModelInstalled(model.Name);
            var item = new ListViewItem(model.Name + (model.IsRecommended ? " (Recommended)" : ""));
            item.SubItems.Add(model.SizeFormatted);
            item.SubItems.Add(isInstalled ? "Ready (Installed)" : "Not Installed");
            item.SubItems.Add(model.Description);
            item.Tag = model;

            if (isInstalled)
            {
                item.ForeColor = Color.FromArgb(21, 87, 36);
            }

            _lvModels.Items.Add(item);
        }

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        if (_lvModels.SelectedItems.Count == 0)
        {
            _btnDownload.Enabled = false;
            _btnDelete.Enabled = false;
            return;
        }

        var model = (WhisperModelInfo)_lvModels.SelectedItems[0].Tag!;
        bool isInstalled = _modelManager.IsModelInstalled(model.Name);

        _btnDownload.Enabled = !isInstalled;
        _btnDelete.Enabled = isInstalled;
    }

    private async void BtnDownload_Click(object? sender, EventArgs e)
    {
        if (_lvModels.SelectedItems.Count == 0) return;
        var model = (WhisperModelInfo)_lvModels.SelectedItems[0].Tag!;

        if (_downloadCts != null)
        {
            // Cancel active download
            _downloadCts.Cancel();
            _btnDownload.Text = "Download Model";
            return;
        }

        _downloadCts = new CancellationTokenSource();
        _btnDownload.Text = "Cancel";
        _btnDelete.Enabled = false;
        _btnClose.Enabled = false;
        _lvModels.Enabled = false;
        _progressBar.Visible = true;
        _lblProgress.Visible = true;
        _progressBar.Value = 0;

        var progress = new Progress<(long BytesDownloaded, long TotalBytes, double Percent, double SpeedMbPerSec)>(p =>
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateProgress(p.BytesDownloaded, p.TotalBytes, p.Percent, p.SpeedMbPerSec)));
            }
            else
            {
                UpdateProgress(p.BytesDownloaded, p.TotalBytes, p.Percent, p.SpeedMbPerSec);
            }
        });

        try
        {
            await _modelManager.DownloadModelAsync(model, progress, _downloadCts.Token);
            MessageBox.Show(this, $"Model '{model.Name}' successfully downloaded and ready for use.", "Download Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (OperationCanceledException)
        {
            MessageBox.Show(this, "Model download was cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to download model: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _downloadCts?.Dispose();
            _downloadCts = null;
            _btnDownload.Text = "Download Model";
            _btnClose.Enabled = true;
            _lvModels.Enabled = true;
            _progressBar.Visible = false;
            _lblProgress.Visible = false;
            RefreshModelList();
        }
    }

    private void UpdateProgress(long bytes, long total, double percent, double speed)
    {
        _progressBar.Value = Math.Clamp((int)percent, 0, 100);
        double downloadedMb = bytes / (1024.0 * 1024.0);
        double totalMb = total / (1024.0 * 1024.0);
        _lblProgress.Text = $"Downloading: {downloadedMb:0.0} / {totalMb:0.0} MB ({percent:0.0}%) — {speed:0.0} MB/s";
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (_lvModels.SelectedItems.Count == 0) return;
        var model = (WhisperModelInfo)_lvModels.SelectedItems[0].Tag!;

        if (_modelManager.IsModelInUse(model.Name))
        {
            MessageBox.Show(this, $"Model '{model.Name}' is currently in use by an active transcription session.", "Model In Use", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show(this, $"Failed to delete model: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
