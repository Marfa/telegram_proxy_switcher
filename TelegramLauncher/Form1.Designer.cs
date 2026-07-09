namespace TelegramLauncher;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.pickProxyButton = new System.Windows.Forms.Button();
        this.stopPickButton = new System.Windows.Forms.Button();
        this.connectButton = new System.Windows.Forms.Button();
        this.aboutButton = new System.Windows.Forms.Button();
        this.statusLabel = new System.Windows.Forms.Label();
        this.detailsTextBox = new System.Windows.Forms.TextBox();
        this.stickTimer = new System.Windows.Forms.Timer(this.components);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(23, 33, 43);
        this.ClientSize = new System.Drawing.Size(360, 110);
        this.Controls.Add(this.detailsTextBox);
        this.Controls.Add(this.statusLabel);
        this.Controls.Add(this.aboutButton);
        this.Controls.Add(this.connectButton);
        this.Controls.Add(this.stopPickButton);
        this.Controls.Add(this.pickProxyButton);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.Name = "Form1";
        this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
        this.Text = "TG Proxy";
        this.TopMost = false;
        this.Load += new System.EventHandler(this.Form1_Load);
        this.pickProxyButton.BackColor = System.Drawing.Color.FromArgb(41, 148, 230);
        this.pickProxyButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(58, 157, 232);
        this.pickProxyButton.FlatAppearance.BorderSize = 1;
        this.pickProxyButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.pickProxyButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        this.pickProxyButton.ForeColor = System.Drawing.Color.White;
        this.pickProxyButton.Location = new System.Drawing.Point(16, 20);
        this.pickProxyButton.Name = "pickProxyButton";
        this.pickProxyButton.Size = new System.Drawing.Size(287, 48);
        this.pickProxyButton.TabIndex = 0;
        this.pickProxyButton.Text = "Подобрать прокси";
        this.pickProxyButton.UseVisualStyleBackColor = false;
        this.pickProxyButton.Click += new System.EventHandler(this.pickProxyButton_Click);
        this.stopPickButton.BackColor = System.Drawing.Color.FromArgb(201, 80, 80);
        this.stopPickButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(220, 96, 96);
        this.stopPickButton.FlatAppearance.BorderSize = 1;
        this.stopPickButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.stopPickButton.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        this.stopPickButton.ForeColor = System.Drawing.Color.White;
        this.stopPickButton.Location = new System.Drawing.Point(16, 122);
        this.stopPickButton.Name = "stopPickButton";
        this.stopPickButton.Size = new System.Drawing.Size(287, 30);
        this.stopPickButton.TabIndex = 4;
        this.stopPickButton.Text = "Остановить подбор";
        this.stopPickButton.UseVisualStyleBackColor = false;
        this.stopPickButton.Visible = false;
        this.stopPickButton.Click += new System.EventHandler(this.stopPickButton_Click);
        this.connectButton.BackColor = System.Drawing.Color.FromArgb(84, 174, 92);
        this.connectButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(102, 188, 110);
        this.connectButton.FlatAppearance.BorderSize = 1;
        this.connectButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.connectButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.connectButton.ForeColor = System.Drawing.Color.White;
        this.connectButton.Location = new System.Drawing.Point(16, 136);
        this.connectButton.Name = "connectButton";
        this.connectButton.Size = new System.Drawing.Size(327, 38);
        this.connectButton.TabIndex = 2;
        this.connectButton.Text = "Подключиться к прокси";
        this.connectButton.UseVisualStyleBackColor = false;
        this.connectButton.Visible = false;
        this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
        this.aboutButton.BackColor = System.Drawing.Color.FromArgb(49, 64, 81);
        this.aboutButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(64, 80, 99);
        this.aboutButton.FlatAppearance.BorderSize = 1;
        this.aboutButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.aboutButton.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        this.aboutButton.ForeColor = System.Drawing.Color.FromArgb(224, 226, 233);
        this.aboutButton.Location = new System.Drawing.Point(313, 29);
        this.aboutButton.Name = "aboutButton";
        this.aboutButton.Size = new System.Drawing.Size(30, 30);
        this.aboutButton.TabIndex = 5;
        this.aboutButton.Text = "i";
        this.aboutButton.UseVisualStyleBackColor = false;
        this.aboutButton.Click += new System.EventHandler(this.aboutButton_Click);
        this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.statusLabel.ForeColor = System.Drawing.Color.FromArgb(224, 226, 233);
        this.statusLabel.Location = new System.Drawing.Point(16, 74);
        this.statusLabel.Name = "statusLabel";
        this.statusLabel.Size = new System.Drawing.Size(327, 20);
        this.statusLabel.TabIndex = 1;
        this.statusLabel.Text = "Запустите Telegram Desktop.";
        this.statusLabel.Visible = false;
        this.detailsTextBox.BackColor = System.Drawing.Color.FromArgb(27, 39, 51);
        this.detailsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.detailsTextBox.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.detailsTextBox.ForeColor = System.Drawing.Color.FromArgb(224, 226, 233);
        this.detailsTextBox.Location = new System.Drawing.Point(16, 100);
        this.detailsTextBox.Multiline = true;
        this.detailsTextBox.Name = "detailsTextBox";
        this.detailsTextBox.ReadOnly = true;
        this.detailsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.detailsTextBox.Size = new System.Drawing.Size(327, 56);
        this.detailsTextBox.TabIndex = 3;
        this.detailsTextBox.Visible = false;
        this.stickTimer.Interval = 400;
        this.stickTimer.Tick += new System.EventHandler(this.stickTimer_Tick);
    }

    #endregion

    private Button pickProxyButton;
    private Button stopPickButton;
    private Button connectButton;
    private Button aboutButton;
    private Label statusLabel;
    private TextBox detailsTextBox;
    private System.Windows.Forms.Timer stickTimer;
}
