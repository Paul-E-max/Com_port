using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Thorlabs.TLPM_64.Interop;

namespace ThorlabsPowerMeterApp.Services
{
    /// <summary>
    /// Service for interacting with Thorlabs power meters
    /// </summary>
    public class PowerMeterService : IDisposable
    {
        private TLPM _device;
        private bool _isConnected;
        private bool _disposed;

        /// <summary>
        /// Event fired when a measurement error occurs
        /// </summary>
        public event EventHandler<string> OnError;

        /// <summary>
        /// Gets whether the device is currently connected
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// Scans for available Thorlabs power meters
        /// </summary>
        /// <returns>List of device resource names and descriptions</returns>
        public List<DeviceInfo> ScanDevices()
        {
            return ScanDevices(out _);
        }

        /// <summary>
        /// Scans for available Thorlabs power meters with diagnostic output
        /// </summary>
        public List<DeviceInfo> ScanDevices(out string diagnosticInfo)
        {
            var devices = new List<DeviceInfo>();
            var diag = new StringBuilder();
            diag.AppendLine("=== Thorlabs Device Scan Diagnostics ===");

            try
            {
                diag.AppendLine("Creating TLPM search handle...");
                HandleRef handle = new HandleRef();
                
                using (var searchDevice = new TLPM(handle.Handle))
                {
                    diag.AppendLine("TLPM handle created successfully.");
                    
                    uint count = 0;
                    diag.AppendLine("Calling findRsrc()...");
                    int result = searchDevice.findRsrc(out count);
                    diag.AppendLine($"findRsrc() returned: {result}, count: {count}");

                    if (result != 0)
                    {
                        diag.AppendLine($"WARNING: findRsrc returned error code {result}");
                        diag.AppendLine("This may indicate NI-VISA is not installed or not configured.");
                    }

                    if (count > 0)
                    {
                        for (uint i = 0; i < count; i++)
                        {
                            StringBuilder resourceName = new StringBuilder(1024);
                            StringBuilder modelName = new StringBuilder(1024);
                            StringBuilder serialNumber = new StringBuilder(1024);
                            StringBuilder manufacturer = new StringBuilder(1024);
                            bool available = false;

                            searchDevice.getRsrcName(i, resourceName);
                            diag.AppendLine($"Device {i}: Resource = {resourceName}");
                            
                            try
                            {
                                searchDevice.getRsrcInfo(i, modelName, serialNumber, manufacturer, out available);
                                diag.AppendLine($"  Model: {modelName}, S/N: {serialNumber}, Available: {available}");
                            }
                            catch (Exception infoEx)
                            {
                                diag.AppendLine($"  Could not get device info: {infoEx.Message}");
                                modelName.Append("Unknown Model");
                                serialNumber.Append("N/A");
                            }

                            devices.Add(new DeviceInfo
                            {
                                ResourceName = resourceName.ToString(),
                                ModelName = modelName.ToString(),
                                SerialNumber = serialNumber.ToString(),
                                IsAvailable = available
                            });
                        }
                    }
                    else
                    {
                        diag.AppendLine("");
                        diag.AppendLine("No devices found. Possible causes:");
                        diag.AppendLine("1. NI-VISA is not installed");
                        diag.AppendLine("2. Device is using TMC driver instead of NI-VISA");
                        diag.AppendLine("3. Device is not properly connected");
                        diag.AppendLine("");
                        diag.AppendLine("SOLUTION: Use Thorlabs 'Driver Switcher' tool to switch");
                        diag.AppendLine("your PM100USB from TMC to NI-VISA driver.");
                        diag.AppendLine("The tool is typically at:");
                        diag.AppendLine(@"C:\Program Files\Thorlabs\OPM\DriverSwitcher.exe");
                    }
                }
            }
            catch (Exception ex)
            {
                diag.AppendLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    diag.AppendLine($"Inner: {ex.InnerException.Message}");
                }
                
                if (ex.Message.Contains("VISA") || ex.Message.Contains("visa"))
                {
                    diag.AppendLine("");
                    diag.AppendLine("NI-VISA library not found. Please install NI-VISA from:");
                    diag.AppendLine("https://www.ni.com/en/support/downloads/drivers/download.ni-visa.html");
                }
                
                OnError?.Invoke(this, $"Error scanning devices: {ex.Message}");
            }

