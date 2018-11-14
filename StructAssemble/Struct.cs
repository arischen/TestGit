using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonStruct.LCPrim;
using CommonStruct.LC3D;

namespace StructAssemble
{
    public struct Axis
    {
        public short AxisId;  //轴ID
        public double Rate;   //脉冲与实际位置换算比例
        public int Band;      //诊断到位与否的误差脉冲数
        public double HomeVel;
        public double HomeAcc;
        public int HomeVO;
        public double MoveVel;
        public double MoveAcc;
        public double MoveDec;
        public double JogVel;
        public double JogAcc;
        public double JogDec;
        public double TrigVel;
    }
    public struct DiDoStruct
    {
        public string name;
        public int cardId;
        public int portId;
        public int value;
        public int cardType;
        public string remark;
        public DiDoStruct(string strName, int intCardId, int intPortId, int intValue)
        {
            name = strName;
            cardId = intCardId;
            portId = intPortId;
            value = intValue;
            cardType = 0;
            remark = "备注";
        }
    }

    public struct MotionStruct
    {
        public int alm;
        public int pel;
        public int mel;
        public int org;
        public int emg;
        public int ez;
        public int inp;
        public int svon;
        public int dir;
        public int mdn;
        public int hmv;
        public int jog;
    }
    
    public struct UpdateInfo
    {
        public MotionStruct[] motionIO;
        public double[] CurAxisPos;
        public bool[] Di;
        public bool[] Do;
        public UpdateInfo(int axisNums,int DiNums,int DoNums)
        {
            motionIO = new MotionStruct[axisNums];
            CurAxisPos = new double[axisNums];
            Di = new bool[DiNums];
            Do = new bool[DoNums];
        }
    }
    
    public struct PosInfo
    {
        public string PosName;
        public double XPos;
        public double YPos;
    }

    public struct MotionPos
    {
        public PosInfo[] posInfo;
        public MotionPos(int posNums)
        {
            posInfo = new PosInfo[posNums];
        }
    }

    public struct ThresholdInfo
    {
        public string ThrName;
        public double UpLimit;
        public double DownLimit;
    }

    public struct ThresholdParam
    {
        public ThresholdInfo[] thrInfo;
        public ThresholdParam(int thrNums)
        {
            thrInfo = new ThresholdInfo[thrNums];
        }
    }

    public struct SystemParam
    {
        public int IgnoreDoor;//忽视安全门检测
        public int IgnoreCamera;//忽视CCD
        public int IgnoreLaser;//忽视激光
        public int WorkPieceNum;
        public int AxisNum;
        public int OutTime;

        public double LoadTrayDistance;//每次切换Tray盘轴上升/下降的距离
        public double LoadTrayDistanceSeg;
        public double LoadFullTrayFinishDistance;
        public double UnloadNullTrayFinishDistance;
        public double LoadFullTrayUpDistance;//New LoadTraySwitch
        public double UnloadFullFinishDistance;
        public int LoadGantrySuckDelay;//上料龙门吸取延时（单位ms）
        public int UnloadGantrySuckDelay;//下料龙门吸取延时（单位ms）
        public int SuckAxisSuckDelay;//横移轴吸取延时（单位ms）
        public int TrayZMoveMaxCount;

        public double LoadTrayFinishPosX;//龙门上料放置工件的位置X(上料模组等待位)
        public double LoadTrayFinishPosY;//龙门上料放置工件的位置Y(上料模组等待位)

        public double UnloadTrayFinishPosX;//龙门下料放置工件的位置X(下料模组等待位)
        public double UnloadTrayFinishPosY;//龙门下料放置工件的位置Y(下料模组等待位)
        
        public double UnloadSupplyRegion1PosX;//龙门下料放置所有工件到补料盘区域1的位置X
        public double UnloadSupplyRegion1PosY;//龙门下料放置所有工件到补料盘区域1的位置Y
        public double UnloadSupplyRegion2PosX;//龙门下料放置所有工件到补料盘区域2的位置X
        public double UnloadSupplyRegion2PosY;//龙门下料放置所有工件到补料盘区域2的位置Y

