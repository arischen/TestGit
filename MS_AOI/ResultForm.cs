using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SQLHelper;

namespace MS_AOI
{
    public partial class ResultForm : Form
    {
        DataSet ds;
        int selectIndex = 0;
        public ResultForm()
        {
            InitializeComponent();
        }

        private void cbxSelectType_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectIndex = cbxSelectType.SelectedIndex;
            switch(selectIndex)
            {
                case 0:
                    grbDate.Enabled = true;
                    grbBarcode.Enabled = false;
                    break;
                case 1:
                    grbDate.Enabled = false ;
                    grbBarcode.Enabled = true ;
                    break;
            }
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
           switch(selectIndex)
            {
                case 0:
                    DateTime dtStart = dtpStartTime .Value;
                    DateTime dtEnd = dtpEndTime.Value;
                    ds = AOI_BLL.GetSQLByTime(dtStart, dtEnd);
                    break;
                case 1:
                    string barcode = tbBarcode.Text.Trim();
                    if (barcode == "") { MessageBox.Show("条码不能为空！！"); return; }
                    ds = AOI_BLL.GetSQLByBarcode(barcode);
                    break;
            }
            dgvResult.DataSource = ds.Tables[0];
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
