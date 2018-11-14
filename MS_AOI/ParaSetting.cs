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
using XmlHelper;

namespace MS_AOI
{
    public partial class ParaSetting : Form
    {
        private LogicModule logicModule;
        private Button[] btn_LoadGantrySeq;
        private Button[] btn_UnloadGantrySeq;
        private Button[] btn_UnloadGantrySupplySeq;
        private int CurCalibLoadGantrySeq;
        private int CurCalibUnloadGantrySeq;
        private int CurCalibUnloadGantrySupplySeq;

        public ParaSetting(ref LogicModule logic)
        {
            InitializeComponent();
            logicModule = logic;

            btn_LoadGantrySeq = new Button[] { btn_LoadGantryPos0 , btn_LoadGantryPos1 , btn_LoadGantryPos2 , btn_LoadGantryPos3 , btn_LoadGantryPos4 ,
                                               btn_LoadGantryPos5 , btn_LoadGantryPos6 , btn_LoadGantryPos7 , btn_LoadGantryPos8 , btn_LoadGantryPos9 ,
                                               btn_LoadGantryPos10 , btn_LoadGantryPos11 , btn_LoadGantryPos12 , btn_LoadGantryPos13 , btn_LoadGantryPos14,
                                               btn_LoadGantryPos15 , btn_LoadGantryPos16 , btn_LoadGantryPos17 , btn_LoadGantryPos18 , btn_LoadGantryPos19 ,
                                               btn_LoadGantryPos20 , btn_LoadGantryPos21 , btn_LoadGantryPos22 , btn_LoadGantryPos23 , btn_LoadGantryPos24 ,
                                               btn_LoadGantryPos25 , btn_LoadGantryPos26 , btn_LoadGantryPos27 , btn_LoadGantryPos28 , btn_LoadGantryPos29};

            btn_UnloadGantrySeq = new Button[] { btn_UnloadGantryPos0 , btn_UnloadGantryPos1 , btn_UnloadGantryPos2 , btn_UnloadGantryPos3 , btn_UnloadGantryPos4 , btn_UnloadGantryPos5,
                                                 btn_UnloadGantryPos6 , btn_UnloadGantryPos7 , btn_UnloadGantryPos8 , btn_UnloadGantryPos9 , btn_UnloadGantryPos10 , btn_UnloadGantryPos11,
                                                 btn_UnloadGantryPos12 , btn_UnloadGantryPos13 , btn_UnloadGantryPos14 , btn_UnloadGantryPos15 , btn_UnloadGantryPos16 , btn_UnloadGantryPos17};

            btn_UnloadGantrySupplySeq = new Button[] {btn_CalibSupplyRegion1_0,btn_CalibSupplyRegion1_1,btn_CalibSupplyRegion1_2,btn_CalibSupplyRegion1_3,
                                                    btn_CalibSupplyRegion1_4,btn_CalibSupplyRegion1_5,btn_CalibSupplyRegion1_6,btn_CalibSupplyRegion1_7,
                                                    btn_CalibSupplyRegion2_0,btn_CalibSupplyRegion2_1,btn_CalibSupplyRegion2_2,btn_CalibSupplyRegion2_3,
                                                    btn_CalibSupplyRegion2_4,btn_CalibSupplyRegion2_5,btn_CalibSupplyRegion2_6,btn_CalibSupplyRegion2_7 };

        }

        private void ParaSetting_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void ParaSetting_Load(object sender, EventArgs e)
        {
            InitSysParaUI();
            ShowSysPara(logicModule.systemParam);
        }

        private void btn_CalibLoadGantrySel_Click(object sender, EventArgs e)
        {
            for (int j = 0; j < btn_LoadGantrySeq.Length; j++)
                btn_LoadGantrySeq[j].BackColor = SystemColors.Control;


            for (int i = 0; i < btn_LoadGantrySeq.Length; i++)
            {
                if (sender == btn_LoadGantrySeq[i])
                {
                    btn_LoadGantrySeq[i].BackColor = Color.LightGreen;
                    CurCalibLoadGantrySeq = i;
                    lbl_CalibLoadGantrySeq.Text = CurCalibLoadGantrySeq.ToString();
                    lbl_CalibLoadGantryPosX.Text = logicModule.LoadGantryMotionPos.posInfo[i].XPos.ToString();
                    lbl_CalibLoadGantryPosY.Text = logicModule.LoadGantryMotionPos.posInfo[i].YPos.ToString();
                    return;
                }
            }
        }

        private void btn_CalibUnloadGantrySel_Click(object sender, EventArgs e)
        {
            for (int j = 0; j < btn_UnloadGantrySeq.Length; j++)
                btn_UnloadGantrySeq[j].BackColor = SystemColors.Control;

            for (int i = 0; i < btn_UnloadGantrySeq.Length; i++)
            {
                if (sender == btn_UnloadGantrySeq[i])
                {
                    btn_UnloadGantrySeq[i].BackColor = Color.LightGreen;
                    CurCalibUnloadGantrySeq = i;
                    lbl_CalibUnloadGantrySeq.Text = CurCalibUnloadGantrySeq.ToString();
                    lbl_CalibUnloadGantryPosX.Text = logicModule.UnloadGantryMotionPos.posInfo[i].XPos.ToString();
                    lbl_CalibUnloadGantryPosY.Text = logicModule.UnloadGantryMotionPos.posInfo[i].YPos.ToString();
                    return;
                }
            }
        }

        private void btn_CalibUnloadGantrySupplySel_Click(object sender, EventArgs e)
        {
            for (int j = 0; j < btn_UnloadGantrySupplySeq.Length; j++)
                btn_UnloadGantrySupplySeq[j].BackColor = SystemColors.Control;

            for (int i = 0; i < btn_UnloadGantrySupplySeq.Length; i++)
            {
                if (sender == btn_UnloadGantrySupplySeq[i])
                {
                    btn_UnloadGantrySupplySeq[i].BackColor = Color.LightGreen;
                    CurCalibUnloadGantrySupplySeq = i;
                    lbl_CalibUnloadGantrySupplySeq.Text = CurCalibUnloadGantrySupplySeq.ToString();
                    lbl_CalibUnloadGantrySupplyPosX.Text = logicModule.UnloadGantrySupplyMotionPos.posInfo[i].XPos.ToString();
                    lbl_CalibUnloadGantrySupplyPosY.Text = logicModule.UnloadGantrySupplyMotionPos.posInfo[i].YPos.ToString();
                    return;
                }
            }
        }

        public void UpdateMotionStatus(object sender)
        {
            if (!this.InvokeRequired)
            {
                UpdateInfo curInfo = (UpdateInfo)sender;

                txt_CalibLoadGantryXPos.Text = curInfo.CurAxisPos[8].ToString();
                txt_CalibLoadGantryYPos.Text = curInfo.CurAxisPos[9].ToString();
                txt_CalibUnloadGantryXPos.Text = curInfo.CurAxisPos[10].ToString();
                txt_CalibUnloadGantryYPos.Text = curInfo.CurAxisPos[11].ToString();
                txt_CalibUnloadGantrySupplyXPos.Text = curInfo.CurAxisPos[10].ToString();
                txt_CalibUnloadGantrySupplyYPos.Text = curInfo.CurAxisPos[11].ToString();
                txt_CalibSuckAxisPos.Text = curInfo.CurAxisPos[5].ToString();

                if (rbt_CalibLoadNullAxis.Checked)
                    txt_CalibTrayZPos.Text = curInfo.CurAxisPos[12].ToString();
                if (rbt_CalibLoadFullAxis.Checked)
                    txt_CalibTrayZPos.Text = curInfo.CurAxisPos[13].ToString();
                if (rbt_CalibUnloadNullAxis.Checked)
                    txt_CalibTrayZPos.Text = curInfo.CurAxisPos[14].ToString();
                if (rbt_CalibUnloadFullAxis.Checked)
                    txt_CalibTrayZPos.Text = curInfo.CurAxisPos[15].ToString();
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(UpdateMotionStatus), sender);
            }
        }

        private void btn_SaveLoadGantryPos_Click(object sender, EventArgs e)
        {
            logicModule.LoadGantryMotionPos.posInfo[CurCalibLoadGantrySeq].XPos = Convert.ToDouble(txt_CalibLoadGantryXPos.Text);
            logicModule.LoadGantryMotionPos.posInfo[CurCalibLoadGantrySeq].YPos = Convert.ToDouble(txt_CalibLoadGantryYPos.Text);

            if (XmlSerializerHelper.WriteXML((object)logicModule.LoadGantryMotionPos, logicModule.pathLoadGantryMotionPos, typeof(MotionPos)))
                MessageBox.Show("点位更新成功");
            else
                MessageBox.Show("点位更新失败");
        }

        private void btn_CalibLoadGantryPosMove_Click(object sender, EventArgs e)
        {
            if (logicModule.loadgantrymovesafesignal)
            {
                double posx = Convert.ToDouble(lbl_CalibLoadGantryPosX.Text);
                double posy = Convert.ToDouble(lbl_CalibLoadGantryPosY.Text);
                logicModule.AbsMove(logicModule.LogicConfigValue.ECATAxis[0], posx);
                logicModule.AbsMove(logicModule.LogicConfigValue.ECATAxis[1], posy);
            }
            else
            {
                MessageBox.Show("请确认上料龙门的所有气缸都已缩回");
            }
        }

        private void btn_CalibUnloadGantryPosMove_Click(object sender, EventArgs e)
        {
            if (logicModule.unloadgantrymovesafesignal)
            {
                double posx = Convert.ToDouble(lbl_CalibUnloadGantryPosX.Text);
                double posy = Convert.ToDouble(lbl_CalibUnloadGantryPosY.Text);
                logicModule.AbsMove(logicModule.LogicConfigValue.ECATAxis[2], posx);
                logicModule.AbsMove(logicModule.LogicConfigValue.ECATAxis[3], posy);
            }
            else
            {
                MessageBox.Show("请确认下料龙门的所有气缸都已缩回");
            }
        }

        private void btn_CalibUnloadGantrySupplyPosMove_Click(object sender, EventArgs e)
        {
            if (logicModule.unloadgantrymovesafesignal)
            {
                double posx = Convert.ToDouble(lbl_CalibUnloadGantrySupplyPosX.Text);
                double posy = Convert.ToDouble(lbl_CalibUnloadGantrySupplyPosY.Text);
                logicModule.AbsMove(logicModule.LogicConfigValue.ECATAxis[2], posx);
                logicModule.AbsMove(logicModule.LogicConfigValue.ECATAxis[3], posy);
            }
            else
            {
                MessageBox.Show("请确认下料龙门的所有气缸都已缩回");
            }
        }