        //放置NG料的位置，分为ABC三个级别
        public double UnloadTrayNGAPosX;
        public double UnloadTrayNGAPosY;
        public double UnloadTrayNGBPosX;
        public double UnloadTrayNGBPosY;
        public double UnloadTrayNGCPosX;
        public double UnloadTrayNGCPosY;

        public double LoadTrayAvoidPosX;
        public double LoadTrayAvoidPosY;
        public double UnloadTrayAvoidPosX;
        public double UnloadTrayAvoidPosY;

        public int NGDrawerAlmNum;//NG料盒中工件数目警告数量

        public double SuckAxisLeftPos;//横移轴左取放位
        public double SuckAxisRightPos;//横移轴右取放位

        public double TraySwitchLoadFullRollSensePos;       //上料满Tray盘翻转感应位
        public double TraySwitchLoadFullDownLimit;          //上料满Tray盘运动最低点
        public double TraySwitchLoadFullUpLimit;            //上料满Tray盘运动最高点(只要确保抽屉拉出无阻碍即可)
        public double TraySwitchLoadNullDownLimit;          //上料空Tray盘运动最低点
        public double TraySwitchLoadNullRollSensePos;       //上料空Tray盘翻转感应位
        public double TraySwitchLoadNullUpLimit;            //上料空Tray盘运动最高点

        public double TraySwitchUnloadFullRollSensePos;     //下料满Tray盘翻转感应位
        public double TraySwitchUnloadFullUpLimit;          //下料满Tray盘运动最高点
        public double TraySwitchUnloadFullDownLimit;        //下料满Tray盘运动最低点
        public double TraySwitchUnloadNullRollSensePos;     //下料空Tray盘翻转感应位
        public double TraySwitchUnloadNullDownLimit;        //下料空Tray盘运动最低点
        public double TraySwitchUnloadNullUpLimit;          //下料空Tray盘运动最高点(只要确保抽屉拉出无阻碍即可)

        public int TrayMaxNum;               //一次性可以放置的Tray盘数目
        public int TrayAlmNum;               //Tray盘警告数目，到达这个数目后，可以解开锁扣更换Tray盘
        public int ProductionRecordHourBeat;     //生产数据量统计节拍
        public int  isCheckInPutProductionRecordHourBeat;//是否检查输入产品批次

        public int LoadGantrySuckerBreakDelay;
        public int UnloadGantrySuckerBreakDelay;
        public int SuckAxisSuckerBreakDelay;

    }

    public enum STATUS
    {
        READY_STATUS = 0,
        STOP_STATUS = 1,
        AUTO_STATUS = 2,
        PAUSE_STATUS = 3,
        MANUAL_STATUS = 4,
    }

    //对于第二张IO卡，需要判断值是否大于31。若大于31，则需要换为另一个CardID
    public enum DINAME
    {
        Di_StartBtn = 0,
        Di_StopBtn = 1,
        Di_ResetBtn = 2,
        Di_SwitchBtn = 3,
        Di_EmgStopBtn = 4,
        Di_AirPressure = 5,
        Di_CarrierSense = 6,
        Di_EnterPushUp1StretchBit = 7,
        Di_EnterPushUp1RetractBit = 8,
        Di_EnterStirStretchBit = 9,
        Di_EnterStirRetractBit = 10,
        Di_EnterMoveStretchBit = 11,
        Di_EnterMoveRetractBit = 12,
        Di_EnterPushUp2StretchBit = 13,
        Di_EnterPushUp2RetractBit = 14,
        Di_Door1Check = 15,
        Di_Door2Check = 16,
        Di_Door3Check = 17,
        Di_Door4Check = 18,
        Di_Door5Check = 19,
        Di_Door6Check = 20,
        Di_Door7Check = 21,
        Di_Door8Check = 22,
        Di_Door9Check = 23,
        Di_Door10Check = 24,
    }

