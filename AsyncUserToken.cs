using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QuickSocket
{
    public class AsyncUserToken
    {
        public int Port { get; set; }

        public EndPoint Remote { get; set; }

        public Socket Socket { get; set; }

        public DateTime ConnectTime { get; set; }

        public List<byte> Buffer { get; set; }

        public AsyncUserToken() =>
            this.Buffer = new List<byte>();
    }
}
