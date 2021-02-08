using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FlatBread.Buffer
{
    internal class Packet
    {
        /// <summary>
        /// 包头是否完整
        /// </summary>
        internal bool HasHeader { get { return HeadTargetLength > -1 && HeadTargetLength == HeadCurrentLength; } }

        /// <summary>
        /// 包头的目标长度
        /// </summary>
        internal int HeadTargetLength { get; set; }

        /// <summary>
        /// 包头的当前长度
        /// </summary>
        internal int HeadCurrentLength { get; set; }

        /// <summary>
        /// 包头缓存
        /// </summary>
        internal byte[] HeadCache { get; set; }

        /// <summary>
        /// 包体是否完整
        /// </summary>
        internal bool HasBody { get { return BodyTargetLength > -1 && BodyTargetLength == BodyCurrentLength; } }

        /// <summary>
        /// 包体的目标长度
        /// </summary>
        internal int BodyTargetLength { get; set; }

        /// <summary>
        /// 包体的当前长度
        /// </summary>
        internal int BodyCurrentLength { get; set; }

        /// <summary>
        /// 封包缓存
        /// </summary>
        internal List<byte[]> Cache { get; set; }

        /// <summary>
        /// 封包所有内容
        /// </summary>
        internal byte[] PacketContent { get; set; }

        internal Packet()
        {
            Cache = new List<byte[]>();
            HeadTargetLength = -1;
            HeadCurrentLength = -1;
            BodyTargetLength = -1;
            BodyCurrentLength = -1;
        }

        /// <summary>
        /// 加载包头
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int LoadHead(Span<byte> stream)
        {
            int Offset = 0;
            //如果包头不完整
            if (!HasHeader && stream.Length > 0)
            {
                //如果大于-1 说明包头未加载完成
                if (HeadTargetLength > -1)
                {
                    //可以读完包头
                    if (stream.Length >= (HeadTargetLength - HeadCurrentLength))
                    {
                        stream.Slice(0, HeadTargetLength - HeadCurrentLength).ToArray().CopyTo(HeadCache, 1);
                        HeadCurrentLength = HeadTargetLength;
                        //获取包体长度
                        var lengthBit = HeadCache.AsSpan(1);
                        Offset += lengthBit.Length;
                        switch (lengthBit.Length)
                        {
                            case 1: BodyTargetLength = lengthBit[0]; break;
                            case 2: BodyTargetLength = BitConverter.ToInt16(lengthBit); break;
                            case 3: BodyTargetLength = BitConverter.ToInt32(lengthBit); break;
                        }
                        BodyCurrentLength = 0;
                    }
                    else
                    {
                        stream.Slice(0, stream.Length).ToArray().CopyTo(HeadCache, HeadCurrentLength);
                        HeadCurrentLength += stream.Length;
                        Offset += stream.Length;
                    }
                }
                //包头未加载过首先获取包头类型
                else
                {
                    //创建包头缓存
                    var packetType = stream[0];
                    switch (packetType)
                    {
                        case 1: //byte 1字节
                            HeadTargetLength = 2;
                            break;
                        case 2: //short 2字节
                            HeadTargetLength = 3;
                            break;
                        case 3: //int 4字节
                            HeadTargetLength = 5;
                            break;
                    }
                    HeadCache = new byte[HeadTargetLength];
                    //优先加载封包类型
                    HeadCache[0] = packetType;
                    HeadCurrentLength = 1;
                    Offset += 1;
                    //递归执行未加载完成的问题
                    if (stream.Length > 1)
                        Offset += LoadHead(stream.Slice(1));
                }
            }
            return Offset;
        }

        /// <summary>
        /// 加载包体
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        internal int LoadBody(Span<byte> stream)
        {
            int Offset = 0;
            if (!HasBody && stream.Length > 0)
            {
                //初始化包体
                if (PacketContent == null)
                    PacketContent = new byte[BodyTargetLength];

                //可以一次性读完
                var residue = BodyTargetLength - BodyCurrentLength;
                if (stream.Length >= residue)
                {
                    stream.Slice(0, residue).ToArray()
                        .CopyTo(PacketContent, BodyCurrentLength);
                    BodyCurrentLength = BodyTargetLength;
                    Offset += residue;
                }
                else
                {
                    stream.Slice(0, stream.Length).ToArray()
                        .CopyTo(PacketContent, BodyCurrentLength);
                    BodyCurrentLength += stream.Length;
                    Offset += stream.Length;
                }
            }
            return Offset;
        }

        /// <summary>
        /// 封包是否接收完成
        /// </summary>
        /// <returns></returns>
        internal bool IsCompleted()
        {
            if (HasHeader && HasBody)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 重载隐式转换
        /// </summary>
        /// <param name="packet"></param>
        public static implicit operator byte[](Packet packet) => packet.PacketContent;
    }
}
