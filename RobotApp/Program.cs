using DocumentFormat.OpenXml.Vml.Spreadsheet;
using ImLibrary.IM;
using ImLibrary.Model;
using RobotApp.IM;
using RobotApp.MiniDialog;
using RobotApp.Util;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Windows.Forms;

namespace RobotApp
{
    [SupportedOSPlatform("windows")]
    internal static class Program
    {       
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            admin();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            LoginForm loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK) // 假设登录时返回OK
            {
                UserConfig uc = CentralApi.GetUserConfig("game.bindInfo");
                if (uc == null)
                {
                    SelectGroupForm sgf = new SelectGroupForm();
                    if (sgf.ShowDialog() != DialogResult.OK)
                    {
                        Application.Exit();
                    }
                    else
                    {
                        uc = CentralApi.GetUserConfig("game.bindInfo");
                    }
                }
                if (uc == null)
                {
                    MessageBox.Show("获取绑定信息失败");
                    Application.Exit();
                    Environment.Exit(0);
                }
                else
                {
                    try
                    {
                        RobotClient.Robot.PraseBindJsonInfo(uc.configValue);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        Application.Exit();
                        Environment.Exit(0);
                    }                    

                    DBHelperSQLite.InitDataBase();
                    Application.Run(new MainForm());
                }                    
            }
        }

        public static void admin()
        {
            if (WindowsIdentity.GetCurrent().Owner == WindowsIdentity.GetCurrent().User) // Check for Admin privileges
            {
                try
                {
                    ProcessStartInfo info = new ProcessStartInfo(Application.ExecutablePath);
                    info.UseShellExecute = true;
                    info.Verb = "runas"; // invoke UAC prompt
                    Process.Start(info);
                }
                catch (Win32Exception ex)
                {
                    if (ex.NativeErrorCode == 1223)
                        MessageBox.Show("该程序需要管理员权限，您必须选择“是”");
                    else
                        MessageBox.Show("程序发生错误，错误信息是：" + ex.Message);
                }
                Application.Exit();
                Environment.Exit(0);
            }
        }
    }
}
