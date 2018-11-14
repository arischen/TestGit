using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;//Marshal
using CommonStruct.LC3D;
using System.Xml.Serialization;
using CsvHelper;

namespace KeyenceLJ
{
    public class KeyenceControl
    {
        LJV7IF_ETHERNET_CONFIG ethernetConfig;
        #region Enum

        /// <summary>
        /// Send command definition
        /// </summary>
        /// <remark>Defined for separate return code distinction</remark>
        public enum SendCommand
        {
            /// <summary>None</summary>
            None,
            /// <summary>Restart</summary>
            RebootController,
            /// <summary>Trigger</summary>
            Trigger,
            /// <summary>Start measurement</summary>
            StartMeasure,
            /// <summary>Stop measurement</summary>
            StopMeasure,
            /// <summary>Auto zero</summary>
            AutoZero,
            /// <summary>Timing</summary>
            Timing,
            /// <summary>Reset</summary>
            Reset,
            /// <summary>Program switch</summary>
            ChangeActiveProgram,
            /// <summary>Get measurement results</summary>
            GetMeasurementValue,

            /// <summary>Get profiles</summary>
            GetProfile,
            /// <summary>Get batch profiles (operation mode "high-speed (profile only)")</summary>
            GetBatchProfile,
            /// <summary>Get profiles (operation mode "advanced (with OUT measurement)")</summary>
            GetProfileAdvance,
            /// <summary>Get batch profiles (operation mode "advanced (with OUT measurement)").</summary>
            GetBatchProfileAdvance,

            /// <summary>Start storage</summary>
            StartStorage,
            /// <summary>Stop storage</summary>
            StopStorage,
            /// <summary>Get storage status</summary>
            GetStorageStatus,
            /// <summary>Manual storage request</summary>
            RequestStorage,
            /// <summary>Get storage data</summary>
            GetStorageData,
            /// <summary>Get profile storage data</summary>
            GetStorageProfile,
            /// <summary>Get batch profile storage data.</summary>
            GetStorageBatchProfile,

            /// <summary>Initialize USB high-speed data communication</summary>
            HighSpeedDataUsbCommunicationInitalize,
            /// <summary>Initialize Ethernet high-speed data communication</summary>
            HighSpeedDataEthernetCommunicationInitalize,
            /// <summary>Request preparation before starting high-speed data communication</summary>
            PreStartHighSpeedDataCommunication,
            /// <summary>Start high-speed data communication</summary>
            StartHighSpeedDataCommunication,
        }

        #endregion
        public SendCommand sendCommand;
        private DeviceData[] deviceData;
        List<ProfileData> profileDatas2 = new List<ProfileData>();//2A批处理数据
        List<XDPOINT[,]> laserAllData = new List<XDPOINT[,]>();//发送3D的数据
        Dictionary<string, XDPOINT[,]> Laser2ADicData = new Dictionary<string, XDPOINT[,]>();//Laser2ADicdata 缓存
        public string LaserDataPath = Directory.GetCurrentDirectory() + @"\3DLaserdata\";//3DLaserData保存数据路径

        List<int[]> datalaserA = new List<int[]>();
        List<int[]> datalaserB = new List<int[]>();
        List<int[]> datalaser2A = new List<int[]>();

        //static List<int[]> receiveBuffer = new List<int[]>();
        object lockinit = new object();
        private HighSpeedDataCallBack CallbackGetData;
        private HighSpeedDataCallBack CallbackGetData2;
        public struct FixXDPara
        {
            public int nNO;
            public double Xpos;
            public double Ypos;
            public double YInterval;
        }
        public List<int[]> DatalaserA
        {
            get
            {
                return datalaserA;
            }
        }

