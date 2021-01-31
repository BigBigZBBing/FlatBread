using FlatBread.User;
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
    public class OperationEventArgs : SocketAsyncEventArgs
    {
        /// <summary>
        /// 解封包
        /// </summary>
        /// <param name="BufferSize"></param>
        /// <param name="OnReceive"></param>
        /// <param name="bytes"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Decode(int BufferSize, Action<byte[]> bytes)
        {
            UserTokenInfo UserToken = this.UserToken as UserTokenInfo;
            //所有的字节流
            Memory<byte> all = MemoryBuffer.Slice(0, BytesTransferred > BufferSize ? BufferSize : BytesTransferred);
            int offset = 0;
            while (offset < BytesTransferred)
            {
                //标记
                int sign = 0 + offset;
                //用户暂存不等于空
                if (!UserToken.DecodeQueue.IsEmpty)
                {
                    //剩下的字节
                    int rest = BitConverter.ToInt32(UserToken.DecodeQueue.Slice(0, 4).Span);
                    //暂存的缓存
                    var cache = UserToken.DecodeQueue.Slice(4);
                    //拼接字节流
                    Memory<byte> content = new byte[cache.Length + rest];
                    //Copy内容
                    cache.CopyTo(content.Slice(0, cache.Length));
                    all.Slice(0, rest).CopyTo(content.Slice(cache.Length));

                    bytes?.Invoke(content.ToArray());

                    sign = 0 + rest;

                    offset += sign;
                }
                else
                {
                    //获取占位类型
                    Span<byte> type = all.Slice(0 + offset, 1).Span;
                    sign += 1;

                    Span<byte> Len = null;
                    int segment = 0;
                    //判断占位类型 获取占位字符的偏移量
                    switch (type[0])
                    {
                        case 1: { Len = all.Slice(1 + offset, 1).Span; segment = Len[0]; } break;
                        case 2: { Len = all.Slice(1 + offset, 2).Span; segment = BitConverter.ToInt16(Len); } break;
                        case 3: { Len = all.Slice(1 + offset, 4).Span; segment = BitConverter.ToInt32(Len); } break;
                    }
                    sign += Len.Length;

                    if ((sign + segment) > BufferSize)
                    {
                        //存下的长度
                        int save = BufferSize - sign;
                        //遗留的长度
                        int rest = (sign + segment) - BufferSize;
                        //创建缓存字节流
                        Memory<byte> cache = new byte[4 + save];
                        //给前4个字节保存剩下的长度
                        BitConverter.GetBytes(rest).CopyTo(cache.Slice(0, 4));
                        //保存暂存的到缓存
                        all.Slice(sign, save).CopyTo(cache.Slice(4, save));
                        //暂存到当前用户的缓存
                        UserToken.DecodeQueue = cache;
                        break;
                    }

                    //获取内容的二进制流
                    byte[] content = all.Slice(Len.Length + 1, segment).ToArray();
                    sign += content.Length;

                    bytes?.Invoke(content);

                    //增加偏移量
                    offset += sign;
                }

            }
        }
    }
}
