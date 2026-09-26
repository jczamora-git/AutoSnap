namespace AutoSnap.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Panel pnlHeader;
    private Label lblAppTitle;
    private Label lblSubtitle;
    private Label lblStatusBadge;
    private Button btnSettings;
    private Panel pnlMain;
    private TableLayoutPanel tblLayout;
    private Panel pnlSourceCard;
    private GroupBox grpSource;
    private RadioButton rbSourceChrome;
    private RadioButton rbSourceVideo;
    private RadioButton rbSourceMonitor;
    private RadioButton rbSourceWindow;
    private Panel pnlSelectedSource;
    private Label lblSelectedSource;
    private Label lblSelectedSubtext;
    private Label lblChromeStatusBadge;
    private Label lblSelectedPrefix;
    private Button btnOpenChrome;
    private Button btnStopChrome;
    private Button btnSelectSource;
    private Panel pnlPreviewCard;
    private Label lblPreviewHeader;
    private Panel pnlPreviewWrapper;
    private PictureBox picPreview;
    private Label lblPreviewStatus;
    private Panel pnlControlsCard;
    private GroupBox grpInterval;
    private ComboBox cmbInterval;
    private Panel pnlCustomInterval;
    private NumericUpDown numCustomInterval;
    private ComboBox cmbCustomUnit;
    private TableLayoutPanel tblStats;
    private Panel pnlStatCountdown;
    private Label lblCountdownTitle;
    private Label lblCountdownVal;
    private Panel pnlStatCount;
    private Label lblCapturedCountTitle;
    private Label lblCapturedCountVal;
    private Panel pnlStatDuration;
    private Label lblDurationTitle;
    private Label lblDurationVal;
    private Panel pnlOutputInfo;
    private Label lblOutputPrefix;
    private Label lblOutputDir;
    private Button btnOpenFolder;
    private Label lblLastSaved;
    private FlowLayoutPanel flowButtons;
    private Button btnTakeSnapshot;
    private Button btnStart;
    private Button btnPause;
    private Button btnStop;
    private NotifyIcon notifyIcon;
    private ContextMenuStrip trayMenu;
    private ToolStripMenuItem trayMenuOpen;
    private ToolStripSeparator trayMenuSep1;
    private ToolStripMenuItem trayMenuSnapshot;
    private ToolStripMenuItem trayMenuPauseResume;
    private ToolStripMenuItem trayMenuStop;
    private ToolStripSeparator trayMenuSep2;
    private ToolStripMenuItem trayMenuExit;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        pnlHeader = new Panel();
        btnSettings = new Button();
        lblStatusBadge = new Label();
        lblSubtitle = new Label();
        lblAppTitle = new Label();
        pnlMain = new Panel();
        tblLayout = new TableLayoutPanel();
        pnlSourceCard = new Panel();
        pnlPreviewCard = new Panel();
        lblPreviewHeader = new Label();
        pnlPreviewWrapper = new Panel();
        lblPreviewStatus = new Label();
        picPreview = new PictureBox();
        pnlSelectedSource = new Panel();
        btnSelectSource = new Button();
        btnOpenChrome = new Button();
        btnStopChrome = new Button();
        lblSelectedSource = new Label();
        lblSelectedSubtext = new Label();
        lblChromeStatusBadge = new Label();
        lblSelectedPrefix = new Label();
        grpSource = new GroupBox();
        rbSourceVideo = new RadioButton();
        rbSourceChrome = new RadioButton();
        rbSourceMonitor = new RadioButton();
        rbSourceWindow = new RadioButton();
        pnlControlsCard = new Panel();
        flowButtons = new FlowLayoutPanel();
        btnTakeSnapshot = new Button();
        btnStart = new Button();
        btnPause = new Button();
        btnStop = new Button();
        lblLastSaved = new Label();
        pnlOutputInfo = new Panel();
        btnOpenFolder = new Button();
        lblOutputDir = new Label();
        lblOutputPrefix = new Label();
        tblStats = new TableLayoutPanel();
        pnlStatDuration = new Panel();
        lblDurationVal = new Label();
        lblDurationTitle = new Label();
        pnlStatCount = new Panel();
        lblCapturedCountVal = new Label();
        lblCapturedCountTitle = new Label();
        pnlStatCountdown = new Panel();
        lblCountdownVal = new Label();
        lblCountdownTitle = new Label();
        grpInterval = new GroupBox();
        pnlCustomInterval = new Panel();
        cmbCustomUnit = new ComboBox();
        numCustomInterval = new NumericUpDown();
        cmbInterval = new ComboBox();
        notifyIcon = new NotifyIcon(components);
        trayMenu = new ContextMenuStrip(components);
        trayMenuOpen = new ToolStripMenuItem();
        trayMenuSep1 = new ToolStripSeparator();
        trayMenuSnapshot = new ToolStripMenuItem();
        trayMenuPauseResume = new ToolStripMenuItem();
        trayMenuStop = new ToolStripMenuItem();
        trayMenuSep2 = new ToolStripSeparator();
        trayMenuExit = new ToolStripMenuItem();

        pnlHeader.SuspendLayout();
        pnlMain.SuspendLayout();
        tblLayout.SuspendLayout();
        pnlSourceCard.SuspendLayout();
        pnlPreviewCard.SuspendLayout();
        pnlPreviewWrapper.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
        pnlSelectedSource.SuspendLayout();
        grpSource.SuspendLayout();
        pnlControlsCard.SuspendLayout();
        flowButtons.SuspendLayout();
        pnlOutputInfo.SuspendLayout();
        tblStats.SuspendLayout();
        pnlStatDuration.SuspendLayout();
        pnlStatCount.SuspendLayout();
        pnlStatCountdown.SuspendLayout();
        grpInterval.SuspendLayout();
        pnlCustomInterval.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numCustomInterval).BeginInit();
        trayMenu.SuspendLayout();
        SuspendLayout();

        // 
        // pnlHeader
        // 
        pnlHeader.BackColor = Color.FromArgb(248, 249, 250);
        pnlHeader.Controls.Add(btnSettings);
        pnlHeader.Controls.Add(lblStatusBadge);
        pnlHeader.Controls.Add(lblSubtitle);
        pnlHeader.Controls.Add(lblAppTitle);
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Location = new Point(0, 0);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Padding = new Padding(16, 12, 16, 12);
        pnlHeader.Size = new Size(884, 65);
        pnlHeader.TabIndex = 0;

        // 
        // btnSettings
        // 
        btnSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSettings.BackColor = Color.White;
        btnSettings.FlatStyle = FlatStyle.Flat;
        btnSettings.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        btnSettings.ForeColor = Color.FromArgb(60, 60, 60);
        btnSettings.Location = new Point(780, 16);
        btnSettings.Name = "btnSettings";
        btnSettings.Size = new Size(88, 32);
        btnSettings.TabIndex = 3;
        btnSettings.Text = "⚙ Settings";
        btnSettings.UseVisualStyleBackColor = false;
        btnSettings.Click += btnSettings_Click;

        // 
        // lblStatusBadge
        // 
        lblStatusBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblStatusBadge.BackColor = Color.FromArgb(230, 230, 230);
        lblStatusBadge.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        lblStatusBadge.ForeColor = Color.FromArgb(80, 80, 80);
        lblStatusBadge.Location = new Point(660, 16);
        lblStatusBadge.Name = "lblStatusBadge";
        lblStatusBadge.Size = new Size(110, 32);
        lblStatusBadge.TabIndex = 2;
        lblStatusBadge.Text = "Idle";
        lblStatusBadge.TextAlign = ContentAlignment.MiddleCenter;

        // 
        // lblSubtitle
        // 
        lblSubtitle.AutoSize = true;
        lblSubtitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
        lblSubtitle.ForeColor = Color.FromArgb(110, 110, 110);
        lblSubtitle.Location = new Point(16, 36);
        lblSubtitle.Name = "lblSubtitle";
        lblSubtitle.Size = new Size(169, 15);
        lblSubtitle.TabIndex = 1;
        lblSubtitle.Text = "Automatic Screenshot Capture";

        // 
        // lblAppTitle
        // 
        lblAppTitle.AutoSize = true;
        lblAppTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
        lblAppTitle.ForeColor = Color.FromArgb(30, 30, 30);
        lblAppTitle.Location = new Point(14, 10);
        lblAppTitle.Name = "lblAppTitle";
        lblAppTitle.Size = new Size(100, 25);
        lblAppTitle.TabIndex = 0;
        lblAppTitle.Text = "AutoSnap";

        // 
        // pnlMain
        // 
        pnlMain.Controls.Add(tblLayout);
        pnlMain.Dock = DockStyle.Fill;
        pnlMain.Location = new Point(0, 65);
        pnlMain.Name = "pnlMain";
        pnlMain.Padding = new Padding(16);
        pnlMain.Size = new Size(884, 596);
        pnlMain.TabIndex = 1;

        // 
        // tblLayout
        // 
        tblLayout.ColumnCount = 2;
        tblLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
        tblLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
        tblLayout.Controls.Add(pnlSourceCard, 0, 0);
        tblLayout.Controls.Add(pnlControlsCard, 1, 0);
        tblLayout.Dock = DockStyle.Fill;
        tblLayout.Location = new Point(16, 16);
        tblLayout.Name = "tblLayout";
        tblLayout.RowCount = 1;
        tblLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tblLayout.Size = new Size(852, 564);
        tblLayout.TabIndex = 0;

        // 
        // pnlSourceCard
        // 
        pnlSourceCard.Controls.Add(pnlPreviewCard);
        pnlSourceCard.Controls.Add(pnlSelectedSource);
        pnlSourceCard.Controls.Add(grpSource);
        pnlSourceCard.Dock = DockStyle.Fill;
        pnlSourceCard.Location = new Point(0, 0);
        pnlSourceCard.Margin = new Padding(0, 0, 8, 0);
        pnlSourceCard.Name = "pnlSourceCard";
        pnlSourceCard.Size = new Size(460, 564);
        pnlSourceCard.TabIndex = 0;

        // 
        // pnlPreviewCard
        // 
        pnlPreviewCard.Controls.Add(lblPreviewHeader);
        pnlPreviewCard.Controls.Add(pnlPreviewWrapper);
        pnlPreviewCard.Dock = DockStyle.Fill;
        pnlPreviewCard.Location = new Point(0, 114);
        pnlPreviewCard.Name = "pnlPreviewCard";
        pnlPreviewCard.Padding = new Padding(0, 8, 0, 0);
        pnlPreviewCard.Size = new Size(460, 450);
        pnlPreviewCard.TabIndex = 2;

        // 
        // lblPreviewHeader
        // 
        lblPreviewHeader.AutoSize = true;
        lblPreviewHeader.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
        lblPreviewHeader.ForeColor = Color.FromArgb(70, 70, 70);
        lblPreviewHeader.Location = new Point(0, 8);
        lblPreviewHeader.Name = "lblPreviewHeader";
        lblPreviewHeader.Size = new Size(74, 15);
        lblPreviewHeader.TabIndex = 0;
        lblPreviewHeader.Text = "Live Preview";

        // 
        // pnlPreviewWrapper
        // 
        pnlPreviewWrapper.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        pnlPreviewWrapper.BackColor = Color.FromArgb(30, 30, 35);
        pnlPreviewWrapper.Controls.Add(lblPreviewStatus);
        pnlPreviewWrapper.Controls.Add(picPreview);
        pnlPreviewWrapper.Location = new Point(0, 28);
        pnlPreviewWrapper.Name = "pnlPreviewWrapper";
        pnlPreviewWrapper.Size = new Size(460, 422);
        pnlPreviewWrapper.TabIndex = 1;

        // 
        // lblPreviewStatus
        // 
        lblPreviewStatus.Dock = DockStyle.Fill;
        lblPreviewStatus.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        lblPreviewStatus.ForeColor = Color.FromArgb(160, 160, 170);
        lblPreviewStatus.Location = new Point(0, 0);
        lblPreviewStatus.Name = "lblPreviewStatus";
        lblPreviewStatus.Size = new Size(460, 422);
        lblPreviewStatus.TabIndex = 1;
        lblPreviewStatus.Text = "No source selected";
        lblPreviewStatus.TextAlign = ContentAlignment.MiddleCenter;

        // 
        // picPreview
        // 
        picPreview.Dock = DockStyle.Fill;
        picPreview.Location = new Point(0, 0);
        picPreview.Name = "picPreview";
        picPreview.Size = new Size(460, 422);
        picPreview.SizeMode = PictureBoxSizeMode.Zoom;
        picPreview.TabIndex = 0;
        picPreview.TabStop = false;

        // 
        // pnlSelectedSource
        // 
        pnlSelectedSource.BackColor = Color.FromArgb(245, 247, 250);
        pnlSelectedSource.Controls.Add(btnStopChrome);
        pnlSelectedSource.Controls.Add(btnOpenChrome);
        pnlSelectedSource.Controls.Add(btnSelectSource);
        pnlSelectedSource.Controls.Add(lblChromeStatusBadge);
        pnlSelectedSource.Controls.Add(lblSelectedSubtext);
        pnlSelectedSource.Controls.Add(lblSelectedSource);
        pnlSelectedSource.Controls.Add(lblSelectedPrefix);
        pnlSelectedSource.Dock = DockStyle.Top;
        pnlSelectedSource.Location = new Point(0, 64);
        pnlSelectedSource.Name = "pnlSelectedSource";
        pnlSelectedSource.Padding = new Padding(10, 6, 10, 6);
        pnlSelectedSource.Size = new Size(460, 64);
        pnlSelectedSource.TabIndex = 1;

        // 
        // btnSelectSource
        // 
        btnSelectSource.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSelectSource.BackColor = Color.FromArgb(0, 120, 215);
        btnSelectSource.FlatStyle = FlatStyle.Flat;
        btnSelectSource.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold, GraphicsUnit.Point);
        btnSelectSource.ForeColor = Color.White;
        btnSelectSource.Location = new Point(335, 6);
        btnSelectSource.Name = "btnSelectSource";
        btnSelectSource.Size = new Size(115, 25);
        btnSelectSource.TabIndex = 4;
        btnSelectSource.Text = "Change...";
        btnSelectSource.UseVisualStyleBackColor = false;
        btnSelectSource.Click += btnSelectSource_Click;

        // 
        // btnOpenChrome
        // 
        btnOpenChrome.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnOpenChrome.BackColor = Color.White;
        btnOpenChrome.FlatStyle = FlatStyle.Flat;
        btnOpenChrome.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point);
        btnOpenChrome.ForeColor = Color.FromArgb(0, 120, 215);
        btnOpenChrome.Location = new Point(335, 34);
        btnOpenChrome.Name = "btnOpenChrome";
        btnOpenChrome.Size = new Size(115, 23);
        btnOpenChrome.TabIndex = 5;
        btnOpenChrome.Text = "🌐 Capture Page";
        btnOpenChrome.UseVisualStyleBackColor = false;
        btnOpenChrome.Visible = false;
        btnOpenChrome.Click += btnOpenChrome_Click;

        // 
        // btnStopChrome
        // 
        btnStopChrome.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnStopChrome.BackColor = Color.FromArgb(248, 215, 218);
        btnStopChrome.FlatStyle = FlatStyle.Flat;
        btnStopChrome.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point);
        btnStopChrome.ForeColor = Color.FromArgb(114, 28, 36);
        btnStopChrome.Location = new Point(335, 34);
        btnStopChrome.Name = "btnStopChrome";
        btnStopChrome.Size = new Size(115, 23);
        btnStopChrome.TabIndex = 6;
        btnStopChrome.Text = "⏹ Stop Sharing";
        btnStopChrome.UseVisualStyleBackColor = false;
        btnStopChrome.Visible = false;
        btnStopChrome.Click += btnStopChrome_Click;

        // 
        // lblSelectedSource
        // 
        lblSelectedSource.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblSelectedSource.AutoEllipsis = true;
        lblSelectedSource.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
        lblSelectedSource.ForeColor = Color.FromArgb(30, 30, 30);
        lblSelectedSource.Location = new Point(62, 9);
        lblSelectedSource.Name = "lblSelectedSource";
        lblSelectedSource.Size = new Size(185, 18);
        lblSelectedSource.TabIndex = 1;
        lblSelectedSource.Text = "None";

        // 
        // lblChromeStatusBadge
        // 
        lblChromeStatusBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblChromeStatusBadge.AutoSize = true;
        lblChromeStatusBadge.Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold, GraphicsUnit.Point);
        lblChromeStatusBadge.ForeColor = Color.SeaGreen;
        lblChromeStatusBadge.Location = new Point(252, 11);
        lblChromeStatusBadge.Name = "lblChromeStatusBadge";
        lblChromeStatusBadge.Size = new Size(72, 13);
        lblChromeStatusBadge.TabIndex = 2;
        lblChromeStatusBadge.Text = "● Connected";
        lblChromeStatusBadge.Visible = false;

        // 
        // lblSelectedSubtext
        // 
        lblSelectedSubtext.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblSelectedSubtext.AutoEllipsis = true;
        lblSelectedSubtext.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point);
        lblSelectedSubtext.ForeColor = Color.FromArgb(120, 120, 120);
        lblSelectedSubtext.Location = new Point(62, 34);
        lblSelectedSubtext.Name = "lblSelectedSubtext";
        lblSelectedSubtext.Size = new Size(275, 18);
        lblSelectedSubtext.TabIndex = 3;
        lblSelectedSubtext.Text = "";

        // 
        // lblSelectedPrefix
        // 
        lblSelectedPrefix.AutoSize = true;
        lblSelectedPrefix.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        lblSelectedPrefix.ForeColor = Color.FromArgb(100, 100, 100);
        lblSelectedPrefix.Location = new Point(8, 9);
        lblSelectedPrefix.Name = "lblSelectedPrefix";
        lblSelectedPrefix.Size = new Size(54, 15);
        lblSelectedPrefix.TabIndex = 0;
        lblSelectedPrefix.Text = "Selected:";

        // 
        // grpSource
        // 
        grpSource.Controls.Add(rbSourceVideo);
        grpSource.Controls.Add(rbSourceChrome);
        grpSource.Controls.Add(rbSourceMonitor);
        grpSource.Controls.Add(rbSourceWindow);
        grpSource.Dock = DockStyle.Top;
        grpSource.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
        grpSource.Location = new Point(0, 0);
        grpSource.Name = "grpSource";
        grpSource.Padding = new Padding(8);
        grpSource.Size = new Size(460, 64);
        grpSource.TabIndex = 0;
        grpSource.TabStop = false;
        grpSource.Text = "Capture Source";

        // 
        // rbSourceWindow
        // 
        rbSourceWindow.AutoSize = true;
        rbSourceWindow.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
        rbSourceWindow.Location = new Point(8, 26);
        rbSourceWindow.Name = "rbSourceWindow";
        rbSourceWindow.Size = new Size(86, 17);
        rbSourceWindow.TabIndex = 0;
        rbSourceWindow.Text = "Application";
        rbSourceWindow.UseVisualStyleBackColor = true;
        rbSourceWindow.CheckedChanged += rbSourceType_CheckedChanged;

        // 
        // rbSourceMonitor
        // 
        rbSourceMonitor.AutoSize = true;
        rbSourceMonitor.Checked = true;
        rbSourceMonitor.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
        rbSourceMonitor.Location = new Point(100, 26);
        rbSourceMonitor.Name = "rbSourceMonitor";
        rbSourceMonitor.Size = new Size(62, 17);
        rbSourceMonitor.TabIndex = 1;
        rbSourceMonitor.TabStop = true;
        rbSourceMonitor.Text = "Display";
        rbSourceMonitor.UseVisualStyleBackColor = true;
        rbSourceMonitor.CheckedChanged += rbSourceType_CheckedChanged;

        // 
        // rbSourceChrome
        // 
        rbSourceChrome.AutoSize = true;
        rbSourceChrome.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
        rbSourceChrome.Location = new Point(172, 26);
        rbSourceChrome.Name = "rbSourceChrome";
        rbSourceChrome.Size = new Size(88, 17);
        rbSourceChrome.TabIndex = 2;
        rbSourceChrome.Text = "Chrome Tab";
        rbSourceChrome.UseVisualStyleBackColor = true;
        rbSourceChrome.CheckedChanged += rbSourceType_CheckedChanged;

        // 
        // rbSourceVideo
        // 
        rbSourceVideo.AutoSize = true;
        rbSourceVideo.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
        rbSourceVideo.Location = new Point(270, 26);
        rbSourceVideo.Name = "rbSourceVideo";
        rbSourceVideo.Size = new Size(135, 17);
        rbSourceVideo.TabIndex = 3;
        rbSourceVideo.Text = "Local Video (Phase 2)";
        rbSourceVideo.UseVisualStyleBackColor = true;
        rbSourceVideo.CheckedChanged += rbSourceType_CheckedChanged;

        // 
        // pnlControlsCard
        // 
        pnlControlsCard.Controls.Add(flowButtons);
        pnlControlsCard.Controls.Add(lblLastSaved);
        pnlControlsCard.Controls.Add(pnlOutputInfo);
        pnlControlsCard.Controls.Add(tblStats);
        pnlControlsCard.Controls.Add(grpInterval);
        pnlControlsCard.Dock = DockStyle.Fill;
        pnlControlsCard.Location = new Point(476, 0);
        pnlControlsCard.Margin = new Padding(8, 0, 0, 0);
        pnlControlsCard.Name = "pnlControlsCard";
        pnlControlsCard.Size = new Size(376, 564);
        pnlControlsCard.TabIndex = 1;

        // 
        // flowButtons
        // 
        flowButtons.Controls.Add(btnTakeSnapshot);
        flowButtons.Controls.Add(btnStart);
        flowButtons.Controls.Add(btnPause);
        flowButtons.Controls.Add(btnStop);
        flowButtons.Dock = DockStyle.Top;
        flowButtons.Location = new Point(0, 314);
        flowButtons.Name = "flowButtons";
        flowButtons.Padding = new Padding(0, 8, 0, 0);
        flowButtons.Size = new Size(376, 120);
        flowButtons.TabIndex = 4;

        // 
        // btnTakeSnapshot
        // 
        btnTakeSnapshot.BackColor = Color.White;
        btnTakeSnapshot.FlatStyle = FlatStyle.Flat;
        btnTakeSnapshot.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
        btnTakeSnapshot.ForeColor = Color.FromArgb(0, 120, 215);
        btnTakeSnapshot.Location = new Point(0, 8);
        btnTakeSnapshot.Margin = new Padding(0, 0, 8, 8);
        btnTakeSnapshot.Name = "btnTakeSnapshot";
        btnTakeSnapshot.Size = new Size(370, 38);
        btnTakeSnapshot.TabIndex = 0;
        btnTakeSnapshot.Text = "📸  Take Snapshot Now";
        btnTakeSnapshot.UseVisualStyleBackColor = false;
        btnTakeSnapshot.Click += btnTakeSnapshot_Click;

        // 
        // btnStart
        // 
        btnStart.BackColor = Color.FromArgb(40, 167, 69);
        btnStart.FlatStyle = FlatStyle.Flat;
        btnStart.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
        btnStart.ForeColor = Color.White;
        btnStart.Location = new Point(0, 54);
        btnStart.Margin = new Padding(0, 0, 6, 8);
        btnStart.Name = "btnStart";
        btnStart.Size = new Size(130, 42);
        btnStart.TabIndex = 1;
        btnStart.Text = "▶  Start Capture";
        btnStart.UseVisualStyleBackColor = false;
        btnStart.Click += btnStart_Click;

        // 
        // btnPause
        // 
        btnPause.BackColor = Color.White;
        btnPause.Enabled = false;
        btnPause.FlatStyle = FlatStyle.Flat;
        btnPause.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
        btnPause.ForeColor = Color.FromArgb(50, 50, 50);
        btnPause.Location = new Point(136, 54);
        btnPause.Margin = new Padding(0, 0, 6, 8);
        btnPause.Name = "btnPause";
        btnPause.Size = new Size(110, 42);
        btnPause.TabIndex = 2;
        btnPause.Text = "❚❚  Pause";
        btnPause.UseVisualStyleBackColor = false;
        btnPause.Click += btnPause_Click;

        // 
        // btnStop
        // 
        btnStop.BackColor = Color.White;
        btnStop.Enabled = false;
        btnStop.FlatStyle = FlatStyle.Flat;
        btnStop.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
        btnStop.ForeColor = Color.FromArgb(220, 53, 69);
        btnStop.Location = new Point(252, 54);
        btnStop.Margin = new Padding(0, 0, 0, 8);
        btnStop.Name = "btnStop";
        btnStop.Size = new Size(118, 42);
        btnStop.TabIndex = 3;
        btnStop.Text = "■  Stop";
        btnStop.UseVisualStyleBackColor = false;
        btnStop.Click += btnStop_Click;

        // 
        // lblLastSaved
        // 
        lblLastSaved.AutoEllipsis = true;
        lblLastSaved.Dock = DockStyle.Top;
        lblLastSaved.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        lblLastSaved.ForeColor = Color.FromArgb(80, 80, 80);
        lblLastSaved.Location = new Point(0, 276);
        lblLastSaved.Name = "lblLastSaved";
        lblLastSaved.Padding = new Padding(2, 6, 2, 6);
        lblLastSaved.Size = new Size(376, 38);
        lblLastSaved.TabIndex = 3;
        lblLastSaved.Text = "Ready to capture";

        // 
        // pnlOutputInfo
        // 
        pnlOutputInfo.BackColor = Color.FromArgb(248, 249, 250);
        pnlOutputInfo.Controls.Add(btnOpenFolder);
        pnlOutputInfo.Controls.Add(lblOutputDir);
        pnlOutputInfo.Controls.Add(lblOutputPrefix);
        pnlOutputInfo.Dock = DockStyle.Top;
        pnlOutputInfo.Location = new Point(0, 222);
        pnlOutputInfo.Name = "pnlOutputInfo";
        pnlOutputInfo.Padding = new Padding(10, 8, 10, 8);
        pnlOutputInfo.Size = new Size(376, 54);
        pnlOutputInfo.TabIndex = 2;

        // 
        // btnOpenFolder
        // 
        btnOpenFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnOpenFolder.BackColor = Color.White;
        btnOpenFolder.FlatStyle = FlatStyle.Flat;
        btnOpenFolder.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
        btnOpenFolder.Location = new Point(286, 12);
        btnOpenFolder.Name = "btnOpenFolder";
        btnOpenFolder.Size = new Size(82, 30);
        btnOpenFolder.TabIndex = 2;
        btnOpenFolder.Text = "Open Folder";
        btnOpenFolder.UseVisualStyleBackColor = false;
        btnOpenFolder.Click += btnOpenFolder_Click;

        // 
        // lblOutputDir
        // 
        lblOutputDir.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblOutputDir.AutoEllipsis = true;
        lblOutputDir.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
        lblOutputDir.ForeColor = Color.FromArgb(40, 40, 40);
        lblOutputDir.Location = new Point(10, 26);
        lblOutputDir.Name = "lblOutputDir";
        lblOutputDir.Size = new Size(270, 18);
        lblOutputDir.TabIndex = 1;
        lblOutputDir.Text = "C:\\Users\\...\\Pictures\\AutoSnap";

        // 
        // lblOutputPrefix
        // 
        lblOutputPrefix.AutoSize = true;
        lblOutputPrefix.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold, GraphicsUnit.Point);
        lblOutputPrefix.ForeColor = Color.FromArgb(100, 100, 100);
        lblOutputPrefix.Location = new Point(8, 8);
        lblOutputPrefix.Name = "lblOutputPrefix";
        lblOutputPrefix.Size = new Size(101, 15);
        lblOutputPrefix.TabIndex = 0;
        lblOutputPrefix.Text = "Output Directory:";

        // 
        // tblStats
        // 
        tblStats.ColumnCount = 3;
        tblStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        tblStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        tblStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        tblStats.Controls.Add(pnlStatDuration, 2, 0);
        tblStats.Controls.Add(pnlStatCount, 1, 0);
        tblStats.Controls.Add(pnlStatCountdown, 0, 0);
        tblStats.Dock = DockStyle.Top;
        tblStats.Location = new Point(0, 126);
        tblStats.Name = "tblStats";
        tblStats.RowCount = 1;
        tblStats.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tblStats.Size = new Size(376, 96);
        tblStats.TabIndex = 1;

        // 
        // pnlStatDuration
        // 
        pnlStatDuration.BackColor = Color.FromArgb(245, 247, 250);
        pnlStatDuration.Controls.Add(lblDurationVal);
        pnlStatDuration.Controls.Add(lblDurationTitle);
        pnlStatDuration.Dock = DockStyle.Fill;
        pnlStatDuration.Location = new Point(254, 3);
        pnlStatDuration.Margin = new Padding(4, 3, 0, 8);
        pnlStatDuration.Name = "pnlStatDuration";
        pnlStatDuration.Padding = new Padding(6);
        pnlStatDuration.Size = new Size(122, 85);
        pnlStatDuration.TabIndex = 2;

        // 
        // lblDurationVal
        // 
        lblDurationVal.Dock = DockStyle.Fill;
        lblDurationVal.Font = new Font("Segoe UI", 12.5F, FontStyle.Bold, GraphicsUnit.Point);
        lblDurationVal.ForeColor = Color.FromArgb(40, 40, 40);
        lblDurationVal.Location = new Point(6, 28);
        lblDurationVal.Name = "lblDurationVal";
        lblDurationVal.Size = new Size(110, 51);
        lblDurationVal.TabIndex = 1;
        lblDurationVal.Text = "00:00:00";
        lblDurationVal.TextAlign = ContentAlignment.MiddleCenter;

        // 
        // lblDurationTitle
        // 
        lblDurationTitle.Dock = DockStyle.Top;
        lblDurationTitle.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point);
        lblDurationTitle.ForeColor = Color.FromArgb(110, 110, 110);
        lblDurationTitle.Location = new Point(6, 6);
        lblDurationTitle.Name = "lblDurationTitle";
        lblDurationTitle.Size = new Size(110, 22);
        lblDurationTitle.TabIndex = 0;
        lblDurationTitle.Text = "Session Duration";
        lblDurationTitle.TextAlign = ContentAlignment.TopCenter;

        // 
        // pnlStatCount
        // 
        pnlStatCount.BackColor = Color.FromArgb(245, 247, 250);
        pnlStatCount.Controls.Add(lblCapturedCountVal);
        pnlStatCount.Controls.Add(lblCapturedCountTitle);
        pnlStatCount.Dock = DockStyle.Fill;
        pnlStatCount.Location = new Point(129, 3);
        pnlStatCount.Margin = new Padding(4, 3, 4, 8);
        pnlStatCount.Name = "pnlStatCount";
        pnlStatCount.Padding = new Padding(6);
        pnlStatCount.Size = new Size(117, 85);
        pnlStatCount.TabIndex = 1;

        // 
        // lblCapturedCountVal
        // 
        lblCapturedCountVal.Dock = DockStyle.Fill;
        lblCapturedCountVal.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
        lblCapturedCountVal.ForeColor = Color.FromArgb(0, 120, 215);
        lblCapturedCountVal.Location = new Point(6, 28);
        lblCapturedCountVal.Name = "lblCapturedCountVal";
        lblCapturedCountVal.Size = new Size(105, 51);
        lblCapturedCountVal.TabIndex = 1;
        lblCapturedCountVal.Text = "0";
        lblCapturedCountVal.TextAlign = ContentAlignment.MiddleCenter;

        // 
        // lblCapturedCountTitle
        // 
        lblCapturedCountTitle.Dock = DockStyle.Top;
        lblCapturedCountTitle.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point);
        lblCapturedCountTitle.ForeColor = Color.FromArgb(110, 110, 110);
        lblCapturedCountTitle.Location = new Point(6, 6);
        lblCapturedCountTitle.Name = "lblCapturedCountTitle";
        lblCapturedCountTitle.Size = new Size(105, 22);
        lblCapturedCountTitle.TabIndex = 0;
        lblCapturedCountTitle.Text = "Snapshots";
        lblCapturedCountTitle.TextAlign = ContentAlignment.TopCenter;

        // 
        // pnlStatCountdown
        // 
        pnlStatCountdown.BackColor = Color.FromArgb(245, 247, 250);
        pnlStatCountdown.Controls.Add(lblCountdownVal);
        pnlStatCountdown.Controls.Add(lblCountdownTitle);
        pnlStatCountdown.Dock = DockStyle.Fill;
        pnlStatCountdown.Location = new Point(0, 3);
        pnlStatCountdown.Margin = new Padding(0, 3, 4, 8);
        pnlStatCountdown.Name = "pnlStatCountdown";
        pnlStatCountdown.Padding = new Padding(6);
        pnlStatCountdown.Size = new Size(121, 85);
        pnlStatCountdown.TabIndex = 0;

        // 
        // lblCountdownVal
        // 
        lblCountdownVal.Dock = DockStyle.Fill;
        lblCountdownVal.Font = new Font("Segoe UI", 12.5F, FontStyle.Bold, GraphicsUnit.Point);
        lblCountdownVal.ForeColor = Color.FromArgb(40, 40, 40);
        lblCountdownVal.Location = new Point(6, 28);
        lblCountdownVal.Name = "lblCountdownVal";
        lblCountdownVal.Size = new Size(109, 51);
        lblCountdownVal.TabIndex = 1;
        lblCountdownVal.Text = "--:--:--";
        lblCountdownVal.TextAlign = ContentAlignment.MiddleCenter;

        // 
        // lblCountdownTitle
        // 
        lblCountdownTitle.Dock = DockStyle.Top;
        lblCountdownTitle.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point);
        lblCountdownTitle.ForeColor = Color.FromArgb(110, 110, 110);
        lblCountdownTitle.Location = new Point(6, 6);
        lblCountdownTitle.Name = "lblCountdownTitle";
        lblCountdownTitle.Size = new Size(109, 22);
        lblCountdownTitle.TabIndex = 0;
        lblCountdownTitle.Text = "Next Snapshot";
        lblCountdownTitle.TextAlign = ContentAlignment.TopCenter;

        // 
        // grpInterval
        // 
        grpInterval.Controls.Add(pnlCustomInterval);
        grpInterval.Controls.Add(cmbInterval);
        grpInterval.Dock = DockStyle.Top;
        grpInterval.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
        grpInterval.Location = new Point(0, 0);
        grpInterval.Name = "grpInterval";
        grpInterval.Padding = new Padding(10);
        grpInterval.Size = new Size(376, 126);
        grpInterval.TabIndex = 0;
        grpInterval.TabStop = false;
        grpInterval.Text = "Snapshot Interval";

        // 
        // pnlCustomInterval
        // 
        pnlCustomInterval.Controls.Add(cmbCustomUnit);
        pnlCustomInterval.Controls.Add(numCustomInterval);
        pnlCustomInterval.Dock = DockStyle.Top;
        pnlCustomInterval.Location = new Point(10, 62);
        pnlCustomInterval.Name = "pnlCustomInterval";
        pnlCustomInterval.Padding = new Padding(0, 8, 0, 0);
        pnlCustomInterval.Size = new Size(356, 48);
        pnlCustomInterval.TabIndex = 1;
        pnlCustomInterval.Visible = false;

        // 
        // cmbCustomUnit
        // 
        cmbCustomUnit.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbCustomUnit.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        cmbCustomUnit.FormattingEnabled = true;
        cmbCustomUnit.Items.AddRange(new object[] { "Seconds", "Minutes", "Hours" });
        cmbCustomUnit.Location = new Point(110, 10);
        cmbCustomUnit.Name = "cmbCustomUnit";
        cmbCustomUnit.Size = new Size(120, 23);
        cmbCustomUnit.TabIndex = 1;

        // 
        // numCustomInterval
        // 
        numCustomInterval.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        numCustomInterval.Location = new Point(4, 10);
        numCustomInterval.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
        numCustomInterval.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numCustomInterval.Name = "numCustomInterval";
        numCustomInterval.Size = new Size(95, 23);
        numCustomInterval.TabIndex = 0;
        numCustomInterval.Value = new decimal(new int[] { 5, 0, 0, 0 });

        // 
        // cmbInterval
        // 
        cmbInterval.Dock = DockStyle.Top;
        cmbInterval.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbInterval.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        cmbInterval.FormattingEnabled = true;
        cmbInterval.Items.AddRange(new object[] {
            "1 second",
            "5 seconds",
            "10 seconds",
            "30 seconds",
            "1 minute",
            "5 minutes",
            "10 minutes",
            "30 minutes",
            "1 hour",
            "Custom"
        });
        cmbInterval.Location = new Point(10, 26);
        cmbInterval.Name = "cmbInterval";
        cmbInterval.Size = new Size(356, 25);
        cmbInterval.TabIndex = 0;
        cmbInterval.SelectedIndexChanged += cmbInterval_SelectedIndexChanged;

        // 
        // notifyIcon
        // 
        notifyIcon.ContextMenuStrip = trayMenu;
        notifyIcon.Icon = SystemIcons.Application;
        notifyIcon.Text = "AutoSnap - Automatic Screenshot Capture";
        notifyIcon.Visible = true;
        notifyIcon.DoubleClick += notifyIcon_DoubleClick;

        // 
        // trayMenu
        // 
        trayMenu.Items.AddRange(new ToolStripItem[] {
            trayMenuOpen,
            trayMenuSep1,
            trayMenuSnapshot,
            trayMenuPauseResume,
            trayMenuStop,
            trayMenuSep2,
            trayMenuExit
        });
        trayMenu.Name = "trayMenu";
        trayMenu.Size = new Size(181, 148);

        // 
        // trayMenuOpen
        // 
        trayMenuOpen.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        trayMenuOpen.Name = "trayMenuOpen";
        trayMenuOpen.Size = new Size(180, 22);
        trayMenuOpen.Text = "Open AutoSnap";
        trayMenuOpen.Click += trayMenuOpen_Click;

        // 
        // trayMenuSep1
        // 
        trayMenuSep1.Name = "trayMenuSep1";
        trayMenuSep1.Size = new Size(177, 6);

        // 
        // trayMenuSnapshot
        // 
        trayMenuSnapshot.Name = "trayMenuSnapshot";
        trayMenuSnapshot.Size = new Size(180, 22);
        trayMenuSnapshot.Text = "Take Snapshot Now";
        trayMenuSnapshot.Click += trayMenuSnapshot_Click;

        // 
        // trayMenuPauseResume
        // 
        trayMenuPauseResume.Name = "trayMenuPauseResume";
        trayMenuPauseResume.Size = new Size(180, 22);
        trayMenuPauseResume.Text = "Pause Capture";
        trayMenuPauseResume.Click += trayMenuPauseResume_Click;

        // 
        // trayMenuStop
        // 
        trayMenuStop.Name = "trayMenuStop";
        trayMenuStop.Size = new Size(180, 22);
        trayMenuStop.Text = "Stop Capture";
        trayMenuStop.Click += trayMenuStop_Click;

        // 
        // trayMenuSep2
        // 
        trayMenuSep2.Name = "trayMenuSep2";
        trayMenuSep2.Size = new Size(177, 6);

        // 
        // trayMenuExit
        // 
        trayMenuExit.Name = "trayMenuExit";
        trayMenuExit.Size = new Size(180, 22);
        trayMenuExit.Text = "Exit";
        trayMenuExit.Click += trayMenuExit_Click;

        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        ClientSize = new Size(884, 661);
        Controls.Add(pnlMain);
        Controls.Add(pnlHeader);
        DoubleBuffered = true;
        Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        MinimumSize = new Size(820, 600);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "AutoSnap - Automatic Screenshot Capture";
        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        pnlMain.ResumeLayout(false);
        tblLayout.ResumeLayout(false);
        pnlSourceCard.ResumeLayout(false);
        pnlPreviewCard.ResumeLayout(false);
        pnlPreviewCard.PerformLayout();
        pnlPreviewWrapper.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
        pnlSelectedSource.ResumeLayout(false);
        pnlSelectedSource.PerformLayout();
        grpSource.ResumeLayout(false);
        grpSource.PerformLayout();
        pnlControlsCard.ResumeLayout(false);
        flowButtons.ResumeLayout(false);
        pnlOutputInfo.ResumeLayout(false);
        pnlOutputInfo.PerformLayout();
        tblStats.ResumeLayout(false);
        pnlStatDuration.ResumeLayout(false);
        pnlStatCount.ResumeLayout(false);
        pnlStatCountdown.ResumeLayout(false);
        grpInterval.ResumeLayout(false);
        pnlCustomInterval.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)numCustomInterval).EndInit();
        trayMenu.ResumeLayout(false);
        ResumeLayout(false);
    }
}
