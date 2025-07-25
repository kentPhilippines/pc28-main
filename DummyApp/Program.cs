using ImLibrary.IM;
using ImLibrary.Model;
using System.Runtime.Versioning;
using System.Windows.Forms;
using System;
using Newtonsoft.Json.Linq;

namespace DummyApp;

[SupportedOSPlatform("windows")]
internal class Program
{
    /// <summary>
    ///     应用程序的主入口点。
    /// </summary>
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        var loginForm = new LoginForm();
        if (loginForm.ShowDialog() == DialogResult.OK) // 假设登录时返回OK
        {
            var uc = CentralApi.GetUserConfig("game.bindInfo");
            if (uc == null)
            {
                MessageBox.Show("请先在机器人端绑定群");
                Application.Exit();
            }
            else
            {
                JObject jo = JObject.Parse(uc.configValue);
                string userId = (string)jo["UserId"];
                string defaultGroup = (string)jo["DefauleGroup"];

                var robot = new IMUser();
                robot.UserId = userId;
                var imGroup = new IMGroup();
                imGroup.GroupId = defaultGroup;
                robot.DefauleGroup = imGroup;
                var dummyMain = MainForm.GetInstance();
                dummyMain.SetGroupInfo(robot);

                // 创建并运行主窗体
                Application.Run(dummyMain);
            }
        }
    }
}