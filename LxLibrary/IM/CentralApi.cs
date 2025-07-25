using CommonLibrary;
using ImLibrary.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;

namespace ImLibrary.IM
{
    public partial class CentralApi
    {
        private static Dictionary<string, UserConfig> configMaps = new Dictionary<string, UserConfig>();

        public static void Login(string username, string pwssword)
        {
            try
            {
                string loginUrl = IMConstant.ROBOT_SERVER + "/login";
                Dictionary<string, object> postData = new Dictionary<string, object>();
                postData["username"] = username;
                postData["password"] = pwssword;
                
                LogUtil.Log($"尝试登录到服务器: {loginUrl}");
                JObject jo = HttpHelper.Instance.HttpPost<JObject>(loginUrl, postData.ToJson());
                
                if (jo == null)
                {
                    throw new Exception("服务器返回空响应，请检查网络连接或服务器状态");
                }
                
                if ((int)jo["code"] == 200)
                {
                    HttpHelper.Instance.Headers.TryAdd("Authorization", (string)jo["token"]);
                    LogUtil.Log("登录成功");
                    Debug.WriteLine("登录成功");
                }
                else
                {
                    string errorMsg = (string)jo["msg"] ?? "未知登录错误";
                    LogUtil.Log($"登录失败: {errorMsg}");
                    throw new Exception(errorMsg);
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("HTML内容"))
            {
                LogUtil.Log("登录失败: 服务器返回HTML页面，可能服务器未正常运行");
                throw new Exception("无法连接到服务器，请检查服务器是否正常运行或网络连接是否正常", ex);
            }
            catch (HttpRequestException ex)
            {
                LogUtil.Log($"登录网络请求失败: {ex.Message}");
                throw new Exception("网络连接失败，请检查网络设置和服务器地址是否正确", ex);
            }
            catch (TimeoutException ex)
            {
                LogUtil.Log($"登录超时: {ex.Message}");
                throw new Exception("连接服务器超时，请检查网络连接或稍后重试", ex);
            }
            catch (Exception ex)
            {
                LogUtil.Log($"登录过程发生未知错误: {ex.Message}");
                throw;
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
            try
            {
                if(configMaps.TryGetValue(configKey, out UserConfig uc))
                {
                    return uc;
                }
                
                string url = IMConstant.ROBOT_SERVER + "/game/config/key";
                Dictionary<string, string> paramters = new Dictionary<string, string>();
                paramters["configKey"] = configKey;
                
                LogUtil.Log($"获取用户配置: {configKey}");
                ResponseData<UserConfig> response = HttpHelper.Instance.HttpGet<ResponseData<UserConfig>>(url, paramters);
                
                if (response == null)
                {
                    LogUtil.Log($"获取配置失败: 服务器返回空响应，配置键: {configKey}");
                    return null;
                }
                
                if (response.code==200)
                {
                    configMaps.TryAdd(configKey, response.data);
                    LogUtil.Log($"成功获取配置: {configKey}");
                    return response.data;
                }
                if(response.code == 403)
                {
                    LogUtil.Log($"获取配置权限不足: {configKey} - {response.msg}");
                    throw new Exception(response.msg);
                }
                
                LogUtil.Log($"获取配置失败: {configKey} - 代码: {response.code}, 消息: {response.msg}");
                return null;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("HTML内容"))
            {
                LogUtil.Log($"获取配置失败: 服务器返回HTML页面，配置键: {configKey}");
                throw new Exception($"无法获取配置 '{configKey}'，服务器可能未正常运行", ex);
            }
            catch (HttpRequestException ex)
            {
                LogUtil.Log($"获取配置网络请求失败: {configKey} - {ex.Message}");
                throw new Exception($"获取配置 '{configKey}' 时网络连接失败", ex);
            }
            catch (Exception ex)
            {
                LogUtil.Log($"获取配置时发生未知错误: {configKey} - {ex.Message}");
                throw;
            }
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
