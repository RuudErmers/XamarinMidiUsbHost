using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Hardware.Usb;
using Android.Content;
using Android.Views;

namespace CasioXWP1Controller.Droid
{
    [Activity(Label = "CasioXWP1Controller", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(bundle);

            this.Window.AddFlags(WindowManagerFlags.Fullscreen);
            this.Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            new DeviceOSAndroid((int)Resources.DisplayMetrics.WidthPixels, (int)Resources.DisplayMetrics.HeightPixels);
            new CasioXWP1((UsbManager)GetSystemService(Context.UsbService));
            Android.Util.Log.Info("midi", "loading");
            LoadApplication(new App());
        }
    }
    public class DeviceOSAndroid : DeviceConnection.DeviceOS
    {
        public DeviceOSAndroid(int width, int height) : base(width, height) { }
        public  override void DoExit() { Android.OS.Process.KillProcess(Android.OS.Process.MyPid()); }
    }
}

