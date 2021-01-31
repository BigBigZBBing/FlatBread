using FlatBread.Inherit;
using FlatBread.Log;
using System.Net;
using System.Net.Sockets;
using FlatBread.User;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FlatBread.Tcp
{
    /// <summary>
    /// TCP服务端方案
    /// </summary>
    public class TcpServer
    {
        /// <summary>
        /// 服务器地址
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 服务器端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 最大连接数
        /// </summary>
        public int MaxConnect { get; set; } = 1000;

        /// <summary>
        /// 最大等待队列数
        /// </summary>
        public int MaxWaitQueue { get; set; } = 100;

        /// <summary>
        /// 缓冲位大小
        /// </summary>
        public int BufferSize { get; set; } = 4096;

        /// <summary>
        /// 缓冲池单元
        /// </summary>
        public int BufferPoolUnit { get; set; }

        /// <summary>
        /// 缓冲池最大容量
        /// </summary>
        public int BufferPoolSize { get { return _BufferPoolSize = BufferSize * BufferPoolUnit; } }
        int _BufferPoolSize = 0;

        /// <summary>
        /// 地址族
        /// <para>默认为IPV4</para>
        /// </summary>
        public AddressFamily AddressFamily { get; set; } = AddressFamily.InterNetwork;

        /// <summary>
        /// 构造函数
        /// <para>地址默认为当前计算机地址</para>
        /// </summary>
        /// <param name="Port">服务器端口</param>
        public TcpServer(int Port)
        {
            this.Port = Port;
        }

        /// <summary>
        /// 服务端Socket
        /// </summary>
        Socket ServerSocket { get; set; }

        /// <summary>
        /// 开启服务
        /// </summary>
        public void Open()
        {
            //1.服务端绑定、监听
            {
                switch (AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        ServerSocket = new Socket(AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        ServerSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
                        break;
                    case AddressFamily.InterNetworkV6:
                        ServerSocket = new Socket(AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        ServerSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, Port));
                        break;
                }
                ServerSocket.Listen(65535);
            }
            LogHelper.LogInfo("Tcp服务已经开始监听~");

            //2.初始化用户端接套字容器池、缓存池
            {
                SocketEventPool = new AcceptEventArgsPool(BufferSize, MaxConnect, AcceptProcess, CenterControl);
            }
            LogHelper.LogInfo("容器池已加载完毕~");

            //3.开始调用用户端接套字容器池监听
            {
                AcceptAsync();
            }
        }

        /// <summary>
        /// 用户端接套字容器池
        /// </summary>
        AcceptEventArgsPool SocketEventPool { get; set; }

        /// <summary>
        /// 接收用户端的连接
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AcceptAsync()
        {
            var evetArgs = SocketEventPool.Pop();
            //异步接收连接
            if (!ServerSocket.AcceptAsync(evetArgs))
            {
                LogHelper.LogWarn("异步接收用户回调失败 同步进入回调函数~");

                //如果异步接收失败则同步接收
                AcceptProcess(null, evetArgs);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AcceptProcess(object sender, SocketAsyncEventArgs e)
        {
            AcceptEventArgs eventArgs = e as AcceptEventArgs;

            //接收用户成功
            if (eventArgs.LastOperation == SocketAsyncOperation.Accept && eventArgs.SocketError == SocketError.Success)
            {
                //创建会话信息(当前会话)
                UserTokenInfo UserToken = eventArgs.UserToken as UserTokenInfo;
                UserToken.ConnectTime = DateTime.Now;
                var EndPoint = (IPEndPoint)eventArgs.AcceptSocket.RemoteEndPoint;
                var AllHost = Dns.GetHostEntry(EndPoint.Address).AddressList;
                UserToken.UserHost = string.Join('|', AllHost.Select(x => x.ToString()).ToArray());
                UserToken.UserPort = ((IPEndPoint)(eventArgs.AcceptSocket.RemoteEndPoint)).Port;
                UserToken.AcceptEvent = eventArgs;
                OnConnect?.Invoke(UserToken);
                eventArgs.ReceiveEventArgs.UserToken = UserToken;
                eventArgs.SendEventArgs.UserToken = UserToken;

                //异步接收客户端行为
                ReceiveProcess(UserToken);
            }
            else
            {
                LogHelper.LogError($"连接失败~ 收到的状态:SocketError.{eventArgs.SocketError}");
                //接收用户失败就清理后送回池
                eventArgs.Clear();
                SocketEventPool.Push(eventArgs);
            }
            //继续接收下一个
            AcceptAsync();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ReceiveProcess(UserTokenInfo UserToken)
        {
            //异步接收客户端消息
            if (!UserToken.AcceptEvent.AcceptSocket
                    .ReceiveAsync(UserToken.AcceptEvent.ReceiveEventArgs))
            {
                SocketAsyncReceive(UserToken.AcceptEvent.ReceiveEventArgs);
            }
        }

        /// <summary>
        /// 完成接收用户端接套字指令的中控调度室
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CenterControl(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                //检测到接收行为
                case SocketAsyncOperation.Receive:
                    SocketAsyncReceive(e);
                    break;
                //检测到发送行为
                case SocketAsyncOperation.Send:
                    SocketAsyncSend(e);
                    break;
                //检测到连接行为(这步在接收接收过程那已处理)
                case SocketAsyncOperation.Connect: break;
                //检测到断开行为
                case SocketAsyncOperation.Disconnect:
                    SocketAsyncDisconnect(e);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SocketAsyncReceive(SocketAsyncEventArgs e)
        {
            OperationEventArgs eventArgs = e as OperationEventArgs;
            UserTokenInfo UserToken = eventArgs.UserToken as UserTokenInfo;
            if (eventArgs.SocketError == SocketError.Success && eventArgs.BytesTransferred > 0)
            {
                //解码回调
                eventArgs.Decode(BufferSize, bytes => OnReceive?.Invoke(UserToken, bytes));

                //释放行为接套字的连接(此步骤无意义,只是以防万一)
                eventArgs.AcceptSocket = null;

                //继续接收消息
                if (!UserToken.AcceptEvent.AcceptSocket.ReceiveAsync(e))
                {
                    //此次接收没有接收完毕 递归接收
                    SocketAsyncReceive(e);
                }
            }
            else
            {
                //客户端正常走这步
                OnExit?.Invoke(UserToken);
                //清理连接接套字
                UserToken.Clear();
                //推回接套字池
                SocketEventPool.Push(UserToken.AcceptEvent);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SocketAsyncSend(SocketAsyncEventArgs e)
        {
            OperationEventArgs eventArgs = e as OperationEventArgs;
            UserTokenInfo UserToken = eventArgs.UserToken as UserTokenInfo;
            if (eventArgs.SocketError == SocketError.Success)
            {
                if (!e.AcceptSocket.SendAsync(eventArgs))
                {

                }
            }
            else
            {

            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SocketAsyncDisconnect(SocketAsyncEventArgs e)
        {
            OperationEventArgs eventArgs = e as OperationEventArgs;
            UserTokenInfo UserToken = eventArgs.UserToken as UserTokenInfo;
            OnExit?.Invoke(UserToken);
            //检测到用户退出送回套接字池
            SocketEventPool.Push(UserToken.AcceptEvent);
        }

        public Action<UserTokenInfo> OnConnect { get; set; }
        public Action<UserTokenInfo, byte[]> OnReceive { get; set; }
        public Action<UserTokenInfo> OnExit { get; set; }
    }
}
