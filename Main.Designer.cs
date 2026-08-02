namespace GithubKookBot
{
    partial class Main
    {
        private System.Windows.Forms.Timer timerCheck;
        private System.Windows.Forms.Timer uiRefreshTimer;
        private System.ComponentModel.IContainer components = null;
        private ToolStrip toolStrip;
        private ToolStripButton btnOpenConfig;
        private ToolStripSeparator sep1;
        private ToolStripButton btnCheckNow;
        private ToolStripSeparator sep2;
        private ToolStripButton btnToggleTimer;
        private RichTextBox rtbLog;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
        private ToolStripStatusLabel lblNextCheck;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            timerCheck = new System.Windows.Forms.Timer(components);
            uiRefreshTimer = new System.Windows.Forms.Timer(components);
            toolStrip = new ToolStrip();
            btnOpenConfig = new ToolStripButton();
            sep1 = new ToolStripSeparator();
            btnCheckNow = new ToolStripButton();
            sep2 = new ToolStripSeparator();
            btnToggleTimer = new ToolStripButton();
            rtbLog = new RichTextBox();
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            lblNextCheck = new ToolStripStatusLabel();
            toolStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // timerCheck
            //
            timerCheck.Tick += timerCheck_Tick;
            //
            // uiRefreshTimer
            //
            uiRefreshTimer.Interval = 1000;
            uiRefreshTimer.Tick += UiRefreshTimer_Tick;
            //
            // toolStrip
            //
            toolStrip.ImageScalingSize = new Size(24, 24);
            toolStrip.Items.AddRange(new ToolStripItem[] { btnOpenConfig, sep1, btnCheckNow, sep2, btnToggleTimer });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(1232, 33);
            //
            // btnOpenConfig
            //
            btnOpenConfig.Name = "btnOpenConfig";
            btnOpenConfig.Size = new Size(70, 28);
            btnOpenConfig.Text = "设置";
            btnOpenConfig.Click += btnOpenConfig_Click;
            //
            // sep1
            //
            sep1.Name = "sep1";
            sep1.Size = new Size(6, 33);
            //
            // btnCheckNow
            //
            btnCheckNow.Name = "btnCheckNow";
            btnCheckNow.Size = new Size(86, 28);
            btnCheckNow.Text = "测试推送";
            btnCheckNow.Click += btnCheckNow_Click;
            //
            // sep2
            //
            sep2.Name = "sep2";
            sep2.Size = new Size(6, 33);
            //
            // btnToggleTimer
            //
            btnToggleTimer.Name = "btnToggleTimer";
            btnToggleTimer.Size = new Size(122, 28);
            btnToggleTimer.Text = "启动定时检查";
            btnToggleTimer.Click += btnToggleTimer_Click;
            //
            // rtbLog
            //
            rtbLog.BackColor = Color.Black;
            rtbLog.Dock = DockStyle.Fill;
            rtbLog.Font = new Font("Consolas", 9F);
            rtbLog.ForeColor = Color.Lime;
            rtbLog.Location = new Point(0, 33);
            rtbLog.Name = "rtbLog";
            rtbLog.ReadOnly = true;
            rtbLog.Size = new Size(1232, 688);
            rtbLog.TabIndex = 1;
            rtbLog.Text = "";
            //
            // statusStrip
            //
            statusStrip.ImageScalingSize = new Size(24, 24);
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, lblNextCheck });
            statusStrip.Location = new Point(0, 721);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1232, 31);
            //
            // lblStatus
            //
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(46, 24);
            lblStatus.Text = "就绪";
            //
            // lblNextCheck
            //
            lblNextCheck.Name = "lblNextCheck";
            lblNextCheck.Size = new Size(0, 24);
            //
            // Main
            //
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1232, 752);
            Controls.Add(rtbLog);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);
            Font = new Font("Microsoft YaHei UI", 9F);
            MinimumSize = new Size(1245, 768);
            Text = "GitHub → Kook 订阅通知机器人";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}