    public enum DONAME
    {
        Do_LightRed = 0,
        Do_LightYellow = 1,
        Do_LightGreen = 2,
        Do_Buzzer = 3,
        Do_ServoReset = 4,
        Do_StartBtnShow = 5,
        Do_StopBtnShow = 6,
        Do_ResetBtnShow = 7,
        Do_EnterPushUpStretchControl = 8,
        Do_EnterPushUpRetractControl = 9,
        Do_EnterStirStretchControl = 10,
        Do_EnterStirRetractControl = 11,
        Do_EnterMoveStretchControl = 12,
        Do_EnterMoveRetractControl = 13,
        Do_LedLight = 14,
    }

    public enum ECATDINAME
    {
        Di_LoadZStretchBit=0,
        Di_LoadZRetractBit=1,
        Di_LoadBufferLeftStretchBit=2,
        Di_LoadBufferLeftRetractBit=3,
        Di_LoadBufferRightStretchBit=4,
        Di_LoadBufferRightRetractBit = 5,
        Di_LoadLeftVacumCheck = 6,
        Di_LoadRightVacumCheck = 7,
        Di_UnloadZStretchBit = 8,
        Di_UnloadZRetractBit = 9,
        Di_UnloadBufferLeft1StretchBit = 10,
        Di_UnloadBufferLeft1RetractBit = 11,
        Di_UnloadBufferLeft2StretchBit = 12,
        Di_UnloadBufferLeft2RetractBit = 13,
        Di_UnloadBufferLeft3StretchBit = 14,
        Di_UnloadBufferLeft3RetractBit = 15,

        Di_UnloadBufferLeft4StretchBit = 16,
        Di_UnloadBufferLeft4RetractBit = 17,
        Di_UnloadBufferRight1StretchBit = 18,
        Di_UnloadBufferRight1RetractBit = 19,
        Di_UnloadBufferRight2StretchBit = 20,
        Di_UnloadBufferRight2RetractBit = 21,
        Di_UnloadBufferRight3StretchBit = 22,
        Di_UnloadBufferRight3RetractBit = 23,
        Di_UnloadBufferRight4StretchBit = 24,
        Di_UnloadBufferRight4RetractBit = 25,
        Di_UnloadLeft1VacumCheck = 26,
        Di_UnloadLeft2VacumCheck = 27,
        Di_UnloadLeft3VacumCheck = 28,
        Di_UnloadLeft4VacumCheck = 29,
        Di_UnloadRight1VacumCheck = 30,
        Di_UnloadRight2VacumCheck = 31,

        Di_UnloadRight3VacumCheck = 32,
        Di_UnloadRight4VacumCheck = 33,
        Di_LoadMoveStretchBit=34,
        Di_LoadMoveRetractBit=35,
        Di_LoadMotionMoveStretchBit = 36,
        Di_LoadMotionMoveRetractBit = 37,
        Di_SuckZStretchBit = 38,
        Di_SuckZRetractBit = 39,
        Di_LoadVacumCheck = 40,
        Di_SuckLoadVacumCheck = 41,
        Di_SuckUnloadVacumCheck = 42,
        Di_UnloadMoveStretchBit = 43,
        Di_UnloadMoveRetractBit = 44,
        Di_UnloadMotionMoveStretchBit=45,
        Di_UnloadMotionMoveRetractBit = 46,
        Di_UnloadVacumCheck=47,

        Di_LoadTrayMoveStretchBit=48,
        Di_LoadTrayMoveRetractBit=49,
        Di_LoadTraySuckStretchBit=50,
        Di_LoadTraySuckRetractBit=51,
        Di_LoadNullDrawerInsideSense=52,
        Di_LoadNullDrawerOutsideSense = 53,
        Di_LoadFullDrawerInsideSense = 54,
        Di_LoadFullDrawerOutsideSense = 55,
        Di_LoadNullDrawerInPosition = 56,
        Di_LoadFullDrawerInPosition = 57,
        Di_UnloadTrayMoveStretchBit=58,
        Di_UnloadTrayMoveRetractBit=59,
        Di_UnloadTraySuckStretchBit=60,
        Di_UnloadTraySuckRetractBit=61,
        Di_LoadTrayVacumCheck=62,
        Di_UnloadTrayVacumCheck=63,

