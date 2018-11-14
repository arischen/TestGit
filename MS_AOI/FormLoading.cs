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
    public partial class FormLoading : Form
    {
        private int count = 0;

        public FormLoading()
        {
            InitializeComponent();
        }

        private void timerLoading_Tick(object sender, EventArgs e)
        {
            progressBar1.Value = count % 100;
            count++;
            if (MainForm.bMainFormLoadDone)
            {
                this.Close();
                timerLoading.Enabled = false;
            }
        }
    }
}
