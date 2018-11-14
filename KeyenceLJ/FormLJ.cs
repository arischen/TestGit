using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;//Marshal
using System.Windows.Forms;
using KeyenceLJ.KeyenceForm;
using CommonStruct.LC3D;
namespace KeyenceLJ
{
    public partial class FormLJ : Form
    {
        //LJ
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
        /// <summary>Measurement data list</summary>
		private List<MeasureData> _measureDatas;
        private SendCommand _sendCommand;
        private int _currentDeviceId;
        private DeviceData[] _deviceData;
        private Label[] _deviceStatusLabels;
        /// <summary>Array of labels that indicate the number of received profiles </summary>
        private Label[] _receivedProfileCountLabels;
     //   private KeyenceControl keyenceModule= new KeyenceControl();
        public FormLJ()
        {
            InitializeComponent();
            #region//LJ初始化参数
            _sendCommand = SendCommand.None;
            _deviceData = new DeviceData[NativeMethods.DeviceCount];
            _measureDatas = new List<MeasureData>();

            _deviceStatusLabels = new Label[] {
                _lblDeviceStatus0, _lblDeviceStatus1, _lblDeviceStatus2,
                _lblDeviceStatus3, _lblDeviceStatus4, _lblDeviceStatus5};

            _receivedProfileCountLabels = new Label[] {
                _lblReceiveProfileCount0, _lblReceiveProfileCount1, _lblReceiveProfileCount2,
                _lblReceiveProfileCount3, _lblReceiveProfileCount4, _lblReceiveProfileCount5};

            for (int i = 0; i < NativeMethods.DeviceCount; i++)
            {
                _deviceData[i] = new DeviceData();
                _deviceStatusLabels[i].Text = _deviceData[i].GetStatusString();
            }
            // Communication button comment setting
            //SetCommandBtnString();
            // Control initialization
            _cmbMeasureX.DataSource = GetMeasureRangeList();
            _cmbReceivedBinning.DataSource = GetReceivedBiginning();
            _cmbCompressX.DataSource = GetCompressX();

            _cmbMeasureX.SelectedValue = Define.MEASURE_RANGE_FULL;
            _cmbReceivedBinning.SelectedValue = Define.RECEIVED_BINNING_OFF;
            _cmbCompressX.SelectedValue = Define.COMPRESS_X_OFF;

            //DLLtab后面的  _cbxSelectProgram.SelectedIndex = 2;
            #endregion
            ////
        }

        private void _btnUsbOpen_Click(object sender, EventArgs e)
        {
            int rc = NativeMethods.LJV7IF_UsbOpen(_currentDeviceId);
            // @Point
            // # Enter the "_currentDeviceId" set here for the communication settings into the arguments of each DLL function.
            // # If you want to get data from multiple controllers, prepare and set multiple "_currentDeviceId" values,
            //   enter these values into the arguments of the DLL functions, and then use the functions.

            AddLogResult(rc, "USB_OPEN");

            _deviceData[_currentDeviceId].Status = (rc == (int)Rc.Ok) ? DeviceStatus.Usb : DeviceStatus.NoConnection;
            _deviceStatusLabels[_currentDeviceId].Text = _deviceData[_currentDeviceId].GetStatusString();

        }

        private void _btnStartMeasure_Click(object sender, EventArgs e)
        {
            _sendCommand = SendCommand.StartMeasure;

            int rc = NativeMethods.LJV7IF_StartMeasure(_currentDeviceId);
            AddLogResult(rc, "START_MEASURE");
        }

        private void _btnStopMeasure_Click(object sender, EventArgs e)
        {
            _sendCommand = SendCommand.StopMeasure;

            int rc = NativeMethods.LJV7IF_StopMeasure(_currentDeviceId);
            AddLogResult(rc, "STOP_MEASURE");
        }

        private void _btnStartStorage_Click(object sender, EventArgs e)
        {
            _sendCommand = SendCommand.StartStorage;
            int rc = NativeMethods.LJV7IF_StartStorage(_currentDeviceId);
            AddLogResult(rc, "START_STORAGE");
        }

        private void _btnStopStorage_Click(object sender, EventArgs e)
        {
            _sendCommand = SendCommand.StopStorage;

            int rc = NativeMethods.LJV7IF_StopStorage(_currentDeviceId);
            AddLogResult(rc, "STOP_STORAGE");
        }

