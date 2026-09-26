namespace AutoSnap.Forms;

partial class SettingsForm
{
    private System.ComponentModel.IContainer components = null;
    private TabControl tabControlSettings;
    private TabPage tabGeneral;
    private TabPage tabCapture;
    private TabPage tabChrome;
    private TabPage tabStorage;
    private Panel panelBottom;
    private Button btnSave;
    private Button btnCancel;
    private GroupBox grpStoragePath;
    private TextBox txtOutputFolder;
    private Button btnBrowseOutput;
    private GroupBox grpTray;
    private CheckBox chkContinueWhileMinimized;
    private CheckBox chkStartMinimized;
    private CheckBox chkMinimizeToTray;
    private GroupBox grpImageFormat;
    private RadioButton rbFormatPng;
    private RadioButton rbFormatJpg;
    private Label lblQuality;
    private TrackBar trackQuality;
    private NumericUpDown numQuality;
    private GroupBox grpResolution;
    private ComboBox cmbScale;
    private Label lblScale;
    private GroupBox grpChromePaths;
    private Label lblChromeExe;
    private TextBox txtChromeExe;
    private Button btnBrowseChromeExe;
    private Button btnAutoDetectChrome;
    private Label lblChromeNotice;
    private GroupBox grpStorageRules;
    private CheckBox chkStorageCleanup;
    private Label lblMaxStorage;
    private NumericUpDown numMaxStorage;
    private Label lblMb;
    private Label lblDays;
    private NumericUpDown numDeleteDays;
    private Label lblDeleteDays;

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
        tabControlSettings = new TabControl();
        tabGeneral = new TabPage();
        grpTray = new GroupBox();
        chkContinueWhileMinimized = new CheckBox();
        chkStartMinimized = new CheckBox();
        chkMinimizeToTray = new CheckBox();
        grpStoragePath = new GroupBox();
        btnBrowseOutput = new Button();
        txtOutputFolder = new TextBox();
        tabCapture = new TabPage();
        grpResolution = new GroupBox();
        cmbScale = new ComboBox();
        lblScale = new Label();
        grpImageFormat = new GroupBox();
        numQuality = new NumericUpDown();
        trackQuality = new TrackBar();
        lblQuality = new Label();
        rbFormatPng = new RadioButton();
        rbFormatJpg = new RadioButton();
        tabChrome = new TabPage();
        grpChromePaths = new GroupBox();
        lblChromeNotice = new Label();
        btnAutoDetectChrome = new Button();
        btnBrowseChromeExe = new Button();
        txtChromeExe = new TextBox();
        lblChromeExe = new Label();
        tabStorage = new TabPage();
        grpStorageRules = new GroupBox();
        lblDays = new Label();
        numDeleteDays = new NumericUpDown();
        lblDeleteDays = new Label();
        lblMb = new Label();
        numMaxStorage = new NumericUpDown();
        lblMaxStorage = new Label();
        chkStorageCleanup = new CheckBox();
        panelBottom = new Panel();
        btnCancel = new Button();
        btnSave = new Button();

