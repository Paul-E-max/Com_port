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
        const int QUICK_TEXT_WIDTH = 481;
        const int QUICK_TEXT_LESS_HEIGHT = 627 - FORM_BAR;
        const int QUICK_TEXT_MORE_HEIGHT = 691 - FORM_BAR;
        const int MINIMUM_PERIOD = 100;

        bool RepeatCommandPrimmed = false;
        bool CheckEnvironmentVariables = true; // Assume that environment variables can be used if necessary.
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
            string loadFilename = COMportForm.QUICK_TEXT + "_" + Program.comportform.VersionComboBox.Text + COMportForm.TEXT_FILE_EXT;
            string[] lines = new string[0];

            if (!File.Exists(loadFilename)) loadFilename = COMportForm.QUICK_TEXT + COMportForm.TEXT_FILE_EXT; // Default quick text without the project name appended.
            //
            if (File.Exists(loadFilename))
            {
                lines = File.ReadAllLines(loadFilename);
                if (lines.Length > 0) CheckEnvironmentVariables = false;
            }
            CommandLine1TextBox.Text = setText( lines, 1, ManualEnter1CheckBox);
            CommandLine2TextBox.Text = setText(lines, 2, ManualEnter2CheckBox);
            CommandLine3TextBox.Text = setText(lines, 3, ManualEnter3CheckBox);
            CommandLine4TextBox.Text = setText(lines, 4, ManualEnter4CheckBox);
            CommandLine5TextBox.Text = setText(lines, 5, ManualEnter5CheckBox);
            CommandLine6TextBox.Text = setText(lines, 6, ManualEnter6CheckBox);
            CommandLine7TextBox.Text = setText(lines, 7, ManualEnter7CheckBox);
            CommandLine8TextBox.Text = setText(lines, 8, ManualEnter8CheckBox);
            CommandLine9TextBox.Text = setText(lines, 9, ManualEnter9CheckBox);
            CommandLine10TextBox.Text = setText(lines, 10, ManualEnter10CheckBox);
            CommandLine11TextBox.Text = setText(lines, 11, ManualEnter11CheckBox);
            CommandLine12TextBox.Text = setText(lines, 12, ManualEnter12CheckBox);
            CommandLine13TextBox.Text = setText(lines, 13, ManualEnter13CheckBox);
            CommandLine14TextBox.Text = setText(lines, 14, ManualEnter14CheckBox);
            CommandLine15TextBox.Text = setText(lines, 15, ManualEnter15CheckBox);
            CommandLine16TextBox.Text = setText(lines, 16, ManualEnter16CheckBox);
            CommandLine17TextBox.Text = setText(lines, 17, ManualEnter17CheckBox);
            CommandLine18TextBox.Text = setText(lines, 18, ManualEnter18CheckBox);
            CommandLine19TextBox.Text = setText(lines, 19, ManualEnter19CheckBox);
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
        private string setText( string[] lines, int index, CheckBox checkbox )
        {
            string toRead = findParameterIn( lines, index, "");

            if (null == toRead) toRead = "";
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
            }
            // If not found in the source file, then check environment varibles as per previous version of COMport.exe
            //
            if( ( 0 == result.Length ) && CheckEnvironmentVariables)
            {
                result = Environment.GetEnvironmentVariable("COMport_QuickText" + index, EnvironmentVariableTarget.User);
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
            string saveFilename = COMportForm.QUICK_TEXT + "_" + Program.comportform.VersionComboBox.Text + COMportForm.TEXT_FILE_EXT;

            if (File.Exists(saveFilename)) File.Delete(saveFilename);
            //
            using (StreamWriter output = File.CreateText(saveFilename))
            {
                saveToQuickTextFile(output, CommandLine1TextBox.Text, ManualEnter1CheckBox);
                saveToQuickTextFile(output, CommandLine2TextBox.Text, ManualEnter2CheckBox);
                saveToQuickTextFile(output, CommandLine3TextBox.Text, ManualEnter3CheckBox);
                saveToQuickTextFile(output, CommandLine4TextBox.Text, ManualEnter4CheckBox);
                saveToQuickTextFile(output, CommandLine5TextBox.Text, ManualEnter5CheckBox);
                saveToQuickTextFile(output, CommandLine6TextBox.Text, ManualEnter6CheckBox);
                saveToQuickTextFile(output, CommandLine7TextBox.Text, ManualEnter7CheckBox);
                saveToQuickTextFile(output, CommandLine8TextBox.Text, ManualEnter8CheckBox);
                saveToQuickTextFile(output, CommandLine9TextBox.Text, ManualEnter9CheckBox);
                saveToQuickTextFile(output, CommandLine10TextBox.Text, ManualEnter10CheckBox);
                saveToQuickTextFile(output, CommandLine11TextBox.Text, ManualEnter11CheckBox);
                saveToQuickTextFile(output, CommandLine12TextBox.Text, ManualEnter12CheckBox);
                saveToQuickTextFile(output, CommandLine13TextBox.Text, ManualEnter13CheckBox);
                saveToQuickTextFile(output, CommandLine14TextBox.Text, ManualEnter14CheckBox);
                saveToQuickTextFile(output, CommandLine15TextBox.Text, ManualEnter15CheckBox);
                saveToQuickTextFile(output, CommandLine16TextBox.Text, ManualEnter16CheckBox);
                saveToQuickTextFile(output, CommandLine17TextBox.Text, ManualEnter17CheckBox);
                saveToQuickTextFile(output, CommandLine18TextBox.Text, ManualEnter18CheckBox);
                saveToQuickTextFile(output, CommandLine19TextBox.Text, ManualEnter19CheckBox);
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
        private void CommandLine1TextBox_Enter(object sender, EventArgs e) { AcceptButton = ExeText1Button; }
        private void CommandLine2TextBox_Enter(object sender, EventArgs e) { AcceptButton = ExeText2Button; }
        private void CommandLine3TextBox_Enter(object sender, EventArgs e) { AcceptButton = ExeText3Button; }
        private void CommandLine4TextBox_Enter(object sender, EventArgs e) { AcceptButton = ExeText4Button; }
        private void CommandLine5TextBox_Enter(object sender, EventArgs e) { AcceptButton = ExeText5Button; }
        private void CommandLine6TextBox_Enter(object sender, EventArgs e) { AcceptButton = ExeText6Button; }
        private void CommandLine7TextBox_Enter(object sender, EventArgs e) { AcceptButton = ExeText7Button; }
        private void CommandLine8TextBox_Enter(object sender, EventArgs e) { AcceptButton = ExeText8Button; }
        private void CommandLine9TextBox_Enter(object sender, EventArgs e) { AcceptButton = ExeText9Button; }
        private void CommandLine10TextBox_Enter(object sender, EventArgs e) { AcceptButton = ExeText10Button; }
        private void CommandLine11TextBox_Enter(object sender, EventArgs e) { AcceptButton = ExeText11Button; }
        private void CommandLine12TextBox_Enter(object sender, EventArgs e) { AcceptButton = ExeText12Button; }
        private void CommandLine13TextBox_Enter(object sender, EventArgs e) { AcceptButton = ExeText13Button; }
        private void CommandLine14TextBox_Enter(object sender, EventArgs e) { AcceptButton = ExeText14Button; }
        private void CommandLine15TextBox_Enter(object sender, EventArgs e) { AcceptButton = ExeText15Button; }
        private void CommandLine16TextBox_Enter(object sender, EventArgs e) { AcceptButton = ExeText16Button; }
        private void CommandLine17TextBox_Enter(object sender, EventArgs e) { AcceptButton = ExeText17Button; }
        private void CommandLine18TextBox_Enter(object sender, EventArgs e) { AcceptButton = ExeText18Button; }
        private void CommandLine19TextBox_Enter(object sender, EventArgs e) { AcceptButton = ExeText19Button; }
    }
}
