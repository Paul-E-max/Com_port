// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
// @File:       comms_D2XX.cs
// @Project:    DISCOVER_COM_port
// @Author:     Foster & Freeman Ltd - Nick Atkins
// @Created:    11.04.2023
//
// @Brief:      D2xx interface for serial devices.
//
// @Tools:      Visual Studio 2019, C#
//
// @Revision:
// 13.05.2025-MD In OpenBySerial(), either "Unknown" or "BOOTLOAD" opens the first available D2XX port.
// 01.06.2023-MD Tranplanted into DISCOVER COM port.
// 11.05.2023-NA Initial version.
//
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using FTD2XX_NET;

namespace COMport
{
    class D2XX
    {
        public bool IsConnected = false;        // Connection Status - Can be queried to check whether we have an active connection
        public string Terminator = "\n";        // Terminator - Can be changed based on the current device type

        private FTDI.FT_STATUS FTDI_Status = FTDI.FT_STATUS.FT_OK;
        private FTDI FTDI_Device = new FTDI();

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		/// <summary>
		/// Scan for available D2XX ports, output debug information to console about ports
		/// </summary>
		/// <returns>Available device count</returns>
		///
        public UInt32 ScanD2XX()
        {
            UInt32 FTDI_DeviceCount = 0;

            // Check how many active D2XX devices we have
            FTDI_Status = FTDI_Device.GetNumberOfDevices(ref FTDI_DeviceCount);

            // Output number of devices to console
            if (FTDI_Status == FTDI.FT_STATUS.FT_OK)
            {
                Debug.WriteLine("Number of FTDI devices: " + FTDI_DeviceCount.ToString());
            }
            else
            {
                Debug.WriteLine("Failed to get number of devices (error " + FTDI_Status.ToString() + ")");
            }

            // Allocate storage for device info list
            FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[FTDI_DeviceCount];

            // Populate our device list
            FTDI_Status = FTDI_Device.GetDeviceList(ftdiDeviceList);

            // Dump device list to console for debugging
            if (FTDI_Status == FTDI.FT_STATUS.FT_OK)
            {
                for (UInt32 i = 0; i < FTDI_DeviceCount; i++)
                {
                    Debug.WriteLine("Device Index: " + i.ToString());
                    Debug.WriteLine("Flags: " + String.Format("{0:x}", ftdiDeviceList[i].Flags));
                    Debug.WriteLine("Type: " + ftdiDeviceList[i].Type.ToString());
                    Debug.WriteLine("ID: " + String.Format("{0:x}", ftdiDeviceList[i].ID));
                    Debug.WriteLine("Location ID: " + String.Format("{0:x}", ftdiDeviceList[i].LocId));
                    Debug.WriteLine("Serial Number: " + ftdiDeviceList[i].SerialNumber.ToString());
                    Debug.WriteLine("Description: " + ftdiDeviceList[i].Description.ToString());
                    Debug.WriteLine("");
                }
            }
            return FTDI_DeviceCount;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Open a D2XX connection using the device serial attribute, output debug information to console about ports
        /// </summary>
        /// <param name="DeviceSerial"></param>
        /// <returns>Available device count</returns>
        ///
        /// Note that "Unknown" or "BOOTLOAD" will open the first D2XX port available.
        ///
        public void OpenBySerial(string DeviceSerial)
        {
            UInt32 FTDI_DeviceCount = 0;

            // Check how many active D2XX devices we have
            FTDI_Status = FTDI_Device.GetNumberOfDevices(ref FTDI_DeviceCount);

            // Output number of devices to console
            if (FTDI_Status == FTDI.FT_STATUS.FT_OK)
            {
                Debug.WriteLine("Number of FTDI devices: " + FTDI_DeviceCount.ToString());
            }
            else
            {
                Debug.WriteLine("Failed to get number of devices (error " + FTDI_Status.ToString() + ")");
            }

            // Allocate storage for device info list
            FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[FTDI_DeviceCount];

            // Populate our device list
            FTDI_Status = FTDI_Device.GetDeviceList(ftdiDeviceList);

            // Dump list to console for debugging
            if (FTDI_Status == FTDI.FT_STATUS.FT_OK)
            {
                for (UInt32 i = 0; i < FTDI_DeviceCount; i++)
                {
                    Debug.WriteLine("Device Index: " + i.ToString());
                    Debug.WriteLine("Flags: " + String.Format("{0:x}", ftdiDeviceList[i].Flags));
                    Debug.WriteLine("Type: " + ftdiDeviceList[i].Type.ToString());
                    Debug.WriteLine("ID: " + String.Format("{0:x}", ftdiDeviceList[i].ID));
                    Debug.WriteLine("Location ID: " + String.Format("{0:x}", ftdiDeviceList[i].LocId));
                    Debug.WriteLine("Serial Number: " + ftdiDeviceList[i].SerialNumber.ToString());
                    Debug.WriteLine("Description: " + ftdiDeviceList[i].Description.ToString());
                    Debug.WriteLine("");
                }
            }

            // Attempt to open using serial number
            // Only open if there isn't another port open, to prevent errors
            if (FTDI_Device.IsOpen == false)
            {
                if ((FTDI_DeviceCount > 0) && (("Unknown" == DeviceSerial) || ("BOOTLOAD" == DeviceSerial))) DeviceSerial = ftdiDeviceList[0].SerialNumber; // Just use first available device.
                //
                FTDI_Status = FTDI_Device.OpenBySerialNumber(DeviceSerial);
            }

            // Check status after attempting a conneciton, output to console
            if (FTDI_Status != FTDI.FT_STATUS.FT_OK)
            {
                Debug.WriteLine("Failed to open device (error " + FTDI_Status.ToString() + ")");
            }
            else
            {
                Debug.WriteLine(DeviceSerial + " Device Opened");
            }

            // Update connection status
            if (FTDI_Device.IsOpen == true)
            {
                this.IsConnected = true;
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Close a D2xx port, only if we have one open
        /// </summary>
        ///
        public void Close()
        {
            // Only close if there is a port open, to prevent errors
            if (FTDI_Device.IsOpen == true)
            {
                FTDI_Device.Close();
            }

            // Check port is closed and update
            if (FTDI_Device.IsOpen == false)
            {
                this.IsConnected = false;
                Debug.WriteLine("Device Closed");
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Write an array of bytes out to an open D2xx device
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns>Data read back from device</returns>
        ///
        public void Write(byte[] buffer, int offset, int count)
        {
            uint bytesWritten = 0;

            FTDI_Status = FTDI_Device.Write(buffer, count, ref bytesWritten);
            Debug.WriteLine(count + " byte(s) sent: [" + buffer[0].ToString() + "]");

            // Catch any errors and output to console
            if (FTDI_Status != FTDI.FT_STATUS.FT_OK)
            {
                Debug.WriteLine("Failed to write to device (error " + FTDI_Status.ToString() + ")");
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Write a string to an open D2xx device
        /// </summary>
        /// <param name="text"></param>
        /// 
        ///
        public void WriteLine(string text)
        {
            UInt32 numBytesWritten = 0;

            // Add terminator to the line write
            text = text + Terminator;

            // Write data to buffer
            FTDI_Status = FTDI_Device.Write(text, text.Length, ref numBytesWritten);
            Debug.WriteLine("Command Sent: " + text);

            // Catch any errors and output to console
            if (FTDI_Status != FTDI.FT_STATUS.FT_OK)
            {
                Debug.WriteLine("Failed to write to device (error " + FTDI_Status.ToString() + ")");
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Read a line from an open D2xx device
        /// </summary>
        /// <returns>Text read back from device</returns>
        ///
        public string ReadLine()
        {
            string text = "";

            UInt32 numBytesAvailable = 0;
            UInt32 numBytesRead = 0;

            Thread.Sleep(30);       // Sleep to ensure previous write has completed

            // Check how many bytes we have before reading
            FTDI_Status = FTDI_Device.GetRxBytesAvailable(ref numBytesAvailable);

            // Output error if we can't read back number of bytes
            if (FTDI_Status != FTDI.FT_STATUS.FT_OK)
            {
                Debug.WriteLine("Failed to get number of bytes available to read (error " + FTDI_Status.ToString() + ")");
            }

            Debug.WriteLine("Read Bytes: " + numBytesAvailable.ToString());

            // Read out data in buffer and also write to console
            FTDI_Status = FTDI_Device.Read(out text, numBytesAvailable, ref numBytesRead);
            if (FTDI_Status != FTDI.FT_STATUS.FT_OK)
            {
                Debug.WriteLine("Failed to read data (error " + FTDI_Status.ToString() + ")");
            }

            Debug.WriteLine("Data read from device: " + text);

            return text;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Read a char from an open D2xx device
        /// </summary>
        /// <returns>Char read back from device</returns>
        ///
        public int ReadChar()
        {
            string text = "";

            UInt32 numBytesAvailable = 0;
            UInt32 numBytesRead = 0;

            Thread.Sleep(30);                    // Sleep to ensure previous write has completed

            // Check how many bytes we have before reading
            FTDI_Status = FTDI_Device.GetRxBytesAvailable(ref numBytesAvailable);

            // Output error if we can't read back number of bytes
            if (FTDI_Status != FTDI.FT_STATUS.FT_OK)
            {
                Debug.WriteLine("Failed to get number of bytes available to read (error " + FTDI_Status.ToString() + ")");
            }

            Debug.WriteLine("Read 1 byte");

            // Read out single char and also write to console
            FTDI_Status = FTDI_Device.Read(out text, 1, ref numBytesRead);
            if (FTDI_Status != FTDI.FT_STATUS.FT_OK)
            {
                Debug.WriteLine("Failed to read data (error " + FTDI_Status.ToString() + ")");
            }

            Debug.WriteLine("Data read from device: " + text);

            return text[0];
        }


        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Read a defined length of data back from an open D2xx device
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns>Data read back from device</returns>
        ///
        public void Read(byte[] buffer, int offset, int count)
        {
            string text = "";

            UInt32 numBytesAvailable = 0;
            UInt32 numBytesRead = 0;

            Thread.Sleep(30);                    // Sleep to ensure previous write has completed

            // Check how many bytes we have before reading
            FTDI_Status = FTDI_Device.GetRxBytesAvailable(ref numBytesAvailable);

            // Output error if we can't read back number of bytes
            if (FTDI_Status != FTDI.FT_STATUS.FT_OK)
            {
                Debug.WriteLine("Failed to get number of bytes available to read (error " + FTDI_Status.ToString() + ")");
            }

          
            FTDI_Status = FTDI_Device.Read(buffer, (uint)count, ref numBytesRead);
            if (FTDI_Status != FTDI.FT_STATUS.FT_OK)
            {
                Debug.WriteLine("Failed to read data (error " + FTDI_Status.ToString() + ")");
            }

            // Read out data count and also write to console
            Debug.WriteLine("Read Bytes: " + numBytesRead.ToString());

            Debug.WriteLine("Data read from device: " + text);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Get the number of bytes currently availble in the D2XX RX buffer
        /// </summary>
        /// <returns>Number of bytes availalble to read back</returns>
        ///
        public UInt32 BytesToRead()
        {
            UInt32 numBytesAvailable = 0;

            // Check how many bytes we have
            FTDI_Status = FTDI_Device.GetRxBytesAvailable(ref numBytesAvailable);

            // Output error if we can't read back number of bytes
            if (FTDI_Status != FTDI.FT_STATUS.FT_OK)
            {
                Debug.WriteLine("Failed to get number of bytes available to read (error " + FTDI_Status.ToString() + ")");
            }

            return numBytesAvailable;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Flush both the RX and TX buffers of the D2XX device
        /// </summary>
        ///
        public void ClearBuffer()
        {
            // Purge TX buffer and check for errors
            FTDI_Status = FTDI_Device.Purge(FTDI.FT_PURGE.FT_PURGE_TX);
            if (FTDI_Status != FTDI.FT_STATUS.FT_OK)
            {
                Debug.WriteLine("Failed to clear output buffer (error " + FTDI_Status.ToString() + ")");
            }

            // Purge RX buffer and check for errors
            FTDI_Status = FTDI_Device.Purge(FTDI.FT_PURGE.FT_PURGE_RX);
            if (FTDI_Status != FTDI.FT_STATUS.FT_OK)
            {
                Debug.WriteLine("Failed to clear input buffer (error " + FTDI_Status.ToString() + ")");
            }
        }
    }
}
