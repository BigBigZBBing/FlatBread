using FlatBread.Buffer;
using FlatBread.Enum;
using FlatBread.Inherit;
using FlatBread.Session;
using System;
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
        /// 连接通道
        /// </summary>
        internal Socket Channel
        {
            get
            {
                switch (Mode)
                {
                    case SocketMode.Client:
                        return ShakeHandEvent?.ConnectSocket;
                    case SocketMode.Server:
                        return ShakeHandEvent?.AcceptSocket;
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// 连接套接字
        /// </summary>
        internal ShakeHandEventArgs ShakeHandEvent { get; set; }

        private Packet _Cache;
        /// <summary>
        /// 暂存消息包
        /// </summary>
        internal Packet Cache
        {
            get
            {
                if (_Cache == null)
                {
                    _Cache = new Packet();
                }
                return _Cache;
            }
            set
            {
                _Cache = value;
            }
        }


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
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SendMessage(string message)
        {
            byte[] content = Encoding.UTF8.GetBytes(message);
            ShakeHandEvent.SendEventArgs.Encode(content);
            //Channel.Send(ShakeHandEvent.SendEventArgs.MemoryBuffer.Span);
            if (!Channel.SendAsync(ShakeHandEvent.SendEventArgs))
            {
                //Console.WriteLine("同步发送");
            }
            else
            {
                //Console.WriteLine("异步发送");
            }
        }
    }
}
