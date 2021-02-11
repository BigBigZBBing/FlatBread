using FlatBread.Buffer;
using FlatBread.Enum;
using FlatBread.Inherit;
using FlatBread.Log;
using FlatBread.Session;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace FlatBread.Session
{
    /// <summary>
    /// 服务端的用户会话级模型
    /// </summary>
    public class UserTokenSession : BasicSession
    {
        /// <summary>
        /// Session类型
        /// </summary>
        internal SocketMode Mode { get; set; }

        /// <summary>
        /// 连接运行时
        /// </summary>
        internal bool Connecting { get; set; }

        /// <summary>
        /// 连接通道
        /// </summary>
        internal Socket Channel
        {
            get => Mode switch
            {
                SocketMode.Client => ShakeHandEvent?.ConnectSocket,
                SocketMode.Server => ShakeHandEvent?.AcceptSocket,
                _ => null
            };
        }

        /// <summary>
        /// 连接套接字
        /// </summary>
        internal ShakeHandEventArgs ShakeHandEvent { get; set; }

        Packet _Cache;
        /// <summary>
        /// 暂存消息包
        /// </summary>
        internal Packet Cache
        {
            get
            {
                if (_Cache == null)
                    _Cache = new Packet();
                return _Cache;
            }
            set
            {
                _Cache = value;
            }
        }

        /// <summary>
        /// 未发送成功的消息(会话级)
        /// </summary>
        internal ConcurrentQueue<byte[]> NoSuccessMessage { get; set; } = new ConcurrentQueue<byte[]>();

        /// <summary>
        /// 清空缓存
        /// </summary>
        internal void Clear()
        {
            UserCode = null;
            UserHost = null;
            UserPort = null;
            OperationTime = null;
            ShakeHandEvent.Clear();
            NoSuccessMessage.Clear();
            Cache = null;
        }

        /// <summary>
        /// 消息包
        /// </summary>
        /// <param name="message"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendMessage(byte[] message)
        {
            ShakeHandEvent.SendEventArgs.Encode(message);

            Console.WriteLine("Connecting:" + Connecting);
            /*连接中所有的发送操作全进入队列排队*/
            if (Connecting)
            {
                NoSuccessMessage.Enqueue(message);
            }
            /*
             * 服务端关闭后会保留一次通道
             * 之后通道就会关闭为NULL
             */
            else if (Channel == null)
            {
                NoSuccessMessage.Enqueue(message);
                ShakeHandEvent.ConnectAction();
            }
            /*
             * 同步方案 但是这样做不到更高性能的发送 则先放弃使用
             * 异步方案 虽能达到发送的最高性能 但是过快会导致SAEA的被占用 后期使用SAEA轮替方案解决(暂时未解决)
             * Channel.Send(ShakeHandEvent.SendEventArgs.MemoryBuffer.Span);
            */
            else if (!Channel.SendAsync(ShakeHandEvent.SendEventArgs))
            {
                ShakeHandEvent.SendEventArgs.SendAction(ShakeHandEvent.SendEventArgs);
            }
        }

        /// <summary>
        /// 死包
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SendDisconnect()
        {
            ShakeHandEvent.SendEventArgs.Disconnect();
            Channel.SendAsync(ShakeHandEvent.SendEventArgs);
        }

        /// <summary>
        /// 重生包
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SendReconnect()
        {
            ShakeHandEvent.SendEventArgs.Reconnection();
            Channel.SendAsync(ShakeHandEvent.SendEventArgs);
        }
    }
}
