using Newtonsoft.Json.Linq;
using RobotApp.Dao;
using System;
using System.Collections.Generic;
//虽然 .NET 5+ 支持 ClickOnce，但应用无权访问 System.Deployment.Application 命名空间。有关更多详细信息，请参阅 https://github.com/dotnet/deployment-tools/issues/27 和 https://github.com/dotnet/deployment-tools/issues/53。
//using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace RobotApp.Util
{
    [SupportedOSPlatform("windows")]
    internal class ReadWriteINI2
    {
        private static string AppPath
        {
            get
            {
                return AppContext.BaseDirectory;
            }
        }
/*        private static string AppPath{
            get {
                // 检查应用程序是否是通过ClickOnce部署的
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    // 获取当前程序的数据目录路径
                    return ApplicationDeployment.CurrentDeployment.DataDirectory;
                }
                else
                {
                    return AppContext.BaseDirectory;
                }
            }
        }*/
        #region API函数声明

        /// <summary>
        /// 写入ini文件
        /// </summary>
        /// <param name="section">节点名称</param>
        /// <param name="key">键</param>
        /// <param name="val">值</param>
        /// <param name="filePath">ini路径</param>
        /// <returns>0失败/其他成功</returns>
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="section">节点名称，[]内的段落名</param>
        /// <param name="key">键</param>
        /// <param name="def">值(未读取到数据时设置的默认返回值)</param>
        /// 对应API函数的def参数，它的值由用户指定，是当在配置文件中没有找到具体的Value时，就用def的值来代替。可以为空
        /// <param name="retVal">读取到的结果值</param>
        /// <param name="size">读取缓冲区大小</param>
        /// <param name="filePath">ini配置文件的路径加ini文件名</param>
        /// <returns>读取到的字节数量</returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        #endregion

        /// <summary>
        /// 写INI,添加新节点、键值，及编辑修改键对应的值
        /// </summary>
        /// <param name="in_filename">ini配置文件的路径加ini文件名</param>
        /// <param name="Section">节点名称，[]内的段落名</param>
        /// <param name="Key">键</param>
        /// <param name="Value">值</param>
        public static bool IniFile_SetVal(string in_filename, string Section, string Key, string Value)
        {
            if (File.Exists(in_filename))
            {
                if (Value.Contains("\r\n"))
                {
                    Value = Value.Replace("\r\n", "><br><");
                }
                // 调用winapi函数将Key=Value写入Section节点下
                long len = WritePrivateProfileString(Section, Key, Value, in_filename);
                if (len == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 读ini
        /// </summary>
        /// <param name="in_filename">ini配置文件的路径加ini文件名</param>
        /// <param name="Section">节点名称，[]内的段落名</param>
        /// <param name="Key">键</param>
        /// <returns></returns>
        public static string IniFile_GetVal(string in_filename, string Section, string Key)
        {
            if (File.Exists(in_filename))
            {
                // 声明接收的数据
                StringBuilder builder = new StringBuilder(1024);
                // 调用winapi函数读取Section节点下Key的值
                int len = GetPrivateProfileString(Section, Key, "", builder, 1024, in_filename);
                if (len == 0)
                    return "";
                else                    
                    return (builder.Replace("><br><", "\r\n").ToString());
            }
            else
                return string.Empty;
        }


        /// <summary>
        /// 写config.ini
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static bool ConfigFile_SetVal(string Section, string Key, string Value)
        {
            string fileName = AppPath + @"Configs\config.ini";
            return IniFile_SetVal(fileName, Section, Key, Value);
        }

        /// <summary>
        /// 读config.ini
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static string ConfigFile_GetVal(string Section, string Key)
        {
            string fileName = AppPath + @"Configs\config.ini";
            return IniFile_GetVal(fileName, Section, Key);
        }

        /// <summary>
        /// 写config.ini
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static bool ConfigFile_SetVal(string Key, string Value)
        {
            string fileName = AppPath + @"Configs\config.ini";
            string Section = Key.Split('_')[0];
            string oldValue = ConfigFile_GetVal(Key);
            if(oldValue != Value)
            {
                OplogDao.Add("配置【"+ Key + "】值 ["+oldValue+"] 更改为 ["+Value+"]");
            }
            return IniFile_SetVal(fileName, Section, Key, Value);
        }

        /// <summary>
        /// 读config.ini
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static string ConfigFile_GetVal(string Key)
        {
            string fileName = AppPath + @"Configs\config.ini";
            string Section = Key.Split('_')[0];
            return IniFile_GetVal(fileName, Section, Key);
        }

        /// <summary>
        /// 读config.ini
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static int ConfigFile_GetIntVal(string Key)
        {
            string fileName = AppPath + @"Configs\config.ini";
            string Section = Key.Split('_')[0];
            string val = IniFile_GetVal(fileName, Section, Key);
            if(int.TryParse(val, out int iv)) 
            {
                return iv;
            }
            return 0;
        }
    }
}
