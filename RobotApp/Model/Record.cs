using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotApp.Model
{
    enum RecordType
    {
        上分 = 1,
        下分 = 2,
        下注 = 3,
        派奖 = 4,
        退分 = 5,
        自动流水 = 6,
        自助回水 = 7,
        退回剩余自助流水 = 8,
        退分撤消 = 9,        
        取消下注 = 10,
        清除下注 = 11,
        剩余自助流水 = 12,
        自动回水 = 13,
        自助流水 = 14,
    //  退回剩余自助流水 = 13,
        客服修改 = 99, //客服修改，需备注修改原因
    }

    internal class Record
    {
        public int Id { get; set; }
        /// <summary>
        /// 用户Code
        /// </summary>
        public string? Code { get; set; }
        /// <summary>
        /// 原积分
        /// </summary>
        public int OldAmount { get; set; }
        /// <summary>
        /// 帐变积分
        /// </summary>
        public int NowAmount { get; set; }
        /// <summary>
        /// 现积分
        /// </summary>
        public int Amount { get; set; }
        /// <summary>
        /// 帐变原因
        /// </summary>
        public RecordType type { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
        /// <summary>
        /// 帐变时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 计算时间
        /// </summary>
        public DateTime CalcTime { get; set; }
        /// <summary>
        /// 帐变操作人
        /// </summary>
        public string? CreateUser { get; set; }
        /// <summary>
        /// 上下分ID
        /// </summary>
        public int ScoreId { get; set; }
    }
}
