namespace AutoSnap.Forms;

partial class SourcePickerForm
{
    private System.ComponentModel.IContainer components = null;
    private TabControl tabControlSources;
    private TabPage tabWindows;
    private TabPage tabMonitors;
    private Panel panelBottom;
    private Button btnSelect;
    private Button btnCancel;
    private Button btnRefresh;
    private ListView listViewWindows;
    private ColumnHeader colWinTitle;
    private ColumnHeader colWinProcess;
    private ColumnHeader colWinHwnd;
    private ColumnHeader colWinResolution;
    private Panel panelWindowTop;
    private TextBox txtSearch;
    private Label lblSearch;
    private Label lblWindowCount;
    private ListView listViewMonitors;
    private ColumnHeader colMonName;
    private ColumnHeader colMonResolution;
    private ColumnHeader colMonPrimary;
    private ColumnHeader colMonPosition;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
            _windowImageList?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        tabControlSources = new TabControl();
        tabWindows = new TabPage();
        listViewWindows = new ListView();
        colWinTitle = new ColumnHeader();
        colWinProcess = new ColumnHeader();
        colWinHwnd = new ColumnHeader();
        colWinResolution = new ColumnHeader();
        panelWindowTop = new Panel();
        lblWindowCount = new Label();
        txtSearch = new TextBox();
        lblSearch = new Label();
        tabMonitors = new TabPage();
        listViewMonitors = new ListView();
        colMonName = new ColumnHeader();
        colMonResolution = new ColumnHeader();
        colMonPrimary = new ColumnHeader();
        colMonPosition = new ColumnHeader();
        panelBottom = new Panel();
        btnRefresh = new Button();
        btnCancel = new Button();
        btnSelect = new Button();

        tabControlSources.SuspendLayout();
        tabWindows.SuspendLayout();
        panelWindowTop.SuspendLayout();
        tabMonitors.SuspendLayout();
        panelBottom.SuspendLayout();
        SuspendLayout();

        // 
        // tabControlSources
        // 
        tabControlSources.Controls.Add(tabWindows);
        tabControlSources.Controls.Add(tabMonitors);
        tabControlSources.Dock = DockStyle.Fill;
        tabControlSources.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        tabControlSources.ItemSize = new Size(160, 32);
        tabControlSources.Location = new Point(12, 12);
        tabControlSources.Name = "tabControlSources";
        tabControlSources.SelectedIndex = 0;
        tabControlSources.Size = new Size(660, 430);
        tabControlSources.SizeMode = TabSizeMode.Fixed;
        tabControlSources.TabIndex = 0;

        // 
        // tabWindows
        // 
        tabWindows.Controls.Add(listViewWindows);
        tabWindows.Controls.Add(panelWindowTop);
        tabWindows.Location = new Point(4, 36);
        tabWindows.Name = "tabWindows";
        tabWindows.Padding = new Padding(8);
        tabWindows.Size = new Size(652, 390);
        tabWindows.TabIndex = 0;
        tabWindows.Text = "Application Windows";
        tabWindows.UseVisualStyleBackColor = true;

        // 
        // panelWindowTop
        // 
        panelWindowTop.Controls.Add(lblWindowCount);
        panelWindowTop.Controls.Add(txtSearch);
        panelWindowTop.Controls.Add(lblSearch);
        panelWindowTop.Dock = DockStyle.Top;
        panelWindowTop.Location = new Point(8, 8);
        panelWindowTop.Name = "panelWindowTop";
        panelWindowTop.Size = new Size(636, 40);
        panelWindowTop.TabIndex = 0;

        // 
        // lblSearch
        // 
        lblSearch.AutoSize = true;
        lblSearch.Location = new Point(4, 11);
        lblSearch.Name = "lblSearch";
        lblSearch.Size = new Size(49, 17);
        lblSearch.TabIndex = 0;
        lblSearch.Text = "Search:";

        // 
        // txtSearch
        // 
        txtSearch.Location = new Point(59, 8);
        txtSearch.Name = "txtSearch";
        txtSearch.PlaceholderText = "Type window title or process name...";
        txtSearch.Size = new Size(320, 24);
        txtSearch.TabIndex = 1;
        txtSearch.TextChanged += txtSearch_TextChanged;

        // 
        // lblWindowCount
        // 
        lblWindowCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblWindowCount.ForeColor = Color.Gray;
        lblWindowCount.Location = new Point(420, 11);
        lblWindowCount.Name = "lblWindowCount";
        lblWindowCount.Size = new Size(210, 17);
        lblWindowCount.TabIndex = 2;
        lblWindowCount.Text = "0 windows";
        lblWindowCount.TextAlign = ContentAlignment.MiddleRight;

