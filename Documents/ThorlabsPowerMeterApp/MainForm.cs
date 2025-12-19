// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
// @File:       MainForm.cs
// @Project:    ThorlabsPowerMeterApp
// @Author:     Thorlabs Power Meter Integration
// @Created:    18.12.2024
//
// @Brief:      Main form for Thorlabs Power Meter application.
//              Provides device connection, real-time measurement display,
//              statistics tracking, data logging, and real-time graphing.
//
// @Revision:
// 18.12.2024 - v1.0.0 - Initial version with WinUSB/TLPMX support.
// 19.12.2024 - v1.1.0 - Added real-time power graph with auto-scaling.
//
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ThorlabsPowerMeterApp.Services;
using ThorlabsPowerMeterApp.Controls;

namespace ThorlabsPowerMeterApp
{
    public partial class MainForm : Form
    {
        private PowerMeterService _powerMeter;        // NI-VISA mode (TLPM)
        private PowerMeterServiceX _powerMeterX;      // WinUSB mode (TLPMX)
        private Timer _measurementTimer;
        private StreamWriter _logWriter;
        private bool _isLogging;
        private List<double> _measurements;
        private double _minValue = double.MaxValue;
        private double _maxValue = double.MinValue;
        private string _currentUnit = "mW";
        private bool _useWinUsb = true; // Default to WinUSB/TLPMX mode

        // UI Controls
        private ComboBox _deviceComboBox;
        private Button _scanButton;
        private Button _connectButton;
        private Label _statusLabel;
        private Panel _statusIndicator;
        private Label _measurementLabel;
        private Label _unitLabel;
        private Label _minLabel;
        private Label _maxLabel;
        private Label _avgLabel;
        private NumericUpDown _wavelengthNumeric;
        private NumericUpDown _beamDiameterNumeric;
        private NumericUpDown _avgCountNumeric;
        private CheckBox _autoRangeCheckBox;
        private Button _applySettingsButton;
        private Button _startLogButton;
        private Button _stopLogButton;
        private Label _logFileLabel;
        private ComboBox _unitComboBox;
        private Button _resetStatsButton;
        private CheckBox _winUsbCheckBox;

        // Chart controls
        private PowerMeterChart _powerChart;
        private CheckBox _showChartCheckBox;
        private Button _chart30sButton;
        private Button _chart60sButton;
        private Button _chart5mButton;
        private Panel _chartPanel;

        public MainForm()
        {
            InitializeComponent();
            SetupForm();
            SetupControls();
            SetupEvents();

            // NI-VISA mode service (TLPM)
            _powerMeter = new PowerMeterService();
            _powerMeter.OnError += PowerMeter_OnError;

            // WinUSB mode service (TLPMX - supports libusb/WinUSB drivers)
            _powerMeterX = new PowerMeterServiceX();
            _powerMeterX.OnError += PowerMeter_OnError;

            _measurements = new List<double>();

            _measurementTimer = new Timer();
            _measurementTimer.Interval = 100; // 10 Hz update rate
            _measurementTimer.Tick += MeasurementTimer_Tick;
        }

        private void UsbTmc_OnDebug(object sender, string message)
        {
            System.Diagnostics.Debug.WriteLine($"[USB TMC] {message}");
        }

