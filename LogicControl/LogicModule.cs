using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using StructAssemble;
using XmlHelper;
using MoveControlAssemble;
using System.Windows.Forms;
using Wtool;
using KeyenceLJ;
using System.Linq;
using CommonStruct;
using CommonStruct.LC3D;
using CommonStruct.LCPrim;
using CsvHelper;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using TaskControlLib;
using UserControls;
using LCPrimaryLib;
using CommonStruct.Communicate;
using System.Data;
using OfficeOpenXml;//excel文档操作


namespace LogicControl
{
    public class LogicModule
    {
        public bool RefreshActive = true;

        //edited by lei.c AutoRunActive初始值从true改为false
        public bool AutoRunActive = false;
        //wodiguai

        public int CurStatus = 0;
        public bool bInitEnvironmentFinished = false;
        public bool bInitTrayZFinished = false;

        #region Actual Data Flow
        public bool[,] CameraCheckDoneArrayTray = new bool[3, 4];
        public int[,] CameraCheckResultArrayTray = new int[3, 4];
        public bool[,] LaserCheckDoneArrayTray = new bool[3, 4];
        public int[,] LaserCheckResultArrayTray = new int[3, 4];
        public int[,] FinalCheckResultArrayTray = new int[3, 4];

        #endregion

        CalibMatrix CCDCalibMatrix = new CalibMatrix();//CCD标定矩阵
        private bool isWaitingCCD = false;
        private bool isCheckingCCD = false;
        public int CurCCDDataFileCount = 0;//当前实时记录的CCD数据文件的编号
        public int CurLaserDataFileCount = 0;//当前实时记录的Laser数据文件的编号

        public int LoadModuleWorkPieceNum = 0;      //上料模组工件数目
        public int UnloadModuleWorkPieceNum = 0;    //下料模组工件数目
        public int EnterMoveWorkPieceNum = 0;       //入料工位工件数目
        public int CCDBackLashPulseNo = 200;
        public bool EnableBackLash = false;
        public string CurWarningStr = null;//当前故障字符串
        private string lastWorkClassify, curWorkClassify;

        public bool isEnableImprovePassRatio = true;//若任意FAI的偏差超过0.5mm，则不计入良率
        public double PassRatioMaxMargin = 0.5;//不计入良率的最大偏差值，初始值为0.5mm

        //******************Excel 樊竞明********************//
        public string ExcelFinalDataDirPath = "D: \\FinalData";
        public string ExcelFinalDataFilePath = null;
        public int ExceStartlRowNum = 18;
        public int ExcelWriteRowNum = 0;
        public int ExcelTotalNum = 0;
        public int ExcelNgNum = 0;
        public string ExcelDetectBatchMsg = " ";
        public int ExcelTestBatchSerialNum = 1;
        public bool isInputProductBatch=false ;
        public string ExcelFinalDataFilePathSummary = null;//总表文件名 20181010
        public FinalDataSummery myFinaldataSummary = new FinalDataSummery(); //总数据 20181010
        public string DataSummaryBackDirPath = System.Environment.CurrentDirectory + "\\DataSummaryBackUp";//总数据文件夹路径20181010
        public string DataSummaryBackFilePath = System.Environment.CurrentDirectory + "\\DataSummaryBackUp\\DataSummaryBack.xml";//总数据文件路径 20181010
        //****************************************//

        #region added by lei.c 专门用于测试各个子流程，而新增的短路条件
        public bool debugThreadMainAxis = false;
        public bool debugThreadA = false;
        public bool debugThreadB = false;
        public bool debugThreadC = false;

        public bool debugWorkpieceInPos = false;
        public bool debugLoadRollSense = false;
        public bool debugUnloadRollSense = false;

        public bool debugThreadLoadTraySwitch = false;
        public bool debugThreadUnloadTraySwitch = false;
        public bool debugThreadLoadGantry = false;
        public bool debugThreadUnloadGantry = false;
        public bool debugThreadSuckAxis = false;
        public bool debugThreadLoadModule = false;
        public bool debugThreadUnloadModule = false;
        public bool debugThreadUnloadGantryPlaceAllWorkPiece = false;
        #endregion

        #region 常量
        public int[] MainAxisMoveSeg = { -33333, -33333, -33334 };//A&B工位移动33333个pulse，C工位移动33334个pulse
        int UserLevel = 0;
        #endregion

        #region 私有变量
        public Barcodes barcodes = new Barcodes();
        public List<Barcodes> barcodeList = new List<Barcodes>();
        private MoveControlClass.ADlink adlink = new MoveControlClass.ADlink();
        private MoveControlClass.ADlink7432 Dask7432 = new MoveControlClass.ADlink7432();
        private MoveControlClass.ECATIOControl ECATIO = new MoveControlClass.ECATIOControl();
        public CSClient client = new CSClient();
        public bool tcp_enable = false;
        public string tcp_Recive = string.Empty;
        private FileStream CTLogfs, MoveLogfs, DownTimeLogfs, RunDataLogfs,CCDMoveLogfs;
        private StreamWriter CTLogsw, MoveLogsw, DownTimeLogsw, RunDataLogsw,CCDMoveLogsw;
        bool PauseButtonClicked = false;

        public int TotalCheckNum, TotalCheckANum, TotalCheckBNum, TotalCheckCNum, TotalCheckDNum,TotalCheckENum;

        private DebugLog debug = null;
        string MoveLogPath = @".\Log\Move-" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + ".txt";
        string CTLogPath = @".\Log\CT-" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + ".txt";
        string DownTimeLogPath = @".\Log\DownTime-" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + ".txt";
        string CCDMoveLogPath= @".\Log\CCDMoveLog-" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + ".txt";

        public LogicConfig logicConfig = new LogicConfig();   //配置文件

        public string pathLogicConfig = Directory.GetCurrentDirectory() + @"\MyConfig\LogicConfig.xml";
        public string pathMotionPos = Directory.GetCurrentDirectory() + @"\MyConfig\MotionPos.xml";
        public string pathLoadGantryMotionPos = Directory.GetCurrentDirectory() + @"\PositionConfig\LoadGantryMotionPos.xml";
        public string pathUnloadGantryMotionPos = Directory.GetCurrentDirectory() + @"\PositionConfig\UnloadGantryMotionPos.xml";
        public string pathUnloadGantrySupplyMotionPos = Directory.GetCurrentDirectory() + @"\PositionConfig\UnloadGantrySupplyMotionPos.xml";
        public string pathSystemParam = Directory.GetCurrentDirectory() + @"\PositionConfig\SystemPara.xml";
        public string pathThresParam = Directory.GetCurrentDirectory() + @"\MyConfig\Threshold.xml";
        public string pathThresParamShow = Directory.GetCurrentDirectory() + @"\MyConfig\ThresholdShow.xml";
        public string pathCard = Directory.GetCurrentDirectory() + @"\MyConfig\204C.xml";
        public string pathECATCard = Directory.GetCurrentDirectory() + @"\MyConfig\8338.xml";
        public string pathLaserRawDataOffset = Directory.GetCurrentDirectory() + @"\MyConfig\LaserRawDataOffset.csv";
        public string pathCCDMatrix = Directory.GetCurrentDirectory() + @"\MyConfig\CCDMatrix.xml";

        public bool[] logicIgnore = new bool[3];//logicModule的屏蔽数组
        public bool bWaitPut = true;
        public bool bIgnoreSupply = false;

        public bool bIsLaird = false;
        public bool bIsSunway = true;
        public bool isResetClicked = false;
        public bool isNewLoadTraySwitch = false;

        public bool isDisThrow = false;//0905 add by ben

        int PartATrayNo, PartBTrayNo, PartCTrayNo;
        object locker = new object();
        object ccdlocker = new object();

        public DateTime AutoRunStartTime;
        public DateTime AutoRunLastTime;
        public DateTime AutoRunStepBStartTime;
        public DateTime AutoRunStepCStartTime;
        public double AverageCT = 0;
        public double LastCT = 0;
        public CTInfoStruct myCTInfo;
        public bool bMiniInit = false;

        //延时停止功能
        public bool bDelayStop = false;
        public bool bDelayStopCount = false;
        public int iDelayStopCount = -1;

        public Thread InitLoadNullThread, InitLoadFullThread, InitUnloadNullThread, InitUnloadFullThread;

        public bool gbEnableMonitorCCDCommunication = true;
        #endregion

        private Thread mainAxisThread, partAThread, partBThread, partCThread, loadTrayThread, unloadTrayThread, loadGantryThread;
        private Thread unloadGantryThread, loadModuleThread, unloadModuleThread, suckAxisMoveThread, unloadGantryPlaceAllWorkPieceThread;
        private Thread loadTrayAllSwitchThread, unloadTrayAllSwitchThread;
        private Thread refreshThread,refreshStatus;
        private Thread updateThreadFast, updateThreadSlow;
        private Thread debugThread, debugBThread, debugCThread;

        public bool mainAxisThreadFinish, partAThreadFinish, partBThreadFinish, partCThreadFinish, loadTrayThreadFinish, unloadTrayThreadFinish, loadGantryThreadFinish, unloadGantryThreadFinish;
        public bool loadModuleThreadFinish, unloadModuleThreadFinish, suckAxisMoveThreadFinish, unloadGantryPlaceAllWorkPieceThreadFinish, loadTrayAllSwitchThreadFinish, unloadTrayAllSwitchThreadFinish;

        #region 委托与事件
        public delegate void UpdateObjectDelegate(object sender);
        public event UpdateObjectDelegate UpdateMotionStatus;    // 初始化测量统计界面
        public event UpdateObjectDelegate UpdateSysInfo;         // 更新系统参数
        public event UpdateObjectDelegate UpdateThreshold;       // 更新点位信息
        public event UpdateObjectDelegate UpdateWaringLog;       // 更新报警日志
        public event UpdateObjectDelegate UpdateWaringLogNG;     // 更新异常NG在LOG里
        public event UpdateObjectDelegate UpdateCCDFinalResult;  //更新CCD测试结果
        public event UpdateObjectDelegate UpdateLaserFinalResult;//更新Laser测试结果
        public event UpdateObjectDelegate UpdateCTInfo;          //更新CT信息
        public event UpdateObjectDelegate UpdateStationAllDataArray;  //更新工位所有FAI数据

        //added by lei.c
        public event UpdateObjectDelegate UpdateFinalResultList;//更新测试结果列表
        public event UpdateObjectDelegate UpdateCCDResult;//更新主界面CCD检测结果
        public event UpdateObjectDelegate UpdateLaserResult;//更新主界面Laser检测结果
        public event UpdateObjectDelegate UpdateLaserFaiResult;//更新主界面LaserFai检测结果
        public event UpdateObjectDelegate UpdatePassRatio;//更新主界面的通过率

        public event UpdateObjectDelegate UpdateCCDOffset;//更新CCD补偿
        public event UpdateObjectDelegate UpdateLaserOffset;//更新Laser补偿
        public event UpdateObjectDelegate UpdateInitButtons;
        public event UpdateObjectDelegate UpdateCCDUndoDataGridView;
        public event UpdateObjectDelegate UpdateLaserUndoDataGridView;
        public event UpdateObjectDelegate UpdateDebugButton;
        public event UpdateObjectDelegate UpdateUserLevelUI;
        public event UpdateObjectDelegate UpdateTrueCT;//更新真实CT（只包含Auto状态时间）
        public event UpdateObjectDelegate UpdateWorkPieceInfo;//更新班次产量信息
        public event UpdateObjectDelegate UpdateButtonStatus;//更新Button状态
        public event UpdateObjectDelegate UpdateDownTimeInfo;    //更新DownTimeInfo

        public delegate void UpdateVoidDelegate();
        public event UpdateVoidDelegate ResetMainFormPassRatio;
        public event UpdateVoidDelegate ClearMainFormDataGridView;
        public event UpdateVoidDelegate HideCalibAndDebugForm;
        public event UpdateVoidDelegate ReadyStatusRelatedActivity;
        public event UpdateVoidDelegate AutoStatusRelatedActivity;
        public event UpdateVoidDelegate PauseStatusRelatedActivity;
        public event UpdateVoidDelegate StopStatusRelatedActivity;
        public event UpdateVoidDelegate ManualStatusRelatedActivity;

        #endregion

        public double[] testCCDRawData = new double[205];
        public Point2D[] testCCDPicPos = new Point2D[3];
        public bool isTestCCDInfluence = false;//正常情况默认为false

        public PassRatio myPassRatio = new PassRatio(0, 0, 0.0, 0, 0, 0, 0, 0, 0);

        public bool isSet1Param = false;//0908 改为public by ben
        private bool isLaserRunning = false;
        public KeyenceControl keyenceLJ = new KeyenceControl();
        public Dictionary<string, XDPOINT[,]> Laser12DicData = new Dictionary<string, XDPOINT[,]>();//Laser12Dicdata 缓存
        public Dictionary<string, XDPOINT[,]> LaserADicData = new Dictionary<string, XDPOINT[,]>();//LaserADicdata 缓存
        public Dictionary<string, XDPOINT[,]> LaserBDicData = new Dictionary<string, XDPOINT[,]>();//LaserBDicdata 缓存
        public Task taskConfig;            //初始激光Task配置
        List<string> ThreadNames = new List<string>();//线程名称
        public MovePathConfig moveConfig;//当前路径主参数---分出所有条子轨迹
        public string LaserDataPath = @"E:\3DLaserdata\";
        private Calib3DSturct[] CalibPathNO;
        bool LaserResult = false;
        string LaserErrCode = null;
        public int nLaserData = 0;
        List<XDPOINT[,]> LaserAllData = new List<XDPOINT[,]>();//发送3D的数据
        public TaskControl[] taskControl = new TaskControl[4];
        public string[] Task3DPaths = new string[4];//所有3DTask 路径全程数组
        public string moveConfigPath = Directory.GetCurrentDirectory() + @"\MyConfig\LaserPath.xml";//所有轨迹路径全称
        public string Calib3Dpath = Directory.GetCurrentDirectory() + @"\MyConfig\Calib3DConfig.xml";
        public Calib3D calib3D;//3d标定矩阵

        public StationCheckPara[] StationCheckSetting = new StationCheckPara[3];
        public string[] StationCheckParaPaths = new string[3];

        public int[] UnloadGantrySuckerSuckControls = new int[] {(int)ECATDONAME.Do_UnloadLeft1VacumSuck, (int)ECATDONAME.Do_UnloadLeft2VacumSuck, (int)ECATDONAME.Do_UnloadLeft3VacumSuck, (int)ECATDONAME.Do_UnloadLeft4VacumSuck,
                                                                 (int)ECATDONAME.Do_UnloadRight1VacumSuck, (int)ECATDONAME.Do_UnloadRight2VacumSuck,(int)ECATDONAME.Do_UnloadRight3VacumSuck,(int)ECATDONAME.Do_UnloadRight4VacumSuck};
        public int[] UnloadGantrySuckerBreakControls = new int[] { (int)ECATDONAME.Do_UnloadLeft1VacumBreak, (int)ECATDONAME.Do_UnloadLeft2VacumBreak, (int)ECATDONAME.Do_UnloadLeft3VacumBreak, (int)ECATDONAME.Do_UnloadLeft4VacumBreak,
                                                                   (int)ECATDONAME.Do_UnloadRight1VacumBreak,(int)ECATDONAME.Do_UnloadRight2VacumBreak,(int)ECATDONAME.Do_UnloadRight3VacumBreak,(int)ECATDONAME.Do_UnloadRight4VacumBreak};
        public int[] UnloadGantrySuckerCheckBits = new int[] {(int)ECATDINAME.Di_UnloadLeft1VacumCheck, (int)ECATDINAME.Di_UnloadLeft2VacumCheck, (int)ECATDINAME.Di_UnloadLeft3VacumCheck, (int)ECATDINAME.Di_UnloadLeft4VacumCheck,
                                                              (int)ECATDINAME.Di_UnloadRight1VacumCheck,(int)ECATDINAME.Di_UnloadRight2VacumCheck,(int)ECATDINAME.Di_UnloadRight3VacumCheck,(int)ECATDINAME.Di_UnloadRight4VacumCheck};
        public int[] UnloadGantryCylinderStretchControls = new int[] { (int)ECATDONAME.Do_UnloadBufferLeft1StretchControl, (int)ECATDONAME.Do_UnloadBufferLeft2StretchControl, (int)ECATDONAME.Do_UnloadBufferLeft3StretchControl, (int)ECATDONAME.Do_UnloadBufferLeft4StretchControl,
                                                                       (int)ECATDONAME.Do_UnloadBufferRight1StretchControl,(int)ECATDONAME.Do_UnloadBufferRight2StretchControl,(int)ECATDONAME.Do_UnloadBufferRight3StretchControl,(int)ECATDONAME.Do_UnloadBufferRight4StretchControl };
        public int[] UnloadGantryCylinderRetractControls = new int[] { (int)ECATDONAME.Do_UnloadBufferLeft1RetractControl, (int)ECATDONAME.Do_UnloadBufferLeft2RetractControl, (int)ECATDONAME.Do_UnloadBufferLeft3RetractControl, (int)ECATDONAME.Do_UnloadBufferLeft4RetractControl,
                                                                       (int)ECATDONAME.Do_UnloadBufferRight1RetractControl,(int)ECATDONAME.Do_UnloadBufferRight2RetractControl,(int)ECATDONAME.Do_UnloadBufferRight3RetractControl,(int)ECATDONAME.Do_UnloadBufferRight4RetractControl };
        public int[] UnloadGantryCylinderStretchCheckBits = new int[] { (int)ECATDINAME.Di_UnloadBufferLeft1StretchBit, (int)ECATDINAME.Di_UnloadBufferLeft2StretchBit, (int)ECATDINAME.Di_UnloadBufferLeft3StretchBit, (int)ECATDINAME.Di_UnloadBufferLeft4StretchBit,
                                                                        (int)ECATDINAME.Di_UnloadBufferRight1StretchBit,(int)ECATDINAME.Di_UnloadBufferRight2StretchBit,(int)ECATDINAME.Di_UnloadBufferRight3StretchBit,(int)ECATDINAME.Di_UnloadBufferRight4StretchBit};
        public int[] UnloadGantryCylinderRetractCheckBits = new int[] {(int)ECATDINAME.Di_UnloadBufferLeft1RetractBit, (int)ECATDINAME.Di_UnloadBufferLeft2RetractBit, (int)ECATDINAME.Di_UnloadBufferLeft3RetractBit, (int)ECATDINAME.Di_UnloadBufferLeft4RetractBit,
                                                                        (int)ECATDINAME.Di_UnloadBufferRight1RetractBit,(int)ECATDINAME.Di_UnloadBufferRight2RetractBit,(int)ECATDINAME.Di_UnloadBufferRight3RetractBit,(int)ECATDINAME.Di_UnloadBufferRight4RetractBit};

        public int ANumOriginal = 0;
        public int BNumOriginal = 0;
        public int CNumOriginal = 0;
        public int DNumOriginal = 0;
        public int ENumOriginal = 0;
        public int DropNumOriginal = 0;

        public bool isLoadTraySupplied = false;
        public bool isUnloadTraySupplied = false;
        public bool MainFormInitDoing = false;
        public bool MainFormMiniInitDoing = false;

        public FaiInfo[] myFaiInfoArray = new FaiInfo[25];

        #region CCD光源测试相关参数
        public int DelayBeforeLightChange = 40;
        public int DelayAfterLightChange = 50;
        public int DelayAfterSecondT1 = 40;
        public int DelayAfterLineMove = 90;
        #endregion

        public double LaserDiff = 0.075;
        public bool LaserRandomResult = true;

        #region FAI分布柱状图相关数据
        public List<int[]> faiDistributeCountList = new List<int[]>();//共25个fai，每个fai分割成40个区间 

        #endregion

        #region 属性
        public LogicConfig LogicConfigValue
        {
            get
            {
                return logicConfig;
            }
        }
        #endregion

        //初始化 ---加载配置文件 + 板卡、相机、IO卡初始化
        public bool Init(out string errorCode)
        {
            int ret = 0;
            debug = new DebugLog();

            CTLogfs = new FileStream(CTLogPath, System.IO.FileMode.Append, System.IO.FileAccess.Write);
            CTLogsw = new StreamWriter(CTLogfs, System.Text.Encoding.Default);
            CTLogsw.AutoFlush = true;

            MoveLogfs = new FileStream(MoveLogPath, System.IO.FileMode.Append, System.IO.FileAccess.Write);
            MoveLogsw = new StreamWriter(MoveLogfs, System.Text.Encoding.Default);
            MoveLogsw.AutoFlush = true;

            DownTimeLogfs = new FileStream(DownTimeLogPath, System.IO.FileMode.Append, System.IO.FileAccess.Write);
            DownTimeLogsw = new StreamWriter(DownTimeLogfs, System.Text.Encoding.Default);
            DownTimeLogsw.AutoFlush = true;

            CCDMoveLogfs = new FileStream(CCDMoveLogPath, System.IO.FileMode.Append, System.IO.FileAccess.Write);
            CCDMoveLogsw = new StreamWriter(CCDMoveLogfs, System.Text.Encoding.Default);
            CCDMoveLogsw.AutoFlush = true;

            errorCode = "";
            bool result = true;
            try
            {
                InitDataValue();
                InitDataFlow();

                //读取配置文件代码
                if (InitSystemConfig(ref errorCode) == false)
                    return false;

                WorkCount.GetOriginalInfo();

                #region 凌华卡&7432卡初始化
                int boardIdAdlink = logicConfig.boardIdCard;
                result = adlink.Init(ref boardIdAdlink, 0, pathCard, logicConfig.PulseAxis);
                if (!result)
                {
                    WarningSolution("【1】脉冲运动控制卡208C初始化失败");
                    errorCode = "脉冲运动控制卡初始化失败";
                    return false;
                }
                else
                {
                    UpdateWaringLog.Invoke((object)("脉冲运动控制卡初始化成功"));
                    for (int i = 0; i < logicConfig.PulseAxis.Length; i++)
                    {
                        ServoCtrl(logicConfig.PulseAxis[i].AxisId, true);
                    }
                }

                int ECATBoardIdCard = logicConfig.ECATBoardIdCard;
                result = adlink.Init(ref ECATBoardIdCard, 0, pathECATCard, logicConfig.ECATAxis);
                if (!result)
                {
                    WarningSolution("【2】EtherCAT运动控制卡8338初始化失败");
                    errorCode = "EtherCAT运动控制卡初始化失败";
                    return false;
                }
                else
                {
                    UpdateWaringLog.Invoke((object)"EtherCAT运动控制卡初始化成功");
                }

                //ret = APS168.APS_scan_field_bus(1, 0);
                //if (ret >= 0)
                //{
                //    UpdateWaringLog.Invoke((object)"EtherCAT总线扫描成功");
                //}
                //else
                //{
                //    UpdateWaringLogNG.Invoke((object)"EtherCAT总线扫描失败");
                //    errorCode = "EtherCAT总线扫描失败";
                //    return false;
                //}

                ret = APS168.APS_start_field_bus(1, 0, 8);
                if (ret >= 0)
                {
                    UpdateWaringLog.Invoke((object)"EtherCAT总线开启成功");
                    for (int i = 0; i < logicConfig.ECATAxis.Length; i++)
                    {
                        ServoCtrl(logicConfig.ECATAxis[i].AxisId, true);
                        for (int j = 0; j < 6 * 16; j++)
                            IOControl.ECATWriteDO(j, false);
                    }
                }
                else
                {
                    WarningSolution("【3】EtherCAT总线开启失败");
                    errorCode = "EtherCAT总线开启失败";
                    return false;
                }

                result = Dask7432.RegisterCard(0);
                if (!result)
                {
                    WarningSolution("【4】PCI7432初始化失败");
                    //errorCode = "PCI7432初始化失败";
                    return false;
                }
                else
                {
                    UpdateWaringLog.Invoke((object)("PCI7432初始化成功"));
                    if (!Dask7432.WriteAllDo(0, 0))
                    {
                        WarningSolution("【5】PCI7432写入DO失败");
                        errorCode = "PCI7432写入DO失败";
                        return false;
                    }
                    //IOControl.WriteDO((int)DONAME.Do_LedLight, true);
                }

                updateThreadFast = new Thread(new ThreadStart(UpdateThreadFast));   //开启实时更新快线程
                updateThreadFast.IsBackground = true;
                updateThreadFast.Priority = ThreadPriority.Highest;//0828
                updateThreadFast.Start();
                updateThreadSlow = new Thread(new ThreadStart(UpdateThreadSlow));   //开启实时更新慢线程
                updateThreadSlow.IsBackground = true;
                updateThreadSlow.Priority = ThreadPriority.Highest;//0828
                updateThreadSlow.Start();
                Thread.Sleep(100);
                refreshThread = new Thread(new ThreadStart(RefreshThread));  //操作响应及报警
                refreshThread.IsBackground = true;
                refreshThread.Start();
                refreshStatus = new Thread(new ThreadStart(RefreshStatus));  //操作响应及报警
                refreshStatus.IsBackground = true;
                refreshStatus.Start();
                #endregion

                #region Laser任务初始化
                //Task3DPaths=new string[8];
                #region 激光初始化
                if (keyenceLJ.Init(out errorCode))//0,"192.168.0.1", 24691,
                    UpdateWaringLog.Invoke((object)("Keyence激光初始化成功"));
                else
                {
                    WarningSolution("【6】Keyence激光初始化失败");
                    errorCode = "Keyence初始化成功失败" + errorCode;
                    return false;
                }
                if (keyenceLJ.EthernetOpen(0, "192.168.0.1", 24691, out errorCode)) //控制器1
                    UpdateWaringLog.Invoke((object)("1次LJ[0]-192.168.0.1通信成功"));
                else
                {
                    if (keyenceLJ.EthernetOpen(0, "192.168.0.1", 24691, out errorCode)) //控制器1
                        UpdateWaringLog.Invoke((object)("2次LJ[0]-192.168.0.1通信成功"));
                    else
                    {
                        WarningSolution("【7】LJ[0]通信失败");
                        errorCode = "KeyenceLJ-1通信失败" + errorCode;
                        return false;
                    }
                }
                isSet1Param = false;
                int trigCount = 0;
                for (int g = 0; g < moveConfig.moveConfig.Count(); g++)
                {
                    trigCount = moveConfig.moveConfig[g].nTrigNum;
                }
                if (!SetLaserHightSpeedParam(trigCount, out errorCode))
                {
                    WarningSolution("【8】Keyence激光初始设置失败");
                    errorCode = "Keyence初始初始设置失败" + errorCode;
                    return false;
                }
                #endregion

                #region 轨迹号与标定配置文件比对
                CalibPathNO = new Calib3DSturct[calib3D.calib3DStd.Length];
                for (int i = 0; i < calib3D.calib3DStd.Length; i++)
                {
                    if (i == calib3D.calib3DStd[i].PathId)
                    {
                        CalibPathNO[i] = calib3D.calib3DStd[i];
                    }
                }
                #endregion
                #endregion

                return KeyenceConnect();//CCD相机连接
            }
            catch (System.Exception ex)
            {
                errorCode = ex.ToString();
                WriteLog(ex.ToString());
                return false;
            }
        }

        public bool InitSystemConfig(ref string errorCode)
        {
            if (InitLogicConfig(ref logicConfig, ref errorCode) == false)
                return false;
            if (InitMoveConfig() == false)
                return false;
            if (InitCCDMotionPos(ref CCDMotionPos, ref errorCode) == false)
                return false;
            if (InitLoadGantryMotionPos(ref LoadGantryMotionPos, ref errorCode) == false)
                return false;
            if (InitUnloadGantryMotionPos(ref UnloadGantryMotionPos, ref errorCode) == false)
                return false;
            if (InitUnloadGantrySupplyMotionPos(ref UnloadGantrySupplyMotionPos, ref errorCode) == false)
                return false;
            if (InitSystemPara(ref systemParam, ref errorCode) == false)
                return false;
            if (InitThreshold(ref ThrParam, ref errorCode) == false)
                return false;
            if (InitThresholdShow(ref ThrParamShow, ref errorCode) == false)
                return false;
            if (InitCalib3DStd(ref errorCode) == false)
                return false;
            if (InitTaskConfig() == false)
                return false;
            InitCCDCalibMatrix(ref CCDCalibMatrix);
            InitStationCheckSetting();
            if (DataOffset.ReadLaserRawDataOffset(pathLaserRawDataOffset) == false)//读取laser原始数据offset
                return false;

            if (DataOffset.ReadAllOffset() == false)//读取CCD和Laser的Offset&Gradient
                return false;
            else
            {
                UpdateCCDOffset(new CCDDataOffset(DataOffset.ccdGradient, DataOffset.ccdOffset));
                UpdateWaringLog.Invoke((object)"CCD Offset读取成功");
                UpdateLaserOffset(new LaserDataOffset(DataOffset.laserGradient, DataOffset.laserOffset));
                UpdateWaringLog.Invoke((object)"Laser Offset读取成功");
            }

            return true;
        }

        private void InitDataFlow()
        {
            myPassRatio = new PassRatio(0, 0, 0.0, 0, 0, 0, 0, 0, 0);

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < systemParam.WorkPieceNum; j++)
                {
                    CameraCheckDoneArrayTray[i, j] = false;
                    LaserCheckDoneArrayTray[i, j] = false;
                    CameraCheckResultArrayTray[i, j] = 0;
                    LaserCheckResultArrayTray[i, j] = 0;
                    FinalCheckResultArrayTray[i, j] = 0;
                }
            }
        }

        public void ResetPassRatio()
        {
            ANumOriginal = 0;
            BNumOriginal = 0;
            CNumOriginal = 0;
            DNumOriginal = 0;
            ENumOriginal = 0;
            DropNumOriginal = 0;

            myPassRatio = new PassRatio(0, 0, 0.0, ANumOriginal, BNumOriginal, CNumOriginal, DNumOriginal, ENumOriginal, DropNumOriginal);
            UpdatePassRatio(myPassRatio);
        }

        private void InitDataValue()
        {
            PartATrayNo = 0; PartBTrayNo = 1; PartCTrayNo = 2;
            StationCheckSetting[0] = new StationCheckPara(4);
            StationCheckSetting[1] = new StationCheckPara(4);
            StationCheckSetting[2] = new StationCheckPara(4);
            StationCheckParaPaths[0] = Directory.GetCurrentDirectory() + @"\MyConfig\StationACheckPara.xml";//A工位检测
            StationCheckParaPaths[1] = Directory.GetCurrentDirectory() + @"\MyConfig\StationBCheckPara.xml";//B工位检测
            StationCheckParaPaths[2] = Directory.GetCurrentDirectory() + @"\MyConfig\StationCCheckPara.xml";//C工位检测
            AutoRunPartAStationEnable[0] = AutoRunPartAStationEnable[1] = AutoRunPartAStationEnable[2] = true;

            faiDistributeCountList.Clear();
            for (int i = 0; i < 25; i++)
                faiDistributeCountList.Add(new int[40]);

            myFaiInfoArray = new FaiInfo[25];
            for (int i = 0; i < 25; i++)
                myFaiInfoArray[i] = new FaiInfo(0, 0, 0, 0);

            TotalCheckANum = TotalCheckBNum = TotalCheckCNum = TotalCheckDNum = TotalCheckNum = 0;
            if (DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 20)
            {
                curWorkClassify = "Day";
                lastWorkClassify = "Day";
            }
            else
            {
                curWorkClassify = "Night";
                lastWorkClassify = "Night";
            }
        }

        private void InitStationCheckSetting()
        {
            for (int i = 0; i < 3; i++)
            {
                if (!File.Exists(StationCheckParaPaths[i]))
                {
                    StationCheckSetting[i].CCDCheck = true;
                    StationCheckSetting[i].LaserCheck = true;
                    for (int j = 0; j < 4; j++)
                    {
                        StationCheckSetting[i].CCDHoleCheck[j] = true;
                        StationCheckSetting[i].LaserHoleCheck[j] = true;
                    }
                    bool result = XmlSerializerHelper.WriteXML((object)StationCheckSetting[i], StationCheckParaPaths[i], typeof(StationCheckPara));
                    if (!result)
                    {
                        switch (i)
                        {
                            case 0:
                                WarningSolution("【9】站点A检查配置文件丢失");
                                break;
                            case 1:
                                WarningSolution("【10】站点B检查配置文件丢失");
                                break;
                            case 2:
                                WarningSolution("【11】站点C检查配置文件丢失");
                                break;
                        }
                        return;
                    }
                }
                else
                {
                    bool bFlag = false;
                    StationCheckSetting[i] = (StationCheckPara)XmlSerializerHelper.ReadXML(StationCheckParaPaths[i], typeof(StationCheckPara), out bFlag);
                    switch (i)
                    {
                        case 0:
                            UpdateWaringLog.Invoke((object)"站点A配置文件读取成功");
                            break;
                        case 1:
                            UpdateWaringLog.Invoke((object)"站点B配置文件读取成功");
                            break;
                        case 2:
                            UpdateWaringLog.Invoke((object)"站点C配置文件读取成功");
                            break;
                    }
                }
            }
        }

        public UpdateInfo CurInfo = new UpdateInfo(16, 137, 111);
        public MotionPos CCDMotionPos;
        public SystemParam systemParam = new SystemParam();
        public int systemParamSelectedIdx = -1;
        public ThresholdParam ThrParam = new ThresholdParam();
        public ThresholdParam ThrParamShow = new ThresholdParam();

        private void UpdateThreadFast()
        {
            bool[] diStatus1 = new bool[16]; bool[] doStatus1 = new bool[16];
            bool[] diStatus2 = new bool[16]; bool[] doStatus2 = new bool[16];
            bool[] diStatus3 = new bool[16]; bool[] doStatus3 = new bool[16];
            bool[] diStatus4 = new bool[16]; bool[] doStatus4 = new bool[16];
            bool[] diStatus5 = new bool[16]; bool[] doStatus5 = new bool[16];
            bool[] diStatus6 = new bool[16]; bool[] doStatus6 = new bool[16];
            bool[] diStatus7 = new bool[16];

            while (RefreshActive)
            {
                //实时更新IO点
                bool result = ECATIO.GetAllDi(logicConfig.ECATBoardIdCard, 0, ref diStatus1) &&
                              ECATIO.GetAllDi(logicConfig.ECATBoardIdCard, 1, ref diStatus2) &&
                              ECATIO.GetAllDi(logicConfig.ECATBoardIdCard, 2, ref diStatus3) &&
                              ECATIO.GetAllDi(logicConfig.ECATBoardIdCard, 3, ref diStatus4) &&
                              ECATIO.GetAllDi(logicConfig.ECATBoardIdCard, 4, ref diStatus5) &&
                              ECATIO.GetAllDi(logicConfig.ECATBoardIdCard, 5, ref diStatus6) &&
                              ECATIO.GetAllDi(logicConfig.ECATBoardIdCard, 6, ref diStatus7) &&
                              ECATIO.GetAllDo(logicConfig.ECATBoardIdCard, 0, ref doStatus1) &&
                              ECATIO.GetAllDo(logicConfig.ECATBoardIdCard, 1, ref doStatus2) &&
                              ECATIO.GetAllDo(logicConfig.ECATBoardIdCard, 2, ref doStatus3) &&
                              ECATIO.GetAllDo(logicConfig.ECATBoardIdCard, 3, ref doStatus4) &&
                              ECATIO.GetAllDo(logicConfig.ECATBoardIdCard, 4, ref doStatus5) &&
                              ECATIO.GetAllDo(logicConfig.ECATBoardIdCard, 5, ref doStatus6);

                if (result)
                {
                    for (int i = 25; i < 41; i++)
                    {
                        CurInfo.Di[i] = diStatus1[i - 25];
                    }
                    for (int i = 41; i < 57; i++)
                    {
                        CurInfo.Di[i] = diStatus2[i - 41];
                    }
                    for (int i = 57; i < 73; i++)
                    {
                        CurInfo.Di[i] = diStatus3[i - 57];
                    }
                    for (int i = 73; i < 89; i++)
                    {
                        CurInfo.Di[i] = diStatus4[i - 73];
                    }
                    for (int i = 89; i < 105; i++)
                    {
                        CurInfo.Di[i] = diStatus5[i - 89];
                    }
                    for (int i = 105; i < 121; i++)
                    {
                        CurInfo.Di[i] = diStatus6[i - 105];
                    }
                    for (int i = 121; i < 137; i++)
                    {
                        CurInfo.Di[i] = diStatus7[i - 121];
                    }

                    for (int i = 15; i < 31; i++)
                    {
                        CurInfo.Do[i] = doStatus1[i - 15];
                    }
                    for (int i = 31; i < 47; i++)
                    {
                        CurInfo.Do[i] = doStatus2[i - 31];
                    }
                    for (int i = 47; i < 63; i++)
                    {
                        CurInfo.Do[i] = doStatus3[i - 47];
                    }
                    for (int i = 63; i < 79; i++)
                    {
                        CurInfo.Do[i] = doStatus4[i - 63];
                    }
                    for (int i = 79; i < 95; i++)
                    {
                        CurInfo.Do[i] = doStatus5[i - 79];
                    }
                    for (int i = 95; i < 111; i++)
                    {
                        CurInfo.Do[i] = doStatus6[i - 95];
                    }
                }

                if ((isLoadTraySupplied == true) && (LoadFullTrayDrawerOpened == false))
                    LoadFullTrayDrawerOpened = (LastLoadFullTrayDrawerInPosStatus == false) && (CurInfo.Di[(int)ECATDINAME.Di_LoadFullDrawerInPosition + 25] == true);
                else if (isLoadTraySupplied == false)
                    LoadFullTrayDrawerOpened = false;

                if ((isUnloadTraySupplied == true) && (UnloadNullTrayDrawerOpened == false))
                    UnloadNullTrayDrawerOpened = (LastUnloadNullTrayDrawerInPosStatus == false) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadNullDrawerTrayInPosition + 25] == true);
                else if (isUnloadTraySupplied == false)
                    UnloadNullTrayDrawerOpened = false;

                LastLoadFullTrayDrawerInPosStatus = CurInfo.Di[(int)ECATDINAME.Di_LoadFullDrawerInPosition + 25];
                LastUnloadNullTrayDrawerInPosStatus = CurInfo.Di[(int)ECATDINAME.Di_UnloadNullDrawerTrayInPosition + 25];

                Thread.Sleep(15);//noted by lei.c 延长sleep时间
            }
        }

        private void UpdateThreadSlow()
        {
            bool[] diStatus7432 = new bool[32];
            bool[] doStatus7432 = new bool[32];

            while (RefreshActive)
            {
                //实时更新坐标
                double[] pulseAxisPos = new double[0]; double[] ecatAxisPos = new double[0];
                bool result = adlink.GetEncodingPosition(logicConfig.PulseAxis, ref pulseAxisPos);
                result = adlink.GetEncodingPosition(logicConfig.ECATAxis, ref ecatAxisPos);
                for (int i = 0; i < logicConfig.PulseAxis.Length; i++)
                {
                    CurInfo.CurAxisPos[i] = pulseAxisPos[i];
                }
                for (int i = 0; i < logicConfig.ECATAxis.Length; i++)
                {
                    CurInfo.CurAxisPos[i + logicConfig.PulseAxis.Length] = ecatAxisPos[i];
                }

                //实时更新IO点
                result = Dask7432.ReadAllDi(0, ref diStatus7432) &&
                    Dask7432.ReadAllDo(0, ref doStatus7432);

                if (result)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        CurInfo.Di[i] = diStatus7432[i];
                    }
                    for (int i = 5; i < 15; i++)
                    {
                        CurInfo.Di[i] = diStatus7432[i + 1];
                    }
                    for (int i = 15; i < 25; i++)
                    {
                        CurInfo.Di[i] = diStatus7432[i + 7];
                    }

                    for (int i = 0; i < 15; i++)
                    {
                        CurInfo.Do[i] = doStatus7432[i];
                    }
                }
                //实时更新轴模组状态信息
                for (int i = 0; i < 6; i++)
                {
                    int[] data = new int[16];
                    adlink.ReadMotionIO(logicConfig.PulseAxis[i].AxisId, ref data);
                    int[] dataMotion = new int[32];
                    adlink.ReadMotionStatus(logicConfig.PulseAxis[i].AxisId, ref dataMotion);
                    CurInfo.motionIO[i].alm = data[0];
                    CurInfo.motionIO[i].pel = data[1];
                    CurInfo.motionIO[i].mel = data[2];
                    CurInfo.motionIO[i].org = data[3];
                    CurInfo.motionIO[i].emg = data[4];
                    CurInfo.motionIO[i].ez = data[5];
                    CurInfo.motionIO[i].inp = data[6];
                    CurInfo.motionIO[i].svon = data[7];
                    CurInfo.motionIO[i].dir = dataMotion[4];
                    CurInfo.motionIO[i].mdn = dataMotion[5];
                    CurInfo.motionIO[i].hmv = dataMotion[6];
                    CurInfo.motionIO[i].jog = dataMotion[15];
                }
                //实时更新总线轴模组状态信息
                for (int i = 0; i < 8; i++)
                {
                    int[] data = new int[16];
                    adlink.ReadMotionIO(logicConfig.ECATAxis[i].AxisId, ref data);
                    int[] dataMotion = new int[32];
                    adlink.ReadMotionStatus(logicConfig.ECATAxis[i].AxisId, ref dataMotion);
                    CurInfo.motionIO[i + 6].alm = data[0];
                    CurInfo.motionIO[i + 6].pel = data[1];
                    CurInfo.motionIO[i + 6].mel = data[2];
                    CurInfo.motionIO[i + 6].org = data[3];
                    CurInfo.motionIO[i + 6].emg = data[4];
                    CurInfo.motionIO[i + 6].ez = data[5];
                    CurInfo.motionIO[i + 6].inp = data[6];
                    CurInfo.motionIO[i + 6].svon = data[7];
                    CurInfo.motionIO[i + 6].dir = dataMotion[4];
                    CurInfo.motionIO[i + 6].mdn = dataMotion[5];
                    CurInfo.motionIO[i + 6].hmv = dataMotion[6];
                    CurInfo.motionIO[i + 6].jog = dataMotion[15];
                }
                if (UpdateMotionStatus != null)
                {
                    UpdateMotionStatus.Invoke(CurInfo);
                }

                UpdateSafetySignal();
                UpdateLogPath();

                Thread.Sleep(25);//noted by lei.c 延长sleep时间
            }
        }

        private void RefreshStatus()
        {
            int lastStatus = -1;
            while (RefreshActive)
            {
                if (lastStatus == -1)
                {
                    DoStatusBindingStuff(CurStatus);
                    lastStatus = CurStatus;
                }
                else
                {
                    if (CurStatus != lastStatus)
                    {
                        DoStatusBindingStuff(CurStatus);
                        UpdateButtonStatus(CurStatus);
                    }
                    lastStatus = CurStatus;
                }

                MonitorCCDCommunication();
                Thread.Sleep(100);
            }
        }

        private void DoStatusBindingStuff(int Status)
        {
            switch (Status)
            {
                case (int)STATUS.READY_STATUS:
                    ReadyStatusRelatedActivity();
                    break;
                case (int)STATUS.AUTO_STATUS:
                    AutoStatusRelatedActivity();
                    break;
                case (int)STATUS.PAUSE_STATUS:
                    PauseStatusRelatedActivity();
                    break;
                case (int)STATUS.STOP_STATUS:
                    StopStatusRelatedActivity();
                    break;
                case (int)STATUS.MANUAL_STATUS:
                    ManualStatusRelatedActivity();
                    break;
                default:
                    return;
            }
        }

        private void MonitorCCDCommunication()
        {
            if (gbEnableMonitorCCDCommunication)
            {
                if (CurStatus == (int)STATUS.AUTO_STATUS && client.IsConnected==false)
                {
                    Tcp_DisConnect();
                    tcp_enable = false;

                    if (KeyenceConnect()==false)
                    {
                        Tcp_DisConnect();
                        tcp_enable = false;

                        WarningSolution("CCD通讯断线,程序停止");
                        MessageBox.Show("CCD通讯断线,程序停止");
                        SwitchToPauseMode();
                    }
                }
            }
        }

        private void UpdateSafetySignal()
        {
            mainaxismovesafesignal = (CurInfo.Di[(int)ECATDINAME.Di_SuckZRetractBit + 25]) && (!CurInfo.Di[(int)ECATDINAME.Di_SuckZStretchBit + 25]) &&
                                         (!CurInfo.Di[(int)DINAME.Di_EnterPushUp1StretchBit]) && (!CurInfo.Di[(int)DINAME.Di_EnterPushUp2StretchBit]) &&
                                         (CurInfo.Di[(int)DINAME.Di_EnterPushUp1RetractBit]) && (CurInfo.Di[(int)DINAME.Di_EnterPushUp2RetractBit]) &&
                                         (CurInfo.Di[(int)DINAME.Di_EnterStirRetractBit]) && (!CurInfo.Di[(int)DINAME.Di_EnterStirStretchBit]) &&
                                         (CurInfo.Di[(int)DINAME.Di_EnterMoveStretchBit]) && (!CurInfo.Di[(int)DINAME.Di_EnterMoveRetractBit]);

            mainaxishomesafesignal = (!CurInfo.Di[(int)DINAME.Di_CarrierSense]) && mainaxismovesafesignal;

            loadgantrymovesafesignal = CurInfo.Di[(int)ECATDINAME.Di_LoadZRetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadZStretchBit + 25]) &&
                                       CurInfo.Di[(int)ECATDINAME.Di_LoadBufferLeftRetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadBufferLeftStretchBit + 25]) &&
                                       CurInfo.Di[(int)ECATDINAME.Di_LoadBufferRightRetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadBufferRightStretchBit + 25]);

            unloadgantrymovesafesignal = CurInfo.Di[(int)ECATDINAME.Di_UnloadZRetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadZStretchBit + 25]) &&
                                         CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft1RetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft1StretchBit + 25]) &&
                                         CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft2RetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft2StretchBit + 25]) &&
                                         CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft3RetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft3StretchBit + 25]) &&
                                         CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft4RetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft4StretchBit + 25]) &&
                                         CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight1RetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight1StretchBit + 25]) &&
                                         CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight2RetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight2StretchBit + 25]) &&
                                         CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight3RetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight3StretchBit + 25]) &&
                                         CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight4RetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight4StretchBit + 25]);

            suckaxissafesignal = (CurInfo.Di[(int)ECATDINAME.Di_SuckZRetractBit + 25]) && (!CurInfo.Di[(int)ECATDINAME.Di_SuckZStretchBit + 25]);

            LoadGantrySuckSafeSignal = (CurInfo.Di[(int)ECATDINAME.Di_LoadTrayMoveStretchBit + 25]) && (!CurInfo.Di[(int)ECATDINAME.Di_LoadTrayMoveRetractBit + 25]);
            UnloadGantryPlaceSafeSignal = (CurInfo.Di[(int)ECATDINAME.Di_UnloadTrayMoveStretchBit + 25]) && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadTrayMoveRetractBit + 25]);

        }

        private void UpdateLogPath()
        {
            string tempMoveLogPath = @".\Log\Move-" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + ".txt";
            string tempCTLogPath = @".\Log\CT-" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + ".txt";
            string tempDownTimeLogPath = @".\Log\DownTime-" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + ".txt";
            string tempCCDMoveLogPath= @".\Log\CCDMoveLog-" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + ".txt";

            if (tempMoveLogPath != MoveLogPath)
            {
                MoveLogPath = tempMoveLogPath;
                MoveLogfs = new FileStream(MoveLogPath, System.IO.FileMode.Append, System.IO.FileAccess.Write);
                MoveLogsw = new StreamWriter(MoveLogfs, System.Text.Encoding.Default);
                MoveLogsw.AutoFlush = true;
            }

            if (tempCTLogPath != CTLogPath)
            {
                CTLogPath = tempCTLogPath;
                CTLogfs = new FileStream(CTLogPath, System.IO.FileMode.Append, System.IO.FileAccess.Write);
                CTLogsw = new StreamWriter(CTLogfs, System.Text.Encoding.Default);
                CTLogsw.AutoFlush = true;
            }

            if (tempDownTimeLogPath != DownTimeLogPath)
            {
                DownTimeLogPath = tempDownTimeLogPath;
                DownTimeLogfs = new FileStream(DownTimeLogPath, System.IO.FileMode.Append, System.IO.FileAccess.Write);
                DownTimeLogsw = new StreamWriter(DownTimeLogfs, System.Text.Encoding.Default);
                DownTimeLogsw.AutoFlush = true;
            }

            if (tempCCDMoveLogPath != CCDMoveLogPath)
            {
                CCDMoveLogPath = tempCCDMoveLogPath;
                CCDMoveLogfs = new FileStream(CCDMoveLogPath, System.IO.FileMode.Append, System.IO.FileAccess.Write);
                CCDMoveLogsw = new StreamWriter(CCDMoveLogfs, System.Text.Encoding.Default);
                CCDMoveLogsw.AutoFlush = true;
            }
        }

        private string ShowWarningAxisInfo(int i)
        {
            string temp = null;
            switch (i)
            {
                case 0:
                    temp = "旋转工作台驱动器报警";
                    break;
                case 1:
                    temp = "CCD检测X轴驱动器报警";
                    break;
                case 2:
                    temp = "CCD检测Y轴驱动器报警";
                    break;
                case 3:
                    temp = "激光检测X轴驱动器报警";
                    break;
                case 4:
                    temp = "激光检测Y轴驱动器报警";
                    break;
                case 5:
                    temp = "横移轴驱动器报警";
                    break;
                case 6:
                    temp = "上料龙门X轴驱动器报警";
                    break;
                case 7:
                    temp = "上料龙门Y轴驱动器报警";
                    break;
                case 8:
                    temp = "下料龙门X轴驱动器报警";
                    break;
                case 9:
                    temp = "下料龙门Y轴驱动器报警";
                    break;
                case 10:
                    temp = "上料空Tray Z轴驱动器报警";
                    break;
                case 11:
                    temp = "上料满Tray Z轴驱动器报警";
                    break;
                case 12:
                    temp = "下料空Tray Z轴驱动器报警";
                    break;
                case 13:
                    temp = "下料满Tray Z轴驱动器报警";
                    break;
            }
            return temp;
        }

        public bool UnresetbleAlm, MainAxisThreadAlm, PartAThreadAlm, CCDCheckThreadAlm, LaserCheckThreadAlm;
        public bool LoadTrayThreadAlm, LoadGantryThreadAlm, LoadModuleThreadAlm, SuckAxisThreadAlm, UnloadModuleThreadAlm, UnloadGantryThreadAlm;
        public bool UnloadTrayThreadAlm, UnloadGantryPlaceAllWorkPieceThreadAlm, LoadTrayAllSwitchThreadAlm, UnloadTrayAllSwitchThreadAlm;
        public bool ResetBeep = false;
        public bool SafetyDoorSignal = false;

        public bool LastSwitchBtnStatus = false;
        public bool LastEmgStopStatus = false;

        private void RefreshThread()
        {
            while (RefreshActive)
            {
                // 安全门检测
                SafetyDoorSignal = CurInfo.Di[(int)DINAME.Di_Door1Check] && CurInfo.Di[(int)DINAME.Di_Door2Check] &&
                                   CurInfo.Di[(int)DINAME.Di_Door3Check] && CurInfo.Di[(int)DINAME.Di_Door4Check] &&
                                   CurInfo.Di[(int)DINAME.Di_Door5Check] && CurInfo.Di[(int)DINAME.Di_Door6Check] &&
                                   CurInfo.Di[(int)DINAME.Di_Door7Check] && CurInfo.Di[(int)DINAME.Di_Door8Check] &&
                                   CurInfo.Di[(int)DINAME.Di_Door9Check] && CurInfo.Di[(int)DINAME.Di_Door10Check];

                if ((!SafetyDoorSignal) && (systemParam.IgnoreDoor == 0) && (CurStatus == (int)STATUS.AUTO_STATUS))
                {
                    SwitchToPauseMode();
                    WarningSolution("【12】安全门未关");
                    MessageBox.Show("安全门未关，程序暂停！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                //1014
                if ((CurStatus == (int)STATUS.AUTO_STATUS) && (CurInfo.Di[(int)DINAME.Di_AirPressure]==false))
                {
                    SwitchToEmgStopMode();
                    MessageBox.Show("气压压力过低，停止运行，需重新初始化！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // 报警及复位
                if (!UnresetbleAlm)
                {
                    for (int i = 0; i < 14; i++)
                    {
                        if (CurInfo.motionIO[i].alm == 1)
                        {
                            WarningSolution("【13】" + ShowWarningAxisInfo(i));
                        }
                    }
                    if (!CurInfo.Di[(int)DINAME.Di_EmgStopBtn])
                    {
                        WarningSolution("【14】急停被按下！");
                    }
                }
                UnresetbleAlm = (CurInfo.motionIO[0].alm == 1) || (CurInfo.motionIO[1].alm == 1) || (CurInfo.motionIO[2].alm == 1) || (CurInfo.motionIO[3].alm == 1) ||
                                (CurInfo.motionIO[4].alm == 1) || (CurInfo.motionIO[5].alm == 1) || (CurInfo.motionIO[6].alm == 1) || (CurInfo.motionIO[7].alm == 1) ||
                                (CurInfo.motionIO[8].alm == 1) || (CurInfo.motionIO[9].alm == 1) || (CurInfo.motionIO[10].alm == 1) || (CurInfo.motionIO[11].alm == 1) ||
                                (CurInfo.motionIO[12].alm == 1) || (CurInfo.motionIO[13].alm == 1) || (!CurInfo.Di[(int)DINAME.Di_EmgStopBtn]);
                if (UnresetbleAlm)
                {
                    MotionStop();
                    if (CurStatus != (int)STATUS.MANUAL_STATUS)
                        CurStatus = (int)STATUS.STOP_STATUS;
                }

                // 按下启动按钮
                if (CurInfo.Di[(int)DINAME.Di_StartBtn])
                {
                    StartButton();
                }

                //按下复位按钮done
                if (CurInfo.Di[(int)DINAME.Di_ResetBtn])
                {
                    ResetButton();
                }

                //按下急停按钮done
                //if (!CurInfo.Di[(int)DINAME.Di_EmgStopBtn])
                //{
                //    EmgStopButtion();
                //}
                if ((LastEmgStopStatus != CurInfo.Di[(int)DINAME.Di_EmgStopBtn]) && (!CurInfo.Di[(int)DINAME.Di_EmgStopBtn]))
                {
                    EmgStopButton();
                }

                //按下停止按钮done,actually，it's a pause button
                if (CurInfo.Di[(int)DINAME.Di_StopBtn])
                {
                    PauseButton();
                }

                //旋转手自动切换按钮
                if (CurInfo.Di[(int)DINAME.Di_SwitchBtn] != LastSwitchBtnStatus)
                {
                    if (CurInfo.Di[(int)DINAME.Di_SwitchBtn] == false)
                    {
                        switch (CurStatus)
                        {
                            case (int)STATUS.READY_STATUS:
                                SwitchToManualMode();
                                break;
                            case (int)STATUS.AUTO_STATUS:
                                MessageBox.Show("当前状态为自动运行，无法切换至手动，请先停止");
                                break;
                            case (int)STATUS.PAUSE_STATUS:
                                SwitchToManualMode();
                                break;
                            case (int)STATUS.STOP_STATUS:
                                SwitchToManualMode();
                                break;
                            case (int)STATUS.MANUAL_STATUS:
                                break;
                            default: break;
                        }
                    }
                    else
                    {
                        switch (CurStatus)
                        {
                            case (int)STATUS.MANUAL_STATUS:
                                NormalVelocity();
                                CurStatus = (int)STATUS.READY_STATUS;
                                break;
                            default: break;
                        }
                    }
                }

                if (CurInfo.Di[(int)DINAME.Di_SwitchBtn] == false)
                {
                    switch (CurStatus)
                    {
                        case (int)STATUS.READY_STATUS:
                            SwitchToManualMode();
                            break;
                        case (int)STATUS.AUTO_STATUS:
                            MessageBox.Show("当前状态为自动运行，无法切换至手动，请先停止");
                            break;
                        case (int)STATUS.PAUSE_STATUS:
                            SwitchToManualMode();
                            break;
                        case (int)STATUS.STOP_STATUS:
                            SwitchToManualMode();
                            break;
                        case (int)STATUS.MANUAL_STATUS:
                            break;
                        default: break;
                    }
                }

                LastSwitchBtnStatus = CurInfo.Di[(int)DINAME.Di_SwitchBtn];
                LastEmgStopStatus = CurInfo.Di[(int)DINAME.Di_EmgStopBtn];
                Thread.Sleep(50);
            }
        }

        public void NormalVelocity()
        {
            bool bFlag = false;
            logicConfig = XmlSerializerHelper.ReadXML(pathLogicConfig, typeof(LogicConfig), out bFlag) as LogicConfig;
        }

        //0906 add by ben*******
        #region Laser任务初始化
        public bool LaserMiniInit(out string errorCode)
        {
            isSet1Param = false;
            int trigCount = 0;
            for (int g = 0; g < moveConfig.moveConfig.Count(); g++)
            {
                trigCount = moveConfig.moveConfig[g].nTrigNum;
            }
            if (!SetLaserHightSpeedParam(trigCount, out errorCode))
            {
                WarningSolution("【8】Keyence激光初始设置失败");
                errorCode = "Keyence初始初始设置失败" + errorCode;
                return false;
            }
            return true;

        }
        #endregion
        //0906 add by ben*******

        public void RecoverAllPart()
        {
            if (CheckDrawerSignal() == false)
                return;

            CCDChecking = true;
            //isSet1Param = true;
            AutoRunEnablePartA = !logicIgnore[0];
            AutoRunEnablePartB = !logicIgnore[1];
            AutoRunEnablePartC = !logicIgnore[2];
            AutoRunEnableMainAxis = true;
            LoadTrayEnable = true;
            UnloadTrayEnable = true;
            if ((bDelayStop == false) && (bDelayStopCount == false))
                LoadGantryEnable = true;
            else
            {
                if (LoadGantryCircleCount < iDelayStopCount)
                    LoadGantryEnable = true;
            }
            UnloadGantryEnable = true;
            LoadModuleEnable = true;
            UnloadModuleEnable = true;
            SuckAxisMoveEnable = true;
            UnloadGantryPlaceAllWorkPieceEnable = true;
            LoadTrayAllSwitchEnable = true;
            UnloadTrayAllSwitchEnable = true;

        }

        public bool CheckDrawerSignal()
        {
            string tempstring = "";
            if (CurInfo.Di[(int)ECATDINAME.Di_NGDrawerInPosition + 25] || CurInfo.Di[(int)ECATDINAME.Di_LoadNullDrawerInPosition + 25] ||
                CurInfo.Di[(int)ECATDINAME.Di_LoadFullDrawerInPosition + 25] || CurInfo.Di[(int)ECATDINAME.Di_UnloadNullDrawerTrayInPosition + 25] ||
                CurInfo.Di[(int)ECATDINAME.Di_UnloadFullDrawerTrayInPosition + 25])
            {
                if (CurInfo.Di[(int)ECATDINAME.Di_NGDrawerInPosition + 25])
                    tempstring += "NG料盒抽屉未到位，请推到位" + "\n";
                if (CurInfo.Di[(int)ECATDINAME.Di_LoadNullDrawerInPosition + 25])
                    tempstring += "上料空Tray抽屉未到位，请推到位" + "\n";
                if (CurInfo.Di[(int)ECATDINAME.Di_LoadFullDrawerInPosition + 25])
                    tempstring += "上料满Tray抽屉未到位，请推到位" + "\n";
                if (CurInfo.Di[(int)ECATDINAME.Di_UnloadNullDrawerTrayInPosition + 25])
                    tempstring += "下料空Tray抽屉未到位，请推到位" + "\n";
                if (CurInfo.Di[(int)ECATDINAME.Di_UnloadFullDrawerTrayInPosition + 25])
                    tempstring += "下料满Tray抽屉未到位，请推到位" + "\n";
                MessageBox.Show(tempstring);
                return false;
            }
            return true;
        }

        public void PauseAllPart()
        {
            CTIgnorePause = true; //0907 by ben 计算当前CT忽略暂停时间

            AutoRunEnablePartA = false;
            AutoRunEnablePartB = false;
            AutoRunEnablePartC = false;
            AutoRunEnableMainAxis = false;
            LoadTrayEnable = false;
            UnloadTrayEnable = false;
            LoadGantryEnable = false;
            UnloadGantryEnable = false;
            LoadModuleEnable = false;
            UnloadModuleEnable = false;
            SuckAxisMoveEnable = false;
            WriteLog("***此处调用了暂停，。。。。。。被置为fales");//0907 by ben
            UnloadGantryPlaceAllWorkPieceEnable = false;
            LoadTrayAllSwitchEnable = false;
            UnloadTrayAllSwitchEnable = false;
        }

        //added by lei.c 切换至手动模式
        private void SwitchToManualMode()
        {
            //SlowVelocity();
            PauseAllPart();
            AutoRunActive = false;
            CurStatus = (int)STATUS.MANUAL_STATUS;
        }

        public void SlowVelocity()
        {
            logicConfig.PulseAxis[0].MoveVel = 20000;
            logicConfig.PulseAxis[0].MoveAcc = 200000;
            logicConfig.PulseAxis[0].MoveDec = 200000;

            logicConfig.PulseAxis[1].MoveVel = 20000;
            logicConfig.PulseAxis[1].MoveAcc = 200000;
            logicConfig.PulseAxis[1].MoveDec = 200000;

            logicConfig.PulseAxis[2].MoveVel = 10000;
            logicConfig.PulseAxis[2].MoveAcc = 100000;
            logicConfig.PulseAxis[2].MoveDec = 100000;

            logicConfig.PulseAxis[3].MoveVel = 4000;
            logicConfig.PulseAxis[3].MoveAcc = 40000;
            logicConfig.PulseAxis[3].MoveDec = 40000;

            logicConfig.PulseAxis[4].MoveVel = 10000;
            logicConfig.PulseAxis[4].MoveAcc = 100000;
            logicConfig.PulseAxis[4].MoveDec = 100000;

            logicConfig.PulseAxis[5].MoveVel = 20000;
            logicConfig.PulseAxis[5].MoveAcc = 100000;
            logicConfig.PulseAxis[5].MoveDec = 100000;

            for (int i = 0; i < logicConfig.ECATAxis.Length; i++)
            {
                logicConfig.ECATAxis[i].MoveVel = 20000;
                logicConfig.ECATAxis[i].MoveAcc = 200000;
                logicConfig.ECATAxis[i].MoveDec = 200000;
            }
        }

        //added by lei.c 切换至停止emgstop模式
        public void SwitchToEmgStopMode()
        {
            PauseAllPart();
            AutoRunActive = false;
            CCDChecking = false;
            bInitEnvironmentFinished = false;
            bInitTrayZFinished = false;
            AutoRunStepA = 0;
            AutoRunStepB = 0;
            AutoRunStepC = 0;
            MotionStop();//轴急停
            if (CurStatus != (int)STATUS.MANUAL_STATUS)
            {
                CurStatus = (int)STATUS.STOP_STATUS;
            }

            IOControl.ECATWriteDO((int)ECATDONAME.Do_CCDDownLightCmd, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_CCDUpLightCmd, false);
        }

        public void SwitchToPauseMode()
        {
            CCDChecking = false;
            CurStatus = (int)STATUS.PAUSE_STATUS;
            PauseAllPart();
        }

        //added by lei.c 新改写的Reset，用于取代之前的Reset
        public void SystemReset()
        {
            bool SystemResetDone = false;
            int SysResetStep = 0;

            while (!SystemResetDone)
            {
                switch (SysResetStep)
                {
                    case (int)SystemResetStep.AxisStop:
                        for (int i = 0; i < 6; i++)
                            adlink.StopMove(logicConfig.PulseAxis[i]);
                        SysResetStep = (int)SystemResetStep.SuckerMotion;
                        break;
                    case (int)SystemResetStep.SuckerMotion:
                        //待添加
                        SysResetStep = (int)SystemResetStep.StepStop;
                        break;
                    case (int)SystemResetStep.StepStop:
                        IOControl.WriteDO((int)DONAME.Do_ServoReset, true);
                        Thread.Sleep(150);
                        IOControl.WriteDO((int)DONAME.Do_ServoReset, false);
                        SysResetStep = (int)SystemResetStep.InitEnvironment;
                        break;
                    case (int)SystemResetStep.InitEnvironment:
                        InitTestEnvironment();
                        SysResetStep = (int)SystemResetStep.DataReset;
                        break;
                    case (int)SystemResetStep.DataReset:
                        ResetFlag();
                        SystemResetDone = true;
                        break;
                }
                Thread.Sleep(50);
            }
        }

        //自动运行的标志位复位
        public void ResetFlag()
        {
            AutoRunActive = false;
            RecoverAllPart();

            MainAxisMoveFinish = AutoRunPartAFinished = AutoRunPartBFinished = AutoRunPartCFinished = true;
            LoadTrayFinished = true; UnloadTrayFinished = true;
            LoadTrayAllSwitchFinished = true; UnloadTrayAllSwitchFinished = true;
            LoadGantrySuckFinished = false; UnloadGantrySuckFinished = false;
            LoadModuleFinished = true; UnloadModuleFinished = true;
            SuckAxisMoveFinished = false;


            AutoRunMainAxisCircleCount = AutoRunPartACircleCount = AutoRunPartBCircleCount = AutoRunPartCCircleCount = 0;
            LoadTrayCircleCount = UnloadTrayCircleCount = LoadGantryCircleCount = UnloadGantryCircleCount = LoadModuleCircleCount = UnloadModuleCircleCount = SuckAxisMoveCircleCount = UnloadGantryPlaceAllWorkPieceCircleCount = 0;
            CurLoadFullTraySeq = CurUnloadFullTraySeq = 0;

            AutoRunStepA = 0; AutoRunStepB = 0; AutoRunStepC = 0;
            LoadTrayStep = UnloadTrayStep = LoadModuleStep = UnloadModuleStep = SuckAxisMoveStep = LoadGantryStep = UnloadGantryStep = UnloadGantryPlaceAllWorkPieceStep = 0;

            AutoRunPartAStretchFinish = LoadModuleMoveCylinderFinish = LoadModuleMotionMoveCylinderFinish = UnloadModuleMoveCylinderFinish = UnloadModuleMotionMoveCylinderFinish = false;
            UnloadGantryPlaceAllWorkPieceFinish = true;
            CurLoadFullTraySeq = CurUnloadFullTraySeq = 0;
            SuckAxisSuck1Finish = SuckAxisSuck2Finish = SuckAxisPlace1Finish = SuckAxisPlace2Finish = false;
            AutoRunLaserFinishCount = 0;
            PartATrayNo = 0; PartBTrayNo = 1; PartCTrayNo = 2;

            CCDChecking = false;
            RecvCount = 0;
            RecvClassify = 0;

            myCTInfo = new CTInfoStruct(0, 0, 0, 0);

            bMiniInit = false;

            SupplyRegion1Condition = new int[8];
            SupplyRegion2Condition = new int[8];
            bDelayStop = false;
            bDelayStopCount = false; iDelayStopCount = -1;

            ReadyForUnloadAllSwitch = false;
            #region 复位故障
            ResetBeep = true;
            MainAxisThreadAlm = PartAThreadAlm = CCDCheckThreadAlm = LaserCheckThreadAlm = LoadTrayAllSwitchThreadAlm = UnloadTrayAllSwitchThreadAlm = false;
            LoadTrayThreadAlm = LoadGantryThreadAlm = LoadModuleThreadAlm = SuckAxisThreadAlm = false;
            UnloadModuleThreadAlm = UnloadGantryThreadAlm = UnloadTrayThreadAlm = UnloadGantryPlaceAllWorkPieceThreadAlm = false;
            #endregion

            //0826
            LoadModuleWorkPieceNum = 0;      //上料模组工件数目
            UnloadModuleWorkPieceNum = 0;    //下料模组工件数目
            EnterMoveWorkPieceNum = 0;       //入料工位工件数目

            isResetClicked = true;
            isLoadTraySupplied = false;
            isUnloadTraySupplied = false;

            LoadTraySuckTryCount = 0;
            CCDRedoCount = 0;
            LaserRedoCount = 0;

            PauseButtonClicked = false;
            LoadFullTrayDrawerOpened = false;
            UnloadNullTrayDrawerOpened = false;

            UnloadNullTraySuckTryCount = 0;
            LoadFullTraySuckTryCount = 0;
        }

        public void AutoRun()
        {
            //edited by lei.c
            if (mainAxisThread != null)
                mainAxisThread.Abort();
            mainAxisThread = new Thread(new ThreadStart(MainAxisMoveThread));
            mainAxisThread.IsBackground = true;
            mainAxisThread.Start();


            if (partAThread != null)
                partAThread.Abort();
            partAThread = new Thread(new ThreadStart(AutoRunPartAThread));
            partAThread.IsBackground = true;
            partAThread.Start();

            if (partBThread != null)
                partBThread.Abort();
            partBThread = new Thread(new ThreadStart(AutoRunPartBThread));
            partBThread.IsBackground = true;
            partBThread.Start();


            if (partCThread != null)
                partCThread.Abort();
            partCThread = new Thread(new ThreadStart(AutoRunPartCThread));
            partCThread.IsBackground = true;
            partCThread.Start();


            if (loadTrayThread != null)
                loadTrayThread.Abort();
            loadTrayThread = new Thread(new ThreadStart(LoadTrayThread));
            loadTrayThread.IsBackground = true;
            loadTrayThread.Start();


            if (unloadTrayThread != null)
                unloadTrayThread.Abort();
            unloadTrayThread = new Thread(new ThreadStart(UnloadTrayThread));
            unloadTrayThread.IsBackground = true;
            unloadTrayThread.Start();


            if (loadGantryThread != null)
                loadGantryThread.Abort();
            loadGantryThread = new Thread(new ThreadStart(LoadGantryThread));
            loadGantryThread.IsBackground = true;
            loadGantryThread.Start();


            if (unloadGantryThread != null)
                unloadGantryThread.Abort();
            unloadGantryThread = new Thread(new ThreadStart(UnloadGantryThread));
            unloadGantryThread.IsBackground = true;
            unloadGantryThread.Start();


            if (loadModuleThread != null)
                loadModuleThread.Abort();
            loadModuleThread = new Thread(new ThreadStart(LoadModuleThread));
            loadModuleThread.IsBackground = true;
            loadModuleThread.Start();


            if (unloadModuleThread != null)
                unloadModuleThread.Abort();
            unloadModuleThread = new Thread(new ThreadStart(UnloadModuleThread));
            unloadModuleThread.IsBackground = true;
            unloadModuleThread.Start();


            if (suckAxisMoveThread != null)
                suckAxisMoveThread.Abort();
            suckAxisMoveThread = new Thread(new ThreadStart(SuckAxisMoveThread));
            suckAxisMoveThread.IsBackground = true;
            suckAxisMoveThread.Start();


            if (unloadGantryPlaceAllWorkPieceThread != null)
                unloadGantryPlaceAllWorkPieceThread.Abort();
            unloadGantryPlaceAllWorkPieceThread = new Thread(new ThreadStart(UnloadGantryPlaceAllWorkPieceThread));
            unloadGantryPlaceAllWorkPieceThread.IsBackground = true;
            unloadGantryPlaceAllWorkPieceThread.Start();


            if (loadTrayAllSwitchThread != null)
                loadTrayAllSwitchThread.Abort();
            loadTrayAllSwitchThread = new Thread(new ThreadStart(LoadTrayAllSwitchThread));
            loadTrayAllSwitchThread.IsBackground = true;
            loadTrayAllSwitchThread.Start();


            if (unloadTrayAllSwitchThread != null)
                unloadTrayAllSwitchThread.Abort();
            unloadTrayAllSwitchThread = new Thread(new ThreadStart(UnloadTrayAllSwitchThread));
            unloadTrayAllSwitchThread.IsBackground = true;
            unloadTrayAllSwitchThread.Start();
        }

        public bool MainAxisMoveFinish = false;
        public bool AutoRunEnableMainAxis = false;//added by lei.c 表明是否开启主轴动作
        public int AutoRunMainAxisCircleCount = 0;
        public bool mainaxishomesafesignal = false;
        public bool mainaxismovesafesignal = false;
        public bool loadgantrymovesafesignal = false;
        public bool unloadgantrymovesafesignal = false;
        public bool suckaxissafesignal = false;
        public bool LoadGantrySuckSafeSignal = false;
        public bool UnloadGantryPlaceSafeSignal = false;
        public bool LoadFullTrayDrawerOpened = false;//上料满抽屉是否被打开过
        private bool LastLoadFullTrayDrawerInPosStatus = false;
        public bool UnloadNullTrayDrawerOpened = false;//下料空抽屉是否被打开过
        private bool LastUnloadNullTrayDrawerInPosStatus = false;

        #region 主轴运动120°工序
        public void MainAxisMoveThread()
        {
            try
            {
                while (AutoRunActive)
                {
                    if (AutoRunEnableMainAxis)
                    {
                        if (MainAxisMoveFinish && ((AutoRunPartAFinished || logicIgnore[0]) && (AutoRunPartBFinished || logicIgnore[1]) && (AutoRunPartCFinished || logicIgnore[2])))
                        {
                            WriteLog("【主轴】：主轴转动开始");
                            if (MainAxisMove(MainAxisMoveSeg[PartATrayNo], false) == true)
                            {
                                WriteLog("【主轴】：主轴转动完成");
                                AutoRunMainAxisCircleCount++;

                                #region 计算CT代码
                                if (AutoRunMainAxisCircleCount == 3)
                                {
                                    AutoRunStartTime = DateTime.Now;
                                    AutoRunLastTime = DateTime.Now;
                                }
                                if (AutoRunMainAxisCircleCount >= 4)
                                {
                                    myCTInfo.averagect = CalcAverageCT(AutoRunStartTime, AutoRunMainAxisCircleCount);
                                    if (CTIgnorePause == false)//0907  add by ben 
                                    {
                                        TimeSpan tempSpan = DateTime.Now - AutoRunLastTime;
                                        myCTInfo.lastct = (tempSpan.Minutes * 60 + tempSpan.Seconds + tempSpan.Milliseconds / 1000.0) / systemParam.WorkPieceNum;
                                        UpdateCTInfo(myCTInfo);
                                    }
                                    AutoRunLastTime = DateTime.Now;
                                    CTIgnorePause = false;
                                    //WriteCTLog("【CT】 Average CT=" + myCTInfo.averagect.ToString("0.###") + "s   Last CT=" + myCTInfo.lastct.ToString("0.###") +
                                    //           "s   Last CCD=" + myCTInfo.lastCCDScanTime.ToString("0.###") + "s   Last Laser=" + myCTInfo.lastLaserScanTime.ToString("0.###") + "s");
                                }
                                #endregion

                                #region 更新班次的数量
                                WorkCount.WorkPieceCount = WorkCount.WorkPieceCount + 4;
                                WorkCount.WriteDownWorkCountInfo();
                                UpdateWorkPieceInfo(new WorkPieceInfo(WorkCount.isNight, WorkCount.WorkPieceCount));//更新班次产量数目
                                #endregion
                                AutoRunPartABCInit();
                                UpdateAllTrayNo();
                                CCDListInit();
                                //UpdateDataFlowPartA(PartATrayNo);
                                MainAxisThreadAlm = false;
                                MainAxisMoveFinish = true;
                                
                            }
                            else
                            {
                                WarningSolution("【主轴】【报警】：【16】主轴转动异常停止");
                                MainAxisThreadAlm = true;
                                MainAxisMoveFinish = false;
                                AutoRunEnableMainAxis = false;
                            }
                        }
                    }
                    else
                    {
                        MainAxisMoveFinish = true;
                    }
                    Thread.Sleep(40);
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(ThreadAbortException))
                {
                    string exStr = "";
                    exStr += "【主轴】" + ex.ToString() + "\n";
                    exStr += "AutoRunMainAxisCircleCount=" + AutoRunMainAxisCircleCount.ToString() + "\n";
                    MessageBox.Show(exStr);
                }
            }
        }

        private double CalcAverageCT(DateTime startTime, int CircleCount)
        {
            double tempCT = 0;
            DateTime now = DateTime.Now;
            tempCT = (now - startTime).Days * 86400 + (now - startTime).Hours * 3600 + (now - startTime).Minutes * 60 + (now - startTime).Seconds + (now - startTime).Milliseconds / 1000.0;
            return tempCT / (CircleCount - 3) / systemParam.WorkPieceNum;
        }

        private void AutoRunPartABCInit()
        {
            AutoRunPartAFinished = false;
            AutoRunPartBFinished = false;
            AutoRunPartCFinished = false;

            AutoRunStepA = 0;
            AutoRunStepB = 0;
            AutoRunStepC = 0;
        }

        private bool UpdateCCDInfomation(int CircleCount, int ATrayNo)
        {
            if (CircleCount <= 3)
                return true;

            DateTime StartTime = DateTime.Now;

            if (!logicIgnore[1])//屏蔽CCD
            {
                StartTime = DateTime.Now;
                while (true)
                {
                    if (CurStatus != (int)STATUS.AUTO_STATUS)
                        return true;

                    if (CameraCheckDoneArrayTray[ATrayNo, 0] && CameraCheckDoneArrayTray[ATrayNo, 1]
                        && CameraCheckDoneArrayTray[ATrayNo, 2] && CameraCheckDoneArrayTray[ATrayNo, 3])
                    {
                        int[] temparray = new int[systemParam.WorkPieceNum];
                        for (int i = 0; i < systemParam.WorkPieceNum; i++)
                            temparray[i] = CameraCheckResultArrayTray[ATrayNo, i];

                        FinalResultUpdateStru temp = new FinalResultUpdateStru(CircleCount - 2, ATrayNo, temparray);
                        UpdateCCDFinalResult(temp);
                        break;
                    }
                    else
                    {
                        if (!OutTimeCount(StartTime, 3))
                        {
                            WarningSolution("【入料工位】【警报】：更新CCD数据超时");
                            return false;
                        }
                        Thread.Sleep(30);
                    }
                }
            }
            return true;
        }

        private void CCDListInit()
        {
            while (isWaitingCCD || isCheckingCCD || TcpIpRecvStatus)
                Thread.Sleep(10);

            RecvCount = 0;
            RecvClassify = 0;
            CCDRawDataList.Clear();
            CCDRecvStatusList.Clear();
        }
        #endregion

        #region 1#工位AutoRun工序

        public bool CTIgnorePause = false;//add by ben 0907 忽略异常停止时那次CT;

        public int AutoRunStepA = 0;//added by lei.c, notice this is different from AutoRunStep
        public int AutoRunPartACircleCount = 0;//added by lei.c
        public bool AutoRunEnablePartA = false;//added by lei.c 表明是否开启工位1动作
        public bool AutoRunPartAFinished = false;//表明工位1动作是否完成
        public bool AutoRunPartAStretchFinish = false;//表明上下料工位载具是否伸出
        public bool[] AutoRunPartAStationEnable = new bool[3];//表明各个工站是否执行入料动作，0-A站，1-B站，2-C站

        public void AutoRunPartAThread()
        {
            AutoRunPartACircleCount = 0;
            while (AutoRunActive)
            {
                if (AutoRunEnablePartA)
                {
                    if ((!AutoRunPartAFinished) && (MainAxisMoveFinish || debugThreadA))
                    {
                        AutoRunPartAStatusSwitch(ref AutoRunStepA);
                    }
                }
                Thread.Sleep(40);
            }
        }

        private void AutoRunPartAStatusSwitch(ref int StepA)
        {
            try
            {
                switch (StepA)
                {
                    case (int)AutoRunPartAStep.StretchCylinder:
                        WriteLog("【入料工位】case 0：入料工位流程开始");
                        AutoRunPartAFinished = false;
                        if (AutoRunPartAStationEnable[PartATrayNo] == true)
                        {
                            WriteLog("【入料工位】case 0：入料工位伸出载具开始");
                            if (StretchOutAllCylinder())
                            {
                                AutoRunPartAStretchFinish = true;
                                if (bWaitPut)
                                    StepA = (int)AutoRunPartAStep.WaitSuckAndPutTool;
                                else
                                    StepA = (int)AutoRunPartAStep.RetractCylinder;
                            }
                            else
                            {
                                WriteLog("【入料工位】case 0：入料工位伸出载具完成");
                                AutoRunPartAErrorSolution(0);
                                return;
                            }
                        }
                        else
                            StepA = (int)AutoRunPartAStep.WaitSuckAndPutTool;

                        //更新数据
                        if (!debugThreadA)
                        {
                            WriteLog("【入料工位】case 0：入料工位流程等待数据更新开始");
                            if (UpdateCCDInfomation(AutoRunMainAxisCircleCount, PartATrayNo) == false)
                            {
                                AutoRunPartAErrorSolution(100);
                                return;
                            }

                            if (UpdateLaserInfomation(AutoRunMainAxisCircleCount, PartATrayNo) == false)
                            {
                                AutoRunPartAErrorSolution(100);
                                return;
                            }

                            if (AutoRunMainAxisCircleCount > 3)
                            {
                                int[] tempArray = UpdateFinalInfomation(AutoRunMainAxisCircleCount, PartATrayNo);
                                for (int i = 0; i < systemParam.WorkPieceNum; i++)
                                    FinalCheckResultArrayTray[PartATrayNo, i] = tempArray[i];
                            }

                            //更新数据至all data
                            if ((!bDelayStop) && (!bDelayStopCount) && AutoRunMainAxisCircleCount >= 4)
                            {
                                UpdateStationAllDataArray(new PieceAllData[] { pieceAllDataArray[PartATrayNo, 0], pieceAllDataArray[PartATrayNo, 1], pieceAllDataArray[PartATrayNo, 2], pieceAllDataArray[PartATrayNo, 3] });
                                //更新chartForm和ZoomForm
                                UpdateDistributeCount(new PieceAllData[] { pieceAllDataArray[PartATrayNo, 0], pieceAllDataArray[PartATrayNo, 1], pieceAllDataArray[PartATrayNo, 2], pieceAllDataArray[PartATrayNo, 3] });
                            }

                            if ((bDelayStop || bDelayStopCount) && (AutoRunMainAxisCircleCount <= (2 * iDelayStopCount + 3)))
                            {
                                UpdateStationAllDataArray(new PieceAllData[] { pieceAllDataArray[PartATrayNo, 0], pieceAllDataArray[PartATrayNo, 1], pieceAllDataArray[PartATrayNo, 2], pieceAllDataArray[PartATrayNo, 3] });
                                //更新chartForm和ZoomForm
                                UpdateDistributeCount(new PieceAllData[] { pieceAllDataArray[PartATrayNo, 0], pieceAllDataArray[PartATrayNo, 1], pieceAllDataArray[PartATrayNo, 2], pieceAllDataArray[PartATrayNo, 3] });
                            }

                            WriteLog("【入料工位】case 0：入料工位流程等待数据更新结束");
                        }
                        break;
                    case (int)AutoRunPartAStep.WaitSuckAndPutTool://等待横移轴吸取&放置工件
                        AutoRunPartAFinished = false;
                        if (AutoRunPartAStationEnable[PartATrayNo] == true)
                        {
                            if (WaitSuckAxisSignal())
                                StepA = (int)AutoRunPartAStep.RetractCylinder;
                            else
                            {
                                AutoRunPartAErrorSolution(100);
                                return;
                            }
                        }
                        else
                            StepA = (int)AutoRunPartAStep.RetractCylinder;
                        break;
                    case (int)AutoRunPartAStep.RetractCylinder:
                        if (AutoRunPartAStationEnable[PartATrayNo] == true)
                        {
                            AutoRunPartAStretchFinish = false;
                            if (RetractAllCylinder() == false)
                            {
                                AutoRunPartAErrorSolution(1);
                                return;
                            }
                        }
                        //数据更新
                        for (int i = 0; i < systemParam.WorkPieceNum; i++)
                        {
                            CameraCheckResultArrayTray[PartATrayNo, i] = 0;
                            LaserCheckResultArrayTray[PartATrayNo, i] = 0;
                            CameraCheckDoneArrayTray[PartATrayNo, i] = false;
                            LaserCheckDoneArrayTray[PartATrayNo, i] = false;
                            FinalCheckResultArrayTray[PartATrayNo, i] = 0;
                        }

                        //清除入料工位工件数据
                        pieceAllDataArray[PartATrayNo, 0] = new PieceAllData();
                        pieceAllDataArray[PartATrayNo, 1] = new PieceAllData();
                        pieceAllDataArray[PartATrayNo, 2] = new PieceAllData();
                        pieceAllDataArray[PartATrayNo, 3] = new PieceAllData();

                        StepA = (int)AutoRunPartAStep.StretchCylinder;
                        AutoRunPartAFinished = true;
                        AutoRunPartACircleCount++;

                        if ((bDelayStop || bDelayStopCount) && (AutoRunPartACircleCount >= (2 * iDelayStopCount + 3)))
                            AutoRunEnablePartA = false;

                        break;
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(ThreadAbortException))
                {
                    string exStr = "";
                    exStr += "【入料工位】" + ex.ToString() + "\n";
                    exStr += "AutoRunStepA=" + StepA.ToString() + "\n";
                    MessageBox.Show(exStr);
                }

            }
        }

        //added by lei.c 等待工件到位
        private bool WaitSuckAxisSignal()
        {
            WriteLog("【入料工位】case 1：入料工位等待横移轴信号开始");
            DateTime StartTime = DateTime.Now;
            //if (AutoRunMainAxisCircleCount % 2 == 1)
            if (AutoRunPartACircleCount % 2 == 0)
            {
                while (true)
                {
                    if ((SuckAxisPlace1Finish) || debugWorkpieceInPos == true)
                    {
                        break;
                    }
                    else
                    {
                        if (!OutTimeCount(StartTime, 50))
                        {
                            WarningSolution("【入料工位】【警报】：【17】横移轴放置工件超时异常");
                            return false;
                        }
                        Thread.Sleep(30);
                    }
                }
            }
            else
            {
                while (true)
                {
                    if ((SuckAxisPlace2Finish) || debugWorkpieceInPos == true)
                    {
                        break;
                    }
                    else
                    {
                        if (!OutTimeCount(StartTime, 70))
                        {
                            WarningSolution("【入料工位】【警报】：【18】横移轴放置工件超时异常");
                            return false;
                        }
                        Thread.Sleep(30);
                    }
                }
            }

            debugWorkpieceInPos = false;
            WriteLog("【入料工位】case 1：入料工位等待横移轴信号结束");
            return true;
        }

        private void UpdateDistributeCount(PieceAllData[] myAllData)
        {
            //FAI22
            double Seg = (ThrParam.thrInfo[0].UpLimit - ThrParam.thrInfo[0].DownLimit) / 20.0;
            double tempDownLimit = ThrParam.thrInfo[0].DownLimit - 10 * Seg;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai22 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[0][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[0][39]++;
                else
                    faiDistributeCountList[0][tempSeq]++;
            }
            //FAI130
            Seg = (ThrParam.thrInfo[1].UpLimit - ThrParam.thrInfo[1].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[1].DownLimit - 10 * Seg;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai130 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[1][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[1][39]++;
                else
                    faiDistributeCountList[1][tempSeq]++;
            }
            //FAI131
            Seg = (ThrParam.thrInfo[2].UpLimit - ThrParam.thrInfo[2].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[2].DownLimit - 10 * Seg;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai131 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[2][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[2][39]++;
                else
                    faiDistributeCountList[2][tempSeq]++;
            }
            //FAI133G1
            Seg = (ThrParam.thrInfo[3].UpLimit - ThrParam.thrInfo[3].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[3].DownLimit - 10 * Seg;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai133G1 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[3][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[3][39]++;
                else
                    faiDistributeCountList[3][tempSeq]++;
            }
            //FAI133G2
            Seg = (ThrParam.thrInfo[4].UpLimit - ThrParam.thrInfo[4].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[4].DownLimit - 10 * Seg;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai133G2 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[4][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[4][39]++;
                else
                    faiDistributeCountList[4][tempSeq]++;
            }
            //FAI133G3
            Seg = (ThrParam.thrInfo[5].UpLimit - ThrParam.thrInfo[5].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[5].DownLimit - 10 * Seg;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai133G3 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[5][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[5][39]++;
                else
                    faiDistributeCountList[5][tempSeq]++;
            }
            //FAI133G4
            Seg = (ThrParam.thrInfo[6].UpLimit - ThrParam.thrInfo[6].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[6].DownLimit - 10 * Seg;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai133G4 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[6][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[6][39]++;
                else
                    faiDistributeCountList[6][tempSeq]++;
            }
            //FAI133G6
            Seg = (ThrParam.thrInfo[7].UpLimit - ThrParam.thrInfo[7].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[7].DownLimit - 10 * Seg;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai133G6 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[7][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[7][39]++;
                else
                    faiDistributeCountList[7][tempSeq]++;
            }
            //FAI161
            Seg = (ThrParam.thrInfo[8].UpLimit - ThrParam.thrInfo[8].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[8].DownLimit - 10 * Seg;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai161 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[8][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[8][39]++;
                else
                    faiDistributeCountList[8][tempSeq]++;
            }
            //FAI162
            Seg = (ThrParam.thrInfo[9].UpLimit - ThrParam.thrInfo[9].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[9].DownLimit - 10 * Seg;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai162 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[9][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[9][39]++;
                else
                    faiDistributeCountList[9][tempSeq]++;
            }
            //FAI163
            Seg = (ThrParam.thrInfo[10].UpLimit - ThrParam.thrInfo[10].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[10].DownLimit - 10 * Seg;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai163 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[10][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[10][39]++;
                else
                    faiDistributeCountList[10][tempSeq]++;
            }
            //FAI165
            Seg = (ThrParam.thrInfo[11].UpLimit - ThrParam.thrInfo[11].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[11].DownLimit - 10 * Seg;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai165 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[11][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[11][39]++;
                else
                    faiDistributeCountList[11][tempSeq]++;
            }
            //FAI171
            Seg = (ThrParam.thrInfo[12].UpLimit - ThrParam.thrInfo[12].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[12].DownLimit - 10 * Seg;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai171 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[12][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[12][39]++;
                else
                    faiDistributeCountList[12][tempSeq]++;
            }
            //FAI135
            Seg = (ThrParam.thrInfo[13].UpLimit - ThrParam.thrInfo[13].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[13].DownLimit;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai135 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[13][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[13][39]++;
                else
                    faiDistributeCountList[13][tempSeq]++;
            }
            //FAI136
            Seg = (ThrParam.thrInfo[14].UpLimit - ThrParam.thrInfo[14].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[14].DownLimit;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai136 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[14][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[14][39]++;
                else
                    faiDistributeCountList[14][tempSeq]++;
            }
            //FAI139
            Seg = (ThrParam.thrInfo[15].UpLimit - ThrParam.thrInfo[15].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[15].DownLimit;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai139 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[15][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[15][39]++;
                else
                    faiDistributeCountList[15][tempSeq]++;
            }
            //FAI140
            Seg = (ThrParam.thrInfo[16].UpLimit - ThrParam.thrInfo[16].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[16].DownLimit;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai140 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[16][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[16][39]++;
                else
                    faiDistributeCountList[16][tempSeq]++;
            }
            //FAI151
            Seg = (ThrParam.thrInfo[17].UpLimit - ThrParam.thrInfo[17].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[17].DownLimit;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai151 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[17][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[17][39]++;
                else
                    faiDistributeCountList[17][tempSeq]++;
            }
            //FAI152
            Seg = (ThrParam.thrInfo[18].UpLimit - ThrParam.thrInfo[18].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[18].DownLimit;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai152 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[18][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[18][39]++;
                else
                    faiDistributeCountList[18][tempSeq]++;
            }
            //FAI155
            Seg = (ThrParam.thrInfo[19].UpLimit - ThrParam.thrInfo[19].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[19].DownLimit;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai155 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[19][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[19][39]++;
                else
                    faiDistributeCountList[19][tempSeq]++;
            }
            //FAI156
            Seg = (ThrParam.thrInfo[20].UpLimit - ThrParam.thrInfo[20].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[20].DownLimit;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai156 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[20][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[20][39]++;
                else
                    faiDistributeCountList[20][tempSeq]++;
            }
            //FAI157
            Seg = (ThrParam.thrInfo[21].UpLimit - ThrParam.thrInfo[21].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[21].DownLimit;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai157 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[21][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[21][39]++;
                else
                    faiDistributeCountList[21][tempSeq]++;
            }
            //FAI158
            Seg = (ThrParam.thrInfo[22].UpLimit - ThrParam.thrInfo[22].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[22].DownLimit;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai158 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[22][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[22][39]++;
                else
                    faiDistributeCountList[22][tempSeq]++;
            }
            //FAI160
            Seg = (ThrParam.thrInfo[23].UpLimit - ThrParam.thrInfo[23].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[23].DownLimit;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai160 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[23][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[23][39]++;
                else
                    faiDistributeCountList[23][tempSeq]++;
            }
            //FAI172
            Seg = (ThrParam.thrInfo[24].UpLimit - ThrParam.thrInfo[24].DownLimit) / 20.0;
            tempDownLimit = ThrParam.thrInfo[24].DownLimit;
            for (int i = 0; i < myAllData.Length; i++)
            {
                int tempSeq = (int)Math.Floor((myAllData[i].fai172 - tempDownLimit) / Seg);
                if (tempSeq < 0)
                    faiDistributeCountList[24][0]++;
                else if (tempSeq >= 40)
                    faiDistributeCountList[24][39]++;
                else
                    faiDistributeCountList[24][tempSeq]++;
            }

        }
        #endregion

        #region 2#工位AutoRun工序
        public int AutoRunStepB = 0;//added by lei.c, notice this is different from AutoRunStep
        public int AutoRunPartBCircleCount = 0;//added by lei.c
        public bool AutoRunEnablePartB = false;
        public bool AutoRunPartBFinished = false;//表明工位2动作是否完成
        public int AutoRunLaserFinishCount = 0;//记录总共经过Laser检测的元件数目

        public List<double[]> CCDRawDataList = new List<double[]>();//CCD Data的集合
        public List<bool[]> CCDRecvStatusList = new List<bool[]>();//CCD数据第i次是否接收完成的集合

        public PieceAllData[,] pieceAllDataArray = new PieceAllData[3, 4];

        //返回值：1->正常；-1:->CCD数据问题；-2->CCD运动轴问题
        public int AutoRunPartBComponentOld(int StepB, int TrayNo, int CircleCount, ref int errcode, ref int ComponentStep)
        {
            isCheckingCCD = true;

            WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测动作开始");
            bool BComponentFinish = false;
            ComponentStep = 0;
            AutoRunPartBFinished = false;
            DateTime StartTime = DateTime.Now;
            double PosError = 0;
            Point2D[] truepos = new Point2D[3];
            double[] poserror = new double[6];

            try
            {
                while (!BComponentFinish)
                {
                    switch (ComponentStep)
                    {
                        case 0:
                            WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测移动至C1开始");
                            CameraUpLightOn();
                            if (adlink.LineMove(new Axis[] { logicConfig.PulseAxis[1], logicConfig.PulseAxis[2] }, new double[] { CCDMotionPos.posInfo[3 * StepB].XPos, CCDMotionPos.posInfo[3 * StepB].YPos }, true, ref errcode, ref PosError))
                            {
                                if (StepB != 0)
                                    Thread.Sleep(DelayAfterLineMove);
                                else
                                    Thread.Sleep(DelayAfterLineMove + 50);
                                //WaitAxisInPosition(new Axis[] { logicConfig.PulseAxis[1], logicConfig.PulseAxis[2] });
                                WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测移动至C1结束，定位精度1：" + PosError.ToString());
                                ComponentStep = 1;
                            }
                            else
                            {
                                isCheckingCCD = false;
                                return -2;
                            }
                            break;
                        case 1:
                            if (CCDRecvStatusList.Count != 0)
                            {
                                WriteLog("【CCD检测】等待上一穴数据接收完成开始");
                                lock (ccdlocker)
                                {
                                    StartTime = DateTime.Now;
                                    while (true)
                                    {
                                        if (CCDRecvStatusList[CCDRecvStatusList.Count - 1][2] == true)
                                            break;
                                        else
                                        {
                                            if (!OutTimeCount(StartTime, 4))
                                            {
                                                WarningSolution("【CCD检测】【报警】：等待上一穴数据接收完成超时");
                                                isCheckingCCD = false;
                                                return -1;
                                            }
                                            Thread.Sleep(10);
                                        }
                                        if (!debugThreadB)
                                        {
                                            if (CurStatus == (int)STATUS.STOP_STATUS)
                                            {
                                                isCheckingCCD = false;
                                                return -1;
                                            }
                                        }
                                    }
                                }
                                WriteLog("【CCD检测】等待上一穴数据接收完成结束");
                            }
                            SendEXW(1);
                            if (WaitEXWBack() == false)
                                return -1;
                            PosError = adlink.GetAxisPosError(logicConfig.PulseAxis[1], CCDMotionPos.posInfo[3 * StepB].XPos);
                            WriteLog("【CCD检测】C1定位精度2:" + PosError.ToString());
                            WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测C1第一拍开始");
                            truepos[0] = adlink.GetCurPoint(new Axis[] { logicConfig.PulseAxis[1], logicConfig.PulseAxis[2] });
                            poserror[0] = truepos[0].X - CCDMotionPos.posInfo[3 * StepB].XPos; poserror[1] = truepos[0].Y - CCDMotionPos.posInfo[3 * StepB].YPos;
                            TcpSendMsg("T1\r\n");
                            ComponentStep = 2;
                            break;
                        case 2:
                            Thread.Sleep(DelayBeforeLightChange); //Thread.Sleep(30);
                            CameraDownLightOn();
                            Thread.Sleep(DelayAfterLightChange);//Thread.Sleep(70);
                            WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测C1第一拍完成&第二拍开始");
                            TcpSendMsg("T1\r\n");
                            if (StepB == 0)
                                Thread.Sleep(DelayAfterSecondT1);
                            ComponentStep = 3;
                            break;
                        case 3:
                            CameraUpLightOn();
                            WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测移动至M点开始");
                            if (adlink.LineMove(new Axis[] { logicConfig.PulseAxis[1], logicConfig.PulseAxis[2] }, new double[] { CCDMotionPos.posInfo[3 * StepB + 1].XPos, CCDMotionPos.posInfo[3 * StepB + 1].YPos }, true, ref errcode, ref PosError))
                            {
                                //WaitAxisInPosition(new Axis[] { logicConfig.PulseAxis[1], logicConfig.PulseAxis[2] });
                                WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测移动至M点结束，M定位精度1:" + PosError.ToString());
                                ComponentStep = 4;
                            }
                            else
                            {
                                isCheckingCCD = false;
                                return -2;
                            }
                            break;
                        case 4:
                            PosError = adlink.GetAxisPosError(logicConfig.PulseAxis[1], CCDMotionPos.posInfo[3 * StepB + 1].XPos);
                            WriteLog("【CCD检测】M定位精度2:" + PosError.ToString());
                            WriteLog("【CCD检测】等待本穴数据0接收完成开始");

                            lock (ccdlocker)
                            {
                                StartTime = DateTime.Now;
                                while (true)
                                {
                                    if (CCDRecvStatusList.Count >= (RecvCount + 1) && CCDRecvStatusList[CCDRecvStatusList.Count - 1][0] == true)
                                        break;
                                    else
                                    {
                                        if (!OutTimeCount(StartTime, 4))
                                        {
                                            WarningSolution("【CCD检测】【报警】：等待本穴数据0接收完成超时");
                                            isCheckingCCD = false;
                                            return -1;
                                        }
                                        Thread.Sleep(10);
                                    }
                                    if (!debugThreadB)
                                    {
                                        if (CurStatus == (int)STATUS.STOP_STATUS)
                                        {
                                            isCheckingCCD = false;
                                            return -1;
                                        }
                                    }
                                }
                            }
                            WriteLog("【CCD检测】等待本穴数据0接收完成结束");

                            SendEXW(2);
                            if (WaitEXWBack() == false)
                                return -1;
                            WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测M第一拍开始");
                            truepos[1] = adlink.GetCurPoint(new Axis[] { logicConfig.PulseAxis[1], logicConfig.PulseAxis[2] });
                            poserror[2] = truepos[1].X - CCDMotionPos.posInfo[3 * StepB + 1].XPos; poserror[3] = truepos[1].Y - CCDMotionPos.posInfo[3 * StepB + 1].YPos;
                            TcpSendMsg("T1\r\n");
                            ComponentStep = 5;
                            break;
                        case 5:
                            Thread.Sleep(DelayBeforeLightChange);//Thread.Sleep(30);
                            IOControl.ECATWriteDO((int)ECATDONAME.Do_CCDDownLightCmd, true);
                            Thread.Sleep(DelayAfterLightChange);//Thread.Sleep(70);
                            WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测M上光源第一拍结束&第二拍开始");
                            TcpSendMsg("T1\r\n");
                            //Thread.Sleep(DelayAfterSecondT1);
                            ComponentStep = 6;
                            break;
                        case 6:
                            WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测移动至C2点开始");
                            CameraUpLightOn();
                            if (adlink.LineMove(new Axis[] { logicConfig.PulseAxis[1], logicConfig.PulseAxis[2] }, new double[] { CCDMotionPos.posInfo[3 * StepB + 2].XPos, CCDMotionPos.posInfo[3 * StepB + 2].YPos }, true, ref errcode, ref PosError))
                            {
                                if (StepB != 0)
                                    Thread.Sleep(DelayAfterLineMove);
                                else
                                    Thread.Sleep(DelayAfterLineMove + 50);
                                //WaitAxisInPosition(new Axis[] { logicConfig.PulseAxis[1], logicConfig.PulseAxis[2] });
                                WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测移动至C2点结束，C2定位精度1:" + PosError.ToString());
                                ComponentStep = 7;
                            }
                            else
                            {
                                isCheckingCCD = false;
                                return -2;
                            }
                            break;
                        case 7:
                            PosError = adlink.GetAxisPosError(logicConfig.PulseAxis[1], CCDMotionPos.posInfo[3 * StepB + 2].XPos);
                            WriteLog("【CCD检测】C2定位精度2: " + PosError.ToString());
                            WriteLog("【CCD检测】等待本穴数据1接收完成开始");

                            lock (ccdlocker)
                            {
                                StartTime = DateTime.Now;
                                while (true)
                                {
                                    if (CCDRecvStatusList.Count >= 1 && CCDRecvStatusList[CCDRecvStatusList.Count - 1][1] == true)
                                        break;
                                    else
                                    {
                                        if (!OutTimeCount(StartTime, 4))
                                        {
                                            WarningSolution("【CCD检测】【报警】：等待本穴数据1接收完成超时");
                                            isCheckingCCD = false;
                                            return -1;
                                        }
                                        Thread.Sleep(10);
                                    }
                                    if (!debugThreadB)
                                    {
                                        if (CurStatus == (int)STATUS.STOP_STATUS)
                                        {
                                            isCheckingCCD = false;
                                            return -1;
                                        }
                                    }
                                }
                            }
                            WriteLog("【CCD检测】等待本穴数据1接收完成结束");
                            SendEXW(3);
                            if (WaitEXWBack() == false)
                                return -1;
                            WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测C2第一拍开始");
                            truepos[2] = adlink.GetCurPoint(new Axis[] { logicConfig.PulseAxis[1], logicConfig.PulseAxis[2] });
                            poserror[4] = truepos[2].X - CCDMotionPos.posInfo[3 * StepB + 2].XPos; poserror[5] = truepos[2].Y - CCDMotionPos.posInfo[3 * StepB + 2].YPos;
                            TcpSendMsg("T1\r\n");
                            ComponentStep = 8;
                            break;
                        case 8:
                            Thread.Sleep(DelayBeforeLightChange);//Thread.Sleep(30);
                            CameraDownLightOn();
                            Thread.Sleep(DelayAfterLightChange);//Thread.Sleep(70);
                            WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测C2第一拍结束&第二拍开始");
                            TcpSendMsg("T1\r\n");
                            if (StepB == 0)
                                Thread.Sleep(DelayAfterSecondT1);
                            ComponentStep = 9;

                            //WriteCCDMoveLog("1 " + poserror[0].ToString() + " " + poserror[1].ToString() + " " + poserror[2].ToString() + " " + poserror[3].ToString() + " " + poserror[4].ToString() + " " + poserror[5].ToString());
                            CCDTaskRun myCCDTaskRun = new CCDTaskRun(CCDRecvStatusList.Count, StepB, TrayNo, debugThreadB, truepos);
                            Thread CCDDataShowThread = new Thread(new ParameterizedThreadStart(WaitCCDResult));
                            CCDDataShowThread.IsBackground = true;
                            CCDDataShowThread.Start(myCCDTaskRun);

                            IOControl.ECATWriteDO((int)ECATDONAME.Do_CCDDownLightCmd, false);

                            BComponentFinish = true;
                            WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测动作完成");
                            break;
                    }
                    Thread.Sleep(10);
                }
                isCheckingCCD = false;
                return 1;
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(ThreadAbortException))
                {
                    string exStr = null;
                    exStr += "【CCD检测】" + "Exception:" + ex.ToString() + "\n";
                    exStr += "ComponentStep=" + ComponentStep.ToString() + "\n";
                    exStr += "CCDRecvStatusList";
                    for (int i = 0; i < CCDRecvStatusList.Count; i++)
                    {
                        for (int j = 0; j < CCDRecvStatusList[i].Length; j++)
                        {
                            exStr += CCDRecvStatusList[i][j].ToString() + ",";
                        }
                        exStr += ";";
                    }
                    exStr += "\n";
                    exStr += "RecvClassify=" + RecvClassify.ToString() + "\n";

                    MessageBox.Show(exStr);
                    isCheckingCCD = false;
                }
                return -1;
            }
        }

        //返回值：1->正常；-1:->CCD数据问题；-2->CCD运动轴问题
        public int AutoRunPartBComponent(int StepB, int TrayNo, int CircleCount, ref int errcode, ref int ComponentStep)
        {
            isCheckingCCD = true;

            WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测动作开始");
            bool BComponentFinish = false;
            ComponentStep = 0;
            AutoRunPartBFinished = false;
            DateTime StartTime = DateTime.Now;
            double PosError = 0;
            Point2D[] truepos = new Point2D[3];
            double[] poserror = new double[6];

            try
            {
                while (!BComponentFinish)
                {
                    switch (ComponentStep)
                    {
                        case 0:
                            WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测移动至C1开始");
                            CameraUpLightOn();
                            if (adlink.LineMove(new Axis[] { logicConfig.PulseAxis[1], logicConfig.PulseAxis[2] }, new double[] { CCDMotionPos.posInfo[3 * StepB].XPos, CCDMotionPos.posInfo[3 * StepB].YPos }, true, ref errcode, ref PosError))
                            {
                                if (StepB != 0)
                                    Thread.Sleep(DelayAfterLineMove);
                                else
                                    Thread.Sleep(DelayAfterLineMove + 50);
                                ComponentStep = 1;
                            }
                            else
                            {
                                isCheckingCCD = false;
                                return -2;
                            }
                            break;
                        case 1:
                            if (CCDRecvStatusList.Count != 0)
                            {

                                StartTime = DateTime.Now;
                                while (true)
                                {
                                    if (CCDRecvStatusList[CCDRecvStatusList.Count - 1][2] == true)
                                        break;
                                    else
                                    {
                                        if (!OutTimeCount(StartTime, 4))
                                        {
                                            WarningSolution("【CCD检测】【报警】：等待上一穴数据接收完成超时");
                                            isCheckingCCD = false;
                                            return -1;
                                        }
                                        Thread.Sleep(10);
                                    }
                                    if (!debugThreadB)
                                    {
                                        if (CurStatus == (int)STATUS.STOP_STATUS)
                                        {
                                            isCheckingCCD = false;
                                            return -1;
                                        }
                                    }
                                }
                            }
                            SendEXW(1);
                            if (WaitEXWBack() == false)
                                return -1;
                            PosError = adlink.GetAxisPosError(logicConfig.PulseAxis[1], CCDMotionPos.posInfo[3 * StepB].XPos);
                            truepos[0] = adlink.GetCurPoint(new Axis[] { logicConfig.PulseAxis[1], logicConfig.PulseAxis[2] });
                            poserror[0] = truepos[0].X - CCDMotionPos.posInfo[3 * StepB].XPos; poserror[1] = truepos[0].Y - CCDMotionPos.posInfo[3 * StepB].YPos;
                            TcpSendMsg("T1\r\n");
                            ComponentStep = 2;
                            break;
                        case 2:
                            Thread.Sleep(DelayBeforeLightChange); //Thread.Sleep(30);
                            CameraDownLightOn();
                            Thread.Sleep(DelayAfterLightChange);//Thread.Sleep(70);
                            TcpSendMsg("T1\r\n");
                            if (StepB == 0)
                                Thread.Sleep(DelayAfterSecondT1);
                            ComponentStep = 3;
                            break;
                        case 3:
                            CameraUpLightOn();
                            WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测移动至M点开始");
                            if (adlink.LineMove(new Axis[] { logicConfig.PulseAxis[1], logicConfig.PulseAxis[2] }, new double[] { CCDMotionPos.posInfo[3 * StepB + 1].XPos, CCDMotionPos.posInfo[3 * StepB + 1].YPos }, true, ref errcode, ref PosError))
                            {
                                ComponentStep = 4;
                            }
                            else
                            {
                                isCheckingCCD = false;
                                return -2;
                            }
                            break;
                        case 4:
                            PosError = adlink.GetAxisPosError(logicConfig.PulseAxis[1], CCDMotionPos.posInfo[3 * StepB + 1].XPos);
                            WriteLog("【CCD检测】M定位精度2:" + PosError.ToString());
                            WriteLog("【CCD检测】等待本穴数据0接收完成开始");


                            StartTime = DateTime.Now;
                            while (true)
                            {
                                if (CCDRecvStatusList.Count >= (RecvCount + 1) && CCDRecvStatusList[CCDRecvStatusList.Count - 1][0] == true)
                                    break;
                                else
                                {
                                    if (!OutTimeCount(StartTime, 4))
                                    {
                                        WarningSolution("【CCD检测】【报警】：等待本穴数据0接收完成超时");
                                        isCheckingCCD = false;
                                        return -1;
                                    }
                                    Thread.Sleep(10);
                                }
                                if (!debugThreadB)
                                {
                                    if (CurStatus == (int)STATUS.STOP_STATUS)
                                    {
                                        isCheckingCCD = false;
                                        return -1;
                                    }
                                }
                            }
                            WriteLog("【CCD检测】等待本穴数据0接收完成结束");

                            SendEXW(2);
                            if (WaitEXWBack() == false)
                                return -1;
                            WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测M第一拍开始");
                            truepos[1] = adlink.GetCurPoint(new Axis[] { logicConfig.PulseAxis[1], logicConfig.PulseAxis[2] });
                            poserror[2] = truepos[1].X - CCDMotionPos.posInfo[3 * StepB + 1].XPos; poserror[3] = truepos[1].Y - CCDMotionPos.posInfo[3 * StepB + 1].YPos;
                            TcpSendMsg("T1\r\n");
                            ComponentStep = 5;
                            break;
                        case 5:
                            Thread.Sleep(DelayBeforeLightChange);//Thread.Sleep(30);
                            IOControl.ECATWriteDO((int)ECATDONAME.Do_CCDDownLightCmd, true);
                            Thread.Sleep(DelayAfterLightChange);//Thread.Sleep(70);
                            WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测M上光源第一拍结束&第二拍开始");
                            TcpSendMsg("T1\r\n");
                            ComponentStep = 6;
                            break;
                        case 6:
                            WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测移动至C2点开始");
                            CameraUpLightOn();
                            if (adlink.LineMove(new Axis[] { logicConfig.PulseAxis[1], logicConfig.PulseAxis[2] }, new double[] { CCDMotionPos.posInfo[3 * StepB + 2].XPos, CCDMotionPos.posInfo[3 * StepB + 2].YPos }, true, ref errcode, ref PosError))
                            {
                                if (StepB != 0)
                                    Thread.Sleep(DelayAfterLineMove);
                                else
                                    Thread.Sleep(DelayAfterLineMove + 50);
                                WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测移动至C2点结束，C2定位精度1:" + PosError.ToString());
                                ComponentStep = 7;
                            }
                            else
                            {
                                isCheckingCCD = false;
                                return -2;
                            }
                            break;
                        case 7:
                            PosError = adlink.GetAxisPosError(logicConfig.PulseAxis[1], CCDMotionPos.posInfo[3 * StepB + 2].XPos);

                            StartTime = DateTime.Now;
                            while (true)
                            {
                                if (CCDRecvStatusList[CCDRecvStatusList.Count - 1][1] == true)
                                    break;
                                else
                                {
                                    if (!OutTimeCount(StartTime, 4))
                                    {
                                        WarningSolution("【CCD检测】【报警】：等待本穴数据1接收完成超时");
                                        isCheckingCCD = false;
                                        return -1;
                                    }
                                    Thread.Sleep(10);
                                }
                                if (!debugThreadB)
                                {
                                    if (CurStatus == (int)STATUS.STOP_STATUS)
                                    {
                                        isCheckingCCD = false;
                                        return -1;
                                    }
                                }
                            }

                            WriteLog("【CCD检测】等待本穴数据1接收完成结束");
                            SendEXW(3);
                            if (WaitEXWBack() == false)
                                return -1;
                            WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测C2第一拍开始");
                            truepos[2] = adlink.GetCurPoint(new Axis[] { logicConfig.PulseAxis[1], logicConfig.PulseAxis[2] });
                            poserror[4] = truepos[2].X - CCDMotionPos.posInfo[3 * StepB + 2].XPos; poserror[5] = truepos[2].Y - CCDMotionPos.posInfo[3 * StepB + 2].YPos;
                            TcpSendMsg("T1\r\n");
                            ComponentStep = 8;
                            break;
                        case 8:
                            Thread.Sleep(DelayBeforeLightChange);//Thread.Sleep(30);
                            CameraDownLightOn();
                            Thread.Sleep(DelayAfterLightChange);//Thread.Sleep(70);
                            WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测C2第一拍结束&第二拍开始");
                            TcpSendMsg("T1\r\n");
                            if (StepB == 0)
                                Thread.Sleep(DelayAfterSecondT1);
                            ComponentStep = 9;

                            CCDTaskRun myCCDTaskRun = new CCDTaskRun(CCDRecvStatusList.Count, StepB, TrayNo, debugThreadB, truepos);
                            Thread CCDDataShowThread = new Thread(new ParameterizedThreadStart(WaitCCDResult));
                            CCDDataShowThread.IsBackground = true;
                            CCDDataShowThread.Start(myCCDTaskRun);

                            IOControl.ECATWriteDO((int)ECATDONAME.Do_CCDDownLightCmd, false);

                            BComponentFinish = true;
                            WriteLog("【CCD检测】" + StepB.ToString() + "穴CCD检测动作完成");
                            break;
                    }
                    Thread.Sleep(10);
                }
                isCheckingCCD = false;
                return 1;
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(ThreadAbortException))
                {
                    string exStr = null;
                    exStr += "【CCD检测】" + "Exception:" + ex.ToString() + "\n";
                    exStr += "ComponentStep=" + ComponentStep.ToString() + "\n";
                    exStr += "CCDRecvStatusList";
                    for (int i = 0; i < CCDRecvStatusList.Count; i++)
                    {
                        for (int j = 0; j < CCDRecvStatusList[i].Length; j++)
                        {
                            exStr += CCDRecvStatusList[i][j].ToString() + ",";
                        }
                        exStr += ";";
                    }
                    exStr += "\n";
                    exStr += "RecvClassify=" + RecvClassify.ToString() + "\n";

                    MessageBox.Show(exStr);
                    isCheckingCCD = false;
                }
                return -1;
            }
        }


        bool isEXWBack = false;
        private bool WaitEXWBack()
        {
            DateTime starttime = DateTime.Now;
            while (!isEXWBack)
            {
                if (!OutTimeCount(starttime, 3))
                {
                    WarningSolution("等待EXW返回信号超时");
                    return false;
                }
                Thread.Sleep(10);
            }
            WriteLog("【CCD检测】EXW Back");
            return true;
        }

        private void SendEXW(int seq)
        {
            WriteLog("【CCD检测】发送EXW" + seq.ToString());
            isEXWBack = false;
            switch (seq)
            {
                case 1:
                    TcpSendMsg("EXW,1\r\n");
                    break;
                case 2:
                    TcpSendMsg("EXW,2\r\n");
                    break;
                case 3:
                    TcpSendMsg("EXW,3\r\n");
                    break;
            }
        }

        //只能适用于脉冲轴
        private void WaitAxisInPosition(Axis[] axis)
        {
            DateTime starttime = DateTime.Now;
            bool tempsignal = false;

            while (true)
            {
                for (int i = 0; i < axis.Length; i++)
                {
                    tempsignal = tempsignal && (CurInfo.motionIO[axis[i].AxisId].inp==1);//inp信号还需要看一下停止的时候是0还是1
                }
                if (tempsignal)
                {
                    WriteLog("【轴到位信号抓取成功】");
                    break;
                }
                else
                {
                    if ((DateTime.Now - starttime).Seconds * 1000 + (DateTime.Now - starttime).Milliseconds >= 2000)
                    {
                        WriteLog("【轴到位信号抓取失败】");
                        break;
                    }
                }
                Thread.Sleep(30);
            }
        }

        private void CameraDownLightOn()
        {
            WriteLog("【CCD检测】开启CCD下光源");
            IOControl.ECATWriteDO((int)ECATDONAME.Do_CCDUpLightCmd, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_CCDDownLightCmd, true);
        }

        private void CameraUpLightOn()
        {
            WriteLog("【CCD检测】开启CCD上光源");
            IOControl.ECATWriteDO((int)ECATDONAME.Do_CCDUpLightCmd, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_CCDDownLightCmd, false);
        }

        private void WaitCCDResult(object objCCDTaskRun)//总的检查过的产品的次序，从0开始
        {
            CCDTaskRun myCCDTaskRun = (CCDTaskRun)objCCDTaskRun;
            WriteLog("【CCD检测】" + myCCDTaskRun.StepB.ToString() + "穴CCD数据计算开始");
            DateTime StartTime = DateTime.Now;
            isWaitingCCD = true;
            while (CCDChecking)
            //while (CurStatus == (int)STATUS.AUTO_STATUS)
            {
                if (CCDRecvStatusList.Count >= myCCDTaskRun.ProductSeq  && CCDRawDataList.Count >= myCCDTaskRun.ProductSeq && CCDRecvStatusList[myCCDTaskRun.ProductSeq - 1][2])
                {
                    CCDUpdateStruct CCDUpdateTemp = TransferCCDData(CCDRawDataList[myCCDTaskRun.ProductSeq - 1], myCCDTaskRun);
                    GetPieceAllDataCCDFAI(myCCDTaskRun.PartBTrayNo, myCCDTaskRun.StepB, CCDUpdateTemp);
                    UpdateCCDResult(CCDUpdateTemp);//显示ccd的fai
                    if (!myCCDTaskRun.isDebugMode)
                    {
                        CameraCheckResultArrayTray[myCCDTaskRun.PartBTrayNo, myCCDTaskRun.StepB] = CameraJudge(CCDUpdateTemp);
                        CameraCheckDoneArrayTray[myCCDTaskRun.PartBTrayNo, myCCDTaskRun.StepB] = true;
                    }
                    WriteLog("【CCD检测】" + myCCDTaskRun.StepB.ToString() + "穴CCD数据计算结束");
                    break;
                }
                else
                {
                    if (!OutTimeCount(StartTime, 10))
                    {
                        WarningSolution("【CCD检测】【报警】【19】" + myCCDTaskRun.StepB.ToString() + "穴CCD接收数据发生错误");
                        break;
                    }
                    Thread.Sleep(30);
                }
            }
            isWaitingCCD = false;
        }

        //TrayNo=1,2,3；HoleSeq=0,1,2,3
        private CCDUpdateStruct TransferCCDResultData(CCDUpdateStruct originalResult, int TrayNo, int HoleSeq)
        {
            CCDUpdateStruct temp = originalResult;

            temp.fai22 = Math.Round(DataOffset.ccdGradient[TrayNo * 4 + HoleSeq][0] * originalResult.fai22 + DataOffset.ccdOffset[TrayNo * 4 + HoleSeq][0], 3);
            temp.fai130 = Math.Round(DataOffset.ccdGradient[TrayNo * 4 + HoleSeq][1] * originalResult.fai130 + DataOffset.ccdOffset[TrayNo * 4 + HoleSeq][1], 3);
            temp.fai131 = Math.Round(DataOffset.ccdGradient[TrayNo * 4 + HoleSeq][2] * originalResult.fai131 + DataOffset.ccdOffset[TrayNo * 4 + HoleSeq][2], 3);
            temp.fai133G1 = Math.Round(DataOffset.ccdGradient[TrayNo * 4 + HoleSeq][3] * originalResult.fai133G1 + DataOffset.ccdOffset[TrayNo * 4 + HoleSeq][3], 3);
            temp.fai133G2 = Math.Round(DataOffset.ccdGradient[TrayNo * 4 + HoleSeq][4] * originalResult.fai133G2 + DataOffset.ccdOffset[TrayNo * 4 + HoleSeq][4], 3);
            temp.fai133G3 = Math.Round(DataOffset.ccdGradient[TrayNo * 4 + HoleSeq][5] * originalResult.fai133G3 + DataOffset.ccdOffset[TrayNo * 4 + HoleSeq][5], 3);
            temp.fai133G4 = Math.Round(DataOffset.ccdGradient[TrayNo * 4 + HoleSeq][6] * originalResult.fai133G4 + DataOffset.ccdOffset[TrayNo * 4 + HoleSeq][6], 3);
            temp.fai133G6 = Math.Round(DataOffset.ccdGradient[TrayNo * 4 + HoleSeq][7] * originalResult.fai133G6 + DataOffset.ccdOffset[TrayNo * 4 + HoleSeq][7], 3);
            temp.fai161 = Math.Round(DataOffset.ccdGradient[TrayNo * 4 + HoleSeq][8] * originalResult.fai161 + DataOffset.ccdOffset[TrayNo * 4 + HoleSeq][8], 3);
            temp.fai162 = Math.Round(DataOffset.ccdGradient[TrayNo * 4 + HoleSeq][9] * originalResult.fai162 + DataOffset.ccdOffset[TrayNo * 4 + HoleSeq][9], 3);
            temp.fai163 = Math.Round(DataOffset.ccdGradient[TrayNo * 4 + HoleSeq][10] * originalResult.fai163 + DataOffset.ccdOffset[TrayNo * 4 + HoleSeq][10], 3);
            temp.fai165 = Math.Round(DataOffset.ccdGradient[TrayNo * 4 + HoleSeq][11] * originalResult.fai165 + DataOffset.ccdOffset[TrayNo * 4 + HoleSeq][11], 3);
            temp.fai171 = Math.Round(DataOffset.ccdGradient[TrayNo * 4 + HoleSeq][12] * originalResult.fai171 + DataOffset.ccdOffset[TrayNo * 4 + HoleSeq][12], 3);

            return temp;
        }


        private LaserFAIUpdateStruct TransferLaserResultData(LaserFAIUpdateStruct originalResult, string TrayNo, int HoleSeq)
        {
            int iTrayNo = -1;
            switch (TrayNo)
            {
                case "A":
                    iTrayNo = 0;
                    break;
                case "B":
                    iTrayNo = 1;
                    break;
                case "C":
                    iTrayNo = 2;
                    break;
            }

            LaserFAIUpdateStruct temp = originalResult;
            temp.fai135 = Math.Round(Math.Abs(DataOffset.laserGradient[iTrayNo * 4 + HoleSeq][0] * originalResult.fai135 + DataOffset.laserOffset[iTrayNo * 4 + HoleSeq][0]), 3);
            temp.fai136 = Math.Round(Math.Abs(DataOffset.laserGradient[iTrayNo * 4 + HoleSeq][1] * originalResult.fai136 + DataOffset.laserOffset[iTrayNo * 4 + HoleSeq][1]), 3);
            temp.fai139 = Math.Round(Math.Abs(DataOffset.laserGradient[iTrayNo * 4 + HoleSeq][2] * originalResult.fai139 + DataOffset.laserOffset[iTrayNo * 4 + HoleSeq][2]), 3);
            temp.fai140 = Math.Round(Math.Abs(DataOffset.laserGradient[iTrayNo * 4 + HoleSeq][3] * originalResult.fai140 + DataOffset.laserOffset[iTrayNo * 4 + HoleSeq][3]), 3);
            temp.fai151 = Math.Round(Math.Abs(DataOffset.laserGradient[iTrayNo * 4 + HoleSeq][4] * originalResult.fai151 + DataOffset.laserOffset[iTrayNo * 4 + HoleSeq][4]), 3);
            temp.fai152 = Math.Round(Math.Abs(DataOffset.laserGradient[iTrayNo * 4 + HoleSeq][5] * originalResult.fai152 + DataOffset.laserOffset[iTrayNo * 4 + HoleSeq][5]), 3);
            temp.fai155 = Math.Round(Math.Abs(DataOffset.laserGradient[iTrayNo * 4 + HoleSeq][6] * originalResult.fai155 + DataOffset.laserOffset[iTrayNo * 4 + HoleSeq][6]), 3);
            temp.fai156 = Math.Round(Math.Abs(DataOffset.laserGradient[iTrayNo * 4 + HoleSeq][7] * originalResult.fai156 + DataOffset.laserOffset[iTrayNo * 4 + HoleSeq][7]), 3);
            temp.fai157 = Math.Round(Math.Abs(DataOffset.laserGradient[iTrayNo * 4 + HoleSeq][8] * originalResult.fai157 + DataOffset.laserOffset[iTrayNo * 4 + HoleSeq][8]), 3);
            temp.fai158 = Math.Round(Math.Abs(DataOffset.laserGradient[iTrayNo * 4 + HoleSeq][9] * originalResult.fai158 + DataOffset.laserOffset[iTrayNo * 4 + HoleSeq][9]), 3);
            temp.fai160 = Math.Round(Math.Abs(DataOffset.laserGradient[iTrayNo * 4 + HoleSeq][10] * originalResult.fai160 + DataOffset.laserOffset[iTrayNo * 4 + HoleSeq][10]), 3);
            temp.fai172 = Math.Round(Math.Abs(DataOffset.laserGradient[iTrayNo * 4 + HoleSeq][11] * originalResult.fai172 + DataOffset.laserOffset[iTrayNo * 4 + HoleSeq][11]), 3);

            if (LaserRandomResult == true)
            {
                Random rd = new Random();

                if (temp.fai135 < 0)
                    temp.fai135 = rd.Next(0, 5) / 1000.0;
                if (temp.fai136 < 0)
                    temp.fai136 = rd.Next(0, 5) / 1000.0;
                if (temp.fai139 < 0)
                    temp.fai139 = rd.Next(0, 5) / 1000.0;
                if (temp.fai140 < 0)
                    temp.fai140 = rd.Next(0, 5) / 1000.0;
                if (temp.fai151 < 0)
                    temp.fai151 = rd.Next(0, 5) / 1000.0;
                if (temp.fai152 < 0)
                    temp.fai152 = rd.Next(0, 5) / 1000.0;
                if (temp.fai155 < 0)
                    temp.fai155 = rd.Next(0, 5) / 1000.0;
                if (temp.fai156 < 0)
                    temp.fai156 = rd.Next(0, 5) / 1000.0;
                if (temp.fai157 < 0)
                    temp.fai157 = rd.Next(0, 5) / 1000.0;
                if (temp.fai158 < 0)
                    temp.fai158 = rd.Next(0, 5) / 1000.0;
                if (temp.fai160 < 0)
                    temp.fai160 = rd.Next(0, 5) / 1000.0;
                if (temp.fai172 < 0)
                    temp.fai172 = rd.Next(0, 5) / 1000.0;
            }

            return temp;
        }

        //CCD检测
        private int PartBComponentStep = 0;
        private int CCDRedoCount = 0;
        private void AutoRunPartBStatusSwitch(ref int StepB, ref int errcode)
        {
            int checkresult = 0;
            try
            {
                if ((StationCheckSetting[PartBTrayNo].CCDCheck && AutoRunMainAxisCircleCount >= 2) || debugThreadB)
                {
                    switch (StepB)
                    {
                        case (int)AutoRunPartBStep.WaitCCDHome:
                            WriteLog("【CCD检测】CCD检测动作开始");
                            if (AutoRunPartBCircleCount != 0)
                            {
                                WriteLog("【CCD检测】等待CCD归位开始");
                                DateTime StartTime = DateTime.Now;
                                while (true)
                                {
                                    if (AutoRunPartBHomeDone)
                                        break;
                                    else
                                    {
                                        if (!OutTimeCount(StartTime, 5))
                                        {
                                            AutoRunEnablePartB = false;
                                            CCDCheckThreadAlm = true;
                                            CCDChecking = false;
                                            if (CurStatus != (int)STATUS.STOP_STATUS)
                                            {
                                                CurStatus = (int)STATUS.PAUSE_STATUS;
                                            }
                                            WarningSolution("【CCD检测】【报警】：等待CCD归位超时");
                                            return;
                                        }
                                        Thread.Sleep(30);
                                    }
                                }
                                AutoRunPartBHomeDone = false;
                                WriteLog("【CCD检测】等待CCD归位完成");
                            }
                            StepB = (int)AutoRunPartBStep.CameraCheck1;
                            break;
                        case (int)AutoRunPartBStep.CameraCheck1:
                            AutoRunStepBStartTime = DateTime.Now;
                            if (StationCheckSetting[PartBTrayNo].CCDHoleCheck[0] == true)
                            {
                                checkresult = AutoRunPartBComponent(StepB, PartBTrayNo, AutoRunPartBCircleCount, ref errcode, ref PartBComponentStep);
                                if (checkresult == 1)
                                    StepB = (int)AutoRunPartBStep.CameraCheck2;
                                else
                                {
                                    CCDCheckErrorSolution(PartBTrayNo, PartBComponentStep, checkresult, ref StepB);
                                    return;
                                }
                            }
                            else
                            {
                                CameraCheckDoneArrayTray[PartBTrayNo, 0] = true;
                                StepB = (int)AutoRunPartBStep.CameraCheck2;
                            }
                            break;
                        case (int)AutoRunPartBStep.CameraCheck2:
                            if (StationCheckSetting[PartBTrayNo].CCDHoleCheck[1] == true)
                            {
                                checkresult = AutoRunPartBComponent(StepB, PartBTrayNo, AutoRunPartBCircleCount, ref errcode, ref PartBComponentStep);
                                if (checkresult == 1)
                                    StepB = (int)AutoRunPartBStep.CameraCheck3;
                                else
                                {
                                    CCDCheckErrorSolution(PartBTrayNo, PartBComponentStep, checkresult, ref StepB);
                                    return;
                                }
                            }
                            else
                            {
                                CameraCheckDoneArrayTray[PartBTrayNo, 1] = true;
                                StepB = (int)AutoRunPartBStep.CameraCheck3;
                            }
                            break;
                        case (int)AutoRunPartBStep.CameraCheck3:
                            if (StationCheckSetting[PartBTrayNo].CCDHoleCheck[2] == true)
                            {
                                checkresult = AutoRunPartBComponent(StepB, PartBTrayNo, AutoRunPartBCircleCount, ref errcode, ref PartBComponentStep);
                                if (checkresult == 1)
                                    StepB = (int)AutoRunPartBStep.CameraCheck4;
                                else
                                {
                                    CCDCheckErrorSolution(PartBTrayNo, PartBComponentStep, checkresult, ref StepB);
                                    return;
                                }
                            }
                            else
                            {
                                CameraCheckDoneArrayTray[PartBTrayNo, 2] = true;
                                StepB = (int)AutoRunPartBStep.CameraCheck4;
                            }
                            break;
                        case (int)AutoRunPartBStep.CameraCheck4:
                            if (StationCheckSetting[PartBTrayNo].CCDHoleCheck[3] == true)
                            {
                                checkresult = AutoRunPartBComponent(StepB, PartBTrayNo, AutoRunPartBCircleCount, ref errcode, ref PartBComponentStep);
                                if (checkresult == 1)
                                    StepB = (int)AutoRunPartBStep.StartCCDHome;
                                else
                                {
                                    CCDCheckErrorSolution(PartBTrayNo, PartBComponentStep, checkresult, ref StepB);
                                    return;
                                }
                            }
                            else
                            {
                                CameraCheckDoneArrayTray[PartBTrayNo, 3] = true;
                                StepB = (int)AutoRunPartBStep.StartCCDHome;
                            }
                            break;
                        case (int)AutoRunPartBStep.StartCCDHome:
                            //开启CCD工位回零线程
                            Thread AutoRunPartBHomeThread = new Thread(new ThreadStart(AutoRunPartBHome));
                            AutoRunPartBHomeThread.IsBackground = true;
                            AutoRunPartBHomeThread.Start();

                            AutoRunPartBStatusFinish();
                            break;
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                        CameraCheckDoneArrayTray[PartBTrayNo, i] = true;

                    AutoRunPartBHomeDone = true;
                    AutoRunPartBStatusFinish();
                    return;
                }
            }
            catch (Exception ex)
            {
                StartBeep();//樊竞明20181001

                if (ex.GetType() != typeof(ThreadAbortException))
                {
                    MessageBox.Show("【CCD检测】" + "Exception:" + ex.ToString() + "StepB=" + StepB.ToString() + "  BComponentStep=" + PartBComponentStep.ToString());
                }
            }
        }

        private void AutoRunPartBStatusFinish()
        {
            WriteLog("【CCD检测】：进入收尾流程");
            AutoRunStepB = (int)AutoRunPartBStep.WaitCCDHome;
            AutoRunPartBFinished = true;
            AutoRunPartBCircleCount++;
            WriteLog("【CCD检测】：收尾流程结束");
            myCTInfo.lastCCDScanTime = (DateTime.Now - AutoRunStepBStartTime).Seconds + (DateTime.Now - AutoRunStepBStartTime).Milliseconds / 1000.0;
        }

        private void AutoRunPartCStatusFinish()
        {
            WriteLog("【Laser检测】：收尾流程开始");
            AutoRunStepC = (int)AutoRunPartCStep.WaitLaserHome;
            AutoRunPartCFinished = true;
            AutoRunPartCCircleCount++;
            WriteLog("【Laser检测】：收尾流程结束");
            myCTInfo.lastLaserScanTime = (DateTime.Now - AutoRunStepCStartTime).Seconds + (DateTime.Now - AutoRunStepCStartTime).Milliseconds / 1000.0;

        }

        private void CCDCheckErrorSolution(int TrayNo, int ComponentStep, int checkResult, ref int StepB)
        {
            WriteLog("【CCD检测】：进入错误处理程序");
            AutoRunEnablePartB = false;
            CCDCheckThreadAlm = true;
            CCDChecking = false;
            CCDUndo(TrayNo, ComponentStep, ref StepB);

            if (checkResult == -1)
            {
                Thread.Sleep(1000);
                if (PauseButtonClicked == false)
                {
                    TcpSendMsg("RS\r\n");
                    DateTime starttime = DateTime.Now;
                    while ((tcp_Recive != string.Empty) && (tcp_Recive.Substring(0, 2) != "RS"))
                    {
                        if (!OutTimeCount(starttime, 5))
                        {
                            WarningSolution("CCD控制器复位超时");
                            return;
                        }
                        Thread.Sleep(30);
                    }
                }

                #region 开启重做
                if (CCDRedoCount == 0 && (PauseButtonClicked == false))
                {
                    UpdateWaringLog.Invoke((object)"CCD重做1次");
                    CCDRedoCount++;
                    CCDChecking = true;
                    AutoRunEnablePartB = true;
                    CCDCheckThreadAlm = false;
                }
                else if (CCDRedoCount == 1)
                {
                    UpdateWaringLog.Invoke((object)"CCD重做2次");
                    CCDRedoCount = 0;
                    #region 重做依然不成功，就直接跳过
                    if (PauseButtonClicked == false)
                    {
                        AutoRunStepB = (int)AutoRunPartBStep.StartCCDHome;
                        for (int i = 0; i < systemParam.WorkPieceNum; i++)
                        {
                            CameraCheckDoneArrayTray[TrayNo, i] = true;
                            CameraCheckResultArrayTray[TrayNo, i] = -2;
                        }

                        CCDChecking = true;
                        AutoRunEnablePartB = true;
                        CCDCheckThreadAlm = false;
                    }
                    #endregion
                }
                #endregion
            }
            else if (checkResult == -2)
            {
                if (CurStatus != (int)STATUS.STOP_STATUS)
                {
                    CurStatus = (int)STATUS.PAUSE_STATUS;
                }
                WarningSolution("【CCD检测】CCD运动轴发生故障，请检查");
            }

            WriteLog("【CCD检测】：错误处理程序完成，AutoRunStepB=" + AutoRunStepB.ToString());
        }

        private void LoadTrayErrorSolution(int Step)
        {
            int errcode = 0;
            WriteLog("【上料Tray】：进入错误处理程序");
            if ((Step != 5) || ((Step == 5) && (LoadFullTraySuckTryCount >= 2)))
            {
                LoadTrayEnable = false;
                LoadTrayThreadAlm = true;
                if (CurStatus != (int)STATUS.STOP_STATUS)
                {
                    CurStatus = (int)STATUS.PAUSE_STATUS;
                }
            }
            switch (Step)
            {
                case 0:
                    WarningSolution("【上料Tray】【报警】：【21】上料空Tray Z轴下降过程中出错");
                    break;
                case 1:
                    WarningSolution("【上料Tray】【报警】：【22】上料满Tray Z轴上升过程中出错");
                    break;
                case 2:
                    WarningSolution("【上料Tray】【报警】：上料满Tray未补料，无法整体补料");
                    MessageBox.Show("上料满Tray未补料，无法整体补料，请打开上料满Tray抽屉进行补料", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case 3:
                    LoadTrayMovePistonStretch();
                    LoadTrayStep = 1;
                    MessageBox.Show("移动气缸缩回错误，请确认是否有障碍物", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case 4:
                    LoadTrayZRetract();
                    LoadTrayStep = 2;
                    MessageBox.Show("上料Z气缸伸出错误，请确认是否有障碍物", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case 5:
                    if (LoadFullTraySuckTryCount <= 1)
                    {
                        LoadTraySeparateRetract();
                        for (int i = 0; i < 10; i++)
                        {
                            if (CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInPosition + 25])
                            {
                                adlink.P2PMove(logicConfig.ECATAxis[5], 1, false, ref errcode);
                                Thread.Sleep(50);
                            }
                            else
                                break;
                        }
                        LoadTraySeparateStretch();
                        LoadTrayStep = 3;
                    }
                    else
                    {
                        WarningSolution("上料满Tray吸取超时，请检查吸盘位置是否准确");
                        LoadTraySeparateRetract();
                    }
                    break;
                case 6:
                    LoadTrayZRetract();
                    LoadTrayStep = 7;
                    MessageBox.Show("上料Z气缸伸出错误，请确认是否有障碍物", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case 7:
                    LoadTraySeparateRetract();
                    LoadTrayStep = 11;
                    MessageBox.Show("上料盘分盘气缸伸出错误，请确认是否存在卡盘现象", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case 100:
                    break;
                default:
                    break;
            }
        }

        private void LaserErrorSolution(int TrayNo, int checkResult, ref int StepC)
        {
            string errcode;
            WriteLog("【Laser检测】：进入错误处理程序");
            AutoRunEnablePartC = false;
            LaserCheckThreadAlm = true;
            LaserUndo(TrayNo);

            if (checkResult == -1)
            {
                Thread.Sleep(1000);
                if (PauseButtonClicked == false)
                {
                    if (isSet1Param == false)
                    {
                        if (!LaserMiniInit(out errcode))// 0906 add by ben
                            MessageBox.Show(errcode);
                    }
                }
                #region 开启重做
                if (LaserRedoCount == 0 && (PauseButtonClicked == false))
                {
                    UpdateWaringLog.Invoke((object)"Laser重做1次");
                    LaserRedoCount++;
                    AutoRunEnablePartC = true;
                    LaserCheckThreadAlm = false;
                }
                else if (LaserRedoCount == 1)
                {
                    UpdateWaringLog.Invoke((object)"Laser重做2次");
                    LaserRedoCount = 0;
                    #region 重做依然不成功，就直接跳过
                    if (PauseButtonClicked == false)
                    {
                        AutoRunStepC = (int)AutoRunPartCStep.StartLaserHome;
                        for (int i = 0; i < systemParam.WorkPieceNum; i++)
                        {
                            LaserCheckDoneArrayTray[TrayNo, i] = true;
                            LaserCheckResultArrayTray[TrayNo, i] = -2;
                        }
                        AutoRunEnablePartC = true;
                        LaserCheckThreadAlm = false;
                    }
                    #endregion
                }
                #endregion
            }
            else if (checkResult == -2)
            {
                if (CurStatus != (int)STATUS.STOP_STATUS)
                {
                    CurStatus = (int)STATUS.PAUSE_STATUS;
                }
                WarningSolution("【Laser检测】Laser运动轴发生故障，请检查");
            }
            WriteLog("【Laser检测】：退出错误处理程序");
        }

        private void LaserUndo(int TrayNo)
        {
            WriteLog("【Laser检测】：进入LaserUndo处理程序");
            UpdateLaserUndoDataGridView(TrayNo);
            AutoRunStepC = 0;
            for (int i = 0; i < systemParam.WorkPieceNum; i++)
            {
                LaserCheckDoneArrayTray[TrayNo, i] = false;
                CameraCheckResultArrayTray[TrayNo, i] = 0;
            }
            AutoRunPartCHomeDone = true;
            WriteLog("【Laser检测】：退出LaserUndo处理程序");
        }

        private void LoadTrayAllSwitchErrorSolution(int Step)
        {
            WriteLog("【上料Tray All Switch】：进入错误处理程序");
            LoadTrayAllSwitchEnable = false;
            LoadTrayAllSwitchThreadAlm = true;
            if (CurStatus != (int)STATUS.STOP_STATUS)
            {
                CurStatus = (int)STATUS.PAUSE_STATUS;
            }

            switch (Step)
            {
                case 100:
                    break;
                default:
                    break;
            }
        }

        private void UnloadTrayAllSwitchErrorSolution(int Step)
        {
            WriteLog("【下料Tray All Switch】：进入错误处理程序");
            UnloadTrayAllSwitchEnable = false;
            UnloadTrayAllSwitchThreadAlm = true;
            if (CurStatus != (int)STATUS.STOP_STATUS)
            {
                CurStatus = (int)STATUS.PAUSE_STATUS;
            }
            switch (Step)
            {
                case 100:
                    break;
                default:
                    break;
            }
        }

        private void AutoRunPartAErrorSolution(int Step)
        {
            WriteLog("【入料工位】：进入错误处理程序");
            AutoRunEnablePartA = false;
            PartAThreadAlm = true;
            if (CurStatus != (int)STATUS.STOP_STATUS)
            {
                CurStatus = (int)STATUS.PAUSE_STATUS;
            }
            switch (Step)
            {
                case 0:
                    WarningSolution("【23】入料工位治具伸出出错");
                    break;
                case 1:
                    WarningSolution("【24】入料工位治具缩回出错");
                    break;
                case 100:
                    break;
                default:
                    break;
            }
        }

        private void SuckAxisErrorSolution(int Step)
        {
            WriteLog("【横移轴工位】：进入错误处理程序");
            SuckAxisMoveEnable = false;
            SuckAxisThreadAlm = true;
            if (CurStatus != (int)STATUS.STOP_STATUS)
            {
                CurStatus = (int)STATUS.PAUSE_STATUS;
            }

            switch (Step)
            {
                case 0:
                    WarningSolution("【横移轴】【报警】：【25】横移轴移动至右吸取位1出错");
                    break;
                case 1:
                    WarningSolution("【横移轴】【报警】：【26】横移轴移动至左放料位出错");
                    break;
                case 2:
                    WarningSolution("【横移轴】【报警】：【27】横移轴移动至右吸取位2出错");
                    break;
                case 3:
                    if (MessageBox.Show("横移轴未能够全吸取工件，是否直接执行下一步？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        WarningSolution("【横移轴】【报警】：横移轴未能够全吸取工件，直接执行下一步");
                        SuckAxisMoveStep = 6;
                    }
                    else
                    {
                        WarningSolution("【横移轴】【报警】：横移轴未能够全吸取工件，再次执行上一步");
                        SuckAxisMoveStep = 2;
                    }
                    break;
                case 4:
                    if (MessageBox.Show("横移轴下料位未能够全吸取工件，是否直接执行下一步？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        WarningSolution("【横移轴】【报警】：横移轴下料位未能够全吸取工件，直接执行下一步");
                        SuckAxisMoveStep = 6;
                    }
                    else
                    {
                        WarningSolution("【横移轴】【报警】：横移轴下料位未能够全吸取工件，再次执行上一步");
                        SuckAxisMoveStep = 2;
                    }
                    break;
                case 5:
                    if (MessageBox.Show("横移轴上料位未能够全吸取工件，是否直接执行下一步？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        WarningSolution("【横移轴】【报警】：横移轴上料位未能够全吸取工件，直接执行下一步");
                        SuckAxisMoveStep = 6;
                    }
                    else
                    {
                        WarningSolution("【横移轴】【报警】：横移轴上料位未能够全吸取工件，再次执行上一步");
                        SuckAxisMoveStep = 2;
                    }
                    break;
                case 6:
                    WarningSolution("【横移轴】【报警】：横移轴等待下料模组位置移动气缸动作&入料工位载具缩回超时");
                    break;
                case 7:
                    WarningSolution("【横移轴】【报警】：横移轴等待下料模组移动气缸动作&入料工位载具缩回超时");
                    break;
                case 8:
                    WarningSolution("【横移轴】【报警】：横移轴等待入料工位载具缩回超时");
                    break;
                case 100:
                    break;
            }
        }

        private void UnloadTrayErrorSolution(int Step)
        {
            int errcode = 0;
            WriteLog("【下料Tray】：进入错误处理程序");
            if ((Step != 5) || ((Step == 5) && (UnloadNullTraySuckTryCount >= 2)))
            {
                UnloadTrayEnable = false;
                UnloadTrayThreadAlm = true;
                if (CurStatus != (int)STATUS.STOP_STATUS)
                {
                    CurStatus = (int)STATUS.PAUSE_STATUS;
                }
            }

            switch (Step)
            {
                case 0:
                    WarningSolution("【下料Tray】【报警】：【29】下料龙门轴躲避下料Tray移动气缸过程错误");
                    break;
                case 1:
                    WarningSolution("【下料Tray】【报警】：【30】下料空Tray Z轴上升过程中错误");
                    break;
                case 2:
                    WarningSolution("【下料Tray】【报警】：【31】下料满Tray Z轴下降过程中错误");
                    break;
                case 3:
                    WarningSolution("【下料Tray】【报警】：下料空Tray未补料，无法整体补料");
                    MessageBox.Show("下料空Tray未补料，无法整体补料，请打开下料空Tray抽屉进行补料", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case 4:
                    UnloadTrayZRetract();
                    UnloadTrayStep = 1;
                    MessageBox.Show("下料Z气缸伸出错误，请确认是否有障碍物", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case 5:
                    if (UnloadNullTraySuckTryCount <= 1)
                    {
                        UnloadTraySeparateRetract();
                        for (int i = 0; i < 10; i++)
                        {
                            if (CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInPosition + 25])
                            {
                                adlink.P2PMove(logicConfig.ECATAxis[6], 1, false, ref errcode);
                                Thread.Sleep(50);
                            }
                            else
                                break;
                        }
                        UnloadTraySeparateStretch();
                        UnloadTrayStep = 2;
                    }
                    else
                    {
                        WarningSolution("下料空Tray吸取超时，请检查吸盘位置是否正确");
                        UnloadTraySeparateRetract();
                    }
                    break;
                case 6:
                    UnloadTrayZRetract();
                    UnloadTrayStep = 6;
                    MessageBox.Show("下料Z气缸伸出错误，请确认是否有障碍物", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case 7:
                    UnloadTraySeparateRetract();
                    UnloadTrayStep = 11;
                    MessageBox.Show("下料盘分盘气缸伸出错误，请确认是否存在卡盘现象", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case 100:
                    break;
                default:
                    break;
            }
        }


        private void LoadModuleErrorSolution(int No)
        {
            WriteLog("【上料模组】：进入错误处理程序");
            LoadModuleEnable = false;
            LoadModuleThreadAlm = true;
            if ((CurStatus != (int)STATUS.STOP_STATUS))
            {
                CurStatus = (int)STATUS.PAUSE_STATUS;
            }

            switch (No)
            {
                case 0:
                    WarningSolution("【上料模组】【报警】：【32】上料模组气缸缩回错误");
                    break;
                case 100:
                    break;
            }
        }

        private void UnloadModuleErrorSolution(int No)
        {
            WriteLog("【下料模组工位】：进入错误处理程序");
            UnloadModuleEnable = false;
            LoadModuleThreadAlm = true;
            if ((CurStatus != (int)STATUS.STOP_STATUS))
            {
                CurStatus = (int)STATUS.PAUSE_STATUS;
            }

            switch (No)
            {
                case 0:
                    WarningSolution("【下料模组】【报警】：【33】下料模组气缸缩回错误");
                    break;
                case 100:
                    break;
            }
        }

        private void LoadGantryErrorSolution(int No)
        {
            WriteLog("【上料龙门工位】：进入错误处理程序");
            LoadGantryEnable = false;
            LoadGantryThreadAlm = true;
            if (CurStatus != (int)STATUS.STOP_STATUS)
            {
                CurStatus = (int)STATUS.PAUSE_STATUS;
            }

            switch (No)
            {
                case 0:
                    LoadGantryRightPistonZRetract();
                    if (MessageBox.Show("上料龙门右半部分未能全部吸取工件，是否直接执行下一步？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                        LoadGantryStep = 4;
                    else
                        LoadGantryStep = 1;
                    break;
                case 1:
                    LoadGantryLeftPistonZRetract();
                    if (MessageBox.Show("上料龙门左半部分未能全部吸取工件，是否直接执行下一步？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                        LoadGantryStep = 8;
                    else
                        LoadGantryStep = 5;
                    break;
                case 2:
                    LoadGantryLeftPistonZRetract();
                    if (MessageBox.Show("上料龙门左半部分未能全部吸取工件，是否直接执行下一步？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                        LoadGantryStep = 4;
                    else
                        LoadGantryStep = 1;
                    break;
                case 3:
                    LoadGantryRightPistonZRetract();
                    if (MessageBox.Show("上料龙门右半部分未能全部吸取工件，是否直接执行下一步？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                        LoadGantryStep = 8;
                    else
                        LoadGantryStep = 5;
                    break;
                case 4:
                    LoadGantryRightPistonZRetract();
                    if (MessageBox.Show("上料龙门右半部分未能全部吸取工件，是否直接执行下一步？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                        LoadGantryStep = 9;
                    else
                        LoadGantryStep = 6;
                    break;
                case 5:
                    IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumBreak, false);
                    IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumBreak, false);
                    Thread.Sleep(100);
                    LoadGantryAllPistonZRetract();
                    if (MessageBox.Show("上料模组发生叠料，请清除上料模组未吸取的工件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        LoadGantryStep = 10;
                    }
                    break;
                case 6:
                    IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumBreak, false);
                    IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumBreak, false);
                    Thread.Sleep(100);
                    LoadGantryAllPistonZRetract();
                    if (MessageBox.Show("上料模组发生叠料，请清除上料模组未吸取的工件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        LoadGantryStep = 11;
                    }
                    break;
                case 100:
                    break;
                default:
                    break;
            }
        }

        private void UnloadGantryPlaceAllWorkPieceErrorSolution(int No)
        {
            WriteLog("【下料龙门放置工件工位】：进入错误处理程序");
            UnloadGantryPlaceAllWorkPieceEnable = false;
            UnloadGantryPlaceAllWorkPieceThreadAlm = true;
            if (CurStatus != (int)STATUS.STOP_STATUS)
            {
                CurStatus = (int)STATUS.PAUSE_STATUS;
            }

            switch (No)
            {
                case 1:
                    UnloadGantryAllSuckerSuckWithoutFeedBack();
                    UnloadGantryAllPistonZRetract();
                    if (MessageBox.Show("下料满Tray盘发生叠料，请拿走下料满Tray盘上未吸取的工件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        UnloadGantryPlaceAllWorkPieceStep = 1;
                    }
                    break;
                case 2:
                    UnloadGantryAllSuckerSuckWithoutFeedBack();
                    UnloadGantryAllPistonZRetract();
                    if (MessageBox.Show("下料满Tray盘发生叠料，请拿走下料满Tray盘上未吸取的工件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        UnloadGantryPlaceAllWorkPieceStep = 5;
                    }
                    break;
                case 3:
                    UnloadGantryAllSuckerSuckWithoutFeedBack();
                    UnloadGantryAllPistonZRetract();
                    if (MessageBox.Show("下料满Tray盘发生叠料，请拿走下料满Tray盘上未吸取的工件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        UnloadGantryPlaceAllWorkPieceStep = 6;
                    }
                    break;
                case 100:
                    break;
                default:
                    break;
            }
        }

        private void UnloadGantryErrorSolution(int No)
        {
            WriteLog("【下料龙门】：进入错误处理程序:" + No.ToString());
            UnloadGantryEnable = false;
            UnloadGantryThreadAlm = true;
            if (CurStatus != (int)STATUS.STOP_STATUS)
            {
                CurStatus = (int)STATUS.PAUSE_STATUS;
            }

            switch (No)
            {
                case 0:
                    UpdateUnloadGantryUnSuckWorkPiece();
                    UnloadGantryAllPistonZRetract();
                    WarningSolution("请把未吸取的工件扔进废料盒");
                    if (MessageBox.Show("请把未吸取的工件扔进废料盒", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        UnloadGantryZStretch();
                        UnloadGantryStep = 6;
                    }
                    break;
                case 1:
                    UnloadGantryAllPistonZRetract();
                    if (MessageBox.Show("下料龙门未能全部吸取工件，是否直接执行下一步？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        SupplyRegion2Condition = new int[8];
                        UnloadGantryStep = 17;
                    }
                    else
                    {
                        UnloadGantryStep = 13;
                        WarningSolution("请把补料区未吸取的工件重新摆放");
                        MessageBox.Show("请把补料区未吸取的工件重新摆放");
                    }
                    break;
                case 2:
                    IOControl.ECATWriteDO(UnloadGantrySuckerSuckControls[3], false);
                    IOControl.ECATWriteDO(UnloadGantrySuckerBreakControls[3], true);
                    UnloadGantryZRetract();
                    IOControl.ECATWriteDO(UnloadGantrySuckerBreakControls[3], false);
                    UnloadGantryStep = 11;
                    WarningSolution("请把补料盘1区未吸取的工件(下料龙门左4气缸正下方)重新摆放");
                    MessageBox.Show("请把补料盘1区未吸取的工件(下料龙门左4气缸正下方)重新摆放");
                    break;
                case 3:
                    UnloadGantryAllSuckerSuckWithoutFeedBack();
                    UnloadGantryAllPistonZRetract();

                    if (MessageBox.Show("下料模组发生叠料，请拿走下料模组上未吸取的工件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        UnloadGantryStep = 2;
                    }
                    break;
                case 4:
                    WaitECATPiston2Cmd1FeedbackDone(UnloadGantrySuckerSuckControls[3], UnloadGantrySuckerBreakControls[3], UnloadGantrySuckerCheckBits[3], true);
                    UnloadTrayZRetract();
                    WaitECATPiston2Cmd2FeedbackDone(UnloadGantryCylinderRetractControls[3], UnloadGantryCylinderStretchControls[3], UnloadGantryCylinderRetractCheckBits[3], UnloadGantryCylinderStretchCheckBits[3]);
                    if (MessageBox.Show("补料盘2区发生叠料，请拿走下料龙门左4气缸正下方未吸取的工件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        UnloadGantryPlaceWorkPiece(3);
                        SupplyRegion1Condition[GetRegion1CurSupplyPos(SupplyRegion1Condition)] = 0;
                        SupplyRegion2Condition[GetRegion2CurNullPos(SupplyRegion2Condition)] = 1;
                    }
                    UnloadGantryStep = 11;
                    break;
                case 5:
                    UnloadGantryAllSuckerSuckWithoutFeedBack();
                    UnloadGantryAllPistonZRetract();
                    if (MessageBox.Show("补料区域2发生叠料，请拿走补料区域2上未吸取的工件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        UnloadGantryStep = 13;
                    }
                    break;
                case 6:
                    UnloadGantryAllSuckerSuckWithoutFeedBack();
                    UnloadGantryAllPistonZRetract();
                    if (MessageBox.Show("补料区域1发生叠料，请拿走补料区域1上未吸取的工件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        UnloadGantryStep = 9;
                    }
                    break;
                case 7:
                    UnloadGantryAllSuckerSuckWithoutFeedBack();
                    UnloadGantryAllPistonZRetract();
                    if (MessageBox.Show("补料区域2发生叠料，请拿走补料区域2上未吸取的工件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        UnloadGantryStep = 10;
                    }
                    break;
                case 100:
                    break;
                default:
                    break;
            }
        }

        private void UnloadGantryAllSuckerSuckWithoutFeedBack()
        {
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft1VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft1VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft2VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft2VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft3VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft3VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft4VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft4VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight1VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight1VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight2VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight2VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight3VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight3VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight4VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight4VacumBreak, false);
            Thread.Sleep(250);
        }

        private void UpdateUnloadGantryUnSuckWorkPiece()
        {
            if (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft1VacumCheck + 25] == false)
                UnloadGantryCheckResult[0] = 0;
            if (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft2VacumCheck + 25] == false)
                UnloadGantryCheckResult[1] = 0;
            if (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft3VacumCheck + 25] == false)
                UnloadGantryCheckResult[2] = 0;
            if (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft4VacumCheck + 25] == false)
                UnloadGantryCheckResult[3] = 0;
            if (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight1VacumCheck + 25] == false)
                UnloadGantryCheckResult[4] = 0;
            if (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight2VacumCheck + 25] == false)
                UnloadGantryCheckResult[5] = 0;
            if (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight3VacumCheck + 25] == false)
                UnloadGantryCheckResult[6] = 0;
            if (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight4VacumCheck + 25] == false)
                UnloadGantryCheckResult[7] = 0;
        }


        private void CCDUndo(int TrayNo, int ComponentStep, ref int StepB)
        {
            WriteLog("【CCD检测】：进入CCDUndo处理程序");
            UpdateCCDUndoDataGridView(TrayNo);
            AutoRunStepB = 0;
            while (isWaitingCCD || isCheckingCCD || TcpIpRecvStatus)
                Thread.Sleep(10);
            CCDRawDataList.Clear();
            CCDRecvStatusList.Clear();
            RecvClassify = 0;
            RecvCount = 0;
            for (int i = 0; i < systemParam.WorkPieceNum; i++)
            {
                CameraCheckDoneArrayTray[TrayNo, i] = false;
                CameraCheckResultArrayTray[TrayNo, i] = 0;
            }
            AutoRunPartBHomeDone = true;
        }

        public bool AutoRunPartBHomeDone = false;
        private void AutoRunPartBHome()
        {
            WriteLog("【CCD检测】CCD归零过程开始");
            int errcode = 0; double PosError = 0;
            #region 修正归零过程
            if (adlink.LineMove(new Axis[] { logicConfig.PulseAxis[1], logicConfig.PulseAxis[2] }, new double[] { CCDMotionPos.posInfo[0].XPos - 2, CCDMotionPos.posInfo[0].YPos }, true, ref errcode, ref PosError))
            {
                AutoRunPartBHomeDone = true;
            }
            else
            {
                AutoRunEnablePartB = false;
                CCDCheckThreadAlm = true;
                CCDChecking = false;
                if (CurStatus != (int)STATUS.STOP_STATUS)
                {
                    CurStatus = (int)STATUS.PAUSE_STATUS;
                }
                WarningSolution("【CCD检测】【报警】CCD归零过程出错");
            }
            #endregion
        }

        public void AutoRunPartBThread()
        {
            int errcode = 0;
            CCDChecking = true;
            AutoRunPartBCircleCount = 0;
            CCDRecvStatusList.Clear();

            while (AutoRunActive)
            {
                if (AutoRunEnablePartB)
                {
                    if ((!AutoRunPartBFinished) && (MainAxisMoveFinish || debugThreadB))
                    {
                        AutoRunPartBStatusSwitch(ref AutoRunStepB, ref errcode);
                    }
                }
                Thread.Sleep(20);
            }
        }
        #endregion

        #region 3#工位AutoRun工序
        public int AutoRunStepC = 0;//added by lei.c, notice this is different from AutoRunStep
        public int AutoRunPartCCircleCount = 0;//added by lei.c
        public bool AutoRunEnablePartC = false;
        public bool AutoRunPartCFinished = false;//表明工位3动作是否完成
        //public List<LaserUpdateStruct> LaserCheckUpdateStruList = new List<LaserUpdateStruct>();


        private int SetLinearTrigger(int i, int nCh)
        {
            WriteLog("【Laser检测】" + i.ToString() + "穴设置比较触发开始");
            APS168.APS_reset_trigger_count(logicConfig.boardIdCard, nCh - 1);
            APS168.APS_set_trigger_param(logicConfig.boardIdCard, (Int32)APSDefine.TGR_TRG_EN, Convert.ToInt32("0001", 2));
            APS168.APS_set_trigger_param(logicConfig.boardIdCard, (Int32)APSDefine.TGR_TRG0_SRC, 0);
            APS168.APS_set_trigger_param(logicConfig.boardIdCard, (Int32)APSDefine.TGR_LCMP0_SRC, 4);
            APS168.APS_set_trigger_param(logicConfig.boardIdCard, (short)APSDefine.TGR_TRG0_PWD, 500);
            APS168.APS_set_trigger_param(logicConfig.boardIdCard, (short)APSDefine.TGR_TRG0_TGL, 0);
            APS168.APS_set_trigger_param(logicConfig.boardIdCard, (Int32)APSDefine.TGR_TRG0_SRC, 0x10);

            int returnnum = APS168.APS_set_trigger_linear(logicConfig.boardIdCard, nCh - 1, (int)(moveConfig.moveConfig[i].TrigPos.Xpos * 1000), moveConfig.moveConfig[i].nTrigNum, (int)(moveConfig.moveConfig[i].dTrigInterval * 1000));
            WriteLog("【Laser检测】" + i.ToString() + "穴设置比较触发完成");
            return returnnum;
        }

        //返回值：-1->激光数据问题；-2->轴运动问题
        //1->正常返回

        public int AutoRunPartCComponent(int StepC, int CTrayNo, ref int errcode)
        {
            try
            {
                WriteLog("【Laser检测】" + StepC.ToString() + "穴Laser检测动作开始");
                double PosError = 0;
                LaserAllData.Clear();
                AutoRunPartCFinished = false;
                nLaserData = 0;
                DateTime StartTime = DateTime.Now;
                //isLaserRunning = true;
                StartTime = DateTime.Now;
                WriteLog("【Laser检测】" + StepC.ToString() + "穴移动至轨迹起点开始");
                if (adlink.LineMove(new Axis[] { logicConfig.PulseAxis[3], logicConfig.PulseAxis[4] }, new double[] { moveConfig.moveConfig[StepC].StartPos.Xpos, moveConfig.moveConfig[StepC].StartPos.Ypos }, true, ref errcode, ref PosError))
                {
                    //设置比较触发分通道 LaserRelated
                    WriteLog("【Laser检测】" + StepC.ToString() + "穴移动至轨迹起点结束");
                    SetLinearTrigger(StepC, 1);
                    keyenceLJ.LJClearBuffer(0);
                    if (!isSet1Param)
                    {
                        while (!isSet1Param)
                        {
                            if (!OutTimeCount(StartTime, 10))
                            {
                                if ((CurStatus != (int)STATUS.STOP_STATUS) && (CurStatus != (int)STATUS.PAUSE_STATUS))
                                {
                                    WriteLog("【Laser检测】【*********】35报警前即将暂停");
                                }
                                isLaserRunning = false;
                                WarningSolution("【Laser检测】【报警】：【35】1-Laser检测工位第" + StepC.ToString() + "工序发生问题，LaserErrCode：" + LaserErrCode);
                                LaserCheckThreadAlm = true;
                                return -1;
                            }
                            Thread.Sleep(30);
                        }
                    }
                    TimeSpan st = DateTime.Now - StartTime;
                    //UpdateWaringLog.Invoke((object)"激光运动到开始和设置触发时间:" + st.TotalSeconds.ToString());
                    WriteLog("【Laser检测】激光运动到开始和设置触发时间: " + st.TotalSeconds.ToString());
                    LaserResult = keyenceLJ.StartMeasure(0, out LaserErrCode);
                    if (!LaserResult)
                    {
                        isSet1Param = false;
                        if ((CurStatus != (int)STATUS.STOP_STATUS) && (CurStatus != (int)STATUS.PAUSE_STATUS))
                        {
                            WriteLog("【Laser检测】【*********】StartMeasure后37报警前先将 isSet1Param置为false");//0904
                        }
                        isLaserRunning = false;
                        WarningSolution("【Laser检测】【报警】：【37】Laser检测工位第" + StepC.ToString() + "工序开始批处理发生问题，LaserErrCode：" + LaserErrCode);
                        LaserCheckThreadAlm = true;
                        return -1;
                    }
                    st = DateTime.Now - StartTime;
                    WriteLog("【Laser检测】激光开始批处理时间:" + st.TotalSeconds.ToString());
                    StartTime = DateTime.Now;
                    WriteLog("【Laser检测】" + StepC.ToString() + "穴移动至轨迹终点开始");
                    if (adlink.LineMove(new Axis[] { logicConfig.PulseAxis[3], logicConfig.PulseAxis[4] }, new double[] { moveConfig.moveConfig[StepC].EndPos.Xpos, moveConfig.moveConfig[StepC].EndPos.Ypos }, true, ref errcode, ref PosError))
                    {
                        WriteLog("【Laser检测】" + StepC.ToString() + "穴移动至轨迹终点结束");
                        int countTrig = GetTrigCounts(0);
                        //UpdateWaringLog.Invoke((object)("触发个数0-" + countTrig.ToString()));
                        st = DateTime.Now - StartTime;
                        WriteLog("【Laser检测】激光到位时间:" + st.TotalSeconds.ToString());
                        StartTime = DateTime.Now;
                        LaserResult = keyenceLJ.StopMeasureProfile(0, out LaserErrCode);
                        if (!LaserResult)
                        {
                            isSet1Param = false;
                            if ((CurStatus != (int)STATUS.STOP_STATUS) && (CurStatus != (int)STATUS.PAUSE_STATUS))
                            {
                                WriteLog("【Laser检测】【*********】StopMeasureProfile后39前先将 isSet1Param置为false");//0904
                            }
                            isLaserRunning = false;
                            WarningSolution("【Laser检测】【报警】：【39】Laser检测工位第" + StepC.ToString() + "工序停止批处理发生问题，LaserErrCode：" + LaserErrCode);
                            LaserCheckThreadAlm = true;
                            return -1;
                        }
                        st = DateTime.Now - StartTime;
                        WriteLog("【Laser检测】激光停止批处理时间:" + st.TotalSeconds.ToString());
                        StartTime = DateTime.Now;
                        LaserResult = keyenceLJ.GetBatchProfileData(0, StepC, moveConfig.moveConfig[StepC].nTrigNum, out LaserErrCode);
                        if (!LaserResult)
                        {
                            isSet1Param = false;
                            if ((CurStatus != (int)STATUS.STOP_STATUS) && (CurStatus != (int)STATUS.PAUSE_STATUS))
                            {
                                WriteLog("【Laser检测】【*********】GetBatchProfileData后41前将 isSet1Param置为false");//0904
                            }
                            isLaserRunning = false;
                            WarningSolution("【Laser检测】【报警】：【41】2-Laser检测工位第" + StepC.ToString() + "工序发生问题，LaserErrCode：" + LaserErrCode);
                            LaserCheckThreadAlm = true;
                            return -1;
                        }
                        if (moveConfig.moveConfig[StepC].bIsATrig)
                        {
                            nLaserData = nLaserData + 1;
                            string strNO = StepC.ToString() + "," + (2 * StepC).ToString();
                            LaserADicData.Clear();
                            Thread startLaserACollect = new Thread(new ParameterizedThreadStart(LaserACollect));
                            startLaserACollect.Priority = ThreadPriority.Highest;
                            startLaserACollect.IsBackground = true;
                            startLaserACollect.Name = (2 * StepC).ToString();
                            ThreadNames.Add(startLaserACollect.Name);
                            startLaserACollect.Start(strNO);
                        }
                        if (moveConfig.moveConfig[StepC].bIsBTrig)
                        {
                            nLaserData = nLaserData + 1;
                            string strNO = StepC.ToString() + "," + (2 * StepC + 1).ToString();
                            LaserBDicData.Clear();
                            Thread startLaserBCollect = new Thread(new ParameterizedThreadStart(LaserBCollect));
                            startLaserBCollect.Priority = ThreadPriority.Highest;
                            startLaserBCollect.IsBackground = true;
                            startLaserBCollect.Name = (2 * StepC + 1).ToString();
                            ThreadNames.Add(startLaserBCollect.Name);
                            startLaserBCollect.Start(strNO);
                        }
                        //判断数据是否收集完成
                        DateTime starttime = DateTime.Now;
                        while (nLaserData != (LaserADicData.Count() + LaserBDicData.Count()))
                        {
                            if (!OutTimeCount(starttime, 5))
                            {
                                WarningSolution("数据收集超时");
                                return -1;
                            }
                            Thread.Sleep(30);
                        }
                        WriteLog("【Laser检测】扫描完成，数据整合中");
                        for (int j = 0; j < nLaserData; j++)
                        {
                            LaserAllData.Add(Laser12DicData[(2 * StepC + j).ToString()]);
                        }

                        #region 整理数据Test Code
                        LaserTaskRun myLaserTaskRun = new LaserTaskRun(LaserAllData, AutoRunLaserFinishCount, StepC, CTrayNo, debugThreadC);
                        Thread Task3DRunThread = new Thread(new ParameterizedThreadStart(Task3DRun));
                        Task3DRunThread.IsBackground = true;
                        Task3DRunThread.Start(myLaserTaskRun);
                        //Task3DRun(myLaserTaskRun);//发送数据给3D软件
                        #endregion
                        st = DateTime.Now - StartTime;
                        WriteLog("【Laser检测】数据处理时间:" + st.TotalSeconds.ToString());
                        isLaserRunning = false;
                        AutoRunLaserFinishCount++;
                        WriteLog("【Laser检测】" + StepC.ToString() + "穴Laser检测动作结束");
                        return 1;
                    }
                    else
                    {
                        if ((CurStatus != (int)STATUS.STOP_STATUS) && (CurStatus != (int)STATUS.PAUSE_STATUS))
                        {
                            WriteLog("【Laser检测】【*********】43报警前即将暂停");//0904
                        }
                        isLaserRunning = false;
                        WarningSolution("【Laser检测】【报警】：【43】Laser检测工位第" + StepC.ToString() + "工序发生问题，LaserErrCode：" + LaserErrCode);
                        LaserCheckThreadAlm = true;
                        return -2;
                    }
                }
                else
                {
                    if ((CurStatus != (int)STATUS.STOP_STATUS) && (CurStatus != (int)STATUS.PAUSE_STATUS))
                    {
                        WriteLog("【Laser检测】【*********】45报警前即将暂停");//0904
                    }
                    isLaserRunning = false;
                    WarningSolution("【Laser检测】【报警】：【45】Laser检测工位第" + StepC.ToString() + "工序发生问题，LaserErrCode：" + LaserErrCode);
                    LaserCheckThreadAlm = true;
                    return -2;
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(ThreadAbortException))
                {
                    string exStr = "";
                    exStr += ex.ToString() + "\n";
                    exStr += "StepC=" + StepC.ToString() + "\n";
                }
                return -1;
            }
        }

        public void LaserTestOnce(object sender)
        {
            int errcode = 0;
            int myid = (int)sender;
            //Test Start
            LaserAllData.Clear();
            LaserADicData.Clear();
            LaserBDicData.Clear();
            Laser12DicData.Clear();
            ThreadNames.Clear();
            //Test End
            AutoRunPartCComponent(myid, 0, ref errcode);
        }


        private int LaserRedoCount = 0;
        private void AutoRunPartCStatusSwitch(ref int StepC, ref int errcode)
        {
            int checkresult = 0;
            try
            {
                if ((StationCheckSetting[PartCTrayNo].LaserCheck && AutoRunMainAxisCircleCount >= 3) || debugThreadC)
                {
                    Laser12DicData.Clear();
                    #region 设置laser高速处理
                    //if (!isSet1Param)
                    //{
                    //    int trigCount = 0;
                    //    for (int g = 0; g < moveConfig.moveConfig.Count(); g++)
                    //    {
                    //        trigCount = moveConfig.moveConfig[g].nTrigNum;
                    //    }
                    //    isSet1Param = false;
                    //    Thread setLaser = new Thread(new ParameterizedThreadStart(SetLaserParam));//包含设置线性触发
                    //    setLaser.Priority = ThreadPriority.Highest;
                    //    setLaser.IsBackground = true;
                    //    setLaser.Start(trigCount);
                    //}
                    #endregion
                    switch (StepC)
                    {
                        case (int)AutoRunPartCStep.WaitLaserHome:
                            WriteLog("【Laser检测】开始Laser检测工位流程");
                            if (AutoRunPartCCircleCount != 0)
                            {
                                WriteLog("【Laser检测】等待Laser归位完成开始");
                                DateTime StartTime = DateTime.Now;
                                while (true)
                                {
                                    if (AutoRunPartCHomeDone)
                                        break;
                                    else
                                    {
                                        if (!OutTimeCount(StartTime, 3))
                                        {
                                            AutoRunEnablePartC = false;
                                            LaserCheckThreadAlm = true;
                                            if (CurStatus != (int)STATUS.STOP_STATUS)
                                            {
                                                CurStatus = (int)STATUS.PAUSE_STATUS;
                                            }
                                            WarningSolution("【CCD检测】【报警】：等待Laser归位超时");
                                            return;
                                        }
                                        Thread.Sleep(30);
                                    }
                                }
                                AutoRunPartCHomeDone = false;
                                WriteLog("【Laser检测】等待Laser归位完成结束");
                            }
                            StepC = (int)AutoRunPartCStep.LaserCheck1;
                            break;
                        case (int)AutoRunPartCStep.LaserCheck1:
                            AutoRunStepCStartTime = DateTime.Now;
                            if (StationCheckSetting[PartCTrayNo].LaserHoleCheck[0] == true)
                            {
                                checkresult = AutoRunPartCComponent((int)AutoRunPartCStep.LaserCheck1, PartCTrayNo, ref errcode);
                                if (checkresult == 1)
                                    StepC = (int)AutoRunPartCStep.LaserCheck2;
                                else
                                {
                                    LaserErrorSolution(PartBTrayNo, checkresult, ref StepC);
                                    return;
                                }
                            }
                            else
                            {
                                LaserCheckDoneArrayTray[PartCTrayNo, 0] = true;
                                StepC = (int)AutoRunPartCStep.LaserCheck2;
                            }
                            break;
                        case (int)AutoRunPartCStep.LaserCheck2:
                            if (StationCheckSetting[PartCTrayNo].LaserHoleCheck[1] == true)
                            {
                                checkresult = AutoRunPartCComponent((int)AutoRunPartCStep.LaserCheck2, PartCTrayNo, ref errcode);
                                if (checkresult == 1)
                                    StepC = (int)AutoRunPartCStep.LaserCheck3;
                                else
                                {
                                    LaserErrorSolution(PartBTrayNo, checkresult, ref StepC);
                                    return;
                                }
                            }
                            else
                            {
                                LaserCheckDoneArrayTray[PartCTrayNo, 1] = true;
                                StepC = (int)AutoRunPartCStep.LaserCheck3;
                            }
                            break;
                        case (int)AutoRunPartCStep.LaserCheck3:
                            if (StationCheckSetting[PartCTrayNo].LaserHoleCheck[2] == true)
                            {
                                checkresult = AutoRunPartCComponent((int)AutoRunPartCStep.LaserCheck3, PartCTrayNo, ref errcode);
                                if (checkresult == 1)
                                    StepC = (int)AutoRunPartCStep.LaserCheck4;
                                else
                                {
                                    LaserErrorSolution(PartBTrayNo, checkresult, ref StepC);
                                    return;
                                }
                            }
                            else
                            {
                                LaserCheckDoneArrayTray[PartCTrayNo, 2] = true;
                                StepC = (int)AutoRunPartCStep.LaserCheck4;
                            }
                            break;
                        case (int)AutoRunPartCStep.LaserCheck4:
                            if (StationCheckSetting[PartCTrayNo].LaserHoleCheck[3] == true)
                            {
                                checkresult = AutoRunPartCComponent((int)AutoRunPartCStep.LaserCheck4, PartCTrayNo, ref errcode);
                                if (checkresult == 1)
                                    StepC = (int)AutoRunPartCStep.StartLaserHome;
                                else
                                {
                                    LaserErrorSolution(PartBTrayNo, checkresult, ref StepC);
                                    return;
                                }
                            }
                            else
                            {
                                LaserCheckDoneArrayTray[PartCTrayNo, 3] = true;
                                StepC = (int)AutoRunPartCStep.StartLaserHome;
                            }
                            break;
                        case (int)AutoRunPartCStep.StartLaserHome:
                            //开启Laser工位回零线程
                            Thread AutoRunPartCHomeThread = new Thread(new ThreadStart(AutoRunPartCHome));
                            AutoRunPartCHomeThread.IsBackground = true;
                            AutoRunPartCHomeThread.Start();
                            AutoRunPartCStatusFinish();
                            break;
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                        LaserCheckDoneArrayTray[PartCTrayNo, i] = true;

                    AutoRunPartCHomeDone = true;
                    AutoRunPartCStatusFinish();
                    return;
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(ThreadAbortException))
                {
                    string exStr = "";
                    exStr += "【Laser检测】" + ex.ToString() + "\n";
                    exStr += "AutoRunStepC=" + StepC.ToString() + "\n";
                    MessageBox.Show(exStr);
                }
            }
        }

        public bool AutoRunPartCHomeDone = false;
        private void AutoRunPartCHome()
        {
            WriteLog("【Laser检测】Laser轴归位开始");
            int errcode = 0; double PosError = 0;
            #region 修正归零过程
            if (adlink.LineMove(new Axis[] { logicConfig.PulseAxis[3], logicConfig.PulseAxis[4] }, new double[] { moveConfig.moveConfig[0].StartPos.Xpos, moveConfig.moveConfig[0].StartPos.Ypos }, true, ref errcode, ref PosError))
            {
                AutoRunPartCHomeDone = true;
                WriteLog("【Laser检测】Laser轴归位结束");
            }
            else
            {
                AutoRunEnablePartC = false;
                LaserCheckThreadAlm = true;
                if (CurStatus != (int)STATUS.STOP_STATUS)
                {
                    CurStatus = (int)STATUS.PAUSE_STATUS;
                }
                WarningSolution("【CCD检测】【报警】：Laser归位时出错");
                return;
            }
            #endregion
        }


        public void AutoRunPartCThread()
        {
            int errcode = 0;
            AutoRunPartCCircleCount = 0;
            nLaserData = 0;
            //AutoRunPartCFinished = false;

            //开启数据收集线程
            LaserAllData.Clear();
            LaserADicData.Clear();
            LaserBDicData.Clear();
            Laser12DicData.Clear();
            ThreadNames.Clear();

            while (AutoRunActive)
            {
                if (AutoRunEnablePartC)
                {
                    if ((!AutoRunPartCFinished) && (MainAxisMoveFinish || debugThreadC))
                    {
                        AutoRunPartCStatusSwitch(ref AutoRunStepC, ref errcode);
                    }
                }
                Thread.Sleep(40);
            }
        }
        #endregion

        private int[] UpdateFinalInfomation(int CircleCount, int ATrayNo)
        {
            if (CircleCount <= 3)
                return null;

            int[] FinalResult = GenerateFinalResult(ATrayNo);
            FinalResultUpdateStru temp = new FinalResultUpdateStru(CircleCount, ATrayNo, FinalResult);
            UpdateFinalResultList(temp);//按照CCD的顺序输出FinalResult

            for (int i = 0; i < systemParam.WorkPieceNum; i++)
            {
                switch (FinalResult[i])
                {
                    case 1:
                        TotalCheckANum++;
                        myFinaldataSummary.myPassratioSummary.ANum++;
                        myFinaldataSummary.myPassratioSummary.PassNum++;
                        break;
                    case -1:
                        TotalCheckBNum++;
                        myFinaldataSummary.myPassratioSummary.BNum++;
                        break;
                    case -2:
                        TotalCheckCNum++;
                        myFinaldataSummary.myPassratioSummary.CNum++;
                        break;
                    case -3:
                        TotalCheckDNum++;
                        myFinaldataSummary.myPassratioSummary.DNum++;
                        break;
                    case -4:
                        TotalCheckENum++;
                        break;
                    default:
                        break;
                }
                if (FinalResult[i] != 0 && FinalResult[i] != -4)
                {
                    TotalCheckNum++;
                    myFinaldataSummary.myPassratioSummary.TotalNum++;
                }

            }

            if (CurStatus == (int)STATUS.AUTO_STATUS)
                UpdateTrueCT(new TrueCTCalcStru(DateTime.Now, TotalCheckNum));

            for (int i = 0; i < systemParam.WorkPieceNum; i++)
            {
                pieceAllDataArray[ATrayNo, i].Level = FinalResult[i];
            }

            CalcPassRatio(FinalResult);
            UpdatePassRatio(myPassRatio);
            return FinalResult;
        }

        #region   樊竞明20180902 Excel添加
        //***********************樊竞明20180902**********************************//
        //复制数据模板
        public void ExcelFileCopy()
        {
            string dt = DateTime.Now.ToString("yyMMdd-HHmmss");
            ExcelFinalDataDirPath = "D: \\FinalData\\";
            ExcelFinalDataFilePath = ExcelFinalDataDirPath + dt + "_" + ExcelDetectBatchMsg + "_" + ExcelTestBatchSerialNum.ToString() + ".xlsx";
            if (!Directory.Exists(ExcelFinalDataDirPath))
            {
                Directory.CreateDirectory(ExcelFinalDataDirPath);
                MessageBox.Show("文件夹 【D: \\FinalData\\】 不存在，已经新建此文件夹,请复制文件Excel表【数据输出格式v6.xlsx】到此文件夹路径");
            }

            if (File.Exists("D:\\FinalData\\数据输出格式v6.xlsx"))
            {
                if (!File.Exists(ExcelFinalDataFilePath))
                {
                    ExcelWriteRowNum = 0;
                    ExcelTotalNum = 0;
                    ExcelNgNum = 0;
                    ExcelTestBatchSerialNum += 1;
                    File.Copy("D:\\FinalData\\数据输出格式v6.xlsx", ExcelFinalDataFilePath);
                    //******************************樊竞明 20190930*********************************
                    //ThrParam
                    FileInfo fileinfo = new FileInfo(ExcelFinalDataFilePath);//依路径创建文件流
                    using (ExcelPackage package = new ExcelPackage(fileinfo))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets["Lead数据"];

                        if (ThrParam.thrInfo != null)
                        {
                            for (int i = 0; i < ThrParam.thrInfo.Length; i++)
                            {
                                worksheet.Cells[4, 6 + i].Value = ThrParam.thrInfo[i].UpLimit;
                                worksheet.Cells[4, 8 + i].Value = ThrParam.thrInfo[i].DownLimit;
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("报表模板不存在，请将模板放到D:\\FinalData\\");
            }

        }

        //赋值Excel数据模板的方法重载        
            //根据路径复制数据模板及更改名称
        public void ExcelFileCopy(string Filepath)
        {
            //string  _ExcelFinalDataFilePath = Filepath + "_" + ExcelDetectBatchMsg +   ".xlsx";
            //if (!Directory.Exists(_ExcelFinalDataFilePath))
            //{
            //    Directory.CreateDirectory(_ExcelFinalDataFilePath);
            //    MessageBox.Show("文件夹 【D: \\FinalData\\】 不存在，已经新建此文件夹,请复制文件Excel表【数据输出格式v6.xlsx】到此文件夹路径");
            //}

            if (File.Exists("D:\\FinalData\\数据输出格式v6.xlsx"))
            {
                if (!File.Exists(Filepath))
                {
                    ExcelWriteRowNum = 0;
                    ExcelTotalNum = 0;
                    ExcelNgNum = 0;
                    File.Copy("D:\\FinalData\\数据输出格式v6.xlsx", Filepath);
                    //******************************樊竞明 20190930*********************************
                    //根据系统参数更新各FAI上下限
                    FileInfo fileinfo = new FileInfo(Filepath);//依路径创建文件流
                    using (ExcelPackage package = new ExcelPackage(fileinfo))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets["Lead数据"];

                        if (ThrParam.thrInfo != null)
                        {
                            for (int i = 0; i < ThrParam.thrInfo.Length; i++)
                            {
                                worksheet.Cells[4, 6 + i].Value = ThrParam.thrInfo[i].UpLimit;
                                worksheet.Cells[6, 6 + i].Value = ThrParam.thrInfo[i].DownLimit;
                            }
                        }
                        else
                        {
                            MessageBox.Show("ExcelWrite:" + "系统各FAI参数还未初始化赋值");
                        }
                        package.Save();
                    }
                }
            }
            else
            {
                MessageBox.Show("报表模板 D:\\FinalData\\数据输出格式v6.xlsx 不存在，请将模板放到D:\\FinalData\\");
            }
            
        }
        //根据路径名称复制总的数据模板  樊竞明20181010
        public void ExcelFileCopySummary(string Filepath)
        {
            //string  _ExcelFinalDataFilePath = Filepath + "_" + ExcelDetectBatchMsg +   ".xlsx";
            ///判断并新建备份Summery数据的文件夹
            if (!Directory.Exists(DataSummaryBackDirPath))
            {
                Directory.CreateDirectory(DataSummaryBackDirPath);
            }

            if (File.Exists("D:\\FinalData\\数据输出格式v6总表.xlsx"))
            {
                if (!File.Exists(Filepath))
                {
                    ExcelWriteRowNum = 0;
                    ExcelTotalNum = 0;
                    ExcelNgNum = 0;
                    File.Copy("D:\\FinalData\\数据输出格式v6总表.xlsx", Filepath);
                    //******************************樊竞明 20190930*********************************
                    //根据系统参数更新各FAI上下限
                    FileInfo fileinfo = new FileInfo(Filepath);//依路径创建文件流
                    using (ExcelPackage package = new ExcelPackage(fileinfo))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets["Lead数据"];

                        if (ThrParam.thrInfo != null)
                        {
                            if(bIsSunway)
                            {
                                for (int i = 0; i < ThrParam.thrInfo.Length; i++)
                                {
                                    worksheet.Cells[4, 8 + i].Value = ThrParam.thrInfo[i].UpLimit;
                                    worksheet.Cells[6, 8 + i].Value = ThrParam.thrInfo[i].DownLimit;
                                }
                            }
                            if(bIsLaird)
                            {
                                for (int i = 0; i < ThrParam.thrInfo.Length; i++)
                                {
                                    worksheet.Cells[4, 4 + i].Value = ThrParam.thrInfo[i].UpLimit;
                                    worksheet.Cells[6, 4 + i].Value = ThrParam.thrInfo[i].DownLimit;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("ExcelWrite:" + "系统各FAI参数还未初始化赋值");
                        }
                        package.Save();
                    }
                    myFinaldataSummary = new FinalDataSummery();//新建1个总数据对象，以便清零
                }
            }
            else
            {
                MessageBox.Show("报表模板 D:\\FinalData\\数据输出格式v6总表.xlsx 不存在，请将模板放到D:\\FinalData\\");
            }
        }

        /// <summary>
        ///判断白晚班&&根据设置的时间分段参数判断当前时间的区间段  
        /// </summary>
        /// 
        public void JudgeTimeBlock()
        {
            DateTime CompareTime1 = Convert.ToDateTime("08:00");
            DateTime CompareTime2 = Convert.ToDateTime("20:00");
            int TimeSpanNum;
            string ExcelDirectory_Date = DateTime.Now.ToString("yyyy-MM-dd");
            string ExcelDirectory_DateLast = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            string ExcelDirectory_DataFile = ExcelFinalDataDirPath;
            //计算出1个班需要几个时间段
            if (systemParam.ProductionRecordHourBeat > 0)
            {
                TimeSpanNum = 12 / systemParam.ProductionRecordHourBeat;
            }
            else
            {
                MessageBox.Show("生产数据记录的小时段为0，请设置为>0，<= 12 的数值");
                return;
            }
            //判断白晚班并新建文件夹
            if (DateTime.Compare(DateTime.Now, CompareTime1) > 0 && DateTime.Compare(CompareTime2, DateTime.Now) > 0)
            {
                ExcelDirectory_DataFile = ExcelFinalDataDirPath + "\\" + ExcelDirectory_Date + "_白班";
                if (!Directory.Exists(ExcelDirectory_DataFile))
                {
                    Directory.CreateDirectory(ExcelDirectory_DataFile);
                }
                //总表文件名称
                ExcelFinalDataFilePathSummary = ExcelDirectory_DataFile + "\\" + ExcelDirectory_Date + "_白班总表.xlsx";

                //判断时间段并新建文件
                string NowH = DateTime.Now.ToString("HH");
                int NowHn;
                int.TryParse(NowH, out NowHn);

                for (int i = 1; i <= TimeSpanNum; i++)
                {
                    if (NowHn < (8 + systemParam.ProductionRecordHourBeat * i))
                    {
                        ExcelFinalDataFilePath = ExcelDirectory_DataFile + "\\" + ExcelDirectory_Date + "-" + (8 + systemParam.ProductionRecordHourBeat * (i - 1)).ToString() + "_" + ExcelDetectBatchMsg + "_白班" + ".xlsx";
                        break;
                    }
                }
               
            }
            //00:00-8:00之间晚班判断
            else if (DateTime.Compare(DateTime.Now, CompareTime1) < 0)
            {
                ExcelDirectory_DataFile = ExcelFinalDataDirPath + "\\" + ExcelDirectory_DateLast + "_晚班";
                if (!Directory.Exists(ExcelDirectory_DataFile))
                {
                    Directory.CreateDirectory(ExcelDirectory_DataFile);
                }
                //总表文件名称
                ExcelFinalDataFilePathSummary = ExcelDirectory_DataFile + "\\" + ExcelDirectory_DateLast + "_晚班总表.xlsx";
                //判断时间段并新建文件
                string NowH = DateTime.Now.ToString("HH");
                int NowHn;
                int.TryParse(NowH, out NowHn);
                for (int i = 1; i <= TimeSpanNum; i++)
                {
                    if (NowHn < (0 + systemParam.ProductionRecordHourBeat * i))
                    {
                        ExcelFinalDataFilePath = ExcelDirectory_DataFile + "\\" + ExcelDirectory_DateLast + "-" + (0 + systemParam.ProductionRecordHourBeat * (i - 1)).ToString() + "_" + ExcelDetectBatchMsg + "_晚班" + ".xlsx";
                        break;
                    }
                }
            }
            //20:00-24:00晚班判断
            else if (DateTime.Compare(DateTime.Now,CompareTime2 ) > 0)
            {
                ExcelDirectory_DataFile = ExcelFinalDataDirPath + "\\" + ExcelDirectory_Date + "_晚班";
                if (!Directory.Exists(ExcelDirectory_DataFile))
                {
                    Directory.CreateDirectory(ExcelDirectory_DataFile);
                }
                //总表文件名称
                ExcelFinalDataFilePathSummary = ExcelDirectory_DataFile + "\\" + ExcelDirectory_Date + "_晚班总表.xlsx";
                //判断时间段并新建文件
                string NowH = DateTime.Now.ToString("HH");
                int NowHn;
                int.TryParse(NowH, out NowHn);
                for (int i = 1; i <= TimeSpanNum; i++)
                {
                    if (20<=NowHn && NowHn < (20 + systemParam.ProductionRecordHourBeat * i))
                    {
                        ExcelFinalDataFilePath = ExcelDirectory_DataFile + "\\" + ExcelDirectory_Date + "-" + (20 + systemParam.ProductionRecordHourBeat * (i - 1)).ToString() + "_" + ExcelDetectBatchMsg + "_晚班" + ".xlsx";
                        break;
                    }
                }
            }
            if (!File.Exists(ExcelFinalDataFilePath))
            {
                ExcelFileCopy(ExcelFinalDataFilePath);
            }
           //判断总表是否存在并新建  20181010
            if (!File.Exists(ExcelFinalDataFilePathSummary))
            {
                ExcelFileCopySummary(ExcelFinalDataFilePathSummary);
            }
        }
        public void LogicControl_UpdateStationAllDataExcel(object sender)
        {
            //Step1: 设置成不检查（=0）时，每次先根据时间段判断是否需要新建表格            
            JudgeTimeBlock();           
                
            //Step2: 给表格中填写数据
            try
            {
                PieceAllData[] myStationAllData = (PieceAllData[])sender;//转换Sender为myStationAllData
                FileInfo fileinfo = new FileInfo(ExcelFinalDataFilePath);//依路径创建文件流
                bool ShowMessageNoExistFlag = false;
                if (fileinfo != null)
                {
                    using (ExcelPackage package = new ExcelPackage(fileinfo))   //使用ExcelPackage文件流
                    {

                        ExcelWorksheet worksheet = package.Workbook.Worksheets["Lead数据"];  //将ExcelPackage文件流中的WorkBook中的WorkSheet赋值到ExcelWorkSheet对象

                        for (int i = 0; i < myStationAllData.Length; i++)
                        {
                            if (myStationAllData[i].exist == 0)
                            {
                                //写总产量
                                ExcelTotalNum++;
                                worksheet.Cells[9, 3].Value = ExcelTotalNum;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 2].Value = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                                switch (myStationAllData[i].TrayNo)
                                {
                                    case 0:
                                        worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 4].Value = "A";
                                        break;
                                    case 1:
                                        worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 4].Value = "B";
                                        break;
                                    case 2:
                                        worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 4].Value = "C";
                                        break;
                                }
                                if (bIsLaird)
                                {
                                    switch (myStationAllData[i].Level)
                                    {
                                        case 0:
                                            worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 3].Value = "None";
                                            break;
                                        case 1:
                                            worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 3].Value = "OK";
                                            break;
                                        case -1:
                                            ExcelNgNum ++;
                                            worksheet.Cells[11, 3].Value = ExcelNgNum;
                                            worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 3].Value = "NG";
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                else if (bIsSunway)
                                {
                                    switch (myStationAllData[i].Level)
                                    {
                                        case 0:
                                            worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 3].Value = "None";
                                            break;
                                        case 1:
                                            worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 3].Value = "A";
                                            break;
                                        case -1:
                                            worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 3].Value = "B";
                                            ExcelNgNum++;
                                            //worksheet.Cells[11, 3].Value = ExcelNgNum;
                                            break;
                                        case -2:
                                            worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 3].Value = "C";
                                            ExcelNgNum++;
                                            //worksheet.Cells[11, 3].Value = ExcelNgNum;
                                            break;
                                        case -3:
                                            worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 3].Value = "D";
                                            ExcelNgNum++;
                                            //worksheet.Cells[11, 3].Value = ExcelNgNum;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 0].Value = myStationAllData[i].HoleNo;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 1].Value = myStationAllData[i].fai22;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 2].Value = myStationAllData[i].fai130;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 3].Value = myStationAllData[i].fai131;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 4].Value = myStationAllData[i].fai133G1;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 5].Value = myStationAllData[i].fai133G2;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 6].Value = myStationAllData[i].fai133G3;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 7].Value = myStationAllData[i].fai133G4;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 8].Value = myStationAllData[i].fai133G6;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 9].Value = myStationAllData[i].fai161;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 10].Value = myStationAllData[i].fai162;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 11].Value = myStationAllData[i].fai163;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 12].Value = myStationAllData[i].fai165;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 13].Value = myStationAllData[i].fai171;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 14].Value = myStationAllData[i].fai135;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 15].Value = myStationAllData[i].fai136;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 16].Value = myStationAllData[i].fai139;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 17].Value = myStationAllData[i].fai140;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 18].Value = myStationAllData[i].fai151;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 19].Value = myStationAllData[i].fai152;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 20].Value = myStationAllData[i].fai155;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 21].Value = myStationAllData[i].fai156;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 22].Value = myStationAllData[i].fai157;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 23].Value = myStationAllData[i].fai158;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 24].Value = myStationAllData[i].fai160;
                                worksheet.Cells[ExceStartlRowNum + ExcelWriteRowNum, 5 + 25].Value = myStationAllData[i].fai172;

                                ExcelWriteRowNum += 1;
                            }
                        }
                        package.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ExcelWrite" + "\r\n" + "请查看当前是否有Excel文件是否打开；或者D:\\FinalData\\文件夹下的'数据输出格式v6'是否存在" + ex.ToString());
            }
        }
        public void LogicControl_UpdateStationAllDataExcelSummary()
        {
            try
            {
                FileInfo fileinfo = new FileInfo(ExcelFinalDataFilePathSummary);//依路径创建文件流
                bool ShowMessageNoExistFlag = false;
                if (fileinfo != null)
                {
                    using (ExcelPackage package = new ExcelPackage(fileinfo))   //使用ExcelPackage文件流
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets["Lead数据"];  //将ExcelPackage文件流中的WorkBook中的WorkSheet赋值到ExcelWorkSheet对象
                        if (bIsSunway)
                        {
                            worksheet.Cells[10, 4].Value = myFinaldataSummary.myPassratioSummary.ANum;
                            worksheet.Cells[10, 5].Value = myFinaldataSummary.myPassratioSummary.BNum;
                            worksheet.Cells[10, 6].Value = myFinaldataSummary.myPassratioSummary.CNum;
                            worksheet.Cells[10, 7].Value = myFinaldataSummary.myPassratioSummary.DNum;
                            for (int i = 0; i < myFinaldataSummary.myFaiInfoSummary.Length; i++)
                            {
                                worksheet.Cells[10, 8 + i].Value = myFinaldataSummary.myFaiInfoSummary[i].FaiPassNum;
                                worksheet.Cells[11, 8 + i].Value = myFinaldataSummary.myFaiInfoSummary[i].FaiNGNum;
                            }
                        }
                        else
                        {
                            worksheet.Cells[9, 3].Value = myFinaldataSummary.myPassratioSummary.TotalNum;
                            worksheet.Cells[10, 3].Value = myFinaldataSummary.myPassratioSummary.PassNum ;
                            for (int i = 0; i < myFinaldataSummary.myFaiInfoSummary.Length; i++)
                            {
                                worksheet.Cells[10, 4 + i].Value = myFinaldataSummary.myFaiInfoSummary[i].FaiPassNum;
                                worksheet.Cells[11, 4 + i].Value = myFinaldataSummary.myFaiInfoSummary[i].FaiNGNum;
                            }
                        }
                        package.Save();
                    }                
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ExcelWrite出现错误" + "\r\n" + "请查看当前是否有Excel文件是否打开；或者D:\\FinalData\\文件夹下的'数据输出格式v6总表'是否存在"+ ex.ToString());
            }
        }
        //***************************************************************//
            #endregion
        private void CalcPassRatio(int[] FinalResult)
        {
            if (bIsLaird)
            {
                for (int i = 0; i < systemParam.WorkPieceNum; i++)
                {
                    if (FinalResult[i] != 0)
                    {
                        myPassRatio.TotalNum++;
                        if (FinalResult[i] == 1)
                            myPassRatio.PassNum++;
                    }
                    else
                    {
                        myPassRatio.DropNum++;
                    }
                }

                myPassRatio.Ratio = 1.0 * myPassRatio.PassNum / myPassRatio.TotalNum;
            }
            else
            {
                if (bIsSunway)
                {
                    for (int i = 0; i < systemParam.WorkPieceNum; i++)
                    {
                        if (FinalResult[i] != 0 && FinalResult[i] != -4)
                        {
                            myPassRatio.TotalNum++;
                            if (FinalResult[i] == 1)
                                myPassRatio.PassNum++;
                        }

                        switch (FinalResult[i])
                        {
                            case 0:
                                myPassRatio.DropNum++;
                                break;
                            case 1:
                                myPassRatio.ANum++;
                                break;
                            case -1:
                                myPassRatio.BNum++;
                                break;
                            case -2:
                                myPassRatio.CNum++;
                                break;
                            case -3:
                                myPassRatio.DNum++;
                                break;
                            case -4:
                                myPassRatio.ENum++;
                                break;
                            default:
                                break;
                        }
                    }
                    myPassRatio.Ratio = 1.0 * myPassRatio.PassNum / myPassRatio.TotalNum;
                }
            }
        }

        private int[] GenerateFinalResult(int TrayNo)
        {
            int[] temp = new int[systemParam.WorkPieceNum];

            if (bIsLaird)
            {
                temp[0] = FinalJudgeLaird(TrayNo, 0, 0);
                temp[1] = FinalJudgeLaird(TrayNo, 1, 1);
                temp[2] = FinalJudgeLaird(TrayNo, 2, 2);
                temp[3] = FinalJudgeLaird(TrayNo, 3, 3);
            }
            else
            {
                if (bIsSunway)
                {
                    temp[0] = FinalJudgeSunway(TrayNo, 0, 0);
                    temp[1] = FinalJudgeSunway(TrayNo, 1, 1);
                    temp[2] = FinalJudgeSunway(TrayNo, 2, 2);
                    temp[3] = FinalJudgeSunway(TrayNo, 3, 3);
                }
            }
            return temp;
        }

        private int FinalJudgeSunway(int TrayNo, int ccdIdx, int laserIdx)
        {
            if (isEnableImprovePassRatio == false)
            {
                if (CameraCheckResultArrayTray[TrayNo, ccdIdx] == 0)
                    return 0;
                if ((CameraCheckResultArrayTray[TrayNo, ccdIdx] == 1) && (LaserCheckResultArrayTray[TrayNo, laserIdx] == 1))
                    return 1;
                if ((CameraCheckResultArrayTray[TrayNo, ccdIdx] == 1) && (LaserCheckResultArrayTray[TrayNo, laserIdx] == -1))
                    return -1;
                if ((CameraCheckResultArrayTray[TrayNo, ccdIdx] == -1 && LaserCheckResultArrayTray[TrayNo, laserIdx] == -2) ||
                    (CameraCheckResultArrayTray[TrayNo, ccdIdx] == -1 && LaserCheckResultArrayTray[TrayNo, laserIdx] == 1) ||
                    (CameraCheckResultArrayTray[TrayNo, ccdIdx] == 1 && LaserCheckResultArrayTray[TrayNo, laserIdx] == -2))
                    return -2;
                return -3;
            }
            else
            {
                if (CameraCheckResultArrayTray[TrayNo, ccdIdx] == 0)
                    return 0;
                if ((CameraCheckResultArrayTray[TrayNo, ccdIdx] == 1) && (LaserCheckResultArrayTray[TrayNo, laserIdx] == 1))
                    return 1;
                if ((CameraCheckResultArrayTray[TrayNo, ccdIdx] == 1) && (LaserCheckResultArrayTray[TrayNo, laserIdx] == -1))
                    return -1;
                if ((CameraCheckResultArrayTray[TrayNo, ccdIdx] == -1 && LaserCheckResultArrayTray[TrayNo, laserIdx] == -2) ||
                    (CameraCheckResultArrayTray[TrayNo, ccdIdx] == -1 && LaserCheckResultArrayTray[TrayNo, laserIdx] == 1) ||
                    (CameraCheckResultArrayTray[TrayNo, ccdIdx] == 1 && LaserCheckResultArrayTray[TrayNo, laserIdx] == -2))
                    return -2;
                if ((CameraCheckResultArrayTray[TrayNo, ccdIdx] == -3) || (LaserCheckResultArrayTray[TrayNo, laserIdx] == -4))
                    return -4;

                return -3;
            }


        }

        private int FinalJudgeLaird(int TrayNo, int ccdIdx, int laserIdx)
        {
            if ((!logicIgnore[1]) && (!logicIgnore[2]))
            {
                if (CameraCheckResultArrayTray[TrayNo, ccdIdx] == 0)
                    return 0;

                if ((CameraCheckResultArrayTray[TrayNo, ccdIdx] == 1) && (LaserCheckResultArrayTray[TrayNo, laserIdx] == 1))
                    return 1;
                else
                {
                    if ((CameraCheckResultArrayTray[TrayNo, ccdIdx] == 0) && (LaserCheckResultArrayTray[TrayNo, laserIdx] == 0))
                        return 0;
                    else
                        return -1;
                }
            }
            else
            {
                if (!logicIgnore[1])
                    return CameraCheckResultArrayTray[TrayNo, ccdIdx];
                else
                    return LaserCheckResultArrayTray[TrayNo, laserIdx];
            }

        }

        private bool UpdateLaserInfomation(int CircleCount, int ATrayNo)
        {
            if (CircleCount <= 3)
                return true;

            DateTime StartTime = DateTime.Now;
            if (!logicIgnore[2])//屏蔽Laser
            {
                StartTime = DateTime.Now;
                while (true)
                {
                    if (CurStatus != (int)STATUS.AUTO_STATUS)
                        return true;

                    if (LaserCheckDoneArrayTray[ATrayNo, 0] && LaserCheckDoneArrayTray[ATrayNo, 1]
                        && LaserCheckDoneArrayTray[ATrayNo, 2] && LaserCheckDoneArrayTray[ATrayNo, 3])
                    {
                        int[] temparray = new int[systemParam.WorkPieceNum];
                        for (int i = 0; i < systemParam.WorkPieceNum; i++)
                            temparray[i] = LaserCheckResultArrayTray[ATrayNo, i];

                        FinalResultUpdateStru temp = new FinalResultUpdateStru(CircleCount - 2, ATrayNo, temparray);
                        UpdateLaserFinalResult(temp);
                        break;
                    }
                    else
                    {
                        if (!OutTimeCount(StartTime, 3))
                        {
                            WarningSolution("【入料工位】【警报】：更新Laser数据超时");
                            return false;
                        }
                        Thread.Sleep(30);
                    }

                }
            }
            return true;
        }

        private void GetPieceAllDataCCDFAI(int TrayNo, int HoleNum, CCDUpdateStruct CCDFaiInfo)
        {
            if (CCDFaiInfo.exist == 0)
            {
                pieceAllDataArray[TrayNo, HoleNum].TrayNo = TrayNo;
                pieceAllDataArray[TrayNo, HoleNum].HoleNo = HoleNum;
                pieceAllDataArray[TrayNo, HoleNum].exist = CCDFaiInfo.exist;
                pieceAllDataArray[TrayNo, HoleNum].fai22 = CCDFaiInfo.fai22;
                pieceAllDataArray[TrayNo, HoleNum].fai130 = CCDFaiInfo.fai130;
                pieceAllDataArray[TrayNo, HoleNum].fai131 = CCDFaiInfo.fai131;
                pieceAllDataArray[TrayNo, HoleNum].fai133G1 = CCDFaiInfo.fai133G1;
                pieceAllDataArray[TrayNo, HoleNum].fai133G2 = CCDFaiInfo.fai133G2;
                pieceAllDataArray[TrayNo, HoleNum].fai133G3 = CCDFaiInfo.fai133G3;
                pieceAllDataArray[TrayNo, HoleNum].fai133G4 = CCDFaiInfo.fai133G4;
                pieceAllDataArray[TrayNo, HoleNum].fai133G6 = CCDFaiInfo.fai133G6;
                pieceAllDataArray[TrayNo, HoleNum].fai161 = CCDFaiInfo.fai161;
                pieceAllDataArray[TrayNo, HoleNum].fai162 = CCDFaiInfo.fai162;
                pieceAllDataArray[TrayNo, HoleNum].fai163 = CCDFaiInfo.fai163;
                pieceAllDataArray[TrayNo, HoleNum].fai165 = CCDFaiInfo.fai165;
                pieceAllDataArray[TrayNo, HoleNum].fai171 = CCDFaiInfo.fai171;
            }
            else
            {
                pieceAllDataArray[TrayNo, HoleNum].TrayNo = TrayNo;
                pieceAllDataArray[TrayNo, HoleNum].HoleNo = HoleNum;
                pieceAllDataArray[TrayNo, HoleNum].exist = CCDFaiInfo.exist;
                pieceAllDataArray[TrayNo, HoleNum].fai22 = 0;
                pieceAllDataArray[TrayNo, HoleNum].fai130 = 0;
                pieceAllDataArray[TrayNo, HoleNum].fai131 = 0;
                pieceAllDataArray[TrayNo, HoleNum].fai133G1 = 0;
                pieceAllDataArray[TrayNo, HoleNum].fai133G2 = 0;
                pieceAllDataArray[TrayNo, HoleNum].fai133G3 = 0;
                pieceAllDataArray[TrayNo, HoleNum].fai133G4 = 0;
                pieceAllDataArray[TrayNo, HoleNum].fai133G6 = 0;
                pieceAllDataArray[TrayNo, HoleNum].fai161 = 0;
                pieceAllDataArray[TrayNo, HoleNum].fai162 = 0;
                pieceAllDataArray[TrayNo, HoleNum].fai163 = 0;
                pieceAllDataArray[TrayNo, HoleNum].fai165 = 0;
                pieceAllDataArray[TrayNo, HoleNum].fai171 = 0;
            }
        }

        private void GetPieceAllDataLaserFAI(int TrayNo, int HoleNum, LaserFAIUpdateStruct LaserFaiInfo)
        {
            pieceAllDataArray[TrayNo, HoleNum].fai135 = LaserFaiInfo.fai135;
            pieceAllDataArray[TrayNo, HoleNum].fai136 = LaserFaiInfo.fai136;
            pieceAllDataArray[TrayNo, HoleNum].fai139 = LaserFaiInfo.fai139;
            pieceAllDataArray[TrayNo, HoleNum].fai140 = LaserFaiInfo.fai140;
            pieceAllDataArray[TrayNo, HoleNum].fai151 = LaserFaiInfo.fai151;
            pieceAllDataArray[TrayNo, HoleNum].fai152 = LaserFaiInfo.fai152;
            pieceAllDataArray[TrayNo, HoleNum].fai155 = LaserFaiInfo.fai155;
            pieceAllDataArray[TrayNo, HoleNum].fai156 = LaserFaiInfo.fai156;
            pieceAllDataArray[TrayNo, HoleNum].fai157 = LaserFaiInfo.fai157;
            pieceAllDataArray[TrayNo, HoleNum].fai158 = LaserFaiInfo.fai158;
            pieceAllDataArray[TrayNo, HoleNum].fai160 = LaserFaiInfo.fai160;
            pieceAllDataArray[TrayNo, HoleNum].fai172 = LaserFaiInfo.fai172;
        }

        public bool StretchOutAllCylinder()
        {
            IOControl.WriteDO((int)DONAME.Do_EnterStirStretchControl, false); IOControl.WriteDO((int)DONAME.Do_EnterStirRetractControl, false);
            IOControl.WriteDO((int)DONAME.Do_EnterPushUpRetractControl, false); IOControl.WriteDO((int)DONAME.Do_EnterPushUpStretchControl, false);
            IOControl.WriteDO((int)DONAME.Do_EnterMoveRetractControl, false); IOControl.WriteDO((int)DONAME.Do_EnterMoveStretchControl, false);

            bool tempFinish = false;
            DateTime StartTime = DateTime.Now;
            int StretchStep = 0;
            while (!tempFinish)
            {
                switch (StretchStep)
                {
                    case 0:
                        WriteLog("【入料工位】：入料工位顶升气缸&拨动气缸伸出开始");
                        if (CurInfo.Di[(int)DINAME.Di_EnterPushUp1StretchBit] && (!CurInfo.Di[(int)DINAME.Di_EnterPushUp1RetractBit]) &&
                            CurInfo.Di[(int)DINAME.Di_EnterPushUp2StretchBit] && (!CurInfo.Di[(int)DINAME.Di_EnterPushUp2RetractBit]) &&
                            CurInfo.Di[(int)DINAME.Di_EnterStirStretchBit] && (!CurInfo.Di[(int)DINAME.Di_EnterStirRetractBit]))
                        {
                            StretchStep = 1;
                        }
                        else
                        {
                            IOControl.WriteDO((int)DONAME.Do_EnterPushUpStretchControl, true);
                            IOControl.WriteDO((int)DONAME.Do_EnterPushUpRetractControl, false);
                            IOControl.WriteDO((int)DONAME.Do_EnterStirStretchControl, true);
                            IOControl.WriteDO((int)DONAME.Do_EnterStirRetractControl, false);
                            StartTime = DateTime.Now;
                            while (true)
                            {
                                if (CurInfo.Di[(int)DINAME.Di_EnterPushUp1StretchBit] && (!CurInfo.Di[(int)DINAME.Di_EnterPushUp1RetractBit]) &&
                                    CurInfo.Di[(int)DINAME.Di_EnterPushUp2StretchBit] && (!CurInfo.Di[(int)DINAME.Di_EnterPushUp2RetractBit]) &&
                                    CurInfo.Di[(int)DINAME.Di_EnterStirStretchBit] && (!CurInfo.Di[(int)DINAME.Di_EnterStirRetractBit]))
                                {
                                    WriteLog("【入料工位】：入料工位顶升气缸&拨动气缸伸出完成");
                                    StretchStep = 1;
                                    break;
                                }
                                else
                                {
                                    if (!OutTimeCount(StartTime, 10))
                                    {
                                        WarningSolution("【入料工位】【报警】：【48】入料工位顶升气缸&拨动气缸伸出异常：I10.8,I10.9,I10.10,I10.11,I10.14,I10.15");
                                        return false;
                                    }
                                    Thread.Sleep(30);
                                }
                            }
                        }
                        break;
                    case 1:
                        if ((!CurInfo.Di[(int)DINAME.Di_EnterMoveRetractBit]) || CurInfo.Di[(int)DINAME.Di_EnterMoveStretchBit])
                        {
                            if (MoveCylinderRetract() == false)
                                return false;
                        }
                        tempFinish = true;
                        //注意注释延时语句
                        Thread.Sleep(100);
                        break;
                }
                Thread.Sleep(30);
            }
            WriteLog("【入料工位】：入料工位伸出载具完成");
            return true;
        }

        public bool RetractAllCylinder()
        {
            IOControl.WriteDO((int)DONAME.Do_EnterStirStretchControl, false); IOControl.WriteDO((int)DONAME.Do_EnterStirRetractControl, false);
            IOControl.WriteDO((int)DONAME.Do_EnterPushUpRetractControl, false); IOControl.WriteDO((int)DONAME.Do_EnterPushUpStretchControl, false);
            IOControl.WriteDO((int)DONAME.Do_EnterMoveRetractControl, false); IOControl.WriteDO((int)DONAME.Do_EnterMoveStretchControl, false);

            bool tempFinish = false;

            System.DateTime StartTime = DateTime.Now;
            int RetractStep = 0;
            while (!tempFinish)
            {
                switch (RetractStep)
                {
                    case 0:
                        if (CurInfo.Di[(int)DINAME.Di_EnterMoveStretchBit] && (!CurInfo.Di[(int)DINAME.Di_EnterMoveRetractBit]))
                        {
                            RetractStep = 1;
                        }
                        else
                        {
                            if (MoveCylinderStretch())
                            {
                                RetractStep = 1;
                                Thread.Sleep(200);
                            }
                            else
                                return false;
                        }
                        break;
                    case 1:
                        if (CurInfo.Di[(int)DINAME.Di_EnterPushUp1RetractBit] && (!CurInfo.Di[(int)DINAME.Di_EnterPushUp1StretchBit]) &&
                            CurInfo.Di[(int)DINAME.Di_EnterPushUp2RetractBit] && (!CurInfo.Di[(int)DINAME.Di_EnterPushUp2StretchBit]) &&
                            CurInfo.Di[(int)DINAME.Di_EnterStirRetractBit] && (!CurInfo.Di[(int)DINAME.Di_EnterStirStretchBit]))
                        {
                            tempFinish = true;
                        }
                        else
                        {
                            IOControl.WriteDO((int)DONAME.Do_EnterPushUpRetractControl, true);
                            IOControl.WriteDO((int)DONAME.Do_EnterPushUpStretchControl, false);
                            IOControl.WriteDO((int)DONAME.Do_EnterStirRetractControl, true);
                            IOControl.WriteDO((int)DONAME.Do_EnterStirStretchControl, false);
                            StartTime = DateTime.Now;
                            while (true)
                            {
                                if (CurInfo.Di[(int)DINAME.Di_EnterPushUp1RetractBit] && (!CurInfo.Di[(int)DINAME.Di_EnterPushUp1StretchBit]) &&
                                    CurInfo.Di[(int)DINAME.Di_EnterPushUp2RetractBit] && (!CurInfo.Di[(int)DINAME.Di_EnterPushUp2StretchBit]) &&
                                    CurInfo.Di[(int)DINAME.Di_EnterStirRetractBit] && (!CurInfo.Di[(int)DINAME.Di_EnterStirStretchBit]))
                                {
                                    tempFinish = true;
                                    break;
                                }
                                else
                                {
                                    if (!OutTimeCount(StartTime, 10))
                                    {
                                        WarningSolution("【入料工位】【报警】:【49】顶升气缸/拨动气缸缩回异常：I10.8,I10.9,I10.10,I10.11,I10.14,I10.15");
                                        return false;
                                    }
                                    Thread.Sleep(30);
                                }
                            }
                        }
                        break;
                }
                Thread.Sleep(30);
            }
            return true;
        }

        private bool MoveCylinderStretch()
        {
            WriteLog("【入料工位】：移动气缸伸出开始");
            if (WaitPiston2Cmd2FeedbackDone((int)DONAME.Do_EnterMoveStretchControl, (int)DONAME.Do_EnterMoveRetractControl, (int)DINAME.Di_EnterMoveStretchBit, (int)DINAME.Di_EnterMoveRetractBit))
            {
                WriteLog("【入料工位】：移动气缸伸出结束");
                return true;
            }
            else
            {
                WarningSolution("【52】移动气缸伸出异常：I10.12，I10.13");
                return false;
            }
        }

        private bool MoveCylinderRetract()
        {
            WriteLog("【入料工位】：移动气缸缩回开始");
            if (WaitPiston2Cmd2FeedbackDone((int)DONAME.Do_EnterMoveRetractControl, (int)DONAME.Do_EnterMoveStretchControl, (int)DINAME.Di_EnterMoveRetractBit, (int)DINAME.Di_EnterMoveStretchBit))
            {
                WriteLog("【入料工位】：移动气缸缩回结束");
                return true;
            }
            else
            {
                WarningSolution("【53】移动气缸缩回异常：I10.12，I10.13");
                return false;
            }
        }

        //返回-1代表NG，返回1代表OK
        private int CameraJudge(CCDUpdateStruct CameraUpdateStru)
        {
            if (CameraUpdateStru.exist != 0)
                return 0;

            double[] ccdFaiResult = new double[13];

            ccdFaiResult[0] = JudgeFai(CameraUpdateStru.fai22, ThrParam.thrInfo[0].UpLimit, ThrParam.thrInfo[0].DownLimit);
            ccdFaiResult[1] = JudgeFai(CameraUpdateStru.fai130, ThrParam.thrInfo[1].UpLimit, ThrParam.thrInfo[1].DownLimit);
            ccdFaiResult[2] = JudgeFai(CameraUpdateStru.fai131, ThrParam.thrInfo[2].UpLimit, ThrParam.thrInfo[2].DownLimit);
            ccdFaiResult[3] = JudgeFai(CameraUpdateStru.fai133G1, ThrParam.thrInfo[3].UpLimit, ThrParam.thrInfo[3].DownLimit);
            ccdFaiResult[4] = JudgeFai(CameraUpdateStru.fai133G2, ThrParam.thrInfo[4].UpLimit, ThrParam.thrInfo[4].DownLimit);
            ccdFaiResult[5] = JudgeFai(CameraUpdateStru.fai133G3, ThrParam.thrInfo[5].UpLimit, ThrParam.thrInfo[5].DownLimit);
            ccdFaiResult[6] = JudgeFai(CameraUpdateStru.fai133G4, ThrParam.thrInfo[6].UpLimit, ThrParam.thrInfo[6].DownLimit);
            ccdFaiResult[7] = JudgeFai(CameraUpdateStru.fai133G6, ThrParam.thrInfo[7].UpLimit, ThrParam.thrInfo[7].DownLimit);
            ccdFaiResult[8] = JudgeFai(CameraUpdateStru.fai161, ThrParam.thrInfo[8].UpLimit, ThrParam.thrInfo[8].DownLimit);
            ccdFaiResult[9] = JudgeFai(CameraUpdateStru.fai162, ThrParam.thrInfo[9].UpLimit, ThrParam.thrInfo[9].DownLimit);
            ccdFaiResult[10] = JudgeFai(CameraUpdateStru.fai163, ThrParam.thrInfo[10].UpLimit, ThrParam.thrInfo[10].DownLimit);
            ccdFaiResult[11] = JudgeFai(CameraUpdateStru.fai165, ThrParam.thrInfo[11].UpLimit, ThrParam.thrInfo[11].DownLimit);
            ccdFaiResult[12] = JudgeFai(CameraUpdateStru.fai171, ThrParam.thrInfo[12].UpLimit, ThrParam.thrInfo[12].DownLimit);
            ccdFaiResult[12] = ccdFaiResult[12] / ThrParam.thrInfo[12].DownLimit;


            if (bIsLaird)
            {
                for (int i = 0; i < ccdFaiResult.Length; i++)
                {
                    if (ccdFaiResult[i] != 0)
                        return -1;
                }
                return 1;
            }
            else
            {
                if (isEnableImprovePassRatio == false)
                {
                    double tempResult = FindMax(ccdFaiResult);
                    if (tempResult > 0 && tempResult <= 0.05)
                        return -1;
                    if (tempResult > 0.05)
                        return -2;
                    return 1;
                }
                else
                {
                    double tempResult = FindMax(ccdFaiResult);
                    if (tempResult > 0 && tempResult <= 0.05)
                        return -1;
                    if (tempResult > 0.05 && tempResult < PassRatioMaxMargin)
                        return -2;
                    if (tempResult >= PassRatioMaxMargin)
                        return -3;
                    return 1;
                }
            }
        }

        private int LaserJudge(LaserFAIUpdateStruct LaserUpdateStru)
        {
            double[] laserFaiResult = new double[12];

            laserFaiResult[0] = JudgeFai(LaserUpdateStru.fai135, ThrParam.thrInfo[13].UpLimit, ThrParam.thrInfo[13].DownLimit);
            laserFaiResult[1] = JudgeFai(LaserUpdateStru.fai136, ThrParam.thrInfo[14].UpLimit, ThrParam.thrInfo[14].DownLimit);
            laserFaiResult[2] = JudgeFai(LaserUpdateStru.fai139, ThrParam.thrInfo[15].UpLimit, ThrParam.thrInfo[15].DownLimit);
            laserFaiResult[3] = JudgeFai(LaserUpdateStru.fai140, ThrParam.thrInfo[16].UpLimit, ThrParam.thrInfo[16].DownLimit);
            laserFaiResult[4] = JudgeFai(LaserUpdateStru.fai151, ThrParam.thrInfo[17].UpLimit, ThrParam.thrInfo[17].DownLimit);
            laserFaiResult[5] = JudgeFai(LaserUpdateStru.fai152, ThrParam.thrInfo[18].UpLimit, ThrParam.thrInfo[18].DownLimit);
            laserFaiResult[6] = JudgeFai(LaserUpdateStru.fai155, ThrParam.thrInfo[19].UpLimit, ThrParam.thrInfo[19].DownLimit);
            laserFaiResult[7] = JudgeFai(LaserUpdateStru.fai156, ThrParam.thrInfo[20].UpLimit, ThrParam.thrInfo[20].DownLimit);
            laserFaiResult[8] = JudgeFai(LaserUpdateStru.fai157, ThrParam.thrInfo[21].UpLimit, ThrParam.thrInfo[21].DownLimit);
            laserFaiResult[9] = JudgeFai(LaserUpdateStru.fai158, ThrParam.thrInfo[22].UpLimit, ThrParam.thrInfo[22].DownLimit);
            laserFaiResult[10] = JudgeFai(LaserUpdateStru.fai160, ThrParam.thrInfo[23].UpLimit, ThrParam.thrInfo[23].DownLimit);
            laserFaiResult[11] = JudgeFai(LaserUpdateStru.fai172, ThrParam.thrInfo[24].UpLimit, ThrParam.thrInfo[24].DownLimit);

            if (bIsLaird)
            {
                for (int i = 0; i < laserFaiResult.Length; i++)
                {
                    if (laserFaiResult[i] != 0)
                        return -1;
                }
                return 1;
            }
            else
            {
                if (isEnableImprovePassRatio == false)
                {
                    double tempResult = FindMax(laserFaiResult);
                    if (tempResult > 0 && tempResult <= 0.02)
                        return -1;
                    if (tempResult > 0.02 && tempResult <= 0.05)
                        return -2;
                    if (tempResult > 0.05)
                        return -3;
                    return 1;
                }
                else
                {
                    double tempResult = FindMax(laserFaiResult);
                    if (tempResult > 0 && tempResult <= 0.02)
                        return -1;
                    if (tempResult > 0.02 && tempResult <= 0.05)
                        return -2;
                    if (tempResult > 0.05 && tempResult <PassRatioMaxMargin)
                        return -3;
                    if (tempResult >= PassRatioMaxMargin)
                        return -4;
                    return 1;
                }


            }
        }

        //返回值为0代表OK，返回值不为0代表NG
        public double JudgeFai(double value, double tolUp, double tolDown)
        {
            if (value <= tolUp && value >= tolDown)
                return 0;
            else
            {
                if (value < tolDown)
                    return tolDown - value;
                else
                    return value - tolUp;
            }
        }

        private double FindMax(double[] myarray)
        {
            double tempmax = myarray[0];
            for (int i = 0; i < myarray.Length; i++)
            {
                if (myarray[i] > tempmax)
                    tempmax = myarray[i];
            }
            return tempmax;
        }


        public bool OutTimeCount(DateTime StartTime, int OutTime)
        {
            TimeSpan span = DateTime.Now - StartTime;
            return span.Seconds < OutTime;
        }

        public void HomeMove(int Id)
        {
            if (Id <= 5)
                adlink.HomeMove(logicConfig.PulseAxis[Id]);
            else
                adlink.HomeMove(logicConfig.ECATAxis[Id - 6]);

        }

        public void HomeMoveZ(int Id, bool safetysignal)
        {
            if (safetysignal == false)
            {
                MessageBox.Show("请确认圆盘是否处于安全位置");
                return;
            }
            adlink.HomeMoveZ(logicConfig.PulseAxis[Id]);
        }

        public void ClearAxisAlarm(int Id)
        {
            if (Id <= 5)
                adlink.ClearAlarm(logicConfig.PulseAxis[Id]);
            else
                adlink.ClearAlarm(logicConfig.ECATAxis[Id - 6]);
        }

        public void ServoCtrl(int Id, bool value)
        {
            if (value)
            {
                adlink.Servo_On(Id);
            }
            else
            {
                adlink.Servo_Off(Id);
            }
        }

        public void StartJog(Axis axis, int dir)
        {
            adlink.JogMoveStart(axis, dir);
        }

        public void StopJog(Axis axis)
        {
            adlink.JogMoveStop(axis);
        }

        public void RelMove(Axis axis, double Pos)
        {
            int err = 0;
            adlink.P2PMove(axis, Pos, false, ref err);
        }

        public void AbsMove(Axis axis, double Pos)
        {
            int err = 0;
            adlink.P2PMove(axis, Pos, true, ref err);
        }

        public void MotionStop()
        {
            for (int i = 0; i < logicConfig.PulseAxis.Length; i++)
            {
                adlink.EmgStop(logicConfig.PulseAxis[i]);
            }

            for (int i = 0; i < logicConfig.ECATAxis.Length; i++)
            {
                adlink.EmgStop(logicConfig.ECATAxis[i]);
            }
        }

        public void WriteLog(string message)
        {
            lock (locker)
            {
                string time = ((DateTime.Now.Hour) < 10 ? ("0" + DateTime.Now.Hour.ToString()) : DateTime.Now.Hour.ToString()) + ":" +
                    ((DateTime.Now.Minute) < 10 ? ("0" + DateTime.Now.Minute.ToString()) : DateTime.Now.Minute.ToString()) + ":" +
                    ((DateTime.Now.Second) < 10 ? ("0" + DateTime.Now.Second.ToString()) : DateTime.Now.Second.ToString()) + ":" +
                    DateTime.Now.Millisecond.ToString("000");

                MoveLogsw.WriteLine(time + " : " + message);
            }
        }

        public void WriteCCDMoveLog(string message)
        {
            lock (locker)
            {
                string time = ((DateTime.Now.Hour) < 10 ? ("0" + DateTime.Now.Hour.ToString()) : DateTime.Now.Hour.ToString()) + ":" +
                    ((DateTime.Now.Minute) < 10 ? ("0" + DateTime.Now.Minute.ToString()) : DateTime.Now.Minute.ToString()) + ":" +
                    ((DateTime.Now.Second) < 10 ? ("0" + DateTime.Now.Second.ToString()) : DateTime.Now.Second.ToString()) + ":" +
                    DateTime.Now.Millisecond.ToString("000");

                CCDMoveLogsw.WriteLine(time + " " + message);
            }
        }

        public void WriteDownTime(string status, string starttime, string finishtime, string timespan, string errstr)
        {
            lock (locker)
            {
                DownTimeLogsw.WriteLine(status + "," + starttime + "," + finishtime + "," + timespan + "," + errstr);
            }
        }


        public void WriteCTLog(string message)
        {
            lock (locker)
            {
                string time = ((DateTime.Now.Hour) < 10 ? ("0" + DateTime.Now.Hour.ToString()) : DateTime.Now.Hour.ToString()) + ":" +
                    ((DateTime.Now.Minute) < 10 ? ("0" + DateTime.Now.Minute.ToString()) : DateTime.Now.Minute.ToString()) + ":" +
                    ((DateTime.Now.Second) < 10 ? ("0" + DateTime.Now.Second.ToString()) : DateTime.Now.Second.ToString()) + ":" +
                    DateTime.Now.Millisecond.ToString("000");

                CTLogsw.WriteLine(time + " : " + message);
            }
        }

        public void SaveAxisInfo(object sender)
        {
            logicConfig = sender as LogicConfig;
            XmlSerializerHelper.WriteXML((object)logicConfig, pathLogicConfig, typeof(LogicConfig));
        }

        private void DaskDispose()
        {
            Dask7432.ReleaseCard(0);
        }

        public bool KeyenceConnect()
        {
            try
            {
                DatagramEncoder AorU;
                AorU = new DatagramEncoder(DatagramEncoder.EncodingMethod.ASCII);

                client = new CSClient(AorU);
                client.ReceivedDatagram += new CSNetEvent(RecvData);
                client.DisConnectedServer += new CSNetEvent(ClientClose);
                client.ConnectedServer += new CSNetEvent(ClientConn);
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(ThreadAbortException))
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }

            }

            #region Connect To CCD
            if (client.IsConnected == true)
                Tcp_DisConnect();

            while (client.IsConnected == true)
            {
                Thread.Sleep(30);
            }

            Thread CCDConnect = new Thread(new ThreadStart(Tcp_Connect), 1024);
            CCDConnect.IsBackground = true;
            CCDConnect.Start();

            DateTime StartTime = DateTime.Now;
            while (tcp_enable == false)
            {
                if (!OutTimeCount(StartTime, 3))
                {
                    WarningSolution("【54】CCD通讯失败");
                    return false;
                }
                Thread.Sleep(30);
            }

            UpdateWaringLog.Invoke((object)("CCD通讯 Success"));
            #endregion

            return true;
        }

        private bool TcpIpRecvStatus = false;

        #region added by lei.c CCD检测相关数据
        public bool CCDChecking = false;

        public int RecvCount = 0;
        public int RecvClassify = 0;
        #endregion

        private void RecvData(object sender, CSEventArgs e)
        {
            string splitStr = ",";
            try
            {
                if (TcpIpRecvStatus)
                    return;

                TcpIpRecvStatus = true;

                tcp_Recive = e.Client.Datagram;
                #region CCD数据特殊情况处理
                if (tcp_Recive.Substring(0, 2) == "ER")
                {
                    MessageBox.Show("CCD数据发生错误");
                    TcpIpRecvStatus = false;
                    return;
                }

                if (tcp_Recive.Substring(0, 2) == "RS")
                {
                    UpdateWaringLog.Invoke((object)"CCD数据复位成功");
                    TcpIpRecvStatus = false;
                    return;
                }

                if (tcp_Recive.Substring(0, 3) == "EXW")
                {
                    isEXWBack = true;
                    TcpIpRecvStatus = false;
                    return;
                }
                #endregion
                string[] splitString = tcp_Recive.Split(splitStr.ToCharArray());
                if (splitString.Length != 206)
                {
                    MessageBox.Show("CCD数据长度不符");
                    TcpIpRecvStatus = false;
                    return;
                }

                if (CCDChecking)
                {
                    RecvClassify++;
                    if (RecvClassify > 3)
                        RecvClassify = 1;

                    switch (RecvClassify)
                    {
                        case 1:
                            WriteLog("CCD数据收集0开始");
                            CCDRecvStatusList.Add(new bool[3]);
                            CCDRawDataList.Add(new double[205]);

                            while (CCDRawDataList.Count < RecvCount + 1)
                                Thread.Sleep(10);

                            double.TryParse(splitString[1], out CCDRawDataList[RecvCount][0]);
                            double.TryParse(splitString[2], out CCDRawDataList[RecvCount][1]);
                            double.TryParse(splitString[3], out CCDRawDataList[RecvCount][2]);
                            double.TryParse(splitString[4], out CCDRawDataList[RecvCount][3]);
                            double.TryParse(splitString[5], out CCDRawDataList[RecvCount][4]);
                            double.TryParse(splitString[6], out CCDRawDataList[RecvCount][5]);
                            double.TryParse(splitString[8], out CCDRawDataList[RecvCount][7]);
                            double.TryParse(splitString[9], out CCDRawDataList[RecvCount][8]);
                            double.TryParse(splitString[191], out CCDRawDataList[RecvCount][190]);
                            double.TryParse(splitString[192], out CCDRawDataList[RecvCount][191]);
                            double.TryParse(splitString[205], out CCDRawDataList[RecvCount][204]);

                            for (int i = 182; i < 188; i++)
                                double.TryParse(splitString[i], out CCDRawDataList[RecvCount][i - 1]);

                            for (int i = 12; i < 52; i++)
                                double.TryParse(splitString[i], out CCDRawDataList[RecvCount][i - 1]);

                            for (int j = 92; j < 172; j++)
                                double.TryParse(splitString[j], out CCDRawDataList[RecvCount][j - 1]);

                            for (int k = 195; k < 203; k++)
                                double.TryParse(splitString[k], out CCDRawDataList[RecvCount][k - 1]);

                            WriteLog("CCD数据收集0完成");
                            CCDRecvStatusList[RecvCount][0] = true;
                            break;
                        case 2:
                            double.TryParse(splitString[172], out CCDRawDataList[RecvCount][171]);
                            double.TryParse(splitString[173], out CCDRawDataList[RecvCount][172]);
                            double.TryParse(splitString[174], out CCDRawDataList[RecvCount][173]);
                            double.TryParse(splitString[175], out CCDRawDataList[RecvCount][174]);

                            CCDRecvStatusList[RecvCount][1] = true;
                            WriteLog("CCD数据收集1完成");
                            break;
                        case 3:
                            double.TryParse(splitString[7], out CCDRawDataList[RecvCount][6]);
                            double.TryParse(splitString[10], out CCDRawDataList[RecvCount][9]);
                            double.TryParse(splitString[11], out CCDRawDataList[RecvCount][10]);
                            double.TryParse(splitString[193], out CCDRawDataList[RecvCount][192]);
                            double.TryParse(splitString[194], out CCDRawDataList[RecvCount][193]);
                            double.TryParse(splitString[203], out CCDRawDataList[RecvCount][202]);
                            double.TryParse(splitString[204], out CCDRawDataList[RecvCount][203]);

                            for (int i = 52; i < 92; i++)
                                double.TryParse(splitString[i], out CCDRawDataList[RecvCount][i - 1]);
                            for (int i = 176; i < 182; i++)
                                double.TryParse(splitString[i], out CCDRawDataList[RecvCount][i - 1]);
                            for (int i = 188; i < 191; i++)
                                double.TryParse(splitString[i], out CCDRawDataList[RecvCount][i - 1]);

                            WriteLog("CCD数据收集2完成");
                            CCDRecvStatusList[RecvCount][2] = true;
                            RecvCount++;//只有在CCD检查的时候，才要开启RecvCount计数，RecvCount代表这是第几个完成CCD检查的元件，从0开始
                            break;
                    }
                }
                TcpIpRecvStatus = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("CCD通讯发生故障:"+ex.ToString());
            }
        }


        private void ClientConn(object sender, CSEventArgs e)
        {
            tcp_enable = true;
        }

        private void ClientClose(object sender, CSEventArgs e)
        {
            tcp_enable = false;
        }

        public bool TcpSendMsg(string Msg)
        {
            if (tcp_enable)
            {
                if (client != null)
                {
                    client.Send(Msg);
                    return true;
                }
                else
                    return false;
            }
            else
            {
                MessageBox.Show("Keyence CCD连接异常");
                return false;
            }
        }

        public void Tcp_DisConnect()
        {
            if (client != null)
            {
                client.Close();
            }
        }

        private void EtherCAT_DisConnect()
        {
            int ret = APS168.APS_stop_field_bus(1, 0); // stop field bus communication
        }

        public void Tcp_Connect()
        {
            if (client != null)
            {
                client.Connect("192.168.1.10", 8500);//连接  noted by lei.c 修正目标IP地址
            }
        }

        public void SaveSysInfo(object sender)
        {
            XmlSerializerHelper.WriteXML((object)sender, pathSystemParam, typeof(SystemParam));
        }

        public void YellowLight()
        {
            IOControl.WriteDO((int)DONAME.Do_LightRed, false);
            IOControl.WriteDO((int)DONAME.Do_LightGreen, false);
            IOControl.WriteDO((int)DONAME.Do_LightYellow, true);
        }

        public void GreenLight()
        {
            IOControl.WriteDO((int)DONAME.Do_LightRed, false);
            IOControl.WriteDO((int)DONAME.Do_LightGreen, true);
            IOControl.WriteDO((int)DONAME.Do_LightYellow, false);
        }

        public void RedLight()
        {
            IOControl.WriteDO((int)DONAME.Do_LightRed, true);
            IOControl.WriteDO((int)DONAME.Do_LightGreen, false);
            IOControl.WriteDO((int)DONAME.Do_LightYellow, false);
        }

        private bool InitAxisInfo(ref Axis[] axis)
        {
            #region Test Code
            for (int i = 0; i < axis.Length; i++)
            {
                axis[i].AxisId = (short)i;
                axis[i].Rate = 1000;
                axis[i].Band = 0;
                axis[i].HomeVel = 20000;
                axis[i].HomeAcc = 200000;
                axis[i].HomeVO = 0;
                axis[i].MoveVel = 20000;
                axis[i].MoveAcc = 200000;
                axis[i].MoveDec = 200000;
                axis[i].JogVel = 10000;
                axis[i].JogAcc = 200000;
                axis[i].JogDec = 200000;
            }
            axis[5].Rate = 1;
            axis[5].HomeVel = 10000;
            axis[5].MoveVel = 70000;
            axis[5].MoveAcc = 120000;
            axis[5].MoveDec = 120000;

            axis[0].MoveVel = 120000;
            axis[0].MoveAcc = 1200000;
            axis[0].MoveDec = 1200000;
            axis[1].MoveVel = 120000;
            axis[1].MoveAcc = 1200000;
            axis[1].MoveDec = 1200000;
            #endregion
            return true;
        }

        //实际运行时读取LogicConfig的代码
        private bool InitLogicConfig(ref LogicConfig logicConfig, ref string errString)
        {
            if (!File.Exists(pathLogicConfig))
            {
                logicConfig.PulseAxis = new Axis[8];
                for (int j = 0; j < logicConfig.PulseAxis.Length; j++)
                {
                    logicConfig.PulseAxis[j] = new Axis();
                    logicConfig.PulseAxis[j].AxisId = (short)j;
                    logicConfig.PulseAxis[j].Rate = 1000;
                    logicConfig.PulseAxis[j].Band = 0;
                    logicConfig.PulseAxis[j].HomeVel = 20000;
                    logicConfig.PulseAxis[j].HomeAcc = 200000;
                    logicConfig.PulseAxis[j].HomeVO = 0;
                    logicConfig.PulseAxis[j].MoveVel = 20000;
                    logicConfig.PulseAxis[j].MoveAcc = 200000;
                    logicConfig.PulseAxis[j].MoveDec = 200000;
                    logicConfig.PulseAxis[j].JogVel = 10000;
                    logicConfig.PulseAxis[j].JogAcc = 200000;
                    logicConfig.PulseAxis[j].JogDec = 200000;
                }

                logicConfig.ECATAxis = new Axis[8];
                for (int j = 0; j < logicConfig.ECATAxis.Length; j++)
                {
                    logicConfig.ECATAxis[j] = new Axis();
                    logicConfig.ECATAxis[j].AxisId = (short)j;
                    logicConfig.ECATAxis[j].Rate = 1000;
                    logicConfig.ECATAxis[j].Band = 0;
                    logicConfig.ECATAxis[j].HomeVel = 20000;
                    logicConfig.ECATAxis[j].HomeAcc = 200000;
                    logicConfig.ECATAxis[j].HomeVO = 0;
                    logicConfig.ECATAxis[j].MoveVel = 20000;
                    logicConfig.ECATAxis[j].MoveAcc = 200000;
                    logicConfig.ECATAxis[j].MoveDec = 200000;
                    logicConfig.ECATAxis[j].JogVel = 10000;
                    logicConfig.ECATAxis[j].JogAcc = 200000;
                    logicConfig.ECATAxis[j].JogDec = 200000;
                }

                bool result = XmlSerializerHelper.WriteXML((object)logicConfig, pathLogicConfig, typeof(LogicConfig));
                if (!result)
                {
                    errString = "LogicConfig配置文件丢失";
                    return false;
                }
            }
            else
            {
                bool bFlag = false;
                logicConfig = XmlSerializerHelper.ReadXML(pathLogicConfig, typeof(LogicConfig), out bFlag) as LogicConfig;
                if (null == logicConfig)
                {
                    WarningSolution("【55】LogicConfig配置文件读取失败");
                    errString = "LogicConfig配置文件读取失败";
                    return false;
                }
                else
                {
                    UpdateWaringLog.Invoke((object)("LogicConfig配置文件读取成功"));
                }
            }
            return true;
        }


        private bool InitCCDMotionPos(ref MotionPos motionPos, ref string errString)
        {
            motionPos = new MotionPos(12);
            if (!File.Exists(pathMotionPos))
            {
                for (int j = 0; j < motionPos.posInfo.Length; j++)
                {
                    motionPos.posInfo[j].PosName = "点位" + j.ToString();
                    motionPos.posInfo[j].XPos = 0;
                    motionPos.posInfo[j].YPos = 0;
                }
                bool result = XmlSerializerHelper.WriteXML((object)motionPos, pathMotionPos, typeof(MotionPos));
                if (!result)
                {
                    errString = "CCD运动点位配置文件丢失";
                    return false;
                }
            }
            else
            {
                bool bFlag = false;
                motionPos = (MotionPos)XmlSerializerHelper.ReadXML(pathMotionPos, typeof(MotionPos), out bFlag);
                UpdateWaringLog.Invoke((object)("CCD运动点位配置文件读取成功"));
            }

            return true;
        }

        private bool InitLoadGantryMotionPos(ref MotionPos motionPos, ref string errString)
        {
            motionPos = new MotionPos(18);
            if (!File.Exists(pathLoadGantryMotionPos))
            {
                for (int j = 0; j < motionPos.posInfo.Length; j++)
                {
                    motionPos.posInfo[j].PosName = "点位" + j.ToString();
                    motionPos.posInfo[j].XPos = 0;
                    motionPos.posInfo[j].YPos = 0;
                }
                bool result = XmlSerializerHelper.WriteXML((object)motionPos, pathLoadGantryMotionPos, typeof(MotionPos));
                if (!result)
                {
                    errString = "龙门上料运动点位配置文件丢失";
                    return false;
                }
            }
            else
            {
                bool bFlag = false;
                motionPos = (MotionPos)XmlSerializerHelper.ReadXML(pathLoadGantryMotionPos, typeof(MotionPos), out bFlag);
                UpdateWaringLog.Invoke((object)("龙门上料点位配置文件读取成功"));
            }

            return true;
        }

        private bool InitUnloadGantryMotionPos(ref MotionPos motionPos, ref string errString)
        {
            motionPos = new MotionPos(30);
            if (!File.Exists(pathUnloadGantryMotionPos))
            {
                for (int j = 0; j < motionPos.posInfo.Length; j++)
                {
                    motionPos.posInfo[j].PosName = "点位" + j.ToString();
                    motionPos.posInfo[j].XPos = 0;
                    motionPos.posInfo[j].YPos = 0;
                }
                bool result = XmlSerializerHelper.WriteXML((object)motionPos, pathUnloadGantryMotionPos, typeof(MotionPos));
                if (!result)
                {
                    errString = "龙门下料运动点位配置文件丢失";
                    return false;
                }
            }
            else
            {
                bool bFlag = false;
                motionPos = (MotionPos)XmlSerializerHelper.ReadXML(pathUnloadGantryMotionPos, typeof(MotionPos), out bFlag);
                UpdateWaringLog.Invoke((object)("龙门下料点位配置文件读取成功"));
            }

            return true;
        }

        private bool InitUnloadGantrySupplyMotionPos(ref MotionPos motionPos, ref string errString)
        {
            motionPos = new MotionPos(128);
            if (!File.Exists(pathUnloadGantrySupplyMotionPos))
            {
                for (int j = 0; j < motionPos.posInfo.Length; j++)
                {
                    motionPos.posInfo[j].PosName = "点位" + j.ToString();
                    motionPos.posInfo[j].XPos = 0;
                    motionPos.posInfo[j].YPos = 0;
                }
                bool result = XmlSerializerHelper.WriteXML((object)motionPos, pathUnloadGantrySupplyMotionPos, typeof(MotionPos));
                if (!result)
                {
                    errString = "补料盘点位配置文件丢失";
                    return false;
                }
            }
            else
            {
                bool bFlag = false;
                motionPos = (MotionPos)XmlSerializerHelper.ReadXML(pathUnloadGantrySupplyMotionPos, typeof(MotionPos), out bFlag);
                UpdateWaringLog.Invoke((object)("补料盘点位配置文件读取成功"));
            }

            return true;
        }

        private bool InitSystemPara(ref SystemParam systemParam, ref string errString)
        {
            #region Actual Code
            if (!File.Exists(pathSystemParam))
            {
                systemParam.IgnoreDoor = 1;
                systemParam.IgnoreCamera = 0;
                systemParam.IgnoreLaser = 0;
                systemParam.WorkPieceNum = 8;
                systemParam.AxisNum = 8;
                systemParam.OutTime = 20;

                bool result = XmlSerializerHelper.WriteXML((object)systemParam, pathSystemParam, typeof(SystemParam));
                if (!result)
                {
                    errString = "系统参数配置文件丢失";
                    return false;
                }
            }
            else
            {
                bool bFlag = false;
                systemParam = (SystemParam)XmlSerializerHelper.ReadXML(pathSystemParam, typeof(SystemParam), out bFlag);
                UpdateWaringLog.Invoke((object)("系统参数配置文件读取成功"));
                UpdateSysInfo(systemParam);
            }
            #endregion

            return true;
        }

        private bool InitThreshold(ref ThresholdParam ThrParam, ref string errString)
        {
            ThrParam = new ThresholdParam(25);
            #region Actual Code
            if (!File.Exists(pathThresParam))
            {
                ThrParam = new ThresholdParam(25);
                for (int j = 0; j < ThrParam.thrInfo.Length; j++)
                {
                    ThrParam.thrInfo[j].ThrName = "测量项" + j.ToString();
                    ThrParam.thrInfo[j].UpLimit = 0;
                    ThrParam.thrInfo[j].DownLimit = 0;
                }
                bool result = XmlSerializerHelper.WriteXML((object)ThrParam, pathThresParam, typeof(ThresholdParam));
                if (!result)
                {
                    errString = "阈值信息文件丢失";
                    return false;
                }
            }
            else
            {
                bool bFlag = false;
                ThrParam = (ThresholdParam)XmlSerializerHelper.ReadXML(pathThresParam, typeof(ThresholdParam), out bFlag);
                UpdateWaringLog.Invoke((object)("阈值信息文件读取成功"));
            }
            //UpdateThreshold(ThrParam);
            #endregion

            return true;
        }

        private bool InitThresholdShow(ref ThresholdParam ThrParam, ref string errString)
        {
            ThrParam = new ThresholdParam(25);
            #region Actual Code
            if (!File.Exists(pathThresParamShow))
            {
                ThrParam = new ThresholdParam(25);
                for (int j = 0; j < ThrParam.thrInfo.Length; j++)
                {
                    ThrParam.thrInfo[j].ThrName = "测量项" + j.ToString();
                    ThrParam.thrInfo[j].UpLimit = 0;
                    ThrParam.thrInfo[j].DownLimit = 0;
                }
                bool result = XmlSerializerHelper.WriteXML((object)ThrParam, pathThresParamShow, typeof(ThresholdParam));
                if (!result)
                {
                    errString = "阈值信息文件丢失";
                    return false;
                }
            }
            else
            {
                bool bFlag = false;
                ThrParam = (ThresholdParam)XmlSerializerHelper.ReadXML(pathThresParamShow, typeof(ThresholdParam), out bFlag);
                UpdateWaringLog.Invoke((object)("阈值信息文件读取成功"));
            }
            UpdateThreshold(ThrParam);
            #endregion

            return true;
        }

        private bool InitMoveConfig()
        {

            moveConfig = new MovePathConfig();
            moveConfig.moveConfig = new List<MovePathParam>();
            bool bFlag = false;

            #region Actual Code         
            moveConfig = (MovePathConfig)XmlSerializerHelper.ReadXML(moveConfigPath, typeof(MovePathConfig), out bFlag);
            UpdateWaringLog.Invoke((object)("Laser MoveConfig配置文件读取成功"));
            #endregion
            return true;
        }

        private bool InitCCDCalibMatrix(ref CalibMatrix CCDCalibMatrix)
        {
            CCDCalibMatrix = new CalibMatrix();
            if (!File.Exists(pathCCDMatrix))
            {
                CCDCalibMatrix.a11 = 1;
                CCDCalibMatrix.a12 = 0;
                CCDCalibMatrix.a13 = 0;
                CCDCalibMatrix.a21 = 0;
                CCDCalibMatrix.a22 = 1;
                CCDCalibMatrix.a23 = 0;
                CCDCalibMatrix.a31 = 0;
                CCDCalibMatrix.a32 = 0;
                CCDCalibMatrix.a33 = 1;
                CCDCalibMatrix.a11back = 1;
                CCDCalibMatrix.a12back = 0;
                CCDCalibMatrix.a13back = 0;
                CCDCalibMatrix.a21back = 0;
                CCDCalibMatrix.a22back = 1;
                CCDCalibMatrix.a23back = 0;
                CCDCalibMatrix.a31back = 0;
                CCDCalibMatrix.a32back = 0;
                CCDCalibMatrix.a33back = 1;
            }
            else
            {
                bool bFlag = false;
                CCDCalibMatrix = XmlSerializerHelper.ReadXML(pathCCDMatrix, typeof(CalibMatrix), out bFlag) as CalibMatrix;
                UpdateWaringLog.Invoke((object)("CCD Matrix配置文件读取成功"));
            }

            return true;
        }

        //标定转换
        private Point2D CalibCCDPoint(Point2D rawpoint, CalibMatrix calibMatrix, bool isFront)
        {

            if (isFront)
            {
                Point2D CalibPoint = new Point2D();
                CalibPoint.X = calibMatrix.a11 * rawpoint.X + calibMatrix.a12 * rawpoint.Y + calibMatrix.a13;
                CalibPoint.Y = calibMatrix.a21 * rawpoint.X + calibMatrix.a22 * rawpoint.Y + calibMatrix.a23;
                double z = calibMatrix.a31 * rawpoint.X + calibMatrix.a32 * rawpoint.Y + calibMatrix.a33;
                CalibPoint.X = CalibPoint.X / z;
                CalibPoint.Y = CalibPoint.Y / z;
                return CalibPoint;
            }
            else
            {
                Point2D CalibPoint = new Point2D();
                CalibPoint.X = calibMatrix.a11back * rawpoint.X + calibMatrix.a12back * rawpoint.Y + calibMatrix.a13back;
                CalibPoint.Y = calibMatrix.a21back * rawpoint.X + calibMatrix.a22back * rawpoint.Y + calibMatrix.a23back;
                double z = calibMatrix.a31back * rawpoint.X + calibMatrix.a32back * rawpoint.Y + calibMatrix.a33back;
                CalibPoint.X = CalibPoint.X / z;
                CalibPoint.Y = CalibPoint.Y / z;
                return CalibPoint;
            }

        }

        private double lineSpace(Point2D p1, Point2D p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        //计算P1到P2和P3的连线的距离
        private double CalcDistance(Point2D p1, Point2D p2, Point2D p3)
        {
            double a, b, c;
            a = lineSpace(p2, p3);
            b = lineSpace(p1, p2);
            c = lineSpace(p1, p3);
            double p = (a + b + c) * 0.5;
            if (a + b > c && a + c > b && b + c > a)
            {

                return (2.0 * Math.Sqrt(p * (p - a) * (p - b) * (p - c))) / a;
            }
            else
                return 0;

        }
        //计算P1到P2和P3的连线的垂线的距离，以P2为垂点
        private double CalcP2VerticalDistance(Point2D p1, Point2D p2, Point2D p3)
        {
            double a, b;
            a = CalcDistance(p1, p2, p3);
            b = Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2);
            return Math.Sqrt(b - a * a);
        }

        public Line Perpendicular(Point2D center1, Point2D center2)
        {
            Line line = new Line();
            //两点式
            double _a = center2.Y - center1.Y;
            double _b = -(center2.X - center1.X);
            double _c = -center1.X * (center2.Y - center1.Y) + center1.Y * (center2.X - center1.X);

            if (_a == 0 && _b == 0)
                return line;

            double a = _b;
            double b = (-1) * _a;
            double c = (-1) * a * center2.X - b * center2.Y;
            return line = new Line(a, b, c);
        }

        public double Distance(Line line, Point2D point)
        {
            if (line == null)
                return -1;
            if (line.a == 0 && line.b == 0)
                return -1;
            double dis = Math.Abs(line.a * point.X + line.b * point.Y + line.c) / Math.Sqrt(line.a * line.a + line.b * line.b);
            return dis;
        }


        public CCDUpdateStruct TransferCCDData(double[] CCDdata, CCDTaskRun myCCDTaskRunData)
        {
            int validfai131anum = 0; int validfai131bnum = 0;
            double validfai131a = 0; double validfai131b = 0;

            if (isTestCCDInfluence == true)
            {
                for (int i = 0; i < 205; i++)
                    testCCDRawData[i] = CCDdata[i];

                for (int i = 0; i < 3; i++)
                {
                    testCCDPicPos[i].X = myCCDTaskRunData.PicPos[i].X;
                    testCCDPicPos[i].Y = myCCDTaskRunData.PicPos[i].Y;
                }
            }

            double[] fai161a = new double[20];
            double[] fai161b = new double[20];
            //////////////////////////////////////////
            double[] fai131a = new double[20];
            double[] fai131b = new double[20];

            CCDUpdateStruct CheckCCDData = new CCDUpdateStruct();
            Point2D[] pointAssemble = new Point2D[97];
            Point2D[] calibPointAssemble = new Point2D[97];

            for (int i = 0; i < 90; i++)
            {
                pointAssemble[i] = new Point2D(CCDdata[2 * i + 7], CCDdata[2 * i + 8]);
            }

            for (int i = 90; i < 97; i++)
            {
                pointAssemble[i] = new Point2D(CCDdata[2 * i + 10], CCDdata[2 * i + 11]);
            }

            for (int i = 0; i < 90; i++)
            {
                calibPointAssemble[i] = CalibCCDPoint(pointAssemble[i], CCDCalibMatrix, true);
            }

            for (int i = 90; i < 97; i++)
            {
                calibPointAssemble[i] = CalibCCDPoint(pointAssemble[i], CCDCalibMatrix, false);
            }

            #region 使用拍照时真实位置补偿
            calibPointAssemble[0].X = myCCDTaskRunData.PicPos[0].X - calibPointAssemble[0].X;
            calibPointAssemble[0].Y = myCCDTaskRunData.PicPos[0].Y - calibPointAssemble[0].Y;
            calibPointAssemble[1].X = myCCDTaskRunData.PicPos[2].X - calibPointAssemble[1].X;
            calibPointAssemble[1].Y = myCCDTaskRunData.PicPos[2].Y - calibPointAssemble[1].Y;
            calibPointAssemble[82].X = myCCDTaskRunData.PicPos[1].X - calibPointAssemble[82].X;
            calibPointAssemble[82].Y = myCCDTaskRunData.PicPos[1].Y - calibPointAssemble[82].Y;
            calibPointAssemble[83].X = myCCDTaskRunData.PicPos[1].X - calibPointAssemble[83].X;
            calibPointAssemble[83].Y = myCCDTaskRunData.PicPos[1].Y - calibPointAssemble[83].Y;

            calibPointAssemble[90].X = myCCDTaskRunData.PicPos[0].X - calibPointAssemble[90].X;
            calibPointAssemble[90].Y = myCCDTaskRunData.PicPos[0].Y - calibPointAssemble[90].Y;
            calibPointAssemble[91].X = myCCDTaskRunData.PicPos[2].X - calibPointAssemble[91].X;
            calibPointAssemble[91].Y = myCCDTaskRunData.PicPos[2].Y - calibPointAssemble[91].Y;
            calibPointAssemble[96].X = myCCDTaskRunData.PicPos[2].X - calibPointAssemble[96].X;
            calibPointAssemble[96].Y = myCCDTaskRunData.PicPos[2].Y - calibPointAssemble[96].Y;

            for (int k = 2; k < 22; k++)
            {
                calibPointAssemble[k].X = myCCDTaskRunData.PicPos[0].X - calibPointAssemble[k].X;
                calibPointAssemble[k].Y = myCCDTaskRunData.PicPos[0].Y - calibPointAssemble[k].Y;
            }

            for (int k = 22; k < 42; k++)
            {
                calibPointAssemble[k].X = myCCDTaskRunData.PicPos[2].X - calibPointAssemble[k].X;
                calibPointAssemble[k].Y = myCCDTaskRunData.PicPos[2].Y - calibPointAssemble[k].Y;
            }

            for (int k = 42; k < 82; k++)
            {
                calibPointAssemble[k].X = myCCDTaskRunData.PicPos[0].X - calibPointAssemble[k].X;
                calibPointAssemble[k].Y = myCCDTaskRunData.PicPos[0].Y - calibPointAssemble[k].Y;
            }

            for (int k = 84; k < 87; k++)
            {
                calibPointAssemble[k].X = myCCDTaskRunData.PicPos[2].X - calibPointAssemble[k].X;
                calibPointAssemble[k].Y = myCCDTaskRunData.PicPos[2].Y - calibPointAssemble[k].Y;
            }

            for (int k = 87; k < 90; k++)
            {
                calibPointAssemble[k].X = myCCDTaskRunData.PicPos[0].X - calibPointAssemble[k].X;
                calibPointAssemble[k].Y = myCCDTaskRunData.PicPos[0].Y - calibPointAssemble[k].Y;
            }

            for (int k = 92; k < 96; k++)
            {
                calibPointAssemble[k].X = myCCDTaskRunData.PicPos[0].X - calibPointAssemble[k].X;
                calibPointAssemble[k].Y = myCCDTaskRunData.PicPos[0].Y - calibPointAssemble[k].Y;
            }
            #endregion

            //计算FAI161
            for (int i = 0; i < 20; i++)
            {
                fai161a[i] = CalcP2VerticalDistance(calibPointAssemble[i + 2], calibPointAssemble[0], calibPointAssemble[1]);
                fai161b[i] = CalcP2VerticalDistance(calibPointAssemble[i + 22], calibPointAssemble[0], calibPointAssemble[1]);
            }
            Array.Sort(fai161a); Array.Sort(fai161b);
            //CheckCCDData.fai161 = 0.2 * (fai161a[14] + fai161a[13] + fai161a[12] + fai161a[11] + fai161a[10] + fai161b[14] + fai161b[13] + fai161b[12] + fai161b[11] + fai161b[10]);
            CheckCCDData.fai161afromccd = CalcP2VerticalDistance(calibPointAssemble[95], calibPointAssemble[90], calibPointAssemble[91]);
            CheckCCDData.fai161bfromccd = CalcP2VerticalDistance(calibPointAssemble[96], calibPointAssemble[90], calibPointAssemble[91]);
            CheckCCDData.fai161 = CheckCCDData.fai161afromccd + CheckCCDData.fai161bfromccd;

            //计算FAI131
            for (int i = 0; i < 20; i++)
            {
                if (pointAssemble[i + 42].X != 0 || pointAssemble[i + 42].Y != 0)
                {
                    validfai131anum++;
                    fai131a[i] = CalcDistance(calibPointAssemble[i + 42], calibPointAssemble[0], calibPointAssemble[1]);
                }
                else
                    fai131a[i] = 0;

                if (pointAssemble[i + 62].X != 0 || pointAssemble[i + 62].Y != 0)
                {
                    validfai131bnum++;
                    fai131b[i] = CalcDistance(calibPointAssemble[i + 62], calibPointAssemble[0], calibPointAssemble[1]);
                }
                else
                    fai131b[i] = 0;
            }
            Array.Sort(fai131a); Array.Sort(fai131b);

            if (validfai131anum >= 10)
                validfai131a = 0.2 * (fai131a[14] + fai131a[13] + fai131a[12] + fai131a[11] + fai131a[10]);
            else
                validfai131a = 0.2 * (fai131a[19] + fai131a[18] + fai131a[17] + fai131a[16] + fai131a[15]);

            if (validfai131bnum >= 10)
                validfai131b = 0.2 * (fai131b[14] + fai131b[13] + fai131b[12] + fai131b[11] + fai131b[10]);
            else
                validfai131b = 0.2 * (fai131b[19] + fai131b[18] + fai131b[17] + fai131b[16] + fai131b[15]);

            CheckCCDData.fai131 = validfai131a + validfai131b;
            CheckCCDData.fai130 = validfai131a;


            CheckCCDData.fai133G6 = CalcDistance(calibPointAssemble[82], calibPointAssemble[0], calibPointAssemble[1]);
            CheckCCDData.fai133G4 = CalcDistance(calibPointAssemble[83], calibPointAssemble[0], calibPointAssemble[1]);
            CheckCCDData.fai133G3 = CalcDistance(calibPointAssemble[84], calibPointAssemble[0], calibPointAssemble[1]);
            CheckCCDData.fai133G2 = CalcDistance(calibPointAssemble[85], calibPointAssemble[0], calibPointAssemble[1]);
            CheckCCDData.fai133G1 = CalcDistance(calibPointAssemble[86], calibPointAssemble[0], calibPointAssemble[1]);

            CheckCCDData.fai162 = CalcP2VerticalDistance(calibPointAssemble[92], calibPointAssemble[90], calibPointAssemble[91]);
            CheckCCDData.fai163 = CalcDistance(calibPointAssemble[93], calibPointAssemble[90], calibPointAssemble[91]);
            //CheckCCDData.fai165 = CalcP2VerticalDistance(calibPointAssemble[94], calibPointAssemble[90], calibPointAssemble[91]);源代码 by吕
            CheckCCDData.fai165 = CalcP2VerticalDistance(calibPointAssemble[89], calibPointAssemble[0], calibPointAssemble[1]);

            CheckCCDData.fai162fromAbove = CalcP2VerticalDistance(calibPointAssemble[87], calibPointAssemble[0], calibPointAssemble[1]);
            CheckCCDData.fai163fromAbove = CalcDistance(calibPointAssemble[88], calibPointAssemble[0], calibPointAssemble[1]);
            //CheckCCDData.fai165fromAbove = CalcP2VerticalDistance(calibPointAssemble[89], calibPointAssemble[0], calibPointAssemble[1]);源代码 by吕
            CheckCCDData.fai165fromAbove = CalcP2VerticalDistance(calibPointAssemble[94], calibPointAssemble[90], calibPointAssemble[91]);

            CheckCCDData.fai163fromccd = CCDdata[204];

            CheckCCDData.fai171 = CCDdata[187];
            CheckCCDData.fai171a = CCDdata[188];
            CheckCCDData.fai171b = CCDdata[189];

            CheckCCDData.fai22 = lineSpace(calibPointAssemble[90], calibPointAssemble[91]);

            CheckCCDData.exist = CCDdata[0];
            CheckCCDData.dataNo = myCCDTaskRunData.ProductSeq.ToString();
            CheckCCDData.holeNo = myCCDTaskRunData.StepB.ToString();
            CheckCCDData.TrayNo = ChangeTrayNoToString(myCCDTaskRunData.PartBTrayNo);

            #region 打印数据
            WriteCCDMoveLog(myCCDTaskRunData.PicPos[0].X.ToString() + " " + myCCDTaskRunData.PicPos[0].Y.ToString() + " " + myCCDTaskRunData.PicPos[1].X.ToString() + " " + myCCDTaskRunData.PicPos[1].Y.ToString() + " " + myCCDTaskRunData.PicPos[2].X.ToString() + " " + myCCDTaskRunData.PicPos[2].Y.ToString()
                            + " " + pointAssemble[90].X.ToString() + " " + pointAssemble[90].Y.ToString() + " " + pointAssemble[91].X.ToString() + " " + pointAssemble[91].Y.ToString()
                            + " " + CheckCCDData.fai22.ToString() + " " + CheckCCDData.fai161.ToString());
            #endregion

            CheckCCDData = TransferCCDResultData(CheckCCDData, myCCDTaskRunData.PartBTrayNo, myCCDTaskRunData.StepB);

            return CheckCCDData;

        }

        #region 基恩士激光related
        private bool SetLaserHightSpeedParam(int trigNum, out string errorCode)
        {
            bool result = false;
            errorCode = "";
            //设置KJ[0]批处理点数
            result = keyenceLJ.SetBatchProflilePointNUM(0, trigNum, out errorCode);
            if (!result)
            {
                isLaserRunning = false;
                //UpdateEndCount.Invoke((object)"");
                WarningSolution("【65】设置0批处理个数NG");
                return false;
            }
            result = keyenceLJ.ClearMemoryMeasureProfile(0, out errorCode);
            if (!result)
            {
                isLaserRunning = false;
                WarningSolution("【66】" + errorCode);
                return false;
            }
            result = keyenceLJ.InitHighSpeedCommunication(0, out errorCode);
            if (!result)
            {
                isLaserRunning = false;
                //UpdateEndCount.Invoke((object)"");
                WarningSolution("【67】设置高速0批处理NG" + errorCode);
                return false;
            }
            isSet1Param = true;
            return true;
        }
        //设置激光控制器的参数
        private void SetLaserParam(object test)
        {
            int trigNum = (int)test;
            bool result = false;
            string errorCode = "";
            //设置KJ[0]批处理点数
            result = keyenceLJ.SetBatchProflilePointNUM(0, trigNum, out errorCode);
            if (!result)
            {
                isLaserRunning = false;
                //UpdateEndCount.Invoke((object)"");
                WarningSolution("【69】设置0批处理个数NG");
                return;
            }
            result = keyenceLJ.ClearMemoryMeasureProfile(0, out errorCode);
            if (!result)
            {
                isLaserRunning = false;
                WarningSolution("【70】" + errorCode);
                return;
            }
            result = keyenceLJ.InitHighSpeedCommunication(0, out errorCode);
            if (!result)
            {
                isLaserRunning = false;
                //UpdateEndCount.Invoke((object)"");
                WarningSolution("【71】设置高速0批处理NG" + errorCode);
                return;
            }
            isSet1Param = true;
        }
        //设置激光控制器1的参数
        private void SetLaser1Param(object test)
        {
            int i = (int)test;
            bool result = false;
            string errorCode = "";
            //设置KJ[0]批处理点数
            result = keyenceLJ.SetBatchProflilePointNUM(0, moveConfig.moveConfig[i].nTrigNum - 10, out errorCode);
            if (!result)
            {
                isLaserRunning = false;
                //UpdateEndCount.Invoke((object)"");
                WarningSolution("【73】设置0批处理NG");
                return;
            }
            result = keyenceLJ.ClearMemoryMeasureProfile(0, out errorCode);
            if (!result)
            {
                isLaserRunning = false;
                WarningSolution("【74】" + errorCode);
                return;
            }
            //设置比较触发分通道 LaserRelated
            SetLinearTrigger(i, 1);
            result = keyenceLJ.StartMeasureProfile(0, out errorCode);
            if (!result)
            {
                isLaserRunning = false;
                //UpdateEndCount.Invoke((object)"");
                WarningSolution("【75】开始0批处理NG" + errorCode);
                return;
            }
            isSet1Param = true;
        }



        //LaserACollect和LaserBCollect合并
        //A头数据收集
        private void LaserACollect(object test)
        {
            string strNo = test.ToString();
            string[] No = strNo.Split(',');
            if (No.Count() != 2)
            {
                isLaserRunning = false;
                //UpdateEndCount.Invoke((object)"");
                WarningSolution("【77】标定编号格式错误");
                return;
            }
            int nTimes = Convert.ToInt16(No[0]);
            int nCalibNo = Convert.ToInt16(No[1]);
            bool result = false;
            string errorCode = "";
            XDPOINT[,] temp;

            XDPOINT[,] laserAPoint;//A头
            XDPOINT[,] laserACalibPoint;//A头标定结果
            result = KJDataTo3Dpoint(0, 1, nCalibNo, moveConfig.moveConfig[nTimes].TrigPos.Xpos, moveConfig.moveConfig[nTimes].TrigPos.Ypos, moveConfig.moveConfig[nTimes].dTrigInterval, out errorCode, out laserAPoint, out temp, out laserACalibPoint);
            if (!result)
            {
                isLaserRunning = false;
                //UpdateEndCount.Invoke((object)"");
                WarningSolution("【78】获得0A3DNG" + errorCode);
                return;
            }
            if (taskConfig.isSaveData || taskConfig.isSaveCalibData)
            {
                string name = Thread.CurrentThread.Name;
                while (ThreadNames.IndexOf(name) >= 4)
                {
                    Thread.Sleep(300);
                }
                if (taskConfig.isSaveData)
                {
                    Save3DLaserData(nTimes, 1, laserAPoint);
                }
                if (taskConfig.isSaveCalibData)
                {
                    Save3DLaserCalibData(nTimes, 1, laserACalibPoint);
                }
                ThreadNames.Remove(name);
            }
            Laser12DicData.Add(nCalibNo.ToString(), laserACalibPoint);
            LaserADicData.Add(nTimes.ToString(), laserACalibPoint);

        }

        //B头数据收集
        private void LaserBCollect(object test)
        {
            string strNo = test.ToString();
            string[] No = strNo.Split(',');
            if (No.Count() != 2)
            {
                isLaserRunning = false;
                WarningSolution("【80】标定编号格式错误");
                return;
            }
            int nTimes = Convert.ToInt16(No[0]);
            int nCalibNo = Convert.ToInt16(No[1]);
            bool result = false;
            string errorCode = "";
            XDPOINT[,] temp;
            XDPOINT[,] laserBPoint;//B头
            XDPOINT[,] laserBCalibPoint;//B头标定结果
            result = KJDataTo3Dpoint2(0, 2, nCalibNo, moveConfig.moveConfig[nTimes].TrigPos.Xpos, moveConfig.moveConfig[nTimes].TrigPos.Ypos, moveConfig.moveConfig[nTimes].dTrigInterval, out errorCode, out temp, out laserBPoint, out laserBCalibPoint);
            if (!result)
            {
                isLaserRunning = false;
                WarningSolution("【81】获得0B3DNG" + errorCode);
                return;
            }
            if (taskConfig.isSaveData || taskConfig.isSaveCalibData)
            {
                string name = Thread.CurrentThread.Name;
                while (ThreadNames.IndexOf(name) >= 4)
                {
                    Thread.Sleep(300);
                }
                if (taskConfig.isSaveData)
                {
                    Save3DLaserData(nTimes, 2, laserBPoint);
                }
                if (taskConfig.isSaveCalibData)
                {
                    Save3DLaserCalibData(nTimes, 2, laserBCalibPoint);
                }
                ThreadNames.Remove(name);
            }
            Laser12DicData.Add(nCalibNo.ToString(), laserBCalibPoint);
            LaserBDicData.Add(nTimes.ToString(), laserBCalibPoint);
        }

        #region KeyenceBatchDataTo3DPoint
        public bool KJDataTo3Dpoint(int nControlDeviceId, int nlaser, int nCalibNO, double Xstart, double Ystart, double dXInterval, out string errorcode, out XDPOINT[,] laserAPoint, out XDPOINT[,] laserBPoint, out XDPOINT[,] laserCalibPoint)
        {
            laserAPoint = null;
            laserBPoint = null;
            laserCalibPoint = null;
            errorcode = "";
            if (nlaser == 1)
            {
                //List<int[]> LJlaserA = new List<int[]>();
                List<int[]> LJlaserA = keyenceLJ.DatalaserA;
                //List<int[]> LJlaserA = DeepClone<List<int[]>>(keyenceLJ.DatalaserA);//深拷贝
                laserAPoint = new XDPOINT[LJlaserA.Count, LJlaserA[0].Length];
                laserCalibPoint = new XDPOINT[LJlaserA.Count, LJlaserA[0].Length];
                for (int i = 0; i < laserAPoint.GetLength(0); i++)
                {
                    for (int j = 0; j < laserAPoint.GetLength(1); j++)
                    {
                        laserAPoint[i, j].x = (Xstart + dXInterval * i);
                        laserAPoint[i, j].y = Ystart + 0.01 * j;//7020--10微米
                        laserAPoint[i, j].z = LJlaserA[i][799 - j] / 100000.0;
                        if (nCalibNO != 999)
                        {
                            laserCalibPoint[i, j] = LaserTo3DCalib(laserAPoint[i, j].x, laserAPoint[i, j].y, laserAPoint[i, j].z, CalibPathNO[nCalibNO]);
                        }
                    }
                }

            }
            if (nlaser == 2)
            {
                List<int[]> LJlaserB = DeepClone<List<int[]>>(keyenceLJ.DatalaserB);//深拷贝
                laserBPoint = new XDPOINT[LJlaserB.Count, LJlaserB[0].Length];
                laserCalibPoint = new XDPOINT[LJlaserB.Count, LJlaserB[0].Length];
                for (int i = 0; i < laserBPoint.GetLength(0); i++)
                {
                    for (int j = 0; j < laserBPoint.GetLength(1); j++)
                    {
                        laserBPoint[i, j].x = (Xstart + dXInterval * i);
                        laserBPoint[i, j].y = Ystart + 0.01 * j;//7020--10微米
                        laserBPoint[i, j].z = LJlaserB[i][799 - j] / 100000.0;
                        if (nCalibNO != 999)
                        {
                            laserCalibPoint[i, j] = LaserTo3DCalib(laserBPoint[i, j].x, laserBPoint[i, j].y, laserBPoint[i, j].z, CalibPathNO[nCalibNO]);
                        }
                    }
                }
            }
            return true;
        }

        public bool KJDataTo3Dpoint2(int nControlDeviceId, int nlaser, int nCalibNO, double Xstart, double Ystart, double dXInterval, out string errorcode, out XDPOINT[,] laserAPoint, out XDPOINT[,] laserBPoint, out XDPOINT[,] laserCalibPoint)
        {
            laserAPoint = null;
            laserBPoint = null;
            laserCalibPoint = null;
            errorcode = "";
            DateTime start1 = DateTime.Now;
            if (nlaser == 1)
            {
                //List<int[]> LJlaserA = new List<int[]>();
                //List<int[]> LJlaserA = keyenceLJ.DatalaserA;
                List<int[]> LJlaserA = DeepClone<List<int[]>>(keyenceLJ.DatalaserA);//深拷贝

                laserAPoint = new XDPOINT[LJlaserA.Count, LJlaserA[0].Length];
                laserCalibPoint = new XDPOINT[LJlaserA.Count, LJlaserA[0].Length];
                for (int i = 0; i < laserAPoint.GetLength(0); i++)
                {
                    for (int j = 0; j < laserAPoint.GetLength(1); j++)
                    {
                        laserAPoint[i, j].x = (Xstart + dXInterval * i);
                        laserAPoint[i, j].y = Ystart + 0.01 * j;//7020--10微米
                        laserAPoint[i, j].z = LJlaserA[i][799 - j] / 100000.0;
                        if (nCalibNO != 999)
                        {
                            laserCalibPoint[i, j] = LaserTo3DCalib(laserAPoint[i, j].x, laserAPoint[i, j].y, laserAPoint[i, j].z, CalibPathNO[nCalibNO]);
                        }
                    }
                }
            }
            if (nlaser == 2)
            {
                List<int[]> LJlaserB = keyenceLJ.DatalaserB;
                // List<int[]> LJlaserB = DeepClone<List<int[]>>(keyenceLJ.DatalaserB);//深拷贝
                TimeSpan spc = DateTime.Now - start1;
                //        UpdateWaringLog.Invoke((object)("CopyB" + spc.TotalSeconds.ToString()));
                start1 = DateTime.Now;
                laserBPoint = new XDPOINT[LJlaserB.Count, LJlaserB[0].Length];
                laserCalibPoint = new XDPOINT[LJlaserB.Count, LJlaserB[0].Length];
                for (int i = 0; i < laserBPoint.GetLength(0); i++)
                {
                    for (int j = 0; j < laserBPoint.GetLength(1); j++)
                    {
                        laserBPoint[i, j].x = (Xstart + dXInterval * i);
                        laserBPoint[i, j].y = Ystart + 0.01 * j;//7020--10微米
                        laserBPoint[i, j].z = LJlaserB[i][799 - j] / 100000.0;
                        if (nCalibNO != 999)
                        {
                            laserCalibPoint[i, j] = LaserTo3DCalib(laserBPoint[i, j].x, laserBPoint[i, j].y, laserBPoint[i, j].z, CalibPathNO[nCalibNO]);
                        }
                    }
                }
                spc = DateTime.Now - start1;
                //        UpdateWaringLog.Invoke((object)("B总" + spc.TotalSeconds.ToString()));
            }
            return true;
        }

        #endregion


        #region  深层拷贝引用类型 加static类型就报错
        public T Copy2<T>(T RealObject)
        {
            using (Stream objectStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, RealObject);
                objectStream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(objectStream);
            }
        }
        public T DeepClone<T>(T obj)
        {
            object retval;
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xml = new XmlSerializer(typeof(T));
                xml.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                retval = xml.Deserialize(ms);
                ms.Close();
            }
            return (T)retval;
        }
        #endregion

        //标定转换
        private XDPOINT LaserTo3DCalib(double x, double y, double z, Calib3DSturct CalibMatrix)
        {
            XDPOINT CalibPoint = new XDPOINT();
            CalibPoint.x = CalibMatrix.a11 * x + CalibMatrix.a12 * y + CalibMatrix.a13 * z + CalibMatrix.a14;
            CalibPoint.y = CalibMatrix.a21 * x + CalibMatrix.a22 * y + CalibMatrix.a23 * z + CalibMatrix.a24;
            CalibPoint.z = CalibMatrix.a31 * x + CalibMatrix.a32 * y + CalibMatrix.a33 * z + CalibMatrix.a34;
            CalibPoint.x = Math.Round(CalibPoint.x, 5);
            CalibPoint.y = Math.Round(CalibPoint.y, 5);
            CalibPoint.z = Math.Round(CalibPoint.z, 5);

            if (Math.Abs(z) >= 12)
                CalibPoint.z = 99999;
            return CalibPoint;
        }
        //标定转换
        private XDPOINT LaserTo3DCalib2(double x, double y, Calib3DSturct CalibMatrix, double z = 1.0f)
        {
            XDPOINT CalibPoint = new XDPOINT();
            CalibPoint.x = CalibMatrix.a11 * x + CalibMatrix.a12 * y + CalibMatrix.a13 * z;
            CalibPoint.y = CalibMatrix.a21 * x + CalibMatrix.a22 * y + CalibMatrix.a23 * z;
            return CalibPoint;
        }
        public void Save3DLaserData(int nindex, int nlaser, XDPOINT[,] LaserData)
        {
            string year = System.DateTime.Now.Year.ToString();
            string month = System.DateTime.Now.Month.ToString();
            string day = System.DateTime.Now.Day.ToString();
            string hour = System.DateTime.Now.Hour.ToString();
            string minute = System.DateTime.Now.Minute.ToString();
            string second = System.DateTime.Now.Second.ToString();
            string pathname = string.Format(" path[{0:d}]-laser[{1:d}]", nindex, nlaser);
            string path = LaserDataPath + year + "-" + month + "-" + day + " " + hour + "-" + minute + "-" + second + pathname + ".csv";
            CsvFile file = new CsvFile();

            if (LaserData == null)
            {
                WarningSolution("【83】" + pathname + "-保存出错");
                return;
            }

            for (int i = 0; i < LaserData.GetLength(0); i++)
            {
                CsvRecord record = new CsvRecord();
                for (int j = 0; j < LaserData.GetLength(1); j++)
                {
                    record.Fields.Add(LaserData[i, j].x.ToString());
                    record.Fields.Add(LaserData[i, j].y.ToString());
                    record.Fields.Add(LaserData[i, j].z.ToString());
                }
                file.Records.Add(record);
            }

            CsvWriter writer = new CsvWriter();
            writer.WriteCsv(file, path);
        }
        public void Save3DLaserCalibData(int nindex, int nlaser, XDPOINT[,] LaserData)
        {
            string year = System.DateTime.Now.Year.ToString();
            string month = System.DateTime.Now.Month.ToString();
            string day = System.DateTime.Now.Day.ToString();
            string hour = System.DateTime.Now.Hour.ToString();
            string minute = System.DateTime.Now.Minute.ToString();
            string second = System.DateTime.Now.Second.ToString();
            string pathname = string.Format(" path[{0:d}]-laserCalib[{1:d}]", nindex, nlaser);
            string path = LaserDataPath + year + "-" + month + "-" + day + " " + hour + "-" + minute + "-" + second + pathname + ".csv";
            CsvFile file = new CsvFile();

            for (int i = 0; i < LaserData.GetLength(0); i++)
            {
                CsvRecord record = new CsvRecord();
                for (int j = 0; j < LaserData.GetLength(1); j++)
                {
                    record.Fields.Add(LaserData[i, j].x.ToString());
                    record.Fields.Add(LaserData[i, j].y.ToString());
                    record.Fields.Add(LaserData[i, j].z.ToString());
                }
                file.Records.Add(record);
            }

            CsvWriter writer = new CsvWriter();
            writer.WriteCsv(file, path);
        }

        //3Dtask
        //List<XDPOINT[,]> laserData,int datano,int step,int trayno
        public void Task3DRun(object LaserTaskRun)
        {
            WriteLog("【Laser检测】Laser数据处理开始");
            LaserTaskRun myLaserTaskRun = new LaserTaskRun();
            myLaserTaskRun = (LaserTaskRun)LaserTaskRun;
            int laserResdultRowNo = 0;

            if (taskControl[myLaserTaskRun.step] == null || taskControl[myLaserTaskRun.step].tasks == null || taskControl[myLaserTaskRun.step].tasks.Count <= 0)
            {
                return;
            }

            OutputPrimData DataOut;
            InputPrimData DataIn = new InputPrimData();
            for (int i = 0; i < myLaserTaskRun.laserData.Count; i++)
            {
                PointCloudData pointcloudData = new PointCloudData();

                pointcloudData.SetLaserPoints(myLaserTaskRun.laserData[i]);
                DataIn.lstPointCloudPrim.Add(pointcloudData);
            }
            WriteLog("【Laser检测】Laser数据TaskRun开始");
            taskControl[myLaserTaskRun.step].TaskRun(DataIn, out DataOut);
            WriteLog("【Laser检测】Laser数据TaskRun结束");

            if (DataOut != null)
            {
                LaserUpdateStruct LaserResult = new LaserUpdateStruct();
                LaserResult.LaserFai = new List<FaiValue>();
                for (int i = 0; i < DataOut.lstFaiOutput.Count; i++)
                {
                    if (DataOut.lstFaiOutput[i].isOutput)
                    {
                        LaserResult.LaserFai.Add(new FaiValue());
                        laserResdultRowNo = LaserResult.LaserFai.Count();

                        LaserResult.LaserFai[laserResdultRowNo - 1].factor = 1;
                        LaserResult.LaserFai[laserResdultRowNo - 1].isOutput = DataOut.lstFaiOutput[i].isOutput;
                        LaserResult.LaserFai[laserResdultRowNo - 1].nameFai = DataOut.lstFaiOutput[i].nameFai;
                        LaserResult.LaserFai[laserResdultRowNo - 1].normal = DataOut.lstFaiOutput[i].normal;
                        LaserResult.LaserFai[laserResdultRowNo - 1].offset = 0;
                        LaserResult.LaserFai[laserResdultRowNo - 1].remark = DataOut.lstFaiOutput[i].remark;
                        LaserResult.LaserFai[laserResdultRowNo - 1].tolLow = DataOut.lstFaiOutput[i].tolLow;
                        LaserResult.LaserFai[laserResdultRowNo - 1].tolUp = DataOut.lstFaiOutput[i].tolUp;
                        LaserResult.LaserFai[laserResdultRowNo - 1].valueFai = DataOut.lstFaiOutput[i].valueFai + DataOffset.laserRawDataOffsets[myLaserTaskRun.trayno * 4 + myLaserTaskRun.step].OffsetValues[i];
                    }
                }

                if (LaserResult.LaserFai.Count == 0)
                {
                    FaiValue temp = new FaiValue();
                    temp.valueFai = 9999;

                    for (int i = 0; i < 55; i++)
                        LaserResult.LaserFai.Add(temp);
                }

                if (LaserResult.LaserFai.Count != 0)
                {
                    LaserResult.DataNo = myLaserTaskRun.datano.ToString();
                    LaserResult.HoleNo = myLaserTaskRun.step.ToString();
                    LaserResult.TrayNo = ChangeTrayNoToString(myLaserTaskRun.trayno);

                    UpdateLaserResult.Invoke((LaserUpdateStruct)LaserResult);//返回的结果更新到MainForm界面
                    LaserFAIUpdateStruct LaserFaiResult = ChangeLaserResultToLaserFai(LaserResult.LaserFai, LaserResult.DataNo, LaserResult.HoleNo, LaserResult.TrayNo);
                    UpdateLaserFaiResult(LaserFaiResult);//显示Laser Fai值到MainForm中
                    GetPieceAllDataLaserFAI(myLaserTaskRun.trayno, myLaserTaskRun.step, LaserFaiResult);
                    if (!myLaserTaskRun.isdebugmode)
                    {
                        LaserCheckResultArrayTray[myLaserTaskRun.trayno, myLaserTaskRun.step] = LaserJudge(LaserFaiResult);
                        //LaserCheckResultListTray[myLaserTaskRun.trayno][myLaserTaskRun.step] = 1;
                        LaserCheckDoneArrayTray[myLaserTaskRun.trayno, myLaserTaskRun.step] = true;
                    }
                }
            }
            WriteLog("【Laser检测】Laser数据处理完成");
            return;
        }

        private string ChangeTrayNoToString(int trayno)
        {
            switch (trayno)
            {
                case 0:
                    return "A";
                case 1:
                    return "B";
                case 2:
                    return "C";
                default:
                    return "Null";
            }
        }

        private LaserFAIUpdateStruct ChangeLaserResultToLaserFaiOld(List<FaiValue> laserFai, string datano, string holeno, string trayno)
        {
            LaserFAIUpdateStruct temp = new LaserFAIUpdateStruct();
            temp.dataNo = datano;
            temp.holeNo = holeno;
            temp.TrayNo = trayno;
            List<double[]> tempArrayList = new List<double[]>();
            tempArrayList.Add(new double[13]); tempArrayList.Add(new double[8]); tempArrayList.Add(new double[9]);
            tempArrayList.Add(new double[4]); tempArrayList.Add(new double[4]); tempArrayList.Add(new double[3]);
            tempArrayList.Add(new double[3]); tempArrayList.Add(new double[4]); tempArrayList.Add(new double[4]); tempArrayList.Add(new double[3]);

            int k = 0;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < tempArrayList[i].Length; j++)
                {
                    tempArrayList[i][j] = laserFai[k].valueFai;
                    k++;
                }
            }

            //fai135
            double[] tempArray = new double[tempArrayList[0].Length];
            for (int i = 0; i < tempArray.Length; i++)
            {
                tempArray[i] = Math.Abs(2.121 - Math.Abs(tempArrayList[0][i]));
            }
            Array.Sort(tempArray);
            temp.fai135 = 2.0 * tempArray[12];
            //fai136
            Array.Sort(tempArrayList[0]);
            temp.fai136 = tempArrayList[0][12] - tempArrayList[0][0];
            //fai139
            Array.Sort(tempArrayList[2]);
            temp.fai139 = tempArrayList[2][8] - tempArrayList[2][0];
            //fai140
            Array.Sort(tempArrayList[1]);
            temp.fai140 = tempArrayList[1][7] - tempArrayList[1][0];
            //fai151
            tempArray = new double[tempArrayList[7].Length];
            for (int i = 0; i < tempArray.Length; i++)
            {
                tempArray[i] = Math.Abs(0.3 - Math.Abs(tempArrayList[7][i]));
            }
            Array.Sort(tempArray);
            temp.fai151 = 2.0 * tempArray[3];
            //fai152
            Array.Sort(tempArrayList[3]);
            temp.fai152 = tempArrayList[3][3] - tempArrayList[3][0];
            //fai155
            tempArray = new double[tempArrayList[8].Length];
            for (int i = 0; i < tempArray.Length; i++)
            {
                tempArray[i] = Math.Abs(0.3 - Math.Abs(tempArrayList[8][i]));
            }
            Array.Sort(tempArray);
            temp.fai155 = 2.0 * tempArray[3];
            //fai156
            Array.Sort(tempArrayList[4]);
            temp.fai156 = tempArrayList[4][3] - tempArrayList[4][0];
            //fai157
            tempArray = new double[tempArrayList[6].Length];
            for (int i = 0; i < tempArray.Length; i++)
            {
                tempArray[i] = Math.Abs(3.22 - Math.Abs(tempArrayList[6][i]));
            }
            Array.Sort(tempArray);
            temp.fai157 = 2.0 * tempArray[2];
            //fai158
            Array.Sort(tempArrayList[6]);
            temp.fai158 = tempArrayList[6][2] - tempArrayList[6][0];
            //fai160
            Array.Sort(tempArrayList[5]);
            temp.fai160 = tempArrayList[5][2] - tempArrayList[5][0];
            //fai172
            tempArray = new double[tempArrayList[9].Length];
            for (int i = 0; i < tempArray.Length; i++)
            {
                tempArray[i] = Math.Abs(0.589 - Math.Abs(tempArrayList[9][i]));
            }
            Array.Sort(tempArray);
            temp.fai172 = 2.0 * tempArray[2];

            temp = TransferLaserResultData(temp, trayno, Convert.ToInt32(holeno));
            return temp;
        }

        private LaserFAIUpdateStruct ChangeLaserResultToLaserFai(List<FaiValue> laserFai, string datano, string holeno, string trayno)
        {
            LaserFAIUpdateStruct temp = new LaserFAIUpdateStruct();
            temp.dataNo = datano;
            temp.holeNo = holeno;
            temp.TrayNo = trayno;
            List<double[]> tempArrayList = new List<double[]>();
            tempArrayList.Add(new double[13]);
            tempArrayList.Add(new double[8]);
            tempArrayList.Add(new double[9]);
            tempArrayList.Add(new double[4]);
            tempArrayList.Add(new double[4]);
            tempArrayList.Add(new double[3]);
            tempArrayList.Add(new double[3]);
            tempArrayList.Add(new double[4]);
            tempArrayList.Add(new double[4]);
            tempArrayList.Add(new double[3]);

            int k = 0;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < tempArrayList[i].Length; j++)
                {
                    tempArrayList[i][j] = laserFai[k].valueFai;
                    k++;
                }
            }

            //对测量异常的点位过进行过滤，使用同一测量项下的其他点位代替异常点
            for (int i = 0; i < tempArrayList.Count; i++)
            {
                //寻找每个FAI的首个正常点位
                double tempVal = 0.0;
                for (int j = 0; j < tempArrayList[i].Length; j++)
                {
                    if (Math.Abs(tempArrayList[i][j]) < 5.0)
                    {
                        tempVal = tempArrayList[i][j];
                        break;
                    }
                }
                //使用首个正常点位的值代替异常点
                for (int j = 0; j < tempArrayList[i].Length; j++)
                {
                    if (Math.Abs(tempArrayList[i][j]) > 5.0)    //正常测量的点位值不可能大于5.0
                        tempArrayList[i][j] = tempVal;
                }
            }

            //fai135
            double[] tempArray = new double[tempArrayList[0].Length];
            for (int i = 0; i < tempArray.Length; i++)
            {
                tempArray[i] = Math.Abs(2.121 - Math.Abs(tempArrayList[0][i]));
            }
            Array.Sort(tempArray);
            temp.fai135 = 2.0 * tempArray[12];
            //fai136
            Array.Sort(tempArrayList[0]);
            temp.fai136 = tempArrayList[0][12] - tempArrayList[0][0];
            //fai139
            Array.Sort(tempArrayList[2]);
            temp.fai139 = tempArrayList[2][8] - tempArrayList[2][0];
            //fai140
            Array.Sort(tempArrayList[1]);
            temp.fai140 = tempArrayList[1][7] - tempArrayList[1][0];
            //fai151
            tempArray = new double[tempArrayList[7].Length];
            for (int i = 0; i < tempArray.Length; i++)
            {
                tempArray[i] = Math.Abs(0.3 - Math.Abs(tempArrayList[7][i]));
            }
            Array.Sort(tempArray);
            temp.fai151 = 2.0 * tempArray[3];
            //fai152
            Array.Sort(tempArrayList[3]);
            temp.fai152 = tempArrayList[3][3] - tempArrayList[3][0];
            //fai155
            tempArray = new double[tempArrayList[8].Length];
            for (int i = 0; i < tempArray.Length; i++)
            {
                tempArray[i] = Math.Abs(0.3 - Math.Abs(tempArrayList[8][i]));
            }
            Array.Sort(tempArray);
            temp.fai155 = 2.0 * tempArray[3];
            //fai156
            Array.Sort(tempArrayList[4]);
            temp.fai156 = tempArrayList[4][3] - tempArrayList[4][0];
            //fai157
            tempArray = new double[tempArrayList[6].Length];
            for (int i = 0; i < tempArray.Length; i++)
            {
                tempArray[i] = Math.Abs(3.22 - Math.Abs(tempArrayList[6][i]));
            }
            Array.Sort(tempArray);
            temp.fai157 = 2.0 * tempArray[2];
            //fai158
            Array.Sort(tempArrayList[6]);
            temp.fai158 = tempArrayList[6][2] - tempArrayList[6][0];
            //fai160
            //对P11与P9，P10差异较大的点位进行过滤，使用P9点代替
            //double averageP9andP10 = (tempArrayList[5][0] + tempArrayList[5][1]) / 2.0;
            if (Math.Abs(tempArrayList[5][2] - (tempArrayList[5][0] + tempArrayList[5][1]) / 2.0) > LaserDiff)
                tempArrayList[5][2] = tempArrayList[5][0];
            Array.Sort(tempArrayList[5]);
            temp.fai160 = tempArrayList[5][2] - tempArrayList[5][0];
            //fai172
            //对P11与P9，P10差异较大的点位进行过滤，使用P9点代替
            if (Math.Abs(tempArrayList[9][2] - (tempArrayList[9][0] + tempArrayList[9][1]) / 2.0) > LaserDiff)
                tempArrayList[9][2] = tempArrayList[9][0];
            tempArray = new double[tempArrayList[9].Length];
            for (int i = 0; i < tempArray.Length; i++)
            {
                tempArray[i] = Math.Abs(0.589 - Math.Abs(tempArrayList[9][i]));
            }
            Array.Sort(tempArray);
            temp.fai172 = 2.0 * tempArray[2];

            temp = TransferLaserResultData(temp, trayno, Convert.ToInt32(holeno));
            return temp;
        }


        public void TaskControl_updateMsg(string msg)
        {
            //int nindex = msg.IndexOf('【');
            ////if(nindex>=0)
            ////    msg = msg.Substring(nindex, msg.Length - nindex);
            ////ROI提示显示语句
            //if (nindex < 0)
            //    UpdateWaringLog.Invoke((object)msg);
        }
        #endregion
        public bool ReadMoveConfig(string path, out MovePathConfig moveConfigTemp)
        {
            moveConfigTemp = null;
            bool bflag = false;
            moveConfigTemp = XmlSerializerHelper.ReadXML(path, typeof(MovePathConfig), out bflag) as MovePathConfig;
            if (moveConfigTemp == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public int GetTrigCounts(int nCH)
        {
            int nGetCounts = 0;
            adlink.TrigCountCH(nCH, out nGetCounts);
            return nGetCounts;
        }

        //初始化测试环境
        public void InitTestEnvironment()
        {
            if (bMiniInit)
                MainFormMiniInitDoing = true;
            else
                MainFormInitDoing = true;

            DateTime StartTime = DateTime.Now;
            int errcode = 0;
            bool homemovedone = false;
            //bool loadNullTrayZStop = false; bool loadFullTrayZStop = false;
            //bool unloadNullTrayZStop = false; bool unloadFullTrayZStop = false;
            int initStep = (int)InitStep.TraySwitchDO;
            bool InitDone = false;
            double poserror = 100;
            AutoRunActive = false;
            while (!InitDone)
            {
                switch (initStep)
                {
                    case (int)InitStep.TraySwitchDO:
                        if (LoadTrayVacumBreak() && UnloadTrayVacumBreak() == false)
                        {
                            QuitInitProcess();
                            return;
                        }
                        if (LoadTrayZRetract() && UnloadTrayZRetract() == false)
                        {
                            QuitInitProcess();
                            return;
                        }
                        if (LoadTrayMovePistonStretch() && UnloadTrayMovePistonStretch() == false)
                        {
                            QuitInitProcess();
                            return;
                        }
                        UpdateWaringLog.Invoke((object)"初始化-Tray盘替换相关气缸已到位");
                        initStep = (int)InitStep.GantryDO;
                        break;
                    case (int)InitStep.GantryDO:
                        if (LoadGantryAllSuckerBreak() && UnloadGantryAllAndCloseSuckerBreak() == false)//by 吕
                        {
                            QuitInitProcess();
                            return;
                        }
                        IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumBreak, false);
                        IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumBreak, false);
                        if ((LoadGantryAllPistonZRetract() && UnloadGantryAllPistonZRetract()) == false)
                        {
                            QuitInitProcess();
                            return;
                        }
                        UpdateWaringLog.Invoke((object)"初始化-龙门相关气缸已到位");
                        initStep = (int)InitStep.SuckAxisDO;
                        break;
                    case (int)InitStep.SuckAxisDO:
                        if (SuckAxisSuckerBreak() == false)
                        {
                            QuitInitProcess();
                            return;
                        }
                        if (SuckAxisZRetract() == false)
                        {
                            QuitInitProcess();
                            return;
                        }
                        IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
                        IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);
                        UpdateWaringLog.Invoke((object)"初始化-横移轴相关气缸已到位");
                        initStep = (int)InitStep.CarrierRetract;
                        break;
                    case (int)InitStep.CarrierRetract:
                        if (RetractAllCylinder() == false)
                        {
                            QuitInitProcess();
                            return;
                        }
                        UpdateWaringLog.Invoke((object)"初始化-载具缩回完成");
                        if (!bMiniInit)
                            initStep = (int)InitStep.TraySeparateRetract;
                        else
                            initStep = (int)InitStep.LoadAndUnloadModuleDO;
                        break;
                    case (int)InitStep.TraySeparateRetract:
                        if (LoadTraySeparateRetract() && UnloadTraySeparateRetract() == false)
                        {
                            QuitInitProcess();
                            return;
                        }
                        UpdateWaringLog.Invoke((object)"初始化-分盘气缸缩回完成");
                        initStep = (int)InitStep.LoadAndUnloadModuleDO;
                        break;
                    case (int)InitStep.LoadAndUnloadModuleDO:
                        if (LoadModuleVacumBreak() && UnloadModuleVacumBreak() == false)
                        {
                            QuitInitProcess();
                            return;
                        }
                        IOControl.ECATWriteDO((int)ECATDONAME.Do_BlowControl, false);
                        if (LoadModuleMoveRetract() && UnloadModuleMoveRetract() == false)
                        {
                            QuitInitProcess();
                            return;
                        }
                        if (LoadModulePosMoveRetract() && UnloadModulePosMoveRetract() == false)
                        {
                            QuitInitProcess();
                            return;
                        }
                        UpdateWaringLog.Invoke((object)"初始化-上下料模组气缸到位");
                        initStep = (int)InitStep.GantryAxisHome;
                        break;
                    case (int)InitStep.GantryAxisHome:
                        HomeMove(6); HomeMove(7); HomeMove(8); HomeMove(9);
                        StartTime = DateTime.Now;
                        while (true)
                        {
                            homemovedone = adlink.CheckMoveDone(logicConfig.ECATAxis[0], ref errcode) && adlink.CheckMoveDone(logicConfig.ECATAxis[1], ref errcode) &&
                                           adlink.CheckMoveDone(logicConfig.ECATAxis[2], ref errcode) && adlink.CheckMoveDone(logicConfig.ECATAxis[3], ref errcode);
                            if (homemovedone == true)
                                break;
                            else
                            {
                                if (!OutTimeCount(StartTime, 30))
                                {
                                    WarningSolution("【84】初始化-上下料龙门轴回零失败");
                                    {
                                        QuitInitProcess();
                                        return;
                                    }
                                }
                                Thread.Sleep(30);
                            }
                        }
                        LoadGantryXYMove(new double[] { systemParam.LoadTrayAvoidPosX, systemParam.LoadTrayAvoidPosY }, true);
                        UnloadGantryXYMove(new double[] { systemParam.UnloadTrayAvoidPosX, systemParam.UnloadTrayAvoidPosY }, true);

                        UpdateWaringLog.Invoke((object)"初始化-龙门轴回零成功");
                        if (!bMiniInit)
                            initStep = (int)InitStep.TrayZAxisCheck;
                        else
                            initStep = (int)InitStep.PulseAxisHome;
                        break;
                    case (int)InitStep.TrayZAxisCheck:
                        //adlink.JogMoveStop(logicConfig.ECATAxis[4]); adlink.JogMoveStop(logicConfig.ECATAxis[5]);
                        //adlink.JogMoveStop(logicConfig.ECATAxis[6]); adlink.JogMoveStop(logicConfig.ECATAxis[7]);
                        //adlink.JogMoveStart(logicConfig.ECATAxis[4], 0); adlink.JogMoveStart(logicConfig.ECATAxis[5], 0);
                        //adlink.JogMoveStart(logicConfig.ECATAxis[6], 0); adlink.JogMoveStart(logicConfig.ECATAxis[7], 0);
                        //while (true)
                        //{
                        //    if (loadNullTrayZStop && loadFullTrayZStop && unloadNullTrayZStop && unloadFullTrayZStop)
                        //        break;
                        //    else
                        //        Thread.Sleep(5);

                        //    if ((!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInPosition + 25]) || (CurInfo.motionIO[10].pel == 1))
                        //    {
                        //        adlink.JogMoveStop(logicConfig.ECATAxis[4]);loadNullTrayZStop = true;
                        //    }
                        //    if ((!CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInPosition + 25]) || (CurInfo.motionIO[11].pel == 1))
                        //    {
                        //        adlink.JogMoveStop(logicConfig.ECATAxis[5]); loadFullTrayZStop = true;
                        //    }
                        //    if ((!CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInPosition + 25]) || (CurInfo.motionIO[12].pel == 1))
                        //    {
                        //        adlink.JogMoveStop(logicConfig.ECATAxis[6]); unloadNullTrayZStop = true;
                        //    }
                        //    if ((!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInPosition + 25]) || (CurInfo.motionIO[13].pel == 1))
                        //    {
                        //        adlink.JogMoveStop(logicConfig.ECATAxis[7]); unloadFullTrayZStop = true;
                        //    }
                        //}
                        //if ((CurInfo.motionIO[10].pel != 1) || (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInPosition + 25]))
                        //{
                        //    MessageBox.Show("上料空Tray存在料盘，无法继续初始化！");
                        //    UpdateInitButtons(0);
                        //    return;
                        //}
                        //if ((CurInfo.motionIO[11].pel != 1) || (!CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInPosition + 25]))
                        //{
                        //    MessageBox.Show("上料满Tray存在料盘，无法继续初始化！");
                        //    UpdateInitButtons(0);
                        //    return;
                        //}
                        //if ((CurInfo.motionIO[12].pel != 1) || (!CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInPosition + 25]))
                        //{
                        //    MessageBox.Show("下料空Tray存在料盘，无法继续初始化！");
                        //    UpdateInitButtons(0);
                        //    return;
                        //}
                        //if ((CurInfo.motionIO[13].pel != 1) || (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInPosition + 25]))
                        //{
                        //    MessageBox.Show("下料满Tray存在料盘，无法继续初始化！");
                        //    UpdateInitButtons(0);
                        //    return;
                        //}
                        if (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInPosition + 25])
                        {
                            MessageBox.Show("上料空Tray存在料盘，无法继续初始化！");
                            UpdateInitButtons(0);
                            QuitInitProcess();
                            return;
                        }
                        if (!CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInPosition + 25])
                        {
                            MessageBox.Show("上料满Tray存在料盘，无法继续初始化！");
                            UpdateInitButtons(0);
                            QuitInitProcess();
                            return;
                        }
                        if (!CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInPosition + 25])
                        {
                            MessageBox.Show("下料空Tray存在料盘，无法继续初始化！");
                            UpdateInitButtons(0);
                            QuitInitProcess();
                            return;
                        }
                        if (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInPosition + 25])
                        {
                            MessageBox.Show("下料满Tray存在料盘，无法继续初始化！");
                            UpdateInitButtons(0);
                            QuitInitProcess();
                            return;
                        }
                        initStep = (int)InitStep.TrayZAxisHome;
                        break;
                    case (int)InitStep.TrayZAxisHome:
                        homemovedone = false;
                        HomeMove(10); HomeMove(11); HomeMove(12); HomeMove(13);
                        StartTime = DateTime.Now;
                        while (true)
                        {
                            homemovedone = adlink.CheckMoveDone(logicConfig.ECATAxis[4], ref errcode) && adlink.CheckMoveDone(logicConfig.ECATAxis[5], ref errcode) &&
                                           adlink.CheckMoveDone(logicConfig.ECATAxis[6], ref errcode) && adlink.CheckMoveDone(logicConfig.ECATAxis[7], ref errcode);
                            if (homemovedone == true)
                                break;
                            else
                            {
                                if (!OutTimeCount(StartTime, 40))
                                {
                                    WarningSolution("【85】初始化-Tray盘Z轴回零失败");
                                    QuitInitProcess();
                                    return;
                                }
                                Thread.Sleep(30);
                            }
                        }

                        if (adlink.LineMove(new Axis[] { logicConfig.ECATAxis[5], logicConfig.ECATAxis[6] }, new double[] { systemParam.TraySwitchLoadFullDownLimit, systemParam.TraySwitchUnloadNullDownLimit }, true, ref errcode, ref poserror) == false)
                        {
                            QuitInitProcess();
                            return;
                        }

                        if (adlink.LineMove(new Axis[] { logicConfig.ECATAxis[5], logicConfig.ECATAxis[6] }, new double[] { systemParam.TraySwitchLoadFullUpLimit, systemParam.TraySwitchUnloadNullUpLimit }, true, ref errcode, ref poserror) == false)
                        {
                            QuitInitProcess();
                            return;
                        }

                        UpdateWaringLog.Invoke((object)"初始化-Tay盘Z轴回零成功");
                        initStep = (int)InitStep.PulseAxisHome;
                        break;
                    case (int)InitStep.PulseAxisHome:
                        homemovedone = false;
                        HomeMove(1); HomeMove(2); HomeMove(3); HomeMove(4); HomeMove(5);
                        StartTime = DateTime.Now;
                        while (true)
                        {
                            homemovedone = adlink.CheckMoveDone(logicConfig.PulseAxis[1], ref errcode) && adlink.CheckMoveDone(logicConfig.PulseAxis[2], ref errcode) &&
                                           adlink.CheckMoveDone(logicConfig.PulseAxis[3], ref errcode) && adlink.CheckMoveDone(logicConfig.PulseAxis[4], ref errcode) &&
                                           adlink.CheckMoveDone(logicConfig.PulseAxis[5], ref errcode);
                            if (homemovedone == true)
                                break;
                            else
                            {
                                if (!OutTimeCount(StartTime, 60))
                                {
                                    WarningSolution("【86】初始化-脉冲轴(非主轴)回零失败");
                                    QuitInitProcess();
                                    return;
                                }
                                Thread.Sleep(30);
                            }
                        }
                        homemovedone = false;
                        HomeMoveZ(0, mainaxishomesafesignal);//主轴回零
                        StartTime = DateTime.Now;
                        while (true)
                        {
                            homemovedone = adlink.CheckMoveDone(logicConfig.PulseAxis[0], ref errcode);
                            if (homemovedone == true)
                                break;
                            else
                            {
                                if (!OutTimeCount(StartTime, 60))
                                {
                                    WarningSolution("【86】初始化-主轴回零失败");
                                    QuitInitProcess();
                                    return;
                                }
                                Thread.Sleep(30);
                            }
                        }
                        UpdateWaringLog.Invoke((object)"初始化-脉冲轴回零成功");
                        if (EnableBackLash)
                        {
                            adlink.BackLash_Enable(logicConfig.PulseAxis[1], CCDBackLashPulseNo);
                            adlink.BackLash_Enable(logicConfig.PulseAxis[2], CCDBackLashPulseNo);
                        }
                        initStep = (int)InitStep.ResetCCD;
                        break;
                    case (int)InitStep.ResetCCD:
                        if (tcp_enable)
                            TcpSendMsg("RS\r\n");
                        UpdateWaringLog.Invoke((object)("初始化-CCD复位成功"));
                        initStep = (int)InitStep.ResetLaser;
                        break;
                    case (int)InitStep.ResetLaser:
                        InitDataFlow();
                        InitDone = true;
                        UpdateWaringLog.Invoke((object)("初始化-Laser复位成功"));
                        break;
                }
                Thread.Sleep(30);
            }
            QuitInitProcess();

            ResetFlag();
            MessageBox.Show("初始化成功");
            UpdateWaringLog.Invoke((object)("初始化成功"));
            UpdateInitButtons(0);
            bInitEnvironmentFinished = true;
            return;
        }

        private void QuitInitProcess()
        {
            if (bMiniInit)
                MainFormMiniInitDoing = false;
            else
                MainFormInitDoing = false;
        }



        public void InitTrayZAxis()
        {
            InitLoadNullThread = new Thread(new ThreadStart(LoadNullTrayHome));
            InitLoadNullThread.IsBackground = true;
            InitLoadNullThread.Start();

            InitLoadFullThread = new Thread(new ThreadStart(LoadFullTrayHome));
            InitLoadFullThread.IsBackground = true;
            InitLoadFullThread.Start();

            InitUnloadNullThread = new Thread(new ThreadStart(UnloadNullTrayHome));
            InitUnloadNullThread.IsBackground = true;
            InitUnloadNullThread.Start();

            InitUnloadFullThread = new Thread(new ThreadStart(UnloadFullTrayHome));
            InitUnloadFullThread.IsBackground = true;
            InitUnloadFullThread.Start();

            DateTime starttime = DateTime.Now;
            while (true)
            {
                if (LoadNullTrayHomeFinished && LoadFullTrayHomeFinished && UnloadNullTrayHomeFinished && UnloadFullTrayHomeFinished)
                    break;
                else
                {
                    if (!OutTimeCount(starttime, 60))
                    {
                        WarningSolution("【87】Tray盘Z轴初始化失败");
                        return;
                    }
                    Thread.Sleep(50);
                }
            }

            LoadNullTrayHomeFinished = LoadFullTrayHomeFinished = UnloadNullTrayHomeFinished = UnloadFullTrayHomeFinished = false;

            if (UnloadTraySwitchOnce() == false)
            {
                WarningSolution("【88】下料Tray盘替换出错");
                return;
            }

            if (LoadFullTraySeparateStretch() == false)
                return;
            if (UnloadNullTraySeparateStretch() == false)
                return;

            //Thread InitUnloadTraySwitch = new Thread(new ThreadStart(UnloadTrayThread), 1024);
            //InitUnloadTraySwitch.IsBackground = true;
            //bInitTrayZAxis = true;
            //UnloadTrayEnable = true;
            //UnloadTrayFinished = false;
            //InitUnloadTraySwitch.Start();

            //starttime = DateTime.Now;
            //while (true)
            //{
            //    if (UnloadTrayFinished == true)
            //    {
            //        bInitTrayZAxis = false;
            //        UnloadTrayFinished = false;
            //        UnloadTrayEnable = false;
            //        CheckUnloadTrayNum();
            //        break;
            //    }
            //    else
            //    {
            //        if (!OutTimeCount(starttime, 40))
            //        {
            //            UpdateWaringLogNG.Invoke((object)("【88】下料Tray盘替换出错"));
            //            return;
            //        }
            //        Thread.Sleep(30);
            //    }
            //}

            //0825
            //LoadFullTrayNum = systemParam.TrayMaxNum; LoadNullTrayNum = 0;
            //UnloadNullTrayNum = systemParam.TrayMaxNum; UnloadFullTrayNum = 0;
            if (NGDrawerLock() == false)
                return;
            if (LoadNullTrayDrawerLock() == false)
                return;
            if (LoadFullTrayDrawerLock() == false)
                return;
            if (UnloadNullTrayDrawerLock() == false)
                return;
            if (UnloadFullTrayDrawerLock() == false)
                return;

            bInitTrayZFinished = true;
            UpdateInitButtons(1);
            UpdateWaringLog.Invoke((object)("Tray盘Z轴初始化成功"));
            MessageBox.Show("Tray盘Z轴初始化成功");
        }

        public void InitMainAxisTest()
        {
            DateTime StartTime = DateTime.Now;
            int errcode = 0;
            bool homemovedone = false;
            int initStep = 0;
            bool InitDone = false;

            while (!InitDone)
            {
                switch (initStep)
                {
                    case 0:
                        if (RetractAllCylinder())
                        {
                            WriteLog("载具缩回完成");
                            initStep = 1;
                        }
                        else
                        {
                            WarningSolution("【89】载具缩回发生错误");
                            return;
                        }
                        break;
                    case 1:
                        for (int i = 1; i < 5; i++)
                        {
                            adlink.HomeMove(logicConfig.PulseAxis[i]);
                        }

                        HomeMoveZ(0, mainaxishomesafesignal);
                        StartTime = DateTime.Now;
                        while (true)
                        {
                            homemovedone = adlink.CheckMoveDone(logicConfig.PulseAxis[0], ref errcode) && adlink.CheckMoveDone(logicConfig.PulseAxis[1], ref errcode) &&
                            adlink.CheckMoveDone(logicConfig.PulseAxis[2], ref errcode) && adlink.CheckMoveDone(logicConfig.PulseAxis[3], ref errcode) &&
                            adlink.CheckMoveDone(logicConfig.PulseAxis[4], ref errcode);
                            if (homemovedone == true)
                                break;
                            else
                            {
                                if (!OutTimeCount(StartTime, 30))
                                {
                                    WarningSolution("【90】脉冲轴回零失败");
                                    return;
                                }
                                Thread.Sleep(50);
                            }
                        }
                        initStep = 2;
                        break;
                    case 2:
                        ResetFlag();
                        InitDone = true;
                        break;
                }
                Thread.Sleep(30);
            }
            MessageBox.Show("初始化成功");
        }

        private bool InitCalib3DStd(ref string errString)
        {
            calib3D = new Calib3D();
            calib3D.calib3DStd = new Calib3DSturct[8];

            #region Actual Code
            if (!File.Exists(Calib3Dpath))
            {
                calib3D = new Calib3D();
                calib3D.calib3DStd = new Calib3DSturct[8];
                for (int k = 0; k < 8; k++)
                {
                    calib3D.calib3DStd[k] = new Calib3DSturct();
                    calib3D.calib3DStd[k].PathId = k;
                    calib3D.calib3DStd[k].a11 = 1;
                    calib3D.calib3DStd[k].a12 = 0;
                    calib3D.calib3DStd[k].a13 = 0;
                    calib3D.calib3DStd[k].a14 = 0;
                    calib3D.calib3DStd[k].a21 = 0;
                    calib3D.calib3DStd[k].a22 = 1;
                    calib3D.calib3DStd[k].a23 = 0;
                    calib3D.calib3DStd[k].a24 = 0;
                    calib3D.calib3DStd[k].a31 = 0;
                    calib3D.calib3DStd[k].a32 = 0;
                    calib3D.calib3DStd[k].a33 = 1;
                    calib3D.calib3DStd[k].a34 = 0;
                    calib3D.calib3DStd[k].a41 = 0;
                    calib3D.calib3DStd[k].a42 = 0;
                    calib3D.calib3DStd[k].a43 = 0;
                    calib3D.calib3DStd[k].a44 = 1;
                }
                bool result = XmlSerializerHelper.WriteXML((object)calib3D, Calib3Dpath, typeof(Calib3D));
                if (!result)
                {
                    errString = "Laser Calib 3D矩阵配置文件丢失";
                    return false;
                }
            }
            else
            {
                bool bFlag = false;
                calib3D = XmlSerializerHelper.ReadXML(Calib3Dpath, typeof(Calib3D), out bFlag) as Calib3D;
                if (null == calib3D)
                {
                    WarningSolution("【91】Laser Calib 3D矩阵配置文件读取失败");
                    errString = "Laser Calib 3D矩阵配置文件读取失败";
                    return false;
                }
                else
                {
                    UpdateWaringLog.Invoke((object)("Laser Calib 3D矩阵配置文件读取成功"));
                }
            }

            #endregion
            return true;
        }

        private bool InitTaskConfig()
        {
            taskConfig = new Task();
            //taskConfig.xStartPos = 10.0;
            //taskConfig.yStartPos = 10.0;
            //taskConfig.yEndPos = 0.0;
            //taskConfig.zTrigSafe = 30.0;
            //taskConfig.zNormal = 10.0;
            //taskConfig.rStand = 60.0;
            taskConfig.isSaveData = false;
            taskConfig.isSaveCalibData = false;

            return true;
        }
        //各工位服务的托盘编号更新0：A,1：B，C：2
        private void UpdateAllTrayNo()
        {
            //FinalResultList.Add(new int[8]);
            PartATrayNo--;
            PartBTrayNo--;
            PartCTrayNo--;
            if (PartATrayNo < 0) PartATrayNo = 2;
            if (PartBTrayNo < 0) PartBTrayNo = 2;
            if (PartCTrayNo < 0) PartCTrayNo = 2;
        }

        #region 上料Tray盘替换
        public int LoadTrayCircleCount = 0;
        public bool LoadTrayEnable = false;
        public bool LoadTrayFinished = true;
        public int LoadTrayStep = 0;
        public int LoadFullTraySuckTryCount = 0;

        public void LoadTrayThread()
        {
            int errcode = 0;
            LoadTrayCircleCount = 0;
            LoadTrayStep = 0;

            //开启数据收集线程
            while (AutoRunActive)
            {
                if (LoadTrayEnable)
                {
                    if (!LoadTrayFinished)
                    {
                        LoadTrayStatusSwitch(ref LoadTrayStep, ref errcode);
                    }
                }
                Thread.Sleep(40);
            }
        }

        private void LoadTrayStatusSwitch(ref int Step, ref int errcode)
        {
            double poserror = 100; int loadfullpos = 1000;
            try
            {
                if (isNewLoadTraySwitch)
                {
                    #region 新上料换盘逻辑
                    switch (Step)
                    {
                        case 0://上料XY轴避让
                            WriteLog("【上料Tray】case0：上料XY轴避让开始");
                            if (adlink.LineMove(new Axis[] { logicConfig.ECATAxis[0], logicConfig.ECATAxis[1] }, new double[] { systemParam.LoadTrayAvoidPosX, systemParam.LoadTrayAvoidPosY }, true, ref errcode, ref poserror))
                            {
                                WriteLog("【上料Tray】case0：上料XY轴避让结束");
                                Step = 1;
                            }
                            else
                            {
                                WarningSolution("【上料Tray】【报警】：【92】上料XY轴躲避上料Tray移动气缸错误");
                                LoadTrayErrorSolution(100);
                                return;
                            }
                            break;
                        case 1://上料Tray移动气缸缩回
                            if (LoadTrayMovePistonRetract())
                                Step = 2;
                            else
                            {
                                LoadTrayErrorSolution(100);
                                return;
                            }
                            break;
                        case 2://分Tray气缸缩回
                            if (LoadTraySeparateRetract())
                                Step = 3;
                            else
                            {
                                LoadTrayErrorSolution(100);
                                return;
                            }
                            break;
                        case 3:
                            if (adlink.P2PMove(logicConfig.ECATAxis[5], systemParam.LoadFullTrayUpDistance, false, ref errcode) == false)
                            {
                                adlink.P2PMove(logicConfig.ECATAxis[5], -1, false, ref errcode);
                                Step = 5;
                            }
                            else
                            {
                                Step = 4;
                            }
                            break;
                        case 4://分Tray气缸伸出
                            if (LoadFullTraySeparateStretch() == false)
                            {
                                LoadTrayErrorSolution(100);
                                return;
                            }
                            Step = 5;
                            break;
                        case 5://上料TrayZ轴下降
                            if (LoadTrayZStretch())
                                Step = 6;
                            else
                            {
                                LoadTrayErrorSolution(100);
                                return;
                            }
                            break;
                        case 6://上料Tray真空吸
                            if (LoadTrayVacumSuck())
                                Step = 7;
                            else
                            {
                                LoadTrayErrorSolution(100);
                                return;
                            }
                            break;
                        case 7://上料TrayZ轴上升
                            if (LoadTrayZRetract())
                                Step = 8;
                            else
                            {
                                LoadTrayErrorSolution(100);
                                return;
                            }
                            break;
                        case 8://上料Tray移动气缸伸出
                            if (LoadTrayMovePistonStretch())
                                Step = 9;
                            else
                            {
                                LoadTrayErrorSolution(100);
                                return;
                            }
                            break;
                        case 9://上料TrayZ轴下降
                            if (LoadTrayZStretch())
                                Step = 10;
                            else
                            {
                                LoadTrayErrorSolution(100);
                                return;
                            }
                            break;
                        case 10://上料Tray真空破
                            if (LoadTrayVacumBreak())
                                Step = 11;
                            else
                            {
                                LoadTrayErrorSolution(100);
                                return;
                            }
                            break;
                        case 11://上料TrayZ轴上升
                            if (LoadTrayZRetract())
                                Step = 12;
                            else
                            {
                                LoadTrayErrorSolution(100);
                                return;
                            }
                            break;
                        case 12:
                            WriteLog("【上料Tray】 case 12：上料Tray分盘气缸缩回开始");
                            if (LoadTraySeparateRetract())
                                Step = 13;
                            else
                            {
                                LoadTrayErrorSolution(100);
                                return;
                            }
                            break;
                        case 13:
                            if (LoadFullTraySwitchMove())
                            {
                                if (LoadNullTraySwitchMove())
                                    Step = 14;
                                else
                                {
                                    LoadTrayErrorSolution(100);
                                    return;
                                }
                            }
                            else
                            {
                                //Thread.Sleep(100);
                                if (CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInPosition + 25])
                                    Step = 15;
                                else
                                {
                                    LoadTrayErrorSolution(100);
                                    return;
                                }
                            }
                            break;
                        case 14:
                            WriteLog("【上料Tray】 case 15：上料Tray分盘气缸伸出开始");
                            if (LoadTraySeparateStretch())
                                Step = 16;
                            else
                            {
                                LoadTrayErrorSolution(100);
                                return;
                            }
                            break;
                        case 15:
                            WriteLog("【上料Tray】 case 15：上料Tray整体替换开始");
                            if (LoadFullTrayDrawerOpened == false)
                            {
                                LoadTrayErrorSolution(2);
                                return;
                            }
                            LoadTrayAllSwitchFinished = false;
                            DateTime starttime = DateTime.Now;
                            while (true)
                            {
                                if (LoadTrayAllSwitchFinished)
                                    break;
                                else
                                {
                                    if (!OutTimeCount(starttime, 70))
                                    {
                                        WarningSolution("上料Tray整体替换超时");
                                        LoadTrayErrorSolution(100);
                                        return;
                                    }
                                    Thread.Sleep(50);
                                }
                            }


                            WriteLog("【上料Tray】 case 15：上料Tray整体替换完成");
                            Step = 16;
                            break;
                        case 16:
                            Step = 0;
                            LoadTrayFinished = true;
                            LoadTrayCircleCount++;

                            if (isLoadTraySupplied == false)
                            {
                                adlink.GetCurPostion(logicConfig.ECATAxis[5].AxisId, ref loadfullpos);
                                if (loadfullpos >= (systemParam.TraySwitchLoadFullUpLimit - 30.0) * logicConfig.ECATAxis[5].Rate)
                                {
                                    StartBeep();
                                    if (MessageBox.Show("上料满Tray即将用完，请尽快换料", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                                    {
                                        isLoadTraySupplied = true;
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    #endregion
                }
                else
                {
                    switch (Step)
                    {
                        case 0://上料XY轴避让
                            WriteLog("【上料Tray】case0：上料XY轴避让开始");
                            if (adlink.LineMove(new Axis[] { logicConfig.ECATAxis[0], logicConfig.ECATAxis[1] }, new double[] { systemParam.LoadTrayAvoidPosX, systemParam.LoadTrayAvoidPosY }, true, ref errcode, ref poserror))
                            {
                                WriteLog("【上料Tray】case0：上料XY轴避让结束");
                                Step = 1;
                            }
                            else
                            {
                                WarningSolution("【上料Tray】【报警】：【92】上料XY轴躲避上料Tray移动气缸错误");
                                LoadTrayErrorSolution(100);
                                return;
                            }
                            break;
                        case 1://上料Tray移动气缸缩回
                            if (LoadTrayMovePistonRetract())
                                Step = 2;
                            else
                            {
                                LoadTrayErrorSolution(3);
                                return;
                            }
                            break;
                        case 2://上料TrayZ轴下降
                            if (LoadTrayZStretch())
                                Step = 3;
                            else
                            {
                                LoadTrayErrorSolution(4);
                                return;
                            }
                            break;
                        case 3://上料Tray真空吸
                            if (LoadTrayVacumSuck())
                            {
                                Step = 4;
                                LoadFullTraySuckTryCount = 0;
                            }
                            else
                            {
                                LoadFullTraySuckTryCount++;
                                LoadTrayErrorSolution(5);
                                if (LoadFullTraySuckTryCount >= 2)
                                {
                                    LoadFullTraySuckTryCount = 0;
                                    return;//重点观察
                                }
                            }
                            break;
                        case 4://分Tray气缸缩回
                            if (LoadTraySeparateRetract())
                                Step = 5;
                            else
                            {
                                LoadTrayErrorSolution(100);
                                return;
                            }
                            Thread.Sleep(500);
                            break;
                        case 5://上料TrayZ轴上升
                            if (LoadTrayZRetract())
                                Step = 6;
                            else
                            {
                                LoadTrayErrorSolution(100);
                                return;
                            }
                            break;
                        case 6://上料Tray移动气缸伸出
                            if (LoadTrayMovePistonStretch())
                                Step = 7;
                            else
                            {
                                LoadTrayErrorSolution(100);
                                return;
                            }
                            break;
                        case 7://上料TrayZ轴下降
                            if (LoadTrayZStretch())
                                Step = 8;
                            else
                            {
                                LoadTrayErrorSolution(6);
                                return;
                            }
                            break;
                        case 8://上料Tray真空破
                            if (LoadTrayVacumBreak())
                                Step = 9;
                            else
                            {
                                LoadTrayErrorSolution(100);
                                return;
                            }
                            break;
                        case 9://上料TrayZ轴上升
                            if (LoadTrayZRetract())
                                Step = 10;
                            else
                            {
                                LoadTrayErrorSolution(100);
                                return;
                            }
                            break;
                        case 10:
                            if (LoadFullTraySwitchMove())
                            {
                                if (LoadNullTraySwitchMove())
                                    Step = 11;
                                else
                                {
                                    LoadTrayErrorSolution(100);
                                    return;
                                }
                            }
                            else
                            {
                                //Thread.Sleep(100);
                                if (CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInPosition + 25])
                                    Step = 12;
                                else
                                {
                                    LoadTrayErrorSolution(100);
                                    return;
                                }
                            }
                            break;
                        case 11://分Tray气缸伸出
                            if (LoadTraySeparateStretch() == false)
                            {
                                LoadTrayErrorSolution(7);
                                return;
                            }
                            Step = 13;
                            break;
                        case 12:
                            WriteLog("【上料Tray】 case 12：上料Tray整体替换开始");
                            LoadTrayAllSwitchFinished = false;
                            DateTime starttime = DateTime.Now;
                            while (true)
                            {
                                if (LoadTrayAllSwitchFinished)
                                    break;
                                else
                                {
                                    if (!OutTimeCount(starttime, 50))
                                    {
                                        WarningSolution("上料Tray整体替换超时");
                                        LoadTrayErrorSolution(100);
                                        return;
                                    }
                                    Thread.Sleep(50);
                                }
                            }
                            WriteLog("【上料Tray】 case 12：上料Tray整体替换完成");
                            Step = 13;
                            break;
                        case 13:
                            Step = 0;
                            LoadTrayFinished = true;
                            LoadTrayCircleCount++;
                            if (isLoadTraySupplied == false)
                            {
                                adlink.GetCurPostion(logicConfig.ECATAxis[5].AxisId, ref loadfullpos);
                                if (loadfullpos >= (systemParam.TraySwitchLoadFullUpLimit - 30.0) * logicConfig.ECATAxis[5].Rate)
                                {
                                    StartBeep();
                                    if (MessageBox.Show("上料满Tray即将用完，请尽快换料", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                                    {
                                        isLoadTraySupplied = true;
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(ThreadAbortException))
                {
                    string exStr = "";
                    exStr += "【上料Tray】" + ex.ToString() + "\n";
                    exStr += "LoadTrayCircleCount=" + LoadTrayCircleCount.ToString() + "\n";
                    MessageBox.Show(exStr);
                }
            }
        }

        private bool LoadTrayMovePistonRetract()
        {
            WriteLog("【上料Tray】上料Tray移动气缸缩回开始");
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_LoadTrayMoveRetractControl, (int)ECATDONAME.Do_LoadTrayMoveStretchControl, (int)ECATDINAME.Di_LoadTrayMoveRetractBit, (int)ECATDINAME.Di_LoadTrayMoveStretchBit))
            {
                WriteLog("【上料Tray】上料Tray移动气缸缩回完成");
                return true;
            }
            else
            {
                WarningSolution("【上料Tray】【报警】【93】上料Tray盘移动气缸缩回超时：I24.00,I24.01");
                return false;
            }
        }

        private bool LoadTrayMovePistonStretch()
        {
            WriteLog("【上料Tray】上料Tray移动气缸伸出开始");
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_LoadTrayMoveStretchControl, (int)ECATDONAME.Do_LoadTrayMoveRetractControl, (int)ECATDINAME.Di_LoadTrayMoveStretchBit, (int)ECATDINAME.Di_LoadTrayMoveRetractBit))
            {
                WriteLog("【上料Tray】上料Tray移动气缸伸出完成");
                return true;
            }
            else
            {
                WarningSolution("【上料Tray】【报警】【94】上料Tray盘移动气缸伸出超时：I24.00,I24.01");
                return false;
            }
        }

        private bool LoadTrayVacumSuck()
        {
            WriteLog("【上料Tray】上料Tray真空吸取开始");
            if (WaitECATPiston2Cmd1FeedbackDone((int)ECATDONAME.Do_LoadTrayVacumSuck, (int)ECATDONAME.Do_LoadTrayVacumBreak, (int)ECATDINAME.Di_LoadTrayVacumCheck, true))
            {
                WriteLog("【上料Tray】上料Tray真空吸取完成");
                return true;
            }
            else
            {
                if (LoadFullTraySuckTryCount >= 1)
                    WarningSolution("【上料Tray】【报警】上料Tray盘吸取空Tray动作超时：I24.14");
                return false;
            }
        }

        private bool LoadTrayVacumBreak()
        {
            WriteLog("【上料Tray】上料Tray真空破开始");
            if (WaitECATPiston2Cmd1FeedbackDone((int)ECATDONAME.Do_LoadTrayVacumBreak, (int)ECATDONAME.Do_LoadTrayVacumSuck, (int)ECATDINAME.Di_LoadTrayVacumCheck, false))
            {
                IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadTrayVacumBreak, false);
                WriteLog("【上料Tray】上料Tray真空破完成");
                return true;
            }
            else
            {
                WarningSolution("【上料Tray】【报警】：【96】上料Tray盘放置空Tray动作超时：I24.14");
                return false;
            }
        }

        private bool LoadTraySeparateStretch()
        {
            bool tempsignal = false;
            WriteLog("【上料Tray】上料Tray盘分盘气缸伸出开始");
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadNullTrayRetractBit, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadNullTrayStretchBit, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadFullTrayRetractBit, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadFullTrayStretchBit, true);
            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInside1StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInside1RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInside2StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInside2RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayOutsideStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayOutsideRetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayUpsideStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayUpsideRetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInside1StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInside1RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInside2StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInside2RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayOutsideStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayOutsideRetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayUpsideStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayUpsideRetractBit + 25]);
                if (tempsignal == true)
                {
                    WriteLog("【上料Tray】上料Tray盘分盘气缸伸出完成");
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【上料Tray】【报警】：【97】上料Tray盘分盘气缸伸出超时：检查I26所有信号");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            //Thread.Sleep(300);
            return true;
        }

        private bool LoadFullTraySeparateStretch()
        {
            bool tempsignal = false;
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadFullTrayRetractBit, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadFullTrayStretchBit, true);
            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInside1StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInside1RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInside2StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInside2RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayOutsideStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayOutsideRetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayUpsideStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayUpsideRetractBit + 25]);
                if (tempsignal == true)
                {
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【98】上料满Tray盘分盘气缸伸出超时：检查I26.08~I26.15");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            //Thread.Sleep(300);
            return true;
        }

        private bool LoadNullTraySeparateStretch()
        {
            bool tempsignal = false;
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadNullTrayRetractBit, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadNullTrayStretchBit, true);
            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInside1StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInside1RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInside2StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInside2RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayOutsideStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayOutsideRetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayUpsideStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayUpsideRetractBit + 25]);
                if (tempsignal == true)
                {
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【99】上料空Tray盘分盘气缸伸出超时：检查I26.00~I26.07");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            //Thread.Sleep(300);
            return true;
        }

        private bool LoadTraySeparateRetract()
        {
            WriteLog("【上料Tray】上料Tray分盘气缸缩回开始");
            bool tempsignal = false;
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadNullTrayRetractBit, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadNullTrayStretchBit, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadFullTrayRetractBit, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadFullTrayStretchBit, false);
            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInside1StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInside1RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInside2StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInside2RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayOutsideStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayOutsideRetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayUpsideStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayUpsideRetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInside1StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInside1RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInside2StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInside2RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayOutsideStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayOutsideRetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayUpsideStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayUpsideRetractBit + 25];
                if (tempsignal == true)
                {
                    WriteLog("【上料Tray】上料Tray分盘气缸缩回完成");
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【上料Tray】【报警】：【100】上料Tray盘分盘气缸缩回超时：检查I26所有信号");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            //Thread.Sleep(300);
            return true;
        }

        private bool LoadFullTraySeparateRetract()
        {
            bool tempsignal = false;
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadFullTrayRetractBit, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadFullTrayStretchBit, false);
            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = (!CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInside1StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInside1RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInside2StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInside2RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayOutsideStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayOutsideRetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayUpsideStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayUpsideRetractBit + 25];
                if (tempsignal == true)
                {
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 5))
                    {
                        WarningSolution("【101】上料满Tray盘分盘气缸缩回超时：检查I26.08~I26.15");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            //Thread.Sleep(300);
            return true;
        }

        private bool LoadNullTraySeparateRetract()
        {
            bool tempsignal = false;
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadNullTrayRetractBit, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadNullTrayStretchBit, false);
            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInside1StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInside1RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInside2StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInside2RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayOutsideStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayOutsideRetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayUpsideStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayUpsideRetractBit + 25];
                if (tempsignal == true)
                {
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【102】上料空Tray盘分盘气缸缩回超时：检查I26.00~I26.07");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            //Thread.Sleep(300);
            return true;
        }

        private bool LoadTrayZStretch()
        {
            WriteLog("【上料Tray】上料Tray Z气缸伸出开始");
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_LoadTraySuckStretchControl, (int)ECATDONAME.Do_LoadTraySuckRetractControl, (int)ECATDINAME.Di_LoadTraySuckStretchBit, (int)ECATDINAME.Di_LoadTraySuckRetractBit))
            {
                WriteLog("【上料Tray】上料Tray Z气缸伸出结束");
                return true;
            }
            else
            {
                WarningSolution("【上料Tray】【报警】上料Tray盘上下动作气缸伸出超时：I24.02,I24.03");
                return false;
            }
        }

        private bool LoadTrayZRetract()
        {
            WriteLog("【上料Tray】上料Tray Z气缸缩回开始");
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_LoadTraySuckRetractControl, (int)ECATDONAME.Do_LoadTraySuckStretchControl, (int)ECATDINAME.Di_LoadTraySuckRetractBit, (int)ECATDINAME.Di_LoadTraySuckStretchBit))
            {
                WriteLog("【上料Tray】上料Tray Z气缸缩回完成");
                return true;
            }
            else
            {
                WarningSolution("【上料Tray】【报警】：【104】上料Tray盘上下动作气缸缩回超时：I24.02,I24.03");
                return false;
            }
        }

        private bool WaitLoadTrayFullInPosition()
        {
            DateTime starttime = DateTime.Now;
            while (true)
            {
                if (!CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInPosition + 25])
                {
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 30))
                    {
                        WarningSolution("【105】上料满Tray盘补充过程到位感应信号超时:I25.07");
                        return false;
                    }
                    Thread.Sleep(20);
                }
            }
            return true;
        }

        private bool WaitUnloadTrayNullInPosition()
        {
            DateTime starttime = DateTime.Now;
            while (true)
            {
                if (!CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInPosition + 25])
                {
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 30))
                    {
                        WarningSolution("【106】下料空Tray盘补充过程到位感应信号超时:I25.08");
                        return false;
                    }
                    Thread.Sleep(20);
                }
            }
            return true;
        }

        private bool WaitLoadTrayNullInPosition()
        {
            DateTime starttime = DateTime.Now;
            while (true)
            {
                if (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInPosition + 25])
                {
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 30))
                    {
                        WarningSolution("【107】上料空Tray盘补充过程到位感应信号超时:I25.06");
                        return false;
                    }
                    Thread.Sleep(10);
                }
            }
            return true;
        }

        private bool WaitUnloadTrayFullInPosition()
        {
            DateTime starttime = DateTime.Now;
            while (true)
            {
                if (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInPosition + 25])
                {
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 30))
                    {
                        WarningSolution("【108】下料满Tray盘补充过程到位感应信号超时:I25.09");
                        return false;
                    }
                    Thread.Sleep(10);
                }
            }
            return true;
        }

        private bool LoadNullTraySwitchMove()
        {
            int errcode = 0;
            WriteLog("【上料Tray】上料空Tray Z轴下降开始");
            if (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInPosition + 25])
            {
                if (adlink.P2PMove(logicConfig.ECATAxis[4], -systemParam.LoadTrayDistance, false, ref errcode) == false)
                    return false;
            }

            for (int i = 0; i < systemParam.TrayZMoveMaxCount; i++)//最多再走5次0.5mm
            {
                if (!CurInfo.Di[(int)ECATDINAME.Di_LoadNullTrayInPosition + 25])
                {
                    if (adlink.P2PMove(logicConfig.ECATAxis[4], -systemParam.LoadTrayDistanceSeg, false, ref errcode) == false)
                        return false;
                    Thread.Sleep(100);
                }
                else
                    break;
            }
            adlink.P2PMove(logicConfig.ECATAxis[4], -1.0, false, ref errcode);

            WriteLog("【上料Tray】上料空Tray Z轴下降完成");
            return true;
        }

        private bool LoadFullTraySwitchMove()
        {
            int errcode = 0;
            WriteLog("【上料Tray】上料满Tray Z轴上升开始");

            if (!isNewLoadTraySwitch)
            {
                if (CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInPosition + 25])
                {
                    if (adlink.P2PMove(logicConfig.ECATAxis[5], systemParam.LoadTrayDistance, false, ref errcode) == false)
                    {
                        adlink.P2PMove(logicConfig.ECATAxis[5], -3, false, ref errcode);
                        return false;
                    }
                }
            }

            for (int i = 0; i < systemParam.TrayZMoveMaxCount; i++)//最多再走5次0.5mm
            {
                if (CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInPosition + 25])
                {
                    if (adlink.P2PMove(logicConfig.ECATAxis[5], systemParam.LoadTrayDistanceSeg, false, ref errcode) == false)
                    {
                        adlink.P2PMove(logicConfig.ECATAxis[5], -3, false, ref errcode);
                        return false;
                    }
                    Thread.Sleep(100);
                }
                else
                    break;
            }
            //再往上走3mm
            if (adlink.P2PMove(logicConfig.ECATAxis[5], systemParam.LoadFullTrayFinishDistance, false, ref errcode) == false)
            {
                adlink.P2PMove(logicConfig.ECATAxis[5], -3, false, ref errcode);
                return false;
            }

            WriteLog("【上料Tray】上料满Tray Z轴上升结束");
            return true;
        }

        #endregion

        #region 下料Tray盘替换
        public int UnloadTrayCircleCount = 0;
        public bool UnloadTrayEnable = false;
        public bool UnloadTrayFinished = false;
        public int UnloadTrayStep = 0;
        public bool bInitTrayZAxis = false;//是否处于TrayZ初始化过程
        public bool ReadyForUnloadAllSwitch = false;
        private int UnloadNullTraySuckTryCount = 0;
        public void UnloadTrayThread()
        {
            int errcode = 0;
            UnloadTrayCircleCount = 0;

            //开启数据收集线程
            while (AutoRunActive || bInitTrayZAxis)
            {
                if (UnloadTrayEnable)
                {
                    if (!UnloadTrayFinished)
                    {
                        UnloadTrayStatusSwitch(ref UnloadTrayStep, ref errcode);
                    }
                }
                Thread.Sleep(40);
            }
        }

        public bool UnloadTraySwitchOnce()
        {
            int errcode = 0; double poserror = 0;

            if (adlink.LineMove(new Axis[] { logicConfig.ECATAxis[2], logicConfig.ECATAxis[3] }, new double[] { systemParam.UnloadTrayAvoidPosX, systemParam.UnloadTrayAvoidPosY }, true, ref errcode, ref poserror) == false)
                return false;

            if (UnloadTrayZStretch() == false)
                return false;

            if (UnloadTrayVacumSuck() == false)
                return false;

            if (UnloadTraySeparateRetract() == false)
                return false;

            if (UnloadTrayZRetract() == false)
                return false;

            if (UnloadTrayMovePistonRetract() == false)
                return false;

            if (UnloadTrayZStretch() == false)
                return false;

            if (UnloadTrayVacumBreak() == false)
                return false;

            if (UnloadTrayZRetract() == false)
                return false;

            if (UnloadNullTraySwitchMove() && UnloadFullTraySwitchMove() == false)
                return false;

            if (UnloadTraySeparateStretch() == false)
                return false;

            if (UnloadTrayMovePistonStretch() == false)
                return false;

            return true;
        }

        private void UnloadTrayStatusSwitch(ref int Step, ref int errcode)
        {
            double poserror = 0; int unloadnullpos = 1000;

            try
            {
                switch (Step)
                {
                    case 0:
                        if (ReadyForUnloadAllSwitch)
                        {
                            Step = 12;
                            break;
                        }
                        WriteLog("【下料Tray】case 0：下料龙门避让开始");
                        //下料龙门避让动作
                        if (adlink.LineMove(new Axis[] { logicConfig.ECATAxis[2], logicConfig.ECATAxis[3] }, new double[] { systemParam.UnloadTrayAvoidPosX, systemParam.UnloadTrayAvoidPosY }, true, ref errcode, ref poserror))
                        {
                            Step = 1;
                            WriteLog("【下料Tray】case 0：下料龙门避让完成");
                        }
                        else
                        {
                            UnloadTrayErrorSolution(0);
                            return;
                        }
                        break;
                    case 1://下料Tray Z轴伸出
                        WriteLog("【下料Tray】case 1：下料Tray Z轴伸出开始");
                        if (UnloadTrayZStretch())
                            Step = 2;
                        else
                        {
                            UnloadTrayErrorSolution(4);
                            return;
                        }
                        break;
                    case 2://下料Tray真空吸
                        WriteLog("【下料Tray】case 2：下料Tray真空吸开始");
                        UnloadTraySeparateStretch();
                        if (UnloadTrayVacumSuck())
                        {
                            Step = 3;
                            UnloadNullTraySuckTryCount = 0;
                        }
                        else
                        {
                            UnloadNullTraySuckTryCount++;
                            UnloadTrayErrorSolution(5);
                            if (UnloadNullTraySuckTryCount >= 2)
                            {
                                UnloadNullTraySuckTryCount = 0;
                                return;//重点观察
                            }
                        }
                        break;
                    case 3://下料Tray分盘气缸缩回
                        WriteLog("【下料Tray】case 3：下料Tray分盘气缸缩回开始");
                        if (UnloadTraySeparateRetract())
                            Step = 4;
                        else
                        {
                            UnloadTrayErrorSolution(100);
                            return;
                        }
                        break;
                    case 4://下料Tray Z轴缩回
                        WriteLog("【下料Tray】case 4：下料Tray Z气缸缩回开始");
                        if (UnloadTrayZRetract())
                            Step = 5;
                        else
                        {
                            UnloadTrayErrorSolution(100);
                            return;
                        }
                        break;
                    case 5://下料Tray移动气缸缩回
                        WriteLog("【下料Tray】case 5：下料Tray Z移动气缸缩回开始");
                        if (UnloadTrayMovePistonRetract())
                            Step = 6;
                        else
                        {
                            UnloadTrayErrorSolution(100);
                            return;
                        }
                        break;
                    case 6://下料Tray Z轴伸出
                        WriteLog("【下料Tray】case 6：下料Tray Z气缸伸出开始");
                        if (UnloadTrayZStretch())
                            Step = 7;
                        else
                        {
                            UnloadTrayErrorSolution(6);
                            return;
                        }
                        break;
                    case 7://下料Tray真空破
                        WriteLog("【下料Tray】case 7：下料Tray真空破开始");
                        if (UnloadTrayVacumBreak())
                            Step = 8;
                        else
                        {
                            UnloadTrayErrorSolution(100);
                            return;
                        }
                        break;
                    case 8://下料Tray Z轴缩回
                        WriteLog("【下料Tray】case 8：下料Tray Z气缸缩回开始");
                        if (UnloadTrayZRetract())
                            Step = 9;
                        else
                        {
                            UnloadTrayErrorSolution(100);
                            return;
                        }
                        break;
                    case 9:
                        WriteLog("【下料Tray】case 9：下料Tray移动气缸伸出开始");
                        if (UnloadTrayMovePistonStretch() == false)
                        {
                            UnloadTrayErrorSolution(100);
                            return;
                        }
                        Step = 10;
                        break;
                    case 10:
                        WriteLog("【下料Tray】case 10：下料Tray轴移动开始");
                        if (UnloadNullTraySwitchMove())
                        {
                            if (UnloadFullTraySwitchMove())
                                Step = 11;
                            else
                            {
                                UnloadTrayErrorSolution(100);
                                return;
                            }
                        }
                        else
                        {
                            if (UnloadFullTraySwitchMove() == false)
                            {
                                UnloadTrayErrorSolution(100);
                                return;
                            }

                            //if ((CurInfo.motionIO[12].pel == 1) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInPosition + 25]))
                            if (CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInPosition + 25])
                            {
                                if (ReadyForUnloadAllSwitch == false)
                                {
                                    ReadyForUnloadAllSwitch = true;
                                    Step = 11;
                                }
                            }
                            else
                            {
                                UnloadTrayErrorSolution(100);
                                return;
                            }
                        }
                        break;
                    case 11://下料Tray分盘气缸伸出
                        WriteLog("【下料Tray】case 11：下料Tray分盘气缸伸出开始");
                        if (UnloadTraySeparateStretch())
                            Step = 13;
                        else
                        {
                            UnloadTrayErrorSolution(7);
                            return;
                        }
                        break;
                    case 12:
                        WriteLog("【下料Tray】case 12:下料Tray整体替换开始");
                        if (UnloadNullTrayDrawerOpened == false)
                        {
                            UnloadTrayErrorSolution(3);
                            return;
                        }
                        UnloadTrayAllSwitchFinished = false;
                        DateTime starttime = DateTime.Now;
                        while (true)
                        {
                            if (UnloadTrayAllSwitchFinished)
                                break;
                            else
                            {
                                if (!OutTimeCount(starttime, 70))
                                {
                                    WarningSolution("下料Tray整体替换超时");
                                    UnloadTrayErrorSolution(100);
                                    return;
                                }
                                Thread.Sleep(50);
                            }
                        }
                        ReadyForUnloadAllSwitch = false;
                        WriteLog("【下料Tray】case 12:下料Tray整体替换完成");
                        Step = 13;
                        break;
                    case 13:
                        Step = 0;
                        UnloadTrayFinished = true;
                        UnloadTrayCircleCount++;
                        if (isUnloadTraySupplied == false)
                        {
                            adlink.GetCurPostion(logicConfig.ECATAxis[6].AxisId, ref unloadnullpos);
                            if (unloadnullpos >= (systemParam.TraySwitchUnloadNullUpLimit - 30.0) * logicConfig.ECATAxis[6].Rate)
                            {
                                StartBeep();
                                if (MessageBox.Show("下料空Tray即将用完，请尽快换料", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                                {
                                    isUnloadTraySupplied = true;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(ThreadAbortException))
                {
                    string exStr = "";
                    exStr += "【下料Tray】" + ex.ToString() + "\n";
                    exStr += "UnloadTrayCircleCount=" + UnloadTrayCircleCount.ToString() + "\n";
                    MessageBox.Show(exStr);
                }
            }
        }


        private bool UnloadTrayZStretch()
        {
            WriteLog("【下料Tray】下料Tray Z气缸伸出开始");
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_UnloadTraySuckStretchControl, (int)ECATDONAME.Do_UnloadTraySuckRetractControl, (int)ECATDINAME.Di_UnloadTraySuckStretchBit, (int)ECATDINAME.Di_UnloadTraySuckRetractBit))
            {
                WriteLog("【下料Tray】下料Tray Z气缸伸出完成");
                return true;
            }
            else
            {
                WarningSolution("【下料Tray】【报警】：【115】下料Tray盘上下动作气缸伸出超时:I24.12，I24.13");
                return false;
            }
        }

        private bool UnloadTrayZRetract()
        {
            WriteLog("【下料Tray】下料Tray Z气缸缩回开始");
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_UnloadTraySuckRetractControl, (int)ECATDONAME.Do_UnloadTraySuckStretchControl, (int)ECATDINAME.Di_UnloadTraySuckRetractBit, (int)ECATDINAME.Di_UnloadTraySuckStretchBit))
            {
                WriteLog("【下料Tray】下料Tray Z气缸缩回完成");
                return true;
            }
            else
            {
                WarningSolution("【下料Tray】【报警】：【116】下料Tray盘上下动作气缸缩回超时：I24.12，I24.13");
                return false;
            }
        }

        private bool UnloadTrayVacumSuck()
        {
            WriteLog("【下料Tray】下料Tray真空吸开始");
            if (WaitECATPiston2Cmd1FeedbackDone((int)ECATDONAME.Do_UnloadTrayVacumSuck, (int)ECATDONAME.Do_UnloadTrayVacumBreak, (int)ECATDINAME.Di_UnloadTrayVacumCheck, true))
            {
                WriteLog("【下料Tray】下料Tray真空吸完成");
                return true;
            }
            else
            {
                if (UnloadNullTraySuckTryCount >= 1)
                    WarningSolution("【下料Tray】【报警】：【117】下料Tray盘吸取满Tray动作超时：I24.15");
                return false;
            }
        }

        private bool UnloadTrayVacumBreak()
        {
            WriteLog("【下料Tray】下料Tray真空破开始");
            if (WaitECATPiston2Cmd1FeedbackDone((int)ECATDONAME.Do_UnloadTrayVacumBreak, (int)ECATDONAME.Do_UnloadTrayVacumSuck, (int)ECATDINAME.Di_UnloadTrayVacumCheck, false))
            {
                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadTrayVacumBreak, false);
                WriteLog("【下料Tray】下料Tray真空破完成");
                return true;
            }
            else
            {
                WarningSolution("【下料Tray】【报警】：【118】下料Tray盘放置满Tray动作超时：I24.15");
                return false;
            }
        }

        private bool UnloadTraySeparateStretch()
        {
            WriteLog("【下料Tray】分盘气缸伸出开始】");
            bool tempsignal = false;
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadNullTrayRetractBit, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadNullTrayStretchBit, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadFullTrayRetractBit, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadFullTrayStretchBit, true);
            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInside1StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInside1RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInside2StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInside2RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayOutsideStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayOutsideRetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayUpsideStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayUpsideRetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInside1StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInside1RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInside2StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInside2RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayOutsideStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayOutsideRetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayUpsideStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayUpsideRetractBit + 25]);
                if (tempsignal == true)
                {
                    WriteLog("【下料Tray】分盘气缸伸出完成】");
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【下料Tray】【报警】：【119】下料Tray盘分盘气缸伸出超时】：检查I27所有信号");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        private bool UnloadFullTraySeparateStretch()
        {
            bool tempsignal = false;
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadFullTrayRetractBit, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadFullTrayStretchBit, true);
            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInside1StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInside1RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInside2StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInside2RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayOutsideStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayOutsideRetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayUpsideStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayUpsideRetractBit + 25]);
                if (tempsignal == true)
                {
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【120】下料满Tray盘分盘气缸伸出超时：I27.08~I27.15");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        private bool UnloadNullTraySeparateStretch()
        {
            bool tempsignal = false;
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadNullTrayRetractBit, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadNullTrayStretchBit, true);
            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInside1StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInside1RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInside2StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInside2RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayOutsideStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayOutsideRetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayUpsideStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayUpsideRetractBit + 25]);

                if (tempsignal == true)
                {
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【121】下料空Tray盘分盘气缸伸出超时：I27.00~I27.07");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        private bool UnloadTraySeparateRetract()
        {
            WriteLog("【下料Tray】下料Tray分盘气缸缩回开始");
            bool tempsignal = false;
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadNullTrayRetractBit, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadNullTrayStretchBit, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadFullTrayRetractBit, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadFullTrayStretchBit, false);
            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = (!CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInside1StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInside1RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInside2StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInside2RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayOutsideStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayOutsideRetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayUpsideStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayUpsideRetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInside1StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInside1RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInside2StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInside2RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayOutsideStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayOutsideRetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayUpsideStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayUpsideRetractBit + 25];
                if (tempsignal == true)
                {
                    WriteLog("【下料Tray】下料Tray分盘气缸缩回完成");
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【下料Tray】【报警】：【122】下料Tray盘分盘气缸缩回超时：检查I27所有信号");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        private bool UnloadFullTraySeparateRetract()
        {
            bool tempsignal = false;
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadFullTrayRetractBit, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadFullTrayStretchBit, false);
            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInside1StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInside1RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInside2StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInside2RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayOutsideStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayOutsideRetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayUpsideStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayUpsideRetractBit + 25];
                if (tempsignal == true)
                {
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【123】下料满Tray盘分盘气缸缩回超时：I27.08~I27.15");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        private bool UnloadNullTraySeparateRetract()
        {
            bool tempsignal = false;
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadNullTrayRetractBit, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadNullTrayStretchBit, false);
            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = (!CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInside1StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInside1RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInside2StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInside2RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayOutsideStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayOutsideRetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayUpsideStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayUpsideRetractBit + 25];
                if (tempsignal == true)
                {
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【124】下料空Tray盘分盘气缸缩回超时：I27.00~I27.07");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        private bool UnloadNullTraySwitchMove()
        {
            WriteLog("【下料Tray】下料空Tray Z轴移动开始");
            int errcode = 0;

            if (CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInPosition + 25])
            {
                if (adlink.P2PMove(logicConfig.ECATAxis[6], systemParam.LoadTrayDistance, false, ref errcode) == false)
                {
                    adlink.P2PMove(logicConfig.ECATAxis[6], -3, false, ref errcode);
                    return false;
                }
            }

            for (int i = 0; i < systemParam.TrayZMoveMaxCount; i++)
            {
                if (CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInPosition + 25])
                {
                    if (adlink.P2PMove(logicConfig.ECATAxis[6], systemParam.LoadTrayDistanceSeg, false, ref errcode) == false)
                    {
                        adlink.P2PMove(logicConfig.ECATAxis[6], -3, false, ref errcode);
                        return false;
                    }
                    else
                        Thread.Sleep(100);
                }
                else
                    break;
            }

            if (adlink.P2PMove(logicConfig.ECATAxis[6], systemParam.UnloadNullTrayFinishDistance, false, ref errcode) == false)
            {
                adlink.P2PMove(logicConfig.ECATAxis[6], -3, false, ref errcode);
                return false;
            }

            WriteLog("【下料Tray】下料空Tray Z轴移动完成");
            return true;
        }

        private bool UnloadFullTraySwitchMove()
        {
            WriteLog("【下料Tray】下料满Tray Z轴移动开始");
            int errcode = 0;

            if (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInPosition + 25])
            {
                if (adlink.P2PMove(logicConfig.ECATAxis[7], -systemParam.LoadTrayDistance, false, ref errcode) == false)
                    return false;
            }

            for (int i = 0; i < systemParam.TrayZMoveMaxCount; i++)
            {
                if (!CurInfo.Di[(int)ECATDINAME.Di_UnloadFullTrayInPosition + 25])
                {
                    if (adlink.P2PMove(logicConfig.ECATAxis[7], -systemParam.LoadTrayDistanceSeg, false, ref errcode) == false)
                        return false;
                    else
                        Thread.Sleep(100);
                }
                else
                    break;
            }
            adlink.P2PMove(logicConfig.ECATAxis[7], systemParam.UnloadFullFinishDistance, false, ref errcode);

            WriteLog("【下料Tray】下料满Tray Z轴移动完成");
            return true;
        }

        private bool UnloadTrayMovePistonStretch()
        {
            WriteLog("【下料Tray】移动气缸伸出开始");
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_UnloadTrayMoveStretchControl, (int)ECATDONAME.Do_UnloadTrayMoveRetractControl, (int)ECATDINAME.Di_UnloadTrayMoveStretchBit, (int)ECATDINAME.Di_UnloadTrayMoveRetractBit))
            {
                WriteLog("【下料Tray】移动气缸伸出完成");
                return true;
            }
            else
            {
                WarningSolution("【下料Tray】【报警】：【127】下料Tray盘移动气缸伸出超时:I24.10,I24.11");
                return false;
            }
        }

        private bool UnloadTrayMovePistonRetract()
        {
            WriteLog("【下料Tray】移动气缸缩回开始");
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_UnloadTrayMoveRetractControl, (int)ECATDONAME.Do_UnloadTrayMoveStretchControl, (int)ECATDINAME.Di_UnloadTrayMoveRetractBit, (int)ECATDINAME.Di_UnloadTrayMoveStretchBit))
            {
                WriteLog("【下料Tray】移动气缸缩回完成");
                return true;
            }
            else
            {
                WarningSolution("【下料Tray】【报警】：【128】下料Tray盘移动气缸缩回超时:I24.10,I24.11");
                return false;
            }
        }


        #endregion

        #region 上料龙门取放料
        public int LoadGantryCircleCount = 0;
        public bool LoadGantryEnable = false;
        public bool LoadGantrySuckFinished = false;//8片取放完成的标志位
        public int LoadGantryStep = 0;
        public MotionPos LoadGantryMotionPos;
        public int CurLoadFullTraySeq = 0;//当前上料满Tray放置物料的顺序（0~14）
        public int LoadTraySuckTryCount = 0;

        //还需要充分使用LoadGantryTrayFinished和LoadGantrySuckFinished
        public void LoadGantryThread()
        {
            int errcode = 0;
            //开启数据收集线程
            while (AutoRunActive)
            {
                if (LoadGantryEnable)
                {
                    LoadGantryStatusSwitch(ref LoadGantryStep, ref errcode);
                }
                Thread.Sleep(40);
            }
        }

        private bool WaitLoadModuleInPos()
        {
            DateTime starttime = DateTime.Now;
            bool tempsignal = false;
            while (true)
            {
                tempsignal = CurInfo.Di[(int)ECATDINAME.Di_LoadMoveRetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadMoveStretchBit + 25]) &&
                           CurInfo.Di[(int)ECATDINAME.Di_LoadMotionMoveRetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadMotionMoveStretchBit + 25]) &&
                           (LoadModuleEnable == true);
                if (tempsignal)
                    break;
                else
                {
                    if (!OutTimeCount(starttime, 90))
                    {
                        WarningSolution("【129】龙门上料等待上料模组到位超时:I23.02,I23.03,I23.04,I23.05");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        public bool LoadGantryAllSuckClose()
        {
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumBreak, false);
            return true;
        }
        public int suckNum = 0;
        private void LoadGantryStatusSwitch(ref int Step, ref int errcode)//0~14
        {
            try
            {
                #region 上料龙门情况1
                if ((CurLoadFullTraySeq <= 5) || (CurLoadFullTraySeq >= 9 && CurLoadFullTraySeq <= 14))
                {
                    //********************樊竞明20181001******************************//
                    if (CurLoadFullTraySeq == 1 && !isInputProductBatch && systemParam.isCheckInPutProductionRecordHourBeat == 1)
                    {
                        StartBeep();
                        if (MessageBox.Show("请先将本批次产品的模穴号输入到主界面“检测编号”" + "\r\n" + "若跟上批次相同，请直接点“取消”", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
                        {
                            isInputProductBatch = true;
                        }
                        else
                        {
                            WarningSolution("【提示】提示操作员在主界面输入检测产品的批次/模穴编号");
                            //return;
                        }

                    }
                    //***************************************************************//

                    switch (Step)
                    {
                        case 0:
                            WriteLog("【上料龙门】case 0:上料龙门开始执行，CurLoadFullTraySeq=" + CurLoadFullTraySeq.ToString());
                            LoadGantryAllPistonZRetract();//确保Z气缸&缓存气缸已缩回
                            if (LoadGantryMoveAxis(2 * CurLoadFullTraySeq) == false)//0~29
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 1;
                            break;
                        case 1:
                            WriteLog("【上料龙门】case 1:上料龙门右侧气缸&Z气缸伸出开始");
                            if (LoadGantryRightPistonZStretch() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 2;
                            break;
                        case 2:
                            WriteLog("【上料龙门】case 2:上料龙门右侧吸嘴吸取开始");
                            if (LoadGantryRightSuckerSuck() == false)
                            {
                                LoadGantryErrorSolution(0);
                                return;
                            }
                            else
                                Step = 3;
                            break;
                        case 3:
                            WriteLog("【上料龙门】case 3:上料龙门右侧吸嘴&Z气缸缩回开始");
                            if (LoadGantryRightPistonZRetract() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 50;
                            break;
                        case 50:
                            WriteLog("【上料龙门】case 50:上料龙门判断右侧吸嘴反馈信号");
                            if (!CurInfo.Di[(int)ECATDINAME.Di_LoadRightVacumCheck + 25])
                            {
                                suckNum++;
                                if (suckNum <= 1)
                                {
                                    Step = 0;
                                }
                                else
                                {
                                    suckNum = 0;
                                    #region 右侧料放回
                                    if (LoadGantryMoveAxis(2 * CurLoadFullTraySeq) == false)//0~29
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightPistonZStretch() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightSuckerBreak() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightPistonZRetract() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    #endregion
                                    Step = 0;
                                    if (CurLoadFullTraySeq != 14)
                                        CurLoadFullTraySeq++;
                                    else
                                    {
                                        if (((bDelayStop || bDelayStopCount) && (LoadGantryCircleCount + 1 >= iDelayStopCount)) == false)
                                        {
                                            WriteLog("【上料龙门】等待上料Tray换盘开始");
                                            LoadTrayFinished = false;
                                            DateTime starttime = DateTime.Now;
                                            while (true)
                                            {
                                                if (LoadTrayFinished)
                                                    break;
                                                else
                                                {
                                                    if (!OutTimeCount(starttime, 70))
                                                    {
                                                        WarningSolution("上料Tray换盘超时");
                                                        LoadGantryErrorSolution(100);
                                                        return;
                                                    }
                                                    Thread.Sleep(30);
                                                }
                                            }
                                            WriteLog("【上料龙门】等待上料Tray换盘完成");
                                            CurLoadFullTraySeq = 0;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                suckNum = 0;
                                Step = 4;
                            }
                            break;
                        case 4:
                            WriteLog("【上料龙门】case 4:上料龙门移动至开始" + (2 * CurLoadFullTraySeq + 1).ToString() + "位置开始");
                            if (LoadGantryMoveAxis(2 * CurLoadFullTraySeq + 1) == false)//0~29
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 5;
                            break;
                        case 5:
                            WriteLog("【上料龙门】case 5:上料龙门左侧吸嘴&Z气缸伸出开始");
                            if (LoadGantryLeftPistonZStretch() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 6;
                            break;
                        case 6:
                            WriteLog("【上料龙门】case 6:上料龙门左侧吸嘴吸取开始");
                            if (LoadGantryLeftSuckerSuck() == false)
                            {
                                LoadGantryErrorSolution(1);
                                return;
                            }
                            else
                                Step = 7;
                            break;
                        case 7:
                            WriteLog("【上料龙门】case 7:上料龙门左侧吸嘴&Z气缸缩回开始");
                            if (LoadGantryLeftPistonZRetract() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 75;
                            break;
                        case 75:
                            WriteLog("【上料龙门】case 75:上料龙门判断左反馈信号");
                            if (!CurInfo.Di[(int)ECATDINAME.Di_LoadLeftVacumCheck + 25])
                            {
                                suckNum++;
                                if (suckNum <= 1)
                                {
                                    Step = 4;
                                }
                                else
                                {
                                    suckNum = 0;
                                    #region 把料放回
                                    if (LoadGantryMoveAxis(2 * CurLoadFullTraySeq) == false)//0~29
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightPistonZStretch() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightSuckerBreak() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightPistonZRetract() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryMoveAxis(2 * CurLoadFullTraySeq + 1) == false)//0~29
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftPistonZStretch() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftSuckerBreak() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftPistonZRetract() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    #endregion
                                    LoadGantryStep = 0;
                                    if (CurLoadFullTraySeq != 14)
                                        CurLoadFullTraySeq++;
                                    else
                                    {
                                        if (((bDelayStop || bDelayStopCount) && (LoadGantryCircleCount + 1 >= iDelayStopCount)) == false)
                                        {
                                            WriteLog("【上料龙门】等待上料Tray换盘开始");
                                            LoadTrayFinished = false;
                                            DateTime starttime = DateTime.Now;
                                            while (true)
                                            {
                                                if (LoadTrayFinished)
                                                    break;
                                                else
                                                {
                                                    if (!OutTimeCount(starttime, 70))
                                                    {
                                                        WarningSolution("上料Tray换盘超时");
                                                        LoadGantryErrorSolution(100);
                                                        return;
                                                    }
                                                    Thread.Sleep(30);
                                                }
                                            }
                                            WriteLog("【上料龙门】等待上料Tray换盘完成");
                                            CurLoadFullTraySeq = 0;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                suckNum = 0;
                                Step = 100;
                            }
                            break;
                        case 100:
                            WriteLog("【上料龙门】case 100:上料龙门判断左右反馈信号 无反馈再吸一次");
                            if ((!CurInfo.Di[(int)ECATDINAME.Di_LoadLeftVacumCheck + 25]) || (!CurInfo.Di[(int)ECATDINAME.Di_LoadRightVacumCheck + 25]))
                            {
                                if (!CurInfo.Di[(int)ECATDINAME.Di_LoadRightVacumCheck + 25])//右边
                                {
                                    if (LoadGantryMoveAxis(2 * CurLoadFullTraySeq) == false)//0~29
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightPistonZStretch() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightSuckerSuck() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightPistonZRetract() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                }
                                else if (!CurInfo.Di[(int)ECATDINAME.Di_LoadLeftVacumCheck + 25])//左边
                                {
                                    if (LoadGantryMoveAxis(2 * CurLoadFullTraySeq + 1) == false)//0~29
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftPistonZStretch() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftSuckerSuck() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftPistonZRetract() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                }
                                Step = 200;
                            }
                            else
                            {
                                Step = 8;
                            }
                            break;
                        case 200:
                            WriteLog("【上料龙门】case 200:上料龙门判断左右反馈信号 无反馈放完料直接走");
                            if ((!CurInfo.Di[(int)ECATDINAME.Di_LoadLeftVacumCheck + 25]) || (!CurInfo.Di[(int)ECATDINAME.Di_LoadRightVacumCheck + 25]))
                            {
                                #region 把料放回
                                if (LoadGantryMoveAxis(2 * CurLoadFullTraySeq) == false)//0~29
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryRightPistonZStretch() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryRightSuckerBreak() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryRightPistonZRetract() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryMoveAxis(2 * CurLoadFullTraySeq + 1) == false)//0~29
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryLeftPistonZStretch() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryLeftSuckerBreak() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryLeftPistonZRetract() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                #endregion
                                Step = 0;
                                if (CurLoadFullTraySeq != 14)
                                    CurLoadFullTraySeq++;
                                else
                                {
                                    if (((bDelayStop || bDelayStopCount) && (LoadGantryCircleCount + 1 >= iDelayStopCount)) == false)
                                    {
                                        WriteLog("【上料龙门】等待上料Tray换盘开始");
                                        LoadTrayFinished = false;
                                        DateTime starttime = DateTime.Now;
                                        while (true)
                                        {
                                            if (LoadTrayFinished)
                                                break;
                                            else
                                            {
                                                if (!OutTimeCount(starttime, 70))
                                                {
                                                    WarningSolution("上料Tray换盘超时");
                                                    LoadGantryErrorSolution(100);
                                                    return;
                                                }
                                                Thread.Sleep(30);
                                            }
                                        }
                                        WriteLog("【上料龙门】等待上料Tray换盘完成");
                                        CurLoadFullTraySeq = 0;
                                    }
                                }
                            }
                            else
                            {
                                Step = 8;
                            }
                            break;
                        case 8:
                            WriteLog("【上料龙门】case 8:上料龙门移动至上料模组放料位开始");
                            LoadGantryAllPistonZRetract();//确保Z气缸&缓存气缸已缩回
                            if (LoadGantryMoveToLoadModule() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 9;
                            break;
                        case 9:
                            WriteLog("【上料龙门】case 9:上料龙门等待上料模组到位开始");
                            if (!debugThreadLoadGantry)
                            {
                                if (WaitLoadModuleInPos() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                else
                                    Step = 10;
                            }
                            else
                                Step = 10;
                            break;
                        case 10:
                            WriteLog("【上料龙门】case 10:上料龙门所有吸嘴&Z气缸伸出开始");
                            if (LoadGantryAllPistonZStretch() == false)
                            {
                                LoadGantryErrorSolution(5);
                                return;
                            }
                            else
                                Step = 11;
                            break;
                        case 11:
                            WriteLog("【上料龙门】case 11:上料龙门所有吸嘴真空破开始");
                            Thread.Sleep(200);
                            if (LoadGantryAllSuckerBreak() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 12;
                            break;
                        case 12:
                            WriteLog("【上料龙门】case 12:上料龙门所有气缸&Z气缸缩回开始");
                            if (LoadGantryAllPistonZRetract() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 13;
                            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumBreak, false);
                            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumBreak, false);

                            break;
                        case 13:
                            WriteLog("【上料龙门】case 13:上料龙门收尾工作开始");
                            LoadModuleFinished = false;//上料模组开始工作

                            if (CurLoadFullTraySeq != 14)
                                CurLoadFullTraySeq++;
                            else
                            {
                                if (((bDelayStop || bDelayStopCount) && (LoadGantryCircleCount + 1 >= iDelayStopCount)) == false)
                                {
                                    WriteLog("【上料龙门】等待上料Tray换盘开始");
                                    LoadTrayFinished = false;
                                    DateTime starttime = DateTime.Now;
                                    while (true)
                                    {
                                        if (LoadTrayFinished)
                                            break;
                                        else
                                        {
                                            if (!OutTimeCount(starttime, 70))
                                            {
                                                WarningSolution("上料Tray换盘超时");
                                                LoadGantryErrorSolution(100);
                                                return;
                                            }
                                            Thread.Sleep(30);
                                        }
                                    }
                                    WriteLog("【上料龙门】等待上料Tray换盘完成");
                                    CurLoadFullTraySeq = 0;
                                }
                            }
                            LoadGantryCircleCount++;
                            LoadGantrySuckFinished = true;
                            Step = 0;
                            //若按下DelayStop，则执行完当前的上料龙门后，就不再执行
                            if (bDelayStop)
                            {
                                LoadGantryEnable = false;
                            }
                            if (bDelayStopCount)
                            {
                                if (LoadGantryCircleCount >= iDelayStopCount)
                                    LoadGantryEnable = false;
                            }
                            return;
                    }
                }
                #endregion

                #region 上料龙门情况2
                if (CurLoadFullTraySeq == 6)
                {
                    switch (Step)
                    {
                        case 0:
                            WriteLog("【上料龙门】case 0:上料龙门开始执行，CurLoadFullTraySeq=" + CurLoadFullTraySeq.ToString());
                            LoadGantryAllPistonZRetract();//确保Z气缸&缓存气缸已缩回
                            if (LoadGantryMoveAxis(2 * 6) == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 1;
                            break;
                        case 1:
                            WriteLog("【上料龙门】case 1:上料龙门左侧气缸&Z气缸伸出开始");
                            if (LoadGantryLeftPistonZStretch() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 2;
                            break;
                        case 2:
                            WriteLog("【上料龙门】case 2:上料龙门左侧吸嘴吸取开始");
                            if (LoadGantryLeftSuckerSuck() == false)
                            {
                                LoadGantryErrorSolution(2);
                                return;
                            }
                            else
                                Step = 3;
                            break;
                        case 3:
                            WriteLog("【上料龙门】case 3:上料龙门左侧吸嘴&Z气缸缩回开始");
                            if (LoadGantryLeftPistonZRetract() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 50;
                            break;
                        case 50:
                            WriteLog("【上料龙门】case 50:上料龙门判断左侧吸嘴反馈信号");
                            if (!CurInfo.Di[(int)ECATDINAME.Di_LoadLeftVacumCheck + 25])
                            {
                                suckNum++;
                                if (suckNum <= 1)
                                {
                                    LoadGantryStep = 0;
                                }
                                else
                                {
                                    suckNum = 0;
                                    #region 左侧料放回
                                    if (LoadGantryMoveAxis(2 * 6) == false)//0~29
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftPistonZStretch() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftSuckerBreak() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftPistonZRetract() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    #endregion
                                    LoadGantryStep = 0;
                                    if (CurLoadFullTraySeq != 14)
                                        CurLoadFullTraySeq++;
                                    else
                                    {
                                        if (((bDelayStop || bDelayStopCount) && (LoadGantryCircleCount + 1 >= iDelayStopCount)) == false)
                                        {
                                            WriteLog("【上料龙门】等待上料Tray换盘开始");
                                            LoadTrayFinished = false;
                                            DateTime starttime = DateTime.Now;
                                            while (true)
                                            {
                                                if (LoadTrayFinished)
                                                    break;
                                                else
                                                {
                                                    if (!OutTimeCount(starttime, 70))
                                                    {
                                                        WarningSolution("上料Tray换盘超时");
                                                        LoadGantryErrorSolution(100);
                                                        return;
                                                    }
                                                    Thread.Sleep(30);
                                                }
                                            }
                                            WriteLog("【上料龙门】等待上料Tray换盘完成");
                                            CurLoadFullTraySeq = 0;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                suckNum = 0;
                                Step = 4;
                            }
                            break;
                        case 4:
                            WriteLog("【上料龙门】case 4:上料龙门移动至开始" + (2 * CurLoadFullTraySeq + 1).ToString() + "位置开始");
                            if (LoadGantryMoveAxis(2 * 6 + 1) == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 5;
                            break;
                        case 5:
                            WriteLog("【上料龙门】case 5:上料龙门右侧吸嘴&Z气缸伸出开始");
                            if (LoadGantryRightPistonZStretch() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 6;
                            break;
                        case 6:
                            WriteLog("【上料龙门】case 6:上料龙门右侧吸嘴吸取开始");
                            if (LoadGantryRightSuckerSuck() == false)
                            {
                                LoadGantryErrorSolution(3);
                                return;
                            }
                            else
                                Step = 7;
                            break;
                        case 7:
                            WriteLog("【上料龙门】case 7:上料龙门右侧吸嘴&Z气缸缩回开始");
                            if (LoadGantryRightPistonZRetract() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 75;
                            break;
                        case 75:
                            WriteLog("【上料龙门】case 100:上料龙门判断右反馈信号");
                            if (!CurInfo.Di[(int)ECATDINAME.Di_LoadRightVacumCheck + 25])
                            {
                                suckNum++;
                                if (suckNum <= 1)
                                {
                                    LoadGantryStep = 4;
                                }
                                else
                                {
                                    suckNum = 0;
                                    #region 把料放回
                                    if (LoadGantryMoveAxis(2 * 6) == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftPistonZStretch() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftSuckerBreak() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftPistonZRetract() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryMoveAxis(2 * 6 + 1) == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightPistonZStretch() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightSuckerBreak() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightPistonZRetract() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    #endregion
                                    LoadGantryStep = 0;
                                    if (CurLoadFullTraySeq != 14)
                                        CurLoadFullTraySeq++;
                                    else
                                    {
                                        if (((bDelayStop || bDelayStopCount) && (LoadGantryCircleCount + 1 >= iDelayStopCount)) == false)
                                        {
                                            WriteLog("【上料龙门】等待上料Tray换盘开始");
                                            LoadTrayFinished = false;
                                            DateTime starttime = DateTime.Now;
                                            while (true)
                                            {
                                                if (LoadTrayFinished)
                                                    break;
                                                else
                                                {
                                                    if (!OutTimeCount(starttime, 70))
                                                    {
                                                        WarningSolution("上料Tray换盘超时");
                                                        LoadGantryErrorSolution(100);
                                                        return;
                                                    }
                                                    Thread.Sleep(30);
                                                }
                                            }
                                            WriteLog("【上料龙门】等待上料Tray换盘完成");
                                            CurLoadFullTraySeq = 0;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                suckNum = 0;
                                Step = 100;
                            }
                            break;
                        case 100:
                            WriteLog("【上料龙门】case 100:上料龙门判断左右反馈信号 无反馈再吸一次");
                            if ((!CurInfo.Di[(int)ECATDINAME.Di_LoadLeftVacumCheck + 25]) || (!CurInfo.Di[(int)ECATDINAME.Di_LoadRightVacumCheck + 25]))
                            {
                                if (!CurInfo.Di[(int)ECATDINAME.Di_LoadRightVacumCheck + 25])//右边
                                {
                                    if (LoadGantryMoveAxis(2 * 6 + 1) == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightPistonZStretch() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightSuckerSuck() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightPistonZRetract() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                }
                                else if (!CurInfo.Di[(int)ECATDINAME.Di_LoadLeftVacumCheck + 25])//左边
                                {
                                    if (LoadGantryMoveAxis(2 * 6) == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftPistonZStretch() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftSuckerSuck() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftPistonZRetract() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                }
                                Step = 200;
                            }
                            else
                            {
                                Step = 8;
                            }
                            break;
                        case 200:
                            WriteLog("【上料龙门】case 200:上料龙门判断左右反馈信号 无反馈放完料直接走");
                            if ((!CurInfo.Di[(int)ECATDINAME.Di_LoadLeftVacumCheck + 25]) || (!CurInfo.Di[(int)ECATDINAME.Di_LoadRightVacumCheck + 25]))
                            {
                                #region 把料放回
                                if (LoadGantryMoveAxis(2 * 6 + 1) == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryRightPistonZStretch() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryRightSuckerBreak() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryRightPistonZRetract() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryMoveAxis(2 * 6) == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryLeftPistonZStretch() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryLeftSuckerBreak() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryLeftPistonZRetract() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                #endregion
                                LoadGantryStep = 0;
                                if (CurLoadFullTraySeq != 14)
                                    CurLoadFullTraySeq++;
                                else
                                {
                                    if (((bDelayStop || bDelayStopCount) && (LoadGantryCircleCount + 1 >= iDelayStopCount)) == false)
                                    {
                                        WriteLog("【上料龙门】等待上料Tray换盘开始");
                                        LoadTrayFinished = false;
                                        DateTime starttime = DateTime.Now;
                                        while (true)
                                        {
                                            if (LoadTrayFinished)
                                                break;
                                            else
                                            {
                                                if (!OutTimeCount(starttime, 70))
                                                {
                                                    WarningSolution("上料Tray换盘超时");
                                                    LoadGantryErrorSolution(100);
                                                    return;
                                                }
                                                Thread.Sleep(30);
                                            }
                                        }

                                        WriteLog("【上料龙门】等待上料Tray换盘完成");
                                        CurLoadFullTraySeq = 0;
                                    }
                                }
                            }
                            else
                            {
                                Step = 8;
                            }
                            break;
                        case 8:
                            WriteLog("【上料龙门】case 8:上料龙门移动至上料模组放料位开始");
                            LoadGantryAllPistonZRetract();//确保Z气缸&缓存气缸已缩回
                            if (LoadGantryMoveToLoadModule() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 9;
                            break;
                        case 9:
                            WriteLog("【上料龙门】case 9:上料龙门等待上料模组到位开始");
                            if (!debugThreadLoadGantry)
                            {
                                if (WaitLoadModuleInPos() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                else
                                    Step = 10;
                            }
                            else
                                Step = 10;
                            break;
                        case 10:
                            WriteLog("【上料龙门】case 10:上料龙门所有吸嘴&Z气缸伸出开始");
                            if (LoadGantryAllPistonZStretch() == false)
                            {
                                LoadGantryErrorSolution(5);
                                return;
                            }
                            else
                                Step = 11;
                            break;
                        case 11:
                            WriteLog("【上料龙门】case 11:上料龙门所有吸嘴真空破开始");
                            Thread.Sleep(200);
                            if (LoadGantryAllSuckerBreak() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 12;
                            break;
                        case 12:
                            WriteLog("【上料龙门】case 12:上料龙门所有气缸&Z气缸缩回开始");
                            if (LoadGantryAllPistonZRetract() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 13;
                            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumBreak, false);
                            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumBreak, false);
                            
                            break;
                        case 13:
                            WriteLog("【上料龙门】case 13:上料龙门收尾工作开始");
                            CurLoadFullTraySeq++;
                            LoadGantryCircleCount++;
                            LoadGantrySuckFinished = true;
                            LoadModuleFinished = false;//上料模组开始工作
                            Step = 0;
                            if (bDelayStop)
                                LoadGantryEnable = false;
                            if (bDelayStopCount)
                            {
                                if (LoadGantryCircleCount >= iDelayStopCount)
                                    LoadGantryEnable = false;
                            }
                            return;
                    }
                }
                #endregion

                #region 上料龙门情况3
                if (CurLoadFullTraySeq == 7)
                {
                    
                    switch (Step)
                    {
                        case 0:
                            WriteLog("【上料龙门】case 0:上料龙门开始执行，CurLoadFullTraySeq=" + CurLoadFullTraySeq.ToString());
                            LoadGantryAllPistonZRetract();//确保Z气缸&缓存气缸已缩回
                            if (LoadGantryMoveAxis(2 * 7) == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 1;
                            break;
                        case 1:
                            WriteLog("【上料龙门】case 1:上料龙门左侧吸嘴伸出开始");
                            if (LoadGantryLeftPistonZStretch() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 2;
                            break;
                        case 2:
                            WriteLog("【上料龙门】case 2:上料龙门左侧气缸吸取开始");
                            if (LoadGantryLeftSuckerSuck() == false)
                            {
                                LoadGantryErrorSolution(2);
                                return;
                            }
                            else
                                Step = 3;
                            break;
                        case 3:
                            WriteLog("【上料龙门】case 3:上料龙门左侧气缸&Z气缸缩回开始");
                            if (LoadGantryLeftPistonZRetract() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 4;
                            break;
                        case 4:
                            WriteLog("【上料龙门】case 4:上料龙门等待换盘开始");
                            LoadTrayFinished = false;
                            DateTime starttime = DateTime.Now;
                            while (true)
                            {
                                if (LoadTrayFinished)
                                    break;
                                else
                                {
                                    if (!OutTimeCount(starttime, 70))
                                    {
                                        WarningSolution("上料Tray换盘超时");
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    Thread.Sleep(30);
                                }
                            }
                            Step = 5;
                            WriteLog("【上料龙门】case 4:上料龙门等待换盘结束");
                            break;
                        case 5:
                            WriteLog("【上料龙门】case 5:上料龙门移动至开始" + (2 * CurLoadFullTraySeq + 1).ToString() + "位置开始");
                            if (LoadGantryMoveAxis(2 * 7 + 1) == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 6;
                            break;
                        case 6:
                            WriteLog("【上料龙门】case 6:上料龙门右侧吸嘴&Z气缸伸出开始");
                            if (LoadGantryRightPistonZStretch() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 7;
                            break;
                        case 7:
                            WriteLog("【上料龙门】case 7:上料龙门右侧吸嘴吸取开始");
                            if (LoadGantryRightSuckerSuck() == false)
                            {
                                LoadGantryErrorSolution(4);
                                return;
                            }
                            else
                                Step = 8;
                            break;
                        case 8:
                            WriteLog("【上料龙门】case 8:上料龙门右侧气缸&Z气缸缩回开始");
                            if (LoadGantryRightPistonZRetract())
                            {
                                if (CurInfo.Di[(int)ECATDINAME.Di_LoadRightVacumCheck + 25] || (LoadTraySuckTryCount >= 1))
                                {
                                    Step = 9;
                                    LoadTraySuckTryCount = 0;
                                }
                                else
                                {
                                    LoadTraySuckTryCount++;
                                    Step = 6;
                                }
                            }
                            else
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            break;
                        case 9:
                            WriteLog("【上料龙门】case 9:上料龙门移动至上料模组放料位开始");
                            LoadGantryAllPistonZRetract();//确保Z气缸&缓存气缸已缩回
                            if (LoadGantryMoveToLoadModule() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 10;
                            break;
                        case 10:
                            WriteLog("【上料龙门】case 10:上料龙门等待上料模组到位开始");
                            if (!debugThreadLoadGantry)
                            {
                                if (WaitLoadModuleInPos() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                else
                                    Step = 11;
                            }
                            else
                                Step = 11;
                            break;
                        case 11:
                            WriteLog("【上料龙门】case 11:上料龙门所有气缸&Z气缸伸出开始");
                            if (LoadGantryAllPistonZStretch() == false)
                            {
                                LoadGantryErrorSolution(6);
                                return;
                            }
                            else
                                Step = 12;
                            break;
                        case 12:
                            WriteLog("【上料龙门】case 12:上料龙门所有吸嘴真空破开始");
                            Thread.Sleep(200);
                            if (LoadGantryAllSuckerBreak() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 13;
                            break;
                        case 13:
                            WriteLog("【上料龙门】case 13:上料龙门所有气缸&Z气缸缩回开始");
                            if (LoadGantryAllPistonZRetract() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 14;
                            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumBreak, false);
                            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumBreak, false);
                           
                            break;
                        case 14:
                            WriteLog("【上料龙门】case 14:上料龙门收尾工作开始");
                            CurLoadFullTraySeq++;
                            LoadGantryCircleCount++;
                            LoadGantrySuckFinished = true;
                            LoadModuleFinished = false;//上料模组开始工作
                            Step = 0;
                            if (bDelayStop)
                                LoadGantryEnable = false;
                            if (bDelayStopCount)
                            {
                                if (LoadGantryCircleCount >= iDelayStopCount)
                                    LoadGantryEnable = false;
                            }
                            return;
                    }
                }
                #endregion

                #region 上料龙门情况4
                if (CurLoadFullTraySeq == 8)
                {
                    //********************樊竞明20181001******************************//
                    if ( !isInputProductBatch && systemParam.isCheckInPutProductionRecordHourBeat == 1)
                    {
                        StartBeep();
                        if (MessageBox.Show("请先将本批次产品的模穴号输入到主界面“检测编号”" + "\r\n" + "若跟上批次相同，请直接点“取消”", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
                        {
                            isInputProductBatch = true;
                        }
                        else
                        {
                            WarningSolution("【提示】提示操作员在主界面输入检测产品的批次/模穴编号");
                            //return;
                        }

                    }
                    //***************************************************************//

                    switch (Step)
                    {
                        case 0:
                            WriteLog("【上料龙门】case 0:上料龙门开始执行，CurLoadFullTraySeq=" + CurLoadFullTraySeq.ToString());
                            LoadGantryAllPistonZRetract();//确保Z气缸&缓存气缸已缩回
                            if (LoadGantryMoveAxis(2 * 8) == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 1;
                            break;
                        case 1:
                            WriteLog("【上料龙门】case 1:上料龙门左侧吸嘴伸出开始");
                            if (LoadGantryLeftPistonZStretch() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 2;
                            break;
                        case 2:
                            WriteLog("【上料龙门】case 2:上料龙门左侧吸嘴吸取开始");
                            if (LoadGantryLeftSuckerSuck() == false)
                            {
                                LoadGantryErrorSolution(2);
                                return;
                            }
                            else
                                Step = 3;
                            break;
                        case 3:
                            WriteLog("【上料龙门】case 3:上料龙门左侧吸嘴&Z气缸缩回开始");
                            if (LoadGantryLeftPistonZRetract() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 50;
                            break;
                        case 50:
                            WriteLog("【上料龙门】case 50:上料龙门判断左侧吸嘴反馈信号");
                            if (!CurInfo.Di[(int)ECATDINAME.Di_LoadLeftVacumCheck + 25])
                            {
                                suckNum++;
                                if (suckNum <= 1)
                                {
                                    LoadGantryStep = 0;
                                }
                                else
                                {
                                    suckNum = 0;
                                    #region 左侧料放回
                                    if (LoadGantryMoveAxis(2 * 8) == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftPistonZStretch() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftSuckerBreak() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftPistonZRetract() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    #endregion
                                    LoadGantryStep = 0;
                                    if (CurLoadFullTraySeq != 14)
                                        CurLoadFullTraySeq++;
                                    else
                                    {
                                        if (((bDelayStop || bDelayStopCount) && (LoadGantryCircleCount + 1 >= iDelayStopCount)) == false)
                                        {
                                            WriteLog("【上料龙门】等待上料Tray换盘开始");
                                            LoadTrayFinished = false;
                                            DateTime starttime = DateTime.Now;
                                            while (true)
                                            {
                                                if (LoadTrayFinished)
                                                    break;
                                                else
                                                {
                                                    if (!OutTimeCount(starttime, 70))
                                                    {
                                                        WarningSolution("上料Tray换盘超时");
                                                        LoadGantryErrorSolution(100);
                                                        return;
                                                    }
                                                    Thread.Sleep(30);
                                                }
                                            }


                                            WriteLog("【上料龙门】等待上料Tray换盘完成");
                                            CurLoadFullTraySeq = 0;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                suckNum = 0;
                                Step = 4;
                            }
                            break;
                        case 4:
                            WriteLog("【上料龙门】case 4:上料龙门移动至开始" + (2 * CurLoadFullTraySeq + 1).ToString() + "位置开始");
                            if (LoadGantryMoveAxis(2 * 8 + 1) == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 5;
                            break;
                        case 5:
                            WriteLog("【上料龙门】case 5:上料龙门右侧吸嘴&Z气缸伸出开始");
                            if (LoadGantryRightPistonZStretch() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 6;
                            break;
                        case 6:
                            WriteLog("【上料龙门】case 6:上料龙门右侧吸嘴吸取开始");
                            if (LoadGantryRightSuckerSuck() == false)
                            {
                                LoadGantryErrorSolution(3);
                                return;
                            }
                            else
                                Step = 7;
                            break;
                        case 7:
                            WriteLog("【上料龙门】case 7:上料龙门右侧吸嘴&Z气缸缩回开始");
                            if (LoadGantryRightPistonZRetract() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 75;
                            break;
                        case 75:
                            WriteLog("【上料龙门】case 100:上料龙门判断右反馈信号");
                            if (!CurInfo.Di[(int)ECATDINAME.Di_LoadRightVacumCheck + 25])
                            {
                                suckNum++;
                                if (suckNum <= 1)
                                {
                                    LoadGantryStep = 4;
                                }
                                else
                                {
                                    suckNum = 0;
                                    #region 把料放回
                                    if (LoadGantryMoveAxis(2 * 8) == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftPistonZStretch() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftSuckerBreak() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftPistonZRetract() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryMoveAxis(2 * 8 + 1) == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightPistonZStretch() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightSuckerBreak() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightPistonZRetract() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    #endregion
                                    LoadGantryStep = 0;
                                    if (CurLoadFullTraySeq != 14)
                                        CurLoadFullTraySeq++;
                                    else
                                    {
                                        if (((bDelayStop || bDelayStopCount) && (LoadGantryCircleCount + 1 >= iDelayStopCount)) == false)
                                        {
                                            WriteLog("【上料龙门】等待上料Tray换盘开始");
                                            LoadTrayFinished = false;
                                            DateTime starttime = DateTime.Now;
                                            while (true)
                                            {
                                                if (LoadTrayFinished)
                                                    break;
                                                else
                                                {
                                                    if (!OutTimeCount(starttime, 70))
                                                    {
                                                        WarningSolution("上料Tray换盘超时");
                                                        LoadGantryErrorSolution(100);
                                                        return;
                                                    }
                                                    Thread.Sleep(40);
                                                }
                                            }

                                            WriteLog("【上料龙门】等待上料Tray换盘完成");
                                            CurLoadFullTraySeq = 0;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                suckNum = 0;
                                Step = 100;
                            }
                            break;
                        case 100:
                            WriteLog("【上料龙门】case 100:上料龙门判断左右反馈信号 无反馈再吸一次");
                            if ((!CurInfo.Di[(int)ECATDINAME.Di_LoadLeftVacumCheck + 25]) || (!CurInfo.Di[(int)ECATDINAME.Di_LoadRightVacumCheck + 25]))
                            {
                                if (!CurInfo.Di[(int)ECATDINAME.Di_LoadRightVacumCheck + 25])//右边
                                {
                                    if (LoadGantryMoveAxis(2 * 8 + 1) == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightPistonZStretch() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightSuckerSuck() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryRightPistonZRetract() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                }
                                else if (!CurInfo.Di[(int)ECATDINAME.Di_LoadLeftVacumCheck + 25])//左边
                                {
                                    if (LoadGantryMoveAxis(2 * 8) == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftPistonZStretch() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftSuckerSuck() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                    if (LoadGantryLeftPistonZRetract() == false)
                                    {
                                        LoadGantryErrorSolution(100);
                                        return;
                                    }
                                }
                                Step = 200;
                            }
                            else
                            {
                                Step = 8;
                            }
                            break;
                        case 200:
                            WriteLog("【上料龙门】case 200:上料龙门判断左右反馈信号 无反馈放完料直接走");
                            if ((!CurInfo.Di[(int)ECATDINAME.Di_LoadLeftVacumCheck + 25]) || (!CurInfo.Di[(int)ECATDINAME.Di_LoadRightVacumCheck + 25]))
                            {
                                #region 把料放回
                                if (LoadGantryMoveAxis(2 * 8 + 1) == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryRightPistonZStretch() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryRightSuckerBreak() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryRightPistonZRetract() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryMoveAxis(2 * 8) == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryLeftPistonZStretch() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryLeftSuckerBreak() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                if (LoadGantryLeftPistonZRetract() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                #endregion
                                LoadGantryStep = 0;
                                if (CurLoadFullTraySeq != 14)
                                    CurLoadFullTraySeq++;
                                else
                                {
                                    if (((bDelayStop || bDelayStopCount) && (LoadGantryCircleCount + 1 >= iDelayStopCount)) == false)
                                    {
                                        WriteLog("【上料龙门】等待上料Tray换盘开始");
                                        LoadTrayFinished = false;
                                        DateTime starttime = DateTime.Now;
                                        while (true)
                                        {
                                            if (LoadTrayFinished)
                                                break;
                                            else
                                            {
                                                if (!OutTimeCount(starttime, 70))
                                                {
                                                    WarningSolution("上料Tray换盘超时");
                                                    LoadGantryErrorSolution(100);
                                                    return;
                                                }
                                                Thread.Sleep(30);
                                            }
                                        }
                                        WriteLog("【上料龙门】等待上料Tray换盘完成");
                                        CurLoadFullTraySeq = 0;
                                    }
                                }
                            }
                            else
                            {
                                Step = 8;
                            }
                            break;
                        case 8:
                            WriteLog("【上料龙门】case 8:上料龙门移动至上料模组放料位开始");
                            LoadGantryAllPistonZRetract();//确保Z气缸&缓存气缸已缩回
                            if (LoadGantryMoveToLoadModule() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 9;
                            break;
                        case 9:
                            WriteLog("【上料龙门】case 9:上料龙门等待上料模组到位开始");
                            if (!debugThreadLoadGantry)
                            {
                                if (WaitLoadModuleInPos() == false)
                                {
                                    LoadGantryErrorSolution(100);
                                    return;
                                }
                                else
                                    Step = 10;
                            }
                            else
                                Step = 10;
                            break;
                        case 10:
                            WriteLog("【上料龙门】case 10:上料龙门所有吸嘴&Z气缸伸出开始");
                            if (LoadGantryAllPistonZStretch() == false)
                            {
                                LoadGantryErrorSolution(5);
                                return;
                            }
                            else
                                Step = 11;
                            break;
                        case 11:
                            WriteLog("【上料龙门】case 11:上料龙门所有吸嘴真空破开始");
                            Thread.Sleep(200);
                            if (LoadGantryAllSuckerBreak() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 12;
                            break;
                        case 12:
                            WriteLog("【上料龙门】case 12:上料龙门所有气缸&Z气缸缩回开始");
                            if (LoadGantryAllPistonZRetract() == false)
                            {
                                LoadGantryErrorSolution(100);
                                return;
                            }
                            else
                                Step = 13;
                            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumBreak, false);
                            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumBreak, false);
                           
                            break;
                        case 13:
                            WriteLog("【上料龙门】case 13:上料龙门收尾工作开始");
                            CurLoadFullTraySeq++;
                            LoadGantryCircleCount++;
                            LoadGantrySuckFinished = true;
                            LoadModuleFinished = false;//上料模组开始工作
                            Step = 0;
                            if (bDelayStop)
                                LoadGantryEnable = false;
                            if (bDelayStopCount)
                            {
                                if (LoadGantryCircleCount >= iDelayStopCount)
                                    LoadGantryEnable = false;
                            }
                            return;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(ThreadAbortException))
                {
                    string exStr = "";
                    exStr += "【上料龙门】" + ex.ToString() + "\n";
                    exStr += "LoadGantryCircleCount=" + LoadGantryCircleCount.ToString() + "CurLoadFullTraySeq=" + CurLoadFullTraySeq.ToString() + "\n";
                    MessageBox.Show(exStr);
                }
            }
        }

        //整体移动至上料模组正上方
        public bool LoadGantryMoveToLoadModule()
        {
            int errcode = 0; double poserror = 100;
            if (adlink.LineMove(new Axis[] { logicConfig.ECATAxis[0], logicConfig.ECATAxis[1] }, new double[] { systemParam.LoadTrayFinishPosX, systemParam.LoadTrayFinishPosY }, true, ref errcode, ref poserror))
                return true;
            else
            {
                WarningSolution("【130】龙门上料轴运行至上料模组位置正上方错误");
                return false;
            }
        }

        //龙门上料吸取&放置8个工件，整体来看共15步，0~14
        public bool LoadGantrySuckAllWorkPiece(ref int Seq)
        {
            if ((Seq <= 5) || (Seq >= 9 && Seq <= 14))
            {
                if (LoadGantrySuckBothSide(Seq) == false)
                    return false;

                if (LoadGantryMoveToLoadModule() == false)
                    return false;

                if (!debugThreadLoadGantry)
                {
                    if (WaitLoadModuleInPos() == false)
                        return false;
                }

                if (LoadGantryAllPistonZStretch() == false)
                    return false;
                if (LoadGantryAllSuckerBreak() == false)
                    return false;
                if (LoadGantryAllPistonZRetract() == false)
                    return false;

                if (Seq == 14)
                {
                    LoadTrayFinished = false;
                    while (!LoadTrayFinished)
                        Thread.Sleep(30);
                }
            }

            if (Seq == 6)
            {
                if (LoadGantrySuckLeftSide(2 * 6) == false)
                    return false;

                if (LoadGantrySuckRightSide(2 * 6 + 1) == false)
                    return false;

                if (LoadGantryMoveToLoadModule() == false)
                    return false;

                if (!debugThreadLoadGantry)
                {
                    if (WaitLoadModuleInPos() == false)
                        return false;
                }

                if (LoadGantryAllPistonZStretch() == false)
                    return false;
                if (LoadGantryAllSuckerBreak() == false)
                    return false;
                if (LoadGantryAllPistonZRetract() == false)
                    return false;
            }

            if (Seq == 7)
            {
                if (LoadGantrySuckLeftSide(2 * 7) == false)
                    return false;

                LoadTrayFinished = false;
                while (!LoadTrayFinished)
                    Thread.Sleep(30);

                if (LoadGantrySuckRightSide(2 * 7 + 1) == false)
                    return false;

                if (LoadGantryMoveToLoadModule() == false)
                    return false;

                if (!debugThreadLoadGantry)
                {
                    if (WaitLoadModuleInPos() == false)
                        return false;
                }
                if (LoadGantryAllPistonZStretch() == false)
                    return false;
                if (LoadGantryAllSuckerBreak() == false)
                    return false;
                if (LoadGantryAllPistonZRetract() == false)
                    return false;
            }

            if (Seq == 8)
            {
                if (LoadGantrySuckLeftSide(2 * 8) == false)
                    return false;

                if (LoadGantrySuckRightSide(2 * 8 + 1) == false)
                    return false;

                if (LoadGantryMoveToLoadModule() == false)
                    return false;

                if (!debugThreadLoadGantry)
                {
                    if (WaitLoadModuleInPos() == false)
                        return false;
                }

                if (LoadGantryAllPistonZStretch() == false)
                    return false;
                if (LoadGantryAllSuckerBreak() == false)
                    return false;
                if (LoadGantryAllPistonZRetract() == false)
                    return false;
            }

            Seq++;
            if (Seq > 14)
                Seq = 0;
            return true;
        }

        //包含移动到Tray盘相应位置+吸取动作0~14
        private bool LoadGantrySuckBothSide(int Seq)
        {
            if (LoadGantryMoveAxis(2 * Seq) == false)
                return false;

            if (LoadGantryRightPistonZStretch() == false)
                return false;

            if (LoadGantryRightSuckerSuck() == false)
                return false;

            if (LoadGantryRightPistonZRetract() == false)
                return false;

            //Thread.Sleep(50);

            if (LoadGantryMoveAxis(2 * Seq + 1) == false)
                return false;

            if (LoadGantryLeftPistonZStretch() == false)
                return false;

            if (LoadGantryLeftSuckerSuck() == false)
                return false;

            if (LoadGantryLeftPistonZRetract() == false)
                return false;

            return true;
        }

        private bool LoadGantrySuckLeftSide(int Seq)
        {
            if (LoadGantryMoveAxis(Seq) == false)
                return false;

            if (LoadGantryLeftPistonZStretch() == false)
                return false;

            if (LoadGantryLeftSuckerSuck() == false)
                return false;

            if (LoadGantryLeftPistonZRetract() == false)
                return false;

            return true;
        }

        private bool LoadGantrySuckRightSide(int Seq)
        {
            if (LoadGantryMoveAxis(Seq) == false)
                return false;

            if (LoadGantryRightPistonZStretch() == false)
                return false;

            if (LoadGantryRightSuckerSuck() == false)
                return false;

            if (LoadGantryRightPistonZRetract() == false)
                return false;

            return true;
        }

        public bool LoadGantryZStretch()
        {
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_LoadZStretchControl, (int)ECATDONAME.Do_LoadZRetractControl, (int)ECATDINAME.Di_LoadZStretchBit, (int)ECATDINAME.Di_LoadZRetractBit))
                return true;
            else
                return false;
        }

        public bool LoadGantryZRetract()
        {
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_LoadZRetractControl, (int)ECATDONAME.Do_LoadZStretchControl, (int)ECATDINAME.Di_LoadZRetractBit, (int)ECATDINAME.Di_LoadZStretchBit))
                return true;
            else
                return false;
        }

        public bool LoadGantryAllPistonStretch()
        {
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferLeftStretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferLeftRetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferRightStretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferRightRetractControl, false);
            DateTime starttime = DateTime.Now;
            while (true)
            {
                if (CurInfo.Di[(int)ECATDINAME.Di_LoadBufferLeftStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadBufferLeftRetractBit + 25]) &&
                    CurInfo.Di[(int)ECATDINAME.Di_LoadBufferRightStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadBufferRightRetractBit + 25]))
                    break;
                else
                {
                    if (!OutTimeCount(starttime, 10))
                        return false;
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        public bool LoadGantryAllPistonRetract()
        {
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferLeftStretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferLeftRetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferRightStretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferRightRetractControl, true);
            DateTime starttime = DateTime.Now;
            while (true)
            {
                if ((!CurInfo.Di[(int)ECATDINAME.Di_LoadBufferLeftStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_LoadBufferLeftRetractBit + 25] &&
                    (!CurInfo.Di[(int)ECATDINAME.Di_LoadBufferRightStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_LoadBufferRightRetractBit + 25])
                    break;
                else
                {
                    if (!OutTimeCount(starttime, 10))
                        return false;
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        public bool LoadGantryLeftPistonStretch()
        {
            //龙门上料左气缸伸出
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_LoadBufferLeftStretchControl, (int)ECATDONAME.Do_LoadBufferLeftRetractControl, (int)ECATDINAME.Di_LoadBufferLeftStretchBit, (int)ECATDINAME.Di_LoadBufferLeftRetractBit) == false)
                return false;
            else
                return true;
        }

        public bool LoadGantryLeftPistonRetract()
        {
            //龙门上料左气缸缩回
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_LoadBufferLeftRetractControl, (int)ECATDONAME.Do_LoadBufferLeftStretchControl, (int)ECATDINAME.Di_LoadBufferLeftRetractBit, (int)ECATDINAME.Di_LoadBufferLeftStretchBit) == false)
                return false;
            else
                return true;
        }

        public bool LoadGantryRightPistonStretch()
        {
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_LoadBufferRightStretchControl, (int)ECATDONAME.Do_LoadBufferRightRetractControl, (int)ECATDINAME.Di_LoadBufferRightStretchBit, (int)ECATDINAME.Di_LoadBufferRightRetractBit) == false)
                return false;
            else
                return true;
        }

        public bool LoadGantryRightPistonRetract()
        {
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_LoadBufferRightRetractControl, (int)ECATDONAME.Do_LoadBufferRightStretchControl, (int)ECATDINAME.Di_LoadBufferRightRetractBit, (int)ECATDINAME.Di_LoadBufferRightStretchBit) == false)
                return false;
            else
                return true;
        }

        private bool LoadGantryLeftPistonZStretch()
        {
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferLeftStretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferLeftRetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadZStretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadZRetractControl, false);

            DateTime StartTime = DateTime.Now;
            while (true)
            {
                if (CurInfo.Di[(int)ECATDINAME.Di_LoadBufferLeftStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadBufferLeftRetractBit + 25]) &&
                    CurInfo.Di[(int)ECATDINAME.Di_LoadZStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadZRetractBit + 25]))
                {
                    return true;
                }
                else
                {
                    if (!OutTimeCount(StartTime, 10))
                    {
                        WarningSolution("【217】龙门上料左气缸&Z气缸伸出错误：I21.00~I21.03");
                        return false;
                    }
                    Thread.Sleep(40);
                }
            }
        }

        private bool LoadGantryLeftPistonZRetract()
        {
            WriteLog("【上料龙门】左侧气缸&Z气缸缩回开始");
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferLeftStretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferLeftRetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadZStretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadZRetractControl, true);

            DateTime StartTime = DateTime.Now;
            while (true)
            {
                if (CurInfo.Di[(int)ECATDINAME.Di_LoadBufferLeftRetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadBufferLeftStretchBit + 25]) &&
                    CurInfo.Di[(int)ECATDINAME.Di_LoadZRetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadZStretchBit + 25]))
                {
                    WriteLog("【上料龙门】左侧气缸&Z气缸缩回完成");
                    return true;
                }
                else
                {
                    if (!OutTimeCount(StartTime, 10))
                    {
                        WarningSolution("【上料龙门】【报警】：【218】龙门上料左气缸&Z气缸缩回错误：I21.00~I21.03");
                        return false;
                    }
                    Thread.Sleep(40);
                }
            }
        }

        private bool LoadGantryRightPistonZStretch()
        {
            WriteLog("【上料龙门】上料龙门右侧吸嘴&Z气缸伸出开始");
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferRightStretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferRightRetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadZStretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadZRetractControl, false);

            DateTime StartTime = DateTime.Now;
            while (true)
            {
                if (CurInfo.Di[(int)ECATDINAME.Di_LoadBufferRightStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadBufferRightRetractBit + 25]) &&
                    CurInfo.Di[(int)ECATDINAME.Di_LoadZStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadZRetractBit + 25]))
                {
                    WriteLog("【上料龙门】上料龙门右侧吸嘴&Z气缸伸出完成");
                    return true;
                }
                else
                {
                    if (!OutTimeCount(StartTime, 10))
                    {
                        WarningSolution("【上料龙门】【报警】：【219】龙门上料右气缸&Z气缸伸出错误：I21.00、I21.01、I21.04、I21.05");
                        return false;
                    }
                    Thread.Sleep(40);
                }
            }
        }

        private bool LoadGantryRightPistonZRetract()
        {
            WriteLog("【上料龙门】上料龙门右侧吸嘴&Z气缸缩回开始");
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferRightStretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferRightRetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadZStretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadZRetractControl, true);

            DateTime StartTime = DateTime.Now;
            while (true)
            {
                if (CurInfo.Di[(int)ECATDINAME.Di_LoadBufferRightRetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadBufferRightStretchBit + 25]) &&
                    CurInfo.Di[(int)ECATDINAME.Di_LoadZRetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadZStretchBit + 25]))
                {
                    WriteLog("【上料龙门】上料龙门右侧吸嘴&Z气缸缩回完成");
                    return true;
                }
                else
                {
                    if (!OutTimeCount(StartTime, 10))
                    {
                        WarningSolution("【上料龙门】【报警】：【220】龙门上料右气缸&Z气缸缩回错误");
                        return false;
                    }
                    Thread.Sleep(40);
                }
            }
        }


        private bool LoadGantryAllPistonZStretch()
        {
            WriteLog("【上料龙门】上料龙门所有吸嘴&Z气缸伸出开始");
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferLeftStretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferLeftRetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferRightStretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferRightRetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadZStretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadZRetractControl, false);

            DateTime StartTime = DateTime.Now;
            while (true)
            {
                if (CurInfo.Di[(int)ECATDINAME.Di_LoadBufferLeftStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadBufferLeftRetractBit + 25]) &&
                    CurInfo.Di[(int)ECATDINAME.Di_LoadBufferRightStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadBufferRightRetractBit + 25]) &&
                    CurInfo.Di[(int)ECATDINAME.Di_LoadZStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadZRetractBit + 25]))
                {
                    Thread.Sleep(100);
                    WriteLog("【上料龙门】上料龙门所有吸嘴&Z气缸伸出完成");
                    return true;
                }
                else
                {
                    if (!OutTimeCount(StartTime, 10))
                    {
                        WarningSolution("【上料龙门】【报警】：【221】龙门上料所有气缸&Z气缸伸出错误：I21.00~I21.05");
                        return false;
                    }
                    Thread.Sleep(40);
                }
            }
        }

        private bool LoadGantryAllPistonZRetract()
        {
            WriteLog("【上料龙门】上料龙门所有吸嘴&Z气缸缩回开始");
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferLeftStretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferLeftRetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferRightStretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadBufferRightRetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadZStretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadZRetractControl, true);

            DateTime StartTime = DateTime.Now;
            while (true)
            {
                if (CurInfo.Di[(int)ECATDINAME.Di_LoadBufferLeftRetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadBufferLeftStretchBit + 25]) &&
                    CurInfo.Di[(int)ECATDINAME.Di_LoadBufferRightRetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadBufferRightStretchBit + 25]) &&
                    CurInfo.Di[(int)ECATDINAME.Di_LoadZRetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadZStretchBit + 25]))
                {
                    WriteLog("【上料龙门】上料龙门所有吸嘴&Z气缸缩回完成");
                    return true;
                }
                else
                {
                    if (!OutTimeCount(StartTime, 10))
                    {
                        WarningSolution("【上料龙门】【报警】【222】龙门上料所有气缸&Z气缸缩回错误：I21.00~I21.05");
                        return false;
                    }
                    Thread.Sleep(40);
                }
            }
        }

        public bool LoadGantryAllSuckerSuck()
        {
            bool tempsignal = false;
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumBreak, false);

            Thread.Sleep(100);
            return true;
        }


        public bool LoadGantryAllSuckerBreak()
        {
            Thread.Sleep(systemParam.LoadGantrySuckerBreakDelay);
            WriteLog("【上料龙门】上料龙门所有吸嘴破真空开始");

            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumBreak, true);

            Thread.Sleep(100);

            WriteLog("【上料龙门】上料龙门所有吸嘴破真空完成");

            return true;
        }

        public bool LoadGantryLeftSuckerSuck()
        {
            WriteLog("【上料龙门】上料龙门左侧吸嘴吸取开始");
            //龙门上料左气缸真空吸
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumSuck, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumBreak, false);

            Thread.Sleep(systemParam.LoadGantrySuckDelay);
            WriteLog("【上料龙门】上料龙门左侧吸嘴破吸取完成");

            return true;
        }

        public bool LoadGantryRightSuckerSuck()
        {
            WriteLog("【上料龙门】上料龙门右侧吸嘴吸取开始");
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumSuck, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumBreak, false);
            if (CurLoadFullTraySeq == 7)
            {
                Thread.Sleep(100);
            }

            Thread.Sleep(systemParam.LoadGantrySuckDelay);
            return true;
        }

        public bool LoadGantryLeftSuckerBreak()
        {
            Thread.Sleep(systemParam.LoadGantrySuckerBreakDelay);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumSuck, false);

            Thread.Sleep(100);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumBreak, false);
            return true;
        }

        public bool LoadGantryRightSuckerBreak()
        {
            Thread.Sleep(systemParam.LoadGantrySuckerBreakDelay);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumSuck, false);

            Thread.Sleep(100);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumBreak, false);

            return true;
        }

        private bool LoadGantryMoveAxis(int TrayPosNo)
        {
            int errcode = 0; double poserror = 0;
            DateTime starttime = DateTime.Now;
            while (true)
            {
                if (LoadGantrySuckSafeSignal == true)
                    break;
                else
                {
                    if (!OutTimeCount(starttime, 3))
                    {
                        WarningSolution("上料龙门吸取安全信号出错（移动气缸未伸出）");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }

            WriteLog("【上料龙门】移动至" + TrayPosNo.ToString() + "吸料位置开始");
            if (adlink.LineMove(new Axis[] { logicConfig.ECATAxis[0], logicConfig.ECATAxis[1] }, new double[] { LoadGantryMotionPos.posInfo[TrayPosNo].XPos, LoadGantryMotionPos.posInfo[TrayPosNo].YPos }, true, ref errcode, ref poserror))
            {
                WriteLog("【上料龙门】移动至" + TrayPosNo.ToString() + "吸料位置完成");
                return true;
            }
            else
            {
                WarningSolution("【上料龙门】【报警】【213】上料龙门XY轴运动至上料点" + TrayPosNo.ToString() + "上方出错");
                return false;
            }
        }
        #endregion

        #region 上料模组移动
        public int LoadModuleCircleCount = 0;
        public bool LoadModuleEnable = false;
        public bool LoadModuleFinished = false;//上料模组整体完成标志位
        public int LoadModuleStep = 0;
        public bool LoadModuleMoveCylinderFinish = false;
        public bool LoadModuleMotionMoveCylinderFinish = false;

        public void LoadModuleThread()
        {
            int errcode = 0;
            LoadModuleCircleCount = 0;
            LoadModuleStep = 0;

            //开启数据收集线程
            while (AutoRunActive)
            {
                if (LoadModuleEnable)
                {
                    if (!LoadModuleFinished)
                    {
                        LoadModuleStatusSwitch(ref LoadModuleStep, ref errcode);
                    }
                }
                Thread.Sleep(40);
            }
        }

        private bool LoadModuleMoveStretch()
        {
            WriteLog("【上料模组】移动气缸伸出开始");
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_LoadMoveStretchControl, (int)ECATDONAME.Do_LoadMoveRetractControl, (int)ECATDINAME.Di_LoadMoveStretchBit, (int)ECATDINAME.Di_LoadMoveRetractBit))
            {
                WriteLog("【上料模组】移动气缸伸出完成");
                return true;
            }
            else
            {
                WarningSolution("【上料模组】【报警】：【146】上料模组移动气缸伸出异常:I23.02,I23.03");
                return false;
            }
        }

        private bool LoadModuleMoveRetract()
        {
            WriteLog("【上料模组】移动气缸缩回开始");
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_LoadMoveRetractControl, (int)ECATDONAME.Do_LoadMoveStretchControl, (int)ECATDINAME.Di_LoadMoveRetractBit, (int)ECATDINAME.Di_LoadMoveStretchBit))
            {
                WriteLog("【上料模组】移动气缸缩回完成");
                return true;
            }
            else
            {
                WarningSolution("【上料模组】【报警】：【147】上料模组移动气缸缩回异常:I23.02,I23.03");
                return false;
            }
        }

        private bool LoadModulePosMoveStretch()
        {
            WriteLog("【上料模组】位置移动气缸伸出开始");
            if (WaitECATPiston1Cmd2FeedbackDone((int)ECATDONAME.Do_LoadMoveCylinder, true, (int)ECATDINAME.Di_LoadMotionMoveStretchBit, (int)ECATDINAME.Di_LoadMotionMoveRetractBit))
            {
                WriteLog("【上料模组】位置移动气缸伸出完成");
                return true;
            }
            else
            {
                WarningSolution("【上料模组】【报警】【148】上料模组位置移动气缸伸出异常:I23.04,I23.05");
                return false;
            }
        }

        private bool LoadModulePosMoveRetract()
        {
            WriteLog("【上料模组】位置移动气缸缩回开始");
            if (WaitECATPiston1Cmd2FeedbackDone((int)ECATDONAME.Do_LoadMoveCylinder, false, (int)ECATDINAME.Di_LoadMotionMoveRetractBit, (int)ECATDINAME.Di_LoadMotionMoveStretchBit))
            {
                WriteLog("【上料模组】位置移动气缸缩回完成");
                return true;
            }
            else
            {
                WarningSolution("【上料模组】【报警】【149】上料模组位置移动气缸缩回异常:I23.04,I23.05");
                return false;
            }
        }


        private void LoadModuleStatusSwitch(ref int Step, ref int errcode)
        {
            bool tempsignal = false;
            DateTime starttime = DateTime.Now;

            try
            {
                switch (Step)
                {
                    case 0://吸气动作+吹气动作
                        WriteLog("【上料模组】case 0：上料模组吸气&吹气");
                        if (LoadModuleVacumSuck() == false)
                        {
                            LoadModuleErrorSolution(100);
                            return;
                        }
                        IOControl.ECATWriteDO((int)ECATDONAME.Do_BlowControl, true);
                        Step = 1;
                        break;
                    case 1://移动气缸伸出
                        WriteLog("【上料模组】case 1：上料模组移动气缸伸出开始");
                        if (LoadModuleMoveStretch())//移动气缸伸出
                        {
                            LoadModuleMoveCylinderFinish = true;
                            Step = 2;
                            WriteLog("【上料模组】case 1：上料模组移动气缸伸出完成");
                        }
                        else
                        {
                            LoadModuleErrorSolution(100);
                            return;
                        }
                        break;
                    case 2://关闭底座真空吸&吹气
                        WriteLog("【上料模组】case 2：上料模组关闭吸气&吹气");
                        IOControl.ECATWriteDO((int)ECATDONAME.Do_BlowControl, false);//关闭吹气
                        if (LoadModuleVacumBreak() == false)//关闭底座真空吸
                        {
                            LoadModuleErrorSolution(100);
                            return;
                        }
                        Step = 3;
                        break;
                    case 3://等待横移轴吸取前四个工件
                        WriteLog("【上料模组】case 3：等待横移轴吸取前四个工件开始");
                        if (LoadModule_WaitSuckAxisSuck1())
                            Step = 4;
                        else
                        {
                            LoadModuleErrorSolution(100);
                            return;
                        }
                        break;
                    case 4://位置移动气缸伸出
                        WriteLog("【上料模组】case 4：位置移动气缸伸出开始");
                        if (LoadModulePosMoveStretch())
                        {
                            LoadModuleMotionMoveCylinderFinish = true;
                            WriteLog("【上料模组】case 4：位置移动气缸伸出完成");
                            Step = 5;
                        }
                        else
                        {
                            LoadModuleErrorSolution(100);
                            return;
                        }
                        break;
                    case 5://等待横移轴吸取后四个工件
                        WriteLog("【上料模组】case 5：等待横移轴吸取后四个工件开始");
                        if (LoadModule_WaitSuckAxisSuck2())//等待横移轴吸取后四个工件
                        {
                            Step = 6;
                            WriteLog("【上料模组】case 5：等待横移轴吸取后四个工件完成");
                        }
                        else
                        {
                            LoadModuleErrorSolution(100);
                            return;
                        }
                        break;
                    case 6://移动气缸伸出&位置移动气缸缩回
                        WriteLog("【上料模组】case 6：双气缸缩回开始");
                        LoadModuleMoveCylinderFinish = false;
                        LoadModuleMotionMoveCylinderFinish = false;
                        IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadMoveCylinder, false);
                        IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadMoveStretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadMoveRetractControl, true);
                        starttime = DateTime.Now;
                        while (true)
                        {
                            tempsignal = (!CurInfo.Di[(int)ECATDINAME.Di_LoadMotionMoveStretchBit + 25]) && (CurInfo.Di[(int)ECATDINAME.Di_LoadMotionMoveRetractBit + 25]) &&
                                         (!CurInfo.Di[(int)ECATDINAME.Di_LoadMoveStretchBit + 25]) && (CurInfo.Di[(int)ECATDINAME.Di_LoadMoveRetractBit + 25]);
                            if (tempsignal)
                            {
                                WriteLog("【上料模组】case 6：双气缸缩回完成");
                                break;
                            }
                            else
                            {
                                if (!OutTimeCount(starttime, 10))
                                {
                                    LoadModuleErrorSolution(0);
                                    return;
                                }
                                Thread.Sleep(30);
                            }
                        }
                        //收尾工作
                        Step = 0;
                        Thread.Sleep(300);
                        LoadModuleFinished = true;
                        LoadModuleCircleCount++;
                        WriteLog("【上料模组】case 6：收尾动作完成");
                        break;
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(ThreadAbortException))
                {
                    string exStr = "";
                    exStr += "【上料模组】" + ex.ToString() + "\n";
                    exStr += "LoadModuleCircleCount=" + LoadModuleCircleCount.ToString() + "\n";
                    MessageBox.Show(exStr);
                }
            }
        }

        private bool LoadModuleVacumSuck()
        {
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadVacumSuck, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadVacumBreak, false);
            return true;
            //屏蔽反馈监测
            //DateTime StartTime = DateTime.Now;
            //while (true)
            //{
            //    if (CurInfo.Di[(int)ECATDINAME.Di_LoadVacumCheck + 25] == true)
            //    {
            //        return true;
            //    }
            //    else
            //    {
            //        if (!OutTimeCount(StartTime, 10))
            //        {
            //            UpdateWaringLogNG.Invoke((object)"上料模组真空吸出错");
            //            return false;
            //        }
            //        Thread.Sleep(35);
            //    }
            //}
        }

        private bool LoadModuleVacumBreak()
        {
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadVacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadVacumSuck, false);

            Thread.Sleep(50);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadVacumBreak, false);
            return true;
            //屏蔽反馈监测
            //DateTime StartTime = DateTime.Now;
            //while (true)
            //{
            //    if (CurInfo.Di[(int)ECATDINAME.Di_LoadVacumCheck + 25] == false)
            //    {
            //        IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadVacumBreak, false);
            //        return true;
            //    }
            //    else
            //    {
            //        if (!OutTimeCount(StartTime, 10))
            //        {
            //            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadVacumBreak, false);
            //            UpdateWaringLogNG.Invoke((object)"上料模组真空破出错");
            //            return false;
            //        }
            //        Thread.Sleep(35);
            //    }
            //}
        }

        private bool LoadModule_WaitSuckAxisSuck1()
        {
            WriteLog("【上料模组】等待横移轴吸料1开始");
            DateTime starttime = DateTime.Now;
            while (true)
            {
                if (SuckAxisSuck1Finish)
                {
                    WriteLog("【上料模组】等待横移轴吸料1完成");
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 60))
                    {
                        WarningSolution("【上料模组】【报警】：【150】上料模组等待横移轴吸取工件1超时");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        private bool LoadModule_WaitSuckAxisSuck2()
        {
            DateTime starttime = DateTime.Now;
            while (true)
            {
                if (SuckAxisSuck2Finish)
                    break;
                else
                {
                    if (!OutTimeCount(starttime, 60))
                    {
                        WarningSolution("【151】上料模组等待横移轴吸取工件2超时");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        #endregion

        #region 横移轴动作
        public int SuckAxisMoveCircleCount = 0;
        public bool SuckAxisMoveEnable = false;
        public bool SuckAxisMoveFinished = false;
        public int SuckAxisMoveStep = 0;
        public bool SuckAxisSuck1Finish = false; public bool SuckAxisSuck2Finish = false;
        public bool SuckAxisPlace1Finish = false; public bool SuckAxisPlace2Finish = false;

        public void SuckAxisMoveThread()
        {
            int errcode = 0;
            SuckAxisMoveCircleCount = 0;
            SuckAxisMoveStep = 0;

            while (AutoRunActive)
            {
                if (SuckAxisMoveEnable)
                {
                    SuckAxisMoveStatusSwitch(ref SuckAxisMoveStep, ref errcode);
                }
                Thread.Sleep(40);
            }
        }

        //分第一次和第二次，第一次等待位置气缸到位，第二次等待位置移动气缸到位
        //通过CircleCount区分，CircleCount%2==0,则位置气缸，==1则位置移动气缸
        public void SuckAxisMoveStatusSwitch(ref int Step, ref int errcode)
        {
            bool tempsignal = false;
            DateTime StartTime = DateTime.Now;
            try
            {
                if (SuckAxisMoveCircleCount >= 3)
                {
                    if (SuckAxisMoveCircleCount == 3)
                        UnloadModuleFinished = false;

                    switch (Step)
                    {
                        case 0://确保已经在右取放位
                            WriteLog("【横移轴】case0：横移轴向右移动开始");
                            if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode))
                            {
                                Step = 1;
                                WriteLog("【横移轴】case0：横移轴向右移动完成");
                            }
                            else
                            {
                                SuckAxisErrorSolution(0);
                                return;
                            }
                            break;
                        case 1:
                            WriteLog("【横移轴】case1：横移轴等待右侧信号到位开始");
                            if ((bDelayStop || bDelayStopCount) && (SuckAxisMoveCircleCount >= 2 * LoadGantryCircleCount))
                            {
                                if (SuckAxisMove_WaitSignalsSimple())
                                    Step = 2;
                                else
                                {
                                    SuckAxisErrorSolution(100);
                                    return;
                                }
                            }
                            else
                            {
                                if (!debugThreadSuckAxis)
                                {
                                    if (SuckAxisMove_WaitRightSignals(SuckAxisMoveCircleCount))
                                        Step = 2;
                                    else
                                    {
                                        SuckAxisErrorSolution(100);
                                        return;
                                    }
                                }
                                else
                                    Step = 2;
                            }
                            WriteLog("【横移轴】case1：横移轴等待右侧信号到位结束");
                            break;
                        case 2://横移轴Z气缸伸出
                            WriteLog("【横移轴】case2：横移轴Z气缸伸出开始");
                            if (SuckAxisZStretch())
                                Step = 3;
                            else
                            {
                                SuckAxisErrorSolution(100);
                                return;
                            }
                            WriteLog("【横移轴】case2：横移轴Z气缸伸出结束");
                            break;
                        case 3://横移轴真空吸工件
                            WriteLog("【横移轴】case3：横移轴吸取工件开始");
                            if (!debugThreadSuckAxis)
                            {
                                if ((bDelayStop || bDelayStopCount) && (SuckAxisMoveCircleCount >= 2 * LoadGantryCircleCount))
                                    SuckAxisUnloadSuckerSuck();
                                else
                                    SuckAxisSuckerSuck();
                            }
                            Step = 4;
                            WriteLog("【横移轴】case3：横移轴吸取工件结束");
                            break;
                        case 4://横移轴Z气缸缩回
                            WriteLog("【横移轴】case4：横移轴Z气缸缩回开始");
                            if (SuckAxisZRetract())
                                Step = 5;
                            else
                            {
                                SuckAxisErrorSolution(100);
                                return;
                            }
                            WriteLog("【横移轴】case4：横移轴Z气缸缩回结束：SuckAxisSuck1Finish=" + SuckAxisSuck1Finish.ToString() + " SuckAxisSuck2Finish=" + SuckAxisSuck2Finish.ToString());
                            break;
                        case 5://判断真空吸嘴信号
                            WriteLog("【横移轴】case5：判断真空吸嘴信号");
                            if ((bDelayStop || bDelayStopCount) && (SuckAxisMoveCircleCount >= 2 * LoadGantryCircleCount))
                            {
                                if (CurInfo.Di[(int)ECATDINAME.Di_SuckUnloadVacumCheck + 25] == false)
                                {
                                    SuckAxisUnloadSuckRetry();//横移轴下料位重新吸取一遍；
                                    if (CurInfo.Di[(int)ECATDINAME.Di_SuckUnloadVacumCheck + 25] == false)
                                    {
                                        WriteLog("【横移轴】case5：下料位真空吸嘴尝试两次吸取均无信号");
                                        SuckAxisErrorSolution(4);
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                if ((CurInfo.Di[(int)ECATDINAME.Di_SuckLoadVacumCheck + 25] && CurInfo.Di[(int)ECATDINAME.Di_SuckUnloadVacumCheck + 25]) == false)
                                {
                                    SuckAxisSuckRetry();//横移轴重新吸取一遍
                                    if ((CurInfo.Di[(int)ECATDINAME.Di_SuckLoadVacumCheck + 25] && CurInfo.Di[(int)ECATDINAME.Di_SuckUnloadVacumCheck + 25]) == false)
                                    {
                                        WriteLog("【横移轴】case5：上料/下料位真空吸嘴尝试两次吸取均无信号");
                                        SuckAxisErrorSolution(3);
                                        return;
                                    }
                                }
                            }
                            Step = 6;
                            Thread.Sleep(20);
                            break;
                        case 6://移动至左位开始
                            WriteLog("【横移轴】case6：横移轴移动至左位开始");
                            if (SuckAxisMoveCircleCount % 2 == 0)
                                SuckAxisSuck1Finish = true;
                            else
                                SuckAxisSuck2Finish = true;

                            if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisLeftPos, true, ref errcode))
                                Step = 7;
                            else
                            {
                                SuckAxisErrorSolution(1);
                                return;
                            }
                            WriteLog("【横移轴】case6：横移轴移动至左位结束");
                            break;
                        case 7://横移轴等待左位信号到位
                            WriteLog("【横移轴】case7：横移轴等待左侧信号到位开始");
                            //注意恢复
                            //if (!debugThreadSuckAxis)
                            //{
                            if (SuckAxisMove_WaitLeftSignals(SuckAxisMoveCircleCount))
                                Step = 8;
                            else
                            {
                                SuckAxisErrorSolution(100);
                                return;
                            }
                            //}
                            //else
                            //    Step = 8;
                            WriteLog("【横移轴】case7：横移轴等待左侧信号到位结束");
                            break;
                        case 8://横移轴Z气缸伸出
                            WriteLog("【横移轴】case8：横移轴Z气缸伸出开始");
                            //while ((SuckAxisMoveCircleCount + 1) > (3 + 2 * (UnloadModuleCircleCount + 1)))
                            //    Thread.Sleep(10);
                            if (SuckAxisMoveCircleCount % 2 == 0)
                                SuckAxisSuck1Finish = false;
                            else
                                SuckAxisSuck2Finish = false;

                            if (SuckAxisZStretch())
                                Step = 9;
                            else
                            {
                                SuckAxisErrorSolution(100);
                                return;
                            }
                            WriteLog("【横移轴】case8：横移轴Z气缸伸出结束");
                            break;
                        case 9://横移轴真空破
                            WriteLog("【横移轴】case9：横移轴真空破开始");
                            if (!debugThreadSuckAxis)
                            {
                                if (SuckAxisSuckerBreak())
                                    Step = 10;
                                else
                                {
                                    SuckAxisErrorSolution(100);
                                    return;
                                }
                            }
                            else
                                Step = 10;
                            WriteLog("【横移轴】case9：横移轴真空破结束");
                            break;
                        case 10://横移轴Z气缸缩回
                            WriteLog("【横移轴】case10：横移轴Z气缸缩回开始，SuckAxisCircleCount=" + SuckAxisMoveCircleCount.ToString());
                            if (SuckAxisZRetract())
                            {
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);

                                if (SuckAxisMoveCircleCount % 2 == 0)
                                {
                                    UnloadModuleCheckResult[3] = FinalCheckResultArrayTray[PartATrayNo, 0];
                                    UnloadModuleCheckResult[1] = FinalCheckResultArrayTray[PartATrayNo, 1];
                                    UnloadModuleCheckResult[5] = FinalCheckResultArrayTray[PartATrayNo, 2];
                                    UnloadModuleCheckResult[7] = FinalCheckResultArrayTray[PartATrayNo, 3];
                                    SuckAxisPlace1Finish = true;
                                    StartTime = DateTime.Now;
                                    while (true)
                                    {
                                        tempsignal = (!UnloadModuleMotionMoveCylinderFinish) && (!AutoRunPartAStretchFinish);
                                        if (tempsignal)
                                            break;
                                        else
                                        {
                                            if (!OutTimeCount(StartTime, 5))
                                            {
                                                SuckAxisErrorSolution(6);
                                                return;
                                            }
                                            Thread.Sleep(30);
                                        }
                                    }
                                    SuckAxisPlace1Finish = false;
                                }
                                else
                                {
                                    UnloadModuleCheckResult[2] = FinalCheckResultArrayTray[PartATrayNo, 0];
                                    UnloadModuleCheckResult[0] = FinalCheckResultArrayTray[PartATrayNo, 1];
                                    UnloadModuleCheckResult[4] = FinalCheckResultArrayTray[PartATrayNo, 2];
                                    UnloadModuleCheckResult[6] = FinalCheckResultArrayTray[PartATrayNo, 3];
                                    SuckAxisPlace2Finish = true;
                                    StartTime = DateTime.Now;
                                    while (true)
                                    {
                                        tempsignal = UnloadModuleMotionMoveCylinderFinish && (!AutoRunPartAStretchFinish);
                                        if (tempsignal)
                                            break;
                                        else
                                        {
                                            if (!OutTimeCount(StartTime, 15))
                                            {
                                                SuckAxisErrorSolution(7);
                                                return;
                                            }
                                            Thread.Sleep(30);
                                        }
                                    }
                                    SuckAxisPlace2Finish = false;
                                }
                                Step = 11;
                            }
                            else
                            {
                                SuckAxisErrorSolution(100);
                                return;
                            }
                            WriteLog("【横移轴】case10：横移轴Z气缸缩回结束");
                            break;
                        case 11://横移轴移动至右位
                            WriteLog("【横移轴】case11：横移轴移动至右位开始");
                            if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode))
                            {
                                SuckAxisMoveCircleCount++;
                                Step = 0;

                                if ((bDelayStop || bDelayStopCount) && (SuckAxisMoveCircleCount >= (2 * LoadGantryCircleCount + 3)))
                                    SuckAxisMoveEnable = false;
                            }
                            else
                            {
                                SuckAxisErrorSolution(2);
                                return;
                            }
                            WriteLog("【横移轴】case11：横移轴移动至右位结束");
                            break;
                    }
                }
                else
                {
                    switch (Step)
                    {
                        case 0://确保已经在右取放位
                            if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode))
                            {
                                Step = 1;
                            }
                            else
                            {
                                SuckAxisErrorSolution(0);
                                return;
                            }
                            break;
                        case 1://等待到位信号
                            if ((bDelayStop || bDelayStopCount) && (SuckAxisMoveCircleCount >= 2 * LoadGantryCircleCount))
                            {
                                if (SuckAxisMove_WaitSignalsSimple())
                                    Step = 2;
                                else
                                {
                                    SuckAxisErrorSolution(100);
                                    return;
                                }
                            }
                            else
                            {
                                if (!debugThreadSuckAxis)
                                {
                                    if (SuckAxisMove_WaitRightSignals(SuckAxisMoveCircleCount))
                                        Step = 2;
                                    else
                                    {
                                        SuckAxisErrorSolution(100);
                                        return;
                                    }
                                }
                                else
                                    Step = 2;
                            }
                            break;
                        case 2://横移轴Z气缸伸出
                            if (SuckAxisZStretch())
                                Step = 3;
                            else
                            {
                                SuckAxisErrorSolution(100);
                                return;
                            }
                            break;
                        case 3://横移轴真空吸工件
                            if (!debugThreadSuckAxis)
                                SuckAxisLoadSuckerSuck();
                            Step = 4;
                            break;
                        case 4://横移轴Z气缸缩回
                            if (SuckAxisZRetract())
                                Step = 5;
                            else
                            {
                                SuckAxisErrorSolution(100);
                                return;
                            }
                            break;
                        case 5://判断上料位真空信号
                            if (CurInfo.Di[(int)ECATDINAME.Di_SuckLoadVacumCheck + 25] == false)
                            {
                                SuckAxisErrorSolution(5);
                                return;
                            }
                            Step = 6;
                            break;
                        case 6://横移轴移动至左位
                            if (SuckAxisMoveCircleCount % 2 == 0)
                                SuckAxisSuck1Finish = true;
                            else
                                SuckAxisSuck2Finish = true;

                            if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisLeftPos, true, ref errcode))
                                Step = 7;
                            else
                            {
                                SuckAxisErrorSolution(1);
                                return;
                            }
                            break;
                        case 7://横移轴等待左位信号到位
                            if (SuckAxisMove_WaitSignalsSimple())
                                Step = 8;
                            else
                            {
                                SuckAxisErrorSolution(100);
                                return;
                            }
                            break;
                        case 8://横移轴Z气缸伸出
                            if (SuckAxisMoveCircleCount % 2 == 0)
                                SuckAxisSuck1Finish = false;
                            else
                                SuckAxisSuck2Finish = false;
                            if (SuckAxisZStretch())
                                Step = 9;
                            else
                            {
                                SuckAxisErrorSolution(100);
                                return;
                            }
                            break;
                        case 9://横移轴真空破
                            if (!debugThreadSuckAxis)
                            {
                                if (SuckAxisLoadSuckerBreak())
                                    Step = 10;
                                else
                                {
                                    SuckAxisErrorSolution(100);
                                    return;
                                }
                            }
                            else
                                Step = 10;
                            break;
                        case 10://横移轴Z气缸缩回
                            if (SuckAxisZRetract())
                            {
                                if (SuckAxisMoveCircleCount % 2 == 0)
                                {
                                    SuckAxisPlace1Finish = true;
                                    StartTime = DateTime.Now;
                                    while (true)
                                    {
                                        if (!AutoRunPartAStretchFinish)
                                            break;
                                        else
                                        {
                                            if (!OutTimeCount(StartTime, 5))
                                            {
                                                SuckAxisErrorSolution(8);
                                                return;
                                            }
                                            Thread.Sleep(30);
                                        }

                                    }
                                    SuckAxisPlace1Finish = false;
                                }
                                else
                                {
                                    SuckAxisPlace2Finish = true;
                                    StartTime = DateTime.Now;
                                    while (true)
                                    {
                                        if (!AutoRunPartAStretchFinish)
                                            break;
                                        else
                                        {
                                            if (!OutTimeCount(StartTime, 5))
                                            {
                                                SuckAxisErrorSolution(8);
                                                return;
                                            }
                                            Thread.Sleep(30);
                                        }
                                    }
                                    SuckAxisPlace2Finish = false;
                                }
                                Step = 11;
                            }
                            else
                            {
                                SuckAxisErrorSolution(100);
                                return;
                            }
                            break;
                        case 11://横移轴移动至右位
                            if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode))
                            {
                                SuckAxisMoveCircleCount++;
                                Step = 0;
                            }
                            else
                            {
                                SuckAxisErrorSolution(2);
                                return;
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(ThreadAbortException))
                {
                    string exStr = "";
                    exStr += "【横移轴】" + ex.ToString() + "\n";
                    exStr += "SuckAxisMoveCircleCount=" + SuckAxisMoveCircleCount.ToString() + "\n";
                    MessageBox.Show(exStr);
                }
            }
        }

        private void SuckAxisUnloadSuckRetry()
        {
            SuckAxisZStretch();
            SuckAxisUnloadSuckerSuck();
            SuckAxisZRetract();
        }

        private void SuckAxisSuckRetry()
        {
            SuckAxisZStretch();
            SuckAxisSuckerSuck();
            SuckAxisZRetract();
        }

        private bool SuckAxisMove_WaitLeftSignals(int CircleCount)
        {
            bool tempsignal = false;

            WriteLog("【横移轴】横移轴等待左到位信号开始，SuckAxisCircleCount=" + CircleCount.ToString());
            DateTime StartTime = DateTime.Now;
            if (CircleCount % 2 == 1)
            {
                while (true)
                {
                    tempsignal = CurInfo.Di[(int)ECATDINAME.Di_UnloadMoveStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadMoveRetractBit + 25]) &&
                                CurInfo.Di[(int)ECATDINAME.Di_UnloadMotionMoveRetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadMotionMoveStretchBit + 25]) &&
                                UnloadModuleEnable;

                    if (AutoRunPartAStretchFinish && tempsignal)
                    {
                        WriteLog("【横移轴】横移轴等待左到位信号完成，SuckAxisCircleCount=" + CircleCount.ToString());
                        break;
                    }
                    else
                    {
                        if (!OutTimeCount(StartTime, 80))
                        {
                            WarningSolution("【横移轴】【报警】：【152】横移轴等待入料工位伸出&下料模组移动气缸伸出信号超时");
                            return false;
                        }
                        Thread.Sleep(30);
                    }
                }
            }
            else
            {
                while (true)
                {
                    tempsignal = CurInfo.Di[(int)ECATDINAME.Di_UnloadMoveStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadMoveRetractBit + 25]) &&
                                 CurInfo.Di[(int)ECATDINAME.Di_UnloadMotionMoveStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadMotionMoveRetractBit + 25]) &&
                                 UnloadModuleEnable;

                    if (AutoRunPartAStretchFinish && tempsignal)
                    {
                        WriteLog("【横移轴】横移轴等待左到位信号完成，SuckAxisCircleCount=" + CircleCount.ToString());
                        break;
                    }
                    else
                    {
                        if (!OutTimeCount(StartTime, 80))
                        {
                            WarningSolution("【横移轴】【报警】：【153】横移轴等待入料工位伸出&下料模组位置移动气缸伸出信号超时");
                            return false;
                        }
                        Thread.Sleep(30);
                    }
                }
            }
            return true;
        }

        private bool SuckAxisMove_WaitSignalsSimple()
        {
            WriteLog("【横移轴】：横移轴等待载具伸出开始");
            DateTime StartTime = DateTime.Now;
            while (true)
            {
                if (AutoRunPartAStretchFinish)
                {
                    WriteLog("【横移轴】：横移轴等待载具伸出完成");
                    break;
                }
                else
                {
                    if (!OutTimeCount(StartTime, 40))
                    {
                        WarningSolution("【横移轴】【报警】：【154】横移轴等待到位信号超时：检查入料工位治具伸出相关气缸反馈");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        private bool SuckAxisMove_WaitRightSignals(int CircleCount)
        {
            WriteLog("【横移轴】：横移轴等待载具伸出&上料模组到位开始，SuckAxisCircleCount=" + CircleCount.ToString());
            bool tempsignal = false;
            DateTime StartTime = DateTime.Now;
            if (CircleCount % 2 == 0)
            {
                while (true)
                {
                    //tempsignal = CurInfo.Di[(int)ECATDINAME.Di_LoadMoveStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadMoveRetractBit + 25]) &&
                    //            CurInfo.Di[(int)ECATDINAME.Di_LoadMotionMoveRetractBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadMotionMoveStretchBit + 25]);
                    tempsignal = LoadModuleMoveCylinderFinish;
                    if (AutoRunPartAStretchFinish && tempsignal)
                    {
                        WriteLog("【横移轴】：横移轴等待载具伸出&上料模组到位完成，SuckAxisCircleCount=" + CircleCount.ToString());
                        break;
                    }
                    else
                    {
                        if (!OutTimeCount(StartTime, 70))
                        {
                            WarningSolution("【横移轴】【报警】：【155】横移轴等待入料工位伸出&上料模组移动气缸伸出信号超时");
                            return false;
                        }
                        Thread.Sleep(30);
                    }
                }
            }
            else
            {
                while (true)
                {
                    //tempsignal = CurInfo.Di[(int)ECATDINAME.Di_LoadMoveStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadMoveRetractBit + 25]) &&
                    //             CurInfo.Di[(int)ECATDINAME.Di_LoadMotionMoveStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_LoadMotionMoveRetractBit + 25]);
                    tempsignal = LoadModuleMotionMoveCylinderFinish;
                    if (AutoRunPartAStretchFinish && tempsignal)
                    {
                        WriteLog("【横移轴】：横移轴等待载具伸出&上料模组到位完成，SuckAxisCircleCount=" + CircleCount.ToString());
                        break;
                    }
                    else
                    {
                        if (!OutTimeCount(StartTime, 70))
                        {
                            WarningSolution("【横移轴】【报警】：【156】横移轴等待入料工位伸出&上料模组位置移动气缸伸出信号超时");
                            return false;
                        }
                        Thread.Sleep(30);
                    }
                }
            }
            return true;
        }

        public bool SuckAxisZStretch()
        {
            WriteLog("【横移轴】横移轴Z气缸伸出开始");
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_SuckZStretchControl, (int)ECATDONAME.Do_SuckZRetractControl, (int)ECATDINAME.Di_SuckZStretchBit, (int)ECATDINAME.Di_SuckZRetractBit))
            {
                WriteLog("【横移轴】横移轴Z气缸伸出完成");
                return true;
            }
            else
            {
                WarningSolution("【横移轴】【报警】：【157】横移轴Z气缸伸出错误:I23.06,I23.07");
                return false;
            }
        }

        public bool SuckAxisZRetract()
        {
            WriteLog("【横移轴】横移轴Z气缸缩回开始");
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_SuckZRetractControl, (int)ECATDONAME.Do_SuckZStretchControl, (int)ECATDINAME.Di_SuckZRetractBit, (int)ECATDINAME.Di_SuckZStretchBit))
            {
                WriteLog("【横移轴】横移轴Z气缸缩回完成");
                Thread.Sleep(50);
                return true;
            }
            else
            {
                WarningSolution("【横移轴】【报警】：【158】横移轴Z气缸缩回错误:I23.06,I23.07");
                return false;
            }
        }

        public int SuckAxisSuckerSuck()
        {
            WriteLog("【横移轴】横移轴吸嘴吸料开始");
            IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);

            //DateTime StartTime = DateTime.Now;
            //while (true)
            //{
            //    if ((CurInfo.Di[(int)ECATDINAME.Di_SuckLoadVacumCheck + 25] == true) && (CurInfo.Di[(int)ECATDINAME.Di_SuckUnloadVacumCheck + 25] == true))
            //    {
            //        WriteLog("【横移轴】横移轴吸嘴吸料完成");
            //        return 1;
            //    }
            //    else
            //    {
            //        if (!OutTimeCount(StartTime, 2))
            //        {
            //            if ((CurInfo.Di[(int)ECATDINAME.Di_SuckLoadVacumCheck + 25] == false) && (CurInfo.Di[(int)ECATDINAME.Di_SuckUnloadVacumCheck + 25] == true))
            //            {
            //                UpdateWaringLogNG.Invoke((object)"请重新放置上料模组上未吸取的工件");
            //                return -1;
            //            }

            //            if ((CurInfo.Di[(int)ECATDINAME.Di_SuckLoadVacumCheck + 25] == true) && (CurInfo.Di[(int)ECATDINAME.Di_SuckUnloadVacumCheck + 25] == false))
            //            {
            //                UpdateWaringLogNG.Invoke((object)"请重新放置入料工位上未吸取的工件");
            //                return -2;
            //            }

            //            if ((CurInfo.Di[(int)ECATDINAME.Di_SuckLoadVacumCheck + 25] == false) && (CurInfo.Di[(int)ECATDINAME.Di_SuckUnloadVacumCheck + 25] == false))
            //            {
            //                UpdateWaringLogNG.Invoke((object)"请重新放置未吸取的工件");
            //                return -3;
            //            }
            //        }
            //        Thread.Sleep(35);
            //    }
            //}
            Thread.Sleep(systemParam.SuckAxisSuckDelay);
            return 1;
        }

        private bool SuckAxisLoadSuckerSuck()
        {
            IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
            //注意恢复
            //DateTime StartTime = DateTime.Now;
            //while (true)
            //{
            //    if (CurInfo.Di[(int)ECATDINAME.Di_SuckLoadVacumCheck + 25] == true)
            //    {
            //        return true;
            //    }
            //    else
            //    {
            //        if (!OutTimeCount(StartTime, 5))
            //        {
            //            UpdateWaringLogNG.Invoke((object)"横移轴上料位吸取工件超时");
            //            return false;
            //        }
            //        Thread.Sleep(35);
            //    }
            //}
            Thread.Sleep(systemParam.SuckAxisSuckDelay);
            return true;
        }

        public bool SuckAxisSuckerBreak()
        {
            Thread.Sleep(systemParam.SuckAxisSuckerBreakDelay);

            WriteLog("【横移轴】横移轴吸嘴破真空开始");
            IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumSuck, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumSuck, false);
            //注意恢复
            //DateTime StartTime = DateTime.Now;
            //while (true)
            //{
            //    if ((CurInfo.Di[(int)ECATDINAME.Di_SuckLoadVacumCheck + 25] == false) && (CurInfo.Di[(int)ECATDINAME.Di_SuckUnloadVacumCheck + 25] == false))
            //    {
            //        //IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
            //        //IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);
            //        return true;
            //    }
            //    else
            //    {
            //        if (!OutTimeCount(StartTime, 5))
            //        {
            //            IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
            //            IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);
            //            UpdateWaringLogNG.Invoke((object)"【159】横移轴吸嘴真空破错误：I23.09,I23.10");
            //            return false;
            //        }
            //        Thread.Sleep(35);
            //    }
            //}
            Thread.Sleep(200);
            WriteLog("【横移轴】横移轴吸嘴破真空完成");
            return true;
        }

        private bool SuckAxisLoadSuckerBreak()
        {
            Thread.Sleep(systemParam.SuckAxisSuckerBreakDelay);

            IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumSuck, false);
            DateTime StartTime = DateTime.Now;
            while (true)
            {
                if (CurInfo.Di[(int)ECATDINAME.Di_SuckLoadVacumCheck + 25] == false)
                {
                    return true;
                }
                else
                {
                    if (!OutTimeCount(StartTime, 5))
                    {
                        IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
                        WarningSolution("【160】横移轴上料位吸嘴真空破错误:I23.09");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            //Thread.Sleep(200);
        }

        private bool SuckAxisUnloadSuckerSuck()
        {
            WriteLog("【横移轴】横移轴下料位吸嘴吸料开始");
            IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);
            //DateTime StartTime = DateTime.Now;
            //while (true)
            //{
            //    if (CurInfo.Di[(int)ECATDINAME.Di_SuckUnloadVacumCheck + 25] == true)
            //    {
            //        WriteLog("【横移轴】横移轴下料位吸嘴吸料完成");
            //        return true;
            //    }
            //    else
            //    {
            //        if (!OutTimeCount(StartTime, 2))
            //        {
            //            UpdateWaringLogNG.Invoke((object)"横移轴下料位吸取工件超时");
            //            WriteLog("【横移轴】【报警】：横移轴下料位吸取工件超时");
            //            return false;
            //        }
            //        Thread.Sleep(35);
            //    }
            //}
            Thread.Sleep(systemParam.SuckAxisSuckDelay);
            return true;
        }

        private bool SuckAxisUnloadSuckerBreak()
        {
            Thread.Sleep(systemParam.SuckAxisSuckerBreakDelay);

            IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumSuck, false);
            DateTime StartTime = DateTime.Now;
            while (true)
            {
                if (CurInfo.Di[(int)ECATDINAME.Di_SuckUnloadVacumCheck + 25] == false)
                {
                    return true;
                }
                else
                {
                    if (!OutTimeCount(StartTime, 5))
                    {
                        IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);
                        WarningSolution("【212】横移轴下料位吸嘴真空破错误:I23.11");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            //Thread.Sleep(200);
        }

        #endregion

        #region 下料模组移动

        public int UnloadModuleCircleCount = 0;
        public bool UnloadModuleEnable = false;
        public bool UnloadModuleFinished = false;
        public int UnloadModuleStep = 0;
        public bool UnloadModuleMoveCylinderFinish = false;
        public bool UnloadModuleMotionMoveCylinderFinish = false;
        public int[] UnloadModuleCheckResult = new int[8];//1代表通过，0代表无料，-1代表A，-2代表B，-3代表C

        public void UnloadModuleThread()
        {
            int errcode = 0;
            UnloadModuleCircleCount = 0;
            UnloadModuleStep = 0;

            //开启数据收集线程
            while (AutoRunActive)
            {
                if (UnloadModuleEnable)
                {
                    if (!UnloadModuleFinished)
                    {
                        UnloadModuleStatusSwitch(ref UnloadModuleStep, ref errcode);
                    }
                }
                Thread.Sleep(40);
            }
        }

        private void UnloadModuleStatusSwitch(ref int Step, ref int errcode)
        {
            bool tempsignal = false;
            DateTime starttime = DateTime.Now;

            try
            {
                switch (Step)
                {
                    case 0://下料模组位置气缸动作
                        WriteLog("【下料模组】case 0：下料模组移动气缸伸出开始");
                        if (UnloadModuleMoveStretch())
                        {
                            UnloadModuleCheckResult = new int[8];
                            UnloadModuleMoveCylinderFinish = true;
                            Thread.Sleep(100);
                            Step = 1;
                        }
                        else
                        {
                            UnloadModuleErrorSolution(100);
                            return;
                        }
                        WriteLog("【下料模组】case 0：下料模组移动气缸伸出完成，UnloadModuleMoveCylinderFinish=" + UnloadModuleMoveCylinderFinish.ToString());
                        break;
                    case 1://等待横移轴放置物料1
                        WriteLog("【下料模组】case 1：等待横移轴放料1开始");
                        if (!debugThreadUnloadModule)
                        {
                            if (UnloadModule_WaitWorkPieceLoad1())
                                Step = 2;
                            else
                            {
                                UnloadModuleErrorSolution(100);
                                return;
                            }
                        }
                        else
                            Step = 2;
                        break;
                    case 2://下料模组位置移动气缸动作
                        WriteLog("【下料模组】case 2：下料模组位置移动气缸伸出开始");
                        if (UnloadModulePosMoveStretch())
                        {
                            UnloadModuleMotionMoveCylinderFinish = true;
                            Step = 3;
                        }
                        else
                        {
                            UnloadModuleErrorSolution(100);
                            return;
                        }
                        WriteLog("【下料模组】case 2：下料模组位置移动气缸伸出完成，UnloadModuleMotionMoveCylinderFinish=" + UnloadModuleMotionMoveCylinderFinish.ToString());
                        break;
                    case 3://等待横移轴放置物料2
                        WriteLog("【下料模组】case 3：下料模组等待横移轴放料2开始");
                        if (!debugThreadUnloadModule)
                        {
                            if (UnloadModule_WaitWorkPieceLoad2())
                            {
                                Step = 4;
                            }
                            else
                            {
                                UnloadModuleErrorSolution(100);
                                return;
                            }
                        }
                        else
                            Step = 4;
                        break;
                    case 4://下料模组底座真空吸
                        WriteLog("【下料模组】case 4：下料模组真空底座吸开始");
                        if (UnloadModuleVacumSuck() == false)
                        {
                            UnloadModuleErrorSolution(100);
                            return;
                        }
                        Step = 5;
                        break;
                    case 5://移动气缸&位置移动气缸都缩回
                        WriteLog("【下料模组】case 5：下料模组双气缸缩回开始");
                        UnloadModuleMoveCylinderFinish = false;
                        UnloadModuleMotionMoveCylinderFinish = false;
                        IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadMoveCylinder, false);
                        IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadMoveStretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadMoveRetractControl, true);
                        starttime = DateTime.Now;
                        while (true)
                        {
                            tempsignal = (!CurInfo.Di[(int)ECATDINAME.Di_UnloadMotionMoveStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadMotionMoveRetractBit + 25] &&
                                         (!CurInfo.Di[(int)ECATDINAME.Di_UnloadMoveStretchBit + 25]) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadMoveRetractBit + 25]);
                            if (tempsignal)
                            {
                                WriteLog("【下料模组】case 5：下料模组双气缸缩回完成，UnloadModuleMoveCylinderFinish=" + UnloadModuleMoveCylinderFinish.ToString() + " UnloadModuleMotionMoveCylinderFinish=" + UnloadModuleMotionMoveCylinderFinish.ToString());
                                break;
                            }
                            else
                            {
                                if (!OutTimeCount(starttime, 10))
                                {
                                    UnloadModuleErrorSolution(0);
                                    return;
                                }
                                Thread.Sleep(30);
                            }
                        }
                        Step = 6;
                        break;
                    case 6://收尾工作
                        WriteLog("【下料模组】case 6：下料模组真空底座破开始");
                        if (UnloadModuleVacumBreak() == false)
                        {
                            UnloadModuleErrorSolution(100);
                            return;
                        }
                        Step = 0;
                        //等待下料模组彻底到位
                        Thread.Sleep(200);
                        UnloadModuleFinished = true;
                        UnloadModuleCircleCount++;
                        WriteLog("【横移轴】case 6：横移轴动作完成，UnloadModuleFinished=" + UnloadModuleFinished.ToString());
                        break;
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(ThreadAbortException))
                {
                    string exStr = "";
                    exStr += "【下料模组】" + ex.ToString() + "\n";
                    exStr += "UnloadModuleCircleCount=" + UnloadModuleCircleCount.ToString() + "\n";
                    MessageBox.Show(exStr);
                }
            }
        }

        private bool UnloadModuleMoveStretch()
        {
            WriteLog("【下料模组】移动气缸伸出开始");
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_UnloadMoveStretchControl, (int)ECATDONAME.Do_UnloadMoveRetractControl, (int)ECATDINAME.Di_UnloadMoveStretchBit, (int)ECATDINAME.Di_UnloadMoveRetractBit))
            {
                WriteLog("【下料模组】移动气缸伸出完成");
                return true;
            }
            else
            {
                WarningSolution("【下料模组】【报警】：【161】下料模组移动气缸伸出异常：I23.11,I23.12");
                return false;
            }
        }

        private bool UnloadModuleMoveRetract()
        {
            WriteLog("【下料模组】移动气缸缩回开始");
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_UnloadMoveRetractControl, (int)ECATDONAME.Do_UnloadMoveStretchControl, (int)ECATDINAME.Di_UnloadMoveRetractBit, (int)ECATDINAME.Di_UnloadMoveStretchBit))
            {
                WriteLog("【下料模组】移动气缸缩回完成");
                return true;
            }
            else
            {
                WarningSolution("【下料模组】【报警】：【162】下料模组移动气缸缩回异常：I23.11,I23.12");
                return false;
            }
        }

        private bool UnloadModulePosMoveStretch()
        {
            WriteLog("【下料模组】位置移动气缸伸出开始");
            if (WaitECATPiston1Cmd2FeedbackDone((int)ECATDONAME.Do_UnloadMoveCylinder, true, (int)ECATDINAME.Di_UnloadMotionMoveStretchBit, (int)ECATDINAME.Di_UnloadMotionMoveRetractBit))
            {
                WriteLog("【下料模组】位置移动气缸伸出完成");
                return true;
            }
            else
            {
                WarningSolution("【下料模组】【报警】：【163】下料模组位置移动气缸伸出异常：I23.13,I23.14");
                return false;
            }
        }

        private bool UnloadModulePosMoveRetract()
        {
            WriteLog("【下料模组】位置移动气缸缩回开始");
            if (WaitECATPiston1Cmd2FeedbackDone((int)ECATDONAME.Do_UnloadMoveCylinder, false, (int)ECATDINAME.Di_UnloadMotionMoveRetractBit, (int)ECATDINAME.Di_UnloadMotionMoveStretchBit))
            {
                WriteLog("【下料模组】位置移动气缸缩回完成");
                return true;
            }
            else
            {
                WarningSolution("【下料模组】【报警】：【164】下料模组位置移动气缸缩回异常：I23.13,I23.14");
                return false;
            }
        }

        private bool UnloadModuleVacumSuck()
        {
            WriteLog("【下料模组】下料模组真空吸开始");
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadVacumSuck, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadVacumBreak, false);
            WriteLog("【下料模组】下料模组真空吸完成");
            return true;
            //屏蔽真空监控
            //DateTime StartTime = DateTime.Now;
            //while (true)
            //{
            //    if (CurInfo.Di[(int)ECATDINAME.Di_UnloadVacumCheck + 25] == true)
            //    {
            //        return true;
            //    }
            //    else
            //    {
            //        if (!OutTimeCount(StartTime, 10))
            //        {
            //            UpdateWaringLogNG.Invoke((object)"下料模组真空吸出错");
            //            return false;
            //        }
            //        Thread.Sleep(35);
            //    }
            //}

        }

        private bool UnloadModuleVacumBreak()
        {
            WriteLog("【下料模组】下料模组真空破开始");
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadVacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadVacumSuck, false);

            Thread.Sleep(30);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadVacumBreak, false);
            WriteLog("【下料模组】下料模组真空破完成");
            return true;

            //屏蔽真空监控
            //DateTime StartTime = DateTime.Now;
            //while (true)
            //{
            //    if (CurInfo.Di[(int)ECATDINAME.Di_UnloadVacumCheck + 25] == false)
            //    {
            //        IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadVacumBreak, false);
            //        return true;
            //    }
            //    else
            //    {
            //        if (!OutTimeCount(StartTime, 10))
            //        {
            //            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadVacumBreak, false);
            //            UpdateWaringLogNG.Invoke((object)"下料模组真空破出错");
            //            return false;
            //        }
            //        Thread.Sleep(35);
            //    }
            //}
        }



        private bool UnloadModule_WaitWorkPieceLoad1()
        {
            WriteLog("【下料模组】等待横移轴放料1开始，SuckAxisPlace2Finish=" + SuckAxisPlace2Finish.ToString());
            DateTime starttime = DateTime.Now;
            while (true)
            {
                if (SuckAxisPlace2Finish)
                {
                    WriteLog("【下料模组】等待横移轴放料1完成，SuckAxisPlace2Finish=" + SuckAxisPlace2Finish.ToString());
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 60))
                    {
                        WarningSolution("【下料模组】【报警】：【165】下料模组等待横移轴放料1出错");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }

            return true;
        }

        private bool UnloadModule_WaitWorkPieceLoad2()
        {
            WriteLog("【下料模组】等待横移轴放料2开始，SuckAxisPlace1Finish=" + SuckAxisPlace1Finish.ToString());
            DateTime starttime = DateTime.Now;

            while (true)
            {
                if (SuckAxisPlace1Finish)
                {
                    WriteLog("【下料模组】等待横移轴放料2完成，SuckAxisPlace1Finish=" + SuckAxisPlace1Finish.ToString());
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 60))
                    {
                        WarningSolution("【下料模组】【报警】：【166】下料模组等待横移轴放料2出错");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }
        #endregion

        #region 下料龙门取放料
        public int UnloadGantryCircleCount = 0;
        public bool UnloadGantryEnable = false;
        public bool UnloadGantrySuckFinished = false;//8片取放完成的标志位
        public int UnloadGantryStep = 0;
        public MotionPos UnloadGantryMotionPos;//下料龙门Tray盘放置位置
        public MotionPos UnloadGantrySupplyMotionPos;//补料孔位
        public int[] UnloadGantryCheckResult = new int[8];//1代表通过A，0代表无料，-1代表B，-2代表C，-3代表D
        public int CurUnloadFullTraySeq = 0;//当前下料满Tray放置物料的顺序（0~14）
        public int NGBNum = 0; public int NGCNum = 0; public int NGDNum = 0;//各个级别的NG料盒中的工件数目

        public int[] SupplyRegion1Condition = new int[8];//补料盘1区的OK物料情况
        public int[] SupplyRegion2Condition = new int[8];//补料盘2区的OK物料情况

        public void UnloadGantryThread()
        {
            int errcode = 0;
            UnloadGantryCircleCount = 0;

            //开启数据收集线程
            while (AutoRunActive)
            {
                if (UnloadGantryEnable)
                {
                    UnloadGantryStatusSwitch(ref UnloadGantryStep, ref errcode);
                }
                Thread.Sleep(40);
            }
        }

        //把两盘Tray当作一个整体来看待，两盘料2*6*10共有15个4*2的小块
        private void UnloadGantryStatusSwitch(ref int Step, ref int errcode)
        {
            int Region1Pos = 100; int Region2Pos = 100;

            try
            {
                switch (Step)
                {
                    case 0:
                        //樊竞明添加20180908 确保Z在上方
                        if (UnloadGantryAllPistonZRetract() == false)
                        {
                            UnloadGantryErrorSolution(100);
                            return;
                        }
                        WriteLog("【下料龙门】case0：移动至下料模组正上方开始");
                        if (UnloadGantryMoveToUnloadModule() == false)
                        {
                            UnloadGantryErrorSolution(100);
                            return;
                        }
                        else
                            Step = 1;
                        break;
                    case 1:
                        WriteLog("【下料龙门】case1：等待下料模组撤回开始");
                        if (!debugThreadUnloadGantry)
                        {
                            if (WaitUnloadModuleInRetractPos())
                            {
                                UnloadGantryCheckResult = new int[8];
                                if (logicIgnore[1] || logicIgnore[2])
                                {
                                    for (int i = 0; i < UnloadGantryCheckResult.Length; i++)
                                        UnloadGantryCheckResult[i] = 1;
                                }
                                else
                                {
                                    for (int i = 0; i < UnloadGantryCheckResult.Length; i++)
                                        UnloadGantryCheckResult[i] = UnloadModuleCheckResult[i];
                                }
                            }
                            else
                            {
                                UnloadGantryErrorSolution(100);
                                return;
                            }
                        }
                        else
                        {
                            UnloadGantryCheckResult = new int[8];
                            for (int i = 0; i < UnloadGantryCheckResult.Length; i++)
                                UnloadGantryCheckResult[i] = 1;
                        }
                        Step = 2;
                        break;
                    case 2:
                        WriteLog("【下料龙门】case2：下料龙门气缸伸出开始");
                        if (UnloadGantryAllPistonZStretch() == false)
                        {
                            UnloadGantryErrorSolution(3);
                            return;
                        }
                        else
                            Step = 3;
                        break;
                    case 3:
                        WriteLog("【下料龙门】case3：下料龙门吸嘴吸取物料开始");
                        if (!debugThreadUnloadGantry)
                            UnloadGantryAllSuckerSuck();
                        Step = 4;
                        break;
                    //樊竞明20180908修改
                    case 4:
                        WriteLog("【下料龙门】case4：下料龙门所有吸嘴&不包含Z气缸缩回开始");
                        if (UnloadGantryAllPistonRetract() == false)
                        {
                            UnloadGantryErrorSolution(100);
                            return;
                        }
                        else
                        {
                            Step = 5;
                            //Step = 7;
                            //UnloadModuleFinished = false;//下料模组开始工作
                        }
                        break;
                    case 5:
                        WriteLog("【下料龙门】case5：判断是否完全吸料开始");
                        Thread.Sleep(100);
                        if ((CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft1VacumCheck + 25] && CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft2VacumCheck + 25] &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft3VacumCheck + 25] && CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft4VacumCheck + 25] &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadRight1VacumCheck + 25] && CurInfo.Di[(int)ECATDINAME.Di_UnloadRight2VacumCheck + 25] &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadRight3VacumCheck + 25] && CurInfo.Di[(int)ECATDINAME.Di_UnloadRight4VacumCheck + 25]) == false)
                        {
                            UnloadGantryErrorSolution(0);
                            return;
                        }
                        Step = 6;
                        break;
                    case 6://判断是否NG下料
                        WriteLog("【下料龙门】case6：下料模组开始工作&开始下料");
                        UnloadModuleFinished = false;//下料模组开始工作
                        if (GetUnloadGantryNGNum(UnloadGantryCheckResult) != 0 && (isDisThrow == false))//添加屏蔽抛料  0905 add by ben
                        {
                            WriteLog("【下料龙门】存在NG料/空料，开始抛料");
                            if (IsNGBExist(UnloadGantryCheckResult))
                            {
                                if (UnloadGantryPlaceNGWorkPiece(UnloadGantryCheckResult, -1) == false)
                                {
                                    UnloadGantryErrorSolution(100);
                                    return;
                                }
                            }
                            if (IsNGCExist(UnloadGantryCheckResult))
                            {
                                if (UnloadGantryPlaceNGWorkPiece(UnloadGantryCheckResult, -2) == false)
                                {
                                    UnloadGantryErrorSolution(100);
                                    return;
                                }
                            }
                            if (IsNGDExist(UnloadGantryCheckResult))
                            {
                                if (UnloadGantryPlaceNGWorkPiece(UnloadGantryCheckResult, -3) == false)
                                {
                                    UnloadGantryErrorSolution(100);
                                    return;
                                }
                            }
                            if (IsNGEExist(UnloadGantryCheckResult))
                            {
                                if (UnloadGantryPlaceNGWorkPiece(UnloadGantryCheckResult, -4) == false)
                                {
                                    UnloadGantryErrorSolution(100);
                                    return;
                                }
                            }
                            if (!bIgnoreSupply)//如果不屏蔽补料
                                Step = 8;//NG数目不为0
                            else
                                Step = 7;//屏蔽补料
                            WriteLog("【下料龙门】case 6：抛料完成");
                        }
                        else
                        {
                            Step = 7;//NG数目为0
                            WriteLog("【下料龙门】case 6：全OK");
                        }
                        break;
                    case 7://不存在NG料且不存在空料，直接下料至Tray盘
                        WriteLog("【下料龙门】case 7：等待下料龙门放料开始");
                        //樊竞明20180908添加
                        if (UnloadGantryAllPistonZRetract() == false)
                        {
                            UnloadGantryErrorSolution(100);
                            return;
                        }

                        UnloadGantryPlaceAllWorkPieceFinish = false;
                        DateTime starttime = DateTime.Now;
                        while (true)
                        {
                            if (UnloadGantryPlaceAllWorkPieceFinish)
                                break;
                            else
                            {
                                if (!OutTimeCount(starttime, 70))
                                {
                                    WarningSolution("下料龙门放料超时");
                                    UnloadGantryErrorSolution(100);
                                    return;
                                }
                                Thread.Sleep(40);
                            }
                        }
                        WriteLog("【下料龙门】case 7：等待下料龙门放料完成");
                        UnloadGantryCircleCount++;
                        WriteLog("【下料龙门】Case 7：UnloadGantryCircleCount++; iDelayStopCount：" + iDelayStopCount.ToString() + "; LoadGantryCircleCount:" + LoadGantryCircleCount.ToString() + "; UnloadGantryCircleCount:" + UnloadGantryCircleCount.ToString());
                        Step = 0;
                        if ((bDelayStop || bDelayStopCount) && (UnloadGantryCircleCount >= LoadGantryCircleCount))
                        {
                            bDelayStop = false;
                            bDelayStopCount = false;
                            iDelayStopCount = -1;
                            SwitchToEmgStopMode();
                            WriteLog("【下料龙门】case 7：延时停止完成");
                            MessageBox.Show("延时停止完成");
                            return;
                        }
                        break;
                    case 8://存在NG料或空料
                        if (GetRegion1CurSupplyPos(SupplyRegion1Condition) == 8) //补料区域1已空
                        {
                            Step = 9;
                            WriteLog("【下料龙门】case 8：补料区域1已空");
                        }
                        else//补料区域2已空
                        {
                            Step = 10;
                            WriteLog("【下料龙门】case 8：补料区域2已空");
                        }
                        break;
                    case 9://补料区域1已空
                        int tempresult9 = UnloadAllWorkPieceOnRegion(1);
                        if (tempresult9 == 1)
                        {
                            for (int i = 0; i < UnloadGantryCheckResult.Length; i++)
                            {
                                if (UnloadGantryCheckResult[i] == 1)
                                    SupplyRegion1Condition[i] = 1;
                                else
                                    SupplyRegion1Condition[i] = 0;
                            }
                            Step = 11;
                        }
                        else
                        {
                            if (tempresult9 == -2)
                                UnloadGantryErrorSolution(6);
                            else
                                UnloadGantryErrorSolution(100);
                            return;
                        }
                        break;
                    case 10://补料区域2已空
                        int tempresult10 = UnloadAllWorkPieceOnRegion(2);
                        if (tempresult10 == 1)
                        {
                            for (int i = 0; i < UnloadGantryCheckResult.Length; i++)
                            {
                                if (UnloadGantryCheckResult[i] == 1)
                                    SupplyRegion2Condition[i] = 1;
                                else
                                    SupplyRegion2Condition[i] = 0;
                            }
                            Step = 11;
                        }
                        else
                        {
                            if (tempresult10 == -2)
                                UnloadGantryErrorSolution(7);
                            else
                                UnloadGantryErrorSolution(100);
                            return;
                        }
                        break;
                    case 11://把区域1的工件补充至区域2
                        if (GetRegion2CurFullPos(SupplyRegion2Condition) != 8)//补料区域2有料
                        {
                            WriteLog("【下料龙门】case 11：补料动作开始");
                            while (GetRegion2CurNullPos(SupplyRegion2Condition) != 8 && GetRegion1CurSupplyPos(SupplyRegion1Condition) != 8)//开始补料
                            {
                                Region1Pos = GetRegion1CurSupplyPos(SupplyRegion1Condition);
                                Region2Pos = GetRegion2CurNullPos(SupplyRegion2Condition);

                                int tempresult = MoveRegion1ToRegion2(Region1Pos, Region2Pos);
                                if (tempresult != 1)
                                {
                                    if (tempresult == -2)
                                        UnloadGantryErrorSolution(2);
                                    else if (tempresult == -4)
                                        UnloadGantryErrorSolution(4);
                                    else
                                        UnloadGantryErrorSolution(100);
                                    return;
                                }
                                SupplyRegion1Condition[Region1Pos] = 0;
                                SupplyRegion2Condition[Region2Pos] = 1;

                                Thread.Sleep(40);
                            }
                            WriteLog("【下料龙门】case 11：补料动作完成");
                            Step = 12;
                        }
                        else
                        {
                            UnloadGantryCircleCount++;
                            WriteLog("【下料龙门】Case 11：UnloadGantryCircleCount++; iDelayStopCount：" + iDelayStopCount.ToString() + "; LoadGantryCircleCount:" + LoadGantryCircleCount.ToString() + "; UnloadGantryCircleCount:" + UnloadGantryCircleCount.ToString());
                            Step = 0;
                            if ((bDelayStop || bDelayStopCount) && (UnloadGantryCircleCount >= LoadGantryCircleCount))
                            {
                                if (bDelayStop)
                                    bDelayStop = false;
                                if (bDelayStopCount)
                                    bDelayStopCount = false;
                                iDelayStopCount = -1;
                                SwitchToEmgStopMode();
                                WriteLog("【下料龙门】case 10：延时停止完成");
                                MessageBox.Show("延时停止完成");
                                return;
                            }
                        }
                        break;
                    case 12:
                        if (GetRegion2CurNullPos(SupplyRegion2Condition) == 8)//补料区域2已满
                        {
                            WriteLog("【下料龙门】case 12：补料区域2已满，移动至补料区域2开始");
                            if (UnloadGantryXYMove(new double[] { systemParam.UnloadSupplyRegion2PosX, systemParam.UnloadSupplyRegion2PosY }, true) == true)
                            {
                                WriteLog("【下料龙门】case 12：移动至补料区域2完成");
                                Step = 13;
                            }
                            else
                            {
                                WarningSolution("【下料龙门】【报警】：【173】下料龙门移动至补料盘区域2上方出错");
                                UnloadGantryErrorSolution(100);
                                return;
                            }
                        }
                        else
                        {
                            Step = 0;
                            UnloadGantryCircleCount++;
                            WriteLog("【下料龙门】Case 12：UnloadGantryCircleCount++; iDelayStopCount：" + iDelayStopCount.ToString() + "; LoadGantryCircleCount:" + LoadGantryCircleCount.ToString() + "; UnloadGantryCircleCount:" + UnloadGantryCircleCount.ToString());

                            if ((bDelayStop || bDelayStopCount) && (UnloadGantryCircleCount >= LoadGantryCircleCount))
                            {
                                bDelayStop = false;
                                bDelayStopCount = false;
                                iDelayStopCount = -1;
                                SwitchToEmgStopMode();
                                WriteLog("【下料龙门】case 11：延时停止完成");
                                MessageBox.Show("延时停止完成");
                                return;
                            }
                        }
                        break;
                    case 13:
                        WriteLog("【下料龙门】case 13：所有吸嘴&Z气缸伸出开始");
                        if (UnloadGantryAllPistonZStretch())
                            Step = 14;
                        else
                        {
                            UnloadGantryErrorSolution(5);
                            return;
                        }
                        break;
                    case 14:
                        WriteLog("【下料龙门】case 14：所有吸嘴吸取开始");
                        if (!debugThreadUnloadGantry)
                            UnloadGantryAllSuckerSuck();
                        Step = 15;
                        break;
                    case 15:
                        WriteLog("【下料龙门】case 15：所有吸嘴&Z气缸缩回开始");
                        if (UnloadGantryAllPistonZRetract())
                            Step = 16;
                        else
                        {
                            UnloadGantryErrorSolution(100);
                            return;
                        }
                        break;
                    case 16://判断是否全部吸取
                        WriteLog("【下料龙门】case 16：判断下料龙门吸嘴是否全部吸取开始");
                        Thread.Sleep(100);
                        if ((CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft1VacumCheck + 25] && CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft2VacumCheck + 25] &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft3VacumCheck + 25] && CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft4VacumCheck + 25] &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadRight1VacumCheck + 25] && CurInfo.Di[(int)ECATDINAME.Di_UnloadRight2VacumCheck + 25] &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadRight3VacumCheck + 25] && CurInfo.Di[(int)ECATDINAME.Di_UnloadRight4VacumCheck + 25]) == false)
                        {
                            UnloadGantryAllPistonZStretch();
                            UnloadGantryAllSuckerSuck();
                            UnloadGantryAllPistonZRetract();
                            Thread.Sleep(100);
                            if ((CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft1VacumCheck + 25] && CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft2VacumCheck + 25] &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft3VacumCheck + 25] && CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft4VacumCheck + 25] &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadRight1VacumCheck + 25] && CurInfo.Di[(int)ECATDINAME.Di_UnloadRight2VacumCheck + 25] &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadRight3VacumCheck + 25] && CurInfo.Di[(int)ECATDINAME.Di_UnloadRight4VacumCheck + 25]) == false)
                            {
                                UnloadGantryErrorSolution(1);
                                return;
                            }
                        }
                        SupplyRegion2Condition = new int[8];
                        Step = 17;
                        break;
                    case 17:
                        WriteLog("【下料龙门】case 16：等待下料龙门放料开始");
                        UnloadGantryPlaceAllWorkPieceFinish = false;
                        starttime = DateTime.Now;
                        while (true)
                        {
                            if (UnloadGantryPlaceAllWorkPieceFinish)
                                break;
                            else
                            {
                                if (!OutTimeCount(starttime, 70))
                                {
                                    WarningSolution("下料龙门放料超时");
                                    UnloadGantryErrorSolution(100);
                                    return;
                                }
                                Thread.Sleep(40);
                            }
                        }
                        UnloadGantryCircleCount++;
                        WriteLog("【下料龙门】Case 16：UnloadGantryCircleCount++; iDelayStopCount：" + iDelayStopCount.ToString() + "; LoadGantryCircleCount:" + LoadGantryCircleCount.ToString() + "; UnloadGantryCircleCount:" + UnloadGantryCircleCount.ToString());

                        Step = 0;
                        if ((bDelayStop || bDelayStopCount) && (UnloadGantryCircleCount >= LoadGantryCircleCount))
                        {
                            if (bDelayStop)
                                bDelayStop = false;
                            if (bDelayStopCount)
                                bDelayStopCount = false;
                            iDelayStopCount = -1;
                            SwitchToEmgStopMode();
                            WriteLog("【下料龙门】case 16：延时停止完成");
                            MessageBox.Show("延时停止完成");
                            return;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(ThreadAbortException))
                {
                    string exStr = "";
                    exStr += "【下料龙门】" + ex.ToString() + "\n";
                    exStr += "UnloadGantryCircleCount=" + UnloadGantryCircleCount.ToString() + "\n";
                    MessageBox.Show(exStr);
                }
            }
        }

        public bool UnloadGantryMoveToUnloadModule()
        {
            WriteLog("【下料龙门】移动至下料模组取料位开始");
            if (UnloadGantryXYMove(new double[] { systemParam.UnloadTrayFinishPosX, systemParam.UnloadTrayFinishPosY }, true))
            {
                WriteLog("【下料龙门】移动至下料模组取料位完成");
                return true;
            }
            else
            {
                WarningSolution("【下料龙门】【报警】：【167】龙门下料轴运行至下料模组位置正上方错误");
                return false;
            }
        }

        //龙门下料取料然后放NG料，然后补料，然后放入下料满Tray，0~14
        public bool UnloadGantrySuckAndPlace()
        {
            if (UnloadGantryMoveToUnloadModule() == false)
                return false;

            if (!debugThreadUnloadGantry)
            {
                if (WaitUnloadModuleInRetractPos() == false)
                    return false;
            }

            if (!debugThreadUnloadGantry)
            {
                UnloadGantryCheckResult = new int[8];
                for (int i = 0; i < UnloadGantryCheckResult.Length; i++)
                    UnloadGantryCheckResult[i] = UnloadModuleCheckResult[i];
            }
            if (UnloadGantryAllPistonZStretch() == false)
                return false;

            if (UnloadGantryAllSuckerSuck() == false)
                return false;

            if (UnloadGantryAllPistonZRetract() == false)
                return false;

            if (UnloadGantryPlaceAllWorkPiece(ref CurUnloadFullTraySeq) == false)//0~14
                return false;

            return true;
        }

        private bool WaitUnloadModuleInRetractPos()
        {
            WriteLog("【下料模组】等待下料模组到位开始");
            bool tempsignal = false;
            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = (!CurInfo.Di[(int)ECATDINAME.Di_UnloadMotionMoveStretchBit + 25]) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadMotionMoveRetractBit + 25]) &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadMoveStretchBit + 25]) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadMoveRetractBit + 25]) &&
                             (UnloadModuleEnable == true);

                if (SuckAxisMoveCircleCount >= 4 && tempsignal)
                {
                    WriteLog("【下料模组】等待下料模组到位完成");
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 60))
                    {
                        WarningSolution("【下料模组】【报警】：【168】龙门下料等待下料模组缩回超时：I23.11~I23.14");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        //检查补料盘区域1编号最小的可补料位置，若返回8，则表明没有可补料的位置，当前为空盘
        private int GetRegion1CurSupplyPos(int[] CurCondition)
        {
            for (int i = 0; i < CurCondition.Length; i++)
            {
                if (CurCondition[i] == 1)
                    return i;
            }
            return 8;
        }

        private int GetRegion2CurNullPos(int[] CurCondition)
        {
            for (int i = 0; i < CurCondition.Length; i++)
            {
                if (CurCondition[i] == 0)
                    return i;
            }
            return 8;
        }

        private int GetRegion2CurFullPos(int[] CurCondition)
        {
            for (int i = 0; i < CurCondition.Length; i++)
            {
                if (CurCondition[i] == 1)
                    return i;
            }
            return 8;
        }

        private int GetRegionCurNum(int[] CurCondition)
        {
            int totalnum = 0;
            for (int i = 0; i < CurCondition.Length; i++)
            {
                if (CurCondition[i] == 1)
                    totalnum++;
            }
            return totalnum;
        }

        private int UnloadAllWorkPieceOnRegion(int No)
        {
            if (No == 1)
            {
                WriteLog("【下料龙门】移动至补料区域1开始");
                if (UnloadGantryXYMove(new double[] { systemParam.UnloadSupplyRegion1PosX, systemParam.UnloadSupplyRegion1PosY }, true) == false)
                {
                    WarningSolution("【下料龙门】【报警】：【169】下料龙门移动至补料盘区域1上方出错");
                    return -1;
                }
                WriteLog("【下料龙门】移动至补料区域1完成");
            }

            if (No == 2)
            {
                WriteLog("【下料龙门】移动至补料区域2开始");
                if (UnloadGantryXYMove(new double[] { systemParam.UnloadSupplyRegion2PosX, systemParam.UnloadSupplyRegion2PosY }, true) == false)
                {
                    WarningSolution("【下料龙门】【报警】：【170】下料龙门移动至补料盘区域2上方出错");
                    return -1;
                }
                WriteLog("【下料龙门】移动至补料区域2完成");
            }

            if (UnloadGantryAllPistonZStretch() == false)
                return -2;

            if (UnloadGantryAllSuckerBreak() == false)
                return -3;

            //if (UnloadGantryAllPistonZRetract() == false)
            //    return -4;
            if (UnloadGantryAllPistonRetract() == false)//樊竞明0908
                return -4;

            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft1VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft2VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft3VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft4VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight1VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight2VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight3VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight4VacumBreak, false);

            WriteLog("【下料龙门】补料区域" + No.ToString() + "放料完成");
            return 1;
        }

        public int MoveRegion1ToRegion2(int region1Pos, int region2Pos)
        {
            int errcode = 0; double poserror = 0;

            if (adlink.LineMove(new Axis[] { logicConfig.ECATAxis[2], logicConfig.ECATAxis[3] }, new double[] { UnloadGantrySupplyMotionPos.posInfo[region1Pos].XPos, UnloadGantrySupplyMotionPos.posInfo[region1Pos].YPos }, true, ref errcode, ref poserror) == false)
            {
                WarningSolution("【171】下料龙门吸嘴G移动至补料盘区域1-" + region1Pos.ToString() + "孔位上方出错");
                return -1;
            }

            if (UnloadGantrySuckWorkPiece(3) == false)
                return -2;

            if (adlink.LineMove(new Axis[] { logicConfig.ECATAxis[2], logicConfig.ECATAxis[3] }, new double[] { UnloadGantrySupplyMotionPos.posInfo[region2Pos + 8].XPos, UnloadGantrySupplyMotionPos.posInfo[region2Pos + 8].YPos }, true, ref errcode, ref poserror) == false)
            {
                WarningSolution("【172】下料龙门吸嘴G移动至补料盘区域2-" + region1Pos.ToString() + "孔位上方出错");
                return -3;
            }

            if (UnloadGantryPlaceWorkPiece(3) == false)
                return -4;

            return 1;
        }

        private bool UnloadGantrySuckSupplyRegion2Workpiece()
        {
            int errcode = 0; double poserror = 0;

            if (adlink.LineMove(new Axis[] { logicConfig.ECATAxis[2], logicConfig.ECATAxis[3] }, new double[] { systemParam.UnloadSupplyRegion2PosX, systemParam.UnloadSupplyRegion2PosY }, true, ref errcode, ref poserror) == false)
            {
                WarningSolution("【173】下料龙门移动至补料盘区域2上方出错");
                return false;
            }

            if (UnloadGantryAllPistonZStretch() == false)
                return false;

            if (!debugThreadUnloadGantry)
            {
                if (UnloadGantryAllSuckerSuck() == false)
                    return false;
            }

            if (UnloadGantryAllPistonZRetract() == false)
                return false;

            return true;
        }

        //只用于放置全部OK的料，到下料满Tray放置工件，整体来看共15步，0~14
        //已经包含Tray盘满了以后的替换动作
        public int UnloadGantryPlaceAllWorkPieceCircleCount = 0;
        public bool UnloadGantryPlaceAllWorkPieceEnable = false;
        public bool UnloadGantryPlaceAllWorkPieceFinish = false;
        public int UnloadGantryPlaceAllWorkPieceStep = 0;

        public void UnloadGantryPlaceAllWorkPieceThread()
        {
            while (AutoRunActive)
            {
                if (UnloadGantryPlaceAllWorkPieceEnable)
                {
                    if (!UnloadGantryPlaceAllWorkPieceFinish)
                    {
                        UnloadGantryPlaceAllWorkPieceStatusSwitch(ref UnloadGantryPlaceAllWorkPieceStep, ref CurUnloadFullTraySeq);
                    }
                }
                Thread.Sleep(40);
            }
        }

        public bool UnloadGantryAllSuckClose()
        {
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft1VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft2VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft3VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft4VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight1VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight2VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight3VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight4VacumBreak, false);
            return true;
        }
        private void UnloadGantryPlaceAllWorkPieceStatusSwitch(ref int Step, ref int Seq)
        {
            try
            {
                #region 下料龙门放料情况1
                if (Seq <= 5)
                {
                    switch (Step)
                    {
                        case 0:
                            UnloadGantryAllPistonZRetract();//确保下料龙门Z气缸&所有吸嘴都缩回
                            if (UnloadGantryMoveAxis(Seq) == false)//下料龙门移动到满Tray相应位置上方
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 1;
                            break;
                        case 1:
                            if (UnloadGantryAllPistonZStretch() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(1);
                                return;
                            }
                            else
                                Step = 2;
                            Thread.Sleep(200);
                            break;
                        case 2:
                            if (UnloadGantryAllSuckerBreak() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 3;
                            break;
                        case 3:
                            if (UnloadGantryAllPistonZRetract() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                            {
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft1VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft2VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft3VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft4VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight1VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight2VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight3VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight4VacumBreak, false);
                                Step = 4;
                            }
                            UnloadGantryAllSuckClose();
                            break;
                        case 4:
                            Seq++;
                            Step = 0;
                            UnloadGantryPlaceAllWorkPieceFinish = true;
                            UnloadGantryPlaceAllWorkPieceCircleCount++;
                            WriteLog("【下料龙门放料】下料龙门放料完成，UnloadGantryPlaceAllWorkPieceCircleCount：" + UnloadGantryPlaceAllWorkPieceCircleCount.ToString());
                            return;
                    }
                }
                #endregion

                #region 下料龙门放料情况2
                if (Seq == 6)
                {
                    switch (Step)
                    {
                        case 0:
                            UnloadGantryAllPistonZRetract();//确保下料龙门Z气缸&所有吸嘴都缩回
                            if (UnloadGantryMoveAxis(6) == false)//下料龙门移动到满Tray相应位置上方
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 1;
                            break;
                        case 1:
                            if (UnloadGantryLeftPistonZStretch() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(1);
                                return;
                            }
                            else
                                Step = 2;
                            Thread.Sleep(200);
                            break;
                        case 2:
                            if (UnloadGantryLeftSuckerBreak() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 3;
                            break;
                        case 3:
                            if (UnloadGantryLeftPistonZRetract() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 4;
                            UnloadGantryAllSuckClose();
                            break;
                        case 4:
                            if (UnloadGantryMoveAxis(7) == false)//下料龙门移动到满Tray相应位置上方
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 5;
                            break;
                        case 5:
                            if (UnloadGantryRightPistonZStretch() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(2);
                                return;
                            }
                            else
                                Step = 6;
                            Thread.Sleep(200);
                            break;
                        case 6:
                            if (UnloadGantryRightSuckerBreak() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 7;
                            break;
                        case 7:
                            if (UnloadGantryRightPistonZRetract() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 8;
                            UnloadGantryAllSuckClose();
                            break;
                        case 8:
                            Seq++;
                            Step = 0;
                            UnloadGantryPlaceAllWorkPieceFinish = true;
                            UnloadGantryPlaceAllWorkPieceCircleCount++;
                            WriteLog("【下料龙门放料】下料龙门放料完成，UnloadGantryPlaceAllWorkPieceCircleCount：" + UnloadGantryPlaceAllWorkPieceCircleCount.ToString());
                            return;
                    }
                }
                #endregion

                #region 下料龙门放料情况3
                if (Seq == 7)
                {
                    switch (Step)
                    {
                        case 0:
                            UnloadGantryAllPistonZRetract();//确保下料龙门Z气缸&所有吸嘴都缩回
                            if (UnloadGantryMoveAxis(8) == false)//下料龙门移动到满Tray相应位置上方
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 1;
                            break;
                        case 1:
                            if (UnloadGantryLeftPistonZStretch() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(1);
                                return;
                            }
                            else
                                Step = 2;
                            Thread.Sleep(200);
                            break;
                        case 2:
                            if (UnloadGantryLeftSuckerBreak() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 3;
                            break;
                        case 3:
                            if (UnloadGantryLeftPistonZRetract() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 4;
                            UnloadGantryAllSuckClose();
                            break;
                        case 4:
                            WriteLog("【下料龙门放料】下料Tray换Tray盘开始");
                            UnloadTrayFinished = false;
                            DateTime starttime = DateTime.Now;
                            while (true)
                            {
                                if (UnloadTrayFinished)
                                    break;
                                else
                                {
                                    if (!OutTimeCount(starttime, 60))
                                    {
                                        WarningSolution("下料Tray换Tray超时");
                                        UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                        return;
                                    }
                                    Thread.Sleep(35);
                                }
                            }
                            WriteLog("【下料龙门放料】下料Tray换Tray盘完成");
                            Step = 5;
                            break;
                        case 5:
                            if (UnloadGantryMoveAxis(9) == false)//下料龙门移动到满Tray相应位置上方
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 6;
                            break;
                        case 6:
                            if (UnloadGantryRightPistonZStretch() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(3);
                                return;
                            }
                            else
                                Step = 7;
                            Thread.Sleep(200);
                            break;
                        case 7:
                            if (UnloadGantryRightSuckerBreak() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 8;
                            break;
                        case 8:
                            if (UnloadGantryRightPistonZRetract() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 9;
                            UnloadGantryAllSuckClose();
                            break;
                        case 9:
                            Seq++;
                            Step = 0;
                            UnloadGantryPlaceAllWorkPieceFinish = true;
                            UnloadGantryPlaceAllWorkPieceCircleCount++;
                            WriteLog("【下料龙门放料】下料龙门放料完成，UnloadGantryPlaceAllWorkPieceCircleCount：" + UnloadGantryPlaceAllWorkPieceCircleCount.ToString());
                            return;
                    }
                }
                #endregion

                #region 下料龙门放料情况4
                if (Seq == 8)
                {
                    switch (Step)
                    {
                        case 0:
                            UnloadGantryAllPistonZRetract();//确保下料龙门Z气缸&所有吸嘴都缩回
                            if (UnloadGantryMoveAxis(10) == false)//下料龙门移动到满Tray相应位置上方
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 1;
                            break;
                        case 1:
                            if (UnloadGantryLeftPistonZStretch() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(1);
                                return;
                            }
                            else
                                Step = 2;
                            Thread.Sleep(200);
                            break;
                        case 2:
                            if (UnloadGantryLeftSuckerBreak() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 3;
                            break;
                        case 3:
                            if (UnloadGantryLeftPistonZRetract() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 4;
                            UnloadGantryAllSuckClose();
                            break;
                        case 4:
                            if (UnloadGantryMoveAxis(11) == false)//下料龙门移动到满Tray相应位置上方
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 5;
                            break;
                        case 5:
                            if (UnloadGantryRightPistonZStretch() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 6;
                            Thread.Sleep(200);
                            break;
                        case 6:
                            if (UnloadGantryRightSuckerBreak() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 7;
                            break;
                        case 7:
                            if (UnloadGantryRightPistonZRetract() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 8;
                            UnloadGantryAllSuckClose();
                            break;
                        case 8:
                            Seq++;
                            Step = 0;
                            UnloadGantryPlaceAllWorkPieceFinish = true;
                            UnloadGantryPlaceAllWorkPieceCircleCount++;
                            WriteLog("【下料龙门放料】下料龙门放料完成，UnloadGantryPlaceAllWorkPieceCircleCount：" + UnloadGantryPlaceAllWorkPieceCircleCount.ToString());
                            return;
                    }
                }
                #endregion

                #region 下料龙门放料情况5
                if (Seq >= 9 && Seq <= 13)
                {
                    switch (Step)
                    {
                        case 0:
                            UnloadGantryAllPistonZRetract();//确保下料龙门Z气缸&所有吸嘴都缩回
                            if (UnloadGantryMoveAxis(Seq + 3) == false)//下料龙门移动到满Tray相应位置上方
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 1;
                            break;
                        case 1:
                            if (UnloadGantryAllPistonZStretch() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(1);
                                return;
                            }
                            else
                                Step = 2;
                            Thread.Sleep(200);
                            break;
                        case 2:
                            if (UnloadGantryAllSuckerBreak() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 3;
                            break;
                        case 3:
                            if (UnloadGantryAllPistonZRetract() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                            {
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft1VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft2VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft3VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft4VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight1VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight2VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight3VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight4VacumBreak, false);
                                Step = 4;
                            }

                            UnloadGantryAllSuckClose();
                            break;
                        case 4:
                            Seq++;
                            Step = 0;
                            UnloadGantryPlaceAllWorkPieceFinish = true;
                            UnloadGantryPlaceAllWorkPieceCircleCount++;
                            WriteLog("【下料龙门放料】下料龙门放料完成，UnloadGantryPlaceAllWorkPieceCircleCount：" + UnloadGantryPlaceAllWorkPieceCircleCount.ToString());
                            return;
                    }
                }
                #endregion

                #region 下料龙门放料情况6
                if (Seq == 14)
                {
                    switch (Step)
                    {
                        case 0:
                            UnloadGantryAllPistonZRetract();//确保下料龙门Z气缸&所有吸嘴都缩回
                            if (UnloadGantryMoveAxis(17) == false)//下料龙门移动到满Tray相应位置上方
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 1;
                            break;
                        case 1:
                            if (UnloadGantryAllPistonZStretch() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(1);
                                return;
                            }
                            else
                                Step = 2;
                            Thread.Sleep(200);
                            break;
                        case 2:
                            if (UnloadGantryAllSuckerBreak() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                                Step = 3;
                            break;
                        case 3:
                            if (UnloadGantryAllPistonZRetract() == false)
                            {
                                UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                return;
                            }
                            else
                            {
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft1VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft2VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft3VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft4VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight1VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight2VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight3VacumBreak, false);
                                IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight4VacumBreak, false);
                                Step = 4;
                            }
                            UnloadGantryAllSuckClose();
                            break;
                        case 4:
                            if ((bDelayStop || bDelayStopCount) && ((UnloadGantryCircleCount + 1) >= iDelayStopCount))
                            {
                                Step = 5;
                            }
                            else
                            {
                                WriteLog("【下料龙门放料】下料Tray换Tray盘开始");
                                UnloadTrayFinished = false;
                                DateTime starttime = DateTime.Now;
                                while (true)
                                {
                                    if (UnloadTrayFinished)
                                        break;
                                    else
                                    {
                                        if (!OutTimeCount(starttime, 60))
                                        {
                                            WarningSolution("下料Tray换Tray超时");
                                            UnloadGantryPlaceAllWorkPieceErrorSolution(100);
                                            return;
                                        }
                                        Thread.Sleep(35);
                                    }
                                }

                                Step = 5;
                                WriteLog("【下料龙门放料】下料Tray换Tray盘完成");
                            }
                            break;
                        case 5:
                            Seq = 0;
                            Step = 0;
                            UnloadGantryPlaceAllWorkPieceFinish = true;
                            UnloadGantryPlaceAllWorkPieceCircleCount++;
                            WriteLog("【下料龙门放料】下料龙门放料完成，UnloadGantryPlaceAllWorkPieceCircleCount：" + UnloadGantryPlaceAllWorkPieceCircleCount.ToString());
                            return;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(ThreadAbortException))
                {
                    string exStr = "";
                    exStr += "【下料龙门放料】" + ex.ToString() + "\n";
                    exStr += "UnloadGantryPlaceAllWorkPieceCircleCount=" + UnloadGantryPlaceAllWorkPieceCircleCount.ToString() + "Seq=" + Seq.ToString() + "\n";
                    MessageBox.Show(exStr);
                }
            }
        }

        private bool UnloadGantryPlaceAllWorkPiece(ref int Seq)
        {
            if (Seq <= 5)
            {
                if (UnloadGantryMoveAxis(Seq) == false)//下料龙门移动到空Tray相应位置上方
                    return false;

                if (UnloadGantryAllPistonZStretch() == false)
                    return false;

                if (UnloadGantryAllSuckerBreak() == false)
                    return false;

                if (UnloadGantryAllPistonZRetract() == false)
                    return false;

            }

            if (Seq == 6)
            {
                if (UnloadGantryMoveAxis(6) == false)//下料龙门移动到空Tray相应位置上方
                    return false;

                if (UnloadGantryLeftPistonZStretch() == false)
                    return false;

                if (UnloadGantryLeftSuckerBreak() == false)
                    return false;

                if (UnloadGantryLeftPistonZRetract() == false)
                    return false;

                if (UnloadGantryMoveAxis(7) == false)//下料龙门移动到空Tray相应位置上方
                    return false;

                if (UnloadGantryRightPistonZStretch() == false)
                    return false;

                if (UnloadGantryRightSuckerBreak() == false)
                    return false;

                if (UnloadGantryRightPistonZRetract() == false)
                    return false;
            }

            if (Seq == 7)
            {
                if (UnloadGantryMoveAxis(8) == false)//下料龙门移动到空Tray相应位置上方
                    return false;

                if (UnloadGantryLeftPistonZStretch() == false)
                    return false;

                if (UnloadGantryLeftSuckerBreak() == false)
                    return false;

                if (UnloadGantryLeftPistonZRetract() == false)
                    return false;

                UnloadTrayFinished = false;
                while (!UnloadTrayFinished)
                    Thread.Sleep(35);

                if (UnloadGantryMoveAxis(9) == false)//下料龙门移动到空Tray相应位置上方
                    return false;

                if (UnloadGantryRightPistonZStretch() == false)
                    return false;

                if (UnloadGantryRightSuckerBreak() == false)
                    return false;

                if (UnloadGantryRightPistonZRetract() == false)
                    return false;

            }

            if (Seq == 8)
            {
                if (UnloadGantryMoveAxis(10) == false)//下料龙门移动到空Tray相应位置上方
                    return false;

                if (UnloadGantryLeftPistonZStretch() == false)
                    return false;

                if (UnloadGantryLeftSuckerBreak() == false)
                    return false;

                if (UnloadGantryLeftPistonZRetract() == false)
                    return false;

                if (UnloadGantryMoveAxis(11) == false)//下料龙门移动到空Tray相应位置上方
                    return false;

                if (UnloadGantryRightPistonZStretch() == false)
                    return false;

                if (UnloadGantryRightSuckerBreak() == false)
                    return false;

                if (UnloadGantryRightPistonZRetract() == false)
                    return false;
            }

            if (Seq >= 9 && Seq <= 13)
            {
                if (UnloadGantryMoveAxis(Seq + 3) == false)//下料龙门移动到空Tray相应位置上方
                    return false;

                if (UnloadGantryAllPistonZStretch() == false)
                    return false;

                if (UnloadGantryAllSuckerBreak() == false)
                    return false;

                if (UnloadGantryAllPistonZRetract() == false)
                    return false;
            }

            if (Seq == 14)
            {
                if (UnloadGantryMoveAxis(17) == false)//下料龙门移动到空Tray相应位置上方
                    return false;

                if (UnloadGantryAllPistonZStretch() == false)
                    return false;

                if (UnloadGantryAllSuckerBreak() == false)
                    return false;

                if (UnloadGantryAllPistonZRetract() == false)
                    return false;

                UnloadTrayFinished = false;
                while (!UnloadTrayFinished)
                    Thread.Sleep(35);
            }

            Seq++;
            if (Seq > 14)
                Seq = 0;
            return true;
        }

        #region 下料龙门使用的小程序代码
        //获取当前的NG料&无料的总数目
        private int GetUnloadGantryNGNum(int[] CheckResult)
        {
            int Count = 0;
            for (int i = 0; i < CheckResult.Length; i++)
            {
                if (CheckResult[i] != 1)
                    Count++;
            }
            return Count;
        }
        //是否存在B级NG料
        private bool IsNGBExist(int[] CheckResult)
        {
            for (int i = 0; i < CheckResult.Length; i++)
            {
                if ((CheckResult[i] == -1) || (CheckResult[i] == 0))
                    return true;
            }
            return false;
        }
        //是否存在C级NG料
        private bool IsNGCExist(int[] CheckResult)
        {
            for (int i = 0; i < CheckResult.Length; i++)
            {
                if (CheckResult[i] == -2)
                    return true;
            }
            return false;
        }
        //是否存在D级NG料
        private bool IsNGDExist(int[] CheckResult)
        {
            for (int i = 0; i < CheckResult.Length; i++)
            {
                if (CheckResult[i] == -3)
                    return true;
            }
            return false;
        }
        //是否存在E级NG料
        private bool IsNGEExist(int[] CheckResult)
        {
            for (int i = 0; i < CheckResult.Length; i++)
            {
                if (CheckResult[i] == -4)
                    return true;
            }
            return false;
        }
        //龙门下料Z轴伸出
        public bool UnloadGantryZStretch()
        {
            WriteLog("【下料龙门放料】下料龙门Z气缸伸出开始");
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_UnloadZStretchControl, (int)ECATDONAME.Do_UnloadZRetractControl, (int)ECATDINAME.Di_UnloadZStretchBit, (int)ECATDINAME.Di_UnloadZRetractBit))
            {
                WriteLog("【下料龙门放料】下料龙门Z气缸伸出完成");
                return true;
            }
            else
            {
                WarningSolution("【下料龙门放料】【报警】：【174】下料龙门Z气缸伸出出错:I21.08,I21.09");
                return false;
            }
        }
        //龙门下料Z轴缩回
        public bool UnloadGantryZRetract()
        {
            WriteLog("【下料龙门放料】下料龙门Z气缸缩回开始");
            if (WaitECATPiston2Cmd2FeedbackDone((int)ECATDONAME.Do_UnloadZRetractControl, (int)ECATDONAME.Do_UnloadZStretchControl, (int)ECATDINAME.Di_UnloadZRetractBit, (int)ECATDINAME.Di_UnloadZStretchBit))
            {
                WriteLog("【下料龙门放料】下料龙门Z气缸缩回完成");
                return true;
            }
            else
            {
                WarningSolution("【下料龙门放料】【报警】：【175】下料龙门Z轴缩回出错:I21.08,I21.09");
                return false;
            }
        }

        private string GetUnloadGantrySuckerNo(int No)
        {
            switch (No)
            {
                case 0:
                    return "左1";
                case 1:
                    return "左2";
                case 2:
                    return "左3";
                case 3:
                    return "左4";
                case 4:
                    return "右1";
                case 5:
                    return "右2";
                case 6:
                    return "右3";
                case 7:
                    return "右4";
                default:
                    return "编号错误";
            }
        }

        //龙门下料放置NG料，No代表第几个吸嘴
        private bool UnloadGantryPlaceNG(int No)
        {
            if (WaitECATPiston2Cmd1FeedbackDone(UnloadGantrySuckerBreakControls[No], UnloadGantrySuckerSuckControls[No], UnloadGantrySuckerCheckBits[No], false) == false)
            {
                IOControl.ECATWriteDO(UnloadGantrySuckerBreakControls[No], false);
                WarningSolution("【176】下料龙门" + GetUnloadGantrySuckerNo(No) + "吸嘴真空破错误");
                return false;
            }

            IOControl.ECATWriteDO(UnloadGantrySuckerBreakControls[No], false);

            return true;
        }
        //第i个元件伸出气缸+真空吸+气缸缩回，用于和补料区的交互
        public bool UnloadGantrySuckWorkPiece(int No)
        {
            if (UnloadGantryZStretch() == false)
                return false;

            if (WaitECATPiston2Cmd2FeedbackDone(UnloadGantryCylinderStretchControls[No], UnloadGantryCylinderRetractControls[No], UnloadGantryCylinderStretchCheckBits[No], UnloadGantryCylinderRetractCheckBits[No]) == false)
            {
                WarningSolution("【177】下料龙门" + GetUnloadGantrySuckerNo(No) + "气缸伸出错误");
                return false;
            }
            if (!debugThreadUnloadGantry)
            {
                if (WaitECATPiston2Cmd1FeedbackDone(UnloadGantrySuckerSuckControls[No], UnloadGantrySuckerBreakControls[No], UnloadGantrySuckerCheckBits[No], true) == false)
                {
                    WarningSolution("【178】下料龙门" + GetUnloadGantrySuckerNo(No) + "吸嘴真空吸错误");
                    return false;
                }
            }

            if (WaitECATPiston2Cmd2FeedbackDone(UnloadGantryCylinderRetractControls[No], UnloadGantryCylinderStretchControls[No], UnloadGantryCylinderRetractCheckBits[No], UnloadGantryCylinderStretchCheckBits[No]) == false)
            {
                WarningSolution("【179】下料龙门" + GetUnloadGantrySuckerNo(No) + "气缸缩回错误");
                return false;
            }

            //樊竞明20180908
            //if (UnloadGantryZRetract() == false)
            //    return false;

            return true;
        }
        //第i个元件伸出气缸+真空破+气缸缩回，用于和补料区的交互
        public bool UnloadGantryPlaceWorkPiece(int No)
        {
            if (UnloadGantryZStretch() == false)
                return false;

            if (WaitECATPiston2Cmd2FeedbackDone(UnloadGantryCylinderStretchControls[No], UnloadGantryCylinderRetractControls[No], UnloadGantryCylinderStretchCheckBits[No], UnloadGantryCylinderRetractCheckBits[No]) == false)
            {
                WarningSolution("【下料龙门】【报警】：【180】下料龙门" + GetUnloadGantrySuckerNo(No) + "气缸伸出错误");
                return false;
            }

            if (WaitECATPiston2Cmd1FeedbackDone(UnloadGantrySuckerBreakControls[No], UnloadGantrySuckerSuckControls[No], UnloadGantrySuckerCheckBits[No], false) == false)
            {
                IOControl.ECATWriteDO(UnloadGantrySuckerBreakControls[No], false);
                WarningSolution("【下料龙门】【报警】：【181】下料龙门" + GetUnloadGantrySuckerNo(No) + "吸嘴真空破错误");
                return false;
            }

            if (WaitECATPiston2Cmd2FeedbackDone(UnloadGantryCylinderRetractControls[No], UnloadGantryCylinderStretchControls[No], UnloadGantryCylinderRetractCheckBits[No], UnloadGantryCylinderStretchCheckBits[No]) == false)
            {
                WarningSolution("【下料龙门】【报警】：【182】下料龙门" + GetUnloadGantrySuckerNo(No) + "气缸缩回错误");
                return false;
            }

            IOControl.ECATWriteDO(UnloadGantrySuckerBreakControls[No], false);

            //樊竞明20180908
            //if (UnloadGantryZRetract() == false)
            //    return false;


            return true;
        }
        //下料龙门所有气缸伸出
        public bool UnloadGantryAllPistonStretch()
        {
            bool tempsignal = false;

            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft1StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft1RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft2StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft2RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft3StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft3RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft4StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft4RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight1StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight1RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight2StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight2RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight3StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight3RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight4StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight4RetractControl, false);

            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft1StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft1RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft2StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft2RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft3StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft3RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft4StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft4RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight1StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight1RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight2StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight2RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight3StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight3RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight4StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight4RetractBit + 25]);
                if (tempsignal)
                    break;
                else
                {
                    if (!OutTimeCount(starttime, 5))
                    {
                        WarningSolution("【183】下料龙门所有缓存气缸伸出时出错:I21.10~I21.15,I22.00~I22.09");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        public bool UnloadGantryAllPistonZStretch()
        {
            bool tempsignal = false;
            WriteLog("【下料龙门】下料龙门所有吸嘴&Z气缸伸出开始");
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft1StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft1RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft2StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft2RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft3StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft3RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft4StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft4RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight1StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight1RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight2StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight2RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight3StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight3RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight4StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight4RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadZStretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadZRetractControl, false);

            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft1StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft1RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft2StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft2RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft3StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft3RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft4StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft4RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight1StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight1RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight2StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight2RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight3StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight3RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight4StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight4RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadZStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadZRetractBit + 25]);
                if (tempsignal)
                {
                    WriteLog("【下料龙门】下料龙门所有吸嘴&Z气缸伸出完成");
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 5))
                    {
                        WarningSolution("【下料龙门】【报警】【183】下料龙门所有缓存气缸&Z气缸伸出时出错:I21.10~I21.15,I22.00~I22.09");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            Thread.Sleep(100);//0918
            return true;
        }

        //下料龙门所有左气缸伸出
        private bool UnloadGantryLeftPistonZStretch()
        {
            bool tempsignal = false;

            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft3StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft3RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft4StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft4RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight3StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight3RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight4StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight4RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadZStretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadZRetractControl, false);

            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft3StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft3RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft4StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft4RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight3StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight3RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight4StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight4RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadZStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadZRetractBit + 25]);
                if (tempsignal)
                    break;
                else
                {
                    if (!OutTimeCount(starttime, 5))
                    {
                        WarningSolution("【184】下料龙门左吸嘴气缸&Z气缸伸出时出错:I21.14,I21.15,I22.00,I22.01,I22.06,I22.07,I22.08,I22.09");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        //下料龙门所有右气缸伸出
        private bool UnloadGantryRightPistonZStretch()
        {
            bool tempsignal = false;

            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight1StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight1RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight2StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight2RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft1StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft1RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft2StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft2RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadZStretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadZRetractControl, false);

            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight1StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight1RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight2StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight2RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft1StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft1RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft2StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft2RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadZStretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadZRetractBit + 25]);

                if (tempsignal)
                    break;
                else
                {
                    if (!OutTimeCount(starttime, 5))
                    {
                        WarningSolution("【185】下料龙门右方吸嘴气缸&Z气缸伸出时出错:I21.10,I21.11,I21.12,I21.13,I22.02,I22.03,I22.04,I22.05");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }
        //下料龙门所有气缸缩回
        public bool UnloadGantryAllPistonRetract()
        {
            bool tempsignal = false;

            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft1StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft1RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft2StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft2RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft3StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft3RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft4StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft4RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight1StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight1RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight2StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight2RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight3StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight3RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight4StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight4RetractControl, true);

            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft1StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft1RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft2StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft2RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft3StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft3RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft4StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft4RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight1StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight1RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight2StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight2RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight3StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight3RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight4StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight4RetractBit + 25];

                if (tempsignal)
                    break;
                else
                {
                    if (!OutTimeCount(starttime, 5))
                    {
                        WarningSolution("【186】下料龙门所有吸嘴气缸缩回时出错:I21.10~I21.15,I22.00~I22.09");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        public bool UnloadGantryAllPistonZRetract()
        {
            bool tempsignal = false;
            WriteLog("【下料龙门】下料龙门所有吸嘴&Z气缸缩回开始");
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft1StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft1RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft2StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft2RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft3StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft3RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft4StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft4RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight1StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight1RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight2StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight2RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight3StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight3RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight4StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight4RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadZStretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadZRetractControl, true);

            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft1StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft1RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft2StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft2RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft3StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft3RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft4StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft4RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight1StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight1RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight2StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight2RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight3StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight3RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight4StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight4RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadZStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadZRetractBit + 25];

                if (tempsignal)
                {
                    WriteLog("【下料龙门】下料龙门所有吸嘴&Z气缸缩回完成");
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【下料龙门】【报警】：【186】下料龙门所有吸嘴气缸&Z气缸缩回时出错:I21.10~I21.15,I22.00~I22.09");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }


        //下料龙门所有左气缸缩回
        private bool UnloadGantryLeftPistonZRetract()
        {
            bool tempsignal = false;

            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft3StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft3RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft4StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft4RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight3StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight3RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight4StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight4RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadZStretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadZRetractControl, true);

            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft3StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft3RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft4StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft4RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight3StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight3RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight4StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight4RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadZStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadZRetractBit + 25];

                if (tempsignal)
                    break;
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【187】下料龙门左方吸嘴气缸&Z气缸缩回时出错:I21.14,I21.15,I22.00,I22.01,I22.06,I22.07,I22.08,I22.09");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        //下料龙门所有右气缸缩回
        private bool UnloadGantryRightPistonZRetract()
        {
            bool tempsignal = false;

            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight1StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight1RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight2StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight2RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft1StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft1RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft2StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft2RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadZStretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadZRetractControl, true);

            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight1StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight1RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight2StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight2RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft1StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft1RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft2StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft2RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadZStretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadZRetractBit + 25];

                if (tempsignal)
                    break;
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【188】下料龙门右方吸嘴气缸&Z气缸缩回时出错:I21.10,I21.11,I21.12,I21.13,I22.02,I22.03,I22.04,I22.05");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }


        public bool UnloadGantryLeftPistonStretch()
        {
            bool tempsignal = false;

            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft3StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft3RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft4StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft4RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight3StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight3RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight4StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight4RetractControl, false);

            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft3StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft3RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft4StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft4RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight3StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight3RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight4StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight4RetractBit + 25]);
                if (tempsignal)
                    break;
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【184】下料龙门左方吸嘴气缸伸出时出错:I21.14,I21.15,I22.00,I22.01,I22.06,I22.07,I22.08,I22.09");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        public bool UnloadGantryRightPistonStretch()
        {
            bool tempsignal = false;

            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight1StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight1RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight2StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight2RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft1StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft1RetractControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft2StretchControl, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft2RetractControl, false);

            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight1StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight1RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight2StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight2RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft1StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft1RetractBit + 25]) &&
                             CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft2StretchBit + 25] && (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft2RetractBit + 25]);
                if (tempsignal)
                    break;
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【185】下料龙门右方吸嘴气缸伸出时出错:I21.10,I21.11,I21.12,I21.13,I22.02,I22.03,I22.04,I22.05");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        public bool UnloadGantryLeftPistonRetract()
        {
            bool tempsignal = false;

            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft3StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft3RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft4StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft4RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight3StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight3RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight4StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight4RetractControl, true);

            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft3StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft3RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft4StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft4RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight3StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight3RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight4StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight4RetractBit + 25];

                if (tempsignal)
                    break;
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【187】下料龙门左方吸嘴气缸缩回时出错:I21.14,I21.15,I22.00,I22.01,I22.06,I22.07,I22.08,I22.09");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        public bool UnloadGantryRightPistonRetract()
        {
            bool tempsignal = false;

            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight1StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight1RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight2StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferRight2RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft1StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft1RetractControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft2StretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadBufferLeft2RetractControl, true);

            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight1StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight1RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight2StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferRight2RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft1StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft1RetractBit + 25] &&
                             (!CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft2StretchBit + 25]) && CurInfo.Di[(int)ECATDINAME.Di_UnloadBufferLeft2RetractBit + 25];

                if (tempsignal)
                    break;
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【188】下料龙门右方吸嘴气缸缩回时出错:I21.10,I21.11,I21.12,I21.13,I22.02,I22.03,I22.04,I22.05");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }




        //下料龙门所有吸嘴真空吸
        public bool UnloadGantryAllSuckerSuck()
        {
            bool tempsignal = false;
            WriteLog("【下料龙门】下料龙门吸嘴吸取开始");
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft1VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft1VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft2VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft2VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft3VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft3VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft4VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft4VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight1VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight1VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight2VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight2VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight3VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight3VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight4VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight4VacumBreak, false);

            //DateTime starttime = DateTime.Now;
            //while (true)
            //{
            //    tempsignal = (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft1VacumCheck + 25] == true) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft2VacumCheck + 25] == true) &&
            //                 (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft3VacumCheck + 25] == true) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft4VacumCheck + 25] == true) &&
            //                 (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight1VacumCheck + 25] == true) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight2VacumCheck + 25] == true) &&
            //                 (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight3VacumCheck + 25] == true) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight4VacumCheck + 25] == true);

            //    if (tempsignal)
            //    {
            //        WriteLog("【下料龙门】下料龙门吸嘴吸取物料完成");
            //        break;
            //    }
            //    else
            //    {
            //        if (!OutTimeCount(starttime, 2))
            //        {
            //            UpdateWaringLogNG.Invoke((object)"【189】下料龙门所有吸嘴真空吸出错:I22.10,I22.11,I22.12,I22.13,I22.14,I22.15,I23.00,I23.01");
            //            WriteLog("【下料龙门】【报警】：【189】下料龙门所有吸嘴真空吸出错");
            //            return false;
            //        }
            //        Thread.Sleep(30);
            //    }
            //}
            Thread.Sleep(systemParam.UnloadGantrySuckDelay);
            return true;
        }
        //下料龙门所有左吸嘴真空吸
        public bool UnloadGantryLeftSuckerSuck()
        {
            bool tempsignal = false;

            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft3VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft3VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft4VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft4VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight3VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight3VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight4VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight4VacumBreak, false);

            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft3VacumCheck + 25] == true) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft4VacumCheck + 25] == true) &&
                             (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight3VacumCheck + 25] == true) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight4VacumCheck + 25] == true);
                if (tempsignal)
                    break;
                else
                {
                    if (!OutTimeCount(starttime, 2))
                    {
                        WarningSolution("【190】下料龙门左方吸嘴真空吸出错:I22.12,I22.13,I23.00,I23.01");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }
        //下料龙门所有右吸嘴真空吸
        public bool UnloadGantryRightSuckerSuck()
        {
            bool tempsignal = false;

            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight1VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight1VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight2VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight2VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft1VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft1VacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft2VacumSuck, true); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft2VacumBreak, false);

            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight1VacumCheck + 25] == true) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight2VacumCheck + 25] == true) &&
                             (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft1VacumCheck + 25] == true) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft2VacumCheck + 25] == true);

                if (tempsignal)
                    break;
                else
                {
                    if (!OutTimeCount(starttime, 2))
                    {
                        WarningSolution("【191】下料龙门右方吸嘴真空吸出错：I22.10,I22.11,I22.14,I22.15");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        //新增专用于初始化 检测破真空 by吕
        public bool UnloadGantryAllAndCloseSuckerBreak()
        {
            bool tempsignal = false;
            WriteLog("【下料龙门放料】下料龙门所有吸嘴真空破开始");

            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft1VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft1VacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft2VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft2VacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft3VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft3VacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft4VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft4VacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight1VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight1VacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight2VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight2VacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight3VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight3VacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight4VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight4VacumBreak, true);

            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft1VacumCheck + 25] == false) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft2VacumCheck + 25] == false) &&
                             (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft3VacumCheck + 25] == false) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft4VacumCheck + 25] == false) &&
                             (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight1VacumCheck + 25] == false) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight2VacumCheck + 25] == false) &&
                             (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight3VacumCheck + 25] == false) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight4VacumCheck + 25] == false);

                if (tempsignal)
                {
                    IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft1VacumBreak, false);
                    IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft2VacumBreak, false);
                    IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft3VacumBreak, false);
                    IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft4VacumBreak, false);
                    IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight1VacumBreak, false);
                    IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight2VacumBreak, false);
                    IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight3VacumBreak, false);
                    IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight4VacumBreak, false);
                    WriteLog("【下料龙门放料】下料龙门所有吸嘴真空破完成");
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【下料龙门放料】【报警】：【192】下料龙门所有吸嘴真空破出错:I22.10,I22.11,I22.12,I22.13,I22.14,I22.15,I23.00,I23.01");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        //下料龙门所有吸嘴真空破，不关破真空 by吕
        public bool UnloadGantryAllSuckerBreak()
        {
            Thread.Sleep(systemParam.UnloadGantrySuckerBreakDelay);

            bool tempsignal = false;
            WriteLog("【下料龙门放料】下料龙门所有吸嘴真空破开始");

            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft1VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft1VacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft2VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft2VacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft3VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft3VacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft4VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft4VacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight1VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight1VacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight2VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight2VacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight3VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight3VacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight4VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight4VacumBreak, true);

            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft1VacumCheck + 25] == false) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft2VacumCheck + 25] == false) &&
                             (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft3VacumCheck + 25] == false) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft4VacumCheck + 25] == false) &&
                             (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight1VacumCheck + 25] == false) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight2VacumCheck + 25] == false) &&
                             (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight3VacumCheck + 25] == false) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight4VacumCheck + 25] == false);

                if (tempsignal)
                {
                    WriteLog("【下料龙门放料】下料龙门所有吸嘴真空破完成");
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【下料龙门放料】【报警】：【192】下料龙门所有吸嘴真空破出错:I22.10,I22.11,I22.12,I22.13,I22.14,I22.15,I23.00,I23.01");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }
        //下料龙门所有左吸嘴真空破
        public bool UnloadGantryLeftSuckerBreak()
        {
            Thread.Sleep(systemParam.UnloadGantrySuckerBreakDelay);

            bool tempsignal = false;
            WriteLog("【下料龙门放料】下料龙门左吸嘴破真空开始");
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft3VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft3VacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft4VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft4VacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight3VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight3VacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight4VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight4VacumBreak, true);

            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft3VacumCheck + 25] == false) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft4VacumCheck + 25] == false) &&
                             (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight3VacumCheck + 25] == false) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight4VacumCheck + 25] == false);

                if (tempsignal)
                {
                    //IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight3VacumBreak, false);
                    //IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight4VacumBreak, false);
                    //IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft3VacumBreak, false);
                    //IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft4VacumBreak, false);
                    WriteLog("【下料龙门放料】下料龙门左吸嘴破真空完成");
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【下料龙门放料】【报警】：【193】下料龙门左方吸嘴真空破出错:I22.12,I22.13,I23.00,I23.01");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }
        //下料龙门所有右吸嘴真空破
        public bool UnloadGantryRightSuckerBreak()
        {
            Thread.Sleep(systemParam.UnloadGantrySuckerBreakDelay);

            bool tempsignal = false;
            WriteLog("【下料龙门放料】下料龙门右吸嘴破真空开始");
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight1VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight1VacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight2VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight2VacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft1VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft1VacumBreak, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft2VacumSuck, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft2VacumBreak, true);

            DateTime starttime = DateTime.Now;
            while (true)
            {
                tempsignal = (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight1VacumCheck + 25] == false) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadRight2VacumCheck + 25] == false) &&
                             (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft1VacumCheck + 25] == false) && (CurInfo.Di[(int)ECATDINAME.Di_UnloadLeft2VacumCheck + 25] == false);

                if (tempsignal)
                {
                    //IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight1VacumBreak, false);
                    //IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadRight2VacumBreak, false);
                    //IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft1VacumBreak, false);
                    //IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadLeft2VacumBreak, false);
                    WriteLog("【下料龙门放料】下料龙门右吸嘴破真空完成");
                    break;
                }
                else
                {
                    if (!OutTimeCount(starttime, 10))
                    {
                        WarningSolution("【下料龙门放料】【报警】：【194】下料龙门右方吸嘴真空破出错：I22.10,I22.11,I22.14,I22.15");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }
            return true;
        }

        //下料龙门移动至下料空Tray某处，两个Tray盘为一个周期，共18个位置
        private bool UnloadGantryMoveAxis(int TrayPosNo)//0~17
        {
            DateTime starttime = DateTime.Now;
            while (true)
            {
                if (UnloadGantryPlaceSafeSignal == true)
                    break;
                else
                {
                    if (!OutTimeCount(starttime, 3))
                    {
                        WarningSolution("下料龙门放料安全信号出错（移动气缸未伸出）");
                        return false;
                    }
                    Thread.Sleep(30);
                }
            }

            WriteLog("【下料龙门放料】下料龙门移动至放料位" + TrayPosNo.ToString() + "开始");
            if (UnloadGantryXYMove(new double[] { UnloadGantryMotionPos.posInfo[TrayPosNo].XPos, UnloadGantryMotionPos.posInfo[TrayPosNo].YPos }, true) == false)
            {
                WarningSolution("【下料龙门放料】【报警】：【195】下料龙门轴移动至满Tray" + TrayPosNo.ToString() + "上方发生错误");
                return false;
            }
            WriteLog("【下料龙门放料】下料龙门移动至放料位" + TrayPosNo.ToString() + "完成");
            return true;
        }
        //下料龙门移动至NG盒正上方
        private bool UnloadGantryMoveAxisNG(int level)
        {
            switch (level)
            {
                case -1:
                    if (UnloadGantryXYMove(new double[] { systemParam.UnloadTrayNGAPosX, systemParam.UnloadTrayNGAPosY }, true) == false)
                    {
                        WarningSolution("【下料龙门】【报警】：【196】移动至NG B料盒上方出错");
                        return false;
                    }
                    else
                    {
                        WriteLog("【下料龙门】移动至NG B料盒上方完成");
                        return true;
                    }
                case -2:
                    if (UnloadGantryXYMove(new double[] { systemParam.UnloadTrayNGBPosX, systemParam.UnloadTrayNGBPosY }, true) == false)
                    {
                        WarningSolution("【下料龙门】【报警】：【197】移动至NG C料盒上方出错");
                        return false;
                    }
                    else
                    {
                        WriteLog("【下料龙门】移动至NG C料盒上方完成");
                        return true;
                    }
                case -3:
                    if (UnloadGantryXYMove(new double[] { systemParam.UnloadTrayNGCPosX, systemParam.UnloadTrayNGCPosY }, true) == false)
                    {
                        WarningSolution("【下料龙门】【报警】：【198】移动至NG D料盒上方出错");
                        return false;
                    }
                    else
                    {
                        WriteLog("【下料龙门】移动至NG D料盒上方完成");
                        return true;
                    }
                case -4:
                    if (UnloadGantryXYMove(new double[] { systemParam.UnloadTrayNGCPosX, systemParam.UnloadTrayNGCPosY }, true) == false)
                    {
                        WarningSolution("【下料龙门】【报警】：【198】移动至NG D料盒上方出错");
                        return false;
                    }
                    else
                    {
                        WriteLog("【下料龙门】移动至NG D料盒上方完成");
                        return true;
                    }
                default:
                    return false;
            }
        }
        //下料龙门放置NG料
        private bool UnloadGantryPlaceNGWorkPiece(int[] CheckResult, int NGLevel)
        {
            if (UnloadGantryMoveAxisNG(NGLevel) == false)
                return false;

            for (int i = 0; i < CheckResult.Length; i++)
            {
                if ((CheckResult[i] == NGLevel) || (CheckResult[i] == 0))
                {
                    if (UnloadGantryPlaceNG(i) == false)
                        return false;
                    else
                    {
                        switch (NGLevel)
                        {
                            case -1:
                                NGBNum++;
                                if (NGBNum >= systemParam.NGDrawerAlmNum)
                                {
                                    StartBeep();
                                    if (MessageBox.Show("B料盒工件数目警告，请尽快清空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Question) == DialogResult.OK)
                                    {
                                        NGBNum = 0;
                                    }
                                }
                                break;
                            case -2:
                                NGCNum++;
                                if (NGCNum >= systemParam.NGDrawerAlmNum)
                                {
                                    StartBeep();
                                    if (MessageBox.Show("C料盒工件数目警告，请尽快清空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Question) == DialogResult.OK)
                                    {
                                        NGCNum = 0;
                                    }
                                }
                                break;
                            case -3:
                                NGDNum++;
                                if (NGDNum >= systemParam.NGDrawerAlmNum)
                                {
                                    StartBeep();
                                    if (MessageBox.Show("D料盒工件数目警告，请尽快清空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Question) == DialogResult.OK)
                                    {
                                        NGDNum = 0;
                                    }
                                }
                                break;
                            case -4:
                                NGDNum++;
                                if (NGDNum >= systemParam.NGDrawerAlmNum)
                                {
                                    StartBeep();
                                    if (MessageBox.Show("D料盒工件数目警告，请尽快清空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Question) == DialogResult.OK)
                                    {
                                        NGDNum = 0;
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            WriteLog("【下料龙门】抛" + NGLevel.ToString() + "级别物料完成");
            return true;
        }
        #endregion

        #endregion

        #region 上料Tray整体替换
        public int LoadTrayAllSwitchCircleCount = 0;
        public bool LoadTrayAllSwitchEnable = false;
        public bool LoadTrayAllSwitchFinished = true;
        public int LoadTrayAllSwitchStep = 0;

        public void LoadTrayAllSwitchThread()
        {
            int errcode = 0;
            LoadTrayAllSwitchCircleCount = 0;
            LoadTrayAllSwitchStep = 0;

            //开启数据收集线程
            while (AutoRunActive)
            {
                if (LoadTrayAllSwitchEnable)
                {
                    if (!LoadTrayAllSwitchFinished)
                    {
                        LoadTrayAllSwitchStatusSwitch(ref LoadTrayAllSwitchStep, ref errcode);
                    }
                }
                Thread.Sleep(40);
            }
        }

        private void LoadTrayAllSwitchStatusSwitch(ref int Step, ref int errcode)
        {
            double poserror = 100;
            bool LoadNullInPosition = false; bool LoadFullInPosition = false;
            try
            {
                switch (Step)
                {
                    case 0://上料Tray分盘气缸缩回
                        WriteLog("【上料Tray All Switch】 case 0：分盘气缸缩回开始");
                        if (LoadTraySeparateRetract())
                        {
                            WriteLog("【上料Tray All Switch】 case 0：分盘气缸缩回完成");
                            Step = 1;
                        }
                        else
                        {
                            WarningSolution("【上料Tray All Switch】【报警】：分盘气缸缩回出错");
                            LoadTrayAllSwitchErrorSolution(100);
                            return;
                        }
                        break;
                    case 1://上料满Tray Z轴&上料空Tray Z轴同时移动至各自下极限
                        WriteLog("【上料Tray All Switch】 case 1：上料Tray整体换盘时Z轴下降至下极限开始");
                        if (adlink.LineMove(new Axis[] { logicConfig.ECATAxis[4], logicConfig.ECATAxis[5] }, new double[] { systemParam.TraySwitchLoadNullDownLimit, systemParam.TraySwitchLoadFullDownLimit }, true, ref errcode, ref poserror))
                        {
                            WriteLog("【上料Tray All Switch】：上料Tray整体换盘时Z轴下降至下极限完成");
                            Step = 2;
                        }
                        else
                        {
                            WarningSolution("【上料Tray All Switch】【报警】：上料Tray整体换盘时Z轴下降至下极限过程出错");
                            LoadTrayAllSwitchErrorSolution(100);
                        }
                        break;
                    case 2://上料空Tray Z轴向上移动至上极限（不等待）&上料满Tray Z轴向上Jog
                        adlink.P2PMove(logicConfig.ECATAxis[4], systemParam.TraySwitchLoadNullUpLimit, true);
                        StopJog(logicConfig.ECATAxis[5]);
                        StartJog(logicConfig.ECATAxis[5], 0);
                        WriteLog("【上料Tray All Switch】 case 2：上料Tray整体换盘时Z轴上升开始");
                        Step = 3;
                        break;
                    case 3://等待上料空Tray Z轴checkmotiondone & 上料满Tray Z轴到位信号
                        WriteLog("【上料Tray All Switch】 case 3：上料Tray整体换盘等待Z轴上升完成开始");
                        DateTime starttime = DateTime.Now;
                        while (true)
                        {
                            LoadNullInPosition = adlink.CheckMoveDone(logicConfig.ECATAxis[4], ref errcode);
                            LoadFullInPosition = !CurInfo.Di[(int)ECATDINAME.Di_LoadFullTrayInPosition + 25];
                            if (LoadFullInPosition == true)
                            {
                                WriteLog("【上料Tray All Switch】上料满Tray Z轴等待到位信号完成");
                                StopJog(logicConfig.ECATAxis[5]);
                            }

                            if (LoadNullInPosition && LoadFullInPosition)
                                break;
                            else
                            {
                                if (!OutTimeCount(starttime, 30))
                                {
                                    WarningSolution("【上料Tray All Switch】【报警】：上料Tray整体换盘时Z轴上升过程中出错");
                                    LoadTrayAllSwitchErrorSolution(100);
                                    return;
                                }
                                Thread.Sleep(20);
                            }
                        }
                        WriteLog("【上料Tray All Switch】 case 3：上料Tray整体换盘等待Z轴上升完成结束");
                        Step = 4;
                        break;
                    case 4://伸出分盘气缸
                        WriteLog("【上料Tray All Switch】 case 4：分盘气缸伸出开始");
                        if (LoadTraySeparateStretch())
                        {
                            WriteLog("【上料Tray All Switch】 case 4：分盘气缸伸出完成");
                            Step = 5;
                        }
                        else
                        {
                            WarningSolution("【上料Tray All Switch】【报警】：分盘气缸伸出出错");
                            LoadTrayAllSwitchErrorSolution(100);
                            return;
                        }
                        break;
                    case 5://收尾工序
                        //******************************樊竞明20181001*********************//
                        isInputProductBatch = false;
                        //*****************************************************************//
                        Step = 0;
                        LoadTrayAllSwitchFinished = true;
                        LoadTrayAllSwitchCircleCount++;
                        isLoadTraySupplied = false;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(ThreadAbortException))
                {
                    string exStr = "";
                    exStr += "【上料Tray All Switch】" + ex.ToString() + "\n";
                    exStr += "LoadTrayAllSwitchCircleCount=" + LoadTrayAllSwitchCircleCount.ToString() + "\n";
                    MessageBox.Show(exStr);
                }
            }
        }
        #endregion

        #region 下料Tray整体替换
        public int UnloadTrayAllSwitchCircleCount = 0;
        public bool UnloadTrayAllSwitchEnable = false;
        public bool UnloadTrayAllSwitchFinished = true;
        public int UnloadTrayAllSwitchStep = 0;

        public void UnloadTrayAllSwitchThread()
        {
            int errcode = 0;
            UnloadTrayAllSwitchCircleCount = 0;
            UnloadTrayAllSwitchStep = 0;

            //开启数据收集线程
            while (AutoRunActive)
            {
                if (UnloadTrayAllSwitchEnable)
                {
                    if (!UnloadTrayAllSwitchFinished)
                    {
                        UnloadTrayAllSwitchStatusSwitch(ref UnloadTrayAllSwitchStep, ref errcode);
                    }
                }
                Thread.Sleep(40);
            }
        }

        private void UnloadTrayAllSwitchStatusSwitch(ref int Step, ref int errcode)
        {
            double poserror = 100;
            bool UnloadNullInPosition = false; bool UnloadFullInPosition = false;
            try
            {
                switch (Step)
                {
                    case 0://下料Tray分盘气缸缩回
                        WriteLog("【下料Tray All Switch】 case 0：分盘气缸缩回开始");
                        if (UnloadTraySeparateRetract())
                        {
                            WriteLog("【下料Tray All Switch】 case 0：分盘气缸缩回完成");
                            Step = 1;
                        }
                        else
                        {
                            WarningSolution("【下料Tray All Switch】【报警】：分盘气缸缩回出错");
                            UnloadTrayAllSwitchErrorSolution(100);
                            return;
                        }
                        break;
                    case 1://下料满Tray Z轴&下料空Tray Z轴同时移动至各自下极限
                        WriteLog("【下料Tray All Switch】 case 1：下料Tray整体换盘时Z轴下降至下极限开始");
                        if (adlink.LineMove(new Axis[] { logicConfig.ECATAxis[6], logicConfig.ECATAxis[7] }, new double[] { systemParam.TraySwitchUnloadNullDownLimit, systemParam.TraySwitchUnloadFullDownLimit }, true, ref errcode, ref poserror))
                        {
                            WriteLog("【下料Tray All Switch】：下料Tray整体换盘时Z轴下降至下极限完成");
                            Step = 2;
                        }
                        else
                        {
                            WarningSolution("【下料Tray All Switch】【报警】：下料Tray整体换盘时Z轴下降至下极限过程出错");
                            UnloadTrayAllSwitchErrorSolution(100);
                        }
                        break;
                    case 2://下料满Tray Z轴向上移动至上极限（不等待）&下料空Tray Z轴向上Jog
                        adlink.P2PMove(logicConfig.ECATAxis[7], systemParam.TraySwitchUnloadFullUpLimit, true);
                        StopJog(logicConfig.ECATAxis[6]);
                        StartJog(logicConfig.ECATAxis[6], 0);
                        WriteLog("【下料Tray All Switch】 case 2：下料Tray整体换盘时Z轴上升开始");
                        Step = 3;
                        break;
                    case 3://等待下料满Tray Z轴checkmotiondone & 下料空Tray Z轴到位信号
                        WriteLog("【下料Tray All Switch】 case 3：下料Tray整体换盘等待Z轴上升完成开始");
                        DateTime starttime = DateTime.Now;
                        while (true)
                        {
                            UnloadNullInPosition = adlink.CheckMoveDone(logicConfig.ECATAxis[7], ref errcode);
                            UnloadFullInPosition = !CurInfo.Di[(int)ECATDINAME.Di_UnloadNullTrayInPosition + 25];
                            if (UnloadFullInPosition == true)
                            {
                                WriteLog("【下料Tray All Switch】下料空Tray Z轴等待到位信号完成");
                                StopJog(logicConfig.ECATAxis[6]);
                            }

                            if (UnloadNullInPosition && UnloadFullInPosition)
                                break;
                            else
                            {
                                if (!OutTimeCount(starttime, 30))
                                {
                                    WarningSolution("【下料Tray All Switch】【报警】：下料Tray整体换盘时Z轴上升过程中出错");
                                    UnloadTrayAllSwitchErrorSolution(100);
                                    return;
                                }
                                Thread.Sleep(20);
                            }
                        }
                        WriteLog("【下料Tray All Switch】 case 3：下料Tray整体换盘等待Z轴上升完成结束");
                        Step = 4;
                        break;
                    case 4://伸出分盘气缸
                        WriteLog("【下料Tray All Switch】 case 4：分盘气缸伸出开始");
                        if (UnloadTraySeparateStretch())
                        {
                            WriteLog("【下料Tray All Switch】 case 4：分盘气缸伸出完成");
                            Step = 5;
                        }
                        else
                        {
                            WarningSolution("【下料Tray All Switch】【报警】：分盘气缸伸出出错");
                            UnloadTrayAllSwitchErrorSolution(100);
                            return;
                        }
                        break;
                    case 5://开始换盘
                        WriteLog("【下料Tray All Switch】 case 5：躲避换盘开始");
                        if (adlink.LineMove(new Axis[] { logicConfig.ECATAxis[2], logicConfig.ECATAxis[3] }, new double[] { systemParam.UnloadTrayAvoidPosX, systemParam.UnloadTrayAvoidPosY }, true, ref errcode, ref poserror) == false)
                        {
                            WarningSolution("【下料Tray All Switch】【报警】：下料Tray整体替换过程中躲避换盘出错");
                            UnloadTrayAllSwitchErrorSolution(100);
                            return;
                        }
                        else
                        {
                            WriteLog("【下料Tray All Switch】case 5：躲避换盘完成");
                            Step = 6;
                        }
                        break;
                    case 6:
                        WriteLog("【下料Tray All Switch】 case 6：下料Tray Z气缸伸出开始");
                        if (UnloadTrayZStretch() == false)
                        {
                            WarningSolution("【下料Tray All Switch】【报警】：下料Tray整体替换过程中Z气缸伸出超时");
                            UnloadTrayAllSwitchErrorSolution(100);
                            return;
                        }
                        else
                        {
                            WriteLog("【下料Tray All Switch】case 6：下料Tray Z气缸伸出完成");
                            Step = 7;
                        }
                        break;
                    case 7:
                        WriteLog("【下料Tray All Switch】 case 7：下料Tray真空吸料盘开始");
                        if (UnloadTrayVacumSuck() == false)
                        {
                            WarningSolution("【下料Tray All Switch】【报警】：下料Tray整体替换过程中真空吸料盘超时");
                            UnloadTrayAllSwitchErrorSolution(100);
                            return;
                        }
                        else
                        {
                            WriteLog("【下料Tray All Switch】case 7：下料Tray真空吸料盘完成");
                            Step = 8;
                        }
                        break;
                    case 8:
                        WriteLog("【下料Tray All Switch】 case 8：下料Tray分盘气缸缩回开始");
                        if (UnloadTraySeparateRetract() == false)
                        {
                            WarningSolution("【下料Tray All Switch】【报警】：下料Tray整体替换过程中分盘气缸缩回超时");
                            UnloadTrayAllSwitchErrorSolution(100);
                            return;
                        }
                        else
                        {
                            WriteLog("【下料Tray All Switch】case 8：下料Tray分盘气缸缩回完成");
                            Step = 9;
                        }
                        break;
                    case 9:
                        WriteLog("【下料Tray All Switch】 case 9：下料Tray Z气缸缩回开始");
                        if (UnloadTrayZRetract() == false)
                        {
                            WarningSolution("【下料Tray All Switch】【报警】：下料Tray整体替换过程中Z气缸缩回超时");
                            UnloadTrayAllSwitchErrorSolution(100);
                            return;
                        }
                        else
                        {
                            WriteLog("【下料Tray All Switch】case 9：下料Tray Z气缸缩回完成");
                            Step = 10;
                        }
                        break;
                    case 10:
                        WriteLog("【下料Tray All Switch】 case 10：下料Tray移动气缸缩回开始");
                        if (UnloadTrayMovePistonRetract() == false)
                        {
                            WarningSolution("【下料Tray All Switch】【报警】：下料Tray整体替换过程中移动气缸缩回超时");
                            UnloadTrayAllSwitchErrorSolution(100);
                            return;
                        }
                        else
                        {
                            WriteLog("【下料Tray All Switch】case 10：下料Tray移动气缸缩回完成");
                            Step = 11;
                        }
                        break;
                    case 11:
                        WriteLog("【下料Tray All Switch】 case 11：下料Tray Z气缸伸出开始");
                        if (UnloadTrayZStretch() == false)
                        {
                            WarningSolution("【下料Tray All Switch】【报警】：下料Tray整体替换过程中Z气缸伸出超时");
                            UnloadTrayAllSwitchErrorSolution(100);
                            return;
                        }
                        else
                        {
                            WriteLog("【下料Tray All Switch】case 11：下料Tray Z气缸伸出完成");
                            Step = 12;
                        }
                        break;
                    case 12:
                        WriteLog("【下料Tray All Switch】 case 12：下料Tray真空破开始");
                        if (UnloadTrayVacumBreak() == false)
                        {
                            WarningSolution("【下料Tray All Switch】【报警】：下料Tray整体替换过程中真空破超时");
                            UnloadTrayAllSwitchErrorSolution(100);
                            return;
                        }
                        else
                        {
                            WriteLog("【下料Tray All Switch】case 12：下料Tray真空破完成");
                            Step = 13;
                        }
                        break;
                    case 13:
                        WriteLog("【下料Tray All Switch】 case 13：下料Tray Z气缸缩回开始");
                        if (UnloadTrayZRetract() == false)
                        {
                            WarningSolution("【下料Tray All Switch】【报警】：下料Tray整体替换过程中Z气缸缩回超时");
                            UnloadTrayAllSwitchErrorSolution(100);
                            return;
                        }
                        else
                        {
                            WriteLog("【下料Tray All Switch】case 13：下料Tray Z气缸缩回完成");
                            Step = 14;
                        }
                        break;
                    case 14:
                        WriteLog("【下料Tray All Switch】 case 16：下料空Tray Z轴移动开始");
                        if (UnloadNullTraySwitchMove() == false)
                        {
                            WarningSolution("【下料Tray All Switch】【报警】：下料Tray整体替换过程中下料空Tray Z轴移动出错");
                            UnloadTrayAllSwitchErrorSolution(100);
                            return;
                        }
                        else
                        {
                            WriteLog("【下料Tray All Switch】case 16：下料空Tray Z轴移动完成");
                            Step = 15;
                        }
                        break;
                    case 15:
                        WriteLog("【下料Tray All Switch】 case 16：下料满Tray Z轴移动开始");
                        if (UnloadFullTraySwitchMove() == false)
                        {
                            WarningSolution("【下料Tray All Switch】【报警】：下料Tray整体替换过程中下料满Tray Z轴移动出错");
                            UnloadTrayAllSwitchErrorSolution(100);
                            return;
                        }
                        else
                        {
                            WriteLog("【下料Tray All Switch】case 16：下料满Tray Z轴移动完成");
                            Step = 16;
                        }
                        break;
                    case 16:
                        WriteLog("【下料Tray All Switch】 case 16：下料Tray 分盘气缸伸出开始");
                        if (UnloadTraySeparateStretch() == false)
                        {
                            WarningSolution("【下料Tray All Switch】【报警】：下料Tray整体替换过程中分盘气缸伸出超时");
                            UnloadTrayAllSwitchErrorSolution(100);
                            return;
                        }
                        else
                        {
                            WriteLog("【下料Tray All Switch】case 16：下料Tray 分盘气缸伸出完成");
                            Step = 17;
                        }
                        break;
                    case 17:
                        WriteLog("【下料Tray All Switch】 case 10：下料Tray移动气缸伸出开始");
                        if (UnloadTrayMovePistonStretch() == false)
                        {
                            WarningSolution("【下料Tray All Switch】【报警】：下料Tray整体替换过程中移动气缸伸出超时");
                            UnloadTrayAllSwitchErrorSolution(100);
                            return;
                        }
                        else
                        {
                            WriteLog("【下料Tray All Switch】case 10：下料Tray移动气缸伸出完成");
                            Step = 18;
                        }
                        break;
                    case 18://收尾工序
                        Step = 0;
                        UnloadTrayAllSwitchFinished = true;
                        UnloadTrayAllSwitchCircleCount++;
                        isUnloadTraySupplied = false;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(ThreadAbortException))
                {
                    string exStr = "";
                    exStr += "【下料Tray All Switch】" + ex.ToString() + "\n";
                    exStr += "UnloadTrayAllSwitchCircleCount=" + UnloadTrayAllSwitchCircleCount.ToString() + "\n";
                    MessageBox.Show(exStr);
                }
            }
        }
        #endregion

        #region 锁扣动作
        public bool NGDrawerLock()
        {
            if (CurInfo.Di[(int)ECATDINAME.Di_NGDrawerInPosition + 25])
            {
                WarningSolution("【199】NG抽屉未到位，无法锁扣:I25.10");
                return false;
            }

            IOControl.ECATWriteDO((int)ECATDONAME.Do_NGDrawerLockControl, true);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_NGDrawerUnlockControl, false);

            return true;
        }

        public bool NGDrawerUnlock()
        {
            if (CurInfo.Di[(int)ECATDINAME.Di_NGDrawerInPosition + 25])
            {
                WarningSolution("【200】NG抽屉未到位，无法解锁:I25.10");
                return false;
            }

            IOControl.ECATWriteDO((int)ECATDONAME.Do_NGDrawerLockControl, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_NGDrawerUnlockControl, true);

            return true;
        }

        public bool LoadNullTrayDrawerLock()
        {
            if (CurInfo.Di[(int)ECATDINAME.Di_LoadNullDrawerInPosition + 25])
            {
                WarningSolution("【201】上料空Tray抽屉未到位，无法锁扣:I24.08");
                return false;
            }

            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadNullDrawerLock, true);
            return true;
        }

        public bool LoadNullTrayDrawerUnlock()
        {
            if (CurInfo.Di[(int)ECATDINAME.Di_LoadNullDrawerInPosition + 25])
            {
                WarningSolution("【202】上料空Tray抽屉未到位，无法解锁:I24.08");
                return false;
            }

            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadNullDrawerLock, false);
            return true;
        }

        public bool LoadFullTrayDrawerLock()
        {
            if (CurInfo.Di[(int)ECATDINAME.Di_LoadFullDrawerInPosition + 25])
            {
                WarningSolution("【203】上料满Tray抽屉未到位，无法锁扣:I24.09");
                return false;
            }

            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadFullDrawerLock, true);
            return true;
        }

        public bool LoadFullTrayDrawerUnlock()
        {
            if (CurInfo.Di[(int)ECATDINAME.Di_LoadFullDrawerInPosition + 25])
            {
                WarningSolution("【204】上料满Tray抽屉未到位，无法解锁:I24.09");
                return false;
            }

            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadFullDrawerLock, false);
            return true;
        }

        public bool UnloadNullTrayDrawerLock()
        {
            if (CurInfo.Di[(int)ECATDINAME.Di_UnloadNullDrawerTrayInPosition + 25])
            {
                WarningSolution("【205】下料空Tray抽屉未到位，无法锁扣:I25.04");
                return false;
            }

            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadNullDrawerLockControl, true);
            return true;
        }

        public bool UnloadNullTrayDrawerUnlock()
        {
            if (CurInfo.Di[(int)ECATDINAME.Di_UnloadNullDrawerTrayInPosition + 25])
            {
                WarningSolution("【206】下料空Tray抽屉未到位，无法解锁:I25.04");
                return false;
            }

            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadNullDrawerLockControl, false);
            return true;
        }

        public bool UnloadFullTrayDrawerLock()
        {
            if (CurInfo.Di[(int)ECATDINAME.Di_UnloadFullDrawerTrayInPosition + 25])
            {
                WarningSolution("【207】下料满Tray抽屉未到位，无法锁扣:I25.05");
                return false;
            }

            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadFullDrawerLockControl, true);
            return true;
        }

        public bool UnloadFullTrayDrawerUnlock()
        {
            if (CurInfo.Di[(int)ECATDINAME.Di_UnloadFullDrawerTrayInPosition + 25])
            {
                WarningSolution("【208】下料满Tray抽屉未到位，无法解锁:I25.05");
                return false;
            }

            IOControl.ECATWriteDO((int)ECATDONAME.Do_UnloadFullDrawerLockControl, false);
            return true;
        }
        #endregion

        #region 清理异常上料空满Tray盘 by吕
        //public bool ClearLoadTrayFinished = false;
        public void ClearLoadTray()
        {
            int errcode = 0; double poserror = 0.0;

            if (LoadGantryAllPistonZRetract() == false)//上料龙门所有吸嘴&Z轴缩回
            {
                return;
            }

            if (adlink.LineMove(new Axis[] { logicConfig.ECATAxis[0], logicConfig.ECATAxis[1] }, new double[] { systemParam.LoadTrayAvoidPosX, systemParam.LoadTrayAvoidPosY }, true, ref errcode, ref poserror) == false)
            {
                return;
            }
            if (LoadTrayMovePistonStretch() == false)//上料Tray轴伸出
            {
                return;
            }
            if (LoadTraySeparateRetract() == false)//上料分盘气缸缩回
            {
                return;
            }
            if (adlink.LineMove(new Axis[] { logicConfig.ECATAxis[5], logicConfig.ECATAxis[4] }, new double[] { systemParam.TraySwitchLoadFullUpLimit, systemParam.TraySwitchLoadNullDownLimit }, true, ref errcode, ref poserror) == false)
            {
                return;
            }
            if (adlink.LineMove(new Axis[] { logicConfig.ECATAxis[4] }, new double[] { systemParam.TraySwitchLoadNullUpLimit }, true, ref errcode, ref poserror) == false)
            {
                return;
            }
            UpdateDebugButton(4);
            //ClearLoadTrayFinished = true;
            MessageBox.Show("上料空满Tray清盘完成");
        }
        #endregion

        #region 清理异常下料空满Tray盘 by吕
        public void ClearUnloadTray()
        {
            int errcode = 0; double poserror = 0.0;

            if (UnloadGantryAllPistonZRetract() == false)//下料龙门所有吸嘴&Z轴缩回
            {
                return;
            }
            if (adlink.LineMove(new Axis[] { logicConfig.ECATAxis[2], logicConfig.ECATAxis[3] }, new double[] { systemParam.UnloadTrayAvoidPosX, systemParam.UnloadTrayAvoidPosY }, true, ref errcode, ref poserror) == false)
            {
                return;
            }
            if (UnloadTrayMovePistonRetract() == false)//下料Tray轴缩回
            {
                return;
            }
            if (UnloadTraySeparateRetract() == false)//下料分盘气缸缩回
            {
                return;
            }
            if (adlink.LineMove(new Axis[] { logicConfig.ECATAxis[6], logicConfig.ECATAxis[7] }, new double[] { systemParam.TraySwitchUnloadNullUpLimit, systemParam.TraySwitchUnloadFullDownLimit }, true, ref errcode, ref poserror) == false)
            {
                return;
            }
            if (adlink.LineMove(new Axis[] { logicConfig.ECATAxis[7] }, new double[] { systemParam.TraySwitchUnloadFullUpLimit }, true, ref errcode, ref poserror) == false)
            {
                return;
            }
            UpdateDebugButton(5);
            //ClearLoadTrayFinished = true;
            MessageBox.Show("下料空满Tray清盘完成");
        }
        #endregion

        #region 检测翻转信号
        public bool LoadNullTrayHomeFinished = false;
        public bool LoadFullTrayHomeFinished = false;
        public bool UnloadNullTrayHomeFinished = false;
        public bool UnloadFullTrayHomeFinished = false;

        public void LoadNullTrayHome()
        {
            int curpos = 0; int errcode = 0;
            WriteLog("【上料Tray】上料空Tray Z轴移动至下极限开始");
            if (adlink.P2PMove(logicConfig.ECATAxis[4], systemParam.TraySwitchLoadNullDownLimit, true, ref errcode) == false)
            {
                WarningSolution("【上料Tray】【报警】：【209】上料空Tray移动至下限位出错");
                return;
            }
            WriteLog("【上料Tray】上料空Tray Z轴移动至下极限完成");
            //StartJog(logicConfig.ECATAxis[4], 0);
            //DateTime starttime = DateTime.Now;
            //注意恢复
            //while (true)
            //{
            //    adlink.GetCurPostion(logicConfig.ECATAxis[4].AxisId, ref curpos);
            //    if (curpos / logicConfig.ECATAxis[4].Rate >= systemParam.TraySwitchLoadNullRollSensePos)
            //    {
            //        if (CheckLoadNullRollSignals() == true)
            //        {
            //            UpdateWaringLog.Invoke((object)"上料空Tray检测翻转信号成功");
            //            break;
            //        }
            //        else
            //        {
            //            UpdateWaringLogNG.Invoke((object)"上料空Tray检测翻转信号失败");
            //            StopJog(logicConfig.ECATAxis[4]);
            //            return;
            //        }
            //    }
            //    else
            //    {
            //        if (!OutTimeCount(starttime, 30))
            //        {
            //            UpdateWaringLogNG.Invoke((object)"上料空Tray补料向上移动1失败");
            //            StopJog(logicConfig.ECATAxis[4]);
            //            return;
            //        }
            //    }
            //    Thread.Sleep(50);
            //}

            //while (true)
            //{
            //    adlink.GetCurPostion(logicConfig.ECATAxis[4].AxisId, ref curpos);
            //    if (curpos / logicConfig.ECATAxis[4].Rate >= systemParam.TraySwitchLoadNullUpLimit)
            //    {
            //        StopJog(logicConfig.ECATAxis[4]);
            //        UpdateWaringLog.Invoke((object)"上料空Tray补料移动至上限位成功");
            //        break;
            //    }
            //    else
            //    {
            //        if (!OutTimeCount(starttime, 30))
            //        {
            //            UpdateWaringLogNG.Invoke((object)"【210】上料空Tray补料移动至上限位失败");
            //            StopJog(logicConfig.ECATAxis[4]);
            //            return;
            //        }
            //    }
            //    Thread.Sleep(10);
            //}

            WriteLog("【上料Tray】上料空Tray Z轴上升至上限位开始");
            if (adlink.P2PMove(logicConfig.ECATAxis[4], systemParam.TraySwitchLoadNullUpLimit, true, ref errcode) == false)
            {
                WarningSolution("【上料Tray】【报警】：【210】上料空Tray补料移动至上限位失败");
                return;
            }
            WriteLog("【上料Tray】上料空Tray Z轴上升至上限位完成");
            LoadNullTrayHomeFinished = true;
            return;
        }

        public void LoadFullTrayHome()
        {
            int curpos = 0; int errcode = 0;

            //注意恢复
            //StartJog(logicConfig.ECATAxis[5], 1);
            //DateTime starttime = DateTime.Now;
            //
            //while (true)
            //{
            //    adlink.GetCurPostion(logicConfig.ECATAxis[5].AxisId, ref curpos);
            //    if (curpos / logicConfig.ECATAxis[5].Rate <= systemParam.TraySwitchLoadFullRollSensePos)
            //    {
            //        if (CheckLoadFullRollSignals() == true)
            //        {
            //            UpdateWaringLog.Invoke((object)"上料满Tray检测翻转信号成功");
            //            break;
            //        } 
            //        else
            //        {
            //            UpdateWaringLogNG.Invoke((object)"上料满Tray检测翻转信号失败");
            //            StopJog(logicConfig.ECATAxis[5]);
            //            return;
            //        }
            //    }
            //    else
            //    {
            //        if (!OutTimeCount(starttime, 10))
            //        {
            //            UpdateWaringLogNG.Invoke((object)"上料满Tray补料移动1失败");
            //            StopJog(logicConfig.ECATAxis[5]);
            //            return;
            //        }
            //    }
            //    Thread.Sleep(50);
            //}

            //while (true)
            //{
            //    adlink.GetCurPostion(logicConfig.ECATAxis[5].AxisId, ref curpos);
            //    if (curpos / logicConfig.ECATAxis[5].Rate <= systemParam.TraySwitchLoadFullDownLimit)
            //    {
            //        StopJog(logicConfig.ECATAxis[5]);
            //        UpdateWaringLog.Invoke((object)"上料满Tray补料移动至下限位成功");
            //        break;
            //    }
            //    else
            //    {
            //        if (!OutTimeCount(starttime, 30))
            //        {
            //            UpdateWaringLogNG.Invoke((object)"【211】上料满Tray补料移动至下限位失败");
            //            StopJog(logicConfig.ECATAxis[5]);
            //            return;
            //        }
            //    }
            //    Thread.Sleep(10);
            //}

            //0730 Test Code
            WriteLog("【上料Tray】上料满Tray Z轴下降至下极限开始");
            adlink.P2PMove(logicConfig.ECATAxis[5], systemParam.TraySwitchLoadFullDownLimit, true, ref errcode);
            WriteLog("【上料Tray】上料满Tray Z轴下降至下极限完成，开始向上运动");
            StopJog(logicConfig.ECATAxis[5]);
            StartJog(logicConfig.ECATAxis[5], 0);

            WriteLog("【上料Tray】上料满Tray Z轴等待到位信号开始");
            if (WaitLoadTrayFullInPosition())
            {
                WriteLog("【上料Tray】上料满Tray Z轴等待到位信号完成");
                StopJog(logicConfig.ECATAxis[5]);
                adlink.P2PMove(logicConfig.ECATAxis[5], systemParam.LoadFullTrayFinishDistance, false, ref errcode);
            }
            else
            {
                StopJog(logicConfig.ECATAxis[5]);
                WarningSolution("【上料Tray】【报警】：【212】上料满Tray归位上升过程出错");
                return;
            }

            LoadFullTrayHomeFinished = true;
            return;
        }

        public void UnloadNullTrayHome()
        {
            int curpos = 0; int errcode = 0;

            //注意恢复
            //StartJog(logicConfig.ECATAxis[6], 1);
            //DateTime starttime = DateTime.Now;
            //
            //while (true)
            //{
            //    adlink.GetCurPostion(logicConfig.ECATAxis[6].AxisId, ref curpos);
            //    if (curpos / logicConfig.ECATAxis[6].Rate <= systemParam.TraySwitchUnloadNullRollSensePos)
            //    {
            //        if (CheckUnloadNullRollSignals() == true)
            //        {
            //            UpdateWaringLog.Invoke((object)"下料空Tray检测翻转信号成功");
            //            break;
            //        }
            //        else
            //        {
            //            UpdateWaringLogNG.Invoke((object)"下料空Tray检测翻转信号失败");
            //            StopJog(logicConfig.ECATAxis[6]);
            //            return;
            //        }
            //    }
            //    else
            //    {
            //        if (!OutTimeCount(starttime, 10))
            //        {
            //            UpdateWaringLogNG.Invoke((object)"下料空Tray补料移动1失败");
            //            StopJog(logicConfig.ECATAxis[6]);
            //            return;
            //        }
            //    }
            //    Thread.Sleep(50);
            //}

            //while (true)
            //{
            //    adlink.GetCurPostion(logicConfig.ECATAxis[6].AxisId, ref curpos);
            //    if (curpos / logicConfig.ECATAxis[6].Rate <= systemParam.TraySwitchUnloadNullDownLimit)
            //    {
            //        StopJog(logicConfig.ECATAxis[6]);
            //        UpdateWaringLog.Invoke((object)"下料空Tray补料移动至下限位成功");
            //        break;
            //    }
            //    else
            //    {
            //        if (!OutTimeCount(starttime, 30))
            //        {
            //            UpdateWaringLogNG.Invoke((object)"【213】下料空Tray补料移动至下限位失败");
            //            StopJog(logicConfig.ECATAxis[6]);
            //            return;
            //        }
            //    }
            //    Thread.Sleep(10);
            //}

            //0730 Test Code
            adlink.P2PMove(logicConfig.ECATAxis[6], systemParam.TraySwitchUnloadNullDownLimit, true, ref errcode);
            StopJog(logicConfig.ECATAxis[6]);
            StartJog(logicConfig.ECATAxis[6], 0);

            if (WaitUnloadTrayNullInPosition())
            {
                StopJog(logicConfig.ECATAxis[6]);
            }
            else
            {
                StopJog(logicConfig.ECATAxis[6]);
                WarningSolution("【214】下料空Tray归位上升过程出错");
                return;
            }
            UnloadNullTrayHomeFinished = true;
            return;
        }

        public void UnloadFullTrayHome()
        {
            int curpos = 0;
            int errcode = 0;

            if (adlink.P2PMove(logicConfig.ECATAxis[7], systemParam.TraySwitchUnloadFullDownLimit, true, ref errcode) == false)
            {
                WarningSolution("【215】下料满Tray移动至下限位出错");
                return;
            }

            //StartJog(logicConfig.ECATAxis[7], 0);
            //DateTime starttime = DateTime.Now;
            //注意恢复
            //while (true)
            //{
            //    adlink.GetCurPostion(logicConfig.ECATAxis[7].AxisId, ref curpos);
            //    if (curpos / logicConfig.ECATAxis[7].Rate >= systemParam.TraySwitchUnloadFullRollSensePos)
            //    {
            //        if (CheckUnloadFullRollSignals() == true)
            //        {
            //            UpdateWaringLog.Invoke((object)"下料满Tray检测翻转信号成功");
            //            break;
            //        }
            //        else
            //        {
            //            UpdateWaringLogNG.Invoke((object)"下料满Tray检测翻转信号失败");
            //            StopJog(logicConfig.ECATAxis[7]);
            //            return;
            //        }
            //    }
            //    else
            //    {
            //        if (!OutTimeCount(starttime, 30))
            //        {
            //            UpdateWaringLogNG.Invoke((object)"下料满Tray补料向上移动1失败");
            //            StopJog(logicConfig.ECATAxis[7]);
            //            return;
            //        }
            //    }
            //    Thread.Sleep(50);
            //}

            //while (true)
            //{
            //    adlink.GetCurPostion(logicConfig.ECATAxis[7].AxisId, ref curpos);
            //    if (curpos / logicConfig.ECATAxis[7].Rate >= systemParam.TraySwitchUnloadFullUpLimit)
            //    {
            //        StopJog(logicConfig.ECATAxis[7]);
            //        UpdateWaringLog.Invoke((object)"下料满Tray补料移动至上限位成功");
            //        break;
            //    }
            //    else
            //    {
            //        if (!OutTimeCount(starttime, 30))
            //        {
            //            UpdateWaringLogNG.Invoke((object)"【216】下料满Tray补料移动至上限位失败");
            //            StopJog(logicConfig.ECATAxis[4]);
            //            return;
            //        }
            //    }
            //    Thread.Sleep(10);
            //}

            if (adlink.P2PMove(logicConfig.ECATAxis[7], systemParam.TraySwitchUnloadFullUpLimit, true, ref errcode) == false)
            {
                WarningSolution("【216】下料满Tray补料移动至上限位失败");
                return;
            }
            UnloadFullTrayHomeFinished = true;
            return;
        }

        private bool CheckLoadNullRollSignals()
        {
            bool tempsignal = false;
            tempsignal = (CurInfo.Di[(int)ECATDINAME.Di_LoadNullDrawerInsideSense + 25] && CurInfo.Di[(int)ECATDINAME.Di_LoadNullDrawerOutsideSense + 25]) || debugLoadRollSense;
            return tempsignal;
        }

        private bool CheckLoadFullRollSignals()
        {
            bool tempsignal = false;
            tempsignal = (CurInfo.Di[(int)ECATDINAME.Di_LoadFullDrawerInsideSense + 25] && CurInfo.Di[(int)ECATDINAME.Di_LoadFullDrawerOutsideSense + 25]) || debugLoadRollSense;
            return tempsignal;
        }

        private bool CheckUnloadNullRollSignals()
        {
            bool tempsignal = false;
            tempsignal = (CurInfo.Di[(int)ECATDINAME.Di_UnloadNullDrawerInsideSense + 25] && CurInfo.Di[(int)ECATDINAME.Di_UnloadNullDrawerOutsideSense + 25]) || debugUnloadRollSense;
            return tempsignal;
        }

        private bool CheckUnloadFullRollSignals()
        {
            bool tempsignal = false;
            tempsignal = CurInfo.Di[(int)ECATDINAME.Di_UnloadFullDrawerInsideSense + 25] && CurInfo.Di[(int)ECATDINAME.Di_UnloadFullDrawerOutsideSense + 25] || debugUnloadRollSense;
            return tempsignal;
        }
        #endregion

        #region 活塞动作
        //7432板卡双控双反馈
        public bool WaitPiston2Cmd2FeedbackDone(int truecmd, int falsecmd, int truefeedback, int falsefeedback)
        {
            IOControl.WriteDO(truecmd, true);
            IOControl.WriteDO(falsecmd, false);
            DateTime StartTime = DateTime.Now;
            while (true)
            {
                if (CurInfo.Di[truefeedback] && (!CurInfo.Di[falsefeedback]))
                {
                    return true;
                }
                else
                {
                    if (!OutTimeCount(StartTime, 3))
                    {
                        return false;
                    }
                    Thread.Sleep(40);
                }
            }
        }

        public bool WaitPiston2Cmd4FeedbackDone(int truecmd, int falsecmd, int truefeedback1, int falsefeedback1, int truefeedback2, int falsefeedback2)
        {
            IOControl.WriteDO(truecmd, true);
            IOControl.WriteDO(falsecmd, false);
            DateTime StartTime = DateTime.Now;
            while (true)
            {
                if (CurInfo.Di[truefeedback1] && (!CurInfo.Di[falsefeedback1]) &&
                    CurInfo.Di[truefeedback2] && (!CurInfo.Di[falsefeedback2]))
                {
                    return true;
                }
                else
                {
                    if (!OutTimeCount(StartTime, 10))
                    {
                        return false;
                    }
                    Thread.Sleep(40);
                }
            }
        }

        public bool WaitECATPiston2Cmd2FeedbackDone(int truecmd, int falsecmd, int truefeedback, int falsefeedback)
        {
            IOControl.ECATWriteDO(truecmd, true);
            IOControl.ECATWriteDO(falsecmd, false);
            DateTime StartTime = DateTime.Now;
            while (true)
            {
                if (CurInfo.Di[truefeedback + 25] && (!CurInfo.Di[falsefeedback + 25]))
                {
                    return true;
                }
                else
                {
                    if (!OutTimeCount(StartTime, 5))
                    {
                        return false;
                    }
                    Thread.Sleep(40);
                }
            }
        }

        public bool WaitECATPiston4Cmd2FeedbackDone(int truecmd1, int falsecmd1, int truecmd2, int falsecmd2, int feedback1, int feedback2, bool demandfeedback1, bool demandfeedback2)
        {
            IOControl.ECATWriteDO(truecmd1, true); IOControl.ECATWriteDO(falsecmd1, false);
            IOControl.ECATWriteDO(truecmd2, true); IOControl.ECATWriteDO(falsecmd2, false);
            DateTime StartTime = DateTime.Now;
            while (true)
            {
                if ((CurInfo.Di[feedback1 + 25] == demandfeedback1) && (CurInfo.Di[feedback2 + 25] == demandfeedback2))
                {
                    return true;
                }
                else
                {
                    if (!OutTimeCount(StartTime, 5))
                    {
                        return false;
                    }
                    Thread.Sleep(40);
                }
            }
        }

        public bool WaitECATPiston2Cmd1FeedbackDone(int truecmd, int falsecmd, int feedback, bool demandvalue)
        {
            IOControl.ECATWriteDO(truecmd, true);
            IOControl.ECATWriteDO(falsecmd, false);
            DateTime StartTime = DateTime.Now;
            while (true)
            {
                if (CurInfo.Di[feedback + 25] == demandvalue)
                {
                    return true;
                }
                else
                {
                    if (!OutTimeCount(StartTime, 3))
                    {
                        return false;
                    }
                    Thread.Sleep(40);
                }
            }
        }

        public bool WaitECATPiston1Cmd2FeedbackDone(int cmd, bool demandvalue, int truefeedback, int falsefeedback)
        {
            IOControl.ECATWriteDO(cmd, demandvalue);
            DateTime StartTime = DateTime.Now;
            while (true)
            {
                if (CurInfo.Di[truefeedback + 25] == true && CurInfo.Di[falsefeedback + 25] == false)
                {
                    return true;
                }
                else
                {
                    if (!OutTimeCount(StartTime, 5))
                    {
                        return false;
                    }
                    Thread.Sleep(40);
                }
            }
        }
        #endregion

        #region pin脚取放测试
        public bool SuckPinTestStart = false;
        public void SuckPinTest()
        {
            while (SuckPinTestStart)
            {
                if (SuckAxisMove(systemParam.SuckAxisRightPos, true) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisSuckerSuck() != 1)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                Thread.Sleep(100);

                if (SuckAxisMove(systemParam.SuckAxisLeftPos, true) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisSuckerBreak() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisSuckerSuck() != 1)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                Thread.Sleep(100);

                if (SuckAxisMove(systemParam.SuckAxisRightPos, true) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisSuckerBreak() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);

                Thread.Sleep(100);
            }
        }
        #endregion

        #region 重复性单穴测试
        public void RepeatTestOnce(object myobject)
        {
            RepeatTestStru myRepeatStru = (RepeatTestStru)myobject;
            int errcode = 0;
            int componentstep = 0;
            int currenttime = 0;

            CCDChecking = true;
            debugThreadB = true;
            debugThreadC = true;

            while (currenttime < myRepeatStru.repeattime)
            {
                Laser12DicData.Clear();

                if (StretchOutAllCylinder() == false)
                    return;

                if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisLoadSuckerSuck() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisLeftPos, true, ref errcode) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisLoadSuckerBreak() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);

                if (RetractAllCylinder() == false)
                    return;

                if (MainAxisMove(-33333, false) == false)
                    //if (adlink.P2PMove(logicConfig.PulseAxis[0], -33333, false, ref errcode) == false)
                    return;
                if (!logicIgnore[1])
                {
                    if (AutoRunPartBComponent(myRepeatStru.id, myRepeatStru.stationNo, 0, ref errcode, ref componentstep) != 1)
                        return;
                }
                if (MainAxisMove(-33333, false) == false)
                    //if (adlink.P2PMove(logicConfig.PulseAxis[0], -33333, false, ref errcode) == false)
                    return;

                if (!logicIgnore[2])
                {
                    switch (myRepeatStru.id)
                    {
                        case 0:
                            if (AutoRunPartCComponent(0, myRepeatStru.stationNo, ref errcode) != 1)
                                return;
                            break;
                        case 1:
                            if (AutoRunPartCComponent(1, myRepeatStru.stationNo, ref errcode) != 1)
                                return;
                            break;
                        case 2:
                            if (AutoRunPartCComponent(2, myRepeatStru.stationNo, ref errcode) != 1)
                                return;
                            break;
                        case 3:
                            if (AutoRunPartCComponent(3, myRepeatStru.stationNo, ref errcode) != 1)
                                return;
                            break;
                    }
                }
                if (MainAxisMove(-33334, false) == false)
                    //if (adlink.P2PMove(logicConfig.PulseAxis[0], -33334, false, ref errcode) == false)
                    return;

                if (StretchOutAllCylinder() == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisLoadSuckerSuck() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisLoadSuckerBreak() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);

                if (RetractAllCylinder() == false)
                    return;

                while (isWaitingCCD || isCheckingCCD || TcpIpRecvStatus)
                    Thread.Sleep(10);

                RecvCount = 0;
                CCDRawDataList.Clear();
                CCDRecvStatusList.Clear();
                currenttime++;
            }

            CCDChecking = false;
            debugThreadB = false;
            debugThreadC = false;
            AutoRunPartAStretchFinish = false;
            UpdateDebugButton(1);
            ThreadNames.Clear();
        }
        #endregion

        #region 相关性测试
        public void RepeatTestOnceXgx(object myobject)
        {
            RepeatTestStru myRepeatStru = (RepeatTestStru)myobject;
            int errcode = 0;
            int componentstep = 0;
            int currenttime = 0;

            CCDChecking = true;
            debugThreadB = true;
            debugThreadC = true;

            while (currenttime < 1)
            {
                Laser12DicData.Clear();

                if (StretchOutAllCylinder() == false)
                    return;

                //if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode) == false)
                //    return;

                //if (SuckAxisZStretch() == false)
                //    return;

                //if (SuckAxisLoadSuckerSuck() == false)
                //    return;

                //if (SuckAxisZRetract() == false)
                //    return;

                //if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisLeftPos, true, ref errcode) == false)
                //    return;

                //if (SuckAxisZStretch() == false)
                //    return;

                //if (SuckAxisLoadSuckerBreak() == false)
                //    return;

                //if (SuckAxisZRetract() == false)
                //    return;

                //IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
                //IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);

                if (RetractAllCylinder() == false)
                    return;

                if (MainAxisMove(-33333, false) == false)
                    //if (adlink.P2PMove(logicConfig.PulseAxis[0], -33333, false, ref errcode) == false)
                    return;
                if (!logicIgnore[1])
                {
                    if (AutoRunPartBComponent(myRepeatStru.id, myRepeatStru.stationNo, 0, ref errcode, ref componentstep) != 1)
                        return;
                }
                if (MainAxisMove(-33333, false) == false)
                    //if (adlink.P2PMove(logicConfig.PulseAxis[0], -33333, false, ref errcode) == false)
                    return;

                if (!logicIgnore[2])
                {
                    switch (myRepeatStru.id)
                    {
                        case 0:
                            if (AutoRunPartCComponent(0, myRepeatStru.stationNo, ref errcode) != 1)
                                return;
                            break;
                        case 1:
                            if (AutoRunPartCComponent(1, myRepeatStru.stationNo, ref errcode) != 1)
                                return;
                            break;
                        case 2:
                            if (AutoRunPartCComponent(2, myRepeatStru.stationNo, ref errcode) != 1)
                                return;
                            break;
                        case 3:
                            if (AutoRunPartCComponent(3, myRepeatStru.stationNo, ref errcode) != 1)
                                return;
                            break;
                    }
                }
                if (MainAxisMove(-33334, false) == false)
                    //if (adlink.P2PMove(logicConfig.PulseAxis[0], -33334, false, ref errcode) == false)
                    return;

                if (StretchOutAllCylinder() == false)
                    return;

                //if (SuckAxisZStretch() == false)
                //    return;

                //if (SuckAxisLoadSuckerSuck() == false)
                //    return;

                //if (SuckAxisZRetract() == false)
                //    return;

                //if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode) == false)
                //    return;

                //if (SuckAxisZStretch() == false)
                //    return;

                //if (SuckAxisLoadSuckerBreak() == false)
                //    return;

                //if (SuckAxisZRetract() == false)
                //    return;

                //IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
                //IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);

                //if (RetractAllCylinder() == false)
                //    return;

                while (isWaitingCCD || isCheckingCCD || TcpIpRecvStatus)
                    Thread.Sleep(10);

                RecvCount = 0;
                CCDRawDataList.Clear();
                CCDRecvStatusList.Clear();
                currenttime++;
            }

            CCDChecking = false;
            debugThreadB = false;
            debugThreadC = false;
            AutoRunPartAStretchFinish = false;
            UpdateDebugButton(3);
            ThreadNames.Clear();
        }
        #endregion

        #region 重复性多穴测试
        public void RepeatTestMultiHole(object myobj)
        {
            RepeatTestStru repeatStru = (RepeatTestStru)myobj;
            int errcode = 0;
            int currenttime = 0;

            AutoRunActive = true;
            CCDChecking = true;
            debugThreadB = true;
            AutoRunEnablePartB = true;
            debugThreadC = true;
            AutoRunEnablePartC = true;
            PartBTrayNo = repeatStru.stationNo; PartCTrayNo = repeatStru.stationNo;

            while (currenttime < repeatStru.repeattime)
            {
                Laser12DicData.Clear();

                if (StretchOutAllCylinder() == false)
                    return;

                if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisLoadSuckerSuck() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisLeftPos, true, ref errcode) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisLoadSuckerBreak() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);

                if (RetractAllCylinder() == false)
                    return;

                if (MainAxisMove(-33333, false) == false)
                    //if (adlink.P2PMove(logicConfig.PulseAxis[0], -33333, false, ref errcode) == false)
                    return;
                if (!logicIgnore[1])
                {
                    if (debugThread != null)
                        debugThread.Abort();
                    debugThread = new Thread(new ThreadStart(AutoRunPartBThread), 1204);
                    debugThread.IsBackground = true;
                    AutoRunPartBFinished = false;
                    debugThread.Start();

                    while (!AutoRunPartBFinished)
                        Thread.Sleep(50);
                    debugThread.Abort();
                }
                if (MainAxisMove(-33333, false) == false)
                    //if (adlink.P2PMove(logicConfig.PulseAxis[0], -33333, false, ref errcode) == false)
                    return;

                if (!logicIgnore[2])
                {
                    if (debugThread != null)
                        debugThread.Abort();

                    debugThread = new Thread(new ThreadStart(AutoRunPartCThread));
                    debugThread.IsBackground = true;
                    AutoRunPartCFinished = false;
                    debugThread.Start();

                    while (!AutoRunPartCFinished)
                        Thread.Sleep(50);
                    debugThread.Abort();
                    nLaserData = 0;
                }

                if (MainAxisMove(-33334, false) == false)
                    //if (adlink.P2PMove(logicConfig.PulseAxis[0], -33334, false, ref errcode) == false)
                    return;

                if (StretchOutAllCylinder() == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisLoadSuckerSuck() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisLoadSuckerBreak() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);

                if (RetractAllCylinder() == false)
                    return;

                while (isWaitingCCD || isCheckingCCD || TcpIpRecvStatus)
                    Thread.Sleep(10);

                RecvCount = 0;
                CCDRawDataList.Clear();
                CCDRecvStatusList.Clear();
                AutoRunPartBCircleCount = 0;
                AutoRunPartCCircleCount = 0;
                currenttime++;
            }

            AutoRunActive = false;
            CCDChecking = false;
            debugThreadB = false;
            debugThreadC = false;
            AutoRunEnablePartB = false;
            AutoRunEnablePartC = false;
            AutoRunPartAStretchFinish = false;
            UpdateDebugButton(2);
            ThreadNames.Clear();
        }
        #endregion

        #region 12穴重复性测试
        public void RepeatTestAllMultiHole(object myobj)
        {
            RepeatTestStru repeatStru = (RepeatTestStru)myobj;
            int errcode = 0;
            int currenttime = 0;

            AutoRunActive = true;
            CCDChecking = true;
            debugThreadB = true;
            AutoRunEnablePartB = true;
            debugThreadC = true;
            AutoRunEnablePartC = true;
            //PartBTrayNo = repeatStru.stationNo; PartCTrayNo = repeatStru.stationNo;
            Laser12DicData.Clear();

            #region  移动至0位右侧吸嘴吸
            if (LoadGantryMoveAxis(0) == false)
            {
                LoadGantryErrorSolution(100);
                return;
            }
            if (LoadGantryRightPistonZStretch() == false)
            {
                LoadGantryErrorSolution(100);
                return;
            }
            if (LoadGantryRightSuckerSuck() == false)
            {
                LoadGantryErrorSolution(0);
                return;
            }
            if (LoadGantryRightPistonZRetract() == false)
            {
                LoadGantryErrorSolution(100);
                return;
            }
            #endregion
            #region  移动至1位左侧吸嘴吸
            if (LoadGantryMoveAxis(1) == false)
            {
                LoadGantryErrorSolution(100);
                return;
            }
            if (LoadGantryLeftPistonZStretch() == false)
            {
                LoadGantryErrorSolution(100);
                return;
            }
            if (LoadGantryLeftSuckerSuck() == false)
            {
                LoadGantryErrorSolution(0);
                return;
            }
            if (LoadGantryLeftPistonZRetract() == false)
            {
                LoadGantryErrorSolution(100);
                return;
            }
            #endregion
            #region  放料至上料模组并伸出
            if (LoadGantryMoveToLoadModule() == false)
            {
                LoadGantryErrorSolution(100);
                return;
            }
            if (LoadGantryAllPistonZStretch() == false)
            {
                LoadGantryErrorSolution(5);
                return;
            }
            if (LoadGantryAllSuckerBreak() == false)
            {
                LoadGantryErrorSolution(100);
                return;
            }
            if (LoadGantryAllPistonZRetract() == false)
            {
                LoadGantryErrorSolution(100);
                return;
            }
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumBreak, false);

            if (LoadModuleMoveStretch() == false)
            {
                return;
            }
            #endregion
            #region 前四料放置载具
            if (StretchOutAllCylinder() == false)
                return;

            if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode) == false)
                return;

            if (SuckAxisZStretch() == false)
                return;

            if (SuckAxisLoadSuckerSuck() == false)
                return;

            if (SuckAxisZRetract() == false)
                return;

            if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisLeftPos, true, ref errcode) == false)
                return;

            if (SuckAxisZStretch() == false)
                return;

            if (SuckAxisLoadSuckerBreak() == false)
                return;

            if (SuckAxisZRetract() == false)
                return;

            IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);

            if (RetractAllCylinder() == false)
                return;
            #endregion

            if (MainAxisMove(-33333, false) == false)
                return;
            if (!logicIgnore[1])
            {
                PartBTrayNo = 0;

                if (debugBThread != null)
                    debugBThread.Abort();
                debugBThread = new Thread(new ThreadStart(AutoRunPartBThread));
                debugBThread.IsBackground = true;
                AutoRunPartBFinished = false;
                debugBThread.Start();

                while (!AutoRunPartBFinished)
                    Thread.Sleep(50);
                //debugBThread.Abort();
            }

            #region   前8料放置载具
            if (StretchOutAllCylinder() == false)
                return;
            if (LoadModulePosMoveStretch() == false)
            {
                return;
            }

            if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode) == false)
                return;

            if (SuckAxisZStretch() == false)
                return;

            if (SuckAxisLoadSuckerSuck() == false)
                return;

            if (SuckAxisZRetract() == false)
                return;

            if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisLeftPos, true, ref errcode) == false)
                return;

            if (SuckAxisZStretch() == false)
                return;

            if (SuckAxisLoadSuckerBreak() == false)
                return;

            if (SuckAxisZRetract() == false)
                return;

            IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);

            if (RetractAllCylinder() == false)
                return;
            #endregion

            if (MainAxisMove(-33333, false) == false)
                return;

            if (!logicIgnore[1] && !logicIgnore[2])
            {
                AutoRunPartBFinished = false;

                PartBTrayNo = 2;
                PartCTrayNo = 0;

                if (debugCThread != null)
                    debugCThread.Abort();

                debugCThread = new Thread(new ThreadStart(AutoRunPartCThread));
                debugCThread.IsBackground = true;
                AutoRunPartCFinished = false;
                //AutoRunEnablePartC = true;//3.3.65.3改动
                debugCThread.Start();

                while (!AutoRunPartCFinished)
                    Thread.Sleep(50);
                //AutoRunEnablePartC = false;//3.3.65.3改动

                while (!AutoRunPartBFinished)
                    Thread.Sleep(50);
                nLaserData = 0;
            }

            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadMoveCylinder, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadMoveStretchControl, false); IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadMoveRetractControl, true);

            #region  移动至2位右侧吸嘴吸
            if (LoadGantryMoveAxis(2) == false)
            {
                LoadGantryErrorSolution(100);
                return;
            }
            if (LoadGantryRightPistonZStretch() == false)
            {
                LoadGantryErrorSolution(100);
                return;
            }
            if (LoadGantryRightSuckerSuck() == false)
            {
                LoadGantryErrorSolution(0);
                return;
            }
            if (LoadGantryRightPistonZRetract() == false)
            {
                LoadGantryErrorSolution(100);
                return;
            }
            #endregion
            #region  移动至3位左侧吸嘴吸
            if (LoadGantryMoveAxis(3) == false)
            {
                LoadGantryErrorSolution(100);
                return;
            }
            if (LoadGantryLeftPistonZStretch() == false)
            {
                LoadGantryErrorSolution(100);
                return;
            }
            if (LoadGantryLeftSuckerSuck() == false)
            {
                LoadGantryErrorSolution(0);
                return;
            }
            if (LoadGantryLeftPistonZRetract() == false)
            {
                LoadGantryErrorSolution(100);
                return;
            }
            #endregion
            #region  放料至上料模组并伸出
            if (LoadGantryMoveToLoadModule() == false)
            {
                LoadGantryErrorSolution(100);
                return;
            }
            if (LoadGantryAllPistonZStretch() == false)
            {
                LoadGantryErrorSolution(5);
                return;
            }
            if (LoadGantryAllSuckerBreak() == false)
            {
                LoadGantryErrorSolution(100);
                return;
            }
            if (LoadGantryAllPistonZRetract() == false)
            {
                LoadGantryErrorSolution(100);
                return;
            }
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadLeftVacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_LoadRightVacumBreak, false);

            if (LoadModuleMoveStretch() == false)
            {
                return;
            }
            #endregion
            #region 前12料放置载具
            if (StretchOutAllCylinder() == false)
                return;

            if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode) == false)
                return;

            if (SuckAxisZStretch() == false)
                return;

            if (SuckAxisLoadSuckerSuck() == false)
                return;

            if (SuckAxisZRetract() == false)
                return;

            if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisLeftPos, true, ref errcode) == false)
                return;

            if (SuckAxisZStretch() == false)
                return;

            if (SuckAxisLoadSuckerBreak() == false)
                return;

            if (SuckAxisZRetract() == false)
                return;

            IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);

            if (RetractAllCylinder() == false)
                return;

            if (MainAxisMove(-33334, false) == false)
                return;

            if (!logicIgnore[1] && !logicIgnore[2])
            {
                PartBTrayNo = 1;
                PartCTrayNo = 2;
                AutoRunPartCFinished = false;
                AutoRunPartBFinished = false;
                while (!AutoRunPartBFinished)
                    Thread.Sleep(50);

                while (!AutoRunPartCFinished)
                    Thread.Sleep(50);
                //debugThread.Abort();
                nLaserData = 0;
            }

            #endregion

            #region  横移轴来回放置
            if (StretchOutAllCylinder() == false)
                return;

            if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisLeftPos, true, ref errcode) == false)
                return;

            if (SuckAxisZStretch() == false)
                return;

            if (SuckAxisLoadSuckerSuck() == false)
                return;

            if (SuckAxisZRetract() == false)
                return;

            if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode) == false)
                return;

            if (SuckAxisZStretch() == false)
                return;

            if (SuckAxisLoadSuckerBreak() == false)
                return;

            if (SuckAxisZRetract() == false)
                return;

            IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);

            if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode) == false)
                return;

            if (SuckAxisZStretch() == false)
                return;

            if (SuckAxisLoadSuckerSuck() == false)
                return;

            if (SuckAxisZRetract() == false)
                return;

            if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisLeftPos, true, ref errcode) == false)
                return;

            if (SuckAxisZStretch() == false)
                return;

            if (SuckAxisLoadSuckerBreak() == false)
                return;

            if (SuckAxisZRetract() == false)
                return;

            IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
            IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);


            if (RetractAllCylinder() == false)
                return;
            #endregion

            while (currenttime < repeatStru.repeattime - 1)
            {
                Laser12DicData.Clear();

                if (MainAxisMove(-33334, false) == false)
                    //if (adlink.P2PMove(logicConfig.PulseAxis[0], -33333, false, ref errcode) == false)
                    return;
                if (!logicIgnore[1] && !logicIgnore[2])
                {
                    PartBTrayNo = 0;
                    PartCTrayNo = 1;
                    AutoRunPartCFinished = false;
                    AutoRunPartBFinished = false;
                    while (!AutoRunPartBFinished)
                        Thread.Sleep(50);

                    while (!AutoRunPartCFinished)
                        Thread.Sleep(50);
                    //debugThread.Abort();
                    nLaserData = 0;
                }

                #region  横移轴来回放置
                if (StretchOutAllCylinder() == false)
                    return;

                if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisLeftPos, true, ref errcode) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisLoadSuckerSuck() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisLoadSuckerBreak() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);

                if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisLoadSuckerSuck() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisLeftPos, true, ref errcode) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisLoadSuckerBreak() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);


                if (RetractAllCylinder() == false)
                    return;
                #endregion

                if (MainAxisMove(-33333, false) == false)
                    //if (adlink.P2PMove(logicConfig.PulseAxis[0], -33333, false, ref errcode) == false)
                    return;
                if (!logicIgnore[1] && !logicIgnore[2])
                {
                    PartBTrayNo = 2;
                    PartCTrayNo = 0;
                    AutoRunPartCFinished = false;
                    AutoRunPartBFinished = false;
                    while (!AutoRunPartBFinished)
                        Thread.Sleep(50);

                    while (!AutoRunPartCFinished)
                        Thread.Sleep(50);
                    //debugThread.Abort();
                    nLaserData = 0;
                }

                #region  横移轴来回放置
                if (StretchOutAllCylinder() == false)
                    return;

                if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisLeftPos, true, ref errcode) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisLoadSuckerSuck() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisLoadSuckerBreak() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);

                if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisLoadSuckerSuck() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisLeftPos, true, ref errcode) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisLoadSuckerBreak() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);


                if (RetractAllCylinder() == false)
                    return;
                #endregion


                if (MainAxisMove(-33333, false) == false)
                    //if (adlink.P2PMove(logicConfig.PulseAxis[0], -33334, false, ref errcode) == false)
                    return;

                if (!logicIgnore[1] && !logicIgnore[2])
                {
                    PartBTrayNo = 1;
                    PartCTrayNo = 2;
                    AutoRunPartCFinished = false;
                    AutoRunPartBFinished = false;
                    while (!AutoRunPartBFinished)
                        Thread.Sleep(50);

                    while (!AutoRunPartCFinished)
                        Thread.Sleep(50);
                    //debugThread.Abort();
                    nLaserData = 0;
                }

                #region  横移轴来回放置
                if (StretchOutAllCylinder() == false)
                    return;

                if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisLeftPos, true, ref errcode) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisLoadSuckerSuck() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisLoadSuckerBreak() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);

                if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisLoadSuckerSuck() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                if (adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisLeftPos, true, ref errcode) == false)
                    return;

                if (SuckAxisZStretch() == false)
                    return;

                if (SuckAxisLoadSuckerBreak() == false)
                    return;

                if (SuckAxisZRetract() == false)
                    return;

                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckLoadVacumBreak, false);
                IOControl.ECATWriteDO((int)ECATDONAME.Do_SuckUnloadVacumBreak, false);


                if (RetractAllCylinder() == false)
                    return;
                #endregion

                while (isWaitingCCD || isCheckingCCD || TcpIpRecvStatus)
                    Thread.Sleep(10);

                RecvCount = 0;
                CCDRawDataList.Clear();
                CCDRecvStatusList.Clear();
                AutoRunPartBCircleCount = 0;
                AutoRunPartCCircleCount = 0;
                currenttime++;
            }

            if (MainAxisMove(-33334, false) == false)
                return;
            if (!logicIgnore[2])
            {
                PartCTrayNo = 1;
                AutoRunPartCFinished = false;
                while (!AutoRunPartCFinished)
                    Thread.Sleep(50);
                nLaserData = 0;
            }

            debugBThread.Abort();
            debugCThread.Abort();

            AutoRunActive = false;
            CCDChecking = false;
            debugThreadB = false;
            debugThreadC = false;
            AutoRunEnablePartB = false;
            AutoRunEnablePartC = false;
            AutoRunPartAStretchFinish = false;
            UpdateDebugButton(2);
            ThreadNames.Clear();
        }
        #endregion

        #region 补料盘吸取测试
        public void SupplySuckTest()
        {
            int Region1Pos = 100; int Region2Pos = 100;

            while (GetRegion2CurNullPos(SupplyRegion2Condition) != 8 && GetRegion1CurSupplyPos(SupplyRegion1Condition) != 8)//开始补料
            {
                Region1Pos = GetRegion1CurSupplyPos(SupplyRegion1Condition);
                Region2Pos = GetRegion2CurNullPos(SupplyRegion2Condition);

                int tempresult = MoveRegion1ToRegion2(Region1Pos, Region2Pos);
                if (tempresult != 1)
                {
                    if (tempresult == -2)
                        UnloadGantryErrorSolution(2);
                    else
                        UnloadGantryErrorSolution(100);
                    return;
                }
                SupplyRegion1Condition[Region1Pos] = 0;
                SupplyRegion2Condition[Region2Pos] = 1;

                Thread.Sleep(40);
            }
        }
        #endregion

        #region 各主要轴加保护动作（主轴、上料龙门、下料龙门、横移轴）
        public void MainAxisStartJog(int dir)
        {
            if (mainaxismovesafesignal)
                adlink.JogMoveStart(logicConfig.PulseAxis[0], dir);
            else
                MessageBox.Show("请确认主轴处于安全位置");
        }

        public void MainAxisStopJog()
        {
            adlink.JogMoveStop(logicConfig.PulseAxis[0]);
        }

        public bool MainAxisMove(double targetpos, bool isAbsolute)
        {
            int errcode = 0;
            if (mainaxismovesafesignal)
            {
                return adlink.P2PMove(logicConfig.PulseAxis[0], targetpos, isAbsolute, ref errcode);
            }
            else
            {
                MessageBox.Show("请确认主轴处于安全位置");
                return false;
            }

        }

        public void MainAxisHome()
        {
            if (mainaxishomesafesignal)
                adlink.HomeMoveZ(logicConfig.PulseAxis[0]);
            else
                MessageBox.Show("请确认圆盘处于安全位置");
        }

        public void LoadGantryXStartJog(int dir)
        {
            if (loadgantrymovesafesignal)
                adlink.JogMoveStart(logicConfig.ECATAxis[0], dir);
            else
                MessageBox.Show("请确认上料龙门吸嘴已缩回");
        }

        public void LoadGantryXStopJog()
        {
            adlink.JogMoveStop(logicConfig.ECATAxis[0]);
        }

        public bool LoadGantryXMove(double targetpos, bool isAbsolute)
        {
            int errcode = 0;
            if (loadgantrymovesafesignal)
                return adlink.P2PMove(logicConfig.ECATAxis[0], targetpos, isAbsolute, ref errcode);
            else
            {
                MessageBox.Show("请确认上料龙门吸嘴已缩回");
                return false;
            }
        }

        public void LoadGantryXHome()
        {
            if (loadgantrymovesafesignal)
                adlink.HomeMove(logicConfig.ECATAxis[0]);
            else
                MessageBox.Show("请确认上料龙门吸嘴已缩回");
        }

        public void LoadGantryYStartJog(int dir)
        {
            if (loadgantrymovesafesignal)
                adlink.JogMoveStart(logicConfig.ECATAxis[1], dir);
            else
                MessageBox.Show("请确认上料龙门吸嘴已缩回");
        }

        public void LoadGantryYStopJog()
        {
            adlink.JogMoveStop(logicConfig.ECATAxis[1]);
        }

        public bool LoadGantryYMove(double targetpos, bool isAbsolute)
        {
            int errcode = 0;
            if (loadgantrymovesafesignal)
                return adlink.P2PMove(logicConfig.ECATAxis[1], targetpos, isAbsolute, ref errcode);
            else
            {
                MessageBox.Show("请确认上料龙门吸嘴已缩回");
                return false;
            }
        }

        public void LoadGantryYHome()
        {
            if (loadgantrymovesafesignal)
                adlink.HomeMove(logicConfig.ECATAxis[1]);
            else
                MessageBox.Show("请确认上料龙门吸嘴已缩回");
        }


        public void UnloadGantryXStartJog(int dir)
        {
            if (unloadgantrymovesafesignal)
                adlink.JogMoveStart(logicConfig.ECATAxis[2], dir);
            else
                MessageBox.Show("请确认下料龙门吸嘴已缩回");
        }

        public void UnloadGantryXStopJog()
        {
            adlink.JogMoveStop(logicConfig.ECATAxis[2]);
        }

        public bool UnloadGantryXMove(double targetpos, bool isAbsolute)
        {
            int errcode = 0;
            if (unloadgantrymovesafesignal)
                return adlink.P2PMove(logicConfig.ECATAxis[2], targetpos, isAbsolute, ref errcode);
            else
            {
                MessageBox.Show("请确认下料龙门吸嘴已缩回");
                return false;
            }
        }

        public void UnloadGantryXHome()
        {
            if (unloadgantrymovesafesignal)
                adlink.HomeMove(logicConfig.ECATAxis[2]);
            else
                MessageBox.Show("请确认下料龙门吸嘴已缩回");
        }

        public void UnloadGantryYStartJog(int dir)
        {
            if (unloadgantrymovesafesignal)
                adlink.JogMoveStart(logicConfig.ECATAxis[3], dir);
            else
                MessageBox.Show("请确认下料龙门吸嘴已缩回");
        }

        public void UnloadGantryYStopJog()
        {
            adlink.JogMoveStop(logicConfig.ECATAxis[3]);
        }

        public bool UnloadGantryYMove(double targetpos, bool isAbsolute)
        {
            int errcode = 0;
            if (unloadgantrymovesafesignal)
                return adlink.P2PMove(logicConfig.ECATAxis[3], targetpos, isAbsolute, ref errcode);
            else
            {
                MessageBox.Show("请确认下料龙门吸嘴已缩回");
                return false;
            }
        }

        public void UnloadGantryYHome()
        {
            if (unloadgantrymovesafesignal)
                adlink.HomeMove(logicConfig.ECATAxis[3]);
            else
                MessageBox.Show("请确认下料龙门吸嘴已缩回");
        }

        public bool LoadGantryXYMove(double[] targetpos, bool isAbsolute)
        {
            int errcode = 0; double poserror = 0;

            if (loadgantrymovesafesignal)
            {
                return adlink.LineMove(new Axis[] { logicConfig.ECATAxis[0], logicConfig.ECATAxis[1] }, targetpos, isAbsolute, ref errcode, ref poserror);
            }
            else
            {
                MessageBox.Show("请确认上料龙门吸嘴已缩回");
                return false;
            }
        }

        public bool UnloadGantryXYMove(double[] targetpos, bool isAbsolute)
        {
            int errcode = 0; double poserror = 0;

            //if (unloadgantrymovesafesignal)
            //{
            return adlink.LineMove(new Axis[] { logicConfig.ECATAxis[2], logicConfig.ECATAxis[3] }, targetpos, isAbsolute, ref errcode, ref poserror);
            //}
            //else
            //{
            //    MessageBox.Show("请确认下料龙门吸嘴已缩回");
            //    return false;
            //}
        }

        public void SuckAxisStartJog(int dir)
        {
            if (suckaxissafesignal)
                adlink.JogMoveStart(logicConfig.PulseAxis[5], dir);
            else
                MessageBox.Show("请确认横移轴吸嘴已缩回");
        }

        public void SuckAxisStopJog()
        {
            adlink.JogMoveStop(logicConfig.PulseAxis[5]);
        }

        public bool SuckAxisMove(double targetpos, bool isAbsolute)
        {
            int errcode = 0;
            if (suckaxissafesignal)
                return adlink.P2PMove(logicConfig.PulseAxis[5], targetpos, isAbsolute, ref errcode);
            else
            {
                MessageBox.Show("请确认横移轴吸嘴已缩回");
                return false;
            }
        }

        public void SuckAxisHome()
        {
            if (suckaxissafesignal)
                adlink.HomeMove(logicConfig.PulseAxis[5]);
            else
                MessageBox.Show("请确认横移轴吸嘴已缩回");
        }
        #endregion

        #region 状态切换
        public void ResetButton()
        {
            switch (CurStatus)
            {
                case (int)STATUS.AUTO_STATUS:
                    MessageBox.Show("请先停止再按复位按钮");
                    break;
                case (int)STATUS.PAUSE_STATUS:
                    MessageBox.Show("请先停止再按复位按钮");
                    break;
                default:
                    //临时注释
                    if (MessageBox.Show("是否确认复位？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        WriteLog("【复位】复位按钮按下");
                        if (TcpSendMsg("RS\r\n"))
                        {
                            DateTime starttime = DateTime.Now;
                            while ((tcp_Recive != string.Empty) && (tcp_Recive.Substring(0, 2) != "RS"))
                            {
                                if (!OutTimeCount(starttime, 5))
                                {
                                    WarningSolution("CCD控制器复位超时");
                                    return;
                                }
                                Thread.Sleep(30);
                            }
                            ResetFlag();
                            ResetMainFormPassRatio();
                            ClearMainFormDataGridView();
                            CurStatus = (int)STATUS.READY_STATUS;
                        }
                    }
                    break;
            }
        }

        public void EmgStopButton()
        {
            switch (CurStatus)
            {
                case (int)STATUS.STOP_STATUS:
                    break;
                default:
                    WarningSolution("【急停】急停按钮按下");
                    SwitchToEmgStopMode();
                    break;
            }
        }

        public void PauseButton()
        {
            switch (CurStatus)
            {
                case (int)STATUS.AUTO_STATUS:
                    WarningSolution("【暂停】暂停按钮按下");
                    SwitchToPauseMode();
                    PauseButtonClicked = true;
                    break;
                case (int)STATUS.MANUAL_STATUS:
                    MotionStop();
                    break;
                case (int)STATUS.PAUSE_STATUS:
                    break;
                case (int)STATUS.READY_STATUS:
                    MotionStop();
                    break;
                case (int)STATUS.STOP_STATUS:
                    MessageBox.Show("请先解除急停状态");
                    break;
                default: break;
            }

        }

        public void StartButton()
        {
            switch (CurStatus)
            {
                case (int)STATUS.AUTO_STATUS:
                    CCDChecking = false;
                    PauseAllPart();
                    CurStatus = (int)STATUS.PAUSE_STATUS;
                    break;
                case (int)STATUS.MANUAL_STATUS:
                    MessageBox.Show("当前处于手动模式，无法启动");
                    break;
                case (int)STATUS.PAUSE_STATUS:
                    if (MessageBox.Show("是否确认继续？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        WriteLog("【继续】继续按钮按下");
                        //if (logicModule.bInitEnvironmentFinished == true)
                        //{
                        if (AutoRunEnablePartB == false)
                        {
                            TcpSendMsg("RS\r\n");
                            DateTime starttime = DateTime.Now;
                            while ((tcp_Recive != string.Empty) && (tcp_Recive.Substring(0, 2) != "RS"))
                            {
                                if (!OutTimeCount(starttime, 5))
                                {
                                    WarningSolution("CCD控制器复位超时");
                                    return;
                                }
                                Thread.Sleep(30);
                            }
                        }

                        if (isSet1Param == false)
                        {
                            string errcode;
                            if (!LaserMiniInit(out errcode))// 0906 add by ben
                                MessageBox.Show(errcode);
                        }

                        CurStatus = (int)STATUS.AUTO_STATUS;
                        //logicModule.RecoverAllCircleCount();
                        RecoverAllPart();
                        HideCalibAndDebugForm();
                        PauseButtonClicked = false;
                        //}
                        //else
                        //    MessageBox.Show("请先初始化");
                    }
                    break;
                case (int)STATUS.READY_STATUS:
                    //注意恢复
                    //if (bInitEnvironmentFinished == true)
                    //{
                    //    if (bInitTrayZAxis == true)
                    //    {
                    if (isResetClicked == false)
                    {
                        MessageBox.Show("请先复位再执行启动");
                    }
                    else
                    {
                        if (MessageBox.Show("是否确认启动？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                        {
                            if (CheckDrawerSignal() == false)
                                break;
                            if (isSet1Param == false)
                            {
                                string errcode;
                                if (!LaserMiniInit(out errcode))// 0906 add by ben
                                    MessageBox.Show(errcode);
                            }
                            WriteLog("【启动】启动按钮按下");
                            CurStatus = (int)STATUS.AUTO_STATUS;
                            AutoRunActive = true;
                            RecoverAllPart();
                            AutoRun();
                            //**************//樊竞明添加*******************//
                            //ExcelFileCopy();
                            //JudgeTimeBlock();
                            //********************************************//
                            HideCalibAndDebugForm();
                            isResetClicked = false;
                            PauseButtonClicked = false;
                        }
                    }
                    //    }
                    //    else
                    //    {
                    //        WarningSolution("Tray盘Z轴尚未初始化，请先进行初始化");
                    //    }
                    //}
                    //else
                    //    WarningSolution("尚未初始化，请先进行初始化");
                    break;
                case (int)STATUS.STOP_STATUS:
                    MessageBox.Show("请先解除急停模式");
                    break;
                default:
                    break;
            }
        }

        public void StartBeep()
        {
            for (int i = 0; i < 4; i++)
            {
                IOControl.WriteDO((int)DONAME.Do_Buzzer, true);
                Thread.Sleep(500);
                IOControl.WriteDO((int)DONAME.Do_Buzzer, false);
                Thread.Sleep(500);
            }
        }

        public void StopBeep()
        {
            IOControl.WriteDO((int)DONAME.Do_Buzzer, false);
        }

        #endregion

        public void LogicModuleDispose()
        {
            DaskDispose();
            Tcp_DisConnect();
            EtherCAT_DisConnect();
            CTLogsw.Close();
            CTLogfs.Close();
            MoveLogsw.Close();
            MoveLogfs.Close();
            DownTimeLogsw.Close();
            DownTimeLogfs.Close();
            CCDMoveLogsw.Close();
            CCDMoveLogfs.Close();
        }


        public void UpdateUserLevel(object sender)
        {
            UserLevel = (int)sender;
            UpdateUserLevelUI(UserLevel);
        }

        public bool JudgeUserLevel(int demandLevel)
        {
            if (UserLevel >= demandLevel)
                return true;
            else
                return false;
        }

        public void UserLogOut()
        {
            UserLevel = 0;
        }

        public void WarningSolution(string WarningStr)
        {
            UpdateWaringLogNG.Invoke((object)(WarningStr));
            WriteLog(WarningStr);
            if (CurWarningStr == null)
                CurWarningStr = WarningStr;
        }

        public void ClearMainAxisWorkPiece()
        {
            int errcode = 0;

            SuckAxisZRetract();

            RetractAllCylinder();

            UnloadGantryAllPistonZRetract();

            HomeMoveZ(0, mainaxishomesafesignal);//主轴回零
            DateTime StartTime = DateTime.Now;
            while (true)
            {
                if (adlink.CheckMoveDone(logicConfig.PulseAxis[0], ref errcode) == true)
                    break;
                else
                {
                    if (!OutTimeCount(StartTime, 20))
                    {
                        WarningSolution("主轴回零失败");
                        return;
                    }
                    Thread.Sleep(30);
                }
            }

            UnloadModulePosMoveRetract();

            UnloadModuleMoveStretch();

            StretchOutAllCylinder();

            adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode);

            SuckAxisZStretch();

            SuckAxisUnloadSuckerSuck();

            SuckAxisZRetract();

            RetractAllCylinder();

            adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisLeftPos, true, ref errcode);

            SuckAxisZStretch();

            SuckAxisUnloadSuckerBreak();

            SuckAxisZRetract();

            UnloadModulePosMoveStretch();

            MainAxisMove(-33333, false);

            StretchOutAllCylinder();

            adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode);

            SuckAxisZStretch();

            SuckAxisUnloadSuckerSuck();

            SuckAxisZRetract();

            RetractAllCylinder();

            adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisLeftPos, true, ref errcode);

            SuckAxisZStretch();

            SuckAxisUnloadSuckerBreak();

            SuckAxisZRetract();

            UnloadModulePosMoveRetract();
            UnloadModuleMoveRetract();

            UnloadGantryMoveToUnloadModule();

            UnloadGantryAllPistonZStretch();

            UnloadGantryAllSuckerSuck();

            UnloadGantryAllPistonZRetract();

            UnloadAllWorkPieceOnRegion(1);
            UnloadGantryZRetract();

            UnloadModuleMoveStretch();

            MainAxisMove(-33333, false);

            StretchOutAllCylinder();

            adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisRightPos, true, ref errcode);

            SuckAxisZStretch();

            SuckAxisUnloadSuckerSuck();

            SuckAxisZRetract();

            RetractAllCylinder();

            adlink.P2PMove(logicConfig.PulseAxis[5], systemParam.SuckAxisLeftPos, true, ref errcode);

            SuckAxisZStretch();

            SuckAxisUnloadSuckerBreak();

            SuckAxisZRetract();

            UnloadModulePosMoveRetract();
            UnloadModuleMoveRetract();

            UnloadGantryMoveToUnloadModule();

            UnloadGantryAllPistonZStretch();

            UnloadGantryAllSuckerSuckWithoutFeedBack();

            UnloadGantryAllPistonZRetract();

            UnloadAllWorkPieceOnRegion(2);
            UnloadGantryZRetract();

        }
    }

    public static class IOControl
    {
        private static MoveControlClass.ECATIOControl ECATIO = new MoveControlClass.ECATIOControl();
        private static MoveControlClass.ADlink7432 Dask7432 = new MoveControlClass.ADlink7432();
        public static void ECATWriteDO(int index, bool value)
        {
            int mod_no = index / 16;
            int chn_no = index % 16;
            ECATIO.WriteSingleDo(1, mod_no, chn_no, value);
        }

        public static void WriteDO(int index, bool value)
        {
            Dask7432.WriteSingleDo(0, index, value);
        }
    }

    public static class WorkCount
    {
        private static string WorkCountPath = "E: \\WorkPiece.txt";
        private static List<string> WorkCountLines;
        private static char separator = ' ';
        public static string isNight;
        public static int WorkPieceCount = 0;

        public static void GetOriginalInfo()
        {
            if (File.Exists(WorkCountPath) == false)
            {
                FileStream tempFS = new FileStream(WorkCountPath, System.IO.FileMode.Create);
                tempFS.Close();
            }

            if (DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 20)
                isNight = "Day";
            else
                isNight = "Night";

            string dateStr = DateTime.Now.Year.ToString() + "/" + ((DateTime.Now.Month) < 10 ? ("0" + DateTime.Now.Month.ToString()) : DateTime.Now.Month.ToString()) + "/" +
                            ((DateTime.Now.Day) < 10 ? ("0" + DateTime.Now.Day.ToString()) : DateTime.Now.Day.ToString());

            WorkCountLines = new List<string>(File.ReadAllLines(WorkCountPath, System.Text.Encoding.Default));
            if (WorkCountLines.Count == 0)
            {
                WorkPieceCount = 0;
                WorkCountLines.Add(dateStr + " " + isNight + " WorkPieceCount= 0");
            }
            else
            {
                string[] info = WorkCountLines[WorkCountLines.Count - 1].Split(separator);
                if ((info[0] == dateStr) && (info[1] == isNight))
                {
                    WorkPieceCount = Convert.ToInt32(info[3]);
                }
                else
                {
                    WorkPieceCount = 0;
                    WorkCountLines.Add(dateStr + " " + isNight + " WorkPieceCount= 0");
                    File.WriteAllLines(WorkCountPath, WorkCountLines.ToArray());
                }
            }
        }

        public static void WriteDownWorkCountInfo()
        {
            string[] info = WorkCountLines[WorkCountLines.Count - 1].Split(separator);
            WorkCountLines[WorkCountLines.Count - 1] = info[0] + " " + info[1] + " " + info[2] + " " + WorkPieceCount.ToString();

            string curTime = (DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 20) ? "Day" : "Night";
            if (curTime != isNight)//如果当前班次和上次记录班次不一致
            {
                WorkPieceCount = 0;
                string dateStr = DateTime.Now.Year.ToString() + "/" + ((DateTime.Now.Month) < 10 ? ("0" + DateTime.Now.Month.ToString()) : DateTime.Now.Month.ToString()) + "/" +
                            ((DateTime.Now.Day) < 10 ? ("0" + DateTime.Now.Day.ToString()) : DateTime.Now.Day.ToString());
                WorkCountLines.Add(dateStr + " " + curTime + " WorkPieceCount= 0");
            }
            File.WriteAllLines(WorkCountPath, WorkCountLines.ToArray());
            isNight = curTime;
        }
    }

    public static class DataOffset
    {
        public static List<LaserRawDataOffset> laserRawDataOffsets = new List<LaserRawDataOffset>();
        public static List<double[]> laserOffset = new List<double[]>();
        public static List<double[]> laserGradient = new List<double[]>();
        public static List<double[]> ccdOffset = new List<double[]>();
        public static List<double[]> ccdGradient = new List<double[]>();
        public static string CCDGradientPath = Directory.GetCurrentDirectory() + @"\MyConfig\CCDGradient.csv";
        public static string CCDOffsetPath = Directory.GetCurrentDirectory() + @"\MyConfig\CCDOffset.csv";
        public static string LaserGradientPath = Directory.GetCurrentDirectory() + @"\MyConfig\LaserGradient.csv";
        public static string LaserOffsetPath = Directory.GetCurrentDirectory() + @"\MyConfig\LaserOffset.csv";


        public static bool ReadLaserRawDataOffset(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                laserRawDataOffsets.Clear();
                StreamReader sr = new StreamReader(fs);

                string currentLine;
                while ((currentLine = sr.ReadLine()) != null)
                {
                    if (currentLine == null)
                    {
                        return false;
                    }

                    char separator = ',';
                    string[] info = currentLine.Split(separator);
                    if (info.Length != 57) return false;

                    LaserRawDataOffset offset = new LaserRawDataOffset();
                    offset.StationIndex = int.Parse(info[0]);
                    if (offset.StationIndex < 1 || offset.StationIndex > 3) return false;
                    offset.NestIndex = int.Parse(info[1]);
                    if (offset.NestIndex < 0 || offset.NestIndex > 3) return false;

                    for (int i = 2; i < info.Length; i++)
                    {
                        offset.OffsetValues[i - 2] = float.Parse(info[i]);
                    }

                    laserRawDataOffsets.Add(offset);
                }

                if (laserRawDataOffsets.Count != 12) return false;

                sr.Close();
                fs.Close();

                return true;
            }
        }

        public static List<double[]> ReadOffset(string path, int faiNum)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                List<double[]> tempdata = new List<double[]>();
                int seq = 0;
                for (int i = 0; i < 12; i++)
                    tempdata.Add(new double[faiNum]);

                StreamReader sr = new StreamReader(fs);

                string currentLine;
                currentLine = sr.ReadLine();
                while ((currentLine = sr.ReadLine()) != null)
                {
                    char separator = ',';
                    string[] info = currentLine.Split(separator);
                    if (info.Length != (faiNum + 2)) return null;

                    for (int i = 0; i < faiNum; i++)
                    {
                        tempdata[seq][i] = Convert.ToDouble(info[i + 2]);
                    }
                    seq++;
                }

                sr.Close();
                fs.Close();
                return tempdata;
            }
        }

        public static bool ReadAllOffset()
        {
            ccdOffset = ReadOffset(CCDOffsetPath, 13);
            if (ccdOffset == null) return false;
            laserOffset = ReadOffset(LaserOffsetPath, 12);
            if (ccdOffset == null) return false;
            ccdGradient = ReadOffset(CCDGradientPath, 13);
            if (ccdOffset == null) return false;
            laserGradient = ReadOffset(LaserGradientPath, 12);
            if (ccdOffset == null) return false;
            return true;
        }


        public static bool WriteOffset(DataGridView mydgv, string path, int demandnum)
        {
            CsvWriter myWriter = new CsvWriter();
            myWriter.WriteCsv(GetDgvToTable(mydgv, demandnum), path);
            return true;
        }

        public static DataTable GetDgvToTable(DataGridView dgv, int demandnum)
        {
            DataTable dt = new DataTable();
            DataColumn dc;
            DataRow dr;
            char separator = ',';

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
                    if (countsub <= 1)
                        dr[countsub] = dgv.Rows[count].Cells[countsub].Value.ToString();
                    else
                        dr[countsub] = dgv.Rows[count].Cells[countsub].Value.ToString().Split(separator)[demandnum];
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }
    }

}
