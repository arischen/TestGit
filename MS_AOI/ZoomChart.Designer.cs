namespace MS_AOI
{
    partial class ZoomChart
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.cht_FAIGraph0 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.cht_FAIGraph0)).BeginInit();
            this.SuspendLayout();
            // 
            // cht_FAIGraph0
            // 
            chartArea1.Name = "ChartArea1";
            this.cht_FAIGraph0.ChartAreas.Add(chartArea1);
            this.cht_FAIGraph0.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Name = "Legend1";
            this.cht_FAIGraph0.Legends.Add(legend1);
            this.cht_FAIGraph0.Location = new System.Drawing.Point(0, 0);
            this.cht_FAIGraph0.Margin = new System.Windows.Forms.Padding(10);
            this.cht_FAIGraph0.Name = "cht_FAIGraph0";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.cht_FAIGraph0.Series.Add(series1);
            this.cht_FAIGraph0.Size = new System.Drawing.Size(1380, 946);
            this.cht_FAIGraph0.TabIndex = 0;
            this.cht_FAIGraph0.Text = "chart1";
            this.cht_FAIGraph0.Click += new System.EventHandler(this.cht_FAIGraph0_Click);
            // 
            // ZoomChart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(65)))), ((int)(((byte)(65)))));
            this.ClientSize = new System.Drawing.Size(1380, 946);
            this.Controls.Add(this.cht_FAIGraph0);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ZoomChart";
            this.Text = "ZoomChart";
            ((System.ComponentModel.ISupportInitialize)(this.cht_FAIGraph0)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart cht_FAIGraph0;
    }
}