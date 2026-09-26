namespace AutoSnap.Forms;

partial class ChromeTabPickerForm
{
    private System.ComponentModel.IContainer components = null;
    private Panel pnlHeader;
    private Label lblTitle;
    private Label lblSubtitle;
    private Panel pnlConnectionStatus;
    private Label lblStatusDot;
    private Label lblStatusText;
    private Button btnLaunchChrome;
    private Panel pnlSearch;
    private TextBox txtSearch;
    private Label lblSearch;
    private Button btnRefresh;
    private ListView listViewTabs;
    private ColumnHeader colTitle;
    private ColumnHeader colUrl;
    private ColumnHeader colType;
    private Panel pnlBottom;
    private Button btnSelect;
    private Button btnCancel;
    private Label lblTabCount;

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
        pnlHeader = new Panel();
        lblSubtitle = new Label();
        lblTitle = new Label();
        pnlConnectionStatus = new Panel();
        btnLaunchChrome = new Button();
        lblStatusText = new Label();
        lblStatusDot = new Label();
        pnlSearch = new Panel();
        btnRefresh = new Button();
        txtSearch = new TextBox();
        lblSearch = new Label();
        listViewTabs = new ListView();
        colTitle = new ColumnHeader();
        colUrl = new ColumnHeader();
        colType = new ColumnHeader();
        pnlBottom = new Panel();
        lblTabCount = new Label();
        btnCancel = new Button();
        btnSelect = new Button();
        pnlHeader.SuspendLayout();
        pnlConnectionStatus.SuspendLayout();
        pnlSearch.SuspendLayout();
        pnlBottom.SuspendLayout();
        SuspendLayout();

        // 
        // pnlHeader
        // 
        pnlHeader.BackColor = Color.FromArgb(248, 249, 250);
        pnlHeader.Controls.Add(lblSubtitle);
        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Location = new Point(0, 0);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Padding = new Padding(16, 12, 16, 12);
        pnlHeader.Size = new Size(680, 60);
        pnlHeader.TabIndex = 0;

        // 
        // lblSubtitle
        // 
        lblSubtitle.AutoSize = true;
        lblSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        lblSubtitle.ForeColor = Color.FromArgb(108, 117, 125);
        lblSubtitle.Location = new Point(16, 32);
        lblSubtitle.Name = "lblSubtitle";
        lblSubtitle.Size = new Size(330, 15);
        lblSubtitle.TabIndex = 1;
        lblSubtitle.Text = "Select an open Chrome tab to capture via DevTools Protocol";

