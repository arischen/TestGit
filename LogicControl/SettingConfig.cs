using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StructAssemble;
using System.Diagnostics;
using System.IO;

namespace LogicControl
{
    //noted by lei.c 用来创建轴对象，并对其进行配置
    public class LogicConfig
    {
        public int boardIdCard = 0;
        public int ECATBoardIdCard = 1;
        public Axis[] PulseAxis = new Axis[8];
        public Axis[] ECATAxis = new Axis[8];
    }

    public struct MovePathPos
    {
        public double Xpos;
        public double Ypos;
        public double Zpos;
    };

    public class MovePathConfig
    {
        public string str3DTask;
        public List<MovePathParam> moveConfig;
    }

    public struct MovePathParam
    {
        public string strPathName;
        public MovePathPos StartPos;
        public string strStartPos;
        public MovePathPos EndPos;
        public string strEndPos;
        public MovePathPos TrigPos;
        public string strTrigPos;
        public double dTrigInterval;
        public int nTrigNum;
        public int nMoveNO;
        public bool bIsATrig;
        public bool bIsBTrig;
        public bool bIsRotate;
        public bool bAffirmFlip;
    };

    public class Task
    {
        public double xStartPos;
        public double yStartPos;
        public double yEndPos;
        public double zTrigSafe;
        public double zNormal;
        public double rStand;
        public bool isSaveData;         //是否保存原始数据
        public bool isSaveCalibData;    //是否保存标定后数据
        // public TaskPar[] taskPar;
    }

    public class DebugLog
    {
        public DebugLog()
        {
            TextWriter m_tracer;
            DateTime now = DateTime.Now;
            string name = now.Year.ToString() + now.Month.ToString("00") + now.Day.ToString("00") + ".txt";
            string path = @".\Log\" + name;
            TextWriterTraceListener objTraceListener;
            if (!File.Exists(path))
            {
                m_tracer = File.CreateText(path);
                objTraceListener = new TextWriterTraceListener(m_tracer);
            }
            else
            {
                objTraceListener = new TextWriterTraceListener(path);
            }

            Trace.Listeners.Add(objTraceListener);
            Trace.AutoFlush = true;
        }

        public void WriteLog(string message)
        {
            string time = ((DateTime.Now.Hour) < 10 ? ("0" + DateTime.Now.Hour.ToString()) : DateTime.Now.Hour.ToString()) + ":" +
                    ((DateTime.Now.Minute) < 10 ? ("0" + DateTime.Now.Minute.ToString()) : DateTime.Now.Minute.ToString()) + ":" +
                    ((DateTime.Now.Second) < 10 ? ("0" + DateTime.Now.Second.ToString()) : DateTime.Now.Second.ToString()) + ":" +
                    DateTime.Now.Millisecond.ToString();
            Trace.WriteLine(time + " : " + message);
            //Trace.WriteLine(message);
        }

    }

    public class Barcodes
    {
        public string barcode1 { get; set; }
        public string barcode2 { get; set; }
        public string barcode3 { get; set; }
        public string barcode4 { get; set; }
        public string barcode5 { get; set; }
        public string barcode6 { get; set; }
        public string barcode7 { get; set; }
        public string barcode8 { get; set; }
        public string barcode9 { get; set; }
        public string barcode10 { get; set; }
        public string barcode11 { get; set; }
        public string barcode12 { get; set; }
    }
}
