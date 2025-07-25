using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Codecs;
using System.Windows.Forms;

namespace RobotApp.IM
{
    /// <summary>
    /// Client
    /// </summary>
    internal class TcpSocketClient
    {
        private static TcpSocketClient instance = null;
        private static readonly object syncRoot = new object();
        private MultithreadEventLoopGroup _group;
        private Bootstrap _bootstrap;
        private IChannel _workChannel;

        private TcpSocketClient() { }
        public static TcpSocketClient Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new TcpSocketClient();
                    }
                }
                return instance;
            }
        }
        /// <summary>
        /// 接收命令事件
        /// </summary>
        public event Action<byte[]> OnReceiveRawPacketComomand;

        /// <summary>
        /// 接收命令事件
        /// </summary>
        public event Action OnDisconnect;

        /// <summary>
        /// Client
        /// </summary>
        public string Client { get; set; }

        /// <summary>
        /// 已连接
        /// </summary>
        public bool Connected { get; set; }

        /// <summary>
        /// 接收信息包
        /// </summary>
        /// <param name="bytes">数据</param>
        public void ReceivePacket(byte[] bytes)
        {
            OnReceiveRawPacketComomand?.Invoke(bytes);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void HandleDisconnect()
        {
            OnDisconnect.Invoke();
            Console.WriteLine("连接已断开");
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="ex">异常</param>
        public void HandlerException(Exception ex)
        {
            Console.WriteLine("{0:yyyy-MM-dd HH:mm:ss}发生错误：{1}", DateTime.Now, ex.Message);
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <returns>true:成功;false:失败</returns>
        public bool Connect(string hostAddress, int hostPort)
        {
            if (_group == null)
            {
                _group = new MultithreadEventLoopGroup();
            }
            if (_bootstrap == null)
            {
                _bootstrap = new Bootstrap();
                _bootstrap
                    .Group(_group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true)
                    .Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(3))
                    .Handler(new ActionChannelInitializer<ISocketChannel>(ch =>
                    {
                        IChannelPipeline pipeline = ch.Pipeline;

                        // 发送时加上2个字节的头
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(4));

                        // 解包时移除4个字节的头,要解码的最大数据包长度6*1024、长度域的偏移量0、长度域所占用的字节数4,长度字段值需要调整的值0,解码器在返回帧之前应该跳过的字节数4
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(4+6*1024, 0, 4, 0, 4));

                        pipeline.AddLast(new TcpChannelHandler(this));
                    }));
            }
            Disconnect();
            try
            {
                _workChannel = _bootstrap.ConnectAsync(IPAddress.Parse(hostAddress), hostPort).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("连接时发生错误:"+ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            if (_workChannel != null && _workChannel.Active)
            {
                _workChannel.DisconnectAsync().Wait();
                Connected = false;
                _workChannel = null;
            }
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="bufer">数据</param>
        public void Send(byte[] bufer)
        {
            if (_workChannel != null && _workChannel.Open)
            {
                IByteBuffer byteBuffer = Unpooled.Buffer(bufer.Length);
                byteBuffer.WriteBytes(bufer);                
                _workChannel.WriteAndFlushAsync(byteBuffer);
            }
            else
            {
                Console.WriteLine("掉线了");
            }
        }
    }

}