        public List<int[]> DatalaserB
        {
            get
            {
                return datalaserB;
            }
        }
        public List<int[]> Datalaser2A
        {
            get
            {
                return datalaser2A;
            }
        }
        public KeyenceControl()
        {
            #region//LJ初始化参数
            sendCommand = SendCommand.None;
            deviceData = new DeviceData[NativeMethods.DeviceCount];
           // measureDatas = new List<MeasureData>();

            for (int i = 0; i < NativeMethods.DeviceCount; i++)
            {
                deviceData[i] = new DeviceData();
            }
            #endregion
        }
        private void CommonErrorLog(int rc,out string errorCode)
        {
            errorCode = "";
            switch (rc)
            {
                case (int)Rc.Ok:
                    errorCode ="-> Normal termination";
                    break;
                case (int)Rc.ErrOpenDevice:
                    errorCode= "-> Failed to open the device";
                    break;
                case (int)Rc.ErrNoDevice:
                    errorCode = "-> Device not open";
                    break;
                case (int)Rc.ErrSend:
                    errorCode = "-> Command send error";
                    break;
                case (int)Rc.ErrReceive:
                    errorCode = "-> Response reception error";
                    break;
                case (int)Rc.ErrTimeout:
                    errorCode = "-> Time out";
                    break;
                case (int)Rc.ErrParameter:
                    errorCode = "-> Parameter error";
                    break;
                case (int)Rc.ErrNomemory:
                    errorCode = "-> No free space";
                    break;
                default:
                    errorCode = string.Format("＃Undefined RC(0x{ 0,0:X4})", rc);
                    break;
            }
        }
        public bool Init(out string errorCode)
        {
            
            errorCode = "";
            int rc = NativeMethods.LJV7IF_Initialize();
            if (rc != (int)Rc.Ok)
            {
                errorCode = string.Format("KJ[{0}] : {1}(0x{2:x4})", "Initialize", "NG", rc);
                if (rc < 0x8000)
                {
                    // Common return code
                    string errorCode2 = "";
                    CommonErrorLog(rc,out errorCode2);
                    errorCode = errorCode + errorCode2;
                }
                return false;
             }
            for (int i = 0; i < deviceData.Length; i++)
            {
                deviceData[i].Status = DeviceStatus.NoConnection;
            }

            CallbackGetData = new HighSpeedDataCallBack(GetDataThread);
            //CallbackGetData2 = new HighSpeedDataCallBack(GetDataThread2);
            return true;
        }
        public bool EthernetOpen(int nCurrentDeviceId, string ipIn, int portIn, out string errorCode)
        {
            //初始化该端口
            errorCode = "";
            deviceData[nCurrentDeviceId].Status = DeviceStatus.NoConnection;
            string[] IP = ipIn.Split('.');
            if (IP.Count() < 4)
            {
                errorCode = "IP格式不正确";
                return false;
            }
            IP[0]= IP[0].Trim();
            IP[1]= IP[1].Trim();
            IP[2] = IP[2].Trim();
            IP[3] = IP[3].Trim();
            ethernetConfig.abyIpAddress = new byte[]
             {
                 Convert.ToByte(IP[0]),
                 Convert.ToByte(IP[1]),
                 Convert.ToByte(IP[2]),
                 Convert.ToByte(IP[3])
              };
            ethernetConfig.wPortNo = Convert.ToUInt16(portIn);
            int rc = NativeMethods.LJV7IF_EthernetOpen(nCurrentDeviceId, ref ethernetConfig);
            if (rc == (int)Rc.Ok)
            {
                deviceData[nCurrentDeviceId].Status = DeviceStatus.Ethernet;
                deviceData[nCurrentDeviceId].EthernetConfig = ethernetConfig;
                return true;
            }
            else
            {
                deviceData[nCurrentDeviceId].Status = DeviceStatus.NoConnection;
                errorCode = string.Format("KJ[{0:d}]-[{1}] : {2}(0x{3:x4})", nCurrentDeviceId, "EthernetOpen", "NG", rc);
                if (rc < 0x8000)
                {
                    // Common return code
                    string errorCode2 = "";
                    CommonErrorLog(rc, out errorCode2);
                    errorCode = errorCode + errorCode2;
                }
                return false;
            }

        }
        public bool InitHighSpeedCommunication(int nCurrentDeviceId, out string errorCode)
        {
            int rc;
            errorCode = "";
            lock (lockinit)
            {
                StopHighSpeedCommunication(nCurrentDeviceId);
                LJV7IF_ETHERNET_CONFIG ethernetTemp = deviceData[nCurrentDeviceId].EthernetConfig;

                rc = NativeMethods.LJV7IF_HighSpeedDataEthernetCommunicationInitalize(nCurrentDeviceId, ref ethernetTemp, 24692, CallbackGetData, 10, (uint)nCurrentDeviceId);
                if (rc != (int)Rc.Ok)
                {
                    errorCode = string.Format("KJ[{0:d}]-[{1}] : {2}(0x{3:x4})", nCurrentDeviceId, "HighSpeedInitalize", "NG", rc);
                    return false;
                }
            }

            LJV7IF_PROFILE_INFO profileInfo = new LJV7IF_PROFILE_INFO();
            LJV7IF_HIGH_SPEED_PRE_START_REQ req = new LJV7IF_HIGH_SPEED_PRE_START_REQ();
            req.bySendPos = 2;
            rc = NativeMethods.LJV7IF_PreStartHighSpeedDataCommunication(nCurrentDeviceId, ref req, ref profileInfo);
            if (rc != (int)Rc.Ok)
            {
                errorCode = string.Format("KJ[{0:d}]-[{1}] : {2}(0x{3:x4})", nCurrentDeviceId, "PreStart", "NG", rc);
                return false;
            }
            rc = NativeMethods.LJV7IF_StartHighSpeedDataCommunication(nCurrentDeviceId);
            ThreadSafeBuffer.ClearBuffer(nCurrentDeviceId);
            return true;
        }
        public void LJClearBuffer(int nDeviceId)
        {
            ThreadSafeBuffer.ClearBuffer(nDeviceId);
        }
        public void StopHighSpeedCommunication(int nCurrentDeviceId)
        {
            NativeMethods.LJV7IF_StopHighSpeedDataCommunication(nCurrentDeviceId);
            NativeMethods.LJV7IF_HighSpeedDataCommunicationFinalize(nCurrentDeviceId);
        }
        public bool LJCommClose(int nCurrentDeviceId, out string errorCode)
        {
            errorCode = "";
            int rc = NativeMethods.LJV7IF_CommClose(nCurrentDeviceId);
            if (rc == (int)Rc.Ok)
            {
                deviceData[nCurrentDeviceId].Status = DeviceStatus.NoConnection;
                return true;
            }
            else
            {
                errorCode = string.Format("KJ[{0:d}]-[{1}] : {2}(0x{3:x4})", nCurrentDeviceId, "Comm_Close", "NG", rc);
                if (rc < 0x8000)
                {
                    // Common return code
                    string errorCode2 = "";
                    CommonErrorLog(rc, out errorCode2);
                    errorCode = errorCode + errorCode2;
                }
                return false;
            }
        }
        
