using LogicControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MS_AOI
{
    public partial class UserLogin : Form
    {

        public delegate void UpdateObjectDelegate(object sender);
        public event UpdateObjectDelegate UpdateUserLevel;       //返回用户等级
        private LogicModule logicModule;

        public UserLogin(ref LogicModule logic)
        {
            InitializeComponent();
            logicModule = logic;
            UpdateUserLevel += new UserLogin.UpdateObjectDelegate(logicModule.UpdateUserLevel);
        }

        private void btn_Login_Click(object sender, EventArgs e)
        {
            string name = txt_UserName.Text;
            string password = txt_UserPassWord.Text;
            name = name.Trim();
            password = password.Trim();
            if (name.Equals("Admin") && password.Equals("12345"))
            {
                UpdateUserLevel(100);
                if (MessageBox.Show("用户 " + txt_UserName.Text + " 登录成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                {
                    txt_UserName.Text = "";
                    txt_UserPassWord.Text = "";
                    this.Hide();
                    return;
                }
            }

            if (name.Equals("Engineer") && password.Equals("12345"))
            {
                UpdateUserLevel(50);
                if (MessageBox.Show("用户 " + txt_UserName.Text + " 登录成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                {
                    txt_UserName.Text = "";
                    txt_UserPassWord.Text = "";
                    this.Hide();
                    return;
                }
            }

            if (name.Equals("Op") && password.Equals("12345"))
            {
                UpdateUserLevel(10);
                if (MessageBox.Show("用户 " + txt_UserName.Text + " 登录成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                {
                    txt_UserName.Text = "";
                    txt_UserPassWord.Text = "";
                    this.Hide();
                    return;
                }
            }

            MessageBox.Show("用户 " + txt_UserName.Text + " 登录失败","提示",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            txt_UserName.Text = " ";
            txt_UserPassWord.Text = null;
            txt_UserName.Focus();
            this.Hide();
        }

        private void UserLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
