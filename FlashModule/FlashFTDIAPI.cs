using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using CyUSB;
using System.Windows.Forms;

namespace CypressAPI
{
    public class FlashFTDIAPI
    {
        public static USBDeviceList m_USBDevices;
        public static CyUSBDevice FxDev;
        public static CyUSBDevice m_CyUSBDevice;
        public static String Status;
        private static CyUSBEndPoint m_EndPoint;
        private static CyBulkEndPoint bulkEpt;

        public FlashFTDIAPI()
        {
            Status = "Not connected";
            Initialize();

        }

        public static bool Initialize()
        {
            bool retVal = true;
            m_USBDevices = new USBDeviceList(CyConst.DEVICES_CYUSB);
            m_USBDevices.DeviceAttached += new EventHandler(usbDevices_DeviceAttached);
            m_USBDevices.DeviceRemoved += new EventHandler(usbDevices_DeviceRemoved);
            // Get the first device having VendorID == 0x04B4 and ProductID == 0x8613

            for (int i = 0; i < m_USBDevices.Count; i++)
            {
                m_CyUSBDevice = m_USBDevices[i] as CyFX3Device;
                if ((m_CyUSBDevice != null) && (m_CyUSBDevice.ProductID == 0x4720))
                {
                    //Fr.BootProgrammerDeviceFound();
                    break;
                }
            }
            //m_CyUSBDevice = m_USBDevices[0x04B4, 0x00F0] as CyUSBDevice;

            if (m_CyUSBDevice != null)
            {
                Status = m_CyUSBDevice.FriendlyName + " connected.";
                m_EndPoint = m_CyUSBDevice.EndPoints[0];

            }
            else
            {
                m_EndPoint = null;
            }

            return retVal;
        }

        static void usbDevices_DeviceRemoved(object sender, EventArgs e)
        {
            USBEventArgs usbEvent = e as USBEventArgs;
            Status = usbEvent.FriendlyName + " removed.";
            m_EndPoint = null;
            //    MessageBox.Show("Removed");
        }
        static void usbDevices_DeviceAttached(object sender, EventArgs e)
        {
            try
            {
                USBEventArgs usbEvent = e as USBEventArgs;
                Status = usbEvent.Device.FriendlyName + " connected.";
                m_CyUSBDevice = usbEvent.Device as CyUSBDevice;
                if (m_CyUSBDevice.EndPointCount >= 1)
                {
                    m_EndPoint = m_CyUSBDevice.EndPoints[0];
                }
            }
            catch (Exception exc)
            {
                //MessageBox.Show(exc.Message, "Excepion Occurred After disconnecting");
            }

            //    MessageBox.Show("Connected");
        }

        public bool ReadFwID(byte devAddress, ushort startAddress, int numberOfBytesToRead, out byte[] data)
        {
            bool retVal = false;
            data = new byte[numberOfBytesToRead];

            CyControlEndPoint ctrlEpt = m_EndPoint as CyControlEndPoint;

            ctrlEpt.ReqCode = (byte)Convert.ToInt16(0xB0);
            ctrlEpt.Value = (ushort)devAddress;
            ctrlEpt.Index = (ushort)startAddress;
            ctrlEpt.Direction = CyConst.DIR_FROM_DEVICE;

            retVal = ctrlEpt.XferData(ref data, ref numberOfBytesToRead);

            return retVal;
        }
        public bool WriteSPI(byte reqCode, byte devAddress, ushort startAddress, byte[] data, int numberOfBytesWritten, bool ControlEndPoint = false)
        {
            bool retVal = false;
            if (ControlEndPoint == false)
            {
                if (reqCode == 0xD5)
                {
                    retVal = WriteFWCommand(data);
                }
                else if (reqCode == 0xDC)
                {
                    List<byte> listPayload = new List<byte>();
                    listPayload.Add(0xF2);
                    byte numberOfBytes = (byte)data.Length;
                    listPayload.Add(numberOfBytes);
                    listPayload.AddRange(data);

                    byte[] updBuffer = new byte[listPayload.Count];

                    for (int i = 0; i < listPayload.Count; i++)
                    {
                        updBuffer[i] = listPayload[i];
                    }

                    retVal = WriteFWCommand(updBuffer);
                }
            }
            else
            {
                CyControlEndPoint ctrlEpt = m_EndPoint as CyControlEndPoint;

                ctrlEpt.ReqCode = reqCode;
                ctrlEpt.Value = (ushort)devAddress;
                ctrlEpt.Index = (ushort)startAddress;
                ctrlEpt.Direction = CyConst.DIR_TO_DEVICE;
                retVal = ctrlEpt.XferData(ref data, ref numberOfBytesWritten);
            }

            return retVal;
        }

