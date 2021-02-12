using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FlatBread.Tcp
{
    public partial class TcpClient
    {
        /// <summary>
        /// 用户的名称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 服务地址
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 地址族
        /// <para>默认为IPV4</para>
        /// </summary>
        public AddressFamily AddressFamily { get; set; } = AddressFamily.InterNetwork;
    }
}
