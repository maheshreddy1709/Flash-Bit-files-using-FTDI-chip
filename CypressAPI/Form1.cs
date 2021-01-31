using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using CyUSB;
using FT_Handle = System.UInt32;

namespace CypressAPI
{
    delegate void BootProgrammerDeviceFound();
    delegate void BootProgrammerDeviceNotFound();
    delegate void DownloadUserImg(CyFX3Device Fx3, FX3_FWDWNLOAD_MEDIA_TYPE enmMediaType);
    public partial class Form1 : Form
    {
        int Sync_Form_Resize = 0;
        public CypressFx3 cyFx3;
        CyUSBEndPoint curEndpt;
        string dataCaption;
        CyUSBDevice FxDev;
        CyFX3Device Fx3Dev;
        USBDeviceList cyFx3List;
        static ushort startAddress;
        static byte devAddress;
        static byte[] flashProgrBuffer = new byte[256 * 1024];
        static bool previousState = false;
        public string m_strFilename = "";


        //declare for Micron flash  command
        const byte WRITE_STATUS_CMD = 0x01;
        const byte WRITE_CMD = 0x02;  // Page program
        const byte READ_CMD = 0x03;
        const byte WRITE_DISABLE_CMD = 0x04;
        const byte READ_STATUS_CMD = 0x05;
        const byte WRITE_ENABLE_CMD = 0x06;
        const byte FAST_READ_CMD = 0x0B;
        const byte CHIP_ERASE_CMD = 0xC7;
        const byte SECTOR_ERASE_CMD = 0xD8;
        const byte READ_ID = 0x9F;
        const byte Release_Deep_POWER_Down = 0xAB;

        /////////////////////////////////////////////
        const byte RD_ID_SIZE = 4;/* Read ID command + 3 bytes ID response */
        const byte DUMMY_SIZE = 1;/* Number of dummy bytes for fast read */
        const byte DATA_OFFSET = 4;/* Start of Data for Read/Write */
        const byte COMMAND_OFFSET = 0; /* Flash instruction */
        const byte ADDRESS_1_OFFSET = 1; /* MSB byte of address to read or write */
        const byte ADDRESS_2_OFFSET = 2; /* Middle byte of address to read or write */
        const byte ADDRESS_3_OFFSET = 3; /* LSB byte of address to read or write */
        static int TestAddress;


        /*
         * The following constants specify the extra bytes which are sent to the
         * flash on the SPI interface, that are not data, but control information
         * which includes the command and address
         */
        const byte OVERHEAD_SIZE = 4;

        /*
         * The following constants specify the max amount of data and the size of the
         * the buffer required to hold the data and overhead to transfer the data to
         * and from the flash.
         */
        const UInt32 MAX_DATA = 1024 * 1024;

        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// /////////////////////////////////////////////////////////////////
        /// </summary>
        byte[] WriteBuffer = new byte[256];
        //byte[] ReadBuffer = new byte[MAX_DATA + DATA_OFFSET + DUMMY_SIZE];
        FT_STATUS ftStatus;
        /// It will use the specified configOptions. 
        /// SPI clock phase, clock polarity 
        /// FTDI release notes reference 
        ///

        public enum FT_STATUS
        {
            FT_OK,
            FT_INVALID_HANDLE,
            FT_DEVICE_NOT_FOUND,
            FT_DEVICE_NOT_OPENED,
            FT_IO_ERROR,
            FT_INSUFFICIENT_RESOURCES,
            FT_INVALID_PARAMETER,
            FT_INVALID_BAUD_RATE,

            FT_DEVICE_NOT_OPENED_FOR_ERASE,
            FT_DEVICE_NOT_OPENED_FOR_WRITE,
            FT_FAILED_TO_WRITE_DEVICE,
            FT_EEPROM_READ_FAILED,
            FT_EEPROM_WRITE_FAILED,
            FT_EEPROM_ERASE_FAILED,
            FT_EEPROM_NOT_PRESENT,
            FT_EEPROM_NOT_PROGRAMMED,
            FT_INVALID_ARGS,
            FT_NOT_SUPPORTED,
            FT_OTHER_ERROR,
            FT_DEVICE_LIST_NOT_READY,

        }


        public enum SPI_CONFIG_OPTION_MODE
        {
            MODE0 = 0x00000000,
            MODE1 = 0x00000001,
            MODE2 = 0x00000002,
            MODE3 = 0x00000003
        }
        ///

        /// It will use the specified configOptions. 
        /// Specify the chip select pin. 
        ///

        public enum SPI_CONFIG_OPTION_CS
        {
            DBUS3 = 0x00000000, /*000 00*/
            DBUS4 = 0x00000004, /*001 00*/
            DBUS5 = 0x00000008, /*010 00*/
            DBUS6 = 0x0000000C, /*011 00*/
            DBUS7 = 0x00000010, /*100 00*/
        }
        ///

        /// It will use the specified configOptions. 
        /// Chip select is active high or low. 
        ///

        public enum SPI_CONFIG_OPTION_CS_ACTIVE
        {
            HIGH = 0x00000000,
            LOW = 0x00000020,
        }     ///


        /// Structure set of SPI 
        ///

        [StructLayout(LayoutKind.Sequential)]
        public struct SPIChannelConfig
        {
            ///

            /// To use clock rate 0-30,000,000Hz 
            ///

            public UInt32 ClockRate;
            ///

            /// Latency timer 0-255 (mS) can be specified up to. 
            /// In FT232H until 1-255 it is recommended. 
            ///

            public Byte LatencyTimer;
            ///

            /// SPI mode configuration options 
            ///

            public UInt32 ConfigOptions;
            ///

            /// BIT7-0: Direction of thelines after SPI_InitChannel IS called 
            /// BIT16-8: Value of thelines after SPI_InitChannel IS called 
            /// BIT23-16: Direction of thelines after SPI_CloseChannel IS called 
            /// BIT32-24: Direction of thelines after SPI_InitChannel IS called 
            ///

            public UInt32 Pins;
            ///

            /// Reserved, settings, such as the prohibition. 
            ///

        }   ///


        /// Structure to manage the device information of FT232HL 
        ///

        [StructLayout(LayoutKind.Sequential)]
        public struct FT_DEVICE_LIST_INFO_NODE
        {
            public UInt32 Flags;
            public UInt32 Type;
            public UInt32 ID;
            public UInt32 LOCID;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public char[] SerialNumber;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public char[] Description;
            public IntPtr ftHandle;
        }


        /// Initialization function for using the libmpsse.dll. Always it will first call. 
        ///

