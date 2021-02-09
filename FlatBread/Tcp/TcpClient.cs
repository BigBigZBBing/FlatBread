using FlatBread.Enum;
using FlatBread.Inherit;
using FlatBread.Session;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlatBread.Tcp
{
    public class TcpClient
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

        /// <summary>
        /// 缓冲位大小
        /// </summary>
        internal int BufferSize { get; set; } = 4096;

        /// <summary>
        /// 发送使用的缓冲位
        /// </summary>
        internal byte[] SendBuffer { get; set; }

        /// <summary>
        /// 接收使用的缓冲位
        /// </summary>
        internal Memory<byte> ReceiveBuffer { get; set; }

        /// <summary>
        /// 握手使用的接套字
        /// </summary>
        internal ShakeHandEventArgs ShakeHandEvent { get; set; }

        public TcpClient(string host, int port)
        {
            this.Host = host;
            this.Port = port;

            //初始化发送接套字
            SendBuffer = new byte[BufferSize];
            SendEventArgs SendEvent = new SendEventArgs();
            SendEvent.Completed += AsyncDispatchCenter;
            //发送是根据内容动态缓冲大小

            //初始化接收接套字
            ReceiveBuffer = new byte[BufferSize];
            ReceiveEventArgs ReceiveEvent = new ReceiveEventArgs();
            ReceiveEvent.Completed += AsyncDispatchCenter;
            ReceiveEvent.SetBuffer(ReceiveBuffer);

            ShakeHandEvent = new ShakeHandEventArgs(ReceiveEvent, SendEvent);
            ShakeHandEvent.Completed += AsyncDispatchCenter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartConnect()
        {
            Socket Client = new Socket(AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            var address = Dns.GetHostAddresses(Host);
            if (address.Length == 0) throw new ArgumentNullException("Host to dns analytics is null!");

            //优先初始化用户信息
            UserTokenSession Session = new UserTokenSession();
            Session.ShakeHandEvent = ShakeHandEvent;
            ShakeHandEvent.UserToken = Session;
            ShakeHandEvent.RemoteEndPoint = new IPEndPoint(address.FirstOrDefault(), Port);
            if (!Client.ConnectAsync(ShakeHandEvent))
            {
                ProcessConnect(ShakeHandEvent);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AsyncDispatchCenter(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                //检测到连接行为
                case SocketAsyncOperation.Connect:
                    ProcessConnect(e);
                    break;
                //检测到接收行为
                case SocketAsyncOperation.Receive:
                    SocketAsyncReceive(e);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ProcessConnect(SocketAsyncEventArgs e)
        {
            //连接服务端成功
            if (e.SocketError == SocketError.Success)
            {
                ShakeHandEventArgs ShakeHand = (ShakeHandEventArgs)e;
                UserTokenSession Session = (UserTokenSession)e.UserToken;
                Session.Mode = SocketMode.Client;
                Session.OperationTime = DateTime.Now;
                ShakeHand.ReceiveEventArgs.UserToken = Session;
                Session.ShakeHandEvent = ShakeHand;

                ThreadPool.QueueUserWorkItem((e) => OnConnect?.Invoke(Session));

                //接收服务端传来的流
                if (!Session.Channel.ReceiveAsync(ShakeHand.ReceiveEventArgs))
                {
                    SocketAsyncReceive(ShakeHand.ReceiveEventArgs);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SocketAsyncReceive(SocketAsyncEventArgs e)
        {
            ReceiveEventArgs eventArgs = (ReceiveEventArgs)e;
            UserTokenSession UserToken = (UserTokenSession)eventArgs.UserToken;
            if (eventArgs.SocketError == SocketError.Success && eventArgs.BytesTransferred > 0)
            {
                //解码回调
                eventArgs.Decode(bytes => OnCallBack?.Invoke(bytes));

                //释放行为接套字的连接(此步骤无意义,只是以防万一)
                eventArgs.AcceptSocket = null;

                //继续接收消息
                if (!UserToken.Channel.ReceiveAsync(e))
                {
                    //此次接收没有接收完毕 递归接收
                    SocketAsyncReceive(e);
                }
            }
        }

        /// <summary>
        /// 接收结果回调
        /// </summary>
        public Action<byte[]> OnCallBack { get; set; }

        /// <summary>
        /// 连接成功回调
        /// </summary>
        public Action<UserTokenSession> OnConnect { get; set; }
    }
}
