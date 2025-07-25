using ImLibrary.IM;
using ImLibrary.Model;
using RobotApp.Dao;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace RobotApp.Util
{
    [SupportedOSPlatform("windows")]
    internal class ReadWriteINI
    {
        private static List<UserConfig> ucList;
        public static List<UserConfig> postDataList;

        public static void ReloadConfig()
        {
            ucList = CentralApi.GetUserConfigsByPrefix("");
        }
        /// <summary>
        /// 读config.ini
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static string ConfigFile_GetVal(string Key)
        {
            string Section = Key.Split('_')[0];
            return ConfigFile_GetVal(Section, Key);            
        }

        /// <summary>
        /// 读config.ini
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static string ConfigFile_GetVal(string Section, string Key)
        {
            if(ucList == null)
            {
                ReloadConfig();
            }
            UserConfig uc = ucList.FirstOrDefault(uc => uc.configKey == Key);
            return uc?.configValue;
        }


        /// <summary>
        /// 写config.ini
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static bool ConfigFile_SetVal(string Key, string Value)
        {
            string Section = Key.Split('_')[0];
            string oldValue = ConfigFile_GetVal(Key);
            if (oldValue != Value)
            {
                OplogDao.Add("配置【" + Key + "】值 [" + oldValue + "] 更改为 [" + Value + "]");
            }
            return ConfigFile_SetVal(Section, Key, Value);
        }

        /// <summary>
        /// 写config.ini
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static bool ConfigFile_SetVal(string Section, string Key, string Value)
        {
            UserConfig uc = new UserConfig();
            uc.configName = Key;
            uc.configKey = Key;
            uc.configValue = Value;
            if (postDataList != null)
            {
                postDataList.Add(uc);
                return true;
            }
            else
            {
                return CentralApi.SaveConfig(uc);
            }
        }

        /// <summary>
        /// 批量保存用户配置
        /// </summary>
        /// <returns></returns>
        public static bool SaveConfigList()
        {           
            CentralApi.SaveConfig(postDataList);
            postDataList = null;
            return true;
        }
    }
}