        Di_UnloadNullDrawerInsideSense = 64,
        Di_UnloadNullDrawerOutsideSense = 65,
        Di_UnloadFullDrawerInsideSense = 66,
        Di_UnloadFullDrawerOutsideSense = 67,
        Di_UnloadNullDrawerTrayInPosition = 68,
        Di_UnloadFullDrawerTrayInPosition = 69,
        Di_LoadNullTrayInPosition=70,
        Di_LoadFullTrayInPosition=71,
        Di_UnloadNullTrayInPosition=72,
        Di_UnloadFullTrayInPosition=73,
        Di_NGDrawerInPosition=74,

        Di_LoadNullTrayInside1StretchBit=80,
        Di_LoadNullTrayInside1RetractBit=81,
        Di_LoadNullTrayInside2StretchBit = 82,
        Di_LoadNullTrayInside2RetractBit = 83,
        Di_LoadNullTrayOutsideStretchBit = 84,
        Di_LoadNullTrayOutsideRetractBit = 85,
        Di_LoadNullTrayUpsideStretchBit = 86,
        Di_LoadNullTrayUpsideRetractBit = 87,
        Di_LoadFullTrayInside1StretchBit = 88,
        Di_LoadFullTrayInside1RetractBit = 89,
        Di_LoadFullTrayInside2StretchBit = 90,
        Di_LoadFullTrayInside2RetractBit = 91,
        Di_LoadFullTrayOutsideStretchBit = 92,
        Di_LoadFullTrayOutsideRetractBit = 93,
        Di_LoadFullTrayUpsideStretchBit = 94,
        Di_LoadFullTrayUpsideRetractBit = 95,

        Di_UnloadNullTrayInside1StretchBit = 96,
        Di_UnloadNullTrayInside1RetractBit = 97,
        Di_UnloadNullTrayInside2StretchBit = 98,
        Di_UnloadNullTrayInside2RetractBit = 99,
        Di_UnloadNullTrayOutsideStretchBit = 100,
        Di_UnloadNullTrayOutsideRetractBit = 101,
        Di_UnloadNullTrayUpsideStretchBit = 102,
        Di_UnloadNullTrayUpsideRetractBit = 103,
        Di_UnloadFullTrayInside1StretchBit = 104,
        Di_UnloadFullTrayInside1RetractBit = 105,
        Di_UnloadFullTrayInside2StretchBit = 106,
        Di_UnloadFullTrayInside2RetractBit = 107,
        Di_UnloadFullTrayOutsideStretchBit = 108,
        Di_UnloadFullTrayOutsideRetractBit = 109,
        Di_UnloadFullTrayUpsideStretchBit = 110,
        Di_UnloadFullTrayUpsideRetractBit = 111,
    }

    public enum ECATDONAME
    {
        Do_LoadZStretchControl = 0,
        Do_LoadZRetractControl = 1,
        Do_LoadBufferLeftStretchControl = 2,
        Do_LoadBufferLeftRetractControl = 3,
        Do_LoadBufferRightStretchControl = 4,
        Do_LoadBufferRightRetractControl = 5,
        Do_LoadLeftVacumSuck = 6,
        Do_LoadLeftVacumBreak = 7,
        Do_LoadRightVacumSuck = 8,
        Do_LoadRightVacumBreak = 9,
        Do_UnloadZStretchControl = 10,
        Do_UnloadZRetractControl = 11,
        Do_UnloadBufferLeft1StretchControl = 12,
        Do_UnloadBufferLeft1RetractControl = 13,
        Do_UnloadBufferLeft2StretchControl = 14,
        Do_UnloadBufferLeft2RetractControl = 15,

