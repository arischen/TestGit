using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using StructAssemble;
using System.Windows.Forms;

namespace MoveControlAssemble
{
    public class MoveControlClass
    {
        //凌华运动控制卡
        public class ADlink
        {
            private int maxPortsNum = 24;

            public bool isInited = false;
            public bool isHomed = false;
            public bool isHoming = false;
            public string errorMessage;

            public bool isErrorWarm = false;

            private object lockObj = new object();

            //初始化板卡
            public bool Init(ref int boardId, int mode, string filePath, Axis[] axis)
            {
                int boardIdTemp = boardId;
                if (!isInited)
                {
                    int result = APS168.APS_initial(ref boardId, mode);
                    if (0 == result)
                    {
                        isInited = true;
                        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                        {
                            APS168.APS_load_param_from_file(filePath);
                        }
                        else
                        {
                            APS168.APS_load_parameter_from_flash(boardIdTemp);
                        }
                    }
                    else
                    {
                        return false;
                    }
                    for (int i = 0; i < axis.Length; i++)
                    {
                        int id = axis[i].AxisId;
                        if (0 != APS168.APS_set_servo_on(id, 1))
                        {
                            return false;
                        }
                        APS168.APS_set_command_f(id, 0);
                        APS168.APS_set_position_f(id, 0);
                    }

                    APS168.APS_reset_trigger_count(boardIdTemp, 0);
                    APS168.APS_reset_trigger_count(boardIdTemp, 1);

                    //DOAllReset(boardIdTemp);
                }
                return true;
            }

            public void Servo_Off(int id)
            {
                APS168.APS_set_servo_on(id, 0);
            }

            public void Servo_On(int id)
            {
                APS168.APS_set_servo_on(id, 1);
            }

            public void BackLash_Enable(Axis myAxis,int backlashNum)
            {
                APS168.APS_set_backlash_en(myAxis.AxisId, 1);
                APS168.APS_set_axis_param_f(myAxis.AxisId, (Int32)APSDefine.PRA_BKL_DIST, 200);
                APS168.APS_set_axis_param_f(myAxis.AxisId, (Int32)APSDefine.PRA_BKL_CNSP, 200);
            }

            public void BackLash_Disable(Axis myAxis)
            {
                APS168.APS_set_backlash_en(myAxis.AxisId, 0);
            }

            //指定轴ID进行回原点, int homeMode, int homeDir, int homeHurve, int homeAcc, int homeVm, int homeVo, int homeEza,int homeShift, int homePosition
            //private
            public void HomeMove(Axis axis)
            {
                if(axis.AxisId == 0 || axis.AxisId == 1)
                {
                    BackLash_Disable(axis);
                }

                if (axis.AxisId == 0 || axis.AxisId == 1 || axis.AxisId == 2 || axis.AxisId == 4)
                    APS168.APS_set_axis_param(axis.AxisId, (int)APSDefine.PRA_HOME_DIR, 0); // Set home direction Positive
                else
                    APS168.APS_set_axis_param(axis.AxisId, (int)APSDefine.PRA_HOME_DIR, 1); // Set home direction Negative
                
                APS168.APS_set_axis_param(axis.AxisId, (int)APSDefine.PRA_HOME_MODE, 0); // Set home mode ORG
                APS168.APS_set_axis_param(axis.AxisId, (int)APSDefine.PRA_HOME_VM, (int)axis.HomeVel);
                APS168.APS_set_axis_param(axis.AxisId, (int)APSDefine.PRA_HOME_VO, (int)axis.HomeVO);
                APS168.APS_set_axis_param(axis.AxisId, (int)APSDefine.PRA_HOME_ACC, (int)axis.HomeAcc);
                APS168.APS_home_move(axis.AxisId);
            }

