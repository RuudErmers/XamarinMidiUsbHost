using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Hardware.Usb;
using System.Threading.Tasks;
using DeviceConnection;
using System.Linq;
using System.Threading;

namespace CasioXWP1Controller.Droid
{
    public class CasioXWP1 : DeviceConnection.DeviceConnection
    {
        public CasioXWP1(UsbManager manager) : base()
        {
            this.manager = manager;
            state = 0;
            StartReadThread();
        }
        public override void Panic()
        {
            for (int i = 4; i < 7; i++)
                WriteCC(i, 123, 0);
        }
        public override void SetVolume(int id, int volume)
        {
            WriteCC(id, 7, (byte)volume);
        }
        public override void SetProgram(int midi, int program)
        {
            // todo: different for GM_Patch 
            if ((program & GM_PATCH) != 0)
                WritePRG(midi, (byte)(program & ~GM_PATCH));
            else
                WriteProgramSysEx(midi, program);
        }
        void NoteEvent(int midi, int data1, int data2)
        {
            Android.Util.Log.Info("midi", "Note Event, Midi=" + midi + " Key=" + data1 + " Velocity=" + data2);
        }
        void CCEvent(int midi, int data1, int data2)
        {
            Android.Util.Log.Info("midi", "CC Event, Midi=" + midi + " CC=" + data1 + " Value=" + data2);
            if (data1 == 7)
                OnReadVolume(midi, data2);
        }
        void ProgramEvent(int midi, int program)
        {
            OnReadProgram(midi, program | GM_PATCH);
            Android.Util.Log.Info("midi", "Program Change, Midi=" + midi + " Prgm=" + program);
        }
        byte[] Sysex = new byte[256];
        int SysExP = 0;
        void CheckSysEx()
        {
            const int midip = 16;
            const int prglo = 24;
            const int prghi = 25;
            int midi, plo, phi;
            if (SysExP != 27) return;
            if ((Sysex[5] == 1) && (Sysex[6] == 2))
            {
                midi = Sysex[midip];
                plo = Sysex[prglo];
                phi = Sysex[prghi];
                OnReadProgram(midi, (128 * phi + plo));
                Android.Util.Log.Info("midi", "SysEx PRG, Midi=" + midi + " Program=" + (128 * phi + plo));
            }
        }
        void AddSysExByte(byte b)
        {
          if (SysExP<255) Sysex[SysExP++] = b;
          if (b==247)
            {
                CheckSysEx();
                SysExP=0;
            }
        }
        void AddSysEx(int count, byte data0, byte data1, byte data2)
        {
            if (count > 0) AddSysExByte(data0);
            if (count > 1) AddSysExByte(data1);
            if (count > 2) AddSysExByte(data2);
        }
        void DoMidiInRaw(byte [] buffer, int Count)
        {
            int p = 0;
            while (p < Count)
            {
                switch (buffer[p])
                {
                    case 9: NoteEvent(buffer[p + 1] & 0xF, buffer[p + 2], buffer[p + 3]); p += 4; break;
                    case 11: CCEvent(buffer[p + 1] & 0xF, buffer[p + 2], buffer[p + 3]); p += 4; break;
                    case 12: ProgramEvent(buffer[p + 1] & 0xF, buffer[p + 2]); p += 3; break;
                    case 4: case 7: AddSysEx(3, buffer[p + 1], buffer[p + 2], buffer[p + 3]); p += 4; break;
                    case 5: AddSysEx(1, buffer[p + 1], 0, 0); p += 2; break;
                    case 6: AddSysEx(2, buffer[p + 1], buffer[p + 2], 0); p += 3; break;
                    default: p += Count; break;
                }
            }
        }
        bool threadRun;
        int eventCount = 0;
        private void StartReadThread()
        {
            Android.Util.Log.Info("midi", "Creating Thread");
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                threadRun = true;
                Android.Util.Log.Info("midi", "Running Thread");

                while (threadRun)
                {
                    if (state == 2)// some async read
                    {
                        if (eventCount<20) Android.Util.Log.Info("midi", "Start Reading");
                        int maxPacketSize = endpointIn.MaxPacketSize;
                        byte[] buffer = new byte[maxPacketSize];
                        try
                        {
                            int bytesRead = usbDeviceConnection.BulkTransfer(endpointIn, buffer, maxPacketSize, 500);
                            if (bytesRead > 0) DoMidiInRaw(buffer, bytesRead);
                            string s = "Read " + (eventCount++) + " Count: " + bytesRead + " Bytes:";
                            for (int i = 0; i < bytesRead; i++) s = s + "," + buffer[i];
                            if (eventCount < 20) Android.Util.Log.Info("midi", s);
                        }
                        catch { Android.Util.Log.Info("midi", "Exception"); }
                    }
                    else
                        Thread.Sleep(500);
                }
            }).Start();
        }

        ///////////////////////////////////////////////////////////////////////////
        UsbManager manager;
        UsbDevice usbDevice;
        UsbEndpoint endpointIn,endpointOut;
        UsbInterface usbInterface;
        UsbDeviceConnection usbDeviceConnection;
        const int MIDI_PRG = 0xC0;
        const int MIDI_CC = 0xB0;
        private int state = 0;  // 0 = not connected, 1 = connected, 2 = opened 
        private UsbDevice GetUsbDevice()
        {
            const int VENDOR_ID = 0x07CF;
            const int PRODUCT_ID = 0x6803;
            var matchingDevice = manager.DeviceList.FirstOrDefault(item => item.Value.VendorId == VENDOR_ID && item.Value.ProductId == PRODUCT_ID);
            Android.Util.Log.Info("midi", "matchingDevice:" + matchingDevice);
            // DeviceList is a dictionary with the port as the key, so pull out the device you want.  I save the port too
            return matchingDevice.Value;
        }
        public override void Connect()
        {
            state = 0;
            Android.Util.Log.Info("midi", "opening device");
            usbDevice = GetUsbDevice();
            if (usbDevice == null) return;
            state = 1;
            Android.Util.Log.Info("midi", "usbDevice:" + usbDevice + "Interfaces:" + usbDevice.InterfaceCount);
            // Get permission from the user to access the device (otherwise connection later will be null)
            if (!manager.HasPermission(usbDevice))
            {
                manager.RequestPermission(usbDevice, null);
            }
            return;
        }
        private bool validConnection()
        {
            if (state == 0) { Connect(); connected = false; return false; }
            if (state == 1) { OpenDevice(); }
            if (state != 2) { connected = false; return false; }
            if (GetUsbDevice() == null)
            {
                Android.Util.Log.Info("midi", "Valid Connection failed");
                state = 0;
                connected = false;
                return false;
            }
            connected = true;
            return true;
        }
        private bool WriteCC(int midi, byte data1, byte data2)
        {
            byte[] buffer = new byte[4];
            buffer[0] = 11;
            buffer[1] = (byte)(MIDI_CC + midi);
            buffer[2] = data1;
            buffer[3] = data2;
            return WriteBuffer(buffer);
        }
        private bool WritePRG(int midi, byte program)
        {
            byte[] buffer = new byte[3];
            buffer[0] = 12;
            buffer[1] = (byte)(MIDI_PRG + midi);
            buffer[2] = program;
            return WriteBuffer(buffer);
        }
        private bool WriteProgramSysEx(int midi, int program)
        {
            /*
                        byte[] buffer = new byte[3];
                        buffer[0] = 12;
                        buffer[1] = (byte)(MIDI_PRG + midi);
                        buffer[2] = program;
                        return WriteBuffer(buffer); */
            const int PCM_PATCH = 200;
            if ((midi != 0) && (program < PCM_PATCH)) return false;
            byte[] sysex0 = new byte[25] { 0xF0, 0x44, 0x16, 0x03, 0x7F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF7 };
            byte [] sysex1 = new byte[27] { 0xF0, 0x44, 0x16, 0x03, 0x7F, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x69, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xF7 };
            const int midip = 16;
            const int prglo = 24;
            const int prghi = 25;
            sysex1[midip] = (byte) midi;
            sysex1[prglo] = (byte)(program % 128);
            sysex1[prghi] = (byte)(program / 128);
            WriteSysEx(sysex0);
            return WriteSysEx(sysex1);
        }
        private bool WriteSysEx(byte[] buffer)
        { 
            int chunks, bytesleft;
            byte[] outbuf = new byte[4];
            chunks=(buffer.Length-1) / 3;
  // the last extra chunk must be one, two or three bytes
            bytesleft=buffer.Length - 3*chunks;
            for (int i = 0; i < chunks; i++)
            {
                outbuf[0] = 4;
                outbuf[1] = buffer[3 * i + 0];
                outbuf[2] = buffer[3 * i + 1];
                outbuf[3] = buffer[3 * i + 2];
                WriteBuffer(outbuf);
            }
            outbuf = new byte[bytesleft+1];
            outbuf[0]=(byte)(4+bytesleft);
            for (int i=0;i<bytesleft;i++)
                outbuf[i + 1]=buffer[3 * chunks + i];
            return WriteBuffer(outbuf);
        }

        private bool WriteBuffer(byte[] buffer)
        {
            if (!validConnection()) { return false; }
            int maxPacketSize = endpointOut.MaxPacketSize;
            Android.Util.Log.Info("midi", "maxPacketSize:" + maxPacketSize);
            int written = usbDeviceConnection.BulkTransfer(endpointOut, buffer, buffer.Length, 500);
            Android.Util.Log.Info("midi", "Bytes Written: " + written);
            return written == buffer.Length;
        }
        private void OpenDevice()
        {
            usbDeviceConnection = manager.OpenDevice(usbDevice);
            if (usbDeviceConnection == null)
            {
                Android.Util.Log.Info("midi", "usbDeviceConnection is null");
                return;
            }
            usbInterface = usbDevice.GetInterface(1);
            usbDeviceConnection.ClaimInterface(usbInterface, true);
            // interface 0: 0 endpoints
            Android.Util.Log.Info("midi", "usbInterface endpoints:" + usbInterface.EndpointCount);
            Android.Util.Log.Info("midi", "usbdeviceconnection:" + usbDeviceConnection);
            endpointOut = usbInterface.GetEndpoint(0);
            endpointIn = usbInterface.GetEndpoint(1);
            // 0: Xfer Control, 1: DirMask
            Android.Util.Log.Info("midi", "interface endpoint 0:" + endpointOut.Direction);
            Android.Util.Log.Info("midi", "interface endpoint 1:" + endpointIn.Direction);
            state = 2;
        }
        //        }


    }
}
