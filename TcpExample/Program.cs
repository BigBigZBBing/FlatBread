using FlatBread.Tcp;
using System;
using System.Text;

namespace TcpExample
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpServer tcpServer = new TcpServer(5656);
            tcpServer.Host = "127.0.0.1";
            tcpServer.MaxConnect = 200;
            tcpServer.StartServer();

            tcpServer.OnConnect = (user) =>
            {
                Console.WriteLine("UserCode:" + user.UserCode);
                Console.WriteLine("UserHost:" + user.UserHost);
                Console.WriteLine("UserPort:" + user.UserPort);
                Console.WriteLine("==================Login===================");
            };

            tcpServer.OnReceive = (user, packet) =>
            {
                Console.WriteLine($"内容:{Encoding.UTF8.GetString(packet)} 长度:{packet.Length}");
                user.SendMessage(Encoding.UTF8.GetBytes("服务端收到"));
            };
            tcpServer.OnExit = (user) =>
            {
                Console.WriteLine("UserCode:" + user.UserCode);
                Console.WriteLine("UserHost:" + user.UserHost);
                Console.WriteLine("UserPort:" + user.UserPort);
                Console.WriteLine("==================Exit===================");
            };

            Console.ReadLine();
        }
    }
}
