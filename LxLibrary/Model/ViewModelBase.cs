using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;

namespace ImLibrary.Model
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //ConcurrentDictionary本身是线程安全的
        private ConcurrentDictionary<string, string> _data = new ConcurrentDictionary<string, string>();
        private string MyProperty
        {
            get => _data["MyProperty"]; // 从字典中获取值。如果键不存在，将返回默认值。
            set => _data["MyProperty"] = value; // 在字典中设置值。如果键不存在，将自动添加。然后触发通知。
        }

        protected void SetValue<T>(ref T old, T value, [CallerMemberName] string propertyName = null)
        {
            if ((old == null && value != null) || (old != null && !old.Equals(value)))
            {
                old = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
