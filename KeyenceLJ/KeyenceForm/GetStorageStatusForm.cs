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
    public partial class GetStorageStatusForm : Form
    {
        #region Field
        /// <summary>
        /// Storage status request structure
        /// </summary>
        private LJV7IF_GET_STRAGE_STATUS_REQ _req;
        #endregion

        #region Property
        /// <summary>
        /// Storage status request structure
        /// </summary>
        public LJV7IF_GET_STRAGE_STATUS_REQ Req
        {
            get { return _req; }
        }
        #endregion

        #region Event
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
                    _req.dwRdArea = Convert.ToUInt32(_txtboxInputValue.Text);
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
        public GetStorageStatusForm()
        {
            InitializeComponent();

            // Field initialization
            _req = new LJV7IF_GET_STRAGE_STATUS_REQ();
        }
        #endregion
    }
}
