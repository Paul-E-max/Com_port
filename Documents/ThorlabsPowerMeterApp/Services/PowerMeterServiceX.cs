using System;
using System.Collections.Generic;
using System.Text;
using Thorlabs.TLPMX_64.Interop;

namespace ThorlabsPowerMeterApp.Services
{
    /// <summary>
    /// Power Meter Service using TLPMX library which supports WinUSB/libusb drivers
    /// This works with the Thorlabs "WinUSB" driver setting in Driver Switcher
    /// </summary>
    public class PowerMeterServiceX : IDisposable
    {
        private TLPMX _tlpmx;
        private bool _isConnected;
        private bool _disposed;
        private const ushort CHANNEL = 1; // Default channel for single-channel meters

        public event EventHandler<string> OnError;

        public bool IsConnected => _isConnected;

        /// <summary>
        /// Scan for devices using TLPMX library (supports WinUSB/libusb)
        /// </summary>
        public List<DeviceInfo> ScanDevices(out string diagnosticInfo)
        {
            var devices = new List<DeviceInfo>();
            var sb = new StringBuilder();

            sb.AppendLine("=== TLPMX Device Scan (WinUSB/libusb) ===");
            sb.AppendLine($"Time: {DateTime.Now}");
            sb.AppendLine();

            try
            {
                // Create search instance
                TLPMX searchDevice = new TLPMX(System.IntPtr.Zero);

                // Find resources
                uint count;
                int result = searchDevice.findRsrc(out count);

                sb.AppendLine($"findRsrc result: {result}");
                sb.AppendLine($"Devices found: {count}");
                sb.AppendLine();

                if (count > 0)
                {
                    StringBuilder resourceName = new StringBuilder(1024);
                    
                    for (uint i = 0; i < count; i++)
                    {
                        searchDevice.getRsrcName(i, resourceName);
                        string resource = resourceName.ToString();
                        
                        sb.AppendLine($"Device {i}: {resource}");

                        // Get device info
                        StringBuilder modelName = new StringBuilder(256);
                        StringBuilder serialNumber = new StringBuilder(256);
                        StringBuilder manufacturer = new StringBuilder(256);
                        bool available;

                        try
                        {
                            searchDevice.getRsrcInfo(i, modelName, serialNumber, manufacturer, out available);
                            
                            sb.AppendLine($"  Model: {modelName}");
                            sb.AppendLine($"  Serial: {serialNumber}");
                            sb.AppendLine($"  Manufacturer: {manufacturer}");
                            sb.AppendLine($"  Available: {available}");
                            sb.AppendLine();

                            devices.Add(new DeviceInfo
                            {
                                ResourceName = resource,
                                ModelName = modelName.ToString(),
                                SerialNumber = serialNumber.ToString(),
                                IsAvailable = available
                            });
                        }
                        catch (Exception ex)
                        {
                            sb.AppendLine($"  Error getting info: {ex.Message}");
                            
                            devices.Add(new DeviceInfo
                            {
                                ResourceName = resource,
                                ModelName = "Thorlabs Power Meter",
                                SerialNumber = "",
                                IsAvailable = true
                            });
                        }
                    }
                }

                searchDevice.Dispose();
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Scan error: {ex.Message}");
                sb.AppendLine();
                sb.AppendLine("Note: TLPMX requires the Thorlabs IVI driver to be installed.");
                sb.AppendLine("If using WinUSB driver, you may need NI-VISA runtime as well.");
                OnError?.Invoke(this, $"Error scanning: {ex.Message}");
            }

            diagnosticInfo = sb.ToString();
            return devices;
        }

        /// <summary>
        /// Connect to a power meter
        /// </summary>
        public bool Connect(string resourceName)
        {
            try
            {
                Disconnect();

                // Connect to the device
                _tlpmx = new TLPMX(resourceName, true, true);
                _isConnected = true;

                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"Connection failed: {ex.Message}");
                Disconnect();
                return false;
            }
        }

        /// <summary>
        /// Disconnect from the power meter
        /// </summary>
        public void Disconnect()
        {
            _isConnected = false;
            if (_tlpmx != null)
            {
                try { _tlpmx.Dispose(); }
                catch { }
                _tlpmx = null;
            }
        }

        /// <summary>
        /// Read power measurement
        /// </summary>
        public bool ReadPower(out double power)
        {
            power = 0;
            if (!_isConnected || _tlpmx == null)
                return false;

            try
            {
                _tlpmx.measPower(out power, CHANNEL);
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"Measurement error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Configure settings
        /// </summary>
        public bool ConfigureSettings(double wavelength, double beamDiameter, int avgCount, bool autoRange)
        {
            if (!_isConnected || _tlpmx == null)
                return false;

            try
            {
                _tlpmx.setWavelength(wavelength, CHANNEL);
                _tlpmx.setBeamDia(beamDiameter, CHANNEL);
                _tlpmx.setAvgCnt((short)avgCount, CHANNEL);
                _tlpmx.setPowerAutoRange(autoRange, CHANNEL);
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"Settings error: {ex.Message}");
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
}