        private void _btnGetStorageStatus_Click(object sender, EventArgs e)
        {
            _sendCommand = SendCommand.GetStorageStatus;

            using (GetStorageStatusForm getStorageStatusForm = new GetStorageStatusForm())
            {
                if (DialogResult.OK == getStorageStatusForm.ShowDialog())
                {
                    LJV7IF_GET_STRAGE_STATUS_REQ req = getStorageStatusForm.Req;
                    // @Point
                    // # dwReadArea is the target surface to read.
                    //   The target surface to read indicates where in the internal memory usage area to read.
                    // # The method to use in specifying dwReadArea varies depending on how internal memory is allocated.
                    //   * Double buffer
                    //      0 indicates the active surface, 1 indicates surface A, and 2 indicates surface B.
                    //   * Entire area (overwrite)
                    //      Fixed to 1
                    //   * Entire area (do not overwrite)
                    //      After a setting modification, data is saved in surfaces 1, 2, 3, and so on in order, and 0 is set as the active surface.
                    // # For details, see "9.2.9.2 Internal memory."

                    LJV7IF_GET_STRAGE_STATUS_RSP rsp = new LJV7IF_GET_STRAGE_STATUS_RSP();
                    LJV7IF_STORAGE_INFO storageInfo = new LJV7IF_STORAGE_INFO();

                    int rc = NativeMethods.LJV7IF_GetStorageStatus(_currentDeviceId, ref req, ref rsp, ref storageInfo);
                    // @Point
                    // # Terminology	
                    //  * Base time … time expressed with 32 bits (<- the time when the setting was changed)
                    //  * Accumulated date and time	 … counter value that indicates the elapsed time, in units of 10 ms, from the base time
                    // # The accumulated date and time are stored in the accumulated data.
                    // # The accumulated time of read data is calculated as shown below.
                    //   Accumulated time = "base time (stBaseTime of LJV7IF_GET_STORAGE_RSP)" + "accumulated date and time × 10 ms"

                    AddLogResult(rc, "GET_STORAGE_STATUS");
                    if (rc == (int)Rc.Ok)
                    {
                        // Response data display
                        AddLog(Utility.ConvertToLogString(rsp).ToString());
                        AddLog(Utility.ConvertToLogString(storageInfo).ToString());
                    }
                }
            }
        }

        private void _btnGetStorageData_Click(object sender, EventArgs e)
        {
            _sendCommand = SendCommand.GetStorageData;

            using (GetStorageDataForm getStorageData = new GetStorageDataForm())
            {
                if (DialogResult.OK == getStorageData.ShowDialog())
                {
                    _measureDatas.Clear();
                    LJV7IF_GET_STORAGE_REQ req = getStorageData.Req;
                    // @Point
                    // # dwReadArea is the target surface to read.
                    //    The target surface to read indicates where in the internal memory usage area to read.
                    // # The method to use in specifying dwReadArea varies depending on how internal memory is allocated.
                    //   * Double buffer
                    //      0 indicates the active surface, 1 indicates surface A, and 2 indicates surface B.
                    //   * Entire area (overwrite)
                    //      Fixed to 1
                    //   * Entire area (do not overwrite)
                    //      After a setting modification, data is saved in surfaces 1, 2, 3, and so on in order, and 0 is set as the active surface.
                    // # For details, see "9.2.9.2 Internal memory."

                    LJV7IF_STORAGE_INFO storageInfo = new LJV7IF_STORAGE_INFO();
                    LJV7IF_GET_STORAGE_RSP rsp = new LJV7IF_GET_STORAGE_RSP();
                    uint oneDataSize = (uint)(Marshal.SizeOf(typeof(uint)) + (uint)Utility.GetByteSize(Utility.TypeOfStruct.MEASURE_DATA) * (uint)NativeMethods.MeasurementDataCount);
                    uint allDataSize = Math.Min(Define.READ_DATA_SIZE, oneDataSize * getStorageData.Req.dwDataCnt);
                    byte[] receiveData = new byte[allDataSize];
                    using (PinnedObject pin = new PinnedObject(receiveData))
                    {
                        int rc = NativeMethods.LJV7IF_GetStorageData(_currentDeviceId, ref req, ref storageInfo, ref rsp, pin.Pointer, allDataSize);
                        AddLogResult(rc, "GET_STORAGE_DATA");
                        // @Point
                        // # Terminology	
                        //  * Base time … time expressed with 32 bits (<- the time when the setting was changed)
                        //  * Accumulated date and time	 … counter value that indicates the elapsed time, in units of 10 ms, from the base time
                        // # The accumulated date and time are stored in the accumulated data.
                        // # The accumulated time of read data is calculated as shown below.
                        //   Accumulated time = "base time (stBaseTime of LJV7IF_GET_STORAGE_RSP)" + "accumulated date and time × 10 ms"

                        if (rc == (int)Rc.Ok)
                        {
                            // Temporarily retain the get data.
                            int byteSize = MeasureData.GetByteSize();
                            for (int i = 0; i < (int)rsp.dwDataCnt; i++)
                            {
                                _measureDatas.Add(new MeasureData(receiveData, byteSize * i));
                            }

                            // Response data display
                            AddLog(Utility.ConvertToLogString(storageInfo).ToString());
                            AddLog(Utility.ConvertToLogString(rsp).ToString());
                        }
                    }
                }
            }
        }

