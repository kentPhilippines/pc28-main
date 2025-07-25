//using ImLibrary.Model;
//using CommonLibrary;
//using Newtonsoft.Json.Linq;
//using RobotApp.MiniDialog;
//using RobotApp.Model;
//using RobotApp.Util;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.Versioning;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

//namespace RobotApp.IM
//{
//    [SupportedOSPlatform("windows")]
//    internal class IMApi
//    {
//        public static IMToken GetImToken(string username, string password)
//        {            
//            Dictionary<string, object> postData = new Dictionary<string, object>();
//            postData.Add("areaCode", "+86");
//            postData.Add("phoneNumber", username);
//            postData.Add("password", MD5Encryption.GetMd5Hash32(password));
//            postData.Add("platform", 3);

//            HttpHelper.Instance.Headers.Add("Operationid", Guid.NewGuid().ToString());
//            JObject result = HttpHelper.Instance.HttpPost<JObject>("http://18.167.54.111:10008/account/login", postData.ToJson(), "application/json", 30);
//            if ((int?)result["errCode"] == 0)
//            {
//                IMToken token = new IMToken();
//                token.UserName = (string)result["data"]?["userID"];
//                token.Token = (string)result["data"]?["imToken"];
//                return token;                
//            }
//            else
//            {
//                throw new Exception((string)result["Message"]);
//            }                 
//        }        

//        public static IMToken GetWyToken(string username, string password)
//        {
//            String deviceID = AESUtil.Get16BitLowerCaseMd5(GetSystemInfo.GetCpuID()) + AESUtil.Get16BitLowerCaseMd5(GetSystemInfo.GetDiskID());

//            string url = IMConstant.SERVER_URL + "/rest_post";
//            Dictionary<string, object> newData = new Dictionary<string, object>();
//            newData.Add("android", true);
//            newData.Add("clientVersion", 5);
//            newData.Add("deviceID", deviceID);
//            newData.Add("deviceInfo", "robot");
//            newData.Add("iOS", false);
//            newData.Add("loginName", username);
//            newData.Add("loginPsw", password);
//            newData.Add("osType", "0");
//            newData.Add("vCode", "");
//            newData.Add("web", false);

//            DataFromClient client = new DataFromClient();
//            client.setProcessorId(1009).setNewData(newData.ToJson());
//            JObject jo = null;
//            try
//            {
//                jo = HttpHelper.Instance.HttpPost<JObject>(url, client.ToJson());
//                //Console.WriteLine(jo.ToString());
//            }
//            catch
//            {
//                throw new Exception("登录失败");
//            }
//            if (jo["success"].ToString() == "False")
//            {
//                throw new Exception("登录失败");
//            }
//            jo = JObject.Parse(jo["returnValue"].ToString());
//            if (jo["error"] != null)
//            {
//                throw new Exception((string)jo["error"]);
//            }

//            IMToken token = new IMToken();
//            token.UserName = (string)jo["user_uid"];
//            token.Token = (string)jo["authorization"];
//            return token;            
//        }

//        /// <summary>
//        /// 获取群成员列表
//        /// </summary>
//        /// <returns></returns>
//        public static List<GroupMember> GetGroupMembers()
//        {
//            ResponseData<Dictionary<string, GroupMember>> data = HttpHelper.Instance.HttpPost<ResponseData<Dictionary<string, GroupMember>>>(IMConstant.API_URL + "api/groupBase/groupMembers?gid=" + RobotClient.Robot.DefauleGroup.GroupId);
//            return data.data.Values.ToList();
//        }

//        /// <summary>
//        /// 禁言
//        /// </summary>
//        /// <returns></returns>
//        public static int AddGroupSlience(string userId, string nickName)
//        {
//            ResponseData<string> data = HttpHelper.Instance.HttpPost<ResponseData<string>>(IMConstant.API_URL + "api/groupSlience/addGroupSlience?clusterId=" + RobotClient.Robot.DefauleGroup.GroupId + "&userId=" + userId);
//            if (data.status == 200)
//            {
//                PushMessage pushMessage = new PushMessage();
//                pushMessage.isBanned = true;
//                pushMessage.msg = nickName + "已被禁言";
//                pushMessage.sendId = RobotClient.Robot.UserId;
//                pushMessage.senderId = RobotClient.Robot.UserId;
//                pushMessage.uuid = userId;

//                DataContent dataContent = new DataContent();
//                dataContent.airbubblesType = 0;
//                dataContent.nickName = RobotClient.Robot.NickName;
//                dataContent.cy = "2";
//                dataContent.f = RobotClient.Robot.UserId;
//                dataContent.m = AESUtil.AesEncrypt(pushMessage.ToJson(), RobotClient.Robot.DefauleGroup.GroupId);
//                dataContent.m3 = "Android";
//                dataContent.t = RobotClient.Robot.DefauleGroup.GroupId;
//                dataContent.ty = 12;
//                Protocal p = new Protocal(ProtocalType.C.FROM_CLIENT_TYPE_OF_COMMON_DATA, dataContent.ToJson(), RobotClient.Robot.UserId, "0", 44);
//                TcpSocketClient.Instance.Send(p.ToBytes());
//            }
//            return data.status;
//        }

//        /// <summary>
//        /// 解除禁言
//        /// </summary>
//        /// <returns></returns>
//        public static int DeleteGroupSlience(string userId, string nickName)
//        {
//            ResponseData<string> data = HttpHelper.Instance.HttpPost<ResponseData<string>>(IMConstant.API_URL + "api/groupSlience/deleteGroupSlienceById?clusterId=" + RobotClient.Robot.DefauleGroup.GroupId + "&userId="+userId);
//            if (data.status == 200)
//            {
//                PushMessage pushMessage = new PushMessage();
//                pushMessage.isBanned = false;
//                pushMessage.msg = nickName + "已被解禁了";
//                pushMessage.sendId = RobotClient.Robot.UserId;
//                pushMessage.senderId = RobotClient.Robot.UserId;
//                pushMessage.uuid = userId;

//                DataContent dataContent = new DataContent();
//                dataContent.airbubblesType = 0;
//                dataContent.nickName = RobotClient.Robot.NickName;
//                dataContent.cy = "2";
//                dataContent.f = RobotClient.Robot.UserId;
//                dataContent.m = AESUtil.AesEncrypt(pushMessage.ToJson(), RobotClient.Robot.DefauleGroup.GroupId);
//                dataContent.m3 = "Android";
//                dataContent.t = RobotClient.Robot.DefauleGroup.GroupId;
//                dataContent.ty = 12;
//                Protocal p = new Protocal(ProtocalType.C.FROM_CLIENT_TYPE_OF_COMMON_DATA, dataContent.ToJson(), RobotClient.Robot.UserId, "0", 44);
//                TcpSocketClient.Instance.Send(p.ToBytes());
//            }
//            return data.status;
//        }
//    }
//}
