using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace AppWK1200.Droid
{

    [Activity(
        Theme = "@android:style/Theme.Holo.Light",
        Label = "AppWK1200",
        Icon = "@drawable/icon",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)
        ]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.SetTheme(global::Android.Resource.Style.ThemeHoloLight);
            base.OnCreate(bundle);
            Android.Util.Log.Info("midi", "starting...:");
            this.Window.AddFlags(WindowManagerFlags.Fullscreen);
            this.Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            new DeviceOSAndroid((int)Resources.DisplayMetrics.WidthPixels, (int)Resources.DisplayMetrics.HeightPixels);
            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }
    }
    public class DeviceOSAndroid : DeviceConnection.DeviceOS
    {
        public DeviceOSAndroid(int width, int height) : base(width, height) { }
        public override void DoExit() { Android.OS.Process.KillProcess(Android.OS.Process.MyPid()); }
    }

}

