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
// 11.10.2022-MD Correct Execute buttons - oops, 5 to 19 all indexed button 3!
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
        const int REPEAT_DISABLED = 0;

        bool RepeatCommandPrimmed = false;
        int RepeatCommand = REPEAT_DISABLED;

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
            CommandLine8TextBox.Text = setText(8, ManualEnter8CheckBox);
            CommandLine9TextBox.Text = setText(9, ManualEnter9CheckBox);
            CommandLine10TextBox.Text = setText(10, ManualEnter10CheckBox);
            CommandLine11TextBox.Text = setText(11, ManualEnter11CheckBox);
            CommandLine12TextBox.Text = setText(12, ManualEnter12CheckBox);
            CommandLine13TextBox.Text = setText(13, ManualEnter13CheckBox);
            CommandLine14TextBox.Text = setText(14, ManualEnter14CheckBox);
            CommandLine15TextBox.Text = setText(15, ManualEnter15CheckBox);
            CommandLine16TextBox.Text = setText(16, ManualEnter16CheckBox);
            CommandLine17TextBox.Text = setText(17, ManualEnter17CheckBox);
            CommandLine18TextBox.Text = setText(18, ManualEnter18CheckBox);
            CommandLine19TextBox.Text = setText(19, ManualEnter19CheckBox);
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
            saveEnvironment(8, CommandLine8TextBox.Text, ManualEnter8CheckBox);
            saveEnvironment(9, CommandLine9TextBox.Text, ManualEnter9CheckBox);
            saveEnvironment(10, CommandLine10TextBox.Text, ManualEnter10CheckBox);
            saveEnvironment(11, CommandLine11TextBox.Text, ManualEnter11CheckBox);
            saveEnvironment(12, CommandLine12TextBox.Text, ManualEnter12CheckBox);
            saveEnvironment(13, CommandLine13TextBox.Text, ManualEnter13CheckBox);
            saveEnvironment(14, CommandLine14TextBox.Text, ManualEnter14CheckBox);
            saveEnvironment(15, CommandLine15TextBox.Text, ManualEnter15CheckBox);
            saveEnvironment(16, CommandLine16TextBox.Text, ManualEnter16CheckBox);
            saveEnvironment(17, CommandLine17TextBox.Text, ManualEnter17CheckBox);
            saveEnvironment(18, CommandLine18TextBox.Text, ManualEnter18CheckBox);
            saveEnvironment(19, CommandLine19TextBox.Text, ManualEnter19CheckBox);
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
            executeCommand(1);
            MaySetNewRepeatCommand(1);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (2).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText2Button_Click(object sender, EventArgs e)
        {
            executeCommand(2);
            MaySetNewRepeatCommand(2);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (3).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText3Button_Click(object sender, EventArgs e)
        {
            executeCommand(3);
            MaySetNewRepeatCommand(3);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (4).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText4Button_Click(object sender, EventArgs e)
        {
            executeCommand(4);
            MaySetNewRepeatCommand(4);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (5).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText5Button_Click(object sender, EventArgs e)
        {
            executeCommand(5);
            MaySetNewRepeatCommand(5);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (6).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText6Button_Click(object sender, EventArgs e)
        {
            executeCommand(6);
            MaySetNewRepeatCommand(6);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (7).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText7Button_Click(object sender, EventArgs e)
        {
            executeCommand(7);
            MaySetNewRepeatCommand(7);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (8).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText8Button_Click(object sender, EventArgs e)
        {
            executeCommand(8);
            MaySetNewRepeatCommand(8);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (9).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText9Button_Click(object sender, EventArgs e)
        {
            executeCommand(9);
            MaySetNewRepeatCommand(9);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (10).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText10Button_Click(object sender, EventArgs e)
        {
            executeCommand(10);
            MaySetNewRepeatCommand(10);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (11).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText11Button_Click(object sender, EventArgs e)
        {
            executeCommand(11);
            MaySetNewRepeatCommand(11);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (12).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText12Button_Click(object sender, EventArgs e)
        {
            executeCommand(12);
            MaySetNewRepeatCommand(12);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (13).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText13Button_Click(object sender, EventArgs e)
        {
            executeCommand(13);
            MaySetNewRepeatCommand(13);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (14).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText14Button_Click(object sender, EventArgs e)
        {
            executeCommand(14);
            MaySetNewRepeatCommand(14);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (15).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText15Button_Click(object sender, EventArgs e)
        {
            executeCommand(15);
            MaySetNewRepeatCommand(15);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (16).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText16Button_Click(object sender, EventArgs e)
        {
            executeCommand(16);
            MaySetNewRepeatCommand(16);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (17).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText17Button_Click(object sender, EventArgs e)
        {
            executeCommand(17);
            MaySetNewRepeatCommand(17);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (18).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText18Button_Click(object sender, EventArgs e)
        {
            executeCommand(18);
            MaySetNewRepeatCommand(18);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute the quick text (19).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExeText19Button_Click(object sender, EventArgs e)
        {
            executeCommand(19);
            MaySetNewRepeatCommand(19);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Start button for repeat next command every N seconds . . .
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartRepeatButton_Click(object sender, EventArgs e)
        {
            RepeatCommandPrimmed = true;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Stop button for repeat commands.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopRepeatButton_Click(object sender, EventArgs e)
        {
            RepeatCommandPrimmed = false;
            untickExecuteButton();
            RepeatCommandTimer.Enabled = false;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Start the repeat command if primed to do so . . .
        /// </summary>
        /// <param name="command"></param>
        private void MaySetNewRepeatCommand( int command )
        {
            if( RepeatCommandPrimmed )
            {
                RepeatCommandPrimmed = false;
                untickExecuteButton();
                RepeatCommandTimer.Enabled = false;
                RepeatCommand = command;
                RepeatCommandTimer.Enabled = true;
                tickExecuteButton();
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Untick the last repeat command (put it back to grey).
        /// </summary>
        private void untickExecuteButton()
        {
            if( RepeatCommand != REPEAT_DISABLED )
            {
                setColourExecuteButton(RepeatCommand, false);
                RepeatCommand = REPEAT_DISABLED;
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Tick the repeat command (colour it green).
        /// </summary>
        private void tickExecuteButton()
        {
            setColourExecuteButton(RepeatCommand, true);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Set the background colour of command to green (true) or button grey (false).
        /// </summary>
        /// <param name="command"></param>
        /// <param name="state"></param>
        private void setColourExecuteButton( int command, bool state )
        {
            Color ofButton = (state ? System.Drawing.Color.LimeGreen : System.Drawing.SystemColors.ControlLight);

            switch ( command )
            {
                case 1: ExeText1Button.BackColor = ofButton; break;
                case 2: ExeText2Button.BackColor = ofButton; break;
                case 3: ExeText3Button.BackColor = ofButton; break;
                case 4: ExeText4Button.BackColor = ofButton; break;
                case 5: ExeText5Button.BackColor = ofButton; break;
                case 6: ExeText6Button.BackColor = ofButton; break;
                case 7: ExeText7Button.BackColor = ofButton; break;
                case 8: ExeText8Button.BackColor = ofButton; break;
                case 9: ExeText9Button.BackColor = ofButton; break;
                case 10: ExeText10Button.BackColor = ofButton; break;
                case 11: ExeText11Button.BackColor = ofButton; break;
                case 12: ExeText12Button.BackColor = ofButton; break;
                case 13: ExeText13Button.BackColor = ofButton; break;
                case 14: ExeText14Button.BackColor = ofButton; break;
                case 15: ExeText15Button.BackColor = ofButton; break;
                case 16: ExeText16Button.BackColor = ofButton; break;
                case 17: ExeText17Button.BackColor = ofButton; break;
                case 18: ExeText18Button.BackColor = ofButton; break;
                case 19: ExeText19Button.BackColor = ofButton; break;
                //
                default: break;
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Repeat last command timer has gone off.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RepeatCommandTimer_Tick(object sender, EventArgs e)
        {
            executeCommand(RepeatCommand);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute a command in one of the prepared text boxes.
        /// </summary>
        /// <param name="command"></param>
        private void executeCommand( int command )
        {
            switch( command )
            {
                case 1: Program.comportform.sendLinesToKeyboard(CommandLine1TextBox.Text + (ManualEnter1CheckBox.Checked ? "" : "\\n")); break;
                case 2: Program.comportform.sendLinesToKeyboard(CommandLine2TextBox.Text + (ManualEnter2CheckBox.Checked ? "" : "\\n")); break;
                case 3: Program.comportform.sendLinesToKeyboard(CommandLine3TextBox.Text + (ManualEnter3CheckBox.Checked ? "" : "\\n")); break;
                case 4: Program.comportform.sendLinesToKeyboard(CommandLine4TextBox.Text + (ManualEnter4CheckBox.Checked ? "" : "\\n")); break;
                case 5: Program.comportform.sendLinesToKeyboard(CommandLine5TextBox.Text + (ManualEnter5CheckBox.Checked ? "" : "\\n")); break;
                case 6: Program.comportform.sendLinesToKeyboard(CommandLine6TextBox.Text + (ManualEnter6CheckBox.Checked ? "" : "\\n")); break;
                case 7: Program.comportform.sendLinesToKeyboard(CommandLine7TextBox.Text + (ManualEnter7CheckBox.Checked ? "" : "\\n")); break;
                case 8: Program.comportform.sendLinesToKeyboard(CommandLine8TextBox.Text + (ManualEnter8CheckBox.Checked ? "" : "\\n")); break;
                case 9: Program.comportform.sendLinesToKeyboard(CommandLine9TextBox.Text + (ManualEnter9CheckBox.Checked ? "" : "\\n")); break;
                case 10: Program.comportform.sendLinesToKeyboard(CommandLine10TextBox.Text + (ManualEnter10CheckBox.Checked ? "" : "\\n")); break;
                case 11: Program.comportform.sendLinesToKeyboard(CommandLine11TextBox.Text + (ManualEnter11CheckBox.Checked ? "" : "\\n")); break;
                case 12: Program.comportform.sendLinesToKeyboard(CommandLine12TextBox.Text + (ManualEnter12CheckBox.Checked ? "" : "\\n")); break;
                case 13: Program.comportform.sendLinesToKeyboard(CommandLine13TextBox.Text + (ManualEnter13CheckBox.Checked ? "" : "\\n")); break;
                case 14: Program.comportform.sendLinesToKeyboard(CommandLine14TextBox.Text + (ManualEnter14CheckBox.Checked ? "" : "\\n")); break;
                case 15: Program.comportform.sendLinesToKeyboard(CommandLine15TextBox.Text + (ManualEnter15CheckBox.Checked ? "" : "\\n")); break;
                case 16: Program.comportform.sendLinesToKeyboard(CommandLine16TextBox.Text + (ManualEnter16CheckBox.Checked ? "" : "\\n")); break;
                case 17: Program.comportform.sendLinesToKeyboard(CommandLine17TextBox.Text + (ManualEnter17CheckBox.Checked ? "" : "\\n")); break;
                case 18: Program.comportform.sendLinesToKeyboard(CommandLine18TextBox.Text + (ManualEnter18CheckBox.Checked ? "" : "\\n")); break;
                case 19: Program.comportform.sendLinesToKeyboard(CommandLine19TextBox.Text + (ManualEnter19CheckBox.Checked ? "" : "\\n")); break;
                //
                default: RepeatCommandTimer.Enabled = false; break;
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Set the repeat period.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RepeatEveryTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                RepeatCommandTimer.Interval = Convert.ToInt32(RepeatEveryTextBox.Text) * 1000; // Timer uses milliseconds.
            }
            catch
            {
                RepeatCommandTimer.Interval = 1000; // Just use a reasonable value.
            }
        }
    }
}
