using RobotApp.Model;
using RobotApp.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RobotApp.Dao
{
    [SupportedOSPlatform("windows")]
    internal class BettingDao
    {
        private BettingDao() { }

        public static void AddBetting(Result result, User user)
        {
            string addSql = @"insert into t_bettings (code, result_id, jifen, slyk, blyk, blzf, bl_water, ccnr, is_dummy, amount_zh, amount_single, amount_jdjx, amount_dsxd, is_szh, is_txzh, is_fxzh) 
                values (@code, @resultId, @jifen, @slyk, @blyk, @blzf, @blWater, @ccnr, @isDummy, @amountZh, @amountSingle, @amountJdjx, @amountDsxd, @isSzh, @isTxzh, @isFxzh)";
            DBHelperSQLite.ExecuteSql(addSql, new SQLiteParameter[] {
                new SQLiteParameter("@code", user.UserId),
                new SQLiteParameter("@resultId", result.Id),
                new SQLiteParameter("@jifen", user.Jifen + user.Award),
                new SQLiteParameter("@slyk", user.Slyk),
                new SQLiteParameter("@blyk", user.Blyk),
                new SQLiteParameter("@blzf", user.Blzf),
                new SQLiteParameter("@blWater", user.BlWater),
                new SQLiteParameter("@ccnr", user.Ccnr),
                new SQLiteParameter("@isDummy", user.IsDummy),
                new SQLiteParameter("@amountZh", user.Sum组合),
                new SQLiteParameter("@amountSingle", user.Sum单点),
                new SQLiteParameter("@amountJdjx", user.Sum极值),
                new SQLiteParameter("@amountDsxd", user.Sum大双小单),
                new SQLiteParameter("@isSzh", user.Is杀组合()),
                new SQLiteParameter("@isTxzh", user.Is同向组合()),
                new SQLiteParameter("@isFxzh", user.Is反向组合()),
            });
        }

        public static void AddBettings(Result result, List<User> userList)
        {
            foreach (var user in userList)
            {
                string addSql = @"insert into t_bettings (code, result_id, jifen, slyk, blyk, blzf, bl_water, ccnr, is_dummy) 
                    values (@code, @billName, @issue, @jifen, @slyk, @blyk, @blzf, @ccnr, @isDummy)";
                DBHelperSQLite.ExecuteSql(addSql, new SQLiteParameter[] {
                    new SQLiteParameter("@code", user.UserId),
                    new SQLiteParameter("@resultId", result.Id),
                    new SQLiteParameter("@jifen", user.Jifen),
                    new SQLiteParameter("@slyk", user.Slyk),
                    new SQLiteParameter("@blyk", user.Blyk),
                    new SQLiteParameter("@blzf", user.Blzf),
                    new SQLiteParameter("@blWater", user.BlWater),
                    new SQLiteParameter("@ccnr", user.Ccnr),
                    new SQLiteParameter("@isDummy", user.IsDummy),
                });
            }
        }

        public static DataTable GetBettingList(DateTime begin, DateTime end, string nickName, int isDummy)
        {
            string sql = @"select b.open_time, b.issue, b.num1, b.num2, b.num3, c.nick_name, a.ccnr, a.blzf, a.blyk, a.jifen, a.is_dummy, a.bl_water
                           from t_bettings a left join t_result b on a.result_id=b.id 
                               left join t_user c on a.code=c.code
                           where b.open_time >= @begin and b.open_time < @end and a.blzf>0 and (c.nick_name=@nickName or ''=@nickName)";
            if (isDummy > -1)
            {
                sql += " and a.is_dummy="+isDummy;
            }
            sql += " order by b.issue desc";
            SQLiteDataReader sdr = DBHelperSQLite.ExecuteReader(sql, new SQLiteParameter[] {
                new SQLiteParameter("@begin", begin),
                new SQLiteParameter("@end", end),
                new SQLiteParameter("@nickName", nickName),
            });
            List<Betting> bettingList = new List<Betting>();
            DataTable dt = new DataTable();
            dt.Columns.Add("开奖时间", typeof(DateTime));
            dt.Columns.Add("期号", typeof(int));
            dt.Columns.Add("开奖号码", typeof(string));
            dt.Columns.Add("昵称", typeof(string));
            dt.Columns.Add("猜猜内容", typeof(string));            
            dt.Columns.Add("本局总分", typeof(int));
            dt.Columns.Add("输赢总分", typeof(int));
            dt.Columns.Add("剩余总分", typeof(int));
            dt.Columns.Add("是否假人", typeof(bool));
            dt.Columns.Add("流水", typeof(int));
            while (sdr.Read())
            {
                int num1 = sdr.GetInt32(2);
                int num2 = sdr.GetInt32(3);
                int num3 = sdr.GetInt32(4);
                string kjhm = string.Format("{0}+{1}+{2}={3}", num1, num2, num3, num1+num2+num3);
                dt.Rows.Add(sdr.GetDateTime(0), sdr.GetInt32(1), kjhm, sdr.GetString(5), sdr.GetString(6), sdr.GetInt32(7), sdr.GetInt32(8), sdr.GetInt32(9), sdr.GetBoolean(10), sdr.GetInt32(11));
            }
            sdr.Close();
            return dt;
        }

        public static DataTable CalcWater(DateTime begin, DateTime end, int isDummy)
        {
            string sql = @"SELECT c.nick_name 昵称, sum(a.bl_water) 流水, sum(a.blzf>0) 竞猜期数, sum(a.blyk) 盈亏, sum(a.blzf) 投注总额,sum(a.amount_zh>0) 组合期数, 
		                        a.code, sum(a.amount_zh) 组合分数, sum(a.amount_single>0 || a.amount_jdjx>0) 单点极值期数, sum(a.amount_dsxd) 大双小单分数, 
		                        sum(a.is_szh) 杀组合期数, sum(a.is_txzh) 同向组合期数, sum(a.is_fxzh) 反向组合期数, a.is_dummy 是否假人
                        FROM t_bettings a left join t_result b on a.result_id=b.id 
	                        left join t_user c on a.code=c.code
                        where b.open_time >= @begin and b.open_time < @end";                        
            if (isDummy > -1) {
                sql = sql + " and a.is_dummy="+isDummy;
            }
            sql = sql + " GROUP BY a.code";
            SQLiteDataReader sdr = DBHelperSQLite.ExecuteReader(sql, new SQLiteParameter[] {
                new SQLiteParameter("@begin", begin),
                new SQLiteParameter("@end", end),
            });
            List<Betting> bettingList = new List<Betting>();
            DataTable dt = new DataTable();
            dt.Columns.Add("No", typeof(int));
            dt.Columns.Add("昵称", typeof(string));
            dt.Columns.Add("流水", typeof(int));
            dt.Columns.Add("竞猜期数", typeof(int));
            dt.Columns.Add("盈亏", typeof(int));
            dt.Columns.Add("投注总额", typeof(int));
            dt.Columns.Add("组合期数", typeof(int));
            dt.Columns.Add("Code", typeof(string));
            dt.Columns.Add("组合分数", typeof(int));
            dt.Columns.Add("单点极值期数", typeof(int));
            dt.Columns.Add("大双小单分数", typeof(int));
            dt.Columns.Add("杀组合期数", typeof(int));
            dt.Columns.Add("同向组合期数", typeof(int));
            dt.Columns.Add("反向组合期数", typeof(int));
            dt.Columns.Add("是否假人", typeof(bool));
            int i = 1;
            while (sdr.Read())
            {
                dt.Rows.Add(i++, sdr.GetString(0), sdr.GetInt32(1), sdr.GetInt32(2), sdr.GetInt32(3), sdr.GetInt32(4), sdr.GetInt32(5), sdr.GetString(6),
                    sdr.GetInt32(7), sdr.GetInt32(8), sdr.GetInt32(9), sdr.GetInt32(10), sdr.GetInt32(11), sdr.GetInt32(12), sdr.GetBoolean(13));
            }
            sdr.Close();

            return dt;
        }

        /// <summary>
        /// 获取指定日期所有已退分数
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static DataTable GetAllReturnJifen(DateTime begin, DateTime end)
        {
            string sql = "SELECT code, type, sum(amount) sumAmount FROM t_record where type in (5,6,7,8,9) and calc_time >= @begin and calc_time < @end GROUP BY code, type";
            DataTable amountDt = DBHelperSQLite.Query(sql, new SQLiteParameter[] {
                new SQLiteParameter("@begin", begin),
                new SQLiteParameter("@end", end),
            });
            return amountDt;
        }

        /// <summary>
        /// 获取指定日期所有下注
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public static DataTable GetBettings(DateTime begin, DateTime end, int isDummy)
        {
            string sql = @"select a.code, sum(a.bl_water) sumWater, sum(a.blyk) sumBlyk
			                from t_bettings a left join t_result b on a.result_id=b.id
			                where b.open_time >= @begin and b.open_time < @end";
            if (isDummy > -1)
            {
                sql = sql + " and a.is_dummy=" + isDummy;
            }
            sql += " GROUP BY a.code";
            DataTable dt = DBHelperSQLite.Query(sql, new SQLiteParameter[] {
                new SQLiteParameter("@begin", begin),
                new SQLiteParameter("@end", end)
            });
            return dt;
        }

        /// <summary>
        /// 获取用户今日流水
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static double GetTodayWaterByUser(User user)
        {
            string sql = @"select sum(a.bl_water)
                from t_bettings a
                left join t_result b on a.result_id=b.id
                where a.code=@userCode and strftime('%Y-%m-%d', b.open_time) = strftime('%Y-%m-%d', 'now', 'localtime')";
            SQLiteDataReader sdr = DBHelperSQLite.ExecuteReader(sql, new SQLiteParameter[] {
                new SQLiteParameter("@userCode", user.UserId),
            });
            double water = 0;
            if (sdr.Read())
            {
                if (!sdr.IsDBNull(0))
                {
                    water = sdr.GetDouble(0);
                }
            }
            sdr.Close();
            return water;
        }

        /// <summary>
        /// 获取用户今日下注期数
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static double GetTodayBetCount(User user)
        {
            string sql = @"select count(a.result_id)
                from t_bettings a
                left join t_result b on a.result_id=b.id
                where a.code=@userCode and strftime('%Y-%m-%d', b.open_time) = strftime('%Y-%m-%d', 'now', 'localtime')";
            SQLiteDataReader sdr = DBHelperSQLite.ExecuteReader(sql, new SQLiteParameter[] {
                new SQLiteParameter("@userCode", user.UserId),
            });
            double water = 0;
            if (sdr.Read())
            {
                if (!sdr.IsDBNull(0))
                {
                    water = sdr.GetDouble(0);
                }
            }
            sdr.Close();
            return water;
        }

        /// <summary>
        /// 获取指定日期未回流水下注，用于计算自动流水
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DataTable GetBettingRunningByDate(string userCode, DateTime date)
        {
            string sql = @"select a.id, a.code, a.bl_water, a.blyk
                from t_bettings a left join t_result b on a.result_id=b.id
                where strftime('%Y-%m-%d', b.open_time) = strftime('%Y-%m-%d', @date, 'localtime')
                    and a.running_water_finish=0 and a.code=@userCode";
            DataTable dt = DBHelperSQLite.Query(sql, new SQLiteParameter[] {
                    new SQLiteParameter("@userCode", userCode),
                    new SQLiteParameter("@date", date)
                });
            return dt;
        }

        /// <summary>
        /// 获取指定日期未回回水下注，用于计算自动回水
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DataTable GetBettingReturnByDate(string userCode, DateTime date)
        {
            string sql = @"select a.id, a.code, a.bl_water, a.blyk
                from t_bettings a left join t_result b on a.result_id=b.id
                where strftime('%Y-%m-%d', b.open_time) = strftime('%Y-%m-%d', @date, 'localtime')
                    and a.return_water_finish=0 and a.code=@userCode";
            DataTable dt = DBHelperSQLite.Query(sql, new SQLiteParameter[] {
                    new SQLiteParameter("@userCode", userCode),
                    new SQLiteParameter("@date", date)
                });
            return dt;
        }

        /// <summary>
        /// 更新流水返水状态为已返水
        /// </summary>
        /// <param name="bettingId"></param>
        /// <returns></returns>
        public static void FinishRunningWater(int bettingId)
        {
            string sql = @"update t_bettings set running_water_finish=1 where id=@bettingId";
            DBHelperSQLite.ExecuteNonQuery(sql, new SQLiteParameter[] {
                new SQLiteParameter("@bettingId", bettingId)
            });
        }

        /// <summary>
        /// 更新回水返水状态为已返水
        /// </summary>
        /// <param name="bettingId"></param>
        /// <returns></returns>
        public static void FinishReturnWater(int bettingId)
        {
            string sql = @"update t_bettings set return_water_finish=1 where id=@bettingId";
            DBHelperSQLite.ExecuteNonQuery(sql, new SQLiteParameter[] {
                new SQLiteParameter("@bettingId", bettingId)
            });
        }

        /// <summary>
        /// 今日自动回水总额
        /// </summary>
        /// <param name="bettingId"></param>
        /// <returns></returns>
        public static int TodayReturnWater(string userCode)
        {
            string sql = @"select sum(amount) from t_record where type in (7, 8) and strftime('%Y-%m-%d', calc_time) = strftime('%Y-%m-%d', 'now', 'localtime') and code=@userCode";
            object single = DBHelperSQLite.GetSingle(sql, new SQLiteParameter[] {
                new SQLiteParameter("@userCode", userCode)
            });
            return single==null ? 0 : Convert.ToInt32(single);
        }

        /// <summary>
        /// 更新指定时间段自助回水状态为已回水
        /// </summary>
        /// <param name="bettingId"></param>
        /// <returns></returns>
        public static void FinishReturnWater(DateTime begin, DateTime end)
        {
            string sql = @"update t_bettings set return_water_finish=1 where result_id in (select id from t_result where open_time >= @begin and open_time < @end) and return_water_finish=0";
            DBHelperSQLite.ExecuteNonQuery(sql, new SQLiteParameter[] {
                new SQLiteParameter("@begin", begin),
                new SQLiteParameter("@end", end),
            });
        }

        /// <summary>
        /// 更新指定时间段自助流水状态为已回流水
        /// </summary>
        /// <param name="bettingId"></param>
        /// <returns></returns>
        public static void FinishRunningWater(DateTime begin, DateTime end)
        {
            string sql = @"update t_bettings set running_water_finish=1 where result_id in (select id from t_result where open_time >= @begin and open_time < @end) and running_water_finish=0";
            DBHelperSQLite.ExecuteNonQuery(sql, new SQLiteParameter[] {
                new SQLiteParameter("@begin", begin),
                new SQLiteParameter("@end", end),
            });
        }

        /// <summary>
        /// 取消指定时间段已回自助流水状态
        /// </summary>
        /// <param name="bettingId"></param>
        /// <returns></returns>
        public static void CancelRunningWater(DateTime begin, DateTime end)
        {
            string sql = @"update t_bettings set running_water_finish=0 where result_id in (select id from t_result where open_time >= @begin and open_time < @end) and running_water_finish=1";
            DBHelperSQLite.ExecuteNonQuery(sql, new SQLiteParameter[] {
                new SQLiteParameter("@begin", begin),
                new SQLiteParameter("@end", end),
            });
        }

        /// <summary>
        /// 获取用户今日盈亏
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static double GetTodayWinOrLoseByUser(User user)
        {
            string sql = @"select sum(a.blyk)
                from t_bettings a
                left join t_result b on a.result_id=b.id
                where a.code=@userCode and strftime('%Y-%m-%d', b.open_time) = strftime('%Y-%m-%d', 'now', 'localtime')";
            SQLiteDataReader sdr = DBHelperSQLite.ExecuteReader(sql, new SQLiteParameter[] {
                new SQLiteParameter("@userCode", user.UserId),
            });
            double winOrLose = 0;
            if (sdr.Read())
            {
                if (!sdr.IsDBNull(0))
                {
                    winOrLose = sdr.GetDouble(0);
                }
            }
            sdr.Close();
            return Math.Round(winOrLose, 2);
        }

        /// <summary>
        /// 获取今日盈亏（所有真人）
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static double GetTodayWinOrLose()
        {
            string sql = @"select sum(a.blyk)
                from t_bettings a
                    left join t_result b on a.result_id=b.id
                where a.is_dummy=0 and strftime('%Y-%m-%d', b.open_time) = strftime('%Y-%m-%d', 'now', 'localtime')";
            SQLiteDataReader sdr = DBHelperSQLite.ExecuteReader(sql);
            double winOrLose = 0;
            if (sdr.Read())
            {
                if (!sdr.IsDBNull(0))
                {
                    winOrLose = sdr.GetDouble(0);
                }
            }
            sdr.Close();
            return winOrLose == 0 ? 0 : -winOrLose;
        }
    }
}
