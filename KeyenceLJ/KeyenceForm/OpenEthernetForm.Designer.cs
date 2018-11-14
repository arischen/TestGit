namespace KeyenceLJ.KeyenceForm
{
    partial class OpenEthernetForm
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
            this._lblDescription = new System.Windows.Forms.Label();
            this._txtboxPort = new System.Windows.Forms.TextBox();
            this._btnCancel = new System.Windows.Forms.Button();
            this._btnOk = new System.Windows.Forms.Button();
            this._lblPort = new System.Windows.Forms.Label();
            this._txtboxIpFourthSegment = new System.Windows.Forms.TextBox();
            this._txtboxIpThirdSegment = new System.Windows.Forms.TextBox();
            this._txtboxIpSecondSegment = new System.Windows.Forms.TextBox();
            this._txtboxIpFirstSegment = new System.Windows.Forms.TextBox();
            this._lblIpAddress = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _lblDescription
            // 
            this._lblDescription.AutoSize = true;
            this._lblDescription.Location = new System.Drawing.Point(36, 26);
            this._lblDescription.Name = "_lblDescription";
            this._lblDescription.Size = new System.Drawing.Size(461, 12);
            this._lblDescription.TabIndex = 29;
            this._lblDescription.Text = "[Valid range] The IP address is a byte value and the port is a ushort value.";
            // 
            // _txtboxPort
            // 
            this._txtboxPort.Location = new System.Drawing.Point(105, 86);
            this._txtboxPort.Name = "_txtboxPort";
            this._txtboxPort.Size = new System.Drawing.Size(194, 21);
            this._txtboxPort.TabIndex = 28;
            this._txtboxPort.Text = "24691";
            // 
            // _btnCancel
            // 
            this._btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.Location = new System.Drawing.Point(350, 125);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(75, 25);
            this._btnCancel.TabIndex = 27;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;
            // 
            // _btnOk
            // 
            this._btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._btnOk.Location = new System.Drawing.Point(241, 125);
            this._btnOk.Name = "_btnOk";
            this._btnOk.Size = new System.Drawing.Size(75, 25);
            this._btnOk.TabIndex = 26;
            this._btnOk.Text = "OK";
            this._btnOk.UseVisualStyleBackColor = true;
            // 
            // _lblPort
            // 
            this._lblPort.AutoSize = true;
            this._lblPort.Location = new System.Drawing.Point(36, 90);
            this._lblPort.Name = "_lblPort";
            this._lblPort.Size = new System.Drawing.Size(29, 12);
            this._lblPort.TabIndex = 25;
            this._lblPort.Text = "Port";
            // 
            // _txtboxIpFourthSegment
            // 
            this._txtboxIpFourthSegment.Location = new System.Drawing.Point(255, 53);
            this._txtboxIpFourthSegment.Name = "_txtboxIpFourthSegment";
            this._txtboxIpFourthSegment.Size = new System.Drawing.Size(44, 21);
            this._txtboxIpFourthSegment.TabIndex = 24;
            this._txtboxIpFourthSegment.Text = "1";
            // 
            // _txtboxIpThirdSegment
            // 
            this._txtboxIpThirdSegment.Location = new System.Drawing.Point(205, 53);
            this._txtboxIpThirdSegment.Name = "_txtboxIpThirdSegment";
            this._txtboxIpThirdSegment.Size = new System.Drawing.Size(44, 21);
            this._txtboxIpThirdSegment.TabIndex = 23;
            this._txtboxIpThirdSegment.Text = "0";
            // 
            // _txtboxIpSecondSegment
            // 
            this._txtboxIpSecondSegment.Location = new System.Drawing.Point(155, 53);
            this._txtboxIpSecondSegment.Name = "_txtboxIpSecondSegment";
            this._txtboxIpSecondSegment.Size = new System.Drawing.Size(44, 21);
            this._txtboxIpSecondSegment.TabIndex = 22;
            this._txtboxIpSecondSegment.Text = "168";
            // 
            // _txtboxIpFirstSegment
            // 
            this._txtboxIpFirstSegment.Location = new System.Drawing.Point(105, 53);
            this._txtboxIpFirstSegment.Name = "_txtboxIpFirstSegment";
            this._txtboxIpFirstSegment.Size = new System.Drawing.Size(44, 21);
            this._txtboxIpFirstSegment.TabIndex = 21;
            this._txtboxIpFirstSegment.Text = "192";
            // 
            // _lblIpAddress
            // 
            this._lblIpAddress.AutoSize = true;
            this._lblIpAddress.Location = new System.Drawing.Point(36, 56);
            this._lblIpAddress.Name = "_lblIpAddress";
            this._lblIpAddress.Size = new System.Drawing.Size(65, 12);
            this._lblIpAddress.TabIndex = 20;
            this._lblIpAddress.Text = "IP address";
            // 
            // OpenEthernetForm
            // 
            this.AcceptButton = this._btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(516, 162);
            this.Controls.Add(this._lblDescription);
            this.Controls.Add(this._txtboxPort);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._btnOk);
            this.Controls.Add(this._lblPort);
            this.Controls.Add(this._txtboxIpFourthSegment);
            this.Controls.Add(this._txtboxIpThirdSegment);
            this.Controls.Add(this._txtboxIpSecondSegment);
            this.Controls.Add(this._txtboxIpFirstSegment);
            this.Controls.Add(this._lblIpAddress);
            this.Name = "OpenEthernetForm";
            this.Text = "OpenEthernetForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _lblDescription;
        private System.Windows.Forms.TextBox _txtboxPort;
        public System.Windows.Forms.Button _btnCancel;
        public System.Windows.Forms.Button _btnOk;
        private System.Windows.Forms.Label _lblPort;
        private System.Windows.Forms.TextBox _txtboxIpFourthSegment;
        private System.Windows.Forms.TextBox _txtboxIpThirdSegment;
        private System.Windows.Forms.TextBox _txtboxIpSecondSegment;
        private System.Windows.Forms.TextBox _txtboxIpFirstSegment;
        private System.Windows.Forms.Label _lblIpAddress;
    }
}