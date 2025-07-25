using ImLibrary.IM;
using ImLibrary.Model;
using System.Runtime.Versioning;
using System.Windows.Forms;
using System;
namespace DummyApp;

[SupportedOSPlatform("windows")]
public partial class RobotConfigForm : Form
{
    private static RobotConfigForm instance;

    private RobotConfigForm()
    {
        InitializeComponent();
    }

    public static RobotConfigForm GetInstance()
    {
        if (instance == null || instance.IsDisposed) instance = new RobotConfigForm();
        return instance;
    }

    private void btn保存设置_Click(object sender, EventArgs e)
    {
        var userConfig = new UserConfig();
        userConfig.configName = "监控群ID";
        userConfig.configKey = "game.config.监控群ID";
        userConfig.configValue = txt监控群ID.Text;
        CentralApi.SaveConfig(userConfig);

        userConfig.configName = "机器人ID";
        userConfig.configKey = "game.config.机器人ID";
        userConfig.configValue = txt机器人ID.Text;
        CentralApi.SaveConfig(userConfig);

        MessageBox.Show("保存成功");
    }

    private void RobotConfigForm_Load(object sender, EventArgs e)
    {
        var uc = CentralApi.GetUserConfig("game.config.监控群ID");
        txt监控群ID.Text = uc?.configValue;

        uc = CentralApi.GetUserConfig("game.config.机器人ID");
        txt机器人ID.Text = uc?.configValue;
    }
}