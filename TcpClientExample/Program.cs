using FlatBread.Tcp;
using System;
using System.Text;

namespace TcpClientExample
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient tcpClient = new TcpClient("127.0.0.1", 5656);
            tcpClient.UserName = "张炳彬";
            tcpClient.OnCallBack = (bytes) =>
            {
                Console.WriteLine(bytes.Length);
            };
            tcpClient.StartConnect();

            tcpClient.OnConnect = (session) =>
            {
                Random random = new Random();

                while (true)
                {
                    string text = "测试数据" + random.Next(0, int.MaxValue);
                    session.SendMessage(text);
                    Console.WriteLine(text);
                }

            };

            Console.ReadLine();
        }
    }
}
