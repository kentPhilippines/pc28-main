using RobotApp.Dao;
using RobotApp.IM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobotApp.MiniDialog
{
    [SupportedOSPlatform("windows")]
    public partial class ChatRecordForm : Form
    {        
        private CancellationTokenSource cts = new CancellationTokenSource();
        private Thread thread;
        private static ChatRecordForm instance;

        public static ChatRecordForm GetInstance()
        {
            if (instance == null || instance.IsDisposed)
            {
                instance = new ChatRecordForm();
            }
            instance.Activate();
            return instance;
        }

        private ChatRecordForm()
        {
            InitializeComponent();
        }

        private void btn清空_Click(object sender, EventArgs e)
        {
            txt发送内容.Text = string.Empty;
        }

        private void btn发送_Click(object sender, EventArgs e)
        {
            RobotClient.SendMessage(txt发送内容.Text);
            txt发送内容.Text = string.Empty;
        }

        private void ChatRecordForm_Load(object sender, EventArgs e)
        {
            RobotClient.MsgQueue.Clear();
            List<Model.Message> msgList = MessageDao.GetTodayMessages();
            foreach (var item in msgList)
            {
                RobotClient.MsgQueue.Enqueue(item);
            }

            thread = new Thread(() => AppendMessage(cts.Token));
            thread.Start();
        }

        /// <summary>
        /// 添加一条消息记录
        /// </summary>
        /// <param name="msg"></param>
        private void AppendMessage(CancellationToken token)
        {
            while (!token.IsCancellationRequested) // 检查是否请求了取消
            {
                if (RobotClient.MsgQueue.Count > 0)
                {
                    Model.Message msg = RobotClient.MsgQueue.Dequeue();
                    string item = string.Format("∴{0} {1:HH:mm:ss}\r\n{2}\r\n\r\n", msg.NickName, msg.MsgTime, msg.Msg);
                    if (txt聊天记录.InvokeRequired)
                    {
                        Invoke(() =>
                        {                            
                            txt聊天记录.AppendText(item);
                        });
                    }
                    else
                    {
                        txt聊天记录.AppendText(item);
                    }
                }
                //Thread.Sleep(1000); // 模拟工作
            }            
        }

        private void ChatRecordForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            cts.Cancel(); // 请求取消
            if (thread != null)
            {
                thread.Join(); // 等待线程结束
            }            
        }
    }
}