            diagnosticInfo = diag.ToString();
            return devices;
        }

        /// <summary>
        /// Attempts to connect using a manual USB resource string
        /// </summary>
        /// <param name="usbVid">USB Vendor ID (e.g., 0x1313 for Thorlabs)</param>
        /// <param name="usbPid">USB Product ID</param>
        /// <param name="serialNumber">Device serial number (optional)</param>
        public string BuildUsbResourceString(int usbVid = 0x1313, int usbPid = 0x8072, string serialNumber = "")
        {
            // Standard VISA resource string format for USB TMC devices
            // USB[board]::VID::PID::serial::INSTR
            if (string.IsNullOrEmpty(serialNumber))
            {
                return $"USB0::0x{usbVid:X4}::0x{usbPid:X4}::INSTR";
            }
            return $"USB0::0x{usbVid:X4}::0x{usbPid:X4}::{serialNumber}::INSTR";
        }

        /// <summary>
        /// Connects to a power meter
        /// </summary>
        /// <param name="resourceName">The VISA resource name of the device</param>
        /// <returns>True if connection successful</returns>
        public bool Connect(string resourceName)
        {
            try
            {
                Disconnect(); // Disconnect any existing connection

                _device = new TLPM(resourceName, true, true);
                _isConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"Error connecting: {ex.Message}");
                _isConnected = false;
                return false;
            }
        }

        /// <summary>
        /// Disconnects from the current device
        /// </summary>
        public void Disconnect()
        {
            if (_device != null)
            {
                try
                {
                    _device.Dispose();
                }
                catch { }
                _device = null;
            }
            _isConnected = false;
        }

        /// <summary>
        /// Configures power meter settings
        /// </summary>
        public bool ConfigureSettings(double wavelength, double beamDiameter, int avgCount, bool autoRange)
        {
            if (_device == null) return false;

            try
            {
                _device.setInputFilterState(true);  // Low bandwidth
                _device.setAvgCnt((short)avgCount);
                _device.setAttenuation(0);
                _device.setBeamDia(beamDiameter);
                _device.setWavelength(wavelength);
                _device.setPowerAutoRange(autoRange);
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"Error configuring: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Reads the current power measurement in Watts
        /// </summary>
        public bool ReadPower(out double power)
        {
            power = 0;
            if (_device == null) return false;

            try
            {
                int result = _device.measPower(out power);
                return result == 0;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"Error reading power: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Reads the current power density measurement in W/cm²
        /// </summary>
        public bool ReadPowerDensity(out double powerDensity)
        {
            powerDensity = 0;
            if (_device == null) return false;

            try
            {
                int result = _device.measPowerDens(out powerDensity);
                return result == 0;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"Error reading power density: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets device information
        /// </summary>
        public bool GetDeviceInfo(out string manufacturer, out string model, out string serial, out string firmware)
        {
            manufacturer = model = serial = firmware = "";
            if (_device == null) return false;

            try
            {
                StringBuilder mfr = new StringBuilder(256);
                StringBuilder mdl = new StringBuilder(256);
                StringBuilder sn = new StringBuilder(256);
                StringBuilder fw = new StringBuilder(256);

                _device.identificationQuery(mfr, mdl, sn, fw);

                manufacturer = mfr.ToString();
                model = mdl.ToString();
                serial = sn.ToString();
                firmware = fw.ToString();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the current wavelength setting
        /// </summary>
        public bool GetWavelength(out double wavelength, out double minWavelength, out double maxWavelength)
        {
            wavelength = minWavelength = maxWavelength = 0;
            if (_device == null) return false;

            try
            {
                short attr = 0;
                _device.getWavelength(attr, out wavelength);
                attr = 1; // min
                _device.getWavelength(attr, out minWavelength);
                attr = 2; // max
                _device.getWavelength(attr, out maxWavelength);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Disconnect();
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Information about a discovered device
    /// </summary>
    public class DeviceInfo
    {
        public string ResourceName { get; set; }
        public string ModelName { get; set; }
        public string SerialNumber { get; set; }
        public bool IsAvailable { get; set; }

        public override string ToString()
        {
            return $"{ModelName} - S/N: {SerialNumber}";
        }
    }
}
