using RobotApp.IM;
using RobotApp.Model;
using RobotApp.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace RobotApp.Dao
{
    [SupportedOSPlatform("windows")]
    internal class RecordDao
    {
        private RecordDao() { }

        public static bool AddRecord(Record record)
        {
            string addRecordSql = @"
                    insert into t_record (code, old_amount, amount, now_amount, type, remark, create_time, calc_time, create_user, score_id) 
                    values (@code, @oldAmount, @amount, @nowAmount, @type, @remark, @createTime, @calcTime, @createUser, @scoreId)";
            int count = DBHelperSQLite.ExecuteSql(addRecordSql, new SQLiteParameter[] {
                    new SQLiteParameter("@code", record.Code),
                    new SQLiteParameter("@oldAmount", record.OldAmount),
                    new SQLiteParameter("@amount", record.Amount),
                    new SQLiteParameter("@nowAmount", record.NowAmount),
                    new SQLiteParameter("@type", (int)record.type),
                    new SQLiteParameter("@remark", record.Remark),
                    new SQLiteParameter("@createTime", record.CreateTime),
                    new SQLiteParameter("@calcTime", record.CalcTime),
                    new SQLiteParameter("@createUser", record.CreateUser),
                    new SQLiteParameter("@scoreId", record.ScoreId),
            });
            return count > 0;
        }

        public static DataTable List(DateTime begin, DateTime end, string nickName, object recordType = null)
        {
            string sql = @"select a.code, b.nick_name, a.old_amount, a.amount, a.now_amount, a.type, a.create_time, a.remark
                    from t_record a left join t_user b on a.code=b.code 
                    where b.is_dummy=0 and a.create_time between @beginTime and @endTime and b.nick_name like @nickName";
            if (recordType != null)
            {
                sql += " and a.type=@recordType";
            }
                        
            sql+= " order by a.create_time desc";
            SQLiteParameter[] parpms = new SQLiteParameter[] {
                    new SQLiteParameter("@beginTime", begin),
                    new SQLiteParameter("@endTime", end),
                    new SQLiteParameter("@nickName", "%"+nickName+"%"),
            };
            if (recordType != null)
            {
                List<SQLiteParameter> parameterList = new List<SQLiteParameter>(parpms);
                parameterList.Add(new SQLiteParameter("@recordType", (int)recordType));
                parpms = parameterList.ToArray();
            }
            SQLiteDataReader sdr = DBHelperSQLite.ExecuteReader(sql, parpms);
            DataTable dt = new DataTable();
            dt.Columns.Add("No", typeof(string));
            dt.Columns.Add("用户昵称", typeof(string));
            dt.Columns.Add("原积分", typeof(double));
            dt.Columns.Add("账变积分", typeof(double));
            dt.Columns.Add("现积分", typeof(double));
            dt.Columns.Add("帐变原因", typeof(RecordType));
            dt.Columns.Add("帐变时间", typeof(DateTime));
            dt.Columns.Add("备注", typeof(string));
            int i = 1;
            while (sdr.Read())
            {
                dt.Rows.Add(i++,
                    sdr.GetString(1),
                    sdr.GetDouble(2),
                    sdr.GetDouble(3),
                    sdr.GetDouble(4),
                    (RecordType)sdr.GetInt32(5),
                    sdr.GetDateTime(6),
                    sdr.IsDBNull(7) ? "" : sdr.GetString(7)
                );
            }
            sdr.Close();
            return dt;
        }

        /// <summary>
        /// 回水记录
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="nickName"></param>
        /// <returns></returns>
        public static DataTable Water(DateTime date)
        {
            DateTime date2 = date.AddDays(1);
            string typeStr = string.Join(",", new int[] { (int)RecordType.退分, (int)RecordType.退分撤消, (int)RecordType.自动回水, (int)RecordType.退自动回水, (int)RecordType.自动流水 });
            string sql = @"select b.nick_name, a.amount, a.type, a.remark, a.create_time
                    from t_record a left join t_user b on a.code=b.code
                    where a.create_time between @beginTime and @endTime and b.is_dummy=0 and a.type in (" + typeStr+") order by a.create_time desc";
            SQLiteDataReader sdr = DBHelperSQLite.ExecuteReader(sql, new SQLiteParameter[] {
                    new SQLiteParameter("@beginTime", date),
                    new SQLiteParameter("@endTime", date2),
            });
            DataTable dt = new DataTable();
            dt.Columns.Add("序号", typeof(int));
            dt.Columns.Add("玩家", typeof(string));
            dt.Columns.Add("回水积分", typeof(double));
            dt.Columns.Add("类型", typeof(RecordType));
            dt.Columns.Add("备注", typeof(string));
            dt.Columns.Add("时间", typeof(string));
            int i = 1;
            while (sdr.Read())
            {
                dt.Rows.Add(i++,
                    sdr.GetString(0),
                    sdr.GetDouble(1),
                    (RecordType)sdr.GetInt32(2),
                    sdr.IsDBNull(3) ? "" : sdr.GetString(3),
                    sdr.GetDateTime(4).ToString(RobotClient.DateTimeFormat)
                );
            }
            sdr.Close();
            return dt;
        }

        /// <summary>
        /// 今日自动流水次数
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public static int GetTodayAutoRunningWaterCount(string userCode)
        {
            string sql = @"select count(1)
                from t_record a
                where strftime('%Y-%m-%d', a.create_time) = strftime('%Y-%m-%d', 'now', 'localtime') and a.code=@userCode and a.type=6";
            object single = DBHelperSQLite.GetSingle(sql, new SQLiteParameter[] {
                new SQLiteParameter("@userCode", userCode)
            });
            return single == null ? 0 : Convert.ToInt32(single);
        }

        /// <summary>
        /// 今日自动回水水次数
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public static int GetTodayAutoReturnWaterCount(string userCode)
        {
            string sql = @"select count(1)
                from t_record a
                where strftime('%Y-%m-%d', a.create_time) = strftime('%Y-%m-%d', 'now', 'localtime') and a.code=@userCode and a.type=7";
            object single = DBHelperSQLite.GetSingle(sql, new SQLiteParameter[] {
                new SQLiteParameter("@userCode", userCode)
            });
            return single == null ? 0 : Convert.ToInt32(single);
        }

        /// <summary>
        /// 获取指定日期上分总额
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="day">指定日期，格式yyyy-MM-dd</param>
        /// <returns></returns>
        public static double GetShangFenSum(string userCode, string day)
        {
            string sql = @"select sum(amount)
                from t_record a
                where strftime('%Y-%m-%d', a.create_time) = strftime('%Y-%m-%d', 'now', 'localtime') and a.code=@userCode and a.type=1";
            object single = DBHelperSQLite.GetSingle(sql, new SQLiteParameter[] {
                new SQLiteParameter("@userCode", userCode)
            });
            return single == null ? 0 : Convert.ToDouble(single);
        }

        /// <summary>
        /// 当日所有返水总和
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public static double GetTotalWater(string userCode)
        {
            string sql = @"select sum(a.amount)
                from t_record a
                where strftime('%Y-%m-%d', a.create_time) = strftime('%Y-%m-%d', 'now', 'localtime') and a.code=@userCode and a.type in (5,6,7,8)";
            object waterSum = DBHelperSQLite.GetSingle(sql, new SQLiteParameter[] {
                new SQLiteParameter("@userCode", userCode),
            });
            return waterSum==null?0:Convert.ToDouble(waterSum);
        }
    }
}
