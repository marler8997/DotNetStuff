using System;
using System.Net.Sockets;

namespace More
{
    public interface IDataHandler : IDisposable
    {
        void HandleData(Byte[] data, Int32 offset, Int32 length);
    }
    public interface IDataFilter
    {
        void FilterTo(IDataHandler handler, Byte[] data, Int32 offset, Int32 length);
    }
    public interface IDataHandlerChainFactory
    {
        IDataHandler CreateDataHandlerChain(IDataHandler passTo);
    }
    public class SocketDataHandler : IDataHandler
    {
        public readonly Socket socket;
        public SocketDataHandler(Socket socket)
        {
            this.socket = socket;
        }
        public void HandleData(byte[] data, int offset, int length)
        {
            socket.Send(data, offset, length, SocketFlags.None);
        }
        public void Dispose()
        {
            socket.ShutdownAndDispose();
        }
    }
}
