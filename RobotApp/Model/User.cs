using DocumentFormat.OpenXml.Spreadsheet;
using ImLibrary.Model;
using Microsoft.VisualBasic.ApplicationServices;
using RobotApp.Dao;
using RobotApp.IM;
using RobotApp.MiniDialog;
using RobotApp.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace RobotApp.Model
{
    enum UserStatus
    {
        初始化 = 0,
        正常用户 = 1,        
        已禁言 = 2,
        已删除 = 3
    }

    [SupportedOSPlatform("windows")]
    internal class User : IMUser
    {
        private object _lockObject = new object();
        private string _nickName;
        public override string NickName { 
            get { return _nickName; } 
            set 
            {
                if (_nickName != value)
                {
                    if (UserDao.GetUserByNickName(value) != null)
                    {
                        throw new Exception("该昵称已存在，请换一个！");
                    }

                    string oldValue = _nickName;
                    _nickName = value;
                    UserDao.UpdateUser(this);
                    OplogDao.Add(oldValue + "的昵称修改为" + _nickName);
                    OnPropertyChanged();
                }                
            } 
        }
        /// <summary>
        /// 积分余额
        /// </summary>
        public int Jifen { get; private set; }         

        /// <summary>
        /// 上下分
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="type"></param>
        /// <param name="scoreId"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void ChangeJifen(int amount, RecordType type, int scoreId = 0, string remark="", DateTime? calcTime = null, bool forceChange = false)
        {
            if (!forceChange && Jifen + amount < 0) { 
                throw new Exception("积分不足");
            }
            if (amount == 0)
            {
                throw new ArgumentOutOfRangeException("上下分数不能为0");
            }            

            Record rec = new Record();
            rec.Code = UserId;
            rec.type = type;          
            rec.Amount = amount;
            rec.OldAmount = Jifen;
            rec.NowAmount = Jifen + amount;
            rec.CreateTime = DateTime.Now;
            rec.ScoreId = scoreId;
            rec.Remark = remark;
            if (calcTime != null)
            {
                rec.CalcTime = (DateTime)calcTime;
            }
            RecordDao.AddRecord(rec);

            Jifen += amount;
            if(type == RecordType.下分 || type == RecordType.下注)
            {
                UnfreezeJifen(Math.Abs(amount));
            }
            else
            {
                UserDao.UpdateUser(this);
            }                
            OnPropertyChanged();
        }

        /// <summary>
        /// 冻结积分
        /// </summary>
        public int FrozenJifen { get; private set; }
        /// <summary>
        /// 冻结积分
        /// </summary>
        /// <param name="amount"></param>
        /// <exception cref="Exception"></exception>
        public void FreezeJifen(int amount)
        {
            if (Jifen - amount < 0)
            {
                throw new Exception($"【{NickName}】积分不足！");
            }
            FrozenJifen += amount;
            UserDao.UpdateUser(this);
        }
        /// <summary>
        /// 解冻积分
        /// </summary>
        /// <param name="amount"></param>
        /// <exception cref="Exception"></exception>
        public void UnfreezeJifen(int amount)
        {
            if(FrozenJifen < amount)
            {
                throw new Exception($"【{NickName}】可解冻积分不足，冻结积分【{FrozenJifen}】,需解冻【{amount}】！");
            }
            FrozenJifen -= amount;
            UserDao.UpdateUser(this);
        }

        /// <summary>
        /// 可用积分余额
        /// </summary>
        public int Jifen_Available { get { return Jifen - FrozenJifen; } }

        /// <summary>
        /// 用户状态
        /// </summary>
        private UserStatus _status = UserStatus.初始化;
        public UserStatus Status
        {
            get { return _status; }
            set {
                if (_status != value)
                {
                    _status = value;
                    UserDao.UpdateUser(this);
                    OplogDao.Add(this.NickName + "状态修改为：" + value);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 是否假人
        /// </summary>
        private bool _isDummy = false;
        public bool IsDummy { 
            get { return _isDummy; }
            set
            {
                if (_isDummy != value)
                {
                    _isDummy = value;
                    UserDao.UpdateUser(this);
                    OplogDao.Add(this.NickName + "修改为：" + (value?"假人":"真人"));
                }
            }
        }

        /// <summary>
        /// 拉手Code,所有User都可以当拉手
        /// </summary>
        public string LaShouCode { get; set; }

        /// <summary>
        /// 流水比例
        /// </summary>
        public double RunningWaterScale { get; set; }

        /// <summary>
        /// 回水比例
        /// </summary>
        public double ReturnWaterScale { get; set; }

        public List<BetRecord> RecordList { get; private set; } = new List<BetRecord>();

        public User(string userId, string nickName, int jiFen, int frozenJifen, int slyk, bool isDummy, UserStatus status)
        {            
            this._nickName = nickName;
            this.Jifen = jiFen;
            this.FrozenJifen = frozenJifen;            
            this._status = status;
            this._isDummy = isDummy;
            this.Slyk = slyk;
            this.UserId = userId;
            this.RecordList = BetRecordDao.GetByUserAndResult(this, RobotClient.CurrentResult);
        }

        public User(string userId, string nickName, bool isDummy = false)
        {
            this._nickName = nickName;
            this.UserId = userId;
            this.IsDummy = isDummy;
        }

        public void NewResultUser()
        {            
            RecordList.Clear();            
            OnPropertyChanged();
        }

        /// <summary>
        /// 本轮派奖
        /// </summary>
        public int Award
        {
            get
            {
                return RecordList.Sum(br => br.Award);
            }
            private set { }
        }
        /// <summary>
        /// 本轮盈亏,开奖后计算
        /// </summary>
        public int Blyk { 
            get { 
                return RecordList.Sum(br => br.Award-br.Amount);
            }
            private set { }
        }
        /// <summary>
        /// 本轮流水,开奖后计算
        /// </summary>
        public int BlWater
        {
            get
            {
                return (int)RecordList.Sum(br => br.IsWater ? br.Amount : 0);
            }
        }
        /// <summary>
        /// 上轮盈亏
        /// </summary>
        public int Slyk { get; set; }
        /// <summary>
        /// 本轮总分
        /// </summary>
        public int Blzf { 
            get {
                return RecordList.Sum(br => br.Amount);
            }
        } 

        /// <summary>
        /// 给这个用户回复消息
        /// </summary>
        /// <param name="message"></param>
        public void ReplayMessage(string message)
        {
            if (Config.GetInstance().选择框_提示_艾特)
            {
                RobotClient.SendMessage("@" + NickName + " " + message);
            }
            else
            {
                RobotClient.SendMessage(NickName + " " + message);
            }            
        }

        /// <summary>
        /// 添加下注
        /// </summary>
        public void AddBetRecords(List<BetRecord> betRecords)
        {
            if(betRecords.Count == 0)
            {
                return;
            }
            BetRecordDao.Add(betRecords);
            RecordList.AddRange(betRecords);
            FreezeJifen(betRecords.Sum(b => b.Amount));
            OnPropertyChanged();
        }

        /// <summary>
        /// 清空本期下注
        /// </summary>
        public void ClearBetRecords(string remark= "")
        {
            if(RecordList.Count == 0)
            {
                return;
            }
            int sumAmount = RecordList.Sum(bet => bet.Amount);
            if (BetRecordDao.Delete(RecordList, remark))
            {
                RecordList.Clear();
                if (RobotClient.CurrentResult.Status < ResultStatus.已封盘)
                {
                    UnfreezeJifen(sumAmount);
                }
                OnPropertyChanged();
            }            
        }

        /// <summary>
        /// 猜猜内容
        /// </summary>
        public string Ccnr
        {
            get {
                var sums = RecordList.GroupBy(b => b.Keyword).Select(t => new
                {
                    Keyword = t.Key,
                    Amount = t.Sum(b => b.Amount),
                });
                String betStr = String.Empty;           
                foreach (var item in sums)
                {                               
                    if(int.TryParse(item.Keyword, out int num))
                    {
                        betStr = betStr + item.Keyword + "/" + item.Amount + " ";
                    }
                    else
                    {
                        betStr = betStr + item.Keyword + item.Amount + " ";
                    }
                }
                return betStr;
            }

            set
            {
                if (Config.GetInstance().CheckPermission(PermissionForm.修改用户下注权限))
                {
                    string oldCcnr = Ccnr;
                    if (!string.IsNullOrEmpty(value))
                    {
                        Model.Message msg = new Model.Message();
                        msg.Msg = value;
                        msg.Fp = "0";
                        this.ClearBetRecords("客服修改");
                        this.AddBetRecords(BettingUtil.ProcessMessage(this, msg));
                    }
                    else
                    {
                        this.ClearBetRecords("客服修改");
                    }
                    OplogDao.Add(NickName + "的竞猜内容[" + oldCcnr + "]修改为[" + Ccnr + "]");
                    UserDao.UpdateUser(this);
                    OnPropertyChanged();
                }
            }
         }
    
        public bool Is杀组合()
        {
            string[] zhs = new string[] { "大单", "大双", "小单", "小双" };
            foreach (var record in RecordList)
            {
                zhs = zhs.Where(z => !z.Contains(record.Keyword)).ToArray();
            }
            if (zhs.Length == 1)
            {
                return true;
            }
            return false;
        }

        public bool Is反向组合()
        {
            if ((Count大单 > 0 && Count小单 > 0) || (Count大双 > 0 && Count小双 > 0))
            {
                return true; 
            }
            return false;
        }

        public bool Is同向组合()
        {
            if ((Count大单 > 0 && Count大双 > 0) || (Count小单 > 0 && Count小双 > 0))
            {
                return true;
            }
            return false;
        }

        public bool Is四门组合()
        {
            string[] zhs = new string[] { "大单", "大双", "小单", "小双" };
            foreach (var record in RecordList)
            {
                zhs = zhs.Where(z => !z.Contains(record.Keyword)).ToArray();
            }
            if (zhs.Length == 0)
            {
                return true;
            }
            return false;
        }

        public bool Is大小单双反向()
        {
            if ((Count大 > 0 && Count小 > 0) || (Count单 > 0 && Count双 > 0))
            {
                return true;
            }
            return false;
        }

        public double Sum单点
        {
            get
            {
                return RecordList.Sum(b => int.TryParse(b.Keyword, out int num) ? b.Amount : 0);
            }
        }

        public double Sum极值
        {
            get
            {
                return RecordList.Sum(b => (b.Keyword=="极大" || b.Keyword == "极小") ? b.Amount : 0);
            }
        }

        public double Sum大小单双
        {
            get
            {
                return RecordList.Sum(b => new string[]{ "大", "小", "单", "双"}.Contains(b.Keyword) ? b.Amount : 0);
            }
        }

        public double Sum组合
        {
            get
            {
                return RecordList.Sum(b => new string[] { "大单", "小单", "大双", "小双" }.Contains(b.Keyword) ? b.Amount : 0);
            }
        }

        public double Sum大小单双组合
        {
            get
            {
                return RecordList.Sum(b => new string[] { "大", "小", "单", "双", "大单", "小单", "大双", "小双" }.Contains(b.Keyword) ? b.Amount : 0);
            }
        }

        public double Sum大双小单
        {
            get
            {
                return RecordList.Sum(b => (b.Keyword == "大双" || b.Keyword == "小单") ? b.Amount : 0);
            }
        }

        public int Count大单
        {
            get 
            { 
                return RecordList.Count(b=>b.Keyword=="大单");
            }
        }

        public int Count小单
        {
            get
            {
                return RecordList.Count(b => b.Keyword == "小单");
            }
        }

        public int Count大双
        {
            get
            {
                return RecordList.Count(b => b.Keyword == "大双");
            }
        }

        public int Count小双
        {
            get
            {
                return RecordList.Count(b => b.Keyword == "小双");
            }
        }

        public int Count大
        {
            get
            {
                return RecordList.Count(b => b.Keyword == "大");
            }
        }

        public int Count小
        {
            get
            {
                return RecordList.Count(b => b.Keyword == "小");
            }
        }

        public int Count单
        {
            get
            {
                return RecordList.Count(b => b.Keyword == "单");
            }
        }

        public int Count双
        {
            get
            {
                return RecordList.Count(b => b.Keyword == "双");
            }
        }
    }
}
