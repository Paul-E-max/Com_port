// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
// @File:       COMportForm.cs
// @Project:    DISCOVER_COM_port
// @Author:     Foster & Freeman Ltd - Michael Dodd
// @Created:    31.03.2022
//
// @Brief:      Simple COM port communications device for use with DISCOVER.
//
// @Tools:      Visual Studio 2019, C#
//
// @Revision:
//
// 24.02.2023-MD V1.01.09 - 1) Correction to CR from quick text (V1.01.08 didn't use the NL delay on quick text).
//                          2) Each project can save its own Quick Text file.
//                          3) Last connection made for each project is saved in the user's local file.
// 21.02.2023-MD V1.01.08 - Timer now takes decimal interval rather than just whole seconds.
//                          Character and newline delays are updated by selecting a new project.
// 15.02.2023-MD V1.01.07 - Quick Text items now used the onEnterComboBox text for enter.  Note that previous version
//                          might see unexpected '?' from Quick Text item even when the enter key was correctly set.
// 11.02.2023-MD V1.01.06 - Drop environmental values in favour of file storage.
// 10.02.2023-MD V1.01.05 - More option at the mottom of Quick Text menu.
//                          Disconnect if serial device fails.
// 21.10.2022-MD V1.01.04 - Extend the quick message buttons (only put into DevOps on 06.02.2023)
// 11.10.2022-MD V1.01.03 - Correct Execute buttons - oops, 5 to 19 all indexed button 3!
// 07.10.2022-MD V1.01.02 - Add repeat command buttons off the bottom of the quick text menu.
// 06.10.2022-MD V1.01.01 - Extend the number of quick text boxes, change back to Execute button and add log option.
// 06.10.2022-MD V1.01.00 - Move quick text into a sub-menu by itself - this can be positioned anywhere on the screen.
//
// 05.10.2022-MD V1.00.08 - Add quick text boxes hidden off on far right.
// 26.09.2022-MD V1.00.07 - Correction to thread handling fault that showed up with MVC_FFLEX comms!
// 18.08.2022-MD V1.00.06 - Option to use a response to "Version" where it
//                          differs from project name.  Discard version 1.00.05 because
//                          "Half-duplex" is shifted right by the new "Response" parameter.
// 17.08.2022-MD V1.00.05 - Add "Tx on Enter" option.
// 16.08.2022-MD V1.00.04 - Add half-duplex option.
// 13.07.2022-MD            Use CSV file to get identities.
//                          Environment variables can retain COM and project.
// 28.04.2022-MD            Clear screen button added.
// 06.04.2022-MD            Correction to BS when textbox is scrolled!
// 01.04.2022-MD            Handles BS coming from COM port.
// 31.03.2022-MD V1.00.00   Initial version.
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
        const string APP_NAME = "COMport", VERSION = "V1.01.09"; // UPDATE MANUAL AS APPLICATION EVOLVES.
        //
        const string FILENAME_CSV = APP_NAME + ".CSV";
        public const string TEXT_FILE_EXT = ".TXT";
        const string LASTUSED_TXT = APP_NAME + "_USER" + TEXT_FILE_EXT;

        public const string QUICK_TEXT = APP_NAME + "_QUICK";

        const string CONNECT_LABEL = "Connect";
        const string SCANNING_LABEL = "Connecting";
        const string DISCONNECT_LABEL = "Disconnect";

        const string LOG_START_LABEL = "Start log";
        const string LOG_STOP_LABEL = "Stop log";

        const int TIMEOUT_IMMEDIATE = 1;        // Timeout during clearing of buffer.
        const int TIMEOUT_CLEARS = 20;          // Number of times to attempt clear of buffer.
        const int TIMEOUT_NORMAL = 200;         // Normal period to wait for TX or RX to complete (use -1 for debugging, gives infinite period).
        const int TIMEOUT_MAY = 50;             // If not sure that a response is due.
        const int INTERVAL_RESPOND = 175;       // Period between TX and looking for RX (but surely this doesn't need to be this large).

        const byte BS = 0x08;
        const byte LF = 0x0A;
        public byte CR = 0x0D;

        const int MAX_PROJECTS = 20;            // Reads through FILENAME_CSV file, but abandons data beyond this number of projects.

        struct project
        {
            public string name;
            public string baudrate;
            public string echoOff;
            public string echoOn;
            public string getID;
            public string response;
            public bool halfDuplex;
            public string enterKey;
            public string NLDelay;
            public string ChDelay;
            public string COMport;
        };

        project[] projects = new project[MAX_PROJECTS];
        string EchoOff = "";
        string EchoOn = "";
        string GetID = "";
        string Response = "";
        static public string EnterKey = Environment.NewLine;
        static public int InterLineDelay = 0; // Milliseconds delay following an enter sent to target.
        static public int InterCharDelay = 0; // Millisecond delay between characters sent to target.

        string OutputLogFile = "";
        string typedCommandLine = "";
        bool CheckEnvironmentVariables = true;

        QuickTextMenu menu;

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
            LoadLastUsedInfo();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Save the project and COM port.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void COMportForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (File.Exists(LASTUSED_TXT)) File.Delete(LASTUSED_TXT);
            //
            using (StreamWriter output = File.CreateText(LASTUSED_TXT))
            {
                output.WriteLine("project=" + VersionComboBox.Text);
                output.WriteLine("connection=" + COMportComboBox.Text);
                output.WriteLine("baudrate=" + BaudComboBox.Text);
                output.WriteLine("halfDuplex=" + (HalfDuplexCheckBox.Checked ? "True" : "False"));
                output.WriteLine("onEnter=" + onEnterComboBox.Text);
                //
                // Save any and all project/COM port settings recorded.
                //
                for (int n = 0; n < MAX_PROJECTS; n++)
                {
                    if((projects[n].name.Length > 0) && (projects[n].COMport.Length > 0))
                    {
                        output.WriteLine("connection_" + projects[n].name + "=" + projects[n].COMport);
                    }
                }
            }
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
                                // Attempt to connect to a device with a specific response.
                                //
                                serialPortWriteLine("");
                                if( EchoOff.Length > 0 ) serialPortWriteLine(EchoOff);
                                if( GetID.Length > 0 ) versionIs = serialPortCommandresponse(GetID);
                                if( versionIs.ToUpper().StartsWith( Response ) )
                                {
                                    // Found a valid connection !
                                    //
                                    connectedTo(true);
                                    this.Text += " ---> ";
                                    if( VersionComboBox.Text != Response ) this.Text += VersionComboBox.Text + " ";
                                    this.Text += versionIs;
                                    if( EchoOn.Length > 0 ) serialPortWriteLine(EchoOn);
                                }
                            }
                            else
                            {
                                // With no GetID command, just connected to anything!
                                //
                                connectedTo(true);
                                this.Text += " connected";
                                if (EchoOn.Length > 0) serialPortWriteLine(EchoOn);
                            }
                            CommsTextBox.Focus();
                        }
                        catch
                        {
                            // Don't do anything with this.
                        }
                        // If we succeed in connecting, make a not of typical COM port used.
                        //
                        if( false == VersionComboBox.Enabled )
                        {
                            for( int n=0; n<MAX_PROJECTS; n++ )
                            {
                                if( projects[n].name == VersionComboBox.Text )
                                {
                                    projects[n].COMport = COMportComboBox.Text;
                                    //
                                    break;
                                }
                            }
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
                if (SerialPort.IsOpen)
                {
                    SerialPort.DiscardInBuffer();
                    SerialPort.DiscardOutBuffer();
                    //
                    SerialPort.Close();
                }
                connectedTo(false);
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Connect/disconnect to a device.
        /// </summary>
        /// <param name="state">True to connect, false to disconnect</param>
        private void connectedTo(bool state)
        {
            VersionComboBox.Enabled = !state;
            COMportComboBox.Enabled = !state;
            BaudComboBox.Enabled = !state;
            if (state)
            {
                ConnectButton.Text = DISCONNECT_LABEL;
                HighLightButton(ConnectButton);
            }
            else
            {
                ConnectButton.Text = CONNECT_LABEL;
                LowLightButton(ConnectButton);
            }
            this.Text = APP_NAME + " - " + VERSION;
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
                            updateCommsTextBox(Encoding.ASCII.GetString(inputs, 0, i));
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
                        if (CommsTextBox.TextLength > 0)
                        {
                            CommsTextBox.Text = CommsTextBox.Text.Substring(0, CommsTextBox.TextLength - 1);
                            CommsTextBox.SelectionStart = CommsTextBox.Text.Length; // Place the curser at the end of the text.
                            CommsTextBox.ScrollToCaret();
                        }
                        for ( int shift=1; shift<readLength; shift++)
                        {
                            inputs[shift - 1] = inputs[shift];
                        }
                        readLength--;
                        i--;
                    }
                }
                if (readLength > 0)
                {
                    string toDisplay = Encoding.ASCII.GetString(inputs, 0, readLength);
                    //
                    // Replace any instances of EnterKey with the standard CRLF sequence used
                    // by the environment.
                    //
                    for( i=0; i<(toDisplay.Length - EnterKey.Length + 1); i++ )
                    {
                        if( toDisplay.Substring(i).StartsWith(EnterKey) )
                        {
                            toDisplay = toDisplay.Substring(0, i) + Environment.NewLine + toDisplay.Substring(i + EnterKey.Length);
                            i += (Environment.NewLine.Length - 1);
                        }
                    }
                    updateCommsTextBox(toDisplay);
                }
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Update the comms text box with a new string to be appended.
        /// </summary>
        /// <param name="append"></param>
        void updateCommsTextBox( string append )
        {
            if( InvokeRequired )
            {
                Invoke(new Action<string>(updateCommsTextBox), new object[] { append });
            }
            else
            {
                int oldLength = CommsTextBox.TextLength;
                CommsTextBox.AppendText(append);
    CommsTextBox.SelectionStart = CommsTextBox.Text.Length; // Place the curser at the end of the text.
    CommsTextBox.ScrollToCaret();
                //
                if ( OutputLogFile.Length > 0 )
                {
                    using (StreamWriter logFile = File.AppendText(OutputLogFile))
                    {
                        logFile.Write(append);
                    }
                }
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
            var TxBuffer = new byte[1];
            byte RxChar;
            string chr = "";

            if (false == COMportComboBox.Enabled)
            {
                RxChar = (byte)e.KeyChar;
                TxBuffer[0] = RxChar;

                if (RxChar < 0x80)
                {
                    if (CR == RxChar)
                    {
                        // ENTER has been hit, use the correct value for this connected device.
                        //
                        try
                        {
                            TxBuffer = Encoding.ASCII.GetBytes(EnterKey);
                        }
                        catch
                        {
                            // Just ignore silly items in TxEnter text box.
                        }
                    }
                    try
                    {
                        // Attempt to send this out to the serial device - hope it is still connected.
                        //
                        SerialPort.Write(TxBuffer, 0, 1);
                        //
                        // Enforce any requested delays for characters or new lines.
                        //
                        if (Encoding.ASCII.GetBytes(EnterKey) == TxBuffer)
                        {
                            if( InterLineDelay > 0 )
                            {
                                Thread.Sleep(InterLineDelay);
                            }
                        }
                        else
                        {
                            if (InterCharDelay > 0)
                            {
                                Thread.Sleep(InterCharDelay);
                            }
                        }
                    }
                    catch
                    {
                        // Failed to talk so close that port and apologise for the break in communication.
                        //
                        SerialPort.Close();
                        connectedTo(false);
                        CommsTextBox.AppendText("\r\nERROR: lost connection\r\n\r\n");
                        RxChar = 0; // Effectively killing off whatever was typed in.
                    }
                    if ( HalfDuplexCheckBox.Checked )
                    {
                        // Half-duplex requires printable characters to be displayed on behalf of the
                        // connected device, since it does not generate any echo'd characters.
                        //
                        if (RxChar > 0x1F)
                        {
                            chr = Encoding.ASCII.GetString(TxBuffer, 0, 1);
                        }
                        else
                        {
                            // The ENTER key just needs a space to separate the typed input from the
                            // connected device's output.
                            //
                            if (CR == RxChar) chr = " ";
                        }
                        if (chr.Length > 0)
                        {
                            // Need to echo this to the terminal since the serial device isn't going to!
                            //
                            CommsTextBox.AppendText(chr);
                            typedCommandLine += chr;
                            if( CR == RxChar )
                            {
                                if (OutputLogFile.Length > 0)
                                {
                                    using (StreamWriter logFile = File.AppendText(OutputLogFile))
                                    {
                                        logFile.Write(typedCommandLine);
                                    }
                                }
                                typedCommandLine = "";
                            }
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
                // Start off with no entries filled in.
                //
                for (n = 0; n < MAX_PROJECTS; n++)
                {
                    projects[n] = new project();
                    projects[n].name = "";
                    projects[n].baudrate = "";
                    projects[n].echoOff = "";
                    projects[n].echoOn = "";
                    projects[n].getID = "";
                    projects[n].response = "";
                    projects[n].halfDuplex = false;
                    projects[n].enterKey = "";
                    projects[n].NLDelay = "";
                    projects[n].COMport = ""; // To be filled in by LoadLastUsedInfo().
                }
                // Retrieve all the saved project and baudrate combinations available.
                // These are also placed into the project names dropdown list for easy of selection.
                //
                string[] lines = File.ReadAllLines(FILENAME_CSV);
                //
                for (ln = 0, n = 0; ln < lines.Length; ln++)
                {
                    lines[ln] = lines[ln].Replace("\t", " ");
                    lines[ln] = recursiveReplace(lines[ln], "  ", " ");
                    lines[ln] = recursiveReplace(lines[ln], ", ", ",");
                    lines[ln] = lines[ln].Trim();
                    //
                    // Blank lines and those that start with a ';' are ignored.
                    //
                    if ((lines[ln].Length > 0) && !lines[ln].StartsWith(";"))
                    {
                        // Something left on the line to look at and it isn't a comment.
                        //
                        string[] info = lines[ln].Split(',', (char)11);
                        //
                        if (info.Length > 0) projects[n].name = info[0];
                        if (info.Length > 1) projects[n].baudrate = info[1];
                        if (info.Length > 2) projects[n].echoOff = info[2];
                        if (info.Length > 3) projects[n].echoOn = info[3];
                        if (info.Length > 4) projects[n].getID = info[4];
                        if (info.Length > 5) projects[n].response = info[5];
                        if (info.Length > 6) projects[n].halfDuplex = info[6].ToUpper().StartsWith("Y");
                        if (info.Length > 7) projects[n].enterKey = info[7];
                        if (info.Length > 8) projects[n].NLDelay = info[8];
                        if (info.Length > 9) projects[n].ChDelay = info[9];
                        //
                        VersionComboBox.Items.Add(projects[n].name);
                        n++;
                        if (MAX_PROJECTS == n) break; // Reached the upper limit for the drop down list.
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
                    Response = project.response;
                    if (0 == Response.Length) Response = VersionComboBox.Text;
                    Response = Response.ToUpper();
                    HalfDuplexCheckBox.Checked = project.halfDuplex;
                    onEnterComboBox.Text = project.enterKey;
                    try
                    {
                        InterLineDelay = Convert.ToInt32(project.NLDelay);
                    }
                    catch
                    {
                        InterLineDelay = 0;
                    }
                    try
                    {
                        InterCharDelay = Convert.ToInt32(project.ChDelay);
                    }
                    catch
                    {
                        InterCharDelay = 0;
                    }
                    // Update the QuickText parameters too if open.
                    //
                    if ( Application.OpenForms.OfType<QuickTextMenu>().Count() > 0 )
                    {
                        menu.NLDelayTextBox.Text = InterLineDelay.ToString();
                        menu.CharDelayTextBox.Text = InterCharDelay.ToString();
                    }
                    // If the project has been used with a COM port in the past, set it here.
                    //
                    if (project.COMport.Length > 0) COMportComboBox.Text = project.COMport;
                    //
                    break;
                }
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Load the last used settings.
        /// </summary>
        private void LoadLastUsedInfo()
        {
            string[] lines = new string[0];

            if (File.Exists(LASTUSED_TXT))
            {
                lines = File.ReadAllLines(LASTUSED_TXT);
                if (lines.Length > 0) CheckEnvironmentVariables = false;
            }
            VersionComboBox.Text = findParameterIn(lines, "project", "Unknown");
            COMportComboBox.Text = findParameterIn(lines, "connection", "");
            HalfDuplexCheckBox.Checked = ("True" == findParameterIn(lines, "halfDuplex", "false"));
            BaudComboBox.Text = findParameterIn(lines, "baudrate", "");
            onEnterComboBox.Text = findParameterIn(lines, "onEnter", "");
            //
            checkEnterChar(); // Update EnterKey with onEnterCombox.Text
            //
            // Load any and all project/COM port settings recorded.
            //
            for (int n = 0; n < MAX_PROJECTS; n++)
            {
                if (projects[n].name.Length > 0)
                {
                    projects[n].COMport = findParameterIn(lines, "connection_" + projects[n].name, "");
                }
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Recover a variable if set in the LASTUSED_TXT file.
        /// </summary>
        /// <param name="lines">Source data array</param>
        /// <param name="label">Parameter's label</param>
        /// <param name="defaultTo">This if label not found</param>
        /// <returns></returns>
        /// 
        string findParameterIn(string[] lines, string label, string defaultTo)
        {
            string result = "";
            int index;

            foreach (string line in lines)
            {
                index = line.IndexOf('=');
                //
                if( ( index > 0 ) && ( index < line.Length ) )
                {
                    if( label == line.Substring(0,index) )
                    {
                        result = line.Substring(index + 1);
                        //
                        break;
                    }
                }
            }
            // If still not got a value, there might be something available in the environment variables.
            //
            if ((0 == result.Length) && CheckEnvironmentVariables)
            {
                result = Environment.GetEnvironmentVariable("COMport_" + label, EnvironmentVariableTarget.User);
            }
            if ((null == result) || (0 == result.Length))
            {
                result = defaultTo;
            }
            return result;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// If the Tx on ENTER changes, update the EnterKey value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onEnterComboBox_TextChanged(object sender, EventArgs e)
        {
            EnterKey = Environment.NewLine; // Assume default value if not set.

            if (onEnterComboBox.Text.StartsWith("0x"))
            {
                checkEnterChar();
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Any incoming characters of type onEnterComboBox.Text should be replaced by CR.
        /// </summary>
        private void checkEnterChar()
        {
            try
            {
                EnterKey = Convert.ToChar(Convert.ToUInt32(onEnterComboBox.Text.Substring(2), 16)).ToString();
            }
            catch
            {
                // Just ignore silly items in TxEnter text box, defaults to environment new line string.
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Place the cursor at the end of the text box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommsTextBox_TextChanged(object sender, EventArgs e)
        {
            CommsTextBox.SelectionStart = CommsTextBox.Text.Length; // Place the curser at the end of the text.
            CommsTextBox.ScrollToCaret();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Send a text strings to the keyboard buffer, doing newline with '\n'.
        /// </summary>
        /// <param name="toSend"></param>
        public void sendLinesToKeyboard(string toSend)
        {
            string aline;
            int idx;

            while (toSend.Contains("\\n")) // Note that Environment.NewLine should not be equal to "\\n" (only the strangest of stange people would set it so)!
            {
                idx = toSend.IndexOf("\\n");
                aline = toSend.Substring(0, idx);
                toSend = toSend.Substring(idx + 2);
                //
                sendToKeyboard(aline + Environment.NewLine);
            }
            if (toSend.Length > 0) sendToKeyboard(toSend);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Pump a single string to the keyboard buffer of CommTextBox.
        /// </summary>
        /// <param name="toSend"></param>
        private void sendToKeyboard( string toSend )
        {
            object sender = null;
            KeyPressEventArgs e = new KeyPressEventArgs((char)Keys.Enter);

            for (int idx = 0; idx < toSend.Length; idx++)
            {
                e.KeyChar = Convert.ToChar(toSend.Substring(idx, 1));
                CommsTextBox_KeyPress(sender, e);
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Display the quick text menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuickTextMenuButton_Click(object sender, EventArgs e)
        {
            if ( 0 == Application.OpenForms.OfType<QuickTextMenu>().Count() )
            {
                menu = new QuickTextMenu();
                menu.StartPosition = FormStartPosition.Manual;
                menu.Location = Location;
                menu.Left += ClientSize.Width + 10; // To place it on far right of parent.
                //
                menu.Show();
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Start or restart a log file - if already logging, then stop that one and
        /// start a new one with current timestamp.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartLogButton_Click(object sender, EventArgs e)
        {
            if (LOG_START_LABEL == StartLogButton.Text)
            {
                OutputLogFile = APP_NAME + DateTime.Now.ToString("_yyyyMMdd_HHmmss") + ".LOG";
                try
                {
                    using (StreamWriter LogFile = File.CreateText(OutputLogFile))
                    {
                        // Just need to ensure that it has been created . . .
                    }
                    StartLogButton.Text = LOG_STOP_LABEL;
                }
                catch
                {
                    OutputLogFile = "";
                    StartLogButton.Text = LOG_START_LABEL;
                }
            }
            else
            {
                OutputLogFile = "";
                StartLogButton.Text = LOG_START_LABEL;
            }
        }
    }
}
