using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using DeviceConnection;
using System.Threading.Tasks;
using Android.Util;
using System.Threading;

/*
 // sizes of my Androids
Samsung Galaxy 800 x 480 (but scaled)
Alcatel Pop5 800 x 480
Wonderland 1024 x 600
4.3 Tablet 480 x 236
Samsung Mini 240 x 320
Prestigo 1024 x 700 */
namespace FormUI
{
    public class State
    {
        public int bank;
        public int [] bankprogram = new int[6];
        public int[] subProgram = new int[3];
        public int[] subVolume = new int[3];
        public State()
        {
            bank = 0;
            for (int i = 0; i < 6; i++) bankprogram[i] = 0;
            for (int i = 0; i < 3; i++) subProgram[i] = 0;
            for (int i = 0; i < 3; i++) subVolume[i] = 0;
        }
    }
    public class Favorites
    {
        class Patch
        {
            public string patchname;
            public int[] volume = new int[3];
            public int[] program = new int[3];
            public Patch(string patchname, int program0, int program1, int program2, int volume0, int volume1, int volume2)
            {
                this.patchname = patchname;
                program[0] = program0;
                program[1] = program1;
                program[2] = program2;
                volume[0] = volume0;
                volume[1] = volume1;
                volume[2] = volume2;
            }
        }
        public static bool IsTriPatch(int bank, int program)
        {
            return ((bank==0) && (program< patches.Count));
        }
        public static string GetTriPatchName(int bank, int program)
            // pre: IsTriPatch !
        {
            return patches[program].patchname;
        }
        public static int[] GetTriPatchPrograms(int bank, int program)
        // pre: IsTriPatch !
        {
            return patches[program].program;
        }
        public static int[] GetTriPatchVolumes(int bank, int program)
        // pre: IsTriPatch !
        {
            return patches[program].volume;
        }
        private static List<Patch> patches = new List<Patch>();
        public Favorites()
        {
            if (patches.Count == 0)
            {
                patches.Add(new Patch("Pauls Patch",PatchNames.PCM_PATCH + 321, PatchNames.PCM_PATCH + 217, PatchNames.PCM_PATCH + 377, 101,79,96 ));
                patches.Add(new Patch("Favorite 1", PatchNames.PCM_PATCH + 7, PatchNames.PCM_PATCH + 15, PatchNames.PCM_PATCH + 31, 127, 64, 32));
                patches.Add(new Patch("Favorite 2", PatchNames.PCM_PATCH + 0, PatchNames.PCM_PATCH + 90, PatchNames.PCM_PATCH + 37, 127, 22, 74));
                patches.Add(new Patch("Favorite 3", PatchNames.PCM_PATCH + 0, PatchNames.PCM_PATCH + 116, PatchNames.PCM_PATCH + 37, 127, 36, 74));
                patches.Add(new Patch("Favorite 4", PatchNames.PCM_PATCH + 0, PatchNames.PCM_PATCH + 118, PatchNames.PCM_PATCH + 37, 127, 36, 74));
                patches.Add(new Patch("Favorite 5", PatchNames.PCM_PATCH + 306, PatchNames.PCM_PATCH + 297, PatchNames.PCM_PATCH + 295, 113, 108, 93));
            }
        }
    }
    public class FormUI 
    {
        public Layout<View> layout;
        public PageFullControl pageFullControl;
        public PageSong pageSong;
        public PageOptions pageOptions = null;
        PageNavigate pageNavigate;
        DeviceConnection.DeviceConnection deviceConnection;

        State state = new State();
        public FormUI(DeviceConnection.DeviceConnection deviceConnection)
        {
            this.deviceConnection = deviceConnection;
        }
        public void DoExit()
        {
            DeviceConnection.DeviceOS.GetInstance().DoExit();
        }
        Favorites favorites;
        public void CreateUI(Layout<View> layout, int x,int y,int w,int h)
        {
            this.layout = layout;
            favorites = new Favorites();
            if (!DeviceConnection.DeviceOS.APP_IS_SMALL)
            {
                pageFullControl = new PageFullControl(this, x + 180, y, w - 180, h);
                pageSong = new PageSong(this, x + 180, y, w - 180, h);
                pageNavigate = new PageNavigate(this, x, y, 180, h - 220);
                pageOptions = new PageOptions(this, x, y + h - 220, 180, 220);
            }
            else
            {
                pageFullControl = new PageFullControl(this, x , y+50, w , h-50);
                pageSong = new PageSong(this, x , y+50, w , h-50);
                pageNavigate = new PageNavigate(this, x, y, w, 50);
            //    pageOptions = new PageOptions(this, x, y + h+20 , w, 1);
            }
            deviceConnection.SetOnDeviceData(OnDeviceData);
            Changed();
        }
        public void Panic() { deviceConnection.Panic(); }
        public void Reload() { SetWKProgram(state.bankprogram[state.bank]); }
        public void SetSubVolume(int id, int volume)
        {
            state.subVolume[id] = volume;
            deviceConnection.SetVolume(4+id,volume);
            Changed();
        }
        public void SetWKProgram(int program)
        {
            state.bankprogram[state.bank] = program;
            if (Favorites.IsTriPatch(state.bank, program))
            {
                int [] prgms = Favorites.GetTriPatchPrograms(state.bank, program);
                for (int i=0;i<3;i++) state.subProgram[i] = prgms[i];
                int[] volumes = Favorites.GetTriPatchVolumes(state.bank, program);
                for (int i = 0; i < 3; i++) state.subVolume[i] = volumes[i];
            }
            else
            {
                int prgm = PatchNames.bankstarts[state.bank] + program;
                state.subProgram[0] = prgm;
                state.subProgram[1] = PatchNames.NO_PATCH;
                state.subProgram[2] = PatchNames.NO_PATCH;
                state.subVolume[0] = 100;
                state.subVolume[1] = 0;
                state.subVolume[2] = 0;
            }
            for (int i = 0; i < 3; i++)
                {
                    deviceConnection.SetVolume(4 + i, state.subVolume[i]);
                    deviceConnection.SetProgram(4 + i, state.subProgram[i]);
                }
            Changed();
        }
        public void SetWKBank(int bank)
        {
            state.bank = bank;
            SetWKProgram(state.bankprogram[state.bank]);
        }
        public void SetXPPatch(int program)
        {
            deviceConnection.SetProgram(0,program);
        }
        public void SetWKPatch(int bank,int program)
        {
            state.bankprogram[bank] = program;
            SetWKBank(bank); // also updates program 
        }        
        public void Connect() { deviceConnection.Connect(); Changed(); }
        private void OnDeviceData(int mask, int midi, int data)
        {
            switch (mask)
            {
                case DeviceConnection.DeviceConnection.ON_PRG:
                        if ((midi>=4) && (midi<=6))
                        {
                            state.subProgram[midi - 4] = data;
                            Changed();
                        }
                        break;
                case DeviceConnection.DeviceConnection.ON_VOLUME:
                    if ((midi >= 4) && (midi <= 6))
                    {
                        state.subVolume[midi - 4] = data;
                        Changed();
                    }
                    break;
            }
        }
        private void Changed()
        {
            pageFullControl.StateChanged(state);
            if (pageOptions!=null) pageOptions.SetConnected(deviceConnection.connected);
            if (pageNavigate != null) pageNavigate.SetConnected(deviceConnection.connected);
        }
    }
}
