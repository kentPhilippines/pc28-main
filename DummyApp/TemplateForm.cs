using CommonLibrary;
using DummyApp.Model;
using DummyApp.Util;
using ImLibrary.IM;
using ImLibrary.Model;
using System.Runtime.Versioning;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.IO;

namespace DummyApp;

[SupportedOSPlatform("windows")]
public partial class TemplateForm : Form
{
    private static TemplateForm instance;

    public TemplateForm()
    {
        InitializeComponent();
        dgv下注内容.TopLeftHeaderCell.Value = "序号";
    }

    public static TemplateForm GetInstance()
    {
        if (instance == null || instance.IsDisposed) instance = new TemplateForm();
        return instance;
    }

    private void lst下注模板_DoubleClick(object sender, EventArgs e)
    {
        string templateName = lst下注模板.SelectedItem.ToString();
        if (MessageBox.Show("确认删除模板 " + templateName, "确认删除", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            string configKey = "game.user.template." + templateName;
            CentralApi.DeleteConfig(configKey);
            Reload下注模板();
        }
    }

    private void lst下注模板_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lst下注模板.SelectedItem == null) return;
        txt下注随机时间A.Text = "";
        txt下注随机时间B.Text = "";
        txt连续下注期数A.Text = "";
        txt连续下注期数B.Text = "";
        txt停止下注期数A.Text = "";
        txt停止下注期数B.Text = "";
        dgv下注内容.Rows.Clear();