        public Form GetForm()
        {
            FormLJ formLj = new FormLJ();
            return formLj;
        }
        public bool StartMeasureProfile(int nCurrentDeviceId,out string errorCode)
        {
            int rc;
            errorCode = "";
            lock (lockinit)
            {
                StopHighSpeedCommunication(nCurrentDeviceId);
                LJV7IF_ETHERNET_CONFIG ethernetTemp = deviceData[nCurrentDeviceId].EthernetConfig;
                rc = NativeMethods.LJV7IF_HighSpeedDataEthernetCommunicationInitalize(nCurrentDeviceId, ref ethernetTemp, 24692, CallbackGetData, 10, (uint)nCurrentDeviceId);
                if (rc != (int)Rc.Ok)
                {
                    errorCode = string.Format("KJ[{0:d}]-[{1}] : {2}(0x{3:x4})", nCurrentDeviceId, "HighSpeedInitalize", "NG", rc);
                    return false;
                }
            }
            LJV7IF_PROFILE_INFO profileInfo = new LJV7IF_PROFILE_INFO();
            LJV7IF_HIGH_SPEED_PRE_START_REQ req = new LJV7IF_HIGH_SPEED_PRE_START_REQ();
            req.bySendPos = 2;
            rc = NativeMethods.LJV7IF_PreStartHighSpeedDataCommunication(nCurrentDeviceId, ref req, ref profileInfo);
            if (rc != (int)Rc.Ok)
            {
                errorCode = string.Format("KJ[{0:d}]-[{1}] : {2}(0x{3:x4})", nCurrentDeviceId, "PreStart", "NG", rc);
                return false;
            }
            rc = NativeMethods.LJV7IF_StartHighSpeedDataCommunication(nCurrentDeviceId);
            ThreadSafeBuffer.ClearBuffer(nCurrentDeviceId);
            sendCommand = SendCommand.StartMeasure;
            rc = NativeMethods.LJV7IF_StartMeasure(nCurrentDeviceId);
            if (rc == (int)Rc.Ok)
            {
                return true;
            }
            else
            {
                errorCode = string.Format("KJ[{0:d}]-[{1}] : {2}(0x{3:x4})", nCurrentDeviceId, "Start_Measure", "NG", rc);
                if (rc < 0x8000)
                {
                    // Common return code
                    string errorCode2 = "";
                    CommonErrorLog(rc, out errorCode2);
                    errorCode = errorCode + errorCode2;
                }
                return false;
            }
        }
        public bool StartMeasure(int nCurrentDeviceId, out string errorCode)
        {
            int rc;
            errorCode = "";
            sendCommand = SendCommand.StartMeasure;
            rc = NativeMethods.LJV7IF_StartMeasure(nCurrentDeviceId);
            if (rc == (int)Rc.Ok)
            {
                return true;
            }
            else
            {
                errorCode = string.Format("KJ[{0:d}]-[{1}] : {2}(0x{3:x4})", nCurrentDeviceId, "Start_Measure", "NG", rc);
                if (rc < 0x8000)
                {
                    // Common return code
                    string errorCode2 = "";
                    CommonErrorLog(rc, out errorCode2);
                    errorCode = errorCode + errorCode2;
                }
                return false;
            }
        }
        public bool StopMeasureProfile(int nCurrentDeviceId, out string errorCode)
        {
            errorCode = "";
            sendCommand = SendCommand.StopMeasure;
            int rc = NativeMethods.LJV7IF_StopMeasure(nCurrentDeviceId);
            if (rc == (int)Rc.Ok)
            {
                return true;
            }
            else
            {
                errorCode = string.Format("KJ[{0:d}]-[{1}] : {2}(0x{3:x4})", nCurrentDeviceId, "Stop_Measure", "NG", rc);
                if (rc < 0x8000)
                {
                    // Common return code
                    string errorCode2 = "";
                    CommonErrorLog(rc, out errorCode2);
                    errorCode = errorCode + errorCode2;
                }
                return false;
            }
        }
        public bool ClearMemoryMeasureProfile(int nCurrentDeviceId, out string errorCode)
        {
            errorCode = "";
            sendCommand = SendCommand.StopMeasure;
            int rc = NativeMethods.LJV7IF_ClearMemory(nCurrentDeviceId);
            if (rc == (int)Rc.Ok)
            {
                return true;
            }
            else
            {
                errorCode = string.Format("KJ[{0:d}]-[{1}] : {2}(0x{3:x4})", nCurrentDeviceId, "Clear_Memory", "NG", rc);
                if (rc < 0x8000)
                {
                    // Common return code
                    string errorCode2 = "";
                    CommonErrorLog(rc, out errorCode2);
                    errorCode = errorCode + errorCode2;
                }
                return false;
            }
        }
        private bool CheckReturnCode(Rc rc)
        {
            if (rc == Rc.Ok) return true;
            return false;
        }

