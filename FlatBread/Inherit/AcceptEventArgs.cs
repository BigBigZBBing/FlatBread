using FlatBread.Buffer;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace FlatBread.Inherit
{
    public class AcceptEventArgs : SocketAsyncEventArgs
    {
        /// <summary>
        /// 接收使用的接套字
        /// </summary>
        public OperationEventArgs ReceiveEventArgs { get; set; }

        /// <summary>
        /// 发送使用的接套字
        /// </summary>
        public OperationEventArgs SendEventArgs { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ReceiveEventArgs">接收使用的接套字</param>
        /// <param name="SendEventArgs">发送使用的接套字</param>
        public AcceptEventArgs(OperationEventArgs ReceiveEventArgs, OperationEventArgs SendEventArgs)
        {
            this.ReceiveEventArgs = ReceiveEventArgs;
            this.SendEventArgs = SendEventArgs;
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        public void Clear()
        {
            base.AcceptSocket?.Close();
            base.AcceptSocket?.Dispose();
            base.AcceptSocket = null;
        }
    }
}
