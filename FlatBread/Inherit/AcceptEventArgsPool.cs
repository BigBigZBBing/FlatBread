using FlatBread.Buffer;
using FlatBread.User;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace FlatBread.Inherit
{
    public class AcceptEventArgsPool
    {
        /// <summary>
        /// 接收池
        /// </summary>
        private ConcurrentStack<AcceptEventArgs> AcceptPool { get; set; }

        /// <summary>
        /// 接收接套字 用于定位内存地址
        /// </summary>
        private List<OperationEventArgs> ReceiveEventArgs { get; set; }

        /// <summary>
        /// 发送接套字 用于定位内存地址
        /// </summary>
        private List<OperationEventArgs> SendEventArgs { get; set; }

        /// <summary>
        /// 接收缓冲区池
        /// </summary>
        private BufferPool ReceiveBufferPool { get; set; }

        /// <summary>
        /// 发送缓冲区池
        /// </summary>
        private BufferPool SendBufferPool { get; set; }

        /// <summary>
        /// 接收池
        /// </summary>
        /// <param name="BufferSize"></param>
        /// <param name="PoolSize"></param>
        /// <param name="AcceptCompleted"></param>
        /// <param name="IOCompleted"></param>
        public AcceptEventArgsPool(int BufferSize, int PoolSize, EventHandler<SocketAsyncEventArgs> AcceptCompleted, EventHandler<SocketAsyncEventArgs> IOCompleted)
        {
            AcceptPool = new ConcurrentStack<AcceptEventArgs>();
            ReceiveBufferPool = new BufferPool(BufferSize, PoolSize);
            SendBufferPool = new BufferPool(BufferSize, PoolSize);
            ReceiveEventArgs = new List<OperationEventArgs>();
            SendEventArgs = new List<OperationEventArgs>();
            OperationEventArgs tempEventArgs1 = null;
            OperationEventArgs tempEventArgs2 = null;
            AcceptEventArgs tempEventArgs3 = null;
            UserTokenInfo tempUserTokenInfo = null;
            for (int i = 0; i < PoolSize; i++)
            {
                tempUserTokenInfo = new UserTokenInfo();
                tempEventArgs1 = new OperationEventArgs();
                tempEventArgs1.UserToken = tempUserTokenInfo;
                tempEventArgs1.Completed += IOCompleted;
                tempEventArgs1.SetBuffer(ReceiveBufferPool.Pop());

                tempEventArgs2 = new OperationEventArgs();
                tempEventArgs2.UserToken = tempUserTokenInfo;
                tempEventArgs2.Completed += IOCompleted;
                tempEventArgs2.SetBuffer(SendBufferPool.Pop());

                tempEventArgs3 = new AcceptEventArgs(tempEventArgs1, tempEventArgs2);
                tempEventArgs3.Completed += AcceptCompleted;
                tempEventArgs3.UserToken = tempUserTokenInfo;

                AcceptPool.Push(tempEventArgs3);
            }
        }

        /// <summary>
        /// 推出池
        /// </summary>
        /// <returns></returns>
        public AcceptEventArgs Pop()
        {
            if (AcceptPool.TryPop(out var item))
            {
                return item;
            }
            return null;
        }

        /// <summary>
        /// 推入池
        /// </summary>
        /// <param name="item"></param>
        public void Push(AcceptEventArgs item)
        {
            AcceptPool.Push(item);
        }
    }
}
