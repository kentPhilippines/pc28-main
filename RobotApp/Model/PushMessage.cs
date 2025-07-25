using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotApp.Model
{
    internal class PushMessage
    {
        public string msg { get; set; } //撤销的消息提示
        public string uuid { get; set; } //finger_print_of_protocal
        public string senderId { get; set; }//发送者的id
        public string sendId { get; set; } //发送者的id
        public string adminId { get; set; } //当前群管理员的id
        public bool isBanned { get; set; } //0:表示被禁言,1表示被解禁了
    }
}
