using FlatBread.Tcp;
using System;
using System.Text;
using System.Threading;

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
                    Console.ReadLine();
                    string text = "测试数据" + random.Next(0, int.MaxValue);
                    session.SendMessage(text);
                }

                //while (true)
                //{
                //    string text = "测试数据" + random.Next(0, int.MaxValue);
                //    session.SendMessage(text);
                //    Console.WriteLine(text);
                //}

            };

            tcpClient.OnCallBack = (bytes) =>
            {
                Console.WriteLine("返回内容:" + Encoding.UTF8.GetString(bytes)); ;
            };

            Thread.Sleep(-1);
        }
    }
}
