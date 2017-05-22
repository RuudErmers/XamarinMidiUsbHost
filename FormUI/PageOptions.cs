using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Collections.ObjectModel;

namespace FormUI
{
    public class PageOptions : PageSubPage
    {
        public class Preset
        {
            public int progBank,progNr; // progBank only used for WK1200
        }

        public PageOptions(FormUI parent, int x, int y, int w, int h) : base(parent, x, y, w, h)
        {
        }
        public void SetConnected(bool connected)
        {
            getButton(0).BackgroundColor =  Color.FromHex(connected ? "#8DC53F" : "#E57263");
            getButton(0).Text = connected ? "Connected" : "No Connection";
        }
        protected override void CreateUI(int w, int h)
        {
            AddButton(10, 0, w-20,50,0, "Connect");
            AddButton(10, 55, w - 20, 50,1, "Panic");
            AddButton(10, 110, w - 20, 50,2, "Reload");
            AddButton(10, 165, w - 20, 50, 3, "Close App");  
            SetConnected(false);
        }
        protected override void ButtonClicked(int id)
        {
            switch (id)
            {
                case 0: parent.Connect(); break;
                case 1: parent.Panic(); break;
                case 2: parent.Reload(); break;
                case 3: parent.DoExit(); break;
            }
        }
    }
}