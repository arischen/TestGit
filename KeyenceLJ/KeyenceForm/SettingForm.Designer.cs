namespace KeyenceLJ.KeyenceForm
{
    partial class SettingForm
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
            this._txtDataLength = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._txtboxParameter = new System.Windows.Forms.TextBox();
            this._lblParameter = new System.Windows.Forms.Label();
            this._lblDepth = new System.Windows.Forms.Label();
            this._txtboxDepth = new System.Windows.Forms.TextBox();
            this._lblHexDepth = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this._lblTarget4 = new System.Windows.Forms.Label();
            this._txtboxTarget4 = new System.Windows.Forms.TextBox();
            this._lblHexTarget4 = new System.Windows.Forms.Label();
            this._lblTarget3 = new System.Windows.Forms.Label();
            this._txtboxTarget3 = new System.Windows.Forms.TextBox();
            this._lblHexTarget3 = new System.Windows.Forms.Label();
            this._lblTarget2 = new System.Windows.Forms.Label();
            this._txtboxTarget2 = new System.Windows.Forms.TextBox();
            this._lblHexTarget2 = new System.Windows.Forms.Label();
            this._lblTarget1 = new System.Windows.Forms.Label();
            this._txtboxTarget1 = new System.Windows.Forms.TextBox();
            this._lblHexTarget1 = new System.Windows.Forms.Label();
            this._lblItem = new System.Windows.Forms.Label();
            this._txtboxItem = new System.Windows.Forms.TextBox();
            this._lblHexItem = new System.Windows.Forms.Label();
            this._lblCategor = new System.Windows.Forms.Label();
            this._txtboxCategor = new System.Windows.Forms.TextBox();
            this._lblHexCategor = new System.Windows.Forms.Label();
            this._lblType = new System.Windows.Forms.Label();
            this._txtboxType = new System.Windows.Forms.TextBox();
            this._lblHexType = new System.Windows.Forms.Label();
            this._btnCancel = new System.Windows.Forms.Button();
            this._btnOk = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _txtDataLength
            // 
            this._txtDataLength.Location = new System.Drawing.Point(118, 371);
            this._txtDataLength.Name = "_txtDataLength";
            this._txtDataLength.Size = new System.Drawing.Size(45, 21);
            this._txtDataLength.TabIndex = 45;
            this._txtDataLength.Text = "1";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(31, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(307, 30);
            this.label3.TabIndex = 62;
            this.label3.Text = "For details on the items that follow the category, see the tables in the communic" +
    "ation command specifications.\r\n";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(27, 135);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(344, 38);
            this.label2.TabIndex = 61;
            this.label2.Text = "0x01: Environment settings, 0x02: Common measurement settings, 0x10: Program 0, 0" +
    "x11: Program 1, ..., 0x1F: Program 15";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(31, 83);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(322, 23);
            this.label1.TabIndex = 60;
            this.label1.Text = "0: Write settings area, 1: Running settings area, 2: Save area";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _txtboxParameter
            // 
            this._txtboxParameter.Location = new System.Drawing.Point(27, 413);
            this._txtboxParameter.Multiline = true;
            this._txtboxParameter.Name = "_txtboxParameter";
            this._txtboxParameter.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._txtboxParameter.Size = new System.Drawing.Size(324, 61);
            this._txtboxParameter.TabIndex = 46;
            this._txtboxParameter.Text = "3";
            // 
            // _lblParameter
            // 
            this._lblParameter.AutoSize = true;
            this._lblParameter.Location = new System.Drawing.Point(25, 397);
            this._lblParameter.Name = "_lblParameter";
            this._lblParameter.Size = new System.Drawing.Size(335, 12);
            this._lblParameter.TabIndex = 59;
            this._lblParameter.Text = "Writing parameters (comma-separated hexadecimal values)";
            // 
            // _lblDepth
            // 
            this._lblDepth.AutoSize = true;
            this._lblDepth.Location = new System.Drawing.Point(130, 64);
            this._lblDepth.Name = "_lblDepth";
            this._lblDepth.Size = new System.Drawing.Size(203, 12);
            this._lblDepth.TabIndex = 58;
            this._lblDepth.Text = "Get target area and setting depth";
            // 
            // _txtboxDepth
            // 
            this._txtboxDepth.Location = new System.Drawing.Point(48, 61);
            this._txtboxDepth.MaxLength = 2;
            this._txtboxDepth.Name = "_txtboxDepth";
            this._txtboxDepth.Size = new System.Drawing.Size(76, 21);
            this._txtboxDepth.TabIndex = 32;
            this._txtboxDepth.Text = "01";
            // 
            // _lblHexDepth
            // 
            this._lblHexDepth.AutoSize = true;
            this._lblHexDepth.Location = new System.Drawing.Point(25, 64);
            this._lblHexDepth.Name = "_lblHexDepth";
            this._lblHexDepth.Size = new System.Drawing.Size(17, 12);
            this._lblHexDepth.TabIndex = 57;
            this._lblHexDepth.Text = "0x";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(174, 374);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 55;
            this.label5.Text = "BYTE";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(25, 374);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 54;
            this.label4.Text = "Amount of data";
            // 
            // _lblTarget4
            // 
            this._lblTarget4.AutoSize = true;
            this._lblTarget4.Location = new System.Drawing.Point(130, 343);
            this._lblTarget4.Name = "_lblTarget4";
            this._lblTarget4.Size = new System.Drawing.Size(101, 12);
            this._lblTarget4.TabIndex = 56;
            this._lblTarget4.Text = "Setting target 4";
            // 
            // _txtboxTarget4
            // 
            this._txtboxTarget4.Location = new System.Drawing.Point(48, 340);
            this._txtboxTarget4.MaxLength = 2;
            this._txtboxTarget4.Name = "_txtboxTarget4";
            this._txtboxTarget4.Size = new System.Drawing.Size(76, 21);
            this._txtboxTarget4.TabIndex = 43;
            this._txtboxTarget4.Text = "00";
            // 
            // _lblHexTarget4
            // 
            this._lblHexTarget4.AutoSize = true;
            this._lblHexTarget4.Location = new System.Drawing.Point(25, 343);
            this._lblHexTarget4.Name = "_lblHexTarget4";
            this._lblHexTarget4.Size = new System.Drawing.Size(17, 12);
            this._lblHexTarget4.TabIndex = 53;
            this._lblHexTarget4.Text = "0x";
            // 
            // _lblTarget3
            // 
            this._lblTarget3.AutoSize = true;
            this._lblTarget3.Location = new System.Drawing.Point(130, 311);
            this._lblTarget3.Name = "_lblTarget3";
            this._lblTarget3.Size = new System.Drawing.Size(101, 12);
            this._lblTarget3.TabIndex = 52;
            this._lblTarget3.Text = "Setting target 3";
            // 
            // _txtboxTarget3
            // 
            this._txtboxTarget3.Location = new System.Drawing.Point(48, 308);
            this._txtboxTarget3.MaxLength = 2;
            this._txtboxTarget3.Name = "_txtboxTarget3";
            this._txtboxTarget3.Size = new System.Drawing.Size(76, 21);
            this._txtboxTarget3.TabIndex = 42;
            this._txtboxTarget3.Text = "00";
            // 
            // _lblHexTarget3
            // 
            this._lblHexTarget3.AutoSize = true;
            this._lblHexTarget3.Location = new System.Drawing.Point(25, 311);
            this._lblHexTarget3.Name = "_lblHexTarget3";
            this._lblHexTarget3.Size = new System.Drawing.Size(17, 12);
            this._lblHexTarget3.TabIndex = 51;
            this._lblHexTarget3.Text = "0x";
            // 
            // _lblTarget2
            // 
            this._lblTarget2.AutoSize = true;
            this._lblTarget2.Location = new System.Drawing.Point(130, 280);
            this._lblTarget2.Name = "_lblTarget2";
            this._lblTarget2.Size = new System.Drawing.Size(101, 12);
            this._lblTarget2.TabIndex = 50;
            this._lblTarget2.Text = "Setting target 2";
            // 
            // _txtboxTarget2
            // 
            this._txtboxTarget2.Location = new System.Drawing.Point(48, 277);
            this._txtboxTarget2.MaxLength = 2;
            this._txtboxTarget2.Name = "_txtboxTarget2";
            this._txtboxTarget2.Size = new System.Drawing.Size(76, 21);
            this._txtboxTarget2.TabIndex = 40;
            this._txtboxTarget2.Text = "00";
            // 
            // _lblHexTarget2
            // 
            this._lblHexTarget2.AutoSize = true;
            this._lblHexTarget2.Location = new System.Drawing.Point(25, 280);
            this._lblHexTarget2.Name = "_lblHexTarget2";
            this._lblHexTarget2.Size = new System.Drawing.Size(17, 12);
            this._lblHexTarget2.TabIndex = 49;
            this._lblHexTarget2.Text = "0x";
            // 
            // _lblTarget1
            // 
            this._lblTarget1.AutoSize = true;
            this._lblTarget1.Location = new System.Drawing.Point(130, 249);
            this._lblTarget1.Name = "_lblTarget1";
            this._lblTarget1.Size = new System.Drawing.Size(101, 12);
            this._lblTarget1.TabIndex = 48;
            this._lblTarget1.Text = "Setting target 1";
            // 
            // _txtboxTarget1
            // 
            this._txtboxTarget1.Location = new System.Drawing.Point(48, 245);
            this._txtboxTarget1.MaxLength = 2;
            this._txtboxTarget1.Name = "_txtboxTarget1";
            this._txtboxTarget1.Size = new System.Drawing.Size(76, 21);
            this._txtboxTarget1.TabIndex = 38;
            this._txtboxTarget1.Text = "00";
            // 
            // _lblHexTarget1
            // 
            this._lblHexTarget1.AutoSize = true;
            this._lblHexTarget1.Location = new System.Drawing.Point(25, 249);
            this._lblHexTarget1.Name = "_lblHexTarget1";
            this._lblHexTarget1.Size = new System.Drawing.Size(17, 12);
            this._lblHexTarget1.TabIndex = 47;
            this._lblHexTarget1.Text = "0x";
            // 
            // _lblItem
            // 
            this._lblItem.AutoSize = true;
            this._lblItem.Location = new System.Drawing.Point(130, 217);
            this._lblItem.Name = "_lblItem";
            this._lblItem.Size = new System.Drawing.Size(77, 12);
            this._lblItem.TabIndex = 44;
            this._lblItem.Text = "Setting item";
            // 
            // _txtboxItem
            // 
            this._txtboxItem.Location = new System.Drawing.Point(48, 214);
            this._txtboxItem.MaxLength = 2;
            this._txtboxItem.Name = "_txtboxItem";
            this._txtboxItem.Size = new System.Drawing.Size(76, 21);
            this._txtboxItem.TabIndex = 37;
            this._txtboxItem.Text = "02";
            // 
            // _lblHexItem
            // 
            this._lblHexItem.AutoSize = true;
            this._lblHexItem.Location = new System.Drawing.Point(25, 217);
            this._lblHexItem.Name = "_lblHexItem";
            this._lblHexItem.Size = new System.Drawing.Size(17, 12);
            this._lblHexItem.TabIndex = 41;
            this._lblHexItem.Text = "0x";
            // 
            // _lblCategor
            // 
            this._lblCategor.AutoSize = true;
            this._lblCategor.Location = new System.Drawing.Point(130, 186);
            this._lblCategor.Name = "_lblCategor";
            this._lblCategor.Size = new System.Drawing.Size(53, 12);
            this._lblCategor.TabIndex = 39;
            this._lblCategor.Text = "Category";
            // 
            // _txtboxCategor
            // 
            this._txtboxCategor.Location = new System.Drawing.Point(48, 182);
            this._txtboxCategor.MaxLength = 2;
            this._txtboxCategor.Name = "_txtboxCategor";
            this._txtboxCategor.Size = new System.Drawing.Size(76, 21);
            this._txtboxCategor.TabIndex = 35;
            this._txtboxCategor.Text = "00";
            // 
            // _lblHexCategor
            // 
            this._lblHexCategor.AutoSize = true;
            this._lblHexCategor.Location = new System.Drawing.Point(25, 186);
            this._lblHexCategor.Name = "_lblHexCategor";
            this._lblHexCategor.Size = new System.Drawing.Size(17, 12);
            this._lblHexCategor.TabIndex = 36;
            this._lblHexCategor.Text = "0x";
            // 
            // _lblType
            // 
            this._lblType.AutoSize = true;
            this._lblType.Location = new System.Drawing.Point(130, 114);
            this._lblType.Name = "_lblType";
            this._lblType.Size = new System.Drawing.Size(77, 12);
            this._lblType.TabIndex = 34;
            this._lblType.Text = "Setting type";
            // 
            // _txtboxType
            // 
            this._txtboxType.Location = new System.Drawing.Point(48, 111);
            this._txtboxType.MaxLength = 2;
            this._txtboxType.Name = "_txtboxType";
            this._txtboxType.Size = new System.Drawing.Size(76, 21);
            this._txtboxType.TabIndex = 33;
            this._txtboxType.Text = "10";
            // 
            // _lblHexType
            // 
            this._lblHexType.AutoSize = true;
            this._lblHexType.Location = new System.Drawing.Point(25, 114);
            this._lblHexType.Name = "_lblHexType";
            this._lblHexType.Size = new System.Drawing.Size(17, 12);
            this._lblHexType.TabIndex = 31;
            this._lblHexType.Text = "0x";
            // 
            // _btnCancel
            // 
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.Location = new System.Drawing.Point(276, 490);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(75, 25);
            this._btnCancel.TabIndex = 64;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;
            // 
            // _btnOk
            // 
            this._btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._btnOk.Location = new System.Drawing.Point(132, 490);
            this._btnOk.Name = "_btnOk";
            this._btnOk.Size = new System.Drawing.Size(75, 25);
            this._btnOk.TabIndex = 63;
            this._btnOk.Text = "OK";
            this._btnOk.UseVisualStyleBackColor = true;
            // 
            // SettingForm
            // 
            this.AcceptButton = this._btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(369, 527);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._btnOk);
            this.Controls.Add(this._txtDataLength);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._txtboxParameter);
            this.Controls.Add(this._lblParameter);
            this.Controls.Add(this._lblDepth);
            this.Controls.Add(this._txtboxDepth);
            this.Controls.Add(this._lblHexDepth);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this._lblTarget4);
            this.Controls.Add(this._txtboxTarget4);
            this.Controls.Add(this._lblHexTarget4);
            this.Controls.Add(this._lblTarget3);
            this.Controls.Add(this._txtboxTarget3);
            this.Controls.Add(this._lblHexTarget3);
            this.Controls.Add(this._lblTarget2);
            this.Controls.Add(this._txtboxTarget2);
            this.Controls.Add(this._lblHexTarget2);
            this.Controls.Add(this._lblTarget1);
            this.Controls.Add(this._txtboxTarget1);
            this.Controls.Add(this._lblHexTarget1);
            this.Controls.Add(this._lblItem);
            this.Controls.Add(this._txtboxItem);
            this.Controls.Add(this._lblHexItem);
            this.Controls.Add(this._lblCategor);
            this.Controls.Add(this._txtboxCategor);
            this.Controls.Add(this._lblHexCategor);
            this.Controls.Add(this._lblType);
            this.Controls.Add(this._txtboxType);
            this.Controls.Add(this._lblHexType);
            this.Name = "SettingForm";
            this.Text = "SettingForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _txtDataLength;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _txtboxParameter;
        private System.Windows.Forms.Label _lblParameter;
        private System.Windows.Forms.Label _lblDepth;
        private System.Windows.Forms.TextBox _txtboxDepth;
        private System.Windows.Forms.Label _lblHexDepth;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label _lblTarget4;
        private System.Windows.Forms.TextBox _txtboxTarget4;
        private System.Windows.Forms.Label _lblHexTarget4;
        private System.Windows.Forms.Label _lblTarget3;
        private System.Windows.Forms.TextBox _txtboxTarget3;
        private System.Windows.Forms.Label _lblHexTarget3;
        private System.Windows.Forms.Label _lblTarget2;
        private System.Windows.Forms.TextBox _txtboxTarget2;
        private System.Windows.Forms.Label _lblHexTarget2;
        private System.Windows.Forms.Label _lblTarget1;
        private System.Windows.Forms.TextBox _txtboxTarget1;
        private System.Windows.Forms.Label _lblHexTarget1;
        private System.Windows.Forms.Label _lblItem;
        private System.Windows.Forms.TextBox _txtboxItem;
        private System.Windows.Forms.Label _lblHexItem;
        private System.Windows.Forms.Label _lblCategor;
        private System.Windows.Forms.TextBox _txtboxCategor;
        private System.Windows.Forms.Label _lblHexCategor;
        private System.Windows.Forms.Label _lblType;
        private System.Windows.Forms.TextBox _txtboxType;
        private System.Windows.Forms.Label _lblHexType;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.Button _btnOk;
    }
}