using LogicControl;
using StructAssemble;
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
    public partial class TestCCDInfluence : Form
    {
        private LogicModule logicModule;
        private CCDUpdateStruct myCCDResult;

        public TestCCDInfluence(ref LogicModule logic)
        {
            InitializeComponent();
            logicModule = logic;
        }

        private void btn_CalcResult_Click(object sender, EventArgs e)
        {
            logicModule.isTestCCDInfluence = false;

            CalcAllIndex();

            logicModule.isTestCCDInfluence = true;
        }

        private void CalcAllIndex()
        {
            for (int i = 0; i < 20; i++)
            {
                dgv_TestCCDInfluence.Rows.Add();
                int n = dgv_TestCCDInfluence.Rows.Count;
                CCDTaskRun myCCDTaskRun = new CCDTaskRun(0, 0, 0, false, logicModule.testCCDPicPos);
                myCCDTaskRun.PicPos[0].X = myCCDTaskRun.PicPos[0].X + 0.005 * (i - 10);
                myCCDResult = logicModule.TransferCCDData(logicModule.testCCDRawData, myCCDTaskRun);

                dgv_TestCCDInfluence.Rows[n - 1].Cells[0].Value = myCCDTaskRun.PicPos[0].X.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[1].Value = myCCDTaskRun.PicPos[0].Y.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[2].Value = myCCDTaskRun.PicPos[1].X.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[3].Value = myCCDTaskRun.PicPos[1].Y.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[4].Value = myCCDTaskRun.PicPos[2].X.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[5].Value = myCCDTaskRun.PicPos[2].Y.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[6].Value = myCCDResult.fai22.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[7].Value = myCCDResult.fai161.ToString();
            }

            for (int i = 0; i < 20; i++)
            {
                dgv_TestCCDInfluence.Rows.Add();
                int n = dgv_TestCCDInfluence.Rows.Count;
                CCDTaskRun myCCDTaskRun = new CCDTaskRun(0, 0, 0, false, logicModule.testCCDPicPos);
                myCCDTaskRun.PicPos[0].Y = myCCDTaskRun.PicPos[0].Y + 0.005 * (i - 10);
                myCCDResult = logicModule.TransferCCDData(logicModule.testCCDRawData, myCCDTaskRun);

                dgv_TestCCDInfluence.Rows[n - 1].Cells[0].Value = myCCDTaskRun.PicPos[0].X.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[1].Value = myCCDTaskRun.PicPos[0].Y.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[2].Value = myCCDTaskRun.PicPos[1].X.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[3].Value = myCCDTaskRun.PicPos[1].Y.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[4].Value = myCCDTaskRun.PicPos[2].X.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[5].Value = myCCDTaskRun.PicPos[2].Y.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[6].Value = myCCDResult.fai22.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[7].Value = myCCDResult.fai161.ToString();
            }

            for (int i = 0; i < 20; i++)
            {
                dgv_TestCCDInfluence.Rows.Add();
                int n = dgv_TestCCDInfluence.Rows.Count;
                CCDTaskRun myCCDTaskRun = new CCDTaskRun(0, 0, 0, false, logicModule.testCCDPicPos);
                myCCDTaskRun.PicPos[1].X = myCCDTaskRun.PicPos[1].X + 0.005 * (i - 10);
                myCCDResult = logicModule.TransferCCDData(logicModule.testCCDRawData, myCCDTaskRun);

                dgv_TestCCDInfluence.Rows[n - 1].Cells[0].Value = myCCDTaskRun.PicPos[0].X.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[1].Value = myCCDTaskRun.PicPos[0].Y.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[2].Value = myCCDTaskRun.PicPos[1].X.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[3].Value = myCCDTaskRun.PicPos[1].Y.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[4].Value = myCCDTaskRun.PicPos[2].X.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[5].Value = myCCDTaskRun.PicPos[2].Y.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[6].Value = myCCDResult.fai22.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[7].Value = myCCDResult.fai161.ToString();
            }

            for (int i = 0; i < 20; i++)
            {
                dgv_TestCCDInfluence.Rows.Add();
                int n = dgv_TestCCDInfluence.Rows.Count;
                CCDTaskRun myCCDTaskRun = new CCDTaskRun(0, 0, 0, false, logicModule.testCCDPicPos);
                myCCDTaskRun.PicPos[1].Y = myCCDTaskRun.PicPos[1].Y + 0.005 * (i - 10);
                myCCDResult = logicModule.TransferCCDData(logicModule.testCCDRawData, myCCDTaskRun);

                dgv_TestCCDInfluence.Rows[n - 1].Cells[0].Value = myCCDTaskRun.PicPos[0].X.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[1].Value = myCCDTaskRun.PicPos[0].Y.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[2].Value = myCCDTaskRun.PicPos[1].X.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[3].Value = myCCDTaskRun.PicPos[1].Y.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[4].Value = myCCDTaskRun.PicPos[2].X.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[5].Value = myCCDTaskRun.PicPos[2].Y.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[6].Value = myCCDResult.fai22.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[7].Value = myCCDResult.fai161.ToString();
            }

            for (int i = 0; i < 20; i++)
            {
                dgv_TestCCDInfluence.Rows.Add();
                int n = dgv_TestCCDInfluence.Rows.Count;
                CCDTaskRun myCCDTaskRun = new CCDTaskRun(0, 0, 0, false, logicModule.testCCDPicPos);
                myCCDTaskRun.PicPos[2].X = myCCDTaskRun.PicPos[2].X + 0.005 * (i - 10);
                myCCDResult = logicModule.TransferCCDData(logicModule.testCCDRawData, myCCDTaskRun);

                dgv_TestCCDInfluence.Rows[n - 1].Cells[0].Value = myCCDTaskRun.PicPos[0].X.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[1].Value = myCCDTaskRun.PicPos[0].Y.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[2].Value = myCCDTaskRun.PicPos[1].X.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[3].Value = myCCDTaskRun.PicPos[1].Y.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[4].Value = myCCDTaskRun.PicPos[2].X.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[5].Value = myCCDTaskRun.PicPos[2].Y.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[6].Value = myCCDResult.fai22.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[7].Value = myCCDResult.fai161.ToString();
            }

            for (int i = 0; i < 20; i++)
            {
                dgv_TestCCDInfluence.Rows.Add();
                int n = dgv_TestCCDInfluence.Rows.Count;
                CCDTaskRun myCCDTaskRun = new CCDTaskRun(0, 0, 0, false, logicModule.testCCDPicPos);
                myCCDTaskRun.PicPos[2].Y = myCCDTaskRun.PicPos[2].Y + 0.005 * (i - 10);
                myCCDResult = logicModule.TransferCCDData(logicModule.testCCDRawData, myCCDTaskRun);

                dgv_TestCCDInfluence.Rows[n - 1].Cells[0].Value = myCCDTaskRun.PicPos[0].X.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[1].Value = myCCDTaskRun.PicPos[0].Y.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[2].Value = myCCDTaskRun.PicPos[1].X.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[3].Value = myCCDTaskRun.PicPos[1].Y.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[4].Value = myCCDTaskRun.PicPos[2].X.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[5].Value = myCCDTaskRun.PicPos[2].Y.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[6].Value = myCCDResult.fai22.ToString();
                dgv_TestCCDInfluence.Rows[n - 1].Cells[7].Value = myCCDResult.fai161.ToString();
            }

        }

        public void ShowInfo()
        {
            txt_C1XOriginalValue.Text = logicModule.testCCDPicPos[0].X.ToString();
            txt_C1YOriginalValue.Text = logicModule.testCCDPicPos[0].Y.ToString();
            txt_MXOriginalValue.Text = logicModule.testCCDPicPos[1].X.ToString();
            txt_MYOriginalValue.Text = logicModule.testCCDPicPos[1].Y.ToString();
            txt_C2XOriginalValue.Text = logicModule.testCCDPicPos[2].X.ToString();
            txt_C2YOriginalValue.Text = logicModule.testCCDPicPos[2].Y.ToString();
            txt_C1XSetValue.Text = logicModule.testCCDPicPos[0].X.ToString();
            txt_C1YSetValue.Text = logicModule.testCCDPicPos[0].Y.ToString();
            txt_MXSetValue.Text = logicModule.testCCDPicPos[1].X.ToString();
            txt_MYSetValue.Text = logicModule.testCCDPicPos[1].Y.ToString();
            txt_C2XSetValue.Text = logicModule.testCCDPicPos[2].X.ToString();
            txt_C2YSetValue.Text = logicModule.testCCDPicPos[2].Y.ToString();
        }

        private void TestCCDInfluence_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
