using GithubKookBot.Models;
namespace GithubKookBot
{
    partial class ConfigWindow
    {
        private System.ComponentModel.IContainer components = null;
        private TabControl tabMain;
        private TabPage tabBase;
        private TabPage tabSub;
        private GroupBox grpBase;
        private TextBox txtGithubToken;
        private Label labGithub;
        private TextBox txtKookToken;
        private Label labKookToken;
        private TextBox txtKookChannelId;
        private Label labChannel;
        private NumericUpDown nudInterval;
        private Label labInterval;
        private ComboBox cmbMode;
        private Label labMode;
        private TextBox txtBaiduKey;
        private Label labBaiduKey;
        private TextBox txtBaiduSec;
        private Label labBaiduSec;
        private NumericUpDown nudFetch;
        private Label labFetch;
        private GroupBox grpSubList;
        private DataGridView dgvSub;
        private DataGridViewTextBoxColumn colOwner;
        private DataGridViewTextBoxColumn colRepo;
        private DataGridViewTextBoxColumn colDisp;
        private Button btnAddSub;
        private Button btnDelSub;
        private Panel panelBottom;
        private Button btnSaveAll;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }
        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            tabMain = new TabControl();
            tabBase = new TabPage();
            grpBase = new GroupBox();
            labGithub = new Label();
            txtGithubToken = new TextBox();
            labKookToken = new Label();
            txtKookToken = new TextBox();
            labChannel = new Label();
            txtKookChannelId = new TextBox();
            labInterval = new Label();
            nudInterval = new NumericUpDown();
            labMode = new Label();
            cmbMode = new ComboBox();
            labBaiduKey = new Label();
            txtBaiduKey = new TextBox();
            labBaiduSec = new Label();
            txtBaiduSec = new TextBox();
            labFetch = new Label();
            nudFetch = new NumericUpDown();
            tabSub = new TabPage();
            grpSubList = new GroupBox();
            dgvSub = new DataGridView();
            colOwner = new DataGridViewTextBoxColumn();
            colRepo = new DataGridViewTextBoxColumn();
            colDisp = new DataGridViewTextBoxColumn();
            btnAddSub = new Button();
            btnDelSub = new Button();
            panelBottom = new Panel();
            btnSaveAll = new Button();
            tabMain.SuspendLayout();
            tabBase.SuspendLayout();
            grpBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudInterval).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudFetch).BeginInit();
            tabSub.SuspendLayout();
            grpSubList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSub).BeginInit();
            panelBottom.SuspendLayout();
            SuspendLayout();
            // 
            // tabMain
            // 
            tabMain.Controls.Add(tabBase);
            tabMain.Controls.Add(tabSub);
            tabMain.Dock = DockStyle.Fill;
            tabMain.Location = new Point(0, 0);
            tabMain.Name = "tabMain";
            tabMain.SelectedIndex = 0;
            tabMain.Size = new Size(1004, 580);
            tabMain.TabIndex = 0;
            // 
            // tabBase
            // 
            tabBase.Controls.Add(grpBase);
            tabBase.Location = new Point(4, 33);
            tabBase.Name = "tabBase";
            tabBase.Padding = new Padding(8);
            tabBase.Size = new Size(996, 543);
            tabBase.TabIndex = 0;
            tabBase.Text = "基础配置";
            tabBase.UseVisualStyleBackColor = true;
            // 
            // grpBase
            // 
            grpBase.Controls.Add(labGithub);
            grpBase.Controls.Add(txtGithubToken);
            grpBase.Controls.Add(labKookToken);
            grpBase.Controls.Add(txtKookToken);
            grpBase.Controls.Add(labChannel);
            grpBase.Controls.Add(txtKookChannelId);
            grpBase.Controls.Add(labInterval);
            grpBase.Controls.Add(nudInterval);
            grpBase.Controls.Add(labMode);
            grpBase.Controls.Add(cmbMode);
            grpBase.Controls.Add(labBaiduKey);
            grpBase.Controls.Add(txtBaiduKey);
            grpBase.Controls.Add(labBaiduSec);
            grpBase.Controls.Add(txtBaiduSec);
            grpBase.Controls.Add(labFetch);
            grpBase.Controls.Add(nudFetch);
            grpBase.Dock = DockStyle.Fill;
            grpBase.Location = new Point(8, 8);
            grpBase.Name = "grpBase";
            grpBase.Size = new Size(980, 527);
            grpBase.TabIndex = 0;
            grpBase.TabStop = false;
            grpBase.Text = "基础参数";
            // 
            // labGithub
            // 
            labGithub.AutoSize = true;
            labGithub.Location = new Point(20, 30);
            labGithub.Name = "labGithub";
            labGithub.Size = new Size(147, 24);
            labGithub.TabIndex = 0;
            labGithub.Text = "GitHub Token：";
            // 
            // txtGithubToken
            // 
            txtGithubToken.Location = new Point(200, 28);
            txtGithubToken.Name = "txtGithubToken";
            txtGithubToken.Size = new Size(750, 30);
            txtGithubToken.TabIndex = 1;
            txtGithubToken.UseSystemPasswordChar = false;
            // 
            // labKookToken
            // 
            labKookToken.AutoSize = true;
            labKookToken.Location = new Point(20, 70);
            labKookToken.Name = "labKookToken";
            labKookToken.Size = new Size(177, 24);
            labKookToken.TabIndex = 2;
            labKookToken.Text = "Kook机器人Token：";
            // 
            // txtKookToken
            // 
            txtKookToken.Location = new Point(200, 68);
            txtKookToken.Name = "txtKookToken";
            txtKookToken.Size = new Size(750, 30);
            txtKookToken.TabIndex = 3;
            txtKookToken.UseSystemPasswordChar = false;
            // 
            // labChannel
            // 
            labChannel.AutoSize = true;
            labChannel.Location = new Point(20, 110);
            labChannel.Name = "labChannel";
            labChannel.Size = new Size(126, 24);
            labChannel.TabIndex = 4;
            labChannel.Text = "Kook频道ID：";
            // 
            // txtKookChannelId
            // 
            txtKookChannelId.Location = new Point(200, 108);
            txtKookChannelId.Name = "txtKookChannelId";
            txtKookChannelId.Size = new Size(232, 30);
            txtKookChannelId.TabIndex = 5;
            // 
            // labInterval
            // 
            labInterval.AutoSize = true;
            labInterval.Location = new Point(20, 150);
            labInterval.Name = "labInterval";
            labInterval.Size = new Size(148, 24);
            labInterval.TabIndex = 6;
            labInterval.Text = "检查间隔(分钟)：";
            // 
            // nudInterval
            // 
            nudInterval.Location = new Point(200, 148);
            nudInterval.Maximum = new decimal(new int[] { 1440, 0, 0, 0 });
            nudInterval.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudInterval.Name = "nudInterval";
            nudInterval.Size = new Size(120, 30);
            nudInterval.TabIndex = 7;
            nudInterval.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // labMode
            // 
            labMode.AutoSize = true;
            labMode.Location = new Point(20, 190);
            labMode.Name = "labMode";
            labMode.Size = new Size(100, 24);
            labMode.TabIndex = 8;
            labMode.Text = "更新模式：";
            // 
            // cmbMode
            // 
            cmbMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMode.Items.AddRange(new object[] { "Release", "Commit", "Both" });
            cmbMode.Location = new Point(200, 188);
            cmbMode.Name = "cmbMode";
            cmbMode.Size = new Size(120, 32);
            cmbMode.TabIndex = 9;
            // 
            // labBaiduKey
            // 
            labBaiduKey.AutoSize = true;
            labBaiduKey.Location = new Point(20, 230);
            labBaiduKey.Name = "labBaiduKey";
            labBaiduKey.Size = new Size(161, 24);
            labBaiduKey.TabIndex = 10;
            labBaiduKey.Text = "百度翻译ApiKey：";
            // 
            // txtBaiduKey
            // 
            txtBaiduKey.Location = new Point(200, 228);
            txtBaiduKey.Name = "txtBaiduKey";
            txtBaiduKey.Size = new Size(750, 30);
            txtBaiduKey.TabIndex = 11;
            // 
            // labBaiduSec
            // 
            labBaiduSec.AutoSize = true;
            labBaiduSec.Location = new Point(20, 270);
            labBaiduSec.Name = "labBaiduSec";
            labBaiduSec.Size = new Size(153, 24);
            labBaiduSec.TabIndex = 12;
            labBaiduSec.Text = "百度翻译Secret：";
            // 
            // txtBaiduSec
            // 
            txtBaiduSec.Location = new Point(200, 268);
            txtBaiduSec.Name = "txtBaiduSec";
            txtBaiduSec.Size = new Size(750, 30);
            txtBaiduSec.TabIndex = 13;
            txtBaiduSec.UseSystemPasswordChar = false;
            // 
            // labFetch
            // 
            labFetch.AutoSize = true;
            labFetch.Location = new Point(20, 310);
            labFetch.Name = "labFetch";
            labFetch.Size = new Size(172, 24);
            labFetch.TabIndex = 14;
            labFetch.Text = "测试推送更新条数：";
            // 
            // nudFetch
            // 
            nudFetch.Location = new Point(200, 308);
            nudFetch.Maximum = new decimal(new int[] { 1440, 0, 0, 0 });
            nudFetch.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudFetch.Name = "nudFetch";
            nudFetch.Size = new Size(120, 30);
            nudFetch.TabIndex = 15;
            nudFetch.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // tabSub
            // 
            tabSub.Controls.Add(grpSubList);
            tabSub.Location = new Point(4, 33);
            tabSub.Name = "tabSub";
            tabSub.Padding = new Padding(8);
            tabSub.Size = new Size(996, 543);
            tabSub.TabIndex = 1;
            tabSub.Text = "订阅仓库";
            tabSub.UseVisualStyleBackColor = true;
            // 
            // grpSubList
            // 
            grpSubList.Controls.Add(dgvSub);
            grpSubList.Controls.Add(btnAddSub);
            grpSubList.Controls.Add(btnDelSub);
            grpSubList.Dock = DockStyle.Fill;
            grpSubList.Location = new Point(8, 8);
            grpSubList.Name = "grpSubList";
            grpSubList.Size = new Size(980, 527);
            grpSubList.TabIndex = 0;
            grpSubList.TabStop = false;
            // 
            // dgvSub
            // 
            dgvSub.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Microsoft YaHei UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dgvSub.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvSub.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvSub.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSub.Columns.AddRange(new DataGridViewColumn[] { colOwner, colRepo, colDisp });
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Microsoft YaHei UI", 9F);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dgvSub.DefaultCellStyle = dataGridViewCellStyle2;
            dgvSub.Location = new Point(10, 30);
            dgvSub.Name = "dgvSub";
            dgvSub.RowHeadersVisible = false;
            dgvSub.RowHeadersWidth = 62;
            dgvSub.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSub.MultiSelect = true;
            dgvSub.Size = new Size(954, 459);
            dgvSub.TabIndex = 0;
            // 
            // colOwner
            // 
            colOwner.HeaderText = "所有者";
            colOwner.MinimumWidth = 8;
            colOwner.Name = "colOwner";
            colOwner.Width = 316; 
            colOwner.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colOwner.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // colRepo
            // 
            colRepo.HeaderText = "仓库名";
            colRepo.MinimumWidth = 8;
            colRepo.Name = "colRepo";
            colRepo.Width = 316;
            colRepo.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colRepo.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // colDisp
            // 
            colDisp.HeaderText = "显示名称";
            colDisp.MinimumWidth = 8;
            colDisp.Name = "colDisp";
            colDisp.Width = 316;
            colDisp.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colDisp.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // btnAddSub
            // 
            btnAddSub.Location = new Point(10, 495);
            btnAddSub.Name = "btnAddSub";
            btnAddSub.Size = new Size(110, 36);
            btnAddSub.TabIndex = 1;
            btnAddSub.Text = "新增订阅";
            btnAddSub.UseVisualStyleBackColor = true;
            btnAddSub.Click += btnAddSub_Click;
            // 
            // btnDelSub
            // 
            btnDelSub.Location = new Point(120, 495);
            btnDelSub.Name = "btnDelSub";
            btnDelSub.Size = new Size(110, 36);
            btnDelSub.TabIndex = 2;
            btnDelSub.Text = "删除选中";
            btnDelSub.UseVisualStyleBackColor = true;
            btnDelSub.Click += btnDelSub_Click;
            // 
            // panelBottom
            // 
            panelBottom.Controls.Add(btnSaveAll);
            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Location = new Point(0, 580);
            panelBottom.Name = "panelBottom";
            panelBottom.Size = new Size(1004, 70);
            panelBottom.TabIndex = 1;
            // 
            // btnSaveAll
            // 
            btnSaveAll.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSaveAll.Location = new Point(790, 15);
            btnSaveAll.Name = "btnSaveAll";
            btnSaveAll.Size = new Size(180, 38);
            btnSaveAll.TabIndex = 0;
            btnSaveAll.Text = "保存全部配置";
            btnSaveAll.UseVisualStyleBackColor = true;
            btnSaveAll.Click += btnSaveAll_Click;
            // 
            // ConfigWindow
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1004, 650);
            Controls.Add(tabMain);
            Controls.Add(panelBottom);
            Font = new Font("Microsoft YaHei UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "ConfigWindow";
            StartPosition = FormStartPosition.CenterParent;
            Text = "配置面板";
            tabMain.ResumeLayout(false);
            tabBase.ResumeLayout(false);
            grpBase.ResumeLayout(false);
            grpBase.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudInterval).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudFetch).EndInit();
            tabSub.ResumeLayout(false);
            grpSubList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvSub).EndInit();
            panelBottom.ResumeLayout(false);
            ResumeLayout(false);
        }
        #endregion
        public ConfigSettings NewSetting;
    }
}