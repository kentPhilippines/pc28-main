using CommonLibrary;
using ImLibrary.IM;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities;
using Serilog;
using ServerApiParams;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace ImLibrary.Model
{
    public class IMUser : ViewModelBase
    {
        public virtual string LoginName { get; set; }
        public virtual string Password { get; set; }
        [JsonIgnore]
        public virtual string Token { get; set; }
        [JsonIgnore]
        public virtual string UserId { get; set; }
        [JsonIgnore]
        public virtual string UserCornet { get; set; }
        [JsonIgnore]
        public virtual string FaceUrl { get; set; }
        /// <summary>
        /// 玩家昵称
        /// </summary>
        [JsonIgnore]
        public virtual string NickName { get; set; }

        private WebsocketClient IMSocket { get; set; }
        [JsonIgnore]
        public bool IsRunning { get; private set; } = false;
        ManualResetEvent exitEvent = new ManualResetEvent(false);

        /// <summary>
        /// 接收命令事件
        /// </summary>
        public event Action<IMUser, string, string, string, string> OnReceiveRawPacketComomand;

        /// <summary>
        /// 接收连接断开事件
        /// </summary>
        public event Action<IMUser, DisconnectionInfo> OnDisconnect;

        public event Action<IMUser, ReconnectionInfo> ReconnectionHappened;

        private Thread heartbeatThread;
        // 用于暂停和继续线程
        [JsonIgnore]
        private ManualResetEvent _pauseEvent = new ManualResetEvent(true);  // 初始状态为"非暂停"（允许运行）
        // 用户加入的所有群
        [JsonIgnore]
        public ConcurrentDictionary<string, IMGroup> GroupMap = new ConcurrentDictionary<string, IMGroup>();
        [JsonIgnore]
        public IMGroup DefauleGroup { get; set; }

        private CancellationTokenSource heartbeatTokenSource = new CancellationTokenSource();

        [DllImport("Native/main.dll")]
        private static extern string DecodeMessage(byte[] msgData, int length);

        [DllImport("Native/main.dll")]
        private static extern string createPingMessage(string sendID);

        [DllImport("Native/main.dll")]
        private static extern string createGroupMessage(string sendID, string sendNickname, string sendFaceURL, string groupID, string msgContent);

        public async Task ReConnect()
        {
            Debug.WriteLine("开始重连");
            await IMSocket.Reconnect();
            Debug.WriteLine("结束重连");
        }

        public bool CreateToken(string username, string password)
        {
            LoginName = username;
            Password = password;

            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("Title", username);
            postData.Add("Secret", MD5Encryption.GetMd5Hash32(password));
            long currentMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            postData.Add("CodeTicket", currentMillis);
            postData.Add("operationID", currentMillis);
            postData.Add("platform", 5);
            JObject result = HttpHelper.Instance.HttpPost<JObject>($"{IMConstant.SERVER_URL}/api/account/login", postData.ToJson());
            Debug.WriteLine(result);
            if ((int?)result["Code"] == 200)
            {
                Token = (string)result["Data"]?["Token"];
                UserId = (string)result["Data"]?["UserName"];
                UserCornet = (string)result["Data"]?["UserId"];
                NickName = (string)result["Data"]?["UserId"];
                HttpHelper.GetInstance(UserId).Headers.AddOrUpdate("token", Token, (key, oldValue)=>Token);
                return true;
            }
            else
            {
                throw new Exception((string)result["Message"]);
            }
        }

        public bool TryLogin()
        {         
            if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(Token))
            {
                throw new Exception("请先登录！");
            }

            Task.Run(() =>
            {                
                var url = new Uri($"{IMConstant.WS_URL}?sendID={UserId}&token={Token}&platformID=5&operationID=init:{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");               
                //Debug.WriteLine(url);
                using (IMSocket = new WebsocketClient(url))
                {
                    IMSocket.ReconnectTimeout = TimeSpan.FromSeconds(30);
                    IMSocket.ReconnectionHappened.Subscribe(info => {
                        IsRunning = true;
                        ReconnectionHappened?.Invoke(this, info);
                        Debug.WriteLine($"Reconnection happened, type: {info.Type}");
                    });

                    IMSocket.DisconnectionHappened.Subscribe(info => {
                        Debug.WriteLine(LoginName + " 掉线了"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+" "+info.ToJson());
                        IsRunning = false;                     
                        OnDisconnect?.Invoke(this, info);   
                    });

                    IMSocket.MessageReceived.ObserveOn(TaskPoolScheduler.Default).Subscribe(msg =>
                    {
                        string data = DecodeMessage(msg.Binary, msg.Binary.Length);
                        Debug.WriteLine(LoginName + " " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 收到 " +data);
                        string json = System.Text.Encoding.UTF8.GetString(Convert.FromHexString(data));
                        JObject jo = json.ToEntity<JObject>();                        

                        string fromUid = Convert.ToString(jo["sendId"]); 
                        string nickName = Convert.ToString(jo["senderNickname"]); 
                        string message = Convert.ToString(jo["message"]); 
                        string fp = Convert.ToString(jo["fp"]);
                        string sessionType = Convert.ToString(jo["sessionType"]);
                        string groupId = Convert.ToString(jo["groupId"]);
                        string contentType = Convert.ToString(jo["contentType"]);
                        if (sessionType == "3" && groupId == DefauleGroup.GroupId && contentType=="101")
                        {
                            OnReceiveRawPacketComomand?.Invoke(this, fromUid, nickName, message, fp);
                        }                                                                
                    });
                    IMSocket.Start();

                    //心跳包
                    new Thread(HeartbeatWorker).Start();

                    exitEvent.WaitOne();
                }
            });

            while (IMSocket==null || !IMSocket.IsRunning)
            {
                Thread.Sleep(500);
            }
            
            return true;
        }

        private void HeartbeatWorker()
        {
            int i = 0;
            do
            {
                if (i++ == 0)
                {
                    Debug.WriteLine("发送心跳包[" + LoginName + "]"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    IMSocket.Send(Convert.FromHexString(createPingMessage(UserId)));
                }
                if (i == 10)
                {
                    i = 0;
                }
                Thread.Sleep(1000);
            } while (true);
            //Debug.WriteLine("心跳已停止[" + UserCornet + "]");
        }

        /// <summary>
        /// 注销登录
        /// </summary>
        public bool TryLogout()
        {
            //exitEvent.Set();
            IMSocket?.Stop(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "注销登录");         
            Token = null;
            return true;
        }

        /// <summary>
        /// 获取用户加入的群
        /// </summary>
        /// <returns></returns>
        public bool TryGetGroups()
        {
            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("operationID", Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
            postData.Add("fromUserID", UserId);
            JObject result = HttpHelper.GetInstance(UserId).HttpPost<JObject>(IMConstant.API_URL + "/group/get_joined_group_list", postData.ToJson());
            if (result != null)
            {
                Debug.WriteLine(result.ToString());
                if(result["errCode"].Value<int>() == 500)
                {
                    LogUtil.Log(result.ToJson());
                    return false;                  
                }
                if (result["data"].HasValues)
                {
                    foreach (JObject group in result["data"])
                    {
                        IMGroup g = new IMGroup();
                        g.GroupId = (string)group["groupID"];
                        g.GroupName = (string)group["groupName"];
                        g.GroupType = (string)group["groupType"];
                        GroupMap.TryAdd(g.GroupId, g);
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="toId">接收消息的用户ID, 如群消息则为群ID</param>
        /// <param name="message"></param>
        public string SendMessage(string toId, string message)
        {
            //if (!IMSocket.IsRunning)
            //{
            //    throw new Exception($"掉线了，消息【{message}】发送失败");
            //}
            byte[] utf8Byte = System.Text.Encoding.UTF8.GetBytes(message);           
            IMUser groutMember = DefauleGroup.TryGetMember(UserId);
            byte[] nickNameByte = System.Text.Encoding.UTF8.GetBytes(groutMember?.NickName);
            IMSocket.Send(Convert.FromHexString(createGroupMessage(UserId, Convert.ToHexString(nickNameByte), groutMember?.FaceUrl, toId, Convert.ToHexString(utf8Byte))));
            return "";
        }

        /// <summary>
        /// 发送消息到默认群
        /// </summary>
        /// <param name="message"></param>
        public string SendMessage(string message)
        {
            byte[] utf8Byte = System.Text.Encoding.UTF8.GetBytes(message);
            IMUser groutMember = DefauleGroup.TryGetMember(UserId);
            byte[] nickNameByte = System.Text.Encoding.UTF8.GetBytes(groutMember?.NickName);
            IMSocket.Send(Convert.FromHexString(createGroupMessage(UserId, Convert.ToHexString(nickNameByte), groutMember?.FaceUrl, DefauleGroup.GroupId, Convert.ToHexString(utf8Byte))));
            return "";
        }

        public String ToBindJsonInfo()
        {
            JObject jo = new JObject();
            jo.Add("LoginName", LoginName);
            jo.Add("UserId", UserId);
            jo.Add("Password", Password);
            jo.Add("DefauleGroup", DefauleGroup.GroupId);
            return jo.ToString();
        }

        public void PraseBindJsonInfo(string jsonValue)
        {
            if (DefauleGroup == null)
            {
                JObject jo = JObject.Parse(jsonValue);
                LoginName = (string)jo["LoginName"];
                Password = (string)jo["Password"];

                if (!CreateToken(LoginName, Password))
                {
                    throw new Exception("自动登录失败！");
                }
                if (!TryLogin())
                {
                    throw new Exception("自动登录聊天服务器失败！");
                }
                if (TryGetGroups())
                {
                    DefauleGroup = GroupMap[(string)jo["DefauleGroup"]];
                }
                else
                {
                    throw new Exception("自动获取群列表失败！");
                }
            }
            DefauleGroup.TryGetMembers(UserId);            
        }
    }    
}
