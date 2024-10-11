
namespace COMport
{
    partial class EnterLabel
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
            this.NewLabelTextBox = new System.Windows.Forms.TextBox();
            this.OKbutton = new System.Windows.Forms.Button();
            this.CancelChangesButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // NewLabelTextBox
            // 
            this.NewLabelTextBox.Location = new System.Drawing.Point(16, 15);
            this.NewLabelTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.NewLabelTextBox.Name = "NewLabelTextBox";
            this.NewLabelTextBox.Size = new System.Drawing.Size(159, 22);
            this.NewLabelTextBox.TabIndex = 1;
            // 
            // OKbutton
            // 
            this.OKbutton.Location = new System.Drawing.Point(183, 12);
            this.OKbutton.Margin = new System.Windows.Forms.Padding(4);
            this.OKbutton.Name = "OKbutton";
            this.OKbutton.Size = new System.Drawing.Size(67, 28);
            this.OKbutton.TabIndex = 2;
            this.OKbutton.Text = "OK";
            this.OKbutton.UseVisualStyleBackColor = true;
            this.OKbutton.Click += new System.EventHandler(this.OKbutton_Click);
            // 
            // CancelChangesButton
            // 
            this.CancelChangesButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelChangesButton.Location = new System.Drawing.Point(258, 12);
            this.CancelChangesButton.Margin = new System.Windows.Forms.Padding(4);
            this.CancelChangesButton.Name = "CancelChangesButton";
            this.CancelChangesButton.Size = new System.Drawing.Size(67, 28);
            this.CancelChangesButton.TabIndex = 3;
            this.CancelChangesButton.Text = "Cancel";
            this.CancelChangesButton.UseVisualStyleBackColor = true;
            // 
            // EnterLabel
            // 
            this.AcceptButton = this.OKbutton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(338, 49);
            this.Controls.Add(this.CancelChangesButton);
            this.Controls.Add(this.OKbutton);
            this.Controls.Add(this.NewLabelTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "EnterLabel";
            this.Text = "Enter new label here";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox NewLabelTextBox;
        private System.Windows.Forms.Button OKbutton;
        private System.Windows.Forms.Button CancelChangesButton;
    }
}