        [DllImport("LibMPSSE.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Init_libMPSSE();         ///


        /// The release the resources used by MPSSE library. Certainly it will call at the end. 
        ///

        [DllImport("LibMPSSE.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Cleanup_libMPSSE();         ///


        /// To change the polarity of the specified pin. (ACBUS only possible) 
        ///

        /// FT232H handle 
        /// a bit corresponding to the port you want to output port 1, if you want to input port to 0. 
        /// I to output a Low in High, 0 in the 1 to the corresponding bit. 
        ///FT232H status
        [DllImport("LibMPSSE.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT_STATUS FT_WriteGPIO(FT_Handle ftHandle, byte dir, byte value);         ///


        /// Get the number of connections FT232H. 
        ///

        /// number of connections that have been FT232HUSB interface 
        ///FT232H status
        [DllImport("LibMPSSE.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT_STATUS SPI_GetNumChannels(ref UInt32 numChannels);         ///


        /// Open the SPI. 
        ///

        /// specify the FT232H device to SPI. required index number if multiple FT232H are connected. Usually 0 
        /// FT232H handle 
        ///FT232H status
        [DllImport("LibMPSSE.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT_STATUS SPI_OpenChannel(UInt32 index, ref FT_Handle ftHandle);         ///


        /// It does end processing of SPI. 
        ///

        /// FT232H handle 
        ///FT232H status
        [DllImport("LibMPSSE.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT_STATUS SPI_CloseChannel(FT_Handle ftHandle);         ///


        /// It does initialization of FT232H as SPI master. 
        ///

        /// FT232H handle 
        /// SPI configuration 
        ///FT232H status
        [DllImport("LibMPSSE.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT_STATUS SPI_InitChannel(FT_Handle ftHandle, ref SPIChannelConfig config);         ///


        /// Get information about the specified FT232H. 
        ///

        /// index number of FT232H want to get the information 
        /// acquired FT232H output destination device information 
        ///FT232H status
        [DllImport("LibMPSSE.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT_STATUS SPI_GetChannelInfo(UInt32 index, ref FT_DEVICE_LIST_INFO_NODE ChanInfo);         ///


        /// Read the value of the input port. 
        /// Must be set to the input port using the dir argument of FT_WriteGPIO before you use this method. (ACBUS possible only). 
        ///

        /// FT232H handle 
        /// output destination of the obtained port state 
        ///FT232H status
        [DllImport("LibMPSSE.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT_STATUS FT_ReadGPIO(FT_Handle ftHandle, ref byte value);         ///


        /// I do SPI master transmission as an SPI master. 
        ///

        /// The FT232H handle 
        /// send buffer 
        /// SPI master address 
        /// send you want bytes 
        /// actual number of bytes could be sent 
        /// send options 
        ///FT232H status
        [DllImport("LibMPSSE.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT_STATUS SPI_Write(FT_Handle ftHandle, ref byte buffer, UInt32 SizeToTransfer, ref UInt32 SizeTransferred, UInt32 TransferOptions);         ///


        /// I do SPI master received as SPI master. 
        ///

        /// FT232H handle 
        /// receive buffer 
        /// received bytes 
        /// actual number of bytes can be received in 
        /// receive option 
        ///FT232H status
        [DllImport("LibMPSSE.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT_STATUS SPI_Read(FT_Handle ftHandle, ref byte buffer, UInt32 SizeToTransfer, ref UInt32 SizeTransferred, UInt32 TransferOptions);         ///

        [DllImport("LibMPSSE.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT_STATUS SPI_ReadWrite(FT_Handle ftHandle, ref byte inbuffer, ref byte outbuffer, UInt32 SizeToTransfer, ref UInt32 SizeTransferred, UInt32 TransferOptions);
        /// Examine the state of the MISO line of SPI slave. (To ascertain whether SPI slave is in the state capable of performing the following operations.) 
        ///

        /// FT232H handle 
        /// storage destination of the state of the MISO line. 
        ///FT232H status
        [DllImport("LibMPSSE.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT_STATUS SPI_IsBusy(FT_Handle ftHandle, ref bool State);         ///


        /// Operate the chip select line of the SPI slave communicating. 
        ///

        /// FT232H handle 
        ///         ///

        [DllImport("LibMPSSE.Dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FT_STATUS SPI_ChangeCS(FT_Handle ftHandle, UInt32 ConfigOptions);

        /// //////////////////////////////////////////////////////////////////////
        public Form1()
        {
            InitializeComponent();
            cyFx3 = new CypressFx3();
        }

        public bool InitFX3()
        {
            bool retValue = false;
            try
            {
                cyFx3 = new CypressFx3();

                USBDeviceList list = cyFx3.GetUsbDeviceListObject();

                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].DriverName.ToUpper().Contains("CYUSB"))
                    {
                        retValue = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                retValue = false;
            }

            if (retValue == false)
                MessageBox.Show("Init Error");
            return retValue;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Sync_Form_Resize = 1;
            Form1_Resize(this, null);
        }

        /* Summary
        Event handler for the TextToSend
        */
        private void TextToSend_KeyUp(object sender, KeyEventArgs e)
        {
            string txt = TexToSendBox.Text;
            StringBuilder hexData = new StringBuilder();

            foreach (char c in txt)
            {
                hexData.Append(string.Format("{0:X2} ", Convert.ToByte(c)));
            }

            DataToSendBox.Text = hexData.ToString();
            NumBytesBox.Text = TexToSendBox.Text.Length.ToString();

        }
        /* Summary
        Event handler for the DataToSend
        */
        private void DataToSend_KeyUp(object sender, KeyEventArgs e)
        {
            string hexData = DataToSendBox.Text.Trim();
            string[] hexTokens = hexData.Split(' ');

            StringBuilder data = new StringBuilder();
            StringBuilder txt = new StringBuilder();

            int nToks = 0;

            foreach (string tok in hexTokens)
            {
                if (String.IsNullOrEmpty(tok))
                    continue;

                nToks++;
                try
                {
                    int n = Convert.ToInt32(tok, 16);
                    txt.Append(Convert.ToChar(n));
                    data.Append(tok);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, "Input Error");
                    return;
                }
            }

            DataToSendBox.Text = data.ToString();
            DataToSendBox.Text = txt.ToString();
            DataToSendBox.Text = nToks.ToString();

        }


        /* Summary
            To resize the Data Transfers TabPage in case Control endpoint is selected
        */
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (Sync_Form_Resize == 1)
            {
                Sync_Form_Resize = 0;
            }
        }

        private void DataTransfer_click(object sender, EventArgs e)
        {
            int numberOfBytesToRead = 0;
            byte[] dataBuffer;
            byte[] readDataBuffer;
            bool operationStatus = false;

            int bytes = 0;
            try
            {

                if (!NumBytesBox.Text.Contains("-"))
                {
                    if (NumBytesBox.Text.Contains("0X") || NumBytesBox.Text.Contains("0x"))
                        bytes = Convert.ToInt32(NumBytesBox.Text, 16);
                    else
                        bytes = Convert.ToInt32(NumBytesBox.Text);
                }
                else
                {
                    MessageBox.Show("Enter a valid number of bytes to transfer.", "Invalid Bytes to tranfer");
                    return;
                }
                if (RegTypeCBox.Text.Equals("Read FW ID"))
                {
                    DeviceAddr.Text = "0x0000";
                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                    numberOfBytesToRead = (int)Convert.ToInt32(NumBytesBox.Text);
                    dataBuffer = new byte[numberOfBytesToRead];
                    byte reqCode = 0xB0;
                    toolStripStatusLabel1.Text = "Reading ID From Flash";
                    //Thread.Sleep(3000);
                    operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, numberOfBytesToRead, ref dataBuffer);
                    //Sent Data to Control End Point need to display it now
                    if (operationStatus)
                    {
                        DisplayXferData(dataBuffer, bytes, operationStatus);
                        toolStripStatusLabel1.Text = "Reading FW ID Completed";
                    }
                    else
                        toolStripStatusLabel1.Text = "Reading FW ID Failed";
                }

                if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals(""))
                {
                    DeviceAddr.Text = "0x0000";
                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                    toolStripStatusLabel1.Text = "Write in Progress";
                    dataBuffer = new byte[bytesCount + 1];
                    byte reqCode = 0xC2;

                    //Check for the start address if more than 5 bit then it should be error
                    if (startAddress > 0x1F)
                    {
                        MessageBox.Show("Invalid start Address");
                        return;
                    }

                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                    int i = 0;

                    //foreach (string tok in hexTokens)
                    for (int j = 0; j < hexTokens.Length; j++)
                    {
                        string tok;

                        try
                        {
                            tok = hexTokens[j];
                        }
                        catch
                        {
                            tok = "";
                        }

                        if ((tok.Length > 0) && (bytes > j))
                        {
                            try
                            {
                                dataBuffer[++i] = (byte)Convert.ToInt32(tok, 16);
                            }
                            catch (Exception exc)
                            {
                                MessageBox.Show(exc.Message, "Input Error");
                                return;
                            }
                        }
                    }
                    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
                    //Sent Data to Control End Point need to display it now
                    if (operationStatus)
                        DisplayXferData(dataBuffer, bytes, operationStatus);
                    else
                        toolStripStatusLabel1.Text = "Write Failed";
                }
                else if (InOutBox.Text.Equals("Read") && RegTypeCBox.Text.Equals(""))
                {
                    DeviceAddr.Text = "0x0000";
                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                    numberOfBytesToRead = (int)Convert.ToInt32(NumBytesBox.Text);
                    dataBuffer = new byte[numberOfBytesToRead + 1];
                    readDataBuffer = new byte[numberOfBytesToRead + 1];
                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                    toolStripStatusLabel1.Text = "Reading Data From Eeprom";
                    byte reqCode = 0xC3;
                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                    int i = 0;
                    //Check for start address, should not be greater than 1F
                    //Read actual data
                    operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, numberOfBytesToRead, ref readDataBuffer);
                    //Sent Data to Control End Point need to display it now
                    if (operationStatus)
                    {
                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                        toolStripStatusLabel1.Text = "Reading Data Completed";
                    }
                    else
                        toolStripStatusLabel1.Text = "Reading Data Failed";
                }
                //else if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals("Write PD Comand"))
                //{
                //    DeviceAddr.Text = "0x0000";
                //    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //    int bytesCount = 1; //Convert.ToInt32(NumBytesBox.Text);
                //    toolStripStatusLabel1.Text = "Write in Progress";
                //    dataBuffer = new byte[bytesCount];
                //    byte reqCode = 0xC4;
                //    //Check for the start address if more than 5 bit then it should be error
                //    if (startAddress > 0x1F)
                //    {
                //        MessageBox.Show("Invalid start Address");
                //        return;
                //    }
                //    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //    int i = 0;

                //    //foreach (string tok in hexTokens)
                //    for (int j = 0; j < hexTokens.Length; j++)
                //    {
                //        string tok;

                //        try
                //        {
                //            tok = hexTokens[j];
                //        }
                //        catch
                //        {
                //            tok = "";
                //        }

                //        if ((tok.Length > 0) && (bytes > j))
                //        {
                //            try
                //            {
                //                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                //            }
                //            catch (Exception exc)
                //            {
                //                MessageBox.Show(exc.Message, "Input Error");
                //                return;
                //            }
                //        }
                //    }
                //    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                //    operationStatus = cyFx3.WriteSPI(reqCode,devAddress, startAddress, dataBuffer, bytesCount);
                //    //Sent Data to Control End Point need to display it now
                //    if (operationStatus)
                //        DisplayXferData(dataBuffer, bytes, operationStatus);
                //    else
                //        toolStripStatusLabel1.Text = "Write Failed";
                //}
                //else if (InOutBox.Text.Equals("Read") && RegTypeCBox.Text.Equals("Read PD Comand"))
                //{
                //    DeviceAddr.Text = "0x0000";
                //    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //    numberOfBytesToRead = 1;
                //    dataBuffer = new byte[numberOfBytesToRead];
                //    readDataBuffer = new byte[numberOfBytesToRead];
                //    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //    toolStripStatusLabel1.Text = "Reading Data From Eeprom";
                //    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //    byte reqCode = 0xC5;
                //    int i = 0;
                //     //Read actual data
                //    operationStatus = cyFx3.ReadSPI(reqCode,devAddress, startAddress, numberOfBytesToRead, out readDataBuffer);
                //    //Sent Data to Control End Point need to display it now
                //    if (operationStatus)
                //    {
                //        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //        toolStripStatusLabel1.Text = "Reading Data Completed";
                //    }
                //    else
                //        toolStripStatusLabel1.Text = "Reading Data Failed";
                //}
                //else if (InOutBox.Text.Equals("Read") && RegTypeCBox.Text.Equals("Read Transfer Result"))
                //{
                //    DeviceAddr.Text = "0x0000";
                //    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //    numberOfBytesToRead = 1;
                //    dataBuffer = new byte[numberOfBytesToRead];
                //    readDataBuffer = new byte[numberOfBytesToRead];
                //    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //    toolStripStatusLabel1.Text = "Reading Data From Eeprom";
                //    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //    byte reqCode = 0xC6;
                //    int i = 0;
                //    //Read actual data
                //    operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, numberOfBytesToRead, out readDataBuffer);
                //    //Sent Data to Control End Point need to display it now
                //    if (operationStatus)
                //    {
                //        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //        toolStripStatusLabel1.Text = "Reading Data Completed";
                //    }
                //    else
                //        toolStripStatusLabel1.Text = "Reading Data Failed";
                //}
                //else if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals("Clear IRQ"))
                //{
                //    DeviceAddr.Text = "0x0000";
                //    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //    numberOfBytesToRead = 1;
                //    dataBuffer = new byte[numberOfBytesToRead];
                //    readDataBuffer = new byte[numberOfBytesToRead];
                //    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //    toolStripStatusLabel1.Text = "Write in Progress";
                //    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //    byte reqCode = 0xC7;
                //    if (startAddress > 0x1F)
                //    {
                //        MessageBox.Show("Invalid start Address");
                //        return;
                //    }
                //    int i = 0;

                //    //foreach (string tok in hexTokens)
                //    for (int j = 0; j < hexTokens.Length; j++)
                //    {
                //        string tok;

                //        try
                //        {
                //            tok = hexTokens[j];
                //        }
                //        catch
                //        {
                //            tok = "";
                //        }

                //        if ((tok.Length > 0) && (bytes > j))
                //        {
                //            try
                //            {
                //                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                //            }
                //            catch (Exception exc)
                //            {
                //                MessageBox.Show(exc.Message, "Input Error");
                //                return;
                //            }
                //        }
                //    }
                //    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                //    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
                //    //Sent Data to Control End Point need to display it now
                //    if (operationStatus)
                //    {
                //        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //        toolStripStatusLabel1.Text = "Clearing IRQ Reg. Completed";
                //    }
                //    else
                //        toolStripStatusLabel1.Text = "Clearing IRQ Reg. Failed";
                //}
                //else if (InOutBox.Text.Equals("Read") && RegTypeCBox.Text.Equals("Read IRQ"))
                //{
                //    DeviceAddr.Text = "0x0000";
                //    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //    numberOfBytesToRead = 1;
                //    dataBuffer = new byte[numberOfBytesToRead];
                //    readDataBuffer = new byte[numberOfBytesToRead];
                //    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //    toolStripStatusLabel1.Text = "Reading Data From Eeprom";
                //    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //    byte reqCode = 0xC8;
                //    //Read actual data
                //    operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, numberOfBytesToRead, out readDataBuffer);
                //    //Sent Data to Control End Point need to display it now
                //    if (operationStatus)
                //    {
                //        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //        toolStripStatusLabel1.Text = "Reading Data Completed";
                //    }
                //    else
                //        toolStripStatusLabel1.Text = "Reading Data Failed";
                //}
                //else if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals("Write Interrupt Enable"))
                //{
                //    DeviceAddr.Text = "0x0000";
                //    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //    numberOfBytesToRead = 1;
                //    dataBuffer = new byte[numberOfBytesToRead];
                //    readDataBuffer = new byte[numberOfBytesToRead];
                //    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //    toolStripStatusLabel1.Text = "Write in Progress";
                //    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //    byte reqCode = 0xC9;
                //    int i = 0;

                //    //foreach (string tok in hexTokens)
                //    for (int j = 0; j < hexTokens.Length; j++)
                //    {
                //        string tok;

                //        try
                //        {
                //            tok = hexTokens[j];
                //        }
                //        catch
                //        {
                //            tok = "";
                //        }

                //        if ((tok.Length > 0) && (bytes > j))
                //        {
                //            try
                //            {
                //                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                //            }
                //            catch (Exception exc)
                //            {
                //                MessageBox.Show(exc.Message, "Input Error");
                //                return;
                //            }
                //        }
                //    }
                //    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                //    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
                //    //Sent Data to Control End Point need to display it now
                //    if (operationStatus)
                //    {
                //        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //        toolStripStatusLabel1.Text = "Write Completed";
                //    }
                //    else
                //        toolStripStatusLabel1.Text = "Write Failed";
                //}
                //else if (InOutBox.Text.Equals("Read") && RegTypeCBox.Text.Equals("Read Interrupt Enable"))
                //{
                //    DeviceAddr.Text = "0x0000";
                //    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //    numberOfBytesToRead = 1;
                //    dataBuffer = new byte[numberOfBytesToRead];
                //    readDataBuffer = new byte[numberOfBytesToRead];
                //    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //    toolStripStatusLabel1.Text = "Reading Data From Eeprom";
                //    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //    byte reqCode = 0xCA;
                //    //Read actual data
                //    operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, numberOfBytesToRead, out readDataBuffer);
                //    //Sent Data to Control End Point need to display it now
                //    if (operationStatus)
                //    {
                //        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //        toolStripStatusLabel1.Text = "Reading Data Completed";
                //    }
                //    else
                //        toolStripStatusLabel1.Text = "Reading Data Failed";
                //}
                //Write Register Data
                //else if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals("Write Baud Rate"))
                //{
                //    DeviceAddr.Text = "0x0000";
                //    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //    numberOfBytesToRead = 1;
                //    dataBuffer = new byte[numberOfBytesToRead];
                //    readDataBuffer = new byte[numberOfBytesToRead];
                //    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //    toolStripStatusLabel1.Text = "Write in Progress";
                //    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //    byte reqCode = 0xBA;
                //    int i = 0;

                //    //foreach (string tok in hexTokens)
                //    for (int j = 0; j < hexTokens.Length; j++)
                //    {
                //        string tok;

                //        try
                //        {
                //            tok = hexTokens[j];
                //        }
                //        catch
                //        {
                //            tok = "";
                //        }

                //        if ((tok.Length > 0) && (bytes > j))
                //        {
                //            try
                //            {
                //                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                //            }
                //            catch (Exception exc)
                //            {
                //                MessageBox.Show(exc.Message, "Input Error");
                //                return;
                //            }
                //        }
                //    }
                //    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                //    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
                //    //Sent Data to Control End Point need to display it now
                //    if (operationStatus)
                //    {
                //        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //        toolStripStatusLabel1.Text = "Write Completed";
                //    }
                //    else
                //        toolStripStatusLabel1.Text = "Write Failed";
                //}

                else if (RegTypeCBox.Text.Equals("Write Phy Register"))
                {
                    DeviceAddr.Text = "0x0000";
                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                    numberOfBytesToRead = 1;
                    dataBuffer = new byte[numberOfBytesToRead];
                    readDataBuffer = new byte[numberOfBytesToRead];
                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                    toolStripStatusLabel1.Text = "Write in Progress";
                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                    byte reqCode = 0xFB;
                    int i = 0;


                    if (RegBox.Text.Equals("PDCMD"))
                    {
                        devAddress = 0x00;
                    }
                    else if (RegBox.Text.Equals("XFRST"))
                    {
                        devAddress = 0x01;
                    }
                    else if (RegBox.Text.Equals("ClearIRQ"))
                    {
                        devAddress = 0x02;
                    }
                    else if (RegBox.Text.Equals("Interrupt Enable"))
                    {
                        devAddress = 0x03;
                    }
                    else if (RegBox.Text.Equals("Control 0"))
                    {
                        devAddress = 0x04;
                    }
                    else if (RegBox.Text.Equals("Control 1"))
                    {
                        devAddress = 0x05;
                    }
                    else if (RegBox.Text.Equals("RXERR"))
                    {
                        devAddress = 0x06;
                    }
                    else if (RegBox.Text.Equals("BEC0"))
                    {
                        devAddress = 0x07;
                    }
                    else if (RegBox.Text.Equals("BEC1"))
                    {
                        devAddress = 0x08;
                    }
                    else if (RegBox.Text.Equals("PD Status"))
                    {
                        devAddress = 0x0C;
                    }
                    else if (RegBox.Text.Equals("Rp Sel Ctl 2"))
                    {
                        devAddress = 0x0D;
                    }
                    else if (RegBox.Text.Equals("Init Role"))
                    {
                        devAddress = 0x0E;
                    }
                    else if (RegBox.Text.Equals("Control 3"))
                    {
                        devAddress = 0x0F;
                    }
                    else if (RegBox.Text.Equals("Timer"))
                    {
                        devAddress = 0x16;
                    }
                    else if (RegBox.Text.Equals("BaudRate"))
                    {
                        devAddress = 0x17;
                    }
                    else if (RegBox.Text.Equals("Error Data"))
                    {
                        devAddress = 0x18;
                    }
                    else if (RegBox.Text.Equals("Error Position"))
                    {
                        devAddress = 0x19;
                    }
                    else if (RegBox.Text.Equals("Misc"))
                    {
                        devAddress = 0x1A;
                    }
                    else if (RegBox.Text.Equals("Control 4"))
                    {
                        devAddress = 0x1B;
                    }
                    else if (RegBox.Text.Equals("MSG ID"))
                    {
                        devAddress = 0x1C;
                    }
                    else if (RegBox.Text.Equals("Delay Count LSB"))
                    {
                        devAddress = 0x1E;
                    }
                    else if (RegBox.Text.Equals("Delay Count MSB"))
                    {
                        devAddress = 0x1D;
                    }
                    else if (RegBox.Text.Equals("Delay Control"))
                    {
                        devAddress = 0x1F;
                    }
                    else if (RegBox.Text.Equals("SOP1 MSG ID"))
                    {
                        devAddress = 0x12;
                    }
                    else if (RegBox.Text.Equals("SOP2 MSG ID"))
                    {
                        devAddress = 0x13;
                    }
                    else if (RegBox.Text.Equals("SOP1D MSG ID"))
                    {
                        devAddress = 0x14;
                    }
                    else if (RegBox.Text.Equals("SOP12 MSG ID"))
                    {
                        devAddress = 0x15;
                    }
                    else if (RegBox.Text.Equals("Control 7"))
                    {
                        devAddress = 0x2B;
                    }

                    //foreach (string tok in hexTokens)
                    for (int j = 0; j < hexTokens.Length; j++)
                    {
                        string tok;

                        try
                        {
                            tok = hexTokens[j];
                        }
                        catch
                        {
                            tok = "";
                        }

                        if ((tok.Length > 0) && (bytes > j))
                        {
                            try
                            {
                                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                            }
                            catch (Exception exc)
                            {
                                MessageBox.Show(exc.Message, "Input Error");
                                return;
                            }
                        }
                    }
                    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount, true);
                    //Sent Data to Control End Point need to display it now
                    if (operationStatus)
                    {
                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                        toolStripStatusLabel1.Text = "Write Completed";
                    }
                    else
                        toolStripStatusLabel1.Text = "Write Failed";
                }
                //Reading Phy Register
                else if (RegTypeCBox.Text.Equals("Read Phy Register"))
                {
                    //                    DeviceAddr.Text = "0x0000";
                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                    numberOfBytesToRead = 1;
                    dataBuffer = new byte[numberOfBytesToRead];
                    readDataBuffer = new byte[numberOfBytesToRead];
                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                    toolStripStatusLabel1.Text = "Reading Data From Eeprom";
                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                    byte reqCode = 0xFC;
                    int i = 0;
                    if (RegBox.Text.Equals("PDCMD"))
                    {
                        devAddress = 0x00;
                    }
                    else if (RegBox.Text.Equals("XFRST"))
                    {
                        devAddress = 0x01;
                    }
                    else if (RegBox.Text.Equals("ClearIRQ"))
                    {
                        devAddress = 0x02;
                    }
                    else if (RegBox.Text.Equals("Interrupt Enable"))
                    {
                        devAddress = 0x03;
                    }
                    else if (RegBox.Text.Equals("Control 0"))
                    {
                        devAddress = 0x04;
                    }
                    else if (RegBox.Text.Equals("Control 1"))
                    {
                        devAddress = 0x05;
                    }
                    else if (RegBox.Text.Equals("RXERR"))
                    {
                        devAddress = 0x06;
                    }
                    else if (RegBox.Text.Equals("BEC0"))
                    {
                        devAddress = 0x07;
                    }
                    else if (RegBox.Text.Equals("BEC1"))
                    {
                        devAddress = 0x08;
                    }
                    else if (RegBox.Text.Equals("PD Status"))
                    {
                        devAddress = 0x0C;
                    }
                    else if (RegBox.Text.Equals("Rp Sel Ctl 2"))
                    {
                        devAddress = 0x0D;
                    }
                    else if (RegBox.Text.Equals("Init Role"))
                    {
                        devAddress = 0x0E;
                    }
                    else if (RegBox.Text.Equals("Control 3"))
                    {
                        devAddress = 0x0F;
                    }
                    else if (RegBox.Text.Equals("Timer"))
                    {
                        devAddress = 0x16;
                    }
                    else if (RegBox.Text.Equals("BaudRate"))
                    {
                        devAddress = 0x17;
                    }
                    else if (RegBox.Text.Equals("Error Data"))
                    {
                        devAddress = 0x18;
                    }
                    else if (RegBox.Text.Equals("Error Position"))
                    {
                        devAddress = 0x19;
                    }
                    else if (RegBox.Text.Equals("Misc"))
                    {
                        devAddress = 0x1A;
                    }
                    else if (RegBox.Text.Equals("Control 4"))
                    {
                        devAddress = 0x1B;
                    }
                    else if (RegBox.Text.Equals("MSG ID"))
                    {
                        devAddress = 0x1C;
                    }
                    else if (RegBox.Text.Equals("Delay Count LSB"))
                    {
                        devAddress = 0x1E;
                    }
                    else if (RegBox.Text.Equals("Delay Count MSB"))
                    {
                        devAddress = 0x1D;
                    }
                    else if (RegBox.Text.Equals("Delay Control"))
                    {
                        devAddress = 0x1F;
                    }
                    else if (RegBox.Text.Equals("SOP1 MSG ID"))
                    {
                        devAddress = 0x12;
                    }
                    else if (RegBox.Text.Equals("SOP2 MSG ID"))
                    {
                        devAddress = 0x13;
                    }
                    else if (RegBox.Text.Equals("SOP1D MSG ID"))
                    {
                        devAddress = 0x14;
                    }
                    else if (RegBox.Text.Equals("SOP2D MSG ID"))
                    {
                        devAddress = 0x15;
                    }
                    else if (RegBox.Text.Equals("Control 7"))
                    {
                        devAddress = 0x2B;
                    }
                    //Read actual data
                    operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, numberOfBytesToRead, ref readDataBuffer, true);
                    //Sent Data to Control End Point need to display it now
                    if (operationStatus)
                    {
                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                        toolStripStatusLabel1.Text = "Reading Data Completed";
                    }
                    else
                        toolStripStatusLabel1.Text = "Reading Data Failed";
                }


//                else if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals("Write Control 0"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 1;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //                    toolStripStatusLabel1.Text = "Write in Progress";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xCB;
                //                    int i = 0;

//                    //foreach (string tok in hexTokens)
                //                    for (int j = 0; j < hexTokens.Length; j++)
                //                    {
                //                        string tok;

//                        try
                //                        {
                //                            tok = hexTokens[j];
                //                        }
                //                        catch
                //                        {
                //                            tok = "";
                //                        }

//                        if ((tok.Length > 0) && (bytes > j))
                //                        {
                //                            try
                //                            {
                //                                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                //                            }
                //                            catch (Exception exc)
                //                            {
                //                                MessageBox.Show(exc.Message, "Input Error");
                //                                return;
                //                            }
                //                        }
                //                    }
                //                    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                //                    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Write Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Write Failed";
                //                }
                //                else if (InOutBox.Text.Equals("Read") && RegTypeCBox.Text.Equals("Read Control 0"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 1;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //                    toolStripStatusLabel1.Text = "Reading Data From Eeprom";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xCC;
                //                    //Read actual data
                //                    operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, numberOfBytesToRead, out readDataBuffer);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Reading Data Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Reading Data Failed";
                //                }
                //                else if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals("Write Control 1"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 1;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    toolStripStatusLabel1.Text = "Write in Progress";
                //                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xCD;
                //                    int i = 0;

//                    //foreach (string tok in hexTokens)
                //                    for (int j = 0; j < hexTokens.Length; j++)
                //                    {
                //                        string tok;

//                        try
                //                        {
                //                            tok = hexTokens[j];
                //                        }
                //                        catch
                //                        {
                //                            tok = "";
                //                        }

//                        if ((tok.Length > 0) && (bytes > j))
                //                        {
                //                            try
                //                            {
                //                                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                //                            }
                //                            catch (Exception exc)
                //                            {
                //                                MessageBox.Show(exc.Message, "Input Error");
                //                                return;
                //                            }
                //                        }
                //                    }
                //                    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                //                    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Write Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Write Failed";
                //                }
                //                else if (InOutBox.Text.Equals("Read") && RegTypeCBox.Text.Equals("Read Control 1"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 1;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //                    toolStripStatusLabel1.Text = "Reading Data From Eeprom";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xCE;
                //                    //Read actual data
                //                    operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, numberOfBytesToRead, out readDataBuffer);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Reading Data Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Reading Data Failed";
                //                }
                //                else if (InOutBox.Text.Equals("Read") && RegTypeCBox.Text.Equals("Read Receive Error"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 1;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    toolStripStatusLabel1.Text = "Reading Data From Eeprom";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xCF;
                //                    //Read actual data
                //                    operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, numberOfBytesToRead, out readDataBuffer);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Reading Data Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Reading Data Failed";
                //                }
                //                else if (InOutBox.Text.Equals("Read") && RegTypeCBox.Text.Equals("Read BIST Error Control"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 2;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    toolStripStatusLabel1.Text = "Reading Data From Eeprom";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xD0;
                //                    //Read actual data
                //                    operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, numberOfBytesToRead, out readDataBuffer);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Reading Data Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Reading Data Failed";
                //                }
                //                else if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals("Write WBUF"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    int bytesCount = 32; //Convert.ToInt32(NumBytesBox.Text);
                //                    toolStripStatusLabel1.Text = "Write in Progress";
                //                    int noOfBytes = 0;
                //                    dataBuffer = new byte[bytesCount + 1];
                //                    byte reqCode = 0xD1;
                //                    //Check for the start address if more than 5 bit then it should be error
                //                    if (startAddress > 0x1F)
                //                    {
                //                        MessageBox.Show("Invalid start Address");
                //                        return;
                //                    }
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    int i = 0;

//                    //foreach (string tok in hexTokens)
                //                    for (int j = 0; j < hexTokens.Length; j++)
                //                    {
                //                        string tok;

//                        try
                //                        {
                //                            tok = hexTokens[j];
                //                        }
                //                        catch
                //                        {
                //                            tok = "";
                //                        }

//                        if ((tok.Length > 0))
                //                        {
                //                            try
                //                            {
                //                                dataBuffer[j] = (byte)Convert.ToInt32(tok, 16);
                //                            }
                //                            catch (Exception exc)
                //                            {
                //                                MessageBox.Show(exc.Message, "Input Error");
                //                                return;
                //                            }
                //                        }
                //                        noOfBytes++;
                //                    }
                //                    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                //                    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, noOfBytes);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(dataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Write Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Write Failed";
                //                }
                //                else if (InOutBox.Text.Equals("Read") && RegTypeCBox.Text.Equals("Read RBUF"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 32;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    toolStripStatusLabel1.Text = "Reading Data From Eeprom";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xD2;
                //                    //Read actual data
                //                    operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, numberOfBytesToRead, out readDataBuffer);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes + 1, operationStatus);
                //                        toolStripStatusLabel1.Text = "Reading Data Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Reading Data Failed";
                //                }
                //                else if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals("Write PD Status"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 1;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //                    toolStripStatusLabel1.Text = "Write in Progress";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xB4;
                //                    if (startAddress > 0x1F)
                //                    {
                //                        MessageBox.Show("Invalid start Address");
                //                        return;
                //                    }
                //                    int i = 0;

//                    //foreach (string tok in hexTokens)
                //                    for (int j = 0; j < hexTokens.Length; j++)
                //                    {
                //                        string tok;

//                        try
                //                        {
                //                            tok = hexTokens[j];
                //                        }
                //                        catch
                //                        {
                //                            tok = "";
                //                        }

//                        if ((tok.Length > 0) && (bytes > j))
                //                        {
                //                            try
                //                            {
                //                                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                //                            }
                //                            catch (Exception exc)
                //                            {
                //                                MessageBox.Show(exc.Message, "Input Error");
                //                                return;
                //                            }
                //                        }
                //                    }
                //                    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                //                    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Writing RP Sel Reg. Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Writing RP Sel. Failed";
                //                }
                //                else if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals("Write Ctrl2"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 1;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //                    toolStripStatusLabel1.Text = "Write in Progress";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xB6;
                //                    if (startAddress > 0x1F)
                //                    {
                //                        MessageBox.Show("Invalid start Address");
                //                        return;
                //                    }
                //                    int i = 0;

//                    //foreach (string tok in hexTokens)
                //                    for (int j = 0; j < hexTokens.Length; j++)
                //                    {
                //                        string tok;

//                        try
                //                        {
                //                            tok = hexTokens[j];
                //                        }
                //                        catch
                //                        {
                //                            tok = "";
                //                        }

//                        if ((tok.Length > 0) && (bytes > j))
                //                        {
                //                            try
                //                            {
                //                                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                //                            }
                //                            catch (Exception exc)
                //                            {
                //                                MessageBox.Show(exc.Message, "Input Error");
                //                                return;
                //                            }
                //                        }
                //                    }
                //                    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                //                    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Writing RP Sel Reg. Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Writing RP Sel. Failed";
                //                }
                //                else if (InOutBox.Text.Equals("Read") && RegTypeCBox.Text.Equals("Read Ctrl2"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 1;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //                    toolStripStatusLabel1.Text = "Reading Data From Eeprom";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xD3;
                //                    //Read actual data
                //                    operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, numberOfBytesToRead, out readDataBuffer);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Reading Data Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Reading Data Failed";
                //                }
                ////
                //                else if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals("WriteCtl4"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 1;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //                    toolStripStatusLabel1.Text = "Write in Progress";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xC1;
                //                    if (startAddress > 0x1F)
                //                    {
                //                        MessageBox.Show("Invalid start Address");
                //                        return;
                //                    }
                //                    int i = 0;

//                    //foreach (string tok in hexTokens)
                //                    for (int j = 0; j < hexTokens.Length; j++)
                //                    {
                //                        string tok;

//                        try
                //                        {
                //                            tok = hexTokens[j];
                //                        }
                //                        catch
                //                        {
                //                            tok = "";
                //                        }

//                        if ((tok.Length > 0) && (bytes > j))
                //                        {
                //                            try
                //                            {
                //                                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                //                            }
                //                            catch (Exception exc)
                //                            {
                //                                MessageBox.Show(exc.Message, "Input Error");
                //                                return;
                //                            }
                //                        }
                //                    }
                //                    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                //                    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Writing RP Sel Reg. Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Writing RP Sel. Failed";
                //                }
                //                else if (InOutBox.Text.Equals("Read") && RegTypeCBox.Text.Equals("ReadCtl4"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 1;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //                    toolStripStatusLabel1.Text = "Reading Data From Eeprom";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xC0;
                //                    //Read actual data
                //                    operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, numberOfBytesToRead, out readDataBuffer);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Reading Data Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Reading Data Failed";
                //                }
                //                else if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals("WriteDelayCountLSB"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 1;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //                    toolStripStatusLabel1.Text = "Write in Progress";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xF6;
                //                    if (startAddress > 0x1F)
                //                    {
                //                        MessageBox.Show("Invalid start Address");
                //                        return;
                //                    }
                //                    int i = 0;

//                    //foreach (string tok in hexTokens)
                //                    for (int j = 0; j < hexTokens.Length; j++)
                //                    {
                //                        string tok;

//                        try
                //                        {
                //                            tok = hexTokens[j];
                //                        }
                //                        catch
                //                        {
                //                            tok = "";
                //                        }

//                        if ((tok.Length > 0) && (bytes > j))
                //                        {
                //                            try
                //                            {
                //                                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                //                            }
                //                            catch (Exception exc)
                //                            {
                //                                MessageBox.Show(exc.Message, "Input Error");
                //                                return;
                //                            }
                //                        }
                //                    }
                //                    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                //                    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Writing  Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Writing  Failed";
                //                }
                //                else if (InOutBox.Text.Equals("Read") && RegTypeCBox.Text.Equals("ReadDelayCountLSB"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 1;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //                    toolStripStatusLabel1.Text = "Reading Data From Eeprom";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xF5;
                //                    //Read actual data
                //                    operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, numberOfBytesToRead, out readDataBuffer);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Reading Data Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Reading Data Failed";
                //                }

//                else if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals("WriteDelayCountMSB"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 1;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //                    toolStripStatusLabel1.Text = "Write in Progress";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xF8;
                //                    if (startAddress > 0x1F)
                //                    {
                //                        MessageBox.Show("Invalid start Address");
                //                        return;
                //                    }
                //                    int i = 0;

//                    //foreach (string tok in hexTokens)
                //                    for (int j = 0; j < hexTokens.Length; j++)
                //                    {
                //                        string tok;

//                        try
                //                        {
                //                            tok = hexTokens[j];
                //                        }
                //                        catch
                //                        {
                //                            tok = "";
                //                        }

//                        if ((tok.Length > 0) && (bytes > j))
                //                        {
                //                            try
                //                            {
                //                                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                //                            }
                //                            catch (Exception exc)
                //                            {
                //                                MessageBox.Show(exc.Message, "Input Error");
                //                                return;
                //                            }
                //                        }
                //                    }
                //                    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                //                    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Writing  Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Writing  Failed";
                //                }
                //                else if (InOutBox.Text.Equals("Read") && RegTypeCBox.Text.Equals("ReadDelayCountMSB"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 1;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //                    toolStripStatusLabel1.Text = "Reading Data From Eeprom";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xF7;
                //                    //Read actual data
                //                    operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, numberOfBytesToRead, out readDataBuffer);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Reading Data Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Reading Data Failed";
                //                }

//                else if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals("WriteDelayCTRL"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 1;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //                    toolStripStatusLabel1.Text = "Write in Progress";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xFA;
                //                    if (startAddress > 0x1F)
                //                    {
                //                        MessageBox.Show("Invalid start Address");
                //                        return;
                //                    }
                //                    int i = 0;

//                    //foreach (string tok in hexTokens)
                //                    for (int j = 0; j < hexTokens.Length; j++)
                //                    {
                //                        string tok;

//                        try
                //                        {
                //                            tok = hexTokens[j];
                //                        }
                //                        catch
                //                        {
                //                            tok = "";
                //                        }

//                        if ((tok.Length > 0) && (bytes > j))
                //                        {
                //                            try
                //                            {
                //                                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                //                            }
                //                            catch (Exception exc)
                //                            {
                //                                MessageBox.Show(exc.Message, "Input Error");
                //                                return;
                //                            }
                //                        }
                //                    }
                //                    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                //                    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Writing  Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Writing  Failed";
                //                }
                //                else if (InOutBox.Text.Equals("Read") && RegTypeCBox.Text.Equals("ReadDelayCTRL"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 1;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //                    toolStripStatusLabel1.Text = "Reading Data From Eeprom";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xF9;
                //                    //Read actual data
                //                    operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, numberOfBytesToRead, out readDataBuffer);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Reading Data Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Reading Data Failed";
                //                }

//                else if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals("Send PRSwap Command"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 2;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //                    toolStripStatusLabel1.Text = "Write in Progress";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xDB;
                //                    if (startAddress > 0x1F)
                //                    {
                //                        MessageBox.Show("Invalid start Address");
                //                        return;
                //                    }
                //                    int i = 0;

//                    //foreach (string tok in hexTokens)
                //                    for (int j = 0; j < hexTokens.Length; j++)
                //                    {
                //                        string tok;

//                        try
                //                        {
                //                            tok = hexTokens[j];
                //                        }
                //                        catch
                //                        {
                //                            tok = "";
                //                        }

//                        if ((tok.Length > 0) && (bytes > j))
                //                        {
                //                            try
                //                            {
                //                                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                //                            }
                //                            catch (Exception exc)
                //                            {
                //                                MessageBox.Show(exc.Message, "Input Error");
                //                                return;
                //                            }
                //                        }
                //                        i++;
                //                    }
                //                    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                //                    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Command Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Command Failed";
                //                }
                //                else if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals("Device Init"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 1;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //                    toolStripStatusLabel1.Text = "Write in Progress";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xBE;
                //                    if (startAddress > 0x1F)
                //                    {
                //                        MessageBox.Show("Invalid start Address");
                //                        return;
                //                    }
                //                    int i = 0;

//                    //foreach (string tok in hexTokens)
                //                    for (int j = 0; j < hexTokens.Length; j++)
                //                    {
                //                        string tok;

//                        try
                //                        {
                //                            tok = hexTokens[j];
                //                        }
                //                        catch
                //                        {
                //                            tok = "";
                //                        }

//                        if ((tok.Length > 0) && (bytes > j))
                //                        {
                //                            try
                //                            {
                //                                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                //                            }
                //                            catch (Exception exc)
                //                            {
                //                                MessageBox.Show(exc.Message, "Input Error");
                //                                return;
                //                            }
                //                        }
                //                    }
                //                    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                //                    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Command Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Command Failed";
                //                }
                //                else if (InOutBox.Text.Equals("Read") && RegTypeCBox.Text.Equals("Read Init Role"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 32;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    toolStripStatusLabel1.Text = "Reading Data From Eeprom";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xB7;
                //                    //Read actual data
                //                    operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, numberOfBytesToRead, out readDataBuffer);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes + 1, operationStatus);
                //                        toolStripStatusLabel1.Text = "Reading Data Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Reading Data Failed";
                //                }
                //                else if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals("Write CTRL3 Reg"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 1;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //                    toolStripStatusLabel1.Text = "Write in Progress";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xB8;
                //                    if (startAddress > 0x1F)
                //                    {
                //                        MessageBox.Show("Invalid start Address");
                //                        return;
                //                    }
                //                    int i = 0;

//                    //foreach (string tok in hexTokens)
                //                    for (int j = 0; j < hexTokens.Length; j++)
                //                    {
                //                        string tok;

//                        try
                //                        {
                //                            tok = hexTokens[j];
                //                        }
                //                        catch
                //                        {
                //                            tok = "";
                //                        }

//                        if ((tok.Length > 0) && (bytes > j))
                //                        {
                //                            try
                //                            {
                //                                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                //                            }
                //                            catch (Exception exc)
                //                            {
                //                                MessageBox.Show(exc.Message, "Input Error");
                //                                return;
                //                            }
                //                        }
                //                    }
                //                    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                //                    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Writing RP Sel Reg. Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Writing RP Sel. Failed";
                //                }
                //                else if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals("WriteBaudRate"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 1;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //                    toolStripStatusLabel1.Text = "Write in Progress";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xB3;
                //                    if (startAddress > 0x1F)
                //                    {
                //                        MessageBox.Show("Invalid start Address");
                //                        return;
                //                    }
                //                    int i = 0;

//                    //foreach (string tok in hexTokens)
                //                    for (int j = 0; j < hexTokens.Length; j++)
                //                    {
                //                        string tok;

//                        try
                //                        {
                //                            tok = hexTokens[j];
                //                        }
                //                        catch
                //                        {
                //                            tok = "";
                //                        }

//                        if ((tok.Length > 0) && (bytes > j))
                //                        {
                //                            try
                //                            {
                //                                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                //                            }
                //                            catch (Exception exc)
                //                            {
                //                                MessageBox.Show(exc.Message, "Input Error");
                //                                return;
                //                            }
                //                        }
                //                    }
                //                    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                //                    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Writing RP Sel Reg. Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Writing RP Sel. Failed";
                //                }

//                else if (InOutBox.Text.Equals("Read") && RegTypeCBox.Text.Equals("ReadBaudRate"))
                //                {
                //                    DeviceAddr.Text = "0x0000";
                //                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                //                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                //                    numberOfBytesToRead = 1;
                //                    dataBuffer = new byte[numberOfBytesToRead];
                //                    readDataBuffer = new byte[numberOfBytesToRead];
                //                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                //                    toolStripStatusLabel1.Text = "Reading Data From Eeprom";
                //                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                //                    byte reqCode = 0xB2;
                //                    //Read actual data
                //                    operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, numberOfBytesToRead, out readDataBuffer);
                //                    //Sent Data to Control End Point need to display it now
                //                    if (operationStatus)
                //                    {
                //                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                //                        toolStripStatusLabel1.Text = "Reading Data Completed";
                //                    }
                //                    else
                //                        toolStripStatusLabel1.Text = "Reading Data Failed";
                //                }


                else if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals("VBUS_TESTING"))
                {
                    DeviceAddr.Text = "0x0000";
                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                    numberOfBytesToRead = 1;
                    dataBuffer = new byte[numberOfBytesToRead];
                    readDataBuffer = new byte[numberOfBytesToRead];
                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                    toolStripStatusLabel1.Text = "Write in Progress";
                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                    byte reqCode = 0xF1;
                    int i = 0;

                    //foreach (string tok in hexTokens)
                    for (int j = 0; j < hexTokens.Length; j++)
                    {
                        string tok;

                        try
                        {
                            tok = hexTokens[j];
                        }
                        catch
                        {
                            tok = "";
                        }

                        if ((tok.Length > 0) && (bytes > j))
                        {
                            try
                            {
                                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                            }
                            catch (Exception exc)
                            {
                                MessageBox.Show(exc.Message, "Input Error");
                                return;
                            }
                        }
                    }
                    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount, true);
                    //Sent Data to Control End Point need to display it now
                    if (operationStatus)
                    {
                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                        toolStripStatusLabel1.Text = "Write Completed";
                    }
                    else
                        toolStripStatusLabel1.Text = "Write Failed";
                }
                else if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals("SPI_FLASH"))
                {
                    DeviceAddr.Text = "0x0000";
                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                    numberOfBytesToRead = 1;
                    dataBuffer = new byte[numberOfBytesToRead];
                    readDataBuffer = new byte[numberOfBytesToRead];
                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                    toolStripStatusLabel1.Text = "Write in Progress";
                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                    byte reqCode = 0xF5;
                    int i = 0;

                    //foreach (string tok in hexTokens)
                    for (int j = 0; j < hexTokens.Length; j++)
                    {
                        string tok;

                        try
                        {
                            tok = hexTokens[j];
                        }
                        catch
                        {
                            tok = "";
                        }

                        if ((tok.Length > 0) && (bytes > j))
                        {
                            try
                            {
                                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                            }
                            catch (Exception exc)
                            {
                                MessageBox.Show(exc.Message, "Input Error");
                                return;
                            }
                        }
                    }
                    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
                    //Sent Data to Control End Point need to display it now
                    if (operationStatus)
                    {
                        DisplayXferData(readDataBuffer, bytes, operationStatus);
                        toolStripStatusLabel1.Text = "Write Completed";
                    }
                    else
                        toolStripStatusLabel1.Text = "Write Failed";
                }


                else if (InOutBox.Text.Equals("Read") && RegTypeCBox.Text.Equals("APP CMD MSG"))
                {
                    DeviceAddr.Text = "0x0000";
                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                    numberOfBytesToRead = 32;
                    dataBuffer = new byte[numberOfBytesToRead];
                    readDataBuffer = new byte[numberOfBytesToRead];
                    toolStripStatusLabel1.Text = "Reading Data From Eeprom";
                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                    byte reqCode = 0xD5;
                    int i = 0;

                    //foreach (string tok in hexTokens)
                    for (int j = 0; j < hexTokens.Length; j++)
                    {
                        string tok;

                        try
                        {
                            tok = hexTokens[j];
                        }
                        catch
                        {
                            tok = "";
                        }

                        if ((tok.Length > 0) && (bytes > j))
                        {
                            try
                            {
                                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                            }
                            catch (Exception exc)
                            {
                                MessageBox.Show(exc.Message, "Input Error");
                                return;
                            }
                        }
                        i++;
                    }
                    //Read actual data
                    operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, numberOfBytesToRead, ref readDataBuffer);
                    //Sent Data to Control End Point need to display it now
                    if (operationStatus)
                    {
                        DisplayXferData(readDataBuffer, bytes + 1, operationStatus);
                        toolStripStatusLabel1.Text = "Reading Data Completed";
                    }
                    else
                        toolStripStatusLabel1.Text = "Reading Data Failed";
                }
                else if (InOutBox.Text.Equals("Write") && RegTypeCBox.Text.Equals("APP CMD MSG"))
                {
                    DeviceAddr.Text = "0x0000";
                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                    startAddress = (ushort)Convert.ToInt16(StartAddr.Text, 16);
                    numberOfBytesToRead = 16;
                    dataBuffer = new byte[numberOfBytesToRead];
                    readDataBuffer = new byte[numberOfBytesToRead];
                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                    toolStripStatusLabel1.Text = "Write in Progress";
                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                    byte reqCode = 0xD5;
                    int i = 0;

                    //foreach (string tok in hexTokens)
                    for (int j = 0; j < hexTokens.Length; j++)
                    {
                        string tok;

                        try
                        {
                            tok = hexTokens[j];
                        }
                        catch
                        {
                            tok = "";
                        }

                        if ((tok.Length > 0) && (bytes > j))
                        {
                            try
                            {
                                dataBuffer[i] = (byte)Convert.ToInt32(tok, 16);
                            }
                            catch (Exception exc)
                            {
                                MessageBox.Show(exc.Message, "Input Error");
                                return;
                            }
                        }
                        i++;
                    }
                    //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
                    //Sent Data to Control End Point need to display it now
                    if (operationStatus)
                    {
                        DisplayXferData(dataBuffer, bytes, operationStatus);
                        toolStripStatusLabel1.Text = "Write Completed";
                    }
                    else
                        toolStripStatusLabel1.Text = "Write Failed";
                }
                //Flash PRogramming Requirement
            }
            catch (Exception exc)
            {
                if (bytes < 1)
                {
                    //Just to remove warning
                    exc.ToString();

                    MessageBox.Show("Enter a valid number of bytes to transfer.", "Invalid Bytes to Transfer");
                    return;
                }
                //   if (bytes > 1 && InOutBox.Text.Equals
            }

        }

        /* Summary
            For HID devices called from transfer data button and transfer file button
        */
        /*
        private void DoHidXfer(object sender, EventArgs e)
        {
            bool bResult = false;

            if (DataTransfer.Text.Contains("Feature"))
            {
                if (sender == FileTransferBtn)
                {
                    dataCaption = "Set feature ";
                    OutputTextBox.Text += dataCaption;

                    LoadHidReport();
                    bResult = curHidDev.SetFeature(curHidReport.ID);
                }
                else
                {
                    dataCaption = "Get feature ";
                    OutputTextBox.Text += dataCaption;
                    bResult = curHidDev.GetFeature(curHidReport.ID);
                }
            }

            else if (DataTransfer.Text.Contains("Input"))
            {
                dataCaption = "Get input ";
                OutputTextBox.Text += dataCaption;
                bResult = curHidDev.GetInput(curHidReport.ID);
            }

            else if (DataTransfer.Text.Contains("Output"))
            {
                dataCaption = "Set output ";
                OutputTextBox.Text += dataCaption;

                LoadHidReport();
                bResult = curHidDev.SetOutput(curHidReport.ID);
            }

            if (bResult)
                DisplayXferData(curHidReport.DataBuf, curHidReport.RptByteLen, true);
            else
            {
                OutputTextBox.Text += string.Format("\r\n{0}failed\r\n\r\n", dataCaption);
                OutputTextBox.SelectionStart = OutputTextBox.Text.Length;
                OutputTextBox.ScrollToCaret();
            }
        }

        */
        /* Summary
            Just to print the values in formatted order in output box
        */
        private void DisplayXferData(byte[] buf, int bCnt, bool TransferStatus)
        {
            string dataCaption = null;
            StringBuilder dataStr = new StringBuilder();

            string resultStr = "";
            if (bCnt > 0)
            {
                if (TransferStatus)
                {
                    // resultStr = dataCaption + "completed\r\n";

                    for (int i = 0; i < bCnt && i < buf.Length; i++)
                    {
                        if ((i % 16) == 0) dataStr.Append(string.Format("\r\n{0:X4}", i));
                        dataStr.Append(string.Format(" {0:X2}", buf[i]));
                    }
                }
                else
                {
                    //resultStr = dataCaption + "failed\r\n";
                    resultStr = dataCaption + "failed with Error Code:" + curEndpt.LastError + "\r\n";

                }

                OutputTextBox.Text += dataStr.ToString() + "\r\n" + resultStr + "\r\n";
            }
            else
            {
                if (TransferStatus)
                {
                    resultStr = "Zero-length data transfer completed\r\n";
                }
                else
                {
                    //if (buf.Length > 0)
                    //{
                    //    for (int i = 0; i < buf.Length; i++)
                    //    {
                    //        if ((i % 16) == 0) dataStr.Append(string.Format("\r\n{0:X4}", i));
                    //        dataStr.Append(string.Format(" {0:X2}", buf[i]));
                    //    }

                    //    resultStr = "\r\nPartial data Transferred\r\n";
                    //}

                    //resultStr = dataCaption + "failed\r\n";
                    resultStr = dataCaption + "failed with Error Code:" + curEndpt.LastError + "\r\n";


                }

                OutputTextBox.Text += dataStr.ToString() + "\r\n" + resultStr + "\r\n";
            }


            OutputTextBox.SelectionStart = OutputTextBox.Text.Length;
            OutputTextBox.ScrollToCaret();
        }

        private void ClearBox_Click(object sender, EventArgs e)
        {
            OutputTextBox.Clear();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void sPIProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!PrepareForFirmwareDownload())
                return; // Either Boot loader is not up or downloding of boot programmer is failed

            //Create thread to check and enumerate the Boot Programmer
            MyThread workerObject = new MyThread(this, FxDev, FX3_FWDWNLOAD_MEDIA_TYPE.I2CE2PROM);
            Thread workerThread = new Thread(workerObject.DoWork);
            workerThread.SetApartmentState(ApartmentState.STA);
            workerThread.Start();
        }
        private bool PrepareForFirmwareDownload()
        {
            // Chech for boot loader
            FxDev = cyFx3.GetUsbDeviceObject();
            if (FxDev == null)
                return false;

            CyFX3Device fx = FxDev as CyFX3Device;

            // check for bootloader first, if it is not running then prompt message to user.
            if (!fx.IsBootLoaderRunning())
            {
                MessageBox.Show("Please reset your device to download firmware", "Bootloader is not running");
                return false;
            }
            toolStripStatusLabel1.Text = "Downloading GRL Boot Programmer...";
            Refresh();

            //Download Default IMG file
            FX3_FWDWNLOAD_ERROR_CODE enmbResult = FX3_FWDWNLOAD_ERROR_CODE.SUCCESS;

            string fPath_fromShortcut = Path.Combine(Directory.GetParent(Application.ExecutablePath).FullName, "GRLBootProgrammer.img");

            string fPath_buildDir = Path.Combine(Directory.GetParent(Application.ExecutablePath).FullName, "..\\..\\GRLBootProgrammer.img");

            string fPath = null;

            if (File.Exists(fPath_fromShortcut))
                fPath = fPath_fromShortcut;
            else if (File.Exists(fPath_buildDir))
                fPath = fPath_buildDir;
            else
                MessageBox.Show("Can't find the file", "GRLBootProgrammer.img");

            enmbResult = fx.DownloadFw(fPath, FX3_FWDWNLOAD_MEDIA_TYPE.RAM);

            toolStripStatusLabel1.Text = "Programming of Boot Programmer " + fx.GetFwErrorString(enmbResult);
            Refresh();

            if (enmbResult == FX3_FWDWNLOAD_ERROR_CODE.FAILED)
                return false;

            toolStripStatusLabel1.Text = "Waiting for GRL Boot Programmer device to enumerate....";
            Refresh();

            return true;
        }
        public void BootProgrammerDeviceFound()
        {
            if (InvokeRequired)
            {
                MethodInvoker method = new MethodInvoker(BootProgrammerDeviceFound);
                Invoke(method);
            }

            StatLabel.Text = "GRL Boot Programmer Device Found";
            //Refresh();
        }
        public void BootProgrammerDeviceNotFound()
        {
            StatLabel.Text = "GRL Boot Programmer Device NOT Found";
            Refresh();
        }


        public void DownloadUserImg(CyFX3Device Fx3, FX3_FWDWNLOAD_MEDIA_TYPE enmMediaType, Form Form1Obj)
        {
            OpenFileDialog FOpenDialog = new OpenFileDialog();
            // string tmpFilter = Fr.FOpenDialog.Filter;
            FX3_FWDWNLOAD_ERROR_CODE enmResult = FX3_FWDWNLOAD_ERROR_CODE.FAILED;
            string tmpFilter = FOpenDialog.Filter;
            FOpenDialog.Filter = "Firmware Image files (*.img) | *.img";
            if ((Fx3 != null) && (FOpenDialog.ShowDialog() == DialogResult.OK))
            {
                if (enmMediaType == FX3_FWDWNLOAD_MEDIA_TYPE.I2CE2PROM)
                {// I2C EEPROM Download
                    toolStripStatusLabel1.Text = "Programming of Application Processor in Progress...";
                    //Refresh();
                    enmResult = Fx3.DownloadFw(FOpenDialog.FileName, FX3_FWDWNLOAD_MEDIA_TYPE.I2CE2PROM);
                    toolStripStatusLabel1.Text = "Programming " + Fx3.GetFwErrorString(enmResult);
                    //Refresh();
                }
                else
                {// SPI FLASH FIRMWARE DOWNLOAD
                    //Form1Obj.StatLabel.Text = "Programming of SPI FLASH in Progress...";

                    //Refresh();
                    enmResult = Fx3.DownloadFw(FOpenDialog.FileName, FX3_FWDWNLOAD_MEDIA_TYPE.SPIFLASH);
                    //enmResult = Fx3.DownloadFw("C:\\app.img", FX3_FWDWNLOAD_MEDIA_TYPE.SPIFLASH);
                    //StatLabel.Text = "Programming of SPI FLASH " + Fx3.GetFwErrorString(enmResult);
                    //Refresh();
                }
            }
            else
            {
                toolStripStatusLabel1.Text = "User Cancelled operation";
                Refresh();
            }
            FOpenDialog.FileName = "";
            FOpenDialog.Filter = tmpFilter;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            NumByteBoxLabel.Visible = false;
            NumBytesBox.Visible = false;
            StartAddrLabel.Visible = false;
            StartAddr.Visible = false;
            //InOutBox.Visible = false;
            //StartAddrLabel.Visible = false;
        }

        private void InOutBox_TextUpdate(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                InitFX3();
            }
            catch (Exception ex)
            {

            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        //       private void FileOpen_Click(object sender, EventArgs e)
        //     {
        //          OpenFileDialog fDialog = new OpenFileDialog();
        //            fDialog.Filter = "Bin Files|*.bin";
        //            fDialog.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);//@"C:\";
        //            if (fDialog.ShowDialog() == DialogResult.OK)
        //                {
        //                 //   MessageBox.Show(fDialog.FileName.ToString());
        //                }
        //            FilePath.Text = fDialog.FileName;
        //
        //       }

        private void FlashProgramming_Click(object sender, EventArgs e)
        {
            //byte[] buf = new byte[102400];
            bool operationStatus = false;
            byte[] dataBuffer;
            int bytesRead;
            long fileLength = 0;

            // Read the file 100Kb at a time.
            // BinaryReader fileLength = new BinaryReader(File.Open(FilePath.Text, FileMode.Open;


            dataBuffer = new byte[260];//64

            //   bytesRead = fs.Read(flashProgrBuffer, 0, (256*1024));
            try
            {
                // Open file for reading
                //                System.IO.FileStream _FileStream = new System.IO.FileStream(FilePath.Text, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                // attach filestream to binary reader
                //                System.IO.BinaryReader _BinaryReader = new System.IO.BinaryReader(_FileStream);

                // get total byte length of the file
                //                fileLength = new System.IO.FileInfo(FilePath.Text).Length;


                // read entire file into buffer
                //                flashProgrBuffer = _BinaryReader.ReadBytes((Int32)fileLength);

                // close file reader
                //                _FileStream.Close();
                //                _FileStream.Dispose();
                //                _BinaryReader.Close();
            }
            catch (Exception _Exception)
            {
                // Error
                Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());
            }


            if (fileLength > 0)
            {
                DeviceAddr.Text = "0x0000";
                devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                byte startAddress = 0;
                int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                toolStripStatusLabel1.Text = "Write in Progress";
                string[] hexTokens = DataToSendBox.Text.Split(' ');
                byte reqCode = 0xF2;
                int i = 0;
                int bufferCount = (int)(fileLength / 256);
                int remainBytes = (int)(fileLength % 256);

                dataBuffer[0] = 0x02;

                do
                {
                    dataBuffer[1] = 0x00;//startAddress >> 16;
                    dataBuffer[2] = 0x00;//startAddress >> 8;
                    dataBuffer[3] = 0x00;

                    bytesCount = 260;
                    //flashProgrBuffer.(dataBuffer, bytesCount);
                    if (startAddress != bufferCount)
                        Buffer.BlockCopy(flashProgrBuffer, startAddress * 256, dataBuffer, 4, 256);
                    else
                        Buffer.BlockCopy(flashProgrBuffer, startAddress * 256, dataBuffer, 0, remainBytes);
                    //MessageBox.Show("Buffer:" + dataBuffer.ToString());


                    operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
                    i += 256;
                    startAddress++;
                } while (i < fileLength);
                //Sent Data to Control End Point need to display it now
                if (operationStatus)
                {
                    toolStripStatusLabel1.Text = "Write Completed";
                }
                else
                    toolStripStatusLabel1.Text = "Write Failed";
            }
        }

        private void FlashRead_Click(object sender, EventArgs e)
        {
            {
                //byte[] buf = new byte[102400];
                bool operationStatus = false;
                byte[] dataBuffer;
                int bytesRead;
                long fileLength = 0;

                // Read the file 100Kb at a time.
                // BinaryReader fileLength = new BinaryReader(File.Open(FilePath.Text, FileMode.Open;


                dataBuffer = new byte[256];

                //   bytesRead = fs.Read(flashProgrBuffer, 0, (256*1024));
                try
                {
                    // Open file for reading
                    // System.IO.FileStream _FileStream = new System.IO.FileStream(FilePath.Text, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                    // attach filestream to binary reader
                    // System.IO.BinaryReader _BinaryReader = new System.IO.BinaryReader(_FileStream);

                    // get total byte length of the file
                    //fileLength = new System.IO.FileInfo(FilePath.Text).Length;


                    // read entire file into buffer
                    //flashProgrBuffer = _BinaryReader.ReadBytes((Int32)fileLength);

                    // close file reader
                    //_FileStream.Close();
                    //_FileStream.Dispose();
                    //_BinaryReader.Close();
                }
                catch (Exception _Exception)
                {
                    // Error
                    Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());
                }


                //            if (fileLength > 0)
                {
                    DeviceAddr.Text = "0x0000";
                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                    byte startAddress = 0;
                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                    toolStripStatusLabel1.Text = "Write in Progress";
                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                    byte reqCode = 0xF3;
                    int i = 0;
                    int bufferCount = (int)(fileLength / 252);
                    int remainBytes = (int)(fileLength % 252);

                    dataBuffer[0] = 0x03;
                    do
                    {
                        dataBuffer[1] = 0x00;//startAddress >> 16;
                        dataBuffer[2] = 0x00;//startAddress >> 8;
                        dataBuffer[3] = 0x00;

                        bytesCount = 256;

                        operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, 256, ref dataBuffer);
                        //flashProgrBuffer.(dataBuffer, bytesCount);
                        if (startAddress != bufferCount)
                            Buffer.BlockCopy(dataBuffer, startAddress * 256, flashProgrBuffer, 0, 256);
                        else
                            Buffer.BlockCopy(dataBuffer, startAddress * 256, flashProgrBuffer, 0, remainBytes);
                        //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                        i += 256;
                        startAddress++;
                    } while (i < fileLength);
                    //Sent Data to Control End Point need to display it now
                    if (operationStatus)
                    {
                        toolStripStatusLabel1.Text = "Read Completed";
                    }
                    else
                        toolStripStatusLabel1.Text = "read Failed";
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            {
                //byte[] buf = new byte[102400];
                bool operationStatus = false;
                byte[] dataBuffer;
                int bytesRead;
                long fileLength = 0;

                // Read the file 100Kb at a time.
                // BinaryReader fileLength = new BinaryReader(File.Open(FilePath.Text, FileMode.Open;


                dataBuffer = new byte[20];

                //   bytesRead = fs.Read(flashProgrBuffer, 0, (256*1024));
                try
                {
                    // Open file for reading
                    // System.IO.FileStream _FileStream = new System.IO.FileStream(FilePath.Text, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                    // attach filestream to binary reader
                    // System.IO.BinaryReader _BinaryReader = new System.IO.BinaryReader(_FileStream);

                    // get total byte length of the file
                    //fileLength = new System.IO.FileInfo(FilePath.Text).Length;


                    // read entire file into buffer
                    //flashProgrBuffer = _BinaryReader.ReadBytes((Int32)fileLength);

                    // close file reader
                    //_FileStream.Close();
                    //_FileStream.Dispose();
                    //_BinaryReader.Close();
                }
                catch (Exception _Exception)
                {
                    // Error
                    Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());
                }


                //            if (fileLength > 0)
                {
                    DeviceAddr.Text = "0x0000";
                    devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
                    byte startAddress = 0;
                    int bytesCount = Convert.ToInt32(NumBytesBox.Text);
                    toolStripStatusLabel1.Text = "Write in Progress";
                    string[] hexTokens = DataToSendBox.Text.Split(' ');
                    byte reqCode = 0xFF;
                    int i = 0;
                    int bufferCount = (int)(fileLength / 20);
                    int remainBytes = (int)(fileLength % 20);

                    dataBuffer[0] = 0x9F;
                    do
                    {
                        //                        dataBuffer[1] = 0x00;//startAddress >> 16;
                        //                        dataBuffer[2] = 0x00;//startAddress >> 8;
                        //                        dataBuffer[3] = startAddress;

                        bytesCount = 20;

                        operationStatus = cyFx3.ReadSPI(reqCode, devAddress, startAddress, 20, ref dataBuffer);
                        //flashProgrBuffer.(dataBuffer, bytesCount);
                        if (startAddress != bufferCount)
                            Buffer.BlockCopy(dataBuffer, startAddress * 20, flashProgrBuffer, 0, 20);
                        else
                            Buffer.BlockCopy(dataBuffer, startAddress * 20, flashProgrBuffer, 0, remainBytes);
                        //MessageBox.Show("Buffer:" + dataBuffer.ToString());
                        i += 20;
                        startAddress++;
                    } while (i < fileLength);
                    //Sent Data to Control End Point need to display it now
                    if (operationStatus)
                    {
                        toolStripStatusLabel1.Text = "Read Completed";
                    }
                    else
                        toolStripStatusLabel1.Text = "read Failed";
                }
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void TexToSendBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void RegTypeCBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void File_open()
        {
            m_strFilename = "";
            OpenFileDialog fDialog_Phy = new OpenFileDialog();
            fDialog_Phy.Filter = "Bin Files|*.bin;*.jed";
            fDialog_Phy.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);//@"C:\";
            if (fDialog_Phy.ShowDialog() == DialogResult.OK)
            {
                //FilePath.Text = fDialog_Phy.FileName;
                m_strFilename = fDialog_Phy.FileName;


            }
        }
        private void button6_Click(object sender, EventArgs e)//iCE40LP8K Flash
        {
            bool operationStatus = false;
            byte[] dataBuffer;

            dataBuffer = new byte[1];//64

            File_open();
            DeviceAddr.Text = "0x0000";
            devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
            byte startAddress = 0;
            int bytesCount = Convert.ToInt32(NumBytesBox.Text);
            toolStripStatusLabel1.Text = "Write in Progress";
            string[] hexTokens = DataToSendBox.Text.Split(' ');
            byte reqCode = 0xD5;

            dataBuffer[0] = 0x30;

            operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
            //Sent Data to Control End Point need to display it now
            if (operationStatus)
            {
                toolStripStatusLabel1.Text = "iCE40LP8K Flash Device Selected";
            }
            //else

            if (phy_flash() == FT_STATUS.FT_OK)
            {
                toolStripStatusLabel1.ForeColor = Color.Green;
                toolStripStatusLabel1.Text = "8K Flash Programming Success";
            }
            else
            {
                toolStripStatusLabel1.ForeColor = Color.Red;
                toolStripStatusLabel1.Text = "write 8K flash Failed";
            }

            //    toolStripStatusLabel1.Text = "Write Failed";
        }

        private void button5_Click(object sender, EventArgs e)//MachXO2
        {
            bool operationStatus = false;
            byte[] dataBuffer;

            dataBuffer = new byte[1];//64


            DeviceAddr.Text = "0x0000";
            devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
            byte startAddress = 0;
            int bytesCount = Convert.ToInt32(NumBytesBox.Text);
            toolStripStatusLabel1.Text = "Write in Progress";
            string[] hexTokens = DataToSendBox.Text.Split(' ');
            byte reqCode = 0xD5;

            dataBuffer[0] = 0x2D;

            operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
            //Sent Data to Control End Point need to display it now
            if (operationStatus)
            {
                toolStripStatusLabel1.Text = "MachXO2 Device Selected";
            }
            else
                toolStripStatusLabel1.Text = "Write Failed";

        }

        private void button6_Click_1(object sender, EventArgs e)//iCE40LP8k FPGA
        {
            bool operationStatus = false;
            byte[] dataBuffer;

            dataBuffer = new byte[1];//64


            DeviceAddr.Text = "0x0000";
            devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
            byte startAddress = 0;
            int bytesCount = Convert.ToInt32(NumBytesBox.Text);
            toolStripStatusLabel1.Text = "Write in Progress";
            string[] hexTokens = DataToSendBox.Text.Split(' ');
            byte reqCode = 0xD5;

            dataBuffer[0] = 0x3B;

            operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
            //Sent Data to Control End Point need to display it now
            if (operationStatus)
            {
                toolStripStatusLabel1.Text = "iCE40LP8k FPGA Device Selected";
            }
            else
                toolStripStatusLabel1.Text = "Write Failed";
        }

        private void button7_Click(object sender, EventArgs e)//iCE5LP1K FPGA
        {
            bool operationStatus = false;
            byte[] dataBuffer;

            dataBuffer = new byte[1];//64


            DeviceAddr.Text = "0x0000";
            devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
            byte startAddress = 0;
            int bytesCount = Convert.ToInt32(NumBytesBox.Text);
            toolStripStatusLabel1.Text = "Write in Progress";
            string[] hexTokens = DataToSendBox.Text.Split(' ');
            byte reqCode = 0xD5;

            dataBuffer[0] = 0x3A;

            operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
            //Sent Data to Control End Point need to display it now
            if (operationStatus)
            {
                toolStripStatusLabel1.Text = "iCE5LP1K FPGA Device Selected";
            }
            else
                toolStripStatusLabel1.Text = "Write Failed";
        }

        private void button5_Click_1(object sender, EventArgs e)//iCE5LP1K_Flash
        {
            bool operationStatus = false;
            byte[] dataBuffer;


            dataBuffer = new byte[1];//64

            File_open();
            DeviceAddr.Text = "0x0000";
            devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
            byte startAddress = 0;
            int bytesCount = Convert.ToInt32(NumBytesBox.Text);
            toolStripStatusLabel1.Text = "Write in Progress";
            string[] hexTokens = DataToSendBox.Text.Split(' ');
            byte reqCode = 0xD5;

            dataBuffer[0] = 0x2F;

            operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
            //Sent Data to Control End Point need to display it now
            if (operationStatus)
            {
                toolStripStatusLabel1.Text = "iCE5LP1K_Flash Device Selected";
            }
            if (phy_flash() == FT_STATUS.FT_OK)
            {
                toolStripStatusLabel1.ForeColor = Color.Green;
                toolStripStatusLabel1.Text = "1K Flash Programming Success";
            }
            else
            {
                toolStripStatusLabel1.ForeColor = Color.Red;
                toolStripStatusLabel1.Text = "write 1K flash Failed";
            }

            //else
            //    toolStripStatusLabel1.Text = "Write Failed";
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void NumByteBoxLabel_Click(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("GRL Utility Version 1.2.0 \n\nCopyright (c) Granite River Labs", "GRL Utility");
            //            MessageBox.Show(Util.Assemblies + "\nCopyright (c) Granite River Labs", Text);
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void On_Off_Click(object sender, EventArgs e)
        {

            int presentState;
            //            previousState = previousState;
            bool operationStatus = false;
            byte[] dataBuffer;

            dataBuffer = new byte[1];//64


            DeviceAddr.Text = "0x0000";
            devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
            byte startAddress = 0;
            int bytesCount = Convert.ToInt32(NumBytesBox.Text);
            toolStripStatusLabel1.Text = "Write in Progress";
            string[] hexTokens = DataToSendBox.Text.Split(' ');
            byte reqCode = 0xF1;

            if (!previousState)
            {
                dataBuffer[0] = 0x01;
                previousState = true;
            }

            else if (previousState)
            {
                dataBuffer[0] = 0x0;
                previousState = false;
            }

            operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
            //Sent Data to Control End Point need to display it now
            if (operationStatus)
            {
                toolStripStatusLabel1.Text = "Write Completed";
            }
            else
                toolStripStatusLabel1.Text = "Write Failed";

        }

        private void Vbus_12_Click(object sender, EventArgs e)
        {
            bool operationStatus = false;
            byte[] dataBuffer;

            dataBuffer = new byte[1];//64


            DeviceAddr.Text = "0x0000";
            devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
            byte startAddress = 0;
            int bytesCount = Convert.ToInt32(NumBytesBox.Text);
            toolStripStatusLabel1.Text = "Write in Progress";
            string[] hexTokens = DataToSendBox.Text.Split(' ');
            byte reqCode = 0xF1;

            dataBuffer[0] = 0x03;

            operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
            //Sent Data to Control End Point need to display it now
            if (operationStatus)
            {
                toolStripStatusLabel1.Text = "Write Completed";
            }
            else
                toolStripStatusLabel1.Text = "Write Failed";

        }

        private void Vbus_20_Click(object sender, EventArgs e)
        {
            bool operationStatus = false;
            byte[] dataBuffer;

            dataBuffer = new byte[1];//64


            DeviceAddr.Text = "0x0000";
            devAddress = (byte)Convert.ToInt16(DeviceAddr.Text, 16);
            byte startAddress = 0;
            int bytesCount = Convert.ToInt32(NumBytesBox.Text);
            toolStripStatusLabel1.Text = "Write in Progress";
            string[] hexTokens = DataToSendBox.Text.Split(' ');
            byte reqCode = 0xF1;

            dataBuffer[0] = 0x04;

            operationStatus = cyFx3.WriteSPI(reqCode, devAddress, startAddress, dataBuffer, bytesCount);
            //Sent Data to Control End Point need to display it now
            if (operationStatus)
            {
                toolStripStatusLabel1.Text = "Write Completed";
            }
            else
                toolStripStatusLabel1.Text = "Write Failed";

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static byte[] ReadFile(string filePath)
        {
            byte[] buffer;

            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                int length = (int)fileStream.Length;  // get file length
                buffer = new byte[length];            // create buffer
                int count;                            // actual number of bytes read
                int sum = 0;                          // total number of bytes read


                while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                    sum += count;  // sum is a buffer offset for next reading
            }
            finally
            {
                fileStream.Close();
            }
            return buffer;
        }
        /******************************************************************************
        *
        * This function reads serial flash ID connected to the SPI interface.
        *
        ******************************************************************************/
        public byte FlashReadID(FT_Handle fthandle)
        {
            UInt32 Index = 0;
            UInt32 ByteCount = 4;
            byte[] SendBuffer = new byte[8];
            byte[] RecvBuffer = new byte[8];
            byte[] outbuffer = new byte[4];
            byte[] inbuffer = new byte[4];
            inbuffer[0] = Release_Deep_POWER_Down;
            FT_STATUS FTstat = FT_STATUS.FT_OK;
            SendBuffer[0] = READ_ID;
            SendBuffer[1] = 0;
            SendBuffer[2] = 0;
            SendBuffer[3] = 0;

            for (Index = 0; Index < ByteCount; Index++)
            {
                SendBuffer[4 + Index] = 0x00;
            }
            Index = 0;
            FTstat = SPI_ReadWrite(fthandle, ref outbuffer[0], ref inbuffer[0], 1, ref Index, 0x00000006);
            Thread.Sleep(10);
            FTstat = SPI_ReadWrite(fthandle, ref RecvBuffer[0], ref SendBuffer[0], (ByteCount + 4), ref Index, 0x00000006);
            Thread.Sleep(2);
            for (Index = 0; Index < 8; Index++)
            {
                Console.WriteLine("ID : " + RecvBuffer[Index]);
            }

            return RecvBuffer[1];
        }
        FT_STATUS WriteEnable(FT_Handle fthandle)
        {
            FT_STATUS status = FT_STATUS.FT_OK;
            try
            {
                byte WriteEnableCmd = WRITE_ENABLE_CMD;
                UInt32 sizetransferred = 0;
                status = SPI_Write(fthandle, ref WriteEnableCmd, 1, ref sizetransferred, 0x00000006);
                if (status != FT_STATUS.FT_OK)
                {
                    Cleanup_libMPSSE();
                }
                else
                {
                    Console.WriteLine("write Enable is success");
                }
            }
            catch (Exception)
            {

                throw;
            }
            return status;
        }
        FT_STATUS WriteDisable(FT_Handle fthandle)
        {
            FT_STATUS status = FT_STATUS.FT_OK;
            try
            {
                byte WriteDisableCmd = WRITE_DISABLE_CMD;
                UInt32 sizetransferred = 0;
                status = SPI_Write(fthandle, ref WriteDisableCmd, 1, ref sizetransferred, 0x00000006);
                if (status != FT_STATUS.FT_OK)
                {
                    Cleanup_libMPSSE();
                }
                else
                {
                    Console.WriteLine("write Disable is success");
                }
            }
            catch (Exception)
            {

                throw;
            }
            return status;
        }
        FT_STATUS FlashErase(FT_Handle fthandle)
        {
            FT_STATUS FTstat = FT_STATUS.FT_OK;
            byte WriteEnableCmd = WRITE_ENABLE_CMD;
            byte[] ReadStatusCmd = new byte[2];
            ReadStatusCmd[0] = READ_STATUS_CMD;
            ReadStatusCmd[1] = 0;  /* must send 2 bytes */
            /* must send 2 bytes */
            byte[] WriteStatusCmd = new byte[2];
            WriteStatusCmd[0] = WRITE_STATUS_CMD;
            WriteStatusCmd[1] = 0x0;
            byte[] FlashStatus = new byte[2];


            UInt32 sizetransferred = 0;
            /*
             * Send the write enable command to the flash so that it can be
             * written to, this needs to be sent as a seperate transfer
             * before the erase
             */


            FTstat = SPI_Write(fthandle, ref WriteEnableCmd, 1, ref sizetransferred, 0x00000006);
            /*
             * Wait for the transfer on the SPI bus to be complete before
             * proceeding
             */
            //while (FTstat == FT_STATUS.FT_OK);

            /*
             * Wait for write enable command to the flash to be completed
             */
            while (true)
            {
                /*
                 * Poll the status register of the device to determine
                 * when it completes, by sending a read status command
                 * and receiving the status byte
                 */
                FTstat = SPI_ReadWrite(fthandle, ref FlashStatus[0], ref ReadStatusCmd[0], 2, ref sizetransferred, 0x00000006);

                /*
                 * Wait for the transfer on the SPI bus to be complete
                 * before proceeding
                 */
                // while (FTstat == FT_STATUS.FT_OK);

                /*
                 * If the status indicates the write is done, then stop
                 * waiting; if a value of 0xFF in the status byte is
                 * read from the device and this loop never exits, the
                 * device slave select is possibly incorrect such that
                 * the device status is not being read
                 */
                if ((FlashStatus[1] & 0x02) == 0x02)
                {
                    break;
                }
            }

            /*
             * Clear write protect bits using write status command to the flash
             * this needs to be sent as a seperate transfer before the erase
             */
            FTstat = SPI_Write(fthandle, ref WriteStatusCmd[0], 1, ref sizetransferred, 0x00000006);
            /*
            * Wait for the transfer on the SPI bus to be complete before
            * proceeding
            */
            // while (FTstat == FT_STATUS.FT_OK);

            /*
             * Check for write status command to the flash to be completed
             */
            while (true)
            {
                /*
                 * Poll the status register of the device to determine
                 * when it completes, by sending a read status command
                 * and receiving the status byte
                 */
                FTstat = SPI_ReadWrite(fthandle, ref FlashStatus[0], ref ReadStatusCmd[0], 2, ref sizetransferred, 0x00000006);

                /*
                 * Wait for the transfer on the SPI bus to be complete
                 * before proceeding
                 */
                //while (FTstat==FT_STATUS.FT_OK);

                /*
                 * If the status indicates the write is done, then stop
                 * waiting; if a value of 0xFF in the status byte is
                 * read from the device and this loop never exits, the
                 * device slave select is possibly incorrect such that
                 * the device status is not being read
                 */
                if ((FlashStatus[1] & 0x1C) == 0x0)
                {
                    break;
                }
            }

            /*
             * Send the write enable command to the flash so that it can be
             * written to, this needs to be sent as a seperate transfer
             * before the erase
             */
            FTstat = SPI_Write(fthandle, ref WriteEnableCmd, 1, ref sizetransferred, 0x00000006);
            /*
	         * Wait for the transfer on the SPI bus to be complete before
	         * proceeding
	         */
            //while (FTstat == FT_STATUS.FT_OK);

            /*
             * Wait for write enable command to the flash to be completed
             */
            while (true)
            {
                /*
                 * Poll the status register of the device to determine
                 * when it completes, by sending a read status command
                 * and receiving the status byte
                 */
                FTstat = SPI_ReadWrite(fthandle, ref FlashStatus[0], ref ReadStatusCmd[0], 2, ref sizetransferred, 0x00000006);

                /*
                 * Wait for the transfer on the SPI bus to be complete
                 * before proceeding
                 */
                //while (FTstat==FT_STATUS.FT_OK);

                /*
                 * If the status indicates the write is done, then stop
                 * waiting; if a value of 0xFF in the status byte is
                 * read from the device and this loop never exits, the
                 * device slave select is possibly incorrect such that
                 * the device status is not being read
                 */
                if ((FlashStatus[1] & 0x02) == 0x02)
                {
                    break;
                }
            }

            /*
             * Setup the bulk erase or chip-erase command
             */
            WriteBuffer[COMMAND_OFFSET] = CHIP_ERASE_CMD;

            /*
             * Send the bulk erase command; no receive buffer is specified
             * since there is nothing to receive
             */
            FTstat = SPI_Write(fthandle, ref WriteBuffer[0], 1, ref sizetransferred, 0x00000006);

            //while (FTstat==FT_STATUS.FT_OK);

            /*
             * Wait for the erase command to the flash to be completed
             */
            while (true)
            {
                /*
                 * Poll the status register of the device to determine
                 * when it completes, by sending a read status command
                 * and receiving the status byte
                 */
                FTstat = SPI_ReadWrite(fthandle, ref FlashStatus[0], ref ReadStatusCmd[0], 2, ref sizetransferred, 0x00000006);

                /*
                 * Wait for the transfer on the SPI bus to be complete
                 * before proceeding
                 */
                //while (FTstat == FT_STATUS.FT_OK) ;

                /*
                 * If the status indicates the write is done, then stop
                 * waiting; if a value of 0xFF in the status byte is
                 * read from the device and this loop never exits, the
                 * device slave select is possibly incorrect such that
                 * the device status is not being read
                 */
                if ((FlashStatus[1] & 0x01) == 0)
                {
                    break;
                }
            }
            return FTstat;
        }
        FT_STATUS PAGE_PROGRAM(FT_Handle fthandle, byte[] data)
        {
            FT_STATUS status = FT_STATUS.FT_OK;
            byte[] FlashStatus = new byte[2];
            byte[] ReadStatusCmd = new byte[2];
            ReadStatusCmd[0] = READ_STATUS_CMD;
            ReadStatusCmd[1] = 0;  /* must send 2 bytes */
            byte[] temp = new byte[10];
            byte WriteEnableCmd = WRITE_ENABLE_CMD;
            bool writestatus = false;
            UInt32 sizetransferred = 0;
            try
            {
                status = SPI_IsBusy(fthandle, ref writestatus);
                if (writestatus == true)
                {
                    status = SPI_Write(fthandle, ref data[0], 260, ref sizetransferred, 0x000000006);
                    Thread.Sleep(1);
                    /* * Wait for the write command to the flash to be ,completed it takes some time for the data to be written*/
                    writestatus = false;
                    status = SPI_IsBusy(fthandle, ref writestatus);
                    status = SPI_Write(fthandle, ref WriteEnableCmd, 1, ref sizetransferred, 0x00000006);

                    //while (writestatus == true)
                    //{
                    //    /** Poll the status register of the flash to determine when it completes, by sending a read status command and receiving the status byte*/
                    //    status = SPI_ReadWrite(fthandle, ref  FlashStatus[0], ref ReadStatusCmd[0], 2, ref sizetransferred, 0x00000006);
                    //    writestatus = false;
                    //    status = SPI_IsBusy(fthandle, ref writestatus);
                    //    /** Wait for the transfer on the SPI bus to be complete before proceeding*/
                    //    /** If the status indicates the write is done, then stop waiting, if a value of 0xFF in the status byte is read from the device
                    //        and this loop never exits, the device slave select is possibly incorrect such that the device status is not being read*/
                    //    if ((FlashStatus[1] & 0x01) == 0)
                    //    {
                    //        break;
                    //    }
                    //}
                }
            }
            catch (Exception)
            {

                throw;
            }
            return status;
        }
        /******************************************************************************
        *
        *
        * This function writes to the  serial flash connected to the SPI interface.
        * The flash contains a 256 byte write buffer which can be filled and then a
        * write is automatically performed by the device.  All the data put into the
        * buffer must be in the same page of the device with page boundaries being on
        * 256 byte boundaries.
        *
        * @param	SpiPtr is a pointer to the SPI driver component to use.
        * @param	Address contains the address to write data to in the flash.
        * @param	ByteCount contains the number of bytes to write.
        * @param	Command is the command used to write data to the flash. SPI
        *		device supports only Page Program command to write data to the
        *		flash.
        *
        * @return	None.
        *
        * @note		None.
        *
        ******************************************************************************/
        void FlashWrite(FT_Handle fthandle, int Address, byte Command)
        {
            FT_STATUS status = FT_STATUS.FT_OK;
            byte[] flash8kfile = new byte[260];
            /////////////////////////////////////////////////////////////////////////////////
            FileStream fileStream = new FileStream(m_strFilename, FileMode.Open, FileAccess.Read);
            byte[] savefile = new byte[fileStream.Length + 4];
            savefile = ReadFile(m_strFilename);
            int filelen = (int)savefile.Length;
            /////////////////////////////////////////////////////////////////////////////////
            int j = 0;
            int address = Address;
            if (Command == WRITE_CMD)
            {
                status = WriteEnable(fthandle);
                //* Send the write enable command to the flash so that it can be written to, this needs to be sent as a seperate transfer before the write
                //* Wait for the transfer on the SPI bus to be complete proceeding
                while (filelen > 0)
                {

                    for (int i = 4; i < 260; i++, j++)
                    {
                        if (j == savefile.Length)
                        {
                            break;
                        }
                        flash8kfile[i] = savefile[j];
                    }
                    //* Setup the write command with the specified address and data for the flash
                    flash8kfile[COMMAND_OFFSET] = Command;
                    flash8kfile[ADDRESS_1_OFFSET] = (byte)((address & 0xFF0000) >> 16);
                    flash8kfile[ADDRESS_2_OFFSET] = (byte)((address & 0xFF00) >> 8);
                    flash8kfile[ADDRESS_3_OFFSET] = (byte)(address & 0xFF);
                    status = PAGE_PROGRAM(fthandle, flash8kfile);
                    /*Send the write command, address, and data to the flash to be written, no receive buffer is specified since there is nothing to receive*/
                    address += 256;
                    if (j == savefile.Length)
                    {
                        break;
                    }

                }

                status = WriteDisable(fthandle);
                //FlashRead(fthandle, 0, (UInt32)savefile.Length, READ_CMD);
                UInt32 ByteCount = 4096;
                UInt32 sizetransferred = 0;
                byte[] readfile = new byte[ByteCount + 4];
                byte[] writecmdbuffer = new byte[ByteCount + 4];
                byte[] readsave = new byte[savefile.Length];
                j = 0;
                for (int i = 0; i < savefile.Length; i += 4096)
                {
                    Address = i;
                    writecmdbuffer[COMMAND_OFFSET] = READ_CMD;
                    writecmdbuffer[ADDRESS_1_OFFSET] = (byte)((Address & 0xFF0000) >> 16);
                    writecmdbuffer[ADDRESS_2_OFFSET] = (byte)((Address & 0xFF00) >> 8);
                    writecmdbuffer[ADDRESS_3_OFFSET] = (byte)(Address & 0xFF);
                    /*
                     * Send the read command to the flash to read the specified  of bytes from the flash, send the read command and address receive the specified 
                     * number of bytes of data in the data */
                    status = SPI_ReadWrite(fthandle, ref readfile[0], ref writecmdbuffer[0], (ByteCount + OVERHEAD_SIZE), ref sizetransferred, 0x00000006);
                    for (int k = 0; k < 4096; k++, j++)
                    {

                        if (j == savefile.Length)
                        {
                            break;
                        }
                        readsave[k + i] = readfile[k + 4];
                    }
                }
                j = 0;
                for (int i = 0; i < savefile.Length; i++)
                {
                    if (readsave[i] == savefile[i])
                    {
                        j += 1;
                    }
                }
                if (j == savefile.Length)
                {
                    Console.WriteLine("Program Varifying Success");
                }
                else
                {
                    Console.WriteLine("Program written Failed");
                }
            }
        }
        /******************************************************************************
        *
        * This function reads from the  serial flash connected to the
        * SPI interface.
        *
        * @param	SpiPtr is a pointer to the SPI driver component to use.
        * @param	Address contains the address to read data from in the flash.
        * @param	ByteCount contains the number of bytes to read.
        * @param	Command is the command used to read data from the flash. SPI
        *		device supports one of the Read, Fast Read, Dual Read and Fast
        *		Read commands to read data from the flash.
        *
        * @return	None.
        *
        * @note		None.
        *
        ******************************************************************************/
        void FlashRead(FT_Handle fthandle, int Address, UInt32 ByteCount, byte Command)
        {
            UInt32 sizetransferred = 0;
            /* * Setup the write command with the specified address and data for the flash*/
            byte[] readfile = new byte[ByteCount + 4];
            byte[] writecmdbuffer = new byte[ByteCount + 4];
            writecmdbuffer[COMMAND_OFFSET] = Command;
            writecmdbuffer[ADDRESS_1_OFFSET] = (byte)((Address & 0xFF0000) >> 16);
            writecmdbuffer[ADDRESS_2_OFFSET] = (byte)((Address & 0xFF00) >> 8);
            writecmdbuffer[ADDRESS_3_OFFSET] = (byte)(Address & 0xFF);
            /*
             * Send the read command to the flash to read the specified  of bytes from the flash, send the read command and address receive the specified 
             * number of bytes of data in the data */
            FT_STATUS status = FT_STATUS.FT_OK;
            status = SPI_ReadWrite(fthandle, ref readfile[0], ref writecmdbuffer[0], (ByteCount + OVERHEAD_SIZE), ref sizetransferred, 0x00000006);

        }
        FT_STATUS phy_flash()
        {
            FT_Handle fthandle = 0;
            FT_STATUS FTstat = FT_STATUS.FT_OK;
            FT_DEVICE_LIST_INFO_NODE ftdevlist = new FT_DEVICE_LIST_INFO_NODE();
            UInt32 channels = 0;
            byte mReadId = 0;
            // reset the ftdi pins
            Cleanup_libMPSSE();
            Init_libMPSSE(); //initiate LibMpsse dll
            FTstat = SPI_GetNumChannels(ref channels);  // Get number of channels for FTDI chip FT2232H has 2 CHannels
            FTstat = SPI_OpenChannel(0, ref fthandle);
            Thread.Sleep(10);
            FTstat = SPI_CloseChannel(fthandle);
            Thread.Sleep(10);
            if (FTstat != FT_STATUS.FT_OK)
            {
                Cleanup_libMPSSE();
            }
            FTstat = SPI_GetChannelInfo(0, ref ftdevlist); // FTDI channel information
            if (FTstat != FT_STATUS.FT_OK)
            {
                Cleanup_libMPSSE();
            }
            FTstat = SPI_OpenChannel(0, ref fthandle);  // open channel 0 
            //Enable MPSSE mode
            //Read out the data from FT2232H receive buffer
            if (FTstat != FT_STATUS.FT_OK)
            {
                Cleanup_libMPSSE();
            }
            // SPI configurations with respect to radiobutton checked xo2,flash
            FTstat = flash8k(fthandle);
            Thread.Sleep(10);
            mReadId = FlashReadID(fthandle);
            Thread.Sleep(250);
            if ((mReadId == 32) || (mReadId == 22) || (mReadId == 186))
            {
                FTstat = FlashErase(fthandle);
                int j = 0;
                TestAddress = 0x0;
                FlashWrite(fthandle, 0, WRITE_CMD);
                Thread.Sleep(100);
                FTstat = SPI_CloseChannel(fthandle);
            }
            else
            {
                FTstat = SPI_CloseChannel(fthandle);
                Console.WriteLine("No Valid Device Found");
                FTstat = FT_STATUS.FT_EEPROM_WRITE_FAILED;
            }

            Cleanup_libMPSSE();
            return FTstat;

        }
        public SPIChannelConfig Configuration(FT_Handle fthandle, int bus)
        {
            SPIChannelConfig configuration = new SPIChannelConfig();
            try
            {
                SPIChannelConfig channelConf = new SPIChannelConfig();
                SPIChannelConfig Channelbus4 = new SPIChannelConfig();
                if (bus == 3)
                {

                    channelConf.ClockRate = 15000000;
                    channelConf.LatencyTimer = 5;
                    channelConf.ConfigOptions = (UInt32)(SPI_CONFIG_OPTION_CS.DBUS3) | (UInt32)(SPI_CONFIG_OPTION_MODE.MODE0) | (UInt32)(SPI_CONFIG_OPTION_CS_ACTIVE.LOW);
                    channelConf.Pins = 0xD8DB90DB; //final_val - final_dir - intit_val-intial_dir 1-output; 0-input
                    configuration = channelConf;


                }
                else if (bus == 4)
                {

                    Channelbus4.ClockRate = 15000000;
                    Channelbus4.LatencyTimer = 2;
                    Channelbus4.ConfigOptions = (UInt32)(SPI_CONFIG_OPTION_CS.DBUS4) | (UInt32)(SPI_CONFIG_OPTION_MODE.MODE0) | (UInt32)(SPI_CONFIG_OPTION_CS_ACTIVE.LOW);
                    Channelbus4.Pins = 0xD8DB08DB;
                    configuration = Channelbus4;

                }
                return configuration;
            }
            catch (Exception)
            {
                throw;
            }

        }
        private FT_STATUS flash8k(FT_Handle fthandle)
        {
            FT_STATUS FTstat = FT_STATUS.FT_OK;
            SPIChannelConfig Channelbus4;
            //Thread.Sleep(150);
            Channelbus4 = Configuration(fthandle, 4);
            FTstat = SPI_InitChannel(fthandle, ref Channelbus4);
            //Thread.Sleep(3);
            if (FTstat != FT_STATUS.FT_OK)
            {
                Cleanup_libMPSSE();
            }
            return FTstat;
        }




    }

    public class MyThread : Form1
    {
        USBDevice fx;
        CyFX3Device Fx3;
        Form1 Fr;
        FX3_FWDWNLOAD_MEDIA_TYPE MediaType;
        USBDeviceList cyFx3List;

        public MyThread(Form1 form, USBDevice Usb, FX3_FWDWNLOAD_MEDIA_TYPE enmMediaType)
        {
            fx = Usb;
            Fr = form;
            MediaType = enmMediaType;
        }

        [STAThread]
        public void DoWork()
        {// This method will be called when the thread is started.
            int timeoutCounter = 10; // 10 second time out counter
            Fx3 = null;
            cyFx3List = cyFx3.GetUsbDeviceListObject();
            while (true)
            {
                //Search for Boot Programmer
                System.Threading.Thread.Sleep(1000);
                //RefreshDeviceTree();

                //exit condition
                timeoutCounter--;
                if (timeoutCounter == 0)
                {
                    Fr.BootProgrammerDeviceNotFound();
                    return;
                }


                for (int i = 0; i < cyFx3List.Count; i++)
                {
                    Fx3 = cyFx3List[i] as CyFX3Device;
                    if ((Fx3 != null) && (Fx3.ProductID == 0x4720))
                    {
                        Fr.BootProgrammerDeviceFound();
                    }
                }
                if (Fx3 != null)
                    break; // break while loop

            }

            // Download the user IMG file firmware             
            Fr.DownloadUserImg(Fx3, MediaType, Fr);

        }


    }
}