        private T DeepClone<T>(T obj)
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
        public bool GetBatchProfileData(int nCurrentDeviceId,int nPathNo,int triggerNum, out string errorCode)
        {
            errorCode = "";
            uint notify = 0;
            int batchNo = 0;
            uint ncount = ThreadSafeBuffer.GetCount(nCurrentDeviceId, out notify, out batchNo);        
            //List<int[]> data = DeepClone<List<int[]>>(dataSrc);         
            DateTime start1 = DateTime.Now;
            while (true)
            {
                ncount = ThreadSafeBuffer.GetCount(nCurrentDeviceId, out notify, out batchNo);
                if (ncount >= triggerNum -10)
                {
                    break;
                }
                TimeSpan span = DateTime.Now - start1;
                if(span.TotalSeconds> 5)
                {
                    errorCode ="轨迹"+ nPathNo .ToString()+ "获取激光"+ nCurrentDeviceId.ToString()+"数据超时，已获得数据" + ncount.ToString();
                    ThreadSafeBuffer.ClearBuffer(nCurrentDeviceId);
                    //StopHighSpeedCommunication(nCurrentDeviceId);
                    return false;
                }
                Thread.Sleep(20);
            }
        //    StopHighSpeedCommunication(nCurrentDeviceId);
            List<int[]> data = ThreadSafeBuffer.Get(nCurrentDeviceId, out notify, out batchNo);
            if (data[0].Length == 1607)//控制器1
            {
                datalaserA.Clear();
                datalaserB.Clear();
                for (int i = 0; i < data.Count; i++)
                {
                    int[] a = new int[800];
                    int[] b = new int[800];
                    Array.Copy(data[i], 6, a, 0, 800);
                    Array.Copy(data[i], 806, b, 0, 800);
                    datalaserA.Add(a);
                    datalaserB.Add(b);
                }
            }
            else if (data[0].Length == 807)//控制器2
            {
                datalaser2A.Clear();
                for (int i = 0; i < data.Count; i++)
                {
                    int[] a = new int[800];
                    Array.Copy(data[i], 6, a, 0, 800);
                    datalaser2A.Add(a);
                }
            }
            else
            {
                errorCode = string.Format("KJ[{0:d}]-[{1}] : {2}", nCurrentDeviceId, "处理单行BatchData个数", "NG");
                data.Clear();
                StopHighSpeedCommunication(nCurrentDeviceId);
                return false;
            }
            
            return true;
        }
        public void DataConvert(object datas)
        {
            List<ProfileData> profileDatas = (List<ProfileData>)datas;
            if (profileDatas[0].ProfDatas.Count() == 1600)//控制器1
            {
                datalaserA.Clear();
                datalaserB.Clear();
                for (int i = 0; i < profileDatas.Count; i++)
                {
                    int[] a = new int[800];
                    int[] b = new int[800];
                    Array.Copy(profileDatas[i].ProfDatas, 0, a, 0, 800);
                    Array.Copy(profileDatas[i].ProfDatas, 800, b, 0, 800);
                    datalaserA.Add(a);
                    datalaserB.Add(b);
                }
            }
            else if (profileDatas[0].ProfDatas.Count() == 800)//控制器2
            {
                datalaser2A.Clear();
                for (int i = 0; i < profileDatas.Count; i++)
                {
                    int[] a = new int[800];
                    Array.Copy(profileDatas[i].ProfDatas, 0, a, 0, 800);
                    datalaser2A.Add(a);
                }
            }
            else
            {
                //string errorCode = string.Format("KJ[{0:d}]-[{1}] : {2}", nCurrentDeviceId, "处理单行BatchData个数", "NG");
                return;
            }
        }

