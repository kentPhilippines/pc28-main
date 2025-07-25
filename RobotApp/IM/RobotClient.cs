using CommonLibrary;
using ImLibrary.Model;
using RobotApp.Dao;
using RobotApp.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Versioning;

namespace RobotApp.IM
{
    internal enum LotterCode
    {
        加拿大 = 1,
        宾果 = 2,
    }
    [SupportedOSPlatform("windows")]
    internal partial class RobotClient
    {
        //消息队列
        public static Queue<Model.Message> MsgQueue = new Queue<Model.Message>();
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        
        //当前开彩票类型 1：加拿大28 2:宾果28
        public static LotterCode CurrentLotterCode = LotterCode.加拿大;
        //上期开奖结果
        public static Result PrevResult { 
            get 
            { 
                if(ResultList.Count > 1)
                {
                    return ResultList[1];
                }
                return null;
            } 
        }
        //当期开奖
        public static Result CurrentResult
        {
            get
            {
                if (ResultList.Count > 0)
                {
                    return ResultList[0];
                }
                return null;
            }
        }

        public static IMUser Robot { get; set; } = new IMUser();
        //是否启动机器人
        public static bool ROBOT_RUNNING = false;        

        /// <summary>
        /// 玩家列表
        /// </summary>
        internal static BindingList<User> UserList { get; set; } = new BindingList<User>();
        /// <summary>
        /// 开奖结果
        /// </summary>
        internal static BindingList<Result> ResultList { get; set; } = new BindingList<Result>();
        /// <summary>
        /// 上下分
        /// </summary>
        internal static BindingList<Score> ScoreList { get; set; } = new BindingList<Score>();

        internal static User GetUser(string userId)
        {
            User user = UserList.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                user = UserDao.GetUser(userId);
            }            
            return user;
        }

        internal static Score GetScore(int scoreId)
        {
            return ScoreList.FirstOrDefault(s => s.Id == scoreId);
        }

        internal static List<Score> GetScoreByUserId(string userId)
        {
            return ScoreList.Where(s => s.Code == userId).ToList();
        }

        internal static Result GetResultByIssue(int issue)
        {
            return ResultList.FirstOrDefault(r => r.Issue == issue);
        }

        private RobotClient() { }

