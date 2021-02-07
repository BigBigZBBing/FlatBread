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
            var session = tcpClient.StartConnect();
            while (true)
            {
                string text = Console.ReadLine();
                session.SendMessage(text);
            }

            Console.ReadLine();
        }
    }
}
