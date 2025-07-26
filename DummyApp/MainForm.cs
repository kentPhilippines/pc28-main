using CommonLibrary;
using DummyApp.Model;
using DummyApp.Util;
using ImLibrary.IM;
using ImLibrary.Model;
using System.Windows.Forms;
using System.Runtime.Versioning;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Websocket.Client;

namespace DummyApp;

[SupportedOSPlatform("windows")]
public partial class MainForm : Form
{
    private static MainForm instance;
    private object _lockObject = new object();
    private BindingList<Dummy> dummies = new BindingList<Dummy>();
    public Dictionary<string, IMGroup> GroupMap = new Dictionary<string, IMGroup>();
    private IMUser robot;
    private List<string> templateList = new List<string>();

    public MainForm()
    {
        InitializeComponent();
        dgv托列表.TopLeftHeaderCell.Value = "序号";
        下注模板.DataSource = templateList;
        dgv托列表.AutoGenerateColumns = false;
        dgv托列表.DataSource = dummies;
    }

    public static MainForm GetInstance()
    {
        if (instance == null || instance.IsDisposed) instance = new MainForm();
        return instance;
    }

    public void SetGroupInfo(IMUser robot)
    {
        this.robot = robot;
        tssl监控群ID.Text = this.robot.DefauleGroup.GroupId;
        tssl机器人ID.Text = this.robot.UserId;
    }

