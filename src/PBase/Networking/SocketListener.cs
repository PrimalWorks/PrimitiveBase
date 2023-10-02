using PBase.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PBase.Networking
{
    public class SocketListener : BaseDisposable
    {
        private readonly ISocket m_socket;
        private readonly ISocketAsyncEventArgsFactory m_factory;
        private bool m_isListening;
        private bool m_isStartingToListen;
        private NetAddress m_local;
        private ISocketAsyncEventArgs m_acceptArgs;

        private Func<ISocketAsyncEventArgs, bool> m_acceptCallback;

        public SocketListener(ISocket socket, ISocketAsyncEventArgsFactory factory)
        {
            m_socket = socket;
            m_factory = factory;
        }

        protected override void Dispose(bool disposing)
        {
            if (m_isListening)
            {
                m_socket.Disconnect(false);
            }
            m_isListening = false;
            m_isStartingToListen = false;
            m_socket.Dispose();
        }

        public Task<bool> Listen(NetAddress local, Func<ISocketAsyncEventArgs, bool> accpetCallback)
        {
            PLog.LogTrace($"Start listening on {local}");

            TaskCompletionSource<bool> src = new TaskCompletionSource<bool>();
            if (
                (m_isListening) ||
                (m_isStartingToListen)
                )
            {
                src.SetResult(false);
                return src.Task;
            }
            m_isStartingToListen = true;
            m_local = local;

            m_socket.Bind(local);
            m_socket.Listen();

            try
            {
                m_acceptCallback = accpetCallback;
                m_acceptArgs = m_factory.GetEmptyArgs();
                m_acceptArgs.Completed += OnAcceptCompleted;
                m_acceptArgs.AcceptSocket = null;
                if (!m_socket.AcceptAsync(m_acceptArgs))
                {
                    OnAcceptCompleted(this, m_acceptArgs as SocketAsyncEventArgs);
                }

                PLog.LogTrace($"Listening on {m_local}");
                m_isListening = true;
                src.SetResult(true);
            }
            catch (Exception ex)
            {
                PLog.LogError($"Failed to listen on {m_local}");
                m_isListening = false;
                m_acceptCallback = null;
                m_factory.FreeArgs(m_acceptArgs);
                src.SetException(ex);
            }

            return src.Task;
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            var result = args.SocketError;

            try
            {
                if (result == SocketError.Success)
                {
                    PLog.LogTrace($"Accept socket on {m_local}");
                    if (m_acceptCallback != null)
                    {
                        m_acceptCallback.Invoke(args as ISocketAsyncEventArgs);
                    }                        
                }
                else
                {
                    PLog.LogWarning($"Failed to accept socket on {m_local} with error {result}");
                    m_isListening = false;
                    m_acceptCallback = null;
                    m_factory.FreeArgs(args);
                    return;
                }

            m_acceptArgs.AcceptSocket = null;
            m_socket.AcceptAsync(m_acceptArgs);
            }
            catch (Exception ex)
            {
                PLog.LogError(ex, $"Execption while accepting socket on {m_local}");
                m_isListening = false;
                m_isStartingToListen = false;
                m_acceptCallback = null;
                m_factory.FreeArgs(args);
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
