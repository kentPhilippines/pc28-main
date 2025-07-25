using ImLibrary.IM;
using ImLibrary.Model;
using System.Text.Json.Serialization;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonLibrary;

namespace DummyApp.Model;

public enum DummyStatus
{
    未登录 = 0,
    登录成功 = 1,
    登录失败 = 2,
    连接失败 = 3,
    运行中 = 4,
    已暂停 = 5,
    已退出 = 6,
    不是群成员 = 7
}

public class Dummy : IMUser
{
    private bool _checked = true;
    private int _jialiaoCount; //假聊期数
    private double _jifen;
    private int _restCount; //休息期数
    private DummyStatus _status = DummyStatus.未登录;
    private int _workCount; //下注期数

    public Dummy(string billName, string loginName, string password, string template)
    {
        BillName = billName;
        LoginName = loginName;
        Password = password;
        Template = template;
    }

    [JsonIgnore] public bool IsWorking { get; set; } = false;

    [JsonIgnore]
    public bool Checked
    {
        get => _checked;
        set
        {
            _checked = value;
            OnPropertyChanged();
        }
    }

    public string BillName { get; set; }

    [JsonIgnore]
    public double Jifen
    {
        get => _jifen;
        set
        {
            _jifen = value;
            OnPropertyChanged();
        }
    }

    public string Template { get; set; }

    [JsonIgnore]
    public DummyStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
        }
    }

    public event Action<string> logAction;

    /// <summary>
    ///     启动自动下注
    /// </summary>
    public async Task StartWorker()
    {
        var uc = CentralApi.GetUserConfig("game.user.template." + Template);
        if (uc != null)
        {
            var t = uc.configValue.ToEntity<Template>();
            if (_restCount == 0)
            {
                _workCount = t.Random连续下注期数;
                _restCount = t.Random停止下注期数;
                logAction(string.Format("【{0}】每{1}期休息{2}期", BillName, _workCount, _restCount));
            }

            if (_workCount == 0 && _restCount > 0)
            {
                _restCount--;
                return;
            }

            _workCount--;

            string betContent = t.Random下注内容;
            logAction(string.Format("【{0}】下注随机时间：{1}秒, 内容：{2}", BillName, t.Random下注随机时间, betContent));
            await Task.Delay(1000 * t.Random下注随机时间);
            if (betContent.IndexOf("%") == -1)
            {
                logAction(string.Format("【{0}】下注: {1}", BillName, betContent));
                SendMessage(DefauleGroup.GroupId, betContent);
            }
            else
            {
                string betTemp = betContent.Substring(0, betContent.IndexOf("%"));
                logAction(string.Format("【{0}】下注: {1}", BillName, betTemp));
                SendMessage(DefauleGroup.GroupId, betTemp);
                string pattern = @"%(\d+)-([^%-]+)";
                Match match = Regex.Match(betContent, pattern);
                while (match.Success)
                {
                    logAction(string.Format("【{0}】休息: {1}秒", BillName, match.Groups[1].Value));
                    await Task.Delay(Convert.ToInt32(match.Groups[1].Value) * 1000);

                    logAction(string.Format("【{0}】下注: {1}", BillName, match.Groups[2].Value));
                    SendMessage(DefauleGroup.GroupId, match.Groups[2].Value);
                    match = match.NextMatch();
                }
            }
        }
    }

    //自动查分
    public async Task AutoCha()
    {
        var uc = CentralApi.GetUserConfig("game.user.template." + Template);
        if (uc != null)
        {
            var t = uc.configValue.ToEntity<Template>();
            if (Jifen < t.积分小于)
            {
                string command = t.查指令 + t.Random查随机积分;
                int randSecond = t.Random查随机时间;
                logAction(string.Format("【{0}】延迟{1}秒，{2}", BillName, randSecond, command));
                await Task.Delay(randSecond * 1000);
                SendMessage(DefauleGroup.GroupId, command);
            }
        }
    }

    //自动回分
    public async Task AutoHui()
    {
        var uc = CentralApi.GetUserConfig("game.user.template." + Template);
        if (uc != null)
        {
            var t = uc.configValue.ToEntity<Template>();
            if (Jifen > t.积分大于)
            {
                int randSecond = t.Random回随机积分;
                string command = t.回指令 + t.Random回随机积分;
                logAction(string.Format("【{0}】延迟{1}秒，{2}", BillName, randSecond, command));
                await Task.Delay(t.Random回随机时间 * 1000);
                SendMessage(DefauleGroup.GroupId, command);
            }
        }
    }

    //随机假聊
    public async Task AutoChat(bool 封盘后 = false)
    {
        var uc = CentralApi.GetUserConfig("game.user.template." + Template);
        if (uc != null)
        {
            var t = uc.configValue.ToEntity<Template>();
            if (封盘后 != t.假聊封盘后发送) return;
            if (t.开启假聊)
            {
                if (_jialiaoCount == 0) _jialiaoCount = t.Random假聊随机期数;
                if (_jialiaoCount > 0)
                {
                    _jialiaoCount--;
                    return;
                }

                await Task.Delay(t.Random假聊随机时间 * 1000);
                SendMessage(DefauleGroup.GroupId, t.Random假聊内容);
            }
        }
    }
}