    private void 粘贴帐号_Click(object sender, EventArgs e)
    {
        // 从剪贴板获取数据并粘贴到textBoxAccount中
        string clipboardText = Clipboard.GetText();
        if (!string.IsNullOrEmpty(clipboardText))
        {
            var lines = clipboardText.Split(new string[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var values = line.Split('\t'); // 假设每行通过制表符分隔
                if (values.Length == 3)
                {
                    if (dummies.FirstOrDefault(u => u.LoginName == values[1]) != null)
                    {
                        MessageBox.Show(values[1] + "已存在");
                        return;
                    }

                    var d = new Dummy(values[0], values[1], values[2], "");
                    d.logAction += Dummy_logAction;
                    dummies.Add(d);
                }
                else
                {
                    MessageBox.Show("格式不对");
                    return;
                }
            }
        }
    }

    private async void 一键登录_Click(object sender, EventArgs e)
    {
        if (dummies.Where(d => d.Checked).Count() == 0)
        {
            MessageBox.Show("请选择要操作的假人");
            return;
        }

        一键登录.Enabled = false;
        var checkDummies = dummies.Where(d => d.Checked);
        Task[] loginTasks = new Task[checkDummies.Count()];
        List<Task> taskList = new List<Task>();
        bool errorFlag = false;
        for (int i = 0; i < checkDummies.Count(); i++)
        {
            Dummy dummy = checkDummies.ElementAt(i);
            string defaultGroupId = robot.DefauleGroup.GroupId;
            dummy.OnReceiveRawPacketComomand += Dummy_OnReceiveRawPacketComomand;
            dummy.OnDisconnect += Dummy_OnDisconnect;

            Task task = Task.Run(() =>
            {
                try
                {
                    if (!dummy.CreateToken(dummy.LoginName, dummy.Password))
                    {
                        dummy.Status = DummyStatus.登录失败;
                    }
                    else
                    {
                        if (dummy.TryGetGroups())
                        {
                            if (dummy.GroupMap.TryGetValue(defaultGroupId, out IMGroup defaultGroup))
                            {
                                dummy.DefauleGroup = defaultGroup;
                                dummy.DefauleGroup.TryGetMembers(dummy.UserId);
                                try
                                {
                                    dummy.TryLogin();
                                    dummy.Status = DummyStatus.登录成功;
                                }
                                catch (Exception)
                                {
                                    dummy.Status = DummyStatus.连接失败;
                                }
                            }
                            else
                            {
                                dummy.Status = DummyStatus.不是群成员;
                            }
                        }
                        else
                        {
                            dummy.Status = DummyStatus.不是群成员;
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorFlag = true;
                    dummy.Status = DummyStatus.登录失败;
                    log.Info(dummy.LoginName + " " + ex.Message);
                }
            });
            taskList.Add(task);
            if (taskList.Count == 5)
            {
                await Task.WhenAll(taskList.ToArray());
                taskList = new List<Task>();
            }
        }

        await Task.WhenAll(taskList.ToArray());
        if (errorFlag) return;
        SetButtonStatus(false, true, true);
        Debug.WriteLine("登录成功!");
        log.Error("登录成功,请点一键启动开始自动下注！");
    }

    private void Dummy_OnDisconnect(IMUser user, DisconnectionInfo info)
    {
        var dummy = (Dummy)user;
        dummy.Status = DummyStatus.已退出;
        SetButtonStatus(true);
    }

    private async void Dummy_OnReceiveRawPacketComomand(IMUser imUser, string fromUid, string nickName, string message,
        string fp)
    {
        //Debug.WriteLine(message);
        var dummy = (Dummy)imUser;
        if (robot.UserId != fromUid) return;

        var uc = CentralApi.GetUserConfig("game.config.基础设置");
        var bc = uc.configValue.ToEntity<BasicConfig>();
        if (dummy.IsWorking)
        {
            if (message.StartsWith(bc.开盘标识))
            {
                Debug.WriteLine("开盘了");
                string bill = StringUtil.Extract(message, bc.帐单头标识, bc.帐单尾标识)?.Trim();
                try
                {
                    dummy.Jifen = Convert.ToDouble(StringUtil.ExtractNumberAfterWord(bill, dummy.BillName));
                }
                catch (Exception)
                {
                }

                await dummy.AutoCha();
                await dummy.AutoHui();
                await dummy.AutoChat();
                await dummy.StartWorker();
            }

            if (message.StartsWith(bc.封盘标识))
            {
                Debug.WriteLine("封盘了");
                await dummy.AutoChat(true);
            }
        }
    }

    private void 保存设置_Click(object sender, EventArgs e)
    {
        var uc = new UserConfig();
        uc.configKey = "dummy.list";
        uc.configValue = dummies.ToJson();
        uc.configName = "假人列表";
        if (CentralApi.SaveConfig(uc))
            MessageBox.Show("保存成功");
        else
            MessageBox.Show("保存失败");
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        //导入默认基础设置
        var basicUC = CentralApi.GetUserConfig("game.config.基础设置");
        if (basicUC == null)
        {
            var bc = new BasicConfig();
            bc.开始运行标识 = "开始";
            bc.停止运行标识 = "停止";
            bc.开盘标识 = "在线人数";
            bc.封盘标识 = "已截止投注,不退不改";
            bc.帐单头标识 = "════════════";
            bc.帐单尾标识 = "════════════";
            bc.帐单识别 = "(\\S\\D{0,2})(\\d+)";

            basicUC = new UserConfig();
            basicUC.configName = "基础设置";
            basicUC.configKey = "game.config.基础设置";
            basicUC.configValue = bc.ToJson();
            CentralApi.SaveConfig(basicUC);

            TemplateUtil.ImportTemplate(AppContext.BaseDirectory + @"Template\默认低档下注.ini");
            TemplateUtil.ImportTemplate(AppContext.BaseDirectory + @"Template\默认中档下注.ini");
            TemplateUtil.ImportTemplate(AppContext.BaseDirectory + @"Template\默认高档下注.ini");
        }

        ReloadTemplateList();

        List<Dummy> list = CentralApi.GetUserConfig("dummy.list")?.configValue.ToEntityList<Dummy>();
        if (list != null)
            foreach (Dummy dummy in list)
            {
                dummy.logAction += Dummy_logAction;
                dummies.Add(dummy);
            }
    }

    private void ReloadTemplateList()
    {
        templateList.Clear();
        List<UserConfig> configList = CentralApi.GetUserConfigsByPrefix("game.user.template.");
        if (configList == null)
        {
            MessageBox.Show("获取模板配置时出错,请重试！");
            return;
        }

        templateList.Add("");
        foreach (UserConfig config in configList) templateList.Add(config.configName);
    }

    private void Dummy_logAction(string info)
    {
        log.Info(info);
    }

    private void 自定义下注模板_Click(object sender, EventArgs e)
    {
        TemplateForm.GetInstance().ShowDialog();
    }

    private void 一键退出_Click(object sender, EventArgs e)
    {
        if (dummies.Where(d => d.Checked).Count() == 0)
        {
            MessageBox.Show("请选择要操作的假人");
            return;
        }

        foreach (var dummy in dummies.Where(d => d.Checked))
            if (dummy.TryLogout())
            {
                dummy.IsWorking = false;
                dummy.Status = DummyStatus.已退出;
                log.Info(dummy.LoginName + "成功退出");
            }

        SetButtonStatus(true);
        Console.WriteLine("已全部退出");
    }

    private void dgv托列表_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
    {
        e.Row.HeaderCell.Value = string.Format("{0}", e.Row.Index + 1);
    }

    private void 基础设置_Click(object sender, EventArgs e)
    {
        BasicConfigForm.GetInstance().ShowDialog();
    }

    private void 设置监控群_Click(object sender, EventArgs e)
    {
        RobotConfigForm.GetInstance().ShowDialog();
    }

    private void 一键启动_Click(object sender, EventArgs e)
    {
        foreach (Dummy dummy in dummies.Where(d => d.Checked))
        {
            dummy.IsWorking = true;
            dummy.Status = DummyStatus.运行中;
        }

        SetButtonStatus(false, true, false, true);
        log.Error("一键启动成功，下期将自动开始下注");
    }

    private void tsmi删除_Click(object sender, EventArgs e)
    {
        dgv托列表.ReadOnly = false;

        if (dgv托列表.CurrentRow == null)
            return;

        // 确认用户是否真的想要删除选中的行
        if (MessageBox.Show("确认删除选中的行么?", "确认删除", MessageBoxButtons.YesNo) == DialogResult.No)
            return;

        // 删除选中的行
        dgv托列表.Rows.Remove(dgv托列表.CurrentRow);
    }

    private void 一键暂停_Click(object sender, EventArgs e)
    {
        foreach (Dummy dummy in dummies.Where(d => d.Checked))
            if (dummy.IsWorking)
            {
                dummy.IsWorking = false;
                dummy.Status = DummyStatus.已暂停;
                一键暂停.Text = "恢复运行";
                log.Info("一键暂停成功，下期将停止自动下注");
            }
            else
            {
                dummy.IsWorking = true;
                dummy.Status = DummyStatus.运行中;
                一键暂停.Text = "一键暂停";
                log.Info("恢复运行成功，下期将开始自动下注");
            }
    }

    private void btn添加帐号_Click(object sender, EventArgs e)
    {
        if (VerifyUtil.TryGetEmptyTextBox(out TextBox empty, txt帐单名, txt帐号, txt密码))
        {
            MessageBox.Show(empty.Name.Substring(3) + "不能为空");
            return;
        }

        if (dummies.FirstOrDefault(u => u.LoginName == txt帐号.Text) != null)
        {
            MessageBox.Show(txt帐号.Text + "已存在");
            return;
        }

        var dummy = new Dummy(txt帐单名.Text, txt帐号.Text, txt密码.Text, "");
        dummy.logAction += Dummy_logAction;
        dummies.Add(dummy);
    }

    private void dgv托列表_DataError(object sender, DataGridViewDataErrorEventArgs e)
    {
        try
        {
            // 详细记录错误信息到系统日志
            string errorDetails = $"DataGridView数据错误详情:";
            errorDetails += $"\n  - 控件: dgv托列表";
            errorDetails += $"\n  - 行索引: {e.RowIndex}";
            errorDetails += $"\n  - 列索引: {e.ColumnIndex}";
            errorDetails += $"\n  - 错误上下文: {e.Context}";
            
            if (e.Exception != null)
            {
                errorDetails += $"\n  - 异常类型: {e.Exception.GetType().Name}";
                errorDetails += $"\n  - 异常消息: {e.Exception.Message}";
                errorDetails += $"\n  - 堆栈跟踪: {e.Exception.StackTrace}";
            }

            // 尝试获取出错的单元格值
            try
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && 
                    e.RowIndex < dgv托列表.Rows.Count && 
                    e.ColumnIndex < dgv托列表.Columns.Count)
                {
                    var cellValue = dgv托列表[e.ColumnIndex, e.RowIndex].Value;
                    var columnName = dgv托列表.Columns[e.ColumnIndex].Name;
                    errorDetails += $"\n  - 列名: {columnName}";
                    errorDetails += $"\n  - 单元格值: {cellValue ?? "null"}";
                    errorDetails += $"\n  - 值类型: {cellValue?.GetType().Name ?? "null"}";
                }
            }
            catch (Exception cellEx)
            {
                errorDetails += $"\n  - 获取单元格信息失败: {cellEx.Message}";
            }

            // 记录到系统日志
            LogUtil.Log($"[ERROR] {errorDetails}");

            // 忽略错误并设置默认值
            e.ThrowException = false; // 阻止异常抛出
            
            // 设置单元格的默认值
            try
            {
                DataGridViewCell cell = dgv托列表.CurrentCell;
                if (cell != null)
                {
                    LogUtil.Log($"[WARNING] 设置默认值到出错单元格: 行{e.RowIndex}, 列{e.ColumnIndex}");
                    cell.Value = "错误！！！"; // 或者其他默认值
                }
            }
            catch (Exception setEx)
            {
                LogUtil.Log($"[ERROR] 设置默认值失败: {setEx.Message}");
            }
        }
        catch (Exception logEx)
        {
            // 如果日志记录本身出错，至少记录一个简单的错误
            try
            {
                LogUtil.Log($"[CRITICAL] dgv托列表_DataError事件处理器异常: {logEx.Message}");
            }
            catch
            {
                // 最后的保险，如果连简单日志都失败了
            }
            // 确保仍然阻止异常抛出
            e.ThrowException = false;
        }
    }

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        Application.Exit();
        Environment.Exit(0);
    }

