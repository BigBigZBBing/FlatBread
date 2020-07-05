using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QuickSocket
{
    public class SocketMessage
    {

        Socket Main;
        SocketEventPool SocketEventPool;
        //BufferManager bufferManager;
        List<AsyncUserToken> Clients;

        public delegate void OnClientChange(int client_num, AsyncUserToken client);
        public delegate void OnReceiveData(AsyncUserToken token, byte[] buff);
        public event OnClientChange ClientChange;
        public event OnReceiveData ReceiveData;

        public void Start(IPEndPoint iPEndPoint, int capacity, int bufferSize)
        {
            Main = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Main.Bind(iPEndPoint);
            Main.Listen(capacity);
            SocketEventPool = new SocketEventPool(capacity);
            Clients = new List<AsyncUserToken>();
            //bufferManager = new BufferManager(capacity * bufferSize * 2, bufferSize);
            //bufferManager.InitBuffer();
            SocketAsyncEventArgs readWriteEventArg;

            for (int i = 0; i < capacity; i++)
            {
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += IO_Completed;
                readWriteEventArg.UserToken = new AsyncUserToken();
                //bufferManager.SetBuffer(readWriteEventArg);
                SocketEventPool.Push(readWriteEventArg);
            }

            StartAccept(null);
        }

        void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                acceptEventArg.AcceptSocket = null;
            }

            if (!Main.AcceptAsync(acceptEventArg))
            {
                ProcessAccept(acceptEventArg);
            }
        }
        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        void ProcessAccept(SocketAsyncEventArgs e)
        {
            try
            {
                SocketAsyncEventArgs readEventArgs = SocketEventPool.Pop();
                AsyncUserToken userToken = readEventArgs.UserToken as AsyncUserToken;
                if (userToken != null)
                {
                    userToken.Socket = e.AcceptSocket;
                    userToken.ConnectTime = DateTime.Now;
                    userToken.Remote = e.AcceptSocket.RemoteEndPoint;
                    userToken.Port = ((IPEndPoint)(e.AcceptSocket.RemoteEndPoint)).Port;
                    lock (Clients)
                    {
                        Clients.Add(userToken);
                    }
                    ClientChange?.Invoke(Clients.Count, userToken);
                    if (!e.AcceptSocket.ReceiveAsync(readEventArgs))
                    {
                        ProcessReceive(readEventArgs);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            if (e.SocketError == SocketError.OperationAborted)
                return;
            StartAccept(e);
        }

        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                //case SocketAsyncOperation.Send:
                //ProcessSend(e);
                //break;
                default:
                    throw new ArgumentException("套接字上完成的最后一个操作不是接收或发送");
            }
        }

        void ProcessReceive(SocketAsyncEventArgs e)
        {
            try
            {
                AsyncUserToken token = e.UserToken as AsyncUserToken;
                if (token != null && e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    byte[] data = new byte[e.BytesTransferred];
                    Array.Copy(e.Buffer, e.Offset, data, 0, e.BytesTransferred);
                    token.Buffer.AddRange(data);
                    if (token.Socket.Available == 0)
                    {
                        ReceiveData?.Invoke(token, token.Buffer.ToArray());
                        token.Buffer.Clear();
                    }
                    if (!token.Socket.ReceiveAsync(e))
                        ProcessReceive(e);
                }
                else
                    CloseClientSocket(e);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        void CloseClientSocket(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = e.UserToken as AsyncUserToken;

            lock (Clients) { Clients.Remove(token); }
            try
            {
                token.Socket.Shutdown(SocketShutdown.Both);

            }
            catch { }
            if (token != null && token.Socket != null)
            {
                token.Socket.Close();
                token.Socket.Dispose();
                token.Socket = null;
            }
            ClientChange?.Invoke(Clients.Count, token);
            e.UserToken = new AsyncUserToken();
            SocketEventPool.Push(e);
        }
    }
}
