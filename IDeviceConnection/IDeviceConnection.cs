using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Threading;
using System.Diagnostics;

namespace DeviceConnection
{
    public delegate void OnDeviceData(int mask, int midi, int data);
    public class DeviceOS
    {
        private static DeviceOS deviceOS = null;
        public static DeviceOS GetInstance() { return deviceOS; }
        public static double SCALE_X = 1;
        public static double SCALE_Y = 1;
        public static int APP_WIDTH = 1;
        public static int APP_HEIGHT = 1;
        public static bool APP_IS_SMALL = true;
        public virtual void DoExit() {  }
        public DeviceOS(int width, int height)
        {
            deviceOS = this;
            if (width > height)
            {
                APP_WIDTH = width;
                APP_HEIGHT = height;
            }
            else
            {
                APP_WIDTH = height;
                APP_HEIGHT = width;
            }
            APP_IS_SMALL = APP_HEIGHT < 540;
            if (APP_IS_SMALL)
            {
                SCALE_X = 0.62;
                SCALE_Y = 0.65;
            }

        }
    }
    public class DeviceConnection
    {
        public static int NO_PATCH = 4096;
        public static int GM_PATCH = 2048;
        public static int PCM_PATCH = 200;
        public const int ON_PRG = 1;
        public const int ON_VOLUME = 2;
        public bool connected = false;
        OnDeviceData onDeviceData = null;
        public virtual void SetProgram(int midi, int program) { }
        public virtual void Panic() { }
        public virtual void Reload() { }
        public virtual void SetVolume(int midi, int volume) { }
        public virtual void Connect() { }
        public void SetOnDeviceData(OnDeviceData onDeviceData) { this.onDeviceData = onDeviceData; }
        public void OnReadProgram(int midi, int program)
        {
            if (onDeviceData != null)
                onDeviceData(ON_PRG, midi, program);
        }
        public void OnReadVolume(int midi, int volume)
        {
            if (onDeviceData != null)
                onDeviceData(ON_VOLUME, midi, volume);
        }
        private static DeviceConnection deviceConnection = null;
        public static DeviceConnection GetInstance() { return deviceConnection; }
        public DeviceConnection()
        {
            deviceConnection = this;
        }
    }
}