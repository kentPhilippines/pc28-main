using CommonLibrary;
using RobotApp.Dao;
using RobotApp.IM;
using RobotApp.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Message = RobotApp.Model.Message;

//using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace RobotApp.Util
{
    [SupportedOSPlatform("windows")]
    internal class BettingUtil
    {        
        /// <summary>
        /// 处理下注消息
        /// </summary>
        /// <param name="msg"></param>
        /// <exception cref="Exception"></exception>
        internal static List<BetRecord> ProcessMessage(User user, Message msg, bool isTry = false)
        {            
            string invalidStr = string.Empty;
            List<BetRecord> recordList = new List<BetRecord>();
            string msgStr = msg.Msg;
            if (msgStr.StartsWith("改"))
            {
                msgStr = msgStr.Substring(1);
            }
            string[] msgs = Regex.Split(msgStr, "\\s");
            StringComparison stringComparison = Config.GetInstance().选择框_关键词_区分大小写
                            ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            RegexOptions regexOptions = Config.GetInstance().选择框_关键词_区分大小写
                ? RegexOptions.None : RegexOptions.IgnoreCase;
            foreach (var str in msgs)
            {
                //单点
                if (Regex.IsMatch(str, @"^\d+\D+\d+$"))
                {
                    foreach (var dd in Config.GetInstance().编辑框_关键词_单点.Split('|')) {
                        int keyIndex = str.IndexOf(dd, stringComparison);
                        if (keyIndex > 0)
                        {
                            string keyStr = str.Substring(0, keyIndex);
                            string amountStr = str.Substring(keyIndex + dd.Length);
                            if (int.TryParse(keyStr, out int num))
                            {
                                if (num > 27 || num < 0)
                                {
                                    invalidStr += str + " ";
                                }
                                else
                                {
                                    if (int.TryParse(amountStr, out int amount))
                                    {
                                        BetRecord record = new BetRecord();
                                        if (!isTry) { 
                                            record.ResultId = RobotClient.CurrentResult.Id;
                                            record.UserCode = user.UserId;
                                        }
                                        record.BetType = BetType.SINGLE;
                                        record.Keyword = num.ToString();
                                        record.Amount = amount;
                                        record.Status = BetRecordStatus.已下注;
                                        record.Fp = msg.Fp;
                                        recordList.Add(record);
                                        continue;
                                    }
                                }                                
                            }                          
                        }
                    }               
                }
                else
                {
                    //关键词
                    foreach (Match match in Regex.Matches(str, @"(\D+\d+)|(\d+\D+)"))
                    {
                        string amountStr = Regex.Replace(match.Value, @"\D+", "");
                        string keyword = Regex.Replace(match.Value, @"\d+", "");
                        if (!Config.GetInstance().选择框_关键词_区分大小写)
                        {
                            keyword = keyword.ToUpper();
                        }
                        if (int.TryParse(amountStr, out int amount))
                        {
                            if (Config.GetInstance().KeyPairs.TryGetValue(keyword, out keyword))
                            {
                                BetRecord record = new BetRecord();
                                if (!isTry)
                                {
                                    record.ResultId = RobotClient.CurrentResult.Id;
                                    record.UserCode = user.UserId;
                                }
                                record.BetType = BetType.KEYWORDS;
                                record.Keyword = keyword;
                                record.Amount = amount;
                                record.Status = BetRecordStatus.已下注;
                                record.Fp = msg.Fp;
                                recordList.Add(record);
                            }
                            else
                            {
                                invalidStr += str + " ";
                            }
                        }
                    }                    
                }
                if (invalidStr.TrimEnd() != "")
                {
                    RobotClient.SendMessage(invalidStr + "指令无效，其它指令继续有效！");
                }
            }
            return recordList;
        }

        /// <summary>
        /// 处理游戏规则
        /// </summary>
        /// <param name="recordList"></param>
        internal static string ProcessRule(List<BetRecord> recordList)
        {
            double sum大小单双 = recordList.Sum(b => new string[] { "大", "小", "单", "双" }.Contains(b.Keyword) ? b.Amount : 0);
            double sum组合 = recordList.Sum(b => new string[] { "大单", "小单", "大双", "小双" }.Contains(b.Keyword) ? b.Amount : 0);
            double sum极大极小 = recordList.Sum(b => new string[] { "极大", "极小" }.Contains(b.Keyword) ? b.Amount : 0);
            double sum豹子 = recordList.Sum(b => b.Keyword == "豹子" ? b.Amount : 0);
            double sum顺子 = recordList.Sum(b => b.Keyword == "顺子" ? b.Amount : 0);
            double sum对子 = recordList.Sum(b => b.Keyword == "对子" ? b.Amount : 0);
            double sum单点总和 = recordList.Sum(b => int.TryParse(b.Keyword, out int num) ? b.Amount : 0);
            double sum0 = recordList.Sum(b => b.Keyword == "0" ? b.Amount : 0);
            double sum1 = recordList.Sum(b => b.Keyword == "1" ? b.Amount : 0);
            double sum2 = recordList.Sum(b => b.Keyword == "2" ? b.Amount : 0);
            double sum3 = recordList.Sum(b => b.Keyword == "3" ? b.Amount : 0);
            double sum4 = recordList.Sum(b => b.Keyword == "4" ? b.Amount : 0);
            double sum5 = recordList.Sum(b => b.Keyword == "5" ? b.Amount : 0);
            double sum6 = recordList.Sum(b => b.Keyword == "6" ? b.Amount : 0);
            double sum7 = recordList.Sum(b => b.Keyword == "7" ? b.Amount : 0);
            double sum8 = recordList.Sum(b => b.Keyword == "8" ? b.Amount : 0);
            double sum9 = recordList.Sum(b => b.Keyword == "9" ? b.Amount : 0);
            HashSet<string> numSet = new HashSet<string>();
            foreach (BetRecord b in recordList)
            {
                if (Config.GetInstance().选择框_限额_最低)
                {
                    if (b.Amount < Config.GetInstance().编辑框_限额_最低)
                    {
                        return "单注最低" + Config.GetInstance().编辑框_限额_最低 + "起";
                    }
                }

                if (StringUtil.IsNumeric(b.Keyword))
                {
                    numSet.Add(b.Keyword);
                }              
            }

            //单点限额
            foreach(String numberKeyword in numSet)
            {
                double sumSingleNum = recordList.Sum(item => item.Keyword == numberKeyword ? item.Amount : 0);
                //单点限额
                if (Config.GetInstance().选择框_限额_最高_单点)
                {
                    if (sumSingleNum > Config.GetInstance().编辑框_限额_最高_单点)
                    {
                        return "单点最高限额" + Config.GetInstance().编辑框_限额_最高_单点;
                    }
                }

                //单点单独计算限额
                if (Config.GetInstance().选择框_限额_单点单独计算)
                {
                    if (Config.GetInstance().SingleMaxBet.TryGetValue(numberKeyword, out double maxAmount))
                    {
                        if (sumSingleNum > maxAmount)
                        {
                            return "单点" + numberKeyword + "最高限额" + maxAmount;
                        }
                    }
                }
            }

            //单点总注计算限额
            if (Config.GetInstance().选择框_限额_单点总注计算)
            {
                if (sum单点总和 > Config.GetInstance().编辑框_限额_最高_单点)
                {
                    return "单点总注最高限额" + Config.GetInstance().编辑框_限额_最高_单点;
                }
            }

            //大小单双限额
            if (Config.GetInstance().选择框_限额_最高_大小单双)
            {
                if (sum大小单双 > Config.GetInstance().编辑框_限额_最高_大小单双)
                {
                    return "大小单双最高限额" + Config.GetInstance().编辑框_限额_最高_大小单双;
                }
            }

            //组合
            if (Config.GetInstance().选择框_限额_最高_组合)
            {
                if (sum组合 > Config.GetInstance().编辑框_限额_最高_组合)
                {
                    return "组合最高限额" + Config.GetInstance().编辑框_限额_最高_组合;
                }
            }

            //极大极小
            if (Config.GetInstance().选择框_限额_最高_极值)
            {
                if (sum极大极小 > Config.GetInstance().编辑框_限额_最高_极值)
                {
                    return "极值最高限额" + Config.GetInstance().编辑框_限额_最高_极值;
                }
            }

            //豹对顺
            if (Config.GetInstance().选择框_限额_最高_豹子)
            {
                if (sum豹子 > Config.GetInstance().编辑框_限额_最高_豹子)
                {
                    return "豹子最高限额" + Config.GetInstance().编辑框_限额_最高_豹子;
                }
            }
            if (Config.GetInstance().选择框_限额_最高_对子)
            {
                if (sum顺子 > Config.GetInstance().编辑框_限额_最高_对子)
                {
                    return "顺子最高限额" + Config.GetInstance().编辑框_限额_最高_对子;
                }
            }
            if (Config.GetInstance().选择框_限额_最高_顺子)
            {
                if (sum对子 > Config.GetInstance().编辑框_限额_最高_顺子)
                {
                    return "对子最高限额" + Config.GetInstance().编辑框_限额_最高_顺子;
                }
            }
            
            //总注封顶
            if (Config.GetInstance().选择框_限额_最高_总注)
            {
                double sum = recordList.Sum(r => r.Amount);
                if(sum > Config.GetInstance().编辑框_限额_最高_总注)
                {
                    return "总注封顶:" + Config.GetInstance().编辑框_限额_最高_总注;
                }
            }

            //单点单期最多个数            
            if (Config.GetInstance().选择框_数字_限制个数)
            {
                if (numSet.Count > Config.GetInstance().编辑框_数字_限制个数)
                {
                    return "单点数字每期最多" + Config.GetInstance().编辑框_数字_限制个数+"个";
                }
            }
            
            return "";
        }

        /// <summary>
        /// 处理杀组合规则
        /// </summary>
        /// <param name="user"></param>
        /// <returns>是否通过杀组合规则</returns>
        internal static bool ProcessShaRule(User user)
        {
            if (Config.GetInstance().选择框_禁止_杀组合 && user.Is杀组合())
            {
                bool flag = true;
                if (Config.GetInstance().选择框_例外_杀组合)
                {                   
                    if (user.Sum大小单双组合 > Config.GetInstance().编辑框_例外_杀组合)
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    user.ClearBetRecords("禁止杀组合");
                    user.ReplayMessage("禁止杀组合,本期竞猜全部无效,请重新竞猜。");
                    return false;
                }
            }

            if (Config.GetInstance().选择框_禁止_反向组合)
            {
                if ((user.Count大单 > 0 && user.Count小单 > 0) || (user.Count大双 > 0 && user.Count小双 > 0))
                {
                    bool flag = true;
                    if (Config.GetInstance().选择框_例外_反向组合)
                    {
                        if (user.Sum组合 > Config.GetInstance().编辑框_例外_反向组合)
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        user.ClearBetRecords("禁止反向组合");
                        user.ReplayMessage("禁止反向组合,本期竞猜全部无效,请重新竞猜。");
                        return false;
                    }
                }
            }

            if (Config.GetInstance().选择框_禁止_同向组合)
            {
                if ((user.Count大单 > 0 && user.Count大双 > 0) || (user.Count小单 > 0 && user.Count小双 > 0))
                {
                    bool flag = true;
                    if (Config.GetInstance().选择框_例外_同向组合)
                    {
                        if (user.Sum组合 > Config.GetInstance().编辑框_例外_同向组合)
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        user.ClearBetRecords("禁止同向组合");
                        user.ReplayMessage("禁止同向组合,本期竞猜全部无效,请重新竞猜。");
                        return false;
                    }
                }                
            }

            if(Config.GetInstance().选择框_禁止_大小单双反向)
            {
                if ((user.Count大 > 0 && user.Count小 > 0) || (user.Count单 > 0 && user.Count双 > 0))
                {
                    bool flag = true;
                    if (Config.GetInstance().选择框_例外_大小单双反向)
                    {
                        if (user.Sum大小单双 > Config.GetInstance().编辑框_例外_大小单双反向)
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        user.ClearBetRecords("禁止大小单双反向");
                        user.ReplayMessage("禁止大小单双反向,本期竞猜全部无效,请重新竞猜。");
                        return false;
                    }                    
                }                
            }

            if (Config.GetInstance().选择框_禁止_四门组合 && user.Is四门组合())
            {
                user.ClearBetRecords("禁止四门组合");
                user.ReplayMessage("禁止四门组合,本期竞猜全部无效,请重新竞猜。");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 开奖结算
        /// </summary>
        internal static void ProcessSettlement(Result result, List<User> userList)
        {         
            foreach (User user in userList)
            {              
                if (user.RecordList.Count > 0)
                {
                    BettingUtil.CalcOdds(user.RecordList, result);
                    BettingUtil.ProcessOpenResult(user.RecordList, result);

                    BettingDao.AddBetting(result, user);
                    //中奖了派奖
                    if (user.Award > 0)
                    {                        
                        user.ChangeJifen(user.Award, RecordType.派奖);

                        //中奖后，如有回水需自动退还
                        if (!Config.GetInstance().单选框_退分_回水关闭)
                        {
                            double scale = Config.GetInstance().编辑框_退分_回水统一比例;
                            if (Config.GetInstance().单选框_退分_回水单独设置)
                            {
                                scale = user.ReturnWaterScale;
                            }
                            if (user.Blyk > 0)
                            {
                                int returnWater = (int)(user.Blyk * scale * 0.01);
                                int todayReturnWater = BettingDao.TodayReturnWater(user.UserId);
                                if (todayReturnWater > 0)
                                {
                                    if (todayReturnWater < returnWater)
                                    {
                                        returnWater = todayReturnWater;
                                    }

                                    user.ChangeJifen(returnWater, RecordType.退回剩余自助流水);
                                }
                            }
                        }
                    }                    
                }
                user.Slyk = user.Blyk;
                UserDao.UpdateUser(user);                
            }
            result.Amount = RobotClient.UserList.Where(u => !u.IsDummy).Sum(u => u.Blzf);
            result.Award = RobotClient.UserList.Where(u => !u.IsDummy).Sum(u => u.Award);
            result.Status = ResultStatus.已结算;
            ResultDao.Save(result);
            foreach (User user in userList)
            {
                user.NewResultUser();
            }
        }

        /// <summary>
        /// 获取赔率
        /// </summary>
        /// <param name="recordList"></param>
        /// <param name="keno"></param>
        public static void CalcOdds(List<BetRecord> recordList, Result result)
        {
            double sum大小单双 = recordList.Sum(b => new string[] { "大", "小", "单", "双" }.Contains(b.Keyword) ? b.Amount : 0);
            double sum组合 = recordList.Sum(b => new string[] { "大单", "小单", "大双", "小双" }.Contains(b.Keyword) ? b.Amount : 0);
            foreach (BetRecord b in recordList)
            {
                //单点
                if (StringUtil.IsNumeric(b.Keyword))
                {
                    if (Config.GetInstance().单选框_赔率_单点统一)
                    {
                        b.Odds = Config.GetInstance().编辑框_赔率_定位数字;
                    }

                    if (Config.GetInstance().单选框_赔率_单点单独)
                    {
                        if (Config.GetInstance().SingleOdds.TryGetValue(b.Keyword, out double value))
                        {
                            b.Odds = value;
                        }
                    }

                    if (Config.GetInstance().选择框_数字赔付_最大限额)
                    {
                        b.MaxPay = Config.GetInstance().编辑框_数字赔付_最大限额;
                    }
                }
                else
                {
                    //豹对顺
                    if ("豹子" == b.Keyword)
                    {
                        if (Config.GetInstance().选择框_玩法_豹子)
                        {
                            b.Odds = Config.GetInstance().编辑框_赔率_豹子;
                        }
                    }
                    if ("对子" == b.Keyword)
                    {
                        if (Config.GetInstance().选择框_玩法_对子)
                        {
                            b.Odds = Config.GetInstance().编辑框_赔率_对子;
                        }
                    }
                    if ("顺子" == b.Keyword)
                    {
                        if (Config.GetInstance().选择框_玩法_顺子)
                        {
                            b.Odds = Config.GetInstance().编辑框_赔率_顺子;
                        }
                    }

                    //大小单双
                    if (b.Keyword == "大" || b.Keyword == "小" || b.Keyword == "单" || b.Keyword == "双")
                    {
                        b.Odds = Config.GetInstance().编辑框_赔率_大小单双;
                        if (Config.GetInstance().选择框_特殊赔率_大小单双_1)
                        {
                            if (sum大小单双 > Config.GetInstance().编辑框_大于总注_大小单双_1)
                            {
                                if (result.Sum == 13 || result.Sum == 14) { 
                                    b.Odds = Config.GetInstance().编辑框_特殊赔率_大小单双_1;
                                }
                            }
                        }
                        if (Config.GetInstance().选择框_特殊赔率_大小单双_2)
                        {
                            if (sum大小单双 > Config.GetInstance().编辑框_大于总注_大小单双_2)
                            {                                
                                if (result.Sum == 13 || result.Sum == 14)
                                {
                                    b.Odds = Config.GetInstance().编辑框_特殊赔率_大小单双_2;
                                }
                            }
                        }
                        if (Config.GetInstance().选择框_特殊赔率_大小单双_3)
                        {
                            if (sum大小单双 > Config.GetInstance().编辑框_大于总注_大小单双_3)
                            {                                
                                if (result.Sum == 13 || result.Sum == 14)
                                {
                                    b.Odds = Config.GetInstance().编辑框_特殊赔率_大小单双_3;
                                }
                            }
                        }
                        if (Config.GetInstance().选择框_特殊赔率_大小单双_4)
                        {
                            if (sum大小单双 > Config.GetInstance().编辑框_大于总注_大小单双_4)
                            {
                                if (result.Sum == 13 || result.Sum == 14)
                                {
                                    b.Odds = Config.GetInstance().编辑框_特殊赔率_大小单双_4;
                                }
                            }
                        }

                        if (Config.GetInstance().选择框_大小单双_出豹子)
                        {
                            b.Odds = Config.GetInstance().编辑框_赔率_大小单双_出豹子;
                        }
                        if (Config.GetInstance().选择框_大小单双_出对子)
                        {
                            b.Odds = Config.GetInstance().编辑框_赔率_大小单双_出对子;
                        }
                        if (Config.GetInstance().选择框_大小单双_出顺子)
                        {
                            b.Odds = Config.GetInstance().编辑框_赔率_大小单双_出顺子;
                        }
                    }

                    //组合
                    if (b.Keyword == "大单" || b.Keyword == "大双" || b.Keyword == "小单" || b.Keyword == "小双")
                    {
                        if (Config.GetInstance().单选框_赔率_组合一)
                        {
                            b.Odds = Config.GetInstance().编辑框_赔率_组合;
                            if (Config.GetInstance().选择框_特殊赔率_组合一)
                            {
                                if (result.Sum == 13 || result.Sum == 14)
                                {
                                    b.Odds = Config.GetInstance().编辑框_赔率_组合1314;
                                }
                            }
                        }

                        if (Config.GetInstance().单选框_赔率_组合二)
                        {
                            if (b.Keyword == "小单" || b.Keyword == "大双")
                            {
                                b.Odds = Config.GetInstance().编辑框_赔率_大双小单;
                            }
                            else if (b.Keyword == "大单" || b.Keyword == "小双")
                            {
                                b.Odds = Config.GetInstance().编辑框_赔率_小双大单;
                            }

                            if (Config.GetInstance().选择框_特殊赔率_组合_1)
                            {                                
                                if (sum组合 > Config.GetInstance().编辑框_大于总注_组合_1)
                                {
                                    if (result.Sum == 13 || result.Sum == 14)
                                    {
                                        b.Odds = Config.GetInstance().编辑框_特殊赔率_组合_1;
                                    }
                                }
                            }
                            if (Config.GetInstance().选择框_特殊赔率_组合_2)
                            {
                                if (sum组合 > Config.GetInstance().编辑框_大于总注_组合_2)
                                {
                                    if (result.Sum == 13 || result.Sum == 14)
                                    {
                                        b.Odds = Config.GetInstance().编辑框_特殊赔率_组合_2;
                                    }
                                }
                            }
                            if (Config.GetInstance().选择框_特殊赔率_组合_3)
                            {                                
                                if (sum组合 > Config.GetInstance().编辑框_大于总注_组合_3)
                                {
                                    if (result.Sum == 13 || result.Sum == 14)
                                    {
                                        b.Odds = Config.GetInstance().编辑框_特殊赔率_组合_3;
                                    }
                                }
                            }
                        }

                        if (Config.GetInstance().选择框_组合_出豹子)
                        {
                            b.Odds = Config.GetInstance().编辑框_赔率_组合_出豹子;
                        }
                        if (Config.GetInstance().选择框_组合_出对子)
                        {
                            b.Odds = Config.GetInstance().编辑框_赔率_组合_出对子;
                        }
                        if (Config.GetInstance().选择框_组合_出顺子)
                        {
                            b.Odds = Config.GetInstance().编辑框_赔率_组合_出顺子;
                        }
                    }

                    //极大极小
                    if (!"".Equals(result.JidaJixiao))
                    {
                        if (b.Keyword == "极大" || b.Keyword == "极小")
                        {
                            b.Odds = Config.GetInstance().编辑框_赔率_极小极大;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 开奖处理，计算输赢
        /// </summary>
        public static int ProcessOpenResult(List<BetRecord> recordList, Result result)
        {
            foreach (BetRecord b in recordList)
            {
                #region 是否计入流水
                b.IsWater = true;
                if (Config.GetInstance().选择框_不计入流水_1314_1)
                {
                    if (result.Sum == 13 && (b.Keyword == "小" || b.Keyword == "小单"))
                    {
                        b.IsWater = false;
                    }
                    if (result.Sum == 14 && (b.Keyword == "大" || b.Keyword == "大双"))
                    {
                        b.IsWater = false;
                    }
                }

                if (Config.GetInstance().选择框_不计入流水_1314_2)
                {
                    if (result.Sum == 13 || result.Sum == 14)
                    {
                        b.IsWater = false;
                    }
                }

                if (Config.GetInstance().选择框_不计入流水_大小单双组合)
                {
                    if (b.Keyword == "大" || b.Keyword == "小" || b.Keyword == "单" || b.Keyword == "双" ||
                        b.Keyword == "大单" || b.Keyword == "大双" || b.Keyword == "小单" || b.Keyword == "小双")
                    {
                        b.IsWater = false;
                    }
                }

                if (Config.GetInstance().选择框_超额_大小单双流水)
                {
                    if (b.Amount < Config.GetInstance().编辑框_超额_大小单双流水)
                    {
                        b.IsWater = false;
                    }
                }
                #endregion


                if (int.TryParse(b.Keyword, out int num))
                {
                    if (b.Keyword == result.Sum.ToString())
                    {
                        b.Award = (int)(b.Amount * Convert.ToDecimal(b.Odds));
                    }
                }
                else
                {
                    if (new string[] { result.DaXiao(), result.DanShuang(), result.DaXiao() + result.DanShuang(), result.JidaJixiao }.Contains(b.Keyword))
                    {
                        b.Award = (int)(b.Amount * Convert.ToDecimal(b.Odds));
                    }
                    if (b.Keyword == "豹子" && result.IsBaozi())
                    {
                        b.Award = (int)(b.Amount * Convert.ToDecimal(b.Odds));
                    }
                    if (b.Keyword == "对子" && result.IsDuizi())
                    {
                        b.Award = (int)(b.Amount * Convert.ToDecimal(b.Odds));
                    }
                    if (b.Keyword == "顺子" && result.IsShunzi())
                    {
                        b.Award = (int)(b.Amount * Convert.ToDecimal(b.Odds));
                    }
                }
                b.Status = BetRecordStatus.已结算;
            }
            BetRecordDao.Save(recordList);
            return recordList.Sum(b => b.Award - b.Amount);
        }


        /// <summary>
        /// 封盘处理
        /// </summary>
        internal static void ProcessClosing()
        {
            foreach (User user in RobotClient.UserList)
            {
                if (user.RecordList.Count > 0)
                {                                   
                    user.ChangeJifen(-user.Blzf, RecordType.下注);
                }                
            }
        }

        /// <summary>
        /// 封盘处理
        /// </summary>
        internal static void ProcessClosing(Result result)
        {
            List<User> userList = UserDao.GetUsers(result);
            foreach (User user in userList)
            {
                if (user.RecordList.Count > 0)
                {
                    User uiUser = RobotClient.GetUser(user.UserId);
                    uiUser.ChangeJifen(-user.Blzf, RecordType.下注);
                }
            }
        }

        /// <summary>
        /// 计算用户自动流水
        /// </summary>
        /// <param name="user"></param>
        /// <param name="sumWater"></param>
        /// <param name="sumBlyk"></param>
        /// <returns></returns>
        public static int CalcAutoRunningWater(User user, int sumWater, int sumBlyk)
        {
            if (Config.GetInstance().单选框_退分_流水关闭)
            {
                return 0 ;
            }

            double scale = Config.GetInstance().编辑框_退分_流水统一比例;
            if (Config.GetInstance().单选框_退分_流水单独设置)
            {
                scale = user.RunningWaterScale;
            }  

            int runningWater = (int)(sumWater * scale * 0.01);
            if (Config.GetInstance().选择框_退分_流水最低返水金额)
            {
                if (runningWater < Config.GetInstance().编辑框_退分_流水最低返水金额)
                {
                    return 0;
                }
            }

            return runningWater;
        }

        /// <summary>
        /// 计算自助回水
        /// </summary>
        /// <param name="user"></param>
        /// <param name="sumWater"></param>
        /// <param name="sumBlyk"></param>
        /// <returns></returns>
        internal static int CalcAutoReturnWater(User user, int sumWater, int sumBlyk)
        {
            if (Config.GetInstance().单选框_退分_回水关闭)
            {
                return 0;
            }

            if(sumBlyk >= 0)
            {
                return 0;
            }

            double scale = Config.GetInstance().编辑框_退分_回水统一比例;
            if (Config.GetInstance().单选框_退分_回水单独设置)
            {
                scale = user.ReturnWaterScale;
            }
           
            int returnWater = (int)(Math.Abs(sumBlyk) * scale * 0.01);
            if (Config.GetInstance().选择框_退分_回水最低返水金额)
            {
                if (returnWater < Config.GetInstance().编辑框_退分_回水最低返水金额)
                {
                    return 0;
                }
            }

            return returnWater;
        }

        /// <summary>
        /// 自动流水
        /// </summary>
        /// <param name="timeType">时间类型：1今天 2昨天</param>
        /// <param name="user">用户</param>
        internal static void ProcessAutoRunningWater(int timeType, User user)
        {
            if (Config.GetInstance().单选框_退分_流水关闭) { 
                return;
            }
            DateTime dateTime = DateTime.Today;
            if (timeType == 2)
            {
                dateTime = DateTime.Today.AddDays(-1);
            }
            DataTable dt = BettingDao.GetBettingRunningByDate(user.UserId, dateTime);

            if (dt.Rows.Count==0)
            {
                user.ReplayMessage("无未返水下注记录");
                return;
            }
            var waters = dt.AsEnumerable().Select(row => row.Field<long>("bl_water"));
            var blyks = dt.AsEnumerable().Select(row => row.Field<long>("blyk"));

            double scale = Config.GetInstance().编辑框_退分_流水统一比例;
            if (Config.GetInstance().单选框_退分_流水单独设置)
            {
                scale = user.RunningWaterScale;
            }
            int waterCount = RecordDao.GetTodayAutoRunningWaterCount(user.UserId);
            
            if (Config.GetInstance().选择框_退分_盈利流水次数) {
                if (blyks.Sum() > 0)
                {
                    if(waterCount > Config.GetInstance().编辑框_退分_盈利流水次数)
                    {
                        user.ReplayMessage("盈利时，流水返水次数最多"+ Config.GetInstance().编辑框_退分_盈利流水次数 + "次");
                        return;
                    }
                }
            }
            if (Config.GetInstance().选择框_退分_每日流水次数)
            {
                if (waterCount > Config.GetInstance().编辑框_退分_每日流水次数)
                {
                    user.ReplayMessage("每日流水返水次数最多" + Config.GetInstance().编辑框_退分_每日流水次数 + "次");
                    return;
                }
            }                
                
            if (Config.GetInstance().选择框_退分_流水最低流水占比)
            {
                DateTime today = DateTime.Today;
                string day = today.ToString("yyyy-MM-dd");
                if (timeType == 2)
                {
                    DateTime yesterday = today.AddDays(-1);
                    day = yesterday.ToString("yyyy-MM-dd");
                }
                double shangFenSum = RecordDao.GetShangFenSum(user.UserId, day);
                if ((double)waters.Sum()/shangFenSum< Config.GetInstance().编辑框_退分_流水最低流水占比)
                {
                    user.ReplayMessage("流水占比达到" + Config.GetInstance().编辑框_退分_流水最低流水占比 + "倍才返");
                    return;
                }
            }
            if (Config.GetInstance().选择框_退分_流水最低投注期数)
            {
                if ((double)dt.Rows.Count < Config.GetInstance().编辑框_退分_流水最低投注期数)
                {
                    user.ReplayMessage("下注期数达到" + Config.GetInstance().编辑框_退分_流水最低投注期数 + "期才返");
                    return;
                }
            }

            int runningWater = (int)(waters.Sum() * scale * 0.01);
            if (runningWater == 0)
            {
                user.ReplayMessage("无可返水金额");
                return;
            }
            if (Config.GetInstance().选择框_退分_流水最低返水金额)
            {
                if(runningWater < Config.GetInstance().编辑框_退分_流水最低返水金额)
                {
                    user.ReplayMessage("每次返水金额达到" + Config.GetInstance().编辑框_退分_流水最低返水金额 + "才返");
                    return;
                }
            }

            user.ChangeJifen(runningWater, RecordType.自动流水, 0, "", dateTime);

            if (Config.GetInstance().选择框_退分_流水不显示累计返水)
            {
                user.ReplayMessage("成功返水："+runningWater);
            }
            else
            {
                user.ReplayMessage("\r\n成功返水：" + runningWater+"\r\n累计返水:"+RecordDao.GetTotalWater(user.UserId));
            }     

            //标记已完成返水
            foreach (DataRow row in dt.Rows)
            {
                BettingDao.FinishRunningWater(Convert.ToInt32(row["id"]));
            }            
        }

        /// <summary>
        /// 自助回水
        /// </summary>
        /// <param name="timeType">时间类型：1今天 2昨天</param>
        /// <param name="user">用户</param>
        internal static void ProcessAutoReturnWater(int timeType, User user)
        {
            if (Config.GetInstance().单选框_退分_回水关闭)
            {
                return;
            }
            DateTime dateTime = DateTime.Today;
            if (timeType == 2)
            {
                dateTime = DateTime.Today.AddDays(-1);
            }
            DataTable dt = BettingDao.GetBettingReturnByDate(user.UserId, dateTime);
            if (dt.Rows.Count == 0)
            {
                user.ReplayMessage("无未返水下注记录");
                return;
            }
            var waters = dt.AsEnumerable().Select(row => row.Field<long>("bl_water"));
            var blyks = dt.AsEnumerable().Select(row => row.Field<long>("blyk"));

            double scale = Config.GetInstance().编辑框_退分_回水统一比例;
            if (Config.GetInstance().单选框_退分_回水单独设置)
            {
                scale = user.ReturnWaterScale;
            }
            int waterCount = RecordDao.GetTodayAutoReturnWaterCount(user.UserId);
            
            if (Config.GetInstance().选择框_退分_每日回水次数)
            {
                if (waterCount > Config.GetInstance().编辑框_退分_每日回水次数)
                {
                    user.ReplayMessage("限制每日自助回水" + Config.GetInstance().编辑框_退分_每日回水次数 + "次");
                    return;
                }
            }

            if (Config.GetInstance().选择框_退分_回水最低流水占比)
            {
                DateTime today = DateTime.Today;
                string day = today.ToString("yyyy-MM-dd");
                if (timeType == 2)
                {
                    DateTime yesterday = today.AddDays(-1);
                    day = yesterday.ToString("yyyy-MM-dd");
                }
                double shangFenSum = RecordDao.GetShangFenSum(user.UserId, day);
                if (waters.Sum() / shangFenSum < Config.GetInstance().编辑框_退分_回水最低流水占比)
                {
                    user.ReplayMessage("流水占比达到" + Config.GetInstance().编辑框_退分_回水最低流水占比 + "倍才返");
                    return;
                }
            }
            if (Config.GetInstance().选择框_退分_回水最低投注期数)
            {
                if (dt.Rows.Count < Config.GetInstance().编辑框_退分_回水最低投注期数)
                {
                    user.ReplayMessage("下注期数达到" + Config.GetInstance().编辑框_退分_回水最低投注期数 + "期才返");
                    return;
                }
            }
            if (Config.GetInstance().选择框_退分_回水用户输分)
            {
                if (blyks.Sum() > -Config.GetInstance().编辑框_退分_回水用户输分)
                {
                    user.ReplayMessage("用户输分" + Config.GetInstance().编辑框_退分_回水用户输分 + "以上才返");
                    return;
                }
            }
            int returnWater = (int)(Math.Abs(blyks.Sum()) * scale * 0.01);
            if (returnWater == 0)
            {
                user.ReplayMessage("无可回水金额");
                return;
            }
            if (Config.GetInstance().选择框_退分_回水最低返水金额)
            {
                if (returnWater < Config.GetInstance().编辑框_退分_回水最低返水金额)
                {
                    user.ReplayMessage("每次回水金额达到" + Config.GetInstance().编辑框_退分_回水最低返水金额 + "才返");
                    return;
                }
            }

                            user.ChangeJifen(returnWater, RecordType.自助回水, 0, "", dateTime);

            if (Config.GetInstance().选择框_退分_回水不显示累计返水)
            {
                user.ReplayMessage("成功回水：" + returnWater);
            }
            else
            {
                user.ReplayMessage("\r\n成功回水：" + returnWater + "\r\n累计返水:" + RecordDao.GetTotalWater(user.UserId));
            }

            //标记已完成返水
            foreach (DataRow row in dt.Rows)
            {
                BettingDao.FinishReturnWater(Convert.ToInt32(row["id"]));
            }
        }
    }
}
