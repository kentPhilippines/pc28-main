using System.IO;
using System.Linq;
using System.Windows.Forms;
using CommonLibrary;
using DummyApp.Model;
using ImLibrary.IM;
using ImLibrary.Model;

namespace DummyApp.Util;

public class TemplateUtil
{
    public static void ImportTemplate(string iniFile, string templateName = "")
    {
        string section = "设置";

        var t = new Template();
        if (!File.Exists(iniFile))
        {
            MessageBox.Show(iniFile + "文件不存在");
            return;
        }

        if (templateName == "") templateName = Path.GetFileNameWithoutExtension(iniFile);
        t.模板名称 = templateName;
        t.下注随机时间A = IniReadWrite.GetIntVal(iniFile, section, "下注随机时间A");
        t.下注随机时间B = IniReadWrite.GetIntVal(iniFile, section, "下注随机时间B");
        t.连续下注期数A = IniReadWrite.GetIntVal(iniFile, section, "连续下注期数A");
        t.连续下注期数B = IniReadWrite.GetIntVal(iniFile, section, "连续下注期数B");
        t.停止下注期数A = IniReadWrite.GetIntVal(iniFile, section, "停止下注期数A");
        t.停止下注期数B = IniReadWrite.GetIntVal(iniFile, section, "停止下注期数B");
        t.查指令 = IniReadWrite.GetVal(iniFile, section, "查指令");
        t.回指令 = IniReadWrite.GetVal(iniFile, section, "回指令");
        t.查随机时间A = IniReadWrite.GetIntVal(iniFile, section, "查随机时间A");
        t.查随机时间B = IniReadWrite.GetIntVal(iniFile, section, "查随机时间B");
        t.回随机时间A = IniReadWrite.GetIntVal(iniFile, section, "回随机时间A");
        t.回随机时间B = IniReadWrite.GetIntVal(iniFile, section, "回随机时间B");
        t.积分大于 = IniReadWrite.GetIntVal(iniFile, section, "积分大于");
        t.积分小于 = IniReadWrite.GetIntVal(iniFile, section, "积分小于");
        t.查随机积分A = IniReadWrite.GetIntVal(iniFile, section, "查随机积分A");
        t.查随机积分B = IniReadWrite.GetIntVal(iniFile, section, "查随机积分B");
        t.回随机积分A = IniReadWrite.GetIntVal(iniFile, section, "回随机积分A");
        t.回随机积分B = IniReadWrite.GetIntVal(iniFile, section, "回随机积分B");
        t.假聊随机期数A = IniReadWrite.GetIntVal(iniFile, section, "假聊随机期数A");
        t.假聊随机期数B = IniReadWrite.GetIntVal(iniFile, section, "假聊随机期数B");
        t.假聊随机时间A = IniReadWrite.GetIntVal(iniFile, section, "假聊随机时间A");
        t.假聊随机时间B = IniReadWrite.GetIntVal(iniFile, section, "假聊随机时间B");
        t.开启假聊 = IniReadWrite.GetBoolVal(iniFile, section, "开启假聊");
        t.假聊封盘后发送 = IniReadWrite.GetBoolVal(iniFile, section, "假聊封盘后发送");
        t.假聊内容 = IniReadWrite.GetVal(iniFile, section, "假聊内容")?.Split("<br>").ToList();
        t.下注内容 = IniReadWrite.GetVal(iniFile, section, "下注内容")?.Split("<br>").ToList();

        var userConfig = new UserConfig();
        userConfig.configKey = "game.user.template." + t.模板名称;
        userConfig.configValue = t.ToJson();
        userConfig.configName = t.模板名称;
        CentralApi.SaveConfig(userConfig);
    }
}