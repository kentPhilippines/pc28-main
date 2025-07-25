using RobotApp.IM;
using RobotApp.Model;
using RobotApp.Util;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Runtime.Versioning;

namespace RobotApp.Dao
{
    [SupportedOSPlatform("windows")]
    internal class BetRecordDao
    {
        private BetRecordDao() { }

        public static void Add(List<BetRecord> recordList)
        {
            string sql = @"insert into t_bet_record (result_id, user_code, bet_type, keyword, amount, fp, status, remark, is_water, is_deleted) 
                values (@resultId, @userCode, @betType, @keyword, @amount, @fp, @status, @remark, @isWater, @isDeleted)";
            foreach (BetRecord record in recordList)
            {
                int autoId = DBHelperSQLite.ExecuteSqlReturnAutoIncrId(sql, new SQLiteParameter[] {
                    new SQLiteParameter("@resultId", record.ResultId),
                    new SQLiteParameter("@userCode", record.UserCode),
                    new SQLiteParameter("@betType", (int)record.BetType),
                    new SQLiteParameter("@keyword", record.Keyword),
                    new SQLiteParameter("@amount", record.Amount),
                    new SQLiteParameter("@fp", record.Fp),
                    new SQLiteParameter("@status", (int)record.Status),
                    new SQLiteParameter("@remark", record.Remark),
                    new SQLiteParameter("@isWater", record.IsWater),
                    new SQLiteParameter("@isDeleted", record.IsDeleted),
                });
                record.Id = autoId;
            }
        }

        public static bool Delete(List<BetRecord> recordList, string remark)
        {
            if (recordList.Count == 0)
            {
                return true;
            }
            int[] ids = new int[recordList.Count];
            for (int i = 0; i < recordList.Count; i++)
            {
                ids[i] = recordList[i].Id;
            }
            string sql = $"update t_bet_record set is_deleted=1, remark='{remark}' where id in ({string.Join(",", ids)})";
            return DBHelperSQLite.ExecuteSql(sql) > 0;
        }

        public static void Save(List<BetRecord> recordList)
        {
            foreach (var record in recordList)
            {
                string sql = @"update t_bet_record set odds=@odds, award=@award, status=@status, remark=@remark, is_water=@isWater where id=@id";
                DBHelperSQLite.ExecuteSql(sql, new SQLiteParameter[] {
                    new SQLiteParameter("@odds", record.Odds),
                    new SQLiteParameter("@award", record.Award),
                    new SQLiteParameter("@status", (int)record.Status),
                    new SQLiteParameter("@remark", record.Remark),
                    new SQLiteParameter("@isWater", record.IsWater),
                    new SQLiteParameter("@id", record.Id),
                });
            }
        }

        public static List<BetRecord> GetByUserAndResult(User user, Result result)
        {
            List<BetRecord> recordList = new List<BetRecord>();
            if (result == null)
            {
                return recordList;
            }
            string sql = @"select id, result_id, user_code, bet_type, keyword, amount, odds, award, fp, status, remark, is_water
                                  from t_bet_record where is_deleted=0 and user_code=@userCode and result_id=@resultId";
            SQLiteDataReader sdr = DBHelperSQLite.ExecuteReader(sql, new SQLiteParameter[] {
                new SQLiteParameter("@userCode", user.UserId),
                new SQLiteParameter("@resultId", result.Id),
            });
            
            while (sdr.Read())
            {
                BetRecord record = new BetRecord();
                record.Id = sdr.GetInt32(0);
                record.ResultId = sdr.GetInt32(1);
                record.UserCode = sdr.GetString(2);
                record.BetType = (BetType)sdr.GetInt32(3);
                record.Keyword = sdr.GetString(4);
                record.Amount = sdr.GetInt32(5);
                if (!sdr.IsDBNull(6))
                {
                    record.Odds = sdr.GetDouble(6);
                }
                if (!sdr.IsDBNull(7))
                {
                    record.Award = sdr.GetInt32(7);
                }
                record.Fp = sdr.GetString(8);
                record.Status = (BetRecordStatus)sdr.GetInt32(9);
                if (!sdr.IsDBNull(10))
                {
                    record.Remark = sdr.GetString(10);
                }
                record.IsWater = sdr.GetInt32(11)==1;
                recordList.Add(record);
            }
            sdr.Close();
            return recordList;
        }
    }
}
