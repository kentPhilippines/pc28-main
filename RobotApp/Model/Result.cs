using ImLibrary.Model;
using RobotApp.IM;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace RobotApp.Model
{
    internal enum ResultStatus
    {
        无开盘 = 0,
        竞猜中 = 1,
        已封盘 = 2,
        等待开奖 = 3,
        已开奖 = 4,
        已结算 = 5,
        已返水 = 6,        
    }
    [SupportedOSPlatform("windows")]
    internal class Result : ViewModelBase
    {
        public int Id { get; set; }
        public LotterCode Type { get; set; }
        public int Issue { get; set; }        
        public int? Num1 { get; set; }
        public int? Num2 { get; set; }
        public int? Num3 { get; set; }
        public int? Sum { get; set; }
        public string OpenResult { get; set; }
        public DateTime OpenTime { get; set; }
        public int NextIssue { get; set; }
        public DateTime NextTime { get; set; }        
        public double? Amount { get; set; }
        public double? Award { get; set; }
        public double? WinOrLose { get { return Award - Amount; } }
        private ResultStatus _status;
        public ResultStatus Status { get; set; }
        public List<User> userList = new List<User>();

        /// <summary>
        /// 设置开奖结果
        /// </summary>
        /// <param name="result"></param>
        public Result SetResult(Result result)
        {
            Num1 = result.Num1;
            Num2 = result.Num2;
            Num3 = result.Num3;
            Sum = result.Sum;
            OpenResult = result.OpenResult;
            OpenTime = result.OpenTime;
            NextIssue = result.NextIssue;
            NextTime = result.NextTime;
            if(Status < ResultStatus.已开奖)
            {
                Status = result.Status;
            }            
            return this;
        }

        public string Remark { get; set; }        

        public string ThreeNum { 
            get
            {
                if (Status != ResultStatus.无开盘 && Status < ResultStatus.已开奖)
                {
                    return "";
                }
                return Num1 + "+" + Num2 + "+" + Num3;
            } 
        }

        public string ResultDesc {
            get {
                if (Status!=ResultStatus.无开盘 && Status < ResultStatus.已开奖)
                {
                    return "";
                }
                return Zuhe + BaoziShunziDuizi + JidaJixiao;
            } 
        }

        /// <summary>
        /// 豹子、顺子、对子
        /// </summary>
        /// <returns></returns>
        public string BaoziShunziDuizi
        {
            get
            {
                if (IsBaozi()) return "豹子";
                if (IsShunzi()) return "顺子";
                if (IsDuizi()) return "对子";
                return "";
            }
        }

        /// <summary>
        /// 是否顺子
        /// </summary>
        /// <returns></returns>
        public bool IsShunzi()
        {
            int?[] number = new int?[3] {Num1, Num2, Num3};
            Array.Sort(number);
            if (Config.GetInstance().选择框_顺子890) { 
                if(number[0] == 0 && number[1]==8 && number[2] == 9) return true;
            }
            if (Config.GetInstance().选择框_顺子901)
            {
                if (number[0] == 0 && number[1] == 1 && number[2] == 9) return true;
            }
            return (number[2] - number[1] == 1) && (number[1] - number[0] == 1);
        }

        /// <summary>
        /// 是否对子
        /// </summary>
        /// <returns></returns>
        public bool IsDuizi()
        {
            return (Num1 == Num2 && Num2 != Num3)
                || (Num1 == Num3 && Num2 != Num3)
                || (Num2 == Num3 && Num2 != Num1);
        }

        /// <summary>
        /// 是否豹子
        /// </summary>
        /// <returns></returns>
        public bool IsBaozi()
        {
            return Num1 == Num2 && Num2 == Num3;
        }

        /// <summary>
        /// 极大极小
        /// </summary>
        /// <returns>反回极大极小，如不是返回空字符串</returns>
        public string JidaJixiao
        {
            get { 
                if(Sum >= 22)
                {
                    return "极大";
                }
                else if(Sum <= 5)
                {
                    return "极小";
                }
                return "";
            }
        }

        /// <summary>
        /// 组合
        /// </summary>
        /// <returns></returns>
        public string Zuhe
        {
            get
            {
                return DaXiao() + DanShuang();
            }            
        }

        /// <summary>
        /// 大小
        /// </summary>
        /// <returns></returns>
        public string DaXiao()
        {
            return Sum>13 ? "大":"小";
        }

        /// <summary>
        /// 单双
        /// </summary>
        /// <returns></returns>
        public string DanShuang()
        {
            return Sum%2==1?"单":"双";
        }
    }
}
