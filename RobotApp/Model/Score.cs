using ImLibrary.Model;
using System;

namespace RobotApp.Model
{
    enum ScoreStatus
    {
        待审核 = 0,
        同意 = 1,
        拒绝 = 2,
    }
    internal class Score : ViewModelBase
    {
        public int Id { get; set; }
        /// <summary>
        /// 用户编码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public int Amount { get; set; }
        public string AmountStr { 
            get {
                if (Amount > 0)
                {
                    return "查" + Amount;
                }
                else
                {
                    return "回" + Math.Abs(Amount);    
                }
            }
        }
        /// <summary>
        /// 状态0:待审核|1:已通过|2:已拒绝
        /// </summary>
        public ScoreStatus Status { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? ReviewTime { get; set; }
        /// <summary>
        /// 用户昵称,取自用户表
        /// </summary>
        public string _nickName;
        public string NickName { get { return _nickName; } set { _nickName = value; OnPropertyChanged(); } }
        /// <summary>
        /// 是否假人,取自用户表
        /// </summary>
        public bool IsDummy { get; set; }
    }
}
