using FlatBread.Tcp;
using System;

namespace TcpExample
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpServer tcpServer = new TcpServer(5656);
            tcpServer.Host = "127.0.0.1";
            tcpServer.MaxConnect = 200;
            tcpServer.Open();

            tcpServer.OnConnect = (user) =>
            {
                Console.WriteLine("UserCode:" + user.UserCode);
                Console.WriteLine("UserHost:" + user.UserHost);
                Console.WriteLine("UserId:" + user.UserId);
                Console.WriteLine("UserPort:" + user.UserPort);
                Console.WriteLine("==================Login===================");
            };

            tcpServer.OnReceive = (channel, bytes) =>
            {
                Console.WriteLine(bytes.Length);
            };

            tcpServer.OnExit = (user) =>
            {
                Console.WriteLine("UserCode:" + user.UserCode);
                Console.WriteLine("UserHost:" + user.UserHost);
                Console.WriteLine("UserId:" + user.UserId);
                Console.WriteLine("UserPort:" + user.UserPort);
                Console.WriteLine("==================Exit===================");
            };
            Console.ReadLine();
        }
    }
}
