// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
// @File:       QuickTextMenu.cs
// @Project:    DISCOVER_COM_port
// @Author:     Foster & Freeman Ltd - Michael Dodd
// @Created:    05.10.2022
//
// @Brief:      Simple COM port communications device for use with DISCOVER.
//
// @Tools:      Visual Studio 2019, C#
//
// @Revision:
// 05.10.2022-MD Initial version.
//
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace COMport
{
    public partial class QuickTextMenu : Form
    {
        public QuickTextMenu()
        {
            InitializeComponent();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Load the form and update the entries.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuickTextMenu_Load(object sender, EventArgs e)
        {
            string temp = COMportForm.environmentRead("COMport_delay", "X");
            if (temp != "X") CharDelayTextBox.Text = temp;
            //
            CommandLine1TextBox.Text = setText(1, ManualEnter1CheckBox);
            CommandLine2TextBox.Text = setText(2, ManualEnter2CheckBox);
            CommandLine3TextBox.Text = setText(3, ManualEnter3CheckBox);
            CommandLine4TextBox.Text = setText(4, ManualEnter4CheckBox);
            CommandLine5TextBox.Text = setText(5, ManualEnter5CheckBox);
            CommandLine6TextBox.Text = setText(6, ManualEnter6CheckBox);
            CommandLine7TextBox.Text = setText(7, ManualEnter7CheckBox);
            //
            CharDelayTextBox.Text = COMportForm.InterCharDelay.ToString();
            NLDelayTextBox.Text = COMportForm.InterLineDelay.ToString();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Read from environment the previous command line stored.
        /// </summary>
        /// <param name="idx">One of seven different entries to update</param>
        /// <param name="checkbox">If read item doesn't end in "\n", tick this as "No Enter" required</param>
        /// <returns></returns>
        private string setText( int idx, CheckBox checkbox )
        {
            string toRead = COMportForm.environmentRead(("COMport_QuickText" + idx), "");

            if( toRead.EndsWith("\\n") )
            {
                toRead = toRead.Substring(0, toRead.Length - 2);
            }
            else
            {
                if( toRead.Length > 0 ) checkbox.Checked = true;
            }
            return toRead;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Save the current text configuration
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, EventArgs e)
        {
            saveEnvironment(1, CommandLine1TextBox.Text, ManualEnter1CheckBox);
            saveEnvironment(2, CommandLine2TextBox.Text, ManualEnter2CheckBox);
            saveEnvironment(3, CommandLine3TextBox.Text, ManualEnter3CheckBox);
            saveEnvironment(4, CommandLine4TextBox.Text, ManualEnter4CheckBox);
            saveEnvironment(5, CommandLine5TextBox.Text, ManualEnter5CheckBox);
            saveEnvironment(6, CommandLine6TextBox.Text, ManualEnter6CheckBox);
            saveEnvironment(7, CommandLine7TextBox.Text, ManualEnter7CheckBox);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Save an environment variable with trailing "\n" if necessary.
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="textbox"></param>
        /// <param name="checkbox"></param>
        private void saveEnvironment( int idx, string textbox, CheckBox checkbox )
        {
            if (!checkbox.Checked && (textbox.Length > 0)) textbox += "\\n";
            COMportForm.environmentWrite(("COMport_QuickText" + idx), textbox );
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Exit the menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CharDelayTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                COMportForm.InterCharDelay = Convert.ToInt32(CharDelayTextBox.Text);
            }
            catch
            {
                COMportForm.InterCharDelay = 0;
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NLDelaytextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                COMportForm.InterLineDelay = Convert.ToInt32(NLDelayTextBox.Text);
            }
            catch
            {
                COMportForm.InterLineDelay = 0;
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (1).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ExeText1Button_Click(object sender, EventArgs e)
        {
            Program.comportform.sendLinesToKeyboard(CommandLine1TextBox.Text + (ManualEnter1CheckBox.Checked ? "" : "\\n"));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (2).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText2Button_Click(object sender, EventArgs e)
        {
            Program.comportform.sendLinesToKeyboard(CommandLine2TextBox.Text + (ManualEnter2CheckBox.Checked ? "" : "\\n"));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (3).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText3Button_Click(object sender, EventArgs e)
        {
            Program.comportform.sendLinesToKeyboard(CommandLine3TextBox.Text + (ManualEnter3CheckBox.Checked ? "" : "\\n"));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (4).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText4Button_Click(object sender, EventArgs e)
        {
            Program.comportform.sendLinesToKeyboard(CommandLine4TextBox.Text + (ManualEnter4CheckBox.Checked ? "" : "\\n"));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (5).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText5Button_Click(object sender, EventArgs e)
        {
            Program.comportform.sendLinesToKeyboard(CommandLine5TextBox.Text + (ManualEnter5CheckBox.Checked ? "" : "\\n"));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (6).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText6Button_Click(object sender, EventArgs e)
        {
            Program.comportform.sendLinesToKeyboard(CommandLine6TextBox.Text + (ManualEnter6CheckBox.Checked ? "" : "\\n"));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (7).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText7Button_Click(object sender, EventArgs e)
        {
            Program.comportform.sendLinesToKeyboard(CommandLine7TextBox.Text + (ManualEnter7CheckBox.Checked ? "" : "\\n"));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (8).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText8Button_Click(object sender, EventArgs e)
        {
            Program.comportform.sendLinesToKeyboard(CommandLine8TextBox.Text + (ManualEnter8CheckBox.Checked ? "" : "\\n"));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (9).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText9Button_Click(object sender, EventArgs e)
        {
            Program.comportform.sendLinesToKeyboard(CommandLine9TextBox.Text + (ManualEnter9CheckBox.Checked ? "" : "\\n"));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (10).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText10Button_Click(object sender, EventArgs e)
        {
            Program.comportform.sendLinesToKeyboard(CommandLine10TextBox.Text + (ManualEnter10CheckBox.Checked ? "" : "\\n"));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (11).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText11Button_Click(object sender, EventArgs e)
        {
            Program.comportform.sendLinesToKeyboard(CommandLine11TextBox.Text + (ManualEnter11CheckBox.Checked ? "" : "\\n"));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (12).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText12Button_Click(object sender, EventArgs e)
        {
            Program.comportform.sendLinesToKeyboard(CommandLine12TextBox.Text + (ManualEnter12CheckBox.Checked ? "" : "\\n"));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (13).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText13Button_Click(object sender, EventArgs e)
        {
            Program.comportform.sendLinesToKeyboard(CommandLine13TextBox.Text + (ManualEnter13CheckBox.Checked ? "" : "\\n"));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (14).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText14Button_Click(object sender, EventArgs e)
        {
            Program.comportform.sendLinesToKeyboard(CommandLine14TextBox.Text + (ManualEnter14CheckBox.Checked ? "" : "\\n"));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (15).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText15Button_Click(object sender, EventArgs e)
        {
            Program.comportform.sendLinesToKeyboard(CommandLine15TextBox.Text + (ManualEnter15CheckBox.Checked ? "" : "\\n"));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (16).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText16Button_Click(object sender, EventArgs e)
        {
            Program.comportform.sendLinesToKeyboard(CommandLine16TextBox.Text + (ManualEnter16CheckBox.Checked ? "" : "\\n"));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (17).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText17Button_Click(object sender, EventArgs e)
        {
            Program.comportform.sendLinesToKeyboard(CommandLine17TextBox.Text + (ManualEnter17CheckBox.Checked ? "" : "\\n"));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (18).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText18Button_Click(object sender, EventArgs e)
        {
            Program.comportform.sendLinesToKeyboard(CommandLine18TextBox.Text + (ManualEnter18CheckBox.Checked ? "" : "\\n"));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (19).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText19Button_Click(object sender, EventArgs e)
        {
            Program.comportform.sendLinesToKeyboard(CommandLine19TextBox.Text + (ManualEnter19CheckBox.Checked ? "" : "\\n"));
        }
    }
}
