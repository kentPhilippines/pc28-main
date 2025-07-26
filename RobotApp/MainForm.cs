using CommonLibrary;
using DummyApp.Model;
using ImLibrary.IM;
using ImLibrary.Model;
using RobotApp.Dao;
using RobotApp.IM;
using RobotApp.MiniDialog;
using RobotApp.Model;
using RobotApp.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Websocket.Client;

namespace RobotApp
{
    [SupportedOSPlatform("windows")]
    internal partial class MainForm : Form
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetSystemTime(ref SYSTEMTIME lpSystemTime);

        private DataTable dtCustomKeyword;

        private BindingSource userBindingSource = new BindingSource();
        private BindingSource scoreBindingSource = new BindingSource();
        private BindingSource resultBindingSource = new BindingSource();
        private CountdownTimer countdown; //开奖倒计时

        [StructLayout(LayoutKind.Sequential)]
        struct SYSTEMTIME
        {
            public short year;
            public short month;
            public short dayOfWeek;
            public short day;
            public short hour;
            public short minute;
            public short second;
            public short milliseconds;
        }

        public MainForm()
        {
            InitializeComponent();
            //取消线程检查的方法,取消后出现问题很难查找
            //CheckForIllegalCrossThreadCalls = false;
            dgv玩家列表.AutoGenerateColumns = false;
            dgv玩家列表.TopLeftHeaderCell.Value = "No.";
            userBindingSource.DataSource = RobotClient.UserList;
            dgv玩家列表.DataSource = userBindingSource;
            userBindingSource.ListChanged += UserBindingSource_ListChanged;

            dgv开奖数据.AutoGenerateColumns = false;
            resultBindingSource.DataSource = RobotClient.ResultList;
            dgv开奖数据.DataSource = resultBindingSource;

            dgv查回审核.AutoGenerateColumns = false;
            scoreBindingSource.DataSource = RobotClient.ScoreList;
            dgv查回审核.DataSource = scoreBindingSource;

            //玩家列表各属性绑定
            txt昵称.DataBindings.Add("Text", userBindingSource, "NickName", false, DataSourceUpdateMode.Never);
            txt竞猜内容.DataBindings.Add("Text", userBindingSource, "Ccnr", false, DataSourceUpdateMode.Never);
            chk假人.DataBindings.Add("Checked", userBindingSource, "IsDummy", false, DataSourceUpdateMode.Never);
            txt积分.DataBindings.Add("Text", userBindingSource, "Jifen", false, DataSourceUpdateMode.Never);
            txt上轮输赢.DataBindings.Add("Text", userBindingSource, "Slyk", false, DataSourceUpdateMode.Never);

            //查回审核属性绑定
            txt查回审核昵称.DataBindings.Add("Text", scoreBindingSource, "NickName", false, DataSourceUpdateMode.Never);

            RobotClient.Robot.OnReceiveRawPacketComomand += TcpSocketClient_OnReceiveRawPacketComomand;
            RobotClient.Robot.OnDisconnect += Robot_OnDisconnect; ;
            RobotClient.Robot.ReconnectionHappened += Robot_ReconnectionHappened;

            dgv玩家列表.DoubleBufferedDataGirdView(true);
        }

        private void Robot_ReconnectionHappened(IMUser user, Websocket.Client.ReconnectionInfo info)
        {
            this.Invoke(() =>
            {
                tssl动态提示.Text = "重新连接成功";
                LogUtil.Log($"WebSocket重连成功，游戏继续运行，重连类型: {info.Type}");
                
                // 确保游戏状态UI正确显示
                if (RobotClient.ROBOT_RUNNING)
                {
                    UpdateGameStatusUI();
                }
            });
        }

