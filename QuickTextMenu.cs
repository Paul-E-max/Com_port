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
// 21.11.2024-MD V1.01.24 - 1) Move handling of pumping strings from COMport.cs and QuickTextMenu.cs
//                          2) Save QuickTextMenu inter-character, newline and repeat delays with each page.
//                          3) Move timestamp and hex output checkboxes to the main screen.
// 10.10.2024-MD V1.01.23 - If command marked as "Use file" then pipe the contents of that file in as though typed.
// 02.06.2023-MD V1.01.15 - Correction to right click and accept button.
// 25.04.2023-MD V1.01.13 - Correction to Quick Text menu with equals character in it.
// 21.04.2023-MD V1.01.12 - Check if save is required on exit.
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
using System.Diagnostics;

namespace COMport
{
    public partial class QuickTextMenu : Form
    {
        const int REPEAT_DISABLED = 0;
        const int FORM_BAR = 39;
        const int QUICK_TEXT_WIDTH = 534; // In the GUI, this is set to 550 . . . why the difference ?
        const int QUICK_TEXT_LESS_HEIGHT = 626 - FORM_BAR;
        const int QUICK_TEXT_MORE_HEIGHT = 691 - FORM_BAR;
        const int MINIMUM_PERIOD = 100;
        const string DEFAULT_LABEL = "Command";

        Color SAVE_POSSIBLY_REQUIRED = Color.Blue;
        Color SAVE_NOT_REQUIRED = SystemColors.ControlText;