        // 
        // lblTitle
        // 
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point);
        lblTitle.ForeColor = Color.FromArgb(33, 37, 41);
        lblTitle.Location = new Point(15, 9);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(149, 21);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Select Chrome Tab";

        // 
        // pnlConnectionStatus
        // 
        pnlConnectionStatus.BackColor = Color.FromArgb(241, 243, 245);
        pnlConnectionStatus.Controls.Add(btnLaunchChrome);
        pnlConnectionStatus.Controls.Add(lblStatusText);
        pnlConnectionStatus.Controls.Add(lblStatusDot);
        pnlConnectionStatus.Dock = DockStyle.Top;
        pnlConnectionStatus.Location = new Point(0, 60);
        pnlConnectionStatus.Name = "pnlConnectionStatus";
        pnlConnectionStatus.Padding = new Padding(16, 8, 16, 8);
        pnlConnectionStatus.Size = new Size(680, 42);
        pnlConnectionStatus.TabIndex = 1;

        // 
        // btnLaunchChrome
        // 
        btnLaunchChrome.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnLaunchChrome.BackColor = Color.White;
        btnLaunchChrome.FlatStyle = FlatStyle.Flat;
        btnLaunchChrome.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
        btnLaunchChrome.ForeColor = Color.FromArgb(0, 120, 215);
        btnLaunchChrome.Location = new Point(500, 6);
        btnLaunchChrome.Name = "btnLaunchChrome";
        btnLaunchChrome.Size = new Size(165, 28);
        btnLaunchChrome.TabIndex = 2;
        btnLaunchChrome.Text = "🚀 Launch Capture Chrome";
        btnLaunchChrome.UseVisualStyleBackColor = false;
        btnLaunchChrome.Click += btnLaunchChrome_Click;

        // 
        // lblStatusText
        // 
        lblStatusText.AutoSize = true;
        lblStatusText.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
        lblStatusText.ForeColor = Color.FromArgb(73, 80, 87);
        lblStatusText.Location = new Point(34, 13);
        lblStatusText.Name = "lblStatusText";
        lblStatusText.Size = new Size(134, 15);
        lblStatusText.TabIndex = 1;
        lblStatusText.Text = "Chrome: Not Connected";

        // 
        // lblStatusDot
        // 
        lblStatusDot.AutoSize = true;
        lblStatusDot.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        lblStatusDot.ForeColor = Color.Crimson;
        lblStatusDot.Location = new Point(16, 11);
        lblStatusDot.Name = "lblStatusDot";
        lblStatusDot.Size = new Size(19, 19);
        lblStatusDot.TabIndex = 0;
        lblStatusDot.Text = "●";

        // 
        // pnlSearch
        // 
        pnlSearch.Controls.Add(btnRefresh);
        pnlSearch.Controls.Add(txtSearch);
        pnlSearch.Controls.Add(lblSearch);
        pnlSearch.Dock = DockStyle.Top;
        pnlSearch.Location = new Point(0, 102);
        pnlSearch.Name = "pnlSearch";
        pnlSearch.Padding = new Padding(16, 8, 16, 8);
        pnlSearch.Size = new Size(680, 44);
        pnlSearch.TabIndex = 2;

        // 
        // btnRefresh
        // 
        btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnRefresh.Location = new Point(585, 7);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(80, 28);
        btnRefresh.TabIndex = 2;
        btnRefresh.Text = "🔄 Refresh";
        btnRefresh.UseVisualStyleBackColor = true;
        btnRefresh.Click += btnRefresh_Click;

        // 
        // txtSearch
        // 
        txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtSearch.Location = new Point(68, 10);
        txtSearch.Name = "txtSearch";
        txtSearch.PlaceholderText = "Filter tabs by title or URL...";
        txtSearch.Size = new Size(505, 23);
        txtSearch.TabIndex = 1;
        txtSearch.TextChanged += txtSearch_TextChanged;

        // 
        // lblSearch
        // 
        lblSearch.AutoSize = true;
        lblSearch.Location = new Point(16, 13);
        lblSearch.Name = "lblSearch";
        lblSearch.Size = new Size(36, 15);
        lblSearch.TabIndex = 0;
        lblSearch.Text = "Filter:";

        // 
        // listViewTabs
        // 
        listViewTabs.Columns.AddRange(new ColumnHeader[] { colTitle, colUrl, colType });
        listViewTabs.Dock = DockStyle.Fill;
        listViewTabs.FullRowSelect = true;
        listViewTabs.GridLines = true;
        listViewTabs.Location = new Point(0, 146);
        listViewTabs.MultiSelect = false;
        listViewTabs.Name = "listViewTabs";
        listViewTabs.Size = new Size(680, 274);
        listViewTabs.TabIndex = 3;
        listViewTabs.UseCompatibleStateImageBehavior = false;
        listViewTabs.View = View.Details;
        listViewTabs.DoubleClick += listViewTabs_DoubleClick;

        // 
        // colTitle
        // 
        colTitle.Text = "Tab Title";
        colTitle.Width = 260;

        // 
        // colUrl
        // 
        colUrl.Text = "URL";
        colUrl.Width = 320;

        // 
        // colType
        // 
        colType.Text = "Type";
        colType.Width = 80;

        // 
        // pnlBottom
        // 
        pnlBottom.BackColor = Color.FromArgb(248, 249, 250);
        pnlBottom.Controls.Add(lblTabCount);
        pnlBottom.Controls.Add(btnCancel);
        pnlBottom.Controls.Add(btnSelect);
        pnlBottom.Dock = DockStyle.Bottom;
        pnlBottom.Location = new Point(0, 420);
        pnlBottom.Name = "pnlBottom";
        pnlBottom.Padding = new Padding(16, 10, 16, 10);
        pnlBottom.Size = new Size(680, 50);
        pnlBottom.TabIndex = 4;

        // 
        // lblTabCount
        // 
        lblTabCount.AutoSize = true;
        lblTabCount.ForeColor = Color.FromArgb(108, 117, 125);
        lblTabCount.Location = new Point(16, 17);
        lblTabCount.Name = "lblTabCount";
        lblTabCount.Size = new Size(91, 15);
        lblTabCount.TabIndex = 2;
        lblTabCount.Text = "0 tab(s) available";

        // 
        // btnCancel
        // 
        btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.Location = new Point(585, 10);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(80, 30);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "Cancel";
        btnCancel.UseVisualStyleBackColor = true;

        // 
        // btnSelect
        // 
        btnSelect.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSelect.BackColor = Color.FromArgb(0, 120, 215);
        btnSelect.FlatStyle = FlatStyle.Flat;
        btnSelect.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
        btnSelect.ForeColor = Color.White;
        btnSelect.Location = new Point(485, 10);
        btnSelect.Name = "btnSelect";
        btnSelect.Size = new Size(90, 30);
        btnSelect.TabIndex = 0;
        btnSelect.Text = "Select Tab";
        btnSelect.UseVisualStyleBackColor = false;
        btnSelect.Click += btnSelect_Click;

        // 
        // ChromeTabPickerForm
        // 
        AcceptButton = btnSelect;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnCancel;
        ClientSize = new Size(680, 470);
        Controls.Add(listViewTabs);
        Controls.Add(pnlBottom);
        Controls.Add(pnlSearch);
        Controls.Add(pnlConnectionStatus);
        Controls.Add(pnlHeader);
        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(600, 400);
        Name = "ChromeTabPickerForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Select Chrome Tab — AutoSnap";
        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        pnlConnectionStatus.ResumeLayout(false);
        pnlConnectionStatus.PerformLayout();
        pnlSearch.ResumeLayout(false);
        pnlSearch.PerformLayout();
        pnlBottom.ResumeLayout(false);
        pnlBottom.PerformLayout();
        ResumeLayout(false);
    }
}