            //主轴回零
            public void HomeMoveZ(Axis axis)
            {
                APS168.APS_set_axis_param(axis.AxisId, (int)APSDefine.PRA_HOME_MODE, 0); // Set home mode ORG
                //APS168.APS_set_axis_param(axis.AxisId, (int)APSDefine.PRA_HOME_MODE, 2); // Set home mode Z
                APS168.APS_set_axis_param(axis.AxisId, (int)APSDefine.PRA_HOME_DIR, 0); // Set home direction Positive
                APS168.APS_set_axis_param(axis.AxisId, (int)APSDefine.PRA_HOME_VM, (int)axis.HomeVel);
                APS168.APS_set_axis_param(axis.AxisId, (int)APSDefine.PRA_HOME_VO, (int)axis.HomeVO);
                APS168.APS_set_axis_param(axis.AxisId, (int)APSDefine.PRA_HOME_ACC, (int)axis.HomeAcc);
                APS168.APS_home_move(axis.AxisId);
            }

            //检测是否运动完成
            public bool CheckMoveDone(Axis axis, ref int stopCode)
            {
                System.DateTime StartTime = DateTime.Now;
                //TimeSpan span;
                int axisId = axis.AxisId;
                //int CurPos = 0;
                stopCode = 0;
                int status = 0;
                status = APS168.APS_motion_status(axisId);
                int isDone = (status >> (int)APSDefine.NSTP) & 1;               // Get motion done bit
                if (1 == isDone)
                {
                    APS168.APS_get_stop_code(axisId, ref stopCode);
                    int errorCode = (status >> (int)APSDefine.MTS_ASTP) & 1;    // Get abnormal stop bit
                    if (1 == errorCode)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                return false;
            }

            object locker = new object();
            //JOG模式--开始运动
            public void JogMoveStart(Axis axis, int dir)
            {
                int axisId = axis.AxisId;
                lock (locker)
                {
                    APS168.APS_set_axis_param(axisId, (Int32)APSDefine.PRA_JG_MODE, 0);  // Set jog mode
                    APS168.APS_set_axis_param(axisId, (Int32)APSDefine.PRA_JG_DIR, dir);  // Set jog direction
                    APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_JG_VM, axis.JogVel);
                    APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_JG_CURVE, 0.0);
                    APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_JG_ACC, axis.JogAcc);
                    APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_JG_DEC, axis.JogDec);

