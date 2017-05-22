using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace AppWK1200
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            FormUI.FormUI formUI = new FormUI.FormUI(new DeviceConnectionWK1200());
            formUI.CreateUI(WK1200Form, 0,0, DeviceConnection.DeviceOS.APP_WIDTH, Math.Min(600,DeviceConnection.DeviceOS.APP_HEIGHT));
            WK1200Form.BackgroundColor = Color.FromHex("#0F1620");
        }

    }
}
