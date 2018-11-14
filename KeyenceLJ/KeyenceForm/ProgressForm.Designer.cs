namespace KeyenceLJ.KeyenceForm
{
    partial class ProgressForm
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
            this._lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _lblStatus
            // 
            this._lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblStatus.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblStatus.Location = new System.Drawing.Point(0, 0);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Size = new System.Drawing.Size(288, 116);
            this._lblStatus.TabIndex = 1;
            this._lblStatus.Text = "Processing";
            this._lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ProgressForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(288, 116);
            this.Controls.Add(this._lblStatus);
            this.Name = "ProgressForm";
            this.Text = "ProgressForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label _lblStatus;
    }
}