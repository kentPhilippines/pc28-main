namespace DummyApp
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            panel1 = new System.Windows.Forms.Panel();
            粘贴帐号 = new System.Windows.Forms.Button();
            btn添加帐号 = new System.Windows.Forms.Button();
            txt密码 = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            txt帐号 = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            txt帐单名 = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            dgv托列表 = new System.Windows.Forms.DataGridView();
            cms托列表右键 = new System.Windows.Forms.ContextMenuStrip(components);
            tsmi删除 = new System.Windows.Forms.ToolStripMenuItem();
            tsmi发送消息 = new System.Windows.Forms.ToolStripMenuItem();
            pnlButtom = new System.Windows.Forms.Panel();
            log = new DummyApp.Util.LogTextBox();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            一键登录 = new System.Windows.Forms.Button();
            一键退出 = new System.Windows.Forms.Button();
            一键启动 = new System.Windows.Forms.Button();
            一键暂停 = new System.Windows.Forms.Button();
            基础设置 = new System.Windows.Forms.Button();
            自定义下注模板 = new System.Windows.Forms.Button();
            保存设置 = new System.Windows.Forms.Button();
            btnTest = new System.Windows.Forms.Button();
            ss状态栏 = new System.Windows.Forms.StatusStrip();
            监控群ID = new System.Windows.Forms.ToolStripStatusLabel();
            tssl监控群ID = new System.Windows.Forms.ToolStripStatusLabel();
            机器人ID = new System.Windows.Forms.ToolStripStatusLabel();
            tssl机器人ID = new System.Windows.Forms.ToolStripStatusLabel();
            cms选择右键 = new System.Windows.Forms.ContextMenuStrip(components);
            全部选中ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            全部取消ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            反向选择ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            选择 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            帐单名 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            帐号 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            密码 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            积分 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            下注模板 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            状态 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgv托列表).BeginInit();
            cms托列表右键.SuspendLayout();
            pnlButtom.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            ss状态栏.SuspendLayout();
            cms选择右键.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(粘贴帐号);
            panel1.Controls.Add(btn添加帐号);
            panel1.Controls.Add(txt密码);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(txt帐号);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(txt帐单名);
            panel1.Controls.Add(label1);
            panel1.Dock = System.Windows.Forms.DockStyle.Top;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Margin = new System.Windows.Forms.Padding(4);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(681, 46);
            panel1.TabIndex = 0;
            // 
            // 粘贴帐号
            // 
            粘贴帐号.Location = new System.Drawing.Point(592, 8);
            粘贴帐号.Margin = new System.Windows.Forms.Padding(4);
            粘贴帐号.Name = "粘贴帐号";
            粘贴帐号.Size = new System.Drawing.Size(88, 30);
            粘贴帐号.TabIndex = 5;
            粘贴帐号.Text = "粘贴帐号";
            粘贴帐号.UseVisualStyleBackColor = true;
            粘贴帐号.Click += 粘贴帐号_Click;
            // 
            // btn添加帐号
            // 
            btn添加帐号.Location = new System.Drawing.Point(503, 8);
            btn添加帐号.Margin = new System.Windows.Forms.Padding(4);
            btn添加帐号.Name = "btn添加帐号";
            btn添加帐号.Size = new System.Drawing.Size(88, 30);
            btn添加帐号.TabIndex = 4;
            btn添加帐号.Text = "添加帐号";
            btn添加帐号.UseVisualStyleBackColor = true;
            btn添加帐号.Click += btn添加帐号_Click;
            // 
            // txt密码
            // 
            txt密码.Location = new System.Drawing.Point(383, 9);
            txt密码.Margin = new System.Windows.Forms.Padding(4);
            txt密码.Name = "txt密码";
            txt密码.Size = new System.Drawing.Size(111, 23);
            txt密码.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(340, 14);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(35, 17);
            label3.TabIndex = 0;
            label3.Text = "密码:";
            // 
            // txt帐号
            // 
            txt帐号.Location = new System.Drawing.Point(223, 9);
            txt帐号.Margin = new System.Windows.Forms.Padding(4);
            txt帐号.Name = "txt帐号";
            txt帐号.Size = new System.Drawing.Size(111, 23);
            txt帐号.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(180, 14);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(35, 17);
            label2.TabIndex = 0;
            label2.Text = "帐号:";
            // 
            // txt帐单名
            // 
            txt帐单名.Location = new System.Drawing.Point(62, 9);
            txt帐单名.Margin = new System.Windows.Forms.Padding(4);
            txt帐单名.Name = "txt帐单名";
            txt帐单名.Size = new System.Drawing.Size(111, 23);
            txt帐单名.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(5, 14);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(47, 17);
            label1.TabIndex = 0;
            label1.Text = "帐单名:";
            // 
            // dgv托列表
            // 
            dgv托列表.AllowUserToAddRows = false;
            dgv托列表.AllowUserToResizeColumns = false;
            dgv托列表.AllowUserToResizeRows = false;
            dgv托列表.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgv托列表.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgv托列表.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv托列表.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { 选择, 帐单名, 帐号, 密码, 积分, 下注模板, 状态 });
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            dgv托列表.DefaultCellStyle = dataGridViewCellStyle2;
            dgv托列表.Dock = System.Windows.Forms.DockStyle.Fill;
            dgv托列表.Location = new System.Drawing.Point(0, 46);
            dgv托列表.Margin = new System.Windows.Forms.Padding(4);
            dgv托列表.MultiSelect = false;
            dgv托列表.Name = "dgv托列表";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgv托列表.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dgv托列表.RowHeadersWidth = 50;
            dgv托列表.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgv托列表.Size = new System.Drawing.Size(681, 502);
            dgv托列表.TabIndex = 1;
            dgv托列表.CellContentClick += dgv托列表_CellContentClick;
            dgv托列表.CellMouseDown += dgv托列表_CellMouseDown;
            dgv托列表.DataError += dgv托列表_DataError;
            dgv托列表.RowStateChanged += dgv托列表_RowStateChanged;
            // 
            // cms托列表右键
            // 
            cms托列表右键.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmi删除, tsmi发送消息 });
            cms托列表右键.Name = "cms托列表右键";
            cms托列表右键.Size = new System.Drawing.Size(125, 48);
            // 
            // tsmi删除
            // 
            tsmi删除.Name = "tsmi删除";
            tsmi删除.Size = new System.Drawing.Size(124, 22);
            tsmi删除.Text = "删除";
            tsmi删除.Click += tsmi删除_Click;
            // 
            // tsmi发送消息
            // 
            tsmi发送消息.Name = "tsmi发送消息";
            tsmi发送消息.Size = new System.Drawing.Size(124, 22);
            tsmi发送消息.Text = "发送消息";
            tsmi发送消息.Click += tsmi发送消息_Click;
            // 
            // pnlButtom
            // 
            pnlButtom.Controls.Add(log);
            pnlButtom.Controls.Add(flowLayoutPanel1);
            pnlButtom.Controls.Add(ss状态栏);
            pnlButtom.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlButtom.Location = new System.Drawing.Point(0, 548);
            pnlButtom.Margin = new System.Windows.Forms.Padding(4);
            pnlButtom.Name = "pnlButtom";
            pnlButtom.Size = new System.Drawing.Size(681, 313);
            pnlButtom.TabIndex = 2;
            // 
            // log
            // 
            log.BackColor = System.Drawing.Color.Azure;
            log.BorderStyle = System.Windows.Forms.BorderStyle.None;
            log.Dock = System.Windows.Forms.DockStyle.Fill;
            log.ErrorFontColor = System.Drawing.Color.Red;
            log.InfoFontColor = System.Drawing.Color.Black;
            log.Location = new System.Drawing.Point(0, 99);
            log.Margin = new System.Windows.Forms.Padding(4);
            log.Name = "log";
            log.Size = new System.Drawing.Size(681, 192);
            log.SucceeFontColor = System.Drawing.Color.LightGreen;
            log.TabIndex = 4;
            log.Text = "";
            log.WarnFontColor = System.Drawing.Color.Yellow;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(一键登录);
            flowLayoutPanel1.Controls.Add(一键退出);
            flowLayoutPanel1.Controls.Add(一键启动);
            flowLayoutPanel1.Controls.Add(一键暂停);
            flowLayoutPanel1.Controls.Add(基础设置);
            flowLayoutPanel1.Controls.Add(自定义下注模板);
            flowLayoutPanel1.Controls.Add(保存设置);
            flowLayoutPanel1.Controls.Add(btnTest);
            flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(681, 99);
            flowLayoutPanel1.TabIndex = 1;
            // 
            // 一键登录
            // 
            一键登录.Location = new System.Drawing.Point(4, 4);
            一键登录.Margin = new System.Windows.Forms.Padding(4);
            一键登录.Name = "一键登录";
            一键登录.Size = new System.Drawing.Size(128, 38);
            一键登录.TabIndex = 0;
            一键登录.Text = "一键登录";
            一键登录.UseVisualStyleBackColor = true;
            一键登录.Click += 一键登录_Click;
            // 
            // 一键退出
            // 
            一键退出.Enabled = false;
            一键退出.Location = new System.Drawing.Point(140, 4);
            一键退出.Margin = new System.Windows.Forms.Padding(4);
            一键退出.Name = "一键退出";
            一键退出.Size = new System.Drawing.Size(128, 38);
            一键退出.TabIndex = 0;
            一键退出.Text = "一键退出";
            一键退出.UseVisualStyleBackColor = true;
            一键退出.Click += 一键退出_Click;
            // 
            // 一键启动
            // 
            一键启动.Enabled = false;
            一键启动.Location = new System.Drawing.Point(276, 4);
            一键启动.Margin = new System.Windows.Forms.Padding(4);
            一键启动.Name = "一键启动";
            一键启动.Size = new System.Drawing.Size(128, 38);
            一键启动.TabIndex = 0;
            一键启动.Text = "一键启动";
            一键启动.UseVisualStyleBackColor = true;
            一键启动.Click += 一键启动_Click;
            // 
            // 一键暂停
            // 
            一键暂停.Enabled = false;
            一键暂停.Location = new System.Drawing.Point(412, 4);
            一键暂停.Margin = new System.Windows.Forms.Padding(4);
            一键暂停.Name = "一键暂停";
            一键暂停.Size = new System.Drawing.Size(128, 38);
            一键暂停.TabIndex = 0;
            一键暂停.Text = "一键暂停";
            一键暂停.UseVisualStyleBackColor = true;
            一键暂停.Click += 一键暂停_Click;
            // 
            // 基础设置
            // 
            基础设置.Location = new System.Drawing.Point(548, 4);
            基础设置.Margin = new System.Windows.Forms.Padding(4);
            基础设置.Name = "基础设置";
            基础设置.Size = new System.Drawing.Size(128, 38);
            基础设置.TabIndex = 0;
            基础设置.Text = "基础设置";
            基础设置.UseVisualStyleBackColor = true;
            基础设置.Click += 基础设置_Click;
            // 
            // 自定义下注模板
            // 
            自定义下注模板.Location = new System.Drawing.Point(4, 50);
            自定义下注模板.Margin = new System.Windows.Forms.Padding(4);
            自定义下注模板.Name = "自定义下注模板";
            自定义下注模板.Size = new System.Drawing.Size(128, 38);
            自定义下注模板.TabIndex = 0;
            自定义下注模板.Text = "自定义下注模板";
            自定义下注模板.UseVisualStyleBackColor = true;
            自定义下注模板.Click += 自定义下注模板_Click;
            // 
            // 保存设置
            // 
            保存设置.Location = new System.Drawing.Point(140, 50);
            保存设置.Margin = new System.Windows.Forms.Padding(4);
            保存设置.Name = "保存设置";
            保存设置.Size = new System.Drawing.Size(128, 38);
            保存设置.TabIndex = 1;
            保存设置.Text = "保存设置";
            保存设置.UseVisualStyleBackColor = true;
            保存设置.Click += 保存设置_Click;
            // 
            // btnTest
            // 
            btnTest.Location = new System.Drawing.Point(276, 50);
            btnTest.Margin = new System.Windows.Forms.Padding(4);
            btnTest.Name = "btnTest";
            btnTest.Size = new System.Drawing.Size(128, 38);
            btnTest.TabIndex = 1;
            btnTest.Text = "测试";
            btnTest.UseVisualStyleBackColor = true;
            btnTest.Visible = false;
            btnTest.Click += btnTest_Click;
            // 
            // ss状态栏
            // 
            ss状态栏.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { 监控群ID, tssl监控群ID, 机器人ID, tssl机器人ID });
            ss状态栏.Location = new System.Drawing.Point(0, 291);
            ss状态栏.Name = "ss状态栏";
            ss状态栏.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            ss状态栏.Size = new System.Drawing.Size(681, 22);
            ss状态栏.TabIndex = 3;
            ss状态栏.Text = "statusStrip1";
            // 
            // 监控群ID
            // 
            监控群ID.Name = "监控群ID";
            监控群ID.Size = new System.Drawing.Size(60, 17);
            监控群ID.Text = "监控群ID:";
            // 
            // tssl监控群ID
            // 
            tssl监控群ID.Name = "tssl监控群ID";
            tssl监控群ID.Size = new System.Drawing.Size(14, 17);
            tssl监控群ID.Text = "?";
            // 
            // 机器人ID
            // 
            机器人ID.Name = "机器人ID";
            机器人ID.Size = new System.Drawing.Size(60, 17);
            机器人ID.Text = "机器人ID:";
            // 
            // tssl机器人ID
            // 
            tssl机器人ID.Name = "tssl机器人ID";
            tssl机器人ID.Size = new System.Drawing.Size(14, 17);
            tssl机器人ID.Text = "?";
            // 
            // cms选择右键
            // 
            cms选择右键.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { 全部选中ToolStripMenuItem, 全部取消ToolStripMenuItem, 反向选择ToolStripMenuItem });
            cms选择右键.Name = "cms选择右键";
            cms选择右键.Size = new System.Drawing.Size(101, 70);
            // 
            // 全部选中ToolStripMenuItem
            // 
            全部选中ToolStripMenuItem.Name = "全部选中ToolStripMenuItem";
            全部选中ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            全部选中ToolStripMenuItem.Text = "全选";
            全部选中ToolStripMenuItem.Click += 全部选中ToolStripMenuItem_Click;
            // 
            // 全部取消ToolStripMenuItem
            // 
            全部取消ToolStripMenuItem.Name = "全部取消ToolStripMenuItem";
            全部取消ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            全部取消ToolStripMenuItem.Text = "取消";
            全部取消ToolStripMenuItem.Click += 全部取消ToolStripMenuItem_Click;
            // 
            // 反向选择ToolStripMenuItem
            // 
            反向选择ToolStripMenuItem.Name = "反向选择ToolStripMenuItem";
            反向选择ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            反向选择ToolStripMenuItem.Text = "反选";
            反向选择ToolStripMenuItem.Click += 反向选择ToolStripMenuItem_Click;
            // 
            // 选择
            // 
            选择.DataPropertyName = "Checked";
            选择.HeaderText = "选择";
            选择.Name = "选择";
            选择.Width = 40;
            // 
            // 帐单名
            // 
            帐单名.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            帐单名.DataPropertyName = "BillName";
            帐单名.FillWeight = 20F;
            帐单名.HeaderText = "帐单名";
            帐单名.Name = "帐单名";
            // 
            // 帐号
            // 
            帐号.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            帐号.DataPropertyName = "LoginName";
            帐号.FillWeight = 20F;
            帐号.HeaderText = "帐号";
            帐号.Name = "帐号";
            // 
            // 密码
            // 
            密码.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            密码.DataPropertyName = "Password";
            密码.FillWeight = 20F;
            密码.HeaderText = "密码";
            密码.Name = "密码";
            // 
            // 积分
            // 
            积分.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            积分.DataPropertyName = "Jifen";
            积分.FillWeight = 20F;
            积分.HeaderText = "积分";
            积分.Name = "积分";
            积分.ReadOnly = true;
            // 
            // 下注模板
            // 
            下注模板.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            下注模板.DataPropertyName = "Template";
            下注模板.FillWeight = 20F;
            下注模板.HeaderText = "下注模板";
            下注模板.Name = "下注模板";
            下注模板.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            下注模板.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // 状态
            // 
            状态.DataPropertyName = "Status";
            状态.HeaderText = "状态";
            状态.Name = "状态";
            状态.ReadOnly = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(681, 861);
            Controls.Add(dgv托列表);
            Controls.Add(pnlButtom);
            Controls.Add(panel1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4);
            Name = "MainForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "智能托";
            FormClosed += MainForm_FormClosed;
            Load += MainForm_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgv托列表).EndInit();
            cms托列表右键.ResumeLayout(false);
            pnlButtom.ResumeLayout(false);
            pnlButtom.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            ss状态栏.ResumeLayout(false);
            ss状态栏.PerformLayout();
            cms选择右键.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt帐号;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt帐单名;
        private System.Windows.Forms.Button btn添加帐号;
        private System.Windows.Forms.Button 粘贴帐号;
        private System.Windows.Forms.DataGridView dgv托列表;
        private System.Windows.Forms.TextBox txt密码;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel pnlButtom;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button 一键登录;
        private System.Windows.Forms.Button 一键退出;
        private System.Windows.Forms.Button 一键启动;
        private System.Windows.Forms.Button 一键暂停;
        private System.Windows.Forms.Button 基础设置;
        private System.Windows.Forms.Button 自定义下注模板;
        private System.Windows.Forms.Button 保存设置;
        private System.Windows.Forms.StatusStrip ss状态栏;
        private System.Windows.Forms.ToolStripStatusLabel 监控群ID;
        private System.Windows.Forms.ToolStripStatusLabel tssl监控群ID;
        private System.Windows.Forms.ToolStripStatusLabel 机器人ID;
        private System.Windows.Forms.ToolStripStatusLabel tssl机器人ID;
        private Util.LogTextBox log;
        private System.Windows.Forms.ContextMenuStrip cms托列表右键;
        private System.Windows.Forms.ToolStripMenuItem tsmi删除;
        private System.Windows.Forms.ContextMenuStrip cms选择右键;
        private System.Windows.Forms.ToolStripMenuItem 全部选中ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 全部取消ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 反向选择ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsmi发送消息;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.DataGridViewCheckBoxColumn 选择;
        private System.Windows.Forms.DataGridViewTextBoxColumn 帐单名;
        private System.Windows.Forms.DataGridViewTextBoxColumn 帐号;
        private System.Windows.Forms.DataGridViewTextBoxColumn 密码;
        private System.Windows.Forms.DataGridViewTextBoxColumn 积分;
        private System.Windows.Forms.DataGridViewComboBoxColumn 下注模板;
        private System.Windows.Forms.DataGridViewTextBoxColumn 状态;
    }
}

