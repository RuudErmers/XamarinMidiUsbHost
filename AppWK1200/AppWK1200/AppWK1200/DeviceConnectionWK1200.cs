using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceConnection;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading;
using Android.Util;


namespace AppWK1200
{
    class DeviceConnectionWK1200 : DeviceConnection.DeviceConnection
    {
        public DeviceConnectionWK1200()
        {
            Xamarin.Forms.Device.StartTimer(new TimeSpan(0, 0, 0, 0,200), CheckAJAX);
        }
        public override void SetProgram(int midi,int program)
        {
            string s = string.Format("DM{0:x}{1:x2}{2:x2}{3:x2}{4:x2}", midi, 9, program % 128, 10, program / 128);
            ajax(s);
        }
        public override void Panic()
        {
            string s = string.Format("CM{0:x}{1:x2}{2:x2}", 0,13,0);
            ajax(s);
        }
        public override void Reload()
        {
            string s = string.Format("CM{0:x}{1:x2}{2:x2}", 0, 14, 0);
            ajax(s);
        }
        public override void SetVolume(int midi, int volume)
        {
            string s = string.Format("CM{0:x}{1:x2}{2:x2}", midi,7, volume);
            ajax(s);
        }
        public override void Connect()
        {
            ajax("nop");
        }
        public List<string> ajaxList = new List<string>();
        private int secCounter5=0; 
        public bool CheckAJAX()
        {
            lock(ajaxList)
            {
                if (ajaxList.Count>0)
                {
                    string url = ajaxList[0];
                    ajaxList.RemoveAt(0);
                    DoHttp(url);
                }
                else
                {
                    secCounter5++;
                    if (secCounter5 >= 10)
                    {
                        secCounter5 = 0;
                        DoHttp("nop");
                    }
                }
            }
            return true;
        }
        public void ajax(string url)
        {
            lock (ajaxList)
            {
                ajaxList.Add(url);
            }
        }
        public async Task<string> DoHttp(string request)
        {
            HttpClient hTTPClient = new HttpClient();
            string RestUrl = "http://192.168.4.1/" + request;
            Uri uri = new Uri(RestUrl);
            hTTPClient.Timeout = new TimeSpan(0, 0, 1);
            string result = "";
            Log.Info("midi", "http:"+ RestUrl);
            try
            {
                var response = await hTTPClient.GetAsync(uri);
                connected = response.IsSuccessStatusCode;
                if (response.IsSuccessStatusCode)
                    result = await response.Content.ReadAsStringAsync();
            }
            catch { }
            return result;
        }
    }
}
