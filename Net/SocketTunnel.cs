using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net.Sockets;

namespace More.Net
{
    public interface ITunnel
    {
    }
    public interface ITunnelCallback
    {
        void TunnelClosed(ITunnel tunnel);
    }
    public class OneWaySocketTunnel : ITunnel
    {
        public readonly ITunnelCallback callback;
        public readonly Socket inputSocket, outputSocket;
        public readonly Int32 readBufferSize;

        private readonly MessageLogger messageLogger;
        private readonly IDataLogger dataLogger;

        private Boolean keepRunning;

        public OneWaySocketTunnel(ITunnelCallback callback,
            Socket inputSocket, Socket outputSocket, Int32 readBufferSize,
            MessageLogger messageLogger, IDataLogger dataLogger)
        {
            this.callback = callback;
            this.inputSocket = inputSocket;
            this.outputSocket = outputSocket;

            this.readBufferSize = readBufferSize;

            this.messageLogger = messageLogger;
            this.dataLogger = dataLogger;

            this.keepRunning = false;
        }

        public void Close()
        {
            if (inputSocket.Connected)
            {
                try { inputSocket.Shutdown(SocketShutdown.Both); } catch (SystemException) { }
                inputSocket.Close();
            }
            if (outputSocket.Connected)
            {
                try { outputSocket.Shutdown(SocketShutdown.Both); } catch (SystemException) { }
                outputSocket.Close();
            }
        }

        public void RunPrepare()
        {
            this.keepRunning = true;
        }
        public void Run()
        {
            try
            {
                byte[] buffer = new byte[readBufferSize];

                while (keepRunning)
                {
                    if (messageLogger != null) messageLogger.Log("Reading");

                    Int32 bytesRead = inputSocket.Receive(buffer, SocketFlags.None);

                    if (bytesRead <= 0)
                    {
                        if (messageLogger != null) messageLogger.Log("Got End of Stream");
                        break;
                    }

                    if (messageLogger != null) messageLogger.Log("Read {0} bytes", bytesRead);
                    if (dataLogger != null) dataLogger.LogData(buffer, 0, bytesRead);

                    if (!keepRunning) break;

                    if (messageLogger != null) messageLogger.Log("Sending {0} bytes", bytesRead);
                    outputSocket.Send(buffer, 0, bytesRead, SocketFlags.None);
                }
            }
            catch (SocketException se)
            {
                messageLogger.Log("SocketException: {0}", se.Message);
            }
            catch (ObjectDisposedException ode)
            {
                messageLogger.Log("ObjectDisposedException: {0}", ode.Message);
            }
            finally
            {
                this.keepRunning = false;
                if(callback != null) callback.TunnelClosed(this);
            }
        }
    }


    public class TwoWaySocketTunnel : ITunnelCallback
    {
        public const Int32 DefaultBufferSize = 1024;

        private readonly Socket socketA, socketB;
        private readonly OneWaySocketTunnel tunnelAToB, tunnelBToA;
        private Boolean tunnelClosed;

        private readonly ConnectionMessageLogger messageLogger;
        private readonly IConnectionDataLogger dataLogger;

        public TwoWaySocketTunnel(
            Socket socketA, Socket socketB, Int32 readBufferSize,
            ConnectionMessageLogger messageLogger, IConnectionDataLogger dataLogger)
        {
            if (messageLogger == null) throw new ArgumentNullException("messageLogger");
            if (dataLogger == null) throw new ArgumentNullException("dataLogger");

            if (socketA == null) throw new ArgumentNullException("socketA");
            if (socketB == null) throw new ArgumentNullException("socketB");

            this.socketA = socketA;
            this.socketB = socketB;

            this.tunnelAToB = new OneWaySocketTunnel(this, socketA, socketB, readBufferSize,
                messageLogger.AToBMessageLogger, dataLogger.AToBDataLogger);
            this.tunnelBToA = new OneWaySocketTunnel(this, socketB, socketA, readBufferSize,
                messageLogger.BToAMessageLogger, dataLogger.BToADataLogger);
            this.tunnelClosed = false;

            this.messageLogger = messageLogger;
            this.dataLogger = dataLogger;
        }
        public void StartOneAndRunOne()
        {
            tunnelAToB.RunPrepare();
            tunnelBToA.RunPrepare();

            new Thread(tunnelAToB.Run).Start();
            tunnelBToA.Run();
        }
        void ITunnelCallback.TunnelClosed(ITunnel tunnel)
        {
            if (!tunnelClosed)
            {
                lock (this)
                {
                    if (tunnelClosed) return;
                    tunnelClosed = true;
                }

                try
                {

                    if (socketA.Connected) try
                        {
                            messageLogger.LogAToB("socketA.Shutdown");
                            socketA.Shutdown(SocketShutdown.Both);
                        }
                        catch (SocketException se) { messageLogger.Log("SocketException in socketA.Shutdown: {0}", se.Message); }
                        catch (ObjectDisposedException ode) { messageLogger.Log("ObjectDisposedException in socketA.Shutdown: {0}", ode.Message); }

                    if (socketB.Connected) try
                        {
                            messageLogger.LogBToA("socketB.Shutdown");
                            socketB.Shutdown(SocketShutdown.Both);
                        }
                        catch (SocketException se) { messageLogger.Log("SocketException in socketB.Shutdown: {0}", se.Message); }
                        catch (ObjectDisposedException ode) { messageLogger.Log("ObjectDisposedException in socketB.Shutdown: {0}", ode.Message); }
                }
                finally
                {
                    try { socketA.Close(); } catch(IOException e) { }
                    try { socketB.Close(); } catch(IOException e) { }
                }
            }
        }
    }

}