        Do_UnloadBufferLeft3StretchControl = 16,
        Do_UnloadBufferLeft3RetractControl = 17,
        Do_UnloadBufferLeft4StretchControl = 18,
        Do_UnloadBufferLeft4RetractControl = 19,
        Do_UnloadBufferRight1StretchControl = 20,
        Do_UnloadBufferRight1RetractControl = 21,
        Do_UnloadBufferRight2StretchControl = 22,
        Do_UnloadBufferRight2RetractControl = 23,
        Do_UnloadBufferRight3StretchControl = 24,
        Do_UnloadBufferRight3RetractControl = 25,
        Do_UnloadBufferRight4StretchControl = 26,
        Do_UnloadBufferRight4RetractControl = 27,
        Do_UnloadLeft1VacumSuck = 28,
        Do_UnloadLeft1VacumBreak = 29,
        Do_UnloadLeft2VacumSuck = 30,
        Do_UnloadLeft2VacumBreak = 31,

        Do_UnloadLeft3VacumSuck = 32,
        Do_UnloadLeft3VacumBreak = 33,
        Do_UnloadLeft4VacumSuck = 34,
        Do_UnloadLeft4VacumBreak = 35,
        Do_UnloadRight1VacumSuck = 36,
        Do_UnloadRight1VacumBreak = 37,
        Do_UnloadRight2VacumSuck = 38,
        Do_UnloadRight2VacumBreak = 39,
        Do_UnloadRight3VacumSuck = 40,
        Do_UnloadRight3VacumBreak = 41,
        Do_UnloadRight4VacumSuck = 42,
        Do_UnloadRight4VacumBreak = 43,
        Do_LoadMoveStretchControl = 44,
        Do_LoadMoveRetractControl = 45,
        Do_LoadMoveCylinder = 46,
        Do_UnloadMoveCylinder = 47,

        Do_SuckZStretchControl = 48,
        Do_SuckZRetractControl = 49,
        Do_LoadVacumSuck = 50,
        Do_LoadVacumBreak = 51,
        Do_SuckLoadVacumSuck = 52,
        Do_SuckLoadVacumBreak = 53,
        Do_SuckUnloadVacumSuck = 54,
        Do_SuckUnloadVacumBreak = 55,
        Do_UnloadMoveStretchControl = 56,
        Do_UnloadMoveRetractControl = 57,
        Do_UnloadVacumSuck=58,
        Do_UnloadVacumBreak=59,
        Do_LoadTrayMoveStretchControl=60,
        Do_LoadTrayMoveRetractControl=61,
        Do_LoadTraySuckStretchControl=62,
        Do_LoadTraySuckRetractControl = 63,

        Do_UnloadTrayMoveStretchControl = 64,
        Do_UnloadTrayMoveRetractControl = 65,
        Do_UnloadTraySuckStretchControl = 66,
        Do_UnloadTraySuckRetractControl = 67,
        Do_LoadNullDrawerLock = 68,
        Do_LoadFullDrawerLock = 69,
        Do_UnloadNullDrawerLockControl = 70,
        Do_UnloadFullDrawerLockControl = 71,
        Do_BlowControl=72,
        Do_NGDrawerLockControl = 73,
        Do_LoadTrayVacumSuck = 74,
        Do_LoadTrayVacumBreak = 75,
        Do_UnloadTrayVacumSuck = 76,
        Do_UnloadTrayVacumBreak = 77,
        Do_NGDrawerUnlockControl = 78,

        Do_LoadNullTrayStretchBit = 80,
        Do_LoadNullTrayRetractBit = 81,
        Do_LoadFullTrayStretchBit = 82,
        Do_LoadFullTrayRetractBit = 83,
        Do_UnloadNullTrayStretchBit = 84,
        Do_UnloadNullTrayRetractBit = 85,
        Do_UnloadFullTrayStretchBit = 86,
        Do_UnloadFullTrayRetractBit = 87,
        Do_CCDUpLightCmd = 89,
        Do_CCDDownLightCmd = 90,       
    }

