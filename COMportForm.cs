// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
// @File:       COMportForm.cs
// @Project:    DISCOVER_UTIL\COMport
// @Author:     Foster & Freeman Ltd - Michael Dodd
// @Created:    31.03.2022
//
// @Brief:      Simple COM port communications device for use with DISCOVER.
//
// @Tools:      Visual Studio 2019, C#
//
// @Revision:
// 16.08.2022-MD V1.00.04 - Add half-duplex option.
// 13.07.2022-MD Use CSV file to get identities.
//               Environment variables can retain COM and project.
// 28.04.2022-MD Clear screen button added.
// 06.04.2022-MD Correction to BS when textbox is scrolled!
// 01.04.2022-MD Handles BS coming from COM port.
// 31.03.2022-MD Initial version.
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
    public partial class COMportForm : Form
    {
        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        // Constants
        //
        const string APP_NAME = "COMport";
        const string VERSION = "V1.00.04";
        const string FILENAME_CSV = APP_NAME + ".CSV";

        const string CONNECT_LABEL = "Connect";
        const string SCANNING_LABEL = "Connecting";
        const string DISCONNECT_LABEL = "Disconnect";
        
        const int TIMEOUT_IMMEDIATE = 1;        // Timeout during clearing of buffer.
        const int TIMEOUT_CLEARS = 20;          // Number of times to attempt clear of buffer.
        const int TIMEOUT_NORMAL = 200;         // Normal period to wait for TX or RX to complete (use -1 for debugging, gives infinite period).
        const int TIMEOUT_MAY = 50;             // If not sure that a response is due.
        const int INTERVAL_RESPOND = 175;       // Period between TX and looking for RX.

        const byte BS = 0x08;

        const int MAX_PROJECTS = 12;            // Reads through FILENAME_CSV file, but abandons data beyond this number of projects.

        struct project
        {
            public string name;
            public string baudrate;
            public string echoOff;
            public string echoOn;
            public string getID;
            public bool halfDuplex;
        };

        project[] projects = new project[MAX_PROJECTS];
        string EchoOff = "";
        string EchoOn = "";
        string GetID = "";

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// 
        /// </summary>
        public COMportForm()
        {
            InitializeComponent();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// This occurs before the user sees any response on the screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void COMportForm_Load(object sender, EventArgs e)
        {
            this.Text = APP_NAME + " - " + VERSION;
            loadProjectInfo();
            LoadEnviroment();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Once the frame is displayed, it is OK to fill in details.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void COMportForm_Shown(object sender, EventArgs e)
        {
            scanForAvailableCOMports();
            // LoadConfigurationCSV();
            //
            // If scan found any COM ports and CSV did not define a valid port, then
            // just display the first item in the list.
            //
            if (COMportComboBox.Text.Length > 0)
            {
                // The CSV file had a COM port that is plugged in, so lets try and connect.
                //
                ConnectButton.PerformClick();
            }
            else
            {
                // Guess the first COM port in the list is the one.
                //
                if (COMportComboBox.Items.Count > 0) COMportComboBox.Text = COMportComboBox.Items[0].ToString();
                //
                // If there is only one of them, try to connect to it, otherwise will have to let user select it
                // manually.
                //
                if (1 == COMportComboBox.Items.Count) ConnectButton.PerformClick();
            }

        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Connecting to the selected COM port (or scan if none selected).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            string versionIs = "";

            if (CONNECT_LABEL == ConnectButton.Text)
            {
                // CONNECTING TO COM PORT
                // ----------------------
                //
                if (0 == COMportComboBox.Text.Length)
                {
                    // Nothing select at the time of hitting connect, so rescan now.
                    //
                    scanForAvailableCOMports();
                    //
                    // Grab the first available one.
                    //
                    if (COMportComboBox.Items.Count > 0) COMportComboBox.Text = COMportComboBox.Items[0].ToString();
                }
                if (COMportComboBox.Text.Length > 0)
                {
                    ConnectButton.Text = SCANNING_LABEL;
                    ConnectButton.BackColor = System.Drawing.Color.Yellow;
                    ConnectButton.Refresh();
                    //
                    // Attempt to connect to a COM port . . .
                    //
                    if (COMportComboBox.Text.Substring(0, 3) == "COM")
                    {
                        try // The following is sensitive to communication errors, and will abandon the task if one occurs.
                        {
                            if (SerialPort.IsOpen) SerialPort.Close();
                            SerialPort.PortName = COMportComboBox.Text;
                            SerialPort.ReadTimeout = TIMEOUT_NORMAL;
                            SerialPort.WriteTimeout = TIMEOUT_NORMAL;
                            SerialPort.BaudRate = Convert.ToInt32(BaudComboBox.Text);
                            SerialPort.Open();
                            if (GetID.Length > 0)
                            {
                                serialPortWriteLine("");
                                if( EchoOff.Length > 0 ) serialPortWriteLine(EchoOff);
                                if( GetID.Length > 0 ) versionIs = serialPortCommandresponse(GetID);
                                if( versionIs.ToUpper().StartsWith( VersionComboBox.Text.ToUpper() ) )
                                {
                                    VersionComboBox.Enabled = false;
                                    COMportComboBox.Enabled = false;
                                    BaudComboBox.Enabled = false;
                                    ConnectButton.Text = DISCONNECT_LABEL;
                                    HighLightButton(ConnectButton);
                                    this.Text = APP_NAME + " - " + VERSION + " ---> " + versionIs;
                                    if( EchoOn.Length > 0 ) serialPortWriteLine(EchoOn);
                                }
                            }
                            else
                            {
                                VersionComboBox.Enabled = false;
                                COMportComboBox.Enabled = false;
                                BaudComboBox.Enabled = false;
                                ConnectButton.Text = DISCONNECT_LABEL;
                                HighLightButton(ConnectButton);
                                this.Text = APP_NAME + " - " + VERSION + " connected";
                            }
                            CommsTextBox.Focus();
                        }
                        catch
                        {
                            // Don't do anything with this.
                        }
                    }
                }
            }
            else
            {
                COMportComboBox.Enabled = true;
            }
            if( COMportComboBox.Enabled )
            {
                if (SerialPort.IsOpen ) SerialPort.Close();
                VersionComboBox.Enabled = true;
                COMportComboBox.Enabled = true;
                BaudComboBox.Enabled = true;
                ConnectButton.Text = CONNECT_LABEL;
                LowLightButton(ConnectButton);
                this.Text = APP_NAME + " - " + VERSION;
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Scan for available COM ports and populate COMportComboBox.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        ///
        private void scanForAvailableCOMports()
        {
            COMportComboBox.Items.Clear();
            //
            foreach (string name in SerialPort.GetPortNames())
            {
                COMportComboBox.Items.Add(name);
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Clear out any characters hanging around in the ports output buffer.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        /// 
        private void serialPortClearbuffer()
        {
            SerialPort.ReadTimeout = TIMEOUT_IMMEDIATE;
            //
            for (int i = 0; i < TIMEOUT_CLEARS; i++)
            {
                try
                {
                    SerialPort.ReadLine();
                }
                catch
                {
                    break; // Nothing immediately available, so done.
                }
            }
            SerialPort.ReadTimeout = TIMEOUT_NORMAL;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Write a line to the serial port, ignore response.
        /// </summary>
        private void serialPortWriteLine(string command)
        {
            serialPortClearbuffer();
            //
            SerialPort.WriteLine(command);
            Thread.Sleep(INTERVAL_RESPOND);
            //
            SerialPort.ReadTimeout = TIMEOUT_MAY; // Select a short timeout.
            try
            {
                SerialPort.ReadLine();
            }
            catch
            {
                // ignore no response, possibly just no echo.
            }
            SerialPort.ReadTimeout = TIMEOUT_NORMAL;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Read a line from the serial port, but discard any CRLF characters.
        /// </summary>
        private string serialPortReadLine()
        {
            string response;

            Thread.Sleep(INTERVAL_RESPOND);
            response = SerialPort.ReadLine();
            response = response.Replace("\r", "").Replace("\n", "");
            //
            return response;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Send the command and get a response.
        /// </summary>
        /// <param name="command"> to send </param>
        /// <returns>Responding string</returns>
        private string serialPortCommandresponse(string command)
        {
            string response;

            serialPortClearbuffer(); // Make sure nothing is sitting in the pipeline.
            //
            SerialPort.WriteLine(command);
            response = serialPortReadLine();
            //
            return response;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// High light a button as green with white bold text.
        /// </summary>
        /// <param name="highLightButton">The button to be modified.</param>
        /// <param name="highLightColor">The colour to use, typically green or red.</param>
        /// <returns></returns>
        ///
        private void HighLightButton(Button highLightButton)
        {
            highLightButton.BackColor = System.Drawing.Color.LimeGreen;
            highLightButton.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            highLightButton.Font = (new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, (byte)(0)));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Low light a button as red with black regular text.
        /// </summary>
        /// <param name="lowLightButton">The button to be modified.</param>
        /// <returns></returns>
        ///
        private void LowLightButton(Button lowLightButton)
        {
            lowLightButton.BackColor = System.Drawing.Color.LightCoral;
            lowLightButton.ForeColor = System.Drawing.SystemColors.ControlText;
            lowLightButton.Font = (new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte)(0)));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// When ever serial data is received, display it on CommsTextBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int i;

            if ( false == COMportComboBox.Enabled )
            {
                int readLength = SerialPort.BytesToRead;
                var inputs = new byte[readLength];

                SerialPort.Read(inputs, 0, readLength);
                for ( i=0; i<readLength; i++ )
                {
                    if( BS == inputs[i] )
                    {
                        if ( i != 0 )
                        {
                            // Need to handle the characters in front of the BS.
                            //
                            CommsTextBox.AppendText(Encoding.ASCII.GetString(inputs, 0, i));
                            //
                            // And force the byte array to have BS as the first character.
                            //
                            for (int shift = i; shift < readLength; shift++)
                            {
                                inputs[shift - i] = inputs[shift];
                            }
                            readLength -= i;
                            i = 0;
                        }
                        // The BS causes the CommsTextBox to loose a character at the end of the existing content.
                        //
                        if(CommsTextBox.TextLength > 0) CommsTextBox.Text = CommsTextBox.Text.Substring(0, CommsTextBox.TextLength - 1);
                        //
                        for ( int shift=1; shift<readLength; shift++)
                        {
                            inputs[shift - 1] = inputs[shift];
                        }
                        readLength--;
                        i--;
                    }
                }
                if( readLength > 0 ) CommsTextBox.AppendText(Encoding.ASCII.GetString(inputs, 0, readLength));
                //
                CommsTextBox.SelectionStart = CommsTextBox.Text.Length; // Place the curser at the end of the text.
                CommsTextBox.ScrollToCaret();
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// On key being pressed, send it to COM port.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommsTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            var buffer = new byte[1];
            string chr = "";

            if (false == COMportComboBox.Enabled)
            {
                buffer[0] = (byte)e.KeyChar;

                if (buffer[0] < 0x80)
                {
                    SerialPort.Write(buffer, 0, 1);
                    if( HalfDuplexCheckBox.Checked )
                    {
                        if (buffer[0] > 0x1F)
                        {
                            chr = Encoding.ASCII.GetString(buffer, 0, 1);
                        }
                        else
                        {
                            if (0x0D == buffer[0]) chr = " ";
                        }
                        if (chr.Length > 0)
                        {
                            // Need to echo this to the terminal since the serial device isn't going to!
                            //
                            CommsTextBox.AppendText(chr);
                            //
                            CommsTextBox.SelectionStart = CommsTextBox.Text.Length; // Place the curser at the end of the text.
                            CommsTextBox.ScrollToCaret();
                        }
                    }
                }
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// On clicking COM port combo, rescan for COM ports.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void COMportComboBox_DropDown(object sender, EventArgs e)
        {
            scanForAvailableCOMports();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Clear the screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearSreenButton_Click(object sender, EventArgs e)
        {
            CommsTextBox.Text = "";
            CommsTextBox.Focus();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Replace in [input] all instance of [replace] with [withThis], repeat until no [replace] found.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="replace"></param>
        /// <param name="withThis"></param>
        /// <returns></returns>
        private string recursiveReplace(string input, string replace, string withThis)
        {
            while (input.Contains(replace)) input = input.Replace(replace, withThis);
            //
            return input;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Load project information from BOOTLOAD_app.csv file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// 
        private void loadProjectInfo()
        {
            int n, ln;

            if (File.Exists(FILENAME_CSV))
            {
                string[] lines = File.ReadAllLines(FILENAME_CSV);
                //
                for (n = 0; n < MAX_PROJECTS; n++) projects[n] = new project();
                //
                // Retrieve all the saved project and baudrate combinations available.
                // These are also placed into the project names dropdown list for easy of selection.
                //
                // Blank lines and those that start with a ';' are ignored.
                //
                for (ln = 0, n = 0; ln < lines.Length; ln++)
                {
                    lines[ln] = lines[ln].Replace("\t", " ");
                    lines[ln] = recursiveReplace(lines[ln], "  ", " ");
                    lines[ln] = recursiveReplace(lines[ln], ", ", ",");
                    lines[ln] = lines[ln].Trim();
                    //
                    if ((lines[ln].Length > 0) && !lines[ln].StartsWith(";"))
                    {
                        string[] info = lines[ln].Split(',', (char)11);
                        //
                        if (info.Length >= 5)
                        {
                            projects[n].name = info[0];
                            projects[n].baudrate = info[1];
                            projects[n].echoOff = info[2];
                            projects[n].echoOn = info[3];
                            projects[n].getID = info[4];
                            if (info.Length > 5) projects[n].halfDuplex = info[5].ToUpper().StartsWith("Y");
                            //
                            VersionComboBox.Items.Add(projects[n].name);
                            n++;
                            if (MAX_PROJECTS == n) break; // Reached the upper limit for the drop down list.
                        }
                    }
                }
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// If project is changed, then set the baud rate accordingly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VersionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (project project in projects)
            {
                if (VersionComboBox.Text.ToUpper() == project.name.ToUpper() )
                {
                    VersionComboBox.Text = project.name;
                    BaudComboBox.Text = project.baudrate;
                    EchoOff = project.echoOff;
                    EchoOn = project.echoOn;
                    GetID = project.getID;
                    HalfDuplexCheckBox.Checked = project.halfDuplex;
                    //
                    break;
                }
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Load the environment values for project and COM port.
        /// </summary>
        private void LoadEnviroment()
        {
            VersionComboBox.Text = environmentRead("COMport_project", "Unknown");
            COMportComboBox.Text = environmentRead("COMport_connection", "");
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Recover an environment variable if set.
        /// </summary>
        /// <param name="label">Environment variable name</param>
        /// <param name="defaultTo">This if no environment variable set</param>
        /// <returns></returns>
        /// 
        private string environmentRead(string label, string defaultTo)
        {
            string result = Environment.GetEnvironmentVariable(label, EnvironmentVariableTarget.User);

            if (null == result)
            {
                result = defaultTo;
            }
            return result;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Write an environment variable if not blank.
        /// </summary>
        /// <param name="label">Environment variable name</param>
        /// <param name="variable">The string location to save in the environment variable</param>
        /// <returns></returns>
        /// 
        private void environmentWrite(string label, string variable)
        {
            Environment.SetEnvironmentVariable(label, variable, EnvironmentVariableTarget.User);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Save the project and COM port.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void COMportForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            environmentWrite("COMport_project", VersionComboBox.Text);
            environmentWrite("COMport_connection", COMportComboBox.Text);
        }
    }
}
