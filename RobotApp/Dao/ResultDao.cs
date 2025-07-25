using RobotApp.IM;
using RobotApp.Model;
using RobotApp.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace RobotApp.Dao
{
    [SupportedOSPlatform("windows")]
    internal class ResultDao
    {
        private ResultDao() { }

        public static bool AddResult(Result result)
        {
            string addResultSql = @"
                    insert or ignore into t_result (type, issue, open_time, num1, num2, num3, sum, amount, award, open_result, status, next_issue, next_time, remark) 
                    values (@type, @issue, @openTime, @num1, @num2, @num3, @sum, @amount, @award, @openResult, @status, @nextIssue, @nextTime, @remark)";
            int rowid = DBHelperSQLite.ExecuteSqlReturnAutoIncrId(addResultSql, new SQLiteParameter[] {
                    new SQLiteParameter("@type", (int)result.Type),
                    new SQLiteParameter("@issue", result.Issue),
                    new SQLiteParameter("@openTime", result.OpenTime),
                    new SQLiteParameter("@num1", result.Num1),
                    new SQLiteParameter("@num2", result.Num2),
                    new SQLiteParameter("@num3", result.Num3),
                    new SQLiteParameter("@sum", result.Sum),
                    new SQLiteParameter("@amount", result.Amount),
                    new SQLiteParameter("@award", result.Award),
                    new SQLiteParameter("@openResult", result.OpenResult),
                    new SQLiteParameter("@status", (int)result.Status),
                    new SQLiteParameter("@nextIssue", result.NextIssue),
                    new SQLiteParameter("@nextTime", result.NextTime),
                    new SQLiteParameter("@remark", result.Remark),
            });
            result.Id = rowid;
            return rowid > 0;
        }
        /// <summary>
        /// 保存开奖
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool Save(Result result)
        {
            string addResultSql = @"update t_result set num1=@num1, num2=@num2, num3=@num3, sum=@sum, amount=@amount, award=@award, open_result=@openResult, status=@status, next_issue=@nextIssue, next_time=@nextTime, remark=@remark where id=@id";
            int count = DBHelperSQLite.ExecuteSql(addResultSql, new SQLiteParameter[] {
                    new SQLiteParameter("@num1", result.Num1),
                    new SQLiteParameter("@num2", result.Num2),
                    new SQLiteParameter("@num3", result.Num3),
                    new SQLiteParameter("@sum", result.Sum),
                    new SQLiteParameter("@amount", result.Amount),
                    new SQLiteParameter("@award", result.Award),
                    new SQLiteParameter("@openResult", result.OpenResult),
                    new SQLiteParameter("@status", (int)result.Status),
                    new SQLiteParameter("@nextIssue", result.NextIssue),
                    new SQLiteParameter("@nextTime", result.NextTime),
                    new SQLiteParameter("@remark", result.Remark),
                    new SQLiteParameter("@id", result.Id),
            });
            return count > 0;
        }

        /// <summary>
        /// 保存指定时间内开奖结果状态
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static bool SaveStatus(DateTime begin, DateTime end, ResultStatus status, ResultStatus newStatus)
        {
            string addResultSql = @"update t_result set status=@newStatus where status=@status and open_time >= @begin and open_time < @end";
            int count = DBHelperSQLite.ExecuteSql(addResultSql, new SQLiteParameter[] {
                    new SQLiteParameter("@begin", begin),
                    new SQLiteParameter("@end", end),
                    new SQLiteParameter("@status", (int)status),
                    new SQLiteParameter("@newStatus", (int)newStatus),
            });
            return count > 0;
        }

        /// <summary>
        /// 获取指定期数开奖结果
        /// </summary>
        /// <param name="type"></param>
        /// <param name="issue"></param>
        /// <returns></returns>
        public static Result GetResultByIssue(LotterCode type, int issue)
        {
            string getUserSql = @"select id, issue, open_time, num1, num2, num3, sum, amount, award, remark, open_result, status, next_issue, next_time 
                        from t_result where type=@type and issue=@issue";
            SQLiteDataReader sdr = DBHelperSQLite.ExecuteReader(getUserSql, new SQLiteParameter[] {
                new SQLiteParameter("@issue", issue),
                new SQLiteParameter("@type", (int)type),
            });
            if (sdr.Read())
            {
                Result result = new Result();
                result.Id = sdr.GetInt32(0);
                result.Type = type;
                result.Issue = sdr.GetInt32(1);
                result.OpenTime = sdr.GetDateTime(2);
                if (!sdr.IsDBNull(3))
                {
                    result.Num1 = sdr.GetInt32(3);
                }
                if (!sdr.IsDBNull(4))
                {
                    result.Num2 = sdr.GetInt32(4);
                }
                if (!sdr.IsDBNull(5))
                {
                    result.Num3 = sdr.GetInt32(5);
                }
                if (!sdr.IsDBNull(6))
                {
                    result.Sum = sdr.GetInt32(6);
                }
                if (!sdr.IsDBNull(7))
                {
                    result.Amount = sdr.GetDouble(7);
                }
                if (!sdr.IsDBNull(8))
                {
                    result.Award = sdr.GetDouble(8);
                }
                if (!sdr.IsDBNull(9))
                {
                    result.Remark = sdr.GetString(9);
                }
                if (!sdr.IsDBNull(10))
                { 
                    result.OpenResult = sdr.GetString(10);
                }
                result.Status = (ResultStatus)sdr.GetInt32(11);
                if (!sdr.IsDBNull(12))
                {
                    result.NextIssue = sdr.GetInt32(12);
                }
                if (!sdr.IsDBNull(13))
                {
                    result.NextTime = sdr.GetDateTime(13);
                }
                sdr.Close();
                return result;
            }
            return null;
        }

        /// <summary>
        /// 获取今日开奖结果
        /// </summary>
        /// <returns></returns>
        public static List<Result> GetResult(LotterCode type)
        {
            List<Result> results = new List<Result>();
            string getUserSql = @"select id, issue, open_time, num1, num2, num3, sum, amount, award, remark, open_result, status, next_issue, next_time 
                from t_result where type=@type and strftime('%Y-%m-%d', open_time) = strftime('%Y-%m-%d', 'now', 'localtime') order by issue desc";
            SQLiteDataReader sdr = DBHelperSQLite.ExecuteReader(getUserSql, new SQLiteParameter[] {
                new SQLiteParameter("@type", (int)type)
            });
            while (sdr.Read())
            {
                Result result = new Result();
                result.Id = sdr.GetInt32(0);
                result.Type = type;
                result.Issue = sdr.GetInt32(1);
                result.OpenTime = sdr.GetDateTime(2);
                if (!sdr.IsDBNull(3))
                {
                    result.Num1 = sdr.GetInt32(3);
                }
                if (!sdr.IsDBNull(4))
                {
                    result.Num2 = sdr.GetInt32(4);
                }
                if (!sdr.IsDBNull(5))
                {
                    result.Num3 = sdr.GetInt32(5);
                }
                if (!sdr.IsDBNull(6))
                {
                    result.Sum = sdr.GetInt32(6);
                }
                if (!sdr.IsDBNull(7))
                {
                    result.Amount = sdr.GetDouble(7);
                }
                if(!sdr.IsDBNull(8))
                {
                    result.Award = sdr.GetDouble(8);
                }
                if (!sdr.IsDBNull(9))
                {
                    result.Remark = sdr.GetString(9);
                }
                if (!sdr.IsDBNull(10))
                {
                    result.OpenResult = sdr.GetString(10);
                }
                result.Status = (ResultStatus)sdr.GetInt32(11);
                if (!sdr.IsDBNull(12))
                {
                    result.NextIssue = sdr.GetInt32(12);
                }
                if (!sdr.IsDBNull(13))
                {
                    result.NextTime = sdr.GetDateTime(13);
                }
                results.Add(result);                
            }
            sdr.Close();
            return results;
        }

        /// <summary>
        /// 获取指定状态开奖结果
        /// </summary>
        /// <returns></returns>
        public static List<Result> GetResult(ResultStatus status)
        {
            List<Result> results = new List<Result>();
            string getUserSql = @"select id, issue, open_time, num1, num2, num3, sum, amount, award, remark, open_result, status, next_issue, next_time, type
                from t_result where status=@status";
            SQLiteDataReader sdr = DBHelperSQLite.ExecuteReader(getUserSql, new SQLiteParameter[] {
                new SQLiteParameter("@status", (int)status),
            });
            while (sdr.Read())
            {
                Result result = new Result();
                result.Id = sdr.GetInt32(0);
                result.Issue = sdr.GetInt32(1);
                result.OpenTime = sdr.GetDateTime(2);
                if (!sdr.IsDBNull(3))
                {
                    result.Num1 = sdr.GetInt32(3);
                }
                if (!sdr.IsDBNull(4))
                {
                    result.Num2 = sdr.GetInt32(4);
                }
                if (!sdr.IsDBNull(5))
                {
                    result.Num3 = sdr.GetInt32(5);
                }
                if (!sdr.IsDBNull(6))
                {
                    result.Sum = sdr.GetInt32(6);
                }
                if (!sdr.IsDBNull(7))
                {
                    result.Amount = sdr.GetDouble(7);
                }
                if (!sdr.IsDBNull(8))
                {
                    result.Award = sdr.GetDouble(8);
                }
                if (!sdr.IsDBNull(9))
                {
                    result.Remark = sdr.GetString(9);
                }
                if (!sdr.IsDBNull(10))
                {
                    result.OpenResult = sdr.GetString(10);
                }
                result.Status = (ResultStatus)sdr.GetInt32(11);
                if (!sdr.IsDBNull(12))
                {
                    result.NextIssue = sdr.GetInt32(12);
                }
                if (!sdr.IsDBNull(13))
                {
                    result.NextTime = sdr.GetDateTime(13);
                }
                result.Type = (LotterCode)sdr.GetInt32(14);
                results.Add(result);
            }
            sdr.Close();
            return results;
        }

        /// <summary>
        /// 获取近n期开奖结果
        /// </summary>
        /// <returns></returns>
        public static string GetNResult(LotterCode type, int n)
        {
            List<Result> results = new List<Result>();
            string getUserSql = @"select sum from t_result where type=@type and sum not null order by open_time desc limit "+ n;
            SQLiteDataReader sdr = DBHelperSQLite.ExecuteReader(getUserSql, new SQLiteParameter[] {
                new SQLiteParameter("@type", (int)type),
            });
            string result = "";
            while (sdr.Read())
            {
                if (sdr.IsDBNull(0))
                {
                    result = "ERR " + result;
                }
                else
                {
                    result = sdr.GetInt32(0) + " " + result;
                }                    
            }
            sdr.Close();
            return result.TrimEnd();
        }
    }
}
