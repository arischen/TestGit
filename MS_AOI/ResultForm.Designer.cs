namespace MS_AOI
{
    partial class ResultForm
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
            this.dgvResult = new System.Windows.Forms.DataGridView();
            this.btnQuery = new System.Windows.Forms.Button();
            this.cbxSelectType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dtpStartTime = new System.Windows.Forms.DateTimePicker();
            this.grbDate = new System.Windows.Forms.GroupBox();
            this.dtpEndTime = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.grbBarcode = new System.Windows.Forms.GroupBox();
            this.tbBarcode = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResult)).BeginInit();
            this.grbDate.SuspendLayout();
            this.grbBarcode.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvResult
            // 
            this.dgvResult.AllowUserToAddRows = false;
            this.dgvResult.AllowUserToDeleteRows = false;
            this.dgvResult.AllowUserToResizeColumns = false;
            this.dgvResult.AllowUserToResizeRows = false;
            this.dgvResult.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResult.Location = new System.Drawing.Point(30, 122);
            this.dgvResult.Name = "dgvResult";
            this.dgvResult.RowTemplate.Height = 27;
            this.dgvResult.Size = new System.Drawing.Size(964, 392);
            this.dgvResult.TabIndex = 0;
            // 
            // btnQuery
            // 
            this.btnQuery.Location = new System.Drawing.Point(30, 531);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(189, 47);
            this.btnQuery.TabIndex = 1;
            this.btnQuery.Text = "查询";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // cbxSelectType
            // 
            this.cbxSelectType.FormattingEnabled = true;
            this.cbxSelectType.Items.AddRange(new object[] {
            "日期",
            "条码"});
            this.cbxSelectType.Location = new System.Drawing.Point(130, 30);
            this.cbxSelectType.Name = "cbxSelectType";
            this.cbxSelectType.Size = new System.Drawing.Size(121, 23);
            this.cbxSelectType.TabIndex = 2;
            this.cbxSelectType.SelectedIndexChanged += new System.EventHandler(this.cbxSelectType_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "选择查询条件";
            // 
            // dtpStartTime
            // 
            this.dtpStartTime.CustomFormat = "yyyy/MM/dd HH:mm:ss";
            this.dtpStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStartTime.Location = new System.Drawing.Point(88, 24);
            this.dtpStartTime.Name = "dtpStartTime";
            this.dtpStartTime.Size = new System.Drawing.Size(222, 25);
            this.dtpStartTime.TabIndex = 4;
            // 
            // grbDate
            // 
            this.grbDate.Controls.Add(this.dtpEndTime);
            this.grbDate.Controls.Add(this.label3);
            this.grbDate.Controls.Add(this.label2);
            this.grbDate.Controls.Add(this.dtpStartTime);
            this.grbDate.Enabled = false;
            this.grbDate.Location = new System.Drawing.Point(270, 30);
            this.grbDate.Name = "grbDate";
            this.grbDate.Size = new System.Drawing.Size(325, 88);
            this.grbDate.TabIndex = 5;
            this.grbDate.TabStop = false;
            this.grbDate.Text = "日期";
            // 
            // dtpEndTime
            // 
            this.dtpEndTime.CustomFormat = "yyyy/MM/dd HH:mm:ss";
            this.dtpEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEndTime.Location = new System.Drawing.Point(88, 58);
            this.dtpEndTime.Name = "dtpEndTime";
            this.dtpEndTime.Size = new System.Drawing.Size(222, 25);
            this.dtpEndTime.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 15);
            this.label3.TabIndex = 6;
            this.label3.Text = "结束时间：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "起始时间：";
            // 
            // grbBarcode
            // 
            this.grbBarcode.Controls.Add(this.tbBarcode);
            this.grbBarcode.Controls.Add(this.label5);
            this.grbBarcode.Enabled = false;
            this.grbBarcode.Location = new System.Drawing.Point(601, 28);
            this.grbBarcode.Name = "grbBarcode";
            this.grbBarcode.Size = new System.Drawing.Size(368, 88);
            this.grbBarcode.TabIndex = 8;
            this.grbBarcode.TabStop = false;
            this.grbBarcode.Text = "条码";
            // 
            // tbBarcode
            // 
            this.tbBarcode.Location = new System.Drawing.Point(6, 48);
            this.tbBarcode.Name = "tbBarcode";
            this.tbBarcode.Size = new System.Drawing.Size(362, 25);
            this.tbBarcode.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(112, 15);
            this.label5.TabIndex = 5;
            this.label5.Text = "输入查询条码：";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(589, 541);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(207, 37);
            this.button1.TabIndex = 9;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ResultForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1022, 602);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.grbBarcode);
            this.Controls.Add(this.grbDate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbxSelectType);
            this.Controls.Add(this.btnQuery);
            this.Controls.Add(this.dgvResult);
            this.Name = "ResultForm";
            this.Text = "ResultForm";
            ((System.ComponentModel.ISupportInitialize)(this.dgvResult)).EndInit();
            this.grbDate.ResumeLayout(false);
            this.grbDate.PerformLayout();
            this.grbBarcode.ResumeLayout(false);
            this.grbBarcode.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvResult;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.ComboBox cbxSelectType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtpStartTime;
        private System.Windows.Forms.GroupBox grbDate;
        private System.Windows.Forms.DateTimePicker dtpEndTime;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox grbBarcode;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbBarcode;
        private System.Windows.Forms.Button button1;
    }
}