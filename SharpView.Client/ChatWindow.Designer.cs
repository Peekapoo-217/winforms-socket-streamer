namespace SharpView.Client;

partial class ChatWindow
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
        rtbChatHistory = new RichTextBox();
        lblTypingIndicator = new Label();
        lblQuickConnectError = new Label();
        txtMessage = new TextBox();
        btnSend = new Button();
        panelInput = new Panel();
        ctxMenuChat = new ContextMenuStrip();
        menuCopyText = new ToolStripMenuItem();
        menuQuickConnect = new ToolStripMenuItem();

        panelInput.SuspendLayout();
        SuspendLayout();

        // ─── rtbChatHistory ───
        rtbChatHistory.BackColor = Color.FromArgb(25, 27, 33);
        rtbChatHistory.BorderStyle = BorderStyle.None;
        rtbChatHistory.Dock = DockStyle.Fill;
        rtbChatHistory.Font = new Font("Segoe UI", 9.5F);
        rtbChatHistory.ForeColor = Color.FromArgb(210, 215, 225);
        rtbChatHistory.Location = new Point(0, 0);
        rtbChatHistory.Name = "rtbChatHistory";
        rtbChatHistory.ReadOnly = true;
        rtbChatHistory.ScrollBars = RichTextBoxScrollBars.Vertical;
        rtbChatHistory.Size = new Size(420, 380);
        rtbChatHistory.TabIndex = 0;
        rtbChatHistory.Text = "";
        rtbChatHistory.ContextMenuStrip = ctxMenuChat;

        // ─── ctxMenuChat ───
        ctxMenuChat.Items.AddRange(new ToolStripItem[] { menuCopyText, menuQuickConnect });
        ctxMenuChat.Name = "ctxMenuChat";
        ctxMenuChat.Size = new Size(260, 52);

        // ─── menuCopyText ───
        menuCopyText.Name = "menuCopyText";
        menuCopyText.Size = new Size(259, 24);
        menuCopyText.Text = ChatStrings.MenuCopyText;
        menuCopyText.Click += MenuCopyText_Click;

        // ─── menuQuickConnect ───
        menuQuickConnect.Name = "menuQuickConnect";
        menuQuickConnect.Size = new Size(259, 24);
        menuQuickConnect.Text = ChatStrings.MenuQuickConnect;
        menuQuickConnect.Click += MenuQuickConnect_Click;

        // ─── lblTypingIndicator ───
        lblTypingIndicator.BackColor = Color.FromArgb(30, 33, 42);
        lblTypingIndicator.Dock = DockStyle.Bottom;
        lblTypingIndicator.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
        lblTypingIndicator.ForeColor = Color.FromArgb(140, 160, 200);
        lblTypingIndicator.Location = new Point(0, 360);
        lblTypingIndicator.Name = "lblTypingIndicator";
        lblTypingIndicator.Padding = new Padding(10, 0, 0, 0);
        lblTypingIndicator.Size = new Size(420, 22);
        lblTypingIndicator.TabIndex = 2;
        lblTypingIndicator.Text = "";
        lblTypingIndicator.Visible = false;

        // ─── panelInput ───
        panelInput.BackColor = Color.FromArgb(35, 38, 48);
        panelInput.Controls.Add(btnSend);
        panelInput.Controls.Add(txtMessage);
        panelInput.Dock = DockStyle.Bottom;
        panelInput.Location = new Point(0, 382);
        panelInput.Name = "panelInput";
        panelInput.Padding = new Padding(8, 6, 8, 6);
        panelInput.Size = new Size(420, 50);
        panelInput.TabIndex = 1;

        // ─── lblQuickConnectError ───
        lblQuickConnectError.BackColor = Color.FromArgb(30, 33, 42);
        lblQuickConnectError.Dock = DockStyle.Bottom;
        lblQuickConnectError.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
        lblQuickConnectError.ForeColor = Color.FromArgb(255, 100, 100);
        lblQuickConnectError.Location = new Point(0, 380);
        lblQuickConnectError.Name = "lblQuickConnectError";
        lblQuickConnectError.Padding = new Padding(10, 0, 0, 0);
        lblQuickConnectError.Size = new Size(420, 22);
        lblQuickConnectError.TabIndex = 3;
        lblQuickConnectError.Text = "";
        lblQuickConnectError.Visible = false;

        // ─── txtMessage ───
        txtMessage.BackColor = Color.FromArgb(45, 48, 58);
        txtMessage.BorderStyle = BorderStyle.FixedSingle;
        txtMessage.Dock = DockStyle.Fill;
        txtMessage.Font = new Font("Segoe UI", 10F);
        txtMessage.ForeColor = Color.FromArgb(220, 225, 235);
        txtMessage.Location = new Point(8, 6);
        txtMessage.Name = "txtMessage";
        txtMessage.Size = new Size(324, 30);
        txtMessage.TabIndex = 0;

        // ─── btnSend ───
        btnSend.BackColor = Color.FromArgb(46, 139, 87);
        btnSend.Cursor = Cursors.Hand;
        btnSend.Dock = DockStyle.Right;
        btnSend.FlatAppearance.BorderSize = 0;
        btnSend.FlatStyle = FlatStyle.Flat;
        btnSend.Font = new Font("Segoe UI Semibold", 9.5F);
        btnSend.ForeColor = Color.White;
        btnSend.Location = new Point(340, 6);
        btnSend.Name = "btnSend";
        btnSend.Size = new Size(72, 38);
        btnSend.TabIndex = 1;
        btnSend.Text = ChatStrings.SendButtonText;
        btnSend.UseVisualStyleBackColor = false;

        // ─── ChatWindow ───
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(25, 27, 33);
        ClientSize = new Size(420, 455);
        Controls.Add(rtbChatHistory);
        Controls.Add(lblTypingIndicator);
        Controls.Add(lblQuickConnectError);
        Controls.Add(panelInput);
        Font = new Font("Segoe UI", 9F);
        ForeColor = Color.White;
        FormBorderStyle = FormBorderStyle.SizableToolWindow;
        MinimumSize = new Size(320, 300);
        Name = "ChatWindow";
        StartPosition = FormStartPosition.CenterParent;
        Text = ChatStrings.WindowTitle;
        TopMost = true;

        panelInput.ResumeLayout(false);
        panelInput.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private RichTextBox rtbChatHistory;
    private Label lblTypingIndicator;
    private Label lblQuickConnectError;
    private TextBox txtMessage;
    private Button btnSend;
    private Panel panelInput;
    private ContextMenuStrip ctxMenuChat;
    private ToolStripMenuItem menuCopyText;
    private ToolStripMenuItem menuQuickConnect;
}