                    APS168.APS_jog_start(axisId, 1);
                }
            }
            public void JogMoveStart(Axis axis, int dir, double speed)
            {
                int axisId = axis.AxisId;
                APS168.APS_set_axis_param(axisId, (Int32)APSDefine.PRA_JG_MODE, 0);  // Set jog mode
                APS168.APS_set_axis_param(axisId, (Int32)APSDefine.PRA_JG_DIR, dir);  // Set jog direction
                APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_JG_VM, speed);
                APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_JG_CURVE, 0.0);
                APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_JG_ACC, axis.JogAcc);
                APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_JG_DEC, axis.JogDec);

                APS168.APS_jog_start(axisId, 1);
            }

            public void JogMoveStart(Axis axis, int dir, double speed, ref int errorCode)
            {
                int axisId = axis.AxisId;
                errorCode += APS168.APS_set_axis_param(axisId, (Int32)APSDefine.PRA_JG_MODE, 0);  // Set jog mode
                errorCode += APS168.APS_set_axis_param(axisId, (Int32)APSDefine.PRA_JG_DIR, dir);  // Set jog direction
                errorCode += APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_JG_VM, speed);
                errorCode += APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_JG_CURVE, 0.0);
                errorCode += APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_JG_ACC, axis.JogAcc);
                errorCode += APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_JG_DEC, axis.JogDec);

                errorCode = APS168.APS_jog_start(axisId, 1);
            }

            //JOG模式--停止运动
            public void JogMoveStop(Axis axis)
            {
                APS168.APS_jog_start(axis.AxisId, 0);
            }
            //清除报警
            public void ClearAlarm(Axis axis)
            {
                APS168.APS_write_d_channel_output(0, 0, axis.AxisId, 1);
            }
            public void ClearPos(Axis axis)
            {
                APS168.APS_set_command_f(axis.AxisId, 0.0);
                APS168.APS_set_position_f(axis.AxisId, 0.0);
            }
            public void EmgStop(Axis axis)
            {
                APS168.APS_emg_stop(axis.AxisId);
            }

            //获取当前位置
            public bool GetEncodingPosition(Axis[] axis, ref double[] position)
            {
                int[] positionNow = new int[axis.Length];
                if (axis.Length != position.Length)
                {
                    position = new double[axis.Length];
                }

                for (int i = 0; i < axis.Length; i++)
                {
                    int result = APS168.APS_get_position(axis[i].AxisId, ref positionNow[i]);
                    if (0 == result)
                    {
                        position[i] = (double)positionNow[i] / axis[i].Rate;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }

            public void GetCurPostion(int AxisId,ref int Pos)
            {
                APS168.APS_get_position(AxisId, ref Pos);
            }

            //线性移动（多轴同时移动）
            public void LineMove(Axis[] axis, double[] positions)
            {
                int[] axisIds = new int[axis.Length];
                double[] truepos = new double[axis.Length];

                for (int i = 0; i < axis.Length; i++)
                {
                    axisIds[i] = axis[i].AxisId;
                    APS168.APS_set_axis_param_f(axis[i].AxisId, (int)APSDefine.PRA_SF, 0.5);
                    APS168.APS_set_axis_param_f(axis[i].AxisId, (int)APSDefine.PRA_ACC, axis[i].MoveAcc);
                    APS168.APS_set_axis_param_f(axis[i].AxisId, (int)APSDefine.PRA_DEC, axis[i].MoveDec);
                    APS168.APS_set_axis_param_f(axis[i].AxisId, (int)APSDefine.PRA_VM, axis[i].MoveVel);
                    truepos[i] = positions[i] * axis[i].Rate;
                }
                double transPara = 0;

                ASYNCALL p = new ASYNCALL();
                APS168.APS_line(axis.Length, axisIds, (int)APSDefine.OPT_ABSOLUTE, truepos, ref transPara, ref p);
            }

            //added by lei.c
            //线性移动+同时判断是否移动到位
            public bool LineMove(Axis[] axis, double[] positions, bool isAbsolute, ref int error, ref double PosError)
            {
                int mode = isAbsolute ? (int)APSDefine.OPT_ABSOLUTE : (int)APSDefine.OPT_RELATIVE;

                int[] pulsePosition=new int[axis.Length];
                double[] truepos = new double[axis.Length];
                int[] axisIds = new int[axis.Length];
                for (int i = 0; i < axis.Length; i++)
                {
                    axisIds[i] = axis[i].AxisId;
                    APS168.APS_set_axis_param_f(axis[i].AxisId, (int)APSDefine.PRA_SF, 0.5);
                    APS168.APS_set_axis_param_f(axis[i].AxisId, (int)APSDefine.PRA_ACC, axis[i].MoveAcc);
                    APS168.APS_set_axis_param_f(axis[i].AxisId, (int)APSDefine.PRA_DEC, axis[i].MoveDec);
                    APS168.APS_set_axis_param_f(axis[i].AxisId, (int)APSDefine.PRA_VM, axis[i].MoveVel);
                    truepos[i] = positions[i] * axis[i].Rate;
                }
                double transPara = 0;
                ASYNCALL p = new ASYNCALL();
                APS168.APS_line(axis.Length, axisIds, mode, truepos, ref transPara, ref p);

                DateTime start = DateTime.Now;
                while (!CheckMoveDone(axis[0], ref error))
                {
                    if (0 != error)
                        return false;
                    Thread.Sleep(30);
                    TimeSpan span = DateTime.Now - start;
                    if (30 < span.Seconds)//noted by lei.c 大于30s认为超时
                        return false;
                }


                double errorx = 0;double errory = 0;
                if (axis.Length >= 2)
                {
                    errorx = GetAxisPosError(axis[0], positions[0]);
                    errory = GetAxisPosError(axis[1], positions[1]);
                    PosError = errorx >= errory ? errorx : errory;
                }
                else
                    PosError= GetAxisPosError(axis[0], positions[0]);


                #region 限制定位误差
                //start = DateTime.Now;
                //while (true)
                //{
                //    errorx = GetAxisPosError(axis[0], positions[0]);
                //    errory = GetAxisPosError(axis[1], positions[1]);
                //    PosError = errorx >= errory ? errorx : errory;
                //    if (PosError <= 20)
                //        break;
                //    else
                //    {
                //        TimeSpan span = DateTime.Now - start;
                //        if ((span.Seconds + span.Milliseconds/1000.0) >= 0.1)
                //            break;
                //        else
                //            Thread.Sleep(10);
                //    }
                //}
                #endregion
                return true;
            }

            public double GetAxisPosError(Axis axis, double SetPos)
            {
                int pos = 0;double error = 0;
                GetCurPostion(axis.AxisId, ref pos);
                error = Math.Abs(pos - SetPos * axis.Rate);
                return error;
            }


            public Point2D GetCurPoint(Axis[] axis)
            {
                int posx = 0;int posy = 0;
                Point2D temp = new Point2D();
                GetCurPostion(axis[0].AxisId, ref posx);
                GetCurPostion(axis[1].AxisId, ref posy);
                temp.X = posx / axis[0].Rate;
                temp.Y = posy / axis[1].Rate;
                return temp;
            }

            //速度模式 dir=0 表示Positive  dir=1 表示Negative
            public void VelocityMove(Axis axis, int dir)
            {
                ASYNCALL p = new ASYNCALL();
                APS168.APS_vel(axis.AxisId, dir, axis.MoveVel, ref p);
            }

            public void VelocityMove(Axis axis, double speed, int dir)
            {
                ASYNCALL p = new ASYNCALL();
                APS168.APS_vel(axis.AxisId, dir, speed, ref p);
            }

            //点到点移动(模式可供选择：绝对移动、相对移动）
            public void P2PMove(Axis axis, double position, bool isAbsoluteMode)
            {
                int axisId = axis.AxisId;
                ASYNCALL p = new ASYNCALL();

                int option;
                if (isAbsoluteMode)
                {
                    option = (Int32)APSDefine.OPT_ABSOLUTE;
                }
                else
                {
                    option = (Int32)APSDefine.OPT_RELATIVE;
                }

                APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_VM, axis.MoveVel);
                APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_ACC, axis.MoveAcc);
                APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_DEC, axis.MoveDec);
                APS168.APS_ptp(axisId, option, (int)(position * axis.Rate), ref p);
            }

            //点到点移动 + 同时判断是否移动到位
            public bool P2PMove(Axis axis, double position, bool isAbsoluteMode, ref int error)
            {
                int curpos = 0;
                GetCurPostion(axis.AxisId, ref curpos);
                int axisId = axis.AxisId;
                double poserror = 100;
                ASYNCALL p = new ASYNCALL();

                int option;
                if (isAbsoluteMode)
                {
                    option = (Int32)APSDefine.OPT_ABSOLUTE;
                }
                else
                {
                    option = (Int32)APSDefine.OPT_RELATIVE;
                }

                APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_VM, axis.MoveVel);
                APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_ACC, axis.MoveAcc);
                APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_DEC, axis.MoveDec);
                APS168.APS_ptp(axisId, option, (int)(position * axis.Rate), ref p);

                DateTime start = DateTime.Now;
                while (!CheckMoveDone(axis, ref error))
                {
                    if (0 != error)
                    {
                        return false;
                    }
                    Thread.Sleep(50);
                    TimeSpan span = DateTime.Now - start;
                    if (30 < span.Seconds)
                    {
                        return false;
                    }
                }
                //0825
                //if (isAbsoluteMode == false)
                //{
                //    while (true)
                //    {
                //        poserror = GetAxisPosError(axis, curpos/axis.Rate + position);
                //        if (poserror <= 5)
                //            break;
                //        else
                //            Thread.Sleep(30);
                //    }
                //}
                return true;
            }

            public bool ECATP2PMove(Axis axis, double endposition, ref int error)
            {
                int axisId = axis.AxisId;

                APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_VM, axis.MoveVel);
                APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_ACC, axis.MoveAcc);
                APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_DEC, axis.MoveDec);
                APS168.APS_absolute_move(axisId, (int)(endposition * axis.Rate), (int)axis.MoveVel);

                return true;
            }



            public bool ECATP2PMove(Axis axis, double position, bool isAbsoluteMode, ref int error)
            {
                int curpos = 0;
                GetCurPostion(axis.AxisId, ref curpos);
                int axisId = axis.AxisId;
                double poserror = 100;

                APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_VM, axis.MoveVel);
                APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_ACC, axis.MoveAcc);
                APS168.APS_set_axis_param_f(axisId, (Int32)APSDefine.PRA_DEC, axis.MoveDec);
                if(isAbsoluteMode==false)
                    APS168.APS_relative_move(axisId, (int)(position * axis.Rate), (int)axis.MoveVel);
                else
                    APS168.APS_absolute_move(axisId, (int)(position * axis.Rate), (int)axis.MoveVel);

                DateTime start = DateTime.Now;
                while (!CheckMoveDone(axis, ref error))
                {
                    if (0 != error)
                    {
                        return false;
                    }
                    Thread.Sleep(50);
                    TimeSpan span = DateTime.Now - start;
                    if (10 < span.Seconds)
                    {
                        return false;
                    }
                }

                if (isAbsoluteMode == false)
                {
                    while (true)
                    {
                        poserror = GetAxisPosError(axis, curpos / axis.Rate + position);
                        if (poserror <= 5)
                            break;
                        else
                            Thread.Sleep(30);
                    }
                }
                return true;
            }

            //停止绝对移动/相对移动和线移动性
            public void StopMove(Axis axis)
            {
                APS168.APS_stop_move(axis.AxisId);
            }

            //读取轴的信号
            public void ReadMotionIO(int AxisId, ref int[] data)
            {
                int result = APS168.APS_motion_io_status(AxisId);
                data = new int[16];
                for (int i = 0; i < 16; i++)
                {
                    data[i] = (result >> i) & 1;
                }
            }
            //读取轴的状态
            public void ReadMotionStatus(int AxisId, ref int[] data)
            {
                int result = APS168.APS_motion_status(AxisId);
                data = new int[32];
                for (int i = 0; i < 32; i++)
                {
                    data[i] = (result >> i) & 1;
                }
            }

            //通过第几个DO端口进行设定ON还是Off
            public void WriteDO(int boardId, int doId, int value)
            {
                //    lock (lockObj)
                {
                    int doData = 0;
                    // Monitor.Enter(lockObj);
                    APS168.APS_read_d_output(boardId, 0, ref doData);

                    int[] doDataChannel = new int[maxPortsNum];
                    for (int i = 0; i < maxPortsNum; i++)
                    {
                        doDataChannel[i] = (doData >> i) & 1;
                    }

                    doDataChannel[doId + 8] = value;

                    int doWrite = 0;
                    for (int j = 0; j < maxPortsNum - 1; j++)
                    {
                        doWrite = doWrite | (doDataChannel[j] << j);
                    }
                    APS168.APS_write_d_output(boardId, 0, doWrite);
                    // Monitor.Exit(lockObj);
                }
            }

            //DO端口初始化
            public void DOAllReset(int boardId)
            {
                APS168.APS_write_d_output(boardId, 0, 0);
            }

            //读取第几个输入DI状态
            public bool ReadDI(int boardId, int diId)
            {
                bool bRet = false;
                int digital_input_value = 0;
                APS168.APS_read_d_input(boardId, 0, ref digital_input_value);
                int nstate = 0;
                nstate = ((digital_input_value >> diId) & 1);
                if (nstate == 1)
                {
                    bRet = true;
                }
                else
                {
                    bRet = false;
                }
                return bRet;
            }
            //读取全部DI状态
            public bool ReadAllDI(int boardId, ref int[] diData)
            {
                int diDataTemp = 0;
                int result = APS168.APS_read_d_input(boardId, 0, ref diDataTemp);
                if (0 == result)
                {
                    diData = new int[maxPortsNum];
                    for (int i = 0; i < maxPortsNum - 1; i++)
                    {
                        diData[i] = (diDataTemp >> i) & 1;
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
            public bool ReadAllDO(int boardId, ref int[] doData)
            {
                int doDataTemp = 0;
                int result = APS168.APS_read_d_output(boardId, 0, ref doDataTemp);
                if (0 == result)
                {
                    doData = new int[maxPortsNum];
                    for (int i = 0; i < maxPortsNum - 1; i++)
                    {
                        doData[i] = (doDataTemp >> i) & 1;
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
            //线性比较触发
            public void LinearCompareTrig(int boardId, Axis axis, int nCh, double startposition, double trigInterval, int trigNum)
            {
                Int32 ret = 0;
                // Disable TRG 0 ~ 3
                //ret = APS168.APS_set_trigger_param(boardId, (Int32)APSDefine.TGR_TRG_EN, 0);
                //ret = APS168.APS_reset_trigger_count(boardId, nCh);


                ret = APS168.APS_set_trigger_param(boardId, (Int32)APSDefine.TGR_TRG_EN, 0x3);//Trig enable
                //Linear compare source:  8Timer counter 
                if (nCh == 1)
                {
                    ret = APS168.APS_set_trigger_param(boardId, (Int32)APSDefine.TGR_TRG1_SRC, 0);
                    ret = APS168.APS_set_trigger_param(boardId, (Int32)APSDefine.TGR_LCMP1_SRC, 0);
                    ret = APS168.APS_set_trigger_param(boardId, (Int32)APSDefine.TGR_TRG1_SRC, 0x20);

                    ret = APS168.APS_set_trigger_linear(boardId, 1, (int)(startposition * axis.Rate), trigNum, (int)(trigInterval * axis.Rate));
                    APS168.APS_set_trigger_param(boardId, (short)APSDefine.TGR_TRG1_PWD,1000);// //  pulse width=  value * 0.02 us ' 1 
                }
                else if (nCh == 0) //中间激光头对应的控制器2
                {
                    ret = APS168.APS_set_trigger_param(boardId, (Int32)APSDefine.TGR_TRG0_SRC, 0);
                    ret = APS168.APS_set_trigger_param(boardId, (Int32)APSDefine.TGR_LCMP0_SRC, 2);
                    ret = APS168.APS_set_trigger_param(boardId, (Int32)APSDefine.TGR_TRG0_SRC, 0x10);

                    ret = APS168.APS_set_trigger_linear(boardId, 0, (int)(startposition * axis.Rate), trigNum, (int)(trigInterval * axis.Rate));
                    APS168.APS_set_trigger_param(boardId, (short)APSDefine.TGR_TRG0_PWD, 1000);// //  pulse width=  value * 0.02 us ' 1 
                }
                //APS168.APS_set_trigger_param(boardId, (short)APSDefine.TGR_LCMP0_SRC, 0);
                //ms=1000μs 脉宽为0.1ms
                //APS168.APS_set_trigger_param(boardId, (short)APSDefine.TGR_TRG0_LOGIC, 0); //'逻辑高电平有效
                //APS168.APS_set_trigger_param(boardId, (short)APSDefine.TGR_TRG0_TGL, 1);//'脉冲输出 0  toggle 为 1 



            }

            //电表触发
            public void TableTrig(Int32 boardId, int[] DataArr, int ArraySize, int CH)
            {
                Int32 ret = 0;
                Int32 Board_ID = boardId;
                Int32 i = 0;

                //Int32 ArraySize = Arsize;
                //Int32[] DataArr = new Int32[ArraySize];


                //Stop Timer
                //ret = APS168.APS_set_trigger_param(Board_ID, (Int32)APSDefine.TIMR_EN, 0);

                ////Clear timer
                //ret = APS168.APS_set_timer_counter(Board_ID, 0, 0);

                // Disable TRG 0 ~ 3
                ret = APS168.APS_set_trigger_param(Board_ID, (Int32)APSDefine.TGR_TRG_EN, 0);

                //Disable all CMP
                ret = APS168.APS_set_trigger_param(Board_ID, (Int32)APSDefine.TGR_TRG0_SRC, 0);
                ret = APS168.APS_set_trigger_param(Board_ID, (Int32)APSDefine.TGR_TRG1_SRC, 0);
                //ret = APS168.APS_set_trigger_param(Board_ID, (Int32)APSDefine.TGR_TRG2_SRC, 0);
                //ret = APS168.APS_set_trigger_param(Board_ID, (Int32)APSDefine.TGR_TRG3_SRC, 0);

                //------------------------------------------------------------

                // Enable trigger output

                ret = APS168.APS_set_trigger_param(Board_ID, (Int32)APSDefine.TGR_TRG_EN, 0x3);

                //Linear compare source: Timer counter 
                ret = APS168.APS_set_trigger_param(Board_ID, (Int32)APSDefine.TGR_TCMP0_SRC, 0);
                ret = APS168.APS_set_trigger_param(Board_ID, (Int32)APSDefine.TGR_TCMP1_SRC, 2);

                //Table compare direction: Bi-direction(No direction)
                ret = APS168.APS_set_trigger_param(Board_ID, (Int32)APSDefine.TGR_TCMP0_DIR, 2);
                ret = APS168.APS_set_trigger_param(Board_ID, (Int32)APSDefine.TGR_TCMP1_DIR, 2);

                //Trigger output TRG0 ~ TRG3 source
                //Trigger output 0 source: FCMP0 
                //Trigger output 1 source: FCMP0 
                //Trigger output 2 source: FCMP1 
                //Trigger output 3 source: FCMP1
                ret = APS168.APS_set_trigger_param(Board_ID, (Int32)APSDefine.TGR_TRG0_SRC, 0x8);
                ret = APS168.APS_set_trigger_param(Board_ID, (Int32)APSDefine.TGR_TRG1_SRC, 0x4);
                //ret = APS168.APS_set_trigger_param(Board_ID, (Int32)APSDefine.TGR_TRG2_SRC, 0x8);
                //ret = APS168.APS_set_trigger_param(Board_ID, (Int32)APSDefine.TGR_TRG3_SRC, 0x8);

                //for (i = 0; i < 10; i++)
                //    DataArr[i] = i * 1000;

                //Start Table CMP
                APS168.APS_set_trigger_param(boardId, (short)APSDefine.TGR_TRG0_PWD,5000);// //  pulse width=  value * 0.02 us ' 1 ms=1000μs 脉宽为0.1ms
                APS168.APS_set_trigger_param(boardId, (short)APSDefine.TGR_TRG1_PWD,5000);
                APS168.APS_set_trigger_table(Board_ID, CH, DataArr, ArraySize);

                ////Timer Interval: 1ms
                //ret = APS168.APS_set_trigger_param(Board_ID, (Int32)APSDefine.TIMR_ITV, 10000);

                ////TIMR DIR: Positive count
                //ret = APS168.APS_set_trigger_param(Board_ID, (Int32)APSDefine.TIMR_DIR, 0);

                ////Start Timer
                //ret = APS168.APS_set_trigger_param(Board_ID, (Int32)APSDefine.TIMR_EN, 1);
            }

            public void DisableTableTrig()
            {
                APS168.APS_set_trigger_param(0, (Int32)APSDefine.TGR_TRG_EN, 0);
            }

            //触发通道计数
            public void TrigCountCH(int nCh, out int nCounts)
            {
                int counts = 0;
                APS168.APS_get_trigger_count(0, nCh, ref counts);
                nCounts = counts;
            }

            public void ClearTrigCount(int nCh)
            {
                APS168.APS_reset_trigger_count(0, nCh);
            }

            public void PPTPAll(Axis axis,int Option,double position,double Vm,double SFac)
            {
                ASYNCALL temp = new ASYNCALL();
                APS168.APS_ptp_all(axis.AxisId,Option,position*axis.Rate,10,Vm,20,axis.MoveAcc*10,axis.MoveDec*10,SFac,ref temp);
            }
        }
        //凌华7432卡
        public class ADlink7432
        {
            public bool RegisterCard(ushort CardID)
            {
                short errcode = 0;
                errcode = DASK.Register_Card(DASK.PCI_7432, CardID);
                return errcode >= 0;
            }

            public bool ReleaseCard(ushort CardID)
            {
                short errcode = 0;
                errcode = DASK.Release_Card(CardID);
                return errcode >= 0;
            }

            public bool ReadSingleDi(ushort CardID, int Port, ref bool Value)
            {
                short errcode = 0;
                ushort state = 0;
                errcode = DASK.DI_ReadLine(CardID, 0, (ushort)Port, out state);
                Value = (state == 1);
                return errcode >= 0;
            }

            public bool ReadAllDi(ushort CardID, ref bool[] Value)
            {
                short errcode = 0;
                uint state = 0;
                errcode = DASK.DI_ReadPort(CardID, 0, out state);
                if (errcode < 0)
                {
                    return false;
                }
                Value = new bool[32];
                for (int i = 0; i < 32; i++)
                {
                    Value[i] = (((state >> i) & 0x01) == 0x01);
                }
                return true;
            }

            public bool ReadSingleDo(ushort CardID, int Port, ref bool Value)
            {
                short errcode = 0;
                ushort state = 0;
                errcode = DASK.DO_ReadLine(CardID, 0, (ushort)Port, out state);
                if (errcode < 0)
                {
                    return false;
                }
                Value = (state == 1);
                return true;
            }

            public bool ReadAllDo(ushort CardID, ref bool[] Value)
            {
                short errcode = 0;
                uint state = 0;
                errcode = DASK.DO_ReadPort(CardID, 0, out state);
                if (errcode < 0)
                {
                    return false;
                }
                Value = new bool[32];
                for (int i = 0; i < 32; i++)
                {
                    Value[i] = (((state >> i) & 0x01) == 0x01);
                }
                return true;
            }

            public bool WriteSingleDo(ushort CardID, int Port, bool Value)
            {
                short errcode = 0;
                errcode = DASK.DO_WriteLine(CardID, 0, (ushort)Port, Value ? (ushort)1 : (ushort)0);
                return errcode >= 0;
            }

            public bool WriteAllDo(ushort CardID, uint Value)
            {
                short errcode = 0;
                errcode = DASK.DO_WritePort(CardID, 0, Value);
                return errcode >= 0;
            }
        }

        public class ECATIOControl
        {
            /// <summary>
            /// EtherCAT总线IO卡DO写入
            /// </summary>
            /// <param name="CardID">EtherCAT卡号</param>
            /// <param name="MOD_No">EtherCAT从站站号，从0开始</param>
            /// <param name="Ch_No">需要控制的DO位，从0开始</param>
            /// <param name="Value">需要写入的DO值</param>
            /// <returns></returns>
            public bool WriteSingleDo(int CardID, int MOD_No, int Ch_No, bool Value)
            {
                int DO_Value = Value ? 1 : 0;
                int ret = APS168.APS_set_field_bus_d_channel_output(CardID, 0, MOD_No, Ch_No, DO_Value);
                return ret >= 0;
            }

            /// <summary>
            /// EtherCAT总线IO卡DO读取
            /// </summary>
            /// <param name="CardID">EtherCAT卡号</param>
            /// <param name="MOD_No">EtherCAT从站站号，从0开始</param>
            /// <param name="Ch_No">需要读取的DO位，从0开始</param>
            /// <param name="Value">读取的DO值</param>
            /// <returns></returns>
            public bool GetSingleDo(int CardID,int MOD_No,int Ch_No,ref bool Value)
            {
                int DO_Value = 0;
                int ret = APS168.APS_get_field_bus_d_channel_output(CardID, 0, MOD_No, Ch_No, ref DO_Value);
                Value = DO_Value == 1 ? true : false;
                return ret >= 0;
            }



            public bool GetSingleDi(int CardID, int MOD_No, int Ch_No, ref bool Value)
            {
                int DI_Value = 0;
                int ret=APS168.APS_get_field_bus_d_channel_input(CardID, 0, MOD_No, Ch_No, ref DI_Value);
                Value = DI_Value == 1 ? true : false;
                return ret >= 0;
            }

            public bool GetAllDi(int CardID, int MOD_No, ref bool[] Value)
            {
                Value = new bool[16];
                UInt32 DiValue = 0;
                int ret = APS168.APS_get_field_bus_d_port_input(CardID, 0, MOD_No, 0, ref DiValue);
                int index = 0;
                for (int i = 0; i < 16; i++)
                {
                    Value[i] = (DiValue & (1 << index)) != 0;
                    index++;
                }
                return ret >= 0;
            }

            public bool GetAllDo(int CardID, int MOD_No, ref bool[] Value)
            {
                Value = new bool[16];
                UInt32 DoValue = 0;
                int ret = APS168.APS_get_field_bus_d_port_output(CardID, 0, MOD_No, 0, ref DoValue);
                int index = 0;
                for (int i = 0; i < 16; i++)
                {
                    Value[i] = (DoValue & (1 << index)) != 0;
                    index++;
                }
                return ret >= 0;
            }

        }

        public class ADlink8338
        {

        }

    }
}
