namespace KeyenceLJ.KeyenceForm
{
    partial class GetStorageDataForm
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
            this._txtboxDataCnt = new System.Windows.Forms.TextBox();
            this._txtboxStartNo = new System.Windows.Forms.TextBox();
            this._txtboxSurface = new System.Windows.Forms.TextBox();
            this._lblDataCnt = new System.Windows.Forms.Label();
            this._lblStartNo = new System.Windows.Forms.Label();
            this._lblSurface = new System.Windows.Forms.Label();
            this._btnCancel = new System.Windows.Forms.Button();
            this._btnOk = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _txtboxDataCnt
            // 
            this._txtboxDataCnt.Location = new System.Drawing.Point(14, 105);
            this._txtboxDataCnt.Name = "_txtboxDataCnt";
            this._txtboxDataCnt.Size = new System.Drawing.Size(100, 21);
            this._txtboxDataCnt.TabIndex = 34;
            this._txtboxDataCnt.Text = "1";
            // 
            // _txtboxStartNo
            // 
            this._txtboxStartNo.Location = new System.Drawing.Point(14, 65);
            this._txtboxStartNo.Name = "_txtboxStartNo";
            this._txtboxStartNo.Size = new System.Drawing.Size(100, 21);
            this._txtboxStartNo.TabIndex = 33;
            this._txtboxStartNo.Text = "0";
            // 
            // _txtboxSurface
            // 
            this._txtboxSurface.Location = new System.Drawing.Point(14, 28);
            this._txtboxSurface.Name = "_txtboxSurface";
            this._txtboxSurface.Size = new System.Drawing.Size(100, 21);
            this._txtboxSurface.TabIndex = 32;
            this._txtboxSurface.Text = "0";
            // 
            // _lblDataCnt
            // 
            this._lblDataCnt.AutoSize = true;
            this._lblDataCnt.Location = new System.Drawing.Point(128, 108);
            this._lblDataCnt.Name = "_lblDataCnt";
            this._lblDataCnt.Size = new System.Drawing.Size(143, 12);
            this._lblDataCnt.TabIndex = 39;
            this._lblDataCnt.Text = "Number of items to read";
            // 
            // _lblStartNo
            // 
            this._lblStartNo.AutoSize = true;
            this._lblStartNo.Location = new System.Drawing.Point(128, 72);
            this._lblStartNo.Name = "_lblStartNo";
            this._lblStartNo.Size = new System.Drawing.Size(203, 12);
            this._lblStartNo.TabIndex = 38;
            this._lblStartNo.Text = "Data number to start reading from";
            // 
            // _lblSurface
            // 
            this._lblSurface.AutoSize = true;
            this._lblSurface.Location = new System.Drawing.Point(128, 31);
            this._lblSurface.Name = "_lblSurface";
            this._lblSurface.Size = new System.Drawing.Size(143, 12);
            this._lblSurface.TabIndex = 37;
            this._lblSurface.Text = "Storage surface to read";
            // 
            // _btnCancel
            // 
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.Location = new System.Drawing.Point(221, 145);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(75, 25);
            this._btnCancel.TabIndex = 36;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;
            // 
            // _btnOk
            // 
            this._btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._btnOk.Location = new System.Drawing.Point(130, 145);
            this._btnOk.Name = "_btnOk";
            this._btnOk.Size = new System.Drawing.Size(75, 25);
            this._btnOk.TabIndex = 35;
            this._btnOk.Text = "OK";
            this._btnOk.UseVisualStyleBackColor = true;
            // 
            // GetStorageDataForm
            // 
            this.AcceptButton = this._btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(344, 186);
            this.Controls.Add(this._txtboxDataCnt);
            this.Controls.Add(this._txtboxStartNo);
            this.Controls.Add(this._txtboxSurface);
            this.Controls.Add(this._lblDataCnt);
            this.Controls.Add(this._lblStartNo);
            this.Controls.Add(this._lblSurface);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._btnOk);
            this.Name = "GetStorageDataForm";
            this.Text = "GetStorageDataForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _txtboxDataCnt;
        private System.Windows.Forms.TextBox _txtboxStartNo;
        private System.Windows.Forms.TextBox _txtboxSurface;
        private System.Windows.Forms.Label _lblDataCnt;
        private System.Windows.Forms.Label _lblStartNo;
        private System.Windows.Forms.Label _lblSurface;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.Button _btnOk;
    }
}