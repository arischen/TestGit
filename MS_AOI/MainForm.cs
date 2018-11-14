using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogicControl;
using StructAssemble;
using CsvHelper;
using System.Threading;
using System.Text;
using XmlHelper;
using CommonStruct.LCPrim;
using Wtool;
using OfficeOpenXml;//excel文档操作


namespace MS_AOI
{
    public partial class MainForm : Form
    {
        public LogicModule logicModule = new LogicModule();
        private Label[] _lbResult = new Label[12];
        private TextBox[] tbBarcodeCurrent = new TextBox[12];
        private TextBox[] tbBarcodeResult = new TextBox[12];
        private bool UpdateLaserColumn = true;
        public static bool bMainFormLoadDone = false;
        private int MaxColumnNum = 2000;

        DebugForm debugForm;
        ParaSetting calibForm;
        LaserCloud laserCloudForm;
        UserLogin userLoginForm;
        LoadTraySeqSelect loadTraySeqSelectForm;
        private DistributeChart distributeChartForm;

        private event EventHandler loadEvent; //加载启动条
        Thread StatusMonitorThread;//状态监控线程
        int lastSystemStatus = -1;
        DownTimeRecordStru myDownTimeRecord = new DownTimeRecordStru();
        Thread InitTrayZThread, InitEnvironmentThread;

        public delegate void UpdateObjectDelegate(object sender);
        public event UpdateObjectDelegate UpdateDownTimeInfo;    //更新DownTimeInfo

        CsvWriter CurCCDDataWriter = new CsvWriter();
        CsvWriter CurLaserDataWriter = new CsvWriter();
        string CCDCurDataFilePath = null;
        string LaserCurDataFilePath = null;
        int AutoCheckCTUpdateCount = 1;//自动测量CT更新次数
        double AutoCheckTotalTime = 0;//自动测量CT累积值


        bool isRunning = false;
        Label[] lbl_FaiInfo;
        string RunDataPath;
        TimeSpan AutoStatusStoreTime;
        DateTime AutoStatusStartTime;
        
        public MainForm()
        {
            InitializeComponent();

            lbl_FaiInfo = new Label[] {lbl_FAIInfo0, lbl_FAIInfo1 , lbl_FAIInfo2 , lbl_FAIInfo3 , lbl_FAIInfo4,
                                       lbl_FAIInfo5, lbl_FAIInfo6 , lbl_FAIInfo7 , lbl_FAIInfo8 , lbl_FAIInfo9,
                                       lbl_FAIInfo10, lbl_FAIInfo11 , lbl_FAIInfo12 , lbl_FAIInfo13 , lbl_FAIInfo14,
                                       lbl_FAIInfo15, lbl_FAIInfo16 , lbl_FAIInfo17 , lbl_FAIInfo18 , lbl_FAIInfo19,
                                       lbl_FAIInfo20, lbl_FAIInfo21 , lbl_FAIInfo22 , lbl_FAIInfo23 , lbl_FAIInfo24};

            this.MaximizedBounds = Screen.PrimaryScreen.WorkingArea;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            debugForm = new DebugForm(ref logicModule);
            calibForm = new ParaSetting(ref logicModule);
            laserCloudForm = new LaserCloud(ref logicModule);
            userLoginForm = new UserLogin(ref logicModule);
            loadTraySeqSelectForm = new LoadTraySeqSelect(ref logicModule);
            distributeChartForm = new DistributeChart(ref logicModule);

            isRunning = true;

            Thread MainFormInitThread = new Thread(new ThreadStart(MainInitThread));
            MainFormInitThread.IsBackground = true;
            MainFormInitThread.Start();

            loadEvent += new EventHandler(MainForm_LoadEvent);
            loadEvent.Invoke(new object(), new EventArgs());

            StatusMonitorThread = new Thread(new ThreadStart(StatusMonitor));
            StatusMonitorThread.IsBackground = true;
            StatusMonitorThread.Start();

            UpdateDownTimeInfo+= new UpdateObjectDelegate(MainForm_UpdateDownTimeInfo);

            #region 创建RunData文件
            RunDataPath = @".\Log\RunData-" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + ".txt";

            if (!File.Exists(RunDataPath))
            {
                using (FileStream fs = new FileStream(RunDataPath, FileMode.Create, System.IO.FileAccess.Write))
                {
                    StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
                    sw.WriteLine("Time,TotalNum,ANum,BNum,CNum,DNum");
                    sw.WriteLine(ChangeDateTimeToString(DateTime.Now) + ",0,0,0,0,0");
                    sw.Close();
                    fs.Close();
                }
            }
            #endregion

            CCDCurDataFilePath = "E: \\CCDData\\CCDCurData-0.csv";
            LaserCurDataFilePath = "E: \\3DLaserData\\LaserCurData-0.csv";

            AutoStatusStartTime = DateTime.Now;
            AutoStatusStoreTime = new TimeSpan(0);

            //****************************樊竞明20181010*****************************//
            if (!Directory.Exists (logicModule.DataSummaryBackDirPath ))
            {
                Directory.CreateDirectory(logicModule.DataSummaryBackDirPath);
            }
            if(File.Exists (logicModule.DataSummaryBackFilePath ))
            {
                bool bflag = false;
                logicModule.myFinaldataSummary = XmlSerializerHelper.ReadXML(logicModule.DataSummaryBackFilePath, typeof(FinalDataSummery), out bflag) as FinalDataSummery;
            }
            //*********************************************************************//
        }

        private void MainForm_UpdateDownTimeInfo(object sender)
        {
            if (!this.InvokeRequired)
            {
                DownTimeRecordStru myDownTime = (DownTimeRecordStru)sender;

                dgv_DownTimeRecord.Rows.Add();
                int n = dgv_DownTimeRecord.Rows.Count;

                string StatusStr = null;

                switch (myDownTime.CurStatus)
                {
                    case (int)STATUS.AUTO_STATUS:
                        StatusStr = "Auto自动";
                        break;
                    case (int)STATUS.MANUAL_STATUS:
                        StatusStr = "Manual手动";
                        break;
                    case (int)STATUS.PAUSE_STATUS:
                        StatusStr = "Pause暂停";
                        break;
                    case (int)STATUS.READY_STATUS:
                        StatusStr = "Ready准备";
                        break;
                    case (int)STATUS.STOP_STATUS:
                        StatusStr = "Stop停止";
                        break;
                    default:
                        StatusStr = "N/A";
                        break;
                }

                string StartTimeStr = ChangeDateTimeToString(myDownTime.StartTime);
                string FinishTimeStr = ChangeDateTimeToString(myDownTime.FinishTime);

                double TimeSpanSeconds = myDownTime.DownTimeSpan.Hours * 3600 + myDownTime.DownTimeSpan.Minutes * 60 + myDownTime.DownTimeSpan.Seconds + myDownTime.DownTimeSpan.Milliseconds / 1000.0;

                dgv_DownTimeRecord.Rows[n - 1].Cells[0].Value = n.ToString();
                dgv_DownTimeRecord.Rows[n - 1].Cells[1].Value = StatusStr;
                dgv_DownTimeRecord.Rows[n - 1].Cells[2].Value = StartTimeStr;
                dgv_DownTimeRecord.Rows[n - 1].Cells[3].Value = FinishTimeStr;
                dgv_DownTimeRecord.Rows[n - 1].Cells[4].Value = TimeSpanSeconds.ToString("0.###");
                dgv_DownTimeRecord.Rows[n - 1].Cells[5].Value = myDownTime.ErrorStr;
                logicModule.WriteDownTime(StatusStr, StartTimeStr, FinishTimeStr, TimeSpanSeconds.ToString("0.###"),myDownTime.ErrorStr);
            }
            else
            {
                this.BeginInvoke(new UpdateObjectDelegate(MainForm_UpdateDownTimeInfo), sender);
            }
        }

        private string ChangeDateTimeToString(DateTime myDateTime)
        {
            string temp;
            temp= ((myDateTime.Hour) < 10 ? ("0" + myDateTime.Hour.ToString()) : myDateTime.Hour.ToString()) + ":" +
                  ((myDateTime.Minute) < 10 ? ("0" + myDateTime.Minute.ToString()) : myDateTime.Minute.ToString()) + ":" +
                  ((myDateTime.Second) < 10 ? ("0" + myDateTime.Second.ToString()) : myDateTime.Second.ToString()) + ":" +
                    myDateTime.Millisecond.ToString("000");
            return temp;
        }

        private void MainInitThread()
        {
            debugForm.UpdateAxisInfo += new DebugForm.UpdateObjectDelegate(logicModule.SaveAxisInfo);
            logicModule.UpdateMotionStatus += new LogicModule.UpdateObjectDelegate(debugForm.UpdateMotionStatus);
            logicModule.UpdateMotionStatus += new LogicModule.UpdateObjectDelegate(calibForm.UpdateMotionStatus);
            logicModule.UpdateSysInfo += new LogicModule.UpdateObjectDelegate(debugForm.UpdateSysInfo);
            logicModule.UpdateThreshold += new LogicModule.UpdateObjectDelegate(debugForm.UpdateThreshold);
            logicModule.UpdateWaringLog += new LogicModule.UpdateObjectDelegate(LogicControl_UpdateWaringLog);
            logicModule.UpdateWaringLogNG += new LogicModule.UpdateObjectDelegate(LogicControl_UpdateWaringLogNG);
            logicModule.UpdateCCDFinalResult += new LogicModule.UpdateObjectDelegate(LogicControl_UpdateCCDFinalResult);
            logicModule.UpdateLaserFinalResult += new LogicModule.UpdateObjectDelegate(LogicControl_UpdateLaserFinalResult);
            logicModule.UpdateCTInfo += new LogicModule.UpdateObjectDelegate(LogicControl_UpdateCTInfo);
            logicModule.UpdatePassRatio += new LogicModule.UpdateObjectDelegate(LogicControl_UpdatePassRatio);
            logicModule.UpdateStationAllDataArray += new LogicModule.UpdateObjectDelegate(LogicControl_UpdateStationAllDataArray);
            logicModule.UpdateUserLevelUI += new LogicModule.UpdateObjectDelegate(LogicControl_UpdateUserLevelUI);

            //added by lei.c
            logicModule.UpdateFinalResultList += new LogicModule.UpdateObjectDelegate(LogicControl_UpdateFinalResultList);
            logicModule.UpdateCCDResult += new LogicModule.UpdateObjectDelegate(LogicControl_UpdateCCDResult);
            logicModule.UpdateLaserResult += new LogicModule.UpdateObjectDelegate(LogicControl_UpdateLaserResult);
            logicModule.UpdateCCDOffset += new LogicModule.UpdateObjectDelegate(debugForm.UpdateCCDOffset);
            logicModule.UpdateLaserOffset += new LogicModule.UpdateObjectDelegate(debugForm.UpdateLaserOffset);
            logicModule.UpdateLaserFaiResult += new LogicModule.UpdateObjectDelegate(LogicControl_UpdateLaserFaiResult);
            logicModule.UpdateInitButtons += new LogicModule.UpdateObjectDelegate(LogicControl_EnableButtons);
            logicModule.UpdateTrueCT+= new LogicModule.UpdateObjectDelegate(LogicControl_UpdateTrueCT);
            logicModule.UpdateWorkPieceInfo += new LogicModule.UpdateObjectDelegate(LogicControl_UpdateWorkPieceInfo);

            debugForm.HideDownTimeRecord += new DebugForm.UpdateVoidDelegate(Debug_HideDownTimeRecord);
            debugForm.ShowDownTimeRecord += new DebugForm.UpdateVoidDelegate(Debug_ShowDownTimeRecord);
            debugForm.ResetCCD += new DebugForm.UpdateVoidDelegate(DebugForm_ResetCCD);
            logicModule.ResetMainFormPassRatio += new LogicModule.UpdateVoidDelegate(ResetPassRatio);
            logicModule.ClearMainFormDataGridView += new LogicModule.UpdateVoidDelegate(MainClearAllDataGridView);
            logicModule.HideCalibAndDebugForm += new LogicModule.UpdateVoidDelegate(HideCalibAndDebugForm);
            logicModule.UpdateCCDUndoDataGridView += new LogicModule.UpdateObjectDelegate(LogicControl_UpdateCCDUndoDataGridView);
            logicModule.UpdateLaserUndoDataGridView += new LogicModule.UpdateObjectDelegate(LogicControl_UpdateLaserUndoDataGridView);
            logicModule.UpdateButtonStatus += new LogicModule.UpdateObjectDelegate(LogicControl_UpdateButtonStatus);

            logicModule.ReadyStatusRelatedActivity += new LogicModule.UpdateVoidDelegate(LogicControl_ReadyStatusRelatedActivity);
            logicModule.AutoStatusRelatedActivity += new LogicModule.UpdateVoidDelegate(LogicControl_AutoStatusRelatedActivity);
            logicModule.PauseStatusRelatedActivity += new LogicModule.UpdateVoidDelegate(LogicControl_PauseStatusRelatedActivity);
            logicModule.StopStatusRelatedActivity += new LogicModule.UpdateVoidDelegate(LogicControl_StopStatusRelatedActivity);
            logicModule.ManualStatusRelatedActivity += new LogicModule.UpdateVoidDelegate(LogicControl_ManualStatusRelatedActivity);

            //Actual Code 生成Task3D的存储路径
            //InitTask3DPaths(logicModule.Task3DPath);
            for (int i = 0; i < 4; i++)
                logicModule.Task3DPaths[i] = "E:\\Task3D\\Task" + i.ToString() + ".task";//Test Code

            bool resultTask = laserCloudForm.Task3DInit();
            string errcode;

            if (!logicModule.Init(out errcode))
            {
                MessageBox.Show("Initial Error");
            }
            bMainFormLoadDone = true;
        }

