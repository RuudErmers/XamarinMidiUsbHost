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
    public class PageSong : PageSubPage
    {
        public class Preset
        {
            public int progBank,progNr; // progBank only used for WK1200
        }
        public class Song
        {
            public List<Preset> presetXP;
            public List<Preset> presetWK;
            public Song(string songName,List<Preset> presetXP, List<Preset> presetWK)
            {
                this.presetXP = presetXP;
                this.presetWK = presetWK;
                this.songName = songName;
            }
            public int curXPPatch, curWKPatch;
            public string songName;
        }
        const int BANK_W = 118;
        const int PROG_W = 49;
        const int Y_OFF = 60;
        private List<Song> songs = new List<Song>();
        Song curSong = null;

        public List<Song> Songs { get => songs;  }

        public PageSong(FormUI parent, int x, int y, int w, int h) : base(parent, x, y, w, h)
        {
            List<Preset> xp = new List<Preset>();
            xp.Add(new Preset { progNr = 322 });  // Trumpet
            xp.Add(new Preset { progNr = 323 });  // Mute Trumpet
            xp.Add(new Preset { progNr = 382});    // Steel Guitar
            xp.Add(new Preset { progNr = 296 });   // Violin
            xp.Add(new Preset { progNr = 476 });   // Telstar
            xp.Add(new Preset { progNr = 322 });   // Trumpet
            List<Preset> xw = new List<Preset>();
            xw.Add(new Preset { progBank = 1, progNr = 60 });  // Organ
            xw.Add(new Preset { progBank = 2, progNr = 4 });  // Mellotron Strings
            xw.Add(new Preset { progBank = 2, progNr = 18 });  // Mellotron Choir
            xw.Add(new Preset { progBank = 2, progNr = 84 });  // Mellotron Flute
            Songs.Add(new Song("The Cinema Show",xp, xw));
        }
        protected override void CreateUI(int w, int h)
        {

            for (int i = 0; i < 6; i++)
            {
                AddButton(20 + (BANK_W + 20) * i, Y_OFF + 30, BANK_W, 90, i, "");
                AddButton(20 + (BANK_W + 20) * i, Y_OFF + 210, BANK_W, 90, i+6, "");
            }
            AddLabel(20, Y_OFF-20, 200, 30, "Casio XW-P1");
            AddLabel(20, Y_OFF+160, 200, 30, "Casio WK1200");
        }
        protected override void ButtonClicked(int id)
        {
            if (id < 6) SelectXPPatch(id,true);
            else if (id<12) SelectWKPatch(id-6, true);
        }
        private void SelectXPPatch(int id, bool notify)
        {
            if (curSong == null) return;
            if (id < curSong.presetXP.Count)
            {
                curSong.curXPPatch = id;
                if (notify)
                {
                    int prg = curSong.presetXP[id].progNr;
                    parent.SetXPPatch(prg);
                }
            }
            for (int i = 0; i < 6; i++)
                ButtonSetSelected(i, i == id);                 
        }
        private void SelectWKPatch(int id, bool notify)
        {
            if (curSong == null) return;
            if (id < curSong.presetWK.Count)
            {
                curSong.curWKPatch = id;
                if (notify)
                {
                    parent.SetWKPatch(curSong.presetWK[id].progBank, curSong.presetWK[id].progNr);
                }
            }
            for (int i = 0; i < 6; i++)
                ButtonSetSelected(i+6, i == id);
        }

        private void ButtonSetSelected(int index, bool selected)
        {
            getButton(index).BackgroundColor = Color.FromHex(selected ? "#8DC53F" : "#32597E");
            getButton(index).TextColor = selected ? Color.Black : Color.White;
        }
        public override void ShowPage(int id)
        {
            if (id<Songs.Count)
            {
                curSong = Songs[id];
                for (int i=0;i<6;i++)
                {
                    Button btn = getButton(i);
                    if (i < curSong.presetXP.Count)
                    {
                        btn.IsVisible = true;
                        btn.Text = PatchNames.getProgramName(curSong.presetXP[i].progNr);
                    }
                    else btn.IsVisible = false;

                    btn = getButton(i+6);
                    if (i < curSong.presetWK.Count)
                    {
                        btn.IsVisible = true;
                        btn.Text = PatchNames.getProgramName(curSong.presetWK[i].progBank,curSong.presetWK[i].progNr);
                    }
                    else btn.IsVisible = false;
                }
                SelectXPPatch(curSong.curXPPatch, true);
                SelectWKPatch(curSong.curWKPatch, true);
            }
            base.ShowPage(id);
        }
    }
}