using PBase.Logging;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PBase.Networking
{
    public class SocketConn : BaseDisposable
    {
        private readonly ISocket m_socket;
        private readonly ISocketAsyncEventArgsFactory m_factory;
        private bool m_isConnected;
        private bool m_isConnecting;
        private NetAddress m_remote;

        private Func<ISocketAsyncEventArgs, bool> m_receiveCallback;

        public SocketConn(ISocket socket, ISocketAsyncEventArgsFactory factory)
        {
            m_socket = socket;
            m_factory = factory;
        }

        protected override void Dispose(bool disposing)
        {
            m_isConnected = false;
            m_isConnecting = false;
            m_socket.Dispose();
        }

        public Task<bool> Connect(NetAddress remote, Func<ISocketAsyncEventArgs, bool> receiveCallback)
        {
            PLog.LogTrace($"Connecting to {remote}");
            TaskCompletionSource<bool> src = new TaskCompletionSource<bool>();
            if (
                (m_isConnected) ||
                (m_isConnecting)
                )
            {
                src.SetResult(false);
                return src.Task;
            }
            m_isConnecting = true;
            m_remote = remote;
            try
            {
                m_receiveCallback = receiveCallback;
                ISocketAsyncEventArgs args = m_factory.AllocateArgs();
                args.UserToken = src;
                args.RemoteEndPoint = m_remote;
                args.Completed += OnConnectCompleted;
                if (!m_socket.ConnectAsync(args))
                {
                    OnConnectCompleted(this, args as SocketAsyncEventArgs);
                }
            }
            catch (Exception ex)
            {
                PLog.LogError($"Failed to connect to {m_remote}");
                m_isConnecting = false;
                m_receiveCallback = null;
                src.SetException(ex);
            }

            return src.Task;
        }

        private void OnConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            var src = e.UserToken as TaskCompletionSource<bool>;
            var result = e.SocketError;
            try
            {
                m_factory.FreeArgs(e);

                if (src == null)
                {
                    PLog.LogError("SockConn requires UserToken to be of type TaskCompletionSource<bool>");
                    throw new ArgumentException("SockConn requires UserToken to be of type TaskCompletionSource<bool>");
                }

                if (result == SocketError.Success)
                {
                    PLog.LogTrace($"Connected to {m_remote}");
                    m_isConnected = true;
                    src.SetResult(true);
                }
                else
                {
                    PLog.LogWarning($"Failed to connect to {m_remote} with error {result}");
                    src.SetResult(false);
                    m_isConnecting = false;
                    return;
                }

                //
                // Setup our receive loop
                //
                var args = m_factory.AllocateArgs();
                args.RemoteEndPoint = m_remote;
                args.Completed += (s, a) =>
                {
                    try
                    {
                        PLog.LogTrace($"Received Data from {m_remote}, OnCompleted {a.LastOperation} with {a.SocketError}");
                        if (a.SocketError == SocketError.Success)
                        {
                            if (a.LastOperation == SocketAsyncOperation.Receive)
                            {
                                if (
                                    (m_receiveCallback != null) &&
                                    (!m_receiveCallback.Invoke(a as ISocketAsyncEventArgs))
                                    )
                                {
                                    //
                                    // Handler returned false, so we need new Args for next call
                                    // as the buffer is still being used
                                    //
                                    args = m_factory.AllocateArgs();
                                    args.RemoteEndPoint = m_remote;
                                }
                            }
                            m_socket.ReceiveAsync(args);
                        }
                        else
                        {
                            PLog.LogWarning($"Receive operation failed from {m_remote}, stopping Receive loop");
                            m_isConnected = false;
                            m_receiveCallback = null;
                            m_factory.FreeArgs(args);
                        }
                    }
                    catch (Exception ex)
                    {
                        PLog.LogError(ex, $"Execption while receiving data from {m_remote}");
                        m_isConnected = false;
                        m_receiveCallback = null;
                        m_factory.FreeArgs(args);
                        Console.WriteLine(ex.ToString());
                    }
                };
                m_socket.ReceiveAsync(args);
            }
            catch (Exception ex)
            {
                PLog.LogError(ex, $"Execption while connecting to {m_remote}");
                m_isConnected = false;
                m_isConnecting = false;
                m_receiveCallback = null;
                src.SetException(ex);
            }
        }

        public Task<bool> SendBuffer(byte[] data)
        {
            return SendBuffer(data, 0, data.Length);
        }

        public Task<bool> SendBuffer(byte[] data, int offset, int count)
        {
            PLog.LogTrace($"Sending data to {m_remote}, Data Length = {count}");
            TaskCompletionSource<bool> src = new TaskCompletionSource<bool>();
            if (!m_isConnected)
            {
                PLog.LogWarning("Failed to send. Needs to be connected before calling send");
                src.SetResult(false);
                return src.Task;
            }

            try
            {
                var args = m_factory.GetEmptyArgs(); // No need to Free these, as using own buffer
                args.UserToken = src;
                args.RemoteEndPoint = m_remote;
                args.SetBuffer(data, offset, count);
                args.Completed += OnSendBufferCompleted;

                if (!m_socket.SendAsync(args))
                {
                    OnSendBufferCompleted(this, args as SocketAsyncEventArgs);
                }
            }
            catch (Exception ex)
            {
                PLog.LogError(ex, $"Exception while sending data to {m_remote}");
                src.SetException(ex);
            }

            return src.Task;
        }

        private void OnSendBufferCompleted(object sender, SocketAsyncEventArgs e)
        {
            var src = e.UserToken as TaskCompletionSource<bool>;
            var result = (e.SocketError == SocketError.Success) && (e.LastOperation == SocketAsyncOperation.Send);
            try
            {
                if (result)
                {
                    PLog.LogTrace($"Send to {m_remote} completed.");
                    src.SetResult(true);
                }
                else
                {
                    PLog.LogWarning($"Send operation to {m_remote} failed on {e.LastOperation} with {e.SocketError}");
                    src.SetResult(false);
                }
            }
            catch (Exception ex)
            {
                PLog.LogError(ex, $"Exception while completing send to {m_remote}");
                src.SetException(ex);
            }
        }
    }
}
