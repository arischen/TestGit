namespace KeyenceLJ.KeyenceForm
{
    partial class GetStorageStatusForm
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
            this._txtboxInputValue = new System.Windows.Forms.TextBox();
            this._btnCancel = new System.Windows.Forms.Button();
            this._btnOk = new System.Windows.Forms.Button();
            this._lblSendPos = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _txtboxInputValue
            // 
            this._txtboxInputValue.Location = new System.Drawing.Point(159, 31);
            this._txtboxInputValue.Name = "_txtboxInputValue";
            this._txtboxInputValue.Size = new System.Drawing.Size(107, 21);
            this._txtboxInputValue.TabIndex = 24;
            this._txtboxInputValue.Text = "0";
            // 
            // _btnCancel
            // 
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.Location = new System.Drawing.Point(191, 70);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(75, 25);
            this._btnCancel.TabIndex = 23;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;
            // 
            // _btnOk
            // 
            this._btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._btnOk.Location = new System.Drawing.Point(110, 71);
            this._btnOk.Name = "_btnOk";
            this._btnOk.Size = new System.Drawing.Size(75, 25);
            this._btnOk.TabIndex = 22;
            this._btnOk.Text = "OK";
            this._btnOk.UseVisualStyleBackColor = true;
            // 
            // _lblSendPos
            // 
            this._lblSendPos.AutoSize = true;
            this._lblSendPos.Location = new System.Drawing.Point(14, 36);
            this._lblSendPos.Name = "_lblSendPos";
            this._lblSendPos.Size = new System.Drawing.Size(137, 12);
            this._lblSendPos.TabIndex = 21;
            this._lblSendPos.Text = "Target surface to read";
            // 
            // GetStorageStatusForm
            // 
            this.AcceptButton = this._btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 127);
            this.Controls.Add(this._txtboxInputValue);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._btnOk);
            this.Controls.Add(this._lblSendPos);
            this.Name = "GetStorageStatusForm";
            this.Text = "GetStorageStatusForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _txtboxInputValue;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.Button _btnOk;
        private System.Windows.Forms.Label _lblSendPos;
    }
}