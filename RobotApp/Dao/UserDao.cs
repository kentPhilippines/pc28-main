using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using RobotApp.IM;
using RobotApp.Model;
using RobotApp.Properties;
using RobotApp.Util;

namespace RobotApp.Dao
{
    [SupportedOSPlatform("windows")]
    internal class UserDao
    {   
        private UserDao() { }

        public static void AddUser(User user)
        {            
            string addUserSql = @"insert into t_user (code, nick_name, jifen, slyk, is_dummy, status, la_shou_code) 
                values (@code, @nickName, @jifen, @slyk, @isDummy, @status, @laShouCode)";
            DBHelperSQLite.ExecuteSql(addUserSql, new SQLiteParameter[] {
                new SQLiteParameter("@code", user.UserId),
                new SQLiteParameter("@nickName", user.NickName),
                new SQLiteParameter("@jifen", user.Jifen),
                new SQLiteParameter("@slyk", user.Slyk),
                new SQLiteParameter("@isDummy", user.IsDummy),
                new SQLiteParameter("@status", (int)user.Status),
                new SQLiteParameter("@laShouCode", user.LaShouCode),
            });
        }

        public static void UpdateUser(User user)
        {
            string updateUserSql = @"update t_user set nick_name=@nickName, jifen=@jifen, frozen_jifen=@frozenJifen,
                slyk=@slyk, is_dummy=@isDummy, status=@status, la_shou_code=@laShouCode where code=@code";  
            DBHelperSQLite.ExecuteSql(updateUserSql, new SQLiteParameter[] {                
                new SQLiteParameter("@nickName", user.NickName),
                new SQLiteParameter("@jifen", user.Jifen),
                new SQLiteParameter("@frozenJifen", user.FrozenJifen),
                new SQLiteParameter("@slyk", user.Slyk),
                new SQLiteParameter("@isDummy",user.IsDummy),
                new SQLiteParameter("@status",(int)user.Status),
                new SQLiteParameter("@laShouCode",user.LaShouCode),
                new SQLiteParameter("@code", user.UserId),
            });             
        }

        /// <summary>
        /// 获取指定玩家
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static User GetUser(string code)
        {
            string getUserSql = @"select code, nick_name, jifen, frozen_jifen, slyk, 
                is_dummy, status, la_shou_code from t_user where code=@code";
            SQLiteDataReader sdr=DBHelperSQLite.ExecuteReader(getUserSql, new SQLiteParameter[] {
                new SQLiteParameter("@code", code),
            });
            if (sdr.Read())
            {
                User user = new User(sdr.GetString(0), sdr.GetString(1), sdr.GetInt32(2), sdr.GetInt32(3), sdr.GetInt32(4), sdr.GetBoolean(5), (UserStatus)sdr.GetInt32(6));
                if (!sdr.IsDBNull(7))
                {
                    user.LaShouCode = sdr.GetString(7);
                }
                sdr.Close();
                return user;
            }
            return null;
        }

        /// <summary>
        /// 获取指定昵称玩家
        /// </summary>
        /// <param name="nickName"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static User GetUserByNickName(string nickName)
        {
            string getUserSql = @"select code, nick_name, jifen, frozen_jifen, slyk, 
                is_dummy, status, la_shou_code from t_user where nick_name=@nickName";
            SQLiteDataReader sdr = DBHelperSQLite.ExecuteReader(getUserSql, new SQLiteParameter[] {
                new SQLiteParameter("@nickName", nickName),
            });
            if (sdr.Read())
            {
                User user = new User(sdr.GetString(0), sdr.GetString(1), sdr.GetInt32(2), sdr.GetInt32(3), sdr.GetInt32(4), sdr.GetBoolean(5), (UserStatus)sdr.GetInt32(6));
                if (!sdr.IsDBNull(7))
                {
                    user.LaShouCode = sdr.GetString(7);
                }
                sdr.Close();
                return user;
            }
            return null;
        }

        /// <summary>
        /// 获取所有玩家
        /// </summary>
        /// <returns></returns>
        public static List<User> GetUsers()
        {
            return GetUsers(RobotClient.CurrentResult);
        }

        /// <summary>
        /// 获取所有玩家
        /// </summary>
        /// <returns></returns>
        public static List<User> GetUsers(Result result)
        {
            string getUserSql = @"select code, nick_name, jifen, frozen_jifen,
                slyk, is_dummy, status, la_shou_code, running_water_scale,return_water_scale from t_user where status in (1,2)";
            SQLiteDataReader sdr = DBHelperSQLite.ExecuteReader(getUserSql);
            List<User> userList = new List<User>();
            while (sdr.Read())
            {
                User user = new User(sdr.GetString(0), sdr.GetString(1), sdr.GetInt32(2), sdr.GetInt32(3), sdr.GetInt32(4), sdr.GetBoolean(5), (UserStatus)sdr.GetInt32(6));
                if (!sdr.IsDBNull(7))
                {
                    user.LaShouCode = sdr.GetString(7);
                }
                if (!sdr.IsDBNull(8)) {
                    // 安全地获取Double值，如果转换失败则使用默认值
                    try
                    {
                        user.RunningWaterScale = sdr.GetDouble(8);
                    }
                    catch (InvalidCastException)
                    {
                        // 尝试转换为字符串再转换为Double
                        if (double.TryParse(sdr.GetValue(8).ToString(), out double runningWaterScale))
                        {
                            user.RunningWaterScale = runningWaterScale;
                        }
                        else
                        {
                            user.RunningWaterScale = 0.0; // 默认值
                        }
                    }
                }
                if (!sdr.IsDBNull(9))
                {
                    // 安全地获取Double值，如果转换失败则使用默认值
                    try
                    {
                        user.ReturnWaterScale = sdr.GetDouble(9);
                    }
                    catch (InvalidCastException)
                    {
                        // 尝试转换为字符串再转换为Double
                        if (double.TryParse(sdr.GetValue(9).ToString(), out double returnWaterScale))
                        {
                            user.ReturnWaterScale = returnWaterScale;
                        }
                        else
                        {
                            user.ReturnWaterScale = 0.0; // 默认值
                        }
                    }
                }
                userList.Add(user);
            }
            sdr.Close();
            return userList;
        }
        /// <summary>
        /// 设置流水比例
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="scale"></param>
        public static void SaveRunningWaterScale(string userCode, double scale)
        {
            string updateUserSql = @"update t_user set running_water_scale=@runningWaterScale where code=@code";
            DBHelperSQLite.ExecuteSql(updateUserSql, new SQLiteParameter[] {
                new SQLiteParameter("@runningWaterScale", scale),
                new SQLiteParameter("@code", userCode),
            });            
        }

        /// <summary>
        /// 设置回水比例
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="scale"></param>
        public static void SaveReturnWaterScale(string userCode, double scale)
        {
            string updateUserSql = @"update t_user set return_water_scale=@returnWaterScale where code=@code";
            DBHelperSQLite.ExecuteSql(updateUserSql, new SQLiteParameter[] {
                new SQLiteParameter("@returnWaterScale", scale),
                new SQLiteParameter("@code", userCode),
            });
        }
    }
}
