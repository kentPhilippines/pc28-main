using RobotApp.Model;
using RobotApp.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Runtime.Versioning;

namespace RobotApp.Dao
{
    [SupportedOSPlatform("windows")]
    internal class OplogDao
    {
        private OplogDao() { }

        public static bool Add(string opdesc)
        {
            string sql = @"insert into t_oplog (opdesc, create_time) values (@opdesc, @createTime)";
            int count = DBHelperSQLite.ExecuteSql(sql, new SQLiteParameter[] {
                    new SQLiteParameter("@opdesc", opdesc),
                    new SQLiteParameter("@createTime", DateTime.Now.ToString("s")),
            });
            return count > 0;
        }

        public static DataTable GetOplogs(DateTime begin, DateTime end)
        {
            string getUserSql = @"select id, opdesc, create_time from t_oplog where create_time between @begin and @end order by create_time desc";
            SQLiteDataReader sdr = DBHelperSQLite.ExecuteReader(getUserSql, new SQLiteParameter[]
            {
                new SQLiteParameter("@begin", begin),
                new SQLiteParameter("@end", end),
            });
            DataTable dt = new DataTable();
            dt.Columns.Add("操作时间", typeof(DateTime));
            dt.Columns.Add("操作内容", typeof(string));
            while (sdr.Read())
            {
                dt.Rows.Add(sdr.GetDateTime(2), sdr.GetString(1));    
            }
            sdr.Close();
            return dt;
        }
    }
}
