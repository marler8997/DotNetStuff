using System;
using System.Net.Sockets;

namespace More
{
    public delegate void DataHandler(Byte[] data, Int32 offset, Int32 length);
    public interface IDataHandler : IDisposable
    {
        void HandleData(Byte[] data, Int32 offset, Int32 length);
    }
    
    public interface IDataFilter
    {
        void FilterTo(DataHandler handler, Byte[] data, Int32 offset, Int32 length);
    }

    public class DataFilterHandler : IDataHandler
    {
        readonly IDataFilter filter;
        readonly DataHandler handler;
        readonly IDisposable disposable;
        public DataFilterHandler(IDataFilter filter, DataHandler dataHandler)
        {
            this.filter = filter;
            this.handler = dataHandler;
            this.disposable = null;
        }
        public DataFilterHandler(IDataFilter filter, IDataHandler iDataHandler)
        {
            this.filter = filter;
            this.handler = iDataHandler.HandleData;
            this.disposable = iDataHandler;
        }
        public void HandleData(byte[] data, int offset, int length)
        {
            filter.FilterTo(handler, data, offset, length);
        }
        public void Dispose()
        {
            if (disposable != null) disposable.Dispose();
        }
    }



    public interface IDataHandlerChainFactory
    {
        IDataHandler CreateDataHandlerChain(IDataHandler passTo);
    }
    public class SocketSendDataHandler : IDataHandler
    {
        public readonly Socket socket;
        public SocketSendDataHandler(Socket socket)
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
