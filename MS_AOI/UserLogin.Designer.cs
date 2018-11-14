namespace MS_AOI
{
    partial class UserLogin
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_UserName = new System.Windows.Forms.TextBox();
            this.txt_UserPassWord = new System.Windows.Forms.TextBox();
            this.btn_Login = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(65)))), ((int)(((byte)(65)))));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(113, 86);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "用户名";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(65)))), ((int)(((byte)(65)))));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(113, 144);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "密码";
            // 
            // txt_UserName
            // 
            this.txt_UserName.Location = new System.Drawing.Point(198, 83);
            this.txt_UserName.Name = "txt_UserName";
            this.txt_UserName.Size = new System.Drawing.Size(186, 25);
            this.txt_UserName.TabIndex = 0;
            // 
            // txt_UserPassWord
            // 
            this.txt_UserPassWord.Location = new System.Drawing.Point(198, 141);
            this.txt_UserPassWord.Name = "txt_UserPassWord";
            this.txt_UserPassWord.PasswordChar = '*';
            this.txt_UserPassWord.Size = new System.Drawing.Size(186, 25);
            this.txt_UserPassWord.TabIndex = 1;
            // 
            // btn_Login
            // 
            this.btn_Login.Location = new System.Drawing.Point(116, 198);
            this.btn_Login.Name = "btn_Login";
            this.btn_Login.Size = new System.Drawing.Size(83, 30);
            this.btn_Login.TabIndex = 2;
            this.btn_Login.Text = "登 录";
            this.btn_Login.UseVisualStyleBackColor = true;
            this.btn_Login.Click += new System.EventHandler(this.btn_Login_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Location = new System.Drawing.Point(344, 198);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(83, 30);
            this.btn_Cancel.TabIndex = 3;
            this.btn_Cancel.Text = "取 消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // UserLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(65)))), ((int)(((byte)(65)))));
            this.ClientSize = new System.Drawing.Size(540, 300);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_Login);
            this.Controls.Add(this.txt_UserPassWord);
            this.Controls.Add(this.txt_UserName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(540, 300);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(540, 300);
            this.Name = "UserLogin";
            this.Text = "用户登录";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UserLogin_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_UserName;
        private System.Windows.Forms.TextBox txt_UserPassWord;
        private System.Windows.Forms.Button btn_Login;
        private System.Windows.Forms.Button btn_Cancel;
    }
}