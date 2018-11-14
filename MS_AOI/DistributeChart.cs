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
using System.Windows.Forms.DataVisualization.Charting;

namespace MS_AOI
{
    public partial class DistributeChart : Form
    {
        private LogicModule logicModule;
        private Label[] LblFAISeg;
        private Label[] LblFAISegCount;
        private Chart[] ChtFAI;
        private ZoomChart zoomchartForm;
        private string[] XLabel = new string[40];
        public int selectedFAISeq = -1;

        public DistributeChart(ref LogicModule logic)
        {
            InitializeComponent();
            logicModule = logic;

            LblFAISeg = new Label[] { lbl_Distribute0, lbl_Distribute1, lbl_Distribute2, lbl_Distribute3, lbl_Distribute4, lbl_Distribute5, lbl_Distribute6, lbl_Distribute7,
                                      lbl_Distribute8, lbl_Distribute9, lbl_Distribute10, lbl_Distribute11, lbl_Distribute12, lbl_Distribute13, lbl_Distribute14, lbl_Distribute15,
                                      lbl_Distribute16, lbl_Distribute17, lbl_Distribute18, lbl_Distribute19, lbl_Distribute20, lbl_Distribute21, lbl_Distribute22, lbl_Distribute23,
                                      lbl_Distribute24, lbl_Distribute25, lbl_Distribute26, lbl_Distribute27, lbl_Distribute28, lbl_Distribute29, lbl_Distribute30, lbl_Distribute31,
                                      lbl_Distribute32, lbl_Distribute33, lbl_Distribute34, lbl_Distribute35, lbl_Distribute36, lbl_Distribute37, lbl_Distribute38, lbl_Distribute39};

            LblFAISegCount = new Label[] { lbl_DistributeCount0, lbl_DistributeCount1, lbl_DistributeCount2, lbl_DistributeCount3, lbl_DistributeCount4, lbl_DistributeCount5, lbl_DistributeCount6, lbl_DistributeCount7,
                                           lbl_DistributeCount8, lbl_DistributeCount9, lbl_DistributeCount10, lbl_DistributeCount11, lbl_DistributeCount12, lbl_DistributeCount13, lbl_DistributeCount14, lbl_DistributeCount15,
                                           lbl_DistributeCount16, lbl_DistributeCount17, lbl_DistributeCount18, lbl_DistributeCount19, lbl_DistributeCount20, lbl_DistributeCount21, lbl_DistributeCount22, lbl_DistributeCount23,
                                           lbl_DistributeCount24, lbl_DistributeCount25, lbl_DistributeCount26, lbl_DistributeCount27, lbl_DistributeCount28, lbl_DistributeCount29, lbl_DistributeCount30, lbl_DistributeCount31,
                                           lbl_DistributeCount32, lbl_DistributeCount33, lbl_DistributeCount34, lbl_DistributeCount35, lbl_DistributeCount36, lbl_DistributeCount37, lbl_DistributeCount38, lbl_DistributeCount39,};


            ChtFAI = new Chart[] { cht_FAIGraph0, cht_FAIGraph1, cht_FAIGraph2, cht_FAIGraph3, cht_FAIGraph4,
                                   cht_FAIGraph5, cht_FAIGraph6, cht_FAIGraph7, cht_FAIGraph8, cht_FAIGraph9,
                                   cht_FAIGraph10, cht_FAIGraph11, cht_FAIGraph12, cht_FAIGraph13, cht_FAIGraph14,
                                   cht_FAIGraph15, cht_FAIGraph16, cht_FAIGraph17, cht_FAIGraph18, cht_FAIGraph19,
                                   cht_FAIGraph20, cht_FAIGraph21, cht_FAIGraph22, cht_FAIGraph23, cht_FAIGraph24};

            for (int j = 0; j < 40; j++)
                XLabel[j] = " ";

            RefreshTimer.Start();
            UpdateMiniChartXY();
            UpdateMiniChartTitle();
            UpdateMiniChartStripLine();
            zoomchartForm = new ZoomChart(ref logic);
            zoomchartForm.StartPosition = FormStartPosition.Manual;
            zoomchartForm.Location = (Point)new Size(0, 24);
            zoomchartForm.Size = new Size((int)(System.Windows.Forms.Screen.GetWorkingArea(this).Width * 0.7), System.Windows.Forms.Screen.GetWorkingArea(this).Height - 24);

            zoomchartForm.ResetDistributeChartSelectedFAISeq += new ZoomChart.UpdateVoidDelegate(DistributeChartForm_ResetSelectedFAISeq);
            this.MaximizedBounds = Screen.PrimaryScreen.WorkingArea;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        }

