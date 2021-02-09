using FlatBread.Buffer;
using FlatBread.Enum;
using FlatBread.Log;
using FlatBread.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace FlatBread.Inherit
{
    /// <summary>
    /// 操作行为使用的接套字
    /// </summary>
    public class ReceiveEventArgs : SocketAsyncEventArgs
    {

        /// <summary>
        /// 解封包
        /// </summary>
        /// <param name="bytes"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Decode(Action<MessageMode, byte[]> bytes)
        {
            UserTokenSession UserToken = this.UserToken as UserTokenSession;
            int offset = 0;
            var curBytes = MemoryBuffer.Span;
            while (offset < BytesTransferred)
            {
                try
                {
                    //测试服务端解析二进制流出现问题
                    if (UserToken.Mode == SocketMode.Server)
                        throw new Exception();
                    offset += UserToken.Cache.LoadHead(curBytes.Slice(offset));
                    offset += UserToken.Cache.LoadBody(curBytes.Slice(offset));
                }
                catch
                {
                    LogHelper.LogError("解封包流偏移量出现错误!");
                    UserToken.SendReconnect();
                    return;
                }

                if (UserToken.Cache.IsCompleted())
                {
                    PacketCommand(bytes);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool PacketCommand(Action<MessageMode, byte[]> bytes)
        {
            UserTokenSession UserToken = this.UserToken as UserTokenSession;
            switch (UserToken.Cache.Mode)
            {
                //如果是消息包就回调
                case MessageMode.Message:
                    bytes?.Invoke(UserToken.Cache.Mode, UserToken.Cache);
                    break;
                //如果是请求断开
                case MessageMode.Disconect:
                    SocketDisconect();
                    break;
                //如果是请求重连
                case MessageMode.Reconnect:
                    SocketReconect();
                    break;
            }
            UserToken.Cache = null;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SocketDisconect()
        {
            UserTokenSession UserToken = this.UserToken as UserTokenSession;
            switch (UserToken.Mode)
            {
                case SocketMode.Server:
                    UserToken.SendDisconnect();
                    break;
                case SocketMode.Client:
                    LogHelper.LogError("因服务端原因 被请求断开连接");
                    UserToken.Channel.Disconnect(false);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SocketReconect()
        {
            UserTokenSession UserToken = this.UserToken as UserTokenSession;
            switch (UserToken.Mode)
            {
                case SocketMode.Server:
                    UserToken.SendReconnect();
                    break;
                case SocketMode.Client:
                    LogHelper.LogError("因服务端原因 被请求重新连接");
                    UserToken.Channel.Disconnect(true);
                    UserToken.ShakeHandEvent.ReConnect();
                    break;
            }
        }
    }
}
