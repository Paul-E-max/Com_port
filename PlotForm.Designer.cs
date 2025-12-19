namespace COMport
{
    partial class PlotForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlotForm));
            this.DataChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.ToolPanel = new System.Windows.Forms.Panel();
            this.MaxPointsLabel = new System.Windows.Forms.Label();
            this.MaxPointsComboBox = new System.Windows.Forms.ComboBox();
            this.PauseButton = new System.Windows.Forms.Button();
            this.ClearButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.DataChart)).BeginInit();
            this.ToolPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // DataChart
            // 
            this.DataChart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.DataChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DataChart.Location = new System.Drawing.Point(0, 34);
            this.DataChart.Name = "DataChart";
            this.DataChart.Size = new System.Drawing.Size(684, 377);
            this.DataChart.TabIndex = 0;
            // 
            // ToolPanel
            // 
            this.ToolPanel.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.ToolPanel.Controls.Add(this.MaxPointsLabel);
            this.ToolPanel.Controls.Add(this.MaxPointsComboBox);
            this.ToolPanel.Controls.Add(this.PauseButton);
            this.ToolPanel.Controls.Add(this.ClearButton);
            this.ToolPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ToolPanel.Location = new System.Drawing.Point(0, 0);
            this.ToolPanel.Name = "ToolPanel";
            this.ToolPanel.Size = new System.Drawing.Size(684, 34);
            this.ToolPanel.TabIndex = 1;
            // 
            // MaxPointsLabel
            // 
            this.MaxPointsLabel.AutoSize = true;
            this.MaxPointsLabel.Location = new System.Drawing.Point(176, 10);
            this.MaxPointsLabel.Name = "MaxPointsLabel";
            this.MaxPointsLabel.Size = new System.Drawing.Size(38, 13);
            this.MaxPointsLabel.TabIndex = 3;
            this.MaxPointsLabel.Text = "Points:";
            // 
            // MaxPointsComboBox
            // 
            this.MaxPointsComboBox.FormattingEnabled = true;
            this.MaxPointsComboBox.Items.AddRange(new object[] {
            "50",
            "100",
            "200",
            "500",
            "1000"});
            this.MaxPointsComboBox.Location = new System.Drawing.Point(220, 6);
            this.MaxPointsComboBox.Name = "MaxPointsComboBox";
            this.MaxPointsComboBox.Size = new System.Drawing.Size(60, 21);
            this.MaxPointsComboBox.TabIndex = 2;
            this.MaxPointsComboBox.Text = "100";
            this.MaxPointsComboBox.SelectedIndexChanged += new System.EventHandler(this.MaxPointsComboBox_SelectedIndexChanged);
            // 
            // PauseButton
            // 
            this.PauseButton.Location = new System.Drawing.Point(93, 5);
            this.PauseButton.Name = "PauseButton";
            this.PauseButton.Size = new System.Drawing.Size(75, 24);
            this.PauseButton.TabIndex = 1;
            this.PauseButton.Text = "Pause";
            this.PauseButton.UseVisualStyleBackColor = true;
            this.PauseButton.Click += new System.EventHandler(this.PauseButton_Click);
            // 
            // ClearButton
            // 
            this.ClearButton.Location = new System.Drawing.Point(12, 5);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(75, 24);
            this.ClearButton.TabIndex = 0;
            this.ClearButton.Text = "Clear";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // PlotForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 411);
            this.Controls.Add(this.DataChart);
            this.Controls.Add(this.ToolPanel);
            this.MinimumSize = new System.Drawing.Size(400, 300);
            this.Name = "PlotForm";
            this.Text = "Data Plot";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PlotForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.DataChart)).EndInit();
            this.ToolPanel.ResumeLayout(false);
            this.ToolPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart DataChart;
        private System.Windows.Forms.Panel ToolPanel;
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.Button PauseButton;
        private System.Windows.Forms.ComboBox MaxPointsComboBox;
        private System.Windows.Forms.Label MaxPointsLabel;
    }
}