    private void dgv托列表_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
        if (e.ColumnIndex == 0)
        {
            dummies[e.RowIndex].Checked = !dummies[e.RowIndex].Checked;
            UpdateButtonStatus();
        }
    }

    private void UpdateButtonStatus()
    {
        var checkedDummies = dummies.Where(d => d.Checked);
        if (checkedDummies.Count() == 1 || checkedDummies.GroupBy(d => d.Status).Count() == 1)
        {
            DummyStatus status = checkedDummies.First().Status;
            switch (status)
            {
                case DummyStatus.未登录:
                case DummyStatus.已退出:
                case DummyStatus.不是群成员:
                case DummyStatus.登录失败:
                case DummyStatus.连接失败:
                    SetButtonStatus(true);
                    break;
                case DummyStatus.登录成功:
                    SetButtonStatus(false, true, true);
                    break;
                case DummyStatus.运行中:
                    SetButtonStatus(false, true, false, true);
                    break;
                case DummyStatus.已暂停:
                    SetButtonStatus(false, true, false, true);
                    break;
                default:
                    SetButtonStatus();
                    break;
            }
        }
        else
        {
            SetButtonStatus();
        }
    }

    private void SetButtonStatus(bool 允许登录 = false, bool 允许退出 = false, bool 允许启动 = false, bool 允许暂停 = false)
    {
        this.Invoke(() =>
        {
            一键登录.Enabled = 允许登录;
            一键退出.Enabled = 允许退出;
            一键启动.Enabled = 允许启动;
            一键暂停.Enabled = 允许暂停;
        });
    }

    private void dgv托列表_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            if (e.RowIndex != -1)
            {
                dgv托列表.ClearSelection(); // 清除所有现有选择
                dgv托列表.Rows[e.RowIndex].Selected = true; // 选中当前行
                dgv托列表.CurrentCell = dgv托列表.Rows[e.RowIndex].Cells[e.ColumnIndex];
            }

            if (e.ColumnIndex == 0)
                cms选择右键.Show(MousePosition);
            else
                cms托列表右键.Show(MousePosition);
        }
    }

    private void 全部选中ToolStripMenuItem_Click(object sender, EventArgs e)
    {
        foreach (var d in dummies) d.Checked = true;
        UpdateButtonStatus();
    }

    private void 全部取消ToolStripMenuItem_Click(object sender, EventArgs e)
    {
        foreach (var d in dummies) d.Checked = false;
        UpdateButtonStatus();
    }

    private void 反向选择ToolStripMenuItem_Click(object sender, EventArgs e)
    {
        foreach (var d in dummies) d.Checked = !d.Checked;
        UpdateButtonStatus();
    }

    private void tsmi发送消息_Click(object sender, EventArgs e)
    {
        if (dgv托列表.CurrentRow == null) return;
        string loginName = (string)dgv托列表.CurrentRow.Cells["帐号"].Value;
        Dummy selectDummy = dummies.Where(d => d.LoginName == loginName).First();
        if (selectDummy.Status != DummyStatus.登录成功 && selectDummy.Status != DummyStatus.运行中 &&
            selectDummy.Status != DummyStatus.已暂停)
        {
            MessageBox.Show("请先登录【" + selectDummy.BillName + "】");
            return;
        }

        InputBoxValidation validation = delegate(string val)
        {
            if (val == "")
                return "消息内容不能为空";
            return "";
        };

        string value = "";
        if (InputBox.Show("以【" + selectDummy.BillName + "】帐号发送消息", "请输入要发送的消息内容", ref value, validation) ==
            DialogResult.OK) selectDummy.SendMessage(value);
    }

    private void btnTest_Click(object sender, EventArgs e)
    {
        // 获取当前进程的ID
        int processId = Process.GetCurrentProcess().Id;
        Process process = Process.GetProcessById(processId);

        // 显示线程总数
        log.Info("当前进程的线程总数: " + process.Threads.Count);
    }
}