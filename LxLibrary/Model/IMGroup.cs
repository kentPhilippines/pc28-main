using CommonLibrary;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImLibrary.Model
{
    public class IMGroup
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupCornet { get; set; }
        public string GroupType { get; set; }
        public string GroupNameContainsType
        {
            get {
                return GroupName + (GroupType == "2" ? "【工作群】" : "【普通群】");
            }            
        }

        public List<IMUser> GroupMembers { get; set; } = new List<IMUser>();

        public void TryGetMembers(string userId)
        {
            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("groupID", GroupId);
            postData.Add("count", 10000);
            postData.Add("offset", 0);
            postData.Add("operationID", Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
            JObject jo = HttpHelper.GetInstance(userId).HttpPost<JObject>(IMConstant.API_URL + "/group/get_group_all_member_list", postData.ToJson());
            if ((int)jo["errCode"] == 0)
            {
                foreach(var item in jo["data"])
                {
                    IMUser groupUser = new IMUser();
                    groupUser.UserId = (string)item["userID"];
                    groupUser.NickName = (string)item["nickname"];
                    groupUser.FaceUrl = (string)item["faceURL"];
                    GroupMembers.Add(groupUser);
                }
            }            
        }

        /// <summary>
        /// 禁言
        /// </summary>
        /// <returns></returns>
        public int AddGroupSlience(string userId)
        {
            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("groupID", GroupId);
            postData.Add("mutedSeconds", 86400);
            postData.Add("operationID", Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
            postData.Add("userID", userId);
            JObject jo = HttpHelper.GetInstance(userId).HttpPost<JObject>(IMConstant.API_URL + "/group/mute_group_member", postData.ToJson());
            return (int)jo["errCode"];
        }

        /// <summary>
        /// 解除禁言
        /// </summary>
        /// <returns></returns>
        public int DeleteGroupSlience(string userId)
        {
            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("groupID", GroupId);
            postData.Add("operationID", Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
            postData.Add("userID", userId);
            JObject jo = HttpHelper.GetInstance(userId).HttpPost<JObject>(IMConstant.API_URL + "/group/cancel_mute_group_member", postData.ToJson());
            return (int)jo["errCode"];
        }

        public IMUser TryGetMember(string userId)
        {
            return GroupMembers?.First(m => m.UserId == userId);
        }
    }
}
