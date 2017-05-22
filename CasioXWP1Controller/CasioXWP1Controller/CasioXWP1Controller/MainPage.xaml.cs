using Android.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CasioXWP1Controller
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            Log.Info("midi", "mainpage new UI");
            FormUI.FormUI formUI = new FormUI.FormUI(DeviceConnection.DeviceConnection.GetInstance());
            formUI.CreateUI(WK1200Form, 0, 0, DeviceConnection.DeviceOS.APP_WIDTH, Math.Min(600,DeviceConnection.DeviceOS.APP_HEIGHT));
            WK1200Form.BackgroundColor = Color.FromHex("#0F1620");
        }
    }
}