        /// <summary>
        /// 发送配置的消息
        /// </summary>
        /// <param name="message"></param>
        public static void SendConfigMessage(string message, Result result = null)
        {
            List<User> users = RobotClient.UserList.ToList();
            String wjlb = "";
            double lbzf = 0.0;
            int showUserCount = 0;
            string ccnrhd = "";
            if (Config.GetInstance().选择框_下注核对排序) 
            { 
                users.Sort((a, b) => a.Blzf.CompareTo(b.Blzf));
            }
            foreach (User user in users)
            {
                if (user.RecordList.Count > 0)
                {
                    ccnrhd += user.NickName + " "+ (user.Jifen + user.Blzf) + "[" + user.Ccnr + "]\r\n";
                }
                if (user.IsDummy)
                {
                    if (Config.GetInstance().选择框_假人0分不显示)
                    {
                        if (user.Jifen <= Config.GetInstance().编辑框_假人n分不显示)
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    if (Config.GetInstance().选择框_真人0分不显示)
                    {
                        if (user.Jifen <= Config.GetInstance().编辑框_真人n分不显示)
                        {
                            continue;
                        }
                    }
                }
                showUserCount++;
                lbzf += user.Jifen;
                wjlb += user.NickName + " " + user.Jifen.ToString().PadLeft(Config.GetInstance().编辑框_积分位数, '0');
                if (showUserCount % Config.GetInstance().编辑框_每行人数 == 0)
                {
                    wjlb += "\r\n";
                }
                else
                {
                    wjlb += "\t";
                }
            }

            if (message.Contains("{期号}"))
            {
                if (result == null)
                {
                    if (RobotClient.CurrentResult.Status < ResultStatus.已开奖)
                    {
                        result = RobotClient.CurrentResult;
                    }
                    else
                    {
                        result = RobotClient.PrevResult;
                    }
                }
                message = message.Replace("{期号}", result?.Issue.ToString());              
            }
            if (message.Contains("{玩家列表}"))
            {
                if (wjlb.Trim() == "")
                {
                    wjlb = "无！";
                }
                message = message.Replace("{玩家列表}", wjlb);
            }
            if (message.Contains("{列表总分}"))
            {
                message = message.Replace("{列表总分}", lbzf.ToString());
            }
            if (message.Contains("{在线人数}"))
            {
                message = message.Replace("{在线人数}", users.Count.ToString());
            }
            if (message.Contains("{内容核对}"))
            {
                if (ccnrhd.Trim() == "")
                {
                    ccnrhd = "本期无人下注！";
                }
                message = message.Replace("{内容核对}", ccnrhd.Trim());
            }
            if (message.Contains("{开奖结果}"))
            {
                    message = message.Replace("{开奖结果}", CurrentResult.ThreeNum + "=" + CurrentResult.Sum + " " + CurrentResult.ResultDesc);                  
            }
            if (message.Contains("{开奖时间}"))
            {
                message = message.Replace("{开奖时间}", RobotClient.CurrentResult.OpenTime.ToString(RobotClient.DateTimeFormat));
            }
            if (message.Contains("{开奖网址}"))
            {
                message = message.Replace("{开奖网址}", "https://www.pc666666.com");
            }
            if (message.Contains("{历史数据}"))
            {
                message = message.Replace("{历史数据}", ResultDao.GetNResult(RobotClient.CurrentLotterCode, Config.GetInstance().编辑框_历史号码个数));
            }
            if (message.Contains("{开奖倒计时}"))
            {
                message = message.Replace("{开奖倒计时}", (RobotClient.CurrentResult.OpenTime - DateTime.Now).ToString("mm\\:ss"));
            }
            SendMessage(message);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        public static void SendMessage(string message)
        {
            string fp =  Robot.SendMessage(Robot.DefauleGroup.GroupId, message);

            //保存消息
            Model.Message msg = new Model.Message();
            msg.Code = RobotClient.Robot.UserId;
            msg.NickName = RobotClient.Robot.NickName;
            msg.Msg = message;
            msg.MsgTime = DateTime.Now;
            msg.Fp = fp;
            MessageDao.AddMessage(msg);
            RobotClient.MsgQueue.Enqueue(msg);
        }

        /// <summary>
        /// 获取最新开奖结果
        /// </summary>
        /// <param name="callBack"></param>
        /// <exception cref="Exception"></exception>
        public static Result GetLatestResult(LotterCode lotterCode, int tryCount=99)
        {            
            String url = IMConstant.ROBOT_SERVER + "/game/issue/getLast/" + (int)lotterCode;
            Result result = null;
            try
            {
                result = HttpHelper.Instance.HttpGet<Result>(url);
            }
            catch (Exception e)
            {
                LogUtil.Log($"第{tryCount}次获取开奖出错：" + e.Message);                
            }
            
            if (result != null)
            {
                result.Type = lotterCode;
                result.Status = ResultStatus.已开奖;
                LogUtil.Log($"成功获取开奖结果，期号: {result.Issue}, 状态: {result.Status}");             
            }
            else
            {
                if (tryCount > 1)  // 修改条件，确保还有重试次数
                {
                    LogUtil.Log($"获取开奖结果失败，还有{tryCount-1}次重试机会");
                    Thread.Sleep(1000);  // 等待1秒后重试
                    result = GetLatestResult(lotterCode, tryCount - 1);
                }
                else
                {
                    LogUtil.Log("获取开奖结果失败，重试次数已用完");
                    throw new Exception("获取开奖结果失败，重试次数已用完");
                }              
            }
            return result;
        }


        /// <summary>
        /// 开始新的一期
        /// </summary>
        /// <param name="issue"></param>
        /// <param name="openTime"></param>
        public static void StartGame(Action<Result> callBack, Result result = null)
        {
            if (result == null)
            {
                result = GetLatestResult(RobotClient.CurrentLotterCode);            
            }
            Result newResult = RobotClient.GetResultByIssue(result.NextIssue);
            if(newResult == null)
            {
                newResult = new Result();
                newResult.Type = result.Type;
                newResult.Issue = result.NextIssue;
                newResult.OpenTime = result.NextTime;
                newResult.Status = ResultStatus.竞猜中;
                ResultDao.AddResult(newResult);
                ResultList.Insert(0, newResult);
                foreach (User user in UserList)
                {
                    user.NewResultUser();
                }
            }
            callBack(newResult);
        }

        public static List<Result> Get今日开奖结果()
        {
            LotterCode lotterCode = RobotClient.CurrentLotterCode;
            List<Result> results = ResultDao.GetResult(lotterCode);

            //从开奖服务器补全当日开奖数据
            String url = IMConstant.ROBOT_SERVER + "/game/issue/getToday/" + (int)lotterCode;
            List<Result> serverResult = HttpHelper.Instance.HttpGet<List<Result>>(url);
            foreach (Result result in serverResult)
            {
                Result r = results.FirstOrDefault(x => x.Issue == result.Issue);
                if (r != null) { 
                    if(r.Status < ResultStatus.已开奖)
                    {                        
                        r.Num1 = result.Num1;
                        r.Num2 = result.Num2;
                        r.Num3 = result.Num3;                        
                        r.Sum = result.Sum;
                        r.OpenResult = result.OpenResult;
                        r.NextIssue = result.NextIssue;
                        r.NextTime = result.NextTime;
                        r.Status = ResultStatus.已开奖;
                        ResultDao.Save(r);                        
                    }
                }
                else
                {
                    result.Type = lotterCode;
                    result.Status = ResultStatus.无开盘;
                    result.Amount = 0;
                    result.Award = 0;
                    ResultDao.AddResult(result);
                    results.Add(result);
                }                
            }               
            return results;
        }
    }
}
