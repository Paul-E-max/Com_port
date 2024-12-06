
namespace COMport
{
    partial class COMport
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(COMport));
            this.NoNewlineTimer = new System.Windows.Forms.Timer(this.components);
            this.CommsTextBox = new System.Windows.Forms.TextBox();
            this.BaudComboBox = new System.Windows.Forms.ComboBox();
            this.COMportComboBox = new System.Windows.Forms.ComboBox();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.TimeStampCheckBox = new System.Windows.Forms.CheckBox();
            this.HexOutputCheckBox = new System.Windows.Forms.CheckBox();
            this.ToolTipsCheckBox = new System.Windows.Forms.CheckBox();
            this.QuickTextComboBox = new System.Windows.Forms.ComboBox();
            this.StartLogButton = new System.Windows.Forms.Button();
            this.onEnterComboBox = new System.Windows.Forms.ComboBox();
            this.TxOnEnterLabel = new System.Windows.Forms.Label();
            this.HalfDuplexCheckBox = new System.Windows.Forms.CheckBox();
            this.clearSreenButton = new System.Windows.Forms.Button();
            this.VersionComboBox = new System.Windows.Forms.ComboBox();
            this.SerialPort = new System.IO.Ports.SerialPort(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.toolTips = new System.Windows.Forms.ToolTip(this.components);
            this.TXcharacterTimer = new System.Windows.Forms.Timer(this.components);
            this.D2XX_RXcharacterTimer = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // NoNewlineTimer
            // 
            this.NoNewlineTimer.Tick += new System.EventHandler(this.NoNewlineTimer_Tick);
            // 
            // CommsTextBox
            // 
            this.CommsTextBox.AllowDrop = true;
            this.CommsTextBox.BackColor = System.Drawing.SystemColors.MenuText;
            this.CommsTextBox.CausesValidation = false;
            this.CommsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CommsTextBox.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CommsTextBox.ForeColor = System.Drawing.SystemColors.Window;
            this.CommsTextBox.Location = new System.Drawing.Point(0, 0);
            this.CommsTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.CommsTextBox.MaxLength = 250000;
            this.CommsTextBox.Multiline = true;
            this.CommsTextBox.Name = "CommsTextBox";
            this.CommsTextBox.ReadOnly = true;
            this.CommsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.CommsTextBox.ShortcutsEnabled = false;
            this.CommsTextBox.Size = new System.Drawing.Size(1231, 681);
            this.CommsTextBox.TabIndex = 3;
            this.CommsTextBox.TextChanged += new System.EventHandler(this.CommsTextBox_TextChanged);
            this.CommsTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CommsTextBox_KeyPress);
            this.CommsTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CommsTextBox_KeyUp);
            // 
            // BaudComboBox
            // 
            this.BaudComboBox.FormattingEnabled = true;
            this.BaudComboBox.Items.AddRange(new object[] {
            "300",
            "600",
            "900",
            "1200",
            "2400",
            "4800",
            "9600",
            "19200",
            "38400",
            "57600",
            "115200"});
            this.BaudComboBox.Location = new System.Drawing.Point(424, 7);
            this.BaudComboBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.BaudComboBox.Name = "BaudComboBox";
            this.BaudComboBox.Size = new System.Drawing.Size(95, 24);
            this.BaudComboBox.TabIndex = 1;
            this.BaudComboBox.Text = "115200";
            this.toolTips.SetToolTip(this.BaudComboBox, "Select baudrate");
            // 
            // COMportComboBox
            // 
            this.COMportComboBox.FormattingEnabled = true;
            this.COMportComboBox.Location = new System.Drawing.Point(323, 7);
            this.COMportComboBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.COMportComboBox.Name = "COMportComboBox";
            this.COMportComboBox.Size = new System.Drawing.Size(93, 24);
            this.COMportComboBox.TabIndex = 0;
            this.toolTips.SetToolTip(this.COMportComboBox, "Select COM port to connect to");
            this.COMportComboBox.DropDown += new System.EventHandler(this.COMportComboBox_DropDown);
            this.COMportComboBox.SelectedIndexChanged += new System.EventHandler(this.COMportComboBox_SelectedIndexChanged);
            this.COMportComboBox.Leave += new System.EventHandler(this.COMportComboBox_Leave);
            // 
            // ConnectButton
            // 
            this.ConnectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ConnectButton.Location = new System.Drawing.Point(5, 6);
            this.ConnectButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(129, 30);
            this.ConnectButton.TabIndex = 2;
            this.ConnectButton.Text = "Connect";
            this.toolTips.SetToolTip(this.ConnectButton, "Connect/Disconnect the device");
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.panel1.Controls.Add(this.TimeStampCheckBox);
            this.panel1.Controls.Add(this.HexOutputCheckBox);
            this.panel1.Controls.Add(this.ToolTipsCheckBox);
            this.panel1.Controls.Add(this.QuickTextComboBox);
            this.panel1.Controls.Add(this.StartLogButton);
            this.panel1.Controls.Add(this.onEnterComboBox);
            this.panel1.Controls.Add(this.TxOnEnterLabel);
            this.panel1.Controls.Add(this.HalfDuplexCheckBox);
            this.panel1.Controls.Add(this.clearSreenButton);
            this.panel1.Controls.Add(this.VersionComboBox);
            this.panel1.Controls.Add(this.BaudComboBox);
            this.panel1.Controls.Add(this.COMportComboBox);
            this.panel1.Controls.Add(this.ConnectButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1231, 42);
            this.panel1.TabIndex = 4;
            // 
            // TimeStampCheckBox
            // 
            this.TimeStampCheckBox.AutoSize = true;
            this.TimeStampCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TimeStampCheckBox.Location = new System.Drawing.Point(1131, 12);
            this.TimeStampCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.TimeStampCheckBox.Name = "TimeStampCheckBox";
            this.TimeStampCheckBox.Size = new System.Drawing.Size(52, 21);
            this.TimeStampCheckBox.TabIndex = 18;
            this.TimeStampCheckBox.Text = "Tm";
            this.toolTips.SetToolTip(this.TimeStampCheckBox, "Enable/Disable timestamp output reporting the whole seconds (0-60) and millisecon" +
        "d count");
            this.TimeStampCheckBox.UseVisualStyleBackColor = true;
            this.TimeStampCheckBox.Click += new System.EventHandler(this.SelectConsoleFollowing_Click);
            // 
            // HexOutputCheckBox
            // 
            this.HexOutputCheckBox.AutoSize = true;
            this.HexOutputCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HexOutputCheckBox.Location = new System.Drawing.Point(1072, 12);
            this.HexOutputCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.HexOutputCheckBox.Name = "HexOutputCheckBox";
            this.HexOutputCheckBox.Size = new System.Drawing.Size(55, 21);
            this.HexOutputCheckBox.TabIndex = 17;
            this.HexOutputCheckBox.Text = "hex";
            this.toolTips.SetToolTip(this.HexOutputCheckBox, "Enable/Disable hexadecimal output of the received data");
            this.HexOutputCheckBox.UseVisualStyleBackColor = true;
            this.HexOutputCheckBox.Click += new System.EventHandler(this.SelectConsoleFollowing_Click);
            // 
            // ToolTipsCheckBox
            // 
            this.ToolTipsCheckBox.AutoSize = true;
            this.ToolTipsCheckBox.Checked = true;
            this.ToolTipsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToolTipsCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ToolTipsCheckBox.Location = new System.Drawing.Point(1190, 12);
            this.ToolTipsCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.ToolTipsCheckBox.Name = "ToolTipsCheckBox";
            this.ToolTipsCheckBox.Size = new System.Drawing.Size(39, 21);
            this.ToolTipsCheckBox.TabIndex = 16;
            this.ToolTipsCheckBox.Text = "?";
            this.toolTips.SetToolTip(this.ToolTipsCheckBox, "Enable/Disable the help comments");
            this.ToolTipsCheckBox.UseVisualStyleBackColor = true;
            this.ToolTipsCheckBox.CheckedChanged += new System.EventHandler(this.ToolTipsCheckBox_CheckedChanged);
            this.ToolTipsCheckBox.Click += new System.EventHandler(this.SelectConsoleFollowing_Click);
            // 
            // QuickTextComboBox
            // 
            this.QuickTextComboBox.FormattingEnabled = true;
            this.QuickTextComboBox.Location = new System.Drawing.Point(709, 7);
            this.QuickTextComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.QuickTextComboBox.MaxDropDownItems = 16;
            this.QuickTextComboBox.Name = "QuickTextComboBox";
            this.QuickTextComboBox.Size = new System.Drawing.Size(123, 24);
            this.QuickTextComboBox.TabIndex = 15;
            this.QuickTextComboBox.Text = "QUICK";
            this.toolTips.SetToolTip(this.QuickTextComboBox, "Select a sheet of QUICK Text commands");
            this.QuickTextComboBox.DropDown += new System.EventHandler(this.QuickTextComboBox_DropDown);
            this.QuickTextComboBox.SelectedIndexChanged += new System.EventHandler(this.QuickTextComboBox_SelectedIndexChanged);
            this.QuickTextComboBox.DropDownClosed += new System.EventHandler(this.QuickTextComboBox_DropDownClosed);
            this.QuickTextComboBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.QuickTextComboBox_KeyDown);
            this.QuickTextComboBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.QuickTextComboBox_KeyUp);
            this.QuickTextComboBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.QuickTextComboBox_MouseUp);
            // 
            // StartLogButton
            // 
            this.StartLogButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StartLogButton.Location = new System.Drawing.Point(607, 6);
            this.StartLogButton.Margin = new System.Windows.Forms.Padding(4);
            this.StartLogButton.Name = "StartLogButton";
            this.StartLogButton.Size = new System.Drawing.Size(93, 30);
            this.StartLogButton.TabIndex = 14;
            this.StartLogButton.Text = "Start log";
            this.toolTips.SetToolTip(this.StartLogButton, "Start/Stop logging the communications to a file");
            this.StartLogButton.UseVisualStyleBackColor = true;
            this.StartLogButton.Click += new System.EventHandler(this.StartLogButton_Click);
            // 
            // onEnterComboBox
            // 
            this.onEnterComboBox.FormattingEnabled = true;
            this.onEnterComboBox.Items.AddRange(new object[] {
            "0x0A",
            "0x0D",
            "0x1B"});
            this.onEnterComboBox.Location = new System.Drawing.Point(900, 7);
            this.onEnterComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.onEnterComboBox.Name = "onEnterComboBox";
            this.onEnterComboBox.Size = new System.Drawing.Size(61, 24);
            this.onEnterComboBox.TabIndex = 7;
            this.toolTips.SetToolTip(this.onEnterComboBox, "Typically, use 0x0D for carriage return or 0x0A for line feed");
            this.onEnterComboBox.TextChanged += new System.EventHandler(this.onEnterComboBox_TextChanged);
            // 
            // TxOnEnterLabel
            // 
            this.TxOnEnterLabel.AutoSize = true;
            this.TxOnEnterLabel.Location = new System.Drawing.Point(842, 13);
            this.TxOnEnterLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.TxOnEnterLabel.Name = "TxOnEnterLabel";
            this.TxOnEnterLabel.Size = new System.Drawing.Size(47, 16);
            this.TxOnEnterLabel.TabIndex = 6;
            this.TxOnEnterLabel.Text = "enter =";
            // 
            // HalfDuplexCheckBox
            // 
            this.HalfDuplexCheckBox.AutoSize = true;
            this.HalfDuplexCheckBox.Location = new System.Drawing.Point(526, 12);
            this.HalfDuplexCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.HalfDuplexCheckBox.Name = "HalfDuplexCheckBox";
            this.HalfDuplexCheckBox.Size = new System.Drawing.Size(60, 20);
            this.HalfDuplexCheckBox.TabIndex = 5;
            this.HalfDuplexCheckBox.Text = "Echo";
            this.toolTips.SetToolTip(this.HalfDuplexCheckBox, "Tick if device requires Tx character to be echo\'d locally,\r\nas in half-duplexed. " +
        " Use right-click to enable auto-new-line\r\non display.");
            this.HalfDuplexCheckBox.UseVisualStyleBackColor = true;
            this.HalfDuplexCheckBox.Click += new System.EventHandler(this.SelectConsoleFollowing_Click);
            this.HalfDuplexCheckBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.HalfDuplexCheckBox_MouseDown);
            // 
            // clearSreenButton
            // 
            this.clearSreenButton.Location = new System.Drawing.Point(968, 6);
            this.clearSreenButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.clearSreenButton.Name = "clearSreenButton";
            this.clearSreenButton.Size = new System.Drawing.Size(97, 30);
            this.clearSreenButton.TabIndex = 4;
            this.clearSreenButton.Text = "Clear SCR";
            this.toolTips.SetToolTip(this.clearSreenButton, "Clear the console screen");
            this.clearSreenButton.UseVisualStyleBackColor = true;
            this.clearSreenButton.Click += new System.EventHandler(this.clearSreenButton_Click);
            // 
            // VersionComboBox
            // 
            this.VersionComboBox.FormattingEnabled = true;
            this.VersionComboBox.Location = new System.Drawing.Point(139, 7);
            this.VersionComboBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.VersionComboBox.Name = "VersionComboBox";
            this.VersionComboBox.Size = new System.Drawing.Size(177, 24);
            this.VersionComboBox.TabIndex = 3;
            this.VersionComboBox.Text = "Unknown";
            this.toolTips.SetToolTip(this.VersionComboBox, "Select the project");
            this.VersionComboBox.SelectedIndexChanged += new System.EventHandler(this.VersionComboBox_SelectedIndexChanged);
            // 
            // SerialPort
            // 
            this.SerialPort.DtrEnable = true;
            this.SerialPort.ReadTimeout = 100;
            this.SerialPort.RtsEnable = true;
            this.SerialPort.WriteTimeout = 100;
            this.SerialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.SerialPort_DataReceived);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.panel2.Controls.Add(this.CommsTextBox);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 42);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1231, 681);
            this.panel2.TabIndex = 5;
            // 
            // toolTips
            // 
            this.toolTips.AutoPopDelay = 30000;
            this.toolTips.InitialDelay = 500;
            this.toolTips.IsBalloon = true;
            this.toolTips.ReshowDelay = 100;
            this.toolTips.ShowAlways = true;
            // 
            // TXcharacterTimer
            // 
            this.TXcharacterTimer.Interval = 10;
            this.TXcharacterTimer.Tick += new System.EventHandler(this.TXcharacterTimer_Tick);
            // 
            // D2XX_RXcharacterTimer
            // 
            this.D2XX_RXcharacterTimer.Interval = 50;
            this.D2XX_RXcharacterTimer.Tick += new System.EventHandler(this.D2XX_RXcharacterTimer_Tick);
            // 
            // COMport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.ClientSize = new System.Drawing.Size(1231, 723);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "COMport";
            this.Text = "COM port";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.COMportForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.COMportForm_FormClosed);
            this.Load += new System.EventHandler(this.COMportForm_Load);
            this.Shown += new System.EventHandler(this.COMportForm_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ComboBox BaudComboBox;
        private System.Windows.Forms.ComboBox COMportComboBox;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.Panel panel1;
        private System.IO.Ports.SerialPort SerialPort;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button clearSreenButton;
        private System.Windows.Forms.CheckBox HalfDuplexCheckBox;
        private System.Windows.Forms.Label TxOnEnterLabel;
        private System.Windows.Forms.Button StartLogButton;
        private System.Windows.Forms.ComboBox onEnterComboBox;
        public System.Windows.Forms.TextBox CommsTextBox;
        public System.Windows.Forms.ComboBox VersionComboBox;
        private System.Windows.Forms.ComboBox QuickTextComboBox;
        private System.Windows.Forms.ToolTip toolTips;
        private System.Windows.Forms.CheckBox ToolTipsCheckBox;
        private System.Windows.Forms.Timer TXcharacterTimer;
        private System.Windows.Forms.Timer D2XX_RXcharacterTimer;
        private System.Windows.Forms.Timer NoNewlineTimer;
        private System.Windows.Forms.CheckBox TimeStampCheckBox;
        private System.Windows.Forms.CheckBox HexOutputCheckBox;
    }
}

