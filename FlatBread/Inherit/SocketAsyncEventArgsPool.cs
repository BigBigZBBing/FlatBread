using FlatBread.Buffer;
using FlatBread.Log;
using FlatBread.User;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace FlatBread.Inherit
{
    public class SocketAsyncEventArgsPool : SocketAsyncEventArgs
    {
        /// <summary>
        /// 接套字池
        /// </summary>
        private ConcurrentStack<SocketAsyncEventArgs> AsyncEventArgsPool { get; set; }

        /// <summary>
        /// 接收到用户后
        /// </summary>
        private Action<SocketAsyncEventArgs> IOCompleted { get; set; }

        /// <summary>
        /// 缓冲区池
        /// </summary>
        private BufferPool _BufferPool { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="PoolSize">接套字池大小</param>
        public SocketAsyncEventArgsPool(int BufferSize, int PoolSize, Action<SocketAsyncEventArgs> Completed)
        {
            _BufferPool = new BufferPool(BufferSize, PoolSize);
            AsyncEventArgsPool = new ConcurrentStack<SocketAsyncEventArgs>();
            IOCompleted = Completed;
            for (int i = 0; i < PoolSize; i++)
            {
                AsyncEventArgsPool.Push(InitEventArgs());
            }
        }

        /// <summary>
        /// 初始化用户端接套字容器
        /// </summary>
        /// <returns></returns>
        private SocketAsyncEventArgs InitEventArgs()
        {
            SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
            eventArgs.Completed += (o, s) => IOCompleted(s);
            eventArgs.UserToken = new UserTokenInfo();
            eventArgs.SetBuffer(_BufferPool.Pop());
            return eventArgs;
        }

        /// <summary>
        /// 弹出接套字
        /// </summary>
        /// <returns></returns>
        public SocketAsyncEventArgs Pop()
        {
            if (AsyncEventArgsPool.TryPop(out var item))
            {
                return item;
            }
            LogHelper.LogError("接套字池缺失~");
            return null;
        }

        /// <summary>
        /// 推入接套字池
        /// </summary>
        /// <param name="AsyncEventArgs"></param>
        public void Push(SocketAsyncEventArgs AsyncEventArgs)
        {
            AsyncEventArgsPool.Push(AsyncEventArgs);
        }
    }
}
