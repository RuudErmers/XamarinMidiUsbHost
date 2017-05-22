using Xamarin.Forms;
using System;

namespace FormUI
{
    public class PageFullControl : PageSubPage
    {
        const int ID_BANK0 = 1;
        const int ID_PATCHES = 8;
        const int ID_FAST_PREV = 30;
        const int ID_FAST_NEXT = 31;
        const int ID_PREV = 32;
        const int ID_NEXT = 33;
        const int ID_PANIC = 34;
        const int ID_RELOAD = 35;
        const int ID_PROG = 36;
        const int ID_LAST = 39;
        const int BANK_W = 118;
        const int PROG_W = 49;
        const int Y_OFF = 10;
        int curBank = -1, curProgram = 0, programOffset = 0, patchButtons=0;
        int[] curVolume = new int[3];
        int[] curSubProgram = new int[3];
        public static readonly string[] Banknames = { "Favorites", "Pianos", "String/Brss", "Guitar/Bass", "Synth", "Various" };

        public PageFullControl(FormUI parent, int x, int y, int w, int h) : base(parent, x, y, w, h)
        {
        }
        protected override void CreateUI(int w, int h)
        {
            for (int i = 0; i < 6; i++)
            {
                AddButton(20 + (BANK_W + 20) * i, Y_OFF + 20, BANK_W, 90, ID_BANK0 + i, Banknames[i]);
                if (i != 5)
                {
                    AddButton(20 + (PROG_W + 20) + (BANK_W + 20) * i, Y_OFF + 140, BANK_W, 90, ID_PATCHES + i, "");
                    if (!DeviceConnection.DeviceOS.APP_IS_SMALL)
                        AddButton(20 + (PROG_W + 20) + (BANK_W + 20) * i, Y_OFF + 250, BANK_W, 90, ID_PATCHES + 5 + i, "");

                }
            }
            patchButtons = DeviceConnection.DeviceOS.APP_IS_SMALL ? 5 : 10;
            AddButton(2 + 20 + (PROG_W + 20) * 0, Y_OFF + 140, PROG_W, 90, ID_PREV, "<");
            AddButton(2 + 20 + (PROG_W + 20) * 11, Y_OFF + 140, PROG_W, 90, ID_NEXT, ">");
            getButton(ID_PREV).BackgroundColor = Color.FromHex("#CDE195");
            getButton(ID_NEXT).BackgroundColor = Color.FromHex("#CDE195");

            int y_off = Y_OFF + (DeviceConnection.DeviceOS.APP_IS_SMALL ? 0 : 120);
            for (int i = 0; i < 3; i++)
            {
                AddSlider(20, y_off + 270 + 50 * i, 512, PROG_W);
                AddButton(20 + 556, y_off + 270 + 50 * i, 256, PROG_W, ID_PROG + i, "Progje");
                ButtonSetSelected(ID_PROG + i, false);
            }
//            getButton(ID_FAST_PREV).BackgroundColor = Color.FromHex("#CDE195");
//            getButton(ID_FAST_NEXT).BackgroundColor = Color.FromHex("#CDE195");
        }
        protected override void ButtonClicked(int id)
        {
            if ((id >= ID_BANK0) && (id < ID_BANK0 + 6))
                OnBank(id - ID_BANK0);
            if ((id >= ID_PATCHES) && (id < ID_PATCHES + patchButtons))
                ReqProgram(programOffset + id - ID_PATCHES);
            switch (id)
            {
                case ID_PREV:
                    SetOffset(programOffset - patchButtons);
                    break;
                case ID_NEXT:
                    SetOffset(programOffset + patchButtons);
                    break;
                case ID_FAST_PREV:
                    SetOffset(programOffset - 10);
                    break;
                case ID_FAST_NEXT:
                    SetOffset(programOffset + 10);
                    break;
                case ID_PANIC:
                    parent.Panic();
                    break;
                case ID_RELOAD:
                    parent.Reload();

                    break;
            }
        }
        protected override void SliderChanged(int id, int value)
        {
            parent.SetSubVolume(id, value);
        }
        private void ReqProgram(int prgm)
        {
            if (prgm < 0) return;
            if (prgm > 99) return;
            parent.SetWKProgram(prgm);
        }
        private void OnBank(int bank)
        {
            parent.SetWKBank(bank);
        }
        private void setBankSelected(int bank, bool selected)
        {
            ButtonSetSelected(bank + ID_BANK0, selected);
        }
        private void updateProgram(int prg)
        {
            if (prg != curProgram)
            {
                curProgram = prg;
                SetOffset((curProgram / patchButtons) * patchButtons);
            }
        }
        private void SetOffset(int offset)
        {
            if (offset < 0) offset = 0;
            if (offset > 100- patchButtons) offset = 100 - patchButtons;
            programOffset = offset;
            ShowPatches();
        }
        private string SpaceName(string name)
        {
            for (int i = 0; i < name.Length-2; i++)
            {
                int m;
                if (i % 2 == 0) m = (name.Length - i) / 2; else m = (name.Length + i) / 2;
                if (Char.IsUpper(name[m]))
                {
                    string s1 = name.Substring(0, m - 1);
                    string s2 = name.Substring(m);
                    return s1 + " " + s2;
                }
            }
            return name;
        }
        private void ShowPatches()
        {
            for (int i = 0; i < patchButtons; i++)
            {
                string name = PatchNames.getProgramName(curBank, programOffset + i);
                name = SpaceName(name);
                getButton(ID_PATCHES + i).Text = name;
                ButtonSetSelected(ID_PATCHES + i, programOffset + i == curProgram);
            }
        }
        public void StateChanged(State state)
        {
            if (state.bank != curBank)
            {
                curBank = state.bank;
                curProgram = -1;
                for (int i = 0; i < 6; i++)
                    setBankSelected(i, i == curBank);
            }
            updateProgram(state.bankprogram[state.bank]);
            for (int i = 0; i < 3; i++)
            {
                updateSubProgram(i, state.subProgram[i]);
                updateVolume(i, state.subVolume[i]);
            }

        }
        private void updateVolume(int index, int volume)
        {
            if (curVolume[index] != volume)
            {
                curVolume[index] = volume;
                setSliderValue(index, volume);
            }
        }
        private void updateSubProgram(int index, int program)
        {
            if (curSubProgram[index] != program)
            {
                curSubProgram[index] = program;
                getButton(ID_PROG + index).Text = PatchNames.getProgramName(program);
            }
        }

        private void ButtonSetSelected(int index, bool selected)
        {
            Color _color = Color.FromHex(selected ? "#8DC53F" : "#32597E");
            getButton(index).BackgroundColor = _color;
            getButton(index).TextColor = selected ? Color.Black : Color.White;
        }
    }
 };
