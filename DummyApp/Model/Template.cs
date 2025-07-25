using CommonLibrary;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DummyApp.Model;

internal class Template
{
    public List<string> 假聊内容 = new List<string>();
    public string 模板名称 { get; set; }
    public int 下注随机时间A { get; set; }
    public int 下注随机时间B { get; set; }
    public int 连续下注期数A { get; set; }
    public int 连续下注期数B { get; set; }
    public int 停止下注期数A { get; set; }
    public int 停止下注期数B { get; set; }
    public List<string> 下注内容 { get; set; } = new List<string>();

    public string 查指令 { get; set; }
    public int 查随机时间A { get; set; }
    public int 查随机时间B { get; set; }

    public string 回指令 { get; set; }
    public int 回随机时间A { get; set; }
    public int 回随机时间B { get; set; }

    public int 积分大于 { get; set; }
    public int 回随机积分A { get; set; }
    public int 回随机积分B { get; set; }

    public int 积分小于 { get; set; }
    public int 查随机积分A { get; set; }
    public int 查随机积分B { get; set; }

    public int 假聊随机期数A { get; set; }
    public int 假聊随机期数B { get; set; }

    public int 假聊随机时间A { get; set; }
    public int 假聊随机时间B { get; set; }

    public bool 开启假聊 { get; set; }
    public bool 假聊封盘后发送 { get; set; }

    [JsonIgnore]
    public string Random下注内容
    {
        get
        {
            int randomIndex = ThreadSafeRandom.Next(0, 下注内容.Count); // 生成随机索引
            return 下注内容[randomIndex];
        }
    }

    [JsonIgnore]
    public string Random假聊内容
    {
        get
        {
            int randomIndex = ThreadSafeRandom.Next(0, 假聊内容.Count);
            return 假聊内容[randomIndex];
        }
    }

    [JsonIgnore]
    public int Random下注随机时间
    {
        get
        {
            int randomValue = ThreadSafeRandom.Next(下注随机时间A, 下注随机时间B + 1);
            return randomValue;
        }
    }

    [JsonIgnore]
    public int Random查随机时间
    {
        get
        {
            int randomValue = ThreadSafeRandom.Next(查随机时间A, 查随机时间B + 1);
            return randomValue;
        }
    }

    [JsonIgnore]
    public int Random回随机时间
    {
        get
        {
            int randomValue = ThreadSafeRandom.Next(回随机时间A, 回随机时间B + 1);
            return randomValue;
        }
    }

    [JsonIgnore]
    public int Random查随机积分
    {
        get
        {
            int randomValue = ThreadSafeRandom.Next(查随机积分A, 查随机积分B + 1);
            return randomValue;
        }
    }

    [JsonIgnore]
    public int Random回随机积分
    {
        get
        {
            int randomValue = ThreadSafeRandom.Next(回随机积分A, 回随机积分B + 1);
            return randomValue;
        }
    }

    [JsonIgnore]
    public int Random假聊随机期数
    {
        get
        {
            int randomValue = ThreadSafeRandom.Next(假聊随机期数A, 假聊随机期数B + 1);
            return randomValue;
        }
    }

    [JsonIgnore]
    public int Random假聊随机时间
    {
        get
        {
            int randomValue = ThreadSafeRandom.Next(假聊随机时间A, 假聊随机时间B + 1);
            return randomValue;
        }
    }

    [JsonIgnore]
    public int Random连续下注期数
    {
        get
        {
            int randomValue = ThreadSafeRandom.Next(连续下注期数A, 连续下注期数B + 1);
            return randomValue;
        }
    }

    [JsonIgnore]
    public int Random停止下注期数
    {
        get
        {
            int randomValue = ThreadSafeRandom.Next(停止下注期数A, 停止下注期数B + 1);
            return randomValue;
        }
    }
}