namespace SharpView.Client;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        lblIp = new Label();
        txtIp = new TextBox();
        lblPort = new Label();
        txtPort = new TextBox();
        lblPartnerIdLabel = new Label();
        txtPartnerId = new TextBox();
        lblPasswordLabel = new Label();
        txtPassword = new TextBox();
        btnConnect = new Button();
        btnDisconnect = new Button();
        btnPing = new Button();
        pbScreen = new PictureBox();
        rtbLogs = new RichTextBox();
        lblStatus = new Label();
        lblFrameInfo = new Label();
        panelTop = new Panel();
        panelAuth = new Panel();
        splitContainer = new SplitContainer();

        ((System.ComponentModel.ISupportInitialize)pbScreen).BeginInit();
        panelTop.SuspendLayout();
        panelAuth.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        SuspendLayout();

        // ─── lblIp ───
        lblIp.AutoSize = true;
        lblIp.Font = new Font("Segoe UI Semibold", 9.5F);
        lblIp.Location = new Point(14, 17);
        lblIp.Name = "lblIp";
        lblIp.Size = new Size(55, 21);
        lblIp.TabIndex = 10;
        lblIp.Text = "Relay:";

        // ─── txtIp ───
        txtIp.Font = new Font("Segoe UI", 9.5F);
        txtIp.Location = new Point(66, 13);
        txtIp.Name = "txtIp";
        txtIp.Size = new Size(105, 29);
        txtIp.TabIndex = 9;
        txtIp.Text = "127.0.0.1";

        // ─── lblPort ───
        lblPort.AutoSize = true;
        lblPort.Font = new Font("Segoe UI Semibold", 9.5F);
        lblPort.Location = new Point(180, 17);
        lblPort.Name = "lblPort";
        lblPort.Size = new Size(44, 21);
        lblPort.TabIndex = 8;
        lblPort.Text = "Port:";

        // ─── txtPort ───
        txtPort.Font = new Font("Segoe UI", 9.5F);
        txtPort.Location = new Point(222, 13);
        txtPort.Name = "txtPort";
        txtPort.Size = new Size(60, 29);
        txtPort.TabIndex = 7;
        txtPort.Text = "9000";

        // ─── btnConnect ───
        btnConnect.BackColor = Color.FromArgb(46, 139, 87);
        btnConnect.Cursor = Cursors.Hand;
        btnConnect.FlatStyle = FlatStyle.Flat;
        btnConnect.Font = new Font("Segoe UI Semibold", 9F);
        btnConnect.ForeColor = Color.White;
        btnConnect.Location = new Point(300, 10);
        btnConnect.Name = "btnConnect";
        btnConnect.Size = new Size(100, 36);
        btnConnect.TabIndex = 2;
        btnConnect.Text = "🔗 Connect";
        btnConnect.UseVisualStyleBackColor = false;
        btnConnect.Click += BtnConnect_Click;

        // ─── btnDisconnect ───
        btnDisconnect.BackColor = Color.FromArgb(178, 34, 34);
        btnDisconnect.Cursor = Cursors.Hand;
        btnDisconnect.Enabled = false;
        btnDisconnect.FlatStyle = FlatStyle.Flat;
        btnDisconnect.Font = new Font("Segoe UI Semibold", 9F);
        btnDisconnect.ForeColor = Color.White;
        btnDisconnect.Location = new Point(410, 10);
        btnDisconnect.Name = "btnDisconnect";
        btnDisconnect.Size = new Size(110, 36);
        btnDisconnect.TabIndex = 1;
        btnDisconnect.Text = "✖ Disconnect";
        btnDisconnect.UseVisualStyleBackColor = false;
        btnDisconnect.Click += BtnDisconnect_Click;

        // ─── btnPing ───
        btnPing.BackColor = Color.FromArgb(70, 130, 180);
        btnPing.Cursor = Cursors.Hand;
        btnPing.Enabled = false;
        btnPing.FlatStyle = FlatStyle.Flat;
        btnPing.Font = new Font("Segoe UI Semibold", 9F);
        btnPing.ForeColor = Color.White;
        btnPing.Location = new Point(530, 10);
        btnPing.Name = "btnPing";
        btnPing.Size = new Size(85, 36);
        btnPing.TabIndex = 0;
        btnPing.Text = "📡 Ping";
        btnPing.UseVisualStyleBackColor = false;
        btnPing.Click += BtnPing_Click;

        // ─── panelAuth ─── (Partner ID + Password input row)
        panelAuth.BackColor = Color.FromArgb(38, 42, 52);
        panelAuth.Controls.Add(txtPassword);
        panelAuth.Controls.Add(lblPasswordLabel);
        panelAuth.Controls.Add(txtPartnerId);
        panelAuth.Controls.Add(lblPartnerIdLabel);
        panelAuth.Dock = DockStyle.Top;
        panelAuth.Location = new Point(0, 55);
        panelAuth.Name = "panelAuth";
        panelAuth.Padding = new Padding(14, 6, 14, 6);
        panelAuth.Size = new Size(1029, 42);
        panelAuth.TabIndex = 11;

        // ─── lblPartnerIdLabel ───
        lblPartnerIdLabel.AutoSize = true;
        lblPartnerIdLabel.Font = new Font("Segoe UI Semibold", 9.5F);
        lblPartnerIdLabel.ForeColor = Color.FromArgb(180, 200, 220);
        lblPartnerIdLabel.Location = new Point(14, 10);
        lblPartnerIdLabel.Name = "lblPartnerIdLabel";
        lblPartnerIdLabel.Size = new Size(85, 21);
        lblPartnerIdLabel.TabIndex = 6;
        lblPartnerIdLabel.Text = "Partner ID:";

        // ─── txtPartnerId ───
        txtPartnerId.Font = new Font("Segoe UI", 9.5F);
        txtPartnerId.Location = new Point(105, 6);
        txtPartnerId.MaxLength = 11;
        txtPartnerId.Name = "txtPartnerId";
        txtPartnerId.Size = new Size(120, 29);
        txtPartnerId.TabIndex = 5;
        txtPartnerId.TextAlign = HorizontalAlignment.Center;

        // ─── lblPasswordLabel ───
        lblPasswordLabel.AutoSize = true;
        lblPasswordLabel.Font = new Font("Segoe UI Semibold", 9.5F);
        lblPasswordLabel.ForeColor = Color.FromArgb(180, 200, 220);
        lblPasswordLabel.Location = new Point(245, 10);
        lblPasswordLabel.Name = "lblPasswordLabel";
        lblPasswordLabel.Size = new Size(80, 21);
        lblPasswordLabel.TabIndex = 4;
        lblPasswordLabel.Text = "Password:";

        // ─── txtPassword ───
        txtPassword.Font = new Font("Segoe UI", 9.5F);
        txtPassword.Location = new Point(330, 6);
        txtPassword.MaxLength = 4;
        txtPassword.Name = "txtPassword";
        txtPassword.Size = new Size(65, 29);
        txtPassword.TabIndex = 3;
        txtPassword.TextAlign = HorizontalAlignment.Center;
        txtPassword.UseSystemPasswordChar = true;

        // ─── panelTop ───
        panelTop.Controls.Add(btnPing);
        panelTop.Controls.Add(btnDisconnect);
        panelTop.Controls.Add(btnConnect);
        panelTop.Controls.Add(txtPort);
        panelTop.Controls.Add(lblPort);
        panelTop.Controls.Add(txtIp);
        panelTop.Controls.Add(lblIp);
        panelTop.Dock = DockStyle.Top;
        panelTop.Location = new Point(0, 0);
        panelTop.Name = "panelTop";
        panelTop.Padding = new Padding(11, 10, 11, 5);
        panelTop.Size = new Size(1029, 55);
        panelTop.TabIndex = 1;

        // ─── splitContainer ───
        splitContainer.BackColor = Color.FromArgb(45, 45, 52);
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.Location = new Point(0, 97);
        splitContainer.Name = "splitContainer";
        splitContainer.Orientation = Orientation.Horizontal;
        splitContainer.Panel1.Controls.Add(pbScreen);
        splitContainer.Panel1.Controls.Add(lblFrameInfo);
        splitContainer.Panel2.Controls.Add(rtbLogs);
        splitContainer.Size = new Size(1029, 735);
        splitContainer.SplitterDistance = 520;
        splitContainer.SplitterWidth = 5;
        splitContainer.TabIndex = 0;

        // ─── pbScreen ───
        pbScreen.BackColor = Color.Black;
        pbScreen.Dock = DockStyle.Fill;
        pbScreen.Location = new Point(0, 0);
        pbScreen.Name = "pbScreen";
        pbScreen.Size = new Size(1029, 493);
        pbScreen.SizeMode = PictureBoxSizeMode.Zoom;
        pbScreen.TabIndex = 0;
        pbScreen.TabStop = false;

        // ─── lblFrameInfo ───
        lblFrameInfo.BackColor = Color.FromArgb(20, 20, 25);
        lblFrameInfo.Dock = DockStyle.Bottom;
        lblFrameInfo.Font = new Font("Segoe UI", 8F);
        lblFrameInfo.ForeColor = Color.FromArgb(100, 180, 255);
        lblFrameInfo.Location = new Point(0, 493);
        lblFrameInfo.Name = "lblFrameInfo";
        lblFrameInfo.Padding = new Padding(0, 0, 9, 0);
        lblFrameInfo.Size = new Size(1029, 27);
        lblFrameInfo.TabIndex = 1;
        lblFrameInfo.TextAlign = ContentAlignment.MiddleRight;

        // ─── rtbLogs ───
        rtbLogs.BackColor = Color.FromArgb(25, 25, 30);
        rtbLogs.BorderStyle = BorderStyle.None;
        rtbLogs.Dock = DockStyle.Fill;
        rtbLogs.Font = new Font("Cascadia Code", 9F);
        rtbLogs.ForeColor = Color.FromArgb(200, 210, 220);
        rtbLogs.Location = new Point(0, 0);
        rtbLogs.Name = "rtbLogs";
        rtbLogs.ReadOnly = true;
        rtbLogs.Size = new Size(1029, 210);
        rtbLogs.TabIndex = 0;
        rtbLogs.Text = "";

        // ─── lblStatus ───
        lblStatus.BackColor = Color.FromArgb(35, 35, 42);
        lblStatus.Dock = DockStyle.Bottom;
        lblStatus.Font = new Font("Segoe UI", 9F);
        lblStatus.ForeColor = Color.IndianRed;
        lblStatus.Location = new Point(0, 832);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(1029, 35);
        lblStatus.TabIndex = 2;
        lblStatus.Text = "  ● Disconnected";
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;

        // ─── Form1 ───
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 36);
        ClientSize = new Size(1029, 867);
        Controls.Add(splitContainer);
        Controls.Add(panelAuth);
        Controls.Add(panelTop);
        Controls.Add(lblStatus);
        Font = new Font("Segoe UI", 9F);
        ForeColor = Color.White;
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "SharpView — Client";

        ((System.ComponentModel.ISupportInitialize)pbScreen).EndInit();
        panelTop.ResumeLayout(false);
        panelTop.PerformLayout();
        panelAuth.ResumeLayout(false);
        panelAuth.PerformLayout();
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private Label lblIp;
    private TextBox txtIp;
    private Label lblPort;
    private TextBox txtPort;
    private Label lblPartnerIdLabel;
    private TextBox txtPartnerId;
    private Label lblPasswordLabel;
    private TextBox txtPassword;
    private Button btnConnect;
    private Button btnDisconnect;
    private Button btnPing;
    private PictureBox pbScreen;
    private RichTextBox rtbLogs;
    private Label lblStatus;
    private Label lblFrameInfo;
    private Panel panelTop;
    private Panel panelAuth;
    private SplitContainer splitContainer;
}