    public enum AutoRunPartAStep
    {
        StretchCylinder = 0,
        WaitSuckAndPutTool = 1,
        RetractCylinder = 2,
    }
    //Added by lei.c
    public enum AutoRunPartBStep
    {
        WaitCCDHome = -1,
        CameraCheck1 = 0,
        CameraCheck2 = 1,
        CameraCheck3 = 2,
        CameraCheck4 = 3,
        StartCCDHome = 4,
    }
    //Added by lei.c
    public enum AutoRunPartCStep
    {
        WaitLaserHome = -1,
        LaserCheck1 = 0,
        LaserCheck2 = 1,
        LaserCheck3 = 2,
        LaserCheck4 = 3,
        StartLaserHome = 4,
    }
    //Added by lei.c
    public enum AutoRunPartDStep
    {
        StretchCylinderD = 0,
        UpdateResult = 1,
        SuckerMotion = 2,
        RetractCylinderD =3,
    }

    public enum InitStep
    {
        TraySwitchDO = 0,
        GantryDO = 1,
        SuckAxisDO = 2,
        CarrierRetract = 3,
        TraySeparateRetract = 4,
        LoadAndUnloadModuleDO = 5,
        GantryAxisHome = 6,

        TrayZAxisCheck = 7,

        TrayZAxisHome = 8,
        PulseAxisHome = 9,
        ResetCCD = 10,
        ResetLaser = 11,
    }


    public enum SystemResetStep
    {
        AxisStop,
        SuckerMotion,
        StepStop,
        InitEnvironment,
        DataReset,
    }
   
    public struct Point2D
    {
        public double X;
        public double Y;
        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public struct PointList
    {
        public Point2D[] point;
        public Point2D[] RefPoint;
        public Point2D[] CurPoint;
        public PointList(int Nums)
        {
            point = new Point2D[Nums];
            RefPoint = new Point2D[Nums];
            CurPoint = new Point2D[Nums];
        }
    }

    public class CalibMatrix
    {
        public double a11;
        public double a12;
        public double a13;
        public double a21;
        public double a22;
        public double a23;
        public double a31;
        public double a32;
        public double a33;

        public double a11back;
        public double a12back;
        public double a13back;
        public double a21back;
        public double a22back;
        public double a23back;
        public double a31back;
        public double a32back;
        public double a33back;
    }

    public class Calib3D
    {
        public Calib3DSturct[] calib3DStd;
    }

    public struct Calib3DSturct
    {
        public int PathId;  //第几个标定路径
        public double a11;
        public double a12;
        public double a13;
        public double a14;
        public double a21;
        public double a22;
        public double a23;
        public double a24;
        public double a31;
        public double a32;
        public double a33;
        public double a34;
        public double a41;
        public double a42;
        public double a43;
        public double a44;
    }

    public struct CCDUpdateStruct
    {
        public string dataNo;
        public string holeNo;
        public string TrayNo;
        public double exist;
        public double fai130;
        public double fai131;
        public double fai133G1;
        public double fai133G2;
        public double fai133G3;
        public double fai133G4;
        public double fai133G6;
        public double fai161;
        public double fai161afromccd;
        public double fai161bfromccd;
        public double fai162;
        public double fai163;
        public double fai165;
        public double fai171;
        public double fai171a;
        public double fai171b;
        public double fai165fromAbove;
        public double fai163fromAbove;
        public double fai162fromAbove;
        public double fai163fromccd;
        public double fai22;
        //public double fai161fromTT;
    }


    public struct LaserFAIUpdateStruct
    {
        public string dataNo;
        public string holeNo;
        public string TrayNo;
        public double fai135;
        public double fai136;
        public double fai139;
        public double fai140;
        public double fai151;
        public double fai152;
        public double fai155;
        public double fai156;
        public double fai157;
        public double fai158;
        public double fai160;
        public double fai172;
    }

    public struct LaserUpdateStruct
    {
        public string DataNo;
        public string HoleNo;
        public string TrayNo;
        public List<FaiValue> LaserFai;
    }
    public struct LaserTaskRun
    {
        public List<XDPOINT[,]> laserData;
        public int datano;
        public int step;
        public int trayno;
        public bool isdebugmode;

        public LaserTaskRun(List<XDPOINT[,]> myLaserData, int myDataNo, int myStep, int myTrayNo, bool mydebugmode)
        {
            laserData = myLaserData;
            datano = myDataNo;
            step = myStep;
            trayno = myTrayNo;
            isdebugmode = mydebugmode;
        }
    }