        // 
        // listViewWindows
        // 
        listViewWindows.Columns.AddRange(new ColumnHeader[] { colWinTitle, colWinProcess, colWinHwnd, colWinResolution });
        listViewWindows.Dock = DockStyle.Fill;
        listViewWindows.FullRowSelect = true;
        listViewWindows.GridLines = true;
        listViewWindows.HideSelection = false;
        listViewWindows.Location = new Point(8, 48);
        listViewWindows.MultiSelect = false;
        listViewWindows.Name = "listViewWindows";
        listViewWindows.Size = new Size(636, 334);
        listViewWindows.TabIndex = 1;
        listViewWindows.UseCompatibleStateImageBehavior = false;
        listViewWindows.View = View.Details;
        listViewWindows.DoubleClick += listViewWindows_DoubleClick;

        // 
        // colWinTitle
        // 
        colWinTitle.Text = "Window Title";
        colWinTitle.Width = 280;

        // 
        // colWinProcess
        // 
        colWinProcess.Text = "Process";
        colWinProcess.Width = 140;

        // 
        // colWinHwnd
        // 
        colWinHwnd.Text = "Handle";
        colWinHwnd.Width = 90;

        // 
        // colWinResolution
        // 
        colWinResolution.Text = "Resolution";
        colWinResolution.Width = 100;

        // 
        // tabMonitors
        // 
        tabMonitors.Controls.Add(listViewMonitors);
        tabMonitors.Location = new Point(4, 36);
        tabMonitors.Name = "tabMonitors";
        tabMonitors.Padding = new Padding(8);
        tabMonitors.Size = new Size(652, 390);
        tabMonitors.TabIndex = 1;
        tabMonitors.Text = "Monitors / Displays";
        tabMonitors.UseVisualStyleBackColor = true;

        // 
        // listViewMonitors
        // 
        listViewMonitors.Columns.AddRange(new ColumnHeader[] { colMonName, colMonResolution, colMonPrimary, colMonPosition });
        listViewMonitors.Dock = DockStyle.Fill;
        listViewMonitors.FullRowSelect = true;
        listViewMonitors.GridLines = true;
        listViewMonitors.HideSelection = false;
        listViewMonitors.Location = new Point(8, 8);
        listViewMonitors.MultiSelect = false;
        listViewMonitors.Name = "listViewMonitors";
        listViewMonitors.Size = new Size(636, 374);
        listViewMonitors.TabIndex = 0;
        listViewMonitors.UseCompatibleStateImageBehavior = false;
        listViewMonitors.View = View.Details;
        listViewMonitors.DoubleClick += listViewMonitors_DoubleClick;

        // 
        // colMonName
        // 
        colMonName.Text = "Display Name";
        colMonName.Width = 200;

        // 
        // colMonResolution
        // 
        colMonResolution.Text = "Resolution";
        colMonResolution.Width = 150;

        // 
        // colMonPrimary
        // 
        colMonPrimary.Text = "Primary";
        colMonPrimary.Width = 100;

        // 
        // colMonPosition
        // 
        colMonPosition.Text = "Virtual Position";
        colMonPosition.Width = 150;

        // 
        // panelBottom
        // 
        panelBottom.Controls.Add(btnRefresh);
        panelBottom.Controls.Add(btnCancel);
        panelBottom.Controls.Add(btnSelect);
        panelBottom.Dock = DockStyle.Bottom;
        panelBottom.Location = new Point(12, 442);
        panelBottom.Name = "panelBottom";
        panelBottom.Size = new Size(660, 50);
        panelBottom.TabIndex = 1;

        // 
        // btnRefresh
        // 
        btnRefresh.Location = new Point(0, 10);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(95, 32);
        btnRefresh.TabIndex = 0;
        btnRefresh.Text = "Refresh";
        btnRefresh.UseVisualStyleBackColor = true;
        btnRefresh.Click += btnRefresh_Click;

        // 
        // btnCancel
        // 
        btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.Location = new Point(565, 10);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(95, 32);
        btnCancel.TabIndex = 2;
        btnCancel.Text = "Cancel";
        btnCancel.UseVisualStyleBackColor = true;

        // 
        // btnSelect
        // 
        btnSelect.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSelect.BackColor = Color.FromArgb(0, 120, 215);
        btnSelect.FlatStyle = FlatStyle.Flat;
        btnSelect.ForeColor = Color.White;
        btnSelect.Location = new Point(455, 10);
        btnSelect.Name = "btnSelect";
        btnSelect.Size = new Size(100, 32);
        btnSelect.TabIndex = 1;
        btnSelect.Text = "Select";
        btnSelect.UseVisualStyleBackColor = false;
        btnSelect.Click += btnSelect_Click;

        // 
        // SourcePickerForm
        // 
        AcceptButton = btnSelect;
        CancelButton = btnCancel;
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        ClientSize = new Size(684, 504);
        Controls.Add(tabControlSources);
        Controls.Add(panelBottom);
        Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(600, 450);
        Padding = new Padding(12);
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Select Capture Source";
        tabControlSources.ResumeLayout(false);
        tabWindows.ResumeLayout(false);
        panelWindowTop.ResumeLayout(false);
        panelWindowTop.PerformLayout();
        tabMonitors.ResumeLayout(false);
        panelBottom.ResumeLayout(false);
        ResumeLayout(false);
    }
}