        private void _btnGetProfile_Click(object sender, EventArgs e)
        {
            _sendCommand = SendCommand.GetProfile;

            using (ProfileForm profileForm = new ProfileForm())
            {
                if (DialogResult.OK == profileForm.ShowDialog())
                {
                    _deviceData[_currentDeviceId].ProfileData.Clear();
                    _deviceData[_currentDeviceId].MeasureData.Clear();
                    LJV7IF_GET_PROFILE_REQ req = profileForm.Req;
                    LJV7IF_GET_PROFILE_RSP rsp = new LJV7IF_GET_PROFILE_RSP();
                    LJV7IF_PROFILE_INFO profileInfo = new LJV7IF_PROFILE_INFO();
                    uint oneProfDataBuffSize = GetOneProfileDataSize();
                    uint allProfDataBuffSize = oneProfDataBuffSize * req.byGetProfCnt;
                    int[] profileData = new int[allProfDataBuffSize / Marshal.SizeOf(typeof(int))];

                    using (PinnedObject pin = new PinnedObject(profileData))
                    {
                        int rc = NativeMethods.LJV7IF_GetProfile(_currentDeviceId, ref req, ref rsp, ref profileInfo, pin.Pointer, allProfDataBuffSize);
                        AddLogResult(rc, "GET_PROFILE");
                        if (rc == (int)Rc.Ok)
                        {
                            // Response data display
                            AddLog(Utility.ConvertToLogString(rsp).ToString());
                            AddLog(Utility.ConvertToLogString(profileInfo).ToString());

                            AnalyzeProfileData((int)rsp.byGetProfCnt, ref profileInfo, profileData);

                            // Profile export
                            if (DataExporter.ExportOneProfile(_deviceData[_currentDeviceId].ProfileData.ToArray(), 0, _txtboxProfileFilePath.Text))
                            {
                                AddLog(@"###Saved!!");
                            }
                        }
                    }
                }
            }
        }

        private void _btnProfileFileSave_Click(object sender, EventArgs e)
        {
            if (_profileFileSave.ShowDialog(this) == DialogResult.Cancel) return;
            _txtboxProfileFilePath.Text = _profileFileSave.FileName;
            _txtboxProfileFilePath.SelectionStart = _txtboxProfileFilePath.Text.Length;
        }

        private void _btnLogClear_Click(object sender, EventArgs e)
        {
             _txtboxLog.Clear();
            //bool result = keyenceModule.SetBatchProflilePointNUM(0, 3000, out errorCode);
            //if (!result)
            //{
            //    MessageBox.Show(errorCode);
            //    return;
            //}
            //AddLog("[SetBatch]: OK");
        }

        private void _btnEthernetOpen_Click(object sender, EventArgs e)
        {
            using (OpenEthernetForm openEthernetForm = new OpenEthernetForm())
            {
                if (DialogResult.OK == openEthernetForm.ShowDialog())
                {
                    LJV7IF_ETHERNET_CONFIG ethernetConfig = openEthernetForm.EthernetConfig;
                    // @Point
                    // # Enter the "_currentDeviceId" set here for the communication settings into the arguments of each DLL function.
                    // # If you want to get data from multiple controllers, prepare and set multiple "_currentDeviceId" values,
                    //   enter these values into the arguments of the DLL functions, and then use the functions.

                    int rc = NativeMethods.LJV7IF_EthernetOpen(_currentDeviceId, ref ethernetConfig);
                    AddLogResult(rc, "OpenEthernet");

                    if (rc == (int)Rc.Ok)
                    {
                        _deviceData[_currentDeviceId].Status = DeviceStatus.Ethernet;
                        _deviceData[_currentDeviceId].EthernetConfig = ethernetConfig;
                    }
                    else
                    {
                        _deviceData[_currentDeviceId].Status = DeviceStatus.NoConnection;
                    }
                    _deviceStatusLabels[_currentDeviceId].Text = _deviceData[_currentDeviceId].GetStatusString();
                }
            }
        }
        private int GetSelectedDeviceId()
        {
            foreach (Control control in _pnlDeviceId.Controls)
            {
                RadioButton rd = control as RadioButton;
                if ((rd == null) || (!rd.Checked)) continue;

                return Convert.ToInt32(rd.Tag);
            }

            return -1;
        }
        private void _rdDevice_CheckedChanged(object sender, EventArgs e)
        {
            _currentDeviceId = GetSelectedDeviceId();
        }

