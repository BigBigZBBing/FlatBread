using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace 重写SocketEvent
{
    class Program
    {
        static void Main(string[] args)
        {
            SocketMessage socketMessage = new SocketMessage();

            socketMessage.Start(new IPEndPoint(IPAddress.Any, 13909), 200, 10);
            Console.WriteLine("TCP服务已开启");
            socketMessage.ClientChange += (count, token) =>
            {
                Console.Title = $"在线用户数量:{count}";
                if (token.Socket != null)
                    Console.WriteLine($"用户上线----信息:【IP:{token.Remote}】【Port:{token.Port}】");
                else
                    Console.WriteLine($"用户下线----信息:【IP:{token.Remote}】【Port:{token.Port}】");
            };

            socketMessage.ReceiveData += (token, data) =>
            {
                Console.WriteLine($"接收到用户【{token.Remote}:{token.Port}】的信息:{Encoding.UTF8.GetString(data)}");
            };

            Console.ReadKey();
        }


    }
}
