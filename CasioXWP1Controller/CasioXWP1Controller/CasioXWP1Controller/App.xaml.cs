using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviceConnection;
using Xamarin.Forms;
using Android.Util;

namespace CasioXWP1Controller
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Log.Info("midi", "mainpage go");
            MainPage = new CasioXWP1Controller.MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