        private void DistributeChartForm_ResetSelectedFAISeq()
        {
            selectedFAISeq = -1;
        }

        private void ShowZoomChart(object sender, EventArgs e)
        {
            for (int i = 0; i < 25; i++)
            {
                if (sender == ChtFAI[i])
                {
                    selectedFAISeq = i;
                    UpdateDistributeChartLabel();
                    zoomchartForm.UpdateZoomChartTitle(i);
                    zoomchartForm.UpdateZoomChart(i);
                    for (int j = 0; j < 40; j++)
                        LblFAISegCount[j].Text = logicModule.faiDistributeCountList[i][j].ToString();
                    break;
                }
            }

            zoomchartForm.ShowDialog();
        }

        private void AddStripLine(Chart myChart, double offset1, double offset2)
        {
            myChart.ChartAreas[0].AxisX.StripLines.Clear();

            StripLine stripline1 = new StripLine();
            stripline1.Interval = 0;
            stripline1.IntervalOffset = offset1;
            stripline1.StripWidth = 0.2;
            stripline1.BackColor = Color.Red;
            stripline1.BorderDashStyle = ChartDashStyle.Dash;
            myChart.ChartAreas[0].AxisX.StripLines.Add(stripline1);

            StripLine stripline2 = new StripLine();
            stripline2.Interval = 0;
            stripline2.IntervalOffset = offset2;
            stripline2.StripWidth = 0.2;
            stripline2.BackColor = Color.Red;
            stripline2.BorderDashStyle = ChartDashStyle.Dash;
            myChart.ChartAreas[0].AxisX.StripLines.Add(stripline2);
        }

        private void UpdateMiniChartXY()
        {
            for (int i = 0; i < 25; i++)
            {
                //X轴标签间距
                ChtFAI[i].ChartAreas[0].AxisX.Interval = 1;
                //X坐标轴颜色
                ChtFAI[i].ChartAreas[0].AxisX.LineColor = ColorTranslator.FromHtml("#38587a"); ;
                ChtFAI[i].ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.Black;
                //X轴网络线条
                ChtFAI[i].ChartAreas[0].AxisX.MajorGrid.Enabled = false;
                //Y坐标轴颜色
                ChtFAI[i].ChartAreas[0].AxisY.LineColor = ColorTranslator.FromHtml("#38587a");
                ChtFAI[i].ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.Black;
                //Y轴网格线条
                ChtFAI[i].ChartAreas[0].AxisY.MajorGrid.Enabled = false;
                Legend legend = new Legend("legend");
                legend.Title = "legendTitle";
                ChtFAI[i].Series[0].XValueType = ChartValueType.String;  //设置X轴上的值类型
                ChtFAI[i].Series[0].ChartType = SeriesChartType.Column;    //图类型(柱状图)
                ChtFAI[i].Series[0].Color = Color.Green;
                ChtFAI[i].Series[0].LegendText = legend.Name;
                ChtFAI[i].Series[0].LabelForeColor = Color.Black;
                ChtFAI[i].Series[0].CustomProperties = "DrawingStyle = Cylinder";
                ChtFAI[i].Legends.Add(legend);
                ChtFAI[i].Legends[0].Position.Auto = false;
            }
        }

