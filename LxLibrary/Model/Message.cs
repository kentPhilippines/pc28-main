using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImLibrary.Model
{
    internal class Message
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string NickName { get; set; }
        public string Msg { get; set; }
        public DateTime MsgTime { get; set; }
        public string Fp { get; set; }
    }
}
