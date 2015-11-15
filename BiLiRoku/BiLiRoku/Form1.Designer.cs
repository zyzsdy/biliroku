namespace BiLiRoku
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.roomidTxtBox = new System.Windows.Forms.TextBox();
            this.savepathTxtBox = new System.Windows.Forms.TextBox();
            this.openSaveBtn = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.startBtn = new System.Windows.Forms.Button();
            this.infoTxtBox = new System.Windows.Forms.RichTextBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.nowBytesLabel = new System.Windows.Forms.Label();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.recTimeLabel = new System.Windows.Forms.Label();
            this.nowTimeLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(12, 183);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(83, 12);
            this.linkLabel1.TabIndex = 0;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "关于 BiliRoku";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(203, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "房间号：http://live.bilibili.com/";
            // 
            // roomidTxtBox
            // 
            this.roomidTxtBox.Location = new System.Drawing.Point(227, 18);
            this.roomidTxtBox.Name = "roomidTxtBox";
            this.roomidTxtBox.Size = new System.Drawing.Size(95, 21);
            this.roomidTxtBox.TabIndex = 2;
            this.roomidTxtBox.TextChanged += new System.EventHandler(this.roomidTxtBox_TextChanged);
            // 
            // savepathTxtBox
            // 
            this.savepathTxtBox.Location = new System.Drawing.Point(71, 54);
            this.savepathTxtBox.Name = "savepathTxtBox";
            this.savepathTxtBox.ReadOnly = true;
            this.savepathTxtBox.Size = new System.Drawing.Size(170, 21);
            this.savepathTxtBox.TabIndex = 4;
            // 
            // openSaveBtn
            // 
            this.openSaveBtn.Location = new System.Drawing.Point(247, 52);
            this.openSaveBtn.Name = "openSaveBtn";
            this.openSaveBtn.Size = new System.Drawing.Size(75, 23);
            this.openSaveBtn.TabIndex = 5;
            this.openSaveBtn.Text = "浏览...";
            this.openSaveBtn.UseVisualStyleBackColor = true;
            this.openSaveBtn.Click += new System.EventHandler(this.openSaveBtn_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label3.Location = new System.Drawing.Point(69, 78);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(113, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "重名文件将会被覆盖";
            // 
            // startBtn
            // 
            this.startBtn.Font = new System.Drawing.Font("微软雅黑", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.startBtn.Location = new System.Drawing.Point(14, 107);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(149, 57);
            this.startBtn.TabIndex = 7;
            this.startBtn.Text = "开始";
            this.startBtn.UseVisualStyleBackColor = true;
            this.startBtn.Click += new System.EventHandler(this.startBtn_Click);
            // 
            // infoTxtBox
            // 
            this.infoTxtBox.DetectUrls = false;
            this.infoTxtBox.Location = new System.Drawing.Point(328, 15);
            this.infoTxtBox.Name = "infoTxtBox";
            this.infoTxtBox.ReadOnly = true;
            this.infoTxtBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.infoTxtBox.Size = new System.Drawing.Size(269, 180);
            this.infoTxtBox.TabIndex = 8;
            this.infoTxtBox.Text = "";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "flv";
            this.saveFileDialog1.Filter = "Flash流媒体文件|*.flv";
            // 
            // nowBytesLabel
            // 
            this.nowBytesLabel.AutoSize = true;
            this.nowBytesLabel.Location = new System.Drawing.Point(176, 107);
            this.nowBytesLabel.Name = "nowBytesLabel";
            this.nowBytesLabel.Size = new System.Drawing.Size(65, 12);
            this.nowBytesLabel.TabIndex = 9;
            this.nowBytesLabel.Text = "已下载字节";
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(12, 59);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(65, 12);
            this.linkLabel2.TabIndex = 10;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "保存位置：";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // recTimeLabel
            // 
            this.recTimeLabel.AutoSize = true;
            this.recTimeLabel.Location = new System.Drawing.Point(176, 136);
            this.recTimeLabel.Name = "recTimeLabel";
            this.recTimeLabel.Size = new System.Drawing.Size(53, 12);
            this.recTimeLabel.TabIndex = 11;
            this.recTimeLabel.Text = "录制时间";
            // 
            // nowTimeLabel
            // 
            this.nowTimeLabel.AutoSize = true;
            this.nowTimeLabel.Location = new System.Drawing.Point(176, 163);
            this.nowTimeLabel.Name = "nowTimeLabel";
            this.nowTimeLabel.Size = new System.Drawing.Size(77, 12);
            this.nowTimeLabel.TabIndex = 12;
            this.nowTimeLabel.Text = "当前录制时间";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(607, 207);
            this.Controls.Add(this.nowTimeLabel);
            this.Controls.Add(this.recTimeLabel);
            this.Controls.Add(this.nowBytesLabel);
            this.Controls.Add(this.infoTxtBox);
            this.Controls.Add(this.startBtn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.openSaveBtn);
            this.Controls.Add(this.savepathTxtBox);
            this.Controls.Add(this.roomidTxtBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.linkLabel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Bilibili生放送录制";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox roomidTxtBox;
        private System.Windows.Forms.TextBox savepathTxtBox;
        private System.Windows.Forms.Button openSaveBtn;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button startBtn;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.RichTextBox infoTxtBox;
        private System.Windows.Forms.Label nowBytesLabel;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.Label recTimeLabel;
        private System.Windows.Forms.Label nowTimeLabel;
    }
}

