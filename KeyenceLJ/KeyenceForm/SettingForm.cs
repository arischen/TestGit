using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeyenceLJ.KeyenceForm
{
    public partial class SettingForm : Form
    {
        #region Field
        /// <summary>
        /// Specify to what level the sent settings will be reflected (LJV7IF_SETTING_DEPTH).
        /// </summary>
        private byte _depth;

        /// <summary>
        /// Identify the item that is the target to send.
        /// </summary>
        private LJV7IF_TARGET_SETTING _targetSetting;

        /// <summary>
        /// Specify the buffer that stores the setting data to send.
        /// </summary>
        private byte[] _data;
        #endregion

        #region Property
        /// <summary>
        /// Specify to what level the sent settings will be reflected (LJV7IF_SETTING_DEPTH).
        /// </summary>
        public byte Depth
        {
            get { return _depth; }
        }

        /// <summary>
        /// Identify the item that is the target to send.
        /// </summary>
        public LJV7IF_TARGET_SETTING TargetSetting
        {
            get { return _targetSetting; }
        }
        public int DataLength
        {
            get { return Convert.ToInt32(_txtDataLength.Text); }
        }
        /// <summary>
        /// Specify the buffer that stores the setting data to send.
        /// </summary>
        public byte[] Data
        {
            get { return _data; }
        }
        #endregion

        #region Event
        private void SettingForm_Load(object sender, EventArgs e)
        {
            _txtboxDepth.Text = Convert.ToString(_depth, 16);
            _txtboxType.Text = Convert.ToString(_targetSetting.byType, 16);
            _txtboxCategor.Text = Convert.ToString(_targetSetting.byCategory, 16);
            _txtboxItem.Text = Convert.ToString(_targetSetting.byItem, 16);
            _txtboxTarget1.Text = Convert.ToString(_targetSetting.byTarget1, 16);
            _txtboxTarget2.Text = Convert.ToString(_targetSetting.byTarget2, 16);
            _txtboxTarget3.Text = Convert.ToString(_targetSetting.byTarget3, 16);
            _txtboxTarget4.Text = Convert.ToString(_targetSetting.byTarget4, 16);
            _txtboxParameter.Text = Convert.ToString(_data[0], 16);
        }

        /// <summary>
        /// Close start event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                try
                {
                    _depth = Convert.ToByte(_txtboxDepth.Text, 16);
                    _targetSetting.byType = Convert.ToByte(_txtboxType.Text, 16);
                    _targetSetting.byCategory = Convert.ToByte(_txtboxCategor.Text, 16);
                    _targetSetting.byItem = Convert.ToByte(_txtboxItem.Text, 16);
                    _targetSetting.byTarget1 = Convert.ToByte(_txtboxTarget1.Text, 16);
                    _targetSetting.byTarget2 = Convert.ToByte(_txtboxTarget2.Text, 16);
                    _targetSetting.byTarget3 = Convert.ToByte(_txtboxTarget3.Text, 16);
                    _targetSetting.byTarget4 = Convert.ToByte(_txtboxTarget4.Text, 16);

                    char[] trimChars = new char[] { ' ', ',' };
                    string trimStr = _txtboxParameter.Text.Trim(trimChars);
                    if (trimStr.Length > 0)
                    {
                        string[] aSrc = trimStr.Split(',');
                        if (aSrc.Length > 0)
                        {
                            _data = Array.ConvertAll<string, byte>(aSrc,
                                delegate (string s) { return Convert.ToByte(s, 16); });
                        }
                    }
                    Array.Resize(ref _data, Convert.ToInt32(_txtDataLength.Text));//amount of data byte
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message);
                    e.Cancel = true;
                    return;
                }
            }

            base.OnClosing(e);
        }
        #endregion

        #region Method
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isSetSetting">True when sending settings, false when getting settings</param>
        public SettingForm(bool isSetSetting)
        {
            InitializeComponent();

            // Control display control
            _txtboxParameter.Visible = isSetSetting;
            _lblParameter.Visible = isSetSetting;

            // Settings that change the sampling period of the running area to "100 Hz"
            _depth = 0x01;                      // Setting depth: Running settings area
            _targetSetting.byType = 0x10;       // Setting type: Program number 0
            _targetSetting.byCategory = 0x0;    // Category: Trigger settings
            _targetSetting.byItem = 0x02;       // Setting item: Sampling period
            _targetSetting.byTarget1 = 0x0;     // Setting target 1: None
            _targetSetting.byTarget2 = 0x0;     // Setting target 2: None
            _targetSetting.byTarget3 = 0x0;     // Setting target 3: None
            _targetSetting.byTarget4 = 0x0;     // Setting target 4: None


            _data = new byte[NativeMethods.ProgramSettingSize];
            _data[0] = (byte)0x3;
        }
        #endregion
    }
}
