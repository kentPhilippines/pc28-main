using CommonLibrary;
using RobotApp.Dao;
using RobotApp.IM;
using RobotApp.MiniDialog;
using RobotApp.Model;
using RobotApp.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace RobotApp
{
    [SupportedOSPlatform("windows")]
    public partial class GameAssistantForm : Form
    {
        public string ActivePage { get; set; }
        public string NickName { get; set; }
        private static GameAssistantForm instance;

        public static GameAssistantForm GetInstance()
        {
            if (instance == null || instance.IsDisposed)
            {
                instance = new GameAssistantForm();
            }
            return instance;
        }

        internal void ShowOrActive()
        {
            Show();
            instance.Activate();
            if (!string.IsNullOrWhiteSpace(instance.ActivePage))
            {
                instance.tabMain.SelectTab(instance.ActivePage);
            }
            if (!string.IsNullOrWhiteSpace(instance.NickName))
            {
                instance.txt昵称.Text = instance.NickName;
                instance.btn查询.PerformClick();
            }
        }

        private GameAssistantForm()
        {
            InitializeComponent();
            dgv查回统计.AutoGenerateColumns = false;
            dgv玩家猜猜明细.AutoGenerateColumns = false;
            dgv回水计算.AutoGenerateColumns = false;
            dgv回水记录.AutoGenerateColumns = false;
            dgv帐变记录.AutoGenerateColumns = false;
            List<Panel> pnlList = new List<Panel>();
            for (int i = 1; i < 16; i++)
            {
                Panel pnl = CopyPanel(pnlABCD);
                pnlList.Add(pnl);
                pnl.Location = new Point(pnl.Location.X, pnl.Location.Y + i * 30);
            }
            foreach (Panel pnl in pnlList)
            {
                pnl.Visible = true;
            }
        }

        private Panel CopyPanel(Panel originalPanel)
        {
            Panel newPanel = new Panel
            {
                Location = originalPanel.Location,
                Size = originalPanel.Size,
                Parent = originalPanel.Parent,
                Visible = false
                // 可以复制其他Panel属性
            };

            foreach (Control childControl in originalPanel.Controls)
            {
                Control newControl = (Control)Activator.CreateInstance(childControl.GetType());
                newControl.Name = childControl.Name;
                newControl.Size = childControl.Size;
                newControl.Location = childControl.Location;
                newControl.Text = childControl.Text;
                // 可以复制其他Control属性

                newPanel.Controls.Add(newControl);
            }

            return newPanel;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // 用双缓冲绘制窗口的所有子控件
                return cp;
            }
        }

        private void btn昨天_Click(object sender, EventArgs e)
        {
            DateTime yesterday = DateTime.Today.AddDays(-1);
            date开始日期.Text = yesterday.ToString("yyyy年MM月dd日");
            date结束日期.Text = yesterday.ToString("yyyy年MM月dd日");
        }

        private void btn今天_Click(object sender, EventArgs e)
        {
            DateTime today = DateTime.Today;
            date开始日期.Text = today.ToString("yyyy年MM月dd日");
            date结束日期.Text = today.ToString("yyyy年MM月dd日");
        }

        private void btn查询_Click(object sender, EventArgs e)
        {
            if (tabMain.SelectedTab.Text == "查回统计")
            {
                DateTime begin = DateTime.Parse(date开始日期.Text);
                DateTime end = DateTime.Parse(date结束日期.Text).AddDays(1);
                int isDummy = -1;
                if (rad真人.Checked)
                {
                    isDummy = 0;
                }
                if (rad假人.Checked)
                {
                    isDummy = 1;
                }
                int chahui = -1;
                if (rad上分.Checked)
                {
                    chahui = 1;
                }
                if (rad下分.Checked)
                {
                    chahui = 2;
                }
                int status = -1;
                if (rad同意.Checked)
                {
                    status = 1;
                }
                if (rad拒绝.Checked)
                {
                    status = 2;
                }
                if (rad待审核.Checked)
                {
                    status = 0;
                }
                List<Score> scoreList = ScoreDao.GetScoreList(begin, end, txt昵称.Text, isDummy, chahui, status);
                dgv查回统计.DataSource = scoreList;
                double sf = scoreList.Sum(s => (s.Status == ScoreStatus.同意 && s.Amount > 0) ? s.Amount : 0);
                double xf = scoreList.Sum(s => (s.Status == ScoreStatus.同意 && s.Amount < 0) ? s.Amount : 0);
                txt上分总数.Text = sf.ToString();
                txt回分总数.Text = Math.Abs(xf).ToString();
                txt上下差额.Text = (sf + xf).ToString();
                txt待审差额.Text = scoreList.Sum(s => s.Status == ScoreStatus.待审核 ? s.Amount : 0).ToString();
            }
            else if (tabMain.SelectedTab.Text == "帐变记录")
            {
                DateTime begin = DateTime.Parse(date开始日期.Text);
                DateTime end = DateTime.Parse(date结束日期.Text).AddDays(1);
                dgv帐变记录.DataSource = RecordDao.List(begin, end, txt昵称.Text, cbo帐变原因.SelectedItem);
            }
            else if (tabMain.SelectedTab.Text == "玩家猜猜明细")
            {
                DateTime begin = DateTime.Parse(date开始日期.Text);
                DateTime end = DateTime.Parse(date结束日期.Text).AddDays(1);
                int isDummy = -1;
                if (rad猜猜明细真人.Checked)
                {
                    isDummy = 0;
                }
                if (rad猜猜明细假人.Checked)
                {
                    isDummy = 1;
                }
                DataTable dt = BettingDao.GetBettingList(begin, end, txt昵称.Text, isDummy);
                dgv玩家猜猜明细.DataSource = dt;
                txt猜猜总分.Text = dt.Compute("Sum(本局总分)", "").ToString();
                txt输赢总分.Text = dt.Compute("Sum(输赢总分)", "").ToString();
                txt流水.Text = dt.Compute("Sum(流水)", "").ToString();
                txt最大投注.Text = dt.Compute("Max(本局总分)", "").ToString();
                if (txt流水.Text != "" && txt最大投注.Text != "")
                {
                    txt流水占比.Text = Math.Round(Convert.ToDecimal(txt流水.Text) / Convert.ToDecimal(txt最大投注.Text), 2).ToString();
                }
                else
                {
                    txt流水占比.Text = "";
                }
                List<Score> scoreList = ScoreDao.GetScoreList(begin, end, txt昵称.Text, isDummy, 1, 1);
                int sfzs = scoreList.Where(s => (s.Status == ScoreStatus.同意)).Sum(s => s.Amount);
                if (txt猜猜总分.Text != "" && sfzs != 0)
                {
                    txt投注是充值倍数.Text = Math.Round(Convert.ToDecimal(txt猜猜总分.Text) / Convert.ToDecimal(sfzs), 2).ToString();
                }
                else
                {
                    txt投注是充值倍数.Text = "";
                }
            }
            else if (tabMain.SelectedTab.Text == "操作记录")
            {
                DateTime begin = DateTime.Parse(date开始日期.Text);
                DateTime end = DateTime.Parse(date结束日期.Text).AddDays(1);
                dgv操作记录.DataSource = OplogDao.GetOplogs(begin, end);
            }
        }

        private void dgv查回统计_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SolidBrush b = new SolidBrush(this.dgv查回统计.RowHeadersDefaultCellStyle.ForeColor);
            e.Graphics.DrawString((e.RowIndex + 1).ToString(System.Globalization.CultureInfo.CurrentUICulture), this.dgv查回统计.DefaultCellStyle.Font, b, e.RowBounds.Location.X + 20, e.RowBounds.Location.Y + 4);
        }

        private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            btn查找.Visible = false;
            btn查询.Visible = true;
            if (tabMain.SelectedTab.Text == "回水设置" || tabMain.SelectedTab.Text == "回水记录")
            {
                foreach (Control ctl in pnlTop.Controls)
                {
                    ctl.Visible = false;
                }
            }
            else
            {
                foreach (Control ctl in pnlTop.Controls)
                {
                    ctl.Visible = true;
                }
            }
            if (tabMain.SelectedTab.Text == "操作记录")
            {
                lbl昵称.Visible = false;
                txt昵称.Visible = false;
                if (!Config.GetInstance().CheckPermission(PermissionForm.查看操作记录权限))
                {
                    return;
                }
            }
            if (tabMain.SelectedTab.Text == "回水记录")
            {
                btn刷新记录.PerformClick();
            }
            if (tabMain.SelectedTab.Text == "回水计算")
            {
                btn查找.Visible = true;
                btn查询.Visible = false;
            }
            if (tabMain.SelectedTab.Text == "帐变记录")
            {
                cbo帐变原因.Items.Clear();
                foreach (RecordType rt in Enum.GetValues(typeof(RecordType)))
                {
                    cbo帐变原因.Items.Add(rt);
                }
            }
        }

        private void btn刷新记录_Click(object sender, EventArgs e)
        {
            DateTime date = DateTime.Parse(date回水时间.Text);
            dgv回水记录.DataSource = RecordDao.Water(date);
        }

        private void btn保存回水设置_Click(object sender, EventArgs e)
        {
            foreach (var txt in GetAllTextBoxControls(grp退分计算方法))
            {
                if (txt.Text.Trim() != "" && !int.TryParse(txt.Text, out int num))
                {
                    MessageBox.Show(txt.Text + " 不合法");
                    return;
                }
            }

            WaterConfig.GetInstance().回水条件1 = chk回水条件1.Checked;
            WaterConfig.GetInstance().回水条件2 = chk回水条件2.Checked;
            WaterConfig.GetInstance().回水条件3 = chk回水条件3.Checked;
            WaterConfig.GetInstance().回水条件4 = chk回水条件4.Checked;
            WaterConfig.GetInstance().回水条件5 = chk回水条件5.Checked;
            WaterConfig.GetInstance().回水条件6 = chk回水条件6.Checked;
            WaterConfig.GetInstance().回水条件7 = chk回水条件7.Checked;
            WaterConfig.GetInstance().回水条件8 = chk回水条件8.Checked;

            if (int.TryParse(txt回水条件1.Text, out int val1))
            {
                WaterConfig.GetInstance().回水条件参数1 = val1;
            }
            if (int.TryParse(txt回水条件2.Text, out int val2))
            {
                WaterConfig.GetInstance().回水条件参数2 = val2;
            }
            if (int.TryParse(txt回水条件3.Text, out int val3))
            {
                WaterConfig.GetInstance().回水条件参数3 = val3;
            }
            if (int.TryParse(txt回水条件4.Text, out int val4))
            {
                WaterConfig.GetInstance().回水条件参数4 = val4;
            }
            if (int.TryParse(txt回水条件5.Text, out int val5))
            {
                WaterConfig.GetInstance().回水条件参数5 = val5;
            }
            if (int.TryParse(txt回水条件6.Text, out int val6))
            {
                WaterConfig.GetInstance().回水条件参数6 = val6;
            }
            if (int.TryParse(txt回水条件7.Text, out int val7))
            {
                WaterConfig.GetInstance().回水条件参数7 = val7;
            }
            if (int.TryParse(txt回水条件8.Text, out int val8))
            {
                WaterConfig.GetInstance().回水条件参数8 = val8;
            }

            WaterConfig.GetInstance().按输分比例 = rad按输分比例.Checked;
            WaterConfig.GetInstance().按竞猜总分比例 = rad按竞猜总分比例.Checked;
            WaterConfig.GetInstance().按竞猜总分退分 = rad按竞猜总分退分.Checked;
            WaterConfig.GetInstance().按竞猜期数 = rad按竞猜期数.Checked;

            TextBox[] txtA1 = this.Controls.Find("txtA1", true).OfType<TextBox>().ToArray();
            TextBox[] txtA2 = this.Controls.Find("txtA2", true).OfType<TextBox>().ToArray();
            TextBox[] txtB1 = this.Controls.Find("txtB1", true).OfType<TextBox>().ToArray();
            TextBox[] txtB2 = this.Controls.Find("txtB2", true).OfType<TextBox>().ToArray();
            TextBox[] txtC1 = this.Controls.Find("txtC1", true).OfType<TextBox>().ToArray();
            TextBox[] txtC2 = this.Controls.Find("txtC2", true).OfType<TextBox>().ToArray();
            TextBox[] txtD1 = this.Controls.Find("txtD1", true).OfType<TextBox>().ToArray();
            TextBox[] txtD2 = this.Controls.Find("txtD2", true).OfType<TextBox>().ToArray();

            WaterConfig.GetInstance().A1 = txtA1.Select(x => x.Text).ToArray();
            WaterConfig.GetInstance().A2 = txtA2.Select(x => x.Text).ToArray();
            WaterConfig.GetInstance().B1 = txtB1.Select(x => x.Text).ToArray();
            WaterConfig.GetInstance().B2 = txtB2.Select(x => x.Text).ToArray();
            WaterConfig.GetInstance().C1 = txtC1.Select(x => x.Text).ToArray();
            WaterConfig.GetInstance().C2 = txtC2.Select(x => x.Text).ToArray();
            WaterConfig.GetInstance().D1 = txtD1.Select(x => x.Text).ToArray();
            WaterConfig.GetInstance().D2 = txtD2.Select(x => x.Text).ToArray();

            ReadWriteINI.ConfigFile_SetVal("回水设置", "回水配置", WaterConfig.GetInstance().ToJson());
            MessageBox.Show("保存成功！");
        }

        private void GameAssistantForm_Load(object sender, EventArgs e)
        {
            int lastColumnIndex1 = dgv查回统计.Columns.Count - 1;
            dgv查回统计.Columns[lastColumnIndex1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            int lastColumnIndex2 = dgv玩家猜猜明细.Columns.Count - 1;
            dgv玩家猜猜明细.Columns[lastColumnIndex2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            int lastColumnIndex3 = dgv回水记录.Columns.Count - 1;
            dgv回水记录.Columns[lastColumnIndex3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            int lastColumnIndex4 = dgv操作记录.Columns.Count - 1;
            dgv操作记录.Columns[lastColumnIndex4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            int lastColumnIndex5 = dgv帐变记录.Columns.Count - 1;
            dgv帐变记录.Columns[lastColumnIndex5].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            //dgv回水计算.Columns["回水计算_反向组合期数"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            chk回水条件1.Checked = WaterConfig.GetInstance().回水条件1;
            chk回水条件2.Checked = WaterConfig.GetInstance().回水条件2;
            chk回水条件3.Checked = WaterConfig.GetInstance().回水条件3;
            chk回水条件4.Checked = WaterConfig.GetInstance().回水条件4;
            chk回水条件5.Checked = WaterConfig.GetInstance().回水条件5;
            chk回水条件6.Checked = WaterConfig.GetInstance().回水条件6;
            chk回水条件7.Checked = WaterConfig.GetInstance().回水条件7;
            chk回水条件8.Checked = WaterConfig.GetInstance().回水条件8;

            txt回水条件1.Text = WaterConfig.GetInstance().回水条件参数1.ToString();
            txt回水条件2.Text = WaterConfig.GetInstance().回水条件参数2.ToString();
            txt回水条件3.Text = WaterConfig.GetInstance().回水条件参数3.ToString();
            txt回水条件4.Text = WaterConfig.GetInstance().回水条件参数4.ToString();
            txt回水条件5.Text = WaterConfig.GetInstance().回水条件参数5.ToString();
            txt回水条件6.Text = WaterConfig.GetInstance().回水条件参数6.ToString();
            txt回水条件7.Text = WaterConfig.GetInstance().回水条件参数7.ToString();
            txt回水条件8.Text = WaterConfig.GetInstance().回水条件参数8.ToString();

            rad按输分比例.Checked = WaterConfig.GetInstance().按输分比例;
            rad按竞猜总分比例.Checked = WaterConfig.GetInstance().按竞猜总分比例;
            rad按竞猜总分退分.Checked = WaterConfig.GetInstance().按竞猜总分退分;
            rad按竞猜期数.Checked = WaterConfig.GetInstance().按竞猜期数;

            TextBox[] txtA1 = this.Controls.Find("txtA1", true).OfType<TextBox>().ToArray();
            TextBox[] txtA2 = this.Controls.Find("txtA2", true).OfType<TextBox>().ToArray();
            TextBox[] txtB1 = this.Controls.Find("txtB1", true).OfType<TextBox>().ToArray();
            TextBox[] txtB2 = this.Controls.Find("txtB2", true).OfType<TextBox>().ToArray();
            TextBox[] txtC1 = this.Controls.Find("txtC1", true).OfType<TextBox>().ToArray();
            TextBox[] txtC2 = this.Controls.Find("txtC2", true).OfType<TextBox>().ToArray();
            TextBox[] txtD1 = this.Controls.Find("txtD1", true).OfType<TextBox>().ToArray();
            TextBox[] txtD2 = this.Controls.Find("txtD2", true).OfType<TextBox>().ToArray();

            for (int i = 0; i < WaterConfig.GetInstance().A1.Length; i++)
            {
                txtA1[i].Text = WaterConfig.GetInstance().A1[i].ToString();
            }
            for (int i = 0; i < WaterConfig.GetInstance().A2.Length; i++)
            {
                txtA2[i].Text = WaterConfig.GetInstance().A2[i].ToString();
            }
            for (int i = 0; i < WaterConfig.GetInstance().B1.Length; i++)
            {
                txtB1[i].Text = WaterConfig.GetInstance().B1[i].ToString();
            }
            for (int i = 0; i < WaterConfig.GetInstance().B2.Length; i++)
            {
                txtB2[i].Text = WaterConfig.GetInstance().B2[i].ToString();
            }
            for (int i = 0; i < WaterConfig.GetInstance().C1.Length; i++)
            {
                txtC1[i].Text = WaterConfig.GetInstance().C1[i].ToString();
            }
            for (int i = 0; i < WaterConfig.GetInstance().C2.Length; i++)
            {
                txtC2[i].Text = WaterConfig.GetInstance().C2[i].ToString();
            }
            for (int i = 0; i < WaterConfig.GetInstance().D1.Length; i++)
            {
                txtD1[i].Text = WaterConfig.GetInstance().D1[i].ToString();
            }
            for (int i = 0; i < WaterConfig.GetInstance().D2.Length; i++)
            {
                txtD2[i].Text = WaterConfig.GetInstance().D2[i].ToString();
            }
        }

        private List<TextBox> GetAllTextBoxControls(Control control)
        {
            List<TextBox> textBoxes = new List<TextBox>();
            AddTextBoxes(control, textBoxes);
            return textBoxes;
        }

        private void AddTextBoxes(Control control, List<TextBox> textBoxes)
        {
            if (control is TextBox)
            {
                textBoxes.Add(control as TextBox);
            }

            foreach (Control subControl in control.Controls)
            {
                AddTextBoxes(subControl, textBoxes);
            }
        }

        private void btn回水计算_Click(object sender, EventArgs e)
        {
            DateTime begin = DateTime.Parse(date开始日期.Text);
            DateTime end = DateTime.Parse(date结束日期.Text).AddDays(1);
            int isDummy = -1;
            if (rad回水计算真人.Checked)
            {
                isDummy = 0;
            }
            if (rad回水计算假人.Checked)
            {
                isDummy = 1;
            }
            DataTable dt = BettingDao.CalcWater(begin, end, isDummy);
            dt.Columns.Add("组合占比", typeof(int));
            dt.Columns.Add("计算退分", typeof(int));
            foreach (DataRow dr in dt.Rows)
            {
                if ((int)dr["竞猜期数"] > 0)
                {
                    dr["组合占比"] = (int)Math.Round((int)dr["组合分数"] * 100.0 / (int)dr["投注总额"]);
                }

                /*                #region 计算回水,累进算法
                                if (WaterConfig.GetInstance().按输分比例) {
                                    String[] A1 = WaterConfig.GetInstance().A1;
                                    String[] A2 = WaterConfig.GetInstance().A2;
                                    int preStep = 0;
                                    double water = 0;
                                    if ((double)dr["盈亏"] < 0)
                                    {
                                        for (int i = 0; i < A1.Length; i++)
                                        {
                                            if (string.IsNullOrWhiteSpace(A1[i]))
                                            {
                                                break;
                                            }
                                            if ((double)dr["盈亏"] < -int.Parse(A1[i]))
                                            {
                                                water += (int.Parse(A1[i]) - preStep) * double.Parse(A2[i]) * 0.01;
                                                preStep = int.Parse(A1[i]);
                                            }
                                            else
                                            {
                                                water += (-(double)dr["盈亏"] - preStep) * double.Parse(A2[i]) * 0.01;
                                                break;
                                            }
                                        }
                                    }
                                    dr["计算退分"] = water;
                                }
                                if (WaterConfig.GetInstance().按竞猜总分比例)
                                {
                                    String[] B1 = WaterConfig.GetInstance().B1;
                                    String[] B2 = WaterConfig.GetInstance().B2;
                                    int preStep = 0;
                                    double water = 0;
                                    for (int i = 0; i < B1.Length; i++)
                                    {
                                        if (string.IsNullOrWhiteSpace(B1[i]))
                                        {
                                            break;
                                        }
                                        if ((double)dr["投注总额"] > int.Parse(B1[i]))
                                        {
                                            water += (int.Parse(B1[i]) - preStep) * double.Parse(B2[i]) * 0.01;
                                            preStep = int.Parse(B1[i]);
                                        }
                                        else
                                        {
                                            water += ((double)dr["投注总额"] - preStep) * double.Parse(B2[i]) * 0.01;
                                            break;
                                        }
                                    }
                                    dr["计算退分"] = water;
                                }
                                if (WaterConfig.GetInstance().按竞猜总分退分)
                                {
                                    String[] C1 = WaterConfig.GetInstance().C1;
                                    String[] C2 = WaterConfig.GetInstance().C2;
                                    double water = 0;
                                    for (int i = 0; i < C1.Length; i++)
                                    {
                                        if (string.IsNullOrWhiteSpace(C1[i]))
                                        {
                                            break;
                                        }
                                        if ((double)dr["投注总额"] > int.Parse(C1[i]))
                                        {
                                            water += double.Parse(C2[i]);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    dr["计算退分"] = water;
                                }
                                if (WaterConfig.GetInstance().按竞猜期数)
                                {
                                    String[] D1 = WaterConfig.GetInstance().D1;
                                    String[] D2 = WaterConfig.GetInstance().D2;
                                    double water = 0;
                                    for (int i = 0; i < D1.Length; i++)
                                    {
                                        if (string.IsNullOrWhiteSpace(D1[i]))
                                        {
                                            break;
                                        }
                                        if ((double)dr["竞猜期数"] > int.Parse(D1[i]))
                                        {
                                            water += double.Parse(D2[i]);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    dr["计算退分"] = water;
                                }
                                #endregion*/


                #region 计算回水,直接计算
                if (WaterConfig.GetInstance().按输分比例)
                {
                    String[] A1 = WaterConfig.GetInstance().A1;
                    String[] A2 = WaterConfig.GetInstance().A2;
                    double waterPercent = 0;
                    if ((int)dr["盈亏"] < 0)
                    {
                        for (int i = 0; i < A1.Length; i++)
                        {
                            if (string.IsNullOrWhiteSpace(A1[i]))
                            {
                                break;
                            }
                            if ((int)dr["盈亏"] < -int.Parse(A1[i]))
                            {
                                waterPercent = double.Parse(A2[i]);
                            }
                        }
                    }
                    dr["计算退分"] = -(int)((int)dr["盈亏"] * waterPercent * 0.01);
                }
                if (WaterConfig.GetInstance().按竞猜总分比例)
                {
                    String[] B1 = WaterConfig.GetInstance().B1;
                    String[] B2 = WaterConfig.GetInstance().B2;
                    double waterPercent = 0;
                    for (int i = 0; i < B1.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(B1[i]))
                        {
                            break;
                        }
                        if ((int)dr["投注总额"] > int.Parse(B1[i]))
                        {
                            waterPercent = double.Parse(B2[i]);
                        }
                    }
                    dr["计算退分"] = (int)((int)dr["流水"] * waterPercent * 0.01);
                }
                if (WaterConfig.GetInstance().按竞猜总分退分)
                {
                    String[] C1 = WaterConfig.GetInstance().C1;
                    String[] C2 = WaterConfig.GetInstance().C2;
                    int water = 0;
                    for (int i = 0; i < C1.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(C1[i]))
                        {
                            break;
                        }
                        if ((int)dr["投注总额"] > int.Parse(C1[i]))
                        {
                            water += int.Parse(C2[i]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    dr["计算退分"] = water;
                }
                if (WaterConfig.GetInstance().按竞猜期数)
                {
                    String[] D1 = WaterConfig.GetInstance().D1;
                    String[] D2 = WaterConfig.GetInstance().D2;
                    int water = 0;
                    for (int i = 0; i < D1.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(D1[i]))
                        {
                            break;
                        }
                        if ((int)dr["竞猜期数"] > int.Parse(D1[i]))
                        {
                            water += int.Parse(D2[i]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    dr["计算退分"] = water;
                }
                #endregion

                //处理回水条件
                //竞猜不到  期，无退分
                if (WaterConfig.GetInstance().回水条件1)
                {
                    if ((int)dr["竞猜期数"] < WaterConfig.GetInstance().回水条件参数1)
                    {
                        dr["计算退分"] = 0;
                    }
                }

                //竞猜不到  期，且无组合，无回水
                if (WaterConfig.GetInstance().回水条件2)
                {
                    if ((int)dr["竞猜期数"] < WaterConfig.GetInstance().回水条件参数2 && (int)dr["组合期数"] == 0)
                    {
                        dr["计算退分"] = 0;
                    }
                }

                //所有组合占总竞猜分数  %以下，无回水
                if (WaterConfig.GetInstance().回水条件3)
                {
                    if (Convert.ToInt32(dr["组合占比"]) < WaterConfig.GetInstance().回水条件参数3)
                    {
                        dr["计算退分"] = 0;
                    }
                }

                //单点数字和极大极小一共竞猜少于  次，无回水
                if (WaterConfig.GetInstance().回水条件4)
                {
                    if ((int)dr["单点极值期数"] < WaterConfig.GetInstance().回水条件参数4)
                    {
                        dr["计算退分"] = 0;
                    }
                }

                //"大双"和"小单"竞猜总分达到  %，无回水
                if (WaterConfig.GetInstance().回水条件5)
                {
                    if ((int)dr["大双小单分数"] * 100.0 / (int)dr["投注总额"] >= WaterConfig.GetInstance().回水条件参数5)
                    {
                        dr["计算退分"] = 0;
                    }
                }
                //杀组合达到  期，无回水
                if (WaterConfig.GetInstance().回水条件6)
                {
                    if ((int)dr["杀组合期数"] > WaterConfig.GetInstance().回水条件参数6)
                    {
                        dr["计算退分"] = 0;
                    }
                }
                //反向组合达到  期，无回水
                if (WaterConfig.GetInstance().回水条件7)
                {
                    if ((int)dr["反向组合期数"] > WaterConfig.GetInstance().回水条件参数7)
                    {
                        dr["计算退分"] = 0;
                    }
                }
                //同向组合达到  期，无回水
                if (WaterConfig.GetInstance().回水条件8)
                {
                    if ((int)dr["同向组合期数"] > WaterConfig.GetInstance().回水条件参数8)
                    {
                        dr["计算退分"] = 0;
                    }
                }
            }


            dt.Columns.Add("已退分", typeof(int));
            dt.Columns.Add("自助流水", typeof(int));
            dt.Columns.Add("剩余流水", typeof(int));
            dt.Columns.Add("自助回水", typeof(int));
            dt.Columns.Add("剩余回水", typeof(int));


            DataTable dtBettings = BettingDao.GetBettings(begin, end, isDummy);
            DataTable amountDt = BettingDao.GetAllReturnJifen(begin, end);
            foreach (DataRow dr in dt.Rows)
            {
                //计算自助流水、回水
                string userCode = (string)dr["Code"];
                User user = RobotClient.GetUser(userCode);
                DataRow[] userBettings = dtBettings.Select($"code='{userCode}'");
                int runningWater = 0;
                int returnWater = 0;
                if (userBettings != null)
                {
                    DataRow row = userBettings[0];
                    runningWater = BettingUtil.CalcAutoRunningWater(user, (int)(long)row["sumWater"], (int)(long)row["sumBlyk"]);
                    returnWater = BettingUtil.CalcAutoReturnWater(user, (int)(long)row["sumWater"], (int)(long)row["sumBlyk"]);
                }
                dr["自助流水"] = runningWater;
                dr["自助回水"] = returnWater;

                DataRow[] userData = amountDt.Select($"code='{userCode}'");
                DataRow dr5 = userData.FirstOrDefault(d => (long)d["type"] == 5);
                DataRow dr6 = userData.FirstOrDefault(d => (long)d["type"] == 6);
                DataRow dr7 = userData.FirstOrDefault(d => (long)d["type"] == 7);
                DataRow dr8 = userData.FirstOrDefault(d => (long)d["type"] == 8);
                DataRow dr9 = userData.FirstOrDefault(d => (long)d["type"] == 9);

                //退分5
                int tf = dr5 == null ? 0 : (int)(long)dr5["sumAmount"];
                int cstf = dr9 == null ? 0 : (int)(long)dr9["sumAmount"];
                //自动流水6
                int zdls = dr6 == null ? 0 : (int)(long)dr6["sumAmount"];
                //自助回水7, 退自助回水8
                int zdfs = dr7 == null ? 0 : (int)(long)dr7["sumAmount"];
                int tzdfx = dr8 == null ? 0 : (int)(long)dr8["sumAmount"];

                dr["已退分"] = tf + cstf;
                dr["剩余流水"] = runningWater - zdls;
                dr["剩余回水"] = returnWater - (zdfs + tzdfx);
            }

            dgv回水计算.DataSource = dt;

            txt竞猜总人数.Text = dt.Rows.Count.ToString();
            txt竞猜总分数.Text = dt.Compute("Sum(流水) ", "").ToString();
            txt赢总人数.Text = dt.Compute("Count(盈亏)", "盈亏>0").ToString() + "/" + dt.Compute("Sum(盈亏)", "盈亏>0").ToString();
            txt输总人数.Text = dt.Compute("Count(盈亏)", "盈亏<0").ToString() + "/" + dt.Compute("Sum(盈亏)", "盈亏<0").ToString();
            object jczyk = dt.Compute("Sum(盈亏)", "");
            txt竞猜总盈亏.Text = jczyk == DBNull.Value ? "0" : Convert.ToString(-Convert.ToInt32(jczyk));
            double sum回水总分数 = dt.AsEnumerable().Sum(row => Convert.ToDouble(row["计算退分"]));
            txt回水总分数.Text = Convert.ToString(sum回水总分数);
            txt回水总人数.Text = dt.Compute("Count(计算退分)", "计算退分>0").ToString();
            if (dt.Rows.Count > 0)
            {
                btn退分.Enabled = true;
                btn回水挂帐单.Enabled = true;
                btn流水挂帐单.Enabled = true;
                if (date开始日期.Value.Date == date结束日期.Value.Date)
                {
                    btn撤回退分.Enabled = true;
                    btn撤回已挂剩余自助流水.Enabled = true;
                }
                else
                {
                    btn撤回退分.Enabled = false;
                    btn撤回已挂剩余自助流水.Enabled = false;
                }
            }
            else
            {
                btn退分.Enabled = false;
                btn回水挂帐单.Enabled = false;
                btn流水挂帐单.Enabled = false;
                btn撤回退分.Enabled = false;
                btn撤回已挂剩余自助流水.Enabled = false;
            }
        }

        private void btn数据导出_Click(object sender, EventArgs e)
        {
            string file = null;
            if (tabMain.SelectedTab.Text == "查回统计")
            {
                file = ExcelUtil.ExportDataGridViewToExcel(dgv查回统计, "查回统计");
            }
            else if (tabMain.SelectedTab.Text == "玩家猜猜明细")
            {
                file = ExcelUtil.ExportDataGridViewToExcel(dgv玩家猜猜明细, "猜猜明细");
            }
            else if (tabMain.SelectedTab.Text == "回水计算")
            {
                file = ExcelUtil.ExportDataGridViewToExcel(dgv回水计算, "回水计算");
            }
            else if (tabMain.SelectedTab.Text == "操作记录")
            {
                file = ExcelUtil.ExportDataGridViewToExcel(dgv操作记录, "操作记录");
            }
            else if (tabMain.SelectedTab.Text == "帐变记录")
            {
                file = ExcelUtil.ExportDataGridViewToExcel(dgv帐变记录, "帐变记录");
            }
            if (file != null)
            {
                if (MessageBox.Show("成功导出文件：\n" + file + "\n是否现在打开所在文件夹！", "确认对话框",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    try
                    {
                        ExcelUtil.OpenFileFolder(file);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void chk只显示有退分的_CheckedChanged(object sender, EventArgs e)
        {
            dgv回水计算.CurrentCell = null;
            foreach (DataGridViewRow dgvr in dgv回水计算.Rows)
            {
                if (Convert.ToInt32(dgvr.Cells["回水计算_计算退分"].Value) == 0)
                {
                    dgvr.Visible = !chk只显示有退分的.Checked;
                }
            }
        }

        private void btn查找_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dgvr in dgv回水计算.Rows)
            {
                if (dgvr.Cells["回水计算_昵称"].Value.ToString().Contains(txt昵称.Text))
                {
                    dgvr.Selected = true;
                    dgv回水计算.CurrentCell = dgvr.Cells["回水计算_昵称"];
                }
            }
        }

        private void btn退分_Click(object sender, EventArgs e)
        {
            if(dgv回水计算.Rows.Cast<DataGridViewRow>().Any(row => Convert.ToInt32(row.Cells["已退分"].Value) > 0))
            {
                MessageBox.Show("退分已挂帐单");
                return;
            }
            if (MessageBox.Show("是否确定要退分挂帐单？", "确认对话框",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                DateTime begin = DateTime.Parse(date开始日期.Text);
                DateTime end = DateTime.Parse(date结束日期.Text);
                string remark = begin.ToString("D") + "到" + end.ToString("D");
                foreach (DataGridViewRow dr in dgv回水计算.Rows)
                {
                    string code = (string)dr.Cells["回水计算_Code"].Value;
                    int water = Convert.ToInt32(dr.Cells["回水计算_计算退分"].Value);
                    if (water > 0)
                    {
                        User user = RobotClient.GetUser(code);
                        if (user != null)
                        {
                            user.ChangeJifen(water, RecordType.退分, 0, remark, end);
                            dr.Cells["回水计算_计算退分"].Style.ForeColor = Color.Red;
                            dr.Cells["回水计算_计算退分"].Value = "已退分";
                        }
                        else
                        {
                            MessageBox.Show("退分失败，用户[" + code + "]丢失");
                            return;
                        }
                    }
                }
                ResultDao.SaveStatus(begin, end.AddDays(1), ResultStatus.已结算, ResultStatus.已返水);
                btn退分.Enabled = false;
                OplogDao.Add(begin.ToString("D") + "到" + end.ToString("D") + "退分挂入帐单");
                btn撤回退分.Enabled = false;
                MessageBox.Show("退分成功挂入！");
            }
        }

        private void btn回水挂帐单_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否确定将剩余自助回水挂账单？", "确认对话框",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                DateTime begin = DateTime.Parse(date开始日期.Text);
                DateTime end = DateTime.Parse(date结束日期.Text);
                string remark = begin.ToString("D") + "到" + end.ToString("D");
                foreach (DataGridViewRow dr in dgv回水计算.Rows)
                {
                    string code = (string)dr.Cells["回水计算_Code"].Value;
                    int water = Convert.ToInt32(dr.Cells["剩余回水"].Value);
                    if (water > 0)
                    {
                        User user = RobotClient.GetUser(code);
                        if (user != null)
                        {
                            user.ChangeJifen(water, RecordType.自动回水, 0, remark, end);
                            dr.Cells["剩余回水"].Style.ForeColor = Color.Red;
                            dr.Cells["剩余回水"].Value = "已挂入";
                        }
                        else
                        {
                            MessageBox.Show("剩余回水挂入失败，用户[" + code + "]丢失");
                            return;
                        }
                    }
                }
                BettingDao.FinishReturnWater(begin, end.AddDays(1));
                btn回水挂帐单.Enabled = false;
                OplogDao.Add(begin.ToString("D") + "到" + end.ToString("D") + "剩余回水挂入挂入帐单");
                MessageBox.Show("剩余回水挂入成功！");
            }
        }

        private void btn流水挂帐单_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否确定将剩余自助流水挂账单？", "确认对话框",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                DateTime begin = DateTime.Parse(date开始日期.Text);
                DateTime end = DateTime.Parse(date结束日期.Text);
                string remark = begin.ToString("D") + "到" + end.ToString("D");
                foreach (DataGridViewRow dr in dgv回水计算.Rows)
                {
                    string code = (string)dr.Cells["回水计算_Code"].Value;
                    int water = Convert.ToInt32(dr.Cells["剩余流水"].Value);
                    if (water > 0)
                    {
                        User user = RobotClient.GetUser(code);
                        if (user != null)
                        {
                            user.ChangeJifen(water, RecordType.剩余自助流水, 0, remark, end);
                            dr.Cells["剩余流水"].Style.ForeColor = Color.Red;
                            dr.Cells["剩余流水"].Value = "已挂入";
                        }
                        else
                        {
                            MessageBox.Show("剩余流水挂入失败，用户[" + code + "]丢失");
                            return;
                        }
                    }
                }
                BettingDao.FinishRunningWater(begin, end.AddDays(1));
                btn流水挂帐单.Enabled = false;
                OplogDao.Add(begin.ToString("D") + "到" + end.ToString("D") + "剩余流水挂入帐单");
                btn撤回已挂剩余自助流水.Enabled = false;
                MessageBox.Show("剩余流水挂入成功！");
            }
        }

        private void btn撤回退分_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否确定撤消当前查询到已退分的用户？", "确认对话框",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                DateTime begin = DateTime.Parse(date开始日期.Text);
                DateTime end = DateTime.Parse(date结束日期.Text);
                string remark = begin.ToString("D") + "到" + end.ToString("D");
                foreach (DataGridViewRow dr in dgv回水计算.Rows)
                {
                    string code = (string)dr.Cells["回水计算_Code"].Value;
                    int water = Convert.ToInt32(dr.Cells["已退分"].Value);
                    if (water > 0)
                    {
                        User user = RobotClient.GetUser(code);
                        if (user != null)
                        {
                            user.ChangeJifen(-water, RecordType.退分撤消, 0, remark, end, true);
                            dr.Cells["已退分"].Style.ForeColor = Color.Red;
                            dr.Cells["已退分"].Value = "已撤消";
                        }
                        else
                        {
                            MessageBox.Show("撤消退分失败，用户[" + code + "]丢失");
                            return;
                        }
                    }
                }
                ResultDao.SaveStatus(begin, end.AddDays(1), ResultStatus.已返水, ResultStatus.已结算);
                btn流水挂帐单.Enabled = false;
                OplogDao.Add(begin.ToString("D") + "到" + end.ToString("D") + "撤消退分");
                MessageBox.Show("撤消退分成功！");
            }
        }

        private void btn撤回已挂剩余自助流水_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否确定撤回已挂剩余自助流水？", "确认对话框",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                DateTime begin = DateTime.Parse(date开始日期.Text);
                DateTime end = DateTime.Parse(date结束日期.Text);
                string remark = begin.ToString("D") + "到" + end.ToString("D");
                foreach (DataGridViewRow dr in dgv回水计算.Rows)
                {
                    string code = (string)dr.Cells["回水计算_Code"].Value;
                    int water = Convert.ToInt32(dr.Cells["剩余流水"].Value);
                    if (water > 0)
                    {
                        User user = RobotClient.GetUser(code);
                        if (user != null)
                        {
                            user.ChangeJifen(-water, RecordType.退分撤消, 0, remark, end);
                            dr.Cells["剩余流水"].Style.ForeColor = Color.Red;
                            dr.Cells["剩余流水"].Value = "已撤消";
                        }
                        else
                        {
                            MessageBox.Show("撤消剩余流水失败，用户[" + code + "]丢失");
                            return;
                        }
                    }
                }
                BettingDao.CancelRunningWater(begin, end.AddDays(1));
                btn流水挂帐单.Enabled = false;
                OplogDao.Add(begin.ToString("D") + "到" + end.ToString("D") + "撤回已挂剩余自助流水");
                MessageBox.Show("撤回已挂剩余自助流水成功！");
            }
        }

        private void cbo帐变原因_SelectedIndexChanged(object sender, EventArgs e)
        {
            DateTime begin = DateTime.Parse(date开始日期.Text);
            DateTime end = DateTime.Parse(date结束日期.Text).AddDays(1);
            dgv帐变记录.DataSource = RecordDao.List(begin, end, txt昵称.Text, cbo帐变原因.SelectedItem);
        }
    }
}