        private void Init_LJ_Click(object sender, EventArgs e)
        {
            int rc = NativeMethods.LJV7IF_Initialize();
            AddLogResult(rc, "Initialize");

            for (int i = 0; i < _deviceData.Length; i++)
            {
                _deviceData[i].Status = DeviceStatus.NoConnection;
                _deviceStatusLabels[i].Text = _deviceData[i].GetStatusString();
            }
        }
        #region //Keyence_KJCode
        /// <summary>
        /// Get the measurement range.
        /// </summary>
        /// <returns>List used as the combo box data source</returns>
        private List<DictionaryEntry> GetMeasureRangeList()
        {
            List<DictionaryEntry> list = new List<DictionaryEntry>();
            list.Add(new DictionaryEntry("FULL", Define.MEASURE_RANGE_FULL));
            list.Add(new DictionaryEntry("MIDDLE", Define.MEASURE_RANGE_MIDDLE));
            list.Add(new DictionaryEntry("SMALL", Define.MEASURE_RANGE_SMALL));
            return list;
        }
        /// <summary>
        /// Get the light reception characteristic binning list.
        /// </summary>
        /// <returns>List used as the combo box data source</returns>
        private List<DictionaryEntry> GetReceivedBiginning()
        {
            List<DictionaryEntry> list = new List<DictionaryEntry>();
            list.Add(new DictionaryEntry("OFF", Define.RECEIVED_BINNING_OFF));
            list.Add(new DictionaryEntry("ON", Define.RECEIVED_BINNING_ON));
            return list;
        }
        /// <summary>
        /// Get the light reception characteristic binning list.
        /// </summary>
        /// <returns>List used as the combo box data source</returns>
        private List<DictionaryEntry> GetCompressX()
        {
            List<DictionaryEntry> list = new List<DictionaryEntry>();
            list.Add(new DictionaryEntry("OFF", Define.COMPRESS_X_OFF));
            list.Add(new DictionaryEntry("2", Define.COMPRESS_X_2));
            list.Add(new DictionaryEntry("4", Define.COMPRESS_X_4));
            return list;
        }
        #endregion
        /// </summary>
        /// 		/// <summary>
		/// Return code check
		/// </summary>
		/// <param name="rc">Return code</param>
		/// <returns>Is the return code OK?</returns>
		/// <remarks>If the return code is not OK, display a message and return false.</remarks>
		private bool CheckReturnCode(Rc rc)
        {
            if (rc == Rc.Ok) return true;
            MessageBox.Show(this, string.Format("Error: 0x{0,8:x}", rc), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        /// <summary>
        /// Output profile data to a file.
        /// </summary>
        /// <param name="profileDatas">Profile data to output</param>
        /// <param name="savePath">Full path to the file to save</param>
        /// <remarks>Output data in TSV format.</remarks>
        /// 
        /// //原来是static
        private  void SaveProfile(List<ProfileData> profileDatas, string savePath)
        {
            try
            {
                // Save the profile
                using (StreamWriter sw = new StreamWriter(savePath, false, Encoding.GetEncoding("utf-16")))
                {
                    // X-axis outputX轴坐标间隔0.02mm
                    sw.WriteLine(ProfileData.GetXPosString(profileDatas[0].ProfInfo));

                    // Output the data of each profile
                    foreach (ProfileData profile in profileDatas)
                    {
                        StringBuilder sb = new StringBuilder();
                        int dataCount = profile.ProfDatas.Length;

                        for (int i = 0; i < dataCount; i++)
                        {
                            sb.AppendFormat("{0}\t", profile.ProfDatas[i]);
                        }
                        sw.WriteLine(sb);
                    }
                    AddLog("SaveBatch Success");
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Log output

        /// <summary>
        /// Log output
        /// </summary>
        /// <param name="strLog">Output log</param>
        private void AddLog(string strLog)
        {
            _txtboxLog.AppendText(strLog + Environment.NewLine);
            _txtboxLog.SelectionStart = _txtboxLog.Text.Length;
            _txtboxLog.Focus();
            _txtboxLog.ScrollToCaret();
        }

        /// <summary>
        /// Communication command result log output
        /// </summary>
        /// <param name="rc">Return code from the DLL</param>
        /// <param name="commandName">Command name to be output in the log</param>
        private void AddLogResult(int rc, string commandName)
        {
            if (rc == (int)Rc.Ok)
            {
                AddLog(string.Format("[{0}] : {1}(0x{2:x4})", commandName, "OK", rc));
            }
            else
            {
                AddLog(string.Format("[{0}] : {1}(0x{2:x4})", commandName, "NG", rc));
                AddErrorLog(rc);
            }
        }

        /// <summary>
        /// Error log output
        /// </summary>
        /// <param name="rc">Return code</param>
        private void AddErrorLog(int rc)
        {
            if (rc < 0x8000)
            {
                // Common return code
                CommonErrorLog(rc);
            }
            else
            {
                // Individual return code
                IndividualErrorLog(rc);
            }
        }

        /// <summary>
        /// Add Error
        /// </summary>
        /// <param name="dwError"></param>
        private void AddError(uint dwError)
        {
            _txtboxLog.AppendText("  ErrorCode : 0x" + dwError.ToString("x8") + Environment.NewLine);
            _txtboxLog.SelectionStart = _txtboxLog.Text.Length;
            _txtboxLog.Focus();
            _txtboxLog.ScrollToCaret();
        }

        /// <summary>
        /// Common return code log output
        /// </summary>
        /// <param name="rc">Return code</param>
        private void CommonErrorLog(int rc)
        {
            switch (rc)
            {
                case (int)Rc.Ok:
                    AddLog("-> Normal termination");
                    break;
                case (int)Rc.ErrOpenDevice:
                    AddLog("-> Failed to open the device");
                    break;
                case (int)Rc.ErrNoDevice:
                    AddLog("-> Device not open");
                    break;
                case (int)Rc.ErrSend:
                    AddLog("-> Command send error");
                    break;
                case (int)Rc.ErrReceive:
                    AddLog("-> Response reception error");
                    break;
                case (int)Rc.ErrTimeout:
                    AddLog("-> Time out");
                    break;
                case (int)Rc.ErrParameter:
                    AddLog("-> Parameter error");
                    break;
                case (int)Rc.ErrNomemory:
                    AddLog("-> No free space");
                    break;
                default:
                    AddLog(string.Format("＃Undefined RC(0x{ 0,0:X4})", rc));
                    break;
            }
        }

        /// <summary>
        /// Individual return code log output
        /// </summary>
        /// <param name="rc">Return code</param>
        private void IndividualErrorLog(int rc)
        {
            switch (_sendCommand)
            {
                case SendCommand.RebootController:
                    {
                        switch (rc)
                        {
                            case 0x80A0:
                                AddLog(string.Format("-> {0}", @"Accessing the save area"));
                                break;
                            default:
                                AddLog(string.Format("＃Undefined RC(0x{ 0,0:X4})", rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.Trigger:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format("-> {0}", @"The trigger mode is not [external trigger]"));
                                break;
                            default:
                                AddLog(string.Format("＃Undefined RC(0x{ 0,0:X4})", rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.StartMeasure:
                case SendCommand.StopMeasure:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format("-> {0}", @"Batch measurements are off"));
                                break;
                            case 0x80A0:
                                AddLog(string.Format("-> {0}", @"Batch measurement start processing could not be performed because the REMOTE terminal is off or the LASER_OFF terminal is on"));
                                break;
                            default:
                                AddLog(string.Format("＃Undefined RC(0x{ 0,0:X4})", rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.AutoZero:
                case SendCommand.Timing:
                case SendCommand.Reset:
                case SendCommand.GetMeasurementValue:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format("-> {0}", @"The operation mode is [high-speed (profile only)]"));
                                break;
                            default:
                                AddLog(string.Format("＃Undefined RC(0x{ 0,0:X4})", rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.ChangeActiveProgram:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format("-> {0}", @"The change program setting is [terminal]"));
                                break;
                            default:
                                AddLog(string.Format("＃Undefined RC(0x{ 0,0:X4})", rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.GetProfile:
                case SendCommand.GetProfileAdvance:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format("-> {0}", @"The operation mode is [advanced (with OUT measurement)]"));
                                break;
                            case 0x8081:
                                AddLog(string.Format("-> {0}", @"Batch measurements on and profile compression (time axis) off"));
                                break;
                            case 0x80A0:
                                AddLog(string.Format("-> {0}", @"No profile data"));
                                break;
                            default:
                                AddLog(string.Format("＃Undefined RC(0x{ 0,0:X4})", rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.GetBatchProfile:
                case SendCommand.GetBatchProfileAdvance:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format("-> {0}", @"The operation mode is [advanced (with OUT measurement)]"));
                                break;
                            case 0x8081:
                                AddLog(string.Format("-> {0}", @"Not [batch measurements on and profile compression (time axis) off]"));
                                break;
                            case 0x80A0:
                                AddLog(string.Format("-> {0}", @"No batch data (batch measurements not run even once)"));
                                break;
                            default:
                                AddLog(string.Format("＃Undefined RC(0x{ 0,0:X4})", rc));
                                break;
                        }
                    }
                    break;

                case SendCommand.StartStorage:
                case SendCommand.StopStorage:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format("-> {0}", @"The operation mode is [high-speed (profile only)]"));
                                break;
                            case 0x8081:
                                AddLog(string.Format("-> {0}", @"Storage target setting is [OFF] (no storage)"));
                                break;
                            case 0x8082:
                                AddLog(string.Format("-> {0}", @"The storage condition setting is not [terminal/command]"));
                                break;
                            default:
                                AddLog(string.Format("＃Undefined RC(0x{ 0,0:X4})", rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.GetStorageStatus:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format("-> {0}", @"The operation mode is [high-speed (profile only)]"));
                                break;
                            default:
                                AddLog(string.Format("＃Undefined RC(0x{ 0,0:X4})", rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.GetStorageData:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format("-> {0}", @"The operation mode is [high-speed (profile only)]"));
                                break;
                            case 0x8081:
                                AddLog(string.Format("-> {0}", @"The storage target setting is not [OUT value]"));
                                break;
                            default:
                                AddLog(string.Format("＃Undefined RC(0x{ 0,0:X4})", rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.GetStorageProfile:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format("-> {0}", @"The operation mode is [high-speed (profile only)]"));
                                break;
                            case 0x8081:
                                AddLog(string.Format("-> {0}", @"The storage target setting is not profile, or batch measurements on and profile compression (time axis) off"));
                                break;
                            case 0x8082:
                                AddLog(string.Format("-> {0}", @"Batch measurements on and profile compression (time axis) off"));
                                break;
                            default:
                                AddLog(string.Format("＃Undefined RC(0x{ 0,0:X4})", rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.GetStorageBatchProfile:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format("-> {0}", @"The operation mode is [high-speed (profile only)]"));
                                break;
                            case 0x8081:
                                AddLog(string.Format("-> {0}", @"The storage target setting is not profile, or batch measurements on and profile compression (time axis) off"));
                                break;
                            case 0x8082:
                                AddLog(string.Format("-> {0}", @"Not [batch measurements on and profile compression (time axis) off]"));
                                break;
                            default:
                                AddLog(string.Format("＃Undefined RC(0x{ 0,0:X4})", rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.HighSpeedDataUsbCommunicationInitalize:
                case SendCommand.HighSpeedDataEthernetCommunicationInitalize:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format("-> {0}", @"The operation mode is [advanced (with OUT measurement)]"));
                                break;
                            case 0x80A1:
                                AddLog(string.Format("-> {0}", @"Already performing high-speed communication error (for high-speed communication)"));
                                break;
                            default:
                                AddLog(string.Format("＃Undefined RC(0x{ 0,0:X4})", rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.PreStartHighSpeedDataCommunication:
                case SendCommand.StartHighSpeedDataCommunication:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format("-> {0}", @"The operation mode is [advanced (with OUT measurement)]"));
                                break;
                            case 0x8081:
                                AddLog(string.Format("-> {0}", @"The data specified as the send start position does not exist"));
                                break;
                            case 0x80A0:
                                AddLog(string.Format("-> {0}", @"A high-speed data communication connection was not established"));
                                break;
                            case 0x80A1:
                                AddLog(string.Format("-> {0}", @"Already performing high-speed communication error (for high-speed communication)"));
                                break;
                            case 0x80A4:
                                AddLog(string.Format("-> {0}", @"The send target data was cleared"));
                                break;
                            default:
                                AddLog(string.Format("＃Undefined RC (0x{0,0:X4})", rc));
                                break;
                        }
                    }
                    break;
                default:
                    AddLog(string.Format("＃Undefined RC (0x{0,0:X4})", rc));
                    break;
            }
        } 
        #endregion

        /// <summary>
        /// AnalyzeProfileData
        /// </summary>
        /// <param name="profileCount">Number of profiles that were read</param>
        /// <param name="profileInfo">Profile information structure</param>
        /// <param name="profileData">Acquired profile data</param>
        private void AnalyzeProfileData(int profileCount, ref LJV7IF_PROFILE_INFO profileInfo, int[] profileData)
        {
            int dataUnit = ProfileData.CalculateDataSize(profileInfo);
            AnalyzeProfileData(profileCount, ref profileInfo, profileData, 0, dataUnit);
        }
        /// <summary>
		/// AnalyzeProfileData
		/// </summary>
		/// <param name="profileCount">Number of profiles that were read</param>
		/// <param name="profileInfo">Profile information structure</param>
		/// <param name="profileData">Acquired profile data</param>
		/// <param name="startProfileIndex">Start position of the profiles to copy</param>
		/// <param name="dataUnit">Profile data size</param>
		private void AnalyzeProfileData(int profileCount, ref LJV7IF_PROFILE_INFO profileInfo, int[] profileData, int startProfileIndex, int dataUnit)
        {
            int readPropfileDataSize = ProfileData.CalculateDataSize(profileInfo);
            int[] tempRecvieProfileData = new int[readPropfileDataSize];

            // Profile data retention
            for (int i = 0; i < profileCount; i++)
            {
                Array.Copy(profileData, (startProfileIndex + i * dataUnit), tempRecvieProfileData, 0, readPropfileDataSize);

                _deviceData[_currentDeviceId].ProfileData.Add(new ProfileData(tempRecvieProfileData, profileInfo));
            }
        }
        private uint GetOneProfileDataSize()
        {
            // Buffer size (in units of bytes)
            uint retBuffSize = 0;

            // Basic size
            int basicSize = (int)_cmbMeasureX.SelectedValue / (int)_cmbReceivedBinning.SelectedValue;
            basicSize /= (int)_cmbCompressX.SelectedValue;

            // Number of headers
            retBuffSize += (uint)basicSize * (_rdbtnOneHead.Checked ? 1U : 2U);

            // Envelope setting
            retBuffSize *= (_chkboxEnvelope.Checked ? 2U : 1U);

            //in units of bytes
            retBuffSize *= (uint)Marshal.SizeOf(typeof(uint));

            // Sizes of the header and footer structures
            LJV7IF_PROFILE_HEADER profileHeader = new LJV7IF_PROFILE_HEADER();
            retBuffSize += (uint)Marshal.SizeOf(profileHeader);
            LJV7IF_PROFILE_FOOTER profileFooter = new LJV7IF_PROFILE_FOOTER();
            retBuffSize += (uint)Marshal.SizeOf(profileFooter);

            return retBuffSize;
        }
        List<int[]> laserA = new List<int[]>();
        List<int[]> laserB = new List<int[]>();
        private void _btnGetBatchProfileData_Click(object sender, EventArgs e)
        {
            // Specify the target batch to get.
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

            using (ProgressForm progressForm = new ProgressForm())
            {
                Cursor.Current = Cursors.WaitCursor;

                progressForm.Status = Status.Communicating;
                progressForm.Show(this);
                progressForm.Refresh();

                List<ProfileData> profileDatas = new List<ProfileData>();
                // Get profiles
                using (PinnedObject pin = new PinnedObject(receiveBuffer))
                {
                    Rc rc = (Rc)NativeMethods.LJV7IF_GetBatchProfile(_currentDeviceId, ref req, ref rsp, ref profileInfo, pin.Pointer,
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

                    if (!CheckReturnCode(rc))  return;
                    AddLog(string.Format("[BatchProCount] :({0:d})", rsp.dwGetBatchProfCnt));
                    
                    // Output the data of each profile
                    int unitSize = ProfileData.CalculateDataSize(profileInfo);
                    for (int i = 0; i < rsp.byGetProfCnt; i++)
                    {
                        ProfileData TEMP = new ProfileData(receiveBuffer, unitSize * i, profileInfo);
                        profileDatas.Add(TEMP);
                    }

                    // Get all profiles within the batch.
                    req.byPosMode = (byte)BatchPos.Spec;
                    req.dwGetBatchNo = rsp.dwGetBatchNo;
                    do
                    {
                        // Update the get profile position
                        req.dwGetProfNo = rsp.dwGetBatchTopProfNo + rsp.byGetProfCnt;
                        req.byGetProfCnt = (byte)Math.Min((uint)(byte.MaxValue), (rsp.dwCurrentBatchProfCnt - req.dwGetProfNo));

                        rc = (Rc)NativeMethods.LJV7IF_GetBatchProfile(_currentDeviceId, ref req, ref rsp, ref profileInfo, pin.Pointer,
                            (uint)(receiveBuffer.Length * Marshal.SizeOf(typeof(int))));//2017-08-02 修改原来是Define.DEVICE_ID
                        if (!CheckReturnCode(rc))
                        {
                            AddLog("[GetBatch]: NG ");
                            return;
                        }
                        for (int i = 0; i < rsp.byGetProfCnt; i++)
                        {
                            profileDatas.Add(new ProfileData(receiveBuffer, unitSize * i, profileInfo));
                        }
                    } while (rsp.dwGetBatchProfCnt != (rsp.dwGetBatchTopProfNo + rsp.byGetProfCnt));
                    AddLog(string.Format("[RowDataCount] :({0:d})", profileDatas[0].ProfDatas.Count()));
                    AddLog("[GetBatch]: OK");
                }
               
                progressForm.Status = Status.Saving;
                progressForm.Refresh();



                for(int i=0;i< profileDatas.Count;i++)
                {
                    int[] a = new int[800];
                    int[] b = new int[800];
                    Array.Copy(profileDatas[i].ProfDatas, 0, a, 0, 800);
                    Array.Copy(profileDatas[i].ProfDatas, 800, b, 0, 800);
                    laserA.Add(a);
                    laserB.Add(b);
                }

                XDPOINT[,] laserAPoint = new XDPOINT[laserA.Count, laserA[0].Length];
                for (int i = 0; i < laserAPoint.GetLength(0); i++)
                {
                    for (int j = 0; j < laserAPoint.GetLength(1); j++)
                    {
                        //laserAPoint[i, j].x = 10000 + 0.02 * j;
                        laserAPoint[i, j].x = 10000 + 0.04 * j;
                        laserAPoint[i, j].y = 10 + 0.1 * i;
                        laserAPoint[i, j].z = laserA[i][j] / 100000;
                    }
                }


                // Save the file
                SaveProfile(profileDatas, _txtboxProfileFilePath.Text);
            }
        }

        private void _btnCommClose_Click(object sender, EventArgs e)
        {
            int rc = NativeMethods.LJV7IF_CommClose(_currentDeviceId);
            AddLogResult(rc, "COMM_CLOSE");

            _deviceData[_currentDeviceId].Status = DeviceStatus.NoConnection;
            _deviceStatusLabels[_currentDeviceId].Text = _deviceData[_currentDeviceId].GetStatusString();
        }

        private void _btnClearMemory_Click(object sender, EventArgs e)
        {
            int rc = NativeMethods.LJV7IF_ClearMemory(_currentDeviceId);
            AddLogResult(rc, "CLEAR_MEMORY");
        }

        private void _btnSetSetting_Click(object sender, EventArgs e)
        {
            using (SettingForm settingForm = new SettingForm(true))
            {
                if (DialogResult.OK == settingForm.ShowDialog())
                {
                    LJV7IF_TARGET_SETTING targetSetting = settingForm.TargetSetting;
                    using (PinnedObject pin = new PinnedObject(settingForm.Data))
                    {
                        uint dwError = 0;
                        int rc = NativeMethods.LJV7IF_SetSetting(_currentDeviceId, settingForm.Depth, targetSetting,
                            pin.Pointer, (uint)settingForm.Data.Length, ref dwError);
                        // @Point
                        // # There are three setting areas: a) the write settings area, b) the running area, and c) the save area.
                        //   * Specify a) for the setting level when you want to change multiple settings. However, to reflect settings in the LJ-V operations, you have to call LJV7IF_ReflectSetting.
                        //	 * Specify b) for the setting level when you want to change one setting but you don't mind if this setting is returned to its value prior to the change when the power is turned off.
                        //	 * Specify c) for the setting level when you want to change one setting and you want this new value to be retained even when the power is turned off.

                        // @Point
                        //  As a usage example, we will show how to use SettingForm to configure settings such that sending a setting, with SettingForm using its initial values,
                        //  will change the sampling period in the running area to "100 Hz."
                        //  Also see the GetSetting function.

                        AddLogResult(rc, "SID_SET_SETTING");
                        if (rc != (int)Rc.Ok)
                        {
                            AddError(dwError);
                        }
                    }
                }
            }
        }
    }
}
