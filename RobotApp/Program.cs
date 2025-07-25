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
            // 添加全局异常处理，确保应用程序崩溃时能保存数据
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
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
        
        /// <summary>
        /// 处理UI线程异常
        /// </summary>
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            HandleUnhandledException(e.Exception, "UI线程异常");
        }
        
        /// <summary>
        /// 处理非UI线程异常
        /// </summary>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleUnhandledException(e.ExceptionObject as Exception, "应用程序域异常");
        }
        
        /// <summary>
        /// 统一处理未捕获的异常
        /// </summary>
        private static void HandleUnhandledException(Exception ex, string source)
        {
            try
            {
                // 记录异常日志
                LogUtil.LogEx(ex);
                LogUtil.Log($"应用程序发生{source}，正在尝试保存数据...");
                
                // 紧急保存数据
                EmergencyDataSave();
                
                MessageBox.Show($"程序发生{source}，数据已紧急保存。程序将关闭。\n\n错误信息：{ex?.Message}", 
                               "程序异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception saveEx)
            {
                // 即使保存失败也要记录
                try
                {
                    LogUtil.LogEx(saveEx);
                }
                catch { }
                
                MessageBox.Show($"程序异常且数据保存失败。\n\n原始错误：{ex?.Message}\n保存错误：{saveEx?.Message}", 
                               "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 强制退出
                Environment.Exit(1);
            }
        }
        
        /// <summary>
        /// 紧急数据保存
        /// </summary>
        private static void EmergencyDataSave()
        {
            try
            {
                // 强制同步WAL到主数据库
                DBHelperSQLite.FlushWalToMainDB();
                
                // 尝试备份当前数据
                DBHelperSQLite.BackUpDateBase();
                
                LogUtil.Log("紧急数据保存完成");
            }
            catch (Exception ex)
            {
                LogUtil.LogEx(ex);
                throw;
            }
        }
    }
}
