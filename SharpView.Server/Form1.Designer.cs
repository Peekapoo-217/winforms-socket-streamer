namespace SharpView.Server;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        lblRelayIp = new Label();
        txtRelayIp = new TextBox();
        lblPort = new Label();
        txtPort = new TextBox();
        btnStart = new Button();
        btnStop = new Button();
        btnToggleStream = new Button();
        btnChat = new Button();
        lblFps = new Label();
        lblClients = new Label();

        // Info bar controls
        lblPartnerIdCaption = new Label();
        lblPartnerId = new Label();
        lblPasswordCaption = new Label();
        lblPassword = new Label();

        rtbLogs = new RichTextBox();
        lblStatus = new Label();
        panelTop = new Panel();
        panelInfoBar = new Panel();
        btnCopyId = new Button();
        btnCopyPass = new Button();
        toolTipCopy = new ToolTip();

        panelTop.SuspendLayout();
        panelInfoBar.SuspendLayout();
        SuspendLayout();

        // ─── lblRelayIp ───
        lblRelayIp.AutoSize = true;
        lblRelayIp.Font = new Font("Segoe UI Semibold", 9.5F);
        lblRelayIp.Location = new Point(14, 24);
        lblRelayIp.Name = "lblRelayIp";
        lblRelayIp.Size = new Size(55, 21);
        lblRelayIp.TabIndex = 8;
        lblRelayIp.Text = "Relay:";

        // ─── txtRelayIp ───
        txtRelayIp.Font = new Font("Segoe UI", 9.5F);
        txtRelayIp.Location = new Point(66, 19);
        txtRelayIp.Name = "txtRelayIp";
        txtRelayIp.Size = new Size(105, 29);
        txtRelayIp.TabIndex = 7;
        txtRelayIp.Text = "127.0.0.1";

        // ─── lblPort ───
        lblPort.AutoSize = true;
        lblPort.Font = new Font("Segoe UI Semibold", 9.5F);
        lblPort.Location = new Point(180, 24);
        lblPort.Name = "lblPort";
        lblPort.Size = new Size(44, 21);
        lblPort.TabIndex = 6;
        lblPort.Text = "Port:";

        // ─── txtPort ───
        txtPort.Font = new Font("Segoe UI", 9.5F);
        txtPort.Location = new Point(222, 19);
        txtPort.Name = "txtPort";
        txtPort.Size = new Size(70, 29);
        txtPort.TabIndex = 5;
        txtPort.Text = "9000";

        // ─── btnStart ───
        btnStart.BackColor = Color.FromArgb(46, 139, 87);
        btnStart.Cursor = Cursors.Hand;
        btnStart.FlatStyle = FlatStyle.Flat;
        btnStart.Font = new Font("Segoe UI Semibold", 9F);
        btnStart.ForeColor = Color.White;
        btnStart.Location = new Point(310, 16);
        btnStart.Name = "btnStart";
        btnStart.Size = new Size(90, 43);
        btnStart.TabIndex = 4;
        btnStart.Text = "▶ Start";
        btnStart.UseVisualStyleBackColor = false;
        btnStart.Click += BtnStart_Click;

        // ─── btnStop ───
        btnStop.BackColor = Color.FromArgb(178, 34, 34);
        btnStop.Cursor = Cursors.Hand;
        btnStop.Enabled = false;
        btnStop.FlatStyle = FlatStyle.Flat;
        btnStop.Font = new Font("Segoe UI Semibold", 9F);
        btnStop.ForeColor = Color.White;
        btnStop.Location = new Point(410, 16);
        btnStop.Name = "btnStop";
        btnStop.Size = new Size(90, 43);
        btnStop.TabIndex = 3;
        btnStop.Text = "■ Stop";
        btnStop.UseVisualStyleBackColor = false;
        btnStop.Click += BtnStop_Click;

        // ─── btnToggleStream ───
        btnToggleStream.BackColor = Color.FromArgb(70, 130, 180);
        btnToggleStream.Cursor = Cursors.Hand;
        btnToggleStream.Enabled = false;
        btnToggleStream.FlatStyle = FlatStyle.Flat;
        btnToggleStream.Font = new Font("Segoe UI Semibold", 9F);
        btnToggleStream.ForeColor = Color.White;
        btnToggleStream.Location = new Point(515, 16);
        btnToggleStream.Name = "btnToggleStream";
        btnToggleStream.Size = new Size(100, 43);
        btnToggleStream.TabIndex = 1;
        btnToggleStream.Text = "📺 Stream";
        btnToggleStream.UseVisualStyleBackColor = false;
        btnToggleStream.Click += BtnToggleStream_Click;

        // ─── btnChat ───
        btnChat.BackColor = Color.FromArgb(90, 80, 160);
        btnChat.Cursor = Cursors.Hand;
        btnChat.Enabled = false;
        btnChat.FlatStyle = FlatStyle.Flat;
        btnChat.Font = new Font("Segoe UI Semibold", 9F);
        btnChat.ForeColor = Color.White;
        btnChat.Location = new Point(625, 16);
        btnChat.Name = "btnChat";
        btnChat.Size = new Size(90, 43);
        btnChat.TabIndex = 9;
        btnChat.Text = "💬 Chat";
        btnChat.UseVisualStyleBackColor = false;
        btnChat.Click += BtnChat_Click;

        // ─── lblClients ───
        lblClients.AutoSize = true;
        lblClients.Font = new Font("Segoe UI", 9F);
        lblClients.ForeColor = Color.Gray;
        lblClients.Location = new Point(635, 13);
        lblClients.Name = "lblClients";
        lblClients.Size = new Size(68, 20);
        lblClients.TabIndex = 2;
        lblClients.Text = "Clients: 0";

        // ─── lblFps ───
        lblFps.AutoSize = true;
        lblFps.Font = new Font("Segoe UI", 8F);
        lblFps.ForeColor = Color.FromArgb(100, 180, 255);
        lblFps.Location = new Point(635, 37);
        lblFps.Name = "lblFps";
        lblFps.Size = new Size(0, 19);
        lblFps.TabIndex = 0;

        // ─── panelInfoBar ─── (displays Partner ID + Password)
        panelInfoBar.BackColor = Color.FromArgb(35, 55, 75);
        panelInfoBar.Controls.Add(btnCopyPass);
        panelInfoBar.Controls.Add(lblPassword);
        panelInfoBar.Controls.Add(lblPasswordCaption);
        panelInfoBar.Controls.Add(btnCopyId);
        panelInfoBar.Controls.Add(lblPartnerId);
        panelInfoBar.Controls.Add(lblPartnerIdCaption);
        panelInfoBar.Dock = DockStyle.Top;
        panelInfoBar.Location = new Point(0, 73);
        panelInfoBar.Name = "panelInfoBar";
        panelInfoBar.Padding = new Padding(14, 8, 14, 8);
        panelInfoBar.Size = new Size(754, 45);
        panelInfoBar.TabIndex = 1;
        panelInfoBar.Visible = false;

        // ─── lblPartnerIdCaption ───
        lblPartnerIdCaption.AutoSize = true;
        lblPartnerIdCaption.Dock = DockStyle.Left;
        lblPartnerIdCaption.Font = new Font("Segoe UI Semibold", 10F);
        lblPartnerIdCaption.ForeColor = Color.FromArgb(180, 200, 220);
        lblPartnerIdCaption.Location = new Point(14, 8);
        lblPartnerIdCaption.Name = "lblPartnerIdCaption";
        lblPartnerIdCaption.Size = new Size(95, 23);
        lblPartnerIdCaption.TabIndex = 3;
        lblPartnerIdCaption.Text = "🆔 Partner ID:";
        lblPartnerIdCaption.TextAlign = ContentAlignment.MiddleLeft;

        // ─── lblPartnerId ───
        lblPartnerId.AutoSize = true;
        lblPartnerId.Dock = DockStyle.Left;
        lblPartnerId.Font = new Font("Cascadia Code", 13F, FontStyle.Bold);
        lblPartnerId.ForeColor = Color.FromArgb(100, 255, 180);
        lblPartnerId.Location = new Point(109, 8);
        lblPartnerId.Name = "lblPartnerId";
        lblPartnerId.Padding = new Padding(6, 0, 20, 0);
        lblPartnerId.Size = new Size(130, 30);
        lblPartnerId.TabIndex = 2;
        lblPartnerId.Text = "--- --- ---";
        lblPartnerId.TextAlign = ContentAlignment.MiddleLeft;

        // ─── lblPasswordCaption ───
        lblPasswordCaption.AutoSize = true;
        lblPasswordCaption.Dock = DockStyle.Left;
        lblPasswordCaption.Font = new Font("Segoe UI Semibold", 10F);
        lblPasswordCaption.ForeColor = Color.FromArgb(180, 200, 220);
        lblPasswordCaption.Location = new Point(239, 8);
        lblPasswordCaption.Name = "lblPasswordCaption";
        lblPasswordCaption.Size = new Size(90, 23);
        lblPasswordCaption.TabIndex = 1;
        lblPasswordCaption.Text = "🔑 Password:";
        lblPasswordCaption.TextAlign = ContentAlignment.MiddleLeft;

        // ─── lblPassword ───
        lblPassword.AutoSize = true;
        lblPassword.Dock = DockStyle.Left;
        lblPassword.Font = new Font("Cascadia Code", 13F, FontStyle.Bold);
        lblPassword.ForeColor = Color.FromArgb(255, 200, 80);
        lblPassword.Location = new Point(329, 8);
        lblPassword.Name = "lblPassword";
        lblPassword.Padding = new Padding(6, 0, 0, 0);
        lblPassword.Size = new Size(60, 30);
        lblPassword.TabIndex = 0;
        lblPassword.Text = "----";
        lblPassword.TextAlign = ContentAlignment.MiddleLeft;

        // ─── btnCopyId ───
        btnCopyId.BackColor = Color.FromArgb(50, 70, 95);
        btnCopyId.Cursor = Cursors.Hand;
        btnCopyId.Dock = DockStyle.Left;
        btnCopyId.FlatAppearance.BorderSize = 0;
        btnCopyId.FlatStyle = FlatStyle.Flat;
        btnCopyId.Font = new Font("Segoe UI", 9F);
        btnCopyId.ForeColor = Color.FromArgb(180, 200, 220);
        btnCopyId.Location = new Point(239, 8);
        btnCopyId.Name = "btnCopyId";
        btnCopyId.Size = new Size(32, 29);
        btnCopyId.TabIndex = 10;
        btnCopyId.Text = "📋";
        btnCopyId.UseVisualStyleBackColor = false;
        btnCopyId.Click += BtnCopyId_Click;

        // ─── btnCopyPass ───
        btnCopyPass.BackColor = Color.FromArgb(50, 70, 95);
        btnCopyPass.Cursor = Cursors.Hand;
        btnCopyPass.Dock = DockStyle.Left;
        btnCopyPass.FlatAppearance.BorderSize = 0;
        btnCopyPass.FlatStyle = FlatStyle.Flat;
        btnCopyPass.Font = new Font("Segoe UI", 9F);
        btnCopyPass.ForeColor = Color.FromArgb(180, 200, 220);
        btnCopyPass.Location = new Point(420, 8);
        btnCopyPass.Name = "btnCopyPass";
        btnCopyPass.Size = new Size(32, 29);
        btnCopyPass.TabIndex = 11;
        btnCopyPass.Text = "📋";
        btnCopyPass.UseVisualStyleBackColor = false;
        btnCopyPass.Click += BtnCopyPass_Click;

        // ─── toolTipCopy ───
        toolTipCopy.AutoPopDelay = 2000;
        toolTipCopy.InitialDelay = 0;
        toolTipCopy.ReshowDelay = 100;

        // ─── panelTop ───
        panelTop.Controls.Add(lblFps);
        panelTop.Controls.Add(btnChat);
        panelTop.Controls.Add(btnToggleStream);
        panelTop.Controls.Add(lblClients);
        panelTop.Controls.Add(btnStop);
        panelTop.Controls.Add(btnStart);
        panelTop.Controls.Add(txtPort);
        panelTop.Controls.Add(lblPort);
        panelTop.Controls.Add(txtRelayIp);
        panelTop.Controls.Add(lblRelayIp);
        panelTop.Dock = DockStyle.Top;
        panelTop.Location = new Point(0, 0);
        panelTop.Name = "panelTop";
        panelTop.Padding = new Padding(11, 13, 11, 7);
        panelTop.Size = new Size(754, 73);
        panelTop.TabIndex = 2;

        // ─── rtbLogs ───
        rtbLogs.BackColor = Color.FromArgb(25, 25, 30);
        rtbLogs.BorderStyle = BorderStyle.None;
        rtbLogs.Dock = DockStyle.Fill;
        rtbLogs.Font = new Font("Cascadia Code", 9.5F);
        rtbLogs.ForeColor = Color.FromArgb(200, 210, 220);
        rtbLogs.Location = new Point(0, 118);
        rtbLogs.Name = "rtbLogs";
        rtbLogs.ReadOnly = true;
        rtbLogs.Size = new Size(754, 487);
        rtbLogs.TabIndex = 0;
        rtbLogs.Text = "";

        // ─── lblStatus ───
        lblStatus.BackColor = Color.FromArgb(35, 35, 42);
        lblStatus.Dock = DockStyle.Bottom;
        lblStatus.Font = new Font("Segoe UI", 9F);
        lblStatus.ForeColor = Color.IndianRed;
        lblStatus.Location = new Point(0, 605);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(754, 35);
        lblStatus.TabIndex = 3;
        lblStatus.Text = "  ● Stopped";
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;

        // ─── Form1 ───
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 36);
        ClientSize = new Size(754, 640);
        Controls.Add(rtbLogs);
        Controls.Add(panelInfoBar);
        Controls.Add(panelTop);
        Controls.Add(lblStatus);
        Font = new Font("Segoe UI", 9F);
        ForeColor = Color.White;
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "SharpView — Server";

        panelTop.ResumeLayout(false);
        panelTop.PerformLayout();
        panelInfoBar.ResumeLayout(false);
        panelInfoBar.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private Label lblRelayIp;
    private TextBox txtRelayIp;
    private Label lblPort;
    private TextBox txtPort;
    private Button btnStart;
    private Button btnStop;
    private Button btnToggleStream;
    private Button btnChat;
    private Label lblPartnerIdCaption;
    private Label lblPartnerId;
    private Label lblPasswordCaption;
    private Label lblPassword;
    private Label lblFps;
    private RichTextBox rtbLogs;
    private Label lblStatus;
    private Label lblClients;
    private Panel panelTop;
    private Panel panelInfoBar;
    private Button btnCopyId;
    private Button btnCopyPass;
    private ToolTip toolTipCopy;
}
