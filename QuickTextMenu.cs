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
// 24.02.2023-MD V1.01.09 - Correction to CR from quick text (V1.01.08 didn't use the NL delay on quick text).
// 21.02.2023-MD V1.01.08 - Timer now takes decimal interval rather than just whole seconds.
// 15.02.2023-MD Quick Text now used EnterKey from COMportForm instead of a default '\n'.
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
        const int FORM_BAR = 39;
        const int QUICK_TEXT_WIDTH = 534; // In the GUI, this is set to 550 . . . why the difference ?
        const int QUICK_TEXT_LESS_HEIGHT = 627 - FORM_BAR;
        const int QUICK_TEXT_MORE_HEIGHT = 691 - FORM_BAR;
        const int MINIMUM_PERIOD = 100;
        const string DEFAULT_LABEL = "Command";

        string loadFilename;
        bool RepeatCommandPrimmed = false;
        bool CheckEnvironmentVariables = true; // Assume that environment variables can be used if necessary.
        int RepeatCommand = REPEAT_DISABLED;

        public QuickTextMenu(string filename)
        {
            InitializeComponent();
            loadFilename = filename;
            this.Text = filename;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Load the form and update the entries.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuickTextMenu_Load(object sender, EventArgs e)
        {
            string[] lines = new string[0];

            if (File.Exists(loadFilename))
            {
                lines = File.ReadAllLines(loadFilename);
                if (lines.Length > 0) CheckEnvironmentVariables = false;
            }
            setText(lines, 1, ButtonExeText1, CommandLine1TextBox, ManualEnter1CheckBox);
            setText(lines, 2, ButtonExeText2, CommandLine2TextBox, ManualEnter2CheckBox);
            setText(lines, 3, ButtonExeText3, CommandLine3TextBox, ManualEnter3CheckBox);
            setText(lines, 4, ButtonExeText4, CommandLine4TextBox, ManualEnter4CheckBox);
            setText(lines, 5, ButtonExeText5, CommandLine5TextBox, ManualEnter5CheckBox);
            setText(lines, 6, ButtonExeText6, CommandLine6TextBox, ManualEnter6CheckBox);
            setText(lines, 7, ButtonExeText7, CommandLine7TextBox, ManualEnter7CheckBox);
            setText(lines, 8, ButtonExeText8, CommandLine8TextBox, ManualEnter8CheckBox);
            setText(lines, 9, ButtonExeText9, CommandLine9TextBox, ManualEnter9CheckBox);
            setText(lines, 10, ButtonExeText10, CommandLine10TextBox, ManualEnter10CheckBox);
            setText(lines, 11, ButtonExeText11, CommandLine11TextBox, ManualEnter11CheckBox);
            setText(lines, 12, ButtonExeText12, CommandLine12TextBox, ManualEnter12CheckBox);
            setText(lines, 13, ButtonExeText13, CommandLine13TextBox, ManualEnter13CheckBox);
            setText(lines, 14, ButtonExeText14, CommandLine14TextBox, ManualEnter14CheckBox);
            setText(lines, 15, ButtonExeText15, CommandLine15TextBox, ManualEnter15CheckBox);
            setText(lines, 16, ButtonExeText16, CommandLine16TextBox, ManualEnter16CheckBox);
            setText(lines, 17, ButtonExeText17, CommandLine17TextBox, ManualEnter17CheckBox);
            setText(lines, 18, ButtonExeText18, CommandLine18TextBox, ManualEnter18CheckBox);
            setText(lines, 19, ButtonExeText19, CommandLine19TextBox, ManualEnter19CheckBox);
            //
            CharDelayTextBox.Text = COMportForm.InterCharDelay.ToString();
            NLDelayTextBox.Text = COMportForm.InterLineDelay.ToString();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Read from lines found in Quick Text menu file.
        /// </summary>
        /// <param name="index">One of seven different entries to update</param>
        /// <param name="checkbox">If read item doesn't end in "\n", tick this as "No Enter" required</param>
        /// <returns></returns>
        private void setText( string[] lines, int index, Button button, TextBox text, CheckBox checkbox )
        {
            string toRead = findParameterIn( lines, index, "");

            if (null == toRead) toRead = "";
            string[] inputs = toRead.Split('=');
            button.Text = inputs[0].Substring(1, inputs[0].Length - 2);
            text.Text = inputs[1];
            if (text.Text.EndsWith("\\n") )
            {
                text.Text = text.Text.Substring(0, text.Text.Length - 2);
            }
            else
            {
                if(text.Text.Length > 0 ) checkbox.Checked = true;
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Recover an entry in the file lines already read in and return variable.
        /// </summary>
        /// <param name="lines">Source data array</param>
        /// <param name="index">Parameter's index in lines array</param>
        /// <param name="defaultTo">This if label not found</param>
        /// <returns></returns>
        /// 
        string findParameterIn( string[] lines, int index, string defaultTo)
        {
            string result = defaultTo;

            if (index <= lines.Length)
            {
                result = lines[index - 1];
                if( !result.Contains("\"="))
                {
                    result = "\"" + DEFAULT_LABEL + index + "\"=" + result;
                }
            }
            // If not found in the source file, then check environment varibles as per previous version of COMport.exe
            //
            if( ( 0 == result.Length ) && CheckEnvironmentVariables)
            {
                result = "\"" + DEFAULT_LABEL + index + "\"=" + Environment.GetEnvironmentVariable("COMport_QuickText" + index, EnvironmentVariableTarget.User);
            }
            return result;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Save the current text configuration
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, EventArgs e)
        {
            string saveFilename = loadFilename;

            if (File.Exists(saveFilename)) File.Delete(saveFilename);
            //
            using (StreamWriter output = File.CreateText(saveFilename))
            {
                saveToQuickTextFile(output, "\"" + ButtonExeText1.Text + "\"=" + CommandLine1TextBox.Text, ManualEnter1CheckBox);
                saveToQuickTextFile(output, "\"" + ButtonExeText2.Text + "\"=" + CommandLine2TextBox.Text, ManualEnter2CheckBox);
                saveToQuickTextFile(output, "\"" + ButtonExeText3.Text + "\"=" + CommandLine3TextBox.Text, ManualEnter3CheckBox);
                saveToQuickTextFile(output, "\"" + ButtonExeText4.Text + "\"=" + CommandLine4TextBox.Text, ManualEnter4CheckBox);
                saveToQuickTextFile(output, "\"" + ButtonExeText5.Text + "\"=" + CommandLine5TextBox.Text, ManualEnter5CheckBox);
                saveToQuickTextFile(output, "\"" + ButtonExeText6.Text + "\"=" + CommandLine6TextBox.Text, ManualEnter6CheckBox);
                saveToQuickTextFile(output, "\"" + ButtonExeText7.Text + "\"=" + CommandLine7TextBox.Text, ManualEnter7CheckBox);
                saveToQuickTextFile(output, "\"" + ButtonExeText8.Text + "\"=" + CommandLine8TextBox.Text, ManualEnter8CheckBox);
                saveToQuickTextFile(output, "\"" + ButtonExeText9.Text + "\"=" + CommandLine9TextBox.Text, ManualEnter9CheckBox);
                saveToQuickTextFile(output, "\"" + ButtonExeText10.Text + "\"=" + CommandLine10TextBox.Text, ManualEnter10CheckBox);
                saveToQuickTextFile(output, "\"" + ButtonExeText11.Text + "\"=" + CommandLine11TextBox.Text, ManualEnter11CheckBox);
                saveToQuickTextFile(output, "\"" + ButtonExeText12.Text + "\"=" + CommandLine12TextBox.Text, ManualEnter12CheckBox);
                saveToQuickTextFile(output, "\"" + ButtonExeText13.Text + "\"=" + CommandLine13TextBox.Text, ManualEnter13CheckBox);
                saveToQuickTextFile(output, "\"" + ButtonExeText14.Text + "\"=" + CommandLine14TextBox.Text, ManualEnter14CheckBox);
                saveToQuickTextFile(output, "\"" + ButtonExeText15.Text + "\"=" + CommandLine15TextBox.Text, ManualEnter15CheckBox);
                saveToQuickTextFile(output, "\"" + ButtonExeText16.Text + "\"=" + CommandLine16TextBox.Text, ManualEnter16CheckBox);
                saveToQuickTextFile(output, "\"" + ButtonExeText17.Text + "\"=" + CommandLine17TextBox.Text, ManualEnter17CheckBox);
                saveToQuickTextFile(output, "\"" + ButtonExeText18.Text + "\"=" + CommandLine18TextBox.Text, ManualEnter18CheckBox);
                saveToQuickTextFile(output, "\"" + ButtonExeText19.Text + "\"=" + CommandLine19TextBox.Text, ManualEnter19CheckBox);
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Save a variable with trailing "\n" if necessary to the Quick Text file.
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="textbox"></param>
        /// <param name="checkbox"></param>
        private void saveToQuickTextFile(StreamWriter output, string textbox, CheckBox checkbox )
        {
            if (!checkbox.Checked && (textbox.Length > 0)) textbox += "\\n";
            output.WriteLine(textbox );
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
        private void ButtonExeTextN_MouseUp(object sender, MouseEventArgs e)
        {
            Button sentBy = (Button)sender;
            int buttonNumber = Convert.ToInt32(sentBy.Name.Substring(13));

            if (e.Button == MouseButtons.Right)
            {
                EnterLabel formToEnterLabel = new EnterLabel(sentBy.Text);
                formToEnterLabel.StartPosition = FormStartPosition.CenterParent;
                //formToEnterLabel.Location = new Point(10, 10);
                //
                if ( formToEnterLabel.ShowDialog() == DialogResult.OK )
                {
                    sentBy.Text = formToEnterLabel.newLabel;
                }
            }
            else
            {
                executeCommand(buttonNumber);
                MaySetNewRepeatCommand(buttonNumber);
            }
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
            untickExecuteButton();
            RepeatCommandPrimmed = false;
            RepeatCommandTimer.Enabled = false;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Start the repeat command if primed to do so . . .
        /// </summary>
        /// <param name="command"></param>
        private void MaySetNewRepeatCommand( int command )
        {
            if( RepeatCommandPrimmed && (false == StopRepeatButton.Focused) )
            {
                RepeatCommandPrimmed = false;
                RepeatCommandTimer.Enabled = false;
                untickExecuteButton();
                RepeatCommand = command;
                tickExecuteButton();
                RepeatCommandTimer.Enabled = true;
            }
            else
            {
                Program.comportform.CommsTextBox.Focus();
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
                case 1: ButtonExeText1.BackColor = ofButton; break;
                case 2: ButtonExeText2.BackColor = ofButton; break;
                case 3: ButtonExeText3.BackColor = ofButton; break;
                case 4: ButtonExeText4.BackColor = ofButton; break;
                case 5: ButtonExeText5.BackColor = ofButton; break;
                case 6: ButtonExeText6.BackColor = ofButton; break;
                case 7: ButtonExeText7.BackColor = ofButton; break;
                case 8: ButtonExeText8.BackColor = ofButton; break;
                case 9: ButtonExeText9.BackColor = ofButton; break;
                case 10: ButtonExeText10.BackColor = ofButton; break;
                case 11: ButtonExeText11.BackColor = ofButton; break;
                case 12: ButtonExeText12.BackColor = ofButton; break;
                case 13: ButtonExeText13.BackColor = ofButton; break;
                case 14: ButtonExeText14.BackColor = ofButton; break;
                case 15: ButtonExeText15.BackColor = ofButton; break;
                case 16: ButtonExeText16.BackColor = ofButton; break;
                case 17: ButtonExeText17.BackColor = ofButton; break;
                case 18: ButtonExeText18.BackColor = ofButton; break;
                case 19: ButtonExeText19.BackColor = ofButton; break;
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
            string execString = Convert.ToChar(Program.comportform.CR).ToString();

            switch ( command )
            {
                case 1: Program.comportform.sendLinesToKeyboard(CommandLine1TextBox.Text + (ManualEnter1CheckBox.Checked ? "" : execString)); break;
                case 2: Program.comportform.sendLinesToKeyboard(CommandLine2TextBox.Text + (ManualEnter2CheckBox.Checked ? "" : execString)); break;
                case 3: Program.comportform.sendLinesToKeyboard(CommandLine3TextBox.Text + (ManualEnter3CheckBox.Checked ? "" : execString)); break;
                case 4: Program.comportform.sendLinesToKeyboard(CommandLine4TextBox.Text + (ManualEnter4CheckBox.Checked ? "" : execString)); break;
                case 5: Program.comportform.sendLinesToKeyboard(CommandLine5TextBox.Text + (ManualEnter5CheckBox.Checked ? "" : execString)); break;
                case 6: Program.comportform.sendLinesToKeyboard(CommandLine6TextBox.Text + (ManualEnter6CheckBox.Checked ? "" : execString)); break;
                case 7: Program.comportform.sendLinesToKeyboard(CommandLine7TextBox.Text + (ManualEnter7CheckBox.Checked ? "" : execString)); break;
                case 8: Program.comportform.sendLinesToKeyboard(CommandLine8TextBox.Text + (ManualEnter8CheckBox.Checked ? "" : execString)); break;
                case 9: Program.comportform.sendLinesToKeyboard(CommandLine9TextBox.Text + (ManualEnter9CheckBox.Checked ? "" : execString)); break;
                case 10: Program.comportform.sendLinesToKeyboard(CommandLine10TextBox.Text + (ManualEnter10CheckBox.Checked ? "" : execString)); break;
                case 11: Program.comportform.sendLinesToKeyboard(CommandLine11TextBox.Text + (ManualEnter11CheckBox.Checked ? "" : execString)); break;
                case 12: Program.comportform.sendLinesToKeyboard(CommandLine12TextBox.Text + (ManualEnter12CheckBox.Checked ? "" : execString)); break;
                case 13: Program.comportform.sendLinesToKeyboard(CommandLine13TextBox.Text + (ManualEnter13CheckBox.Checked ? "" : execString)); break;
                case 14: Program.comportform.sendLinesToKeyboard(CommandLine14TextBox.Text + (ManualEnter14CheckBox.Checked ? "" : execString)); break;
                case 15: Program.comportform.sendLinesToKeyboard(CommandLine15TextBox.Text + (ManualEnter15CheckBox.Checked ? "" : execString)); break;
                case 16: Program.comportform.sendLinesToKeyboard(CommandLine16TextBox.Text + (ManualEnter16CheckBox.Checked ? "" : execString)); break;
                case 17: Program.comportform.sendLinesToKeyboard(CommandLine17TextBox.Text + (ManualEnter17CheckBox.Checked ? "" : execString)); break;
                case 18: Program.comportform.sendLinesToKeyboard(CommandLine18TextBox.Text + (ManualEnter18CheckBox.Checked ? "" : execString)); break;
                case 19: Program.comportform.sendLinesToKeyboard(CommandLine19TextBox.Text + (ManualEnter19CheckBox.Checked ? "" : execString)); break;
                //
                default: RepeatCommandTimer.Enabled = false; break; // Invalid command number, stop the repeat timer.
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
                double test = Convert.ToDouble(RepeatEveryTextBox.Text) * 1000.0; // Timer uses milliseconds.
                if( test >= MINIMUM_PERIOD )
                {
                    RepeatCommandTimer.Interval = (int)test;
                }
                else
                {
                    RepeatCommandTimer.Interval = MINIMUM_PERIOD;
                }
            }
            catch
            {
                RepeatCommandTimer.Interval = 1000; // Just use a reasonable one-second value.
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Extend or shrink the menu to handle the extra commands at the bottom.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoreCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SuspendLayout();
            //
            if ( MoreCheckBox.Checked )
            {
                ClientSize = new System.Drawing.Size(QUICK_TEXT_WIDTH, QUICK_TEXT_MORE_HEIGHT);
            }
            else
            {
                ClientSize = new System.Drawing.Size(QUICK_TEXT_WIDTH, QUICK_TEXT_LESS_HEIGHT);
            }
            ResumeLayout(false);
            PerformLayout();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Set accept button to the corrisponding TextBox executed when selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommandLine1TextBox_Enter(object sender, EventArgs e) { AcceptButton = ButtonExeText1; }
        private void CommandLine2TextBox_Enter(object sender, EventArgs e) { AcceptButton = ButtonExeText2; }
        private void CommandLine3TextBox_Enter(object sender, EventArgs e) { AcceptButton = ButtonExeText3; }
        private void CommandLine4TextBox_Enter(object sender, EventArgs e) { AcceptButton = ButtonExeText4; }
        private void CommandLine5TextBox_Enter(object sender, EventArgs e) { AcceptButton = ButtonExeText5; }
        private void CommandLine6TextBox_Enter(object sender, EventArgs e) { AcceptButton = ButtonExeText6; }
        private void CommandLine7TextBox_Enter(object sender, EventArgs e) { AcceptButton = ButtonExeText7; }
        private void CommandLine8TextBox_Enter(object sender, EventArgs e) { AcceptButton = ButtonExeText8; }
        private void CommandLine9TextBox_Enter(object sender, EventArgs e) { AcceptButton = ButtonExeText9; }
        private void CommandLine10TextBox_Enter(object sender, EventArgs e) { AcceptButton = ButtonExeText10; }
        private void CommandLine11TextBox_Enter(object sender, EventArgs e) { AcceptButton = ButtonExeText11; }
        private void CommandLine12TextBox_Enter(object sender, EventArgs e) { AcceptButton = ButtonExeText12; }
        private void CommandLine13TextBox_Enter(object sender, EventArgs e) { AcceptButton = ButtonExeText13; }
        private void CommandLine14TextBox_Enter(object sender, EventArgs e) { AcceptButton = ButtonExeText14; }
        private void CommandLine15TextBox_Enter(object sender, EventArgs e) { AcceptButton = ButtonExeText15; }
        private void CommandLine16TextBox_Enter(object sender, EventArgs e) { AcceptButton = ButtonExeText16; }
        private void CommandLine17TextBox_Enter(object sender, EventArgs e) { AcceptButton = ButtonExeText17; }
        private void CommandLine18TextBox_Enter(object sender, EventArgs e) { AcceptButton = ButtonExeText18; }
        private void CommandLine19TextBox_Enter(object sender, EventArgs e) { AcceptButton = ButtonExeText19; }
    }
}
