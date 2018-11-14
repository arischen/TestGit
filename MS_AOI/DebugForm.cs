using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LogicControl;
using StructAssemble;
using System.Threading;
using System.IO;
using CsvHelper;
using XmlHelper;

namespace MS_AOI
{
    public partial class DebugForm : Form
    {
        public delegate void UpdateObjectDelegate(object sender);
        public delegate void UpdateVoidDelegate();

        public event UpdateObjectDelegate UpdateAxisInfo;    // 初始化测量统计界面
        //public event UpdateObjectDelegate UpdateIgnoreArray;//更新主界面LogicModule的屏蔽属性
        public event UpdateVoidDelegate ResetCCD;
        public event UpdateVoidDelegate HideDownTimeRecord;
        public event UpdateVoidDelegate ShowDownTimeRecord;

        private LogicModule logicModule;
        private Button[] BtnDi;

        private Button[] BtnDo;
        private Button[,] BtnMIO;
        private TextBox[] TxtCCDPos;
        private Button[] BtnDebugCCDMove;
        private CheckBox[] ChkDebugRegion1Condition, ChkDebugRegion2Condition, ChkDebugUnloadModuleCheckResult;

        public string pathMotionPos = Directory.GetCurrentDirectory() + @"\MyConfig\MotionPos.xml";
        public string moveConfigPath = Directory.GetCurrentDirectory() + @"\MyConfig\LaserPath.xml";//所有轨迹路径全称
        public string[] StationCheckParaPaths = new string[4];

        public MovePathConfig debugMoveConfig;

        public TestCCDInfluence ccdInfluenceForm;

        Thread DebugThread;
        Thread DebugMainAxisThread, DebugPartAThread, DebugPartBThread, DebugPartCThread;
        Thread AllRunLoadModule, AllRunUnloadModule, AllRunLoadGantry, AllRunUnloadGantry, AllRunLoadTraySwitch, AllRunUnloadTraySwitch, AllRunUnloadGantryPlaceAllWorkPiece;
        Thread AllRunMainAxis, AllRunCCDCheck, AllRunLaserCheck, AllRunSuckAxisMove, AllRunPartA;
        Thread clearLoadTrayThread, clearUnloadTrayThread,clearMainAxisWorkPieceThread;

        Thread debugSuckPinTestThread, repeatTestOnceThread, repeatTestMultiHoleThread, repeatTestOnceXgxThread, repeatTestAllMultiHoleThread;

        public DebugForm(ref LogicModule logic)
        {
            InitializeComponent();
            logicModule = logic;

            ccdInfluenceForm = new TestCCDInfluence(ref logic);

            cbx_RepeatTestStep.SelectedIndex = 0;
            cbx_RepeatTestStationNo.SelectedIndex = 0;

            BtnDi = new Button[] {btn_Di0,btn_Di1,btn_Di2,btn_Di3,btn_Di4,btn_Di5,btn_Di6,btn_Di7,btn_Di8,
                                  btn_Di9,btn_Di10,btn_Di11,btn_Di12,btn_Di13,btn_Di14,btn_Di15,btn_Di16,
                                  btn_Di17,btn_Di18,btn_Di19,btn_Di20,btn_Di21,btn_Di22,btn_Di23,btn_Di24,
                                  btn_ECATDi1_0,btn_ECATDi1_1,btn_ECATDi1_2,btn_ECATDi1_3,btn_ECATDi1_4,btn_ECATDi1_5,btn_ECATDi1_6,btn_ECATDi1_7,btn_ECATDi1_8,btn_ECATDi1_9,btn_ECATDi1_10,btn_ECATDi1_11,btn_ECATDi1_12,btn_ECATDi1_13,btn_ECATDi1_14,btn_ECATDi1_15,
                                  btn_ECATDi2_0,btn_ECATDi2_1,btn_ECATDi2_2,btn_ECATDi2_3,btn_ECATDi2_4,btn_ECATDi2_5,btn_ECATDi2_6,btn_ECATDi2_7,btn_ECATDi2_8,btn_ECATDi2_9,btn_ECATDi2_10,btn_ECATDi2_11,btn_ECATDi2_12,btn_ECATDi2_13,btn_ECATDi2_14,btn_ECATDi2_15,
                                  btn_ECATDi3_0,btn_ECATDi3_1,btn_ECATDi3_2,btn_ECATDi3_3,btn_ECATDi3_4,btn_ECATDi3_5,btn_ECATDi3_6,btn_ECATDi3_7,btn_ECATDi3_8,btn_ECATDi3_9,btn_ECATDi3_10,btn_ECATDi3_11,btn_ECATDi3_12,btn_ECATDi3_13,btn_ECATDi3_14,btn_ECATDi3_15,
                                  btn_ECATDi4_0,btn_ECATDi4_1,btn_ECATDi4_2,btn_ECATDi4_3,btn_ECATDi4_4,btn_ECATDi4_5,btn_ECATDi4_6,btn_ECATDi4_7,btn_ECATDi4_8,btn_ECATDi4_9,btn_ECATDi4_10,btn_ECATDi4_11,btn_ECATDi4_12,btn_ECATDi4_13,btn_ECATDi4_14,btn_ECATDi4_15,
                                  btn_ECATDi5_0,btn_ECATDi5_1,btn_ECATDi5_2,btn_ECATDi5_3,btn_ECATDi5_4,btn_ECATDi5_5,btn_ECATDi5_6,btn_ECATDi5_7,btn_ECATDi5_8,btn_ECATDi5_9,btn_ECATDi5_10,btn_ECATDi5_11,btn_ECATDi5_12,btn_ECATDi5_13,btn_ECATDi5_14,btn_ECATDi5_15,
                                  btn_ECATDi6_0,btn_ECATDi6_1,btn_ECATDi6_2,btn_ECATDi6_3,btn_ECATDi6_4,btn_ECATDi6_5,btn_ECATDi6_6,btn_ECATDi6_7,btn_ECATDi6_8,btn_ECATDi6_9,btn_ECATDi6_10,btn_ECATDi6_11,btn_ECATDi6_12,btn_ECATDi6_13,btn_ECATDi6_14,btn_ECATDi6_15,
                                  btn_ECATDi7_0,btn_ECATDi7_1,btn_ECATDi7_2,btn_ECATDi7_3,btn_ECATDi7_4,btn_ECATDi7_5,btn_ECATDi7_6,btn_ECATDi7_7,btn_ECATDi7_8,btn_ECATDi7_9,btn_ECATDi7_10,btn_ECATDi7_11,btn_ECATDi7_12,btn_ECATDi7_13,btn_ECATDi7_14,btn_ECATDi7_15,};

            BtnDo = new Button[] {btn_Do0,btn_Do1,btn_Do2,btn_Do3,btn_Do4,btn_Do5,btn_Do6,btn_Do7,btn_Do8,
                                  btn_Do9,btn_Do10,btn_Do11,btn_Do12,btn_Do13,btn_Do14,
                                  btn_ECATDo1_0,btn_ECATDo1_1,btn_ECATDo1_2,btn_ECATDo1_3,btn_ECATDo1_4,btn_ECATDo1_5,btn_ECATDo1_6,btn_ECATDo1_7,btn_ECATDo1_8,btn_ECATDo1_9,btn_ECATDo1_10,btn_ECATDo1_11,btn_ECATDo1_12,btn_ECATDo1_13,btn_ECATDo1_14,btn_ECATDo1_15,
                                  btn_ECATDo2_0,btn_ECATDo2_1,btn_ECATDo2_2,btn_ECATDo2_3,btn_ECATDo2_4,btn_ECATDo2_5,btn_ECATDo2_6,btn_ECATDo2_7,btn_ECATDo2_8,btn_ECATDo2_9,btn_ECATDo2_10,btn_ECATDo2_11,btn_ECATDo2_12,btn_ECATDo2_13,btn_ECATDo2_14,btn_ECATDo2_15,
                                  btn_ECATDo3_0,btn_ECATDo3_1,btn_ECATDo3_2,btn_ECATDo3_3,btn_ECATDo3_4,btn_ECATDo3_5,btn_ECATDo3_6,btn_ECATDo3_7,btn_ECATDo3_8,btn_ECATDo3_9,btn_ECATDo3_10,btn_ECATDo3_11,btn_ECATDo3_12,btn_ECATDo3_13,btn_ECATDo3_14,btn_ECATDo3_15,
                                  btn_ECATDo4_0,btn_ECATDo4_1,btn_ECATDo4_2,btn_ECATDo4_3,btn_ECATDo4_4,btn_ECATDo4_5,btn_ECATDo4_6,btn_ECATDo4_7,btn_ECATDo4_8,btn_ECATDo4_9,btn_ECATDo4_10,btn_ECATDo4_11,btn_ECATDo4_12,btn_ECATDo4_13,btn_ECATDo4_14,btn_ECATDo4_15,
                                  btn_ECATDo5_0,btn_ECATDo5_1,btn_ECATDo5_2,btn_ECATDo5_3,btn_ECATDo5_4,btn_ECATDo5_5,btn_ECATDo5_6,btn_ECATDo5_7,btn_ECATDo5_8,btn_ECATDo5_9,btn_ECATDo5_10,btn_ECATDo5_11,btn_ECATDo5_12,btn_ECATDo5_13,btn_ECATDo5_14,btn_ECATDo5_15,
                                  btn_ECATDo6_0,btn_ECATDo6_1,btn_ECATDo6_2,btn_ECATDo6_3,btn_ECATDo6_4,btn_ECATDo6_5,btn_ECATDo6_6,btn_ECATDo6_7,btn_ECATDo6_8,btn_ECATDo6_9,btn_ECATDo6_10,btn_ECATDo6_11,btn_ECATDo6_12,btn_ECATDo6_13,btn_ECATDo6_14,btn_ECATDo6_15,};

            BtnMIO = new Button[14, 7]
            {
                { btn_MainAxispel,btn_MainAxisorg,btn_MainAxismel,btnMainAxisalm,btn_MainAxissvn,btn_MainAxisjognegative,btn_MainAxisjogpositive },
                { btn_CCDCheckXpel,btn_CCDCheckXorg,btn_CCDCheckXmel,btn_CCDCheckXalm,btn_CCDCheckXsvn,btn_CCDCheckXjognegative,btn_CCDCheckXjogpositive },
                { btn_CCDCheckYpel,btn_CCDCheckYorg,btn_CCDCheckYmel,btn_CCDCheckYalm,btn_CCDCheckYsvn,btn_CCDCheckYjognegative,btn_CCDCheckYjogpositive },
                { btn_LaserCheckXpel,btn_LaserCheckXorg,btn_LaserCheckXmel,btn_LaserCheckXalm,btn_LaserCheckXsvn,btn_LaserCheckXjognegative,btn_LaserCheckXjogpositive },
                { btn_LaserCheckYpel,btn_LaserCheckYorg,btn_LaserCheckYmel,btn_LaserCheckYalm,btn_LaserCheckYsvn,btn_LaserCheckYjognegative,btn_LaserCheckYjogpositive },
                { btn_SuckAxispel,btn_SuckAxisorg,btn_SuckAxismel,btn_SuckAxisalm,btn_SuckAxissvn,btn_SuckAxisjognegative,btn_SuckAxisjogpositive },
                { btn_LoadXpel,btn_LoadXorg,btn_LoadXmel,btn_LoadXalm,btn_LoadXsvn,btn_LoadXjognegative,btn_LoadXjogpositive },
                { btn_LoadYpel,btn_LoadYorg,btn_LoadYmel,btn_LoadYalm,btn_LoadYsvn,btn_LoadYjognegative,btn_LoadYjogpositive },
                { btn_UnloadXpel,btn_UnloadXorg,btn_UnloadXmel,btn_UnloadXalm,btn_UnloadXsvn,btn_UnloadXjognegative,btn_UnloadXjogpositive },
                { btn_UnloadYpel,btn_UnloadYorg,btn_UnloadYmel,btn_UnloadYalm,btn_UnloadYsvn,btn_UnloadYjognegative,btn_UnloadYjogpositive },
                { btn_LoadNullpel,btn_LoadNullorg,btn_LoadNullmel,btn_LoadNullalm,btn_LoadNullsvn,btn_LoadNulljognegative,btn_LoadNulljogpositive },
                { btn_LoadFullpel,btn_LoadFullorg,btn_LoadFullmel,btn_LoadFullalm,btn_LoadFullsvn,btn_LoadFulljognegative,btn_LoadFulljogpositive },
                { btn_UnloadNullpel,btn_UnloadNullorg,btn_UnloadNullmel,btn_UnloadNullalm,btn_UnloadNullsvn,btn_UnloadNulljognegative,btn_UnloadNulljogpositive },
                { btn_UnloadFullpel,btn_UnloadFullorg,btn_UnloadFullmel,btn_UnloadFullalm,btn_UnloadFullsvn,btn_UnloadFulljognegative,btn_UnloadFulljogpositive },

            };

            TxtCCDPos = new TextBox[24]
            {
                txt_debugCCDPos0X,txt_debugCCDPos0Y,txt_debugCCDM0X,txt_debugCCDM0Y,txt_debugCCDPos1X,txt_debugCCDPos1Y,
                txt_debugCCDPos2X,txt_debugCCDPos2Y,txt_debugCCDM1X,txt_debugCCDM1Y,txt_debugCCDPos3X,txt_debugCCDPos3Y,
                txt_debugCCDPos4X,txt_debugCCDPos4Y,txt_debugCCDM2X,txt_debugCCDM2Y,txt_debugCCDPos5X,txt_debugCCDPos5Y,
                txt_debugCCDPos6X,txt_debugCCDPos6Y,txt_debugCCDM3X,txt_debugCCDM3Y,txt_debugCCDPos7X,txt_debugCCDPos7Y,
            };

            BtnDebugCCDMove = new Button[12]
            {
                btn_MoveToCCDPos0,btn_MoveToCCDPosM0,btn_MoveToCCDPos1,btn_MoveToCCDPos2,btn_MoveToCCDPosM1,btn_MoveToCCDPos3,
                btn_MoveToCCDPos4,btn_MoveToCCDPosM2,btn_MoveToCCDPos5,btn_MoveToCCDPos6,btn_MoveToCCDPosM3,btn_MoveToCCDPos7,
            };

            ChkDebugRegion1Condition = new CheckBox[8]
                {chk_Region1Condition0,chk_Region1Condition1,chk_Region1Condition2,chk_Region1Condition3,chk_Region1Condition4,chk_Region1Condition5,chk_Region1Condition6,chk_Region1Condition7};

            ChkDebugRegion2Condition = new CheckBox[8]
                {chk_Region2Condition0,chk_Region2Condition1,chk_Region2Condition2,chk_Region2Condition3,chk_Region2Condition4,chk_Region2Condition5,chk_Region2Condition6,chk_Region2Condition7};

            ChkDebugUnloadModuleCheckResult = new CheckBox[8]
                { chk_UnloadModule0,chk_UnloadModule1,chk_UnloadModule2,chk_UnloadModule3,chk_UnloadModule4,chk_UnloadModule5,chk_UnloadModule6,chk_UnloadModule7};
        }

