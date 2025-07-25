using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;

namespace RobotApp.Util
{
    internal class CountdownTimer
    {
        private Timer _timer;
        private int _remainingTime;
        private ConcurrentDictionary<int, List<Action>> TimerActions = new ConcurrentDictionary<int, List<Action>>();
        public Action<int> Tick;
        public Action OnStart;
        public Action OnStop;
        private object _lock = new object();

        public void AddTimerAction(int remainingTime, Action action)
        {
            if (TimerActions.TryGetValue(remainingTime, out List<Action> actionList))
            {
                actionList.Add(action);
            }
            else
            {
                List<Action> al = new List<Action>();
                al.Add(action);
                TimerActions.TryAdd(remainingTime, al);
            }
        }
        public CountdownTimer(int seconds)
        {
            if (_timer != null)
            {
                _timer.Enabled = false;
                _timer.Stop();
                _timer.Dispose();
            }
            _remainingTime = seconds;
            _timer = new Timer(1000); // 设置计时器的间隔时间为1000毫秒（1秒）
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true; // 让计时器每秒都触发Elapsed事件
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _remainingTime -= 1;
            Tick?.Invoke(_remainingTime);
            //Debug.WriteLine($"剩余时间: {_remainingTime}秒");
            if (TimerActions.TryRemove(_remainingTime, out List<Action> al))
            {
                foreach (Action action in al)
                {
                    action.Invoke();
                }
            }
        }

        public void Start()
        {
            Debug.WriteLine("倒计时开始:" + _remainingTime);
            _timer.Enabled = true;
            _timer.Start();
            OnStart?.Invoke();
        }

        public void StopAndDispose()
        {
            Debug.WriteLine("倒计时结束:" + _remainingTime);
            _timer.Enabled = false;
            _timer.Stop();
            _timer.Dispose();
            OnStop?.Invoke();
        }
    }
}
