using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using Android.Util;

namespace FormUI
{
    public class MyListViewItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public MyListViewItem(int id, string name, string comment) { Id = id; Name = name; Comment = comment; }
    }
    public class PageSubPage
    {
        protected Layout<View> layout;
        protected FormUI parent;
        List<Button> buttonList = new List<Button>();
        protected List<Slider> sliderList = new List<Slider>();
        private double ScaleX() { return DeviceConnection.DeviceOS.SCALE_X; }
        private double ScaleY() { return DeviceConnection.DeviceOS.SCALE_Y; }
        private Rectangle ScaledRectangle(int x, int y, int w, int h)
        {
            return new Rectangle(x * ScaleX(), y * ScaleY(), w * ScaleX(), h * ScaleY());
        }
        public PageSubPage(FormUI parent, int x, int y, int w, int h)
        {
            this.parent = parent;
            layout = new AbsoluteLayout();
            AbsoluteLayout.SetLayoutBounds(layout, ScaledRectangle(x, y, w, h));
            layout.BackgroundColor = Color.FromHex("#0F1620");
            parent.layout.Children.Add(layout);
            CreateUI(w, h);
        }
        protected void AddButton(int x, int y, int w, int h, int id, string caption)
        {
            Button button = new Button();
            button.CommandParameter = id;
            button.Text = caption;
            if (DeviceConnection.DeviceOS.APP_IS_SMALL)
                button.FontSize = 12;
            button.Clicked += Button_Clicked;
            AbsoluteLayout.SetLayoutBounds(button, ScaledRectangle(x, y,w,h));
            layout.Children.Add(button);
            buttonList.Add(button);
        }
        protected void AddListViewItem(ListView lstView,MyListViewItem item)
        {
            ObservableCollection<MyListViewItem> items = (ObservableCollection<MyListViewItem>)(lstView.ItemsSource);            
            items.Add(item);
            if (items.Count == 1)
                lstView.SelectedItem = item;
        }
        protected ListView AddListView(int x, int y, int w, int h)
        {
            ListView lstView = new ListView();
            lstView.ItemsSource = new ObservableCollection<MyListViewItem>();
            var cell = new DataTemplate(typeof(TextCell));
            cell.SetValue(TextCell.TextColorProperty, Color.Black);
            if (DeviceConnection.DeviceOS.APP_IS_SMALL)
                lstView.RowHeight = 30;
            //cell.SetValue(TextCell.TextProperty.)
            lstView.ItemTemplate = cell;
            lstView.BackgroundColor = Color.White;
            lstView.ItemTemplate.SetBinding(TextCell.TextProperty, "Name");
            lstView.ItemTemplate.SetBinding(TextCell.DetailProperty, "Comment");
            AbsoluteLayout.SetLayoutBounds(lstView, ScaledRectangle(x, y, w, h));
            lstView.ItemSelected += LstView_ItemSelected;
            layout.Children.Add(lstView);
            return lstView;
        }
        protected virtual void ListViewSelected(ListView lstView, MyListViewItem item) { }
        private void LstView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ListView lstView = (ListView)sender;
            if (e.SelectedItem == null)
            {
                return; //ItemSelected is called on deselection, which results in SelectedItem being set to null
            }
            ListViewSelected((ListView)sender,(MyListViewItem)e.SelectedItem);
        }

        protected void AddSlider(int x, int y, int w, int h)
        {
            Slider slider = new Slider();
            AbsoluteLayout.SetLayoutBounds(slider, ScaledRectangle(x, y, w, h));
            slider.ValueChanged += Slider_Changed;
            slider.Minimum = 0;
            slider.Maximum = 127;
            layout.Children.Add(slider);
            sliderList.Add(slider);
        }
        protected void AddLabel(int x, int y, int w, int h, string caption)
        {
            Label label = new Label();
            label.Text = caption;
            label.TextColor = Color.White;
            AbsoluteLayout.SetLayoutBounds(label, ScaledRectangle(x, y, w, h));
            layout.Children.Add(label);
        }
        protected Button getButton(int id)
        {
            for (int i = 0; i < buttonList.Count; i++)
            {
                int tag = (int)(buttonList[i].CommandParameter);
                if (tag == id) return buttonList[i];
            }
            return null;
        }
        protected void setSliderValue(int id, int value)
        {
            Log.Info("midi","SetSlider:"+ id +" "+sliderList[id].Value + " "+ value);
            if (Math.Abs(sliderList[id].Value - value) < 2) return;
            sliderList[id].Value = value;
        }
        protected virtual void SliderChanged(int id, int value) { }
        public virtual void HidePage() { layout.IsVisible = false; }
        public virtual void ShowPage(int id) { layout.IsVisible = true; }
        protected void Slider_Changed(object sender, EventArgs e)
        {
            Log.Info("midi", "UpdSlider"+ ((Slider)sender).Value+" "+e );

            for (int i = 0; i < 3; i++)
                    SliderChanged(i,(int)sliderList[i].Value);
        }
        protected virtual void ButtonClicked(int id) { }
        protected void Button_Clicked(object sender, EventArgs e)
        {
            ButtonClicked((int)(((Button)sender).CommandParameter));

        }
        protected virtual void CreateUI(int w, int h) { }



    }
}
