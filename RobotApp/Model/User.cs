﻿using CommonLibrary;
using DocumentFormat.OpenXml.Spreadsheet;
using ImLibrary.Model;
using Microsoft.VisualBasic.ApplicationServices;
using RobotApp.Dao;
using RobotApp.IM;
using RobotApp.MiniDialog;
using RobotApp.Util;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
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

            // 使用数据库事务确保积分变更的原子性，防止软件闪退导致数据不一致
            using (var connection = new SQLiteConnection(DBHelperSQLite.connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. 添加积分变更记录
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
                        
                        string recordSql = @"insert into t_record (code, old_amount, amount, now_amount, type, remark, score_id, create_time, calc_time, create_user) 
                            values (@code, @oldAmount, @amount, @nowAmount, @type, @remark, @scoreId, @createTime, @calcTime, @createUser)";
                        using (var recordCommand = new SQLiteCommand(recordSql, connection, transaction))
                        {
                            recordCommand.Parameters.AddWithValue("@code", rec.Code ?? "");
                            recordCommand.Parameters.AddWithValue("@oldAmount", rec.OldAmount);
                            recordCommand.Parameters.AddWithValue("@amount", rec.Amount);
                            recordCommand.Parameters.AddWithValue("@nowAmount", rec.NowAmount);
                            recordCommand.Parameters.AddWithValue("@type", (int)rec.type);
                            recordCommand.Parameters.AddWithValue("@remark", rec.Remark ?? "");
                            recordCommand.Parameters.AddWithValue("@scoreId", rec.ScoreId);
                            recordCommand.Parameters.AddWithValue("@createTime", rec.CreateTime);
                            recordCommand.Parameters.AddWithValue("@calcTime", calcTime.HasValue ? (object)calcTime.Value : DBNull.Value);
                            recordCommand.Parameters.AddWithValue("@createUser", rec.CreateUser ?? "");
                            recordCommand.ExecuteNonQuery();
                        }

                        // 2. 更新用户积分和冻结积分
                        int oldJifen = Jifen;
                        int oldFrozenJifen = FrozenJifen;
                        
                        Jifen += amount;
                        
                        if(type == RecordType.下分 || type == RecordType.下注)
                        {
                            // 解冻积分
                            int unfreezeAmount = Math.Abs(amount);
                            
                            // 程序重启后自动修复冻结积分不一致问题
                            if(FrozenJifen < unfreezeAmount)
                            {
                                // 检查是否是程序重启后的第一次操作
                                if(RobotClient.CurrentResult == null || RecordList.Count == 0)
                                {
                                    LogUtil.Log($"检测到程序重启后冻结积分不一致，用户【{NickName}】自动修复：原冻结积分={FrozenJifen}，需解冻={unfreezeAmount}");
                                    // 修复冻结积分：如果是下分操作，直接设置为0（完全解冻）
                                    if(type == RecordType.下分)
                                    {
                                        FrozenJifen = 0;
                                        LogUtil.Log($"用户【{NickName}】回分操作，冻结积分已重置为0");
                                    }
                                    else
                                    {
                                        // 如果是下注操作但冻结积分不足，抛出异常
                                        throw new Exception($"【{NickName}】可解冻积分不足，冻结积分【{FrozenJifen}】,需解冻【{unfreezeAmount}】！");
                                    }
                                }
                                else
                                {
                                    throw new Exception($"【{NickName}】可解冻积分不足，冻结积分【{FrozenJifen}】,需解冻【{unfreezeAmount}】！");
                                }
                            }
                            else
                            {
                                FrozenJifen -= unfreezeAmount;
                            }
                        }
                        
                        // 3. 更新用户信息到数据库
                        string updateUserSql = @"update t_user set nick_name=@nickName, jifen=@jifen, frozen_jifen=@frozenJifen,
                            slyk=@slyk, status=@status, is_dummy=@isDummy where code=@code";
                        using (var updateCommand = new SQLiteCommand(updateUserSql, connection, transaction))
                        {
                            updateCommand.Parameters.AddWithValue("@nickName", this.NickName);
                            updateCommand.Parameters.AddWithValue("@jifen", this.Jifen);
                            updateCommand.Parameters.AddWithValue("@frozenJifen", this.FrozenJifen);
                            updateCommand.Parameters.AddWithValue("@slyk", this.Slyk);
                            updateCommand.Parameters.AddWithValue("@status", (int)this.Status);
                            updateCommand.Parameters.AddWithValue("@isDummy", this.IsDummy);
                            updateCommand.Parameters.AddWithValue("@code", this.UserId);
                            updateCommand.ExecuteNonQuery();
                        }

                        // 提交事务
                        transaction.Commit();
                        
                        LogUtil.Log($"用户【{NickName}】积分变更事务提交成功，类型：{type}，变更积分：{amount}，当前积分：{Jifen}，冻结积分：{FrozenJifen}");
                    }
                    catch (Exception ex)
                    {
                        // 回滚事务
                        transaction.Rollback();
                        LogUtil.Log($"用户【{NickName}】积分变更事务失败，已回滚：{ex.Message}");
                        throw; // 重新抛出异常
                    }
                }
            }
            
            // 重要积分变动立即同步到数据库，防止数据丢失
            try
            {
                DBHelperSQLite.FlushWalToMainDB();
            }
            catch (Exception ex)
            {
                // 记录日志但不影响主流程
                LogUtil.LogEx(ex);
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
            
            // 使用数据库事务确保冻结积分操作的原子性和持久性
            using (var connection = new SQLiteConnection(DBHelperSQLite.connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int oldFrozenJifen = FrozenJifen;
                        FrozenJifen += amount;
                        
                        // 更新用户信息到数据库
                        string updateUserSql = @"update t_user set nick_name=@nickName, jifen=@jifen, frozen_jifen=@frozenJifen,
                            slyk=@slyk, status=@status, is_dummy=@isDummy where code=@code";
                        using (var updateCommand = new SQLiteCommand(updateUserSql, connection, transaction))
                        {
                            updateCommand.Parameters.AddWithValue("@nickName", this.NickName);
                            updateCommand.Parameters.AddWithValue("@jifen", this.Jifen);
                            updateCommand.Parameters.AddWithValue("@frozenJifen", this.FrozenJifen);
                            updateCommand.Parameters.AddWithValue("@slyk", this.Slyk);
                            updateCommand.Parameters.AddWithValue("@status", (int)this.Status);
                            updateCommand.Parameters.AddWithValue("@isDummy", this.IsDummy);
                            updateCommand.Parameters.AddWithValue("@code", this.UserId);
                            updateCommand.ExecuteNonQuery();
                        }

                        // 提交事务
                        transaction.Commit();
                        
                        LogUtil.Log($"用户【{NickName}】冻结积分事务提交成功，冻结金额：{amount}，当前冻结积分：{FrozenJifen}");
                    }
                    catch (Exception ex)
                    {
                        // 回滚事务
                        transaction.Rollback();
                        FrozenJifen -= amount; // 恢复内存状态
                        
                        LogUtil.Log($"用户【{NickName}】冻结积分事务失败，已回滚：{ex.Message}");
                        throw; // 重新抛出异常
                    }
                }
            }
            
            // 立即同步到磁盘，防止数据丢失
            try
            {
                DBHelperSQLite.FlushWalToMainDB();
            }
            catch (Exception ex)
            {
                LogUtil.LogEx(ex);
            }
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
            
            // 使用数据库事务确保解冻积分操作的原子性和持久性
            using (var connection = new SQLiteConnection(DBHelperSQLite.connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int oldFrozenJifen = FrozenJifen;
                        FrozenJifen -= amount;
                        
                        // 更新用户信息到数据库
                        string updateUserSql = @"update t_user set nick_name=@nickName, jifen=@jifen, frozen_jifen=@frozenJifen,
                            slyk=@slyk, status=@status, is_dummy=@isDummy where code=@code";
                        using (var updateCommand = new SQLiteCommand(updateUserSql, connection, transaction))
                        {
                            updateCommand.Parameters.AddWithValue("@nickName", this.NickName);
                            updateCommand.Parameters.AddWithValue("@jifen", this.Jifen);
                            updateCommand.Parameters.AddWithValue("@frozenJifen", this.FrozenJifen);
                            updateCommand.Parameters.AddWithValue("@slyk", this.Slyk);
                            updateCommand.Parameters.AddWithValue("@status", (int)this.Status);
                            updateCommand.Parameters.AddWithValue("@isDummy", this.IsDummy);
                            updateCommand.Parameters.AddWithValue("@code", this.UserId);
                            updateCommand.ExecuteNonQuery();
                        }

                        // 提交事务
                        transaction.Commit();
                        
                        LogUtil.Log($"用户【{NickName}】解冻积分事务提交成功，解冻金额：{amount}，当前冻结积分：{FrozenJifen}");
                    }
                    catch (Exception ex)
                    {
                        // 回滚事务
                        transaction.Rollback();
                        FrozenJifen += amount; // 恢复内存状态
                        
                        LogUtil.Log($"用户【{NickName}】解冻积分事务失败，已回滚：{ex.Message}");
                        throw; // 重新抛出异常
                    }
                }
            }
            
            // 立即同步到磁盘，防止数据丢失
            try
            {
                DBHelperSQLite.FlushWalToMainDB();
            }
            catch (Exception ex)
            {
                LogUtil.LogEx(ex);
            }
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
            
            // 记录从数据库加载的冻结积分
            LogUtil.Log($"用户【{nickName}】从数据库加载：积分={jiFen}，冻结积分={frozenJifen}");
            
            // 修复注单加载：添加详细日志和异常处理
            try
            {
                if (RobotClient.CurrentResult != null)
                {
                    this.RecordList = BetRecordDao.GetByUserAndResult(this, RobotClient.CurrentResult);
                    LogUtil.Log($"用户 {nickName}({userId}) 加载注单数据：期次{RobotClient.CurrentResult.Issue}, 注单数量{RecordList.Count}");
                }
                else
                {
                    this.RecordList = new List<BetRecord>();
                    LogUtil.Log($"警告：用户 {nickName}({userId}) 无法加载注单数据，CurrentResult为null");
                }
            }
            catch (Exception ex)
            {
                this.RecordList = new List<BetRecord>();
                LogUtil.Log($"错误：用户 {nickName}({userId}) 加载注单数据失败：{ex.Message}");
            }
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

            // 使用数据库事务确保下注记录保存和积分冻结的原子性
            using (var connection = new SQLiteConnection(DBHelperSQLite.connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. 保存下注记录到数据库（在事务中）
                        string sql = @"insert into t_bet_record (result_id, user_code, bet_type, keyword, amount, fp, status, remark, is_water, is_deleted) 
                            values (@resultId, @userCode, @betType, @keyword, @amount, @fp, @status, @remark, @isWater, @isDeleted)";
                        
                        foreach (BetRecord record in betRecords)
                        {
                            using (var command = new SQLiteCommand(sql, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@resultId", record.ResultId);
                                command.Parameters.AddWithValue("@userCode", record.UserCode);
                                command.Parameters.AddWithValue("@betType", (int)record.BetType);
                                command.Parameters.AddWithValue("@keyword", record.Keyword);
                                command.Parameters.AddWithValue("@amount", record.Amount);
                                command.Parameters.AddWithValue("@fp", record.Fp);
                                command.Parameters.AddWithValue("@status", (int)record.Status);
                                command.Parameters.AddWithValue("@remark", record.Remark);
                                command.Parameters.AddWithValue("@isWater", record.IsWater);
                                command.Parameters.AddWithValue("@isDeleted", record.IsDeleted);
                                
                                command.ExecuteNonQuery();
                                // 获取自增ID
                                command.CommandText = "SELECT last_insert_rowid()";
                                record.Id = Convert.ToInt32(command.ExecuteScalar());
                                command.CommandText = sql; // 重置SQL
                            }
                        }

                        // 2. 冻结积分（在事务中）
                        int freezeAmount = betRecords.Sum(b => b.Amount);
                        if (Jifen - freezeAmount < 0)
                        {
                            throw new Exception($"【{NickName}】积分不足！");
                        }
                        
                        FrozenJifen += freezeAmount;
                        
                        // 更新用户信息到数据库（在事务中）
                        string updateUserSql = @"update t_user set nick_name=@nickName, jifen=@jifen, frozen_jifen=@frozenJifen,
                            slyk=@slyk, status=@status, is_dummy=@isDummy where code=@code";
                        using (var updateCommand = new SQLiteCommand(updateUserSql, connection, transaction))
                        {
                            updateCommand.Parameters.AddWithValue("@nickName", this.NickName);
                            updateCommand.Parameters.AddWithValue("@jifen", this.Jifen);
                            updateCommand.Parameters.AddWithValue("@frozenJifen", this.FrozenJifen);
                            updateCommand.Parameters.AddWithValue("@slyk", this.Slyk);
                            updateCommand.Parameters.AddWithValue("@status", (int)this.Status);
                            updateCommand.Parameters.AddWithValue("@isDummy", this.IsDummy);
                            updateCommand.Parameters.AddWithValue("@code", this.UserId);
                            updateCommand.ExecuteNonQuery();
                        }

                        // 提交事务
                        transaction.Commit();
                        
                        // 3. 只有事务成功后才更新内存状态
                        RecordList.AddRange(betRecords);
                        OnPropertyChanged();
                        
                        LogUtil.Log($"用户【{NickName}】下注事务提交成功，记录数：{betRecords.Count}，冻结积分：{freezeAmount}");
                    }
                    catch (Exception ex)
                    {
                        // 回滚事务
                        transaction.Rollback();
                        LogUtil.Log($"用户【{NickName}】下注事务失败，已回滚：{ex.Message}");
                        throw new Exception($"下注失败：{ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 清空本期下注
        /// </summary>
        /// <param name="remark">备注</param>
        /// <param name="forceUnfreeze">是否强制解冻积分（用于改单等场景）</param>
        public void ClearBetRecords(string remark = "", bool forceUnfreeze = false)
        {
            if(RecordList.Count == 0)
            {
                return;
            }
            int sumAmount = RecordList.Sum(bet => bet.Amount);
            if (BetRecordDao.Delete(RecordList, remark))
            {
                RecordList.Clear();
                // 如果还没封盘或者强制解冻（改单场景），则解冻积分
                if (RobotClient.CurrentResult.Status < ResultStatus.已封盘 || forceUnfreeze)
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

        /// <summary>
        /// 修复积分冻结不一致问题
        /// 检查用户的下注记录总额与冻结积分是否一致，如果不一致则自动修复
        /// </summary>
        public void FixFrozenJifenInconsistency()
        {
            try
            {
                // 智能判断是否需要修复冻结积分
                // 如果程序刚重启且CurrentResult为null，不进行修复，保持数据库中的冻结积分
                if (RobotClient.CurrentResult == null)
                {
                    LogUtil.Log($"用户【{NickName}】程序重启检测：当前期次为null，保持冻结积分={FrozenJifen}，跳过一致性检查");
                    return;
                }
                
                // 计算当前期次的实际下注总额
                int actualBetAmount = RecordList.Sum(br => br.Amount);
                
                // 只有在明确有期次信息且下注记录与冻结积分不匹配时才修复
                if (FrozenJifen != actualBetAmount)
                {
                    // 进一步判断：如果RecordList为空但冻结积分不为0，可能是记录加载问题
                    if (RecordList.Count == 0 && FrozenJifen > 0)
                    {
                        LogUtil.Log($"用户【{NickName}】检测到：冻结积分={FrozenJifen}，但内存中无下注记录，可能是记录加载问题，保持原冻结积分");
                        return; // 不修复，可能是记录还未加载完全
                    }
                    
                    LogUtil.Log($"用户【{NickName}】积分冻结不一致：冻结积分={FrozenJifen}，实际下注={actualBetAmount}，期次={RobotClient.CurrentResult.Issue}，正在修复...");
                    
                    int oldFrozenJifen = FrozenJifen;
                    
                    // 使用数据库事务确保修复操作的原子性和持久性
                    using (var connection = new SQLiteConnection(DBHelperSQLite.connectionString))
                    {
                        connection.Open();
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                // 设置正确的冻结积分
                                FrozenJifen = actualBetAmount;
                                
                                // 更新用户信息到数据库
                                string updateUserSql = @"update t_user set nick_name=@nickName, jifen=@jifen, frozen_jifen=@frozenJifen,
                                    slyk=@slyk, status=@status, is_dummy=@isDummy where code=@code";
                                using (var updateCommand = new SQLiteCommand(updateUserSql, connection, transaction))
                                {
                                    updateCommand.Parameters.AddWithValue("@nickName", this.NickName);
                                    updateCommand.Parameters.AddWithValue("@jifen", this.Jifen);
                                    updateCommand.Parameters.AddWithValue("@frozenJifen", this.FrozenJifen);
                                    updateCommand.Parameters.AddWithValue("@slyk", this.Slyk);
                                    updateCommand.Parameters.AddWithValue("@status", (int)this.Status);
                                    updateCommand.Parameters.AddWithValue("@isDummy", this.IsDummy);
                                    updateCommand.Parameters.AddWithValue("@code", this.UserId);
                                    updateCommand.ExecuteNonQuery();
                                }

                                // 提交事务
                                transaction.Commit();
                                
                                LogUtil.Log($"用户【{NickName}】积分冻结修复事务提交成功：{oldFrozenJifen} -> {FrozenJifen}");
                            }
                            catch (Exception ex)
                            {
                                // 回滚事务
                                transaction.Rollback();
                                FrozenJifen = oldFrozenJifen; // 恢复内存状态
                                
                                LogUtil.Log($"用户【{NickName}】积分冻结修复事务失败，已回滚：{ex.Message}");
                                throw; // 重新抛出异常
                            }
                        }
                    }
                    
                    // 立即同步到磁盘，防止数据丢失
                    try
                    {
                        DBHelperSQLite.FlushWalToMainDB();
                    }
                    catch (Exception ex)
                    {
                        LogUtil.LogEx(ex);
                    }
                    
                    LogUtil.Log($"用户【{NickName}】积分冻结已修复并同步：冻结积分={FrozenJifen}");
                }
                else
                {
                    LogUtil.Log($"用户【{NickName}】冻结积分检查完成：冻结积分={FrozenJifen}，实际下注={actualBetAmount}，数据一致");
                }
            }
            catch (Exception ex)
            {
                LogUtil.Log($"修复用户【{NickName}】积分冻结失败：{ex.Message}");
            }
        }
    }
}
