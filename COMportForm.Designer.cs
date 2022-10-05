
namespace COMport
{
    partial class COMportForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(COMportForm));
            this.CommsTextBox = new System.Windows.Forms.TextBox();
            this.BaudComboBox = new System.Windows.Forms.ComboBox();
            this.COMportComboBox = new System.Windows.Forms.ComboBox();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.DelayTextBox = new System.Windows.Forms.TextBox();
            this.QuickTextBox3 = new System.Windows.Forms.TextBox();
            this.QuickTextBox2 = new System.Windows.Forms.TextBox();
            this.QuickTextBox1 = new System.Windows.Forms.TextBox();
            this.onEnterComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.HalfDuplexCheckBox = new System.Windows.Forms.CheckBox();
            this.clearSreenButton = new System.Windows.Forms.Button();
            this.VersionComboBox = new System.Windows.Forms.ComboBox();
            this.SerialPort = new System.IO.Ports.SerialPort(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
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
            this.CommsTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.CommsTextBox.MaxLength = 250000;
            this.CommsTextBox.Multiline = true;
            this.CommsTextBox.Name = "CommsTextBox";
            this.CommsTextBox.ReadOnly = true;
            this.CommsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.CommsTextBox.ShortcutsEnabled = false;
            this.CommsTextBox.Size = new System.Drawing.Size(922, 554);
            this.CommsTextBox.TabIndex = 3;
            this.CommsTextBox.TextChanged += new System.EventHandler(this.CommsTextBox_TextChanged);
            this.CommsTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CommsTextBox_KeyPress);
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
            this.BaudComboBox.Location = new System.Drawing.Point(339, 6);
            this.BaudComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.BaudComboBox.Name = "BaudComboBox";
            this.BaudComboBox.Size = new System.Drawing.Size(72, 21);
            this.BaudComboBox.TabIndex = 1;
            this.BaudComboBox.Text = "115200";
            // 
            // COMportComboBox
            // 
            this.COMportComboBox.FormattingEnabled = true;
            this.COMportComboBox.Location = new System.Drawing.Point(275, 6);
            this.COMportComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.COMportComboBox.Name = "COMportComboBox";
            this.COMportComboBox.Size = new System.Drawing.Size(59, 21);
            this.COMportComboBox.TabIndex = 0;
            this.COMportComboBox.DropDown += new System.EventHandler(this.COMportComboBox_DropDown);
            // 
            // ConnectButton
            // 
            this.ConnectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ConnectButton.Location = new System.Drawing.Point(4, 4);
            this.ConnectButton.Margin = new System.Windows.Forms.Padding(2);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(97, 24);
            this.ConnectButton.TabIndex = 2;
            this.ConnectButton.Text = "Connect";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.DelayTextBox);
            this.panel1.Controls.Add(this.QuickTextBox3);
            this.panel1.Controls.Add(this.QuickTextBox2);
            this.panel1.Controls.Add(this.QuickTextBox1);
            this.panel1.Controls.Add(this.onEnterComboBox);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.HalfDuplexCheckBox);
            this.panel1.Controls.Add(this.clearSreenButton);
            this.panel1.Controls.Add(this.VersionComboBox);
            this.panel1.Controls.Add(this.BaudComboBox);
            this.panel1.Controls.Add(this.COMportComboBox);
            this.panel1.Controls.Add(this.ConnectButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(922, 34);
            this.panel1.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(554, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Char Delay:";
            // 
            // DelayTextBox
            // 
            this.DelayTextBox.Location = new System.Drawing.Point(619, 8);
            this.DelayTextBox.Name = "DelayTextBox";
            this.DelayTextBox.Size = new System.Drawing.Size(52, 20);
            this.DelayTextBox.TabIndex = 11;
            this.DelayTextBox.Text = "0";
            this.DelayTextBox.TextChanged += new System.EventHandler(this.NLDelayTextBox_TextChanged);
            // 
            // QuickTextBox3
            // 
            this.QuickTextBox3.Location = new System.Drawing.Point(1221, 8);
            this.QuickTextBox3.Name = "QuickTextBox3";
            this.QuickTextBox3.Size = new System.Drawing.Size(140, 20);
            this.QuickTextBox3.TabIndex = 10;
            this.QuickTextBox3.DoubleClick += new System.EventHandler(this.QuickTextBox3_DoubleClick);
            // 
            // QuickTextBox2
            // 
            this.QuickTextBox2.Location = new System.Drawing.Point(1075, 8);
            this.QuickTextBox2.Name = "QuickTextBox2";
            this.QuickTextBox2.Size = new System.Drawing.Size(140, 20);
            this.QuickTextBox2.TabIndex = 9;
            this.QuickTextBox2.DoubleClick += new System.EventHandler(this.QuickTextBox2_DoubleClick);
            // 
            // QuickTextBox1
            // 
            this.QuickTextBox1.Location = new System.Drawing.Point(929, 8);
            this.QuickTextBox1.Name = "QuickTextBox1";
            this.QuickTextBox1.Size = new System.Drawing.Size(140, 20);
            this.QuickTextBox1.TabIndex = 8;
            this.QuickTextBox1.DoubleClick += new System.EventHandler(this.QuickTextBox1_DoubleClick);
            // 
            // onEnterComboBox
            // 
            this.onEnterComboBox.FormattingEnabled = true;
            this.onEnterComboBox.Items.AddRange(new object[] {
            "0x0A",
            "0x0D",
            "0x1B"});
            this.onEnterComboBox.Location = new System.Drawing.Point(762, 8);
            this.onEnterComboBox.Name = "onEnterComboBox";
            this.onEnterComboBox.Size = new System.Drawing.Size(56, 21);
            this.onEnterComboBox.TabIndex = 7;
            this.onEnterComboBox.TextChanged += new System.EventHandler(this.onEnterComboBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(680, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Tx on ENTER:";
            // 
            // HalfDuplexCheckBox
            // 
            this.HalfDuplexCheckBox.AutoSize = true;
            this.HalfDuplexCheckBox.Location = new System.Drawing.Point(415, 11);
            this.HalfDuplexCheckBox.Name = "HalfDuplexCheckBox";
            this.HalfDuplexCheckBox.Size = new System.Drawing.Size(79, 17);
            this.HalfDuplexCheckBox.TabIndex = 5;
            this.HalfDuplexCheckBox.Text = "Half-duplex";
            this.HalfDuplexCheckBox.UseVisualStyleBackColor = true;
            // 
            // clearSreenButton
            // 
            this.clearSreenButton.Location = new System.Drawing.Point(823, 6);
            this.clearSreenButton.Margin = new System.Windows.Forms.Padding(2);
            this.clearSreenButton.Name = "clearSreenButton";
            this.clearSreenButton.Size = new System.Drawing.Size(91, 24);
            this.clearSreenButton.TabIndex = 4;
            this.clearSreenButton.Text = "Clear screen";
            this.clearSreenButton.UseVisualStyleBackColor = true;
            this.clearSreenButton.Click += new System.EventHandler(this.clearSreenButton_Click);
            // 
            // VersionComboBox
            // 
            this.VersionComboBox.FormattingEnabled = true;
            this.VersionComboBox.Location = new System.Drawing.Point(103, 6);
            this.VersionComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.VersionComboBox.Name = "VersionComboBox";
            this.VersionComboBox.Size = new System.Drawing.Size(168, 21);
            this.VersionComboBox.TabIndex = 3;
            this.VersionComboBox.Text = "Unknown";
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
            this.panel2.Location = new System.Drawing.Point(0, 34);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(922, 554);
            this.panel2.TabIndex = 5;
            // 
            // COMportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.ClientSize = new System.Drawing.Size(922, 588);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "COMportForm";
            this.Text = "COM port";
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

        private System.Windows.Forms.TextBox CommsTextBox;
        private System.Windows.Forms.ComboBox BaudComboBox;
        private System.Windows.Forms.ComboBox COMportComboBox;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.Panel panel1;
        private System.IO.Ports.SerialPort SerialPort;
        private System.Windows.Forms.ComboBox VersionComboBox;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button clearSreenButton;
        private System.Windows.Forms.CheckBox HalfDuplexCheckBox;
        private System.Windows.Forms.ComboBox onEnterComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox QuickTextBox3;
        private System.Windows.Forms.TextBox QuickTextBox2;
        private System.Windows.Forms.TextBox QuickTextBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox DelayTextBox;
    }
}

