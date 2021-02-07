using FlatBread.Enum;
using FlatBread.Inherit;
using FlatBread.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

        /// <summary>
        /// 暂存消息(会话级)
        /// </summary>
        internal Memory<byte> DecodeQueue { get; set; }

        /// <summary>
        /// 清空缓存
        /// </summary>
        internal void Clear()
        {
            UserName = null;
            UserCode = null;
            UserHost = null;
            UserPort = null;
            OperationTime = null;
            DecodeQueue = Memory<byte>.Empty;
            ShakeHandEvent.Clear();
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message)
        {
            byte[] content = Encoding.UTF8.GetBytes(message);
            ShakeHandEvent.SendEventArgs.Encode(content);
            Channel.SendAsync(ShakeHandEvent.SendEventArgs);
        }
    }
}
