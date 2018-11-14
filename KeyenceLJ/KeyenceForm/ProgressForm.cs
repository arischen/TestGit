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
    #region Enum
    /// <summary>
    /// Processing status
    /// </summary>
    public enum Status
    {
        Communicating = 0,
        Saving,
    };
    #endregion
    public partial class ProgressForm : Form
    {
        #region Field
        /// <summary>
        /// Processing status
        /// </summary>
        private Status _status;
        #endregion

        #region Properety
        /// <summary>
        /// Processing status
        /// </summary>
        public Status Status
        {
            set
            {
                _status = value;
                switch (_status)
                {
                    case Status.Communicating:
                        _lblStatus.Text = "Communicating";
                        break;
                    case Status.Saving:
                        _lblStatus.Text = "Saving the file";
                        break;
                    default:
                        return;
                }
            }
            get { return _status; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public ProgressForm()
        {
            InitializeComponent();
            _status = Status.Communicating;
        }
        #endregion
    }
}