        private void DebugForm_Load(object sender, EventArgs e)
        {
            cbx_ModuleSel.SelectedIndex = 0;
            cbx_AxisSel.SelectedIndex = 0;
            cbx_SelStation.SelectedIndex = 0;
            StationCheckParaPaths[0] = Directory.GetCurrentDirectory() + @"\MyConfig\StationACheckPara.xml";//A工位检测
            StationCheckParaPaths[1] = Directory.GetCurrentDirectory() + @"\MyConfig\StationBCheckPara.xml";//B工位检测
            StationCheckParaPaths[2] = Directory.GetCurrentDirectory() + @"\MyConfig\StationCCheckPara.xml";//C工位检测
            RefreshList.Start();
            debugMoveConfig = new MovePathConfig();
            debugMoveConfig.moveConfig = new List<MovePathParam>();

            chk_LoadGantry.Checked = true;
            chk_UnloadGantry.Checked = true;
            chk_LoadModule.Checked = true;
            chk_UnloadModule.Checked = true;
            chk_LoadTraySwitch.Checked = true;
            chk_UnloadTraySwitch.Checked = true;
            chk_AutoRunPartA.Checked = true;
            chk_CCDCheck.Checked = true;
            chk_LaserCheck.Checked = true;
            chk_MainAxis.Checked = true;
            chk_SuckAxis.Checked = true;
            chk_UnloadGantryPlaceAllWorkPiece.Checked = true;

            chk_WaitPut.Checked = true;

            logicModule.UpdateDebugButton += new LogicModule.UpdateObjectDelegate(DebugForm_UpdateButtons);

        }

        private void DebugForm_UpdateButtons(object myobj)
        {
            int temp = (int)myobj;
            if (temp == 1)
                btn_RepeatTestOnce.Text = "单穴重复性测试开始";
            else if (temp == 2)
                btn_MultiRepeatTest.Text = "多穴重复性测试开始";
            else if (temp == 3)
                btn_XgxTestOnceClick.Text = "相关性测试开始";
            else if (temp == 4)
                btn_ClearLoadTray.Text = "清理上料空满Tray开始";
            else if (temp == 5)
                btn_ClearUnloadTray.Text = "清理下料空满Tray开始";
        }

        //edited by lei.c 18.6.8
        private void UpdateButtonDIDO(UpdateInfo curInfo)
        {
            //DI更新
            for (int i = 0; i < 137; i++)
            {
                BtnDi[i].BackColor = curInfo.Di[i] ? Color.Green : Color.Silver;
            }

            //DO更新
            for (int i = 0; i < 111; i++)
            {
                BtnDo[i].BackColor = curInfo.Do[i] ? Color.Green : Color.Silver;
            }
        }

        public void UpdateMotionStatus(object sender)
        {
            if (!this.InvokeRequired)
            {
                UpdateInfo curInfo = (UpdateInfo)sender;
                if (cbx_ModuleSel.SelectedIndex >= 0 && cbx_ModuleSel.SelectedIndex <= 5)
                    tbx_CurPos.Text = (curInfo.CurAxisPos[cbx_ModuleSel.SelectedIndex]).ToString();

                if (cbx_ModuleSel.SelectedIndex >= 6 && cbx_ModuleSel.SelectedIndex <= 13)
                    tbx_CurPos.Text = (curInfo.CurAxisPos[cbx_ModuleSel.SelectedIndex + 2]).ToString();

                tbx_DebugCurPosMainAxis.Text = curInfo.CurAxisPos[0].ToString();
                tbx_DebugCurPosCCDCheckX.Text = curInfo.CurAxisPos[1].ToString();
                tbx_DebugCurPosCCDCheckY.Text = curInfo.CurAxisPos[2].ToString();
                tbx_DebugCurPosLaserCheckX.Text = curInfo.CurAxisPos[3].ToString();
                tbx_DebugCurPosLaserCheckY.Text = curInfo.CurAxisPos[4].ToString();
                tbx_DebugCurPosSuck.Text = curInfo.CurAxisPos[5].ToString();

                tbx_DebugCurPosLoadX.Text = curInfo.CurAxisPos[8].ToString();
                tbx_DebugCurPosLoadY.Text = curInfo.CurAxisPos[9].ToString();
                tbx_DebugCurPosUnloadX.Text = curInfo.CurAxisPos[10].ToString();
                tbx_DebugCurPosUnloadY.Text = curInfo.CurAxisPos[11].ToString();
                tbx_DebugCurPosLoadNull.Text = curInfo.CurAxisPos[12].ToString();
                tbx_DebugCurPosLoadFull.Text = curInfo.CurAxisPos[13].ToString();
                tbx_DebugCurPosUnloadNull.Text = curInfo.CurAxisPos[14].ToString();
                tbx_DebugCurPosUnloadFull.Text = curInfo.CurAxisPos[15].ToString();

                txt_DebugCCD2XPos.Text = curInfo.CurAxisPos[1].ToString();
                txt_DebugCCD2YPos.Text = curInfo.CurAxisPos[2].ToString();

                UpdateButtonDIDO(curInfo);
                UpdateDebugAxisIO(curInfo);
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(UpdateMotionStatus), sender);
            }
        }

        private void UpdateDebugAxisIO(UpdateInfo curInfo)
        {
            for (int i = 0; i < curInfo.motionIO.Length - 2; i++)
            {
                BtnMIO[i, 0].BackColor = curInfo.motionIO[i].pel == 1 ? Color.Green : Color.Silver;
                BtnMIO[i, 1].BackColor = curInfo.motionIO[i].org == 1 ? Color.Green : Color.Silver;
                BtnMIO[i, 2].BackColor = curInfo.motionIO[i].mel == 1 ? Color.Green : Color.Silver;
                BtnMIO[i, 3].BackColor = curInfo.motionIO[i].alm == 1 ? Color.Green : Color.Silver;
                BtnMIO[i, 4].BackColor = curInfo.motionIO[i].svon == 1 ? Color.Green : Color.Silver;
            }
        }

