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
    public partial class ZoomChart : Form
    {
        private LogicModule logicModule;
        public string[] XLabel = new string[40]; 

        public delegate void UpdateVoidDelegate();
        public event UpdateVoidDelegate ResetDistributeChartSelectedFAISeq;

        public ZoomChart(ref LogicModule logic)
        {
            InitializeComponent();

            logicModule = logic;
            UpdateChartXYFixed();
        }

        public void UpdateZoomChart(int faiSeq)
        {
            if (logicModule.faiDistributeCountList.Count == 25)
            {
                //绑定数据
                cht_FAIGraph0.Series[0].Points.DataBindXY(XLabel, logicModule.faiDistributeCountList[faiSeq]);

                if (faiSeq <= 12)
                {
                    AddStripLine(11, 30);
                    for (int i = 0; i < 10; i++)
                    {
                        cht_FAIGraph0.Series[0].Points[i].Color = Color.Red;
                        cht_FAIGraph0.Series[0].Points[10 + i].Color = Color.Green;
                        cht_FAIGraph0.Series[0].Points[20 + i].Color = Color.Green;
                        cht_FAIGraph0.Series[0].Points[30 + i].Color = Color.Red;
                    }
                }
                else
                {
                    AddStripLine(1, 20);
                    for (int i = 0; i < 20; i++)
                    {
                        cht_FAIGraph0.Series[0].Points[i].Color = Color.Green;
                        cht_FAIGraph0.Series[0].Points[20 + i].Color = Color.Red;
                    }
                }
            }
        }

        private void AddStripLine(double offset1, double offset2)
        {
            cht_FAIGraph0.ChartAreas[0].AxisX.StripLines.Clear();

            StripLine stripline1 = new StripLine();
            stripline1.Interval = 0;
            stripline1.IntervalOffset = offset1;
            stripline1.StripWidth = 0.2;
            stripline1.BackColor = Color.Red;
            stripline1.BorderDashStyle = ChartDashStyle.Dash;
            cht_FAIGraph0.ChartAreas[0].AxisX.StripLines.Add(stripline1);

            StripLine stripline2 = new StripLine();
            stripline2.Interval = 0;
            stripline2.IntervalOffset = offset2;
            stripline2.StripWidth = 0.2;
            stripline2.BackColor = Color.Red;
            stripline2.BorderDashStyle = ChartDashStyle.Dash;
            cht_FAIGraph0.ChartAreas[0].AxisX.StripLines.Add(stripline2);
        }

        public void UpdateZoomChartTitle(int faiSeq)
        {
            cht_FAIGraph0.Titles.Clear();
            switch (faiSeq)
            {
                case 0:
                    cht_FAIGraph0.Titles.Add("FAI22柱状图数据");
                    break;
                case 1:
                    cht_FAIGraph0.Titles.Add("FAI130柱状图数据");
                    break;
                case 2:
                    cht_FAIGraph0.Titles.Add("FAI131柱状图数据");
                    break;
                case 3:
                    cht_FAIGraph0.Titles.Add("FAI133G1柱状图数据");
                    break;
                case 4:
                    cht_FAIGraph0.Titles.Add("FAI133G2柱状图数据");
                    break;
                case 5:
                    cht_FAIGraph0.Titles.Add("FAI133G3柱状图数据");
                    break;
                case 6:
                    cht_FAIGraph0.Titles.Add("FAI133G4柱状图数据");
                    break;
                case 7:
                    cht_FAIGraph0.Titles.Add("FAI133G6柱状图数据");
                    break;
                case 8:
                    cht_FAIGraph0.Titles.Add("FAI161柱状图数据");
                    break;
                case 9:
                    cht_FAIGraph0.Titles.Add("FAI162柱状图数据");
                    break;
                case 10:
                    cht_FAIGraph0.Titles.Add("FAI163柱状图数据");
                    break;
                case 11:
                    cht_FAIGraph0.Titles.Add("FAI165柱状图数据");
                    break;
                case 12:
                    cht_FAIGraph0.Titles.Add("FAI171柱状图数据");
                    break;
                case 13:
                    cht_FAIGraph0.Titles.Add("FAI135柱状图数据");
                    break;
                case 14:
                    cht_FAIGraph0.Titles.Add("FAI136柱状图数据");
                    break;
                case 15:
                    cht_FAIGraph0.Titles.Add("FAI139柱状图数据");
                    break;
                case 16:
                    cht_FAIGraph0.Titles.Add("FAI140柱状图数据");
                    break;
                case 17:
                    cht_FAIGraph0.Titles.Add("FAI151柱状图数据");
                    break;
                case 18:
                    cht_FAIGraph0.Titles.Add("FAI152柱状图数据");
                    break;
                case 19:
                    cht_FAIGraph0.Titles.Add("FAI155柱状图数据");
                    break;
                case 20:
                    cht_FAIGraph0.Titles.Add("FAI156柱状图数据");
                    break;
                case 21:
                    cht_FAIGraph0.Titles.Add("FAI157柱状图数据");
                    break;
                case 22:
                    cht_FAIGraph0.Titles.Add("FAI158柱状图数据");
                    break;
                case 23:
                    cht_FAIGraph0.Titles.Add("FAI160柱状图数据");
                    break;
                case 24:
                    cht_FAIGraph0.Titles.Add("FAI172柱状图数据");
                    break;
            }
        }

        private void UpdateChartXYFixed()
        {
            cht_FAIGraph0.Titles.Add("柱状图数据分析");
            //X轴标签间距
            cht_FAIGraph0.ChartAreas[0].AxisX.Interval = 1;
            cht_FAIGraph0.ChartAreas[0].AxisX.LabelStyle.IsStaggered = true;
            cht_FAIGraph0.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
            //cht_FAIGraph.ChartAreas[0].AxisX.LabelStyle.Angle = 0;
            cht_FAIGraph0.ChartAreas[0].AxisX.TitleFont = new Font("微软雅黑", 14f, FontStyle.Regular);
            cht_FAIGraph0.ChartAreas[0].AxisX.TitleForeColor = Color.Black;
            //X坐标轴颜色
            cht_FAIGraph0.ChartAreas[0].AxisX.LineColor = ColorTranslator.FromHtml("#38587a"); ;
            cht_FAIGraph0.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.Black;
            cht_FAIGraph0.ChartAreas[0].AxisX.LabelStyle.Font = new Font("微软雅黑", 10f, FontStyle.Regular);
            //X轴网络线条
            cht_FAIGraph0.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
            cht_FAIGraph0.ChartAreas[0].AxisX.MajorGrid.LineColor = ColorTranslator.FromHtml("#2c4c6d");
            //Y坐标轴颜色
            cht_FAIGraph0.ChartAreas[0].AxisY.LineColor = ColorTranslator.FromHtml("#38587a");
            cht_FAIGraph0.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.Black;
            cht_FAIGraph0.ChartAreas[0].AxisY.LabelStyle.Font = new Font("微软雅黑", 10f, FontStyle.Regular);
            //Y坐标轴标题
            cht_FAIGraph0.ChartAreas[0].AxisY.Title = "数量";
            cht_FAIGraph0.ChartAreas[0].AxisY.TitleFont = new Font("微软雅黑", 10f, FontStyle.Regular);
            cht_FAIGraph0.ChartAreas[0].AxisY.TitleForeColor = Color.Black;
            cht_FAIGraph0.ChartAreas[0].AxisY.TextOrientation = TextOrientation.Rotated270;
            cht_FAIGraph0.ChartAreas[0].AxisY.ToolTip = "数量";
            //Y轴网格线条
            cht_FAIGraph0.ChartAreas[0].AxisY.MajorGrid.Enabled = true;
            cht_FAIGraph0.ChartAreas[0].AxisY.MajorGrid.LineColor = ColorTranslator.FromHtml("#2c4c6d");

            cht_FAIGraph0.ChartAreas[0].AxisY2.LineColor = Color.Black;
            cht_FAIGraph0.ChartAreas[0].BackGradientStyle = GradientStyle.TopBottom;
            Legend legend = new Legend("legend");
            legend.Title = "legendTitle";

            cht_FAIGraph0.Series[0].XValueType = ChartValueType.String;  //设置X轴上的值类型
            cht_FAIGraph0.Series[0].Label = "#VAL";                //设置显示X Y的值    
            cht_FAIGraph0.Series[0].LabelForeColor = Color.Black;
            cht_FAIGraph0.Series[0].ToolTip = "#VALX:#VAL";     //鼠标移动到对应点显示数值
            cht_FAIGraph0.Series[0].ChartType = SeriesChartType.Column;    //图类型(柱状图)

            cht_FAIGraph0.Series[0].LegendText = legend.Name;
            cht_FAIGraph0.Series[0].IsValueShownAsLabel = true;
            cht_FAIGraph0.Series[0].LabelForeColor = Color.Black;
            cht_FAIGraph0.Series[0].CustomProperties = "DrawingStyle = Cylinder";
            cht_FAIGraph0.Legends.Add(legend);
            cht_FAIGraph0.Legends[0].Position.Auto = false;
        }

        private void cht_FAIGraph0_Click(object sender, EventArgs e)
        {
            ResetDistributeChartSelectedFAISeq();
            this.Hide();
        }
    }
}