        //更新右侧FAI的分段label内容
        private void UpdateDistributeChartLabel()
        {
            double[] myFAISeg = new double[41];

            lbl_FAIName.Text = ChtFAI[selectedFAISeq].Titles[0].Text;

            double Seg = (logicModule.ThrParam.thrInfo[selectedFAISeq].UpLimit - logicModule.ThrParam.thrInfo[selectedFAISeq].DownLimit) / 20.0;
            if (selectedFAISeq <= 12)
                myFAISeg[0] = logicModule.ThrParam.thrInfo[selectedFAISeq].DownLimit - 10 * Seg;
            else
                myFAISeg[0] = logicModule.ThrParam.thrInfo[selectedFAISeq].DownLimit;

            for (int i = 1; i < 41; i++)
                myFAISeg[i] = myFAISeg[i - 1] + Seg;

            for (int i = 0; i < 40; i++)
            {
                LblFAISeg[i].Text = myFAISeg[i].ToString("0.####") + "-" + myFAISeg[i + 1].ToString("0.####");
                zoomchartForm.XLabel[i] = LblFAISeg[i].Text;
            }
        }

        private void UpdateMiniChartStripLine()
        {
            for (int i = 0; i < 25; i++)
            {
                if (i <= 12)
                    AddStripLine(ChtFAI[i], 11, 30);
                else
                    AddStripLine(ChtFAI[i], 1, 20);
            }
        }

        private void UpdateMiniChartTitle()
        {
            ChtFAI[0].Titles.Add("FAI22柱状图数据");
            ChtFAI[1].Titles.Add("FAI130柱状图数据");
            ChtFAI[2].Titles.Add("FAI131柱状图数据");
            ChtFAI[3].Titles.Add("FAI133G1柱状图数据");
            ChtFAI[4].Titles.Add("FAI133G2柱状图数据");
            ChtFAI[5].Titles.Add("FAI133G3柱状图数据");
            ChtFAI[6].Titles.Add("FAI133G4柱状图数据");
            ChtFAI[7].Titles.Add("FAI133G6柱状图数据");
            ChtFAI[8].Titles.Add("FAI161柱状图数据");
            ChtFAI[9].Titles.Add("FAI162柱状图数据");
            ChtFAI[10].Titles.Add("FAI163柱状图数据");
            ChtFAI[11].Titles.Add("FAI165柱状图数据");
            ChtFAI[12].Titles.Add("FAI171柱状图数据");
            ChtFAI[13].Titles.Add("FAI135柱状图数据");
            ChtFAI[14].Titles.Add("FAI136柱状图数据");
            ChtFAI[15].Titles.Add("FAI139柱状图数据");
            ChtFAI[16].Titles.Add("FAI140柱状图数据");
            ChtFAI[17].Titles.Add("FAI151柱状图数据");
            ChtFAI[18].Titles.Add("FAI152柱状图数据");
            ChtFAI[19].Titles.Add("FAI155柱状图数据");
            ChtFAI[20].Titles.Add("FAI156柱状图数据");
            ChtFAI[21].Titles.Add("FAI157柱状图数据");
            ChtFAI[22].Titles.Add("FAI158柱状图数据");
            ChtFAI[23].Titles.Add("FAI160柱状图数据");
            ChtFAI[24].Titles.Add("FAI172柱状图数据");
        }

        //更新小图&大图
        public void UpdateMiniChart()
        {
            if (logicModule.faiDistributeCountList.Count == 25)
            {
                for (int i = 0; i < 25; i++)
                {
                    ChtFAI[i].Series[0].Points.DataBindXY(XLabel, logicModule.faiDistributeCountList[i]);
                }

                if (selectedFAISeq != -1)
                {
                    zoomchartForm.UpdateZoomChart(selectedFAISeq);//更新大图内容
                    for (int i = 0; i < 40; i++)//更新某个FAI的具体分布数值
                        LblFAISegCount[i].Text = logicModule.faiDistributeCountList[selectedFAISeq][i].ToString();
                }
            }
        }

        private void DistributeChart_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void btn_ResetDistribute_Click(object sender, EventArgs e)
        {
            logicModule.faiDistributeCountList.Clear();
            for (int i = 0; i < 25; i++)
                logicModule.faiDistributeCountList.Add(new int[40]);
        }


        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            UpdateMiniChart();
        }

        private void btn_Test_Click(object sender, EventArgs e)
        {

        }
    }
}