        tabControlSettings.SuspendLayout();
        tabGeneral.SuspendLayout();
        grpTray.SuspendLayout();
        grpStoragePath.SuspendLayout();
        tabCapture.SuspendLayout();
        grpResolution.SuspendLayout();
        grpImageFormat.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numQuality).BeginInit();
        ((System.ComponentModel.ISupportInitialize)trackQuality).BeginInit();
        tabChrome.SuspendLayout();
        grpChromePaths.SuspendLayout();
        tabStorage.SuspendLayout();
        grpStorageRules.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numDeleteDays).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numMaxStorage).BeginInit();
        panelBottom.SuspendLayout();
        SuspendLayout();

        // 
        // tabControlSettings
        // 
        tabControlSettings.Controls.Add(tabGeneral);
        tabControlSettings.Controls.Add(tabCapture);
        tabControlSettings.Controls.Add(tabChrome);
        tabControlSettings.Controls.Add(tabStorage);
        tabControlSettings.Dock = DockStyle.Fill;
        tabControlSettings.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        tabControlSettings.ItemSize = new Size(110, 30);
        tabControlSettings.Location = new Point(12, 12);
        tabControlSettings.Name = "tabControlSettings";
        tabControlSettings.SelectedIndex = 0;
        tabControlSettings.Size = new Size(540, 420);
        tabControlSettings.SizeMode = TabSizeMode.Fixed;
        tabControlSettings.TabIndex = 0;

        // 
        // tabGeneral
        // 
        tabGeneral.Controls.Add(grpTray);
        tabGeneral.Controls.Add(grpStoragePath);
        tabGeneral.Location = new Point(4, 34);
        tabGeneral.Name = "tabGeneral";
        tabGeneral.Padding = new Padding(12);
        tabGeneral.Size = new Size(532, 382);
        tabGeneral.TabIndex = 0;
        tabGeneral.Text = "General";
        tabGeneral.UseVisualStyleBackColor = true;

        // 
        // grpTray
        // 
        grpTray.Controls.Add(chkContinueWhileMinimized);
        grpTray.Controls.Add(chkStartMinimized);
        grpTray.Controls.Add(chkMinimizeToTray);
        grpTray.Dock = DockStyle.Top;
        grpTray.Location = new Point(12, 102);
        grpTray.Name = "grpTray";
        grpTray.Padding = new Padding(12);
        grpTray.Size = new Size(508, 140);
        grpTray.TabIndex = 1;
        grpTray.TabStop = false;
        grpTray.Text = "System Tray & Window";

        // 
        // chkContinueWhileMinimized
        // 
        chkContinueWhileMinimized.AutoSize = true;
        chkContinueWhileMinimized.Location = new Point(16, 95);
        chkContinueWhileMinimized.Name = "chkContinueWhileMinimized";
        chkContinueWhileMinimized.Size = new Size(230, 21);
        chkContinueWhileMinimized.TabIndex = 2;
        chkContinueWhileMinimized.Text = "Continue capturing while minimized";
        chkContinueWhileMinimized.UseVisualStyleBackColor = true;

        // 
        // chkStartMinimized
        // 
        chkStartMinimized.AutoSize = true;
        chkStartMinimized.Location = new Point(16, 62);
        chkStartMinimized.Name = "chkStartMinimized";
        chkStartMinimized.Size = new Size(116, 21);
        chkStartMinimized.TabIndex = 1;
        chkStartMinimized.Text = "Start minimized";
        chkStartMinimized.UseVisualStyleBackColor = true;

        // 
        // chkMinimizeToTray
        // 
        chkMinimizeToTray.AutoSize = true;
        chkMinimizeToTray.Location = new Point(16, 29);
        chkMinimizeToTray.Name = "chkMinimizeToTray";
        chkMinimizeToTray.Size = new Size(122, 21);
        chkMinimizeToTray.TabIndex = 0;
        chkMinimizeToTray.Text = "Minimize to tray";
        chkMinimizeToTray.UseVisualStyleBackColor = true;

        // 
        // grpStoragePath
        // 
        grpStoragePath.Controls.Add(btnBrowseOutput);
        grpStoragePath.Controls.Add(txtOutputFolder);
        grpStoragePath.Dock = DockStyle.Top;
        grpStoragePath.Location = new Point(12, 12);
        grpStoragePath.Name = "grpStoragePath";
        grpStoragePath.Padding = new Padding(12);
        grpStoragePath.Size = new Size(508, 90);
        grpStoragePath.TabIndex = 0;
        grpStoragePath.TabStop = false;
        grpStoragePath.Text = "Default Output Folder";

        // 
        // btnBrowseOutput
        // 
        btnBrowseOutput.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowseOutput.Location = new Point(416, 32);
        btnBrowseOutput.Name = "btnBrowseOutput";
        btnBrowseOutput.Size = new Size(80, 30);
        btnBrowseOutput.TabIndex = 1;
        btnBrowseOutput.Text = "Browse...";
        btnBrowseOutput.UseVisualStyleBackColor = true;
        btnBrowseOutput.Click += btnBrowseOutput_Click;

        // 
        // txtOutputFolder
        // 
        txtOutputFolder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtOutputFolder.Location = new Point(16, 35);
        txtOutputFolder.Name = "txtOutputFolder";
        txtOutputFolder.Size = new Size(394, 24);
        txtOutputFolder.TabIndex = 0;

        // 
        // tabCapture
        // 
        tabCapture.Controls.Add(grpResolution);
        tabCapture.Controls.Add(grpImageFormat);
        tabCapture.Location = new Point(4, 34);
        tabCapture.Name = "tabCapture";
        tabCapture.Padding = new Padding(12);
        tabCapture.Size = new Size(532, 382);
        tabCapture.TabIndex = 1;
        tabCapture.Text = "Capture & Quality";
        tabCapture.UseVisualStyleBackColor = true;

        // 
        // grpResolution
        // 
        grpResolution.Controls.Add(cmbScale);
        grpResolution.Controls.Add(lblScale);
        grpResolution.Dock = DockStyle.Top;
        grpResolution.Location = new Point(12, 162);
        grpResolution.Name = "grpResolution";
        grpResolution.Padding = new Padding(12);
        grpResolution.Size = new Size(508, 88);
        grpResolution.TabIndex = 1;
        grpResolution.TabStop = false;
        grpResolution.Text = "Resolution Scaling";

        // 
        // cmbScale
        // 
        cmbScale.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbScale.FormattingEnabled = true;
        cmbScale.Items.AddRange(new object[] { "Original (100%)", "75% Scale", "50% Scale" });
        cmbScale.Location = new Point(120, 33);
        cmbScale.Name = "cmbScale";
        cmbScale.Size = new Size(180, 25);
        cmbScale.TabIndex = 1;

        // 
        // lblScale
        // 
        lblScale.AutoSize = true;
        lblScale.Location = new Point(16, 36);
        lblScale.Name = "lblScale";
        lblScale.Size = new Size(88, 17);
        lblScale.TabIndex = 0;
        lblScale.Text = "Output Scale:";

        // 
        // grpImageFormat
        // 
        grpImageFormat.Controls.Add(numQuality);
        grpImageFormat.Controls.Add(trackQuality);
        grpImageFormat.Controls.Add(lblQuality);
        grpImageFormat.Controls.Add(rbFormatPng);
        grpImageFormat.Controls.Add(rbFormatJpg);
        grpImageFormat.Dock = DockStyle.Top;
        grpImageFormat.Location = new Point(12, 12);
        grpImageFormat.Name = "grpImageFormat";
        grpImageFormat.Padding = new Padding(12);
        grpImageFormat.Size = new Size(508, 150);
        grpImageFormat.TabIndex = 0;
        grpImageFormat.TabStop = false;
        grpImageFormat.Text = "Image Format";

        // 
        // numQuality
        // 
        numQuality.Location = new Point(340, 93);
        numQuality.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numQuality.Name = "numQuality";
        numQuality.Size = new Size(60, 24);
        numQuality.TabIndex = 4;
        numQuality.Value = new decimal(new int[] { 90, 0, 0, 0 });
        numQuality.ValueChanged += numQuality_ValueChanged;

        // 
        // trackQuality
        // 
        trackQuality.LargeChange = 10;
        trackQuality.Location = new Point(106, 93);
        trackQuality.Maximum = 100;
        trackQuality.Minimum = 1;
        trackQuality.Name = "trackQuality";
        trackQuality.Size = new Size(220, 45);
        trackQuality.TabIndex = 3;
        trackQuality.TickFrequency = 10;
        trackQuality.Value = 90;
        trackQuality.Scroll += trackQuality_Scroll;

        // 
        // lblQuality
        // 
        lblQuality.AutoSize = true;
        lblQuality.Location = new Point(16, 95);
        lblQuality.Name = "lblQuality";
        lblQuality.Size = new Size(84, 17);
        lblQuality.TabIndex = 2;
        lblQuality.Text = "JPEG Quality:";

        // 
        // rbFormatPng
        // 
        rbFormatPng.AutoSize = true;
        rbFormatPng.Location = new Point(16, 58);
        rbFormatPng.Name = "rbFormatPng";
        rbFormatPng.Size = new Size(189, 21);
        rbFormatPng.TabIndex = 1;
        rbFormatPng.Text = "PNG (Lossless, larger files)";
        rbFormatPng.UseVisualStyleBackColor = true;
        rbFormatPng.CheckedChanged += rbFormat_CheckedChanged;

        // 
        // rbFormatJpg
        // 
        rbFormatJpg.AutoSize = true;
        rbFormatJpg.Checked = true;
        rbFormatJpg.Location = new Point(16, 28);
        rbFormatJpg.Name = "rbFormatJpg";
        rbFormatJpg.Size = new Size(211, 21);
        rbFormatJpg.TabIndex = 0;
        rbFormatJpg.TabStop = true;
        rbFormatJpg.Text = "JPEG (Compressed, customizable)";
        rbFormatJpg.UseVisualStyleBackColor = true;
        rbFormatJpg.CheckedChanged += rbFormat_CheckedChanged;

        // 
        // tabChrome
        // 
        tabChrome.Controls.Add(grpChromePaths);
        tabChrome.Location = new Point(4, 34);
        tabChrome.Name = "tabChrome";
        tabChrome.Padding = new Padding(12);
        tabChrome.Size = new Size(532, 382);
        tabChrome.TabIndex = 2;
        tabChrome.Text = "Google Chrome";
        tabChrome.UseVisualStyleBackColor = true;

        // 
        // grpChromePaths
        // 
        grpChromePaths.Controls.Add(lblChromeNotice);
        grpChromePaths.Controls.Add(btnAutoDetectChrome);
        grpChromePaths.Controls.Add(btnBrowseChromeExe);
        grpChromePaths.Controls.Add(txtChromeExe);
        grpChromePaths.Controls.Add(lblChromeExe);
        grpChromePaths.Dock = DockStyle.Top;
        grpChromePaths.Location = new Point(12, 12);
        grpChromePaths.Name = "grpChromePaths";
        grpChromePaths.Padding = new Padding(12);
        grpChromePaths.Size = new Size(508, 180);
        grpChromePaths.TabIndex = 0;
        grpChromePaths.TabStop = false;
        grpChromePaths.Text = "Google Chrome Executable";

        // 
        // lblChromeNotice
        // 
        lblChromeNotice.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblChromeNotice.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        lblChromeNotice.ForeColor = Color.FromArgb(108, 117, 125);
        lblChromeNotice.Location = new Point(16, 88);
        lblChromeNotice.Name = "lblChromeNotice";
        lblChromeNotice.Size = new Size(475, 75);
        lblChromeNotice.TabIndex = 4;
        lblChromeNotice.Text = "AutoSnap opens a lightweight local capture page in your normal, existing Chrome session. When you click 'Choose Chrome Tab', Chrome displays its native screen sharing picker for you to select any open tab. No isolated profiles, debug flags, or extensions required.";

        // 
        // lblChromeExe
        // 
        lblChromeExe.AutoSize = true;
        lblChromeExe.Location = new Point(16, 26);
        lblChromeExe.Name = "lblChromeExe";
        lblChromeExe.Size = new Size(168, 17);
        lblChromeExe.TabIndex = 0;
        lblChromeExe.Text = "Chrome executable location:";

        // 
        // txtChromeExe
        // 
        txtChromeExe.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtChromeExe.Location = new Point(16, 48);
        txtChromeExe.Name = "txtChromeExe";
        txtChromeExe.PlaceholderText = "Auto-detected standard Chrome path if left blank";
        txtChromeExe.Size = new Size(330, 24);
        txtChromeExe.TabIndex = 1;

        // 
        // btnBrowseChromeExe
        // 
        btnBrowseChromeExe.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowseChromeExe.Location = new Point(352, 45);
        btnBrowseChromeExe.Name = "btnBrowseChromeExe";
        btnBrowseChromeExe.Size = new Size(70, 30);
        btnBrowseChromeExe.TabIndex = 2;
        btnBrowseChromeExe.Text = "Browse...";
        btnBrowseChromeExe.UseVisualStyleBackColor = true;
        btnBrowseChromeExe.Click += btnBrowseChromeExe_Click;

        // 
        // btnAutoDetectChrome
        // 
        btnAutoDetectChrome.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnAutoDetectChrome.Location = new Point(426, 45);
        btnAutoDetectChrome.Name = "btnAutoDetectChrome";
        btnAutoDetectChrome.Size = new Size(70, 30);
        btnAutoDetectChrome.TabIndex = 3;
        btnAutoDetectChrome.Text = "Detect";
        btnAutoDetectChrome.UseVisualStyleBackColor = true;
        btnAutoDetectChrome.Click += btnAutoDetectChrome_Click;

        // 
        // tabStorage
        // 
        tabStorage.Controls.Add(grpStorageRules);
        tabStorage.Location = new Point(4, 34);
        tabStorage.Name = "tabStorage";
        tabStorage.Padding = new Padding(12);
        tabStorage.Size = new Size(532, 382);
        tabStorage.TabIndex = 3;
        tabStorage.Text = "Storage";
        tabStorage.UseVisualStyleBackColor = true;

        // 
        // grpStorageRules
        // 
        grpStorageRules.Controls.Add(lblDays);
        grpStorageRules.Controls.Add(numDeleteDays);
        grpStorageRules.Controls.Add(lblDeleteDays);
        grpStorageRules.Controls.Add(lblMb);
        grpStorageRules.Controls.Add(numMaxStorage);
        grpStorageRules.Controls.Add(lblMaxStorage);
        grpStorageRules.Controls.Add(chkStorageCleanup);
        grpStorageRules.Dock = DockStyle.Top;
        grpStorageRules.Location = new Point(12, 12);
        grpStorageRules.Name = "grpStorageRules";
        grpStorageRules.Padding = new Padding(12);
        grpStorageRules.Size = new Size(508, 150);
        grpStorageRules.TabIndex = 0;
        grpStorageRules.TabStop = false;
        grpStorageRules.Text = "Automatic Cleanup";

        // 
        // lblDays
        // 
        lblDays.AutoSize = true;
        lblDays.Location = new Point(270, 108);
        lblDays.Name = "lblDays";
        lblDays.Size = new Size(34, 17);
        lblDays.TabIndex = 6;
        lblDays.Text = "days";

        // 
        // numDeleteDays
        // 
        numDeleteDays.Location = new Point(170, 106);
        numDeleteDays.Maximum = new decimal(new int[] { 365, 0, 0, 0 });
        numDeleteDays.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numDeleteDays.Name = "numDeleteDays";
        numDeleteDays.Size = new Size(90, 24);
        numDeleteDays.TabIndex = 5;
        numDeleteDays.Value = new decimal(new int[] { 30, 0, 0, 0 });

        // 
        // lblDeleteDays
        // 
        lblDeleteDays.AutoSize = true;
        lblDeleteDays.Location = new Point(16, 108);
        lblDeleteDays.Name = "lblDeleteDays";
        lblDeleteDays.Size = new Size(111, 17);
        lblDeleteDays.TabIndex = 4;
        lblDeleteDays.Text = "Delete older than:";

        // 
        // lblMb
        // 
        lblMb.AutoSize = true;
        lblMb.Location = new Point(270, 72);
        lblMb.Name = "lblMb";
        lblMb.Size = new Size(28, 17);
        lblMb.TabIndex = 3;
        lblMb.Text = "MB";

        // 
        // numMaxStorage
        // 
        numMaxStorage.Location = new Point(170, 70);
        numMaxStorage.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
        numMaxStorage.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
        numMaxStorage.Name = "numMaxStorage";
        numMaxStorage.Size = new Size(90, 24);
        numMaxStorage.TabIndex = 2;
        numMaxStorage.Value = new decimal(new int[] { 5000, 0, 0, 0 });

        // 
        // lblMaxStorage
        // 
        lblMaxStorage.AutoSize = true;
        lblMaxStorage.Location = new Point(16, 72);
        lblMaxStorage.Name = "lblMaxStorage";
        lblMaxStorage.Size = new Size(117, 17);
        lblMaxStorage.TabIndex = 1;
        lblMaxStorage.Text = "Max directory size:";

        // 
        // chkStorageCleanup
        // 
        chkStorageCleanup.AutoSize = true;
        chkStorageCleanup.Location = new Point(16, 32);
        chkStorageCleanup.Name = "chkStorageCleanup";
        chkStorageCleanup.Size = new Size(233, 21);
        chkStorageCleanup.TabIndex = 0;
        chkStorageCleanup.Text = "Enable automatic storage cleanup";
        chkStorageCleanup.UseVisualStyleBackColor = true;

        // 
        // panelBottom
        // 
        panelBottom.Controls.Add(btnCancel);
        panelBottom.Controls.Add(btnSave);
        panelBottom.Dock = DockStyle.Bottom;
        panelBottom.Location = new Point(12, 432);
        panelBottom.Name = "panelBottom";
        panelBottom.Size = new Size(540, 50);
        panelBottom.TabIndex = 1;

        // 
        // btnCancel
        // 
        btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.Location = new Point(445, 10);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(90, 32);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "Cancel";
        btnCancel.UseVisualStyleBackColor = true;

        // 
        // btnSave
        // 
        btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSave.BackColor = Color.FromArgb(0, 120, 215);
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.ForeColor = Color.White;
        btnSave.Location = new Point(345, 10);
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(90, 32);
        btnSave.TabIndex = 0;
        btnSave.Text = "Save";
        btnSave.UseVisualStyleBackColor = false;
        btnSave.Click += btnSave_Click;

        // 
        // SettingsForm
        // 
        AcceptButton = btnSave;
        CancelButton = btnCancel;
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        ClientSize = new Size(564, 494);
        Controls.Add(tabControlSettings);
        Controls.Add(panelBottom);
        Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Padding = new Padding(12);
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Settings";
        tabControlSettings.ResumeLayout(false);
        tabGeneral.ResumeLayout(false);
        grpTray.ResumeLayout(false);
        grpTray.PerformLayout();
        grpStoragePath.ResumeLayout(false);
        grpStoragePath.PerformLayout();
        tabCapture.ResumeLayout(false);
        grpResolution.ResumeLayout(false);
        grpResolution.PerformLayout();
        grpImageFormat.ResumeLayout(false);
        grpImageFormat.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numQuality).EndInit();
        ((System.ComponentModel.ISupportInitialize)trackQuality).EndInit();
        tabChrome.ResumeLayout(false);
        grpChromePaths.ResumeLayout(false);
        grpChromePaths.PerformLayout();
        tabStorage.ResumeLayout(false);
        grpStorageRules.ResumeLayout(false);
        grpStorageRules.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numDeleteDays).EndInit();
        ((System.ComponentModel.ISupportInitialize)numMaxStorage).EndInit();
        panelBottom.ResumeLayout(false);
        ResumeLayout(false);
    }
}
