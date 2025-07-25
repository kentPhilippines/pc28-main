using System;
using System.Collections.Concurrent;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;
using System.Drawing;

namespace DummyApp.Util;

/// <summary>
///     线程安全的日志打印控件
///     Create by 陌城
///     http://www.cnblogs.com/StrangeCity/p/4356009.html
/// </summary>
[Description("线程安全的日志打印控件")]
public class LogTextBox : RichTextBox
{
    public delegate void Action();

    private AutoResetEvent _AutoResetEvent = new AutoResetEvent(true);

    private ConcurrentQueue<Action> _queueAction;
    private Thread _threadQueueHnadler;

    public LogTextBox() : base()
    {
        this.BackColor = Color.Black;
        base.ReadOnly = true;
        this.BorderStyle = BorderStyle.None;

        InfoFontColor = Color.White;
        WarnFontColor = Color.Yellow;
        ErrorFontColor = Color.Red;
        SucceeFontColor = Color.LightGreen;

        _AutoResetEvent = new AutoResetEvent(true);
        _queueAction = new ConcurrentQueue<Action>();

        _threadQueueHnadler = new Thread(() =>
        {
            while (true)
                if (_queueAction.Count > 0)
                {
                    if (_queueAction.TryDequeue(out Action action))
                        action();
                    else
                        Thread.Yield(); // 让出 CPU 时间片
                }
                else
                {
                    _AutoResetEvent.WaitOne();
                }
        });
        _threadQueueHnadler.IsBackground = true;
        _threadQueueHnadler.Start();
    }

    [Browsable(false)] public new bool ReadOnly { get; private set; }

    [Category("外观")]
    [Description("普通信息的文本颜色")]
    public Color InfoFontColor { get; set; }

    [Category("外观")]
    [Description("警告信息的文本颜色")]
    public Color WarnFontColor { get; set; }

    [Category("外观")]
    [Description("错误信息的文本颜色")]
    public Color ErrorFontColor { get; set; }

    [Category("外观")]
    [Description("成功信息的文本颜色")]
    public Color SucceeFontColor { get; set; }

    private void PrintMsg(string msg, Color fontColor)
    {
        msg = string.Format("{0} {2}{1}", DateTime.Now.ToString("HH:mm:ss.fff"), Environment.NewLine, msg);
        int p1 = this.TextLength;
        this.AppendText(msg);
        int p2 = msg.Length;
        this.Select(p1, p2);
        this.SelectionColor = fontColor;
        this.ScrollToCaret();
    }

    private void Enqueue(string msg, Color fontColor)
    {
        _queueAction.Enqueue(() =>
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(() => { PrintMsg(msg, fontColor); }));
            else
                PrintMsg(msg, fontColor);
        });

        _AutoResetEvent.Set();
    }

    public void Info(string msg)
    {
        Enqueue(msg, InfoFontColor);
    }

    public void Warn(string msg)
    {
        Enqueue(msg, WarnFontColor);
    }

    public void Error(string msg)
    {
        Enqueue(msg, ErrorFontColor);
    }

    public void Error(Exception ex)
    {
        this.Error(ex.ToString());
    }

    public void Succee(string msg)
    {
        Enqueue(msg, SucceeFontColor);
    }
}