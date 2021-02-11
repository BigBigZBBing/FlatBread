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
            RandomChinese randomChinese = new RandomChinese();
            TcpClient tcpClient = new TcpClient("127.0.0.1", 5656);
            tcpClient.UserName = "张炳彬";
            tcpClient.OnCallBack = (user, packet) =>
            {
                Console.WriteLine(packet.Length);
            };
            tcpClient.StartConnect();

            tcpClient.OnConnect = (session) =>
            {
                Random random = new Random();

                //手动
                //while (true)
                //{
                //    Console.ReadLine();
                //    string text = randomChinese.GetRandomChinese(random.Next(1, 200));
                //    byte[] message = Encoding.UTF8.GetBytes(text);
                //    Console.WriteLine($"发送内容长度:{message.Length}");
                //    session.SendMessage(message);
                //}

                //自动
                while (true)
                {
                    string text = randomChinese.GetRandomChinese(random.Next(1, 200));
                    byte[] message = Encoding.UTF8.GetBytes(text);
                    session.SendMessage(message);
                    Thread.Sleep(100);
                }

            };

            tcpClient.OnCallBack = (user, packet) =>
            {
                Console.WriteLine("返回内容:" + Encoding.UTF8.GetString(packet)); ;
            };

            Thread.Sleep(-1);
        }
    }
}
