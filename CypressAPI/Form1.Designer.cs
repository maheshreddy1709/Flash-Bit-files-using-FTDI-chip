namespace CypressAPI
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.directoryEntry1 = new System.DirectoryServices.DirectoryEntry();
            this.OutputTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.RegBox = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.Vbus_20 = new System.Windows.Forms.Button();
            this.Vbus_12 = new System.Windows.Forms.Button();
            this.On_Off = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button7 = new System.Windows.Forms.Button();
            this.MachXO2 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.Flash8K = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.RegTypeCBox = new System.Windows.Forms.ComboBox();
            this.OperationComboBox = new System.Windows.Forms.Label();
            this.TexToSendBox = new System.Windows.Forms.TextBox();
            this.InOutBox = new System.Windows.Forms.ComboBox();
            this.TextToSend = new System.Windows.Forms.Label();
            this.NumBytesBox = new System.Windows.Forms.TextBox();
            this.NumByteBoxLabel = new System.Windows.Forms.Label();
            this.DataToSendBox = new System.Windows.Forms.TextBox();
            this.DataToSend = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.FlashProgram = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.DataTransfer = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.StartAddr = new System.Windows.Forms.TextBox();
            this.StartAddrLabel = new System.Windows.Forms.Label();
            this.DeviceAddr = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.StatLabel = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.programToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sPIProgramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.StatLabel.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // OutputTextBox
            // 
            this.OutputTextBox.BackColor = System.Drawing.SystemColors.Info;
            resources.ApplyResources(this.OutputTextBox, "OutputTextBox");
            this.OutputTextBox.Name = "OutputTextBox";
            this.OutputTextBox.ReadOnly = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.RegBox);
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.RegTypeCBox);
            this.groupBox1.Controls.Add(this.OperationComboBox);
            this.groupBox1.Controls.Add(this.TexToSendBox);
            this.groupBox1.Controls.Add(this.InOutBox);
            this.groupBox1.Controls.Add(this.TextToSend);
            this.groupBox1.Controls.Add(this.NumBytesBox);
            this.groupBox1.Controls.Add(this.NumByteBoxLabel);
            this.groupBox1.Controls.Add(this.DataToSendBox);
            this.groupBox1.Controls.Add(this.DataToSend);
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Controls.Add(this.StartAddr);
            this.groupBox1.Controls.Add(this.StartAddrLabel);
            this.groupBox1.Controls.Add(this.DeviceAddr);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.ForeColor = System.Drawing.Color.Maroon;
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // RegBox
            // 
            this.RegBox.FormattingEnabled = true;
            this.RegBox.Items.AddRange(new object[] {
            resources.GetString("RegBox.Items"),
            resources.GetString("RegBox.Items1"),
            resources.GetString("RegBox.Items2"),
            resources.GetString("RegBox.Items3"),
            resources.GetString("RegBox.Items4"),
            resources.GetString("RegBox.Items5"),
            resources.GetString("RegBox.Items6"),
            resources.GetString("RegBox.Items7"),
            resources.GetString("RegBox.Items8"),
            resources.GetString("RegBox.Items9"),
            resources.GetString("RegBox.Items10"),
            resources.GetString("RegBox.Items11"),
            resources.GetString("RegBox.Items12"),
            resources.GetString("RegBox.Items13"),
            resources.GetString("RegBox.Items14"),
            resources.GetString("RegBox.Items15"),
            resources.GetString("RegBox.Items16"),
            resources.GetString("RegBox.Items17"),
            resources.GetString("RegBox.Items18"),
            resources.GetString("RegBox.Items19"),
            resources.GetString("RegBox.Items20"),
            resources.GetString("RegBox.Items21"),
            resources.GetString("RegBox.Items22"),
            resources.GetString("RegBox.Items23"),
            resources.GetString("RegBox.Items24"),
            resources.GetString("RegBox.Items25"),
            resources.GetString("RegBox.Items26"),
            resources.GetString("RegBox.Items27")});
            resources.ApplyResources(this.RegBox, "RegBox");
            this.RegBox.Name = "RegBox";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.Vbus_20);
            this.groupBox3.Controls.Add(this.Vbus_12);
            this.groupBox3.Controls.Add(this.On_Off);
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            this.groupBox3.Enter += new System.EventHandler(this.groupBox3_Enter);
            // 
            // Vbus_20
            // 
            resources.ApplyResources(this.Vbus_20, "Vbus_20");
            this.Vbus_20.Name = "Vbus_20";
            this.Vbus_20.UseVisualStyleBackColor = true;
            this.Vbus_20.Click += new System.EventHandler(this.Vbus_20_Click);
            // 
            // Vbus_12
            // 
            resources.ApplyResources(this.Vbus_12, "Vbus_12");
            this.Vbus_12.Name = "Vbus_12";
            this.Vbus_12.UseVisualStyleBackColor = true;
            this.Vbus_12.Click += new System.EventHandler(this.Vbus_12_Click);
            // 
            // On_Off
            // 
            resources.ApplyResources(this.On_Off, "On_Off");
            this.On_Off.Name = "On_Off";
            this.On_Off.UseVisualStyleBackColor = true;
            this.On_Off.Click += new System.EventHandler(this.On_Off_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.Silver;
            this.groupBox2.Controls.Add(this.button7);
            this.groupBox2.Controls.Add(this.MachXO2);
            this.groupBox2.Controls.Add(this.button6);
            this.groupBox2.Controls.Add(this.Flash8K);
            this.groupBox2.Controls.Add(this.button5);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
            // 
            // button7
            // 
            resources.ApplyResources(this.button7, "button7");
            this.button7.Name = "button7";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // MachXO2
            // 
            resources.ApplyResources(this.MachXO2, "MachXO2");
            this.MachXO2.Name = "MachXO2";
            this.MachXO2.UseVisualStyleBackColor = true;
            this.MachXO2.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            resources.ApplyResources(this.button6, "button6");
            this.button6.Name = "button6";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click_1);
            // 
            // Flash8K
            // 
            resources.ApplyResources(this.Flash8K, "Flash8K");
            this.Flash8K.Name = "Flash8K";
            this.Flash8K.UseVisualStyleBackColor = true;
            this.Flash8K.Click += new System.EventHandler(this.button6_Click);
            // 
            // button5
            // 
            resources.ApplyResources(this.button5, "button5");
            this.button5.Name = "button5";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click_1);
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // RegTypeCBox
            // 
            resources.ApplyResources(this.RegTypeCBox, "RegTypeCBox");
            this.RegTypeCBox.BackColor = System.Drawing.SystemColors.Window;
            this.RegTypeCBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RegTypeCBox.FormattingEnabled = true;
            this.RegTypeCBox.Items.AddRange(new object[] {
            resources.GetString("RegTypeCBox.Items"),
            resources.GetString("RegTypeCBox.Items1"),
            resources.GetString("RegTypeCBox.Items2")});
            this.RegTypeCBox.Name = "RegTypeCBox";
            this.RegTypeCBox.SelectedIndexChanged += new System.EventHandler(this.RegTypeCBox_SelectedIndexChanged);
            // 
            // OperationComboBox
            // 
            resources.ApplyResources(this.OperationComboBox, "OperationComboBox");
            this.OperationComboBox.Name = "OperationComboBox";
            // 
            // TexToSendBox
            // 
            resources.ApplyResources(this.TexToSendBox, "TexToSendBox");
            this.TexToSendBox.Name = "TexToSendBox";
            this.TexToSendBox.TextChanged += new System.EventHandler(this.TexToSendBox_TextChanged);
            // 
            // InOutBox
            // 
            resources.ApplyResources(this.InOutBox, "InOutBox");
            this.InOutBox.BackColor = System.Drawing.SystemColors.Window;
            this.InOutBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.InOutBox.FormattingEnabled = true;
            this.InOutBox.Items.AddRange(new object[] {
            resources.GetString("InOutBox.Items"),
            resources.GetString("InOutBox.Items1")});
            this.InOutBox.Name = "InOutBox";
            // 
            // TextToSend
            // 
            resources.ApplyResources(this.TextToSend, "TextToSend");
            this.TextToSend.Name = "TextToSend";
            // 
            // NumBytesBox
            // 
            resources.ApplyResources(this.NumBytesBox, "NumBytesBox");
            this.NumBytesBox.Name = "NumBytesBox";
            // 
            // NumByteBoxLabel
            // 
            resources.ApplyResources(this.NumByteBoxLabel, "NumByteBoxLabel");
            this.NumByteBoxLabel.Name = "NumByteBoxLabel";
            this.NumByteBoxLabel.Click += new System.EventHandler(this.NumByteBoxLabel_Click);
            // 
            // DataToSendBox
            // 
            resources.ApplyResources(this.DataToSendBox, "DataToSendBox");
            this.DataToSendBox.Name = "DataToSendBox";
            // 
            // DataToSend
            // 
            resources.ApplyResources(this.DataToSend, "DataToSend");
            this.DataToSend.Name = "DataToSend";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.FlashProgram, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.button4, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.button3, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.DataTransfer, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.button2, 3, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // FlashProgram
            // 
            resources.ApplyResources(this.FlashProgram, "FlashProgram");
            this.FlashProgram.Name = "FlashProgram";
            this.FlashProgram.UseVisualStyleBackColor = true;
            this.FlashProgram.Click += new System.EventHandler(this.FlashProgramming_Click);
            // 
            // button4
            // 
            resources.ApplyResources(this.button4, "button4");
            this.button4.Name = "button4";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            resources.ApplyResources(this.button3, "button3");
            this.button3.Name = "button3";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.FlashRead_Click);
            // 
            // DataTransfer
            // 
            resources.ApplyResources(this.DataTransfer, "DataTransfer");
            this.DataTransfer.Name = "DataTransfer";
            this.DataTransfer.UseVisualStyleBackColor = true;
            this.DataTransfer.Click += new System.EventHandler(this.DataTransfer_click);
            // 
            // button2
            // 
            resources.ApplyResources(this.button2, "button2");
            this.button2.Name = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.ClearBox_Click);
            // 
            // StartAddr
            // 
            resources.ApplyResources(this.StartAddr, "StartAddr");
            this.StartAddr.Name = "StartAddr";
            // 
            // StartAddrLabel
            // 
            resources.ApplyResources(this.StartAddrLabel, "StartAddrLabel");
            this.StartAddrLabel.Name = "StartAddrLabel";
            // 
            // DeviceAddr
            // 
            resources.ApplyResources(this.DeviceAddr, "DeviceAddr");
            this.DeviceAddr.Name = "DeviceAddr";
            this.DeviceAddr.ReadOnly = true;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // StatLabel
            // 
            resources.ApplyResources(this.StatLabel, "StatLabel");
            this.StatLabel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.StatLabel.Name = "StatLabel";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            resources.ApplyResources(this.toolStripStatusLabel1, "toolStripStatusLabel1");
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.programToolStripMenuItem,
            this.helpToolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // programToolStripMenuItem
            // 
            this.programToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sPIProgramToolStripMenuItem});
            this.programToolStripMenuItem.Name = "programToolStripMenuItem";
            resources.ApplyResources(this.programToolStripMenuItem, "programToolStripMenuItem");
            // 
            // sPIProgramToolStripMenuItem
            // 
            this.sPIProgramToolStripMenuItem.Name = "sPIProgramToolStripMenuItem";
            resources.ApplyResources(this.sPIProgramToolStripMenuItem, "sPIProgramToolStripMenuItem");
            this.sPIProgramToolStripMenuItem.Click += new System.EventHandler(this.sPIProgramToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            resources.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.Controls.Add(this.StatLabel);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.OutputTextBox);
            this.ForeColor = System.Drawing.Color.Brown;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.StatLabel.ResumeLayout(false);
            this.StatLabel.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.DirectoryServices.DirectoryEntry directoryEntry1;
        private System.Windows.Forms.TextBox OutputTextBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox NumBytesBox;
        private System.Windows.Forms.Label NumByteBoxLabel;
        private System.Windows.Forms.TextBox DataToSendBox;
        private System.Windows.Forms.Label DataToSend;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button DataTransfer;
        private System.Windows.Forms.Button FlashProgram;
        private System.Windows.Forms.TextBox StartAddr;
        private System.Windows.Forms.Label StartAddrLabel;
        private System.Windows.Forms.TextBox DeviceAddr;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.StatusStrip StatLabel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem programToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sPIProgramToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.TextBox TexToSendBox;
        private System.Windows.Forms.Label TextToSend;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox RegTypeCBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button MachXO2;
        public System.Windows.Forms.Button Flash8K;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button On_Off;
        private System.Windows.Forms.Button Vbus_12;
        private System.Windows.Forms.Button Vbus_20;
        private System.Windows.Forms.ComboBox RegBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label OperationComboBox;
        private System.Windows.Forms.ComboBox InOutBox;
    }
}

