using FlatBread.Inherit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FlatBread.User
{
    /// <summary>
    /// 用户端的信息模型
    /// </summary>
    public class UserTokenInfo
    {
        /// <summary>
        /// 用户端自定义的命名
        /// </summary>
        public string UserId { get; set; } = "Anonymity";

        /// <summary>
        /// 用户端的唯一编码
        /// </summary>
        public string UserCode { get; set; } = Guid.NewGuid().ToString("D");

        /// <summary>
        /// 用户端的地址
        /// </summary>
        public string UserHost { get; set; }

        /// <summary>
        /// 用户端的端口
        /// </summary>
        public int? UserPort { get; set; }

        /// <summary>
        /// 用户端的连接时间
        /// </summary>
        public DateTime? ConnectTime { get; set; }

        /// <summary>
        /// 连接套接字
        /// </summary>
        internal AcceptEventArgs AcceptEvent { get; set; }

        /// <summary>
        /// 暂存消息(会话级)
        /// </summary>
        internal Memory<byte> DecodeQueue { get; set; }

        /// <summary>
        /// 清空缓存
        /// </summary>
        internal void Clear()
        {
            UserId = null;
            UserCode = null;
            UserHost = null;
            UserPort = null;
            ConnectTime = null;
            DecodeQueue = Memory<byte>.Empty;
            AcceptEvent.Clear();
        }

    }
}
