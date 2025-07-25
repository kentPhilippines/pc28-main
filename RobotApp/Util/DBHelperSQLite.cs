using CommonLibrary;
using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;

namespace RobotApp.Util
{
    [SupportedOSPlatform("windows")]
    internal class DBHelperSQLite
    {
        public static string connectionString;

        private DBHelperSQLite(){}

        public static void InitDataBase()
        {
            if (!Directory.Exists(IMConstant.UserDataPath))
            {
                Directory.CreateDirectory(IMConstant.UserDataPath);
            }
            string dataBaseFileName = IMConstant.UserDataPath + "\\database.db";
            connectionString = @"Data Source=" + dataBaseFileName + ";Pooling=true;FailIfMissing=false;JournalMode=WAL;"; //JournalMode=DELETE

            if (!File.Exists(dataBaseFileName))
            {
                SQLiteConnection.CreateFile(dataBaseFileName);
            }
            //消息表
            if (!ExistsTable("t_message"))
            {
                ExecuteSql(@"
                    CREATE TABLE IF NOT EXISTS ""t_message"" (
                      ""id"" integer PRIMARY KEY AUTOINCREMENT,
                      ""code"" text NOT NULL,
                      ""nick_name"" text NOT NULL,
                      ""msg"" text,
                      ""msg_time"" integer,
                      ""fp"" text
                    );
                ");
            }

            //开奖结果表
            if (!ExistsTable("t_result"))
            {
                ExecuteSql(@"
                    CREATE TABLE IF NOT EXISTS ""t_result"" (
                      ""id"" integer PRIMARY KEY AUTOINCREMENT,
                      ""type"" integer NOT NULL,
                      ""issue"" integer unique NOT NULL,
                      ""open_time"" text NOT NULL,
                      ""num1"" integer,
                      ""num2"" integer,
                      ""num3"" integer,
                      ""sum"" integer,
                      ""amount"" real,
                      ""award"" real,
                      ""status"" integer NOT NULL,
                      ""open_result"" text,
                      ""next_issue"" integer,
                      ""next_time"" text,
                      ""remark"" text
                    );
                ");
            }

            //用户表
            if (!ExistsTable("t_user"))
            {
                ExecuteSql(@"
                    CREATE TABLE IF NOT EXISTS ""t_user"" (
                      ""code"" text NOT NULL PRIMARY KEY,
                      ""nick_name"" text NOT NULL,
                      ""jifen"" integer NOT NULL DEFAULT 0,
                      ""frozen_jifen"" integer NOT NULL DEFAULT 0,
                      ""slyk"" integer NOT NULL DEFAULT 0,                      
                      ""is_dummy"" integer NOT NULL DEFAULT 0,
                      ""status"" integer NOT NULL DEFAULT 0,
                      ""la_shou_code"" text,
                      ""running_water_scale"" real,
                      ""return_water_scale"" real
                    );
                ");
            }

            //投注记录表
            if (!ExistsTable("t_bet_record"))
            {
                ExecuteSql(@"
                    CREATE TABLE IF NOT EXISTS ""t_bet_record"" (
                      ""id"" integer NOT NULL PRIMARY KEY AUTOINCREMENT,
                      ""result_id"" integer NOT NULL,
                      ""user_code"" text NOT NULL,                      
                      ""bet_type"" integer NOT NULL,
                      ""keyword"" text NOT NULL,
                      ""amount"" integer NOT NULL,
                      ""odds"" real,
                      ""award"" real,
                      ""fp"" text NOT NULL,
                      ""status"" integer NOT NULL,                      
                      ""is_water"" int,
                      ""is_deleted"" int,
                      ""remark"" text
                    );
                ");
            }

            //投注记录表(每期)
            if (!ExistsTable("t_bettings"))
            {
                ExecuteSql(@"
                    CREATE TABLE IF NOT EXISTS ""t_bettings"" (
                      ""id"" integer NOT NULL PRIMARY KEY AUTOINCREMENT,
                      ""code"" text NOT NULL,
                      ""result_id"" integer NOT NULL,
                      ""jifen"" integer NOT NULL DEFAULT 0,
                      ""slyk"" integer NOT NULL DEFAULT 0,
                      ""blyk"" integer NOT NULL DEFAULT 0,
                      ""blzf"" integer NOT NULL DEFAULT 0,
                      ""bl_water"" integer NOT NULL DEFAULT 0,
                      ""ccnr"" text NOT NULL DEFAULT '',
                      ""amount_zh"" integer,
                      ""amount_single"" integer,
                      ""amount_jdjx"" integer,
                      ""amount_dsxd"" integer,
                      ""is_szh"" integer,
                      ""is_txzh"" integer,
                      ""is_fxzh"" integer,
                      ""is_dummy"" integer NOT NULL DEFAULT 0,
                      ""running_water_finish"" integer NOT NULL DEFAULT 0,
                      ""return_water_finish"" integer NOT NULL DEFAULT 0
                    );
                ");
            }

            //积分记录表
            if (!ExistsTable("t_score"))
            {
                ExecuteSql(@"
                    CREATE TABLE IF NOT EXISTS ""t_score"" (
                      ""id"" integer NOT NULL PRIMARY KEY AUTOINCREMENT,
                      ""code"" text NOT NULL,
                      ""amount"" integer NOT NULL,
                      ""status"" integer NOT NULL DEFAULT 0,
                      ""remark"" text,
                      ""create_time"" text NOT NULL,
                      ""review_time"" text
                    );
                ");
            }

            //帐户变动记录表
            if (!ExistsTable("t_record"))
            {
                ExecuteSql(@"
                    CREATE TABLE IF NOT EXISTS ""t_record"" (
                      ""id"" integer NOT NULL PRIMARY KEY AUTOINCREMENT,
                      ""code"" text NOT NULL,
                      ""old_amount"" integer NOT NULL,
                      ""amount"" integer NOT NULL,
                      ""now_amount"" integer NOT NULL,
                      ""type"" integer NOT NULL DEFAULT 0,
                      ""remark"" text,
                      ""score_id"" integer NOT NULL,                        
                      ""create_time"" text NOT NULL,
                      ""create_user"" text,
                      ""calc_time"" text
                    );
                ");
            }

            //回水计算表
            if (!ExistsTable("t_water_calc"))
            {
                ExecuteSql(@"
                    CREATE TABLE IF NOT EXISTS ""t_water_calc"" (
                      ""id"" integer NOT NULL PRIMARY KEY AUTOINCREMENT,
                      ""nick_name"" text NOT NULL,
                      ""water"" real,
                      ""win_or_lose"" real,
                      ""bet_count"" integer,
                      ""back_jifen"" real,
                      ""bet_amount_sum"" real,
                      ""bet_zh_count"" integer,
                      ""bet_zh_sum"" real,
                      ""sigle_count"" integer,
                      ""jdjx_count"" integer,
                      ""dashuang_count"" integer,
                      ""xiaodan_count"" integer,
                      ""bet_szh_count"" integer,
                      ""bet_txzh_count"" integer,
                      ""bet_fxzh_count"" integer
                    );
                ");
            }

            //操作记录表
            if (!ExistsTable("t_oplog"))
            {
                ExecuteSql(@"
                    CREATE TABLE IF NOT EXISTS ""t_oplog"" (
                      ""id"" integer NOT NULL PRIMARY KEY AUTOINCREMENT,
                      ""opdesc"" text NOT NULL,
                      ""create_time"" text NOT NULL
                    );
                ");
            }

            //版本标志
            if (!ExistsTable("t_version"))
            {
                ExecuteSql(@"
                    CREATE TABLE IF NOT EXISTS ""t_version"" (
                      ""version"" text NOT NULL
                    );
                ");
                ExecuteSql(@"
                    insert into t_version values(""2504141215"");
                ");
            }
        }
        #region 公用方法

        public static bool ExistsTable(string tableName)
        {
            string sqlStr = $"SELECT count(*) from sqlite_master where type='table' and name='{tableName}'";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sqlStr, connection))
                {
                    try
                    {
                        connection.Open();
                        return !(Convert.ToInt32(cmd.ExecuteScalar()) == 0);//不存在此表，返回false
                    }
                    catch (SQLiteException E)
                    {
                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        public static int GetMaxID(string FieldName, string TableName)
        {
            string strsql = "select max(" + FieldName + ")+1 from " + TableName;
            object obj = GetSingle(strsql);
            if (obj == null)
            {
                return 1;
            }
            else
            {
                return int.Parse(obj.ToString());
            }
        }

        public static bool Exists(string strSql)
        {
            object obj = GetSingle(strSql);
            int cmdresult;
            if (Equals(obj, null) || Equals(obj, DBNull.Value))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public static bool Exists(string strSql, params SQLiteParameter[] cmdParms)
        {
            object obj = GetSingle(strSql, cmdParms);
            int cmdresult;
            if (Equals(obj, null) || Equals(obj, DBNull.Value))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region  执行简单SQL语句

        /// <summary> 
        /// 对SQLite数据库执行增删改操作，返回受影响的行数。 
        /// </summary> 
        /// <param name="sql">要执行的增删改的SQL语句</param> 
        /// <param name="parameters">执行增删改语句所需要的参数，参数必须以它们在SQL语句中的顺序为准</param> 
        /// <returns></returns> 
        public static int ExecuteNonQuery(string sql, SQLiteParameter[] parameters)
        {
            Debug.WriteLine($"当前方法名: {MethodBase.GetCurrentMethod().Name}");
            Debug.WriteLine($"SQL: {sql.Trim()}");
            Debug.WriteLine($"SQLParam: {parameters}");
            int affectedRows = 0;
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try { 
                    connection.Open();
                    using (DbTransaction transaction = connection.BeginTransaction())
                    {
                        using (SQLiteCommand command = new SQLiteCommand(connection))
                        {
                            command.CommandText = sql;
                            if (parameters != null)
                            {
                                command.Parameters.AddRange(parameters);
                            }
                            affectedRows = command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
            return affectedRows;
        }

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString)
        {
            Debug.WriteLine($"当前方法名: {MethodBase.GetCurrentMethod().Name}");
            Debug.WriteLine($"SQL: {SQLString.Trim()}");
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (SQLiteException E)
                    {
                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>    
        public static void ExecuteSqlTran(ArrayList SQLStringList)
        {
            Debug.WriteLine($"当前方法名: {MethodBase.GetCurrentMethod().Name}");
            Debug.WriteLine($"SQL: {SQLStringList}");
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = conn;
                SQLiteTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n].ToString();
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
                catch (SQLiteException E)
                {
                    tx.Rollback();
                    throw new Exception(E.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
        }
        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString, string content)
        {
            Debug.WriteLine($"当前方法名: {MethodBase.GetCurrentMethod().Name}");
            Debug.WriteLine($"SQL: {SQLString.Trim()}");
            Debug.WriteLine($"SQLParam: {content}");
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                SQLiteCommand cmd = new SQLiteCommand(SQLString, connection);
                SQLiteParameter myParameter = new SQLiteParameter("@content", DbType.String);
                myParameter.Value = content;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (SQLiteException E)
                {
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }
        /// <summary>
        /// 向数据库里插入图像格式的字段(和上面情况类似的另一种实例)
        /// </summary>
        /// <param name="strSQL">SQL语句</param>
        /// <param name="fs">图像字节,数据库的字段类型为image的情况</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSqlInsertImg(string strSQL, byte[] fs)
        {
            Debug.WriteLine($"当前方法名: {MethodBase.GetCurrentMethod().Name}");
            Debug.WriteLine($"SQL: {strSQL.Trim()}");
            Debug.WriteLine($"SQLParam: {fs}");
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                SQLiteCommand cmd = new SQLiteCommand(strSQL, connection);
                SQLiteParameter myParameter = new SQLiteParameter("@fs", DbType.Binary);
                myParameter.Value = fs;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (SQLiteException E)
                {
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString)
        {
            Debug.WriteLine($"当前方法名: {MethodBase.GetCurrentMethod().Name}");
            Debug.WriteLine($"SQL: {SQLString.Trim()}");
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if (Equals(obj, null) || Equals(obj, DBNull.Value))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (SQLiteException e)
                    {
                        throw new Exception(e.Message);
                    }
                    finally
                    {                       
                        connection.Close();
                    }
                }
            }
        }
        /// <summary>
        /// 执行查询语句，返回SQLiteDataReader
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>SQLiteDataReader</returns>
        public static SQLiteDataReader ExecuteReader(string strSQL)
        {
            Debug.WriteLine($"当前方法名: {MethodBase.GetCurrentMethod().Name}");
            Debug.WriteLine($"SQL: {strSQL.Trim()}");
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            SQLiteCommand cmd = new SQLiteCommand(strSQL, connection);
            try
            {
                connection.Open();
                SQLiteDataReader myReader = cmd.ExecuteReader();
                return myReader;
            }
            catch (SQLiteException e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 执行查询语句，返回DataTable
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataTable</returns>
        public static DataTable Query(string SQLString)
        {
            Debug.WriteLine($"当前方法名: {MethodBase.GetCurrentMethod().Name}");
            Debug.WriteLine($"SQL: {SQLString.Trim()}");
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                DataTable dt = new DataTable();
                try
                {
                    connection.Open();
                    SQLiteDataAdapter command = new SQLiteDataAdapter(SQLString, connection);
                    command.Fill(dt);
                }
                catch (SQLiteException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
                return dt;
            }
        }


        #endregion

        #region 执行带参数的SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString, params SQLiteParameter[] cmdParms)
        {
            Debug.WriteLine($"当前方法名: {MethodBase.GetCurrentMethod().Name}");
            Debug.WriteLine($"SQL: {SQLString.Trim()}");
            Debug.WriteLine($"SQLParam: {cmdParms}");
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (SQLiteException E)
                    {
                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }                    
            }
        }

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSqlReturnAutoIncrId(string SQLString, params SQLiteParameter[] cmdParms)
        {
            Debug.WriteLine($"当前方法名: {MethodBase.GetCurrentMethod().Name}");
            Debug.WriteLine($"SQL: {SQLString.Trim()}");
            Debug.WriteLine($"SQLParam: {cmdParms}");
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    if (!SQLString.EndsWith(";"))
                    {
                        SQLString += ";";
                    }
                    SQLString += "SELECT last_insert_rowid();";
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        long rowid = (long)cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        return Convert.ToInt32(rowid);
                    }
                    catch (SQLiteException E)
                    {
                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }         
            }
        }


        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的SQLiteParameter[]）</param>
        public static void ExecuteSqlTran(Hashtable SQLStringList)
        {
            Debug.WriteLine($"当前方法名: {MethodBase.GetCurrentMethod().Name}");
            Debug.WriteLine($"SQLParam: {SQLStringList}");
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    SQLiteCommand cmd = new SQLiteCommand();
                    try
                    {
                        //循环
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            SQLiteParameter[] cmdParms = (SQLiteParameter[])myDE.Value;
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();                            
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }


        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString, params SQLiteParameter[] cmdParms)
        {
            Debug.WriteLine($"当前方法名: {MethodBase.GetCurrentMethod().Name}");
            Debug.WriteLine($"SQL: {SQLString.Trim()}");
            Debug.WriteLine($"SQLParam: {cmdParms}");
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if (Equals(obj, null) || Equals(obj, DBNull.Value))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (SQLiteException e)
                    {
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回SQLiteDataReader
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>SQLiteDataReader</returns>
        public static SQLiteDataReader ExecuteReader(string SQLString, params SQLiteParameter[] cmdParms)
        {
            Debug.WriteLine($"当前方法名: {MethodBase.GetCurrentMethod().Name}");
            Debug.WriteLine($"SQL: {SQLString.Trim()}");
            Debug.WriteLine($"SQLParam: {cmdParms}");
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            SQLiteCommand cmd = new SQLiteCommand();
            try
            {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                SQLiteDataReader myReader = cmd.ExecuteReader();
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (SQLiteException e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 执行查询语句，返回DataTable
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataTable</returns>
        public static DataTable Query(string SQLString, params SQLiteParameter[] cmdParms)
        {
            Debug.WriteLine($"当前方法名: {MethodBase.GetCurrentMethod().Name}");
            Debug.WriteLine($"SQL: {SQLString.Trim()}");
            Debug.WriteLine($"SQLParam: {cmdParms}");
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                SQLiteCommand cmd = new SQLiteCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                using (SQLiteDataAdapter da = new SQLiteDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        da.Fill(dt);
                        cmd.Parameters.Clear();
                    }
                    catch (SQLiteException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }
                    return dt;
                }
            }
        }


        private static void PrepareCommand(SQLiteCommand cmd, SQLiteConnection conn, SQLiteTransaction trans, string cmdText, SQLiteParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {
                foreach (SQLiteParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }

        #endregion

        #region 数据安全相关方法
        /// <summary>
        /// 强制将WAL日志同步到主数据库文件
        /// 防止数据丢失
        /// </summary>
        public static void FlushWalToMainDB()
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    
                    // 执行WAL checkpoint，强制将WAL内容写入主数据库
                    using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA wal_checkpoint(TRUNCATE);", connection))
                    {
                        cmd.ExecuteNonQuery();
                        Debug.WriteLine("WAL checkpoint executed successfully");
                    }
                    
                    // 确保所有数据都写入磁盘
                    using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA synchronous = FULL;", connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    
                    connection.Close();
                }
                LogUtil.Log("数据库WAL同步完成");
            }
            catch (Exception ex)
            {
                LogUtil.LogEx(ex);
                throw;
            }
        }
        
        /// <summary>
        /// 定期数据安全检查和同步
        /// </summary>
        public static void PeriodicDataSync()
        {
            try
            {
                FlushWalToMainDB();
                
                // 检查数据库完整性
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA integrity_check;", connection))
                    {
                        var result = cmd.ExecuteScalar()?.ToString();
                        if (result != "ok")
                        {
                            LogUtil.Log($"数据库完整性检查警告: {result}");
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogEx(ex);
            }
        }
        #endregion

        #region 备份数据库
        public static void BackUpDateBase()
        {
            string destDBFileName = "database.db";
            string backUpPath = IMConstant.UserDataPath + @"\数据库备份";
            if (!Directory.Exists(backUpPath))
            {
                Directory.CreateDirectory(backUpPath);
            }
            int index = destDBFileName.IndexOf('.');
            string destDBFileNameProcess = destDBFileName.Insert(index, "_process");
            SQLiteConnection cnDest = new SQLiteConnection(@"Data Source=" + backUpPath + @"\" + destDBFileNameProcess);
            SQLiteConnection cnSource = new SQLiteConnection(@"Data Source=" + IMConstant.UserDataPath + @"\" +destDBFileName);
            try
            {
                cnDest.Open();
                cnSource.Open();

                cnSource.BackupDatabase(cnDest, "main", "main", -1, null, 0);
                cnDest.Close();
                cnSource.Close();

                if (File.Exists(backUpPath + @"\" + destDBFileNameProcess))
                {
                    destDBFileName = destDBFileName.Insert(index, DateTime.Now.ToString("_yyyyMMddHHmmss"));
                    File.Move(backUpPath + @"\" + destDBFileNameProcess, backUpPath + @"\" + destDBFileName);
                }
            }
            catch (Exception e)
            {
                LogUtil.LogEx(e);
                if (cnDest.State == ConnectionState.Open)
                {
                    cnDest.Close();
                }
                if (cnSource.State == ConnectionState.Open)
                {
                    cnSource.Close();
                }

            }

        }
        #endregion
    
        /// <summary>
        /// 关闭数据库
        /// </summary>
        public static void CloseDB()
        {
            
            connectionString = connectionString.Replace("WAL", "DELETE");
            try {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    conn.Close();
                }
            }catch(Exception)
            {
                //CloseDB();
            }
            
        }
    }
}

