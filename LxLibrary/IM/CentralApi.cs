using CommonLibrary;
using ImLibrary.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ImLibrary.IM
{
    public partial class CentralApi
    {
        private static Dictionary<string, UserConfig> configMaps = new Dictionary<string, UserConfig>();

        public static void Login(string username, string pwssword)
        {
            string loginUrl = IMConstant.ROBOT_SERVER + "/login";
            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData["username"] = username;
            postData["password"] = pwssword;
            JObject jo = HttpHelper.Instance.HttpPost<JObject>(loginUrl, postData.ToJson());
            if ((int)jo["code"] == 200)
            {
                HttpHelper.Instance.Headers.TryAdd("Authorization", (string)jo["token"]);
                Debug.WriteLine("登录成功");
            }
            else
            {
                throw new Exception((string)jo["msg"]);
            }
        }

        public static bool DeleteConfig(string configKey)
        {
            string url = IMConstant.ROBOT_SERVER + "/game/config/delete";
            JObject jo = HttpHelper.Instance.HttpPost<JObject>(url, configKey);
            Debug.WriteLine(jo);
            return (int)jo["code"] == 200;
        }

        public static bool SaveConfig(UserConfig userConfig)
        {
            string url = IMConstant.ROBOT_SERVER + "/game/config";
            JObject jo = HttpHelper.Instance.HttpPost<JObject>(url, userConfig.ToJson());
            Debug.WriteLine(jo);
            if ((int)jo["code"] == 200)
            {
                configMaps.Remove(userConfig.configKey);
                return true;
            }            
            return false;
        }

        public static bool SaveConfig(List<UserConfig> userConfigList)
        {
            string url = IMConstant.ROBOT_SERVER + "/game/config/saveAll";
            JObject jo = HttpHelper.Instance.HttpPost<JObject>(url, userConfigList.ToJson());
            Debug.WriteLine(jo);
            if ((int)jo["code"] == 200)
            {
                foreach(UserConfig uc in userConfigList)
                {
                    configMaps.Remove(uc.configKey);
                }                
                return true;
            }
            return false;
        }

        public static UserConfig GetUserConfig(string configKey) {
            if(configMaps.TryGetValue(configKey, out UserConfig uc))
            {
                return uc;
            }
            string url = IMConstant.ROBOT_SERVER + "/game/config/key";
            Dictionary<string, string> paramters = new Dictionary<string, string>();
            paramters["configKey"] = configKey;
            ResponseData<UserConfig> response = HttpHelper.Instance.HttpGet<ResponseData<UserConfig>>(url, paramters);
            if (response.code==200)
            {
                configMaps.TryAdd(configKey, response.data);
                return response.data;
            }
            if(response.code == 403)
            {
                throw new Exception(response.msg);
            }
            return null;
        }

        public static List<UserConfig> GetUserConfigsByPrefix(string configKeyPrefix)
        {
            string url = IMConstant.ROBOT_SERVER + "/game/config/prefix";
            Dictionary<string, string> paramters = new Dictionary<string, string>();
            paramters["prefix"] = configKeyPrefix;
            ResponseData<List<UserConfig>> response = HttpHelper.Instance.HttpGet<ResponseData<List<UserConfig>>>(url, paramters);
            if (response!=null && response.code == 200)
            {
                return response.data;
            }
            return null;
        }
    }
}
