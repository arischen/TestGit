using LogicControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MS_AOI
{
    public partial class LoadTraySeqSelect : Form
    {
        private LogicModule logicModule;
        private Button[] btn_LoadGantrySeq;
        private Button[] btn_UnloadGantrySeq;

        public LoadTraySeqSelect(ref LogicModule logic)
        {
            InitializeComponent();
            logicModule = logic;
            btn_LoadGantrySeq = new Button[] { btn_LoadGantrySelect0,btn_LoadGantrySelect1,btn_LoadGantrySelect2,btn_LoadGantrySelect3,
                                               btn_LoadGantrySelect4,btn_LoadGantrySelect5,btn_LoadGantrySelect6,btn_LoadGantrySelect7,
                                               btn_LoadGantrySelect8,btn_LoadGantrySelect9,btn_LoadGantrySelect10,btn_LoadGantrySelect11,
                                               btn_LoadGantrySelect12,btn_LoadGantrySelect13,btn_LoadGantrySelect14};

            btn_UnloadGantrySeq = new Button[] { btn_UnloadGantrySelect0,btn_UnloadGantrySelect1,btn_UnloadGantrySelect2,btn_UnloadGantrySelect3,
                                                 btn_UnloadGantrySelect4,btn_UnloadGantrySelect5,btn_UnloadGantrySelect6,btn_UnloadGantrySelect7,
                                                 btn_UnloadGantrySelect8,btn_UnloadGantrySelect9,btn_UnloadGantrySelect10,btn_UnloadGantrySelect11,
                                                 btn_UnloadGantrySelect12,btn_UnloadGantrySelect13,btn_UnloadGantrySelect14};
        }

        private void LoadGantrySeqSelect(object sender, EventArgs e)
        {
            for (int i = 0; i < btn_LoadGantrySeq.Length; i++)
                btn_LoadGantrySeq[i].BackColor = SystemColors.Control;

            for (int i = 0; i < btn_LoadGantrySeq.Length; i++)
            {
                if (sender == btn_LoadGantrySeq[i])
                {
                    btn_LoadGantrySeq[i].BackColor = Color.LightGreen;
                    logicModule.CurLoadFullTraySeq = i;
                    return;
                }
            }
        }

        private void UnloadGantrySeqSelect(object sender, EventArgs e)
        {
            for (int i = 0; i < btn_UnloadGantrySeq.Length; i++)
                btn_UnloadGantrySeq[i].BackColor = SystemColors.Control;

            for (int i = 0; i < btn_UnloadGantrySeq.Length; i++)
            {
                if (sender == btn_UnloadGantrySeq[i])
                {
                    btn_UnloadGantrySeq[i].BackColor = Color.LightGreen;
                    logicModule.CurUnloadFullTraySeq = i;
                    return;
                }
            }
        }

        private void btn_Confirm_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
