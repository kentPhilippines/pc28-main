using ImLibrary.IM;
using ImLibrary.Model;
using CommonLibrary;
using RobotApp.MiniDialog;
using RobotApp.Util;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RobotApp.IM
{
    [SupportedOSPlatform("windows")]
    public class Config
    {
        /// <summary>
        /// 关键字对应关系
        /// </summary>
        public Dictionary<string, string> KeyPairs { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// 单点赔率
        /// </summary>
        public Dictionary<string, double> SingleOdds { get; private set; } = new Dictionary<string, double>();
        /// <summary>
        /// 单点最高限额
        /// </summary>
        public Dictionary<string, double> SingleMaxBet { get; private set; } = new Dictionary<string, double>();

        #region 线程安全的单例模式
        private static Config instance = null;
        private static readonly object padlock = new object();

        private Config() { }

        public static Config GetInstance()
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new Config();
                }
                return instance;
            }
        }
        #endregion

        #region 信息设置
        public int 编辑框_倒计时_封盘前提醒 { get; set; }
        public int 编辑框_倒计时_封盘提醒 { get; set; }
        public int 编辑框_倒计时_核对提醒 { get; set; }
        public bool 选择框_核对提醒 { get; set; }
        public string 编辑框_消息_封盘前 { get; set; }
        public string 编辑框_消息_封盘 { get; set; }
        public string 编辑框_消息_封盘后 { get; set; }

        public string 编辑框_消息_开奖前 { get; set; }
        public string 编辑框_消息_开奖 { get; set; }
        public string 编辑框_消息_历史 { get; set; }
        public int 编辑框_历史号码个数 { get; set; }

        public string 编辑框_消息_账单 { get; set; }
        public bool 选择框_假人0分不显示 { get; set; }
        public bool 选择框_真人0分不显示 { get; set; }
        public double 编辑框_假人n分不显示 { get; set; }
        public double 编辑框_真人n分不显示 { get; set; }
        public int 编辑框_每行人数 { get; set; }
        public int 编辑框_积分位数 { get; set; }
        public int 编辑框_延迟发送账单秒 { get; set; }

        public int 编辑框_周期_广告1 { get; set; }
        public int 编辑框_时间_广告1 { get; set; }
        public string 编辑框_消息_广告1 { get; set; }
        public int 编辑框_周期_广告2 { get; set; }
        public int 编辑框_时间_广告2 { get; set; }
        public string 编辑框_消息_广告2 { get; set; }
        public int 编辑框_周期_广告3 { get; set; }
        public int 编辑框_时间_广告3 { get; set; }
        public string 编辑框_消息_广告3 { get; set; }
        public int 编辑框_周期_广告4 { get; set; }
        public int 编辑框_时间_广告4 { get; set; }
        public string 编辑框_消息_广告4 { get; set; }

        public string 编辑框_消息_测试 { get; set; }

        public string 编辑框_消息_启动 { get; set; }
        public string 编辑框_消息_停止 { get; set; }
        #endregion

        #region 赔率设置
        public bool 单选框_赔率_单点统一 { get; set; }
        public bool 单选框_赔率_单点单独 { get; set; }
        public bool 单选框_赔率_组合一 { get; set; }
        public bool 单选框_赔率_组合二 { get; set; }

        public bool 选择框_数字赔付_最大限额 { get; set; }
        public bool 选择框_玩法_豹子 { get; set; }
        public bool 选择框_玩法_对子 { get; set; }
        public bool 选择框_玩法_顺子 { get; set; }
        public bool 选择框_特殊赔率_大小单双_1 { get; set; }
        public bool 选择框_特殊赔率_大小单双_2 { get; set; }
        public bool 选择框_特殊赔率_大小单双_3 { get; set; }
        public bool 选择框_特殊赔率_大小单双_4 { get; set; }
        public bool 选择框_大小单双_出豹子 { get; set; }
        public bool 选择框_大小单双_出对子 { get; set; }
        public bool 选择框_大小单双_出顺子 { get; set; }        
        public bool 选择框_特殊赔率_组合一 { get; set; }
        public bool 选择框_特殊赔率_组合_1 { get; set; }
        public bool 选择框_特殊赔率_组合_2 { get; set; }
        public bool 选择框_特殊赔率_组合_3 { get; set; }
        public bool 选择框_组合_出豹子 { get; set; }
        public bool 选择框_组合_出对子 { get; set; }
        public bool 选择框_组合_出顺子 { get; set; }

        public double 编辑框_赔率_定位数字 { get; set; }
        public double 编辑框_数字赔付_最大限额 { get; set; }
        public string 编辑框_赔率_单点单独 { get; set; }
        public double 编辑框_赔率_豹子 { get; set; }
        public double 编辑框_赔率_对子 { get; set; }
        public double 编辑框_赔率_顺子 { get; set; }
        public double 编辑框_赔率_大小单双 { get; set; }        
        public int 编辑框_大于总注_大小单双_1 { get; set; }
        public int 编辑框_大于总注_大小单双_2 { get; set; }
        public int 编辑框_大于总注_大小单双_3 { get; set; }
        public int 编辑框_大于总注_大小单双_4 { get; set; }
        public double 编辑框_特殊赔率_大小单双_1 { get; set; }
        public double 编辑框_特殊赔率_大小单双_2 { get; set; }
        public double 编辑框_特殊赔率_大小单双_3 { get; set; }
        public double 编辑框_特殊赔率_大小单双_4 { get; set; }
        public double 编辑框_赔率_大小单双_出豹子 { get; set; }
        public double 编辑框_赔率_大小单双_出对子 { get; set; }
        public double 编辑框_赔率_大小单双_出顺子 { get; set; }
        public double 编辑框_赔率_组合 { get; set; }
        public double 编辑框_赔率_组合1314 { get; set; }
        public double 编辑框_赔率_大双小单 { get; set; }
        public double 编辑框_赔率_小双大单 { get; set; }
        public int 编辑框_大于总注_组合_1 { get; set; }
        public int 编辑框_大于总注_组合_2 { get; set; }
        public int 编辑框_大于总注_组合_3 { get; set; }
        public double 编辑框_特殊赔率_组合_1 { get; set; }
        public double 编辑框_特殊赔率_组合_2 { get; set; }
        public double 编辑框_特殊赔率_组合_3 { get; set; }
        public double 编辑框_赔率_组合_出豹子 { get; set; }
        public double 编辑框_赔率_组合_出对子 { get; set; }
        public double 编辑框_赔率_组合_出顺子 { get; set; }
        public double 编辑框_赔率_极小极大 { get; set; }
        #endregion

        #region 游戏规则
        public bool 选择框_限额_最低 { get; set; }
        public bool 选择框_限额_最高_单点 { get; set; }
        public bool 选择框_限额_单点总注计算 { get; set; }
        public bool 选择框_限额_单点单独计算 { get; set; }
        public bool 选择框_限额_最高_大小单双 { get; set; }
        public bool 选择框_限额_最高_组合 { get; set; }
        public bool 选择框_限额_最高_极值 { get; set; }
        public bool 选择框_限额_最高_豹子 { get; set; }
        public bool 选择框_限额_最高_对子 { get; set; }
        public bool 选择框_限额_最高_顺子 { get; set; }
        public bool 选择框_限额_最高_龙虎和 { get; set; }
        public bool 选择框_限额_最高_定位数字 { get; set; }
        public bool 选择框_限额_最高_定位大小单双 { get; set; }
        public bool 选择框_限额_最高_总注 { get; set; }
        public bool 选择框_禁止_杀组合 { get; set; }
        public bool 选择框_例外_杀组合 { get; set; }
        public bool 选择框_禁止_反向组合 { get; set; }
        public bool 选择框_例外_反向组合 { get; set; }
        public bool 选择框_禁止_同向组合 { get; set; }
        public bool 选择框_例外_同向组合 { get; set; }
        public bool 选择框_禁止_四门组合 { get; set; }
        public bool 选择框_禁止_大小单双反向 { get; set; }
        public bool 选择框_例外_大小单双反向 { get; set; }
        public bool 选择框_顺子890 { get; set; }
        public bool 选择框_顺子901 { get; set; }
        public bool 选择框_数字_限制个数 { get; set; }
        public bool 选择框_提示_停猜后 { get; set; }
        public bool 选择框_超额无效 { get; set; }
        public bool 选择框_发送历史图片 { get; set; }
        public bool 选择框_下注核对排序 { get; set; }
        public bool 选择框_不计入流水_1314_1 { get; set; }
        public bool 选择框_不计入流水_1314_2 { get; set; }
        public bool 选择框_不计入流水_大小单双组合 { get; set; }
        public bool 选择框_超额_大小单双流水 { get; set; }
        public bool 选择框_不计入流水_定位 { get; set; }
        public bool 选择框_下注提示 { get; set; }
        public bool 选择框_提示_艾特 { get; set; }
        public bool 选择框_提示_格式错误 { get; set; }
        public bool 选择框_关键字_走势 { get; set; }
        public bool 选择框_关键字_追跟 { get; set; }

        public double 编辑框_限额_最低 { get; set; }
        public double 编辑框_限额_最高_单点 { get; set; }
        public double 编辑框_限额_最高_大小单双 { get; set; }
        public double 编辑框_限额_最高_组合 { get; set; }
        public double 编辑框_限额_最高_极值 { get; set; }
        public double 编辑框_限额_最高_豹子 { get; set; }
        public double 编辑框_限额_最高_对子 { get; set; }
        public double 编辑框_限额_最高_顺子 { get; set; }
        public double 编辑框_限额_最高_龙虎和 { get; set; }
        public double 编辑框_限额_最高_定位数字 { get; set; }
        public double 编辑框_限额_最高_定位大小单双 { get; set; }
        public double 编辑框_限额_最高_总注 { get; set; }
        public double 编辑框_例外_杀组合 { get; set; }
        public double 编辑框_例外_反向组合 { get; set; }
        public double 编辑框_例外_同向组合 { get; set; }
        public double 编辑框_例外_大小单双反向 { get; set; }
        public string 编辑框_限额_单点单独 { get; set; }
        public int 编辑框_数字_限制个数 { get; set; }
        public double 编辑框_超额_大小单双流水 { get; set; }
        #endregion

        #region 命令设置
        public bool 选择框_关键词_区分大小写 { get; set; }
        public string 编辑框_关键词_大 { get; set; }
        public string 编辑框_关键词_小 { get; set; }
        public string 编辑框_关键词_单 { get; set; }
        public string 编辑框_关键词_双 { get; set; }
        public string 编辑框_关键词_大单 { get; set; }
        public string 编辑框_关键词_小单 { get; set; }
        public string 编辑框_关键词_大双 { get; set; }
        public string 编辑框_关键词_小双 { get; set; }
        public string 编辑框_关键词_极大 { get; set; }
        public string 编辑框_关键词_极小 { get; set; }
        public string 编辑框_关键词_豹子 { get; set; }
        public string 编辑框_关键词_上分 { get; set; }
        public string 编辑框_关键词_顺子 { get; set; }
        public string 编辑框_关键词_回分 { get; set; }
        public string 编辑框_关键词_对子 { get; set; }
        public string 编辑框_关键词_单点 { get; set; }
        public string 编辑框_关键词_梭哈 { get; set; }        
        public string 编辑框_关键词_取消 { get; set; }
        #endregion

        #region 语音提示
        public bool 选择框_提示音 { get; set; }
        public string[] 列表框_事件提示音 { get; set; }
        public bool 选择框_卡奖警报 { get; set; }
        public int 编辑框_卡奖警报秒数 { get; set; }
        public int 编辑框_卡奖警报秒数_宾果 { get; set; }
        #endregion

        #region 扩展功能
        public bool 选择框_查回回复 { get; set; }
        public bool 选择框_查回回复_显示积分 { get; set; }
        public bool 选择框_假人自动审核 { get; set; }
        public bool 选择框_查流水 { get; set; }
        public bool 选择框_查历史 { get; set; }
        public bool 选择框_查数据 { get; set; }
        public bool 选择框_查期数 { get; set; }
        public bool 选择框_查数据新格式 { get; set; }
        public bool 选择框_显示颜色 { get; set; }
        public bool 单选框_字体常规 { get; set; }
        public bool 单选框_字体小 { get; set; }

        public string 编辑框_管理名单 { get; set; }
        public int 编辑框_假人回复_查延迟_1 { get; set; }
        public int 编辑框_假人回复_查延迟_2 { get; set; }
        public int 编辑框_假人回复_回延迟_1 { get; set; }
        public int 编辑框_假人回复_回延迟_2 { get; set; }
        public string 编辑框_附加关键字_流水 { get; set; }
        public string 编辑框_附加关键字_历史 { get; set; }
        public string 编辑框_附加关键字_数据 { get; set; }
        public string 编辑框_附加关键字_期数 { get; set; }
        public int 颜色_真人 { get; set; }
        public int 颜色_下注 { get; set; }
        public int 颜色_积分不足 { get; set; }
        public int 颜色_假人 { get; set; }
        public int 颜色_上分 { get; set; }
        public int 颜色_下分 { get; set; }
        public int 编辑框_备份间隔 { get; set; }
        #endregion

        #region 自助退分
        public string 编辑框_退分_今天流水关键字 { get; set; }
        public string 编辑框_退分_昨天流水关键字 { get; set; }
        public string 编辑框_退分_今天回水关键字 { get; set; }
        public string 编辑框_退分_昨天回水关键字 { get; set; }
        public bool 单选框_退分_流水关闭 { get; set; }
        public bool 单选框_退分_流水单独设置 { get; set; }
        public bool 单选框_退分_流水统一设置 { get; set; }
        public bool 选择框_退分_盈利流水次数 { get; set; }
        public bool 选择框_退分_每日流水次数 { get; set; }
        public bool 选择框_退分_流水不显示累计返水 { get; set; }
        public bool 选择框_退分_流水最低返水金额 { get; set; }
        public bool 选择框_退分_流水最低流水占比 { get; set; }
        public bool 选择框_退分_流水最低投注期数 { get; set; }
        public bool 选择框_退分_开启每笔自动流水 { get; set; }
        public double 编辑框_退分_流水统一比例 { get; set; }
        public int 编辑框_退分_盈利流水次数 { get; set; }
        public int 编辑框_退分_每日流水次数 { get; set; }
        public double 编辑框_退分_流水最低返水金额 { get; set; }
        public double 编辑框_退分_流水最低流水占比 { get; set; }
        public int 编辑框_退分_流水最低投注期数 { get; set; }
        public bool 单选框_退分_回水关闭 { get; set; }
        public bool 单选框_退分_回水单独设置 { get; set; }
        public bool 单选框_退分_回水统一设置 { get; set; }
        public bool 选择框_退分_每日回水次数 { get; set; }
        public bool 选择框_退分_回水不显示累计返水 { get; set; }
        public bool 选择框_退分_回水最低返水金额 { get; set; }
        public bool 选择框_退分_回水最低流水占比 { get; set; }
        public bool 选择框_退分_回水最低投注期数 { get; set; }
        public bool 选择框_退分_回水用户输分 { get; set; }
        public bool 选择框_退分_开启每笔自动回水 { get; set; }
        public double 编辑框_退分_回水统一比例 { get; set; }
        public int 编辑框_退分_每日回水次数 { get; set; }
        public double 编辑框_退分_回水最低返水金额 { get; set; }
        public double 编辑框_退分_回水最低流水占比 { get; set; }
        public int 编辑框_退分_回水最低投注期数 { get; set; }
        public double 编辑框_退分_回水用户输分 { get; set; }
        #endregion

        /// <summary>
        /// 重新加载配置
        /// </summary>
        public void ReloadConfig()
        {            
            ReadWriteINI.ReloadConfig();
            Type myType = typeof(Config);
            PropertyInfo[] pis = myType.GetProperties();
            string errorConfig = "";
            foreach (PropertyInfo pi in pis)
            {
                string value = ReadWriteINI.ConfigFile_GetVal(pi.Name);
                if (pi.PropertyType == typeof(bool))
                {
                    pi.SetValue(Config.GetInstance(), value == "真");
                }
                else if (pi.PropertyType == typeof(int))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (int.TryParse(value, out int val))
                        {
                            pi.SetValue(Config.GetInstance(), val);
                        }
                        else
                        {
                            errorConfig += pi.Name + "\r\n";
                        }
                    }
                }
                else if(pi.PropertyType == typeof(double))
                {
                    if (!string.IsNullOrEmpty(value)) {
                        if(double.TryParse(value, out double val))
                        {
                            pi.SetValue(Config.GetInstance(), val);
                        }
                        else
                        {
                            errorConfig += pi.Name + "\r\n";
                        }
                    }
                }
                else if (pi.PropertyType == typeof(string))
                {
                    pi.SetValue(Config.GetInstance(), value);
                }
                else if ((pi.PropertyType == typeof(string[])))
                {
                    if (value != null)
                    {
                        string[] items = value.ToEntity<string[]>();
                        pi.SetValue(Config.GetInstance(), items);
                    }
                }
            }
            if (errorConfig != "")
            {
                MessageBox.Show(errorConfig + "配置错误！");
            }

            KeyPairs.Clear();
            LoadKey(编辑框_关键词_大, "大");
            LoadKey(编辑框_关键词_小, "小");
            LoadKey(编辑框_关键词_单, "单");
            LoadKey(编辑框_关键词_双, "双");
            LoadKey(编辑框_关键词_大单, "大单");
            LoadKey(编辑框_关键词_小单, "小单");
            LoadKey(编辑框_关键词_大双, "大双");
            LoadKey(编辑框_关键词_小双, "小双");
            LoadKey(编辑框_关键词_极大, "极大");
            LoadKey(编辑框_关键词_极小, "极小");
            LoadKey(编辑框_关键词_豹子, "豹子");
            LoadKey(编辑框_关键词_顺子, "顺子");
            LoadKey(编辑框_关键词_对子, "对子");

            SingleOdds.Clear();
            if (单选框_赔率_单点单独)
            {
                string[] odds = Regex.Split(编辑框_赔率_单点单独, "\r\n");
                foreach (string odd in odds)
                {
                    string[] s = odd.Split('=');
                    if (double.TryParse(s[1], out double d))
                    {
                        SingleOdds.Add(s[0], d);
                    }
                    else
                    {
                        MessageBox.Show("单点单独赔率 " + s[0] + " 错误");
                    }
                }
            }

            SingleMaxBet.Clear();
            if (选择框_限额_单点单独计算)
            {
                string[] maxs = Regex.Split(编辑框_限额_单点单独, "\r\n");
                foreach (string max in maxs)
                {
                    string[] s = max.Split('=');
                    if (double.TryParse(s[1], out double d))
                    {
                        SingleMaxBet.Add(s[0], d);
                    }
                    else
                    {
                        MessageBox.Show("最高限额单点单独计算 " + s[0] + " 错误");
                    }
                }
            }

            WaterConfig.GetInstance().Reload();
        }

        private void LoadKey(string keyWord, string realKey)
        {
            if (keyWord == null || keyWord=="")
            {
                keyWord = realKey;
            }
            if (!选择框_关键词_区分大小写)
            {
                keyWord = keyWord.ToUpper();
            }
            foreach (string key in keyWord.Split('|'))
            {
                KeyPairs.Add(key, realKey);
            }
        }

        /// <summary>
        /// 权限检查
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public bool CheckPermission(bool check)
        {
            if (check)
            {
                InputBoxValidation validation = delegate (string val)
                {
                    if (val == "")
                        return "管理密码不能为空";
                    return "";
                };
                string value = "";
                if (InputBox.Show("管理员授权", "*", "请输入管理员密码(默认：123456),请及时修改！", ref value, validation) == DialogResult.OK)
                {
                    if (PermissionForm.PASSWORD != value)
                    {
                        MessageBox.Show("密码错误");
                        return false;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    }
}