        private void UserBindingSource_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            tssl动态提示.Text = string.Format("当前帐单总积分:{0}", RobotClient.UserList.Where(u => u.IsDummy == false).Sum(u => u.Jifen));
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // 用双缓冲绘制窗口的所有子控件
                return cp;
            }
        }

        private void Robot_OnDisconnect(IMUser obj, DisconnectionInfo info)
        {
            LogUtil.Log("连接断开："+info.ToJson());
            
            // 不停止倒计时器和游戏，保持游戏连续运行
            // countdown?.StopAndDispose();  // 注释掉，保持倒计时器运行
            
            tssl动态提示.Text = "连接已断开，尝试重连中...";
            Debug.WriteLine("连接断开，游戏保持运行状态");
            
            // 只播放提示音，不停止游戏
            if (RobotClient.ROBOT_RUNNING)
            {
                SoundUtil.Play("断网");
                LogUtil.Log("网络断开，游戏保持运行状态，等待重连");
                // 停止游戏();  // 注释掉，不停止游戏
            }
        }

        private void TcpSocketClient_OnReceiveRawPacketComomand(IMUser imUser, string fromUid, string nickName, string message, string fp)
        {
            // 记录IM消息接收详细日志
            LogUtil.Log("=== IM消息处理开始 ===");
            LogUtil.Log($"处理时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            LogUtil.Log($"机器人状态: {(RobotClient.ROBOT_RUNNING ? "运行中" : "已停止")}");
            LogUtil.Log($"发送用户ID: {fromUid}");
            LogUtil.Log($"发送昵称: {nickName}");
            LogUtil.Log($"消息内容: {message}");
            LogUtil.Log($"指纹: {fp}");
            LogUtil.Log($"机器人自己ID: {RobotClient.Robot.UserId}");
            
            if (!RobotClient.ROBOT_RUNNING)
            {
                LogUtil.Log("机器人未运行，忽略消息");
                LogUtil.Log("===================");
                return;
            }
            if (fromUid == RobotClient.Robot.UserId)
            {
                LogUtil.Log("消息来自机器人自己，忽略");
                LogUtil.Log("===================");
                return;
            }
            User user = RobotClient.GetUser(fromUid);
            LogUtil.Log($"用户查询结果: {(user != null ? $"找到用户 {user.NickName}" : "用户不存在")}");

            //玩家不存在时新增
            if (user == null)
            {
                LogUtil.Log("用户不存在，准备创建新用户");
                //机器人自己不要添加
                if (fromUid == RobotClient.Robot.UserId)
                {
                    LogUtil.Log("发送者是机器人自己，不创建用户");
                    LogUtil.Log("===================");
                    return;
                }
                //管理员列表内的名称不要添加
                if (Config.GetInstance().编辑框_管理名单.Contains(nickName))
                {
                    LogUtil.Log($"昵称【{nickName}】在管理员名单中，不创建用户");
                    LogUtil.Log("===================");
                    Debug.WriteLine("昵称不能是管理名单内的");
                    return;
                }
                LogUtil.Log($"创建新用户：ID={fromUid}, 昵称={nickName}");
                user = new User(fromUid, nickName);
                UserDao.AddUser(user);
                LogUtil.Log("新用户创建成功");
            }
            try
            {
                string originalMessage = message;
                message = Regex.Replace(message, "<.*?>", string.Empty).Trim();
                LogUtil.Log($"消息清理前: 【{originalMessage}】");
                LogUtil.Log($"消息清理后: 【{message}】");

                #region 自定义回复关键词处理
                LogUtil.Log("开始处理自定义关键词匹配");
                foreach (DataRow dr in dtCustomKeyword.Rows)
                {
                    bool matchFlag = false;
                    foreach (string keysord in dr["关键词"].ToString().Split('|'))
                    {
                        switch ((string)dr["匹配模式"])
                        {
                            case "完全匹配":
                                if (message == keysord)
                                {
                                    matchFlag = true;
                                }
                                break;
                            case "包含":
                                if (message.Contains(keysord))
                                {
                                    matchFlag = true;
                                }
                                break;
                            case "关键词开头":
                                if (message.StartsWith(keysord))
                                {
                                    matchFlag = true;
                                }
                                break;
                            case "关键词结尾":
                                if (message.EndsWith(keysord))
                                {
                                    matchFlag = true;
                                }
                                break;
                        }
                    }
                    if (matchFlag)
                    {
                        LogUtil.Log($"关键词匹配成功：【{dr["关键词"]}】，匹配模式：{dr["匹配模式"]}");
                        if (dr["提示声音"].ToString() != "无")
                        {
                            LogUtil.Log($"播放提示声音：{dr["提示声音"]}");
                            SoundUtil.Play(dr["提示声音"].ToString());
                        }
                        if (!string.IsNullOrEmpty(dr["回复内容"].ToString()))
                        {
                            LogUtil.Log($"自动回复：{dr["回复内容"]}");
                            user.ReplayMessage(dr["回复内容"].ToString());
                        }
                    }
                }
                LogUtil.Log("自定义关键词处理完成");
                #endregion

                # region 保存消息
                LogUtil.Log("开始保存消息到数据库");
                Model.Message msg = new Model.Message();
                msg.Code = fromUid;
                msg.NickName = user.NickName;
                msg.Msg = message;
                msg.MsgTime = DateTime.Now;
                msg.Fp = fp;
                MessageDao.AddMessage(msg);
                RobotClient.MsgQueue.Enqueue(msg);
                LogUtil.Log($"消息已保存并加入队列，消息ID：{msg.Id}");
                #endregion

                #region 自动退分
                LogUtil.Log("检查自动退分触发条件");
                if (!Config.GetInstance().单选框_退分_流水关闭 && Config.GetInstance().编辑框_退分_今天流水关键字 == message)
                {
                    LogUtil.Log($"触发今天流水退分，关键字：{message}");
                    dgv玩家列表.Invoke(() =>
                    {
                        BettingUtil.ProcessAutoRunningWater(1, user);
                    });
                    LogUtil.Log("今天流水退分处理完成");
                    LogUtil.Log("===================");
                    return;
                }
                if (!Config.GetInstance().单选框_退分_流水关闭 && Config.GetInstance().编辑框_退分_昨天流水关键字 == message)
                {
                    LogUtil.Log($"触发昨天流水退分，关键字：{message}");
                    dgv玩家列表.Invoke(() =>
                    {
                        BettingUtil.ProcessAutoRunningWater(2, user);
                    });
                    LogUtil.Log("昨天流水退分处理完成");
                    LogUtil.Log("===================");
                    return;
                }
                if (!Config.GetInstance().单选框_退分_回水关闭 && Config.GetInstance().编辑框_退分_今天回水关键字 == message)
                {
                    LogUtil.Log($"触发今天回水退分，关键字：{message}");
                    dgv玩家列表.Invoke(() =>
                    {
                        BettingUtil.ProcessAutoReturnWater(1, user);
                    });
                    LogUtil.Log("今天回水退分处理完成");
                    LogUtil.Log("===================");
                    return;
                }
                if (!Config.GetInstance().单选框_退分_回水关闭 && Config.GetInstance().编辑框_退分_昨天回水关键字 == message)
                {
                    LogUtil.Log($"触发昨天回水退分，关键字：{message}");
                    dgv玩家列表.Invoke(() =>
                    {
                        BettingUtil.ProcessAutoReturnWater(2, user);
                    });
                    LogUtil.Log("昨天回水退分处理完成");
                    LogUtil.Log("===================");
                    return;
                }
                LogUtil.Log("未触发自动退分条件");
                #endregion


                #region 处理查分、回分
                Score score = new Score();
                StringComparison stringComparison = Config.GetInstance().选择框_关键词_区分大小写
                    ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                //查
                foreach (string cha in Config.GetInstance().编辑框_关键词_上分.Split('|'))
                {
                    if (message.StartsWith(cha, stringComparison) || message.EndsWith(cha, stringComparison))
                    {
                        string amountStr = message.Replace(cha, "");
                        if (stringComparison == StringComparison.OrdinalIgnoreCase)
                        {
                            amountStr = message.ToUpper().Replace(cha.ToUpper(), "");
                        }
                        if (int.TryParse(amountStr, out int amount))
                        {
                            if (amount > 0)
                            {
                                score.Amount = amount;
                                score.Code = fromUid;
                                score.NickName = user.NickName;
                                ScoreDao.AddScore(score);

                                if (Config.GetInstance().选择框_假人自动审核 && user.IsDummy)
                                {
                                    Task.Run(() =>
                                    {
                                        int delay = ThreadSafeRandom.Next(Config.GetInstance().编辑框_假人回复_查延迟_1, Config.GetInstance().编辑框_假人回复_查延迟_2);
                                        Thread.Sleep(delay * 1000);
                                        AutoChekChaHui(score);
                                    });
                                }
                                else
                                {
                                    dgv查回审核.Invoke(() =>
                                    {
                                        RobotClient.ScoreList.Add(score);
                                    });
                                    SoundUtil.Play("查分");
                                }
                            }
                        }
                        return;
                    }
                }

                if (user.Status != UserStatus.正常用户)
                {
                    user.ReplayMessage("玩家状态异常！");
                    return;
                }

                //回
                foreach (string hui in Config.GetInstance().编辑框_关键词_回分.Split('|'))
                {
                    if (message.StartsWith(hui, stringComparison) || message.EndsWith(hui, stringComparison))
                    {
                        string amountStr = message.Replace(hui, "");
                        if (stringComparison == StringComparison.OrdinalIgnoreCase)
                        {
                            amountStr = message.ToUpper().Replace(hui.ToUpper(), "");
                        }
                        if (message == "全回")
                        {
                            amountStr = Convert.ToString(user.Jifen_Available);
                        }
                        if (int.TryParse(amountStr, out int amount))
                        {
                            if (amount > 0)
                            {
                                if (user.Jifen_Available >= amount)
                                {
                                    score.Amount = -amount;
                                    score.Code = fromUid;
                                    score.NickName = user.NickName;
                                    ScoreDao.AddScore(score);

                                    //冻结回的积分     
                                    user.FreezeJifen(Math.Abs(score.Amount));

                                    if (Config.GetInstance().选择框_假人自动审核 && user.IsDummy)
                                    {
                                        Task.Run(() =>
                                        {
                                            int delay = ThreadSafeRandom.Next(Config.GetInstance().编辑框_假人回复_回延迟_1, Config.GetInstance().编辑框_假人回复_回延迟_2);
                                            Thread.Sleep(delay * 1000);
                                            AutoChekChaHui(score);
                                        });
                                    }
                                    else
                                    {
                                        dgv查回审核.Invoke(() =>
                                        {
                                            RobotClient.ScoreList.Add(score);
                                        });
                                        SoundUtil.Play("回分");
                                    }
                                }
                                else
                                {
                                    user.ReplayMessage("\r\n积分[" + user.Jifen + "],\r\n冻结[" + (user.FrozenJifen - user.Blzf) + "],\r\n本轮下注[" + user.Blzf + "],剩余积分不足！");
                                }
                            }
                            return;
                        }
                    }
                }
                #endregion


                #region 处理附加关键字
                //查数据
                if (Config.GetInstance().选择框_查数据)
                {
                    if (message == Config.GetInstance().编辑框_附加关键字_数据)
                    {
                        string bqxz = user.Ccnr;
                        if (string.IsNullOrEmpty(bqxz))
                        {
                            bqxz = "本期未下注";
                        }
                        else
                        {
                            bqxz = "本期下注:[" + bqxz + "]";
                        }
                        double jryk = BettingDao.GetTodayWinOrLoseByUser(user);
                        double water = BettingDao.GetTodayWaterByUser(user);
                        String replay = "\r\n当前积分:{0}\r\n当前盈亏:{1}\r\n当前流水:{2}\r\n{3}\r\n冻结积分:{4}\r\n剩余积分:{5}";
                        int showFrozeJifen = user.FrozenJifen - user.Blzf;
                        if (RobotClient.CurrentResult.Status >= ResultStatus.已封盘)
                        {
                            showFrozeJifen = user.FrozenJifen;
                        }
                        replay = string.Format(replay, user.Jifen, jryk, water, bqxz, showFrozeJifen, user.Jifen_Available);

                        /*@飞鱼 当前积分:46046
                        当前盈亏: 0
                        当前流水: 0
                        本期下注: [14/300 16/200 小3000 小单1000 小双2000]
                        冻结积分: 7500
                        剩余积分: 38546*/
                        user.ReplayMessage(replay);
                    }
                }

                //查流水
                if (Config.GetInstance().选择框_查流水)
                {
                    if (message == Config.GetInstance().编辑框_附加关键字_流水)
                    {
                        double water = BettingDao.GetTodayWaterByUser(user);
                        user.ReplayMessage("今日流水:" + water);
                    }
                }
                //查历史
                if (Config.GetInstance().选择框_查历史)
                {
                    if (message == Config.GetInstance().编辑框_附加关键字_历史)
                    {
                        user.ReplayMessage("走势:" + ResultDao.GetNResult(RobotClient.CurrentLotterCode, Config.GetInstance().编辑框_历史号码个数));
                    }
                }
                //查期数
                if (Config.GetInstance().选择框_查期数)
                {
                    if (message == Config.GetInstance().编辑框_附加关键字_期数)
                    {
                        user.ReplayMessage("\r\n当天下注:" + BettingDao.GetTodayBetCount(user) + "期");
                    }
                }
                //查数据回复新格式

                #endregion


                //取消
                if (Config.GetInstance().编辑框_关键词_取消.Split('|').Contains(message))
                {
                    if (RobotClient.CurrentResult.Status != ResultStatus.竞猜中)
                    {
                        user.ReplayMessage("已封盘,禁止取消!");
                        return;
                    }
                    if (user.RecordList.Count == 0)
                    {
                        user.ReplayMessage("本期未下注，无须取消!");
                        return;
                    }
                    dgv玩家列表.Invoke(() =>
                    {
                        user.ClearBetRecords(msg.Fp);
                    });
                    user.ReplayMessage("取消成功,当前积分：" + user.Jifen);
                    return;
                }

                //改
                if (message.StartsWith("改"))
                {
                    if (user.RecordList.Count == 0)
                    {
                        user.ReplayMessage("本期未下注，无须修改!");
                        return;
                    }
                    if (RobotClient.CurrentResult.Status != ResultStatus.竞猜中)
                    {
                        user.ReplayMessage("已封盘,禁止改单!");
                        return;
                    }
                }
                //关键词                     
                List<BetRecord> recordList = new List<BetRecord>();
                //梭哈
                bool isSoha = false;
                foreach (string s in Config.GetInstance().编辑框_关键词_梭哈.Split('|'))
                {
                    if (message.StartsWith(s, stringComparison) || message.EndsWith(s, stringComparison))
                    {
                        string keyword = message.Replace(s, "");
                        if (stringComparison == StringComparison.OrdinalIgnoreCase)
                        {
                            keyword = message.ToUpper().Replace(s.ToUpper(), "");
                        }
                        if (user.Jifen_Available > 0)
                        {
                            BetRecord record = new BetRecord();
                            record.ResultId = RobotClient.CurrentResult.Id;
                            record.UserCode = user.UserId;
                            record.BetType = BetType.SOHA;
                            record.Keyword = keyword;
                            record.Amount = user.Jifen_Available;
                            record.Status = BetRecordStatus.已下注;
                            record.Fp = msg.Fp;
                            recordList.Add(record);
                            isSoha = true;
                            break;
                        }
                        else
                        {
                            user.ReplayMessage("积分不足，本次梭哈作废，其它下注继续有效。");
                            return;
                        }
                    }
                }

                //追|跟
                if (Config.GetInstance().选择框_关键字_追跟)
                {
                    if (message == "追" || message == "跟")
                    {
                        if (user.RecordList.Count > 0)
                        {
                            user.ReplayMessage("本期已有下注，追跟前请先取消！");
                            return;
                        }
                        List<BetRecord> records = BetRecordDao.GetByUserAndResult(user, RobotClient.PrevResult);
                        if (records.Count == 0)
                        {
                            user.ReplayMessage("上期无下注，无法追跟！");
                            return;
                        }
                        List<BetRecord> newRecords = new List<BetRecord>();
                        foreach (BetRecord br in records)
                        {
                            BetRecord record = new BetRecord();
                            record.ResultId = RobotClient.CurrentResult.Id;
                            record.UserCode = user.UserId;
                            record.BetType = br.BetType;
                            record.Keyword = br.Keyword;
                            record.Amount = br.Amount;
                            record.Status = BetRecordStatus.已下注;
                            record.Remark = message;
                            record.Fp = fp;
                            newRecords.Add(record);
                        }
                        recordList.AddRange(newRecords);
                    }
                }

                LogUtil.Log("开始处理下注逻辑");
                if (!isSoha)
                {
                    recordList.AddRange(BettingUtil.ProcessMessage(user, msg));
                    LogUtil.Log($"BettingUtil.ProcessMessage处理完成，下注记录数：{recordList.Count}");
                }

                if (recordList.Count > 0)
                {
                    LogUtil.Log($"检测到下注记录，共{recordList.Count}条记录");
                    LogUtil.Log($"当前游戏状态：{RobotClient.CurrentResult.Status}");
                    
                    if (RobotClient.CurrentResult.Status != ResultStatus.竞猜中)
                    {
                        LogUtil.Log("游戏已封盘，拒绝下注");
                        if (Config.GetInstance().选择框_提示_停猜后)
                        {
                            user.ReplayMessage("已封盘,禁止下注!");
                        }
                        LogUtil.Log("===================");
                        return;
                    }

                    //本次下注总分
                    double bczf = recordList.Sum(bet => bet.Amount);
                    LogUtil.Log($"本次下注总分：{bczf}");
                    double availableForChange = user.Jifen_Available;
                    LogUtil.Log($"用户当前可用积分：{availableForChange}");
                    
                    if (msg.Msg.StartsWith("改"))
                    {
                        availableForChange += user.Blzf;
                        LogUtil.Log($"检测到改单，可用积分增加本轮下注：{user.Blzf}，总可用：{availableForChange}");
                    }
                    
                    if (availableForChange < bczf)
                    {
                        LogUtil.Log($"积分不足，需要：{bczf}，可用：{availableForChange}");
                        //积分不足
                        if (Config.GetInstance().选择框_超额无效)
                        {
                            LogUtil.Log("超额无效模式，清除本期所有下注");
                            dgv玩家列表.Invoke(() =>
                            {
                                user.ClearBetRecords(msg.Fp);
                            });
                            user.ReplayMessage("积分不足！本期竞猜全部无效，请重新竞猜。");
                        }
                        else
                        {
                            LogUtil.Log("普通模式，提示积分不足");
                            user.ReplayMessage("\r\n积分[" + user.Jifen + "],\r\n已冻结[" + (user.FrozenJifen - user.Blzf) + "],\r\n本轮已下注[" + user.Blzf + "],\r\n积分不足，本次下注无效！");
                        }
                        LogUtil.Log("===================");
                        return;
                    }

                    LogUtil.Log("积分检查通过，开始保存下注记录");
                    bool clearOld = msg.Msg.StartsWith("改");
                    if (clearOld)
                    {
                        LogUtil.Log("改单操作，先清除旧的下注记录");
                    }
                    
                    dgv玩家列表.Invoke(() =>
                    { 
                        if (clearOld)
                        {
                            user.ClearBetRecords(msg.Fp, true); // 改单时强制解冻积分
                        }
                        user.AddBetRecords(recordList);
                    });
                    LogUtil.Log("下注记录保存成功");

                    string result = BettingUtil.ProcessRule(user.RecordList);
                    LogUtil.Log($"规则检查结果：{(string.IsNullOrEmpty(result) ? "通过" : result)}");
                    
                    if (result != "")
                    {
                        LogUtil.Log("规则检查失败");
                        //猜猜超注超额，本期全部竞猜无效
                        if (Config.GetInstance().选择框_超额无效)
                        {
                            LogUtil.Log("超额无效模式，清除所有下注");
                            dgv玩家列表.Invoke(() =>
                            {
                                user.ClearBetRecords(msg.Fp);
                            });
                            user.ReplayMessage(result + "\r\n本期竞猜全部无效，请重新竞猜。");
                        }
                        else
                        {
                            LogUtil.Log("提示规则违规但不清除下注");
                            user.ReplayMessage(result);
                        }
                        LogUtil.Log("===================");
                        return;
                    }

                    //处理杀组合等
                    dgv玩家列表.Invoke(() =>
                    {
                        bool passShaRule = BettingUtil.ProcessShaRule(user);
                    });

                    if (Config.GetInstance().选择框_下注提示)
                    {
                        user.ReplayMessage("本期下注：" + user.Ccnr);
                        LogUtil.Log($"发送下注提示：{user.Ccnr}");
                    }
                }
                else
                {
                    LogUtil.Log("没有解析到下注记录，可能是查询、聊天或其他非下注消息");
                }
                LogUtil.Log("IM消息处理成功完成");
                LogUtil.Log("===================");
            }
            catch (Exception ex)
            {
                LogUtil.LogEx(ex);
                LogUtil.Log($"IM消息处理异常：{ex.Message}");
                LogUtil.Log("===================");
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadConfig(Control control)
        {
            foreach (Control ctrl in control.Controls)
            {
                if (ctrl.Controls.Count > 0)
                {
                    LoadConfig(ctrl);
                }
                string value = ReadWriteINI.ConfigFile_GetVal(ctrl.Name);
                if (value == null)
                {
                    continue;
                }
                //Console.WriteLine(rb.Name + "=" + value);

                if (ctrl.Name.StartsWith("单选框"))
                {
                    RadioButton rb = (RadioButton)ctrl;
                    if (value == "真")
                    {
                        rb.Checked = true;
                    }
                    if (value == "假")
                    {
                        rb.Checked = false;
                    }
                }
                else if (ctrl.Name.StartsWith("选择框"))
                {
                    CheckBox cb = (CheckBox)ctrl;
                    if (value == "真")
                    {
                        cb.Checked = true;
                    }
                    if (value == "假")
                    {
                        cb.Checked = false;
                    }
                }
                else if (ctrl.Name.StartsWith("编辑框"))
                {
                    if (ctrl is NumericUpDown)
                    {
                        NumericUpDown nu = (NumericUpDown)ctrl;
                        nu.Value = int.Parse(value);
                    }
                    else if (ctrl is RichTextBox)
                    {
                        RichTextBox richTextBox = (RichTextBox)ctrl;
                        richTextBox.Text = value;
                    }
                    else
                    {
                        TextBox txt = (TextBox)ctrl;
                        txt.Text = value;
                    }
                }
                else if (ctrl.Name.StartsWith("组合框"))
                {
                    ListBox lb = (ListBox)ctrl;
                }
                else if (ctrl.Name.StartsWith("列表框"))
                {
                    ListBox lb = (ListBox)ctrl;
                    lb.Items.Clear();
                    lb.Items.AddRange(value.ToEntity<string[]>());
                }
                else if (ctrl.Name.StartsWith("颜色"))
                {
                    if (int.TryParse(value, out int colorRGB))
                    {
                        ctrl.BackColor = Color.FromArgb(colorRGB);
                    }
                }
            }
        }

        private void MakeCountdownTimer(Result result)
        {
            if (result == null)
            {
                return;
            }
            Debug.WriteLine("期号：{0}  状态：{1}", result.Issue, result.Status);
            //开奖倒计时
            TimeSpan kjdjs = result.OpenTime - DateTime.Now;
            int second = (int)kjdjs.TotalSeconds;
            if (kjdjs > TimeSpan.Zero)
            {
                countdown = new CountdownTimer(second);

                countdown.Tick = (seconds) =>
                {
                    this.Invoke(() =>
                    {
                        lbl距离开奖.Text = "距离开奖:" + TimeSpan.FromSeconds(seconds).ToString("mm\\:ss");
                    });
                };

                ////封盘前提醒                
                int fpq = Config.GetInstance().编辑框_倒计时_封盘前提醒 + Config.GetInstance().编辑框_倒计时_封盘提醒;
                countdown.AddTimerAction(fpq, () =>
                {
                    RobotClient.SendConfigMessage(Config.GetInstance().编辑框_消息_封盘前);
                });

                //封盘提醒             
                countdown.AddTimerAction(Config.GetInstance().编辑框_倒计时_封盘提醒, () =>
                {
                    this.Invoke(() =>
                    {
                        BettingUtil.ProcessClosing();
                        RobotClient.CurrentResult.Status = ResultStatus.已封盘;
                        ResultDao.Save(RobotClient.CurrentResult);
                        RobotClient.SendConfigMessage(Config.GetInstance().编辑框_消息_封盘);
                        if (RobotClient.ROBOT_RUNNING)
                        {
                            SoundUtil.Play("封盘");
                        }
                    });
                });

                if(second < Config.GetInstance().编辑框_倒计时_封盘提醒)
                {
                    RobotClient.CurrentResult.Status = ResultStatus.已封盘;
                    ResultDao.Save(RobotClient.CurrentResult);
                }
                if (second < 0)
                {
                    RobotClient.CurrentResult.Status = ResultStatus.等待开奖;
                    ResultDao.Save(RobotClient.CurrentResult);
                }

                //封盘后核对
                if (Config.GetInstance().选择框_核对提醒)
                {
                    int hd = Config.GetInstance().编辑框_倒计时_封盘提醒 - Config.GetInstance().编辑框_倒计时_核对提醒;
                    countdown.AddTimerAction(hd, () =>
                    {
                        RobotClient.SendConfigMessage(Config.GetInstance().编辑框_消息_封盘后);
                    });
                }

                //开奖处理将由TransitionToNextPeriod方法统一处理，避免重复逻辑
                countdown.AddTimerAction(0, () =>
                {
                    // 只设置UI状态，具体开奖处理由TransitionToNextPeriod统一处理
                    this.Invoke(() =>
                    {
                        lbl距离开奖.Text = "距离开奖:开奖中";
                        RobotClient.CurrentResult.Status = ResultStatus.等待开奖;
                        ResultDao.Save(RobotClient.CurrentResult);
                        RobotClient.SendConfigMessage(Config.GetInstance().编辑框_消息_开奖前);
                    });

                    // 触发开奖处理 - 统一由ProcessLotteryDrawing方法处理
                    ProcessLotteryDrawing();
                });

                //卡奖警报
                if (Config.GetInstance().选择框_卡奖警报)
                {
                    int kjms = Config.GetInstance().编辑框_卡奖警报秒数;
                    if(RobotClient.CurrentLotterCode == LotterCode.宾果)
                    {
                        kjms = Config.GetInstance().编辑框_卡奖警报秒数_宾果;
                    }
                    countdown.AddTimerAction(-kjms, () =>
                    {
                        if (RobotClient.ROBOT_RUNNING)
                        {
                            Task.Run(async () =>
                            {
                                await StartKajianSound();
                            });
                            this.Invoke(() =>
                            {
                                MessageBox.Show(this, "卡奖了，请处理！");
                                StopKajianSound();
                            });                         
                        }
                    });
                }

                countdown.OnStart += () =>
                {
                    UpdateGameStatusUI();
                };

                countdown.OnStop += () =>
                {
                    ResetGameStatusUI();
                    this.Invoke(() =>
                    {
                        lbl距离开奖.Text = "距离开奖:???";
                    });
                };

                // 设置广告发送定时器
                SetupAdvertisementTimers(countdown);

                //启动倒计时
                countdown.Start();
            }
        }

        // 开奖处理锁，防止重复处理
        private static readonly object lotteryDrawingLock = new object();
        private static bool isProcessingLottery = false;

        /// <summary>
        /// 统一的开奖处理方法，避免重复逻辑
        /// </summary>
        private void ProcessLotteryDrawing()
        {
            // 防止重复处理开奖
            lock (lotteryDrawingLock)
            {
                if (isProcessingLottery)
                {
                    LogUtil.Log("开奖处理正在进行中，跳过重复请求");
                    return;
                }
                isProcessingLottery = true;
            }
            //启动一线程获取最新开奖结果
            Task.Run(() =>
            {
                Result result = null;
                string currentIssue = RobotClient.CurrentResult.Issue.ToString();
                
                do
                {
                    try
                    {
                        result = RobotClient.GetLatestResult(RobotClient.CurrentLotterCode);
                        LogUtil.Log($"获取开奖结果: 期号{result?.Issue}, 当前期号{RobotClient.CurrentResult.Issue}");
                    }
                    catch (Exception ex)
                    {
                        LogUtil.Log($"获取开奖结果异常: {ex.Message}");
                        MessageBox.Show($"{currentIssue}期开奖结果获取异常，请处理！");
                        
                        // 释放开奖处理锁
                        lock (lotteryDrawingLock)
                        {
                            isProcessingLottery = false;
                        }
                        return;
                    }
                }
                while (result == null || result.Issue < RobotClient.CurrentResult.Issue);

                if (result.Issue == RobotClient.CurrentResult.Issue)
                {
                    LogUtil.Log($"开始处理{currentIssue}期开奖结果");
                    
                    // 处理开奖结果，继续保持倒计时器运行
                    result = RobotClient.CurrentResult.SetResult(result);
                    ResultDao.Save(result);

                    this.Invoke(() =>
                    {
                        RobotClient.ResultList.Add(result);
                        BettingUtil.ProcessSettlement(result, RobotClient.UserList.ToList());
                        if (选择框_实时排序.Checked)
                        {
                            SortDataGridView(ListSortDirection.Descending);
                        }
                    });

                    RobotClient.SendConfigMessage(Config.GetInstance().编辑框_消息_开奖);
                    if (Config.GetInstance().编辑框_延迟发送账单秒 > 0)
                    {
                        Thread.Sleep(1000 * Config.GetInstance().编辑框_延迟发送账单秒);
                    }
                    RobotClient.SendConfigMessage(Config.GetInstance().编辑框_消息_账单);

                    LogUtil.Log($"{currentIssue}期开奖处理完成，开始下一期");

                    // 开始下一期，无缝连接倒计时
                    this.Invoke(() =>
                    {
                        RobotClient.StartGame((nextResult) =>
                        {
                            // 计算下一期的倒计时，并平滑过渡
                            TransitionToNextPeriod(nextResult);
                        }, result);
                    });

                    // 释放开奖处理锁
                    lock (lotteryDrawingLock)
                    {
                        isProcessingLottery = false;
                    }
                }
                else
                {
                    LogUtil.Log($"期号不匹配: 获取到{result.Issue}期，期望{RobotClient.CurrentResult.Issue}期");
                    
                    // 释放开奖处理锁
                    lock (lotteryDrawingLock)
                    {
                        isProcessingLottery = false;
                    }
                }
            });
        }

        /// <summary>
        /// 平滑过渡到下一期，无需停止当前倒计时器
        /// </summary>
        /// <param name="nextResult">下一期结果</param>
        private void TransitionToNextPeriod(Result nextResult)
        {
            if (nextResult == null || countdown == null)
            {
                return;
            }

            Debug.WriteLine("平滑过渡到下一期：{0}  状态：{1}", nextResult.Issue, nextResult.Status);
            
            // 计算下一期的倒计时时间
            TimeSpan kjdjs = nextResult.OpenTime - DateTime.Now;
            int second = (int)kjdjs.TotalSeconds;
            
            if (kjdjs > TimeSpan.Zero)
            {
                // 重置倒计时器到新的时间，清除旧的TimerActions
                countdown.Reset(second);

                // 重新设置Tick事件（保持连续的时间显示）
                countdown.Tick = (seconds) =>
                {
                    this.Invoke(() =>
                    {
                        lbl距离开奖.Text = "距离开奖:" + TimeSpan.FromSeconds(seconds).ToString("mm\\:ss");
                    });
                };

                // 重新设置封盘前提醒                
                int fpq = Config.GetInstance().编辑框_倒计时_封盘前提醒 + Config.GetInstance().编辑框_倒计时_封盘提醒;
                countdown.AddTimerAction(fpq, () =>
                {
                    RobotClient.SendConfigMessage(Config.GetInstance().编辑框_消息_封盘前);
                });

                // 重新设置封盘提醒             
                countdown.AddTimerAction(Config.GetInstance().编辑框_倒计时_封盘提醒, () =>
                {
                    this.Invoke(() =>
                    {
                        BettingUtil.ProcessClosing();
                        RobotClient.CurrentResult.Status = ResultStatus.已封盘;
                        ResultDao.Save(RobotClient.CurrentResult);
                        RobotClient.SendConfigMessage(Config.GetInstance().编辑框_消息_封盘);
                        if (RobotClient.ROBOT_RUNNING)
                        {
                            SoundUtil.Play("封盘");
                        }
                    });
                });

                // 处理已经过期的状态
                if(second < Config.GetInstance().编辑框_倒计时_封盘提醒)
                {
                    RobotClient.CurrentResult.Status = ResultStatus.已封盘;
                    ResultDao.Save(RobotClient.CurrentResult);
                }
                if (second < 0)
                {
                    RobotClient.CurrentResult.Status = ResultStatus.等待开奖;
                    ResultDao.Save(RobotClient.CurrentResult);
                }

                // 重新设置封盘后核对
                if (Config.GetInstance().选择框_核对提醒)
                {
                    int hd = Config.GetInstance().编辑框_倒计时_封盘提醒 - Config.GetInstance().编辑框_倒计时_核对提醒;
                    countdown.AddTimerAction(hd, () =>
                    {
                        RobotClient.SendConfigMessage(Config.GetInstance().编辑框_消息_封盘后);
                    });
                }

                // 重新设置开奖处理 - 使用统一的开奖处理方法
                countdown.AddTimerAction(0, () =>
                {
                    this.Invoke(() =>
                    {
                        lbl距离开奖.Text = "距离开奖:开奖中";
                        RobotClient.CurrentResult.Status = ResultStatus.等待开奖;
                        ResultDao.Save(RobotClient.CurrentResult);
                        RobotClient.SendConfigMessage(Config.GetInstance().编辑框_消息_开奖前);
                    });

                    // 调用统一的开奖处理方法，避免重复逻辑
                    ProcessLotteryDrawing();
                });

                // 重新设置卡奖警报
                if (Config.GetInstance().选择框_卡奖警报)
                {
                    int kjms = Config.GetInstance().编辑框_卡奖警报秒数;
                    if(RobotClient.CurrentLotterCode == LotterCode.宾果)
                    {
                        kjms = Config.GetInstance().编辑框_卡奖警报秒数_宾果;
                    }
                    countdown.AddTimerAction(-kjms, () =>
                    {
                        if (RobotClient.ROBOT_RUNNING)
                        {
                            Task.Run(async () =>
                            {
                                await StartKajianSound();
                            });
                            this.Invoke(() =>
                            {
                                MessageBox.Show(this, "卡奖了，请处理！");
                                StopKajianSound();
                            });                         
                        }
                    });
                }

                // 更新UI显示
                UpdateGameStatusUI();

                // 重新设置广告发送定时器
                SetupAdvertisementTimers(countdown);
            }
        }

        /// <summary>
        /// 设置广告发送定时器
        /// </summary>
        /// <param name="countdown">倒计时器对象</param>
        private void SetupAdvertisementTimers(CountdownTimer countdown)
        {
            if (countdown == null || RobotClient.CurrentResult == null)
            {
                return;
            }

            LogUtil.Log("=== 设置广告发送定时器 ===");
            LogUtil.Log($"当前期号: {RobotClient.CurrentResult.Issue}");

            // 广告1
            if (Config.GetInstance().编辑框_周期_广告1 > 0)
            {
                if (!string.IsNullOrEmpty(Config.GetInstance().编辑框_消息_广告1))
                {
                    if (RobotClient.CurrentResult.Issue % Config.GetInstance().编辑框_周期_广告1 == 0)
                    {
                        int sec = Config.GetInstance().编辑框_倒计时_封盘提醒 - Config.GetInstance().编辑框_时间_广告1;
                        countdown.AddTimerAction(sec, () =>
                        {
                            RobotClient.SendMessage(Config.GetInstance().编辑框_消息_广告1);
                        });
                        LogUtil.Log($"广告1已设置: 周期{Config.GetInstance().编辑框_周期_广告1}, 触发时间{sec}秒, 内容: {Config.GetInstance().编辑框_消息_广告1}");
                    }
                }
            }

            // 广告2
            if (Config.GetInstance().编辑框_周期_广告2 > 0)
            {
                if (!string.IsNullOrEmpty(Config.GetInstance().编辑框_消息_广告2))
                {
                    if (RobotClient.CurrentResult.Issue % Config.GetInstance().编辑框_周期_广告2 == 0)
                    {
                        int sec = Config.GetInstance().编辑框_倒计时_封盘提醒 - Config.GetInstance().编辑框_时间_广告2;
                        countdown.AddTimerAction(sec, () =>
                        {
                            RobotClient.SendMessage(Config.GetInstance().编辑框_消息_广告2);
                        });
                        LogUtil.Log($"广告2已设置: 周期{Config.GetInstance().编辑框_周期_广告2}, 触发时间{sec}秒, 内容: {Config.GetInstance().编辑框_消息_广告2}");
                    }
                }
            }

            // 广告3
            if (Config.GetInstance().编辑框_周期_广告3 > 0)
            {
                if (!string.IsNullOrEmpty(Config.GetInstance().编辑框_消息_广告3))
                {
                    if (RobotClient.CurrentResult.Issue % Config.GetInstance().编辑框_周期_广告3 == 0)
                    {
                        int sec = Config.GetInstance().编辑框_倒计时_封盘提醒 - Config.GetInstance().编辑框_时间_广告3;
                        countdown.AddTimerAction(sec, () =>
                        {
                            RobotClient.SendMessage(Config.GetInstance().编辑框_消息_广告3);
                        });
                        LogUtil.Log($"广告3已设置: 周期{Config.GetInstance().编辑框_周期_广告3}, 触发时间{sec}秒, 内容: {Config.GetInstance().编辑框_消息_广告3}");
                    }
                }
            }

            // 广告4
            if (Config.GetInstance().编辑框_周期_广告4 > 0)
            {
                if (!string.IsNullOrEmpty(Config.GetInstance().编辑框_消息_广告4))
                {
                    if (RobotClient.CurrentResult.Issue % Config.GetInstance().编辑框_周期_广告4 == 0)
                    {
                        int sec = Config.GetInstance().编辑框_倒计时_封盘提醒 - Config.GetInstance().编辑框_时间_广告4;
                        countdown.AddTimerAction(sec, () =>
                        {
                            RobotClient.SendMessage(Config.GetInstance().编辑框_消息_广告4);
                        });
                        LogUtil.Log($"广告4已设置: 周期{Config.GetInstance().编辑框_周期_广告4}, 触发时间{sec}秒, 内容: {Config.GetInstance().编辑框_消息_广告4}");
                    }
                }
            }

            LogUtil.Log("=====================");
        }

        /// <summary>
        /// 重新加载所有用户的注单数据（用于修复注单丢失问题）
        /// </summary>
        private void ReloadAllUserBetRecords()
        {
            if (RobotClient.CurrentResult == null)
            {
                LogUtil.Log("无法重新加载注单数据：CurrentResult为null");
                return;
            }

            LogUtil.Log($"开始重新加载所有用户注单数据，当前期次：{RobotClient.CurrentResult.Issue}期");
            
            int totalReloaded = 0;
            foreach (User user in RobotClient.UserList)
            {
                try
                {
                    var oldCount = user.RecordList.Count;
                    user.RecordList.Clear();
                    user.RecordList.AddRange(BetRecordDao.GetByUserAndResult(user, RobotClient.CurrentResult));
                    var newCount = user.RecordList.Count;
                    
                    if (newCount != oldCount)
                    {
                        LogUtil.Log($"用户 {user.NickName}({user.UserId}) 注单数据已更新：{oldCount} -> {newCount}");
                        totalReloaded++;
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.Log($"重新加载用户 {user.NickName}({user.UserId}) 注单数据失败：{ex.Message}");
                }
            }
            
            LogUtil.Log($"注单数据重新加载完成，共更新 {totalReloaded} 个用户的数据");
            
            // 刷新UI显示
            this.Invoke(() =>
            {
                UpdateGameStatusUI();
            });
        }

        /// <summary>
        /// 更新游戏状态相关的UI显示
        /// </summary>
        private void UpdateGameStatusUI()
        {
            this.Invoke(() =>
            {
                // 更新当前竞猜期号
                if (RobotClient.CurrentResult != null)
                {
                    lbl当前竞猜.Text = "当前竞猜:第" + RobotClient.CurrentResult.Issue + "期";
                }
                else
                {
                    lbl当前竞猜.Text = "当前竞猜:第?期";
                }

                // 更新上轮开奖信息
                if (RobotClient.PrevResult != null)
                {
                    Result preResult = RobotClient.PrevResult;
                    if (preResult == null || (preResult.Status > ResultStatus.无开盘 && preResult.Status < ResultStatus.已开奖))
                    {
                        lbl上轮开奖.Text = "上轮开奖:?+?+?=?";
                        lbl上轮输赢.Text = "输赢:?";
                    }
                    else
                    {
                        lbl上轮开奖.Text = "上轮开奖:" + preResult.ThreeNum + "=" + preResult.Sum;
                        double? sy = preResult.WinOrLose;
                        lbl上轮输赢.Text = "输赢:" + (sy == 0 ? 0 : -sy);
                    }
                }
                else
                {
                    lbl上轮开奖.Text = "上轮开奖:?+?+?=?";
                    lbl上轮输赢.Text = "输赢:?";
                }

                // 更新今日总输赢
                if (选择框_今日总输赢.Checked)
                {
                    选择框_今日总输赢.Text = "今日总输赢:" + BettingDao.GetTodayWinOrLose().ToString();
                }
            });
        }

        /// <summary>
        /// 重置游戏状态UI为默认显示
        /// </summary>
        private void ResetGameStatusUI()
        {
            this.Invoke(() =>
            {
                lbl当前竞猜.Text = "当前竞猜:第?期";
                lbl上轮开奖.Text = "上轮开奖:?+?+?=?";
                lbl上轮输赢.Text = "输赢:?";
            });
        }

        private bool keepPlaying = true;
        private async Task StartKajianSound()
        {
            while (keepPlaying)
            {
                SoundUtil.Play("卡奖"); // 在后台线程中播放声音，不阻塞UI线程
                await Task.Delay(3000); // 等待100毫秒，以减少CPU占用，可根据需要调整延迟时间
            }
        }

        private void StopKajianSound()
        {
            keepPlaying = false; // 停止播放声音的标志
        }

        private void timer当前时间_Tick(object sender, EventArgs e)
        {
            stalShowTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void timer开奖数据更新_Tick(object sender, EventArgs e)
        {
            // 定时自动获取开奖数据更新
            try
            {
                LogUtil.Log("定时自动获取开奖数据更新开始");
                LoadTodayResult();
                LogUtil.Log("定时自动获取开奖数据更新完成");
            }
            catch (Exception ex)
            {
                LogUtil.Log($"定时获取开奖数据失败: {ex.Message}");
            }
        }

        private void tsbBg28_Click(object sender, EventArgs e)
        {
            if (RobotClient.ROBOT_RUNNING)
            {
                MessageBox.Show("切换游戏前请先停止当前游戏");
                return;
            }
            DialogResult result = MessageBox.Show("确认要将游戏切换为【宾果28】么？", "确认对话框",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                if (RobotClient.UserList.Count(u => u.RecordList.Count > 0) > 0)
                {
                    if (RobotClient.CurrentResult != null
                        && RobotClient.CurrentResult.Status == ResultStatus.竞猜中)
                    {
                        //MessageBox.Show("上期已有下注并且已过封盘时间但未封盘，请先处理");
                        foreach (User user in RobotClient.UserList)
                        {
                            if (user.RecordList.Count > 0)
                            {
                                user.ClearBetRecords("切换游戏游戏到【宾果28】，自动清除加拿大28[" + RobotClient.CurrentResult.Issue + "]未封盘数据");
                            }
                        }
                        Result currentResult = RobotClient.CurrentResult;
                        currentResult.Amount = 0;
                        ResultDao.Save(currentResult);
                        OplogDao.Add("切换游戏到【宾果28】，自动清除自动清除加拿大28[" + currentResult.Issue + "]期所有下注");
                    }
                }

                countdown?.StopAndDispose();
                this.Text = "宾果28";
                tsbBg28.Enabled = false;
                tsbJnd28.Enabled = true;
                RobotClient.CurrentLotterCode = LotterCode.宾果;
                RobotClient.ResultList.Clear();
                resultBindingSource.Clear();
                LoadTodayResult();

                RobotClient.SendMessage("游戏切换为【宾果28】");
                OplogDao.Add("游戏切换为【宾果28】");
            }
        }

        private void tsbJnd28_Click(object sender, EventArgs e)
        {
            if (RobotClient.ROBOT_RUNNING)
            {
                MessageBox.Show("切换游戏前请先停止当前游戏");
                return;
            }
            DialogResult result = MessageBox.Show("确认要将游戏切换为【加拿大28】么？", "确认对话框",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                if (RobotClient.UserList.Count(u => u.RecordList.Count > 0) > 0)
                {
                    if (RobotClient.CurrentResult != null
                        && RobotClient.CurrentResult.Status == ResultStatus.竞猜中)
                    {
                        //MessageBox.Show("上期已有下注并且已过封盘时间但未封盘，请先处理");
                        foreach (User user in RobotClient.UserList)
                        {
                            if (user.RecordList.Count > 0)
                            {
                                user.ClearBetRecords("切换游戏游戏到【加拿大28】，自动清除宾果28[" + RobotClient.CurrentResult.Issue + "]未封盘数据");
                            }
                        }
                        Result currentResult = RobotClient.CurrentResult;
                        currentResult.Amount = 0;
                        ResultDao.Save(currentResult);
                        OplogDao.Add("切换游戏到【加拿大28】，自动清除自动清除宾果28[" + currentResult.Issue + "]期所有下注");
                    }
                }

                this.Text = "加拿大28";
                tsbBg28.Enabled = true;
                tsbJnd28.Enabled = false;
                RobotClient.CurrentLotterCode = LotterCode.加拿大;
                RobotClient.ResultList.Clear();
                resultBindingSource.Clear();
                LoadTodayResult();
                RobotClient.SendMessage("游戏切换为【加拿大28】");
                OplogDao.Add("游戏切换为【加拿大28】");
            }
        }

        private void tsbSave_Click(object sender, EventArgs e)
        {
            if (tabMain.SelectedTab.Text == "信息设置")
            {
                if (!Config.GetInstance().CheckPermission(PermissionForm.修改信息设置权限))
                {
                    return;
                }
                SaveConfig(信息设置);
            }
            else if (tabMain.SelectedTab.Text == "赔率设置")
            {
                if (!Config.GetInstance().CheckPermission(PermissionForm.修改游戏赔率权限))
                {
                    return;
                }
                SaveConfig(赔率设置);
            }
            else if (tabMain.SelectedTab.Text == "游戏规则")
            {
                if (!Config.GetInstance().CheckPermission(PermissionForm.修改游戏规则权限))
                {
                    return;
                }
                SaveConfig(游戏规则);
            }
            else if (tabMain.SelectedTab.Text == "命令设置")
            {
                //TODO 校验命令是否冲突

                SaveConfig(命令设置);
                //保存关键词回复
                ReadWriteINI.ConfigFile_SetVal("关键词", "自定义关键词", dgv回复关键词.DataSource.ToJson());
            }
            else if (tabMain.SelectedTab.Text == "语音提示")
            {
                SaveConfig(语音提示);
            }
            else if (tabMain.SelectedTab.Text == "扩展功能")
            {
                SaveConfig(扩展功能);
            }
            else if (tabMain.SelectedTab.Text == "自助退分")
            {
                SaveConfig(自助退分);
            }
            else
            {
                return;
            }

            Config.GetInstance().ReloadConfig();
            dgv查回审核.Refresh();
            dgv玩家列表.Refresh();
            MessageBox.Show("保存成功！");
        }

        private void SaveConfig(Control control, bool isRoot = true)
        {
            if (isRoot)
            {
                ReadWriteINI.postDataList = new List<UserConfig>();
            }
            foreach (Control ctrl in control.Controls)
            {
                if (ctrl.Controls.Count > 0)
                {
                    SaveConfig(ctrl, false);
                }
                if (ctrl.Name.StartsWith("单选框"))
                {
                    RadioButton rb = (RadioButton)ctrl;
                    if (rb.Checked)
                    {
                        ReadWriteINI.ConfigFile_SetVal(ctrl.Name, "真");
                    }
                    else
                    {
                        ReadWriteINI.ConfigFile_SetVal(ctrl.Name, "假");
                    }

                }
                else if (ctrl.Name.StartsWith("选择框"))
                {
                    CheckBox cb = (CheckBox)ctrl;
                    if (cb.Checked)
                    {
                        ReadWriteINI.ConfigFile_SetVal(ctrl.Name, "真");
                    }
                    else
                    {
                        ReadWriteINI.ConfigFile_SetVal(ctrl.Name, "假");
                    }
                }
                else if (ctrl.Name.StartsWith("编辑框"))
                {
                    ReadWriteINI.ConfigFile_SetVal(ctrl.Name, ctrl.Text);
                }
                else if (ctrl.Name.StartsWith("组合框"))
                {

                }
                else if (ctrl.Name.StartsWith("列表框"))
                {
                    ListBox lb = (ListBox)ctrl;
                    ReadWriteINI.ConfigFile_SetVal(ctrl.Name, lb.Items.ToJson());
                }
                else if (ctrl.Name.StartsWith("颜色"))
                {
                    ReadWriteINI.ConfigFile_SetVal(ctrl.Name, ctrl.BackColor.ToArgb().ToString());
                }
            }
            if (isRoot)
            {
                ReadWriteINI.SaveConfigList();
            }
        }

        private async void tsbStart_Click(object sender, EventArgs e)
        {
            if (!RobotClient.Robot.IsRunning)
            {
                await RobotClient.Robot.ReConnect();
            }

            if (tsbStart.Checked)
            {
                countdown?.StopAndDispose();
                停止游戏();
                LogUtil.Log("停止游戏！");
                RobotClient.SendMessage(ReadWriteINI.ConfigFile_GetVal("编辑框_消息_停止"));
            }
            else
            {
                if (RobotClient.UserList.Count(u => u.RecordList.Count > 0) > 0)
                {
                    if (RobotClient.CurrentResult != null
                        && RobotClient.CurrentResult.Status == ResultStatus.竞猜中
                        && DateTime.Now > RobotClient.CurrentResult.OpenTime.AddSeconds(-Config.GetInstance().编辑框_倒计时_封盘提醒))
                    {
                        //MessageBox.Show("上期已有下注并且已过封盘时间但未封盘，请先处理");
                        foreach (User user in RobotClient.UserList)
                        {
                            if (user.RecordList.Count > 0)
                            {
                                user.ClearBetRecords("竞猜中意外关机，重启后已过封盘时间，自动清除[" + RobotClient.CurrentResult.Issue + "]未封盘数据");
                            }
                        }
                        Result currentResult = RobotClient.CurrentResult;
                        currentResult.Amount = 0;
                        ResultDao.Save(currentResult);
                        OplogDao.Add("竞猜中意外关机，重启后已过封盘时间，自动清除[" + currentResult.Issue + "]期所有下注");
                    }
                }
                try
                {
                    RobotClient.StartGame((result) =>
                    {
                        MakeCountdownTimer(result);
                    });
                }
                catch (Exception)
                {
                    MessageBox.Show("获取开奖信息失败，可能网络还未准备好，请重试！");
                    return;
                }
                启动游戏();
                LogUtil.Log("启动游戏！");
                RobotClient.SendMessage(ReadWriteINI.ConfigFile_GetVal("编辑框_消息_启动"));
            }
        }

        private void 启动游戏()
        {
            RobotClient.ROBOT_RUNNING = true;
            tssl状态.Text = "启动";
            tsbStart.Checked = true;
            tsbStart.Image = global::RobotApp.Properties.Resources.pause;
            tsbStart.Text = "停止游戏";
            tssl动态提示.Text = "已启动游戏";
        }

        private void 停止游戏()
        {
            RobotClient.ROBOT_RUNNING = false;
            this.Invoke(() =>
            {
                tssl状态.Text = "停止";
                tsbStart.Checked = false;
                tsbStart.Image = global::RobotApp.Properties.Resources.play;
                tsbStart.Text = "启动游戏";
                tssl动态提示.Text = "已停止游戏";
            });
        }

        private void tsbHelper_Click(object sender, EventArgs e)
        {
            GameAssistantForm gameAssistantForm = GameAssistantForm.GetInstance();
            gameAssistantForm.ShowOrActive();
        }

        private void btn测试消息_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(编辑框_消息_测试.Text))
            {
                MessageBox.Show("消息不能为空！");
            }
            else
            {
                RobotClient.SendMessage(编辑框_消息_测试.Text);
            }
        }

        private void btn试算_Click(object sender, EventArgs e)
        {
            string jcnr = txt试算竞猜内容.Text;
            Model.Message msg = new Model.Message();
            msg.Msg = jcnr;
            List<BetRecord> recordList = BettingUtil.ProcessMessage(null, msg, true);
            if (recordList.Count == 0)
            {
                MessageBox.Show("请输入竞猜内容");
                return;
            }
            Result result = new Result();
            string num1 = txt试算号码1.Text;
            if (num1 == null || num1.Trim().Length == 0)
            {
                MessageBox.Show("试算号码1不能为空");
                return;
            }
            if (int.TryParse(num1, out int one))
            {
                if (one < 0 || one > 9)
                {
                    MessageBox.Show("试算号码1只能是0-9的数字");
                    return;
                }
                result.Num1 = one;
            }
            else
            {
                MessageBox.Show("试算号码1只能是数字");
                return;
            }

            string num2 = txt试算号码2.Text;
            if (num1 == null || num1.Trim().Length == 0)
            {
                MessageBox.Show("试算号码2不能为空");
                return;
            }
            if (int.TryParse(num2, out int two))
            {
                if (two < 0 || two > 9)
                {
                    MessageBox.Show("试算号码2只能是0-9的数字");
                    return;
                }
                result.Num2 = two;
            }
            else
            {
                MessageBox.Show("试算号码2只能是数字");
                return;
            }

            string num3 = txt试算号码3.Text;
            if (num1 == null || num1.Trim().Length == 0)
            {
                MessageBox.Show("试算号码3不能为空");
                return;
            }
            if (int.TryParse(num3, out int three))
            {
                if (three < 0 || three > 9)
                {
                    MessageBox.Show("试算号码3只能是0-9的数字");
                    return;
                }
                result.Num3 = three;
            }
            else
            {
                MessageBox.Show("试算号码3只能是数字");
                return;
            }
            result.Sum = result.Num1 + result.Num2 + result.Num3;
            BettingUtil.CalcOdds(recordList, result);
            txt试算输赢.Text = BettingUtil.ProcessOpenResult(recordList, result).ToString();
        }

        private void AutoChekChaHui(Score score)
        {
            if (score.Status != ScoreStatus.待审核)
            {
                return;
            }
            User user = RobotClient.GetUser(score.Code);
            if (RobotClient.UserList.FirstOrDefault(u => u.UserId == user.UserId) == null)
            {
                dgv玩家列表.Invoke(() =>
                {
                    RobotClient.UserList.Add(user);
                });
            }
            //查分同意
            if (score.Amount > 0)
            {
                dgv玩家列表.Invoke(() =>
                {
                    user.ChangeJifen(score.Amount, RecordType.上分, score.Id);
                    user.Status = UserStatus.正常用户;
                });
                score.Status = ScoreStatus.同意;
                score.ReviewTime = DateTime.Now;
                ScoreDao.UpdateScore(score);
                if (!Config.GetInstance().选择框_假人自动审核 || !user.IsDummy)
                {
                    dgv查回审核.Invoke(() =>
                    {
                        RobotClient.ScoreList.Remove(score);
                    });
                }
                RobotClient.SendMessage("@" + user.NickName + " 上分" + score.Amount + "成功！当前积分:" + user.Jifen);
            }

            //回分同意
            if (score.Amount < 0)
            {
                try
                {
                    dgv玩家列表.Invoke(() =>
                    {
                        user.ChangeJifen(score.Amount, RecordType.下分, score.Id);
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                score.Status = ScoreStatus.同意;
                score.ReviewTime = DateTime.Now;
                ScoreDao.UpdateScore(score);
                if (!Config.GetInstance().选择框_假人自动审核 || !user.IsDummy)
                {
                    dgv查回审核.Invoke(() =>
                    {
                        RobotClient.ScoreList.Remove(score);
                    });
                }
                RobotClient.SendMessage("@" + user.NickName + " 回分" + score.Amount + "成功！当前积分:" + user.Jifen);
            }
        }

        private void cmsCHSH_TY_Click(object sender, EventArgs e)
        {
            Score score = (Score)scoreBindingSource.Current;

            AutoChekChaHui(score);
        }

        private void cms同意并设置为假人_Click(object sender, EventArgs e)
        {
            Score score = (Score)scoreBindingSource.Current;
            User user = RobotClient.GetUser(score.Code);

            //查分同意
            if (score.Amount > 0)
            {
                if (RobotClient.UserList.FirstOrDefault(u => u.UserId == user.UserId) == null)
                {
                    RobotClient.UserList.Add(user);
                }
                user.IsDummy = true;
                user.ChangeJifen(score.Amount, RecordType.上分, score.Id);
                user.Status = UserStatus.正常用户;
                score.Status = ScoreStatus.同意;
                score.ReviewTime = DateTime.Now;
                ScoreDao.UpdateScore(score);
                RobotClient.ScoreList.Remove(score);
                RobotClient.SendMessage("@" + user.NickName + " 上分" + score.Amount + "成功！当前积分:" + user.Jifen);
            }
        }

        private void cmsCHSH_JJ_Click(object sender, EventArgs e)
        {
            Score score = (Score)scoreBindingSource.Current;
            score.Status = ScoreStatus.拒绝;
            score.ReviewTime = DateTime.Now;
            ScoreDao.UpdateScore(score);
            User user = RobotClient.GetUser(score.Code);
            if (score.Amount > 0)
            {
                //RobotClient.SendMessage("@" + user.NickName + " 上分" + score.Amount + "失败！");
            }
            else
            {
                user.UnfreezeJifen(Math.Abs(score.Amount));
                //RobotClient.SendMessage("@" + user.NickName + " 回分" + score.Amount + "失败！");
            }
            RobotClient.ScoreList.Remove(score);
        }


        private void cmsCHSH_全部拒绝_Click(object sender, EventArgs e)
        {
            foreach (Score score in RobotClient.ScoreList)
            {
                score.Status = ScoreStatus.拒绝;
                score.ReviewTime = DateTime.Now;
                ScoreDao.UpdateScore(score);
                User user = RobotClient.GetUser(score.Code);
                if (score.Amount > 0)
                {
                    //RobotClient.SendMessage("@" + user.NickName + " 上分" + score.Amount + "失败！");
                }
                else
                {
                    user.UnfreezeJifen(Math.Abs(score.Amount));
                    //RobotClient.SendMessage("@" + user.NickName + " 回分" + score.Amount + "失败！");
                }
            }
            RobotClient.ScoreList.Clear();
        }

        private void cmsCHSH_全部同意_Click(object sender, EventArgs e)
        {
            foreach (Score score in RobotClient.ScoreList)
            {
                User user = RobotClient.GetUser(score.Code);

                //查分同意
                if (score.Amount > 0)
                {
                    if (RobotClient.UserList.FirstOrDefault(u => u.UserId == user.UserId) == null)
                    {
                        RobotClient.UserList.Add(user);
                    }
                    user.ChangeJifen(score.Amount, RecordType.上分, score.Id);
                    user.Status = UserStatus.正常用户;
                    score.Status = ScoreStatus.同意;
                    score.ReviewTime = DateTime.Now;
                    ScoreDao.UpdateScore(score);
                    RobotClient.SendMessage("@" + user.NickName + " 上分" + score.Amount + "成功！当前积分:" + user.Jifen);
                }

                //回分同意
                if (score.Amount < 0)
                {
                    try
                    {
                        user.ChangeJifen(score.Amount, RecordType.下分, score.Id);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }

                    score.Status = ScoreStatus.同意;
                    score.ReviewTime = DateTime.Now;
                    ScoreDao.UpdateScore(score);
                    RobotClient.SendMessage("@" + user.NickName + " 回分" + score.Amount + "成功！当前积分:" + user.Jifen);
                }
            }
            RobotClient.ScoreList.Clear();
        }

        private void 颜色_真人_Click(object sender, EventArgs e)
        {
            if (mycolor.ShowDialog() == DialogResult.OK)
            {
                // 用户选择颜色后，更新文本框背景色
                颜色_真人.BackColor = mycolor.Color;
            }
        }

        private void 颜色_上分_Click(object sender, EventArgs e)
        {
            if (mycolor.ShowDialog() == DialogResult.OK)
            {
                颜色_上分.BackColor = mycolor.Color;
            }
        }

        private void 颜色_下分_Click(object sender, EventArgs e)
        {
            if (mycolor.ShowDialog() == DialogResult.OK)
            {
                颜色_下分.BackColor = mycolor.Color;
            }
        }

        private void 颜色_积分不足_Click(object sender, EventArgs e)
        {
            if (mycolor.ShowDialog() == DialogResult.OK)
            {
                颜色_积分不足.BackColor = mycolor.Color;
            }
        }

        private void 颜色_假人_Click(object sender, EventArgs e)
        {
            if (mycolor.ShowDialog() == DialogResult.OK)
            {
                颜色_假人.BackColor = mycolor.Color;
            }
        }

        private void dgv查回审核_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var row = dgv查回审核.Rows[e.RowIndex];
            string amountValue = row.Cells[2].Value.ToString();
            if (amountValue.StartsWith("回"))
            {
                if (int.TryParse(ReadWriteINI.ConfigFile_GetVal("颜色_下分"), out int colorRGB))
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(colorRGB);
                }
            }
            else
            {
                if (int.TryParse(ReadWriteINI.ConfigFile_GetVal("颜色_上分"), out int colorRGB))
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(colorRGB);
                }
            }
        }

        private void btnTestMessage_Click(object sender, EventArgs e)
        {
            tabMain.SelectedTab = tabMain.TabPages["信息设置"];
            tab信息设置.SelectedTab = tab信息设置.TabPages["信息设置_测试"];
        }

        private void tsbTime_Click(object sender, EventArgs e)
        {
            string url = IMConstant.ROBOT_SERVER; // 替换为远程服务器的URL
            DateTime remoteServerTime = GetRemoteServerTime(url);
            DateTime utcTime = remoteServerTime.ToUniversalTime();
            // 请确保你的应用程序以管理员身份运行
            // 设置新的时间为 2023年4月1日 12:00:00
            SYSTEMTIME newTime = new SYSTEMTIME
            {
                year = (short)utcTime.Year,
                month = (short)utcTime.Month,
                dayOfWeek = (short)utcTime.DayOfWeek, // 假设星期日
                day = (short)utcTime.Day,
                hour = (short)utcTime.Hour,
                minute = (short)utcTime.Minute,
                second = (short)utcTime.Second,
                milliseconds = (short)utcTime.Millisecond
            };

            if (SetSystemTime(ref newTime))
            {
                MessageBox.Show("服务器时间：" + remoteServerTime.ToString());
            }
            else
            {
                MessageBox.Show("无法设置时间，可能是权限不足。\r\n请以管理员身份运行！");
            }
        }

        private DateTime GetRemoteServerTime(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "HEAD";
            request.Timeout = 3000; // 设置超时时间，可根据需要调整

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                string dateHeader = response.Headers["Date"];
                DateTime serverTime;
                bool success = DateTime.TryParse(dateHeader, out serverTime);

                if (success)
                {
                    return serverTime;
                }
                else
                {
                    throw new Exception("Failed to retrieve remote server time.");
                }
            }
        }

        private void btn禁言_Click(object sender, EventArgs e)
        {
            DataGridViewRow dgvr = dgv玩家列表.CurrentRow;
            if (dgvr == null)
            {
                MessageBox.Show("请选择要禁言的玩家");
                return;
            }
            //int code = IMApi.AddGroupSlience(dgvr.Cells["Code"].Value.ToString(), dgvr.Cells["昵称"].Value.ToString());
            int code = RobotClient.Robot.DefauleGroup.AddGroupSlience(dgvr.Cells["Code"].Value.ToString());
            if (code == 0)
            {
                OplogDao.Add("玩家【" + dgvr.Cells["昵称"].Value.ToString() + "】禁言成功");
                MessageBox.Show("禁言成功");
            }
            else
            {
                OplogDao.Add("玩家【" + dgvr.Cells["昵称"].Value.ToString() + "】禁言失败");
                MessageBox.Show("禁言失败");
            }
        }

        private void btn解除禁言_Click(object sender, EventArgs e)
        {
            DataGridViewRow dgvr = dgv玩家列表.CurrentRow;
            if (dgvr == null)
            {
                MessageBox.Show("请选择要解除禁言的玩家");
                return;
            }
            //int code = IMApi.DeleteGroupSlience(dgvr.Cells["Code"].Value.ToString(), dgvr.Cells["昵称"].Value.ToString());
            int code = RobotClient.Robot.DefauleGroup.DeleteGroupSlience(dgvr.Cells["Code"].Value.ToString());
            if (code == 0)
            {
                OplogDao.Add("玩家【" + dgvr.Cells["昵称"].Value.ToString() + "】解除禁言成功");
                MessageBox.Show("解除禁言成功");
            }
            else
            {
                OplogDao.Add("玩家【" + dgvr.Cells["昵称"].Value.ToString() + "】解除禁言失败");
                MessageBox.Show("解除禁言失败");
            }
        }

        private void btn移除玩家_Click(object sender, EventArgs e)
        {
            User currentUser = (User)userBindingSource.Current;
            if (MessageBox.Show("确定要移除玩家【" + currentUser.NickName + "】么？", "确认对话框",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                currentUser.Status = UserStatus.已删除;
                RobotClient.UserList.Remove(currentUser);
            }
        }

        private void btn修改积分_Click(object sender, EventArgs e)
        {
            if (Config.GetInstance().CheckPermission(PermissionForm.修改用户积分权限))
            {
                User currentUser = (User)userBindingSource.Current;
                if (int.TryParse(txt积分.Text, out int jifen))
                {
                    currentUser.ChangeJifen(jifen - currentUser.Jifen, RecordType.客服修改);
                }
                else
                {
                    MessageBox.Show("积分只能为数字");
                }
            }
        }

        private void btn修改竞猜内容_Click(object sender, EventArgs e)
        {
            User currentUser = (User)userBindingSource.Current;
            currentUser.Ccnr = txt竞猜内容.Text;
        }

        private void btn隐藏0积分_Click(object sender, EventArgs e)
        {
            dgv玩家列表.CurrentCell = null; //设为null,解决 与货币管理器的位置关联的行不能设置为不可见 
            foreach (DataGridViewRow dgvr in dgv玩家列表.Rows)
            {
                if (dgvr.Cells["积分"].Value.ToString() == "0")
                {
                    dgvr.Visible = false;
                }
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (RobotClient.ROBOT_RUNNING)
            {
                RobotClient.SendMessage("系统已停止");
            }
            RobotClient.Robot.TryLogout();
            
            // 关闭前确保数据安全
            try
            {
                // 强制保存所有用户数据
                SaveAllUserData();
                
                // 强制同步WAL到主数据库
                DBHelperSQLite.FlushWalToMainDB();
                
                // 执行备份
                DBHelperSQLite.BackUpDateBase();
                
                // 正确关闭数据库连接
                DBHelperSQLite.CloseDB();
                
                LogUtil.Log("应用程序正常关闭，数据已保存");
            }
            catch (Exception ex)
            {
                LogUtil.LogEx(ex);
                // 即使出错也尝试基本的数据保存
                try
                {
                    DBHelperSQLite.FlushWalToMainDB();
                }
                catch { }
            }
            
            Application.Exit();
            Environment.Exit(0);
        }
        
        /// <summary>
        /// 保存所有用户数据到数据库
        /// </summary>
        private void SaveAllUserData()
        {
            try
            {
                foreach (User user in RobotClient.UserList)
                {
                    UserDao.UpdateUser(user);
                }
                LogUtil.Log($"已保存 {RobotClient.UserList.Count} 个用户的数据");
            }
            catch (Exception ex)
            {
                LogUtil.LogEx(ex);
            }
        }

        private void dgv玩家列表_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow dgvr = dgv玩家列表.Rows[e.RowIndex];
            if (dgvr != null)
            {
                string code = (string)dgvr.Cells["Code"].Value;
                User user = RobotClient.GetUser(code);
                if (e.ColumnIndex == 1)
                {
                    //查回如果有记录，同步修改显示的名称
                    foreach (Score score in RobotClient.GetScoreByUserId(user.UserId))
                    {
                        score.NickName = user.NickName;
                    }
                }
            }
        }

        private void btn修改昵称_Click(object sender, EventArgs e)
        {
            if (UserDao.GetUserByNickName(txt查回审核昵称.Text) != null)
            {
                MessageBox.Show("该昵称已存在，请换一个！");
                return;
            }
            Score currentScore = (Score)scoreBindingSource.Current;
            currentScore.NickName = txt查回审核昵称.Text;
            User user = RobotClient.GetUser(currentScore.Code);
            user.NickName = txt查回审核昵称.Text;
        }

        private void btn显_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dgvr in dgv玩家列表.Rows)
            {
                dgvr.Visible = true;
            }
        }

        private void tsbWeiyu_Click(object sender, EventArgs e)
        {
            ChatRecordForm chatRecordForm = ChatRecordForm.GetInstance();
            chatRecordForm.Show();
        }

        private void btn关键词添加_Click(object sender, EventArgs e)
        {
            if (txt关键词.Text.Trim() == "")
            {
                MessageBox.Show("关键词不能为空");
                return;
            }
            dtCustomKeyword.Rows.Add(
                txt关键词.Text,
                rtb回复内容.Text,
                cmb提示声音.Text,
                cmb匹配模式.Text
            );
            txt关键词.Text = string.Empty;
            rtb回复内容.Text = string.Empty;
        }

        private void btn关键词修改_Click(object sender, EventArgs e)
        {
            DataGridViewRow dgvr = dgv回复关键词.CurrentRow;
            if (dgvr == null) { return; }

            if (txt关键词.Text.Trim() == "")
            {
                MessageBox.Show("关键词不能为空");
                return;
            }

            dgvr.Cells[0].Value = txt关键词.Text;
            dgvr.Cells[1].Value = rtb回复内容.Text;
            dgvr.Cells[2].Value = cmb提示声音.Text;
            dgvr.Cells[3].Value = cmb匹配模式.Text;
        }

        private void btn关键词删除_Click(object sender, EventArgs e)
        {
            DataGridViewRow dgvr = dgv回复关键词.CurrentRow;
            if (dgvr == null) { return; }
            dgv回复关键词.Rows.Remove(dgvr);
        }

        private void 语音提示_Enter(object sender, EventArgs e)
        {
            cmb事件提示音.DataSource = SoundUtil.GetAllSoundFileName();
        }

        private void cmb事件提示音_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (列表框_事件提示音.SelectedIndex != -1)
            {
                string item = 列表框_事件提示音.SelectedItem.ToString();
                item = item.Substring(0, item.IndexOf("=") + 1) + cmb事件提示音.Text;
                列表框_事件提示音.Items[列表框_事件提示音.SelectedIndex] = item;
            }
        }

        private void 列表框_事件提示音_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (列表框_事件提示音.SelectedIndex == -1)
            {
                return;
            }
            string item = 列表框_事件提示音.SelectedItem.ToString();
            item = item.Substring(item.IndexOf("=") + 1);
            int inx = cmb事件提示音.Items.IndexOf(item);
            if (inx == -1)
            {
                MessageBox.Show(item + " 文件不存在！");
                return;
            }
            cmb事件提示音.SelectedIndex = cmb事件提示音.Items.IndexOf(item);
        }

        private void dgv查回审核_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                dgv查回审核.ClearSelection();
                dgv查回审核.Rows[e.RowIndex].Selected = true;
                dgv查回审核.CurrentCell = dgv查回审核.Rows[e.RowIndex].Cells[e.ColumnIndex];
                string score = dgv查回审核.Rows[e.RowIndex].Cells["查回审核_分数"].Value.ToString();
                cms同意并设置为假人.Visible = score.StartsWith("查");
                cmsCHSH.Show(MousePosition);
            }
        }

        private void btn手动封盘_Click(object sender, EventArgs e)
        {
            if (RobotClient.CurrentResult.Status < ResultStatus.已封盘)
            {
                if (MessageBox.Show("确定要手动封盘么？", "确认对话框",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    BettingUtil.ProcessClosing();
                    RobotClient.CurrentResult.Status = ResultStatus.已封盘;
                    RobotClient.SendConfigMessage(Config.GetInstance().编辑框_消息_封盘);
                    MessageBox.Show("已手动封盘");
                    OplogDao.Add("执行了手动猜停");
                }
            }
            else
            {
                MessageBox.Show("当前期已封盘，无需手动封盘");
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            #region 启动时加载初始化数据         

            //加载本地今日开奖结果
            List<Result> todayResults = ResultDao.GetResult(RobotClient.CurrentLotterCode);
            foreach (Result result in todayResults)
            {
                RobotClient.ResultList.Add(result);
            }

            // 修复：确保CurrentResult指向正确的当前进行期次
            // 查找状态为"竞猜中"或"已封盘"的期次作为当前期次
            Result currentPeriod = todayResults.FirstOrDefault(r => r.Status == ResultStatus.竞猜中 || r.Status == ResultStatus.已封盘 || r.Status == ResultStatus.等待开奖);
            if (currentPeriod != null)
            {
                // 将当前期次移动到ResultList的第一位
                RobotClient.ResultList.Remove(currentPeriod);
                RobotClient.ResultList.Insert(0, currentPeriod);
                LogUtil.Log($"修复注单加载：设置当前期次为 {currentPeriod.Issue}期，状态：{currentPeriod.Status}");
            }
            else
            {
                LogUtil.Log("警告：未找到当前进行的期次，注单数据可能无法正确加载");
            }

            //加载玩家列表（此时CurrentResult已指向正确的期次）
            foreach (User user in UserDao.GetUsers())
            {
                RobotClient.UserList.Add(user);
                // 修复积分冻结不一致问题
                user.FixFrozenJifenInconsistency();
            }
            SortDataGridView(ListSortDirection.Descending);

            //加载未处理查回
            foreach (Score score in ScoreDao.GetScoreList(0))
            {
                RobotClient.ScoreList.Add(score);
            }

            Config.GetInstance().ReloadConfig();

            rtb使用协议.LoadFile(AppContext.BaseDirectory + @"Document\使用协议.rtf");
            rtb占位字符.LoadFile(AppContext.BaseDirectory + @"Document\占位字符.rtf");

            tssl机器人ID.Text = RobotClient.Robot.LoginName;
            tssl监控群ID.Text = RobotClient.Robot.DefauleGroup.GroupId;
            tssl动态提示.Text = "登录成功，请启动游戏！";
            #endregion

            #region 版本更新时检测，处理数据库
            //数据库更新
            if (long.Parse(VersionDao.GetVersion()) < long.Parse("2411172119"))
            {
                MessageBox.Show("数据该版本已废弃。如新安装，请删除目录下" + RobotClient.Robot.DefauleGroup.GroupId + ".db文件后打开重新测试！");
                this.Close();
            }

            if (VersionDao.GetVersion() == "2411302155")
            {
                DBHelperSQLite.ExecuteSql(@"
                    ALTER TABLE t_record ADD COLUMN calc_time text;
                    update t_version set version='2504141215';
                ");
            }

            #endregion

            #region 权限码处理
            //权限码
            string permissionCode = ReadWriteINI.ConfigFile_GetVal("激活信息", "权限码");
            if (permissionCode == null)
            {
                permissionCode = RSAUtil.PrivateKeyEncrypt(IMConstant.PERMISSION_PRIVATE_KEY, "1111111123456");
                ReadWriteINI.ConfigFile_SetVal("激活信息", "权限码", permissionCode);
            }
            //string permData = RSAUtil.PublicKeyDecrypt(IMConstant.PERMISSION_PUBLIC_KEY, permissionCode);
            string permData = permissionCode;
            PermissionForm.修改用户积分权限 = permData.Substring(0, 1) == "1";
            PermissionForm.修改用户下注权限 = permData.Substring(1, 1) == "1";
            PermissionForm.修改用户假人权限 = permData.Substring(2, 1) == "1";
            PermissionForm.查看操作记录权限 = permData.Substring(3, 1) == "1";
            PermissionForm.修改游戏赔率权限 = permData.Substring(4, 1) == "1";
            PermissionForm.修改游戏规则权限 = permData.Substring(5, 1) == "1";
            PermissionForm.修改信息设置权限 = permData.Substring(6, 1) == "1";
            PermissionForm.PASSWORD = permData.Substring(7);
            #endregion


            LoadConfig(tabMain);
            //加载时启动消息为空视为帐号未初始化，保存默认设置
            if (string.IsNullOrEmpty(Config.GetInstance().编辑框_消息_启动))
            {
                SaveConfig(tabMain);
                Config.GetInstance().ReloadConfig();
            }

            //回复关键词加载
            string customKeyword = ReadWriteINI.ConfigFile_GetVal("关键词", "自定义关键词");
            if (string.IsNullOrEmpty(customKeyword) || customKeyword == "[]")
            {
                dtCustomKeyword = new DataTable();
                dtCustomKeyword.Columns.Add("关键词", typeof(string));
                dtCustomKeyword.Columns.Add("回复内容", typeof(string));
                dtCustomKeyword.Columns.Add("提示声音", typeof(string));
                dtCustomKeyword.Columns.Add("匹配模式", typeof(string));
            }
            else
            {
                dtCustomKeyword = customKeyword.ToEntity<DataTable>();
            }
            dgv回复关键词.DataSource = dtCustomKeyword;

            #region 数据安全和备份任务
            // 每5分钟进行一次数据同步，防止数据丢失
            Task.Run(() =>
            {
                do
                {
                    try
                    {
                        Thread.Sleep(1000 * 60 * 5); // 每5分钟同步一次
                        DBHelperSQLite.PeriodicDataSync();
                    }
                    catch (Exception ex)
                    {
                        LogUtil.LogEx(ex);
                    }
                } while (true);
            });
            
            // 自动保存所有用户数据任务 - 每2分钟执行一次
            Task.Run(() =>
            {
                do
                {
                    try
                    {
                        Thread.Sleep(1000 * 60 * 2); // 每2分钟保存一次
                        SaveAllUserData();
                        LogUtil.Log("自动保存用户数据完成");
                    }
                    catch (Exception ex)
                    {
                        LogUtil.LogEx(ex);
                    }
                } while (true);
            });
            
            // 原有的备份任务
            Task.Run(() =>
            {
                do
                {
                    try
                    {
                        Thread.Sleep(1000 * 60 * Config.GetInstance().编辑框_备份间隔);
                        
                        // 先同步数据，再备份
                        DBHelperSQLite.FlushWalToMainDB();
                        DBHelperSQLite.BackUpDateBase();
                        
                        LogUtil.Log("定期备份完成");
                    }
                    catch (Exception ex)
                    {
                        LogUtil.LogEx(ex);
                    }
                } while (true);
            });
            #endregion
        }

        private void btn搜_Click(object sender, EventArgs e)
        {
            InputBoxValidation validation = delegate (string val)
            {
                if (val == "")
                    return "帐单名不能为空";
                return "";
            };

            string value = "";
            if (InputBox.Show("搜索", "搜索帐单名的显示位置", ref value, validation) == DialogResult.OK)
            {
                foreach (DataGridViewRow row in dgv玩家列表.Rows)
                {
                    if (row.Cells["昵称"].Value.ToString().Contains(value))
                    {
                        row.Selected = true;
                        dgv玩家列表.CurrentCell = row.Cells["昵称"];
                        break;
                    }
                }
            }
        }

        private void btn查_Click(object sender, EventArgs e)
        {
            InputBoxValidation validation = delegate (string val)
            {
                if (val == "")
                    return "帐单名不能为空";
                return "";
            };

            string value = "";
            if (InputBox.Show("查询", "查询帐单名是否存在", ref value, validation) == DialogResult.OK)
            {
                foreach (DataGridViewRow row in dgv玩家列表.Rows)
                {
                    if (row.Cells["昵称"].Value.ToString() == value)
                    {
                        MessageBox.Show(value + " 已存在！");
                        return;
                    }
                }
                MessageBox.Show(value + " 不存在！");
            }
        }

        private void btn总_Click(object sender, EventArgs e)
        {
            string zjf = string.Format("当前帐单总积分:{0}", RobotClient.UserList.Where(u => u.IsDummy == false).Sum(u => u.Jifen));
            tssl动态提示.Text = zjf;
            MessageBox.Show(zjf);
        }

        private bool jifenDesc = true;
        private void btn排_Click(object sender, EventArgs e)
        {
            if (jifenDesc)
            {
                SortDataGridView(ListSortDirection.Descending);
            }
            else
            {
                SortDataGridView(ListSortDirection.Ascending);
            }
            jifenDesc = !jifenDesc;
        }

        /// <summary>
        /// 玩家列表按积分排序
        /// </summary>
        /// <param name="sortOrder"></param>
        private void SortDataGridView(ListSortDirection sortOrder)
        {
            var sortedList = (sortOrder == ListSortDirection.Descending)
                ? RobotClient.UserList.OrderBy(x => x.IsDummy).ThenByDescending(x => x.Jifen)
                : RobotClient.UserList.OrderBy(x => x.IsDummy).ThenBy(x => x.Jifen);
            RobotClient.UserList = new BindingList<User>();
            foreach (var user in sortedList)
            {
                RobotClient.UserList.Add(user);
            }
            userBindingSource.DataSource = RobotClient.UserList;
            dgv玩家列表.DataSource = userBindingSource;
        }

        private void btn导_Click(object sender, EventArgs e)
        {
            string file = ExcelUtil.ExportDataGridViewToExcel(dgv玩家列表, "玩家列表", true);
            if (file != null)
            {
                if (MessageBox.Show("成功导出文件：\n" + file + "\n是否现在打开所在文件夹！", "确认对话框",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    try
                    {
                        ExcelUtil.OpenFileFolder(file);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void btn清除所有下注_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("清除本期所有下注，是否继续？", "确认对话框",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                foreach (User user in RobotClient.UserList)
                {
                    if (user.RecordList.Count > 0)
                    {
                        int blzf = user.Blzf;
                        user.ClearBetRecords("客服清除所有下注");
                        if (RobotClient.CurrentResult.Status >= ResultStatus.已封盘)
                        {
                            user.ChangeJifen(blzf, RecordType.清除下注);
                        }
                    }
                }
                Result currentResult = RobotClient.CurrentResult;
                currentResult.Amount = 0;
                ResultDao.Save(currentResult);
                OplogDao.Add("清除了[" + currentResult.Issue + "]期所有下注");
                MessageBox.Show("清除成功！");
            }
        }

        private void tsbPermission_Click(object sender, EventArgs e)
        {
            PermissionForm permissionForm = new PermissionForm();
            permissionForm.ShowDialog();
        }

        private void btn内容核对_Click(object sender, EventArgs e)
        {
            if (RobotClient.CurrentResult.Status == ResultStatus.竞猜中)
            {
                MessageBox.Show("封盘后才能发送核对帐单");
                return;
            }
            RobotClient.SendConfigMessage(Config.GetInstance().编辑框_消息_封盘后);
            MessageBox.Show("发送成功！");
        }

        private void btn填入开奖_Click(object sender, EventArgs e)
        {
            ManualOpenForm mof = new ManualOpenForm();
            DialogResult dr = mof.ShowDialog();
            if (dr == DialogResult.OK)
            {
                if (选择框_实时排序.Checked)
                {
                    SortDataGridView(ListSortDirection.Descending);
                }
            }
        }

        private void btnAdTest1_Click(object sender, EventArgs e)
        {
            RobotClient.SendMessage(Config.GetInstance().编辑框_消息_广告1);
        }

        private void btnAdTest2_Click(object sender, EventArgs e)
        {
            RobotClient.SendMessage(Config.GetInstance().编辑框_消息_广告2);
        }

        private void btnAdTest3_Click(object sender, EventArgs e)
        {
            RobotClient.SendMessage(Config.GetInstance().编辑框_消息_广告3);
        }

        private void btnAdTest4_Click(object sender, EventArgs e)
        {
            RobotClient.SendMessage(Config.GetInstance().编辑框_消息_广告4);
        }

        private void btn发布积分列表_Click(object sender, EventArgs e)
        {
            RobotClient.SendConfigMessage(Config.GetInstance().编辑框_消息_账单);
        }

        private void 选择框_今日总输赢_CheckedChanged(object sender, EventArgs e)
        {
            if (选择框_今日总输赢.Checked)
            {
                选择框_今日总输赢.Text = "今日总输赢:" + BettingDao.GetTodayWinOrLose().ToString();
            }
            else
            {
                选择框_今日总输赢.Text = "今日总输赢:******";
            }
        }

        private void 新增虚拟玩家_Click(object sender, EventArgs e)
        {
            InputBoxValidation validation = delegate (string val)
            {
                if (val == "")
                    return "虚拟玩家ID不能为空";
                return "";
            };

            string value = "";
            if (InputBox.Show("新增虚拟玩家", "请输入虚拟玩家ID", ref value, validation) == DialogResult.OK)
            {
                var matchUsers = RobotClient.Robot.DefauleGroup.GroupMembers.Where(m => m.UserId == value);
                if (matchUsers.Count() > 0)
                {
                    var member = matchUsers.First();
                    User user = new User(member.UserId, member.NickName, true);
                    UserDao.AddUser(user);
                    RobotClient.UserList.Add(user);
                    MessageBox.Show("新增成功");
                }
                else
                {
                    MessageBox.Show("新增失败,用户" + value + "不在群中。");
                }
            }
        }

        private void dgv玩家列表_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (Config.GetInstance().选择框_显示颜色)
            {
                var row = dgv玩家列表.Rows[e.RowIndex];

                if (((int)row.Cells["积分"].Value) < Config.GetInstance().编辑框_限额_最低)
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(Config.GetInstance().颜色_积分不足);
                }
                else
                {
                    if ((bool)row.Cells["是否假人"].Value)
                    {
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(Config.GetInstance().颜色_假人);
                    }
                    else
                    {
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(Config.GetInstance().颜色_真人);

                    }
                }
            }
        }

        private void chk假人_Click(object sender, EventArgs e)
        {
            if (Config.GetInstance().CheckPermission(PermissionForm.修改用户假人权限))
            {
                ((User)userBindingSource.Current).IsDummy = chk假人.Checked;
            }
        }

        private void 查询竞猜明细_Click(object sender, EventArgs e)
        {
            DataGridViewRow dgvr = dgv玩家列表.CurrentRow;
            if (dgvr == null)
            {
                MessageBox.Show("请选择要操作的玩家");
                return;
            }
            string code = dgvr.Cells["Code"].Value.ToString();
            User user = UserDao.GetUser(code);
            GameAssistantForm form = GameAssistantForm.GetInstance();
            form.ActivePage = "玩家猜猜明细";
            form.NickName = user.NickName;
            form.ShowOrActive();
        }

        private void 查询查回记录_Click(object sender, EventArgs e)
        {
            DataGridViewRow dgvr = dgv玩家列表.CurrentRow;
            if (dgvr == null)
            {
                MessageBox.Show("请选择要操作的玩家");
                return;
            }
            string code = dgvr.Cells["Code"].Value.ToString();
            User user = UserDao.GetUser(code);
            GameAssistantForm form = GameAssistantForm.GetInstance();
            form.ActivePage = "查回统计";
            form.NickName = user.NickName;
            form.ShowOrActive();
        }

        private void 查询帐变记录_Click(object sender, EventArgs e)
        {
            DataGridViewRow dgvr = dgv玩家列表.CurrentRow;
            if (dgvr == null)
            {
                MessageBox.Show("请选择要操作的玩家");
                return;
            }
            string code = dgvr.Cells["Code"].Value.ToString();
            User user = UserDao.GetUser(code);
            GameAssistantForm form = GameAssistantForm.GetInstance();
            form.ActivePage = "帐变记录";
            form.NickName = user.NickName;
            form.ShowOrActive();
        }

        private void dgv玩家列表_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0)
            {
                return;
            }
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                dgv玩家列表.ClearSelection();
                dgv玩家列表.Rows[e.RowIndex].Selected = true;
                dgv玩家列表.CurrentCell = dgv玩家列表.Rows[e.RowIndex].Cells[e.ColumnIndex];
                DataGridViewRow dgvr = dgv玩家列表.Rows[e.RowIndex];
                double runningWaterScale = (double)dgvr.Cells["流水比例"].Value;
                if (runningWaterScale != 0)
                {
                    设置流水比例.Text = "设置流水比例[" + runningWaterScale + "]";
                }
                else
                {
                    设置流水比例.Text = "设置流水比例";
                }
                double returnWaterScale = (double)dgvr.Cells["回水比例"].Value;
                if (returnWaterScale != 0)
                {
                    设置回水比例.Text = "设置回水比例[" + returnWaterScale + "]";
                }
                else
                {
                    设置回水比例.Text = "设置回水比例";
                }
                玩家列表右键.Show(MousePosition);
            }
        }

        private void 命令设置_Enter(object sender, EventArgs e)
        {
            //获取所有声音文件
            cmb提示声音.DataSource = SoundUtil.GetAllSoundFileName(true);
        }

        private void dgv回复关键词_SelectionChanged(object sender, EventArgs e)
        {
            DataGridViewRow dgvr = dgv回复关键词.CurrentRow;
            if (dgvr == null) { return; }
            txt关键词.Text = dgvr.Cells[0].Value.ToString();
            rtb回复内容.Text = dgvr.Cells[1].Value.ToString();
            cmb提示声音.Text = dgvr.Cells[2].Value.ToString();
            cmb匹配模式.Text = dgvr.Cells[3].Value.ToString();
        }

        private void btn提示声音_Click(object sender, EventArgs e)
        {
            if (cmb提示声音.Text != "无")
            {
                SoundUtil.Play(cmb提示声音.Text, true);
            }
        }

        private void btn测试备份_Click(object sender, EventArgs e)
        {
            DBHelperSQLite.BackUpDateBase();
        }

        private void 单选框_赔率_组合一_Click(object sender, EventArgs e)
        {
            单选框_赔率_组合一.Checked = true;
            单选框_赔率_组合二.Checked = false;
        }

        private void 单选框_赔率_组合二_Click(object sender, EventArgs e)
        {
            单选框_赔率_组合一.Checked = false;
            单选框_赔率_组合二.Checked = true;
        }

        private void btn播放事件提示音_Click(object sender, EventArgs e)
        {
            SoundUtil.Play(cmb事件提示音.Text, true);
        }

        private void 设置流水比例_Click(object sender, EventArgs e)
        {
            InputBoxValidation validation = delegate (string val)
            {
                if (!string.IsNullOrEmpty(val))
                {
                    if (double.TryParse(val, out double scale))
                    {
                        if (scale < 0 || scale > 50)
                        {
                            return "流水比例最大范围0~0.5";
                        }
                    }
                    else
                    {
                        return "流水比例错误！";
                    }
                }
                else
                {
                    return "流水比例不能为空！";
                }
                return "";
            };
            string value = "";
            if (InputBox.Show("设置流水比例", "请输入自助流水比例,例如设置0.3,流水10000返30", ref value, validation) == DialogResult.OK)
            {
                double scale = 0;
                double.TryParse(value, out scale);
                DataGridViewRow dgvr = dgv玩家列表.CurrentRow;
                string userCode = dgvr.Cells["Code"].Value.ToString();
                User user = RobotClient.GetUser(userCode);
                user.RunningWaterScale = scale;
                UserDao.SaveRunningWaterScale(userCode, scale);
            }
        }

        private void 设置回水比例_Click(object sender, EventArgs e)
        {
            InputBoxValidation validation = delegate (string val)
            {
                if (!string.IsNullOrEmpty(val))
                {
                    if (double.TryParse(val, out double scale))
                    {
                        if (scale < 0 || scale > 50)
                        {
                            return "回水比例最大范围0~0.5";
                        }
                    }
                    else
                    {
                        return "回水比例错误！";
                    }
                }
                else
                {
                    return "回水比例不能为空！";
                }
                return "";
            };
            string value = "";
            if (InputBox.Show("设置回水比例", "请输入自助回水比例,例如设置0.3,输分10000返30", ref value, validation) == DialogResult.OK)
            {
                double scale = 0;
                double.TryParse(value, out scale);
                DataGridViewRow dgvr = dgv玩家列表.CurrentRow;
                string userCode = dgvr.Cells["Code"].Value.ToString();
                User user = RobotClient.GetUser(userCode);
                user.ReturnWaterScale = scale;
                UserDao.SaveReturnWaterScale(userCode, scale);
            }
        }

        private void 选择框_退分_开启每笔自动流水_CheckedChanged(object sender, EventArgs e)
        {
            if (选择框_退分_开启每笔自动流水.Checked)
            {
                选择框_退分_盈利流水次数.Checked = false;
                选择框_退分_每日流水次数.Checked = false;
                选择框_退分_流水不显示累计返水.Checked = false;
                选择框_退分_流水最低返水金额.Checked = false;
                选择框_退分_流水最低流水占比.Checked = false;
                选择框_退分_流水最低投注期数.Checked = false;
                选择框_退分_盈利流水次数.Enabled = false;
                选择框_退分_每日流水次数.Enabled = false;
                选择框_退分_流水不显示累计返水.Enabled = false;
                选择框_退分_流水最低返水金额.Enabled = false;
                选择框_退分_流水最低流水占比.Enabled = false;
                选择框_退分_流水最低投注期数.Enabled = false;
            }
            else
            {
                选择框_退分_盈利流水次数.Enabled = true;
                选择框_退分_每日流水次数.Enabled = true;
                选择框_退分_流水不显示累计返水.Enabled = true;
                选择框_退分_流水最低返水金额.Enabled = true;
                选择框_退分_流水最低流水占比.Enabled = true;
                选择框_退分_流水最低投注期数.Enabled = true;
            }
        }

        private void 选择框_退分_开启每笔自动回水_CheckedChanged(object sender, EventArgs e)
        {
            if (选择框_退分_开启每笔自动回水.Checked)
            {
                选择框_退分_每日回水次数.Checked = false;
                选择框_退分_回水不显示累计返水.Checked = false;
                选择框_退分_回水最低返水金额.Checked = false;
                选择框_退分_回水最低流水占比.Checked = false;
                选择框_退分_回水最低投注期数.Checked = false;
                选择框_退分_回水用户输分.Checked = false;
                选择框_退分_每日回水次数.Enabled = false;
                选择框_退分_回水不显示累计返水.Enabled = false;
                选择框_退分_回水最低返水金额.Enabled = false;
                选择框_退分_回水最低流水占比.Enabled = false;
                选择框_退分_回水最低投注期数.Enabled = false;
                选择框_退分_回水用户输分.Enabled = false;
            }
            else
            {
                选择框_退分_每日回水次数.Enabled = true;
                选择框_退分_回水不显示累计返水.Enabled = true;
                选择框_退分_回水最低返水金额.Enabled = true;
                选择框_退分_回水最低流水占比.Enabled = true;
                选择框_退分_回水最低投注期数.Enabled = true;
                选择框_退分_回水用户输分.Enabled = true;
            }
        }

        private void dgv玩家列表_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            e.Row.HeaderCell.Value = string.Format("{0}", e.Row.Index + 1);
        }

        private void dgv玩家列表_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // 如果错误类型是由于尝试设置无效值导致的
            if (e.Exception != null && e.Exception.GetType() == typeof(FormatException))
            {
                MessageBox.Show("输入的值格式不正确，请检查输入值！");
                // 取消编辑状态
                dgv玩家列表.CancelEdit();
            }
        }

        private void tspAit_Click(object sender, EventArgs e)
        {
            DummyApp.MainForm dummyMain = DummyApp.MainForm.GetInstance();
            dummyMain.SetGroupInfo(RobotClient.Robot);
            dummyMain.Show();
        }

        private async Task UpdateUIAsync(string message)
        {
            await Task.Run(() =>
            {
                // 这里可以执行一些不需要UI的异步操作
            }).ContinueWith(_ =>
            {
                // 回到UI线程  
                this.Text = message;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void btn获取开奖数据_Click(object sender, EventArgs e)
        {
            LoadTodayResult();
            MessageBox.Show("数据更新成功");
        }

        private void LoadTodayResult()
        {
            btn获取开奖数据.Enabled = false;
            foreach (Result result in RobotClient.Get今日开奖结果())
            {
                Result r = RobotClient.ResultList.FirstOrDefault(r => r.Issue == result.Issue);
                if (r == null)
                {
                    RobotClient.ResultList.Add(result);
                }
                else
                {
                    r.SetResult(result);
                }
            }
            var resultList = RobotClient.ResultList.OrderByDescending(x => x.Issue);
            resultBindingSource.DataSource = resultList;
            btn获取开奖数据.Enabled = true;
        }

        private void tsbPassword_Click(object sender, EventArgs e)
        {
            UpdatePasswordForm upf = new UpdatePasswordForm();
            upf.ShowDialog();
        }

        private void btn自动创建假人_Click(object sender, EventArgs e)
        {
            List<Dummy> list = CentralApi.GetUserConfig("dummy.list")?.configValue.ToEntityList<Dummy>();
            List<string> nameList = new List<string>();
            String errInfo = "";
            if (list != null)
            {
                foreach (Dummy dummy in list)
                {
                    try
                    {
                        if (dummy.CreateToken(dummy.LoginName, dummy.Password))
                        {
                            if (RobotClient.GetUser(dummy.UserId) == null)
                            {
                                User user = new User(dummy.UserId, dummy.BillName, true);
                                user.Status = UserStatus.正常用户;
                                UserDao.AddUser(user);
                                RobotClient.UserList.Add(user);
                                nameList.Add(user.NickName);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        errInfo +=$"{dummy.BillName}：{ex.Message}\n";
                        continue;
                    }
            }
            }
            if (nameList.Count > 0)
            {
                String message = "自动创建成功，新增假人如下：\n" + string.Join(",", nameList);
                if (errInfo != "")
                {
                    message += "\n\n错误信息：\n" + errInfo;
                }
                MessageBox.Show(message);
            }
            else
            {
                String message = "执行成功,但无任何假人新增！";
                if (errInfo != "")
                {
                    message += "\n\n错误信息：\n" + errInfo;
                }
                MessageBox.Show(message);
            }
            MessageBox.Show("请重新启动托程序，否则托将无法正常工作！");
        }

        private void btn清除历史数据_Click(object sender, EventArgs e)
        {
            Hashtable SQLStringList = new Hashtable();
            int days = Convert.ToInt32(nud保留天数.Value);
            SQLStringList.Add($"delete from t_bet_record where result_id in (select id from t_result where open_time < DATE('now', '-{days} days'))", new SQLiteParameter[] { });
            SQLStringList.Add($"delete from t_bettings where result_id in (select id from t_result where open_time < DATE('now', '-{days} days'))", new SQLiteParameter[] { });
            SQLStringList.Add($"delete from t_message where msg_time < DATE('now', '-{days} days')", new SQLiteParameter[] { });
            SQLStringList.Add($"delete from t_oplog where create_time < DATE('now', '-{days} days')", new SQLiteParameter[] { });
            SQLStringList.Add($"delete from t_record where create_time < DATE('now', '-{days} days')", new SQLiteParameter[] { });
            SQLStringList.Add($"delete from t_result where open_time < DATE('now', '-{days} days')", new SQLiteParameter[] { });
            SQLStringList.Add($"delete from t_score where create_time < DATE('now', '-{days} days')", new SQLiteParameter[] { });
            try
            {
                DBHelperSQLite.ExecuteSqlTran(SQLStringList);
            }
            catch
            {
                MessageBox.Show("清除失败");
                return;
            }

            //加载本地今日开奖结果
            RobotClient.ResultList.Clear();
            foreach (Result result in ResultDao.GetResult(RobotClient.CurrentLotterCode))
            {
                RobotClient.ResultList.Add(result);
            }
            //加载未处理查回
            RobotClient.ScoreList.Clear();
            foreach (Score score in ScoreDao.GetScoreList(0))
            {
                RobotClient.ScoreList.Add(score);
            }
            MessageBox.Show("清除成功");            
        }
    }
}
