namespace KeyenceLJ
{
    partial class FormLJ
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._btnUsbOpen = new System.Windows.Forms.Button();
            this._btnStartMeasure = new System.Windows.Forms.Button();
            this._btnStopMeasure = new System.Windows.Forms.Button();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this._btnStartStorage = new System.Windows.Forms.Button();
            this._btnStopStorage = new System.Windows.Forms.Button();
            this._btnGetStorageStatus = new System.Windows.Forms.Button();
            this._btnGetStorageData = new System.Windows.Forms.Button();
            this._btnGetStorageProfile = new System.Windows.Forms.Button();
            this._btnGetStorageBatchProfile = new System.Windows.Forms.Button();
            this._lblSettingAttention = new System.Windows.Forms.Label();
            this._grpReceived = new System.Windows.Forms.GroupBox();
            this._chkboxEnvelope = new System.Windows.Forms.CheckBox();
            this._cmbCompressX = new System.Windows.Forms.ComboBox();
            this._lblCompressX = new System.Windows.Forms.Label();
            this._grpMeasureRange = new System.Windows.Forms.GroupBox();
            this._cmbReceivedBinning = new System.Windows.Forms.ComboBox();
            this._cmbMeasureX = new System.Windows.Forms.ComboBox();
            this._lblReceivedBinning = new System.Windows.Forms.Label();
            this._lblMeasureX = new System.Windows.Forms.Label();
            this._grpHead = new System.Windows.Forms.GroupBox();
            this._rdbtnOneHead = new System.Windows.Forms.RadioButton();
            this._rdbtnTwoHead = new System.Windows.Forms.RadioButton();
            this._rdbtnWide = new System.Windows.Forms.RadioButton();
            this._btnGetProfile = new System.Windows.Forms.Button();
            this._grpExport = new System.Windows.Forms.GroupBox();
            this._txtboxProfileFilePath = new System.Windows.Forms.TextBox();
            this._btnGetBatchProfileData = new System.Windows.Forms.Button();
            this._btnProfileFileSave = new System.Windows.Forms.Button();
            this._lblSavePath = new System.Windows.Forms.Label();
            this._nudProfileNo = new System.Windows.Forms.NumericUpDown();
            this._btnSaveMeasureData = new System.Windows.Forms.Button();
            this._btnSave = new System.Windows.Forms.Button();
            this.label24 = new System.Windows.Forms.Label();
            this._grpLog = new System.Windows.Forms.GroupBox();
            this._txtboxLog = new System.Windows.Forms.TextBox();
            this._btnLogClear = new System.Windows.Forms.Button();
            this._btnEthernetOpen = new System.Windows.Forms.Button();
            this._pnlDeviceId = new System.Windows.Forms.Panel();
            this._lblDeviceStatus5 = new System.Windows.Forms.Label();
            this._lblDeviceStatus4 = new System.Windows.Forms.Label();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this._lblReceiveProfileCount5 = new System.Windows.Forms.Label();
            this._lblReceiveProfileCount4 = new System.Windows.Forms.Label();
            this._lblReceiveProfileCount3 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this._lblReceiveProfileCount0 = new System.Windows.Forms.Label();
            this._lblReceiveProfileCount1 = new System.Windows.Forms.Label();
            this._lblReceiveProfileCount2 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this._lblDeviceStatus3 = new System.Windows.Forms.Label();
            this._lblDeviceStatus2 = new System.Windows.Forms.Label();
            this._lblDeviceStatus1 = new System.Windows.Forms.Label();
            this._lblDeviceStatus0 = new System.Windows.Forms.Label();
            this._rdDevice3 = new System.Windows.Forms.RadioButton();
            this._rdDevice2 = new System.Windows.Forms.RadioButton();
            this._rdDevice1 = new System.Windows.Forms.RadioButton();
            this._rdDevice0 = new System.Windows.Forms.RadioButton();
            this._lblConectedDevice = new System.Windows.Forms.Label();
            this.Init_LJ = new System.Windows.Forms.Button();
            this._profileFileSave = new System.Windows.Forms.SaveFileDialog();
            this._btnCommClose = new System.Windows.Forms.Button();
            this._btnClearMemory = new System.Windows.Forms.Button();
            this._btnSetSetting = new System.Windows.Forms.Button();
            this.groupBox7.SuspendLayout();
            this._grpReceived.SuspendLayout();
            this._grpMeasureRange.SuspendLayout();
            this._grpHead.SuspendLayout();
            this._grpExport.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._nudProfileNo)).BeginInit();
            this._grpLog.SuspendLayout();
            this._pnlDeviceId.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _btnUsbOpen
            // 
            this._btnUsbOpen.BackColor = System.Drawing.Color.LightGray;
            this._btnUsbOpen.Location = new System.Drawing.Point(50, 34);
            this._btnUsbOpen.Name = "_btnUsbOpen";
            this._btnUsbOpen.Size = new System.Drawing.Size(96, 23);
            this._btnUsbOpen.TabIndex = 34;
            this._btnUsbOpen.Text = "打开USB";
            this._btnUsbOpen.UseVisualStyleBackColor = false;
            this._btnUsbOpen.Click += new System.EventHandler(this._btnUsbOpen_Click);
            // 
            // _btnStartMeasure
            // 
            this._btnStartMeasure.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this._btnStartMeasure.Location = new System.Drawing.Point(50, 150);
            this._btnStartMeasure.Name = "_btnStartMeasure";
            this._btnStartMeasure.Size = new System.Drawing.Size(96, 23);
            this._btnStartMeasure.TabIndex = 32;
            this._btnStartMeasure.Text = "开始测量";
            this._btnStartMeasure.UseVisualStyleBackColor = false;
            this._btnStartMeasure.Click += new System.EventHandler(this._btnStartMeasure_Click);
            // 
            // _btnStopMeasure
            // 
            this._btnStopMeasure.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this._btnStopMeasure.Location = new System.Drawing.Point(50, 179);
            this._btnStopMeasure.Name = "_btnStopMeasure";
            this._btnStopMeasure.Size = new System.Drawing.Size(96, 23);
            this._btnStopMeasure.TabIndex = 33;
            this._btnStopMeasure.Text = "停止测量";
            this._btnStopMeasure.UseVisualStyleBackColor = false;
            this._btnStopMeasure.Click += new System.EventHandler(this._btnStopMeasure_Click);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this._btnStartStorage);
            this.groupBox7.Controls.Add(this._btnStopStorage);
            this.groupBox7.Controls.Add(this._btnGetStorageStatus);
            this.groupBox7.Controls.Add(this._btnGetStorageData);
            this.groupBox7.Controls.Add(this._btnGetStorageProfile);
            this.groupBox7.Controls.Add(this._btnGetStorageBatchProfile);
            this.groupBox7.Location = new System.Drawing.Point(389, 407);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(158, 203);
            this.groupBox7.TabIndex = 31;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Storage-related functions";
            this.groupBox7.Visible = false;
            // 
            // _btnStartStorage
            // 
            this._btnStartStorage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this._btnStartStorage.Location = new System.Drawing.Point(7, 25);
            this._btnStartStorage.Name = "_btnStartStorage";
            this._btnStartStorage.Size = new System.Drawing.Size(143, 23);
            this._btnStartStorage.TabIndex = 0;
            this._btnStartStorage.Text = "StartStorage";
            this._btnStartStorage.UseVisualStyleBackColor = false;
            this._btnStartStorage.Click += new System.EventHandler(this._btnStartStorage_Click);
            // 
            // _btnStopStorage
            // 
            this._btnStopStorage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this._btnStopStorage.Location = new System.Drawing.Point(7, 54);
            this._btnStopStorage.Name = "_btnStopStorage";
            this._btnStopStorage.Size = new System.Drawing.Size(143, 23);
            this._btnStopStorage.TabIndex = 1;
            this._btnStopStorage.Text = "StopStorage";
            this._btnStopStorage.UseVisualStyleBackColor = false;
            this._btnStopStorage.Click += new System.EventHandler(this._btnStopStorage_Click);
            // 
            // _btnGetStorageStatus
            // 
            this._btnGetStorageStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this._btnGetStorageStatus.ForeColor = System.Drawing.SystemColors.ControlText;
            this._btnGetStorageStatus.Location = new System.Drawing.Point(7, 85);
            this._btnGetStorageStatus.Name = "_btnGetStorageStatus";
            this._btnGetStorageStatus.Size = new System.Drawing.Size(143, 23);
            this._btnGetStorageStatus.TabIndex = 2;
            this._btnGetStorageStatus.Text = "GetStorageStatus";
            this._btnGetStorageStatus.UseVisualStyleBackColor = false;
            this._btnGetStorageStatus.Click += new System.EventHandler(this._btnGetStorageStatus_Click);
            // 
            // _btnGetStorageData
            // 
            this._btnGetStorageData.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this._btnGetStorageData.ForeColor = System.Drawing.SystemColors.ControlText;
            this._btnGetStorageData.Location = new System.Drawing.Point(7, 116);
            this._btnGetStorageData.Name = "_btnGetStorageData";
            this._btnGetStorageData.Size = new System.Drawing.Size(143, 23);
            this._btnGetStorageData.TabIndex = 3;
            this._btnGetStorageData.Text = "GetStorageData";
            this._btnGetStorageData.UseVisualStyleBackColor = false;
            this._btnGetStorageData.Click += new System.EventHandler(this._btnGetStorageData_Click);
            // 
            // _btnGetStorageProfile
            // 
            this._btnGetStorageProfile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this._btnGetStorageProfile.ForeColor = System.Drawing.SystemColors.ControlText;
            this._btnGetStorageProfile.Location = new System.Drawing.Point(7, 144);
            this._btnGetStorageProfile.Name = "_btnGetStorageProfile";
            this._btnGetStorageProfile.Size = new System.Drawing.Size(143, 23);
            this._btnGetStorageProfile.TabIndex = 4;
            this._btnGetStorageProfile.Text = "GetStorageProfile";
            this._btnGetStorageProfile.UseVisualStyleBackColor = false;
            // 
            // _btnGetStorageBatchProfile
            // 
            this._btnGetStorageBatchProfile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this._btnGetStorageBatchProfile.ForeColor = System.Drawing.SystemColors.ControlText;
            this._btnGetStorageBatchProfile.Location = new System.Drawing.Point(7, 172);
            this._btnGetStorageBatchProfile.Name = "_btnGetStorageBatchProfile";
            this._btnGetStorageBatchProfile.Size = new System.Drawing.Size(145, 23);
            this._btnGetStorageBatchProfile.TabIndex = 5;
            this._btnGetStorageBatchProfile.Text = "GetStorageBatchProfile";
            this._btnGetStorageBatchProfile.UseVisualStyleBackColor = false;
            // 
            // _lblSettingAttention
            // 
            this._lblSettingAttention.AutoSize = true;
            this._lblSettingAttention.Location = new System.Drawing.Point(48, 376);
            this._lblSettingAttention.Name = "_lblSettingAttention";
            this._lblSettingAttention.Size = new System.Drawing.Size(353, 12);
            this._lblSettingAttention.TabIndex = 30;
            this._lblSettingAttention.Text = "*Match the setting of the controller with this application";
            this._lblSettingAttention.Visible = false;
            // 
            // _grpReceived
            // 
            this._grpReceived.Controls.Add(this._chkboxEnvelope);
            this._grpReceived.Controls.Add(this._cmbCompressX);
            this._grpReceived.Controls.Add(this._lblCompressX);
            this._grpReceived.Location = new System.Drawing.Point(49, 498);
            this._grpReceived.Name = "_grpReceived";
            this._grpReceived.Size = new System.Drawing.Size(317, 50);
            this._grpReceived.TabIndex = 29;
            this._grpReceived.TabStop = false;
            this._grpReceived.Text = "Profile settings";
            this._grpReceived.Visible = false;
            // 
            // _chkboxEnvelope
            // 
            this._chkboxEnvelope.AutoSize = true;
            this._chkboxEnvelope.Checked = true;
            this._chkboxEnvelope.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkboxEnvelope.Location = new System.Drawing.Point(196, 20);
            this._chkboxEnvelope.Name = "_chkboxEnvelope";
            this._chkboxEnvelope.Size = new System.Drawing.Size(120, 16);
            this._chkboxEnvelope.TabIndex = 2;
            this._chkboxEnvelope.Text = "Envelope setting";
            this._chkboxEnvelope.UseVisualStyleBackColor = true;
            // 
            // _cmbCompressX
            // 
            this._cmbCompressX.DisplayMember = "Key";
            this._cmbCompressX.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbCompressX.FormattingEnabled = true;
            this._cmbCompressX.Location = new System.Drawing.Point(133, 18);
            this._cmbCompressX.Name = "_cmbCompressX";
            this._cmbCompressX.Size = new System.Drawing.Size(59, 20);
            this._cmbCompressX.TabIndex = 1;
            this._cmbCompressX.ValueMember = "Value";
            // 
            // _lblCompressX
            // 
            this._lblCompressX.AutoSize = true;
            this._lblCompressX.Location = new System.Drawing.Point(6, 22);
            this._lblCompressX.Name = "_lblCompressX";
            this._lblCompressX.Size = new System.Drawing.Size(125, 12);
            this._lblCompressX.TabIndex = 0;
            this._lblCompressX.Text = "Compression (X axis)";
            // 
            // _grpMeasureRange
            // 
            this._grpMeasureRange.Controls.Add(this._cmbReceivedBinning);
            this._grpMeasureRange.Controls.Add(this._cmbMeasureX);
            this._grpMeasureRange.Controls.Add(this._lblReceivedBinning);
            this._grpMeasureRange.Controls.Add(this._lblMeasureX);
            this._grpMeasureRange.Location = new System.Drawing.Point(50, 431);
            this._grpMeasureRange.Name = "_grpMeasureRange";
            this._grpMeasureRange.Size = new System.Drawing.Size(317, 67);
            this._grpMeasureRange.TabIndex = 28;
            this._grpMeasureRange.TabStop = false;
            this._grpMeasureRange.Text = "Imaging settings";
            this._grpMeasureRange.Visible = false;
            // 
            // _cmbReceivedBinning
            // 
            this._cmbReceivedBinning.DisplayMember = "Key";
            this._cmbReceivedBinning.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbReceivedBinning.FormattingEnabled = true;
            this._cmbReceivedBinning.Location = new System.Drawing.Point(201, 42);
            this._cmbReceivedBinning.Name = "_cmbReceivedBinning";
            this._cmbReceivedBinning.Size = new System.Drawing.Size(91, 20);
            this._cmbReceivedBinning.TabIndex = 3;
            this._cmbReceivedBinning.ValueMember = "Value";
            // 
            // _cmbMeasureX
            // 
            this._cmbMeasureX.DisplayMember = "Key";
            this._cmbMeasureX.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbMeasureX.FormattingEnabled = true;
            this._cmbMeasureX.Location = new System.Drawing.Point(201, 17);
            this._cmbMeasureX.Name = "_cmbMeasureX";
            this._cmbMeasureX.Size = new System.Drawing.Size(91, 20);
            this._cmbMeasureX.TabIndex = 1;
            this._cmbMeasureX.ValueMember = "Value";
            // 
            // _lblReceivedBinning
            // 
            this._lblReceivedBinning.AutoSize = true;
            this._lblReceivedBinning.Location = new System.Drawing.Point(13, 46);
            this._lblReceivedBinning.Name = "_lblReceivedBinning";
            this._lblReceivedBinning.Size = new System.Drawing.Size(47, 12);
            this._lblReceivedBinning.TabIndex = 2;
            this._lblReceivedBinning.Text = "Binning";
            // 
            // _lblMeasureX
            // 
            this._lblMeasureX.AutoSize = true;
            this._lblMeasureX.Location = new System.Drawing.Point(12, 19);
            this._lblMeasureX.Name = "_lblMeasureX";
            this._lblMeasureX.Size = new System.Drawing.Size(179, 12);
            this._lblMeasureX.TabIndex = 0;
            this._lblMeasureX.Text = "Measurement range X direction";
            // 
            // _grpHead
            // 
            this._grpHead.Controls.Add(this._rdbtnOneHead);
            this._grpHead.Controls.Add(this._rdbtnTwoHead);
            this._grpHead.Controls.Add(this._rdbtnWide);
            this._grpHead.Location = new System.Drawing.Point(50, 390);
            this._grpHead.Name = "_grpHead";
            this._grpHead.Size = new System.Drawing.Size(317, 38);
            this._grpHead.TabIndex = 27;
            this._grpHead.TabStop = false;
            this._grpHead.Text = "Head";
            this._grpHead.Visible = false;
            // 
            // _rdbtnOneHead
            // 
            this._rdbtnOneHead.AutoSize = true;
            this._rdbtnOneHead.Location = new System.Drawing.Point(21, 17);
            this._rdbtnOneHead.Name = "_rdbtnOneHead";
            this._rdbtnOneHead.Size = new System.Drawing.Size(71, 16);
            this._rdbtnOneHead.TabIndex = 0;
            this._rdbtnOneHead.Text = "One Head";
            this._rdbtnOneHead.UseVisualStyleBackColor = true;
            // 
            // _rdbtnTwoHead
            // 
            this._rdbtnTwoHead.AutoSize = true;
            this._rdbtnTwoHead.Checked = true;
            this._rdbtnTwoHead.Location = new System.Drawing.Point(104, 17);
            this._rdbtnTwoHead.Name = "_rdbtnTwoHead";
            this._rdbtnTwoHead.Size = new System.Drawing.Size(71, 16);
            this._rdbtnTwoHead.TabIndex = 1;
            this._rdbtnTwoHead.TabStop = true;
            this._rdbtnTwoHead.Text = "Two Head";
            this._rdbtnTwoHead.UseVisualStyleBackColor = true;
            // 
            // _rdbtnWide
            // 
            this._rdbtnWide.AutoSize = true;
            this._rdbtnWide.Location = new System.Drawing.Point(187, 17);
            this._rdbtnWide.Name = "_rdbtnWide";
            this._rdbtnWide.Size = new System.Drawing.Size(119, 16);
            this._rdbtnWide.TabIndex = 2;
            this._rdbtnWide.Text = "Two heads (wide)";
            this._rdbtnWide.UseVisualStyleBackColor = true;
            // 
            // _btnGetProfile
            // 
            this._btnGetProfile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this._btnGetProfile.ForeColor = System.Drawing.SystemColors.ControlText;
            this._btnGetProfile.Location = new System.Drawing.Point(411, 332);
            this._btnGetProfile.Name = "_btnGetProfile";
            this._btnGetProfile.Size = new System.Drawing.Size(96, 23);
            this._btnGetProfile.TabIndex = 26;
            this._btnGetProfile.Text = "GetProfile";
            this._btnGetProfile.UseVisualStyleBackColor = false;
            this._btnGetProfile.Visible = false;
            this._btnGetProfile.Click += new System.EventHandler(this._btnGetProfile_Click);
            // 
            // _grpExport
            // 
            this._grpExport.Controls.Add(this._txtboxProfileFilePath);
            this._grpExport.Controls.Add(this._btnGetBatchProfileData);
            this._grpExport.Controls.Add(this._btnProfileFileSave);
            this._grpExport.Controls.Add(this._lblSavePath);
            this._grpExport.Location = new System.Drawing.Point(5, 237);
            this._grpExport.Name = "_grpExport";
            this._grpExport.Size = new System.Drawing.Size(219, 92);
            this._grpExport.TabIndex = 25;
            this._grpExport.TabStop = false;
            this._grpExport.Text = "保存结果文件";
            // 
            // _txtboxProfileFilePath
            // 
            this._txtboxProfileFilePath.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._txtboxProfileFilePath.Location = new System.Drawing.Point(6, 37);
            this._txtboxProfileFilePath.Name = "_txtboxProfileFilePath";
            this._txtboxProfileFilePath.ReadOnly = true;
            this._txtboxProfileFilePath.Size = new System.Drawing.Size(207, 23);
            this._txtboxProfileFilePath.TabIndex = 1;
            // 
            // _btnGetBatchProfileData
            // 
            this._btnGetBatchProfileData.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._btnGetBatchProfileData.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._btnGetBatchProfileData.Location = new System.Drawing.Point(6, 64);
            this._btnGetBatchProfileData.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this._btnGetBatchProfileData.Name = "_btnGetBatchProfileData";
            this._btnGetBatchProfileData.Size = new System.Drawing.Size(178, 28);
            this._btnGetBatchProfileData.TabIndex = 35;
            this._btnGetBatchProfileData.Text = "获得高速批处理轮廓";
            this._btnGetBatchProfileData.UseVisualStyleBackColor = true;
            this._btnGetBatchProfileData.Click += new System.EventHandler(this._btnGetBatchProfileData_Click);
            // 
            // _btnProfileFileSave
            // 
            this._btnProfileFileSave.Location = new System.Drawing.Point(144, 15);
            this._btnProfileFileSave.Name = "_btnProfileFileSave";
            this._btnProfileFileSave.Size = new System.Drawing.Size(31, 20);
            this._btnProfileFileSave.TabIndex = 2;
            this._btnProfileFileSave.Text = "...";
            this._btnProfileFileSave.UseVisualStyleBackColor = true;
            this._btnProfileFileSave.Click += new System.EventHandler(this._btnProfileFileSave_Click);
            // 
            // _lblSavePath
            // 
            this._lblSavePath.AutoSize = true;
            this._lblSavePath.Location = new System.Drawing.Point(6, 17);
            this._lblSavePath.Name = "_lblSavePath";
            this._lblSavePath.Size = new System.Drawing.Size(53, 12);
            this._lblSavePath.TabIndex = 0;
            this._lblSavePath.Text = "保存目录";
            // 
            // _nudProfileNo
            // 
            this._nudProfileNo.Location = new System.Drawing.Point(329, 330);
            this._nudProfileNo.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this._nudProfileNo.Name = "_nudProfileNo";
            this._nudProfileNo.Size = new System.Drawing.Size(43, 21);
            this._nudProfileNo.TabIndex = 4;
            this._nudProfileNo.Visible = false;
            // 
            // _btnSaveMeasureData
            // 
            this._btnSaveMeasureData.Location = new System.Drawing.Point(360, 350);
            this._btnSaveMeasureData.Name = "_btnSaveMeasureData";
            this._btnSaveMeasureData.Size = new System.Drawing.Size(147, 23);
            this._btnSaveMeasureData.TabIndex = 6;
            this._btnSaveMeasureData.Text = "Save the measurement value";
            this._btnSaveMeasureData.UseVisualStyleBackColor = true;
            this._btnSaveMeasureData.Visible = false;
            // 
            // _btnSave
            // 
            this._btnSave.Location = new System.Drawing.Point(182, 350);
            this._btnSave.Name = "_btnSave";
            this._btnSave.Size = new System.Drawing.Size(132, 23);
            this._btnSave.TabIndex = 5;
            this._btnSave.Text = "Save the profile";
            this._btnSave.UseVisualStyleBackColor = true;
            this._btnSave.Visible = false;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(152, 331);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(173, 12);
            this.label24.TabIndex = 3;
            this.label24.Text = "Index of the profile to save";
            this.label24.Visible = false;
            // 
            // _grpLog
            // 
            this._grpLog.Controls.Add(this._txtboxLog);
            this._grpLog.Controls.Add(this._btnLogClear);
            this._grpLog.Location = new System.Drawing.Point(230, 192);
            this._grpLog.Name = "_grpLog";
            this._grpLog.Size = new System.Drawing.Size(339, 143);
            this._grpLog.TabIndex = 24;
            this._grpLog.TabStop = false;
            this._grpLog.Text = "操作结果状态";
            // 
            // _txtboxLog
            // 
            this._txtboxLog.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._txtboxLog.Location = new System.Drawing.Point(6, 45);
            this._txtboxLog.Multiline = true;
            this._txtboxLog.Name = "_txtboxLog";
            this._txtboxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._txtboxLog.Size = new System.Drawing.Size(315, 92);
            this._txtboxLog.TabIndex = 1;
            // 
            // _btnLogClear
            // 
            this._btnLogClear.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this._btnLogClear.Location = new System.Drawing.Point(7, 17);
            this._btnLogClear.Name = "_btnLogClear";
            this._btnLogClear.Size = new System.Drawing.Size(96, 23);
            this._btnLogClear.TabIndex = 0;
            this._btnLogClear.Text = "清除log";
            this._btnLogClear.UseVisualStyleBackColor = true;
            this._btnLogClear.Click += new System.EventHandler(this._btnLogClear_Click);
            // 
            // _btnEthernetOpen
            // 
            this._btnEthernetOpen.BackColor = System.Drawing.Color.LightGray;
            this._btnEthernetOpen.ForeColor = System.Drawing.SystemColors.ControlText;
            this._btnEthernetOpen.Location = new System.Drawing.Point(50, 63);
            this._btnEthernetOpen.Name = "_btnEthernetOpen";
            this._btnEthernetOpen.Size = new System.Drawing.Size(96, 23);
            this._btnEthernetOpen.TabIndex = 23;
            this._btnEthernetOpen.Text = "打开Ethernet";
            this._btnEthernetOpen.UseVisualStyleBackColor = false;
            this._btnEthernetOpen.Click += new System.EventHandler(this._btnEthernetOpen_Click);
            // 
            // _pnlDeviceId
            // 
            this._pnlDeviceId.BackColor = System.Drawing.Color.DarkGray;
            this._pnlDeviceId.Controls.Add(this._lblDeviceStatus5);
            this._pnlDeviceId.Controls.Add(this._lblDeviceStatus4);
            this._pnlDeviceId.Controls.Add(this.radioButton2);
            this._pnlDeviceId.Controls.Add(this.radioButton1);
            this._pnlDeviceId.Controls.Add(this.panel1);
            this._pnlDeviceId.Controls.Add(this.label22);
            this._pnlDeviceId.Controls.Add(this.label23);
            this._pnlDeviceId.Controls.Add(this._lblDeviceStatus3);
            this._pnlDeviceId.Controls.Add(this._lblDeviceStatus2);
            this._pnlDeviceId.Controls.Add(this._lblDeviceStatus1);
            this._pnlDeviceId.Controls.Add(this._lblDeviceStatus0);
            this._pnlDeviceId.Controls.Add(this._rdDevice3);
            this._pnlDeviceId.Controls.Add(this._rdDevice2);
            this._pnlDeviceId.Controls.Add(this._rdDevice1);
            this._pnlDeviceId.Controls.Add(this._rdDevice0);
            this._pnlDeviceId.Location = new System.Drawing.Point(231, 36);
            this._pnlDeviceId.Name = "_pnlDeviceId";
            this._pnlDeviceId.Size = new System.Drawing.Size(334, 146);
            this._pnlDeviceId.TabIndex = 22;
            this._pnlDeviceId.Tag = "";
            // 
            // _lblDeviceStatus5
            // 
            this._lblDeviceStatus5.AutoSize = true;
            this._lblDeviceStatus5.Location = new System.Drawing.Point(55, 124);
            this._lblDeviceStatus5.Name = "_lblDeviceStatus5";
            this._lblDeviceStatus5.Size = new System.Drawing.Size(71, 12);
            this._lblDeviceStatus5.TabIndex = 65;
            this._lblDeviceStatus5.Text = "Unconnected";
            // 
            // _lblDeviceStatus4
            // 
            this._lblDeviceStatus4.AutoSize = true;
            this._lblDeviceStatus4.Location = new System.Drawing.Point(55, 105);
            this._lblDeviceStatus4.Name = "_lblDeviceStatus4";
            this._lblDeviceStatus4.Size = new System.Drawing.Size(71, 12);
            this._lblDeviceStatus4.TabIndex = 64;
            this._lblDeviceStatus4.Text = "Unconnected";
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(8, 122);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(29, 16);
            this.radioButton2.TabIndex = 63;
            this.radioButton2.Tag = "5";
            this.radioButton2.Text = "&5";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.CheckedChanged += new System.EventHandler(this._rdDevice_CheckedChanged);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(8, 102);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(29, 16);
            this.radioButton1.TabIndex = 62;
            this.radioButton1.Tag = "4";
            this.radioButton1.Text = "&4";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this._rdDevice_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.panel1.Controls.Add(this._lblReceiveProfileCount5);
            this.panel1.Controls.Add(this._lblReceiveProfileCount4);
            this.panel1.Controls.Add(this._lblReceiveProfileCount3);
            this.panel1.Controls.Add(this.label21);
            this.panel1.Controls.Add(this._lblReceiveProfileCount0);
            this.panel1.Controls.Add(this._lblReceiveProfileCount1);
            this.panel1.Controls.Add(this._lblReceiveProfileCount2);
            this.panel1.Location = new System.Drawing.Point(205, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(121, 140);
            this.panel1.TabIndex = 59;
            // 
            // _lblReceiveProfileCount5
            // 
            this._lblReceiveProfileCount5.AutoSize = true;
            this._lblReceiveProfileCount5.BackColor = System.Drawing.Color.Transparent;
            this._lblReceiveProfileCount5.Location = new System.Drawing.Point(3, 120);
            this._lblReceiveProfileCount5.Name = "_lblReceiveProfileCount5";
            this._lblReceiveProfileCount5.Size = new System.Drawing.Size(11, 12);
            this._lblReceiveProfileCount5.TabIndex = 6;
            this._lblReceiveProfileCount5.Text = "0";
            // 
            // _lblReceiveProfileCount4
            // 
            this._lblReceiveProfileCount4.AutoSize = true;
            this._lblReceiveProfileCount4.BackColor = System.Drawing.Color.Transparent;
            this._lblReceiveProfileCount4.Location = new System.Drawing.Point(3, 102);
            this._lblReceiveProfileCount4.Name = "_lblReceiveProfileCount4";
            this._lblReceiveProfileCount4.Size = new System.Drawing.Size(11, 12);
            this._lblReceiveProfileCount4.TabIndex = 5;
            this._lblReceiveProfileCount4.Text = "0";
            // 
            // _lblReceiveProfileCount3
            // 
            this._lblReceiveProfileCount3.AutoSize = true;
            this._lblReceiveProfileCount3.BackColor = System.Drawing.Color.Transparent;
            this._lblReceiveProfileCount3.Location = new System.Drawing.Point(3, 83);
            this._lblReceiveProfileCount3.Name = "_lblReceiveProfileCount3";
            this._lblReceiveProfileCount3.Size = new System.Drawing.Size(11, 12);
            this._lblReceiveProfileCount3.TabIndex = 4;
            this._lblReceiveProfileCount3.Text = "0";
            // 
            // label21
            // 
            this.label21.Font = new System.Drawing.Font("Tahoma", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label21.Location = new System.Drawing.Point(3, 3);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(174, 30);
            this.label21.TabIndex = 0;
            this.label21.Text = "Number of \r\nreceived profiles";
            // 
            // _lblReceiveProfileCount0
            // 
            this._lblReceiveProfileCount0.AutoSize = true;
            this._lblReceiveProfileCount0.BackColor = System.Drawing.Color.Transparent;
            this._lblReceiveProfileCount0.Location = new System.Drawing.Point(3, 31);
            this._lblReceiveProfileCount0.Name = "_lblReceiveProfileCount0";
            this._lblReceiveProfileCount0.Size = new System.Drawing.Size(11, 12);
            this._lblReceiveProfileCount0.TabIndex = 1;
            this._lblReceiveProfileCount0.Text = "0";
            // 
            // _lblReceiveProfileCount1
            // 
            this._lblReceiveProfileCount1.AutoSize = true;
            this._lblReceiveProfileCount1.BackColor = System.Drawing.Color.Transparent;
            this._lblReceiveProfileCount1.Location = new System.Drawing.Point(3, 49);
            this._lblReceiveProfileCount1.Name = "_lblReceiveProfileCount1";
            this._lblReceiveProfileCount1.Size = new System.Drawing.Size(11, 12);
            this._lblReceiveProfileCount1.TabIndex = 2;
            this._lblReceiveProfileCount1.Text = "0";
            // 
            // _lblReceiveProfileCount2
            // 
            this._lblReceiveProfileCount2.AutoSize = true;
            this._lblReceiveProfileCount2.BackColor = System.Drawing.Color.Transparent;
            this._lblReceiveProfileCount2.Location = new System.Drawing.Point(3, 66);
            this._lblReceiveProfileCount2.Name = "_lblReceiveProfileCount2";
            this._lblReceiveProfileCount2.Size = new System.Drawing.Size(11, 12);
            this._lblReceiveProfileCount2.TabIndex = 3;
            this._lblReceiveProfileCount2.Text = "0";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Font = new System.Drawing.Font("Tahoma", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label22.Location = new System.Drawing.Point(42, 12);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(145, 13);
            this.label22.TabIndex = 1;
            this.label22.Text = "State (USB / IP address)";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Font = new System.Drawing.Font("Tahoma", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label23.Location = new System.Drawing.Point(19, 12);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(20, 13);
            this.label23.TabIndex = 0;
            this.label23.Text = "ID";
            // 
            // _lblDeviceStatus3
            // 
            this._lblDeviceStatus3.AutoSize = true;
            this._lblDeviceStatus3.Location = new System.Drawing.Point(55, 87);
            this._lblDeviceStatus3.Name = "_lblDeviceStatus3";
            this._lblDeviceStatus3.Size = new System.Drawing.Size(71, 12);
            this._lblDeviceStatus3.TabIndex = 61;
            this._lblDeviceStatus3.Text = "Unconnected";
            // 
            // _lblDeviceStatus2
            // 
            this._lblDeviceStatus2.AutoSize = true;
            this._lblDeviceStatus2.Location = new System.Drawing.Point(55, 69);
            this._lblDeviceStatus2.Name = "_lblDeviceStatus2";
            this._lblDeviceStatus2.Size = new System.Drawing.Size(71, 12);
            this._lblDeviceStatus2.TabIndex = 7;
            this._lblDeviceStatus2.Text = "Unconnected";
            // 
            // _lblDeviceStatus1
            // 
            this._lblDeviceStatus1.AutoSize = true;
            this._lblDeviceStatus1.Location = new System.Drawing.Point(55, 52);
            this._lblDeviceStatus1.Name = "_lblDeviceStatus1";
            this._lblDeviceStatus1.Size = new System.Drawing.Size(71, 12);
            this._lblDeviceStatus1.TabIndex = 5;
            this._lblDeviceStatus1.Text = "Unconnected";
            // 
            // _lblDeviceStatus0
            // 
            this._lblDeviceStatus0.AutoSize = true;
            this._lblDeviceStatus0.Location = new System.Drawing.Point(55, 35);
            this._lblDeviceStatus0.Name = "_lblDeviceStatus0";
            this._lblDeviceStatus0.Size = new System.Drawing.Size(71, 12);
            this._lblDeviceStatus0.TabIndex = 3;
            this._lblDeviceStatus0.Text = "Unconnected";
            // 
            // _rdDevice3
            // 
            this._rdDevice3.AutoSize = true;
            this._rdDevice3.Location = new System.Drawing.Point(8, 85);
            this._rdDevice3.Name = "_rdDevice3";
            this._rdDevice3.Size = new System.Drawing.Size(29, 16);
            this._rdDevice3.TabIndex = 60;
            this._rdDevice3.Tag = "3";
            this._rdDevice3.Text = "&3";
            this._rdDevice3.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
            this._rdDevice3.UseVisualStyleBackColor = true;
            this._rdDevice3.CheckedChanged += new System.EventHandler(this._rdDevice_CheckedChanged);
            // 
            // _rdDevice2
            // 
            this._rdDevice2.AutoSize = true;
            this._rdDevice2.Location = new System.Drawing.Point(8, 68);
            this._rdDevice2.Name = "_rdDevice2";
            this._rdDevice2.Size = new System.Drawing.Size(29, 16);
            this._rdDevice2.TabIndex = 6;
            this._rdDevice2.Tag = "2";
            this._rdDevice2.Text = "&2";
            this._rdDevice2.UseVisualStyleBackColor = true;
            this._rdDevice2.CheckedChanged += new System.EventHandler(this._rdDevice_CheckedChanged);
            // 
            // _rdDevice1
            // 
            this._rdDevice1.AutoSize = true;
            this._rdDevice1.Location = new System.Drawing.Point(8, 50);
            this._rdDevice1.Name = "_rdDevice1";
            this._rdDevice1.Size = new System.Drawing.Size(29, 16);
            this._rdDevice1.TabIndex = 4;
            this._rdDevice1.Tag = "1";
            this._rdDevice1.Text = "&1";
            this._rdDevice1.UseVisualStyleBackColor = true;
            this._rdDevice1.CheckedChanged += new System.EventHandler(this._rdDevice_CheckedChanged);
            // 
            // _rdDevice0
            // 
            this._rdDevice0.AutoSize = true;
            this._rdDevice0.Checked = true;
            this._rdDevice0.Location = new System.Drawing.Point(8, 32);
            this._rdDevice0.Name = "_rdDevice0";
            this._rdDevice0.Size = new System.Drawing.Size(29, 16);
            this._rdDevice0.TabIndex = 2;
            this._rdDevice0.TabStop = true;
            this._rdDevice0.Tag = "0";
            this._rdDevice0.Text = "&0";
            this._rdDevice0.UseVisualStyleBackColor = true;
            this._rdDevice0.CheckedChanged += new System.EventHandler(this._rdDevice_CheckedChanged);
            // 
            // _lblConectedDevice
            // 
            this._lblConectedDevice.AutoSize = true;
            this._lblConectedDevice.Location = new System.Drawing.Point(232, 16);
            this._lblConectedDevice.Name = "_lblConectedDevice";
            this._lblConectedDevice.Size = new System.Drawing.Size(173, 12);
            this._lblConectedDevice.TabIndex = 21;
            this._lblConectedDevice.Text = "控制器连接状态 (最多6个连接)";
            // 
            // Init_LJ
            // 
            this.Init_LJ.BackColor = System.Drawing.Color.LightGray;
            this.Init_LJ.Location = new System.Drawing.Point(50, 5);
            this.Init_LJ.Name = "Init_LJ";
            this.Init_LJ.Size = new System.Drawing.Size(96, 23);
            this.Init_LJ.TabIndex = 20;
            this.Init_LJ.Text = "初始化LJ";
            this.Init_LJ.UseVisualStyleBackColor = false;
            this.Init_LJ.Click += new System.EventHandler(this.Init_LJ_Click);
            // 
            // _profileFileSave
            // 
            this._profileFileSave.Filter = "Profile (*.txt)|*.txt | all files (*.*)|*.*";
            // 
            // _btnCommClose
            // 
            this._btnCommClose.BackColor = System.Drawing.Color.LightGray;
            this._btnCommClose.Location = new System.Drawing.Point(50, 92);
            this._btnCommClose.Name = "_btnCommClose";
            this._btnCommClose.Size = new System.Drawing.Size(96, 23);
            this._btnCommClose.TabIndex = 35;
            this._btnCommClose.Text = "关闭Comm";
            this._btnCommClose.UseVisualStyleBackColor = false;
            this._btnCommClose.Click += new System.EventHandler(this._btnCommClose_Click);
            // 
            // _btnClearMemory
            // 
            this._btnClearMemory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this._btnClearMemory.Location = new System.Drawing.Point(50, 208);
            this._btnClearMemory.Name = "_btnClearMemory";
            this._btnClearMemory.Size = new System.Drawing.Size(96, 23);
            this._btnClearMemory.TabIndex = 36;
            this._btnClearMemory.Text = "清除内存";
            this._btnClearMemory.UseVisualStyleBackColor = false;
            this._btnClearMemory.Click += new System.EventHandler(this._btnClearMemory_Click);
            // 
            // _btnSetSetting
            // 
            this._btnSetSetting.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this._btnSetSetting.ForeColor = System.Drawing.SystemColors.ControlText;
            this._btnSetSetting.Location = new System.Drawing.Point(50, 121);
            this._btnSetSetting.Name = "_btnSetSetting";
            this._btnSetSetting.Size = new System.Drawing.Size(96, 23);
            this._btnSetSetting.TabIndex = 37;
            this._btnSetSetting.Text = "处理设置";
            this._btnSetSetting.UseVisualStyleBackColor = false;
            this._btnSetSetting.Click += new System.EventHandler(this._btnSetSetting_Click);
            // 
            // FormLJ
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(598, 348);
            this.Controls.Add(this._btnSetSetting);
            this.Controls.Add(this._btnClearMemory);
            this.Controls.Add(this._btnCommClose);
            this.Controls.Add(this._nudProfileNo);
            this.Controls.Add(this._btnUsbOpen);
            this.Controls.Add(this._btnSave);
            this.Controls.Add(this.label24);
            this.Controls.Add(this._btnSaveMeasureData);
            this.Controls.Add(this._btnStartMeasure);
            this.Controls.Add(this._btnStopMeasure);
            this.Controls.Add(this.groupBox7);
            this.Controls.Add(this._lblSettingAttention);
            this.Controls.Add(this._grpReceived);
            this.Controls.Add(this._grpMeasureRange);
            this.Controls.Add(this._grpHead);
            this.Controls.Add(this._btnGetProfile);
            this.Controls.Add(this._grpExport);
            this.Controls.Add(this._grpLog);
            this.Controls.Add(this._btnEthernetOpen);
            this.Controls.Add(this._pnlDeviceId);
            this.Controls.Add(this._lblConectedDevice);
            this.Controls.Add(this.Init_LJ);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormLJ";
            this.Text = "FormLJ";
            this.groupBox7.ResumeLayout(false);
            this._grpReceived.ResumeLayout(false);
            this._grpReceived.PerformLayout();
            this._grpMeasureRange.ResumeLayout(false);
            this._grpMeasureRange.PerformLayout();
            this._grpHead.ResumeLayout(false);
            this._grpHead.PerformLayout();
            this._grpExport.ResumeLayout(false);
            this._grpExport.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._nudProfileNo)).EndInit();
            this._grpLog.ResumeLayout(false);
            this._grpLog.PerformLayout();
            this._pnlDeviceId.ResumeLayout(false);
            this._pnlDeviceId.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _btnUsbOpen;
        private System.Windows.Forms.Button _btnStartMeasure;
        private System.Windows.Forms.Button _btnStopMeasure;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Button _btnStartStorage;
        private System.Windows.Forms.Button _btnStopStorage;
        private System.Windows.Forms.Button _btnGetStorageStatus;
        private System.Windows.Forms.Button _btnGetStorageData;
        private System.Windows.Forms.Button _btnGetStorageProfile;
        private System.Windows.Forms.Button _btnGetStorageBatchProfile;
        private System.Windows.Forms.Label _lblSettingAttention;
        private System.Windows.Forms.GroupBox _grpReceived;
        private System.Windows.Forms.CheckBox _chkboxEnvelope;
        private System.Windows.Forms.ComboBox _cmbCompressX;
        private System.Windows.Forms.Label _lblCompressX;
        private System.Windows.Forms.GroupBox _grpMeasureRange;
        private System.Windows.Forms.ComboBox _cmbReceivedBinning;
        private System.Windows.Forms.ComboBox _cmbMeasureX;
        private System.Windows.Forms.Label _lblReceivedBinning;
        private System.Windows.Forms.Label _lblMeasureX;
        private System.Windows.Forms.GroupBox _grpHead;
        private System.Windows.Forms.RadioButton _rdbtnOneHead;
        private System.Windows.Forms.RadioButton _rdbtnTwoHead;
        private System.Windows.Forms.RadioButton _rdbtnWide;
        private System.Windows.Forms.Button _btnGetProfile;
        private System.Windows.Forms.GroupBox _grpExport;
        private System.Windows.Forms.NumericUpDown _nudProfileNo;
        private System.Windows.Forms.TextBox _txtboxProfileFilePath;
        private System.Windows.Forms.Button _btnSaveMeasureData;
        private System.Windows.Forms.Button _btnSave;
        private System.Windows.Forms.Button _btnProfileFileSave;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label _lblSavePath;
        private System.Windows.Forms.GroupBox _grpLog;
        private System.Windows.Forms.TextBox _txtboxLog;
        private System.Windows.Forms.Button _btnLogClear;
        private System.Windows.Forms.Button _btnEthernetOpen;
        private System.Windows.Forms.Panel _pnlDeviceId;
        private System.Windows.Forms.Label _lblDeviceStatus5;
        private System.Windows.Forms.Label _lblDeviceStatus4;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label _lblReceiveProfileCount5;
        private System.Windows.Forms.Label _lblReceiveProfileCount4;
        private System.Windows.Forms.Label _lblReceiveProfileCount3;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label _lblReceiveProfileCount0;
        private System.Windows.Forms.Label _lblReceiveProfileCount1;
        private System.Windows.Forms.Label _lblReceiveProfileCount2;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label _lblDeviceStatus3;
        private System.Windows.Forms.Label _lblDeviceStatus2;
        private System.Windows.Forms.Label _lblDeviceStatus1;
        private System.Windows.Forms.Label _lblDeviceStatus0;
        private System.Windows.Forms.RadioButton _rdDevice3;
        private System.Windows.Forms.RadioButton _rdDevice2;
        private System.Windows.Forms.RadioButton _rdDevice1;
        private System.Windows.Forms.RadioButton _rdDevice0;
        private System.Windows.Forms.Label _lblConectedDevice;
        private System.Windows.Forms.Button Init_LJ;
        private System.Windows.Forms.SaveFileDialog _profileFileSave;
        private System.Windows.Forms.Button _btnGetBatchProfileData;
        private System.Windows.Forms.Button _btnCommClose;
        private System.Windows.Forms.Button _btnClearMemory;
        private System.Windows.Forms.Button _btnSetSetting;
    }
}