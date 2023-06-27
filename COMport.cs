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
// 27.06.2023-MD V1.01.18 - Right-click "Tx on Enter" for auto-new-line on the display output.
// 07.06.2023-MD V1.01.17 - Embed the FTDI DLL within EXE file.
// 02.06.2023-MD V1.01.16 - 1) Add D2XX support (specifically for VSC900, but other D2XX projects are applicable).
//                          2) Correction to right click and accept button.
// 01.06.2023-MD V1.01.15 - 1) Use get ID command rather than blank line to establish a command line connection (blank line offens VSC900).
//                          2) Correction for when logging is active, was sending "\r\n" as newline instead of CR . . .
// 04.05.2023-MD V1.01.14 - If only one item in a scan is found, then use this in preference to value in COMport_USER.TXT
// 25.04.2023-MD V1.01.13 - Correction to Quick Text menu with equals character in it.
// 23.04.2023-MD V1.01.12 - 1) Implement Ctrl+C and Ctrl+V for copy and paste.
//                          2) Rename COMport.csv to COMport.TXT
//                          3) Stop that ding on Quick text menu ENTER!
// 21.04.2023-MD V1.01.12 - 1) Drop the old enviroment variables altogether.
//                          2) Improve inter-character delay handling.
// 21.04.2023-MD V1.01.11 - Switch the COMport_quickTextName_project.TXT to COMport_project_quickTextName.TXT
// 18.04.2023-MD V1.01.10 - 1) Generalise the ButtonExeText function for all 1-19 buttons.
//                          2) Changed QUICK text menu button to drop down to select a file (currently COMport_quickTextName_project.TXT).
//                          3) Label on execute buttons in quick text can be modified.
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
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Reflection;

namespace COMport
{
    public partial class COMport : Form
    {
        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        // Constants
        //
        const string APP_NAME = "COMport", VERSION = "V1.01.18"; // UPDATE MANUALLY AS APPLICATION EVOLVES.
        //
        public const string TEXT_FILE_EXT = ".TXT";
        const string LASTUSED_TXT = APP_NAME + "_USER" + TEXT_FILE_EXT;

        const string CONNECT_LABEL = "Connect";
        const string SCANNING_LABEL = "Connecting";
        const string DISCONNECT_LABEL = "Disconnect";

        const string LOG_START_LABEL = "Start log";
        const string LOG_STOP_LABEL = "Stop log";
        const string NEW_SHEET = "new sheet";

        const int TIMEOUT_IMMEDIATE = 1;        // Timeout during clearing of buffer.
        const int TIMEOUT_CLEARS = 20;          // Number of times to attempt clear of buffer.
        const int TIMEOUT_NORMAL = 200;         // Normal period to wait for TX or RX to complete (use -1 for debugging, gives infinite period).
        const int TIMEOUT_MAY = 50;             // If not sure that a response is due.
        const int INTERVAL_RESPOND = 175;       // Period between TX and looking for RX (but surely this doesn't need to be this large).
        const int MAY_TIMEOUT = 25;             // If not sure that a response is due.

        const byte BS = 0x08;
        const byte LF = 0x0A;
        public byte CR = 0x0D;

        const string FILENAME_CSV = APP_NAME + ".CSV";              // Obsolite project filename (don't like using CSV extention).
        const string FILENAME_PROJECTS = APP_NAME + TEXT_FILE_EXT;  // New projects filename.
        const int MAX_PROJECTS = 20;                                // Reads through FILENAME_PROJECTS file, but abandons data beyond this number of projects.

        struct project
        {
            public string name;
            public string baudrate;
            public string echoOff;
            public string echoOn;
            public string getID;
            public string response;
            public CheckState halfDuplex;
            public string enterKey;
            public string NLDelay;
            public string ChDelay;
            public string COMport;
        };

        project[] projects = new project[MAX_PROJECTS];
        string EchoOff = "";
        string EchoOn = "";
        string GetID = "";
        string ExpectedResponse = ""; // Expected response updated by VersionComboBox_SelectedIndexChanged().
        static public string EnterKey = Environment.NewLine;
        static public int InterLineDelay = 0; // Milliseconds delay following an enter sent to target.
        static public int InterCharDelay = 0; // Millisecond delay between characters sent to target.

        string OutputLogFile = "";
        string typedCommandLine = "";
        bool DoingDropDown = false;

        // Sending the characters out requires a ring buffer to pace them out with a timer
        // when character and/or new line delays are required.
        //
        const int RING_BUFFER_SIZE = 256;

        byte[] RingBuffer = new byte[RING_BUFFER_SIZE];
        byte input_ptr = 0;
        byte output_ptr = 0;

