using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogicControl;
using TaskControlLib;

namespace MS_AOI
{
    public partial class LaserCloud : Form
    {
        private LogicModule logicModule;

        public LaserCloud(ref LogicModule logic)
        {
            InitializeComponent();
            logicModule = logic;
        }

        private void LaserCloud_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 4; i++)
                logicModule.Task3DPaths[i] = "E:\\Task3D\\Task" + i.ToString() + ".task";//Test Code

            bool resultTask = Task3DInit();
            if (resultTask)
            {
                MessageBox.Show("重新加载任务成功");
            }
            else
            {
                MessageBox.Show("加载失败！");
            }
        }

        private void Only3D_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel1Collapsed = false;
            splitContainer1.Panel2Collapsed = true;
        }

        private void Only2D_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel1Collapsed = true;
            splitContainer1.Panel2Collapsed = false;
        }

        private void together3D2D_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel1Collapsed = false;
            splitContainer1.Panel2Collapsed = false;
        }

        public bool Task3DInit()
        {
            for (int i = 0; i < logicModule.Task3DPaths.Length; i++)
            {
                logicModule.taskControl[i] = new TaskControl();
                logicModule.taskControl[i].Graph3D = map3DMain;
                logicModule.taskControl[i].ColorRuler3D = colorRulerMain;
                logicModule.taskControl[i].Graph2D = View2D;

                logicModule.taskControl[i].updateMsg += logicModule.TaskControl_updateMsg;
                logicModule.taskControl[i].OpenMeasureTask(logicModule.Task3DPaths[i]);
            }
            return true;
        }

        private void LaserCloud_Load(object sender, EventArgs e)
        {
            splitContainer1.Panel1Collapsed = false;
            splitContainer1.Panel2Collapsed = true;
        }
    }
}