        //同时获取激光数据
        public static void GetDataThread(IntPtr buffer, uint size, uint count, uint notify, uint user)
        {
            uint profileSize = (uint)(size / Marshal.SizeOf(typeof(int)));
            List<int[]> receiveBuffer = new List<int[]>();
            int[] bufferArray = new int[profileSize * count];
            Marshal.Copy(buffer, bufferArray, 0, (int)(profileSize * count));

            for (int i = 0; i < count; i++)
            {
                int[] oneProfile = new int[profileSize];
                Array.Copy(bufferArray, i * profileSize, oneProfile, 0, profileSize);
                receiveBuffer.Add(oneProfile);
            }
            ThreadSafeBuffer.Add((int)user, receiveBuffer, notify);
        }
        public static void GetDataThread2(IntPtr buffer, uint size, uint count, uint notify, uint user)
        {
            uint profileSize = (uint)(size / Marshal.SizeOf(typeof(int)));
            List<int[]> receiveBuffer = new List<int[]>();
            int[] bufferArray = new int[profileSize * count];
            Marshal.Copy(buffer, bufferArray, 0, (int)(profileSize * count));

            for (int i = 0; i < count; i++)
            {
                int[] oneProfile = new int[profileSize];
                Array.Copy(bufferArray, i * profileSize, oneProfile, 0, profileSize);
                receiveBuffer.Add(oneProfile);
            }
            ThreadSafeBuffer.Add((int)user, receiveBuffer, notify);
        }

