namespace MS_AOI
{
    partial class LaserCloud
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
            this.components = new System.ComponentModel.Container();
            this.contextMenuStripView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.Only3D = new System.Windows.Forms.ToolStripMenuItem();
            this.Only2D = new System.Windows.Forms.ToolStripMenuItem();
            this.together3D2D = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel3D = new System.Windows.Forms.Panel();
            this.colorRulerMain = new UserControls.ColorRuler();
            this.map3DMain = new UserControls.Map3D();
            this.panel2D = new System.Windows.Forms.Panel();
            this.View2D = new UserControls.Map2D();
            this.contextMenuStripView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel3D.SuspendLayout();
            this.panel2D.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStripView
            // 
            this.contextMenuStripView.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStripView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Only3D,
            this.Only2D,
            this.together3D2D});
            this.contextMenuStripView.Name = "contextMenuStripView";
            this.contextMenuStripView.Size = new System.Drawing.Size(179, 76);
            // 
            // Only3D
            // 
            this.Only3D.Name = "Only3D";
            this.Only3D.Size = new System.Drawing.Size(178, 24);
            this.Only3D.Text = "全显示3D";
            this.Only3D.Click += new System.EventHandler(this.Only3D_Click);
            // 
            // Only2D
            // 
            this.Only2D.Name = "Only2D";
            this.Only2D.Size = new System.Drawing.Size(178, 24);
            this.Only2D.Text = "全显示2D";
            this.Only2D.Click += new System.EventHandler(this.Only2D_Click);
            // 
            // together3D2D
            // 
            this.together3D2D.Name = "together3D2D";
            this.together3D2D.Size = new System.Drawing.Size(178, 24);
            this.together3D2D.Text = "3D2D一起显示";
            this.together3D2D.Click += new System.EventHandler(this.together3D2D_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.ContextMenuStrip = this.ContextMenuStrip;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel3D);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel2D);
            this.splitContainer1.Size = new System.Drawing.Size(917, 555);
            this.splitContainer1.SplitterDistance = 470;
            this.splitContainer1.TabIndex = 2;
            // 
            // panel3D
            // 
            this.panel3D.ContextMenuStrip = this.contextMenuStripView;
            this.panel3D.Controls.Add(this.colorRulerMain);
            this.panel3D.Controls.Add(this.map3DMain);
            this.panel3D.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3D.Location = new System.Drawing.Point(0, 0);
            this.panel3D.Name = "panel3D";
            this.panel3D.Size = new System.Drawing.Size(470, 555);
            this.panel3D.TabIndex = 0;
            // 
            // colorRulerMain
            // 
            this.colorRulerMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.colorRulerMain.Dock = System.Windows.Forms.DockStyle.Right;
            this.colorRulerMain.DownColor = System.Drawing.Color.DarkBlue;
            this.colorRulerMain.Location = new System.Drawing.Point(411, 0);
            this.colorRulerMain.Margin = new System.Windows.Forms.Padding(0);
            this.colorRulerMain.Name = "colorRulerMain";
            this.colorRulerMain.Size = new System.Drawing.Size(59, 555);
            this.colorRulerMain.TabIndex = 2;
            this.colorRulerMain.UpColor = System.Drawing.Color.Gray;
            this.colorRulerMain.Visible = false;
            // 
            // map3DMain
            // 
            this.map3DMain.AxisColorX = System.Drawing.Color.Red;
            this.map3DMain.AxisColorY = System.Drawing.Color.Green;
            this.map3DMain.AxisColorZ = System.Drawing.Color.Blue;
            this.map3DMain.ColorDisMax = 5D;
            this.map3DMain.ColorDisMin = -5D;
            this.map3DMain.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.map3DMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.map3DMain.EnableMouseEvents = true;
            this.map3DMain.EnableZoom = true;
            this.map3DMain.ForeColor = System.Drawing.Color.Yellow;
            this.map3DMain.GridCellCount = 10;
            this.map3DMain.GridShowXY = false;
            this.map3DMain.GridShowXZ = false;
            this.map3DMain.GridShowYZ = false;
            this.map3DMain.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.map3DMain.IsUseTexture = false;
            this.map3DMain.Location = new System.Drawing.Point(0, 0);
            this.map3DMain.Margin = new System.Windows.Forms.Padding(0);
            this.map3DMain.Name = "map3DMain";
            this.map3DMain.PixelsPerMM = 50D;
            this.map3DMain.Size = new System.Drawing.Size(470, 555);
            this.map3DMain.TabIndex = 1;
            this.map3DMain.Tag = "";
            this.map3DMain.ViewMode = CommonStruct.LC3D.ViewMode.Default;
            this.map3DMain.Zoom = 1D;
            // 
            // panel2D
            // 
            this.panel2D.ContextMenuStrip = this.contextMenuStripView;
            this.panel2D.Controls.Add(this.View2D);
            this.panel2D.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2D.Location = new System.Drawing.Point(0, 0);
            this.panel2D.Name = "panel2D";
            this.panel2D.Size = new System.Drawing.Size(443, 555);
            this.panel2D.TabIndex = 0;
            // 
            // View2D
            // 
            this.View2D.AxisColorX = System.Drawing.Color.Red;
            this.View2D.AxisColorY = System.Drawing.Color.Green;
            this.View2D.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.View2D.Dock = System.Windows.Forms.DockStyle.Fill;
            this.View2D.EnableMouseEvents = true;
            this.View2D.EnableZoom = true;
            this.View2D.ForeColor = System.Drawing.Color.Yellow;
            this.View2D.GridCellCount = 10;
            this.View2D.GridCellInterval = 0.5F;
            this.View2D.GridLineColorXY = System.Drawing.Color.White;
            this.View2D.GridLineWidth = 1F;
            this.View2D.GridShowXY = false;
            this.View2D.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.View2D.IsUseTexture = false;
            this.View2D.Location = new System.Drawing.Point(0, 0);
            this.View2D.Margin = new System.Windows.Forms.Padding(0);
            this.View2D.Name = "View2D";
            this.View2D.PathTexture = "";
            this.View2D.PixelsPerMM = 50D;
            this.View2D.Size = new System.Drawing.Size(443, 555);
            this.View2D.TabIndex = 1;
            this.View2D.Tag = "";
            this.View2D.ViewMode = CommonStruct.LC3D.ViewMode.Top;
            this.View2D.Zoom = 1D;
            // 
            // LaserCloud
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(917, 555);
            this.Controls.Add(this.splitContainer1);
            this.Name = "LaserCloud";
            this.Text = "Laser点云显示";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LaserCloud_FormClosing);
            this.Load += new System.EventHandler(this.LaserCloud_Load);
            this.contextMenuStripView.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel3D.ResumeLayout(false);
            this.panel2D.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ContextMenuStrip contextMenuStripView;
        private System.Windows.Forms.ToolStripMenuItem Only3D;
        private System.Windows.Forms.ToolStripMenuItem Only2D;
        private System.Windows.Forms.ToolStripMenuItem together3D2D;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel3D;
        private UserControls.ColorRuler colorRulerMain;
        private UserControls.Map3D map3DMain;
        private System.Windows.Forms.Panel panel2D;
        private UserControls.Map2D View2D;
    }
}