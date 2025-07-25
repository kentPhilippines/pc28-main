using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.Versioning;
using System.Windows.Forms;
using CommonLibrary;
using RobotApp.IM;

namespace RobotApp.Util
{
    [SupportedOSPlatform("windows")]
    public class SoundUtil
    {
        public static void Play(string soundfile, bool forcePaly = false) {
            if (!forcePaly && !Config.GetInstance().选择框_提示音)
            {
                return;
            }
            foreach (string s in Config.GetInstance().列表框_事件提示音)
            {
                if (s.StartsWith(soundfile))
                {
                    soundfile = s.Substring(s.IndexOf("=")+1);
                }
            }
            string soundFilePath = IMConstant.UserDataPath + @"\Sounds\" + soundfile;
            if (!File.Exists(soundFilePath))
            {
                string currentDirectory = AppContext.BaseDirectory;
                soundFilePath = currentDirectory + @"Sounds\" + soundfile;
            }            
            if (!File.Exists(soundFilePath)) 
            {
                MessageBox.Show(soundfile+"音频文件不存在！");
            }
            else
            {
                //MusicPlay.PlayMusic(soundFilePath);
                using (SoundPlayer player = new SoundPlayer(soundFilePath))
                {
                    player.Load(); // 加载声音文件
                    player.Play(); // 播放声音
                }
            }            
        }

        public static string[] GetAllSoundFileName(bool nullable = false)
        {
            HashSet<string> filenames = new HashSet<string>();
            if (nullable)
            {
                filenames.Add("无");
            }
            string directoryPath = AppContext.BaseDirectory + "Sounds";
            string[] files = Directory.GetFiles(directoryPath);
            foreach (string file in files) {
                string fn = Path.GetFileName(file);
                filenames.Add(fn);
            }
            directoryPath = IMConstant.UserDataPath + @"\Sounds";            
            if (!Path.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            files = Directory.GetFiles(directoryPath);
            foreach (string file in files)
            {
                string fn = Path.GetFileName(file);
                filenames.Add(fn);
            }
            return filenames.ToArray();
        }
    }
}
