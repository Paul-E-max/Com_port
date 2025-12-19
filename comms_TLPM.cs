// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
// @File:       comms_TLPM.cs
// @Project:    DISCOVER_COM_port
// @Author:     Foster & Freeman Ltd
// @Created:    19.12.2024
//
// @Brief:      Thorlabs Power Meter (TLPM) communication wrapper.
//              Provides device scanning, connection, and power measurement
//              using the Thorlabs TLPMX library (WinUSB mode).
//
// @Tools:      Visual Studio 2019, C#
//
// @Revision:
// 19.12.2024    Initial version based on ThorlabsPowerMeterApp.
//
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace COMport
{
    /// <summary>
    /// Thorlabs Power Meter wrapper class using TLPMX library.
    /// Provides scanning, connection, and power measurement functionality.
    /// </summary>
    public class TLPM : IDisposable
    {
        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        // Member variables
        //
        private Thorlabs.TLPM_64.Interop.TLPM _device;
        private bool _isConnected;
        private bool _disposed;
        private const ushort CHANNEL = 1;

        private List<TLPMDeviceInfo> _scannedDevices = new List<TLPMDeviceInfo>();
        private string _lastError = "";

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        // Properties
        //
        public bool IsConnected => _isConnected;
        public string LastError => _lastError;

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Scan for Thorlabs power meter devices.
        /// </summary>
        /// <returns>Number of devices found</returns>
        public int ScanDevices()
        {
            _scannedDevices.Clear();

            try
            {
                var searchDevice = new Thorlabs.TLPM_64.Interop.TLPM(IntPtr.Zero);

                uint count;
                searchDevice.findRsrc(out count);

                Debug.WriteLine($"[TLPM] Found {count} device(s)");

                if (count > 0)
                {
                    StringBuilder resourceName = new StringBuilder(1024);

                    for (uint i = 0; i < count; i++)
                    {
                        searchDevice.getRsrcName(i, resourceName);
                        string resource = resourceName.ToString();

                        StringBuilder modelName = new StringBuilder(256);
                        StringBuilder serialNumber = new StringBuilder(256);
                        StringBuilder manufacturer = new StringBuilder(256);
                        bool available;

                        try
                        {
                            searchDevice.getRsrcInfo(i, modelName, serialNumber, manufacturer, out available);
                        }
                        catch
                        {
                            modelName.Append("Thorlabs PM");
                            serialNumber.Append("");
                            available = true;
                        }

                        _scannedDevices.Add(new TLPMDeviceInfo
                        {
                            ResourceName = resource,
                            ModelName = modelName.ToString(),
                            SerialNumber = serialNumber.ToString(),
                            IsAvailable = available
                        });

                        Debug.WriteLine($"[TLPM] Device {i}: {modelName} S/N: {serialNumber}");
                    }
                }

                searchDevice.Dispose();
            }
            catch (Exception ex)
            {
                _lastError = $"Scan error: {ex.Message}";
                Debug.WriteLine($"[TLPM] {_lastError}");
            }

            return _scannedDevices.Count;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Get list of scanned devices for display.
        /// </summary>
        public List<TLPMDeviceInfo> GetDeviceList()
        {
            return _scannedDevices;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Connect to a device by index in the scanned list.
        /// </summary>
        /// <param name="deviceIndex">Index in scanned device list</param>
        /// <returns>True if connected successfully</returns>
        public bool Connect(int deviceIndex)
        {
            if (deviceIndex < 0 || deviceIndex >= _scannedDevices.Count)
            {
                _lastError = "Invalid device index";
                return false;
            }

            return Connect(_scannedDevices[deviceIndex].ResourceName);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Connect to a device by resource name.
        /// </summary>
        /// <param name="resourceName">VISA resource name</param>
        /// <returns>True if connected successfully</returns>
        public bool Connect(string resourceName)
        {
            try
            {
                Disconnect();

                _device = new Thorlabs.TLPM_64.Interop.TLPM(resourceName, true, true);
                _isConnected = true;

                Debug.WriteLine($"[TLPM] Connected to {resourceName}");
                return true;
            }
            catch (Exception ex)
            {
                _lastError = $"Connection failed: {ex.Message}";
                Debug.WriteLine($"[TLPM] {_lastError}");
                Disconnect();
                return false;
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Disconnect from the current device.
        /// </summary>
        public void Disconnect()
        {
            _isConnected = false;
            if (_device != null)
            {
                try { _device.Dispose(); }
                catch { }
                _device = null;
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Read power measurement in Watts.
        /// </summary>
        /// <param name="power">Power value in Watts</param>
        /// <returns>True if read successful</returns>
        public bool ReadPower(out double power)
        {
            power = 0;
            if (!_isConnected || _device == null)
                return false;

            try
            {
                _device.measPower(out power);
                return true;
            }
            catch (Exception ex)
            {
                _lastError = $"Measurement error: {ex.Message}";
                Debug.WriteLine($"[TLPM] {_lastError}");
                return false;
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Get power reading as a formatted string suitable for display.
        /// Returns the power value with appropriate unit (W, mW, µW, nW).
        /// </summary>
        /// <returns>Formatted power string</returns>
        public string ReadPowerString()
        {
            double power;
            if (!ReadPower(out power))
                return "Error";

            // Auto-scale units
            if (Math.Abs(power) >= 1.0)
                return $"{power:F4} W";
            else if (Math.Abs(power) >= 1e-3)
                return $"{power * 1e3:F4} mW";
            else if (Math.Abs(power) >= 1e-6)
                return $"{power * 1e6:F4} µW";
            else
                return $"{power * 1e9:F4} nW";
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Configure wavelength setting.
        /// </summary>
        /// <param name="wavelengthNm">Wavelength in nanometers</param>
        public bool SetWavelength(double wavelengthNm)
        {
            if (!_isConnected || _device == null)
                return false;

            try
            {
                _device.setWavelength(wavelengthNm);
                return true;
            }
            catch (Exception ex)
            {
                _lastError = $"Set wavelength error: {ex.Message}";
                return false;
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Configure device settings for proper operation.
        /// Call this after connection to get meaningful readings.
        /// </summary>
        /// <param name="wavelengthNm">Wavelength in nm (default 635 for red LED)</param>
        /// <param name="avgCount">Averaging count (default 10)</param>
        /// <param name="autoRange">Enable auto-range (default true)</param>
        public bool ConfigureSettings(double wavelengthNm = 635, int avgCount = 10, bool autoRange = true)
        {
            if (!_isConnected || _device == null)
                return false;

            try
            {
                Debug.WriteLine($"[TLPM] Configuring: wavelength={wavelengthNm}nm, avg={avgCount}, autoRange={autoRange}");
                
                _device.setWavelength(wavelengthNm);
                _device.setAvgCnt((short)avgCount);
                _device.setPowerAutoRange(autoRange);
                
                Debug.WriteLine("[TLPM] Configuration complete");
                return true;
            }
            catch (Exception ex)
            {
                _lastError = $"Configuration error: {ex.Message}";
                Debug.WriteLine($"[TLPM] {_lastError}");
                return false;
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Get device identification string.
        /// </summary>
        public string GetIdentification()
        {
            if (!_isConnected || _device == null)
                return "";

            try
            {
                StringBuilder manufacturer = new StringBuilder(256);
                StringBuilder model = new StringBuilder(256);
                StringBuilder serial = new StringBuilder(256);
                StringBuilder firmware = new StringBuilder(256);

                _device.identificationQuery(manufacturer, model, serial, firmware);

                return $"{manufacturer},{model},{serial},{firmware}";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Dispose of resources.
        /// </summary>
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
    /// Information about a discovered TLPM device.
    /// </summary>
    public class TLPMDeviceInfo
    {
        public string ResourceName { get; set; }
        public string ModelName { get; set; }
        public string SerialNumber { get; set; }
        public bool IsAvailable { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(SerialNumber))
                return $"{ModelName} - S/N: {SerialNumber}";
            return ModelName;
        }
    }
}