    public struct CCDTaskRun
    {
        public int ProductSeq;
        public int StepB;
        public int PartBTrayNo;
        public bool isDebugMode;
        public Point2D[] PicPos;

        public CCDTaskRun(int myProSeq, int myStepB,int myPartBTrayNo,bool myDebugMode,Point2D[] myPos)
        {
            ProductSeq = myProSeq;
            StepB = myStepB;
            PartBTrayNo = myPartBTrayNo;
            isDebugMode = myDebugMode;
            PicPos = new Point2D[myPos.Length];
            for (int i = 0; i < myPos.Length; i++)
            {
                PicPos[i].X = myPos[i].X;
                PicPos[i].Y = myPos[i].Y;
            }
        }

    }

    public struct StationCheckPara
    {
        public bool LaserCheck;
        public bool CCDCheck;
        public bool[] CCDHoleCheck;
        public bool[] LaserHoleCheck;
        public StationCheckPara(int num)
        {
            LaserCheck = true;
            CCDCheck = true;
            CCDHoleCheck = new bool[num];
            LaserHoleCheck = new bool[num];

            for (int i = 0; i < num; i++)
            {
                CCDHoleCheck[i] = true;
                LaserHoleCheck[i] = true;
            }
        }
    }

    public struct CCDFinalResultUpdateStru
    {
        public int CircleCount;
        public int TrayNo;
        public int[] CCDFinalResult;
        public CCDFinalResultUpdateStru(int myCircleCount,int myTrayNo,int[] myFinalResult)
        {
            CCDFinalResult = new int[myFinalResult.Length];

            CircleCount = myCircleCount;
            TrayNo = myTrayNo;
            for (int i = 0; i <myFinalResult.Length; i++)
            {
                CCDFinalResult[i] = myFinalResult[i];
            }
        }
    }

    public struct FinalResultUpdateStru
    {
        public int CircleCount;
        public int TrayNo;
        public int[] FinalResult;
        public FinalResultUpdateStru(int myCircleCount, int myTrayNo, int[] myFinalResult)
        {
            FinalResult = new int[myFinalResult.Length];

            CircleCount = myCircleCount;
            TrayNo = myTrayNo;
            for (int i = 0; i < myFinalResult.Length; i++)
            {
                FinalResult[i] = myFinalResult[i];
            }
        }
    }


    public struct PassRatio
    {
        public int TotalNum;
        public int PassNum;
        public double Ratio;
        public int ANum;
        public int BNum;
        public int CNum;
        public int DNum;
        public int ENum;
        public int DropNum;

        public PassRatio(int total, int pass, double ratio,int aNum,int bNum, int cNum, int dNum, int eNum, int dropNum)
        {
            TotalNum = total;
            PassNum = pass;
            Ratio = ratio;
            ANum = aNum;
            BNum = bNum;
            CNum = cNum;
            DNum = dNum;
            ENum = eNum;
            DropNum = dropNum;
        }
    }

    public struct FinalResultStru
    {
        public int[] FinalResult;

        public FinalResultStru(int[] myResult)
        {
            FinalResult = new int[myResult.Length];
            for (int i = 0; i < myResult.Length; i++)
            {
                FinalResult[i] = myResult[i];
            }
        }
    }

    public class Line
    {
        public double a;
        public double b;
        public double c;
        public Line()
        {
            a = 0;
            b = 0;
            c = 0;
        }
        public Line(double _a, double _b, double _c)
        {
            a = _a;
            b = _b;
            c = _c;
        }
    }

    public struct RepeatTestStru
    {
        public int stationNo;
        public int id;
        public int repeattime;
        public RepeatTestStru(int myStationNo, int myid, int myRepeatTime)
        {
            stationNo = myStationNo;
            id = myid;
            repeattime = myRepeatTime;
        }
    }