        private void SetupForm()
        {
            this.Text = "Thorlabs Power Meter Reader v1.1.0";
            this.Size = new Size(900, 560);  // Wider for chart
            this.MinimumSize = new Size(900, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
        }

        private void SetupControls()
        {
            int y = 15;
            int labelWidth = 80;

            // === Device Section ===
            var deviceGroupBox = CreateGroupBox("Device Connection", 10, y, 485, 105);
            this.Controls.Add(deviceGroupBox);

            // Row 1: Mode selection
            _winUsbCheckBox = new CheckBox
            {
                Text = "Use WinUSB/TMC (no NI-VISA required)",
                Location = new Point(10, 22),
                Size = new Size(250, 20),
                Checked = true,
                ForeColor = Color.FromArgb(100, 200, 255)
            };
            _winUsbCheckBox.CheckedChanged += (s, e) => { _useWinUsb = _winUsbCheckBox.Checked; };
            deviceGroupBox.Controls.Add(_winUsbCheckBox);

            // Row 2: Device selection
            var deviceLabel = CreateLabel("Device:", 10, 48, labelWidth);
            deviceGroupBox.Controls.Add(deviceLabel);

            _deviceComboBox = new ComboBox
            {
                Location = new Point(90, 45),
                Size = new Size(250, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            deviceGroupBox.Controls.Add(_deviceComboBox);

            _scanButton = CreateButton("Scan", 350, 44, 60, 27);
            deviceGroupBox.Controls.Add(_scanButton);

            _connectButton = CreateButton("Connect", 415, 44, 60, 27);
            deviceGroupBox.Controls.Add(_connectButton);

            // Row 3: Status
            var statusLbl = CreateLabel("Status:", 10, 78, labelWidth);
            deviceGroupBox.Controls.Add(statusLbl);

            _statusIndicator = new Panel
            {
                Location = new Point(90, 81),
                Size = new Size(12, 12),
                BackColor = Color.Gray
            };
            deviceGroupBox.Controls.Add(_statusIndicator);

            _statusLabel = CreateLabel("Disconnected", 108, 78, 300);
            _statusLabel.ForeColor = Color.Gray;
            deviceGroupBox.Controls.Add(_statusLabel);

            y += 120;

            // === Measurement Display Section ===
            var measurementGroupBox = CreateGroupBox("Measurement", 10, y, 485, 130);
            this.Controls.Add(measurementGroupBox);

            var measurementPanel = new Panel
            {
                Location = new Point(10, 25),
                Size = new Size(465, 60),
                BackColor = Color.FromArgb(20, 20, 20),
                BorderStyle = BorderStyle.FixedSingle
            };
            measurementGroupBox.Controls.Add(measurementPanel);

            _measurementLabel = new Label
            {
                Text = "---",
                Font = new Font("Consolas", 32F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 83),
                Location = new Point(10, 8),
                Size = new Size(350, 45),
                TextAlign = ContentAlignment.MiddleRight
            };
            measurementPanel.Controls.Add(_measurementLabel);

            _unitLabel = new Label
            {
                Text = "mW",
                Font = new Font("Segoe UI", 16F),
                ForeColor = Color.FromArgb(150, 150, 150),
                Location = new Point(365, 18),
                Size = new Size(90, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };
            measurementPanel.Controls.Add(_unitLabel);

            // Stats row
            _minLabel = CreateLabel("Min: ---", 10, 95, 140);
            _minLabel.ForeColor = Color.FromArgb(100, 180, 255);
            measurementGroupBox.Controls.Add(_minLabel);

            _maxLabel = CreateLabel("Max: ---", 160, 95, 140);
            _maxLabel.ForeColor = Color.FromArgb(255, 100, 100);
            measurementGroupBox.Controls.Add(_maxLabel);

            _avgLabel = CreateLabel("Avg: ---", 310, 95, 100);
            _avgLabel.ForeColor = Color.FromArgb(255, 200, 100);
            measurementGroupBox.Controls.Add(_avgLabel);

            _resetStatsButton = CreateButton("Reset", 420, 92, 55, 25);
            measurementGroupBox.Controls.Add(_resetStatsButton);

            y += 145;

            // === Settings Section ===
            var settingsGroupBox = CreateGroupBox("Settings", 10, y, 485, 100);
            this.Controls.Add(settingsGroupBox);

            // Row 1
            var wlLabel = CreateLabel("Wavelength:", 10, 28, 80);
            settingsGroupBox.Controls.Add(wlLabel);

            _wavelengthNumeric = CreateNumericUpDown(95, 25, 80, 200, 1100, 650, 1);
            settingsGroupBox.Controls.Add(_wavelengthNumeric);

            var nmLabel = CreateLabel("nm", 180, 28, 30);
            settingsGroupBox.Controls.Add(nmLabel);

            var beamLabel = CreateLabel("Beam Ø:", 220, 28, 60);
            settingsGroupBox.Controls.Add(beamLabel);

            _beamDiameterNumeric = CreateNumericUpDown(285, 25, 70, 0.1m, 50, 9.5m, 1);
            settingsGroupBox.Controls.Add(_beamDiameterNumeric);

            var mmLabel = CreateLabel("mm", 360, 28, 30);
            settingsGroupBox.Controls.Add(mmLabel);

            // Row 2
            var avgLabel = CreateLabel("Averaging:", 10, 58, 80);
            settingsGroupBox.Controls.Add(avgLabel);

            _avgCountNumeric = CreateNumericUpDown(95, 55, 80, 1, 1000, 20, 0);
            settingsGroupBox.Controls.Add(_avgCountNumeric);

            _autoRangeCheckBox = new CheckBox
            {
                Text = "Auto Range",
                Location = new Point(220, 57),
                Size = new Size(100, 20),
                Checked = true,
                ForeColor = Color.White
            };
            settingsGroupBox.Controls.Add(_autoRangeCheckBox);

            var unitLabel = CreateLabel("Unit:", 330, 58, 40);
            settingsGroupBox.Controls.Add(unitLabel);

            _unitComboBox = new ComboBox
            {
                Location = new Point(375, 55),
                Size = new Size(70, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _unitComboBox.Items.AddRange(new[] { "W", "mW", "µW", "nW", "dBm" });
            _unitComboBox.SelectedIndex = 1; // mW default
            settingsGroupBox.Controls.Add(_unitComboBox);

            _applySettingsButton = CreateButton("Apply", 415, 25, 60, 50);
            settingsGroupBox.Controls.Add(_applySettingsButton);

            y += 115;

            // === Logging Section ===
            var loggingGroupBox = CreateGroupBox("Data Logging", 10, y, 485, 60);
            this.Controls.Add(loggingGroupBox);

            _startLogButton = CreateButton("Start Logging", 10, 22, 100, 27);
            loggingGroupBox.Controls.Add(_startLogButton);

            _stopLogButton = CreateButton("Stop Logging", 120, 22, 100, 27);
            _stopLogButton.Enabled = false;
            loggingGroupBox.Controls.Add(_stopLogButton);

            _logFileLabel = CreateLabel("Log file: (none)", 235, 25, 240);
            _logFileLabel.ForeColor = Color.Gray;
            loggingGroupBox.Controls.Add(_logFileLabel);

            // === Chart Section ===
            _chartPanel = new Panel
            {
                Location = new Point(505, 15),
                Size = new Size(380, 480),
                BackColor = Color.FromArgb(30, 30, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(_chartPanel);

            // Chart header with controls
            var chartHeaderPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(378, 35),
                BackColor = Color.FromArgb(40, 40, 40)
            };
            _chartPanel.Controls.Add(chartHeaderPanel);

            _showChartCheckBox = new CheckBox
            {
                Text = "Power Graph",
                Location = new Point(10, 8),
                Size = new Size(100, 20),
                Checked = true,
                ForeColor = Color.FromArgb(0, 200, 255)
            };
            chartHeaderPanel.Controls.Add(_showChartCheckBox);

            _chart30sButton = CreateButton("30s", 160, 5, 50, 25);
            _chart30sButton.BackColor = Color.FromArgb(0, 120, 180);  // Selected
            chartHeaderPanel.Controls.Add(_chart30sButton);

            _chart60sButton = CreateButton("60s", 215, 5, 50, 25);
            chartHeaderPanel.Controls.Add(_chart60sButton);

            _chart5mButton = CreateButton("5m", 270, 5, 50, 25);
            chartHeaderPanel.Controls.Add(_chart5mButton);

            // Chart control
            _powerChart = new PowerMeterChart
            {
                Location = new Point(0, 36),
                Size = new Size(378, 442),
                HistorySeconds = 30
            };
            _chartPanel.Controls.Add(_powerChart);
        }

        private void SetupEvents()
        {
            _scanButton.Click += ScanButton_Click;
            _connectButton.Click += ConnectButton_Click;
            _applySettingsButton.Click += ApplySettingsButton_Click;
            _startLogButton.Click += StartLogButton_Click;
            _stopLogButton.Click += StopLogButton_Click;
            _resetStatsButton.Click += ResetStatsButton_Click;
            _unitComboBox.SelectedIndexChanged += UnitComboBox_SelectedIndexChanged;
            this.FormClosing += MainForm_FormClosing;

            // Chart events
            _showChartCheckBox.CheckedChanged += (s, e) => { _chartPanel.Visible = _showChartCheckBox.Checked; };
            _chart30sButton.Click += (s, e) => SetChartHistory(30);
            _chart60sButton.Click += (s, e) => SetChartHistory(60);
            _chart5mButton.Click += (s, e) => SetChartHistory(300);
        }

        private void SetChartHistory(int seconds)
        {
            _powerChart.HistorySeconds = seconds;
            _powerChart.ClearData();

            // Update button styles
            var activeColor = Color.FromArgb(0, 120, 180);
            var inactiveColor = Color.FromArgb(60, 60, 60);

            _chart30sButton.BackColor = seconds == 30 ? activeColor : inactiveColor;
            _chart60sButton.BackColor = seconds == 60 ? activeColor : inactiveColor;
            _chart5mButton.BackColor = seconds == 300 ? activeColor : inactiveColor;
        }

        private GroupBox CreateGroupBox(string text, int x, int y, int width, int height)
        {
            return new GroupBox
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                ForeColor = Color.White
            };
        }

        private Label CreateLabel(string text, int x, int y, int width)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 20),
                ForeColor = Color.White
            };
        }

        private Button CreateButton(string text, int x, int y, int width, int height)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }

        private NumericUpDown CreateNumericUpDown(int x, int y, int width, decimal min, decimal max, decimal value, int decimals)
        {
            return new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(width, 25),
                Minimum = min,
                Maximum = max,
                Value = value,
                DecimalPlaces = decimals,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White
            };
        }

        // === Event Handlers ===

        private void ScanButton_Click(object sender, EventArgs e)
        {
            _deviceComboBox.Items.Clear();
            _scanButton.Enabled = false;
            _scanButton.Text = "...";

            try
            {
                if (_useWinUsb)
                {
                    // Use TLPMX (supports WinUSB/libusb drivers)
                    string diagnosticInfo;
                    var devices = _powerMeterX.ScanDevices(out diagnosticInfo);

                    if (devices.Count == 0)
                    {
                        var result = MessageBox.Show(
                            "No devices found via TLPMX (WinUSB mode).\n\n" +
                            "Make sure:\n" +
                            "• PM100USB is connected via USB\n" +
                            "• Driver is set to 'WinUSB' in Thorlabs Driver Switcher\n\n" +
                            "Click YES to see diagnostic info.",
                            "No Devices Found",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (result == DialogResult.Yes)
                        {
                            ShowDiagnostics(diagnosticInfo);
                        }
                    }
                    else
                    {
                        foreach (var device in devices)
                        {
                            _deviceComboBox.Items.Add(device);
                        }
                        _deviceComboBox.SelectedIndex = 0;
                        _statusLabel.Text = $"Found {devices.Count} device(s)";
                    }
                }
                else
                {
                    // Use NI-VISA driver (TLPM)
                    string diagnosticInfo;
                    var devices = _powerMeter.ScanDevices(out diagnosticInfo);

                    if (devices.Count == 0)
                    {
                        var result = MessageBox.Show(
                            "No devices found via NI-VISA.\n\n" +
                            "Try enabling 'Use WinUSB/TMC' mode instead,\n" +
                            "or click YES to see diagnostics.",
                            "No Devices Found",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (result == DialogResult.Yes)
                        {
                            ShowDiagnostics(diagnosticInfo);
                        }
                    }
                    else
                    {
                        foreach (var device in devices)
                        {
                            _deviceComboBox.Items.Add(device);
                        }
                        _deviceComboBox.SelectedIndex = 0;
                    }
                }
            }
            finally
            {
                _scanButton.Enabled = true;
                _scanButton.Text = "Scan";
            }
        }

        private void ShowDiagnostics(string diagnosticInfo)
        {
            var diagForm = new Form
            {
                Text = "Scan Diagnostics",
                Size = new System.Drawing.Size(600, 400),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = System.Drawing.Color.FromArgb(30, 30, 30)
            };

            var textBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Both,
                Font = new System.Drawing.Font("Consolas", 10F),
                BackColor = System.Drawing.Color.FromArgb(20, 20, 20),
                ForeColor = System.Drawing.Color.LightGray,
                Text = diagnosticInfo
            };

            diagForm.Controls.Add(textBox);
            diagForm.ShowDialog(this);
        }

        private bool IsConnected => _useWinUsb ? _powerMeterX.IsConnected : _powerMeter.IsConnected;

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                // Disconnect
                _measurementTimer.Stop();
                
                if (_useWinUsb)
                    _powerMeterX.Disconnect();
                else
                    _powerMeter.Disconnect();
                
                UpdateConnectionStatus(false);
                _connectButton.Text = "Connect";
                _winUsbCheckBox.Enabled = true;
            }
            else
            {
                // Connect
                var selectedItem = _deviceComboBox.SelectedItem;
                
                if (selectedItem == null)
                {
                    MessageBox.Show("Please select a device first.", "No Device Selected",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _connectButton.Enabled = false;
                _connectButton.Text = "...";

                try
                {
                    bool success = false;

                    if (_useWinUsb && selectedItem is DeviceInfo winUsbDevice)
                    {
                        success = _powerMeterX.Connect(winUsbDevice.ResourceName);
                    }
                    else if (!_useWinUsb && selectedItem is DeviceInfo visaDevice)
                    {
                        success = _powerMeter.Connect(visaDevice.ResourceName);
                    }
                    else
                    {
                        MessageBox.Show("Device type mismatch. Please scan again.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (success)
                    {
                        UpdateConnectionStatus(true);
                        _connectButton.Text = "Disconnect";
                        _winUsbCheckBox.Enabled = false; // Lock mode while connected

                        // Apply initial settings
                        ApplySettings();

                        // Start measurement timer
                        _measurementTimer.Start();
                    }
                    else
                    {
                        MessageBox.Show("Failed to connect to the device.", "Connection Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _connectButton.Text = "Connect";
                    }
                }
                finally
                {
                    _connectButton.Enabled = true;
                }
            }
        }

        private void ApplySettingsButton_Click(object sender, EventArgs e)
        {
            ApplySettings();
        }

        private void ApplySettings()
        {
            if (!IsConnected) return;

            double wavelength = (double)_wavelengthNumeric.Value;
            double beamDiameter = (double)_beamDiameterNumeric.Value;
            int avgCount = (int)_avgCountNumeric.Value;
            bool autoRange = _autoRangeCheckBox.Checked;

            bool success = false;

            if (_useWinUsb)
            {
                success = _powerMeterX.ConfigureSettings(wavelength, beamDiameter, avgCount, autoRange);
            }
            else
            {
                success = _powerMeter.ConfigureSettings(wavelength, beamDiameter, avgCount, autoRange);
            }

            if (success)
            {
                _statusLabel.Text = "Settings applied";
            }
        }

        private void MeasurementTimer_Tick(object sender, EventArgs e)
        {
            if (!IsConnected) return;

            double power;
            bool success = false;

            if (_useWinUsb)
            {
                success = _powerMeterX.ReadPower(out power);
            }
            else
            {
                success = _powerMeter.ReadPower(out power);
            }

            if (success)
            {
                // Convert to selected unit
                double displayValue = ConvertPower(power, _currentUnit);
                _measurementLabel.Text = FormatMeasurement(displayValue);

                // Update stats
                _measurements.Add(power);
                if (power < _minValue) _minValue = power;
                if (power > _maxValue) _maxValue = power;

                double sum = 0;
                foreach (var m in _measurements) sum += m;
                double avg = sum / _measurements.Count;

                _minLabel.Text = $"Min: {FormatMeasurement(ConvertPower(_minValue, _currentUnit))}";
                _maxLabel.Text = $"Max: {FormatMeasurement(ConvertPower(_maxValue, _currentUnit))}";
                _avgLabel.Text = $"Avg: {FormatMeasurement(ConvertPower(avg, _currentUnit))}";

                // Log if enabled
                if (_isLogging && _logWriter != null)
                {
                    _logWriter.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff},{power:E6}");
                    _logWriter.Flush();
                }

                // Update chart with converted value
                if (_powerChart != null && _showChartCheckBox.Checked)
                {
                    _powerChart.AddDataPoint(displayValue, _currentUnit);
                }
            }
        }

        private double ConvertPower(double watts, string unit)
        {
            switch (unit)
            {
                case "W": return watts;
                case "mW": return watts * 1e3;
                case "µW": return watts * 1e6;
                case "nW": return watts * 1e9;
                case "dBm": return watts > 0 ? 10 * Math.Log10(watts * 1000) : -99;
                default: return watts * 1e3;
            }
        }

        private string FormatMeasurement(double value)
        {
            if (_currentUnit == "dBm")
            {
                return value.ToString("F2");
            }
            else if (Math.Abs(value) >= 100)
            {
                return value.ToString("F2");
            }
            else if (Math.Abs(value) >= 10)
            {
                return value.ToString("F3");
            }
            else if (Math.Abs(value) >= 1)
            {
                return value.ToString("F4");
            }
            else
            {
                return value.ToString("F5");
            }
        }

        private void StartLogButton_Click(object sender, EventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                DefaultExt = "csv",
                FileName = $"power_log_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _logWriter = new StreamWriter(saveDialog.FileName, false);
                    _logWriter.WriteLine("Timestamp,Power_W");
                    _isLogging = true;

                    _startLogButton.Enabled = false;
                    _stopLogButton.Enabled = true;
                    _logFileLabel.Text = $"Log: {Path.GetFileName(saveDialog.FileName)}";
                    _logFileLabel.ForeColor = Color.FromArgb(0, 200, 83);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error starting log: {ex.Message}", "Log Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void StopLogButton_Click(object sender, EventArgs e)
        {
            StopLogging();
        }

        private void StopLogging()
        {
            _isLogging = false;
            if (_logWriter != null)
            {
                _logWriter.Close();
                _logWriter = null;
            }

            _startLogButton.Enabled = true;
            _stopLogButton.Enabled = false;
            _logFileLabel.ForeColor = Color.Gray;
        }

        private void ResetStatsButton_Click(object sender, EventArgs e)
        {
            _measurements.Clear();
            _minValue = double.MaxValue;
            _maxValue = double.MinValue;
            _minLabel.Text = "Min: ---";
            _maxLabel.Text = "Max: ---";
            _avgLabel.Text = "Avg: ---";
        }

        private void UnitComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentUnit = _unitComboBox.SelectedItem?.ToString() ?? "mW";
            _unitLabel.Text = _currentUnit;
        }

        private void UpdateConnectionStatus(bool connected)
        {
            if (connected)
            {
                _statusIndicator.BackColor = Color.FromArgb(0, 200, 83);
                _statusLabel.Text = "Connected";
                _statusLabel.ForeColor = Color.FromArgb(0, 200, 83);
            }
            else
            {
                _statusIndicator.BackColor = Color.Gray;
                _statusLabel.Text = "Disconnected";
                _statusLabel.ForeColor = Color.Gray;
                _measurementLabel.Text = "---";
            }
        }

        private void PowerMeter_OnError(object sender, string message)
        {
            this.BeginInvoke((Action)(() =>
            {
                _statusLabel.Text = message;
                _statusLabel.ForeColor = Color.FromArgb(255, 100, 100);
            }));
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopLogging();
            _measurementTimer.Stop();
            _powerMeter?.Dispose();
            _powerMeterX?.Dispose();
        }
    }
}