        public bool GetBatchProfileData2(int nCurrentDeviceId,int pathNO,double startX,double startY,double YInter, out string errorCode)
        {
            errorCode = "";
            LJV7IF_GET_BATCH_PROFILE_REQ req = new LJV7IF_GET_BATCH_PROFILE_REQ();
            req.byTargetBank = (byte)ProfileBank.Active;
            req.byPosMode = (byte)BatchPos.Commited;
            req.dwGetBatchNo = 0;
            req.dwGetProfNo = 0;
            req.byGetProfCnt = byte.MaxValue;
            req.byErase = 0;

            LJV7IF_GET_BATCH_PROFILE_RSP rsp = new LJV7IF_GET_BATCH_PROFILE_RSP();
            LJV7IF_PROFILE_INFO profileInfo = new LJV7IF_PROFILE_INFO();

            int profileDataSize = Define.MAX_PROFILE_COUNT +
                (Marshal.SizeOf(typeof(LJV7IF_PROFILE_HEADER)) + Marshal.SizeOf(typeof(LJV7IF_PROFILE_FOOTER))) / Marshal.SizeOf(typeof(int));
            int[] receiveBuffer = new int[profileDataSize * req.byGetProfCnt];

            //        Cursor.Current = Cursors.WaitCursor;
            //List<ProfileData> profileDatas2 = new List<ProfileData>();//批处理数据
            //profileDatas.Clear();
            // Get profiles
            using (PinnedObject pin = new PinnedObject(receiveBuffer))
            {
                Rc rc = (Rc)NativeMethods.LJV7IF_GetBatchProfile(nCurrentDeviceId, ref req, ref rsp, ref profileInfo, pin.Pointer,
                    (uint)(receiveBuffer.Length * Marshal.SizeOf(typeof(int))));
                // @Point
                // # When reading all the profiles from a single batch, the specified number of profiles may not be read.
                // # To read the remaining profiles after the first set of profiles have been read, set the specification method (byPosMode)to 0x02, 
                //   specify the batch number (dwGetBatchNo), and then set the number to start reading profiles from (dwGetProfNo) and 
                //   the number of profiles to read (byGetProfCnt) to values that specify a range of profiles that have not been read to read the profiles in order.
                // # In more detail, this process entails:
                //   * First configure req as listed below and call this function again.
                //      byPosMode = LJV7IF_BATCH_POS_SPEC
                //      dwGetBatchNo = batch number that was read
                //      byGetProfCnt = Profile number of unread in the batch
                //   * Furthermore, if all profiles in the batch are not read,update the starting position for reading profiles (req.dwGetProfNo) and
                //     the number of profiles to read (req.byGetProfCnt), and then call LJV7IF_GetBatchProfile again. (Repeat this process until all the profiles have been read.)

                if (!CheckReturnCode(rc))
                {
                    errorCode = string.Format("KJ[{0:d}]-[{1}] : {2}", nCurrentDeviceId, "GetBatch", "NG");
                    if ((int)rc < 0x8000)
                    {
                        // Common return code
                        string errorCode2 = "";
                        CommonErrorLog((int)rc, out errorCode2);
                        errorCode = errorCode + errorCode2;
                    }
                    return false;
                }
                //AddLog(string.Format("[ProCount] :({0:d})", rsp.dwGetBatchProfCnt));
                // Output the data of each profile
                int unitSize = ProfileData.CalculateDataSize(profileInfo);
                for (int i = 0; i < rsp.byGetProfCnt; i++)
                {
                    ProfileData TEMP = new ProfileData(receiveBuffer, unitSize * i, profileInfo);
                    profileDatas2.Add(TEMP);
                }

                // Get all profiles within the batch.
                req.byPosMode = (byte)BatchPos.Spec;
                req.dwGetBatchNo = rsp.dwGetBatchNo;
                do
                {
                    // Update the get profile position
                    req.dwGetProfNo = rsp.dwGetBatchTopProfNo + rsp.byGetProfCnt;
                    req.byGetProfCnt = (byte)Math.Min((uint)(byte.MaxValue), (rsp.dwCurrentBatchProfCnt - req.dwGetProfNo));

                    rc = (Rc)NativeMethods.LJV7IF_GetBatchProfile(nCurrentDeviceId, ref req, ref rsp, ref profileInfo, pin.Pointer,
                        (uint)(receiveBuffer.Length * Marshal.SizeOf(typeof(int))));
                    if (!CheckReturnCode(rc))
                    {
                        errorCode = string.Format("KJ[{0:d}]-[{1}] : {2}", nCurrentDeviceId, "GetBatch", "NG");
                        if ((int)rc < 0x8000)
                        {
                            // Common return code
                            string errorCode2 = "";
                            CommonErrorLog((int)rc, out errorCode2);
                            errorCode = errorCode + errorCode2;
                        }
                        return false;
                    }
                    for (int i = 0; i < rsp.byGetProfCnt; i++)
                    {
                        profileDatas2.Add(new ProfileData(receiveBuffer, unitSize * i, profileInfo));
                    }
                } while (rsp.dwGetBatchProfCnt != (rsp.dwGetBatchTopProfNo + rsp.byGetProfCnt));
            }
            FixXDPara fixXDPara = new FixXDPara();
            fixXDPara.nNO = pathNO;
            fixXDPara.Xpos = startX;
            fixXDPara.Ypos = startY;
            fixXDPara.YInterval = YInter;
            Thread startLaserCollect2 = new Thread(new ParameterizedThreadStart(LaserCollect2));
            startLaserCollect2.IsBackground = true;
            startLaserCollect2.Start(fixXDPara);
            //if (profileDatas2[0].ProfDatas.Count() == 1600)//控制器1
            //{
            //    datalaserA.Clear();
            //    datalaserB.Clear();
            //    for (int i = 0; i < profileDatas2.Count; i++)
            //    {
            //        int[] a = new int[800];
            //        int[] b = new int[800];
            //        Array.Copy(profileDatas2[i].ProfDatas, 0, a, 0, 800);
            //        Array.Copy(profileDatas2[i].ProfDatas, 800, b, 0, 800);
            //        datalaserA.Add(a);
            //        datalaserB.Add(b);
            //    }
            //}
            /*else*/
            //if (profileDatas2[0].ProfDatas.Count() == 800)//控制器2
            //{
            //    datalaser2A.Clear();
            //    for (int i = 0; i < profileDatas2.Count; i++)
            //    {
            //        int[] a = new int[800];
            //        Array.Copy(profileDatas2[i].ProfDatas, 0, a, 0, 800);
            //        datalaser2A.Add(a);
            //    }
            //}
            //else
            //{
            //    errorCode = string.Format("KJ[{0:d}]-[{1}] : {2}", nCurrentDeviceId, "处理单行BatchData个数", "NG");
            //    return false;
            //}
            return true;
        }
        private void LaserCollect2(object test)
        {
            FixXDPara ThreadFixPara = (FixXDPara)test;
            List<ProfileData> profileDatasTemp = new List<ProfileData>();//批处理数据
            profileDatasTemp = profileDatas2;
            List<int[]> LJlaser2A = new List<int[]>();
            if (profileDatasTemp[0].ProfDatas.Count() == 800)//控制器2
            {
                for (int i = 0; i < profileDatasTemp.Count; i++)
                {
                    int[] a = new int[800];
                    Array.Copy(profileDatasTemp[i].ProfDatas, 0, a, 0, 800);

                    LJlaser2A.Add(a);
                }
            }
            
            //触发完后开始收集数据
            XDPOINT[,] laser2APoint;//2A头

            laser2APoint = new XDPOINT[LJlaser2A.Count, LJlaser2A[0].Length];
            for (int i = 0; i < laser2APoint.GetLength(0); i++)
            {
                for (int j = 0; j < laser2APoint.GetLength(1); j++)
                {
                    //laser2APoint[i, j].x = ThreadFixPara.Xpos + 0.02 * j;
                    laser2APoint[i, j].x = ThreadFixPara.Xpos + 0.04 * j;
                    laser2APoint[i, j].y = ThreadFixPara.Ypos + ThreadFixPara.YInterval * i;
                    laser2APoint[i, j].z = LJlaser2A[i][799 - j] / 100000.0;
                }
            }
            laserAllData.Add(laser2APoint);
            Laser2ADicData.Add(ThreadFixPara.nNO.ToString(), laser2APoint);
            Save3DLaserData(ThreadFixPara.nNO, 3, laser2APoint);
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
            if (!File.Exists(path))
            {
                CsvRecord recordHead = new CsvRecord();
                recordHead.Fields.Add("3DLaser");
                file.Records.Add(recordHead);
                CsvWriter writerHead = new CsvWriter();
                writerHead.WriteCsv(file, path);

            }
            file.Records.Clear();
            file.Populate(path, false);

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
        /// <summary>
        /// 字符串转16进制字节数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        private static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
        //设置批处理点数
        public bool SetBatchProflilePointNUM(int nCurrentDeviceId,int nBatchNum, out string errorCode)
        {
            errorCode = "";
            LJV7IF_TARGET_SETTING targetSetting = new LJV7IF_TARGET_SETTING();
            byte[] settingData = new byte[NativeMethods.ProgramSettingSize];
            try
            {        
                targetSetting.byType = (byte)0x10; ;
                targetSetting.byCategory = (byte)0x0;
                targetSetting.byItem = (byte)0xA;
                targetSetting.byTarget1 = (byte)0x0;
                targetSetting.byTarget2 = (byte)0x0;
                targetSetting.byTarget3 = (byte)0x0;
                targetSetting.byTarget4 = (byte)0x0;
                
                settingData[0] = (byte)0x3;//初始化可不要
                char[] trimChars = new char[] { ' ', ',' };
                //把个数转成16进制
                string strNum0X = nBatchNum.ToString("x4");//X4就是4位16进制
                strNum0X.Trim();
                //把16进制转成字节数组 byte ,byte  a.Remove(0, a.Length - 2);最后2位
                strNum0X = strNum0X.Substring(strNum0X.Length - 2, 2) + "," + strNum0X.Substring(strNum0X.Length-4,2);//从前面开始截取2个字符
                //
                string trimStr = strNum0X.Trim(trimChars);
                if (trimStr.Length > 0)
                {
                    string[] aSrc = trimStr.Split(',');
                    if (aSrc.Length > 0)
                    {
                        settingData = Array.ConvertAll<string, byte>(aSrc,
                            delegate (string s) { return Convert.ToByte(s, 16); });
                    }
                }
                Array.Resize(ref settingData, 2);//amount of data byte;

            }
            catch (Exception ex)
            {
                errorCode = nCurrentDeviceId.ToString() +":"+ ex.Message;
                return false;
            }

            using (PinnedObject pin = new PinnedObject(settingData))
            {
                uint dwError = 0;
                byte Depth = 0x01;
                int rc = NativeMethods.LJV7IF_SetSetting(nCurrentDeviceId, Depth, targetSetting,
                    pin.Pointer, 2, ref dwError);
                // @Point
                // # There are three setting areas: a) the write settings area, b) the running area, and c) the save area.
                //   * Specify a) for the setting level when you want to change multiple settings. However, to reflect settings in the LJ-V operations, you have to call LJV7IF_ReflectSetting.
                //	 * Specify b) for the setting level when you want to change one setting but you don't mind if this setting is returned to its value prior to the change when the power is turned off.
                //	 * Specify c) for the setting level when you want to change one setting and you want this new value to be retained even when the power is turned off.

                // @Point
                //  As a usage example, we will show how to use SettingForm to configure settings such that sending a setting, with SettingForm using its initial values,
                //  will change the sampling period in the running area to "100 Hz."
                //  Also see the GetSetting function.
                if (rc != (int)Rc.Ok)
                {
                    errorCode = string.Format("KJ[{0:d}]-[{1}] : {2}", nCurrentDeviceId, "设置BatchProfile点数", "NG");
                    if (rc < 0x8000)
                    {
                        // Common return code
                        string errorCode2 = "";
                        CommonErrorLog(rc, out errorCode2);
                        errorCode = errorCode + errorCode2;
                    }
                    return false;
                }
            }
            return true;
        }

    }
}