        string loadFilename;
        bool RepeatCommandPrimmed = false;
        int RepeatCommand = REPEAT_DISABLED;
        int quickTextLineDelay = 0;
        int quickTextCharDelay = 0;

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Setup quick text menus.
        /// </summary>
        /// <param name="filename"></param>
        public QuickTextMenu(string filename)
        {
            InitializeComponent();
            loadFilename = filename;
            this.Text = filename;       // Put up the title for this quick text file.
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
            int offset = 0;

            if (File.Exists(loadFilename)) lines = File.ReadAllLines(loadFilename);
            offset = CheckForTimings(lines);
            //
            setText(lines, offset + 1, ButtonExeText1, CommandLine1TextBox,    ManualEnter1CheckBox,  UseFileCheckBox1);
            setText(lines, offset + 2, ButtonExeText2, CommandLine2TextBox,    ManualEnter2CheckBox,  UseFileCheckBox2);
            setText(lines, offset + 3, ButtonExeText3, CommandLine3TextBox,    ManualEnter3CheckBox,  UseFileCheckBox3);
            setText(lines, offset + 4, ButtonExeText4, CommandLine4TextBox,    ManualEnter4CheckBox,  UseFileCheckBox4);
            setText(lines, offset + 5, ButtonExeText5, CommandLine5TextBox,    ManualEnter5CheckBox,  UseFileCheckBox5);
            setText(lines, offset + 6, ButtonExeText6, CommandLine6TextBox,    ManualEnter6CheckBox,  UseFileCheckBox6);
            setText(lines, offset + 7, ButtonExeText7, CommandLine7TextBox,    ManualEnter7CheckBox,  UseFileCheckBox7);
            setText(lines, offset + 8, ButtonExeText8, CommandLine8TextBox,    ManualEnter8CheckBox,  UseFileCheckBox8);
            setText(lines, offset + 9, ButtonExeText9, CommandLine9TextBox,    ManualEnter9CheckBox,  UseFileCheckBox9);
            setText(lines, offset + 10, ButtonExeText10, CommandLine10TextBox, ManualEnter10CheckBox, UseFileCheckBox10);
            setText(lines, offset + 11, ButtonExeText11, CommandLine11TextBox, ManualEnter11CheckBox, UseFileCheckBox11);
            setText(lines, offset + 12, ButtonExeText12, CommandLine12TextBox, ManualEnter12CheckBox, UseFileCheckBox12);
            setText(lines, offset + 13, ButtonExeText13, CommandLine13TextBox, ManualEnter13CheckBox, UseFileCheckBox13);
            setText(lines, offset + 14, ButtonExeText14, CommandLine14TextBox, ManualEnter14CheckBox, UseFileCheckBox14);
            setText(lines, offset + 15, ButtonExeText15, CommandLine15TextBox, ManualEnter15CheckBox, UseFileCheckBox15);
            setText(lines, offset + 16, ButtonExeText16, CommandLine16TextBox, ManualEnter16CheckBox, UseFileCheckBox16);
            setText(lines, offset + 17, ButtonExeText17, CommandLine17TextBox, ManualEnter17CheckBox, UseFileCheckBox17);
            setText(lines, offset + 18, ButtonExeText18, CommandLine18TextBox, ManualEnter18CheckBox, UseFileCheckBox18);
            setText(lines, offset + 19, ButtonExeText19, CommandLine19TextBox, ManualEnter19CheckBox, UseFileCheckBox19);
            //
            SaveButton.BackColor = SystemColors.Control;
            SaveButton.ForeColor = SAVE_NOT_REQUIRED;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Read from lines found in Quick Text menu file.
        /// </summary>
        /// <param name="index">One of seven different entries to update</param>
        /// <param name="checkbox">If read item doesn't end in "\n", tick this as "No Enter" required</param>
        /// <returns></returns>
        private void setText( string[] lines, int index, Button button, TextBox text, CheckBox checkbox, CheckBox fileRef )
        {
            string toRead = findParameterByIndex( lines, index, "");

            if (toRead.StartsWith("<<") && toRead.EndsWith(">>"))
            {
                // This is a filename.
                //
                fileRef.Checked = true;
                toRead = toRead.Substring(2, toRead.Length - 4);
            }

            string[] inputs = toRead.Split('=');
            
            try
            {
                // Trim off the leading and trailing quote.
                //
                if (inputs.Length > 1)
                {
                    button.Text = inputs[0].Substring(1, inputs[0].Length - 2);
                }
                else
                {
                    button.Text = DEFAULT_LABEL + index.ToString();
                }
            }
            catch
            {
                button.Text = DEFAULT_LABEL + index.ToString();
            }
            try
            {
                if (inputs.Length > 1)
                {
                    text.Text = toRead.Substring(inputs[0].Length + 1);
                }
                else
                {
                    text.Text = inputs[0];
                }
            }
            catch
            {
                text.Text = "";
            }
            if (text.Text.EndsWith("\\n") )
            {
                // This is a string that contains a enter.
                //
                text.Text = text.Text.Substring(0, text.Text.Length - 2);
            }
            else
            {
                // This is a string that uses a manual enter.
                //
                if (text.Text.Length > 0 ) checkbox.Checked = true;
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
        string findParameterByIndex( string[] lines, int index, string defaultTo)
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
            return result;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Check for the timings that is optional in the first line.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        private int CheckForTimings(string[] lines)
        {
            int offset = 0;

            // These are the default values if not defined in the file just read.
            //
            CharDelayTextBox.Text = COMport.InterCharDelay.ToString();
            NLDelayTextBox.Text = COMport.InterLineDelay.ToString();
            //
            // If the file just read does define them, there will be three comma separated values.
            //
            // For example 10,500,5
            //
            // If a string is missing, then leave it as its default value.
            //
            try
            {
                if ((lines[0].Length > 0) && !lines[0].StartsWith("\""))
                {
                    string[] parameters = lines[0].Split(',');

                    offset++; // So that the loading by setText() points to the button parameters.
                              //
                    if (parameters.Length > 0)
                    {
                        if (parameters[0].Length > 0)
                        {
                            if (String.Compare(CharDelayTextBox.Text, parameters[0]) < 0) CharDelayTextBox.Text = parameters[0];
                        }
                    }

                    if (parameters.Length > 1)
                    {
                        if (parameters[1].Length > 0)
                        {
                            if (String.Compare(NLDelayTextBox.Text, parameters[1]) < 0) NLDelayTextBox.Text = parameters[1];
                        }
                    }
                    if (parameters.Length > 2)
                    {
                        if (parameters[2].Length > 0) RepeatEveryTextBox.Text = parameters[2];
                    }
                }
            }
            catch
            {
                // Default settings done, leave it at that.
            }
            return offset;
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
                // Option to save the timings within the quick text page.
                //
                string timings = "";

                if( CharDelayTextBox.Text != COMport.InterCharDelay.ToString() )
                {
                    timings += CharDelayTextBox.Text;
                }
                timings += ",";
                //
                if( NLDelayTextBox.Text != COMport.InterLineDelay.ToString() )
                {
                    timings += NLDelayTextBox.Text;
                }
                timings += "," + RepeatEveryTextBox.Text;
                //
                output.WriteLine(timings);
                //
                saveToQuickTextFile(output, "\"" + ButtonExeText1.Text + "\"=" + CommandLine1TextBox.Text,   ManualEnter1CheckBox,  UseFileCheckBox1);
                saveToQuickTextFile(output, "\"" + ButtonExeText2.Text + "\"=" + CommandLine2TextBox.Text,   ManualEnter2CheckBox,  UseFileCheckBox2);
                saveToQuickTextFile(output, "\"" + ButtonExeText3.Text + "\"=" + CommandLine3TextBox.Text,   ManualEnter3CheckBox,  UseFileCheckBox3);
                saveToQuickTextFile(output, "\"" + ButtonExeText4.Text + "\"=" + CommandLine4TextBox.Text,   ManualEnter4CheckBox,  UseFileCheckBox4);
                saveToQuickTextFile(output, "\"" + ButtonExeText5.Text + "\"=" + CommandLine5TextBox.Text,   ManualEnter5CheckBox,  UseFileCheckBox5);
                saveToQuickTextFile(output, "\"" + ButtonExeText6.Text + "\"=" + CommandLine6TextBox.Text,   ManualEnter6CheckBox,  UseFileCheckBox6);
                saveToQuickTextFile(output, "\"" + ButtonExeText7.Text + "\"=" + CommandLine7TextBox.Text,   ManualEnter7CheckBox,  UseFileCheckBox7);
                saveToQuickTextFile(output, "\"" + ButtonExeText8.Text + "\"=" + CommandLine8TextBox.Text,   ManualEnter8CheckBox,  UseFileCheckBox8);
                saveToQuickTextFile(output, "\"" + ButtonExeText9.Text + "\"=" + CommandLine9TextBox.Text,   ManualEnter9CheckBox,  UseFileCheckBox9);
                saveToQuickTextFile(output, "\"" + ButtonExeText10.Text + "\"=" + CommandLine10TextBox.Text, ManualEnter10CheckBox, UseFileCheckBox10);
                saveToQuickTextFile(output, "\"" + ButtonExeText11.Text + "\"=" + CommandLine11TextBox.Text, ManualEnter11CheckBox, UseFileCheckBox11);
                saveToQuickTextFile(output, "\"" + ButtonExeText12.Text + "\"=" + CommandLine12TextBox.Text, ManualEnter12CheckBox, UseFileCheckBox12);
                saveToQuickTextFile(output, "\"" + ButtonExeText13.Text + "\"=" + CommandLine13TextBox.Text, ManualEnter13CheckBox, UseFileCheckBox13);
                saveToQuickTextFile(output, "\"" + ButtonExeText14.Text + "\"=" + CommandLine14TextBox.Text, ManualEnter14CheckBox, UseFileCheckBox14);
                saveToQuickTextFile(output, "\"" + ButtonExeText15.Text + "\"=" + CommandLine15TextBox.Text, ManualEnter15CheckBox, UseFileCheckBox15);
                saveToQuickTextFile(output, "\"" + ButtonExeText16.Text + "\"=" + CommandLine16TextBox.Text, ManualEnter16CheckBox, UseFileCheckBox16);
                saveToQuickTextFile(output, "\"" + ButtonExeText17.Text + "\"=" + CommandLine17TextBox.Text, ManualEnter17CheckBox, UseFileCheckBox17);
                saveToQuickTextFile(output, "\"" + ButtonExeText18.Text + "\"=" + CommandLine18TextBox.Text, ManualEnter18CheckBox, UseFileCheckBox18);
                saveToQuickTextFile(output, "\"" + ButtonExeText19.Text + "\"=" + CommandLine19TextBox.Text, ManualEnter19CheckBox, UseFileCheckBox19);
            }
            SaveButton.BackColor = SystemColors.Control;
            SaveButton.ForeColor = SAVE_NOT_REQUIRED;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Save a variable with trailing "\n" if necessary to the Quick Text file.
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="textbox"></param>
        /// <param name="checkbox"></param>
        private void saveToQuickTextFile(StreamWriter output, string textbox, CheckBox checkbox, CheckBox fileRef )
        {
            if (!checkbox.Checked && (textbox.Length > 0)) textbox += "\\n";
            if (fileRef.Checked) textbox = "<<" + textbox + ">>";
            output.WriteLine(textbox);
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
        /// Exiting the form, check to see if save is required.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuickTextMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            if( SaveButton.ForeColor == SAVE_POSSIBLY_REQUIRED )
            {
                if( DialogResult.Yes == MessageBox.Show("Do you want to save the changes",loadFilename + " modified", MessageBoxButtons.YesNo) )
                {
                    SaveButton.PerformClick();
                }
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Check for right clicks on the button label to set it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonExeTextN_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Button sentBy = (Button)sender;
                int buttonNumber = Convert.ToInt32(sentBy.Name.Substring("ButtonExeText".Length));

                // Right click of execute button means change its label.
                //
                EnterLabel formToEnterLabel = new EnterLabel(sentBy.Text);
                formToEnterLabel.StartPosition = FormStartPosition.CenterParent;
                //
                if (formToEnterLabel.ShowDialog() == DialogResult.OK)
                {
                    sentBy.Text = formToEnterLabel.newLabel;
                    SaveButton.ForeColor = SAVE_POSSIBLY_REQUIRED;
                }
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// On clicking the execution button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonExeTextN_Click(object sender, EventArgs e)
        {
            Button sentBy = (Button)sender;
            int buttonNumber = Convert.ToInt32(sentBy.Name.Substring("ButtonExeText".Length));

            executeCommand(buttonNumber);
            MaySetNewRepeatCommand(buttonNumber);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Set accept button to the corrisponding TextBox executed when selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommandLineTextBox_Enter(object sender, EventArgs e)
        {
            string controlName = "ButtonExeText" + ((TextBox)sender).Name.Substring("CommandLine".Length).Replace("TextBox", "");

            AcceptButton = (Button)Controls.Find(controlName, false)[0];
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
        /// Send a text strings to the keyboard buffer, doing newline if not manual enter.
        /// </summary>
        /// <param name="toSend"></param>
        /// <param name="manualEnter"></param>
        /// <param name="pipeFile"></param>
        public void sendLinesToKeyboard(string toSend, bool manualEnter, bool pipeFile)
        {
            // Splice and dice strings up into whole lines.
            //
            while (toSend.Contains("\\n"))
            {
                // Note that Environment.NewLine should not be equal to "\\n" (only the strangest of stange people would set it so)!
                //
                int idx = toSend.IndexOf("\\n");
                string aline = toSend.Substring(0, idx);
                toSend = toSend.Substring(idx + 2);
                //
                sendLinesToKeyboard(aline + Convert.ToChar(Program.comportform.CR), false, pipeFile);
            }
            if (pipeFile)
            {
                toSend = toSend.TrimEnd('\r');
                //
                if (File.Exists(toSend))
                {
                    string[] lines = File.ReadAllLines(toSend);

                    foreach (string line in lines)
                    {
                        sendLinesToKeyboard(line, false, false); // Sends one line at a time to device.
                    }
                }
                else
                {
                    sendLinesToKeyboard("// Can't find file: " + toSend, false, false); // Sends error message as a comment line.
                }
            }
            else
            {
                if ((false == manualEnter) && !toSend.EndsWith(Convert.ToChar(Program.comportform.CR).ToString()))
                {
                    toSend += Convert.ToChar(Program.comportform.CR);
                }
                if (toSend.StartsWith("//"))
                {
                    // Display lines as comment by inserting control characters in to switch activity on/off . . .
                    //
                    toSend = Program.comportform.COMMENT_ON + toSend;
                }
                Program.comportform.sendToKeyboard(toSend, quickTextCharDelay, quickTextLineDelay);
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Execute a command in one of the prepared text boxes.
        /// </summary>
        /// <param name="command"></param>
        private void executeCommand( int command )
        {
            switch ( command )
            {
                case 1: sendLinesToKeyboard(CommandLine1TextBox.Text, ManualEnter1CheckBox.Checked, UseFileCheckBox1.Checked); break;
                case 2: sendLinesToKeyboard(CommandLine2TextBox.Text, ManualEnter2CheckBox.Checked, UseFileCheckBox2.Checked); break;
                case 3: sendLinesToKeyboard(CommandLine3TextBox.Text, ManualEnter3CheckBox.Checked, UseFileCheckBox3.Checked); break;
                case 4: sendLinesToKeyboard(CommandLine4TextBox.Text, ManualEnter4CheckBox.Checked, UseFileCheckBox4.Checked); break;
                case 5: sendLinesToKeyboard(CommandLine5TextBox.Text, ManualEnter5CheckBox.Checked, UseFileCheckBox5.Checked); break;
                case 6: sendLinesToKeyboard(CommandLine6TextBox.Text, ManualEnter6CheckBox.Checked, UseFileCheckBox6.Checked); break;
                case 7: sendLinesToKeyboard(CommandLine7TextBox.Text, ManualEnter7CheckBox.Checked, UseFileCheckBox7.Checked); break;
                case 8: sendLinesToKeyboard(CommandLine8TextBox.Text, ManualEnter8CheckBox.Checked, UseFileCheckBox8.Checked); break;
                case 9: sendLinesToKeyboard(CommandLine9TextBox.Text, ManualEnter9CheckBox.Checked, UseFileCheckBox9.Checked); break;
                case 10: sendLinesToKeyboard(CommandLine10TextBox.Text, ManualEnter10CheckBox.Checked, UseFileCheckBox10.Checked); break;
                case 11: sendLinesToKeyboard(CommandLine11TextBox.Text, ManualEnter11CheckBox.Checked, UseFileCheckBox11.Checked); break;
                case 12: sendLinesToKeyboard(CommandLine12TextBox.Text, ManualEnter12CheckBox.Checked, UseFileCheckBox12.Checked); break;
                case 13: sendLinesToKeyboard(CommandLine13TextBox.Text, ManualEnter13CheckBox.Checked, UseFileCheckBox13.Checked); break;
                case 14: sendLinesToKeyboard(CommandLine14TextBox.Text, ManualEnter14CheckBox.Checked, UseFileCheckBox14.Checked); break;
                case 15: sendLinesToKeyboard(CommandLine15TextBox.Text, ManualEnter15CheckBox.Checked, UseFileCheckBox15.Checked); break;
                case 16: sendLinesToKeyboard(CommandLine16TextBox.Text, ManualEnter16CheckBox.Checked, UseFileCheckBox16.Checked); break;
                case 17: sendLinesToKeyboard(CommandLine17TextBox.Text, ManualEnter17CheckBox.Checked, UseFileCheckBox17.Checked); break;
                case 18: sendLinesToKeyboard(CommandLine18TextBox.Text, ManualEnter18CheckBox.Checked, UseFileCheckBox18.Checked); break;
                case 19: sendLinesToKeyboard(CommandLine19TextBox.Text, ManualEnter19CheckBox.Checked, UseFileCheckBox19.Checked); break;
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
        /// Set the save button to indicate that a save is probably required.
        /// </summary>
        private void savePossiblyRequired()
        {
            SaveButton.BackColor = Color.White;
            SaveButton.ForeColor = SAVE_POSSIBLY_REQUIRED;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// When something is updated, note that we need to save.
        /// Also, may need to modify the Manual Enter flags.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Contents_Changed(object sender, EventArgs e)
        {
            savePossiblyRequired();
            //
            updateManualFlags(sender, UseFileCheckBox1, ManualEnter1CheckBox);
            updateManualFlags(sender, UseFileCheckBox2, ManualEnter2CheckBox);
            updateManualFlags(sender, UseFileCheckBox3, ManualEnter3CheckBox);
            updateManualFlags(sender, UseFileCheckBox4, ManualEnter4CheckBox);
            updateManualFlags(sender, UseFileCheckBox5, ManualEnter5CheckBox);
            updateManualFlags(sender, UseFileCheckBox6, ManualEnter6CheckBox);
            updateManualFlags(sender, UseFileCheckBox7, ManualEnter7CheckBox);
            updateManualFlags(sender, UseFileCheckBox8, ManualEnter8CheckBox);
            updateManualFlags(sender, UseFileCheckBox9, ManualEnter9CheckBox);
            updateManualFlags(sender, UseFileCheckBox10, ManualEnter10CheckBox);
            updateManualFlags(sender, UseFileCheckBox11, ManualEnter11CheckBox);
            updateManualFlags(sender, UseFileCheckBox12, ManualEnter12CheckBox);
            updateManualFlags(sender, UseFileCheckBox13, ManualEnter13CheckBox);
            updateManualFlags(sender, UseFileCheckBox14, ManualEnter14CheckBox);
            updateManualFlags(sender, UseFileCheckBox15, ManualEnter15CheckBox);
            updateManualFlags(sender, UseFileCheckBox16, ManualEnter16CheckBox);
            updateManualFlags(sender, UseFileCheckBox17, ManualEnter17CheckBox);
            updateManualFlags(sender, UseFileCheckBox18, ManualEnter18CheckBox);
            updateManualFlags(sender, UseFileCheckBox19, ManualEnter19CheckBox);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// If "Use File" is selected, force the "Manual Enter" flag off and disable.
        /// If "Use File" is deselected, re-enable "Manual Enter".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="useFile"></param>
        /// <param name="ManualEnter"></param>
        void updateManualFlags(object sender, CheckBox useFile, CheckBox ManualEnter)
        {
            if (sender == useFile)
            {
                if (useFile.Checked)
                {
                    ManualEnter.Checked = false;
                    ManualEnter.Enabled = false;
                }
                else
                {
                    ManualEnter.Enabled = true;
                }
            }
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
                quickTextCharDelay = Convert.ToInt32(CharDelayTextBox.Text);
            }
            catch
            {
                quickTextCharDelay = 0;
            }
            savePossiblyRequired();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NLDelayTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                quickTextLineDelay = Convert.ToInt32(NLDelayTextBox.Text);
            }
            catch
            {
                quickTextLineDelay = 0;
            }
            savePossiblyRequired();
        }
    }
}
