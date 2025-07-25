using RobotApp.Model;
using RobotApp.Util;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Runtime.Versioning;

namespace RobotApp.Dao
{
    [SupportedOSPlatform("windows")]
    internal class ScoreDao
    {
        private ScoreDao() { }

        public static bool AddScore(Score score)
        {
            string addUserSql = @"insert into t_score (code, amount, status, remark, create_time) values (@code, @amount, @status, @remark, @createTime)";
            int autoId = DBHelperSQLite.ExecuteSqlReturnAutoIncrId(addUserSql, new SQLiteParameter[] {
                    new SQLiteParameter("@code", score.Code),
                    new SQLiteParameter("@amount", score.Amount),
                    new SQLiteParameter("@status", (int)score.Status),
                    new SQLiteParameter("@remark",score.Remark),
                    new SQLiteParameter("@createTime", DateTime.Now.ToString("s")),
            });
            score.Id = autoId;
            return autoId > 0;
        }

        public static bool UpdateScore(Score score) 
        {
            string updateUserSql = @"update t_score set code=@code, amount=@amount, status=@status, remark=@remark, review_time=@reviewTime where id=@id";
            int count = DBHelperSQLite.ExecuteSql(updateUserSql, new SQLiteParameter[] {
                    new SQLiteParameter("@code", score.Code),
                    new SQLiteParameter("@amount", score.Amount),
                    new SQLiteParameter("@status", (int)score.Status),
                    new SQLiteParameter("@remark",score.Remark),
                    new SQLiteParameter("@reviewTime", score.ReviewTime?.ToString("s")),
                    new SQLiteParameter("@id", score.Id),
            });
            return count > 0;
        }

        public static Score GetScore(int id)
        {
            string getUserSql = @"select id, code, amount, remark, create_time, review_time, status from t_score where id=" + id;
            SQLiteDataReader sdr = DBHelperSQLite.ExecuteReader(getUserSql);
            if (sdr.Read())
            {
                Score score = new Score();
                score.Id = sdr.GetInt32(0);
                score.Code = sdr.GetString(1);
                score.Amount = sdr.GetInt32(2);
                if (!sdr.IsDBNull(3))
                {
                    score.Remark = sdr.GetString(3);
                }
                score.CreateTime = sdr.GetDateTime(4);
                if (!sdr.IsDBNull(5))
                {
                    score.ReviewTime = sdr.GetDateTime(5);
                }
                score.Status = (ScoreStatus)sdr.GetInt32(6);
                User user = UserDao.GetUser(score.Code);
                score.NickName = user.NickName;
                score.IsDummy = user.IsDummy;
                sdr.Close();
                return score;
            }
            return null;
        }

        public static List<Score> GetScoreList(params int[] status)
        {
            string sql = @"select a.id, a.code, b.nick_name, a.amount, a.remark, a.create_time, a.review_time, a.status, b.is_dummy from t_score a left join t_user b on a.code=b.code";
            if (status.Length > 0)
            {
                sql += " where a.status in (" + string.Join(",", status) + ")";
            }
            SQLiteDataReader sdr = DBHelperSQLite.ExecuteReader(sql);
            List<Score> scoreList = new List<Score>();
            while (sdr.Read())
            {
                Score score = new Score();
                score.Id = sdr.GetInt32(0);
                score.Code = sdr.GetString(1);
                score.NickName = sdr.GetString(2);
                score.Amount = sdr.GetInt32(3);
                if (!sdr.IsDBNull(4))
                {
                    score.Remark = sdr.GetString(4);
                }                
                score.CreateTime = sdr.GetDateTime(5);
                if(!sdr.IsDBNull(6))
                {
                    score.ReviewTime = sdr.GetDateTime(6);
                }
                score.Status = (ScoreStatus)sdr.GetInt32(7);
                score.IsDummy = sdr.GetBoolean(8);
                scoreList.Add(score);
            }
            sdr.Close();
            return scoreList;
        }

        public static List<Score> GetScoreList(DateTime begin, DateTime end, string nickName, int isDummy, int chahui, int status)
        {
            string sql = @"select a.id, a.code, b.nick_name, a.amount, a.remark, a.create_time, a.review_time, a.status, b.is_dummy from t_score a
                left join t_user b on a.code = b.code
                where a.create_time between @begin and @end and b.nick_name like @nickName";
            if (isDummy > -1)
            {
                sql = sql + " and b.is_dummy=" + isDummy;
            }
            if (chahui > -1)
            {
                if(chahui == 1)
                {
                    sql = sql + " and a.amount>0";
                }
                if(chahui == 2)
                {
                    sql = sql + " and a.amount<0";
                }                
            }
            if (status > -1) 
            {
                sql = sql + " and a.status="+status;
            }
            sql += " order by a.create_time desc";
            SQLiteDataReader sdr = DBHelperSQLite.ExecuteReader(sql, new SQLiteParameter[] {
                new SQLiteParameter("@begin", begin),
                new SQLiteParameter("@end", end),
                new SQLiteParameter("@nickName", "%"+nickName+"%"),
            });
            List<Score> scoreList = new List<Score>();
            while (sdr.Read())
            {
                Score score = new Score();
                score.Id = sdr.GetInt32(0);
                score.Code = sdr.GetString(1);
                score.NickName = sdr.GetString(2);
                score.Amount = sdr.GetInt32(3);
                if (!sdr.IsDBNull(4))
                {
                    score.Remark = sdr.GetString(4);
                }
                score.CreateTime = sdr.GetDateTime(5);
                score.ReviewTime = sdr.IsDBNull(6) ? null : sdr.GetDateTime(6);
                score.Status = (ScoreStatus)sdr.GetInt32(7);
                score.IsDummy = sdr.GetBoolean(8);
                scoreList.Add(score);
            }
            sdr.Close();
            return scoreList;
        }
    }
}
