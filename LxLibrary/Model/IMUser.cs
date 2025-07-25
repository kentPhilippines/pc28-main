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
                
                // 记录登录成功信息供测试使用
                LogUtil.Log("=== 登录成功，获取到认证信息 ===");
                LogUtil.Log($"用户名: {LoginName}");
                LogUtil.Log($"用户ID: {UserId}");
                LogUtil.Log($"Token: {Token}");
                LogUtil.Log($"用户Cornet: {UserCornet}");
                LogUtil.Log($"昵称: {NickName}");
                LogUtil.Log("===========================");
                
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
                try
                {
                    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var fullUrl = $"{IMConstant.WS_URL}?sendID={UserId}&token={Token}&platformID=5&operationID=init:{timestamp}";
                    var url = new Uri(fullUrl);
                    
                    // 详细记录WebSocket连接信息供测试使用
                    LogUtil.Log("=== WebSocket连接测试信息 ===");
                    LogUtil.Log($"完整连接URL: {fullUrl}");
                    LogUtil.Log($"基础地址: {IMConstant.WS_URL}");
                    LogUtil.Log($"用户ID: {UserId}");
                    LogUtil.Log($"Token: {Token}");
                    LogUtil.Log($"平台ID: 5");
                    LogUtil.Log($"操作ID: init:{timestamp}");
                    LogUtil.Log($"时间戳: {timestamp}");
                    LogUtil.Log("============================");
                    LogUtil.Log($"正在连接WebSocket: {url}");
                    
                    using (IMSocket = new WebsocketClient(url))
                    {
                        // 设置重连策略 - 优化为更快重连
                        IMSocket.ReconnectTimeout = TimeSpan.FromSeconds(15);  // 缩短重连间隔
                        IMSocket.ErrorReconnectTimeout = TimeSpan.FromSeconds(5);  // 缩短错误重连间隔
                        
                        IMSocket.ReconnectionHappened.Subscribe(info => {
                            IsRunning = true;
                            ReconnectionHappened?.Invoke(this, info);
                            LogUtil.Log($"WebSocket重连成功, 类型: {info.Type}");
                            Debug.WriteLine($"Reconnection happened, type: {info.Type}");
                        });

                        IMSocket.DisconnectionHappened.Subscribe(info => {
                            string logMessage = $"{LoginName} WebSocket连接断开 {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                            string disconnectReason = "未知原因";
                            
                            // 详细分析断开原因
                            if (info.Exception != null)
                            {
                                if (info.Exception is System.Net.WebSockets.WebSocketException wsEx)
                                {
                                    if (wsEx.Message.Contains("status code '706'"))
                                    {
                                        disconnectReason = "服务器配置问题或代理阻止";
                                        logMessage += " - 服务器返回错误状态码706，可能是服务器配置问题或网络代理阻止了WebSocket连接";
                                        LogUtil.Log("WebSocket连接失败: 服务器不支持WebSocket升级或存在网络代理问题");
                                    }
                                    else if (wsEx.Message.Contains("status code"))
                                    {
                                        disconnectReason = "WebSocket握手失败";
                                        logMessage += $" - WebSocket握手失败: {wsEx.Message}";
                                    }
                                    else
                                    {
                                        disconnectReason = "WebSocket协议错误";
                                        logMessage += $" - WebSocket错误: {wsEx.Message}";
                                    }
                                }
                                else if (info.Exception.Message.Contains("timeout"))
                                {
                                    disconnectReason = "连接超时";
                                    logMessage += " - 连接超时，可能是网络不稳定";
                                }
                                else if (info.Exception.Message.Contains("network"))
                                {
                                    disconnectReason = "网络问题";
                                    logMessage += " - 网络连接问题";
                                }
                                else
                                {
                                    disconnectReason = "连接异常";
                                    logMessage += $" - 连接异常: {info.Exception.Message}";
                                }
                                LogUtil.Log($"WebSocket异常详情: {info.Exception}");
                            }
                            else
                            {
                                // 分析断开类型
                                switch (info.Type)
                                {
                                    case DisconnectionType.NoMessageReceived:
                                        disconnectReason = "服务器无响应/心跳包超时";
                                        logMessage += " - 服务器无响应，可能心跳包超时";
                                        break;
                                    case DisconnectionType.Lost:
                                        disconnectReason = "网络连接丢失";
                                        logMessage += " - 网络连接丢失";
                                        break;
                                    case DisconnectionType.ByUser:
                                        disconnectReason = "用户主动断开";
                                        logMessage += " - 用户主动断开连接";
                                        break;
                                    case DisconnectionType.Error:
                                        disconnectReason = "连接错误";
                                        logMessage += " - 连接发生错误";
                                        break;
                                    default:
                                        disconnectReason = "正常断开";
                                        logMessage += " - 正常断开连接";
                                        break;
                                }
                            }
                            
                            LogUtil.Log($"断开原因: {disconnectReason}");
                            LogUtil.Log(logMessage);
                            Debug.WriteLine(logMessage + " " + info.ToJson());
                            IsRunning = false;                     
                            OnDisconnect?.Invoke(this, info);   
                        });

                    IMSocket.MessageReceived.ObserveOn(TaskPoolScheduler.Default).Subscribe(msg =>
                    {
                        string data = DecodeMessage(msg.Binary, msg.Binary.Length);
                        Debug.WriteLine(LoginName + " " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 收到 " +data);
                        
                        // 记录接收消息详细信息供测试使用
                        LogUtil.Log("=== 接收WebSocket消息 ===");
                        LogUtil.Log($"用户: {LoginName}");
                        LogUtil.Log($"时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        LogUtil.Log($"原始二进制长度: {msg.Binary.Length}");
                        LogUtil.Log($"解码后数据(Hex): {data}");
                        
                        string json = System.Text.Encoding.UTF8.GetString(Convert.FromHexString(data));
                        LogUtil.Log($"JSON消息: {json}");
                        
                        JObject jo = json.ToEntity<JObject>();                        

                        string fromUid = Convert.ToString(jo["sendId"]); 
                        string nickName = Convert.ToString(jo["senderNickname"]); 
                        string message = Convert.ToString(jo["message"]); 
                        string fp = Convert.ToString(jo["fp"]);
                        string sessionType = Convert.ToString(jo["sessionType"]);
                        string groupId = Convert.ToString(jo["groupId"]);
                        string contentType = Convert.ToString(jo["contentType"]);
                        
                        LogUtil.Log($"发送用户ID: {fromUid}");
                        LogUtil.Log($"发送昵称: {nickName}");
                        LogUtil.Log($"消息内容: {message}");
                        LogUtil.Log($"会话类型: {sessionType}");
                        LogUtil.Log($"群组ID: {groupId}");
                        LogUtil.Log($"内容类型: {contentType}");
                        LogUtil.Log("=======================");
                        
                        if (sessionType == "3" && groupId == DefauleGroup.GroupId && contentType=="101")
                        {
                            OnReceiveRawPacketComomand?.Invoke(this, fromUid, nickName, message, fp);
                        }                                                                
                    });
                        // 启动WebSocket连接
                        IMSocket.Start();

                        //心跳包
                        new Thread(HeartbeatWorker).Start();

                        exitEvent.WaitOne();
                    }
                }
                catch (System.Net.WebSockets.WebSocketException wsEx)
                {
                    string errorMessage = "WebSocket连接失败: ";
                    if (wsEx.Message.Contains("status code '706'"))
                    {
                        errorMessage += "服务器返回错误状态码706。这通常表示:\n" +
                                      "1. 服务器不支持WebSocket连接\n" +
                                      "2. 网络代理或防火墙阻止了WebSocket升级\n" +
                                      "3. 服务器配置有问题\n" +
                                      "建议检查网络设置或联系服务器管理员";
                    }
                    else if (wsEx.Message.Contains("status code"))
                    {
                        errorMessage += $"WebSocket握手失败: {wsEx.Message}";
                    }
                    else
                    {
                        errorMessage += wsEx.Message;
                    }
                    
                    LogUtil.Log(errorMessage);
                    LogUtil.LogEx(wsEx);
                    IsRunning = false;
                    throw new Exception(errorMessage, wsEx);
                }
                catch (UriFormatException uriEx)
                {
                    string errorMessage = $"WebSocket连接地址格式错误: {IMConstant.WS_URL}";
                    LogUtil.Log(errorMessage);
                    LogUtil.LogEx(uriEx);
                    IsRunning = false;
                    throw new Exception(errorMessage, uriEx);
                }
                catch (Exception ex)
                {
                    string errorMessage = $"WebSocket连接时发生未知错误: {ex.Message}";
                    LogUtil.Log(errorMessage);
                    LogUtil.LogEx(ex);
                    IsRunning = false;
                    throw new Exception(errorMessage, ex);
                }
            });

            // 等待连接建立，增加超时机制
            int waitCount = 0;
            int maxWaitCount = 60; // 最多等待30秒 (60 * 500ms)
            
            while ((IMSocket == null || !IMSocket.IsRunning) && waitCount < maxWaitCount)
            {
                Thread.Sleep(500);
                waitCount++;
            }
            
            if (waitCount >= maxWaitCount)
            {
                string timeoutMessage = "WebSocket连接超时，请检查网络连接和服务器状态";
                LogUtil.Log(timeoutMessage);
                IsRunning = false;
                throw new TimeoutException(timeoutMessage);
            }
            
            LogUtil.Log("=== WebSocket连接建立成功 ===");
            LogUtil.Log($"用户: {LoginName} ({UserId})");
            LogUtil.Log($"连接状态: 已建立");
            LogUtil.Log($"连接时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            LogUtil.Log($"服务器地址: {IMConstant.WS_URL}");
            LogUtil.Log("========================");
            return true;
        }

        private void HeartbeatWorker()
        {
            int i = 0;
            try
            {
                LogUtil.Log($"心跳包线程启动: {LoginName}");
                
                do
                {
                    // 检查WebSocket连接状态
                    if (IMSocket == null || !IMSocket.IsRunning || !IsRunning)
                    {
                        LogUtil.Log($"WebSocket连接已断开，停止心跳包: {LoginName}");
                        break;
                    }
                    
                    if (i++ == 0)
                    {
                        try
                        {
                            Debug.WriteLine("发送心跳包[" + LoginName + "]"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            string pingMessage = createPingMessage(UserId);
                            
                            // 记录心跳包详细信息供测试使用
                            LogUtil.Log("=== 心跳包发送信息 ===");
                            LogUtil.Log($"用户: {LoginName} ({UserId})");
                            LogUtil.Log($"时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                            LogUtil.Log($"心跳包消息(Hex): {pingMessage}");
                            LogUtil.Log($"心跳包长度: {pingMessage.Length}");
                            LogUtil.Log("==================");
                            
                            IMSocket.Send(Convert.FromHexString(pingMessage));
                        }
                        catch (Exception pingEx)
                        {
                            LogUtil.Log($"发送心跳包失败: {LoginName} - {pingEx.Message}");
                            // 心跳包发送失败，可能连接已断开
                            break;
                        }
                    }
                    
                    if (i == 10)
                    {
                        i = 0;
                    }
                    
                    Thread.Sleep(1000);
                    
                } while (IsRunning && IMSocket != null && IMSocket.IsRunning);
            }
            catch (Exception ex)
            {
                LogUtil.Log($"心跳包线程异常: {LoginName} - {ex.Message}");
                LogUtil.LogEx(ex);
            }
            finally
            {
                LogUtil.Log($"心跳包线程已停止: {LoginName}");
            }
        }

        /// <summary>
        /// 诊断WebSocket连接问题
        /// </summary>
        /// <returns>诊断信息</returns>
        public string DiagnoseConnectionIssues()
        {
            var diagnosis = new System.Text.StringBuilder();
            diagnosis.AppendLine("=== WebSocket连接诊断信息 ===");
            diagnosis.AppendLine($"用户: {LoginName} ({UserId})");
            diagnosis.AppendLine($"连接URL: {IMConstant.WS_URL}");
            diagnosis.AppendLine($"当前状态: {(IsRunning ? "运行中" : "已停止")}");
            diagnosis.AppendLine($"WebSocket状态: {(IMSocket?.IsRunning == true ? "已连接" : "未连接")}");
            diagnosis.AppendLine();
            
            diagnosis.AppendLine("常见问题排查:");
            diagnosis.AppendLine("1. 检查网络连接是否正常");
            diagnosis.AppendLine("2. 确认服务器地址是否正确");
            diagnosis.AppendLine("3. 检查是否有防火墙或代理阻止WebSocket连接");
            diagnosis.AppendLine("4. 确认服务器是否支持WebSocket协议");
            diagnosis.AppendLine("5. 检查Token是否有效且未过期");
            
            if (IMSocket != null && !IMSocket.IsRunning)
            {
                diagnosis.AppendLine();
                diagnosis.AppendLine("建议操作:");
                diagnosis.AppendLine("- 尝试重新登录");
                diagnosis.AppendLine("- 检查网络设置");
                diagnosis.AppendLine("- 联系系统管理员确认服务器状态");
            }
            
            return diagnosis.ToString();
        }

        /// <summary>
        /// 注销登录
        /// </summary>
        public bool TryLogout()
        {
            try
            {
                LogUtil.Log($"正在注销登录: {LoginName}");
                IsRunning = false;
                exitEvent.Set();
                IMSocket?.Stop(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "注销登录");         
                Token = null;
                LogUtil.Log($"注销登录成功: {LoginName}");
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.Log($"注销登录时出现异常: {LoginName} - {ex.Message}");
                LogUtil.LogEx(ex);
                return false;
            }
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
            
            string groupMessage = createGroupMessage(UserId, Convert.ToHexString(nickNameByte), groutMember?.FaceUrl, toId, Convert.ToHexString(utf8Byte));
            
            // 记录发送群组消息详细信息供测试使用
            LogUtil.Log("=== 发送群组消息到指定群 ===");
            LogUtil.Log($"发送用户: {LoginName} ({UserId})");
            LogUtil.Log($"目标群ID: {toId}");
            LogUtil.Log($"消息内容: {message}");
            LogUtil.Log($"发送昵称: {groutMember?.NickName}");
            LogUtil.Log($"头像URL: {groutMember?.FaceUrl}");
            LogUtil.Log($"消息UTF8字节: {Convert.ToHexString(utf8Byte)}");
            LogUtil.Log($"昵称UTF8字节: {Convert.ToHexString(nickNameByte)}");
            LogUtil.Log($"生成的消息(Hex): {groupMessage}");
            LogUtil.Log($"时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            LogUtil.Log("========================");
            
            IMSocket.Send(Convert.FromHexString(groupMessage));
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
            
            string groupMessage = createGroupMessage(UserId, Convert.ToHexString(nickNameByte), groutMember?.FaceUrl, DefauleGroup.GroupId, Convert.ToHexString(utf8Byte));
            
            // 记录发送群组消息详细信息供测试使用
            LogUtil.Log("=== 发送群组消息到默认群 ===");
            LogUtil.Log($"发送用户: {LoginName} ({UserId})");
            LogUtil.Log($"默认群ID: {DefauleGroup.GroupId}");
            LogUtil.Log($"默认群名称: {DefauleGroup.GroupName}");
            LogUtil.Log($"消息内容: {message}");
            LogUtil.Log($"发送昵称: {groutMember?.NickName}");
            LogUtil.Log($"头像URL: {groutMember?.FaceUrl}");
            LogUtil.Log($"消息UTF8字节: {Convert.ToHexString(utf8Byte)}");
            LogUtil.Log($"昵称UTF8字节: {Convert.ToHexString(nickNameByte)}");
            LogUtil.Log($"生成的消息(Hex): {groupMessage}");
            LogUtil.Log($"时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            LogUtil.Log("========================");
            
            IMSocket.Send(Convert.FromHexString(groupMessage));
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
