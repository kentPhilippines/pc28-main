using System;
using System.IO;
using System.Text;

namespace CommonLibrary
{
    public class LogUtil
    {
        private static object _lock = new object();

        public static void Log(string message)
        {
            lock (_lock)
            {
                string dir = Path.Combine(IMConstant.UserDataPath, "Log");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                string file = Path.Combine(dir, string.Format("{0}.log", DateTime.Now.Date.ToString("yyyy-MM-dd")));
                using (StreamWriter sw = new StreamWriter(file, true))
                {
                    sw.WriteLine(string.Format("{0:yyyy-MM-dd HH:mm:ss}: {1}", DateTime.Now, message));
                }
            }
        }

        public static string LogEx(Exception ex)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(ex.Message);
            if (ex.InnerException != null)
            {
                builder.Append(" innerExeption").Append(":").AppendLine(ex.InnerException.Message);
            }
            builder.Append(" "+ex.StackTrace);
            Log(builder.ToString());
            return builder.ToString();
        }
    }
}