        public void UpdateSysInfo(object sender)
        {
            if (!this.InvokeRequired)
            {
                SystemParam sysPram = (SystemParam)sender;
                ckb_IgCCD.Checked = (sysPram.IgnoreCamera == 1);
                ckb_IgDoor.Checked = (sysPram.IgnoreDoor == 1);
                ckb_IgLaser.Checked = (sysPram.IgnoreLaser == 1);
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(UpdateSysInfo), sender);
            }
        }


        public void UpdateThreshold(object sender)
        {
            if (!this.InvokeRequired)
            {
                ThresholdParam thrParam = (ThresholdParam)sender;
                for (int i = 0; i < thrParam.thrInfo.Length; i++)
                {
                    if (dgv_Threshold.Rows.Count < thrParam.thrInfo.Length)
                    {
                        dgv_Threshold.Rows.Add(i, new DataGridViewRow());
                    }
                    dgv_Threshold.Rows[i].Cells[0].Value = thrParam.thrInfo[i].ThrName;
                    dgv_Threshold.Rows[i].Cells[1].Value = thrParam.thrInfo[i].UpLimit;
                    dgv_Threshold.Rows[i].Cells[2].Value = thrParam.thrInfo[i].DownLimit;
                }
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(UpdateThreshold), sender);
            }
        }

        public void UpdateCCDOffset(object sender)
        {
            if (!this.InvokeRequired)
            {
                CCDDataOffset myCCDDataOffset = (CCDDataOffset)sender;
                int n = 0;
                dgv_CCDOffsetPara.Rows.Clear();
                for (int i = 0; i < 12; i++)
                {
                    dgv_CCDOffsetPara.Rows.Add();
                    n = dgv_CCDOffsetPara.Rows.Count;
                    switch (i / 4)
                    {
                        case 0:
                            dgv_CCDOffsetPara.Rows[n - 1].Cells[0].Value = "A";
                            break;
                        case 1:
                            dgv_CCDOffsetPara.Rows[n - 1].Cells[0].Value = "B";
                            break;
                        case 2:
                            dgv_CCDOffsetPara.Rows[n - 1].Cells[0].Value = "C";
                            break;
                    }
                    dgv_CCDOffsetPara.Rows[n - 1].Cells[1].Value = (i % 4).ToString();
                    for (int j = 0; j < 13; j++)
                    {
                        dgv_CCDOffsetPara.Rows[n - 1].Cells[j + 2].Value = myCCDDataOffset.ccdGradient[i][j].ToString() + "," + myCCDDataOffset.ccdOffset[i][j].ToString();
                    }
                }
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(UpdateCCDOffset), sender);
            }
        }

        public void UpdateLaserOffset(object sender)
        {
            if (!this.InvokeRequired)
            {
                LaserDataOffset myLaserDataOffset = (LaserDataOffset)sender;
                int n = 0;
                dgv_LaserOffsetPara.Rows.Clear();
                for (int i = 0; i < 12; i++)
                {
                    dgv_LaserOffsetPara.Rows.Add();
                    n = dgv_LaserOffsetPara.Rows.Count;
                    switch (i / 4)
                    {
                        case 0:
                            dgv_LaserOffsetPara.Rows[n - 1].Cells[0].Value = "A";
                            break;
                        case 1:
                            dgv_LaserOffsetPara.Rows[n - 1].Cells[0].Value = "B";
                            break;
                        case 2:
                            dgv_LaserOffsetPara.Rows[n - 1].Cells[0].Value = "C";
                            break;
                    }
                    dgv_LaserOffsetPara.Rows[n - 1].Cells[1].Value = (i % 4).ToString();
                    for (int j = 0; j < 12; j++)
                    {
                        dgv_LaserOffsetPara.Rows[n - 1].Cells[j + 2].Value = myLaserDataOffset.laserGradient[i][j].ToString() + "," + myLaserDataOffset.laserOffset[i][j].ToString();
                    }
                }
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(UpdateLaserOffset), sender);
            }
        }

        private void btn_Do_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < BtnDo.Length; i++)
            {
                if (sender == BtnDo[i])
                {
                    if (i <= 14)
                    {
                        IOControl.WriteDO(i, !logicModule.CurInfo.Do[i]);
                        return;
                    }
                    if (i >= 15)
                    {
                        IOControl.ECATWriteDO(i - 15, !logicModule.CurInfo.Do[i]);
                        return;
                    }
                }
            }
        }

        private void btn_svn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 14; i++)
            {
                if (sender == BtnMIO[i, 3])//ALM
                {
                    if (logicModule.CurInfo.motionIO[i].alm == 1)
                    {
                        logicModule.ClearAxisAlarm(i);
                        return;
                    }
                }
                else if (sender == BtnMIO[i, 4])//SVN
                {
                    if (i <= 5)
                        logicModule.ServoCtrl(logicModule.logicConfig.PulseAxis[i].AxisId, logicModule.CurInfo.motionIO[i].svon == 0);
                    else
                        logicModule.ServoCtrl(logicModule.logicConfig.ECATAxis[i - 6].AxisId, logicModule.CurInfo.motionIO[i].svon == 0);
                    return;
                }
                else if (sender == BtnMIO[i, 1])//ORG
                {
                    if (i != 0)
                    {
                        if (i == 5)
                        {
                            logicModule.SuckAxisHome();
                            return;
                        }

                        if (i == 6)
                        {
                            logicModule.LoadGantryXHome();
                            return;
                        }
                        if (i == 7)
                        {
                            logicModule.LoadGantryYHome();
                            return;
                        }
                        if (i == 8)
                        {
                            logicModule.UnloadGantryXHome();
                            return;
                        }
                        if (i == 9)
                        {
                            logicModule.UnloadGantryYHome();
                            return;
                        }

                        logicModule.HomeMove(i);
                        return;
                    }
                    if (i == 0)
                    {
                        logicModule.MainAxisHome();
                        return;
                    }
                }
            }
        }

        private void btn_MouseDown(object sender, MouseEventArgs e)
        {
            if (cbx_ModuleSel.SelectedIndex < 0)
                return;

            if (cbx_ModuleSel.SelectedIndex <= 5)
            {
                if (rbt_JOG.Checked)
                {
                    Axis curAxis = logicModule.LogicConfigValue.PulseAxis[cbx_ModuleSel.SelectedIndex];
                    if (btn_Speed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    if (cbx_ModuleSel.SelectedIndex == 0)
                    {
                        logicModule.MainAxisStartJog((sender == btnPositive) ? 0 : 1);
                    }
                    else
                    {
                        if (cbx_ModuleSel.SelectedIndex == 5)
                            logicModule.SuckAxisStartJog((sender == btnPositive) ? 0 : 1);
                        else
                            logicModule.StartJog(curAxis, (sender == btnPositive) ? 0 : 1);
                    }
                }
                if (rbt_Rel.Checked)
                {
                    double Pos = (sender == btnPositive) ? Convert.ToDouble(tbx_Step.Text) : -Convert.ToDouble(tbx_Step.Text);
                    if (cbx_ModuleSel.SelectedIndex == 0)
                        logicModule.MainAxisMove(Pos, false);
                    else
                    {
                        if (cbx_ModuleSel.SelectedIndex == 5)
                            logicModule.SuckAxisMove(Pos, false);
                        else
                            logicModule.RelMove(logicModule.LogicConfigValue.PulseAxis[cbx_ModuleSel.SelectedIndex], Pos);
                    }
                }
                if (rbt_Abs.Checked)
                {
                    double Pos = Convert.ToDouble(tbx_Step.Text);
                    if (cbx_ModuleSel.SelectedIndex == 0)
                        logicModule.MainAxisMove(Pos, true);
                    else
                    {
                        if (cbx_ModuleSel.SelectedIndex == 5)
                            logicModule.SuckAxisMove(Pos, true);
                        else
                            logicModule.AbsMove(logicModule.LogicConfigValue.PulseAxis[cbx_ModuleSel.SelectedIndex], Pos);
                    }
                }
            }
            else
            {
                if (rbt_JOG.Checked)
                {
                    Axis curAxis = logicModule.LogicConfigValue.ECATAxis[cbx_ModuleSel.SelectedIndex - 6];
                    if (btn_Speed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }

                    if (cbx_ModuleSel.SelectedIndex == 6)
                    {
                        logicModule.LoadGantryXStartJog((sender == btnPositive) ? 0 : 1);
                    }
                    if (cbx_ModuleSel.SelectedIndex == 7)
                    {
                        logicModule.LoadGantryYStartJog((sender == btnPositive) ? 0 : 1);
                    }
                    if (cbx_ModuleSel.SelectedIndex == 8)
                    {
                        logicModule.UnloadGantryXStartJog((sender == btnPositive) ? 0 : 1);
                    }
                    if (cbx_ModuleSel.SelectedIndex == 9)
                    {
                        logicModule.UnloadGantryYStartJog((sender == btnPositive) ? 0 : 1);
                    }
                    if (cbx_ModuleSel.SelectedIndex != 6 && cbx_ModuleSel.SelectedIndex != 7 && cbx_ModuleSel.SelectedIndex != 8 && cbx_ModuleSel.SelectedIndex != 9)
                    {
                        logicModule.StartJog(curAxis, (sender == btnPositive) ? 0 : 1);
                    }
                }
                if (rbt_Rel.Checked)
                {
                    double Pos = (sender == btnPositive) ? Convert.ToDouble(tbx_Step.Text) : -Convert.ToDouble(tbx_Step.Text);
                    if (cbx_ModuleSel.SelectedIndex == 6)
                    {
                        logicModule.LoadGantryXMove(Pos, false);
                    }
                    if (cbx_ModuleSel.SelectedIndex == 7)
                    {
                        logicModule.LoadGantryYMove(Pos, false);
                    }
                    if (cbx_ModuleSel.SelectedIndex == 8)
                    {
                        logicModule.UnloadGantryXMove(Pos, false);
                    }
                    if (cbx_ModuleSel.SelectedIndex == 9)
                    {
                        logicModule.UnloadGantryYMove(Pos, false);
                    }
                    if (cbx_ModuleSel.SelectedIndex != 6 && cbx_ModuleSel.SelectedIndex != 7 && cbx_ModuleSel.SelectedIndex != 8 && cbx_ModuleSel.SelectedIndex != 9)
                    {
                        logicModule.RelMove(logicModule.LogicConfigValue.ECATAxis[cbx_ModuleSel.SelectedIndex - 6], Pos);
                    }
                }

                if (rbt_Abs.Checked)
                {
                    double Pos = Convert.ToDouble(tbx_Step.Text);
                    if (cbx_ModuleSel.SelectedIndex == 6)
                    {
                        logicModule.LoadGantryXMove(Pos, true);
                    }
                    if (cbx_ModuleSel.SelectedIndex == 7)
                    {
                        logicModule.LoadGantryYMove(Pos, true);
                    }
                    if (cbx_ModuleSel.SelectedIndex == 8)
                    {
                        logicModule.UnloadGantryXMove(Pos, true);
                    }
                    if (cbx_ModuleSel.SelectedIndex == 9)
                    {
                        logicModule.UnloadGantryYMove(Pos, true);
                    }
                    if (cbx_ModuleSel.SelectedIndex != 6 && cbx_ModuleSel.SelectedIndex != 7 && cbx_ModuleSel.SelectedIndex != 8 && cbx_ModuleSel.SelectedIndex != 9)
                    {
                        logicModule.AbsMove(logicModule.LogicConfigValue.ECATAxis[cbx_ModuleSel.SelectedIndex - 6], Pos);
                    }
                }
            }
        }

        private void btn_MouseUp(object sender, MouseEventArgs e)
        {
            if (cbx_ModuleSel.SelectedIndex <= 5)
            {
                if (rbt_JOG.Checked)
                {
                    logicModule.StopJog(logicModule.LogicConfigValue.PulseAxis[cbx_ModuleSel.SelectedIndex]);
                }
            }
            else
            {
                if (rbt_JOG.Checked)
                {
                    logicModule.StopJog(logicModule.LogicConfigValue.ECATAxis[cbx_ModuleSel.SelectedIndex - 6]);
                }
            }
        }

        private void btn_Speed_Click(object sender, EventArgs e)
        {
            if (btn_Speed.Text == "低 速")
                btn_Speed.Text = "高 速";
            else
                btn_Speed.Text = "低 速";
        }

        private void RefreshList_Tick(object sender, EventArgs e)
        {
            btn_GTConnect.Text = logicModule.tcp_enable ? "断开" : "连接";
            tbx_Rec1.Text = logicModule.tcp_Recive;

            if(tabControl1.SelectedTab.Text=="系统参数")
                JudgeShowDownTimeButton();

        }

        private void JudgeShowDownTimeButton()
        {
            Point panelPos = PointToScreen(panel_ShowDownTime.Location);
            if (Control.MousePosition.X >= panelPos.X && Control.MousePosition.X <= panelPos.X + panel_ShowDownTime.Size.Width &&
                Control.MousePosition.Y >= panelPos.Y+20 && Control.MousePosition.Y <= panelPos.Y + panel_ShowDownTime.Size.Height+20)
                btn_ShowDownTime.Show();
            else
                btn_ShowDownTime.Hide();
        }


        private void cbx_AxisSel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbx_AxisSel.SelectedIndex <= 5)
            {
                txt_AxisId.Text = logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].AxisId.ToString();
                txt_Rate.Text = logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].Rate.ToString();
                txt_HomeVel.Text = logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].HomeVel.ToString();
                txt_HomeAcc.Text = logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].HomeAcc.ToString();
                txt_HomeVO.Text = logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].HomeVO.ToString();
                txt_MoveVel.Text = logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].MoveVel.ToString();
                txt_MoveAcc.Text = logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].MoveAcc.ToString();
                txt_MoveDec.Text = logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].MoveDec.ToString();
                txt_JogVel.Text = logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].JogVel.ToString();
                txt_JogAcc.Text = logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].JogAcc.ToString();
                txt_JogDec.Text = logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].JogDec.ToString();
            }
            else
            {
                txt_AxisId.Text = logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].AxisId.ToString();
                txt_Rate.Text = logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].Rate.ToString();
                txt_HomeVel.Text = logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].HomeVel.ToString();
                txt_HomeAcc.Text = logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].HomeAcc.ToString();
                txt_HomeVO.Text = logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].HomeVO.ToString();
                txt_MoveVel.Text = logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].MoveVel.ToString();
                txt_MoveAcc.Text = logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].MoveAcc.ToString();
                txt_MoveDec.Text = logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].MoveDec.ToString();
                txt_JogVel.Text = logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].JogVel.ToString();
                txt_JogAcc.Text = logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].JogAcc.ToString();
                txt_JogDec.Text = logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].JogDec.ToString();
            }
        }

        private void btn_saveAxisParam_Click(object sender, EventArgs e)
        {
            if (cbx_AxisSel.SelectedIndex <= 5)
            {
                logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].AxisId = Convert.ToInt16(txt_AxisId.Text);
                logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].Rate = Convert.ToDouble(txt_Rate.Text);
                logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].HomeVel = Convert.ToDouble(txt_HomeVel.Text);
                logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].HomeAcc = Convert.ToDouble(txt_HomeAcc.Text);
                logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].HomeVO = Convert.ToInt32(txt_HomeVO.Text);
                logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].MoveVel = Convert.ToDouble(txt_MoveVel.Text);
                logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].MoveAcc = Convert.ToDouble(txt_MoveAcc.Text);
                logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].MoveDec = Convert.ToDouble(txt_MoveDec.Text);
                logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].JogVel = Convert.ToDouble(txt_JogVel.Text);
                logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].JogAcc = Convert.ToDouble(txt_JogAcc.Text);
                logicModule.logicConfig.PulseAxis[cbx_AxisSel.SelectedIndex].JogDec = Convert.ToDouble(txt_JogDec.Text);
            }
            else
            {
                logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].AxisId = Convert.ToInt16(txt_AxisId.Text);
                logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].Rate = Convert.ToDouble(txt_Rate.Text);
                logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].HomeVel = Convert.ToDouble(txt_HomeVel.Text);
                logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].HomeAcc = Convert.ToDouble(txt_HomeAcc.Text);
                logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].HomeVO = Convert.ToInt32(txt_HomeVO.Text);
                logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].MoveVel = Convert.ToDouble(txt_MoveVel.Text);
                logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].MoveAcc = Convert.ToDouble(txt_MoveAcc.Text);
                logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].MoveDec = Convert.ToDouble(txt_MoveDec.Text);
                logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].JogVel = Convert.ToDouble(txt_JogVel.Text);
                logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].JogAcc = Convert.ToDouble(txt_JogAcc.Text);
                logicModule.logicConfig.ECATAxis[cbx_AxisSel.SelectedIndex - 6].JogDec = Convert.ToDouble(txt_JogDec.Text);
            }

            UpdateAxisInfo(logicModule.logicConfig);
        }

        private void DebugForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void btn_GTConnect_Click(object sender, EventArgs e)
        {
            if (btn_GTConnect.Text == "断开")
            {
                logicModule.Tcp_DisConnect();
                logicModule.tcp_enable = false;
                btn_GTConnect.Text = "连接";
            }
            else
            {
                if (logicModule.KeyenceConnect())
                {
                    btn_GTConnect.Text = "断开";
                    return;
                }
                MessageBox.Show("Keyence连接失败！");
            }
        }

        private void btn_Send_Click(object sender, EventArgs e)
        {
            string Msg = tbx_SendMsg.Text + "\r\n";
            logicModule.TcpSendMsg(Msg);
        }

        private void rbt_JOG_CheckedChanged(object sender, EventArgs e)
        {
            if (rbt_JOG.Checked)
            {
                tbx_Step.Enabled = false;
                btn_Speed.Enabled = true;
            }
            else
            {
                tbx_Step.Enabled = true;
                btn_Speed.Enabled = false;
            }
        }

        private void btn_ThreadATestStart_Click(object sender, EventArgs e)
        {
            if (btn_ThreadATestStart.Text == "上下料整体测试开始")
            {
                btn_ThreadATestStart.Text = "上下料整体测试停止";

                DebugThread = new Thread(new ThreadStart(logicModule.AutoRunPartAThread), 1024);
                DebugThread.IsBackground = true;
                logicModule.debugThreadA = true;
                logicModule.AutoRunActive = true;
                DebugThread.Start();
            }
            else
            {
                btn_ThreadATestStart.Text = "上下料整体测试开始";
                //logicModule.SystemReset();
                DebugThread.Abort();
            }
        }

        private void btn_ThreadBTestStart_Click(object sender, EventArgs e)
        {
            if (btn_ThreadBTestStart.Text == "CCD整体测试开始")
            {
                btn_ThreadBTestStart.Text = "CCD整体测试停止";
                DebugThread = new Thread(new ThreadStart(logicModule.AutoRunPartBThread), 1204);
                DebugThread.IsBackground = true;
                logicModule.debugThreadB = true;
                logicModule.AutoRunActive = true;
                logicModule.AutoRunEnablePartB = true;
                logicModule.AutoRunPartBFinished = false;
                logicModule.AutoRunPartBCircleCount = 0;
                DebugThread.Start();
            }
            else
            {
                btn_ThreadBTestStart.Text = "CCD整体测试开始";
                DebugThread.Abort();
                logicModule.AutoRunEnablePartB = false;
                logicModule.debugThreadB = false;
                logicModule.AutoRunActive = false;
                logicModule.AutoRunPartBFinished = false;
                logicModule.RecvCount = 0;
                logicModule.CCDRawDataList.Clear();
                logicModule.CCDRecvStatusList.Clear();
                logicModule.AutoRunPartBCircleCount = 0;
                //logicModule.SystemReset();
            }
        }

        private void btn_ThreadCTestStart_Click(object sender, EventArgs e)
        {
            if (btn_ThreadCTestStart.Text == "Laser整体测试开始")
            {
                btn_ThreadCTestStart.Text = "Laser整体测试停止";
                DebugThread = new Thread(new ThreadStart(logicModule.AutoRunPartCThread));
                DebugThread.IsBackground = true;
                logicModule.debugThreadC = true;
                logicModule.AutoRunActive = true;
                logicModule.AutoRunEnablePartC = true;
                logicModule.AutoRunPartCFinished = false;
                DebugThread.Start();
            }
            else
            {
                btn_ThreadCTestStart.Text = "Laser整体测试开始";
                DebugThread.Abort();
                //logicModule.SystemReset();
                logicModule.AutoRunPartCCircleCount = 0;
                logicModule.nLaserData = 0;
                logicModule.debugThreadC = false;
                logicModule.AutoRunActive = false;
                logicModule.AutoRunPartCFinished = false;
                logicModule.AutoRunStepC = 0;
                logicModule.AutoRunEnablePartC = !ckb_IgLaser.Checked;

            }
        }

        private void btn_TestBComponent_Click(object sender, EventArgs e)
        {
            int errcode = 0;
            int componentstep = 0;

            if (string.IsNullOrEmpty(txt_CCDTestStep.Text))
            { return; }

            string testIdxStr = txt_CCDTestStep.Text;
            int testIdx = -1;
            int.TryParse(testIdxStr, out testIdx);
            if (testIdx < 0 || testIdx > 3)
            { return; }
            logicModule.debugThreadB = true;
            logicModule.CCDChecking = true;
            if (logicModule.AutoRunPartBComponent(testIdx, 0, 0, ref errcode, ref componentstep) == 1)
            {
                MessageBox.Show("CCD工位测试完成");
            }
            logicModule.CCDChecking = false;
            logicModule.debugThreadB = false;

            logicModule.RecvCount = 0;
            logicModule.RecvClassify = 0;
            logicModule.CCDRawDataList.Clear();
            logicModule.CCDRecvStatusList.Clear();
        }

        private void btn_AxisHome_Click(object sender, EventArgs e)
        {
            logicModule.HomeMove(1);
            logicModule.HomeMove(2);
        }

        private void btn_TestCComponent_Click(object sender, EventArgs e)
        {
            int testIdx = -1;

            if (btn_TestCComponent.Text == "单穴测试开始")
            {
                logicModule.Laser12DicData.Clear();
                if (string.IsNullOrEmpty(txt_LaserTestStep.Text))
                { return; }

                int.TryParse(txt_LaserTestStep.Text, out testIdx);
                if (testIdx < 0 || testIdx > 3)
                { return; }
                logicModule.debugThreadC = true;

                DebugThread = new Thread(new ParameterizedThreadStart(logicModule.LaserTestOnce));
                DebugThread.IsBackground = true;
                DebugThread.Start(testIdx);
                //logicModule.AutoRunPartCComponent(testIdx, 0 ,ref errcode);
                btn_TestCComponent.Text = "单穴测试结束";
            }
            else
            {
                DebugThread.Abort();
                btn_TestCComponent.Text = "单穴测试开始";
                logicModule.debugThreadC = false;
            }
        }

        private void btn_JogMouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < 14; i++)
            {
                if (sender == BtnMIO[i, 6])
                {
                    if (i <= 5)
                    {
                        if (i == 0)
                            logicModule.MainAxisStartJog(0);
                        else
                        {
                            if (i == 5)
                                logicModule.SuckAxisStartJog(0);
                            else
                                logicModule.StartJog(logicModule.LogicConfigValue.PulseAxis[i], 0);
                        }
                    }
                    else
                    {
                        if (i == 6)
                            logicModule.LoadGantryXStartJog(0);

                        if (i == 7)
                            logicModule.LoadGantryYStartJog(0);

                        if (i == 8)
                            logicModule.UnloadGantryXStartJog(0);

                        if (i == 9)
                            logicModule.UnloadGantryYStartJog(0);

                        if (i <= 13 && i >= 10)
                        {
                            logicModule.StartJog(logicModule.LogicConfigValue.ECATAxis[i - 6], 0);
                        }
                    }

                }
                else if (sender == BtnMIO[i, 5])
                {
                    if (i <= 5)
                    {
                        if (i == 0)
                            logicModule.MainAxisStartJog(1);
                        else
                        {
                            if (i == 5)
                                logicModule.SuckAxisStartJog(1);
                            else
                                logicModule.StartJog(logicModule.LogicConfigValue.PulseAxis[i], 1);
                        }
                    }
                    else
                    {
                        if (i == 6)
                            logicModule.LoadGantryXStartJog(1);

                        if (i == 7)
                            logicModule.LoadGantryYStartJog(1);

                        if (i == 8)
                            logicModule.UnloadGantryXStartJog(1);

                        if (i == 9)
                            logicModule.UnloadGantryYStartJog(1);

                        if (i <= 13 && i >= 10)
                        {
                            logicModule.StartJog(logicModule.LogicConfigValue.ECATAxis[i - 6], 1);
                        }
                    }
                }
            }
        }

        private void btn_JogMouseUp(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < logicModule.logicConfig.PulseAxis.Length + logicModule.logicConfig.ECATAxis.Length; i++)
            {
                if (sender == BtnMIO[i, 5] || sender == BtnMIO[i, 6])
                {
                    if (i <= 5)
                    {
                        logicModule.StopJog(logicModule.LogicConfigValue.PulseAxis[i]);
                        return;
                    }
                    else
                    {
                        logicModule.StopJog(logicModule.LogicConfigValue.ECATAxis[i - 6]);
                        return;
                    }
                }
            }
        }

        private void btn_DebugCCDMovePos(object sender, EventArgs e)
        {
            for (int i = 0; i < 24; i++)
            {
                if (sender == BtnDebugCCDMove[i])
                {
                    double posx = Convert.ToDouble(TxtCCDPos[2 * i].Text);
                    double posy = Convert.ToDouble(TxtCCDPos[2 * i + 1].Text);
                    logicModule.AbsMove(logicModule.LogicConfigValue.PulseAxis[1], posx);
                    logicModule.AbsMove(logicModule.LogicConfigValue.PulseAxis[2], posy);
                    return;
                }
            }
        }

        private void btn_WriteAllCCDPos_Click(object sender, EventArgs e)
        {
            if (txt_debugCCDPos0X.Text == "")
            {
                MessageBox.Show("请先读取CCD的移动位置");
                return;
            }

            for (int i = 0; i < 12; i++)
            {
                logicModule.CCDMotionPos.posInfo[i].XPos = Convert.ToDouble(TxtCCDPos[2 * i].Text);
                logicModule.CCDMotionPos.posInfo[i].YPos = Convert.ToDouble(TxtCCDPos[2 * i + 1].Text);
            }

            bool result = XmlSerializerHelper.WriteXML((object)logicModule.CCDMotionPos, pathMotionPos, typeof(MotionPos));

            if (!result)
            {
                MessageBox.Show("点位文件更新失败");
            }
            else
            {
                MessageBox.Show("点位文件更新成功");
            }
        }

        private void btn_ReadAllCCDPos_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 12; i++)
            {
                TxtCCDPos[2 * i].Text = logicModule.CCDMotionPos.posInfo[i].XPos.ToString();
                TxtCCDPos[2 * i + 1].Text = logicModule.CCDMotionPos.posInfo[i].YPos.ToString();
            }
        }

        private void btn_DebugCCD2XSpeed_Click(object sender, EventArgs e)
        {
            if (btn_DebugCCD2XSpeed.Text == "低 速")
                btn_DebugCCD2XSpeed.Text = "高 速";
            else
                btn_DebugCCD2XSpeed.Text = "低 速";
        }

        private void btn_DebugCCD2YSpeed_Click(object sender, EventArgs e)
        {
            if (btn_DebugCCD2YSpeed.Text == "低 速")
            {
                btn_DebugCCD2YSpeed.Text = "高 速";
            }
            else
            {
                btn_DebugCCD2YSpeed.Text = "低 速";
            }
        }

        private void btn_DebugCCD_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender == btn_DebugCCD2XPositive)
            {
                if (rbt_DebugCCDJOGMove.Checked)
                {
                    Axis curAxis = logicModule.LogicConfigValue.PulseAxis[1];
                    if (btn_DebugCCD2XSpeed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    logicModule.StartJog(curAxis, 0);
                }
                else if (rbt_DebugCCDABSMove.Checked)
                {
                    double Pos = Convert.ToDouble(txt_Debug2XStep.Text);
                    logicModule.AbsMove(logicModule.LogicConfigValue.PulseAxis[1], Pos);
                }
                else if (rbt_DebugCCDRELMove.Checked)
                {
                    double Pos = Convert.ToDouble(txt_Debug2XStep.Text);
                    logicModule.RelMove(logicModule.LogicConfigValue.PulseAxis[1], Pos);
                }
            }
            else if (sender == btn_DebugCCD2XNegative)
            {
                if (rbt_DebugCCDJOGMove.Checked)
                {
                    Axis curAxis = logicModule.LogicConfigValue.PulseAxis[1];
                    if (btn_DebugCCD2XSpeed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    logicModule.StartJog(curAxis, 1);
                }
                else if (rbt_DebugCCDABSMove.Checked)
                {
                    double Pos = Convert.ToDouble(txt_Debug2XStep.Text);
                    logicModule.AbsMove(logicModule.LogicConfigValue.PulseAxis[1], Pos);
                }
                else if (rbt_DebugCCDRELMove.Checked)
                {
                    double Pos = -1.0 * Convert.ToDouble(txt_Debug2XStep.Text);
                    logicModule.RelMove(logicModule.LogicConfigValue.PulseAxis[1], Pos);
                }
            }
            else if (sender == btn_DebugCCD2YPositive)
            {
                if (rbt_DebugCCDJOGMove.Checked)
                {
                    Axis curAxis = logicModule.LogicConfigValue.PulseAxis[0];
                    if (btn_DebugCCD2YSpeed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    logicModule.StartJog(curAxis, 0);
                }
                else if (rbt_DebugCCDABSMove.Checked)
                {
                    double Pos = Convert.ToDouble(txt_Debug2YStep.Text);
                    logicModule.AbsMove(logicModule.LogicConfigValue.PulseAxis[0], Pos);
                }
                else if (rbt_DebugCCDRELMove.Checked)
                {
                    double Pos = Convert.ToDouble(txt_Debug2YStep.Text);
                    logicModule.RelMove(logicModule.LogicConfigValue.PulseAxis[0], Pos);
                }
            }
            else if (sender == btn_DebugCCD2YNegative)
            {
                if (rbt_DebugCCDJOGMove.Checked)
                {
                    Axis curAxis = logicModule.LogicConfigValue.PulseAxis[0];
                    if (btn_DebugCCD2YSpeed.Text == "高 速")
                    {
                        curAxis.JogVel = 5 * curAxis.JogVel;
                    }
                    logicModule.StartJog(curAxis, 1);
                }
                else if (rbt_DebugCCDABSMove.Checked)
                {
                    double Pos = Convert.ToDouble(txt_Debug2YStep.Text);
                    logicModule.AbsMove(logicModule.LogicConfigValue.PulseAxis[0], Pos);
                }
                else if (rbt_DebugCCDRELMove.Checked)
                {
                    double Pos = -1.0 * Convert.ToDouble(txt_Debug2YStep.Text);
                    logicModule.RelMove(logicModule.LogicConfigValue.PulseAxis[0], Pos);
                }
            }
        }

        private void btn_DebugCCD_MouseUp(object sender, MouseEventArgs e)
        {
            if (rbt_DebugCCDJOGMove.Checked)
            {
                logicModule.StopJog(logicModule.LogicConfigValue.PulseAxis[1]);
                logicModule.StopJog(logicModule.LogicConfigValue.PulseAxis[2]);
            }
        }

        private void btn_ApplyIgnore_Click(object sender, EventArgs e)
        {
            if (logicModule.CurStatus != (int)STATUS.AUTO_STATUS && logicModule.CurStatus != (int)STATUS.PAUSE_STATUS)
            {
                logicModule.logicIgnore[0] = ckb_IgLoad.Checked;
                logicModule.logicIgnore[1] = ckb_IgCCD.Checked;
                logicModule.logicIgnore[2] = ckb_IgLaser.Checked;
                logicModule.bWaitPut = chk_WaitPut.Checked;
                logicModule.bIsLaird = rbt_Laird.Checked;
                logicModule.bIsSunway = rbt_Sunway.Checked;
                logicModule.isDisThrow = chk_IgThrow.Checked;//0905 add by ben 屏蔽抛料

                if (ckb_IgDoor.Checked)
                    logicModule.systemParam.IgnoreDoor = 1;
                else
                    logicModule.systemParam.IgnoreDoor = 0;
                logicModule.bIgnoreSupply = chk_IgSupply.Checked;
                logicModule.isNewLoadTraySwitch = chk_NewLoadSwitch.Checked;

                bool result = XmlSerializerHelper.WriteXML((object)logicModule.systemParam,logicModule.pathSystemParam, typeof(SystemParam));
                if (!result)
                    MessageBox.Show("配置文件更新失败");
                else
                    MessageBox.Show("配置文件更新成功");
            }
            else
            {
                MessageBox.Show("设备运行中，无法修改运行配置！");
            }
        }

        private void btn_DebugReadMoveConfig_Click(object sender, EventArgs e)
        {
            bool bFlag = false;
            debugMoveConfig = new MovePathConfig();
            debugMoveConfig.moveConfig = new List<MovePathParam>();

            if (dgv_MainLaserMoveConfig.Rows.Count != 0)
            {
                dgv_MainLaserMoveConfig.Rows.Clear();
            }

            debugMoveConfig = (MovePathConfig)XmlSerializerHelper.ReadXML(moveConfigPath, typeof(MovePathConfig), out bFlag);

            for (int i = 0; i < debugMoveConfig.moveConfig.Count; i++)
            {
                dgv_MainLaserMoveConfig.Rows.Add();
                int n = dgv_MainLaserMoveConfig.Rows.Count;
                dgv_MainLaserMoveConfig.Rows[n - 2].Cells[0].Value = debugMoveConfig.str3DTask;
                dgv_MainLaserMoveConfig.Rows[n - 2].Cells[1].Value = debugMoveConfig.moveConfig[i].strPathName;
                dgv_MainLaserMoveConfig.Rows[n - 2].Cells[2].Value = debugMoveConfig.moveConfig[i].StartPos.Xpos.ToString();
                dgv_MainLaserMoveConfig.Rows[n - 2].Cells[3].Value = debugMoveConfig.moveConfig[i].StartPos.Ypos.ToString();
                dgv_MainLaserMoveConfig.Rows[n - 2].Cells[4].Value = debugMoveConfig.moveConfig[i].EndPos.Xpos.ToString();
                dgv_MainLaserMoveConfig.Rows[n - 2].Cells[5].Value = debugMoveConfig.moveConfig[i].EndPos.Ypos.ToString();
                dgv_MainLaserMoveConfig.Rows[n - 2].Cells[6].Value = debugMoveConfig.moveConfig[i].TrigPos.Xpos.ToString();
                dgv_MainLaserMoveConfig.Rows[n - 2].Cells[7].Value = debugMoveConfig.moveConfig[i].TrigPos.Ypos.ToString();
                dgv_MainLaserMoveConfig.Rows[n - 2].Cells[8].Value = debugMoveConfig.moveConfig[i].dTrigInterval.ToString();
                dgv_MainLaserMoveConfig.Rows[n - 2].Cells[9].Value = debugMoveConfig.moveConfig[i].nTrigNum.ToString();
                dgv_MainLaserMoveConfig.Rows[n - 2].Cells[10].Value = debugMoveConfig.moveConfig[i].nMoveNO.ToString();
                dgv_MainLaserMoveConfig.Rows[n - 2].Cells[11].Value = debugMoveConfig.moveConfig[i].bIsATrig.ToString();
                dgv_MainLaserMoveConfig.Rows[n - 2].Cells[12].Value = debugMoveConfig.moveConfig[i].bIsBTrig.ToString();
            }
        }

        private void btn_DebugAddMoveConfig_Click(object sender, EventArgs e)
        {
            dgv_MainLaserMoveConfig.Rows.Add();
        }

        private void btn_DebugDeleteMoveConfig_Click(object sender, EventArgs e)
        {
            int deleteCount = dgv_MainLaserMoveConfig.SelectedRows.Count;
            if (deleteCount < 1)
            {
                MessageBox.Show("请至少选中一行");
                return;
            }
            else
            {
                if (DialogResult.Yes == MessageBox.Show("是否删除选中的数据？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information))
                {
                    for (int i = 0; i < dgv_MainLaserMoveConfig.Rows.Count - 1; i++)
                    {
                        if (true == dgv_MainLaserMoveConfig.Rows[i].Selected)
                        {
                            dgv_MainLaserMoveConfig.Rows.RemoveAt(i);
                            debugMoveConfig.moveConfig.RemoveAt(i);
                        }
                    }
                }
            }
        }

        private void btn_DebugWriteMoveConfig_Click(object sender, EventArgs e)
        {
            bool result = false;
            bool bFlag = false;

            if (debugMoveConfig.str3DTask != "")
            {
                debugMoveConfig.str3DTask = Convert.ToString(dgv_MainLaserMoveConfig.Rows[0].Cells[0].Value);
                debugMoveConfig.moveConfig.Clear();

                MovePathParam debugMoveConfigTemp = new MovePathParam();
                foreach (DataGridViewRow myRow in dgv_MainLaserMoveConfig.Rows)
                {
                    debugMoveConfigTemp.strPathName = Convert.ToString(myRow.Cells[1].Value);
                    debugMoveConfigTemp.StartPos.Xpos = Convert.ToDouble(myRow.Cells[2].Value);
                    debugMoveConfigTemp.StartPos.Ypos = Convert.ToDouble(myRow.Cells[3].Value);
                    debugMoveConfigTemp.EndPos.Xpos = Convert.ToDouble(myRow.Cells[4].Value);
                    debugMoveConfigTemp.EndPos.Ypos = Convert.ToDouble(myRow.Cells[5].Value);
                    debugMoveConfigTemp.TrigPos.Xpos = Convert.ToDouble(myRow.Cells[6].Value);
                    debugMoveConfigTemp.TrigPos.Ypos = Convert.ToDouble(myRow.Cells[7].Value);
                    debugMoveConfigTemp.dTrigInterval = Convert.ToDouble(myRow.Cells[8].Value);
                    debugMoveConfigTemp.nTrigNum = Convert.ToInt32(myRow.Cells[9].Value);
                    debugMoveConfigTemp.nMoveNO = Convert.ToInt32(myRow.Cells[10].Value);
                    debugMoveConfigTemp.bIsATrig = Convert.ToBoolean(myRow.Cells[11].Value);
                    debugMoveConfigTemp.bIsBTrig = Convert.ToBoolean(myRow.Cells[12].Value);

                    debugMoveConfig.moveConfig.Add(debugMoveConfigTemp);
                }

                debugMoveConfig.moveConfig.RemoveAt(debugMoveConfig.moveConfig.Count - 1);
                result = XmlSerializerHelper.WriteXML((object)debugMoveConfig, moveConfigPath, typeof(MovePathConfig));

                logicModule.moveConfig = (MovePathConfig)XmlSerializerHelper.ReadXML(moveConfigPath, typeof(MovePathConfig), out bFlag);
            }

            if (!result)
                MessageBox.Show("激光轨迹文件更新失败");
            else
                MessageBox.Show("激光轨迹文件更新成功");
        }

        private void btn_CCDReset_Click(object sender, EventArgs e)
        {
            ResetCCD();
        }

        private void chk_SaveLaserRawData_CheckedChanged(object sender, EventArgs e)
        {
            logicModule.taskConfig.isSaveData = chk_SaveLaserRawData.Checked;
        }

        private void chk_SaveLaserCalibData_CheckedChanged(object sender, EventArgs e)
        {
            logicModule.taskConfig.isSaveCalibData = chk_SaveLaserCalibData.Checked;
        }

        private void btn_ApplyTestSetting_Click(object sender, EventArgs e)
        {
            logicModule.StationCheckSetting[cbx_SelStation.SelectedIndex].LaserCheck = chk_LaserCheckStation.Checked;
            logicModule.StationCheckSetting[cbx_SelStation.SelectedIndex].CCDCheck = chk_CCDCheckStation.Checked;
            logicModule.StationCheckSetting[cbx_SelStation.SelectedIndex].CCDHoleCheck[0] = chk_CCDHoleCheck0.Checked;
            logicModule.StationCheckSetting[cbx_SelStation.SelectedIndex].CCDHoleCheck[1] = chk_CCDHoleCheck1.Checked;
            logicModule.StationCheckSetting[cbx_SelStation.SelectedIndex].CCDHoleCheck[2] = chk_CCDHoleCheck2.Checked;
            logicModule.StationCheckSetting[cbx_SelStation.SelectedIndex].CCDHoleCheck[3] = chk_CCDHoleCheck3.Checked;

            logicModule.StationCheckSetting[cbx_SelStation.SelectedIndex].LaserHoleCheck[0] = chk_LaserHoleCheck0.Checked;
            logicModule.StationCheckSetting[cbx_SelStation.SelectedIndex].LaserHoleCheck[1] = chk_LaserHoleCheck1.Checked;
            logicModule.StationCheckSetting[cbx_SelStation.SelectedIndex].LaserHoleCheck[2] = chk_LaserHoleCheck2.Checked;
            logicModule.StationCheckSetting[cbx_SelStation.SelectedIndex].LaserHoleCheck[3] = chk_LaserHoleCheck3.Checked;


            WriteStationCheckPara(cbx_SelStation.SelectedIndex);
        }

        private void cbx_SelStation_SelectedIndexChanged(object sender, EventArgs e)
        {
            chk_LaserCheckStation.Checked = logicModule.StationCheckSetting[cbx_SelStation.SelectedIndex].LaserCheck;
            chk_CCDCheckStation.Checked = logicModule.StationCheckSetting[cbx_SelStation.SelectedIndex].CCDCheck;
            chk_CCDHoleCheck0.Checked = logicModule.StationCheckSetting[cbx_SelStation.SelectedIndex].CCDHoleCheck[0];
            chk_CCDHoleCheck1.Checked = logicModule.StationCheckSetting[cbx_SelStation.SelectedIndex].CCDHoleCheck[1];
            chk_CCDHoleCheck2.Checked = logicModule.StationCheckSetting[cbx_SelStation.SelectedIndex].CCDHoleCheck[2];
            chk_CCDHoleCheck3.Checked = logicModule.StationCheckSetting[cbx_SelStation.SelectedIndex].CCDHoleCheck[3];

            chk_LaserHoleCheck0.Checked = logicModule.StationCheckSetting[cbx_SelStation.SelectedIndex].LaserHoleCheck[0];
            chk_LaserHoleCheck1.Checked = logicModule.StationCheckSetting[cbx_SelStation.SelectedIndex].LaserHoleCheck[1];
            chk_LaserHoleCheck2.Checked = logicModule.StationCheckSetting[cbx_SelStation.SelectedIndex].LaserHoleCheck[2];
            chk_LaserHoleCheck3.Checked = logicModule.StationCheckSetting[cbx_SelStation.SelectedIndex].LaserHoleCheck[3];
        }

        private void btn_WriteStationCheckPara_Click(object sender, EventArgs e)
        {
            if (WriteStationCheckPara(cbx_SelStation.SelectedIndex))
                MessageBox.Show("工位配置文件更新成功");
            else
                MessageBox.Show("工位配置文件更新失败");
        }

        private bool WriteStationCheckPara(int selectedIndex)
        {
            StationCheckPara StationCheckParaTemp = new StationCheckPara(8);

            bool result = XmlSerializerHelper.WriteXML((object)logicModule.StationCheckSetting[selectedIndex], StationCheckParaPaths[selectedIndex], typeof(StationCheckPara));
            if (!result)
                return false;
            return true;
        }

        private void btn_LoadCylinderMove_Click(object sender, EventArgs e)
        {
            if (btn_LoadCylinderMove.Text == "载具伸出")
            {
                if (logicModule.StretchOutAllCylinder() == true)
                    btn_LoadCylinderMove.Text = "载具缩回";
                else
                    MessageBox.Show("载具伸出出错");
            }
            else
            {
                if (logicModule.RetractAllCylinder() == true)
                    btn_LoadCylinderMove.Text = "载具伸出";
                else
                    MessageBox.Show("载具缩回出错");
            }
        }


        private void btn_CCDUpLightControl_Click(object sender, EventArgs e)
        {
            if (btn_CCDUpLightControl.Text == "上光源亮")
            {
                IOControl.ECATWriteDO((int)ECATDONAME.Do_CCDUpLightCmd, true);
                btn_CCDUpLightControl.Text = "上光源灭";
            }
            else
            {
                IOControl.ECATWriteDO((int)ECATDONAME.Do_CCDUpLightCmd, false);
                btn_CCDUpLightControl.Text = "上光源亮";
            }
        }

        private void btn_CCDDownLightControl_Click(object sender, EventArgs e)
        {
            if (btn_CCDDownLightControl.Text == "下光源亮")
            {
                IOControl.ECATWriteDO((int)ECATDONAME.Do_CCDDownLightCmd, true);
                btn_CCDDownLightControl.Text = "下光源灭";
            }
            else
            {
                IOControl.ECATWriteDO((int)ECATDONAME.Do_CCDDownLightCmd, false);
                btn_CCDDownLightControl.Text = "下光源亮";
            }
        }

        private void btn_ResetOffsetPara_Click(object sender, EventArgs e)
        {
            UpdateCCDOffset(new CCDDataOffset(DataOffset.ccdGradient, DataOffset.ccdOffset));
        }

        private void btn_ResetLaserOffsetPara_Click(object sender, EventArgs e)
        {
            UpdateLaserOffset(new LaserDataOffset(DataOffset.laserGradient, DataOffset.laserOffset));
        }

        private void btn_ApplyCCDOffsetPara_Click(object sender, EventArgs e)
        {
            DataOffset.WriteOffset(dgv_CCDOffsetPara, DataOffset.CCDGradientPath, 0);
            DataOffset.WriteOffset(dgv_CCDOffsetPara, DataOffset.CCDOffsetPath, 1);

            DataOffset.ccdGradient = DataOffset.ReadOffset(DataOffset.CCDGradientPath, 13);
            DataOffset.ccdOffset = DataOffset.ReadOffset(DataOffset.CCDOffsetPath, 13);

            if ((DataOffset.ccdGradient != null) && (DataOffset.ccdOffset != null))
                MessageBox.Show("CCD补偿文件更新成功");
            else
                MessageBox.Show("CCD补偿文件更新失败");
        }

        private void btn_ApplyLaserOffsetPara_Click(object sender, EventArgs e)
        {
            DataOffset.WriteOffset(dgv_LaserOffsetPara, DataOffset.LaserGradientPath, 0);
            DataOffset.WriteOffset(dgv_LaserOffsetPara, DataOffset.LaserOffsetPath, 1);

            DataOffset.laserGradient = DataOffset.ReadOffset(DataOffset.LaserGradientPath, 12);
            DataOffset.laserOffset = DataOffset.ReadOffset(DataOffset.LaserOffsetPath, 12);

            if ((DataOffset.laserGradient != null) && (DataOffset.laserOffset != null))
                MessageBox.Show("Laser补偿文件更新成功");
            else
                MessageBox.Show("Laser补偿文件更新失败");
        }

        private void btn_DebugNGDrawerUnlock_Click(object sender, EventArgs e)
        {
            if (btn_DebugNGDrawerUnlock.Text == "NG抽屉解锁")
            {
                if (logicModule.NGDrawerUnlock())
                    btn_DebugNGDrawerUnlock.Text = "NG抽屉锁扣";
            }
            else
            {
                if (logicModule.NGDrawerLock())
                    btn_DebugNGDrawerUnlock.Text = "NG抽屉解锁";
            }
        }

        private void btn_DebugLoadNullUnlock_Click(object sender, EventArgs e)
        {
            if (btn_DebugLoadNullUnlock.Text == "上料空解锁")
            {
                if (logicModule.LoadNullTrayDrawerUnlock())
                    btn_DebugLoadNullUnlock.Text = "上料空锁扣";
            }
            else
            {
                if (logicModule.LoadNullTrayDrawerLock())
                    btn_DebugLoadNullUnlock.Text = "上料空解锁";
            }
        }

        private void btn_DebugLoadFullUnlock_Click(object sender, EventArgs e)
        {
            if (btn_DebugLoadFullUnlock.Text == "上料满解锁")
            {
                if (logicModule.LoadFullTrayDrawerUnlock())
                    btn_DebugLoadFullUnlock.Text = "上料满锁扣";
            }
            else
            {
                if (logicModule.LoadFullTrayDrawerLock())
                    btn_DebugLoadFullUnlock.Text = "上料满解锁";
            }
        }

        private void btn_DebugUnloadNullUnlock_Click(object sender, EventArgs e)
        {
            if (btn_DebugUnloadNullUnlock.Text == "下料空解锁")
            {
                if (logicModule.UnloadNullTrayDrawerUnlock())
                    btn_DebugUnloadNullUnlock.Text = "下料空锁扣";
            }
            else
            {
                if (logicModule.UnloadNullTrayDrawerLock())
                    btn_DebugUnloadNullUnlock.Text = "下料空解锁";
            }
        }

        private void btn_DebugUnloadFullUnlock_Click(object sender, EventArgs e)
        {
            if (btn_DebugUnloadFullUnlock.Text == "下料满解锁")
            {
                if (logicModule.UnloadFullTrayDrawerUnlock())
                    btn_DebugUnloadFullUnlock.Text = "下料满锁扣";
            }
            else
            {
                if (logicModule.UnloadFullTrayDrawerLock())
                    btn_DebugUnloadFullUnlock.Text = "下料满解锁";
            }
        }

        private void btn_DebugSuckAxis_Click(object sender, EventArgs e)
        {
            if (btn_DebugSuckAxis.Text == "横移轴整体测试开始")
            {
                btn_DebugSuckAxis.Text = "横移轴整体测试停止";

                DebugThread = new Thread(new ThreadStart(logicModule.SuckAxisMoveThread), 1024);
                DebugThread.IsBackground = true;
                logicModule.debugThreadSuckAxis = true;
                logicModule.AutoRunActive = true;
                logicModule.SuckAxisMoveEnable = true;
                logicModule.SuckAxisMoveFinished = false;
                DebugThread.Start();
            }
            else
            {
                btn_DebugSuckAxis.Text = "横移轴整体测试开始";
                logicModule.AutoRunActive = false;
                logicModule.SuckAxisMoveCircleCount = 0;
                logicModule.SuckAxisMoveStep = 0;
                logicModule.debugThreadSuckAxis = false;
                logicModule.SuckAxisMoveFinished = false;
                logicModule.SuckAxisMoveEnable = false;
                DebugThread.Abort();
                //logicModule.SystemReset();
            }
        }

        private void btn_LoadTraySwitch_Click(object sender, EventArgs e)
        {
            if (btn_LoadTraySwitch.Text == "替换上料Tray盘开始")
            {
                btn_LoadTraySwitch.Text = "替换上料Tray盘停止";
                DebugThread = new Thread(new ThreadStart(logicModule.LoadTrayThread), 1024);
                DebugThread.IsBackground = true;

                logicModule.debugThreadLoadTraySwitch = true;
                logicModule.AutoRunActive = true;
                logicModule.LoadTrayEnable = true;
                logicModule.LoadTrayFinished = false;
                DebugThread.Start();
            }
            else
            {
                btn_LoadTraySwitch.Text = "替换上料Tray盘开始";
                logicModule.AutoRunActive = false;
                logicModule.LoadTrayCircleCount = 0;
                logicModule.LoadTrayStep = 0;
                logicModule.debugThreadLoadTraySwitch = false;
                logicModule.LoadTrayFinished = false;
                logicModule.LoadTrayEnable = false;
                DebugThread.Abort();
                //logicModule.SystemReset();
            }
        }

        private void btn_UnloadTraySwitch_Click(object sender, EventArgs e)
        {
            if (btn_UnloadTraySwitch.Text == "替换下料Tray盘开始")
            {
                btn_UnloadTraySwitch.Text = "替换下料Tray盘停止";
                DebugThread = new Thread(new ThreadStart(logicModule.UnloadTrayThread), 1024);
                DebugThread.IsBackground = true;

                //logicModule.debugThreadUnloadTraySwitch = true;
                logicModule.AutoRunActive = true;
                logicModule.UnloadTrayEnable = true;
                logicModule.UnloadTrayFinished = false;
                DebugThread.Start();
            }
            else
            {
                btn_UnloadTraySwitch.Text = "替换下料Tray盘开始";
                logicModule.AutoRunActive = false;
                logicModule.UnloadTrayCircleCount = 0;
                logicModule.UnloadTrayStep = 0;
                logicModule.debugThreadUnloadTraySwitch = false;
                logicModule.UnloadTrayFinished = false;
                logicModule.UnloadTrayEnable = false;
                DebugThread.Abort();
                //logicModule.SystemReset();
            }
        }

        private void btn_DebugLoadTraySwitchRollInPos_Click(object sender, EventArgs e)
        {
            logicModule.debugLoadRollSense = true;
        }

        private void btn_ApplyEnterSetting_Click(object sender, EventArgs e)
        {
            logicModule.AutoRunPartAStationEnable[0] = chk_StationAEnter.Checked;
            logicModule.AutoRunPartAStationEnable[1] = chk_StationBEnter.Checked;
            logicModule.AutoRunPartAStationEnable[2] = chk_StationCEnter.Checked;
        }

        private void btn_SuckPinTest_Click(object sender, EventArgs e)
        {
            if (btn_SuckPinTest.Text == "Pin吸取测试开始")
            {
                debugSuckPinTestThread = new Thread(new ThreadStart(logicModule.SuckPinTest), 1024);
                debugSuckPinTestThread.IsBackground = true;
                logicModule.SuckPinTestStart = true;
                debugSuckPinTestThread.Start();

                btn_SuckPinTest.Text = "Pin吸取测试关闭";
            }
            else
            {
                logicModule.SuckPinTestStart = false;
                debugSuckPinTestThread.Abort();

                btn_SuckPinTest.Text = "Pin吸取测试开始";
            }


        }

        private void btn_DebugUnloadGantryZStretch_Click(object sender, EventArgs e)
        {
            if (btn_DebugUnloadGantryZStretch.Text == "龙门下料Z轴下降")
            {
                if (logicModule.UnloadGantryZStretch())
                {
                    btn_DebugUnloadGantryZStretch.Text = "龙门下料Z轴上升";
                    MessageBox.Show("龙门下料Z轴下降成功");
                }
                else
                    MessageBox.Show("龙门下料Z轴下降失败");
            }
            else
            {
                if (logicModule.UnloadGantryZRetract())
                {
                    btn_DebugUnloadGantryZStretch.Text = "龙门下料Z轴下降";
                    MessageBox.Show("龙门下料Z轴上升成功");
                }
                else
                    MessageBox.Show("龙门下料Z轴上升失败");
            }
        }

        private void btn_DebugUnloadGantryPistonStretch_Click(object sender, EventArgs e)
        {
            if (btn_DebugUnloadGantryPistonStretch.Text == "龙门下料气缸下降")
            {
                if (logicModule.UnloadGantryAllPistonStretch())
                {
                    btn_DebugUnloadGantryPistonStretch.Text = "龙门下料气缸上升";
                    MessageBox.Show("龙门下料气缸下降成功");
                }
                else
                    MessageBox.Show("龙门下料气缸下降失败");
            }
            else
            {
                if (logicModule.UnloadGantryAllPistonRetract())
                {
                    btn_DebugUnloadGantryPistonStretch.Text = "龙门下料气缸下降";
                    MessageBox.Show("龙门下料气缸上升成功");
                }
                else
                    MessageBox.Show("龙门下料气缸上升失败");
            }
        }

        private void btn_ReloadConfig_Click(object sender, EventArgs e)
        {
            string errcode = null;
            if (logicModule.JudgeUserLevel(50) == true)
            {
                if (logicModule.InitSystemConfig(ref errcode))
                    MessageBox.Show("参数重新加载成功");
                else
                    MessageBox.Show("参数重新加载失败");
                return;
            }

            if (logicModule.CurStatus != (int)STATUS.AUTO_STATUS && logicModule.CurStatus != (int)STATUS.PAUSE_STATUS)
            {
                if (logicModule.InitSystemConfig(ref errcode))
                    MessageBox.Show("参数重新加载成功");
                else
                    MessageBox.Show("参数重新加载失败");
            }
            else
                MessageBox.Show("设备运行中，无法修改运行配置！");
        }

        private void btn_ApplyBackLash_Click(object sender, EventArgs e)
        {
            Int32.TryParse(txt_CCDBackLashPulseNo.Text, out logicModule.CCDBackLashPulseNo);
            logicModule.EnableBackLash = ckb_EnableBackLash.Checked;
        }

        private void btn_ClearUnloadTray_Click(object sender, EventArgs e)
        {
            MessageBox.Show("该功能暂未开放，敬请期待！");
            //if (btn_ClearUnloadTray.Text == "清理下料空满Tray开始")
            //{
            //    if (clearUnloadTrayThread != null)
            //    {
            //        clearUnloadTrayThread.Abort();
            //    }
            //    clearUnloadTrayThread = new Thread(new ThreadStart(logicModule.ClearUnloadTray));
            //    clearUnloadTrayThread.IsBackground = true;
            //    clearUnloadTrayThread.Start();
            //    btn_ClearUnloadTray.Text = "清理下料空满Tray结束";
            //}
            //else
            //{
            //    clearUnloadTrayThread.Abort();
            //    btn_ClearUnloadTray.Text = "清理下料空满Tray开始";
            //}
        }

        private void btn_ShowDownTime_Click(object sender, EventArgs e)
        {
            if (btn_ShowDownTime.Text == "显示DownTime")
            {
                ShowDownTimeRecord();
                btn_ShowDownTime.Text = "隐藏DownTime";
            }
            else
            {
                HideDownTimeRecord();
                btn_ShowDownTime.Text = "显示DownTime";
            }
        }

        private void btn_LaserMiniInit_Click(object sender, EventArgs e)
        {
            string errcode;
            if (!logicModule.LaserMiniInit(out errcode))
                MessageBox.Show(errcode);
        }

        private void btn_TestCCDInfluence_Click(object sender, EventArgs e)
        {
            ccdInfluenceForm.ShowInfo();
            ccdInfluenceForm.Show();

        }

        private void btn_ApplyDelay_Click(object sender, EventArgs e)
        {
            int.TryParse(txt_DelayAfterLightChange.Text, out logicModule.DelayAfterLightChange);
            int.TryParse(txt_DelayBeforeLightChange.Text, out logicModule.DelayBeforeLightChange);
            int.TryParse(txt_DelayAfterSecondT1.Text, out logicModule.DelayAfterSecondT1);
            int.TryParse(txt_DelayAfterLineMove.Text, out logicModule.DelayAfterLineMove);

            logicModule.WriteCCDMoveLog("DelayBeforeLightChange=" + txt_DelayBeforeLightChange.Text + "DelayAfterLightChange=" + txt_DelayAfterLightChange.Text + "DelayAfterSecondT1=" + txt_DelayAfterSecondT1.Text + "DelayAfterLineMove=" + txt_DelayAfterLineMove.Text);
            MessageBox.Show("CCD延时参数修改成功");
        }

        private void btn_ApplyLaserDiff_Click(object sender, EventArgs e)
        {
            double.TryParse(txt_LaserDiff.Text, out logicModule.LaserDiff);

            MessageBox.Show("参数修改成功");
        }

        private void chk_EnableMonitorCCDCom_CheckedChanged(object sender, EventArgs e)
        {
            logicModule.gbEnableMonitorCCDCommunication = chk_EnableMonitorCCDCom.Checked;
        }

        private void btn_ApplyImprovePassRatio_Click(object sender, EventArgs e)
        {
            logicModule.isEnableImprovePassRatio = chk_EnableImprovePassRatio.Checked;
            double.TryParse(txt_PassRatioMaxMargin.Text, out logicModule.PassRatioMaxMargin);
        }

        private void chk_ApplyRandomLaserResult_CheckedChanged(object sender, EventArgs e)
        {
            logicModule.LaserRandomResult = chk_ApplyRandomLaserResult.Checked;
        }

        private void btn_AllMultiRepeatTest_Click(object sender, EventArgs e)
        {
            RepeatTestStru temp = new RepeatTestStru(cbx_RepeatTestStationNo.SelectedIndex, cbx_RepeatTestStep.SelectedIndex, Convert.ToInt32(txt_RepeatTestTime.Text));

            if (btn_AllMultiRepeatTest.Text == "12穴重复性测试开始")
            {
                if (repeatTestAllMultiHoleThread != null)
                    repeatTestAllMultiHoleThread.Abort();
                repeatTestAllMultiHoleThread = new Thread(new ParameterizedThreadStart(logicModule.RepeatTestAllMultiHole));
                repeatTestAllMultiHoleThread.IsBackground = true;
                repeatTestAllMultiHoleThread.Start(temp);
                btn_AllMultiRepeatTest.Text = "12穴重复性测试结束";
            }
            else
            {
                logicModule.AutoRunActive = false;
                logicModule.CCDChecking = false;
                logicModule.debugThreadB = false;
                logicModule.debugThreadC = false;
                logicModule.AutoRunEnablePartB = false;
                logicModule.AutoRunEnablePartC = false;
                logicModule.AutoRunPartAStretchFinish = false;
                logicModule.RecvCount = 0;
                logicModule.CCDRawDataList.Clear();
                logicModule.CCDRecvStatusList.Clear();
                repeatTestAllMultiHoleThread.Abort();
                btn_AllMultiRepeatTest.Text = "12穴重复性测试开始";
            }
        }

        private void btn_ClearMainAxisWorkPiece_Click(object sender, EventArgs e)
        {
            if (btn_ClearMainAxisWorkPiece.Text == "清理转盘内物料开始")
            {
                btn_ClearMainAxisWorkPiece.Text = "清理转盘内物料结束";
                if (clearMainAxisWorkPieceThread != null)
                    clearMainAxisWorkPieceThread.Abort();
                clearMainAxisWorkPieceThread = new Thread(new ThreadStart(logicModule.ClearMainAxisWorkPiece), 1024);
                clearMainAxisWorkPieceThread.IsBackground = true;
                clearMainAxisWorkPieceThread.Start();
            }
            else
            {
                clearMainAxisWorkPieceThread.Abort();
                btn_ClearMainAxisWorkPiece.Text = "清理转盘内物料开始";
            }
        }

        private void btn_ClearLoadTray_Click(object sender, EventArgs e)
        {
            MessageBox.Show("该功能暂未开放，敬请期待！");
            //if (btn_ClearLoadTray.Text == "清理上料空满Tray开始")
            //{
            //    if (clearLoadTrayThread != null)
            //    {
            //        clearLoadTrayThread.Abort();
            //    }
            //    clearLoadTrayThread = new Thread(new ThreadStart(logicModule.ClearLoadTray));
            //    clearLoadTrayThread.IsBackground = true;
            //    clearLoadTrayThread.Start();
            //    btn_ClearLoadTray.Text = "清理上料空满Tray结束";
            //}
            //else
            //{
            //    clearLoadTrayThread.Abort();
            //    btn_ClearLoadTray.Text = "清理上料空满Tray开始";
            //}
        }

        private void btn_XgxTestOnceClick_Click(object sender, EventArgs e)
        {
            RepeatTestStru temp = new RepeatTestStru(cbx_RepeatTestStationNo.SelectedIndex, cbx_RepeatTestStep.SelectedIndex, Convert.ToInt32(txt_RepeatTestTime.Text));

            if (btn_XgxTestOnceClick.Text == "相关性测试开始")
            {
                if (repeatTestOnceXgxThread != null)
                {
                    repeatTestOnceXgxThread.Abort();
                }
                repeatTestOnceXgxThread = new Thread(new ParameterizedThreadStart(logicModule.RepeatTestOnceXgx));
                repeatTestOnceXgxThread.IsBackground = true;
                repeatTestOnceXgxThread.Start(temp);
                btn_RepeatTestOnce.Text = "相关性测试结束";
            }
            else
            {
                logicModule.CCDChecking = false;
                logicModule.debugThreadB = false;
                logicModule.debugThreadC = false;
                logicModule.AutoRunPartAStretchFinish = false;
                logicModule.RecvCount = 0;
                logicModule.CCDRawDataList.Clear();
                logicModule.CCDRecvStatusList.Clear();
                repeatTestOnceXgxThread.Abort();
                btn_XgxTestOnceClick.Text = "相关性测试开始";
            }
        }

        private void btn_MultiRepeatTest_Click(object sender, EventArgs e)
        {
            RepeatTestStru temp = new RepeatTestStru(cbx_RepeatTestStationNo.SelectedIndex, cbx_RepeatTestStep.SelectedIndex, Convert.ToInt32(txt_RepeatTestTime.Text));

            if (btn_MultiRepeatTest.Text == "多穴重复性测试开始")
            {
                if (repeatTestMultiHoleThread != null)
                    repeatTestMultiHoleThread.Abort();
                repeatTestMultiHoleThread = new Thread(new ParameterizedThreadStart(logicModule.RepeatTestMultiHole));
                repeatTestMultiHoleThread.IsBackground = true;
                repeatTestMultiHoleThread.Start(temp);
                btn_MultiRepeatTest.Text = "多穴重复性测试结束";
            }
            else
            {
                logicModule.AutoRunActive = false;
                logicModule.CCDChecking = false;
                logicModule.debugThreadB = false;
                logicModule.debugThreadC = false;
                logicModule.AutoRunEnablePartB = false;
                logicModule.AutoRunEnablePartC = false;
                logicModule.AutoRunPartAStretchFinish = false;
                logicModule.RecvCount = 0;
                logicModule.CCDRawDataList.Clear();
                logicModule.CCDRecvStatusList.Clear();
                repeatTestMultiHoleThread.Abort();
                btn_MultiRepeatTest.Text = "多穴重复性测试开始";
            }
        }

        private void btn_RepeatTestOnce_Click(object sender, EventArgs e)
        {
            RepeatTestStru temp = new RepeatTestStru(cbx_RepeatTestStationNo.SelectedIndex, cbx_RepeatTestStep.SelectedIndex, Convert.ToInt32(txt_RepeatTestTime.Text));

            if (btn_RepeatTestOnce.Text == "单穴重复性测试开始")
            {
                repeatTestOnceThread = new Thread(new ParameterizedThreadStart(logicModule.RepeatTestOnce));
                repeatTestOnceThread.IsBackground = true;
                repeatTestOnceThread.Start(temp);
                btn_RepeatTestOnce.Text = "单穴重复性测试结束";
            }
            else
            {
                logicModule.CCDChecking = false;
                logicModule.debugThreadB = false;
                logicModule.debugThreadC = false;
                logicModule.AutoRunPartAStretchFinish = false;
                logicModule.RecvCount = 0;
                logicModule.CCDRawDataList.Clear();
                logicModule.CCDRecvStatusList.Clear();
                repeatTestOnceThread.Abort();
                btn_RepeatTestOnce.Text = "单穴重复性测试开始";
            }
        }

        private void btn_DebugUnloadGantrySuckerSuck_Click(object sender, EventArgs e)
        {
            if (btn_DebugUnloadGantrySuckerSuck.Text == "龙门下料真空吸")
            {
                if (logicModule.UnloadGantryAllSuckerSuck())
                    MessageBox.Show("龙门下料真空吸成功");
                else
                    MessageBox.Show("龙门下料真空吸失败");
                btn_DebugUnloadGantrySuckerSuck.Text = "龙门下料真空破";
            }
            else
            {
                if (logicModule.UnloadGantryAllSuckerBreak())
                    MessageBox.Show("龙门下料真空破成功");
                else
                    MessageBox.Show("龙门下料真空破失败");
                btn_DebugUnloadGantrySuckerSuck.Text = "龙门下料真空吸";
            }
        }

        private void btn_DebugUnloadTraySwitchRollInPos_Click(object sender, EventArgs e)
        {
            logicModule.debugUnloadRollSense = true;
        }

        private void btn_DebugSupplyPlaceTest_Click(object sender, EventArgs e)
        {
            //int[] CheckResult = new int[8];
            for (int i = 0; i < 8; i++)
            {
                if (ChkDebugRegion1Condition[i].Checked)
                    logicModule.SupplyRegion1Condition[i] = 1;
                else
                    logicModule.SupplyRegion1Condition[i] = 0;

                if (ChkDebugRegion2Condition[i].Checked)
                    logicModule.SupplyRegion2Condition[i] = 1;
                else
                    logicModule.SupplyRegion2Condition[i] = 0;
            }

            //for (int i = 0; i < 8; i++)
            //{
            //    if (ChkDebugUnloadModuleCheckResult[i].Checked)
            //        logicModule.UnloadGantryCheckResult[i] = 1;
            //    else
            //        logicModule.UnloadGantryCheckResult[i] = -1;
            //}
            //logicModule.CurUnloadFullTraySeq = Convert.ToInt32(txt_UnloadGantrySeq.Text);
            //logicModule.debugThreadUnloadGantry = true;

            logicModule.SupplySuckTest();

            //logicModule.debugThreadUnloadGantry = false;
            //logicModule.CurUnloadFullTraySeq = 0;
            logicModule.SupplyRegion1Condition = new int[8];
            logicModule.SupplyRegion2Condition = new int[8];
            logicModule.UnloadGantryCheckResult = new int[8];
        }

        private void btn_DebugLoadModule_Click(object sender, EventArgs e)
        {
            if (btn_DebugLoadModule.Text == "上料模组测试开始")
            {
                btn_DebugLoadModule.Text = "上料模组测试停止";
                DebugThread = new Thread(new ThreadStart(logicModule.LoadModuleThread), 1024);
                DebugThread.IsBackground = true;

                logicModule.debugThreadLoadModule = true;
                logicModule.AutoRunActive = true;
                logicModule.LoadModuleEnable = true;
                logicModule.LoadModuleFinished = false;
                DebugThread.Start();
            }
            else
            {
                btn_DebugLoadModule.Text = "上料模组测试开始";
                logicModule.AutoRunActive = false;

                logicModule.LoadModuleCircleCount = 0;
                logicModule.LoadModuleStep = 0;
                logicModule.debugThreadLoadModule = false;
                logicModule.LoadModuleFinished = false;
                logicModule.LoadModuleEnable = false;
                DebugThread.Abort();
                //logicModule.SystemReset();
            }
        }

        private void btn_DebugUnloadModule_Click(object sender, EventArgs e)
        {
            if (btn_DebugUnloadModule.Text == "下料模组测试开始")
            {
                btn_DebugUnloadModule.Text = "下料模组测试停止";
                DebugThread = new Thread(new ThreadStart(logicModule.UnloadModuleThread), 1024);
                DebugThread.IsBackground = true;

                logicModule.debugThreadUnloadModule = true;
                logicModule.AutoRunActive = true;
                logicModule.UnloadModuleEnable = true;
                logicModule.UnloadModuleFinished = false;
                DebugThread.Start();
            }
            else
            {
                btn_DebugUnloadModule.Text = "下料模组测试开始";
                logicModule.AutoRunActive = false;

                logicModule.UnloadModuleCircleCount = 0;
                logicModule.UnloadModuleStep = 0;
                logicModule.debugThreadUnloadModule = false;
                logicModule.UnloadModuleFinished = false;
                logicModule.UnloadModuleEnable = false;
                DebugThread.Abort();
                //logicModule.SystemReset();
            }
        }

        private void btn_DebugLoadGantryMove_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txt_LoadGantryStep.Text))
            { return; }

            string testIdxStr = txt_LoadGantryStep.Text;
            int testIdx = -1;
            int.TryParse(testIdxStr, out testIdx);
            if (testIdx < 0 || testIdx > 14)
            { return; }

            logicModule.debugThreadLoadGantry = true;
            logicModule.LoadGantrySuckAllWorkPiece(ref testIdx);
            logicModule.debugThreadLoadGantry = false;
        }

        private void btn_DebugUnloadGantryMove_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txt_UnloadGantryStep.Text))
            { return; }

            string testIdxStr = txt_UnloadGantryStep.Text;
            int testIdx = -1;
            int.TryParse(testIdxStr, out testIdx);
            if (testIdx < 0 || testIdx > 14)
            { return; }

            logicModule.debugThreadUnloadGantry = true;
            logicModule.CurUnloadFullTraySeq = testIdx;
            logicModule.UnloadGantrySuckAndPlace();
            logicModule.debugThreadUnloadGantry = false;
            logicModule.CurUnloadFullTraySeq = 0;
        }

        private void btn_LoadNullTrayRollSwitch_Click(object sender, EventArgs e)
        {
            logicModule.LoadNullTrayHome();
        }

        private void btn_LoadFullTrayRollSwitch_Click(object sender, EventArgs e)
        {
            logicModule.LoadFullTrayHome();
        }

        private void btn_WorkpieceInPos_Click(object sender, EventArgs e)
        {
            logicModule.debugWorkpieceInPos = true;
        }

        private void btn_DebugMainAxisInit_Click(object sender, EventArgs e)
        {
            Thread InitMainAxisTestThread = new Thread(new ThreadStart(logicModule.InitMainAxisTest), 1024);
            InitMainAxisTestThread.IsBackground = true;
            InitMainAxisTestThread.Start();
        }

        private void btn_AllRunTest_Click(object sender, EventArgs e)
        {
            if (btn_AllRunTest.Text == "联动测试开始")
            {
                btn_AllRunTest.Text = "联动测试结束";
                logicModule.AutoRunActive = true;

                if (chk_LoadTraySwitch.Checked)
                {
                    AllRunLoadTraySwitch = new Thread(new ThreadStart(logicModule.LoadTrayThread), 1024);
                    AllRunLoadTraySwitch.IsBackground = true;
                    logicModule.debugThreadLoadTraySwitch = true;
                    logicModule.LoadTrayEnable = true;
                    logicModule.LoadTrayFinished = true;
                    AllRunLoadTraySwitch.Start();
                }

                if (chk_UnloadTraySwitch.Checked)
                {
                    AllRunUnloadTraySwitch = new Thread(new ThreadStart(logicModule.UnloadTrayThread), 1024);
                    AllRunUnloadTraySwitch.IsBackground = true;
                    logicModule.debugThreadUnloadTraySwitch = true;
                    logicModule.UnloadTrayEnable = true;
                    logicModule.UnloadTrayFinished = true;
                    AllRunUnloadTraySwitch.Start();
                }

                if (chk_LoadGantry.Checked)
                {
                    AllRunLoadGantry = new Thread(new ThreadStart(logicModule.LoadGantryThread), 1024);
                    AllRunLoadGantry.IsBackground = true;
                    //logicModule.debugThreadLoadGantry = true;
                    logicModule.LoadGantryEnable = true;
                    logicModule.LoadGantrySuckFinished = false;
                    AllRunLoadGantry.Start();
                }

                if (chk_UnloadGantry.Checked)
                {
                    AllRunUnloadGantry = new Thread(new ThreadStart(logicModule.UnloadGantryThread), 1024);
                    AllRunUnloadGantry.IsBackground = true;
                    logicModule.debugThreadUnloadGantry = true;
                    logicModule.UnloadGantryEnable = true;
                    logicModule.UnloadGantrySuckFinished = false;
                    AllRunUnloadGantry.Start();
                }

                if (chk_LoadModule.Checked)
                {
                    AllRunLoadModule = new Thread(new ThreadStart(logicModule.LoadModuleThread), 1024);
                    AllRunLoadModule.IsBackground = true;
                    logicModule.debugThreadLoadModule = false;
                    logicModule.LoadModuleEnable = true;
                    logicModule.LoadModuleFinished = true;
                    AllRunLoadModule.Start();
                }

                if (chk_UnloadModule.Checked)
                {
                    AllRunUnloadModule = new Thread(new ThreadStart(logicModule.UnloadModuleThread), 1024);
                    AllRunUnloadModule.IsBackground = true;
                    logicModule.debugThreadUnloadModule = false;
                    logicModule.UnloadModuleEnable = true;
                    logicModule.UnloadModuleFinished = false;
                    AllRunUnloadModule.Start();
                }

                if (chk_SuckAxis.Checked)
                {
                    AllRunSuckAxisMove = new Thread(new ThreadStart(logicModule.SuckAxisMoveThread), 1024);
                    AllRunSuckAxisMove.IsBackground = true;
                    logicModule.debugThreadSuckAxis = false;
                    logicModule.SuckAxisMoveEnable = true;
                    logicModule.SuckAxisMoveFinished = false;
                    AllRunSuckAxisMove.Start();
                }

                if (chk_MainAxis.Checked)
                {
                    AllRunMainAxis = new Thread(new ThreadStart(logicModule.MainAxisMoveThread), 1024);
                    AllRunMainAxis.IsBackground = true;
                    //logicModule.debugThreadMainAxis = true;
                    logicModule.AutoRunEnableMainAxis = true;
                    logicModule.MainAxisMoveFinish = true;
                    AllRunMainAxis.Start();
                }

                if (chk_AutoRunPartA.Checked)
                {
                    AllRunPartA = new Thread(new ThreadStart(logicModule.AutoRunPartAThread), 1024);
                    AllRunPartA.IsBackground = true;
                    //logicModule.debugThreadA = true;
                    logicModule.AutoRunEnablePartA = true;
                    logicModule.AutoRunPartAFinished = true;
                    AllRunPartA.Start();
                }

                if (chk_CCDCheck.Checked)
                {
                    AllRunCCDCheck = new Thread(new ThreadStart(logicModule.AutoRunPartBThread), 1024);
                    AllRunCCDCheck.IsBackground = true;
                    //logicModule.debugThreadB = true;
                    logicModule.AutoRunEnablePartB = true;
                    logicModule.AutoRunPartBFinished = true;
                    AllRunCCDCheck.Start();
                }

                if (chk_LaserCheck.Checked)
                {
                    AllRunLaserCheck = new Thread(new ThreadStart(logicModule.AutoRunPartCThread), 1024);
                    AllRunLaserCheck.IsBackground = true;
                    //logicModule.debugThreadC = true;
                    logicModule.AutoRunEnablePartC = true;
                    logicModule.AutoRunPartCFinished = true;
                    AllRunLaserCheck.Start();
                }

                if (chk_UnloadGantryPlaceAllWorkPiece.Checked)
                {
                    AllRunUnloadGantryPlaceAllWorkPiece = new Thread(new ThreadStart(logicModule.UnloadGantryPlaceAllWorkPieceThread), 1024);
                    AllRunUnloadGantryPlaceAllWorkPiece.IsBackground = true;
                    logicModule.debugThreadUnloadGantryPlaceAllWorkPiece = true;
                    logicModule.UnloadGantryPlaceAllWorkPieceEnable = true;
                    logicModule.UnloadGantryPlaceAllWorkPieceFinish = true;
                    AllRunUnloadGantryPlaceAllWorkPiece.Start();
                }

                chk_LoadGantry.Enabled = false;
                chk_UnloadGantry.Enabled = false;
                chk_LoadModule.Enabled = false;
                chk_UnloadModule.Enabled = false;
                chk_LoadTraySwitch.Enabled = false;
                chk_UnloadTraySwitch.Enabled = false;
                chk_AutoRunPartA.Enabled = false;
                chk_CCDCheck.Enabled = false;
                chk_LaserCheck.Enabled = false;
                chk_MainAxis.Enabled = false;
                chk_SuckAxis.Enabled = false;
                chk_UnloadGantryPlaceAllWorkPiece.Enabled = false;
            }
            else
            {
                btn_AllRunTest.Text = "联动测试开始";
                logicModule.AutoRunActive = false;

                if (chk_LoadTraySwitch.Checked)
                {
                    logicModule.debugThreadLoadTraySwitch = false;
                    logicModule.LoadTrayEnable = false;
                    logicModule.LoadTrayFinished = false;
                    logicModule.LoadTrayCircleCount = 0;
                    AllRunLoadTraySwitch.Abort();
                }

                if (chk_UnloadTraySwitch.Checked)
                {
                    logicModule.debugThreadUnloadTraySwitch = false;
                    logicModule.UnloadTrayEnable = false;
                    logicModule.UnloadTrayFinished = false;
                    logicModule.UnloadTrayCircleCount = 0;
                    AllRunUnloadTraySwitch.Abort();
                }

                if (chk_LoadGantry.Checked)
                {
                    logicModule.debugThreadLoadGantry = false;
                    logicModule.LoadGantryEnable = false;
                    logicModule.LoadGantrySuckFinished = false;
                    logicModule.LoadGantryCircleCount = 0;
                    logicModule.CurLoadFullTraySeq = 0;
                    AllRunLoadGantry.Abort();
                }

                if (chk_UnloadGantry.Checked)
                {
                    logicModule.debugThreadUnloadGantry = false;
                    logicModule.UnloadGantryEnable = false;
                    logicModule.UnloadGantrySuckFinished = false;
                    logicModule.UnloadGantryCircleCount = 0;
                    AllRunUnloadGantry.Abort();
                }

                if (chk_LoadModule.Checked)
                {
                    logicModule.debugThreadLoadModule = false;
                    logicModule.LoadModuleEnable = false;
                    logicModule.LoadModuleFinished = false;
                    logicModule.LoadModuleCircleCount = 0;
                    AllRunLoadModule.Abort();
                }

                if (chk_UnloadModule.Checked)
                {
                    logicModule.debugThreadUnloadModule = false;
                    logicModule.UnloadModuleEnable = false;
                    logicModule.UnloadModuleFinished = false;
                    logicModule.UnloadModuleCircleCount = 0;
                    AllRunUnloadModule.Abort();
                }

                if (chk_SuckAxis.Checked)
                {
                    logicModule.debugThreadSuckAxis = false;
                    logicModule.SuckAxisMoveEnable = false;
                    logicModule.SuckAxisMoveFinished = false;
                    logicModule.SuckAxisMoveCircleCount = 0;
                    logicModule.SuckAxisPlace1Finish = false;
                    logicModule.SuckAxisPlace2Finish = false;
                    AllRunSuckAxisMove.Abort();
                }

                if (chk_MainAxis.Checked)
                {
                    logicModule.debugThreadMainAxis = false;
                    logicModule.AutoRunEnableMainAxis = false;
                    logicModule.MainAxisMoveFinish = false;
                    logicModule.AutoRunMainAxisCircleCount = 0;
                    AllRunMainAxis.Abort();
                }

                if (chk_AutoRunPartA.Checked)
                {
                    logicModule.debugThreadA = false;
                    logicModule.AutoRunEnablePartA = false;
                    logicModule.AutoRunPartAFinished = false;
                    logicModule.AutoRunPartACircleCount = 0;
                    AllRunPartA.Abort();
                }

                if (chk_CCDCheck.Checked)
                {
                    logicModule.debugThreadB = false;
                    logicModule.AutoRunEnablePartB = false;
                    logicModule.AutoRunPartBFinished = false;
                    logicModule.AutoRunPartBCircleCount = 0;
                    AllRunCCDCheck.Abort();
                }

                if (chk_LaserCheck.Checked)
                {
                    logicModule.debugThreadC = false;
                    logicModule.AutoRunEnablePartC = false;
                    logicModule.AutoRunPartCFinished = false;
                    logicModule.AutoRunPartCCircleCount = 0;
                    AllRunLaserCheck.Abort();
                }

                if (chk_UnloadGantryPlaceAllWorkPiece.Checked)
                {
                    logicModule.debugThreadUnloadGantryPlaceAllWorkPiece = false;
                    logicModule.UnloadGantryPlaceAllWorkPieceEnable = false;
                    logicModule.UnloadGantryPlaceAllWorkPieceFinish = false;
                    logicModule.UnloadGantryPlaceAllWorkPieceCircleCount = 0;
                    AllRunUnloadGantryPlaceAllWorkPiece.Abort();
                }

                logicModule.ResetFlag();

                chk_LoadGantry.Enabled = true;
                chk_UnloadGantry.Enabled = true;
                chk_LoadModule.Enabled = true;
                chk_UnloadModule.Enabled = true;
                chk_LoadTraySwitch.Enabled = true;
                chk_UnloadTraySwitch.Enabled = true;
                chk_AutoRunPartA.Enabled = true;
                chk_CCDCheck.Enabled = true;
                chk_LaserCheck.Enabled = true;
                chk_MainAxis.Enabled = true;
                chk_SuckAxis.Enabled = true;
            }
        }

        private void btn_UnloadNullTrayRollSwitch_Click(object sender, EventArgs e)
        {
            logicModule.UnloadNullTrayHome();
        }

        private void btn_UnloadFullTrayRollSwitch_Click(object sender, EventArgs e)
        {
            logicModule.UnloadFullTrayHome();
        }

        private void btn_DebugMainAxis_Click(object sender, EventArgs e)
        {
            if (btn_DebugMainAxis.Text == "转盘整体测试开始")
            {
                btn_DebugMainAxis.Text = "转盘整体测试结束";

                DebugMainAxisThread = new Thread(new ThreadStart(logicModule.MainAxisMoveThread), 1024);
                DebugMainAxisThread.IsBackground = true;
                logicModule.debugThreadMainAxis = true;
                logicModule.AutoRunEnableMainAxis = true;
                logicModule.MainAxisMoveFinish = true;

                DebugPartAThread = new Thread(new ThreadStart(logicModule.AutoRunPartAThread), 1024);
                DebugPartAThread.IsBackground = true;
                logicModule.debugThreadA = true;
                logicModule.AutoRunEnablePartA = true;
                logicModule.AutoRunPartAFinished = true;

                DebugPartBThread = new Thread(new ThreadStart(logicModule.AutoRunPartBThread), 1024);
                DebugPartBThread.IsBackground = true;
                logicModule.debugThreadB = true;
                logicModule.AutoRunEnablePartB = true;
                logicModule.AutoRunPartBFinished = true;


                DebugPartCThread = new Thread(new ThreadStart(logicModule.AutoRunPartCThread), 1024);
                DebugPartCThread.IsBackground = true;
                logicModule.debugThreadC = true;
                logicModule.AutoRunEnablePartC = true;
                logicModule.AutoRunPartCFinished = true;

                logicModule.AutoRunActive = true;
                DebugMainAxisThread.Start();
                DebugPartAThread.Start();
                DebugPartBThread.Start();
                DebugPartCThread.Start();
            }
            else
            {
                btn_DebugMainAxis.Text = "转盘整体测试开始";
                logicModule.AutoRunActive = false;

                logicModule.debugThreadMainAxis = false;
                logicModule.AutoRunEnableMainAxis = false;
                logicModule.AutoRunMainAxisCircleCount = 0;
                logicModule.MainAxisMoveFinish = false;

                logicModule.debugThreadA = false;
                logicModule.AutoRunEnablePartA = false;
                logicModule.AutoRunPartACircleCount = 0;
                logicModule.AutoRunStepA = 0;
                logicModule.AutoRunPartAFinished = false;

                logicModule.debugThreadB = false;
                logicModule.AutoRunEnablePartB = false;
                logicModule.AutoRunPartBCircleCount = 0;
                logicModule.AutoRunStepB = 0;
                logicModule.AutoRunPartBFinished = false;

                logicModule.debugThreadC = false;
                logicModule.AutoRunEnablePartC = false;
                logicModule.AutoRunPartCCircleCount = 0;
                logicModule.AutoRunStepC = 0;
                logicModule.AutoRunPartCFinished = false;

                logicModule.RecvCount = 0;
                logicModule.CCDRawDataList.Clear();
                logicModule.CCDRecvStatusList.Clear();

                DebugMainAxisThread.Abort();
                DebugPartAThread.Abort();
                DebugPartBThread.Abort();
                DebugPartCThread.Abort();

                //logicModule.SystemReset();
            }
        }
    }
}
