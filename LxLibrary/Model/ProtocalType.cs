using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImLibrary.Model
{
    public class ProtocalType { 
        public class C
        {
            /** 由客户端发出 - 协议类型：客户端登陆 */
            public static readonly int FROM_CLIENT_TYPE_OF_LOGIN = 0;
            /** 由客户端发出 - 协议类型：心跳包 */
            public static readonly int FROM_CLIENT_TYPE_OF_KEEP_ALIVE = 1;
            /** 由客户端发出 - 协议类型：发送通用数据 */
            public static readonly int FROM_CLIENT_TYPE_OF_COMMON_DATA = 2;
            /** 由客户端发出 - 协议类型：客户端退出登陆 */
            public static readonly int FROM_CLIENT_TYPE_OF_LOGOUT = 3;

            /** 由客户端发出 - 协议类型：QoS保证机制中的消息应答包（目前只支持客户端间的QoS机制哦） */
            public static readonly int FROM_CLIENT_TYPE_OF_RECIVED = 4;

            /** 由客户端发出 - 协议类型：C2S时的回显指令（此指令目前仅用于测试时） */
            public static readonly int FROM_CLIENT_TYPE_OF_ECHO = 5;
        }

        //------------------------------------------------------- from server
        public class S
        {
            /** 由服务端发出 - 协议类型：响应客户端的登陆 */
            public static readonly int FROM_SERVER_TYPE_OF_RESPONSE_LOGIN = 50;
            /** 由服务端发出 - 协议类型：响应客户端的心跳包 */
            public static readonly int FROM_SERVER_TYPE_OF_RESPONSE_KEEP_ALIVE = 51;

            /** 由服务端发出 - 协议类型：反馈给客户端的错误信息 */
            public static readonly int FROM_SERVER_TYPE_OF_RESPONSE_FOR_ERROR = 52;

            /** 由服务端发出 - 协议类型：反馈回显指令给客户端 */
            public static readonly int FROM_SERVER_TYPE_OF_RESPONSE_ECHO = 53;
	    }
    }
}
