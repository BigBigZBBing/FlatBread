using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace FlatBread.Inherit
{
    /// <summary>
    /// 操作行为使用的接套字
    /// </summary>
    internal class SendEventArgs : SocketAsyncEventArgs
    {
        //压封包
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Encode(byte[] message)
        {
            var len = message.Length;
            Span<byte> packet;
            if (len <= byte.MaxValue)
            {
                packet = new byte[1 + 1 + len];
                packet[0] = 1;
                packet[1] = (byte)len;
                message.CopyTo(packet.Slice(2));
            }
            else if (len <= short.MaxValue)
            {
                packet = new byte[1 + 2 + len];
                packet[0] = 2;
                packet[2] = (byte)(len >> 8);
                packet[1] = (byte)len;
                message.CopyTo(packet.Slice(3));
            }
            else
            {
                packet = new byte[1 + 4 + len];
                packet[0] = 3;
                packet[4] = (byte)(len >> 24);
                packet[3] = (byte)(len >> 16);
                packet[2] = (byte)(len >> 8);
                packet[1] = (byte)len;
                message.CopyTo(packet.Slice(5));
            }

            //设置缓冲区
            SetBuffer(packet.ToArray());
            //给缓冲区赋值
            packet.CopyTo(MemoryBuffer.Span);
        }
    }
}