        string OriginalBaudRate = "";                       // Keeps a record of baudrate while D2XX is selected.

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        // FTDI specific items
        //
        const string D2XX_SELECTION = "D2XX";
        const bool FTDI_VCP = false;
        const bool FTDI_D2XX = true;
        //
        bool FTDI_mode = FTDI_VCP;
        //
        // This is the global instance of D2XX, but need to delay the initialisation of it until
        // after DLL has been loaded, see COMportForm_Load() function . . .
        //
        D2XX D2xxDevice;

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Main entry point into COMport application code.
        /// </summary>
        public COMport()
        {
            LoadDLLfiles();
            InitializeComponent();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Load any DLL files required.
        /// </summary>
        private void LoadDLLfiles()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string resourceName = new AssemblyName(args.Name).Name + ".dll";
                string resource = Array.Find(this.GetType().Assembly.GetManifestResourceNames(), element => element.EndsWith(resourceName));

                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// This occurs before the user sees any response on the screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void COMportForm_Load(object sender, EventArgs e)
        {
            D2xxDevice = new D2XX();        // Need to delay the actual initialisation of D2XX until after the FTDI DLL has been loaded!
            //
            this.Text = APP_NAME + " - " + VERSION;
            loadProjectInfo();              // Fills in any project information available, but doesn't affect the GUI selections.
            LoadLastUsedInfo();
            populateQuickTextComboBox();
            StopLogButton();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Once the frame is displayed, it is OK to fill in details.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void COMportForm_Shown(object sender, EventArgs e)
        {
            if (FTDI_D2XX == FTDI_mode)
            {
                ConnectButton.PerformClick(); // Any D2XX device is worth a go.
            }
            else
            {
                scanForAvailableCOMports();
                //
                if (1 == COMportComboBox.Items.Count)
                {
                    // There is only one COM port available, make a note of the original COM port selected by
                    // the USER file and attempt to connect to the one available COM port.  If it fails, then
                    // just restore the original COM port saved above.
                    //
                    string originalCOMport = COMportComboBox.Text;

                    COMportComboBox.Text = COMportComboBox.Items[0].ToString();
                    ConnectButton.PerformClick();
                    if (VersionComboBox.Enabled)
                    {
                        // When version box is enabled, it means the connection failed, so restore the COM port.
                        //
                        if (originalCOMport.Length > 0)
                        {
                            COMportComboBox.Text = originalCOMport;
                        }
                    }
                }
                else
                {
                    // If there is a COM port defined by USER file, try to connect to it.  If not, just display
                    // the first item in the list.
                    //
                    if (COMportComboBox.Text.Length > 0)
                    {
                        // The USER file had a COM port that is plugged in, so lets try and connect.
                        //
                        ConnectButton.PerformClick();
                    }
                    else
                    {
                        // Guess the first COM port in the list is the one.
                        //
                        if (COMportComboBox.Items.Count > 0) COMportComboBox.Text = COMportComboBox.Items[0].ToString();
                    }
                }
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Before closing the main form, make sure all unsaved menus are sorted.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void COMportForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool closingMore = true;

            while (closingMore)
            {
                closingMore = false;
                //
                foreach (QuickTextMenu menu in Application.OpenForms.OfType<QuickTextMenu>())
                {
                    menu.Close();
                    while (menu.IsAccessible) /* wait here for the menu to actually close . . . */;
                    closingMore = true;
                    //
                    break;
                }
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Save the project and COM port.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void COMportForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (File.Exists(LASTUSED_TXT)) File.Delete(LASTUSED_TXT);
                //
                using (StreamWriter output = File.CreateText(LASTUSED_TXT))
                {
                    output.WriteLine("project=" + VersionComboBox.Text);
                    output.WriteLine("connection=" + COMportComboBox.Text);
                    output.WriteLine("baudrate=" + BaudComboBox.Text);
                    output.WriteLine("halfDuplex=" + ((CheckState.Indeterminate == HalfDuplexCheckBox.CheckState) ? "Indeterminate" : HalfDuplexCheckBox.Checked ? "True" : "False"));
                    output.WriteLine("onEnter=" + onEnterComboBox.Text);
                    output.WriteLine("toolTips=" + (ToolTipsCheckBox.Checked ? "True" : "False"));
                    //
                    // Save any and all project/COM port settings recorded.
                    //
                    for (int n = 0; n < MAX_PROJECTS; n++)
                    {
                        if ((projects[n].name.Length > 0) && (projects[n].COMport.Length > 0))
                        {
                            output.WriteLine("connection_" + projects[n].name + "=" + projects[n].COMport);
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("ERROR: Failed to save user settings in " + LASTUSED_TXT);
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
                // CONNECTING TO A PORT
                // --------------------
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
                    // Attempt to connect to the selected port.
                    //
                    if ((COMportComboBox.Text.Substring(0, 3) == "COM") || (FTDI_D2XX == FTDI_mode))
                    {
                        try // The following is sensitive to communication errors, and will abandon the task if one occurs.
                        {
                            Port_Close();
                            //
                            if (FTDI_D2XX == FTDI_mode)
                            {
                                D2xxDevice.OpenBySerial(VersionComboBox.Text);
                                D2XX_RXcharacterTimer.Enabled = true;
                            }
                            else
                            {
                                SerialPort.PortName = COMportComboBox.Text;
                                SerialPort.ReadTimeout = TIMEOUT_NORMAL;
                                SerialPort.WriteTimeout = TIMEOUT_NORMAL;
                                SerialPort.BaudRate = Convert.ToInt32(BaudComboBox.Text);
                                SerialPort.Open();
                            }
                            if (GetID.Length > 0)
                            {
                                // Attempt to connect to a device with a specific response.
                                //
                                Port_WritePauseAndDiscard(GetID); // This is just sent to establish a connection and ensure last character sent was ENTER.
                                //
                                if( EchoOff.Length > 0 ) Port_WritePauseAndDiscard(EchoOff);
                                if( GetID.Length > 0 ) versionIs = Port_WriteAndRespond(GetID, "versionIs");
                                if( versionIs.ToUpper().StartsWith( ExpectedResponse ) )
                                {
                                    // Found a valid connection !
                                    //
                                    connectedTo(true);
                                    this.Text += " ---> ";
                                    if( VersionComboBox.Text != ExpectedResponse ) this.Text += VersionComboBox.Text + " ";
                                    this.Text += versionIs;
                                    if( EchoOn.Length > 0 ) Port_WritePauseAndDiscard(EchoOn);
                                }
                            }
                            else
                            {
                                // With no GetID command, just connected to anything!
                                //
                                connectedTo(true);
                                this.Text += " connected";
                                if (EchoOn.Length > 0) Port_WritePauseAndDiscard(EchoOn);
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
                // DISCONNECTING PORT
                // ------------------
                //
                if (FTDI_D2XX == FTDI_mode)
                {
                    D2xxDevice.Close();
                    D2XX_RXcharacterTimer.Enabled = false;
                }
                else
                {
                    COMportComboBox.Enabled = true; // Indicates the the connection is not made (for VCP only).
                }
            }
            if (FTDI_D2XX == FTDI_mode)
            {
                if (!D2xxDevice.IsConnected) connectedTo(false);
            }
            else
            {
                if (COMportComboBox.Enabled)
                {
                    if (SerialPort.IsOpen)
                    {
                        SerialPort.DiscardInBuffer();
                        SerialPort.DiscardOutBuffer();
                        //
                        Port_Close();
                    }
                    connectedTo(false);
                }
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
            // Check for D2XX devices.
            //
            if (D2xxDevice.ScanD2XX() > 0) COMportComboBox.Items.Add(D2XX_SELECTION);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// High light a button as green with white bold text.
        /// </summary>
        /// <param name="highLightButton">The button to be modified.</param>
        /// <param name="highLightColor">The colour to use, typically green or red.</param>
        /// <returns></returns>
        ///
        private void HighLightButton(System.Windows.Forms.Button highLightButton)
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
        private void LowLightButton(System.Windows.Forms.Button lowLightButton)
        {
            lowLightButton.BackColor = System.Drawing.Color.LightCoral;
            lowLightButton.ForeColor = System.Drawing.SystemColors.ControlText;
            lowLightButton.Font = (new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte)(0)));
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Process the received characters from the input device.
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="readLength"></param>
        private void processRXcharacters( byte[] inputs, int readLength )
        {
            int i;

            for (i = 0; i < readLength; i++)
            {
                if (BS == inputs[i])
                {
                    if (i != 0)
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
                    for (int shift = 1; shift < readLength; shift++)
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
                for (i = 0; i < (toDisplay.Length - EnterKey.Length + 1); i++)
                {
                    if (toDisplay.Substring(i).StartsWith(EnterKey))
                    {
                        toDisplay = toDisplay.Substring(0, i) + Environment.NewLine + toDisplay.Substring(i + EnterKey.Length);
                        i += (Environment.NewLine.Length - 1);
                    }
                }
                updateCommsTextBox(toDisplay);
                //
                if (CheckState.Indeterminate == HalfDuplexCheckBox.CheckState)
                {
                    updateCommsTextBox("\r\n");
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
        /// Appears that Ctrl_C and Ctrl+V can only be captured in the KeyUP handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommsTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyValue == 'C')
                {
                    // Copy the selected text into the clipboard.
                    //
                    Clipboard.SetText(CommsTextBox.SelectedText);
                    //
                    e.Handled = true;
                }
                if (e.KeyValue == 'V')
                {
                    // Paste the clipboard into the keyboard input stream.
                    //
                    sendToKeyboard(Clipboard.GetText());
                    //
                    e.Handled = true;
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
            byte KeyboardChar = (byte)e.KeyChar;

            // If the character is printable and the ring buffer would not overflow . . .
            //
            if ((KeyboardChar < 0x80) && ((input_ptr + 1) != output_ptr))
            {
                RingBuffer[input_ptr++] = KeyboardChar; // Place the new character into the ring buffer.
                handleRingBuffer();                     // Deal with any idle characers in the ring buffer.
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Deal with any idle characers in the ring buffer.
        /// </summary>
        private void handleRingBuffer()
        {
            while(( input_ptr != output_ptr ) && ( false == TXcharacterTimer.Enabled ))
            {
                // There is currently no inter-character delay in progress, so just go a head and send the characters.
                //
                sendToDevice(RingBuffer[output_ptr]);
                //
                // If the inter-character delays are non-zero, start the character timer going ready to trigger the next one.
                //
                if (CR == RingBuffer[output_ptr])
                {
                    if (InterLineDelay > 0)
                    {
                        TXcharacterTimer.Interval = InterLineDelay;
                        TXcharacterTimer.Start();
                    }
                }
                else
                {
                    if (InterCharDelay > 0)
                    {
                        TXcharacterTimer.Interval = InterCharDelay;
                        TXcharacterTimer.Start();
                    }
                }
                output_ptr++;
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Timer is kicking off the next character.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TXcharacterTimer_Tick(object sender, EventArgs e)
        {
            TXcharacterTimer.Stop(); // Each tick could be the last, it depends upon what other characters are waiting about.
            handleRingBuffer();    // Deal with any idle characers in the ring buffer.
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Send a byte to the connected device . . . may just end up in a buffer of course!
        /// </summary>
        /// <param name="keyboardChar"></param>
        private void sendToDevice(byte keyboardChar)
        {
            var TxBuffer = new byte[1]; // SerialPort.Write uses an array of bytes to transmit.

            if (false == COMportComboBox.Enabled)
            {
                TxBuffer[0] = keyboardChar;

                if (keyboardChar < 0x80)
                {
                    if (CR == keyboardChar)
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
                        Port_Write(TxBuffer, 0, 1);
                    }
                    catch
                    {
                        // Failed to talk so close that port and apologise for the break in communication.
                        //
                        Port_Close();
                        connectedTo(false);
                        CommsTextBox.AppendText("\r\nERROR: lost connection\r\n\r\n");
                        keyboardChar = 0; // Effectively discarding the character.
                    }
                    if (HalfDuplexCheckBox.Checked)
                    {
                        // Half-duplex requires printable characters to be displayed on behalf of the
                        // connected device, since it does not generate any echo'd characters.
                        //
                        string chr = "";

                        if (keyboardChar > 0x1F)
                        {
                            chr = Encoding.ASCII.GetString(TxBuffer, 0, 1);
                        }
                        else
                        {
                            // Carriage return (ENTER key) just needs a space to separate the typed input from the connected device's response.
                            //
                            if (CR == keyboardChar) chr = " ";
                        }
                        if (chr.Length > 0)
                        {
                            // Need to echo this to the terminal since the serial device isn't going to!
                            //
                            CommsTextBox.AppendText(chr);
                            typedCommandLine += chr;
                            //
                            if (CR == keyboardChar)
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
        /// Check for D2XX and disable/enable baudrate accordingly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void COMportComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateFTDImode();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void COMportComboBox_Leave(object sender, EventArgs e)
        {
            updateFTDImode();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Switch between the two FTDI modes . . .
        /// </summary>
        private void updateFTDImode()
        {
            if (D2XX_SELECTION == COMportComboBox.Text)
            {
                if ("N/A" != BaudComboBox.Text) OriginalBaudRate = BaudComboBox.Text;
                //
                BaudComboBox.Text = "N/A";
                BaudComboBox.Enabled = false;
                //
                FTDI_mode = FTDI_D2XX;
            }
            if (COMportComboBox.Text.StartsWith("COM"))
            {
                if (("N/A" == BaudComboBox.Text) && (OriginalBaudRate.Length > 0)) BaudComboBox.Text = OriginalBaudRate;
                BaudComboBox.Enabled = true;
                //
                FTDI_mode = FTDI_VCP;
            }
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
                projects[n].halfDuplex = CheckState.Unchecked;
                projects[n].enterKey = "";
                projects[n].NLDelay = "";
                projects[n].COMport = ""; // To be filled in by LoadLastUsedInfo().
            }
            // Want to encourage the use of the new project file name.
            //
            if (File.Exists(FILENAME_CSV) && !File.Exists(FILENAME_PROJECTS)) File.Move(FILENAME_CSV, FILENAME_PROJECTS);
            //
            if (File.Exists(FILENAME_PROJECTS))
            {
                // Retrieve all the saved project and baudrate combinations available.
                // These are also placed into the project names dropdown list for easy of selection.
                //
                string[] lines = File.ReadAllLines(FILENAME_PROJECTS);
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
                        if (info.Length > 6)
                        {
                            string halfDuplex = info[6].ToUpper();
                            
                            if ("YES" == halfDuplex)
                            {
                                projects[n].halfDuplex = CheckState.Checked;
                            }
                            else if( "Y/N" == halfDuplex )
                            {
                                projects[n].halfDuplex = CheckState.Indeterminate;
                            }
                            else
                            {
                                projects[n].halfDuplex = CheckState.Unchecked;
                            }
                        }
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
        /// If project is changed, then set the optional parameters accordingly.
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
                    //
                    // Baudrate field may indicate that this project uses D2XX mode only.
                    //
                    if (D2XX_SELECTION == project.baudrate) setModeToD2XX(); else setModeToVCP(project.COMport, project.baudrate);
                    EchoOff = project.echoOff;
                    EchoOn = project.echoOn;
                    GetID = project.getID;
                    ExpectedResponse = project.response;
                    if (0 == ExpectedResponse.Length) ExpectedResponse = VersionComboBox.Text;
                    ExpectedResponse = ExpectedResponse.ToUpper();
                    HalfDuplexCheckBox.CheckState = project.halfDuplex;
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
                    foreach( QuickTextMenu menu in Application.OpenForms.OfType<QuickTextMenu>() )
                    {
                        menu.NLDelayTextBox.Text = InterLineDelay.ToString();
                        menu.CharDelayTextBox.Text = InterCharDelay.ToString();
                    }
                    break;
                }
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Set the mode to D2XX.
        /// </summary>
        private void setModeToD2XX()
        {
            FTDI_mode = FTDI_D2XX;
            //
            BaudComboBox.Text = "N/A";
            BaudComboBox.Enabled = false; // Prevent user from modifying this.
            COMportComboBox.Text = D2XX_SELECTION;
            COMportComboBox.Enabled = false; // and indicate that this is D2XX rather than a COM port.
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Set the mode to Virtual COM Port.
        /// </summary>
        /// <param name="COMport"></param>
        private void setModeToVCP(string COMport, string baudrate)
        {
            FTDI_mode = FTDI_VCP;
            //
            BaudComboBox.Text = baudrate;
            BaudComboBox.Enabled = true; // Allow user to modify the baudrate.
            //
            // If the project has been used with a COM port in the past, set it here.
            //
            if (COMport.Length > 0)
            {
                COMportComboBox.Text = COMport;
            }
            else
            {
                COMportComboBox.Text = "N/A";
            }
            COMportComboBox.Enabled = true; // Allow user to modify the COM port selected.
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Load the last used settings.
        /// </summary>
        private void LoadLastUsedInfo()
        {
            string[] lines = new string[0];
            string userCOMport;
            string userBaudrate;
            string halfDuplex;

            if (File.Exists(LASTUSED_TXT)) lines = File.ReadAllLines(LASTUSED_TXT);
            //
            VersionComboBox.Text = findParameterIn(lines, "project", "Unknown");
            userCOMport = findParameterIn(lines, "connection", "");
            halfDuplex = findParameterIn(lines, "halfDuplex", "false");
            if ("True" == halfDuplex)
            {
                HalfDuplexCheckBox.Checked = true;
            }
            else if ("Indeterminate" == halfDuplex)
            {
                HalfDuplexCheckBox.CheckState = CheckState.Indeterminate;
            }
            userBaudrate = findParameterIn(lines, "baudrate", "");
            onEnterComboBox.Text = findParameterIn(lines, "onEnter", "");
            ToolTipsCheckBox.Checked = ("True" == findParameterIn(lines, "toolTips", "True"));
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
            if(D2XX_SELECTION == userCOMport) setModeToD2XX(); else setModeToVCP(userCOMport, userBaudrate);
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
            string result = defaultTo; // If label not found, assume this value.
            int indexToDelimiter;

            foreach (string line in lines)
            {
                indexToDelimiter = line.IndexOf('=');
                //
                if( ( indexToDelimiter > 0 ) && ( indexToDelimiter < line.Length ) )
                {
                    if( label == line.Substring(0,indexToDelimiter) )
                    {
                        result = line.Substring(indexToDelimiter + 1);
                        //
                        break;
                    }
                }
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
            CommsTextBox.Focus(); // Assume once this control has been selected that we need to type on the console.
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
        /// Send a text strings to the keyboard buffer, doing newline if not manual enter.
        /// </summary>
        /// <param name="toSend"></param>
        /// <param name="manualEnter"></param>
        public void sendLinesToKeyboard(string toSend, bool manualEnter)
        {
            string aline;
            int idx;

            if (false == manualEnter) toSend += "\\n";
            //
            while (toSend.Contains("\\n")) // Note that Environment.NewLine should not be equal to "\\n" (only the strangest of stange people would set it so)!
            {
                idx = toSend.IndexOf("\\n");
                aline = toSend.Substring(0, idx);
                toSend = toSend.Substring(idx + 2);
                //
                sendToKeyboard(aline + Convert.ToChar(CR)); // The CR will be converted within handleRingBuffer() further on down stream.
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
            KeyPressEventArgs e = new KeyPressEventArgs((char)Keys.Enter); // Set this to a bogus value that is then replaced within the following for loop.

            for (int idx = 0; idx < toSend.Length; idx++)
            {
                e.KeyChar = Convert.ToChar(toSend.Substring(idx, 1));
                CommsTextBox_KeyPress(sender, e);
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Populate dropdown with available files and indicate that we are starting the dropdown process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuickTextComboBox_DropDown(object sender, EventArgs e)
        {
            populateQuickTextComboBox();
            DoingDropDown = true;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Indicate that we have finished dropdown process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuickTextComboBox_DropDownClosed(object sender, EventArgs e)
        {
            DoingDropDown = false;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Populate the QuickTextComboBox dropdown box.
        /// </summary>
        private void populateQuickTextComboBox()
        {
            string searchFor = APP_NAME + "_" + VersionComboBox.Text + "_";
            var filesThatmatch = Directory.EnumerateFiles(".", searchFor + "*" + TEXT_FILE_EXT);
            int sheets = 0;

            QuickTextComboBox.Items.Clear();
            //
            foreach (string filename in filesThatmatch)
            {
                int clipBegin = searchFor.Length + 2; // Where the +2 is because of an inferred ".\" in the filename.
                int clipLength = filename.Length - clipBegin - TEXT_FILE_EXT.Length;
                QuickTextComboBox.Items.Add(filename.Substring(clipBegin, clipLength));
                //
                sheets++;
            }
            if (0 == sheets)
            {
                // Default item is always COMport_project_QUICK.TXT
                //
                QuickTextComboBox.Items.Add("QUICK");
            }
            QuickTextComboBox.Items.Add(NEW_SHEET); // This label at the end enables one to add new sheets.
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Detect ENTER being hit while typing a new quick text menu item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuickTextComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                selectQuickTextMenu(QuickTextComboBox.Text);
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Click on the "Quick text menu" button, display the quick text menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuickTextComboBox_MouseUp(object sender, MouseEventArgs e)
        {
            selectQuickTextMenu(QuickTextComboBox.Text);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Use the current Quick text menu listed to open a new menu.
        /// </summary>
        private void selectQuickTextMenu(string menuName)
        {
            if( (menuName.Length > 0) && !DoingDropDown )
            {
                string filename = menuName.Replace(' ', '_');

                QuickTextMenu menu = new QuickTextMenu(APP_NAME + "_" + VersionComboBox.Text + "_" + filename + TEXT_FILE_EXT);
                //
                menu.StartPosition = FormStartPosition.Manual;
                menu.Location = Location;
                menu.Left += ClientSize.Width + 10; // To place it on far right of parent.
                //
                menu.Show();
                //
                CommsTextBox.Focus(); // Return to the text screen as normal . . .
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Detect "new sheet" being selected from dropdown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuickTextComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (QuickTextComboBox.SelectedItem.ToString() == NEW_SHEET)
            {
                QuickTextComboBox.SelectedText = "";
            }
            else
            {
                selectQuickTextMenu(QuickTextComboBox.Text);
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Enable/disable the toolTips
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolTipsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            toolTips.Active = ToolTipsCheckBox.Checked;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Always assume that the console is selected following this click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectConsoleFollowing_Click(object sender, EventArgs e)
        {
            CommsTextBox.Focus();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Just need to suppress the ding generated by ENTER key!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuickTextComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
            }

        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Start or stop logging to a timestamped file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartLogButton_Click(object sender, EventArgs e)
        {
            if (LOG_START_LABEL == StartLogButton.Text)
            {
                // Start logging is selected.
                //
                OutputLogFile = APP_NAME + DateTime.Now.ToString("_yyyyMMdd_HHmmss") + ".LOG";
                try
                {
                    using (StreamWriter LogFile = File.CreateText(OutputLogFile))
                    {
                        // Just need to ensure that it has been created . . .
                    }
                    StartLogButton.Text = LOG_STOP_LABEL;
                    StartLogButton.BackColor = System.Drawing.SystemColors.ControlDark;
                    StartLogButton.ForeColor = System.Drawing.SystemColors.ControlLightLight;
                    StartLogButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, (byte)(0));
                }
                catch
                {
                    StopLogButton();
                }
            }
            else
            {
                StopLogButton();
            }
            CommsTextBox.Focus(); // Always assume that the console is selected following this click.
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Change the start/stop logging button to its STOP state.
        /// </summary>
        private void StopLogButton()
        {
            StartLogButton.BackColor = System.Drawing.SystemColors.ActiveBorder;
            StartLogButton.ForeColor = System.Drawing.SystemColors.ControlText;
            StartLogButton.Font = (new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte)(0)));
            //
            OutputLogFile = ""; // Disables the log process.
            StartLogButton.Text = LOG_START_LABEL;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// When ever serial data is received, display it on CommsTextBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (false == COMportComboBox.Enabled)
            {
                int readLength = SerialPort.BytesToRead;
                var inputs = new byte[readLength];

                SerialPort.Read(inputs, 0, readLength);
                processRXcharacters(inputs, readLength);
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Check for characters coming in from D2XX device.
        /// 
        /// NOTES:
        /// 
        /// Would have much preferred to have done this using DataReceived process in
        /// the same fashion as SerialPort_DataReceived, but couldn't find the right
        /// way to initialise and utilise the thing.  This alternative just ticks
        /// along at 10ms intervals to check for characters coming in from the D2XX
        /// device and deals with them using the shared processRXcharacters function.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void D2XX_RXcharacterTimer_Tick(object sender, EventArgs e)
        {
            int readLength = (int)D2xxDevice.BytesToRead();

            if (readLength > 0)
            {
                var inputs = new byte[readLength];

                D2xxDevice.Read(inputs, 0, readLength);
                processRXcharacters(inputs, readLength);
            }
        }

        // ------------------------------------------------------------------------------------------------------------------
        // ------------------------------------------------------------------------------------------------------------------
        // -----------------------                                                                 --------------------------
        // -----------------------    Dual Port Type Handling (Serial Port (VCP) and D2xx)         --------------------------
        // -----------------------                                                                 --------------------------
        // ------------------------------------------------------------------------------------------------------------------
        // ------------------------------------------------------------------------------------------------------------------

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Clear the data buffer contents for the serial port or D2xx device
        /// </summary>
        private void Port_ClearBuffer()
        {
            if (FTDI_D2XX == FTDI_mode)
            {
                D2xxDevice.ClearBuffer();
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    if (false == Port_PossibleRead(1)) break;
                }
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Close the current device.
        /// </summary>
        private void Port_Close()
        {
            if (FTDI_D2XX == FTDI_mode)
            {
                if( D2xxDevice.IsConnected ) D2xxDevice.Close();
            }
            else
            {
                if (SerialPort.IsOpen) SerialPort.Close();
            }
        }

        private void Port_Write(byte[] buffer, int offset, int count)
        {
            if (FTDI_D2XX == FTDI_mode)
            {
                D2xxDevice.Write(buffer, offset, count);
            }
            else
            {
                SerialPort.Write(buffer, offset, count);
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Write a line to the serial port or D2xx device
        /// </summary>
        private void Port_WriteLine(string command)
        {
#if DEBUG_LOGGING
			logToTestFile(command);
#endif
            if (FTDI_D2XX == FTDI_mode)
            {
                D2xxDevice.WriteLine(command);
            }
            else
            {
                SerialPort.WriteLine(command);
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Read a line from the serial port or D2xx device
        /// </summary>
        /// <returns>Line read from the port</returns>
        private string Port_ReadLine()
        {
            string response;

#if DEBUG_LOGGING
			logToTestFile(command);
#endif
            if (FTDI_D2XX == FTDI_mode)
            {
                response = D2xxDevice.ReadLine();
            }
            else
            {
                response = SerialPort.ReadLine();
            }
            return response;
        }


        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Read a character from the serial port or D2xx device
        /// </summary>
        /// <returns>Character read from the port</returns>
        private int Port_ReadChar()
        {
            int character;

            if (FTDI_D2XX == FTDI_mode)
            {
                character = D2xxDevice.ReadChar();
            }
            else
            {
                character = SerialPort.ReadChar();
            }

            return character;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Read a number of characters from the serial port of D2xx device
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// 
        private void Port_Read(byte[] buffer, int offset, int count)
        {

            if (FTDI_D2XX == FTDI_mode)
            {
                D2xxDevice.Read(buffer, offset, count);
            }
            else
            {
                SerialPort.Read(buffer, offset, count);
            }

        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Possibly need to read a response, if echo is off it just times out. From the serial port or D2xx device
        /// </summary>
        /// <returns>Response</returns>
        private bool Port_PossibleRead(int shorterTimeout)
        {
            bool result = false;

            try
            {
                if (FTDI_D2XX == FTDI_mode)
                {
                    D2xxDevice.ReadLine();
                    result = true;
                }
                else
                {
                    SerialPort.ReadTimeout = shorterTimeout;
                    Port_ReadLine("any response");
                    result = true;
                }
            }
            catch
            {
                // ignore no response, possibly just no echo.
            }
            if (FTDI_VCP == FTDI_mode) SerialPort.ReadTimeout = TIMEOUT_NORMAL;
            //
            return result;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Gets the numbber of avialable bytes from the serial port or D2xx device
        /// </summary>
        /// <returns>Number of bytes</returns>
        private int Port_BytesToRead()
        {
            int result = 0;

            if (FTDI_D2XX == FTDI_mode)
            {

                result = (int)D2xxDevice.BytesToRead();
            }
            else
            {
                result = SerialPort.BytesToRead;
            }
            return result;
        }

        // ------------------------------------------------------------------------------------------------------------------------
        // ------------------------------------------------------------------------------------------------------------------------
        // -------------------------------                                           ----------------------------------------------
        // -------------------------------           NEW SERIAL PORT FUNCTIONS       ----------------------------------------------
        // -------------------------------                                           ----------------------------------------------
        // ------------------------------------------------------------------------------------------------------------------------
        // ------------------------------------------------------------------------------------------------------------------------

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Write a line to the serial port then get a response.
        /// </summary>
        private string Port_WriteAndRespond(string command, string prompt)
        {
            Port_WriteLine(command);
            //
            // A delay is not required here because there is a set timeout provided in read line below . . .
            // Thread.Sleep(INTERVAL_RESPOND);
            //
            string response = Port_ReadLine(prompt);
            //
            return response;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Write a line to the serial port, pause for a moment before ignore the response.
        /// </summary>
        private void Port_WritePauseAndDiscard(string command)
        {
            Port_ClearBuffer();
            //
            Port_WriteLine(command);
            Thread.Sleep(INTERVAL_RESPOND);
            Port_PossibleRead(MAY_TIMEOUT);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Write a line to the serial port and ignore any response.
        /// </summary>
        private void Port_WriteAndDiscard(string command)
        {
            Port_WriteLine(command);
            Port_PossibleRead(MAY_TIMEOUT);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Right-click of Tx on enter check box selects intermediate state which
        /// forces a CRLF into screen display if not supplied by target device.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HalfDuplexCheckBox_MouseDown(object sender, MouseEventArgs e)
        {
            if( e.Button == MouseButtons.Right )
            {
                HalfDuplexCheckBox.CheckState = CheckState.Indeterminate;
            }
            else
            {
                return;
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Read a line back from serial device, discard the implied LF too
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="response"></param>
        private string Port_ReadLine(string prompt)
        {
            string response;

#if DEBUG_LOGGING
			logToTestFile(command);
#endif
            if (FTDI_D2XX == FTDI_mode)
            {
                response = D2xxDevice.ReadLine();
            }
            else
            {
                response = SerialPort.ReadLine();
                //
                if ("\r" == EnterKey) // The above CR read a line in.
                {
                    byte[] input = new byte[1];
                    Port_Read(input, 0, 1); // Which would return (and discard) the LF character that followed the above CR.
                }
            }
#if DEBUG_LOGGING
#if DEBUG_BY_STRING
			// Log strings being sent . . .
			//
			logToTestFile(", " + prompt + " = {" + response + "}\r\n");
#else
			// Log strings as character values . . .
			//
			byte[] bytes = Encoding.ASCII.GetBytes(response);
			string list = "";
			for (int i = 0; i < bytes.Length; i++)
			{
				if (i > 0) list += ", ";
				list += String.Format("{0}", bytes[i]);
			}
			logToTestFile(", " + prompt + " = {" + list + "}\r\n");
#endif
#endif
            response = response.Replace("\r", "").Replace("\n", "");
            //
            return response;
        }
    }
}