        private void LogicControl_UpdateButtonStatus(object sender)
        {
            if (!this.InvokeRequired)
            {
                int myStatus = (int)sender;
                switch (myStatus)
                {
                    case (int)STATUS.AUTO_STATUS:
                        btn_InitialTest.Enabled = false;
                        btn_start.Enabled = false;
                        btn_stop.Enabled = true;
                        btn_DelayStop.Enabled = true;
                        btn_DelayStopCount.Enabled = true;
                        btn_emgstop.Enabled = true;
                        btn_reset.Enabled = false;
                        btn_InitialTrayZ.Enabled = false;
                        break;
                    case (int)STATUS.MANUAL_STATUS:
                        btn_InitialTest.Enabled = true;
                        btn_start.Enabled = false;
                        btn_stop.Enabled = true;
                        btn_DelayStop.Enabled = false;
                        btn_DelayStopCount.Enabled = false;
                        btn_emgstop.Enabled = true;
                        btn_reset.Enabled = true;
                        btn_InitialTrayZ.Enabled = true;
                        break;
                    case (int)STATUS.PAUSE_STATUS:
                        btn_InitialTest.Enabled = false;
                        btn_start.Enabled = true;
                        btn_stop.Enabled = true;
                        btn_DelayStop.Enabled = true;
                        btn_DelayStopCount.Enabled = true;
                        btn_emgstop.Enabled = true;
                        btn_reset.Enabled = false;
                        btn_InitialTrayZ.Enabled = false;
                        break;
                    case (int)STATUS.READY_STATUS:
                        btn_InitialTest.Enabled = false;
                        btn_start.Enabled = true;
                        btn_stop.Enabled = true;
                        btn_DelayStop.Enabled = false;
                        btn_DelayStopCount.Enabled = false;
                        btn_emgstop.Enabled = true;
                        btn_reset.Enabled = true;
                        btn_InitialTrayZ.Enabled = false;
                        break;
                    case (int)STATUS.STOP_STATUS:
                        btn_InitialTest.Enabled = false;
                        btn_start.Enabled = false;
                        btn_stop.Enabled = false;
                        btn_DelayStop.Enabled = false;
                        btn_DelayStopCount.Enabled = false;
                        btn_emgstop.Enabled = true;
                        btn_reset.Enabled = true;
                        btn_InitialTrayZ.Enabled = false;
                        break;
                }

                if (logicModule.bDelayStop)
                    btn_DelayStop.Text = "延时停止中";
                else
                    btn_DelayStop.Text = "当前延时停止";

                if (logicModule.bDelayStopCount)
                    btn_DelayStopCount.Text = "延时停止中";
                else
                    btn_DelayStopCount.Text = "延时停止";
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(LogicControl_UpdateButtonStatus), sender);
            }
        }

