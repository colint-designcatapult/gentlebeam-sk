using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Xcc.Infra.Services.UPS
{
    public interface IHidDevice : IDisposable
    {
        bool IsConnected();
        void Write(byte[] data);
        byte[] Read();
        void Close();
    }

    public class HidDevice : IHidDevice
    {
        #region constants

        private const int DIGCF_DEFAULT = 0x1;
        private const int DIGCF_PRESENT = 0x2;
        private const int DIGCF_ALLCLASSES = 0x4;
        private const int DIGCF_PROFILE = 0x8;
        private const int DIGCF_DEVICEINTERFACE = 0x10;

        private const short FILE_ATTRIBUTE_NORMAL = 0x80;
        private const short INVALID_HANDLE_VALUE = -1;
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint CREATE_NEW = 1;
        private const uint CREATE_ALWAYS = 2;
        private const uint OPEN_EXISTING = 3;

        #endregion

        #region win32_API_declarations

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern nint SetupDiGetClassDevs(ref Guid ClassGuid,
                                                         nint Enumerator,
                                                         nint hwndParent,
                                                         uint Flags);

        [DllImport("hid.dll", SetLastError = true)]
        private static extern void HidD_GetHidGuid(ref Guid hidGuid);

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInterfaces(
           nint hDevInfo,
           //ref SP_DEVINFO_DATA devInfo,
           nint devInfo,
           ref Guid interfaceClassGuid,
           uint memberIndex,
           ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData
        );

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(
           nint hDevInfo,
           ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
           ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,
           uint deviceInterfaceDetailDataSize,
           out uint requiredSize,
           ref SP_DEVINFO_DATA deviceInfoData
        );

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiDestroyDeviceInfoList(nint DeviceInfoSet);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess,
            uint dwShareMode, nint lpSecurityAttributes, uint dwCreationDisposition,
            uint dwFlagsAndAttributes, nint hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile(SafeFileHandle hFile, byte[] lpBuffer,
           uint nNumberOfBytesToRead, ref uint lpNumberOfBytesRead, nint lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteFile(SafeFileHandle hFile, byte[] lpBuffer,
           uint nNumberOfBytesToWrite, ref uint lpNumberOfBytesWritten, nint lpOverlapped);

        [DllImport("hid.dll", SetLastError = true)]
        private static extern bool HidD_GetPreparsedData(
            SafeFileHandle hObject,
            ref nint PreparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        private static extern bool HidD_FreePreparsedData(ref nint PreparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        private static extern int HidP_GetCaps(
            nint pPHIDP_PREPARSED_DATA,                   // IN PHIDP_PREPARSED_DATA  PreparsedData,
            ref HIDP_CAPS myPHIDP_CAPS);                // OUT PHIDP_CAPS  Capabilities

        [DllImport("hid.dll", SetLastError = true)]
        private static extern bool HidD_GetAttributes(SafeFileHandle hObject, ref HIDD_ATTRIBUTES Attributes);

        [DllImport("hid.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool HidD_GetFeature(
           nint hDevice,
           nint hReportBuffer,
           uint ReportBufferLength);

        [DllImport("hid.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool HidD_SetFeature(
           nint hDevice,
           nint ReportBuffer,
           uint ReportBufferLength);

        [DllImport("hid.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool HidD_GetProductString(
           SafeFileHandle hDevice,
           nint Buffer,
           uint BufferLength);

        [DllImport("hid.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool HidD_GetSerialNumberString(
           SafeFileHandle hDevice,
           nint Buffer,
           uint BufferLength);

        [DllImport("hid.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool HidD_GetManufacturerString(
            SafeFileHandle hDevice,
            nint Buffer,
            uint BufferLength);

        #endregion

        #region structs

        public struct InterfaceDetails
        {
            public string manufacturer;
            public string product;
            public decimal serialNumber;
            public ushort VID;
            public ushort PID;
            public string devicePath;
            public int IN_reportByteLength;
            public int OUT_reportByteLength;
            public ushort versionNumber;
        }

        // HIDP_CAPS
        [StructLayout(LayoutKind.Sequential)]
        private struct HIDP_CAPS
        {
            public ushort Usage;                 // USHORT
            public ushort UsagePage;             // USHORT
            public ushort InputReportByteLength;
            public ushort OutputReportByteLength;
            public ushort FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public ushort[] Reserved;                // USHORT  Reserved[17];			
            public ushort NumberLinkCollectionNodes;
            public ushort NumberInputButtonCaps;
            public ushort NumberInputValueCaps;
            public ushort NumberInputDataIndices;
            public ushort NumberOutputButtonCaps;
            public ushort NumberOutputValueCaps;
            public ushort NumberOutputDataIndices;
            public ushort NumberFeatureButtonCaps;
            public ushort NumberFeatureValueCaps;
            public ushort NumberFeatureDataIndices;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public nint Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVICE_INTERFACE_DATA
        {
            public uint cbSize;
            public Guid InterfaceClassGuid;
            public uint Flags;
            public nint Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public int cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string DevicePath;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HIDD_ATTRIBUTES
        {
            public int Size;
            public short VendorID;
            public short ProductID;
            public short VersionNumber;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct COMMTIMEOUTS
        {
            public uint ReadIntervalTimeout;
            public uint ReadTotalTimeoutMultiplier;
            public uint ReadTotalTimeoutConstant;
            public uint WriteTotalTimeoutMultiplier;
            public uint WriteTotalTimeoutConstant;
        }

        #endregion

        #region globals

        public bool deviceConnected
        {
            get; set;
        }
        public event dataReceivedEvent? dataReceived;    //The calling class can subscribe to this event
        public delegate void dataReceivedEvent(byte[] message);

        private SafeFileHandle? handle_read;
        private SafeFileHandle? handle_write;
        private FileStream? FS_read;
        private FileStream? FS_write;
        private HIDP_CAPS capabilities;
        private InterfaceDetails productInfo;
        private byte[]? readData;
        private bool useAsyncReads;
        
        #endregion

        #region static_methods

        public static InterfaceDetails[] GetConnectedDevices()
        {
            InterfaceDetails[] devices = Array.Empty<InterfaceDetails>();

            //Create structs to hold interface information
            SP_DEVINFO_DATA devInfo = new SP_DEVINFO_DATA();
            SP_DEVICE_INTERFACE_DATA devIface = new SP_DEVICE_INTERFACE_DATA();
            devInfo.cbSize = (uint)Marshal.SizeOf(devInfo);
            devIface.cbSize = (uint)Marshal.SizeOf(devIface);

            Guid G = new Guid();
            HidD_GetHidGuid(ref G); //Get the guid of the HID device class

            nint i = SetupDiGetClassDevs(ref G, nint.Zero, nint.Zero, DIGCF_DEVICEINTERFACE | DIGCF_PRESENT);

            //Loop through all available entries in the device list, until false
            SP_DEVICE_INTERFACE_DETAIL_DATA didd = new SP_DEVICE_INTERFACE_DETAIL_DATA();
            if (nint.Size == 8) // for 64 bit operating systems
                didd.cbSize = 8;
            else
                didd.cbSize = 4 + Marshal.SystemDefaultCharSize; // for 32 bit systems

            int j = -1;
            bool b = true;
            int error;
            SafeFileHandle tempHandle;

            while (b)
            {
                j++;

                b = SetupDiEnumDeviceInterfaces(i, nint.Zero, ref G, (uint)j, ref devIface);
                error = Marshal.GetLastWin32Error();
                if (b == false)
                    break;

                uint requiredSize = 0;
                bool b1 = SetupDiGetDeviceInterfaceDetail(i, ref devIface, ref didd, 256, out requiredSize, ref devInfo);
                string devicePath = didd.DevicePath;

                //create file handles using CT_CreateFile
                tempHandle = CreateFile(devicePath, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE,
                    nint.Zero, OPEN_EXISTING, 0, nint.Zero);

                //get capabilites - use getPreParsedData, and getCaps
                //store the reportlengths
                nint ptrToPreParsedData = new nint();
                bool ppdSucsess = HidD_GetPreparsedData(tempHandle, ref ptrToPreParsedData);
                if (ppdSucsess == false)
                    continue;

                HIDP_CAPS capabilities = new HIDP_CAPS();
                int hidCapsSucsess = HidP_GetCaps(ptrToPreParsedData, ref capabilities);

                HIDD_ATTRIBUTES attributes = new HIDD_ATTRIBUTES();
                bool hidAttribSucsess = HidD_GetAttributes(tempHandle, ref attributes);

                string? productName = "";
                string? SN = "";
                string? manfString = "";
                nint buffer = Marshal.AllocHGlobal(126);//max alloc for string; 
                if (HidD_GetProductString(tempHandle, buffer, 126)) productName = Marshal.PtrToStringAuto(buffer);
                if (HidD_GetSerialNumberString(tempHandle, buffer, 126)) SN = Marshal.PtrToStringAuto(buffer);
                if (HidD_GetManufacturerString(tempHandle, buffer, 126)) manfString = Marshal.PtrToStringAuto(buffer);
                Marshal.FreeHGlobal(buffer);

                //Call freePreParsedData to release some stuff
                HidD_FreePreparsedData(ref ptrToPreParsedData);

                //If connection was successful, record the values in a global struct
                InterfaceDetails productInfo = new InterfaceDetails
                {
                    devicePath = devicePath,
                    manufacturer = manfString ?? string.Empty,
                    product = productName ?? string.Empty,
                    PID = (ushort)attributes.ProductID,
                    VID = (ushort)attributes.VendorID,
                    versionNumber = (ushort)attributes.VersionNumber,
                    IN_reportByteLength = capabilities.InputReportByteLength,
                    OUT_reportByteLength = capabilities.OutputReportByteLength
                };

                if (StringIsInteger(SN))
                    productInfo.serialNumber = Convert.ToDecimal(SN);  //Check that serial number is actually a number

                int newSize = devices.Length + 1;
                Array.Resize(ref devices, newSize);
                devices[newSize - 1] = productInfo;
            }
            SetupDiDestroyDeviceInfoList(i);

            return devices;
        }

        #endregion

        #region constructors & initializers

        /// <summary>
        /// Creates an object to handle read/write functionality for a USB HID device
        /// Uses one filestream for each of read/write to allow for a write to occur during a blocking
        /// asynchronous read
        /// </summary>
        /// <param name="VID">The vendor ID of the USB device to connect to</param>
        /// <param name="PID">The product ID of the USB device to connect to</param>
        /// <param name="serialNumber">The serial number of the USB device to connect to</param>
        /// <param name="useAsyncReads">True - Read the device and generate events on data being available</param>
        public HidDevice()
        {

        }

        public void Initialize(ushort VID, ushort PID, ushort serialNumber, bool useAsyncReads)
        {
            InterfaceDetails[] devices = GetConnectedDevices();

            //loop through all connected devices to find one with the correct details
            for (int i = 0; i < devices.Length; i++)
            {
                if (devices[i].VID == VID && devices[i].PID == PID && devices[i].serialNumber == serialNumber)
                    InitDevice(devices[i].devicePath, useAsyncReads);
            }

            if (!deviceConnected)
            {
                string hexVID = NumToHexString(VID);
                string hexPID = NumToHexString(PID);
                throw new Exception("Device with VID: 0x" + hexVID + " PID: 0x" + hexPID + " SerialNumber: " + serialNumber.ToString() + " could not be found");
            }
        }

        /// <summary>
        /// Creates an object to handle read/write functionality for a USB HID device
        /// Uses one filestream for each of read/write to allow for a write to occur during a blocking
        /// asynchronous read
        /// </summary>
        /// <param name="devicePath">The USB device path - from getConnectedDevices</param>
        /// <param name="useAsyncReads">True - Read the device and generate events on data being available</param>
        public void Initialize(string devicePath, bool useAsyncReads)
        {
            InitDevice(devicePath, useAsyncReads);

            if (!deviceConnected)
            {
                throw new Exception("Device could not be found");
            }
        }

        public bool IsConnected()
        {
            if (handle_read is null)
                throw new Exception($"Unable to check if device is alive: {nameof(handle_read)} is null");

            HIDD_ATTRIBUTES attributes = new HIDD_ATTRIBUTES();
            return HidD_GetAttributes(handle_read, ref attributes);
        }

        #endregion

        #region functions

        private void InitDevice(string devicePath, bool useAsyncReads)
        {
            deviceConnected = false;

            //create file handles using CT_CreateFile
            handle_read = CreateFile(devicePath, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE,
                nint.Zero, OPEN_EXISTING, 0, nint.Zero);

            handle_write = CreateFile(devicePath, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE,
                nint.Zero, OPEN_EXISTING, 0, nint.Zero);

            //get capabilites - use getPreParsedData, and getCaps
            //store the reportlengths
            nint ptrToPreParsedData = new nint();
            bool ppdSucsess = HidD_GetPreparsedData(handle_read, ref ptrToPreParsedData);

            capabilities = new HIDP_CAPS();
            int hidCapsSucsess = HidP_GetCaps(ptrToPreParsedData, ref capabilities);

            HIDD_ATTRIBUTES attributes = new HIDD_ATTRIBUTES();
            bool hidAttribSucsess = HidD_GetAttributes(handle_read, ref attributes);

            string? productName = "";
            string? SN = "";
            string? manfString = "";
            nint buffer = Marshal.AllocHGlobal(126);//max alloc for string; 
            if (HidD_GetProductString(handle_read, buffer, 126)) productName = Marshal.PtrToStringAuto(buffer);
            if (HidD_GetSerialNumberString(handle_read, buffer, 126)) SN = Marshal.PtrToStringAuto(buffer);
            if (HidD_GetManufacturerString(handle_read, buffer, 126)) manfString = Marshal.PtrToStringAuto(buffer);
            Marshal.FreeHGlobal(buffer);

            //Call freePreParsedData to release some stuff
            HidD_FreePreparsedData(ref ptrToPreParsedData);
            //SetupDiDestroyDeviceInfoList(i);

            if (handle_read.IsInvalid)
                return;

            deviceConnected = true;

            //If connection was successful, record the values in a global struct
            productInfo = new InterfaceDetails();
            productInfo.devicePath = devicePath;
            productInfo.manufacturer = manfString ?? string.Empty;
            productInfo.product = productName ?? string.Empty;

            if (string.IsNullOrEmpty(SN))
            {
                SN = "0";
            }
            string[] tokensSN = SN.Split('-');

            if (tokensSN.Length > 1)
            {
                SN = tokensSN[1];// take the second part of the string separated by "-" / the actual SN 
            }

            productInfo.serialNumber = Convert.ToInt32(SN); // Yossi - removed - not working...
            productInfo.PID = (ushort)attributes.ProductID;
            productInfo.VID = (ushort)attributes.VendorID;
            productInfo.versionNumber = (ushort)attributes.VersionNumber;
            productInfo.IN_reportByteLength = capabilities.InputReportByteLength;
            productInfo.OUT_reportByteLength = capabilities.OutputReportByteLength;

            //use a filestream object to bring this stuff into .NET
            FS_read = new FileStream(handle_read, FileAccess.ReadWrite, capabilities.OutputReportByteLength, false);
            FS_write = new FileStream(handle_write, FileAccess.ReadWrite, capabilities.InputReportByteLength, false);

            this.useAsyncReads = useAsyncReads;
            if (useAsyncReads)
                ReadAsync();
        }

        public void Close()
        {
            FS_read?.Close();
            FS_write?.Close();

            if (handle_read != null && !handle_read.IsInvalid)
                handle_read.Close();
            if (handle_write != null && !handle_write.IsInvalid)
                handle_write.Close();

            deviceConnected = false;
        }

        public void Write(byte[] data)
        {
            if (FS_write is null)
                throw new Exception($"Filestream is unable to write: {nameof(FS_write)} is null");

            if (data.Length > capabilities.OutputReportByteLength)
                throw new Exception("Output report must not exceed " + (capabilities.OutputReportByteLength - 1).ToString() + " bytes");

            //uint numBytesWritten = 0;
            byte[] packet = new byte[capabilities.OutputReportByteLength];
            Array.Copy(data, 0, packet, 1, data.Length);            //start at 1, as the first byte must be zero for HID report
            packet[0] = 0;

            if (FS_write.CanWrite)
            {
                FS_write.Write(packet, 0, packet.Length);
            }
            else
            {
                throw new Exception("Filestream unable to write");
            }
        }

        public byte[] Read()
        {
            // This read function is for normal synchronous reads
            if (FS_read is null)
                throw new Exception($"Device is unable to read: {nameof(FS_read)} is null");

            if (useAsyncReads == true)
                throw new Exception("A synchronous read cannot be executed when operating in async mode");

            //Call readFile
            byte[] readBuf = new byte[capabilities.InputReportByteLength];

            FS_read.Read(readBuf, 0, readBuf.Length);
            return readBuf;
        }

        public void ReadAsync()
        {
            // This read function will be used with asynchronous operation, called by the constructor if async reads are used

            if (FS_read is null)
                throw new Exception($"Device is unable to read: {nameof(FS_read)} is null");

            readData = new byte[capabilities.InputReportByteLength];
            if (FS_read.CanRead)
                FS_read.BeginRead(readData, 0, readData.Length, new AsyncCallback(GetInputReportData), readData);
            else
                throw new Exception("Device is unable to read");
        }

        private void GetInputReportData(IAsyncResult ar)
        {
            if(FS_read is null)
                throw new Exception($"Device is unable to read: {nameof(FS_read)} is null");

            if (readData is null)
                throw new Exception($"Device is unable to read: {nameof(readData)} is null");

            FS_read.EndRead(ar); //must call an endread before starting another one
            //TODO handle exception with PCB is reaet

            //Reset the read thread to read the next report
            if (FS_read.CanRead)
                FS_read.BeginRead(readData, 0, readData.Length, new AsyncCallback(GetInputReportData), readData);
            else
                throw new Exception("Device is unable to read");

            dataReceived?.Invoke(readData);                                     //triggers the event to be heard by the calling class
        }

        #endregion

        #region utilities

        private static bool StringIsInteger(string? val)
        {
            return double.TryParse(val, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.CurrentCulture, out double result);
        }

        private string NumToHexString(ushort num)
        {
            return $"{num:X}";
        }

        #endregion

        public void Dispose()
        {
            Close();
        }
    }
}
