using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotApp.IM
{
    /// <summary>
    /// Tcp通道处理
    /// </summary>
    internal class TcpChannelHandler : SimpleChannelInboundHandler<object>
    {
        private TcpSocketClient tcpSocketClient;

        /// <summary>
        /// Tcp
        /// </summary>
        /// <param name="client">client</param>
        public TcpChannelHandler(TcpSocketClient client)
        {
            tcpSocketClient = client;
        }

        /// <summary>
        /// 通道连接后调用
        /// </summary>
        /// <param name="context">上下文</param>
        public override void ChannelActive(IChannelHandlerContext context)
        {
            tcpSocketClient.Client = context.Channel.Id.AsShortText();
            tcpSocketClient.Connected = true;
        }

        /// <summary>
        /// 通道读取完成
        /// </summary>
        /// <param name="context">上下文</param>
        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }

        /// <summary>
        /// 异常
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="exception">错误</param>
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            tcpSocketClient.HandlerException(exception);
        }

        /// <summary>
        /// 连接断开
        /// </summary>
        /// <param name="context">上下文</param>
        public override void ChannelInactive(IChannelHandlerContext context)
        {
            tcpSocketClient.Disconnect();
            tcpSocketClient.Connected = false;
            tcpSocketClient.HandleDisconnect();
        }

        /// <summary>
        /// 读取通道数据
        /// </summary>
        /// <param name="ctx">通道上下文</param>
        /// <param name="msg">数据缓冲区</param>
        protected override void ChannelRead0(IChannelHandlerContext ctx, object msg)
        {
            if (!(msg is IByteBuffer buffer))
            {
                return;
            }

            int readableBytes = buffer.ReadableBytes;
            if (readableBytes == 0)
            {
                return;
            }

            var bytes = new byte[readableBytes];
            buffer.GetBytes(buffer.ReaderIndex, bytes);
            tcpSocketClient.ReceivePacket(bytes);
        }
    }

}
