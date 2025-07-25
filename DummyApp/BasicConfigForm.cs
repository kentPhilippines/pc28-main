using CommonLibrary;
using DummyApp.Model;
using ImLibrary.IM;
using ImLibrary.Model;
using System.Runtime.Versioning;
using System.Windows.Forms;
using System;
namespace DummyApp;

[SupportedOSPlatform("windows")]
public partial class BasicConfigForm : Form
{
    private static BasicConfigForm instance;

    private BasicConfigForm()
    {
        InitializeComponent();
    }

    public static BasicConfigForm GetInstance()
    {
        if (instance == null || instance.IsDisposed) instance = new BasicConfigForm();
        return instance;
    }

    private void btn保存设置_Click(object sender, EventArgs e)
    {
        var bc = new BasicConfig();
        bc.开始运行标识 = txt开始运行标识.Text;
        bc.停止运行标识 = txt停止运行标识.Text;
        bc.开盘标识 = txt开盘标识.Text;
        bc.封盘标识 = txt封盘标识.Text;
        bc.帐单头标识 = txt帐单头标识.Text;
        bc.帐单尾标识 = txt帐单尾标识.Text;
        bc.帐单识别 = txt帐单识别.Text;

        var uc = new UserConfig();
        uc.configName = "基础设置";
        uc.configKey = "game.config.基础设置";
        uc.configValue = bc.ToJson();
        CentralApi.SaveConfig(uc);

        MessageBox.Show("保存成功");
    }

    private void BasicConfigForm_Load(object sender, EventArgs e)
    {
        var uc = CentralApi.GetUserConfig("game.config.基础设置");
        if (uc != null)
        {
            var bc = uc.configValue.ToEntity<BasicConfig>();
            txt开始运行标识.Text = bc.开始运行标识;
            txt停止运行标识.Text = bc.停止运行标识;
            txt开盘标识.Text = bc.开盘标识;
            txt封盘标识.Text = bc.封盘标识;
            txt帐单头标识.Text = bc.帐单头标识;
            txt帐单尾标识.Text = bc.帐单尾标识;
            txt帐单识别.Text = bc.帐单识别;
        }
    }
}