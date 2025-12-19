using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace ThorlabsPowerMeterApp.Services
{
    /// <summary>
    /// USB TMC service using WinUSB for direct bulk transfer communication
    /// Works with Thorlabs power meters in TMC mode without NI-VISA
    /// </summary>
    public class UsbTmcService : IDisposable
    {
        private const int THORLABS_VID = 0x1313;
        private const int PM100USB_PID = 0x8072;

        private SafeFileHandle _deviceHandle;
        private IntPtr _winUsbHandle = IntPtr.Zero;
        private byte _bulkInPipe;
        private byte _bulkOutPipe;
        private bool _isConnected;
        private bool _disposed;
        private byte _bTag = 1;

        public event EventHandler<string> OnError;
        public event EventHandler<string> OnDebug;

        public bool IsConnected => _isConnected;

        #region Win32 and WinUSB API

        // SetupAPI
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevs(
            ref Guid classGuid,
            string enumerator,
            IntPtr hwndParent,
            uint flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInterfaces(
            IntPtr deviceInfoSet,
            IntPtr deviceInfoData,
            ref Guid interfaceClassGuid,
            uint memberIndex,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(
            IntPtr deviceInfoSet,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
            IntPtr deviceInterfaceDetailData,
            uint deviceInterfaceDetailDataSize,
            out uint requiredSize,
            IntPtr deviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

        // Kernel32
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        // WinUSB
        [DllImport("winusb.dll", SetLastError = true)]
        private static extern bool WinUsb_Initialize(
            SafeFileHandle DeviceHandle,
            out IntPtr InterfaceHandle);

        [DllImport("winusb.dll", SetLastError = true)]
        private static extern bool WinUsb_Free(IntPtr InterfaceHandle);

        [DllImport("winusb.dll", SetLastError = true)]
        private static extern bool WinUsb_QueryInterfaceSettings(
            IntPtr InterfaceHandle,
            byte AlternateInterfaceNumber,
            out USB_INTERFACE_DESCRIPTOR UsbAltInterfaceDescriptor);

        [DllImport("winusb.dll", SetLastError = true)]
        private static extern bool WinUsb_QueryPipe(
            IntPtr InterfaceHandle,
            byte AlternateInterfaceNumber,
            byte PipeIndex,
            out WINUSB_PIPE_INFORMATION PipeInformation);

        [DllImport("winusb.dll", SetLastError = true)]
        private static extern bool WinUsb_WritePipe(
            IntPtr InterfaceHandle,
            byte PipeID,
            byte[] Buffer,
            uint BufferLength,
            out uint LengthTransferred,
            IntPtr Overlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        private static extern bool WinUsb_ReadPipe(
            IntPtr InterfaceHandle,
            byte PipeID,
            byte[] Buffer,
            uint BufferLength,
            out uint LengthTransferred,
            IntPtr Overlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        private static extern bool WinUsb_SetPipePolicy(
            IntPtr InterfaceHandle,
            byte PipeID,
            uint PolicyType,
            uint ValueLength,
            ref uint Value);

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid InterfaceClassGuid;
            public int Flags;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct USB_INTERFACE_DESCRIPTOR
        {
            public byte bLength;
            public byte bDescriptorType;
            public byte bInterfaceNumber;
            public byte bAlternateSetting;
            public byte bNumEndpoints;
            public byte bInterfaceClass;
            public byte bInterfaceSubClass;
            public byte bInterfaceProtocol;
            public byte iInterface;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WINUSB_PIPE_INFORMATION
        {
            public int PipeType;
            public byte PipeId;
            public ushort MaximumPacketSize;
            public byte Interval;
        }

        private const uint DIGCF_PRESENT = 0x02;
        private const uint DIGCF_DEVICEINTERFACE = 0x10;
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint FILE_SHARE_READ = 0x01;
        private const uint FILE_SHARE_WRITE = 0x02;
        private const uint OPEN_EXISTING = 3;
        private const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        
        private const uint PIPE_TRANSFER_TIMEOUT = 0x03;
        private const int USB_ENDPOINT_DIRECTION_MASK = 0x80;

        // USB TMC Device Interface GUID
        private static readonly Guid GUID_DEVINTERFACE_USB_DEVICE = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED");
        // Thorlabs USB TMC GUID
        private static readonly Guid GUID_USBTMC = new Guid("A9FDBB24-128A-11d5-9961-00108335E361");
        // Standard WinUSB device interface GUID
        private static readonly Guid GUID_DEVINTERFACE_WINUSB = new Guid("DEE824EF-729B-4A0E-9C14-B7117D33A817");
        // Thorlabs libusb/WinUSB GUID (used by their driver switcher)
        private static readonly Guid GUID_THORLABS_LIBUSB = new Guid("409A2B99-2C70-45E4-91CC-F3C28B28F5FD");

        #endregion

        /// <summary>
        /// Scan for Thorlabs USB TMC devices
        /// </summary>
        public List<UsbDeviceInfo> ScanDevices()
        {
            var devices = new List<UsbDeviceInfo>();

            // List of GUIDs to try for Thorlabs WinUSB devices
            Guid[] guidsToTry = new Guid[]
            {
                GUID_THORLABS_LIBUSB,    // Thorlabs libusb/WinUSB
                GUID_DEVINTERFACE_WINUSB, // Standard WinUSB
                GUID_USBTMC,              // USB TMC
                GUID_DEVINTERFACE_USB_DEVICE  // Generic USB
            };

            foreach (var guid in guidsToTry)
            {
                OnDebug?.Invoke(this, $"Scanning with GUID: {guid}");
                var foundDevices = ScanWithGuid(guid);
                
                foreach (var dev in foundDevices)
                {
                    // Check if it's a Thorlabs device and not already in list
                    if (dev.DevicePath.ToLower().Contains("vid_1313") && 
                        !devices.Exists(d => d.DevicePath == dev.DevicePath))
                    {
                        devices.Add(dev);
                    }
                }
            }

            // If still none found, try registry as last resort
            if (devices.Count == 0)
            {
                OnDebug?.Invoke(this, "No devices via SetupAPI, trying registry...");
                devices = ScanViaRegistry();
            }

            return devices;
        }

        private List<UsbDeviceInfo> ScanWithGuid(Guid interfaceGuid)
        {
            var devices = new List<UsbDeviceInfo>();

            try
            {
                IntPtr deviceInfoSet = SetupDiGetClassDevs(
                    ref interfaceGuid,
                    null,
                    IntPtr.Zero,
                    DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

                if (deviceInfoSet == IntPtr.Zero || deviceInfoSet == new IntPtr(-1))
                {
                    return devices;
                }

                try
                {
                    SP_DEVICE_INTERFACE_DATA interfaceData = new SP_DEVICE_INTERFACE_DATA();
                    interfaceData.cbSize = Marshal.SizeOf(interfaceData);

                    uint index = 0;
                    while (SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, ref interfaceGuid, index, ref interfaceData))
                    {
                        string devicePath = GetDevicePath(deviceInfoSet, ref interfaceData);
                        
                        if (!string.IsNullOrEmpty(devicePath))
                        {
                            OnDebug?.Invoke(this, $"Found device: {devicePath}");

                            // Check if it's a Thorlabs device
                            if (devicePath.ToLower().Contains("vid_1313"))
                            {
                                // Extract product info from path
                                string description = "Thorlabs PM100USB";
                                if (devicePath.ToLower().Contains("pid_8072"))
                                    description = "PM100USB";
                                else if (devicePath.ToLower().Contains("pid_8078"))
                                    description = "PM100D";

                                // Only add Interface 0 (TMC), not Interface 1 (DFU)
                                if (devicePath.ToLower().Contains("mi_00") || !devicePath.ToLower().Contains("mi_"))
                                {
                                    devices.Add(new UsbDeviceInfo
                                    {
                                        DevicePath = devicePath,
                                        Description = description,
                                        VendorId = THORLABS_VID,
                                        ProductId = ExtractPid(devicePath)
                                    });
                                }
                            }
                        }
                        index++;
                    }
                }
                finally
                {
                    SetupDiDestroyDeviceInfoList(deviceInfoSet);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"Scan error: {ex.Message}");
            }

            return devices;
        }

        private string GetDevicePath(IntPtr deviceInfoSet, ref SP_DEVICE_INTERFACE_DATA interfaceData)
        {
            uint requiredSize = 0;
            SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref interfaceData, IntPtr.Zero, 0, out requiredSize, IntPtr.Zero);

            if (requiredSize == 0)
                return null;

            IntPtr detailDataBuffer = Marshal.AllocHGlobal((int)requiredSize);
            try
            {
                // cbSize depends on architecture
                Marshal.WriteInt32(detailDataBuffer, IntPtr.Size == 8 ? 8 : 6);

                if (SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref interfaceData, detailDataBuffer, requiredSize, out requiredSize, IntPtr.Zero))
                {
                    return Marshal.PtrToStringAuto(IntPtr.Add(detailDataBuffer, 4));
                }
            }
            finally
            {
                Marshal.FreeHGlobal(detailDataBuffer);
            }

            return null;
        }

        private List<UsbDeviceInfo> ScanViaRegistry()
        {
            var devices = new List<UsbDeviceInfo>();

            try
            {
                using (var usbKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB"))
                {
                    if (usbKey != null)
                    {
                        foreach (string vidPid in usbKey.GetSubKeyNames())
                        {
                            if (vidPid.ToUpper().Contains("VID_1313"))
                            {
                                using (var deviceKey = usbKey.OpenSubKey(vidPid))
                                {
                                    if (deviceKey != null)
                                    {
                                        foreach (string serial in deviceKey.GetSubKeyNames())
                                        {
                                            using (var instanceKey = deviceKey.OpenSubKey(serial))
                                            {
                                                if (instanceKey != null)
                                                {
                                                    string friendlyName = instanceKey.GetValue("FriendlyName")?.ToString()
                                                        ?? instanceKey.GetValue("DeviceDesc")?.ToString()
                                                        ?? "Thorlabs PM100USB";

                                                    if (friendlyName.Contains(";"))
                                                        friendlyName = friendlyName.Split(';')[1];

                                                    devices.Add(new UsbDeviceInfo
                                                    {
                                                        DevicePath = $"USB\\{vidPid}\\{serial}",
                                                        Description = friendlyName,
                                                        SerialNumber = serial,
                                                        VendorId = THORLABS_VID,
                                                        ProductId = ExtractPid(vidPid)
                                                    });
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnDebug?.Invoke(this, $"Registry scan error: {ex.Message}");
            }

            return devices;
        }

        private int ExtractPid(string path)
        {
            try
            {
                int pidIndex = path.ToUpper().IndexOf("PID_");
                if (pidIndex >= 0 && pidIndex + 8 <= path.Length)
                {
                    string pidStr = path.Substring(pidIndex + 4, 4);
                    return Convert.ToInt32(pidStr, 16);
                }
            }
            catch { }
            return 0;
        }

        /// <summary>
        /// Connect to a USB TMC device using WinUSB
        /// </summary>
        public bool Connect(string devicePath)
        {
            try
            {
                Disconnect();

                OnDebug?.Invoke(this, $"Connecting to: {devicePath}");

                // Open the device
                _deviceHandle = CreateFile(
                    devicePath,
                    GENERIC_READ | GENERIC_WRITE,
                    FILE_SHARE_READ | FILE_SHARE_WRITE,
                    IntPtr.Zero,
                    OPEN_EXISTING,
                    FILE_ATTRIBUTE_NORMAL | FILE_FLAG_OVERLAPPED,
                    IntPtr.Zero);

                if (_deviceHandle == null || _deviceHandle.IsInvalid)
                {
                    int error = Marshal.GetLastWin32Error();
                    OnError?.Invoke(this, $"CreateFile failed (Error {error})");
                    return false;
                }

                // Initialize WinUSB
                if (!WinUsb_Initialize(_deviceHandle, out _winUsbHandle))
                {
                    int error = Marshal.GetLastWin32Error();
                    OnError?.Invoke(this, $"WinUsb_Initialize failed (Error {error})");
                    Disconnect();
                    return false;
                }

                // Get interface descriptor
                USB_INTERFACE_DESCRIPTOR interfaceDescriptor;
                if (!WinUsb_QueryInterfaceSettings(_winUsbHandle, 0, out interfaceDescriptor))
                {
                    OnError?.Invoke(this, "Failed to query interface settings");
                    Disconnect();
                    return false;
                }

                OnDebug?.Invoke(this, $"Interface: Class={interfaceDescriptor.bInterfaceClass}, SubClass={interfaceDescriptor.bInterfaceSubClass}, Endpoints={interfaceDescriptor.bNumEndpoints}");

                // Find bulk IN and OUT pipes
                _bulkInPipe = 0;
                _bulkOutPipe = 0;

                for (byte i = 0; i < interfaceDescriptor.bNumEndpoints; i++)
                {
                    WINUSB_PIPE_INFORMATION pipeInfo;
                    if (WinUsb_QueryPipe(_winUsbHandle, 0, i, out pipeInfo))
                    {
                        OnDebug?.Invoke(this, $"Pipe {i}: Type={pipeInfo.PipeType}, ID=0x{pipeInfo.PipeId:X2}");

                        // PipeType 2 = Bulk
                        if (pipeInfo.PipeType == 2)
                        {
                            if ((pipeInfo.PipeId & USB_ENDPOINT_DIRECTION_MASK) != 0)
                            {
                                _bulkInPipe = pipeInfo.PipeId;
                            }
                            else
                            {
                                _bulkOutPipe = pipeInfo.PipeId;
                            }
                        }
                    }
                }

                if (_bulkInPipe == 0 || _bulkOutPipe == 0)
                {
                    OnError?.Invoke(this, $"Could not find bulk endpoints (IN=0x{_bulkInPipe:X2}, OUT=0x{_bulkOutPipe:X2})");
                    Disconnect();
                    return false;
                }

                OnDebug?.Invoke(this, $"Bulk pipes: IN=0x{_bulkInPipe:X2}, OUT=0x{_bulkOutPipe:X2}");

                // Set timeout (5 seconds)
                uint timeout = 5000;
                WinUsb_SetPipePolicy(_winUsbHandle, _bulkInPipe, PIPE_TRANSFER_TIMEOUT, 4, ref timeout);
                WinUsb_SetPipePolicy(_winUsbHandle, _bulkOutPipe, PIPE_TRANSFER_TIMEOUT, 4, ref timeout);

                _isConnected = true;

                // Try to get device identity
                string identity = Query("*IDN?");
                OnDebug?.Invoke(this, $"Device identity: {identity}");

                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"Connect error: {ex.Message}");
                Disconnect();
                return false;
            }
        }

        public void Disconnect()
        {
            _isConnected = false;

            if (_winUsbHandle != IntPtr.Zero)
            {
                try { WinUsb_Free(_winUsbHandle); } catch { }
                _winUsbHandle = IntPtr.Zero;
            }

            if (_deviceHandle != null && !_deviceHandle.IsInvalid)
            {
                try { _deviceHandle.Close(); } catch { }
                _deviceHandle = null;
            }

            _bulkInPipe = 0;
            _bulkOutPipe = 0;
        }

        /// <summary>
        /// Build USB TMC DEV_DEP_MSG_OUT packet
        /// </summary>
        private byte[] BuildTmcOutPacket(string command)
        {
            byte[] cmdBytes = Encoding.ASCII.GetBytes(command);
            int dataLength = cmdBytes.Length;
            int padding = (4 - (dataLength % 4)) % 4;
            int totalLength = 12 + dataLength + padding;

            _bTag++;
            if (_bTag == 0) _bTag = 1;

            byte[] packet = new byte[totalLength];

            // USB TMC Header (12 bytes)
            packet[0] = 1;                    // MsgID: DEV_DEP_MSG_OUT
            packet[1] = _bTag;                // bTag
            packet[2] = (byte)(~_bTag);       // bTagInverse
            packet[3] = 0;                    // Reserved
            
            // TransferSize (4 bytes, little-endian)
            packet[4] = (byte)(dataLength & 0xFF);
            packet[5] = (byte)((dataLength >> 8) & 0xFF);
            packet[6] = (byte)((dataLength >> 16) & 0xFF);
            packet[7] = (byte)((dataLength >> 24) & 0xFF);
            
            packet[8] = 1;                    // bmTransferAttributes: EOM = 1
            packet[9] = 0;                    // Reserved
            packet[10] = 0;                   // Reserved
            packet[11] = 0;                   // Reserved

            // Data
            Array.Copy(cmdBytes, 0, packet, 12, dataLength);

            return packet;
        }

        /// <summary>
        /// Build USB TMC REQUEST_DEV_DEP_MSG_IN packet
        /// </summary>
        private byte[] BuildTmcInRequestPacket(int maxLength)
        {
            _bTag++;
            if (_bTag == 0) _bTag = 1;

            byte[] packet = new byte[12];

            packet[0] = 2;                    // MsgID: REQUEST_DEV_DEP_MSG_IN
            packet[1] = _bTag;                // bTag
            packet[2] = (byte)(~_bTag);       // bTagInverse
            packet[3] = 0;                    // Reserved
            
            // TransferSize (4 bytes, little-endian)
            packet[4] = (byte)(maxLength & 0xFF);
            packet[5] = (byte)((maxLength >> 8) & 0xFF);
            packet[6] = (byte)((maxLength >> 16) & 0xFF);
            packet[7] = (byte)((maxLength >> 24) & 0xFF);
            
            packet[8] = 0;                    // bmTransferAttributes
            packet[9] = 0;                    // TermChar
            packet[10] = 0;                   // Reserved
            packet[11] = 0;                   // Reserved

            return packet;
        }

        public bool Write(string command)
        {
            if (!_isConnected || _winUsbHandle == IntPtr.Zero) return false;

            try
            {
                byte[] packet = BuildTmcOutPacket(command);
                uint bytesWritten;

                bool success = WinUsb_WritePipe(_winUsbHandle, _bulkOutPipe, packet, (uint)packet.Length, out bytesWritten, IntPtr.Zero);
                
                if (!success)
                {
                    int error = Marshal.GetLastWin32Error();
                    OnError?.Invoke(this, $"Write failed (Error {error})");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"Write error: {ex.Message}");
                return false;
            }
        }

        public string Read(int timeout = 2000)
        {
            if (!_isConnected || _winUsbHandle == IntPtr.Zero) return null;

            try
            {
                // Send read request
                byte[] request = BuildTmcInRequestPacket(1024);
                uint bytesWritten;
                
                if (!WinUsb_WritePipe(_winUsbHandle, _bulkOutPipe, request, (uint)request.Length, out bytesWritten, IntPtr.Zero))
                {
                    int error = Marshal.GetLastWin32Error();
                    OnDebug?.Invoke(this, $"Read request failed (Error {error})");
                    return null;
                }

                // Read response
                byte[] buffer = new byte[1024];
                uint bytesRead;

                if (!WinUsb_ReadPipe(_winUsbHandle, _bulkInPipe, buffer, (uint)buffer.Length, out bytesRead, IntPtr.Zero))
                {
                    int error = Marshal.GetLastWin32Error();
                    OnDebug?.Invoke(this, $"Read failed (Error {error})");
                    return null;
                }

                if (bytesRead > 12)
                {
                    // Parse TMC response header
                    int dataLength = BitConverter.ToInt32(buffer, 4);
                    int actualLength = Math.Min(dataLength, (int)bytesRead - 12);
                    
                    if (actualLength > 0)
                    {
                        string response = Encoding.ASCII.GetString(buffer, 12, actualLength);
                        return response.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"Read error: {ex.Message}");
            }

            return null;
        }

        public string Query(string command, int timeout = 2000)
        {
            if (Write(command))
            {
                Thread.Sleep(50);
                return Read(timeout);
            }
            return null;
        }

        public bool ReadPower(out double power)
        {
            power = 0;
            string response = Query("MEAS:POW?");
            if (!string.IsNullOrEmpty(response))
            {
                // Handle scientific notation
                if (double.TryParse(response, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out power))
                {
                    return true;
                }
            }
            return false;
        }

        public bool SetWavelength(double wavelengthNm) => Write($"SENS:CORR:WAV {wavelengthNm}");
        public bool SetBeamDiameter(double diameterMm) => Write($"SENS:CORR:BEAM {diameterMm / 1000.0}");
        public bool SetAveraging(int count) => Write($"SENS:AVER:COUN {count}");
        public bool SetAutoRange(bool enabled) => Write($"SENS:POW:RANG:AUTO {(enabled ? "ON" : "OFF")}");
        public string GetIdentity() => Query("*IDN?");

        public void Dispose()
        {
            if (!_disposed)
            {
                Disconnect();
                _disposed = true;
            }
        }
    }

    public class UsbDeviceInfo
    {
        public string DevicePath { get; set; }
        public string Description { get; set; }
        public string SerialNumber { get; set; }
        public int VendorId { get; set; }
        public int ProductId { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(SerialNumber))
                return $"{Description} (S/N: {SerialNumber})";
            return Description;
        }
    }
}