        string templateName = lst下注模板.SelectedItem.ToString();
        grp模板设置.Text = "【" + templateName + "】模板设置";
        txt模板名称.Text = templateName;
        string configKey = "game.user.template." + templateName;
        var uc = CentralApi.GetUserConfig(configKey);
        if (uc != null)
        {
            var template = uc.configValue.ToEntity<Template>();
            txt下注随机时间A.Text = Convert.ToString(template.下注随机时间A);
            txt下注随机时间B.Text = Convert.ToString(template.下注随机时间B);
            txt连续下注期数A.Text = Convert.ToString(template.连续下注期数A);
            txt连续下注期数B.Text = Convert.ToString(template.连续下注期数B);
            txt停止下注期数A.Text = Convert.ToString(template.停止下注期数A);
            txt停止下注期数B.Text = Convert.ToString(template.停止下注期数B);
            foreach (string xznr in template.下注内容) dgv下注内容.Rows.Add(xznr);

            txt查指令.Text = template.查指令;
            txt查随机时间A.Text = Convert.ToString(template.查随机时间A);
            txt查随机时间B.Text = Convert.ToString(template.查随机时间B);

            txt回指令.Text = template.回指令;
            txt回随机时间A.Text = Convert.ToString(template.回随机时间A);
            txt回随机时间B.Text = Convert.ToString(template.回随机时间B);

            txt积分大于.Text = Convert.ToString(template.积分大于);
            txt回随机积分A.Text = Convert.ToString(template.回随机积分A);
            txt回随机积分B.Text = Convert.ToString(template.回随机积分B);

            txt积分小于.Text = Convert.ToString(template.积分小于);
            txt查随机积分A.Text = Convert.ToString(template.查随机积分A);
            txt查随机积分B.Text = Convert.ToString(template.查随机积分B);

            txt假聊随机期数A.Text = Convert.ToString(template.假聊随机期数A);
            txt假聊随机期数B.Text = Convert.ToString(template.假聊随机期数B);

            txt假聊随机时间A.Text = Convert.ToString(template.假聊随机时间A);
            txt假聊随机时间B.Text = Convert.ToString(template.假聊随机时间B);

            chk开启假聊.Checked = template.开启假聊;
            chk假聊封盘后发送.Checked = template.假聊封盘后发送;

            txt假聊内容.Text = "";
            foreach (string msg in template.假聊内容) txt假聊内容.Text += msg;
        }
    }

    private void btn新增下注_Click(object sender, EventArgs e)
    {
        dgv下注内容.Rows.Add(txt新增下注.Text);
    }

    private void btn保存下注模板设置_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(txt模板名称.Text))
        {
            MessageBox.Show("模板名称不能为空！");
            return;
        }

        var template = new Template();
        template.模板名称 = txt模板名称.Text;
        template.下注随机时间A = Convert.ToInt32(txt下注随机时间A.Text);
        template.下注随机时间B = Convert.ToInt32(txt下注随机时间B.Text);
        template.连续下注期数A = Convert.ToInt32(txt连续下注期数A.Text);
        template.连续下注期数B = Convert.ToInt32(txt连续下注期数B.Text);
        template.停止下注期数A = Convert.ToInt32(txt停止下注期数A.Text);
        template.停止下注期数B = Convert.ToInt32(txt停止下注期数B.Text);
        foreach (DataGridViewRow dgvr in dgv下注内容.Rows)
        {
            var xznr = dgvr.Cells["下注内容"].Value;
            if (xznr != null) template.下注内容.Add(xznr.ToString());
        }

        template.查指令 = txt查指令.Text;
        template.查随机时间A = Convert.ToInt32(txt查随机时间A.Text);
        template.查随机时间B = Convert.ToInt32(txt查随机时间B.Text);

        template.回指令 = txt回指令.Text;
        template.回随机时间A = Convert.ToInt32(txt回随机时间A.Text);
        template.回随机时间B = Convert.ToInt32(txt回随机时间B.Text);

        template.积分大于 = Convert.ToInt32(txt积分大于.Text);
        template.回随机积分A = Convert.ToInt32(txt回随机积分A.Text);
        template.回随机积分B = Convert.ToInt32(txt回随机积分B.Text);

        template.积分小于 = Convert.ToInt32(txt积分小于.Text);
        template.查随机积分A = Convert.ToInt32(txt查随机积分A.Text);
        template.查随机积分B = Convert.ToInt32(txt查随机积分B.Text);

        template.假聊随机期数A = Convert.ToInt32(txt假聊随机期数A.Text);
        template.假聊随机期数B = Convert.ToInt32(txt假聊随机期数B.Text);

        template.假聊随机时间A = Convert.ToInt32(txt假聊随机时间A.Text);
        template.假聊随机时间B = Convert.ToInt32(txt假聊随机时间B.Text);

        template.开启假聊 = chk开启假聊.Checked;
        template.假聊封盘后发送 = chk假聊封盘后发送.Checked;

        foreach (string msg in txt假聊内容.Text.Split("<br>")) template.假聊内容.Add(msg);

        var userConfig = new UserConfig();
        userConfig.configKey = "game.user.template." + template.模板名称;
        userConfig.configValue = template.ToJson();
        userConfig.configName = template.模板名称;
        CentralApi.SaveConfig(userConfig);
        Reload下注模板();
        MessageBox.Show("保存成功");
    }

    private void dgv下注内容_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
    {
        e.Row.HeaderCell.Value = string.Format("{0}", e.Row.Index + 1);
    }

    private void btn删除选中下注_Click(object sender, EventArgs e)
    {
        // 确保DataGridView是可以编辑的
        dgv下注内容.ReadOnly = false;

        // 如果没有选中任何行，则退出方法
        if (dgv下注内容.CurrentRow == null)
            return;

        // 确认用户是否真的想要删除选中的行
        if (MessageBox.Show("确认删除选中的行么?", "确认删除", MessageBoxButtons.YesNo) == DialogResult.No)
            return;

        // 删除选中的行
        dgv下注内容.Rows.Remove(dgv下注内容.CurrentRow);
    }

    private void btn删除全部下注_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show("确认删除所有行么?", "确认删除", MessageBoxButtons.YesNo) == DialogResult.No)
            return;
        dgv下注内容.Rows.Clear();
    }

    private void TemplateForm_Load(object sender, EventArgs e)
    {
        Reload下注模板();
    }

    private void Reload下注模板()
    {
        lst下注模板.Items.Clear();
        List<UserConfig> configList = CentralApi.GetUserConfigsByPrefix("game.user.template.");
        if (configList == null)
        {
            MessageBox.Show("获取模板配置时出错,请重试！");
            return;
        }

        foreach (UserConfig config in configList) lst下注模板.Items.Add(config.configName);
    }

    private void btn导入默认低档_Click(object sender, EventArgs e)
    {
        ImportTemplate("默认低档下注");
        Reload下注模板();
    }

    private void btn导入默认中档_Click(object sender, EventArgs e)
    {
        ImportTemplate("默认中档下注");
        Reload下注模板();
    }

    private void btn导入默认高档_Click(object sender, EventArgs e)
    {
        ImportTemplate("默认高档下注");
        Reload下注模板();
    }

    private void ImportTemplate(string templateName)
    {
        ofd导入.FileName = templateName;
        ofd导入.Filter = "配置文件(*.ini)|*.ini";
        DialogResult dr = ofd导入.ShowDialog();
        if (dr == DialogResult.OK)
        {
            string iniFile = ofd导入.FileName;
            TemplateUtil.ImportTemplate(iniFile, templateName);
            MessageBox.Show("保存成功");
        }
    }

    private void btn修改选中内容_Click(object sender, EventArgs e)
    {
        dgv下注内容.BeginEdit(true);
    }

    private void btn导入下注内容_Click(object sender, EventArgs e)
    {
        ofd导入.FileName = "下注内容.exls";
        ofd导入.Filter = "Excel文件(*.xlsx)|*.xlsx";
        DialogResult dr = ofd导入.ShowDialog();
        if (dr == DialogResult.OK)
        {
            string excelFile = ofd导入.FileName;
            try
            {
                ExcelUtil.ImportExcelToDataGridView(excelFile, dgv下注内容);
                MessageBox.Show("导入成功,请记得保存模板设置");
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(IOException))
                    MessageBox.Show($"{excelFile}\n文件正在使用中，请先保存并关闭该文件");
                else
                    MessageBox.Show("导入失败:" + ex.Message);
            }
        }
    }

    private void btn导出下注内容_Click(object sender, EventArgs e)
    {
        if (dgv下注内容.Rows.Count == 0)
        {
            MessageBox.Show("下注内容为空，导出失败！");
            return;
        }

        string file = ExcelUtil.ExportDataGridViewToExcel(dgv下注内容, "下注内容");
        if (file != null)
            if (MessageBox.Show("成功导出文件：\n" + file + "\n是否现在打开所在文件夹！", "确认对话框",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
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