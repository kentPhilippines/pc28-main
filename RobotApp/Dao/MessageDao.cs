using RobotApp.Model;
using RobotApp.Util;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Runtime.Versioning;
using Message = RobotApp.Model.Message;

namespace RobotApp.Dao
{
    [SupportedOSPlatform("windows")]
    internal class MessageDao
    {   
        private MessageDao() { }

        /// <summary>
        /// 添加一条消息
        /// </summary>
        /// <param name="msg"></param>
        public static void AddMessage(Message msg)
        {
            string addUserSql = @"insert into t_message (code, nick_name, msg, msg_time, fp) 
                values (@code, @nickName, @msg, @msgTime, @Fp)";
            DBHelperSQLite.ExecuteSql(addUserSql, new SQLiteParameter[] {
                new SQLiteParameter("@code", msg.Code),
                new SQLiteParameter("@nickName", msg.NickName),
                new SQLiteParameter("@msg", msg.Msg),
                new SQLiteParameter("@msgTime", msg.MsgTime),
                new SQLiteParameter("@Fp", msg.Fp),
            });
        }

        /// <summary>
        /// 获取今日所有消息
        /// </summary>
        /// <returns></returns>
        public static List<Message> GetTodayMessages()
        {
            string getUserSql = @"select id, code, nick_name, msg, msg_time, fp from t_message where strftime('%Y-%m-%d', msg_time) = strftime('%Y-%m-%d', 'now', 'localtime')";
            SQLiteDataReader sdr = DBHelperSQLite.ExecuteReader(getUserSql);
            List<Message> msgList = new List<Message>();
            while (sdr.Read())
            {
                Message msg = new Message();
                msg.Id = sdr.GetInt32(0);
                msg.Code = sdr.GetString(1);
                msg.NickName = sdr.GetString(2);
                msg.Msg = sdr.GetString(3);
                msg.MsgTime = sdr.GetDateTime(4);
                msg.Fp = sdr.GetString(5);
                msgList.Add(msg);
            }
            sdr.Close();
            return msgList;
        }
    }
}
