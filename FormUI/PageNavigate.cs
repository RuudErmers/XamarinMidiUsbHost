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
    class PageNavigate : PageSubPage
    {
        int curId = -1;
        ListView lstView;
        Picker picker;
        public PageNavigate(FormUI parent, int x, int y, int w, int h) : base(parent, x, y, w, h)
        {
        }
        protected override void CreateUI(int w, int h)
        {
            if (!DeviceConnection.DeviceOS.APP_IS_SMALL)
                AddListBox(0,0,w,h-20);
            else
            {
                picker = new Picker();
                AbsoluteLayout.SetLayoutBounds(picker, new Rectangle(0, 0, w / 2, 50));
                picker.Items.Add("Full Control");
                for (int i = 0; i < parent.pageSong.Songs.Count; i++)
                    picker.Items.Add(parent.pageSong.Songs[i].songName);
                picker.Items.Add("Close App");
                picker.Title = "Select:";

                picker.SelectedIndexChanged += PickerIndexChanged;
                picker.SelectedIndex = 0;
                picker.TextColor = Color.White;
                layout.Children.Add(picker);
                const int BANK_W = 118;
                const int PROG_W = 49;
                AddButton(20 + (BANK_W + 20) * 3, 0, BANK_W, PROG_W, 0, "Connect");
                AddButton(20 + (BANK_W + 20) * 4, 0, BANK_W, PROG_W, 1, "Panic");
                AddButton(20 + (BANK_W + 20) * 5, 0, BANK_W, PROG_W, 2, "Reload");
                ButtonSetSelected(0, false);
                ButtonSetSelected(1, false);
                ButtonSetSelected(2, false);
            }
        }
        public void SetConnected(bool connected)
        {
            if (!DeviceConnection.DeviceOS.APP_IS_SMALL) return;
            getButton(0).BackgroundColor = Color.FromHex(connected ? "#8DC53F" : "#E57263");
            getButton(0).Text = connected ? "Conn OK" : "No Conn";
        }
        public void PickerIndexChanged(object sender, EventArgs e)
            {
                if (picker.SelectedIndex == picker.Items.Count - 1)
                {
                    parent.DoExit();
                    return;
                }
                SelectIndex(picker.SelectedIndex);
            }
        private void SelectIndex(int id)
        {
            // hide page
            switch (curId)
            {
                case -1: parent.pageSong.HidePage(); break;
                case 0: parent.pageFullControl.HidePage(); break;
                default: parent.pageSong.HidePage(); break;
            }
            // show page
            curId = id;
            switch (curId)
            {
                case 0: parent.pageFullControl.ShowPage(curId); break;
                default: parent.pageSong.ShowPage(curId - 1); break;
            }
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
        protected override void ListViewSelected(ListView lstView, MyListViewItem item) 
        {
            SelectIndex(item.Id);
        }
        private void AddListBox(int x, int y, int w, int h)
            {
                lstView = AddListView(x, y, w, h);
                AddListViewItem(lstView, new MyListViewItem(0, "Full Control", ""));
                for (int i=0;i< parent.pageSong.Songs.Count;i++)
                  AddListViewItem(lstView, new MyListViewItem(i+1,parent.pageSong.Songs[i].songName, ""));
            }
        private void ButtonSetSelected(int index, bool selected)
        {
            getButton(index).BackgroundColor = Color.FromHex(selected ? "#8DC53F" : "#E57263");
            getButton(index).TextColor = selected ? Color.Black : Color.White;
        }
    }

 }
