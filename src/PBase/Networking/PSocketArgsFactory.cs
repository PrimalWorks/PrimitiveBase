using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace PBase.Networking
{
    public class PSocketArgsFactory : ISocketAsyncEventArgsFactory
    {
        private readonly byte[] m_buffer;
        private readonly Stack<int> m_freeIndexPool;
        private int m_currentIndex;
        private readonly int m_bufferSize;

        public PSocketArgsFactory(int totalBytes, int bufferSize)
        {
            m_buffer = new byte[totalBytes];
            m_currentIndex = 0;
            m_bufferSize = bufferSize;
            m_freeIndexPool = new Stack<int>();
        }

        public ISocketAsyncEventArgs GetEmptyArgs()
        {
            return new PSocketAsyncEventArgs();
        }

        public ISocketAsyncEventArgs AllocateArgs()
        {
            var args = new PSocketAsyncEventArgs();
            if (m_freeIndexPool.Count > 0)
            {
                args.SetBuffer(m_buffer, m_freeIndexPool.Pop(), m_bufferSize);
            }
            else
            {
                if ((m_buffer.Length - m_bufferSize) < m_currentIndex)
                {
                    throw new ArgumentException("Exhausted Memory Allocation for SocketAsyncEventArgs");
                }
                args.SetBuffer(m_buffer, m_currentIndex, m_bufferSize);
                m_currentIndex += m_bufferSize;
            }
            return args;
        }

        public void FreeArgs(SocketAsyncEventArgs args)
        {
            if (args is ISocketAsyncEventArgs)
            {
                FreeArgs(args as ISocketAsyncEventArgs);
            }
            else
            {
                throw new ArgumentException("Can only free types of ISocketAsyncEventArgs");
            }
        }

        public void FreeArgs(ISocketAsyncEventArgs args)
        {
            if (args.Buffer != m_buffer)
            {
                throw new ArgumentException("Can only free args allocated by this factory");
            }

            m_freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }
    }
}
