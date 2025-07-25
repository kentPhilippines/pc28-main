using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotApp.Model
{
    enum BetRecordStatus
    {
        已下注 = 0, //已下注
        已结算 = 1, //已结算
        已反水 = 2, //已反水
    }
    enum BetType
    {
        SINGLE = 1, //单点
        KEYWORDS = 2, //关键字
        SOHA = 3, //梭哈
    }
    internal class BetRecord
    {
        public int Id { get; set; }
        public int ResultId { get; set; }
        public string UserCode { get; set; }
        public BetType BetType { get; set; }
        public string Keyword { get; set; }
        public int Amount { get; set; }
        /// <summary>
        /// 赔率
        /// </summary>
        public double Odds { get; set; }
        /// <summary>
        /// 最大赔付金额
        /// </summary>
        public double MaxPay { get; set; }
        public int Award { get; set; }
        public string Fp { get; set; }
        public BetRecordStatus Status { get; set; }
        public string Remark { get; set; }
        /// <summary>
        /// 是否计入流水
        /// </summary>
        public bool IsWater {  get; set; }
        /// <summary>
        /// 是否计入流水
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }
}
