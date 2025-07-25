using CommonLibrary;
using Org.BouncyCastle.Utilities;
using RobotApp.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace RobotApp.IM
{
    [SupportedOSPlatform("windows")]
    internal class WaterConfig
    {
        #region 线程安全的单例模式及加载配置
        private static WaterConfig instance = null;
        private static readonly object padlock = new object();

        private WaterConfig() { }

        public static WaterConfig GetInstance()
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new WaterConfig();
                }
                return instance;
            }
        }

        //得新加载配置
        public void Reload()
        {
            string waterConfig = ReadWriteINI.ConfigFile_GetVal("回水设置", "回水配置");
            if (waterConfig != null)
            {
                instance = waterConfig.ToEntity<WaterConfig>();
            }
        }
        #endregion


        public bool 回水条件1 { get; set; }
        public bool 回水条件2 { get; set; }
        public bool 回水条件3 { get; set; }
        public bool 回水条件4 { get; set; }
        public bool 回水条件5 { get; set; }
        public bool 回水条件6 { get; set; }
        public bool 回水条件7 { get; set; }
        public bool 回水条件8 { get; set; }

        public int 回水条件参数1 { get; set; }
        public int 回水条件参数2 { get; set; }
        public int 回水条件参数3 { get; set; }
        public int 回水条件参数4 { get; set; }
        public int 回水条件参数5 { get; set; }
        public int 回水条件参数6 { get; set; }
        public int 回水条件参数7 { get; set; }
        public int 回水条件参数8 { get; set; }

        public bool 按输分比例 { get; set; }
        public bool 按竞猜总分比例 { get; set; }
        public bool 按竞猜总分退分 { get; set; }
        public bool 按竞猜期数 { get; set; }

        public string[] A1 { get; set; } = new string[] { };
        public string[] A2 { get; set; } = new string[] { };
        public string[] B1 { get; set; } = new string[] { };
        public string[] B2 { get; set; } = new string[] { };
        public string[] C1 { get; set; } = new string[] { };
        public string[] C2 { get; set; } = new string[] { };
        public string[] D1 { get; set; } = new string[] { };
        public string[] D2 { get; set; } = new string[] { };
    }
}