        public bool ReadSPI(byte reqCode, byte devAddress, ushort startAddress, int numberOfBytesToRead, ref byte[] data, bool ControlEndPOint = false)
        {
            bool retVal = false;
            if (ControlEndPOint == false)
                ReadFWCommand(ref data);
            else
            {
                data = new byte[numberOfBytesToRead];
                CyControlEndPoint ctrlEpt = m_EndPoint as CyControlEndPoint;

                ctrlEpt.TimeOut = 40000;
                ctrlEpt.ReqCode = reqCode;
                ctrlEpt.Value = (ushort)devAddress;
                ctrlEpt.Index = (ushort)startAddress;
                ctrlEpt.Direction = CyConst.DIR_FROM_DEVICE;

                retVal = ctrlEpt.XferData(ref data, ref numberOfBytesToRead);
            }

            return retVal;
        }

        public bool WriteFWCommand(byte[] buffer)
        {
            bool retValue = false;
            try
            {
                //int iBufSize = buffer.Length;
                int iBufSize = 512;//Keep Constant for any no. of bytes sent
                m_USBDevices = new USBDeviceList(CyConst.DEVICES_CYUSB);
                FxDev = m_USBDevices[0] as CyUSBDevice;
                bulkEpt = FxDev.BulkOutEndPt;

                retValue = bulkEpt.XferData(ref buffer, ref iBufSize, false);
            }
            catch (Exception ex)
            {
                retValue = false;
            }
            return retValue;
        }
        public bool ReadFWCommand(ref byte[] inBuffer)
        {
            bool retValue = false;
            try
            {
                int buffCnt = 512;
                m_USBDevices = new USBDeviceList(CyConst.DEVICES_CYUSB);
                FxDev = m_USBDevices[0] as CyUSBDevice;
                bulkEpt = FxDev.BulkOutEndPt;
                retValue = bulkEpt.XferData(ref inBuffer, ref buffCnt, false);

                Thread.Sleep(50);

                bulkEpt = FxDev.BulkInEndPt;
                if (bulkEpt != null)
                {
                    inBuffer = new byte[512];
                    retValue = bulkEpt.XferData(ref inBuffer, ref buffCnt, false);
                }

            }
            catch (Exception ex)
            {
                retValue = false;
            }
            return retValue;
        }

        public bool EraseFlash(byte devAddress, ushort startAddress, byte[] data, int numberOfBytesWritten)
        {
            bool retVal = false;

            CyControlEndPoint ctrlEpt = m_EndPoint as CyControlEndPoint;

            ctrlEpt.ReqCode = (byte)Convert.ToInt16(0xC4);
            ctrlEpt.Value = (ushort)devAddress;
            ctrlEpt.Index = (ushort)startAddress;
            ctrlEpt.Direction = CyConst.DIR_TO_DEVICE;

            retVal = ctrlEpt.XferData(ref data, ref numberOfBytesWritten);

            return retVal;
        }
        public bool CheckSPIBusStatus(byte devAddress, ushort startAddress, int numberOfBytesToRead, out byte[] data)
        {
            bool retVal = false;
            data = new byte[numberOfBytesToRead];

            CyControlEndPoint ctrlEpt = m_EndPoint as CyControlEndPoint;

            ctrlEpt.ReqCode = (byte)Convert.ToInt16(0xC4);
            ctrlEpt.Value = (ushort)devAddress;
            ctrlEpt.Index = (ushort)startAddress;
            ctrlEpt.Direction = CyConst.DIR_FROM_DEVICE;

            retVal = ctrlEpt.XferData(ref data, ref numberOfBytesToRead);

            return retVal;
        }

        public bool SetHPDOn(int duration)
        {
            bool retVal = false;
            int numberOfBytesToRead = 2;
            CyControlEndPoint ctrlEpt = m_EndPoint as CyControlEndPoint;
            byte[] data = new byte[numberOfBytesToRead];

            ctrlEpt.ReqCode = (byte)Convert.ToInt16(0xBC);
            ctrlEpt.Value = (ushort)duration;
            ctrlEpt.Index = (ushort)1;
            ctrlEpt.Direction = CyConst.TGT_OTHER;

            retVal = ctrlEpt.XferData(ref data, ref numberOfBytesToRead);

            return retVal;
        }

        public bool SetHPDOFF(int duration)
        {
            bool retVal = false;
            int numberOfBytesToRead = 2;
            CyControlEndPoint ctrlEpt = m_EndPoint as CyControlEndPoint;
            byte[] data = new byte[numberOfBytesToRead];

            ctrlEpt.ReqCode = (byte)Convert.ToInt16(0xBC);
            ctrlEpt.Value = (ushort)duration;
            ctrlEpt.Index = (ushort)0;
            ctrlEpt.Direction = CyConst.TGT_OTHER;

            retVal = ctrlEpt.XferData(ref data, ref numberOfBytesToRead);

            return retVal;
        }

        public CyUSBDevice GetUsbDeviceObject()
        {
            return m_CyUSBDevice;
        }
        public USBDeviceList GetUsbDeviceListObject()
        {
            return m_USBDevices;
        }
        public CyUSBEndPoint GetUsbControlEndPoint()
        {
            return m_EndPoint;
        }
    }
}
