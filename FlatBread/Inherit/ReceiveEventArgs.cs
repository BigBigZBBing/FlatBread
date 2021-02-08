using FlatBread.Buffer;
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
        internal void Decode(Action<byte[]> bytes)
        {
            UserTokenSession UserToken = this.UserToken as UserTokenSession;
            int offset = 0;
            var curBytes = MemoryBuffer.Span;
            while (offset < BytesTransferred)
            {
                offset += UserToken.Cache.LoadHead(curBytes.Slice(offset));
                offset += UserToken.Cache.LoadBody(curBytes.Slice(offset));
                if (UserToken.Cache.IsCompleted())
                {
                    bytes?.Invoke(UserToken.Cache);
                    UserToken.Cache = null;
                }
            }
        }
    }
}