    public struct CTInfoStruct
    {
        public double averagect;
        public double lastct;
        public double lastCCDScanTime;
        public double lastLaserScanTime;
        public CTInfoStruct(double myaverage, double mylast,double ccdscantime,double laserscantime)
        {
            averagect = myaverage;
            lastct = mylast;
            lastCCDScanTime = ccdscantime;
            lastLaserScanTime = laserscantime;
        }
    }

    public class LaserRawDataOffset
    {
        public int StationIndex;
        public int NestIndex;
        public double[] OffsetValues;

        public LaserRawDataOffset()
        {
            OffsetValues = new double[55];
        }
    }


    public class CCDDataOffset
    {
        public List<double[]> ccdGradient;
        public List<double[]> ccdOffset;

        public CCDDataOffset(List<double[]> myGradient,List<double[]> myOffset)
        {
            ccdGradient = new List<double[]>();
            ccdOffset = new List<double[]>();
            for (int i = 0; i < myGradient.Count; i++)
            {
                ccdGradient.Add(new double[13]);
                ccdOffset.Add(new double[13]);
            }

            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    ccdGradient[i][j] = myGradient[i][j];
                    ccdOffset[i][j] = myOffset[i][j];
                }
            }
        }
    }

    public class LaserDataOffset
    {
        public List<double[]> laserGradient;
        public List<double[]> laserOffset;

        public LaserDataOffset(List<double[]> myGradient, List<double[]> myOffset)
        {
            laserGradient = new List<double[]>();
            laserOffset = new List<double[]>();
            for (int i = 0; i < myGradient.Count; i++)
            {
                laserGradient.Add(new double[12]);
                laserOffset.Add(new double[12]);
            }

            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    laserGradient[i][j] = myGradient[i][j];
                    laserOffset[i][j] = myOffset[i][j];
                }
            }
        }
    }

    public struct PieceAllData
    {
        public int TrayNo;
        public int HoleNo;
        public int Level;
        public double exist;
        public double fai22;
        public double fai130;
        public double fai131;
        public double fai133G1;
        public double fai133G2;
        public double fai133G3;
        public double fai133G4;
        public double fai133G6;
        public double fai161;
        public double fai162;
        public double fai163;
        public double fai165;
        public double fai171;
        public double fai135;
        public double fai136;
        public double fai139;
        public double fai140;
        public double fai151;
        public double fai152;
        public double fai155;
        public double fai156;
        public double fai157;
        public double fai158;
        public double fai160;
        public double fai172;
    }

    public struct FaiInfo
    {
        public double FaiMean;
        public int FaiPassNum;
        public int FaiTotalNum;
        public int FaiNGNum;

        public FaiInfo(double myMean, int myPassNum,int myTotalNum,int myNGNum)
        {
            FaiMean = myMean;
            FaiPassNum = myPassNum;
            FaiTotalNum = myTotalNum;
            FaiNGNum = myNGNum;
        }
    }
    // 最终数据  樊竞明20181009
    public class  FinalDataSummery
    {
        public FaiInfo[] myFaiInfoSummary = new FaiInfo[25];
        public PassRatio myPassratioSummary = new PassRatio(0,0,0,0,0,0,0,0,0);//

        public FinalDataSummery()
        {
            for (int i = 0; i < myFaiInfoSummary.Length;i++)
            {
                myFaiInfoSummary[i] = new FaiInfo (0, 0, 0, 0);
            }
        }
    }
    public struct DownTimeRecordStru
    {
        public int CurStatus;
        public DateTime StartTime;
        public DateTime FinishTime;
        public TimeSpan DownTimeSpan;
        public string ErrorStr;
    }

    public struct TrueCTCalcStru
    {
        public DateTime curTime;
        public int CheckNum;
        public TrueCTCalcStru(DateTime mytime,int myNum)
        {
            curTime = mytime;
            CheckNum = myNum;
        }
    }

    public struct WorkPieceInfo
    {
        public string WorkClassify;
        public int WorkPieceNum;
        public WorkPieceInfo(string mywork, int piecenum)
        {
            WorkClassify = mywork;
            WorkPieceNum = piecenum;
        }
    }

}
