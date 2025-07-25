using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CommonLibrary
{
    public class IniReadWrite
    {
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
        /// <param name="ini_filename">ini配置文件的路径加ini文件名</param>
        /// <param name="Section">节点名称，[]内的段落名</param>
        /// <param name="Key">键</param>
        /// <param name="Value">值</param>
        public static bool SetVal(string ini_filename, string Section, string Key, string Value)
        {
            if (File.Exists(ini_filename))
            {
                if (Value.Contains("\r\n"))
                {
                    Value = Value.Replace("\r\n", "><br><");
                }
                // 调用winapi函数将Key=Value写入Section节点下
                long len = WritePrivateProfileString(Section, Key, Value, ini_filename);
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
        /// <param name="ini_filename">ini配置文件的路径加ini文件名</param>
        /// <param name="Section">节点名称，[]内的段落名</param>
        /// <param name="Key">键</param>
        /// <returns></returns>
        public static string GetVal(string ini_filename, string Section, string Key)
        {
            if (File.Exists(ini_filename))
            {
                // 声明接收的数据
                StringBuilder builder = new StringBuilder(1024);
                // 调用winapi函数读取Section节点下Key的值
                int len = GetPrivateProfileString(Section, Key, "", builder, 1024, ini_filename);
                if (len == 0)
                    return "";
                else                    
                    return (builder.Replace("><br><", "\r\n").ToString());
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// 读ini
        /// </summary>
        /// <param name="ini_filename">ini配置文件的路径加ini文件名</param>
        /// <param name="Section">节点名称，[]内的段落名</param>
        /// <param name="Key">键</param>
        /// <returns></returns>
        public static int GetIntVal(string ini_filename, string Section, string Key, int defaultVal=0)
        {
            string val = GetVal(ini_filename, Section, Key);
            int.TryParse(val, out defaultVal);
            return defaultVal;
        }

        /// <summary>
        /// 读ini
        /// </summary>
        /// <param name="ini_filename">ini配置文件的路径加ini文件名</param>
        /// <param name="Section">节点名称，[]内的段落名</param>
        /// <param name="Key">键</param>
        /// <returns></returns>
        public static bool GetBoolVal(string ini_filename, string Section, string Key, bool defaultVal = false)
        {
            string val = GetVal(ini_filename, Section, Key);
            bool.TryParse(val, out defaultVal);
            return defaultVal;
        }
    }
}
