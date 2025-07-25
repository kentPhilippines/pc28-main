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
    internal class VersionDao
    {
        private VersionDao() { }

        public static string GetVersion()
        {
            string sql = @"select version from t_version limit 1";
            string version = (string)DBHelperSQLite.GetSingle(sql);
            return version;
        }
    }
}