        private void LogicControl_ReadyStatusRelatedActivity()
        {
            if (!this.InvokeRequired)
            {
                btn_start.Text = "启 动";
                logicModule.YellowLight();
                lbl_CurMode.Text = "自动";
                lbl_CurStatus.Text = "Ready";
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateVoidDelegate(LogicControl_ReadyStatusRelatedActivity));
            }
        }

        private void LogicControl_AutoStatusRelatedActivity()
        {
            if (!this.InvokeRequired)
            {
                IOControl.WriteDO((int)DONAME.Do_LedLight, false);
                //btn_start.Text = "暂 停";
                logicModule.GreenLight();
                lbl_CurMode.Text = "自动";
                lbl_CurStatus.Text = "Auto";
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateVoidDelegate(LogicControl_AutoStatusRelatedActivity));
            }
        }

        private void LogicControl_PauseStatusRelatedActivity()
        {
            if (!this.InvokeRequired)
            {
                btn_start.Text = "继 续";
                logicModule.YellowLight();
                lbl_CurMode.Text = "自动";
                lbl_CurStatus.Text = "Pause";
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateVoidDelegate(LogicControl_PauseStatusRelatedActivity));
            }
        }

        private void LogicControl_StopStatusRelatedActivity()
        {
            if (!this.InvokeRequired)
            {
                btn_start.Text = "启 动";
                logicModule.RedLight();
                lbl_CurMode.Text = "自动";
                lbl_CurStatus.Text = "Stop";
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateVoidDelegate(LogicControl_StopStatusRelatedActivity));
            }
        }

        private void LogicControl_ManualStatusRelatedActivity()
        {
            if (!this.InvokeRequired)
            {
                logicModule.YellowLight();
                lbl_CurMode.Text = "手动";
                lbl_CurStatus.Text = "Manual";
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateVoidDelegate(LogicControl_ManualStatusRelatedActivity));
            }
        }

        private void LogicControl_UpdateWorkPieceInfo(object sender)
        {
            if (!this.InvokeRequired)
            {
                WorkPieceInfo myInfo = (WorkPieceInfo)sender;
                //lbl_WorkClassify.Text = myInfo.WorkClassify;
                lbl_WorkPieceCount.Text = myInfo.WorkPieceNum.ToString();
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(LogicControl_UpdateWorkPieceInfo), sender);
            }
        }

        private void LogicControl_UpdateTrueCT(object sender)
        {
            if (!this.InvokeRequired)
            {
                TrueCTCalcStru myTrueCT = (TrueCTCalcStru)sender;
                TimeSpan totaltime = AutoStatusStoreTime + (myTrueCT.curTime - AutoStatusStartTime);
                double totalseconds = totaltime.Hours * 3600 + totaltime.Minutes * 60 + totaltime.Seconds + totaltime.Milliseconds / 1000.0;
                lbl_TrueCT.Text = (totalseconds / myTrueCT.CheckNum).ToString("0.###") + "s";
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(LogicControl_UpdateTrueCT), sender);
            }
        }

        private void HideCalibAndDebugForm()
        {
            if (!this.InvokeRequired)
            {
                debugForm.Hide();
                calibForm.Hide();
            }
            else
            {
                this.BeginInvoke(new DebugForm.UpdateVoidDelegate(HideCalibAndDebugForm));
            }
        }

        private void Debug_ShowDownTimeRecord()
        {
            if (!this.InvokeRequired)
            {
                dgv_DownTimeRecord.Show();
                label2.Show();
                lbl_CurCT.Show();
                label14.Show();
                lbl_UPH.Show();
                label16.Show();
                lbl_TrueCT.Show();
                label32.Show();
                lbl_AutoCheckCT.Show();
            }
            else
            {
                this.BeginInvoke(new DebugForm.UpdateVoidDelegate(Debug_ShowDownTimeRecord));
            }
        }

        private void Debug_HideDownTimeRecord()
        {
            if (!this.InvokeRequired)
            {
                dgv_DownTimeRecord.Hide();
                label2.Hide();
                lbl_CurCT.Hide();
                label14.Hide();
                lbl_UPH.Hide();
                label16.Hide();
                lbl_TrueCT.Hide();
                label32.Hide();
                lbl_AutoCheckCT.Hide();
            }
            else
            {
                this.BeginInvoke(new DebugForm.UpdateVoidDelegate(Debug_HideDownTimeRecord));
            }
        }

        private void LogicControl_EnableButtons(object sender)
        {
            if (!this.InvokeRequired)
            {
                int obj = (int)sender;
                switch (obj)
                {
                    case 0:
                        btn_InitialTrayZ.Enabled = true;
                        btn_InitialTest.Text = "初始化开始";
                        break;
                    case 1:
                        btn_InitialTrayZ.Text = "Tray轴初始化开始";
                        break;
                }
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(LogicControl_EnableButtons), sender);
            }    
        }

        private void LogicControl_UpdateCCDUndoDataGridView(object sender)
        {
            if (!this.InvokeRequired)
            {
                int myTrayNo = (int)sender; int RowNum = 0;
                string myTrayNoStr = null;
                switch (myTrayNo)
                {
                    case 0:
                        myTrayNoStr = "A";
                        break;
                    case 1:
                        myTrayNoStr = "B";
                        break;
                    case 2:
                        myTrayNoStr = "C";
                        break;
                }

                for (int i = 0; i < logicModule.systemParam.WorkPieceNum; i++)
                {
                    RowNum = dgv_MainCCDData.RowCount;
                    if (RowNum == 0)
                        break;
                    if (dgv_MainCCDData.Rows[RowNum - 1].Cells[1].Value.ToString() == myTrayNoStr)
                        dgv_MainCCDData.Rows.RemoveAt(RowNum - 1);
                    else
                        break;
                }

                for (int i = 0; i < logicModule.systemParam.WorkPieceNum; i++)
                {
                    RowNum = dgv_CCDFinalResult.RowCount;
                    if (RowNum == 0)
                        break;
                    if (dgv_CCDFinalResult.Rows[RowNum - 1].Cells[1].Value.ToString() == myTrayNoStr)
                        dgv_CCDFinalResult.Rows.RemoveAt(RowNum - 1);
                    else
                        break;
                }
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(LogicControl_UpdateCCDUndoDataGridView), sender);
            }
        }


        private void LogicControl_UpdateLaserUndoDataGridView(object sender)
        {
            if (!this.InvokeRequired)
            {
                int myTrayNo = (int)sender; int RowNum = 0;
                string myTrayNoStr = null;
                switch (myTrayNo)
                {
                    case 0:
                        myTrayNoStr = "A";
                        break;
                    case 1:
                        myTrayNoStr = "B";
                        break;
                    case 2:
                        myTrayNoStr = "C";
                        break;
                }

                for (int i = 0; i < logicModule.systemParam.WorkPieceNum; i++)
                {
                    RowNum = dgv_MainLaserData.RowCount;
                    if (RowNum == 0)
                        break;
                    if (dgv_MainLaserData.Rows[RowNum - 1].Cells[1].Value.ToString() == myTrayNoStr)
                        dgv_MainLaserData.Rows.RemoveAt(RowNum - 1);
                    else
                        break;
                }

                for (int i = 0; i < logicModule.systemParam.WorkPieceNum; i++)
                {
                    RowNum = dgv_LaserFinalResult.RowCount;
                    if (RowNum == 0)
                        break;
                    if (dgv_LaserFinalResult.Rows[RowNum - 1].Cells[1].Value.ToString() == myTrayNoStr)
                        dgv_LaserFinalResult.Rows.RemoveAt(RowNum - 1);
                    else
                        break;
                }
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(LogicControl_UpdateLaserUndoDataGridView), sender);
            }
        }

        private void LogicControl_UpdatePassRatio(object sender)
        {
            if (!this.InvokeRequired)
            {
                PassRatio temp = (PassRatio)sender;
                lbl_PassRatio.Text = temp.PassNum.ToString() + "/" + temp.TotalNum.ToString() + "=" + (temp.Ratio * 100.0).ToString("0.##") + "%";
                lbl_TotalProductNum.Text = temp.TotalNum.ToString();
                if (logicModule.bIsSunway)
                {
                    lbl_ANum.Text = temp.ANum.ToString();
                    lbl_BNum.Text = temp.BNum.ToString();
                    lbl_CNum.Text = temp.CNum.ToString();
                    lbl_DNum.Text = temp.DNum.ToString();
                    if (logicModule.isEnableImprovePassRatio == true)
                    {
                        lbl_ENum.Show();
                        label47.Show();
                        lbl_ENum.Text = temp.ENum.ToString();
                    }
                    else
                    {
                        lbl_ENum.Hide();
                        label47.Hide();
                    }
                }
                else if (logicModule.bIsLaird)
                {
                    lbl_ANum.Hide(); lbl_BNum.Hide(); lbl_CNum.Hide(); lbl_DNum.Hide();lbl_ENum.Hide();
                    label9.Hide(); label10.Hide(); label11.Hide(); label12.Hide();label47.Hide();
                }
                lbl_DropNum.Text = temp.DropNum.ToString();

                lbl_TotalCheckNum.Text = logicModule.TotalCheckNum.ToString();
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(LogicControl_UpdatePassRatio), sender);
            }
        }

        private void LogicControl_UpdateStationAllDataArray(object sender)
        {
            if (!this.InvokeRequired)
            {
                PieceAllData[] myStationAllData = (PieceAllData[])sender;
                //***********樊竞明20180902***************//
                Action ExcelWriteAllFinalData = () =>
                {
                    logicModule.LogicControl_UpdateStationAllDataExcel(myStationAllData);
                };
                ExcelWriteAllFinalData.BeginInvoke(null, null);
                //******************************************//    
                for (int i = 0; i < myStationAllData.Length; i++)
                {
                    if (myStationAllData[i].exist == 0)
                    {
                        dgv_AllData.Rows.Add();
                        int n = dgv_AllData.Rows.Count;
                        dgv_AllData.Rows[n - 1].Cells[0].Value = n.ToString();
                        switch (myStationAllData[i].TrayNo)
                        {
                            case 0:
                                dgv_AllData.Rows[n - 1].Cells[1].Value = "A";
                                break;
                            case 1:
                                dgv_AllData.Rows[n - 1].Cells[1].Value = "B";
                                break;
                            case 2:
                                dgv_AllData.Rows[n - 1].Cells[1].Value = "C";
                                break;

                        }
                        dgv_AllData.Rows[n - 1].Cells[2].Value = myStationAllData[i].HoleNo.ToString();
                        dgv_AllData.Rows[n - 1].Cells[3].Value = myStationAllData[i].exist.ToString();

                        if (logicModule.bIsLaird)
                        {
                            switch (myStationAllData[i].Level)
                            {
                                case 0:
                                    dgv_AllData.Rows[n - 1].Cells[4].Value = "None";
                                    break;
                                case 1:
                                    dgv_AllData.Rows[n - 1].Cells[4].Value = "OK";
                                    break;
                                case -1:
                                    dgv_AllData.Rows[n - 1].Cells[4].Value = "NG";
                                    break;
                                default:
                                    break;
                            }
                        }
                        else if (logicModule.bIsSunway)
                        {
                            switch (myStationAllData[i].Level)
                            {
                                case 0:
                                    dgv_AllData.Rows[n - 1].Cells[4].Value = "None";
                                    break;
                                case 1:
                                    dgv_AllData.Rows[n - 1].Cells[4].Value = "A";
                                    break;
                                case -1:
                                    dgv_AllData.Rows[n - 1].Cells[4].Value = "B";
                                    break;
                                case -2:
                                    dgv_AllData.Rows[n - 1].Cells[4].Value = "C";
                                    break;
                                case -3:
                                    dgv_AllData.Rows[n - 1].Cells[4].Value = "D";
                                    break;
                                default:
                                    break;
                            }
                        }

                        dgv_AllData.Rows[n - 1].Cells[5].Value = myStationAllData[i].fai22.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[6].Value = myStationAllData[i].fai130.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[7].Value = myStationAllData[i].fai131.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[8].Value = myStationAllData[i].fai133G1.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[9].Value = myStationAllData[i].fai133G2.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[10].Value = myStationAllData[i].fai133G3.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[11].Value = myStationAllData[i].fai133G4.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[12].Value = myStationAllData[i].fai133G6.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[13].Value = myStationAllData[i].fai161.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[14].Value = myStationAllData[i].fai162.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[15].Value = myStationAllData[i].fai163.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[16].Value = myStationAllData[i].fai165.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[17].Value = myStationAllData[i].fai171.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[18].Value = myStationAllData[i].fai135.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[19].Value = myStationAllData[i].fai136.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[20].Value = myStationAllData[i].fai139.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[21].Value = myStationAllData[i].fai140.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[22].Value = myStationAllData[i].fai151.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[23].Value = myStationAllData[i].fai152.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[24].Value = myStationAllData[i].fai155.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[25].Value = myStationAllData[i].fai156.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[26].Value = myStationAllData[i].fai157.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[27].Value = myStationAllData[i].fai158.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[28].Value = myStationAllData[i].fai160.ToString("0.###");
                        dgv_AllData.Rows[n - 1].Cells[29].Value = myStationAllData[i].fai172.ToString("0.###");

                        UpdateFaiInfoMeanValue(myStationAllData[i], ref logicModule.myFaiInfoArray);
                        ChangeAllFaiColor(myStationAllData[i], ref logicModule.myFaiInfoArray);
                        for (int j = 0; j < 25; j++)
                        {
                            logicModule.myFaiInfoArray[j].FaiTotalNum = dgv_AllData.Rows.Count;
                        }
                    }
                }

                for (int i = 0; i < 25; i++)
                {
                    lbl_FaiInfo[i].Text = logicModule.myFaiInfoArray[i].FaiPassNum + "/" + logicModule.myFaiInfoArray[i].FaiNGNum + "/" + ((1.0 * logicModule.myFaiInfoArray[i].FaiPassNum / (logicModule.myFaiInfoArray[i].FaiPassNum + logicModule.myFaiInfoArray[i].FaiNGNum)) * 100).ToString("0.##") + "%";
                    /////*****************樊竞明20181010-总表数据使用**********************///
                    //logicModule.myFinaldataSummary.myFaiInfoSummary[i].FaiPassNum += logicModule.myFaiInfoArray[i].FaiPassNum;
                    //logicModule.myFinaldataSummary.myFaiInfoSummary[i].FaiNGNum += logicModule.myFaiInfoArray[i].FaiNGNum;
                    //////****************************************************************///
                }
                /////*****************樊竞明20181010-总表数据使用**********************///
                Action ExcelWriteAllFinalDataSummery = () =>
                {
                    logicModule.LogicControl_UpdateStationAllDataExcelSummary();
                };
                ExcelWriteAllFinalDataSummery.BeginInvoke(null, null);
                //////****************************************************************///

                if (dgv_AllData.Rows.Count > MaxColumnNum)
                {
                    SaveAllFaiData();
                    dgv_AllData.Rows.Clear();
                }
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(LogicControl_UpdateStationAllDataArray), sender);
            }
        }


        private void LogicControl_UpdateUserLevelUI(object sender)
        {
            if (!this.InvokeRequired)
            {
                int UserLevel = (int)sender;
                if (UserLevel == 10)
                {
                    btn_Setting.Hide();
                    btn_Calib.Hide();
                    btn_ShowLaserCloud.Hide();
                    btn_ClearTip.Show();
                    return;
                }
                if (UserLevel == 50)
                {
                    btn_Setting.Show();
                    btn_Calib.Show();
                    btn_ShowLaserCloud.Hide();
                    btn_ClearTip.Show();
                    return;
                }
                if (UserLevel == 100)
                {
                    btn_Setting.Show();
                    btn_Calib.Show();
                    btn_ShowLaserCloud.Show();
                    btn_ClearTip.Show();
                    return;
                }
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(LogicControl_UpdateUserLevelUI), sender);
            }
        }

        private void LogicControl_UpdateLaserFaiResult(object sender)
        {
            if (!this.InvokeRequired)
            {
                LaserFAIUpdateStruct myLaserFaiStru = (LaserFAIUpdateStruct)sender;
                dgv_MainLaserData.Rows.Add();
                int n = dgv_MainLaserData.Rows.Count;

                dgv_MainLaserData.Rows[n - 1].Cells[0].Value = n.ToString();
                dgv_MainLaserData.Rows[n - 1].Cells[1].Value = myLaserFaiStru.TrayNo;
                dgv_MainLaserData.Rows[n - 1].Cells[2].Value = myLaserFaiStru.holeNo;

                dgv_MainLaserData.Rows[n - 1].Cells[4].Value = myLaserFaiStru.fai135.ToString("0.###");
                dgv_MainLaserData.Rows[n - 1].Cells[5].Value = myLaserFaiStru.fai136.ToString("0.###");
                dgv_MainLaserData.Rows[n - 1].Cells[6].Value = myLaserFaiStru.fai139.ToString("0.###");
                dgv_MainLaserData.Rows[n - 1].Cells[7].Value = myLaserFaiStru.fai140.ToString("0.###");
                dgv_MainLaserData.Rows[n - 1].Cells[8].Value = myLaserFaiStru.fai151.ToString("0.###");
                dgv_MainLaserData.Rows[n - 1].Cells[9].Value = myLaserFaiStru.fai152.ToString("0.###");
                dgv_MainLaserData.Rows[n - 1].Cells[10].Value = myLaserFaiStru.fai155.ToString("0.###");
                dgv_MainLaserData.Rows[n - 1].Cells[11].Value = myLaserFaiStru.fai156.ToString("0.###");
                dgv_MainLaserData.Rows[n - 1].Cells[12].Value = myLaserFaiStru.fai157.ToString("0.###");
                dgv_MainLaserData.Rows[n - 1].Cells[13].Value = myLaserFaiStru.fai158.ToString("0.###");
                dgv_MainLaserData.Rows[n - 1].Cells[14].Value = myLaserFaiStru.fai160.ToString("0.###");
                dgv_MainLaserData.Rows[n - 1].Cells[15].Value = myLaserFaiStru.fai172.ToString("0.###");

                ChangeLaserFaiColor(myLaserFaiStru);
                dgv_MainLaserData.FirstDisplayedScrollingRowIndex = dgv_MainLaserData.Rows.Count - 1;

                if (n >= MaxColumnNum)
                {
                    SaveLaserFai();
                    dgv_MainLaserData.Rows.Clear();
                }
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(LogicControl_UpdateLaserFaiResult), sender);
            }
        }

        private void LogicControl_UpdateCTInfo(object sender)
        {
            if (!this.InvokeRequired)
            {
                CTInfoStruct temp = (CTInfoStruct)sender;
                lbl_CurCT.Text = temp.averagect.ToString("0.###") + "s";
                lbl_LastCT.Text = temp.lastct.ToString("0.###") + "s";
                lbl_CCDScanTime.Text = temp.lastCCDScanTime.ToString("0.###") + "s";
                lbl_LaserScanTime.Text = temp.lastLaserScanTime.ToString("0.###") + "s";
                if (temp.averagect != 0)
                    lbl_UPH.Text = (3600.0 / temp.averagect).ToString("0.###") + "pcs/hour";
                else
                    lbl_UPH.Text = 0.ToString("0.###") + "pcs/hour";

                if (temp.lastct < 2.0)
                {
                    AutoCheckTotalTime = AutoCheckTotalTime + temp.lastct;
                    lbl_AutoCheckCT.Text = (AutoCheckTotalTime / AutoCheckCTUpdateCount).ToString("0.###") + "s";
                    AutoCheckCTUpdateCount++;
                }
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(LogicControl_UpdateCTInfo), sender);
            }

        }

        private void LogicControl_UpdateLaserFinalResult(object sender)
        {
            if (!this.InvokeRequired)
            {
                FinalResultUpdateStru temp = (FinalResultUpdateStru)sender;
                dgv_LaserFinalResult.Rows.Add();
                int n = dgv_LaserFinalResult.Rows.Count;

                dgv_LaserFinalResult.Rows[n - 1].Cells[0].Value = n.ToString();
                dgv_LaserFinalResult.Rows[n - 1].Cells[1].Value = temp.TrayNo;
                dgv_LaserFinalResult.Rows[n - 1].Cells[2].Value = temp.FinalResult[0].ToString();
                dgv_LaserFinalResult.Rows[n - 1].Cells[3].Value = temp.FinalResult[1].ToString();//0908   by ben
                dgv_LaserFinalResult.Rows[n - 1].Cells[4].Value = temp.FinalResult[2].ToString();
                dgv_LaserFinalResult.Rows[n - 1].Cells[5].Value = temp.FinalResult[3].ToString();//0908   by ben
                ChangeDataGridViewColor(dgv_LaserFinalResult);
                dgv_LaserFinalResult.FirstDisplayedScrollingRowIndex = dgv_LaserFinalResult.Rows.Count - 1;

                if (n >= MaxColumnNum)
                {
                    SaveLaserResult();
                    dgv_LaserFinalResult.Rows.Clear();
                }
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(LogicControl_UpdateLaserFinalResult), sender);
            }
        }

        private void LogicControl_UpdateCCDFinalResult(object sender)
        {
            if (!this.InvokeRequired)
            {
                FinalResultUpdateStru temp = (FinalResultUpdateStru)sender;
                dgv_CCDFinalResult.Rows.Add();
                int n = dgv_CCDFinalResult.Rows.Count;

                dgv_CCDFinalResult.Rows[n - 1].Cells[0].Value = n.ToString();
                dgv_CCDFinalResult.Rows[n - 1].Cells[1].Value = temp.TrayNo;
                dgv_CCDFinalResult.Rows[n - 1].Cells[2].Value = temp.FinalResult[0].ToString();
                dgv_CCDFinalResult.Rows[n - 1].Cells[3].Value = temp.FinalResult[1].ToString();
                dgv_CCDFinalResult.Rows[n - 1].Cells[4].Value = temp.FinalResult[2].ToString();
                dgv_CCDFinalResult.Rows[n - 1].Cells[5].Value = temp.FinalResult[3].ToString();
                ChangeDataGridViewColor(dgv_CCDFinalResult);
                dgv_CCDFinalResult.FirstDisplayedScrollingRowIndex = dgv_CCDFinalResult.Rows.Count - 1;

                if (n >= MaxColumnNum)
                {
                    SaveCCDResult();
                    dgv_CCDFinalResult.Rows.Clear();
                }
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(LogicControl_UpdateCCDFinalResult), sender);
            }
        }

        private void ChangeDataGridViewColor(DataGridView myDataGridView)
        {
            int n = myDataGridView.Rows.Count;
            for (int i = 0; i < logicModule.systemParam.WorkPieceNum; i++)
            {
                if (myDataGridView.Rows[n - 1].Cells[i + 2].Value.ToString() == "1")
                    myDataGridView.Rows[n - 1].Cells[i + 2].Style.BackColor = Color.Green;
                else
                {
                    if (myDataGridView.Rows[n - 1].Cells[i + 2].Value.ToString() != "0")
                        myDataGridView.Rows[n - 1].Cells[i + 2].Style.BackColor = Color.Red;
                }
            }
        }

        private void ChangeCCDFaiColor(CCDUpdateStruct CCDResultData)
        {
            int n = dgv_MainCCDData.Rows.Count;
            bool checkresult = true;

            if (logicModule.JudgeFai(CCDResultData.fai22, logicModule.ThrParam.thrInfo[0].UpLimit, logicModule.ThrParam.thrInfo[0].DownLimit) != 0)
            {
                dgv_MainCCDData.Rows[n - 1].Cells[5].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(CCDResultData.fai130, logicModule.ThrParam.thrInfo[1].UpLimit, logicModule.ThrParam.thrInfo[1].DownLimit) != 0)
            {
                dgv_MainCCDData.Rows[n - 1].Cells[6].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(CCDResultData.fai131, logicModule.ThrParam.thrInfo[2].UpLimit, logicModule.ThrParam.thrInfo[2].DownLimit) != 0)
            {
                dgv_MainCCDData.Rows[n - 1].Cells[7].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(CCDResultData.fai133G1, logicModule.ThrParam.thrInfo[3].UpLimit, logicModule.ThrParam.thrInfo[3].DownLimit) != 0)
            {
                dgv_MainCCDData.Rows[n - 1].Cells[8].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(CCDResultData.fai133G2, logicModule.ThrParam.thrInfo[4].UpLimit, logicModule.ThrParam.thrInfo[4].DownLimit) != 0)
            {
                dgv_MainCCDData.Rows[n - 1].Cells[9].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(CCDResultData.fai133G3, logicModule.ThrParam.thrInfo[5].UpLimit, logicModule.ThrParam.thrInfo[5].DownLimit) != 0)
            {
                dgv_MainCCDData.Rows[n - 1].Cells[10].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(CCDResultData.fai133G4, logicModule.ThrParam.thrInfo[6].UpLimit, logicModule.ThrParam.thrInfo[6].DownLimit) != 0)
            {
                dgv_MainCCDData.Rows[n - 1].Cells[11].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(CCDResultData.fai133G6, logicModule.ThrParam.thrInfo[7].UpLimit, logicModule.ThrParam.thrInfo[7].DownLimit) != 0)
            {
                dgv_MainCCDData.Rows[n - 1].Cells[12].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(CCDResultData.fai161, logicModule.ThrParam.thrInfo[8].UpLimit, logicModule.ThrParam.thrInfo[8].DownLimit) != 0)
            {
                dgv_MainCCDData.Rows[n - 1].Cells[13].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(CCDResultData.fai162, logicModule.ThrParam.thrInfo[9].UpLimit, logicModule.ThrParam.thrInfo[9].DownLimit) != 0)
            {
                dgv_MainCCDData.Rows[n - 1].Cells[14].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(CCDResultData.fai163, logicModule.ThrParam.thrInfo[10].UpLimit, logicModule.ThrParam.thrInfo[10].DownLimit) != 0)
            {
                dgv_MainCCDData.Rows[n - 1].Cells[15].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(CCDResultData.fai165, logicModule.ThrParam.thrInfo[11].UpLimit, logicModule.ThrParam.thrInfo[11].DownLimit) != 0)
            {
                dgv_MainCCDData.Rows[n - 1].Cells[16].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(CCDResultData.fai171, logicModule.ThrParam.thrInfo[12].UpLimit, logicModule.ThrParam.thrInfo[12].DownLimit) != 0)
            {
                dgv_MainCCDData.Rows[n - 1].Cells[17].Style.BackColor = Color.Red;
                checkresult = false;
            }

            if (checkresult)
            {
                dgv_MainCCDData.Rows[n - 1].Cells[4].Style.BackColor = Color.Green;
                dgv_MainCCDData.Rows[n - 1].Cells[4].Value = "OK";
            }
            else
            {
                dgv_MainCCDData.Rows[n - 1].Cells[4].Style.BackColor = Color.Red;
                dgv_MainCCDData.Rows[n - 1].Cells[4].Value = "NG";
            }
        }

        private void ChangeLaserFaiColor(LaserFAIUpdateStruct LaserResultData)
        {
            int n = dgv_MainLaserData.Rows.Count;
            bool checkresult = true;

            if (logicModule.JudgeFai(LaserResultData.fai135, logicModule.ThrParam.thrInfo[13].UpLimit, logicModule.ThrParam.thrInfo[13].DownLimit) != 0)
            {
                dgv_MainLaserData.Rows[n - 1].Cells[4].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(LaserResultData.fai136, logicModule.ThrParam.thrInfo[14].UpLimit, logicModule.ThrParam.thrInfo[14].DownLimit) != 0)
            {
                dgv_MainLaserData.Rows[n - 1].Cells[5].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(LaserResultData.fai139, logicModule.ThrParam.thrInfo[15].UpLimit, logicModule.ThrParam.thrInfo[15].DownLimit) != 0)
            {
                dgv_MainLaserData.Rows[n - 1].Cells[6].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(LaserResultData.fai140, logicModule.ThrParam.thrInfo[16].UpLimit, logicModule.ThrParam.thrInfo[16].DownLimit) != 0)
            {
                dgv_MainLaserData.Rows[n - 1].Cells[7].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(LaserResultData.fai151, logicModule.ThrParam.thrInfo[17].UpLimit, logicModule.ThrParam.thrInfo[17].DownLimit) != 0)
            {
                dgv_MainLaserData.Rows[n - 1].Cells[8].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(LaserResultData.fai152, logicModule.ThrParam.thrInfo[18].UpLimit, logicModule.ThrParam.thrInfo[18].DownLimit) != 0)
            {
                dgv_MainLaserData.Rows[n - 1].Cells[9].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(LaserResultData.fai155, logicModule.ThrParam.thrInfo[19].UpLimit, logicModule.ThrParam.thrInfo[19].DownLimit) != 0)
            {
                dgv_MainLaserData.Rows[n - 1].Cells[10].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(LaserResultData.fai156, logicModule.ThrParam.thrInfo[20].UpLimit, logicModule.ThrParam.thrInfo[20].DownLimit) != 0)
            {
                dgv_MainLaserData.Rows[n - 1].Cells[11].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(LaserResultData.fai157, logicModule.ThrParam.thrInfo[21].UpLimit, logicModule.ThrParam.thrInfo[21].DownLimit) != 0)
            {
                dgv_MainLaserData.Rows[n - 1].Cells[12].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(LaserResultData.fai158, logicModule.ThrParam.thrInfo[22].UpLimit, logicModule.ThrParam.thrInfo[22].DownLimit) != 0)
            {
                dgv_MainLaserData.Rows[n - 1].Cells[13].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(LaserResultData.fai160, logicModule.ThrParam.thrInfo[23].UpLimit, logicModule.ThrParam.thrInfo[23].DownLimit) != 0)
            {
                dgv_MainLaserData.Rows[n - 1].Cells[14].Style.BackColor = Color.Red;
                checkresult = false;
            }
            if (logicModule.JudgeFai(LaserResultData.fai172, logicModule.ThrParam.thrInfo[24].UpLimit, logicModule.ThrParam.thrInfo[24].DownLimit) != 0)
            {
                dgv_MainLaserData.Rows[n - 1].Cells[15].Style.BackColor = Color.Red;
                checkresult = false;
            }

            if (checkresult)
            {
                dgv_MainLaserData.Rows[n - 1].Cells[3].Style.BackColor = Color.Green;
                dgv_MainLaserData.Rows[n - 1].Cells[3].Value = "OK";
            }
            else
            {
                dgv_MainLaserData.Rows[n - 1].Cells[3].Style.BackColor = Color.Red;
                dgv_MainLaserData.Rows[n - 1].Cells[3].Value = "NG";
            }


        }

        private void UpdateFaiInfoMeanValue(PieceAllData myAllData, ref FaiInfo[] myFaiArray)
        {
            myFaiArray[0].FaiMean = (myFaiArray[0].FaiMean * myFaiArray[0].FaiTotalNum + myAllData.fai22) / (myFaiArray[0].FaiTotalNum + 1);
            myFaiArray[1].FaiMean = (myFaiArray[1].FaiMean * myFaiArray[1].FaiTotalNum + myAllData.fai130) / (myFaiArray[1].FaiTotalNum + 1);
            myFaiArray[2].FaiMean = (myFaiArray[2].FaiMean * myFaiArray[2].FaiTotalNum + myAllData.fai131) / (myFaiArray[2].FaiTotalNum + 1);
            myFaiArray[3].FaiMean = (myFaiArray[3].FaiMean * myFaiArray[3].FaiTotalNum + myAllData.fai133G1) / (myFaiArray[3].FaiTotalNum + 1);
            myFaiArray[4].FaiMean = (myFaiArray[4].FaiMean * myFaiArray[4].FaiTotalNum + myAllData.fai133G2) / (myFaiArray[4].FaiTotalNum + 1);
            myFaiArray[5].FaiMean = (myFaiArray[5].FaiMean * myFaiArray[5].FaiTotalNum + myAllData.fai133G3) / (myFaiArray[5].FaiTotalNum + 1);
            myFaiArray[6].FaiMean = (myFaiArray[6].FaiMean * myFaiArray[6].FaiTotalNum + myAllData.fai133G4) / (myFaiArray[6].FaiTotalNum + 1);
            myFaiArray[7].FaiMean = (myFaiArray[7].FaiMean * myFaiArray[7].FaiTotalNum + myAllData.fai133G6) / (myFaiArray[7].FaiTotalNum + 1);
            myFaiArray[8].FaiMean = (myFaiArray[8].FaiMean * myFaiArray[8].FaiTotalNum + myAllData.fai161) / (myFaiArray[8].FaiTotalNum + 1);
            myFaiArray[9].FaiMean = (myFaiArray[9].FaiMean * myFaiArray[9].FaiTotalNum + myAllData.fai162) / (myFaiArray[9].FaiTotalNum + 1);
            myFaiArray[10].FaiMean = (myFaiArray[10].FaiMean * myFaiArray[10].FaiTotalNum + myAllData.fai163) / (myFaiArray[10].FaiTotalNum + 1);
            myFaiArray[11].FaiMean = (myFaiArray[11].FaiMean * myFaiArray[11].FaiTotalNum + myAllData.fai165) / (myFaiArray[11].FaiTotalNum + 1);
            myFaiArray[12].FaiMean = (myFaiArray[12].FaiMean * myFaiArray[12].FaiTotalNum + myAllData.fai171) / (myFaiArray[12].FaiTotalNum + 1);
            myFaiArray[13].FaiMean = (myFaiArray[13].FaiMean * myFaiArray[13].FaiTotalNum + myAllData.fai135) / (myFaiArray[13].FaiTotalNum + 1);
            myFaiArray[14].FaiMean = (myFaiArray[14].FaiMean * myFaiArray[14].FaiTotalNum + myAllData.fai136) / (myFaiArray[14].FaiTotalNum + 1);
            myFaiArray[15].FaiMean = (myFaiArray[15].FaiMean * myFaiArray[15].FaiTotalNum + myAllData.fai139) / (myFaiArray[15].FaiTotalNum + 1);
            myFaiArray[16].FaiMean = (myFaiArray[16].FaiMean * myFaiArray[16].FaiTotalNum + myAllData.fai140) / (myFaiArray[16].FaiTotalNum + 1);
            myFaiArray[17].FaiMean = (myFaiArray[17].FaiMean * myFaiArray[17].FaiTotalNum + myAllData.fai151) / (myFaiArray[17].FaiTotalNum + 1);
            myFaiArray[18].FaiMean = (myFaiArray[18].FaiMean * myFaiArray[18].FaiTotalNum + myAllData.fai152) / (myFaiArray[18].FaiTotalNum + 1);
            myFaiArray[19].FaiMean = (myFaiArray[19].FaiMean * myFaiArray[19].FaiTotalNum + myAllData.fai155) / (myFaiArray[19].FaiTotalNum + 1);
            myFaiArray[20].FaiMean = (myFaiArray[20].FaiMean * myFaiArray[20].FaiTotalNum + myAllData.fai156) / (myFaiArray[20].FaiTotalNum + 1);
            myFaiArray[21].FaiMean = (myFaiArray[21].FaiMean * myFaiArray[21].FaiTotalNum + myAllData.fai157) / (myFaiArray[21].FaiTotalNum + 1);
            myFaiArray[22].FaiMean = (myFaiArray[22].FaiMean * myFaiArray[22].FaiTotalNum + myAllData.fai158) / (myFaiArray[22].FaiTotalNum + 1);
            myFaiArray[23].FaiMean = (myFaiArray[23].FaiMean * myFaiArray[23].FaiTotalNum + myAllData.fai160) / (myFaiArray[23].FaiTotalNum + 1);
            myFaiArray[24].FaiMean = (myFaiArray[24].FaiMean * myFaiArray[24].FaiTotalNum + myAllData.fai172) / (myFaiArray[24].FaiTotalNum + 1);
        }

        private void ChangeAllFaiColor(PieceAllData myAllData, ref FaiInfo[] myFaiArray)
        {
            int n = dgv_AllData.Rows.Count;
            bool checkresult = true;

            if (logicModule.JudgeFai(myAllData.fai22, logicModule.ThrParam.thrInfo[0].UpLimit, logicModule.ThrParam.thrInfo[0].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[5].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[0].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[0].FaiNGNum++;
            }
            else
            {
                myFaiArray[0].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[0].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai130, logicModule.ThrParam.thrInfo[1].UpLimit, logicModule.ThrParam.thrInfo[1].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[6].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[1].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[1].FaiNGNum++;
            }
            else
            {
                myFaiArray[1].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[1].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai131, logicModule.ThrParam.thrInfo[2].UpLimit, logicModule.ThrParam.thrInfo[2].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[7].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[2].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[2].FaiNGNum++;
            }
            else
            {
                myFaiArray[2].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[2].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai133G1, logicModule.ThrParam.thrInfo[3].UpLimit, logicModule.ThrParam.thrInfo[3].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[8].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[3].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[3].FaiNGNum++;
            }
            else
            {
                myFaiArray[3].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[3].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai133G2, logicModule.ThrParam.thrInfo[4].UpLimit, logicModule.ThrParam.thrInfo[4].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[9].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[4].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[4].FaiNGNum++;
            }
            else
            {
                myFaiArray[4].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[4].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai133G3, logicModule.ThrParam.thrInfo[5].UpLimit, logicModule.ThrParam.thrInfo[5].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[10].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[5].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[5].FaiNGNum++;
            }
            else
            {
                myFaiArray[5].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[5].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai133G4, logicModule.ThrParam.thrInfo[6].UpLimit, logicModule.ThrParam.thrInfo[6].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[11].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[6].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[6].FaiNGNum++;
            }
            else
            {
                myFaiArray[6].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[6].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai133G6, logicModule.ThrParam.thrInfo[7].UpLimit, logicModule.ThrParam.thrInfo[7].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[12].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[7].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[7].FaiNGNum++;
            }
            else
            {
                myFaiArray[7].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[7].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai161, logicModule.ThrParam.thrInfo[8].UpLimit, logicModule.ThrParam.thrInfo[8].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[13].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[8].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[8].FaiNGNum++;
            }
            else
            {
                myFaiArray[8].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[8].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai162, logicModule.ThrParam.thrInfo[9].UpLimit, logicModule.ThrParam.thrInfo[9].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[14].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[9].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[9].FaiNGNum++;
            }
            else
            {
                myFaiArray[9].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[9].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai163, logicModule.ThrParam.thrInfo[10].UpLimit, logicModule.ThrParam.thrInfo[10].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[15].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[10].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[10].FaiNGNum++;
            }
            else
            {
                myFaiArray[10].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[10].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai165, logicModule.ThrParam.thrInfo[11].UpLimit, logicModule.ThrParam.thrInfo[11].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[16].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[11].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[11].FaiNGNum++;
            }
            else
            {
                myFaiArray[11].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[11].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai171, logicModule.ThrParam.thrInfo[12].UpLimit, logicModule.ThrParam.thrInfo[12].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[17].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[12].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[12].FaiNGNum++;
            }
            else
            {
                myFaiArray[12].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[12].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai135, logicModule.ThrParam.thrInfo[13].UpLimit, logicModule.ThrParam.thrInfo[13].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[18].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[13].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[13].FaiNGNum++;
            }
            else
            {
                myFaiArray[13].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[13].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai136, logicModule.ThrParam.thrInfo[14].UpLimit, logicModule.ThrParam.thrInfo[14].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[19].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[14].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[14].FaiNGNum++;
            }
            else
            {
                myFaiArray[14].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[14].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai139, logicModule.ThrParam.thrInfo[15].UpLimit, logicModule.ThrParam.thrInfo[15].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[20].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[15].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[15].FaiNGNum++;
            }
            else
            {
                myFaiArray[15].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[15].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai140, logicModule.ThrParam.thrInfo[16].UpLimit, logicModule.ThrParam.thrInfo[16].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[21].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[16].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[16].FaiNGNum++;
            }
            else
            {
                myFaiArray[16].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[16].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai151, logicModule.ThrParam.thrInfo[17].UpLimit, logicModule.ThrParam.thrInfo[17].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[22].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[17].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[17].FaiNGNum++;
            }
            else
            {
                myFaiArray[17].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[17].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai152, logicModule.ThrParam.thrInfo[18].UpLimit, logicModule.ThrParam.thrInfo[18].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[23].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[18].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[18].FaiNGNum++;
            }
            else
            {
                myFaiArray[18].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[18].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai155, logicModule.ThrParam.thrInfo[19].UpLimit, logicModule.ThrParam.thrInfo[19].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[24].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[19].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[19].FaiNGNum++;
            }
            else
            {
                myFaiArray[19].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[19].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai156, logicModule.ThrParam.thrInfo[20].UpLimit, logicModule.ThrParam.thrInfo[20].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[25].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[20].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[20].FaiNGNum++;
            }
            else
            {
                myFaiArray[20].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[20].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai157, logicModule.ThrParam.thrInfo[21].UpLimit, logicModule.ThrParam.thrInfo[21].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[26].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[21].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[21].FaiNGNum++;
            }
            else
            {
                myFaiArray[21].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[21].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai158, logicModule.ThrParam.thrInfo[22].UpLimit, logicModule.ThrParam.thrInfo[22].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[27].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[22].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[22].FaiNGNum++;
            }
            else
            {
                myFaiArray[22].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[22].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai160, logicModule.ThrParam.thrInfo[23].UpLimit, logicModule.ThrParam.thrInfo[23].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[28].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[23].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[23].FaiNGNum++;
            }
            else
            {
                myFaiArray[23].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[23].FaiPassNum++;
            }

            if (logicModule.JudgeFai(myAllData.fai172, logicModule.ThrParam.thrInfo[24].UpLimit, logicModule.ThrParam.thrInfo[24].DownLimit) != 0)
            {
                dgv_AllData.Rows[n - 1].Cells[29].Style.BackColor = Color.Red;
                checkresult = false;
                myFaiArray[24].FaiNGNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[24].FaiNGNum++;
            }
            else
            {
                myFaiArray[24].FaiPassNum++;
                logicModule.myFinaldataSummary.myFaiInfoSummary[24].FaiPassNum++;
            }

            if (checkresult)
                dgv_AllData.Rows[n - 1].Cells[4].Style.BackColor = Color.Green;
            else
                dgv_AllData.Rows[n - 1].Cells[4].Style.BackColor = Color.Red;

        }

        private void MainForm_LoadEvent(object sender, EventArgs e)
        {
            FormLoading load = new FormLoading();
            load.ShowDialog();
        }

        private void DebugForm_ResetCCD()
        {
            logicModule.TcpSendMsg("RS\r\n");
            logicModule.RecvCount = 0;
            logicModule.RecvClassify = 0;
        }

        private void InitTask3DPaths(string task3DPath)
        {
            int i = 0;
            DirectoryInfo folder = new DirectoryInfo(task3DPath);

            foreach (FileInfo file in folder.GetFiles("*.task"))
            {
                logicModule.Task3DPaths[i] = file.FullName;
                i++;
            }
        }

        private void LogicControl_UpdateLaserResult(object sender)
        {
            if (!this.InvokeRequired)
            {
                LaserUpdateStruct LaserResultData = (LaserUpdateStruct)sender;

                int LaserRowNum;
                if (UpdateLaserColumn == true)
                {
                    dgv_LaserAllData.Columns.Add("DataNo", "序号");
                    dgv_LaserAllData.Columns.Add("ProductNo", "穴号");
                    dgv_LaserAllData.Columns.Add("TrayNo", "托盘号");

                    for (int i = 0; i < LaserResultData.LaserFai.Count; i++)
                    {
                        dgv_LaserAllData.Columns.Add(LaserResultData.LaserFai[i].remark, LaserResultData.LaserFai[i].remark);
                    }

                    for (int i = 0; i < 30; i++)
                    {
                        dgv_LaserAllData.Columns.Add("temp" + i.ToString(), "temp" + i.ToString());
                    }

                    for (int i = 0; i < dgv_LaserAllData.Columns.Count; i++)
                    {
                        dgv_LaserAllData.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                    UpdateLaserColumn = false;
                }

                dgv_LaserAllData.Rows.Add();
                LaserRowNum = dgv_LaserAllData.Rows.Count;
                dgv_LaserAllData.Rows[LaserRowNum - 2].Cells[0].Value = (LaserRowNum - 1).ToString();
                dgv_LaserAllData.Rows[LaserRowNum - 2].Cells[1].Value = LaserResultData.HoleNo;
                dgv_LaserAllData.Rows[LaserRowNum - 2].Cells[2].Value = LaserResultData.TrayNo;

                for (int i = 0; i < LaserResultData.LaserFai.Count; i++)
                {
                    dgv_LaserAllData.Rows[LaserRowNum - 2].Cells[i + 3].Value = LaserResultData.LaserFai[i].valueFai.ToString();
                }

                dgv_LaserAllData.FirstDisplayedScrollingRowIndex = dgv_LaserAllData.Rows.Count - 1;

                if (LaserRowNum > MaxColumnNum)
                {
                    SaveLaserAllData();
                    dgv_LaserAllData.Rows.Clear();
                }
                //AppendLaserData(LaserResultData);
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(LogicControl_UpdateLaserResult), sender);
            }

        }

        private void AppendLaserData(LaserUpdateStruct LaserData)
        {
            FileStream fs = new FileStream(LaserCurDataFilePath, System.IO.FileMode.Append, System.IO.FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
            string data = "";

            data = LaserData.DataNo.ToString();
            data += ",";
            data += LaserData.HoleNo.ToString();
            data += ",";
            data += LaserData.TrayNo.ToString();
            data += ",";

            for (int i = 0; i < LaserData.LaserFai.Count; i++)
            {
                data += LaserData.LaserFai[i].valueFai.ToString();
                data += ",";
            }

            sw.WriteLine(data);

            sw.Close();
            fs.Close();
        }

        private void AppendCCDData(CCDUpdateStruct CCDData)
        {
            FileStream fs = new FileStream(CCDCurDataFilePath, System.IO.FileMode.Append, System.IO.FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
            string data = "";

            data = CCDData.dataNo.ToString();
            data += ",";
            data += CCDData.holeNo.ToString();
            data += ",";
            data += CCDData.TrayNo.ToString();
            data += ",";
            data += CCDData.exist.ToString();
            data += ",";
            data += CCDData.fai130.ToString("0.###");
            data += ",";
            data += CCDData.fai131.ToString("0.###");
            data += ",";
            data += CCDData.fai161.ToString("0.###");
            data += ",";
            data += CCDData.fai162.ToString("0.###");
            data += ",";
            data += CCDData.fai163.ToString("0.###");
            data += ",";
            data += CCDData.fai165.ToString("0.###");
            data += ",";
            //data += CCDData.fai161fromTT.ToString("0.###");

            sw.WriteLine(data);
            sw.Close();
            fs.Close();

        }

        //更新日志信息
        private void LogicControl_UpdateWaringLog(object sender)
        {
            if (!this.InvokeRequired)
            {
                string logResult = sender as string;
                string time = ChangeDateTimeToString(DateTime.Now);

                dgv_InfoList.Rows.Add(0, new DataGridViewRow());
                dgv_InfoList.Rows[dgv_InfoList.Rows.Count - 1].Cells[0].Value = time;
                dgv_InfoList.Rows[dgv_InfoList.Rows.Count - 1].Cells[1].Style.ForeColor = Color.White;
                dgv_InfoList.Rows[dgv_InfoList.Rows.Count - 1].Cells[1].Value = logResult;

                dgv_InfoList.Rows[dgv_InfoList.Rows.Count - 1].Selected = true;
                dgv_InfoList.FirstDisplayedScrollingRowIndex = dgv_InfoList.Rows.Count - 1;
                if (dgv_InfoList.Rows.Count >= 150)
                {
                    dgv_InfoList.Rows.RemoveAt(0);//删除第一行
                }
                //logicModule.WriteLog(time + "  " + logResult);
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(LogicControl_UpdateWaringLog), sender);
            }
        }
        //更新NG日志信息
        private void LogicControl_UpdateWaringLogNG(object sender)
        {
            if (!this.InvokeRequired)
            {
                string logResult = sender as string;
                string time = ChangeDateTimeToString(DateTime.Now);

                dgv_InfoList.Rows.Add(0, new DataGridViewRow());
                dgv_InfoList.Rows[dgv_InfoList.Rows.Count - 1].Cells[0].Value = time;
                dgv_InfoList.Rows[dgv_InfoList.Rows.Count - 1].Cells[1].Style.ForeColor = Color.Red;
                dgv_InfoList.Rows[dgv_InfoList.Rows.Count - 1].Cells[1].Value = logResult;
                dgv_InfoList.Rows[dgv_InfoList.Rows.Count - 1].Selected = true;
                dgv_InfoList.FirstDisplayedScrollingRowIndex = dgv_InfoList.Rows.Count - 1;
                if (dgv_InfoList.Rows.Count >= 150)
                {
                    dgv_InfoList.Rows.RemoveAt(0);//删除第一行
                }
                //logicModule.WriteLog(time + "  " + logResult);
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(LogicControl_UpdateWaringLogNG), sender);
            }
        }

        //added by lei.c用于更新dgv_resultlistnew
        private void LogicControl_UpdateFinalResultList(object sender)
        {
            if (!this.InvokeRequired)
            {
                FinalResultUpdateStru temp = (FinalResultUpdateStru)sender;
                dgv_ResultListNew.Rows.Add();
                int n = dgv_ResultListNew.Rows.Count;

                dgv_ResultListNew.Rows[n - 1].Cells[0].Value = n.ToString();
                dgv_ResultListNew.Rows[n - 1].Cells[1].Value = temp.TrayNo;
                dgv_ResultListNew.Rows[n - 1].Cells[2].Value = temp.FinalResult[0].ToString();
                dgv_ResultListNew.Rows[n - 1].Cells[3].Value = temp.FinalResult[1].ToString();
                dgv_ResultListNew.Rows[n - 1].Cells[4].Value = temp.FinalResult[2].ToString();
                dgv_ResultListNew.Rows[n - 1].Cells[5].Value = temp.FinalResult[3].ToString();
                ChangeDataGridViewColor(dgv_ResultListNew);
                dgv_ResultListNew.FirstDisplayedScrollingRowIndex = dgv_ResultListNew.Rows.Count - 1;

                if (n >= MaxColumnNum)
                {
                    SaveFinalResult();
                    dgv_ResultListNew.Rows.Clear();
                }
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(LogicControl_UpdateFinalResultList), sender);
            }
        }

        //18/4/16 Added by lei.c刷新debug界面的ccd datagridview
        //18/4/17 改为刷新mainform界面的datagridview
        private void LogicControl_UpdateCCDResult(object sender)
        {
            if (!this.InvokeRequired)
            {
                CCDUpdateStruct CCDResultData = (CCDUpdateStruct)sender;
                dgv_MainCCDData.Rows.Add();
                int n = dgv_MainCCDData.Rows.Count;

                dgv_MainCCDData.Rows[n - 1].Cells[0].Value = n.ToString();
                dgv_MainCCDData.Rows[n - 1].Cells[1].Value = CCDResultData.TrayNo;
                dgv_MainCCDData.Rows[n - 1].Cells[2].Value = CCDResultData.holeNo;
                dgv_MainCCDData.Rows[n - 1].Cells[3].Value = CCDResultData.exist.ToString();

                dgv_MainCCDData.Rows[n - 1].Cells[5].Value = CCDResultData.fai22.ToString("0.###");
                dgv_MainCCDData.Rows[n - 1].Cells[6].Value = CCDResultData.fai130.ToString("0.###");
                dgv_MainCCDData.Rows[n - 1].Cells[7].Value = CCDResultData.fai131.ToString("0.###");
                dgv_MainCCDData.Rows[n - 1].Cells[8].Value = CCDResultData.fai133G1.ToString("0.###");
                dgv_MainCCDData.Rows[n - 1].Cells[9].Value = CCDResultData.fai133G2.ToString("0.###");
                dgv_MainCCDData.Rows[n - 1].Cells[10].Value = CCDResultData.fai133G3.ToString("0.###");
                dgv_MainCCDData.Rows[n - 1].Cells[11].Value = CCDResultData.fai133G4.ToString("0.###");
                dgv_MainCCDData.Rows[n - 1].Cells[12].Value = CCDResultData.fai133G6.ToString("0.###");
                dgv_MainCCDData.Rows[n - 1].Cells[13].Value = CCDResultData.fai161.ToString("0.###");
                dgv_MainCCDData.Rows[n - 1].Cells[14].Value = CCDResultData.fai162.ToString("0.###");
                dgv_MainCCDData.Rows[n - 1].Cells[15].Value = CCDResultData.fai163.ToString("0.###");
                dgv_MainCCDData.Rows[n - 1].Cells[16].Value = CCDResultData.fai165.ToString("0.###");
                dgv_MainCCDData.Rows[n - 1].Cells[17].Value = CCDResultData.fai171.ToString("0.###");
                dgv_MainCCDData.Rows[n - 1].Cells[18].Value = CCDResultData.fai171a.ToString("0.###");
                dgv_MainCCDData.Rows[n - 1].Cells[19].Value = CCDResultData.fai171b.ToString("0.###");
                dgv_MainCCDData.Rows[n - 1].Cells[20].Value = CCDResultData.fai161afromccd.ToString("0.###");
                dgv_MainCCDData.Rows[n - 1].Cells[21].Value = CCDResultData.fai161bfromccd.ToString("0.###");
                dgv_MainCCDData.Rows[n - 1].Cells[22].Value = CCDResultData.fai162fromAbove.ToString("0.###");
                dgv_MainCCDData.Rows[n - 1].Cells[23].Value = CCDResultData.fai163fromAbove.ToString("0.###");
                dgv_MainCCDData.Rows[n - 1].Cells[24].Value = CCDResultData.fai163fromccd.ToString("0.###");
                dgv_MainCCDData.Rows[n - 1].Cells[25].Value = CCDResultData.fai165fromAbove.ToString("0.###");

                ChangeCCDFaiColor(CCDResultData);
                dgv_MainCCDData.FirstDisplayedScrollingRowIndex = dgv_MainCCDData.Rows.Count - 1;

                if (n >= MaxColumnNum)
                {
                    SaveCCDData();
                    dgv_MainCCDData.Rows.Clear();
                }

                //实时写入CurCCDData
                //AppendCCDData(CCDResultData);
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateObjectDelegate(LogicControl_UpdateCCDResult), sender);
            }

        }

        private void btn_Setting_Click(object sender, EventArgs e)
        {
            if (logicModule.JudgeUserLevel(50) == true)
            {
                logicModule.bInitEnvironmentFinished = false;
                logicModule.bInitTrayZFinished = false;
                logicModule.isResetClicked = false;
                debugForm.Show();
                return;
            }
            else
            {
                if (logicModule.CurStatus == (int)STATUS.MANUAL_STATUS)
                {
                    logicModule.bInitEnvironmentFinished = false;
                    logicModule.bInitTrayZFinished = false;
                    logicModule.isResetClicked = false;
                    debugForm.Show();
                    return;
                }
                else
                    MessageBox.Show("处于非手动模式，无法调试");
            }
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            logicModule.StartButton();
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            logicModule.PauseButton();
        }

        private void btn_reset_Click(object sender, EventArgs e)
        {
            logicModule.ResetButton();
        }

        private void StatusMonitor()
        {
            while (isRunning)
            {
                if (lastSystemStatus == -1)
                {
                    lastSystemStatus = (int)logicModule.CurStatus;
                    myDownTimeRecord.StartTime = DateTime.Now;
                    myDownTimeRecord.CurStatus = (int)logicModule.CurStatus;
                } 

                if (lastSystemStatus != (int)logicModule.CurStatus)//状态发生变化
                {
                    myDownTimeRecord.FinishTime = DateTime.Now;//旧状态结束时间
                    myDownTimeRecord.DownTimeSpan = myDownTimeRecord.FinishTime - myDownTimeRecord.StartTime;//旧状态持续时间
                    UpdateDownTimeInfo(myDownTimeRecord);//更新旧状态的信息

                    #region 获取暂停原因
                    if ((int)logicModule.CurStatus == (int)STATUS.PAUSE_STATUS)
                    {
                        DateTime StartTime = DateTime.Now;
                        while (true)
                        {
                            if (logicModule.CurWarningStr != null)
                            {
                                myDownTimeRecord.ErrorStr = logicModule.CurWarningStr;//如果进入pause状态，则收集造成pause的原因
                                break;
                            }
                            else
                            {
                                if (!logicModule.OutTimeCount(StartTime, 3))
                                {
                                    break;
                                }
                                Thread.Sleep(50);
                            }
                        }
                    } 
                    else
                        myDownTimeRecord.ErrorStr = null;
                    #endregion

                    myDownTimeRecord.StartTime = DateTime.Now;//新状态的开始时间
                    myDownTimeRecord.CurStatus = (int)logicModule.CurStatus;//新状态的状态表示

                    if ((int)logicModule.CurStatus == (int)STATUS.AUTO_STATUS)//如果新状态为Auto，则更新Auto状态的起始时间
                    {
                        AutoStatusStartTime = DateTime.Now;
                    }
                }

                if ((int)logicModule.CurStatus == (int)STATUS.AUTO_STATUS)//如果当前状态为auto，则将CurWarningStr置为null，表示无错误
                {
                    logicModule.CurWarningStr = null;
                }

                if ((lastSystemStatus == (int)STATUS.AUTO_STATUS) && (logicModule.CurStatus == (int)STATUS.PAUSE_STATUS))
                    logicModule.StartBeep();

                if ((lastSystemStatus == (int)STATUS.PAUSE_STATUS) && (logicModule.CurStatus == (int)STATUS.AUTO_STATUS))
                    logicModule.StopBeep();

                if ((lastSystemStatus == (int)STATUS.AUTO_STATUS) && (logicModule.CurStatus != (int)STATUS.AUTO_STATUS))//退出Auto状态
                {
                    AutoStatusStoreTime = AutoStatusStoreTime + (DateTime.Now - AutoStatusStartTime);
                }

                lastSystemStatus = (int)logicModule.CurStatus;

                if (DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 20)
                    lbl_WorkClassify.Text = "白班";
                else
                    lbl_WorkClassify.Text = "晚班";


                Thread.Sleep(100);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("是否确认退出？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                logicModule.AutoRunActive = false;
                logicModule.RefreshActive = false;
                isRunning = false;
                for (int i = 0; i < logicModule.logicConfig.PulseAxis.Length; i++)
                {
                    logicModule.ServoCtrl(logicModule.logicConfig.PulseAxis[i].AxisId, false);
                }

                for (int i = 0; i < logicModule.logicConfig.ECATAxis.Length; i++)
                {
                    logicModule.ServoCtrl(logicModule.logicConfig.ECATAxis[i].AxisId, false);
                }
                SaveSummaryData();//樊竞明20181010
                SaveRunData();
            }
            else
            {
                e.Cancel = true;
            }
        }
        //************************樊竞明20181010**************************//
        public void  SaveSummaryData()
        {
           bool result = XmlSerializerHelper.WriteXML((object)logicModule.myFinaldataSummary , logicModule.DataSummaryBackFilePath , typeof(FinalDataSummery));
        }
        //***************************************************************//
        private void SaveRunData()
        {
            string rundataPath= @".\Log\RunData-" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + ".txt";
            if (rundataPath == RunDataPath)
            {
                using (FileStream fs = new FileStream(rundataPath, FileMode.Append, System.IO.FileAccess.Write))
                {
                    StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
                    sw.WriteLine(ChangeDateTimeToString(DateTime.Now)+","+logicModule.TotalCheckNum.ToString()+","+logicModule.TotalCheckANum.ToString()+","+
                        logicModule.TotalCheckBNum.ToString() + ","+ logicModule.TotalCheckCNum.ToString() + ","+ logicModule.TotalCheckDNum.ToString() + ",");
                    sw.Close();
                    fs.Close();
                }
            }
            else
            {
                using (FileStream fs = new FileStream(rundataPath, FileMode.Create, System.IO.FileAccess.Write))
                {
                    StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
                    sw.WriteLine("Time,TotalNum,ANum,BNum,CNum,DNum");
                    sw.WriteLine(ChangeDateTimeToString(DateTime.Now) + "," + logicModule.TotalCheckNum.ToString() + "," + logicModule.TotalCheckANum.ToString() + "," +
                        logicModule.TotalCheckBNum.ToString() + "," + logicModule.TotalCheckCNum.ToString() + "," + logicModule.TotalCheckDNum.ToString() + ",");
                    sw.Close();
                    fs.Close();
                }
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            StatusMonitorThread.Abort();
            logicModule.LogicModuleDispose();    
        }

        //added by lei.c
        private void btn_ResultListNewClear_Click(object sender, EventArgs e)
        {
            dgv_ResultListNew.Rows.Clear();
        }

        private void btn_emgstop_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否确认停止？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                logicModule.EmgStopButton();
            }
        }

        private void btn_MainClearCCDData_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab.Text == "CCD测量数据")
            {
                dgv_MainCCDData.Rows.Clear();
                InitNewCurCCDDataFile();
            }
            else if (tabControl1.SelectedTab.Text == "Laser All Data")
            {
                dgv_LaserAllData.Rows.Clear();
                dgv_LaserAllData.Columns.Clear();
                UpdateLaserColumn = true;
                InitNewCurLaserDataFile();
            }
            else if (tabControl1.SelectedTab.Text == "Final Result")
                dgv_ResultListNew.Rows.Clear();
            else if (tabControl1.SelectedTab.Text == "CCD判断结果")
                dgv_CCDFinalResult.Rows.Clear();
            else if (tabControl1.SelectedTab.Text == "Laser判断结果")
                dgv_LaserFinalResult.Rows.Clear();
            else if (tabControl1.SelectedTab.Text == "Laser测量数据")
                dgv_MainLaserData.Rows.Clear();
            else if (tabControl1.SelectedTab.Text == "All Data")
                dgv_AllData.Rows.Clear();
            else if (tabControl1.SelectedTab.Text == "状态记录")
                dgv_DownTimeRecord.Rows.Clear();

        }

        private void MainClearAllDataGridView()
        {
            if (!this.InvokeRequired)
            {
                dgv_MainCCDData.Rows.Clear();
                InitNewCurCCDDataFile();

                dgv_LaserAllData.Rows.Clear();
                dgv_LaserAllData.Columns.Clear();
                UpdateLaserColumn = true;
                InitNewCurLaserDataFile();

                dgv_ResultListNew.Rows.Clear();
                dgv_CCDFinalResult.Rows.Clear();
                dgv_LaserFinalResult.Rows.Clear();
                dgv_MainLaserData.Rows.Clear();
                dgv_AllData.Rows.Clear();
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateVoidDelegate(MainClearAllDataGridView));
            }
        }

        private void InitNewCurLaserDataFile()
        {
            logicModule.CurLaserDataFileCount++;
            LaserCurDataFilePath = "E: \\3DLaserData\\LaserCurData-" + logicModule.CurLaserDataFileCount.ToString() + ".csv";
        }

        private void InitNewCurCCDDataFile()
        {
            logicModule.CurCCDDataFileCount++;
            CCDCurDataFilePath = "E: \\CCDData\\CCDCurData-" + logicModule.CurCCDDataFileCount.ToString() + ".csv";
        }

        private void btn_ExportCCDData_Click(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedTab.Text)
            {
                case "CCD测量数据":
                    SaveCCDData();
                    break;
                case "Laser测量数据":
                    SaveLaserFai();
                    break;
                case "CCD判断结果":
                    SaveCCDResult();
                    break;
                case "Laser判断结果":
                    SaveLaserResult();
                    break;
                case "Laser All Data":
                    SaveLaserAllData();
                    break;
                case "Final Result":
                    SaveFinalResult();
                    break;
                case "All Data":
                    SaveAllFaiData();
                    break;
                case "状态记录":
                    SaveDownTimeRecord();
                    break;
            }
        }

        public DataTable GetDgvToTable(DataGridView dgv)
        {
            DataTable dt = new DataTable();
            DataColumn dc;
            DataRow dr;

            // 列强制转换
            for (int count = 0; count < dgv.Columns.Count; count++)
            {
                dc = new DataColumn(dgv.Columns[count].Name);
                dt.Columns.Add(dc);
            }

            // 循环行
            for (int count = 0; count < dgv.Rows.Count; count++)
            {
                dr = dt.NewRow();
                for (int countsub = 0; countsub < dgv.Columns.Count; countsub++)
                {
                    dr[countsub] = Convert.ToString(dgv.Rows[count].Cells[countsub].Value);
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        private void SaveCCDData()
        {
            CsvWriter myWriter = new CsvWriter();
            string timeStr = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" +
                            DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString() + "-" + DateTime.Now.Second.ToString();
            string filePath = "E: \\CCDData\\" + timeStr;
            filePath = filePath + "CCDData.csv";
            myWriter.WriteCsv(GetDgvToTable(dgv_MainCCDData), filePath);
        }

        private void SaveLaserFai()
        {
            CsvWriter myWriter = new CsvWriter();
            string timeStr = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" +
                            DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString() + "-" + DateTime.Now.Second.ToString();
            string filePath = "E: \\3DLaserData\\" + timeStr;
            filePath = filePath + "LaserFai.csv";
            myWriter.WriteCsv(GetDgvToTable(dgv_MainLaserData), filePath);
        }

        private void SaveCCDResult()
        {
            CsvWriter myWriter = new CsvWriter();
            string timeStr = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" +
                            DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString() + "-" + DateTime.Now.Second.ToString();
            string filePath = "E: \\CCDData\\" + timeStr;
            filePath = filePath + "CCDResult.csv";
            myWriter.WriteCsv(GetDgvToTable(dgv_CCDFinalResult), filePath);
        }

        private void SaveLaserResult()
        {
            CsvWriter myWriter = new CsvWriter();
            string timeStr = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" +
                            DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString() + "-" + DateTime.Now.Second.ToString();
            string filePath = "E: \\3DLaserData\\" + timeStr;
            filePath = filePath + "LaserResult.csv";
            myWriter.WriteCsv(GetDgvToTable(dgv_LaserFinalResult), filePath);
        }

        private void SaveLaserAllData()
        {
            CsvWriter myWriter = new CsvWriter();
            string timeStr = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" +
                            DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString() + "-" + DateTime.Now.Second.ToString();
            string filePath = "E: \\3DLaserData\\" + timeStr;
            filePath = filePath + "LaserAllData.csv";
            myWriter.WriteCsv(GetDgvToTable(dgv_LaserAllData), filePath);
        }

        private void SaveFinalResult()
        {
            CsvWriter myWriter = new CsvWriter();
            string timeStr = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" +
                            DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString() + "-" + DateTime.Now.Second.ToString();
            string filePath = "E: \\3DLaserData\\" + timeStr;
            filePath = filePath + "FinalResultData.csv";
            myWriter.WriteCsv(GetDgvToTable(dgv_ResultListNew), filePath);
        }

        private void SaveAllFaiData()
        {
            CsvWriter myWriter = new CsvWriter();
            string timeStr = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" +
                            DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString() + "-" + DateTime.Now.Second.ToString();
            string filePath = "E: \\CCDData\\" + timeStr;
            filePath = filePath + "AllFaiData.csv";
            myWriter.WriteCsv(GetDgvToTable(dgv_AllData), filePath);

            FileStream fs = new FileStream(filePath, System.IO.FileMode.Append, System.IO.FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
            string data = "";
            data = "Mean Value,0,0,0,0,";
            for (int i = 0; i < 25; i++)
            {
                data += logicModule.myFaiInfoArray[i].FaiMean.ToString("0.##") + ",";
            }
            sw.WriteLine(data);
            data = "Pass Ratio,0,0,0,0,";
            for (int i = 0; i < 25; i++)
            {
                data += (1.0 * logicModule.myFaiInfoArray[i].FaiPassNum / logicModule.myFaiInfoArray[i].FaiTotalNum).ToString("0.##") + ",";
            }
            sw.WriteLine(data);
            sw.Close();
            fs.Close();
        }

        private void SaveDownTimeRecord()
        {
            CsvWriter myWriter = new CsvWriter();
            string timeStr = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" +
                            DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString() + "-" + DateTime.Now.Second.ToString();
            string filePath = "E: \\CCDData\\" + timeStr;
            filePath = filePath + "DownTimeRecord.csv";
            myWriter.WriteCsv(GetDgvToTable(dgv_DownTimeRecord), filePath);
        }

        private void btn_InitialTest_Click(object sender, EventArgs e)
        {
            if (logicModule.MainFormMiniInitDoing == true)
            {
                MessageBox.Show("正在进行Mini初始化，无法执行初始化动作");
                return;
            }

            if (btn_InitialTest.Text == "初始化开始")
            {
                if (MessageBox.Show("是否确认初始化？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    if (logicModule.CheckDrawerSignal() == false)
                        return;

                    logicModule.WriteLog("【初始化】初始化按钮按下");

                    if (InitEnvironmentThread != null)
                        InitEnvironmentThread.Abort();
                    InitEnvironmentThread = new Thread(new ThreadStart(logicModule.InitTestEnvironment), 1024);
                    InitEnvironmentThread.IsBackground = true;
                    InitEnvironmentThread.Start();
                    btn_InitialTest.Text = "初始化结束";
                }
            }
            else
            {
                InitEnvironmentThread.Abort();
                btn_InitialTest.Text = "初始化开始";
            }
        }


        //修改该方法适用于重置各Fai值良率 by吕
        private void btn_ResetPassRatio_Click(object sender, EventArgs e)
        {
            ResetPassRatio();
        }

        private void ResetPassRatio()
        {
            if (!this.InvokeRequired)
            {
                logicModule.ResetPassRatio();
                if (logicModule.bIsLaird)
                {
                    lbl_PassRatio.Text = "0/0=0.0%";
                }
                else
                {
                    lbl_PassRatio.Text = "0/0=0.0%";
                    for (int j = 0; j < 25; j++)
                    {
                        lbl_FaiInfo[j].Text = "0/0/0.0%";
                        logicModule.myFaiInfoArray[j].FaiTotalNum = 0;
                        logicModule.myFaiInfoArray[j].FaiPassNum = 0;
                        logicModule.myFaiInfoArray[j].FaiNGNum = 0;
                    }
                }

                lbl_ANum.Text = "0"; lbl_BNum.Text = "0"; lbl_CNum.Text = "0"; lbl_DNum.Text = "0";
                lbl_DropNum.Text = "0";
            }
            else
            {
                this.BeginInvoke(new LogicModule.UpdateVoidDelegate(ResetPassRatio));
            }
        }

        private void btn_ExportInfoList_Click(object sender, EventArgs e)
        {
            CsvWriter myWriter = new CsvWriter();
            string timeStr = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" +
                            DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString() + "-" + DateTime.Now.Second.ToString();
            string filePath = "E: \\CCDData\\" + timeStr;
            filePath = filePath + "InfoList.csv";
            myWriter.WriteCsv(GetDgvToTable(dgv_InfoList), filePath);
        }

        private void chk_SlowVelocity_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_SlowVelocity.Checked)
            {
                logicModule.SlowVelocity();
                MessageBox.Show("进入慢速测试状态");
            }
            else
            {
                logicModule.NormalVelocity();
                MessageBox.Show("进入正常速度状态");
            }
        }

        private void btn_NGDrawerUnlock_Click(object sender, EventArgs e)
        {
            if (btn_NGDrawerUnlock.Text == "NG抽屉解锁")
            {
                if (logicModule.NGDrawerUnlock())
                    btn_NGDrawerUnlock.Text = "NG抽屉锁扣";
            }
            else
            {
                if (logicModule.NGDrawerLock())
                    btn_NGDrawerUnlock.Text = "NG抽屉解锁";
            }
        }

        private void btn_LoadNullUnlock_Click(object sender, EventArgs e)
        {
            if (btn_LoadNullUnlock.Text == "上料空解锁")
            {
                if (logicModule.LoadNullTrayDrawerUnlock())
                    btn_LoadNullUnlock.Text = "上料空锁扣";
            }
            else
            {
                if (logicModule.LoadNullTrayDrawerLock())
                    btn_LoadNullUnlock.Text = "上料空解锁";
            }
        }

        private void btn_LoadFullUnlock_Click(object sender, EventArgs e)
        {
            if (btn_LoadFullUnlock.Text == "上料满解锁")
            {
                if (logicModule.LoadFullTrayDrawerUnlock())
                    btn_LoadFullUnlock.Text = "上料满锁扣";
            }
            else
            {
                if (logicModule.LoadFullTrayDrawerLock())
                    btn_LoadFullUnlock.Text = "上料满解锁";
            }
        }

        private void btn_UnloadNullUnlock_Click(object sender, EventArgs e)
        {
            if (btn_UnloadNullUnlock.Text == "下料空解锁")
            {
                if (logicModule.UnloadNullTrayDrawerUnlock())
                    btn_UnloadNullUnlock.Text = "下料空锁扣";
            }
            else
            {
                if (logicModule.UnloadNullTrayDrawerLock())
                    btn_UnloadNullUnlock.Text = "下料空解锁";
            }
        }

        private void btn_UnloadFullUnlock_Click(object sender, EventArgs e)
        {
            if (btn_UnloadFullUnlock.Text == "下料满解锁")
            {
                if (logicModule.UnloadFullTrayDrawerUnlock())
                    btn_UnloadFullUnlock.Text = "下料满锁扣";
            }
            else
            {
                if (logicModule.UnloadFullTrayDrawerLock())
                    btn_UnloadFullUnlock.Text = "下料满解锁";
            }
        }

        private void btn_DelayStop_Click(object sender, EventArgs e)
        {
            bool temp = !logicModule.bDelayStop;
            if (temp)
            {
                logicModule.bDelayStop = true;
                logicModule.iDelayStopCount = logicModule.LoadGantryCircleCount + 1;
            }
            else
            {
                logicModule.bDelayStop = false;
                logicModule.LoadGantryEnable = true;
            }
        }

        private void btn_ClearInfo_Click(object sender, EventArgs e)
        {
            dgv_InfoList.Rows.Clear();
        }

        private void btn_DelayStopCount_Click(object sender, EventArgs e)
        {
            bool temp = !logicModule.bDelayStopCount;
            if (temp)
            {
                if (string.IsNullOrEmpty(txt_DelayStopCount.Text))
                { return; }

                string testIdxStr = txt_DelayStopCount.Text;
                int testIdx = -1;
                int.TryParse(testIdxStr, out testIdx);
                if (testIdx <= 0 || testIdx > 14)
                {
                    MessageBox.Show("请输入1`14之间的数字");
                    return;
                }
                else
                    logicModule.iDelayStopCount = logicModule.LoadGantryCircleCount + testIdx + 1;
                logicModule.bDelayStopCount = true;
                logicModule.WriteLog("开始延时停止DelayStopCount——iDelayStopCount：" + logicModule.iDelayStopCount.ToString() + "; LoadGantryCircleCount:" + logicModule.LoadGantryCircleCount.ToString() + "; UnloadGantryCircleCount:" + logicModule.UnloadGantryCircleCount.ToString());
            }
            else
            {
                logicModule.bDelayStopCount = false;
                logicModule.iDelayStopCount = -1;
            }
        }

        private void btn_Calib_Click(object sender, EventArgs e)
        {
            if (logicModule.JudgeUserLevel(50) == true)
            {
                logicModule.bInitEnvironmentFinished = false;
                logicModule.bInitTrayZFinished = false;
                logicModule.isResetClicked = false;
                calibForm.Show();
                return;
            }
            else
            {
                if (logicModule.CurStatus != (int)STATUS.MANUAL_STATUS)
                    MessageBox.Show("处于非手动模式，无法标定");
                else
                {
                    logicModule.bInitEnvironmentFinished = false;
                    logicModule.bInitTrayZFinished = false;
                    logicModule.isResetClicked = false;
                    calibForm.Show();
                }
            }
        }

        private void 登录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            userLoginForm.StartPosition = FormStartPosition.CenterScreen;
            userLoginForm.ShowDialog();
        }

        private void btn_ShowLaserCloud_Click(object sender, EventArgs e)
        {
            if (logicModule.JudgeUserLevel(50) == false)
            {
                MessageBox.Show("当前用户无法打开标定界面，请与工程师联系", "提示", MessageBoxButtons.OK);
                return;
            }

            laserCloudForm.Show();
        }

        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 4; i++)
                logicModule.Task3DPaths[i] = "E:\\Task3D\\Task" + i.ToString() + ".task";//Test Code

            bool resultTask = laserCloudForm.Task3DInit();
            if (resultTask)
                MessageBox.Show("重新加载任务成功");
            else
                MessageBox.Show("加载失败！");
        }

        private void mini初始化ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (logicModule.MainFormInitDoing || logicModule.MainFormMiniInitDoing)
            {
                MessageBox.Show("正在进行初始化，无法执行Mini初始化动作");
                return;
            }

            if (MessageBox.Show("是否确认初始化？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                if (logicModule.CheckDrawerSignal() == false)
                    return;
                logicModule.WriteLog("【mini初始化】mini初始化按钮按下");
                if (InitEnvironmentThread != null)
                    InitEnvironmentThread.Abort();
                logicModule.bMiniInit = true;
                InitEnvironmentThread = new Thread(new ThreadStart(logicModule.InitTestEnvironment), 1024);
                InitEnvironmentThread.IsBackground = true;
                InitEnvironmentThread.Start();

            }
        }

        private void btn_RunConfig_Click(object sender, EventArgs e)
        {
            loadTraySeqSelectForm.StartPosition = FormStartPosition.CenterScreen;
            loadTraySeqSelectForm.ShowDialog();
        }

        private void btn_ResetCT_Click(object sender, EventArgs e)
        {
            logicModule.AutoRunStartTime = DateTime.Now;
            logicModule.AutoRunLastTime = DateTime.Now;
            logicModule.myCTInfo = new CTInfoStruct(0, 0, 0, 0);
            LogicControl_UpdateCTInfo(logicModule.myCTInfo);
        }

        #region  樊竞明  添加关于产品批次/模穴号输入
        //手动输入新的产品批次号
        private void txt_TestBatch_TextChanged(object sender, EventArgs e)
        {
            logicModule.ExcelDetectBatchMsg = txt_TestBatch.Text;
            logicModule.ExcelTestBatchSerialNum = 1;
        }
        private void txt_TestBatch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar ==13)
            {
                //logicModule.JudgeTimeBlock();
                logicModule.isInputProductBatch = true;
            }
        }
       
        #endregion
        private void btn_ClearTip_Click(object sender, EventArgs e)
        {
            dgv_InfoList.Rows.Clear();
        }

        private void 注销ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logicModule.UserLogOut();
            if (logicModule.JudgeUserLevel(0))
                MessageBox.Show("当前账户注销成功");
            else
                MessageBox.Show("当前账户注销失败");
            debugForm.Hide();
            calibForm.Hide();
        }

        private void btn_ShowDistribute_Click(object sender, EventArgs e)
        {
            distributeChartForm.selectedFAISeq = -1;
            distributeChartForm.Show();
        }

        private void btn_InitialTrayZ_Click(object sender, EventArgs e)
        {
            if (btn_InitialTrayZ.Text == "Tray轴初始化开始")
            {
                if (MessageBox.Show("是否确认Tray轴初始化？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    if (logicModule.CheckDrawerSignal() == false)
                        return;
                    logicModule.WriteLog("【Tray轴初始化】Tray轴初始化按钮按下");
                    if (InitTrayZThread != null)
                        InitTrayZThread.Abort();
                    InitTrayZThread = new Thread(new ThreadStart(logicModule.InitTrayZAxis), 1024);
                    InitTrayZThread.IsBackground = true;
                    InitTrayZThread.Start();
                    btn_InitialTrayZ.Text = "Tray轴初始化结束";
                }
            }
            else
            {
                logicModule.InitLoadNullThread.Abort();
                logicModule.InitLoadFullThread.Abort();
                logicModule.InitUnloadNullThread.Abort();
                logicModule.InitUnloadFullThread.Abort();
                InitTrayZThread.Abort();
                btn_InitialTrayZ.Text = "Tray轴初始化开始";
            }
        }
    }
}