        private void btn_CalibLoadGantryPos_MouseDown(object sender, MouseEventArgs e)
        {
            if(logicModule.loadgantrymovesafesignal==false)
                MessageBox.Show("请确认上料龙门的所有气缸都已缩回");

            if (sender == btn_CalibLoadGantryXPositive)
            {
                Axis curAxis = logicModule.LogicConfigValue.ECATAxis[0];
                if (rbt_CalibLoadGantryJog.Checked)
                {
                    if (btn_CalibLoadGantryXSpeed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    logicModule.StartJog(curAxis, 0);
                }
                else if (rbt_CalibLoadGantryAbs.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibLoadGantryXStep.Text);
                    logicModule.AbsMove(curAxis, Pos);
                }
                else if (rbt_CalibLoadGantryRel.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibLoadGantryXStep.Text);
                    logicModule.RelMove(curAxis, Pos);
                }
            }
            else if (sender == btn_CalibLoadGantryXNegative)
            {
                Axis curAxis = logicModule.LogicConfigValue.ECATAxis[0];
                if (rbt_CalibLoadGantryJog.Checked)
                {
                    if (btn_CalibLoadGantryXSpeed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    logicModule.StartJog(curAxis, 1);
                }
                else if (rbt_CalibLoadGantryAbs.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibLoadGantryXStep.Text);
                    logicModule.AbsMove(curAxis, Pos);
                }
                else if (rbt_CalibLoadGantryRel.Checked)
                {
                    double Pos = -1.0 * Convert.ToDouble(txt_CalibLoadGantryXStep.Text);
                    logicModule.RelMove(curAxis, Pos);
                }
            }
            else if (sender == btn_CalibLoadGantryYPositive)
            {
                Axis curAxis = logicModule.LogicConfigValue.ECATAxis[1];
                if (rbt_CalibLoadGantryJog.Checked)
                {
                    if (btn_CalibLoadGantryYSpeed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    logicModule.StartJog(curAxis, 0);
                }
                else if (rbt_CalibLoadGantryAbs.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibLoadGantryYStep.Text);
                    logicModule.AbsMove(curAxis, Pos);
                }
                else if (rbt_CalibLoadGantryRel.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibLoadGantryYStep.Text);
                    logicModule.RelMove(curAxis, Pos);
                }
            }
            else if (sender == btn_CalibLoadGantryYNegative)
            {
                Axis curAxis = logicModule.LogicConfigValue.ECATAxis[1];
                if (rbt_CalibLoadGantryJog.Checked)
                {
                    if (btn_CalibLoadGantryYSpeed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    logicModule.StartJog(curAxis, 1);
                }
                else if (rbt_CalibLoadGantryAbs.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibLoadGantryYStep.Text);
                    logicModule.AbsMove(curAxis, Pos);
                }
                else if (rbt_CalibLoadGantryRel.Checked)
                {
                    double Pos = -1.0 * Convert.ToDouble(txt_CalibLoadGantryYStep.Text);
                    logicModule.RelMove(curAxis, Pos);
                }
            }
        }

        private void btn_CalibLoadGantryPos_MouseUp(object sender, MouseEventArgs e)
        {
            if (rbt_CalibLoadGantryJog.Checked)
            {
                logicModule.StopJog(logicModule.LogicConfigValue.ECATAxis[0]);
                logicModule.StopJog(logicModule.LogicConfigValue.ECATAxis[1]);
            }
        }

        private void btn_CalibUnloadGantryPos_MouseUp(object sender, MouseEventArgs e)
        {
            if (rbt_CalibUnloadGantryJog.Checked)
            {
                logicModule.StopJog(logicModule.LogicConfigValue.ECATAxis[2]);
                logicModule.StopJog(logicModule.LogicConfigValue.ECATAxis[3]);
            }
        }

        private void btn_CalibUnloadGantryPos_MouseDown(object sender, MouseEventArgs e)
        {
            if (logicModule.unloadgantrymovesafesignal == false)
                MessageBox.Show("请确认下料龙门的所有气缸都已缩回");

            if (sender == btn_CalibUnloadGantryXPositive)
            {
                Axis curAxis = logicModule.LogicConfigValue.ECATAxis[2];
                if (rbt_CalibUnloadGantryJog.Checked)
                {
                    if (btn_CalibUnloadGantryXSpeed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    logicModule.StartJog(curAxis, 0);
                }
                else if (rbt_CalibUnloadGantryAbs.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibUnloadGantryXStep.Text);
                    logicModule.AbsMove(curAxis, Pos);
                }
                else if (rbt_CalibUnloadGantryRel.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibUnloadGantryXStep.Text);
                    logicModule.RelMove(curAxis, Pos);
                }
            }
            else if (sender == btn_CalibUnloadGantryXNegative)
            {
                Axis curAxis = logicModule.LogicConfigValue.ECATAxis[2];
                if (rbt_CalibUnloadGantryJog.Checked)
                {
                    if (btn_CalibUnloadGantryXSpeed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    logicModule.StartJog(curAxis, 1);
                }
                else if (rbt_CalibUnloadGantryAbs.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibUnloadGantryXStep.Text);
                    logicModule.AbsMove(curAxis, Pos);
                }
                else if (rbt_CalibUnloadGantryRel.Checked)
                {
                    double Pos = -1.0 * Convert.ToDouble(txt_CalibUnloadGantryXStep.Text);
                    logicModule.RelMove(curAxis, Pos);
                }
            }
            else if (sender == btn_CalibUnloadGantryYPositive)
            {
                Axis curAxis = logicModule.LogicConfigValue.ECATAxis[3];
                if (rbt_CalibUnloadGantryJog.Checked)
                {
                    if (btn_CalibUnloadGantryYSpeed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    logicModule.StartJog(curAxis, 0);
                }
                else if (rbt_CalibUnloadGantryAbs.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibUnloadGantryYStep.Text);
                    logicModule.AbsMove(curAxis, Pos);
                }
                else if (rbt_CalibUnloadGantryRel.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibUnloadGantryYStep.Text);
                    logicModule.RelMove(curAxis, Pos);
                }
            }
            else if (sender == btn_CalibUnloadGantryYNegative)
            {
                Axis curAxis = logicModule.LogicConfigValue.ECATAxis[3];
                if (rbt_CalibUnloadGantryJog.Checked)
                {
                    if (btn_CalibUnloadGantryYSpeed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    logicModule.StartJog(curAxis, 1);
                }
                else if (rbt_CalibUnloadGantryAbs.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibUnloadGantryYStep.Text);
                    logicModule.AbsMove(curAxis, Pos);
                }
                else if (rbt_CalibUnloadGantryRel.Checked)
                {
                    double Pos = -1.0 * Convert.ToDouble(txt_CalibUnloadGantryYStep.Text);
                    logicModule.RelMove(curAxis, Pos);
                }
            }
        }

        private void btn_LoadGantryZMove_Click(object sender, EventArgs e)
        {
            if (btn_LoadGantryZMove.Text == "Z轴气缸下降")
            {
                logicModule.LoadGantryZStretch();
                btn_LoadGantryZMove.Text = "Z轴气缸上升";
            }
            else
            {
                logicModule.LoadGantryZRetract();
                btn_LoadGantryZMove.Text = "Z轴气缸下降";
            }
        }

        private void btn_LoadGantryLeftPistonMove_Click(object sender, EventArgs e)
        {
            if (btn_LoadGantryLeftPistonMove.Text == "左半缓存气缸下降")
            {
                logicModule.LoadGantryLeftPistonStretch();
                btn_LoadGantryLeftPistonMove.Text = "左半缓存气缸上升";
            }
            else
            {
                logicModule.LoadGantryLeftPistonRetract();
                btn_LoadGantryLeftPistonMove.Text = "左半缓存气缸下降";
            }
        }

        private void btn_LoadGantryRightPistonMove_Click(object sender, EventArgs e)
        {
            if (btn_LoadGantryRightPistonMove.Text == "右半缓存气缸下降")
            {
                logicModule.LoadGantryRightPistonStretch();
                btn_LoadGantryRightPistonMove.Text = "右半缓存气缸上升";
            }
            else
            {
                logicModule.LoadGantryRightPistonRetract();
                btn_LoadGantryRightPistonMove.Text = "右半缓存气缸下降";
            }
        }

        private void btn_LoadGantryAllPistonMove_Click(object sender, EventArgs e)
        {
            if (btn_LoadGantryAllPistonMove.Text == "缓存气缸全部下降")
            {
                logicModule.LoadGantryAllPistonStretch();
                btn_LoadGantryAllPistonMove.Text = "缓存气缸全部上升";
            }
            else
            {
                logicModule.LoadGantryAllPistonRetract();
                btn_LoadGantryAllPistonMove.Text = "缓存气缸全部下降";
            }
        }

        private void btn_LoadGantryLeftPistonSuck_Click(object sender, EventArgs e)
        {
            if (btn_LoadGantryLeftPistonSuck.Text == "左半缓存气缸吸")
            {
                logicModule.LoadGantryLeftSuckerSuck();
                btn_LoadGantryLeftPistonSuck.Text = "左半缓存气缸破";
            }
            else
            {
                logicModule.LoadGantryLeftSuckerBreak();
                btn_LoadGantryLeftPistonSuck.Text = "左半缓存气缸吸";
            }
        }

        private void btn_LoadGantryRightPistonSuck_Click(object sender, EventArgs e)
        {
            if (btn_LoadGantryRightPistonSuck.Text == "右半缓存气缸吸")
            {
                logicModule.LoadGantryRightSuckerSuck();
                btn_LoadGantryRightPistonSuck.Text = "右半缓存气缸破";
            }
            else
            {
                logicModule.LoadGantryRightSuckerBreak();
                btn_LoadGantryRightPistonSuck.Text = "右半缓存气缸吸";
            }
        }

        private void btn_LoadGantryAllPistonSuck_Click(object sender, EventArgs e)
        {
            if (btn_LoadGantryAllPistonSuck.Text == "缓存气缸全部吸")
            {
                logicModule.LoadGantryAllSuckerSuck();
                btn_LoadGantryAllPistonSuck.Text = "缓存气缸全部破";
            }
            else
            {
                logicModule.LoadGantryAllSuckerBreak();
                btn_LoadGantryAllPistonSuck.Text = "缓存气缸全部吸";
            }
        }

        private void btn_CalibLoadGantryXSpeed_Click(object sender, EventArgs e)
        {
            if (btn_CalibLoadGantryXSpeed.Text == "低 速")
                btn_CalibLoadGantryXSpeed.Text = "高 速";
            else
                btn_CalibLoadGantryXSpeed.Text = "低 速";
        }

        private void btn_CalibLoadGantryYSpeed_Click(object sender, EventArgs e)
        {
            if (btn_CalibLoadGantryYSpeed.Text == "低 速")
                btn_CalibLoadGantryYSpeed.Text = "高 速";
            else
                btn_CalibLoadGantryYSpeed.Text = "低 速";
        }

        private void btn_UnloadGantryZMove_Click(object sender, EventArgs e)
        {
            if (btn_UnloadGantryZMove.Text == "Z轴气缸下降")
            {
                logicModule.UnloadGantryZStretch();
                btn_UnloadGantryZMove.Text = "Z轴气缸上升";
            }
            else
            {
                logicModule.UnloadGantryZRetract();
                btn_UnloadGantryZMove.Text = "Z轴气缸下降";
            }
        }

        private void btn_UnloadGantryLeftPistonMove_Click(object sender, EventArgs e)
        {
            if (btn_UnloadGantryLeftPistonMove.Text == "左半缓存气缸下降")
            {
                logicModule.UnloadGantryLeftPistonStretch();
                btn_UnloadGantryLeftPistonMove.Text = "左半缓存气缸上升";
            }
            else
            {
                logicModule.UnloadGantryLeftPistonRetract();
                btn_UnloadGantryLeftPistonMove.Text = "左半缓存气缸下降";
            }
        }

        private void btn_UnloadGantryRightPistonMove_Click(object sender, EventArgs e)
        {
            if (btn_UnloadGantryRightPistonMove.Text == "右半缓存气缸下降")
            {
                logicModule.UnloadGantryRightPistonStretch();
                btn_UnloadGantryRightPistonMove.Text = "右半缓存气缸上升";
            }
            else
            {
                logicModule.UnloadGantryRightPistonRetract();
                btn_UnloadGantryRightPistonMove.Text = "右半缓存气缸下降";
            }
        }

        private void btn_UnloadGantryAllPistonMove_Click(object sender, EventArgs e)
        {
            if (btn_UnloadGantryAllPistonMove.Text == "缓存气缸全部下降")
            {
                logicModule.UnloadGantryAllPistonStretch();
                btn_UnloadGantryAllPistonMove.Text = "缓存气缸全部上升";
            }
            else
            {
                logicModule.UnloadGantryAllPistonRetract();
                btn_UnloadGantryAllPistonMove.Text = "缓存气缸全部下降";
            }
        }

        private void btn_UnloadGantryLeftPistonSuck_Click(object sender, EventArgs e)
        {
            if (btn_UnloadGantryLeftPistonSuck.Text == "左半缓存气缸吸")
            {
                logicModule.UnloadGantryLeftSuckerSuck();
                btn_UnloadGantryLeftPistonSuck.Text = "左半缓存气缸破";
            }
            else
            {
                logicModule.UnloadGantryLeftSuckerBreak();
                btn_UnloadGantryLeftPistonSuck.Text = "左半缓存气缸吸";
            }
        }

        private void btn_UnloadGantryRightPistonSuck_Click(object sender, EventArgs e)
        {
            if (btn_UnloadGantryRightPistonSuck.Text == "右半缓存气缸吸")
            {
                logicModule.UnloadGantryRightSuckerSuck();
                btn_UnloadGantryRightPistonSuck.Text = "右半缓存气缸破";
            }
            else
            {
                logicModule.UnloadGantryRightSuckerBreak();
                btn_UnloadGantryRightPistonSuck.Text = "右半缓存气缸吸";
            }
        }

        private void btn_UnloadGantryAllPistonSuck_Click(object sender, EventArgs e)
        {
            if (btn_UnloadGantryAllPistonSuck.Text == "缓存气缸全部吸")
            {
                logicModule.UnloadGantryAllSuckerSuck();
                btn_UnloadGantryAllPistonSuck.Text = "缓存气缸全部破";
            }
            else
            {
                logicModule.UnloadGantryAllSuckerBreak();
                btn_UnloadGantryAllPistonSuck.Text = "缓存气缸全部吸";
            }
        }

        private void btn_SaveUnloadGantryPos_Click(object sender, EventArgs e)
        {
            logicModule.UnloadGantryMotionPos.posInfo[CurCalibUnloadGantrySeq].XPos = Convert.ToDouble(txt_CalibUnloadGantryXPos.Text);
            logicModule.UnloadGantryMotionPos.posInfo[CurCalibUnloadGantrySeq].YPos = Convert.ToDouble(txt_CalibUnloadGantryYPos.Text);

            if (XmlSerializerHelper.WriteXML((object)logicModule.UnloadGantryMotionPos, logicModule.pathUnloadGantryMotionPos, typeof(MotionPos)))
                MessageBox.Show("点位更新成功");
            else
                MessageBox.Show("点位更新失败");
        }

        private void btn_CalibUnloadGantryXSpeed_Click(object sender, EventArgs e)
        {
            if (btn_CalibUnloadGantryXSpeed.Text == "低 速")
                btn_CalibUnloadGantryXSpeed.Text = "高 速";
            else
                btn_CalibUnloadGantryXSpeed.Text = "低 速";
        }

        private void btn_CalibUnloadGantryYSpeed_Click(object sender, EventArgs e)
        {
            if (btn_CalibUnloadGantryYSpeed.Text == "低 速")
                btn_CalibUnloadGantryYSpeed.Text = "高 速";
            else
                btn_CalibUnloadGantryYSpeed.Text = "低 速";
        }

        private void btn_SaveUnloadGantrySupplyPos_Click(object sender, EventArgs e)
        {
            logicModule.UnloadGantrySupplyMotionPos.posInfo[CurCalibUnloadGantrySupplySeq].XPos = Convert.ToDouble(txt_CalibUnloadGantrySupplyXPos.Text);
            logicModule.UnloadGantrySupplyMotionPos.posInfo[CurCalibUnloadGantrySupplySeq].YPos = Convert.ToDouble(txt_CalibUnloadGantrySupplyYPos.Text);

            if (XmlSerializerHelper.WriteXML((object)logicModule.UnloadGantrySupplyMotionPos, logicModule.pathUnloadGantrySupplyMotionPos, typeof(MotionPos)))
                MessageBox.Show("点位更新成功");
            else
                MessageBox.Show("点位更新失败");
        }

        private void btn_CalibUnloadGantrySupplyXSpeed_Click(object sender, EventArgs e)
        {
            if (btn_CalibUnloadGantrySupplyXSpeed.Text == "低 速")
                btn_CalibUnloadGantrySupplyXSpeed.Text = "高 速";
            else
                btn_CalibUnloadGantrySupplyXSpeed.Text = "低 速";
        }

        private void btn_CalibUnloadGantrySupplyYSpeed_Click(object sender, EventArgs e)
        {
            if (btn_CalibUnloadGantrySupplyYSpeed.Text == "低 速")
                btn_CalibUnloadGantrySupplyYSpeed.Text = "高 速";
            else
                btn_CalibUnloadGantrySupplyYSpeed.Text = "低 速";
        }

        private void btn_UnloadGantrySupplyZMove_Click(object sender, EventArgs e)
        {
            if (btn_UnloadGantrySupplyZMove.Text == "Z轴气缸下降")
            {
                logicModule.UnloadGantryZStretch();
                btn_UnloadGantrySupplyZMove.Text = "Z轴气缸上升";
            }
            else
            {
                logicModule.UnloadGantryZRetract();
                btn_UnloadGantrySupplyZMove.Text = "Z轴气缸下降";
            }
        }

        private void btn_UnloadGantrySupplyLeftPistonMove_Click(object sender, EventArgs e)
        {
            if (btn_UnloadGantrySupplyLeftPistonMove.Text == "左4缓存气缸下降")
            {
                logicModule.WaitECATPiston2Cmd2FeedbackDone(logicModule.UnloadGantryCylinderStretchControls[3], logicModule.UnloadGantryCylinderRetractControls[3],
                                                            logicModule.UnloadGantryCylinderStretchCheckBits[3], logicModule.UnloadGantryCylinderRetractCheckBits[3]);
                btn_UnloadGantrySupplyLeftPistonMove.Text = "左4缓存气缸上升";
            }
            else
            {
                logicModule.WaitECATPiston2Cmd2FeedbackDone(logicModule.UnloadGantryCylinderRetractControls[3], logicModule.UnloadGantryCylinderStretchControls[3],
                                                            logicModule.UnloadGantryCylinderRetractCheckBits[3], logicModule.UnloadGantryCylinderStretchCheckBits[3]);
                btn_UnloadGantrySupplyLeftPistonMove.Text = "左4缓存气缸下降";
            }
        }

        private void btn_UnloadGantrySupplyLeftPistonSuck_Click(object sender, EventArgs e)
        {
            if (btn_UnloadGantrySupplyLeftPistonSuck.Text == "左4缓存气缸吸")
            {
                logicModule.WaitECATPiston2Cmd1FeedbackDone(logicModule.UnloadGantrySuckerSuckControls[3], logicModule.UnloadGantrySuckerBreakControls[3],
                                                            logicModule.UnloadGantrySuckerCheckBits[3], true);
                btn_UnloadGantrySupplyLeftPistonSuck.Text = "左4缓存气缸破";
            }
            else
            {
                logicModule.WaitECATPiston2Cmd1FeedbackDone(logicModule.UnloadGantrySuckerBreakControls[3], logicModule.UnloadGantrySuckerSuckControls[3],
                                                            logicModule.UnloadGantrySuckerCheckBits[3], false);
                btn_UnloadGantrySupplyLeftPistonSuck.Text = "左4缓存气缸吸";
            }
        }

        private void btn_CalibUnloadGantrySupplyPos_MouseUp(object sender, MouseEventArgs e)
        {
            if (rbt_CalibUnloadGantrySupplyJog.Checked)
            {
                logicModule.StopJog(logicModule.LogicConfigValue.ECATAxis[2]);
                logicModule.StopJog(logicModule.LogicConfigValue.ECATAxis[3]);
            }
        }

        private void btn_CalibUnloadGantrySupplyPos_MouseDown(object sender, MouseEventArgs e)
        {
            if (logicModule.unloadgantrymovesafesignal == false)
                MessageBox.Show("请确认下料龙门的所有气缸都已缩回");

            if (sender == btn_CalibUnloadGantrySupplyXPositive)
            {
                Axis curAxis = logicModule.LogicConfigValue.ECATAxis[2];
                if (rbt_CalibUnloadGantrySupplyJog.Checked)
                {
                    if (btn_CalibUnloadGantrySupplyXSpeed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    logicModule.StartJog(curAxis, 0);
                }
                else if (rbt_CalibUnloadGantrySupplyAbs.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibUnloadGantrySupplyXStep.Text);
                    logicModule.AbsMove(curAxis, Pos);
                }
                else if (rbt_CalibUnloadGantrySupplyRel.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibUnloadGantrySupplyXStep.Text);
                    logicModule.RelMove(curAxis, Pos);
                }
            }
            else if (sender == btn_CalibUnloadGantrySupplyXNegative)
            {
                Axis curAxis = logicModule.LogicConfigValue.ECATAxis[2];
                if (rbt_CalibUnloadGantrySupplyJog.Checked)
                {
                    if (btn_CalibUnloadGantrySupplyXSpeed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    logicModule.StartJog(curAxis, 1);
                }
                else if (rbt_CalibUnloadGantrySupplyAbs.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibUnloadGantrySupplyXStep.Text);
                    logicModule.AbsMove(curAxis, Pos);
                }
                else if (rbt_CalibUnloadGantrySupplyRel.Checked)
                {
                    double Pos = -1.0 * Convert.ToDouble(txt_CalibUnloadGantrySupplyXStep.Text);
                    logicModule.RelMove(curAxis, Pos);
                }
            }
            else if (sender == btn_CalibUnloadGantrySupplyYPositive)
            {
                Axis curAxis = logicModule.LogicConfigValue.ECATAxis[3];
                if (rbt_CalibUnloadGantrySupplyJog.Checked)
                {
                    if (btn_CalibUnloadGantrySupplyYSpeed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    logicModule.StartJog(curAxis, 0);
                }
                else if (rbt_CalibUnloadGantrySupplyAbs.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibUnloadGantrySupplyYStep.Text);
                    logicModule.AbsMove(curAxis, Pos);
                }
                else if (rbt_CalibUnloadGantrySupplyRel.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibUnloadGantrySupplyYStep.Text);
                    logicModule.RelMove(curAxis, Pos);
                }
            }
            else if (sender == btn_CalibUnloadGantrySupplyYNegative)
            {
                Axis curAxis = logicModule.LogicConfigValue.ECATAxis[3];
                if (rbt_CalibUnloadGantrySupplyJog.Checked)
                {
                    if (btn_CalibUnloadGantrySupplyYSpeed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    logicModule.StartJog(curAxis, 1);
                }
                else if (rbt_CalibUnloadGantrySupplyAbs.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibUnloadGantrySupplyYStep.Text);
                    logicModule.AbsMove(curAxis, Pos);
                }
                else if (rbt_CalibUnloadGantrySupplyRel.Checked)
                {
                    double Pos = -1.0 * Convert.ToDouble(txt_CalibUnloadGantrySupplyYStep.Text);
                    logicModule.RelMove(curAxis, Pos);
                }
            }
        }

        private void btn_SaveLoadGantryPlacePos_Click(object sender, EventArgs e)
        {
            logicModule.systemParam.LoadTrayFinishPosX = Convert.ToDouble(txt_CalibLoadGantryXPos.Text);
            logicModule.systemParam.LoadTrayFinishPosY = Convert.ToDouble(txt_CalibLoadGantryYPos.Text);

            if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                MessageBox.Show("上料龙门放料点位更新成功");
            else
                MessageBox.Show("上料龙门放料点位更新失败");
        }

        private void btn_SaveUnloadGantrySuckPos_Click(object sender, EventArgs e)
        {
            logicModule.systemParam.UnloadTrayFinishPosX = Convert.ToDouble(txt_CalibUnloadGantryXPos.Text);
            logicModule.systemParam.UnloadTrayFinishPosY = Convert.ToDouble(txt_CalibUnloadGantryYPos.Text);

            if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                MessageBox.Show("下料龙门吸料点位更新成功");
            else
                MessageBox.Show("下料龙门吸料点位更新失败");
        }

        private void btn_SaveLoadGantryAvoidPos_Click(object sender, EventArgs e)
        {
            logicModule.systemParam.LoadTrayAvoidPosX = Convert.ToDouble(txt_CalibLoadGantryXPos.Text);
            logicModule.systemParam.LoadTrayAvoidPosY = Convert.ToDouble(txt_CalibLoadGantryYPos.Text);

            if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                MessageBox.Show("上料龙门躲避点位更新成功");
            else
                MessageBox.Show("上料龙门躲避点位更新失败");
        }

        private void btn_SaveUnloadGantryAvoidPos_Click(object sender, EventArgs e)
        {
            logicModule.systemParam.UnloadTrayAvoidPosX = Convert.ToDouble(txt_CalibUnloadGantryXPos.Text);
            logicModule.systemParam.UnloadTrayAvoidPosY = Convert.ToDouble(txt_CalibUnloadGantryYPos.Text);

            if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                MessageBox.Show("下料龙门躲避点位更新成功");
            else
                MessageBox.Show("下料龙门躲避点位更新失败");
        }

        private void btn_SaveUnloadGantryNGBPos_Click(object sender, EventArgs e)
        {
            logicModule.systemParam.UnloadTrayNGAPosX = Convert.ToDouble(txt_CalibUnloadGantryXPos.Text);
            logicModule.systemParam.UnloadTrayNGAPosY = Convert.ToDouble(txt_CalibUnloadGantryYPos.Text);

            if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                MessageBox.Show("NG B点位更新成功");
            else
                MessageBox.Show("NG B点位更新失败");
        }

        private void btn_SaveUnloadGantryNGCPos_Click(object sender, EventArgs e)
        {
            logicModule.systemParam.UnloadTrayNGBPosX = Convert.ToDouble(txt_CalibUnloadGantryXPos.Text);
            logicModule.systemParam.UnloadTrayNGBPosY = Convert.ToDouble(txt_CalibUnloadGantryYPos.Text);

            if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                MessageBox.Show("NG C点位更新成功");
            else
                MessageBox.Show("NG C点位更新失败");
        }

        private void btn_SaveUnloadGantryNGDPos_Click(object sender, EventArgs e)
        {
            logicModule.systemParam.UnloadTrayNGCPosX = Convert.ToDouble(txt_CalibUnloadGantryXPos.Text);
            logicModule.systemParam.UnloadTrayNGCPosY = Convert.ToDouble(txt_CalibUnloadGantryYPos.Text);

            if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                MessageBox.Show("NG D点位更新成功");
            else
                MessageBox.Show("NG D点位更新失败");
        }

        private void btn_SaveUnloadGantrySupplyRegion1Pos_Click(object sender, EventArgs e)
        {
            logicModule.systemParam.UnloadSupplyRegion1PosX = Convert.ToDouble(txt_CalibUnloadGantryXPos.Text);
            logicModule.systemParam.UnloadSupplyRegion1PosY = Convert.ToDouble(txt_CalibUnloadGantryYPos.Text);

            if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                MessageBox.Show("补料位区域1吸取点位更新成功");
            else
                MessageBox.Show("补料位区域1吸取点位更新失败");
        }

        private void btn_SaveUnloadGantrySupplyRegion2Pos_Click(object sender, EventArgs e)
        {
            logicModule.systemParam.UnloadSupplyRegion2PosX = Convert.ToDouble(txt_CalibUnloadGantryXPos.Text);
            logicModule.systemParam.UnloadSupplyRegion2PosY = Convert.ToDouble(txt_CalibUnloadGantryYPos.Text);

            if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                MessageBox.Show("补料位区域2吸取点位更新成功");
            else
                MessageBox.Show("补料位区域2吸取点位更新失败");
        }

        private void btn_CalibMoveToSuckAxisLeft_Click(object sender, EventArgs e)
        {
            Axis curAxis= logicModule.LogicConfigValue.PulseAxis[5];
            double Pos = Convert.ToDouble(txt_CalibSuckAxisLeftPos.Text);
            logicModule.AbsMove(curAxis, Pos);
        }

        private void btn_CalibMoveToSuckAxisRight_Click(object sender, EventArgs e)
        {
            Axis curAxis = logicModule.LogicConfigValue.PulseAxis[5];
            double Pos = Convert.ToDouble(txt_CalibSuckAxisRightPos.Text);
            logicModule.AbsMove(curAxis, Pos);
        }

        private void btn_CalibReadSuckAxisPos_Click(object sender, EventArgs e)
        {
            txt_CalibSuckAxisLeftPos.Text = logicModule.systemParam.SuckAxisLeftPos.ToString();
            txt_CalibSuckAxisRightPos.Text = logicModule.systemParam.SuckAxisRightPos.ToString();
        }

        private void btn_CalibSuckAxisPos_MouseDown(object sender, MouseEventArgs e)
        {
            if (logicModule.suckaxissafesignal == false)
                MessageBox.Show("请确认横移轴Z气缸已缩回");

            if (sender == btn_CalibSuckAxisPositive)
            {
                Axis curAxis = logicModule.LogicConfigValue.PulseAxis[5];
                if (rbt_CalibSuckAxisJog.Checked)
                {
                    if (btn_CalibSuckAxisSpeed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    logicModule.StartJog(curAxis, 0);
                }
                else if (rbt_CalibSuckAxisAbs.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibSuckAxisStep.Text);
                    logicModule.AbsMove(curAxis, Pos);
                }
                else if (rbt_CalibSuckAxisRel.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibSuckAxisStep.Text);
                    logicModule.RelMove(curAxis, Pos);
                }
            }
            else if (sender == btn_CalibSuckAxisNegative)
            {
                Axis curAxis = logicModule.LogicConfigValue.PulseAxis[5];
                if (rbt_CalibSuckAxisJog.Checked)
                {
                    if (btn_CalibSuckAxisSpeed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    logicModule.StartJog(curAxis, 1);
                }
                else if (rbt_CalibSuckAxisAbs.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibSuckAxisStep.Text);
                    logicModule.AbsMove(curAxis, Pos);
                }
                else if (rbt_CalibSuckAxisRel.Checked)
                {
                    double Pos = -1.0 * Convert.ToDouble(txt_CalibSuckAxisStep.Text);
                    logicModule.RelMove(curAxis, Pos);
                }
            }
        }

        private void btn_CalibSuckAxisPos_MouseUp(object sender, MouseEventArgs e)
        {
            if (rbt_CalibSuckAxisJog.Checked)
            {
                logicModule.StopJog(logicModule.LogicConfigValue.PulseAxis[5]);
            }
        }

        private void btn_CalibSuckAxisSpeed_Click(object sender, EventArgs e)
        {
            if (btn_CalibSuckAxisSpeed.Text == "低 速")
                btn_CalibSuckAxisSpeed.Text = "高 速";
            else
                btn_CalibSuckAxisSpeed.Text = "低 速";
        }

        private void btn_CalibWriteSuckAxisLeftPos_Click(object sender, EventArgs e)
        {
            logicModule.systemParam.SuckAxisLeftPos = Convert.ToDouble(txt_CalibSuckAxisPos.Text);
            txt_CalibSuckAxisLeftPos.Text = txt_CalibSuckAxisPos.Text;

            if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                MessageBox.Show("横移轴左吸取点位更新成功");
            else
                MessageBox.Show("横移轴左吸取点位更新失败");
        }

        private void btn_CalibWriteSuckAxisRightPos_Click(object sender, EventArgs e)
        {
            logicModule.systemParam.SuckAxisRightPos = Convert.ToDouble(txt_CalibSuckAxisPos.Text);
            txt_CalibSuckAxisRightPos.Text = txt_CalibSuckAxisPos.Text;

            if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                MessageBox.Show("横移轴右吸取点位更新成功");
            else
                MessageBox.Show("横移轴右吸取点位更新失败");
        }

        private void btn_SuckAxisZMove_Click(object sender, EventArgs e)
        {
            if (btn_SuckAxisZMove.Text == "Z轴气缸下降")
            {
                logicModule.SuckAxisZStretch();
                btn_SuckAxisZMove.Text = "Z轴气缸上升";
            }
            else
            {
                logicModule.SuckAxisZRetract();
                btn_SuckAxisZMove.Text = "Z轴气缸下降";
            }
        }

        private void btn_SuckAxisSuck_Click(object sender, EventArgs e)
        {
            if (btn_SuckAxisSuck.Text == "缓存气缸全部吸")
            {
                logicModule.SuckAxisSuckerSuck();
                btn_SuckAxisSuck.Text = "缓存气缸全部破";
            }
            else
            {
                logicModule.SuckAxisSuckerBreak();
                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);
                btn_SuckAxisSuck.Text = "缓存气缸全部吸";
            }
        }

        private void btn_CalibTrayZ_MouseDown(object sender, MouseEventArgs e)
        {
            Axis curAxis = new Axis();
            if (rbt_CalibLoadNullAxis.Checked)
                curAxis = logicModule.LogicConfigValue.PulseAxis[4];
            if (rbt_CalibLoadFullAxis.Checked)
                curAxis = logicModule.LogicConfigValue.PulseAxis[5];
            if (rbt_CalibUnloadNullAxis.Checked)
                curAxis = logicModule.LogicConfigValue.PulseAxis[6];
            if (rbt_CalibUnloadFullAxis.Checked)
                curAxis = logicModule.LogicConfigValue.PulseAxis[7];

            if (sender == btn_CalibTrayZPositive)
            {
                if (rbt_CalibTrayZJog.Checked)
                {
                    if (btn_CalibTrayZSpeed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    logicModule.StartJog(curAxis, 0);
                }
                else if (rbt_CalibTrayZAbs.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibTrayZStep.Text);
                    logicModule.AbsMove(curAxis, Pos);
                }
                else if (rbt_CalibTrayZRel.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibTrayZStep.Text);
                    logicModule.RelMove(curAxis, Pos);
                }
            }
            else if (sender == btn_CalibTrayZNegative)
            {
                if (rbt_CalibTrayZJog.Checked)
                {
                    if (btn_CalibSuckAxisSpeed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    logicModule.StartJog(curAxis, 1);
                }
                else if (rbt_CalibTrayZAbs.Checked)
                {
                    double Pos = Convert.ToDouble(txt_CalibTrayZStep.Text);
                    logicModule.AbsMove(curAxis, Pos);
                }
                else if (rbt_CalibTrayZRel.Checked)
                {
                    double Pos = -1.0 * Convert.ToDouble(txt_CalibTrayZStep.Text);
                    logicModule.RelMove(curAxis, Pos);
                }
            }
        }

        private void btn_CalibTrayZ_MouseUp(object sender, MouseEventArgs e)
        {
            if (rbt_CalibTrayZJog.Checked)
            {
                logicModule.StopJog(logicModule.LogicConfigValue.ECATAxis[4]);
                logicModule.StopJog(logicModule.LogicConfigValue.ECATAxis[5]);
                logicModule.StopJog(logicModule.LogicConfigValue.ECATAxis[6]);
                logicModule.StopJog(logicModule.LogicConfigValue.ECATAxis[7]);
            }
        }

        private void btn_CalibTrayZSpeed_Click(object sender, EventArgs e)
        {
            if (btn_CalibTrayZSpeed.Text == "低 速")
                btn_CalibTrayZSpeed.Text = "高 速";
            else
                btn_CalibTrayZSpeed.Text = "低 速";
        }

        private void btn_SaveTrayZUpLimit_Click(object sender, EventArgs e)
        {
            if (rbt_CalibLoadFullAxis.Checked)
            {
                logicModule.systemParam.TraySwitchLoadFullUpLimit = Convert.ToDouble(txt_CalibTrayZPos.Text);

                if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                    MessageBox.Show("上料满上限点位更新成功");
                else
                    MessageBox.Show("上料满上限点位更新失败");
                return;
            }

            if (rbt_CalibLoadNullAxis.Checked)
            {
                logicModule.systemParam.TraySwitchLoadNullUpLimit = Convert.ToDouble(txt_CalibTrayZPos.Text);

                if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                    MessageBox.Show("上料空上限点位更新成功");
                else
                    MessageBox.Show("上料空上限点位更新失败");
                return;
            }

            if (rbt_CalibUnloadFullAxis.Checked)
            {
                logicModule.systemParam.TraySwitchUnloadFullUpLimit = Convert.ToDouble(txt_CalibTrayZPos.Text);

                if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                    MessageBox.Show("下料满上限点位更新成功");
                else
                    MessageBox.Show("下料满上限点位更新失败");
                return;
            }

            if (rbt_CalibUnloadNullAxis.Checked)
            {
                logicModule.systemParam.TraySwitchUnloadNullUpLimit = Convert.ToDouble(txt_CalibTrayZPos.Text);

                if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                    MessageBox.Show("下料空上限点位更新成功");
                else
                    MessageBox.Show("下料空上限点位更新失败");
                return;
            }

        }

        private void btn_SaveTrayZSensePos_Click(object sender, EventArgs e)
        {
            if (rbt_CalibLoadFullAxis.Checked)
            {
                logicModule.systemParam.TraySwitchLoadFullRollSensePos = Convert.ToDouble(txt_CalibTrayZPos.Text);

                if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                    MessageBox.Show("上料满翻转点位更新成功");
                else
                    MessageBox.Show("上料满翻转点位更新失败");
                return;
            }

            if (rbt_CalibLoadNullAxis.Checked)
            {
                logicModule.systemParam.TraySwitchLoadNullRollSensePos = Convert.ToDouble(txt_CalibTrayZPos.Text);

                if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                    MessageBox.Show("上料空翻转点位更新成功");
                else
                    MessageBox.Show("上料空翻转点位更新失败");
                return;
            }

            if (rbt_CalibUnloadFullAxis.Checked)
            {
                logicModule.systemParam.TraySwitchUnloadFullRollSensePos = Convert.ToDouble(txt_CalibTrayZPos.Text);

                if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                    MessageBox.Show("下料满翻转点位更新成功");
                else
                    MessageBox.Show("下料满翻转点位更新失败");
                return;
            }

            if (rbt_CalibUnloadNullAxis.Checked)
            {
                logicModule.systemParam.TraySwitchUnloadNullRollSensePos = Convert.ToDouble(txt_CalibTrayZPos.Text);

                if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                    MessageBox.Show("下料空翻转点位更新成功");
                else
                    MessageBox.Show("下料空翻转点位更新失败");
                return;
            }
        }

        private void btn_SaveTrayZDownLimit_Click(object sender, EventArgs e)
        {
            if (rbt_CalibLoadFullAxis.Checked)
            {
                logicModule.systemParam.TraySwitchLoadFullDownLimit = Convert.ToDouble(txt_CalibTrayZPos.Text);

                if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                    MessageBox.Show("上料满下限点位更新成功");
                else
                    MessageBox.Show("上料满下限点位更新失败");
                return;
            }

            if (rbt_CalibLoadNullAxis.Checked)
            {
                logicModule.systemParam.TraySwitchLoadNullDownLimit = Convert.ToDouble(txt_CalibTrayZPos.Text);

                if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                    MessageBox.Show("上料空下限点位更新成功");
                else
                    MessageBox.Show("上料空下限点位更新失败");
                return;
            }

            if (rbt_CalibUnloadFullAxis.Checked)
            {
                logicModule.systemParam.TraySwitchUnloadFullDownLimit = Convert.ToDouble(txt_CalibTrayZPos.Text);

                if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                    MessageBox.Show("下料满下限点位更新成功");
                else
                    MessageBox.Show("下料满下限点位更新失败");
                return;
            }

            if (rbt_CalibUnloadNullAxis.Checked)
            {
                logicModule.systemParam.TraySwitchUnloadNullDownLimit = Convert.ToDouble(txt_CalibTrayZPos.Text);

                if (XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam)))
                    MessageBox.Show("下料空下限点位更新成功");
                else
                    MessageBox.Show("下料空下限点位更新失败");
                return;
            }
        }

        private void btnMoveToLoadTrayXY_Click(object sender, EventArgs e)
        {
            if (logicModule.LoadGantryAllPistonRetract() && logicModule.LoadGantryZRetract())
            {
                if (logicModule.LoadGantryMoveToLoadModule() == true)
                {
                    lbl_CalibLoadGantryPosX.Text = logicModule.systemParam.LoadTrayFinishPosX.ToString();
                    lbl_CalibLoadGantryPosY.Text = logicModule.systemParam.LoadTrayFinishPosY.ToString();
                    MessageBox.Show("移动完成");
                }
            }
            else
            {
                MessageBox.Show("请确保上料龙门Z轴&左右气缸缩回");
            }
        }

        private void btnMoveToUnloadTrayXY_Click(object sender, EventArgs e)
        {
            if (logicModule.UnloadGantryZRetract() && logicModule.UnloadGantryAllPistonRetract())
            {
                if (logicModule.UnloadGantryMoveToUnloadModule() == true)
                {
                    lbl_CalibLoadGantryPosX.Text = logicModule.systemParam.LoadTrayFinishPosX.ToString();
                    lbl_CalibLoadGantryPosY.Text = logicModule.systemParam.LoadTrayFinishPosY.ToString();
                    MessageBox.Show("移动完成");
                }
            }
            else
            {
                MessageBox.Show("请确保下料龙门Z轴&所有气缸缩回");
            }
        }


        private void InitSysParaUI()
        {
            if (dgv_CalibSysPara.Rows.Count == 0)
            {
                for(int i=0;i<55 ;i++)
                    dgv_CalibSysPara.Rows.Add();
                dgv_CalibSysPara.Rows[0].Cells[0].Value = "IgnoreDoor"; dgv_CalibSysPara.Rows[0].Cells[1].Value = "屏蔽安全门信号";
                dgv_CalibSysPara.Rows[1].Cells[0].Value = "IgnoreCamera"; dgv_CalibSysPara.Rows[1].Cells[1].Value = "屏蔽CCD检测动作";
                dgv_CalibSysPara.Rows[2].Cells[0].Value = "IgnoreLaser"; dgv_CalibSysPara.Rows[2].Cells[1].Value = "屏蔽Laser检测动作";
                dgv_CalibSysPara.Rows[3].Cells[0].Value = "WorkPieceNum"; dgv_CalibSysPara.Rows[3].Cells[1].Value = "单工位工件个数";
                dgv_CalibSysPara.Rows[4].Cells[0].Value = "AxisNum"; dgv_CalibSysPara.Rows[4].Cells[1].Value = "系统轴数目";
                dgv_CalibSysPara.Rows[5].Cells[0].Value = "OutTime"; dgv_CalibSysPara.Rows[5].Cells[1].Value = "超时判断时间(s)";
                dgv_CalibSysPara.Rows[6].Cells[0].Value = "LoadTrayDistance"; dgv_CalibSysPara.Rows[6].Cells[1].Value = "上/下料Tray盘固定上升/下降距离(mm)";
                dgv_CalibSysPara.Rows[7].Cells[0].Value = "LoadTrayDistanceSeg"; dgv_CalibSysPara.Rows[7].Cells[1].Value = "上/下料Tray分段上升/下降距离(mm)";
                dgv_CalibSysPara.Rows[8].Cells[0].Value = "LoadFullTrayFinishDistance"; dgv_CalibSysPara.Rows[8].Cells[1].Value = "上料满盘到位后继续上升距离(mm)";
                dgv_CalibSysPara.Rows[9].Cells[0].Value = "UnloadNullTrayFinishDistance"; dgv_CalibSysPara.Rows[9].Cells[1].Value = "下料空盘到位后继续上升距离(mm)";
                dgv_CalibSysPara.Rows[10].Cells[0].Value = "LoadFullTrayUpDistance"; dgv_CalibSysPara.Rows[10].Cells[1].Value = "上料满盘初始上升高度（适用于上料换盘新逻辑,mm）";
                dgv_CalibSysPara.Rows[11].Cells[0].Value = "UnloadFullFinishDistance"; dgv_CalibSysPara.Rows[11].Cells[1].Value = "下料满盘到位后继续下降距离(mm)";
                dgv_CalibSysPara.Rows[12].Cells[0].Value = "LoadGantrySuckDelay"; dgv_CalibSysPara.Rows[12].Cells[1].Value = "上料龙门吸取工件延时(ms)";
                dgv_CalibSysPara.Rows[13].Cells[0].Value = "UnloadGantrySuckDelay"; dgv_CalibSysPara.Rows[13].Cells[1].Value = "下料龙门吸取工件延时(ms)";
                dgv_CalibSysPara.Rows[14].Cells[0].Value = "TrayZMoveMaxCount"; dgv_CalibSysPara.Rows[14].Cells[1].Value = "Tray轴移动分段距离最大执行次数";
                dgv_CalibSysPara.Rows[15].Cells[0].Value = "LoadTrayFinishPosX"; dgv_CalibSysPara.Rows[15].Cells[1].Value = "上料龙门放料至上料模组X轴坐标(mm)";
                dgv_CalibSysPara.Rows[16].Cells[0].Value = "LoadTrayFinishPosY"; dgv_CalibSysPara.Rows[16].Cells[1].Value = "上料龙门放料至上料模组Y轴坐标(mm)";
                dgv_CalibSysPara.Rows[17].Cells[0].Value = "UnloadTrayFinishPosX"; dgv_CalibSysPara.Rows[17].Cells[1].Value = "下料龙门从下料模组吸取工件X轴坐标(mm)";
                dgv_CalibSysPara.Rows[18].Cells[0].Value = "UnloadTrayFinishPosY"; dgv_CalibSysPara.Rows[18].Cells[1].Value = "下料龙门从下料模组吸取工件Y轴坐标(mm)";
                dgv_CalibSysPara.Rows[19].Cells[0].Value = "UnloadSupplyRegion1PosX"; dgv_CalibSysPara.Rows[19].Cells[1].Value = "下料龙门放料至补料区上半区X轴坐标(mm)";
                dgv_CalibSysPara.Rows[20].Cells[0].Value = "UnloadSupplyRegion1PosY"; dgv_CalibSysPara.Rows[20].Cells[1].Value = "下料龙门放料至补料区上半区Y轴坐标(mm)";
                dgv_CalibSysPara.Rows[21].Cells[0].Value = "UnloadSupplyRegion2PosX"; dgv_CalibSysPara.Rows[21].Cells[1].Value = "下料龙门放料至补料区下半区X轴坐标(mm)";
                dgv_CalibSysPara.Rows[22].Cells[0].Value = "UnloadSupplyRegion2PosY"; dgv_CalibSysPara.Rows[22].Cells[1].Value = "下料龙门放料至补料区下半区Y轴坐标(mm)";
                dgv_CalibSysPara.Rows[23].Cells[0].Value = "UnloadTrayNGAPosX"; dgv_CalibSysPara.Rows[23].Cells[1].Value = "下料龙门抛料至B料盒X轴坐标(mm)";
                dgv_CalibSysPara.Rows[24].Cells[0].Value = "UnloadTrayNGAPosY"; dgv_CalibSysPara.Rows[24].Cells[1].Value = "下料龙门抛料至B料盒Y轴坐标(mm)";
                dgv_CalibSysPara.Rows[25].Cells[0].Value = "UnloadTrayNGBPosX"; dgv_CalibSysPara.Rows[25].Cells[1].Value = "下料龙门抛料至C料盒X轴坐标(mm)";
                dgv_CalibSysPara.Rows[26].Cells[0].Value = "UnloadTrayNGBPosY"; dgv_CalibSysPara.Rows[26].Cells[1].Value = "下料龙门抛料至C料盒Y轴坐标(mm)";
                dgv_CalibSysPara.Rows[27].Cells[0].Value = "UnloadTrayNGCPosX"; dgv_CalibSysPara.Rows[27].Cells[1].Value = "下料龙门抛料至D料盒X轴坐标(mm)";
                dgv_CalibSysPara.Rows[28].Cells[0].Value = "UnloadTrayNGCPosY"; dgv_CalibSysPara.Rows[28].Cells[1].Value = "下料龙门抛料至D料盒Y轴坐标(mm)";
                dgv_CalibSysPara.Rows[29].Cells[0].Value = "LoadTrayAvoidPosX"; dgv_CalibSysPara.Rows[29].Cells[1].Value = "上料龙门躲避上料Tray换盘时X轴坐标(mm)";
                dgv_CalibSysPara.Rows[30].Cells[0].Value = "LoadTrayAvoidPosY"; dgv_CalibSysPara.Rows[30].Cells[1].Value = "上料龙门躲避上料Tray换盘时Y轴坐标(mm)";
                dgv_CalibSysPara.Rows[31].Cells[0].Value = "UnloadTrayAvoidPosX"; dgv_CalibSysPara.Rows[31].Cells[1].Value = "下料龙门躲避下料Tray换盘时X轴坐标(mm)";
                dgv_CalibSysPara.Rows[32].Cells[0].Value = "UnloadTrayAvoidPosY"; dgv_CalibSysPara.Rows[32].Cells[1].Value = "下料龙门躲避下料Tray换盘时Y轴坐标(mm)";
                dgv_CalibSysPara.Rows[33].Cells[0].Value = "NGDrawerAlmNum"; dgv_CalibSysPara.Rows[33].Cells[1].Value = "NG料盒警告数目";
                dgv_CalibSysPara.Rows[34].Cells[0].Value = "SuckAxisLeftPos"; dgv_CalibSysPara.Rows[34].Cells[1].Value = "横移轴左方吸取坐标(mm)";
                dgv_CalibSysPara.Rows[35].Cells[0].Value = "SuckAxisRightPos"; dgv_CalibSysPara.Rows[35].Cells[1].Value = "横移轴右方吸取坐标(mm)";
                dgv_CalibSysPara.Rows[36].Cells[0].Value = "TraySwitchLoadFullRollSensePos"; dgv_CalibSysPara.Rows[36].Cells[1].Value = "上料满Tray翻转位置坐标(mm)";
                dgv_CalibSysPara.Rows[37].Cells[0].Value = "TraySwitchLoadFullDownLimit"; dgv_CalibSysPara.Rows[37].Cells[1].Value = "上料满Tray初始化移动下限位(mm)";
                dgv_CalibSysPara.Rows[38].Cells[0].Value = "TraySwitchLoadFullUpLimit"; dgv_CalibSysPara.Rows[38].Cells[1].Value = "上料满Tray初始化移动上限位(mm)";
                dgv_CalibSysPara.Rows[39].Cells[0].Value = "TraySwitchLoadNullDownLimit"; dgv_CalibSysPara.Rows[39].Cells[1].Value = "上料空Tray初始化移动下限位(mm)";
                dgv_CalibSysPara.Rows[40].Cells[0].Value = "TraySwitchLoadNullRollSensePos"; dgv_CalibSysPara.Rows[40].Cells[1].Value = "上料空Tray翻转位置坐标(mm)";
                dgv_CalibSysPara.Rows[41].Cells[0].Value = "TraySwitchLoadNullUpLimit"; dgv_CalibSysPara.Rows[41].Cells[1].Value = "上料空Tray初始化移动上限位(mm)";
                dgv_CalibSysPara.Rows[42].Cells[0].Value = "TraySwitchUnloadFullRollSensePos"; dgv_CalibSysPara.Rows[42].Cells[1].Value = "下料满Tray翻转位置坐标(mm)";
                dgv_CalibSysPara.Rows[43].Cells[0].Value = "TraySwitchUnloadFullUpLimit"; dgv_CalibSysPara.Rows[43].Cells[1].Value = "下料满Tray初始化移动上限位(mm)";
                dgv_CalibSysPara.Rows[44].Cells[0].Value = "TraySwitchUnloadFullDownLimit"; dgv_CalibSysPara.Rows[44].Cells[1].Value = "下料满Tray初始化移动下限位(mm)";
                dgv_CalibSysPara.Rows[45].Cells[0].Value = "TraySwitchUnloadNullRollSensePos"; dgv_CalibSysPara.Rows[45].Cells[1].Value = "下料空Tray翻转位置坐标(mm)";
                dgv_CalibSysPara.Rows[46].Cells[0].Value = "TraySwitchUnloadNullDownLimit"; dgv_CalibSysPara.Rows[46].Cells[1].Value = "下料空Tray初始化移动下限位(mm)";
                dgv_CalibSysPara.Rows[47].Cells[0].Value = "TraySwitchUnloadNullUpLimit"; dgv_CalibSysPara.Rows[47].Cells[1].Value = "下料空Tray初始化移动上限位(mm)";
                dgv_CalibSysPara.Rows[48].Cells[0].Value = "TrayMaxNum"; dgv_CalibSysPara.Rows[48].Cells[1].Value = "Tray盘单次上料最大值";
                dgv_CalibSysPara.Rows[49].Cells[0].Value = "TrayAlmNum"; dgv_CalibSysPara.Rows[49].Cells[1].Value = "Tray盘缺料警告值";
                //******************************樊竞明20181001***************************//
                dgv_CalibSysPara.Rows[50].Cells[0].Value = "ProductionRecordHourBeat"; dgv_CalibSysPara.Rows[50].Cells[1].Value = "生产数据统计节拍(单位:小时，>0&&<=12)";
                dgv_CalibSysPara.Rows[51].Cells[0].Value = "isCheckInPutProductionRecordHourBeat"; dgv_CalibSysPara.Rows[51].Cells[1].Value = "是否每次都提醒设置生产批次号";

                dgv_CalibSysPara.Rows[52].Cells[0].Value = "LoadGantrySuckerBreakDelay"; dgv_CalibSysPara.Rows[52].Cells[1].Value = "上料龙门吸嘴真空破延时(ms)";
                dgv_CalibSysPara.Rows[53].Cells[0].Value = "UnloadGantrySuckerBreakDelay"; dgv_CalibSysPara.Rows[53].Cells[1].Value = "下料龙门吸嘴真空破延时(ms)";
                dgv_CalibSysPara.Rows[54].Cells[0].Value = "SuckAxisSuckerBreakDelay"; dgv_CalibSysPara.Rows[54].Cells[1].Value = "横移轴吸嘴真空破延时(ms)";
            }
        }

        private void ShowSysPara(SystemParam mySysPara)
        {
            dgv_CalibSysPara.Rows[0].Cells[2].Value = mySysPara.IgnoreDoor.ToString();
            dgv_CalibSysPara.Rows[1].Cells[2].Value = mySysPara.IgnoreCamera.ToString();
            dgv_CalibSysPara.Rows[2].Cells[2].Value = mySysPara.IgnoreLaser.ToString();
            dgv_CalibSysPara.Rows[3].Cells[2].Value = mySysPara.WorkPieceNum.ToString();
            dgv_CalibSysPara.Rows[4].Cells[2].Value = mySysPara.AxisNum.ToString();
            dgv_CalibSysPara.Rows[5].Cells[2].Value = mySysPara.OutTime.ToString();
            dgv_CalibSysPara.Rows[6].Cells[2].Value = mySysPara.LoadTrayDistance.ToString();
            dgv_CalibSysPara.Rows[7].Cells[2].Value = mySysPara.LoadTrayDistanceSeg.ToString();
            dgv_CalibSysPara.Rows[8].Cells[2].Value = mySysPara.LoadFullTrayFinishDistance.ToString();
            dgv_CalibSysPara.Rows[9].Cells[2].Value = mySysPara.UnloadNullTrayFinishDistance.ToString();
            dgv_CalibSysPara.Rows[10].Cells[2].Value = mySysPara.LoadFullTrayUpDistance.ToString();
            dgv_CalibSysPara.Rows[11].Cells[2].Value = mySysPara.UnloadFullFinishDistance.ToString();
            dgv_CalibSysPara.Rows[12].Cells[2].Value = mySysPara.LoadGantrySuckDelay.ToString();
            dgv_CalibSysPara.Rows[13].Cells[2].Value = mySysPara.UnloadGantrySuckDelay.ToString();
            dgv_CalibSysPara.Rows[14].Cells[2].Value = mySysPara.TrayZMoveMaxCount.ToString();
            dgv_CalibSysPara.Rows[15].Cells[2].Value = mySysPara.LoadTrayFinishPosX.ToString();
            dgv_CalibSysPara.Rows[16].Cells[2].Value = mySysPara.LoadTrayFinishPosY.ToString();
            dgv_CalibSysPara.Rows[17].Cells[2].Value = mySysPara.UnloadTrayFinishPosX.ToString();
            dgv_CalibSysPara.Rows[18].Cells[2].Value = mySysPara.UnloadTrayFinishPosY.ToString();
            dgv_CalibSysPara.Rows[19].Cells[2].Value = mySysPara.UnloadSupplyRegion1PosX.ToString();
            dgv_CalibSysPara.Rows[20].Cells[2].Value = mySysPara.UnloadSupplyRegion1PosY.ToString();
            dgv_CalibSysPara.Rows[21].Cells[2].Value = mySysPara.UnloadSupplyRegion2PosX.ToString();
            dgv_CalibSysPara.Rows[22].Cells[2].Value = mySysPara.UnloadSupplyRegion2PosY.ToString();
            dgv_CalibSysPara.Rows[23].Cells[2].Value = mySysPara.UnloadTrayNGAPosX.ToString();
            dgv_CalibSysPara.Rows[24].Cells[2].Value = mySysPara.UnloadTrayNGAPosY.ToString();
            dgv_CalibSysPara.Rows[25].Cells[2].Value = mySysPara.UnloadTrayNGBPosX.ToString();
            dgv_CalibSysPara.Rows[26].Cells[2].Value = mySysPara.UnloadTrayNGBPosY.ToString();
            dgv_CalibSysPara.Rows[27].Cells[2].Value = mySysPara.UnloadTrayNGCPosX.ToString();
            dgv_CalibSysPara.Rows[28].Cells[2].Value = mySysPara.UnloadTrayNGCPosY.ToString();
            dgv_CalibSysPara.Rows[29].Cells[2].Value = mySysPara.LoadTrayAvoidPosX.ToString();
            dgv_CalibSysPara.Rows[30].Cells[2].Value = mySysPara.LoadTrayAvoidPosY.ToString();
            dgv_CalibSysPara.Rows[31].Cells[2].Value = mySysPara.UnloadTrayAvoidPosX.ToString();
            dgv_CalibSysPara.Rows[32].Cells[2].Value = mySysPara.UnloadTrayAvoidPosY.ToString();
            dgv_CalibSysPara.Rows[33].Cells[2].Value = mySysPara.NGDrawerAlmNum.ToString();
            dgv_CalibSysPara.Rows[34].Cells[2].Value = mySysPara.SuckAxisLeftPos.ToString();
            dgv_CalibSysPara.Rows[35].Cells[2].Value = mySysPara.SuckAxisRightPos.ToString();
            dgv_CalibSysPara.Rows[36].Cells[2].Value = mySysPara.TraySwitchLoadFullRollSensePos.ToString();
            dgv_CalibSysPara.Rows[37].Cells[2].Value = mySysPara.TraySwitchLoadFullDownLimit.ToString();
            dgv_CalibSysPara.Rows[38].Cells[2].Value = mySysPara.TraySwitchLoadFullUpLimit.ToString();
            dgv_CalibSysPara.Rows[39].Cells[2].Value = mySysPara.TraySwitchLoadNullDownLimit.ToString();
            dgv_CalibSysPara.Rows[40].Cells[2].Value = mySysPara.TraySwitchLoadNullRollSensePos.ToString();
            dgv_CalibSysPara.Rows[41].Cells[2].Value = mySysPara.TraySwitchLoadNullUpLimit.ToString();
            dgv_CalibSysPara.Rows[42].Cells[2].Value = mySysPara.TraySwitchUnloadFullRollSensePos.ToString();
            dgv_CalibSysPara.Rows[43].Cells[2].Value = mySysPara.TraySwitchUnloadFullUpLimit.ToString();
            dgv_CalibSysPara.Rows[44].Cells[2].Value = mySysPara.TraySwitchUnloadFullDownLimit.ToString();
            dgv_CalibSysPara.Rows[45].Cells[2].Value = mySysPara.TraySwitchUnloadNullRollSensePos.ToString();
            dgv_CalibSysPara.Rows[46].Cells[2].Value = mySysPara.TraySwitchUnloadNullDownLimit.ToString();
            dgv_CalibSysPara.Rows[47].Cells[2].Value = mySysPara.TraySwitchUnloadNullUpLimit.ToString();
            dgv_CalibSysPara.Rows[48].Cells[2].Value = mySysPara.TrayMaxNum.ToString();
            dgv_CalibSysPara.Rows[49].Cells[2].Value = mySysPara.TrayAlmNum.ToString();
            //*****************************樊竞明20181001************************//
            dgv_CalibSysPara.Rows[50].Cells[2].Value = mySysPara.ProductionRecordHourBeat.ToString("00");
            dgv_CalibSysPara.Rows[51].Cells[2].Value = mySysPara.isCheckInPutProductionRecordHourBeat.ToString();

            dgv_CalibSysPara.Rows[52].Cells[2].Value = mySysPara.LoadGantrySuckerBreakDelay.ToString();
            dgv_CalibSysPara.Rows[53].Cells[2].Value = mySysPara.UnloadGantrySuckerBreakDelay.ToString();
            dgv_CalibSysPara.Rows[54].Cells[2].Value = mySysPara.SuckAxisSuckerBreakDelay.ToString();
        }

        private void UpdateSysPara()
        {
            logicModule.systemParam.IgnoreDoor = Convert.ToInt32(dgv_CalibSysPara.Rows[0].Cells[2].Value);
            logicModule.systemParam.IgnoreCamera = Convert.ToInt32(dgv_CalibSysPara.Rows[1].Cells[2].Value);
            logicModule.systemParam.IgnoreLaser = Convert.ToInt32(dgv_CalibSysPara.Rows[2].Cells[2].Value);
            logicModule.systemParam.WorkPieceNum = Convert.ToInt32(dgv_CalibSysPara.Rows[3].Cells[2].Value);
            logicModule.systemParam.AxisNum = Convert.ToInt32(dgv_CalibSysPara.Rows[4].Cells[2].Value);
            logicModule.systemParam.OutTime = Convert.ToInt32(dgv_CalibSysPara.Rows[5].Cells[2].Value);
            logicModule.systemParam.LoadTrayDistance = Convert.ToDouble(dgv_CalibSysPara.Rows[6].Cells[2].Value);
            logicModule.systemParam.LoadTrayDistanceSeg = Convert.ToDouble(dgv_CalibSysPara.Rows[7].Cells[2].Value);
            logicModule.systemParam.LoadFullTrayFinishDistance = Convert.ToDouble(dgv_CalibSysPara.Rows[8].Cells[2].Value);
            logicModule.systemParam.UnloadNullTrayFinishDistance = Convert.ToDouble(dgv_CalibSysPara.Rows[9].Cells[2].Value);
            logicModule.systemParam.LoadFullTrayUpDistance = Convert.ToDouble(dgv_CalibSysPara.Rows[10].Cells[2].Value);
            logicModule.systemParam.UnloadFullFinishDistance = Convert.ToDouble(dgv_CalibSysPara.Rows[11].Cells[2].Value);
            logicModule.systemParam.LoadGantrySuckDelay = Convert.ToInt32(dgv_CalibSysPara.Rows[12].Cells[2].Value);
            logicModule.systemParam.UnloadGantrySuckDelay = Convert.ToInt32(dgv_CalibSysPara.Rows[13].Cells[2].Value);
            logicModule.systemParam.TrayZMoveMaxCount = Convert.ToInt32(dgv_CalibSysPara.Rows[14].Cells[2].Value);
            logicModule.systemParam.LoadTrayFinishPosX = Convert.ToDouble(dgv_CalibSysPara.Rows[15].Cells[2].Value);
            logicModule.systemParam.LoadTrayFinishPosY = Convert.ToDouble(dgv_CalibSysPara.Rows[16].Cells[2].Value);
            logicModule.systemParam.UnloadTrayFinishPosX = Convert.ToDouble(dgv_CalibSysPara.Rows[17].Cells[2].Value);
            logicModule.systemParam.UnloadTrayFinishPosY = Convert.ToDouble(dgv_CalibSysPara.Rows[18].Cells[2].Value);
            logicModule.systemParam.UnloadSupplyRegion1PosX = Convert.ToDouble(dgv_CalibSysPara.Rows[19].Cells[2].Value);
            logicModule.systemParam.UnloadSupplyRegion1PosY = Convert.ToDouble(dgv_CalibSysPara.Rows[20].Cells[2].Value);
            logicModule.systemParam.UnloadSupplyRegion2PosX = Convert.ToDouble(dgv_CalibSysPara.Rows[21].Cells[2].Value);
            logicModule.systemParam.UnloadSupplyRegion2PosY = Convert.ToDouble(dgv_CalibSysPara.Rows[22].Cells[2].Value);
            logicModule.systemParam.UnloadTrayNGAPosX = Convert.ToDouble(dgv_CalibSysPara.Rows[23].Cells[2].Value);
            logicModule.systemParam.UnloadTrayNGAPosY = Convert.ToDouble(dgv_CalibSysPara.Rows[24].Cells[2].Value);
            logicModule.systemParam.UnloadTrayNGBPosX = Convert.ToDouble(dgv_CalibSysPara.Rows[25].Cells[2].Value);
            logicModule.systemParam.UnloadTrayNGBPosY = Convert.ToDouble(dgv_CalibSysPara.Rows[26].Cells[2].Value);
            logicModule.systemParam.UnloadTrayNGCPosX = Convert.ToDouble(dgv_CalibSysPara.Rows[27].Cells[2].Value);
            logicModule.systemParam.UnloadTrayNGCPosY = Convert.ToDouble(dgv_CalibSysPara.Rows[28].Cells[2].Value);
            logicModule.systemParam.LoadTrayAvoidPosX = Convert.ToDouble(dgv_CalibSysPara.Rows[29].Cells[2].Value);
            logicModule.systemParam.LoadTrayAvoidPosY = Convert.ToDouble(dgv_CalibSysPara.Rows[30].Cells[2].Value);
            logicModule.systemParam.UnloadTrayAvoidPosX = Convert.ToDouble(dgv_CalibSysPara.Rows[31].Cells[2].Value);
            logicModule.systemParam.UnloadTrayAvoidPosY = Convert.ToDouble(dgv_CalibSysPara.Rows[32].Cells[2].Value);
            logicModule.systemParam.NGDrawerAlmNum = Convert.ToInt32(dgv_CalibSysPara.Rows[33].Cells[2].Value);
            logicModule.systemParam.SuckAxisLeftPos = Convert.ToDouble(dgv_CalibSysPara.Rows[34].Cells[2].Value);
            logicModule.systemParam.SuckAxisRightPos = Convert.ToDouble(dgv_CalibSysPara.Rows[35].Cells[2].Value);
            logicModule.systemParam.TraySwitchLoadFullRollSensePos = Convert.ToDouble(dgv_CalibSysPara.Rows[36].Cells[2].Value);
            logicModule.systemParam.TraySwitchLoadFullDownLimit = Convert.ToDouble(dgv_CalibSysPara.Rows[37].Cells[2].Value);
            logicModule.systemParam.TraySwitchLoadFullUpLimit = Convert.ToDouble(dgv_CalibSysPara.Rows[38].Cells[2].Value);
            logicModule.systemParam.TraySwitchLoadNullDownLimit = Convert.ToDouble(dgv_CalibSysPara.Rows[39].Cells[2].Value);
            logicModule.systemParam.TraySwitchLoadNullRollSensePos = Convert.ToDouble(dgv_CalibSysPara.Rows[40].Cells[2].Value);
            logicModule.systemParam.TraySwitchLoadNullUpLimit = Convert.ToDouble(dgv_CalibSysPara.Rows[41].Cells[2].Value);
            logicModule.systemParam.TraySwitchUnloadFullRollSensePos = Convert.ToDouble(dgv_CalibSysPara.Rows[42].Cells[2].Value);
            logicModule.systemParam.TraySwitchUnloadFullUpLimit = Convert.ToDouble(dgv_CalibSysPara.Rows[43].Cells[2].Value);
            logicModule.systemParam.TraySwitchUnloadFullDownLimit = Convert.ToDouble(dgv_CalibSysPara.Rows[44].Cells[2].Value);
            logicModule.systemParam.TraySwitchUnloadNullRollSensePos = Convert.ToDouble(dgv_CalibSysPara.Rows[45].Cells[2].Value);
            logicModule.systemParam.TraySwitchUnloadNullDownLimit = Convert.ToDouble(dgv_CalibSysPara.Rows[46].Cells[2].Value);
            logicModule.systemParam.TraySwitchUnloadNullUpLimit = Convert.ToDouble(dgv_CalibSysPara.Rows[47].Cells[2].Value);
            logicModule.systemParam.TrayMaxNum = Convert.ToInt32(dgv_CalibSysPara.Rows[48].Cells[2].Value);
            logicModule.systemParam.TrayAlmNum = Convert.ToInt32(dgv_CalibSysPara.Rows[49].Cells[2].Value);
            //******************樊竞明20181001
            logicModule.systemParam.ProductionRecordHourBeat = Convert.ToInt32(dgv_CalibSysPara.Rows[50].Cells[2].Value);
            logicModule.systemParam.isCheckInPutProductionRecordHourBeat = Convert.ToInt32(dgv_CalibSysPara.Rows[51].Cells[2].Value);

            logicModule.systemParam.LoadGantrySuckerBreakDelay = Convert.ToInt32(dgv_CalibSysPara.Rows[52].Cells[2].Value);
            logicModule.systemParam.UnloadGantrySuckerBreakDelay = Convert.ToInt32(dgv_CalibSysPara.Rows[53].Cells[2].Value);
            logicModule.systemParam.SuckAxisSuckerBreakDelay = Convert.ToInt32(dgv_CalibSysPara.Rows[54].Cells[2].Value);

            bool result = XmlSerializerHelper.WriteXML((object)logicModule.systemParam, logicModule.pathSystemParam, typeof(SystemParam));
            if (!result)
                MessageBox.Show("配置文件更新失败");
            else
                MessageBox.Show("配置文件更新成功");
        }

        private void btn_ApplySysPara_Click(object sender, EventArgs e)
        {
            UpdateSysPara();
        }

        private void btn_ResetSysPara_Click(object sender, EventArgs e)
        {
            ShowSysPara(logicModule.systemParam);
            MessageBox.Show("参数重新加载成功");
        }
    }
}
