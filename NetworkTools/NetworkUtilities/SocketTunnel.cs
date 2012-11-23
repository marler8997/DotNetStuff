using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net.Sockets;

namespace Marler.NetworkTools
{
    public interface SocketTunnelCallback
    {
        void TunnelClosed(SocketTunnelThread tunnel);
    }
    public class SocketTunnelThread
    {
        public readonly SocketTunnelCallback callback;
        public readonly Socket inputSocket, outputSocket;
        public readonly Int32 readBufferSize;

        private readonly MessageLogger messageLogger;
        private readonly IDataLogger dataLogger;

        private Boolean keepRunning;

        public SocketTunnelThread(SocketTunnelCallback callback, Socket inputSocket, Socket outputSocket, Int32 readBufferSize,
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
        public void RunPrepare()
        {
            this.keepRunning = true;
        }
        private void Run()
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
                callback.TunnelClosed(this);
            }
        }
    }
    /*
    public class SocketTunnel
    {
        public const Int32 DefaultBufferSize = 1024;

        private readonly ConnectionMessageLogger connectionMessageLogger;
        private readonly IConnectionDataLogger connectionDataLogger;
        private readonly Socket socketA, socketB;

        //public readonly String messageLoggerName;
        private readonly SocketTunnelThread tunnelThreadA, tunnelThreadB;

        private Boolean tunnelClosed;

        public SocketTunnel(ConnectionMessageLogger connectionMessageLogger, IConnectionDataLogger connectionDataLogger,
            Socket socketA, Socket socketB, Int32 readBufferSize)
        {
            if (connectionMessageLogger == null) throw new ArgumentNullException("connectionMessageLogger");
            if (connectionDataLogger == null) throw new ArgumentNullException("connectionDataLogger");

            if (socketA == null) throw new ArgumentNullException("socketA");
            if (socketB == null) throw new ArgumentNullException("socketB");

            this.connectionMessageLogger = connectionMessageLogger;
            this.connectionDataLogger = connectionDataLogger;

            this.socketA = socketA;
            this.socketB = socketB;

            //this.messageLoggerName = (connectionMessageLogger. == null) ? String.Empty : messageLogger.name;

            this.tunnelThreadA = new SocketTunnelThread(this, connectionMessageLogger.AToBMessageLogger,
                connectionDataLogger.AToBDataLogger, socketA, socketB, readBufferSize);
            this.tunnelThreadB = new SocketTunnelThread(this, connectionMessageLogger.BToAMessageLogger,
                connectionDataLogger.BToADataLogger, socketB, socketA, readBufferSize);
            this.tunnelClosed = false;
        }

        public void Start()
        {
            tunnelThreadA.Start();
            tunnelThreadB.Start();
        }

        private void ThreadClosed(SocketTunnelThread tunnelThread)
        {
            tunnelThreadA.ExpectClose();
            tunnelThreadB.ExpectClose();

            if (!tunnelClosed)
            {
                lock (this)
                {
                    if (tunnelClosed)
                    {
                        return;
                    }
                    else
                    {
                        tunnelClosed = true;
                    }
                }

                try
                {

                    if (socketA.Connected) try
                        {
                            connectionMessageLogger.LogAToB("socketA.Shutdown");
                            socketA.Shutdown(SocketShutdown.Both);
                        }
                        catch (SocketException se) { connectionMessageLogger.Log("SocketException in socketA.Shutdown: {0}", se.Message); }
                        catch (ObjectDisposedException ode) { connectionMessageLogger.Log("ObjectDisposedException in socketA.Shutdown: {0}", ode.Message); }

                    if (socketB.Connected) try
                        {
                            connectionMessageLogger.LogBToA("socketB.Shutdown");
                            socketB.Shutdown(SocketShutdown.Both);
                        }
                        catch (SocketException se) { connectionMessageLogger.Log("SocketException in socketB.Shutdown: {0}", se.Message); }
                        catch (ObjectDisposedException ode) { connectionMessageLogger.Log("ObjectDisposedException in socketB.Shutdown: {0}", ode.Message); }
                }
                finally
                {
                    socketA.Close();
                    socketB.Close();
                }
            }
        }



    }
    */



















    /*
    private class SocketTunnelThread
    {
        private readonly SocketTunnel socketTunnel;

        private readonly MessageLogger messageLogger;
        private readonly IDataLogger dataLogger;

        private readonly Socket inputSocket, outputSocket;
        public readonly Int32 readBufferSize;

        private Thread thread;
        private Boolean keepRunning;

        public SocketTunnelThread(SocketTunnel socketTunnel, MessageLogger messageLogger, IDataLogger dataLogger,
            Socket inputSocket, Socket outputSocket, Int32 readBufferSize)
        {
            this.socketTunnel = socketTunnel;
            this.messageLogger = messageLogger;
            this.dataLogger = dataLogger;

            this.inputSocket = inputSocket;
            this.outputSocket = outputSocket;
            this.readBufferSize = readBufferSize;

            this.thread = null;
            this.keepRunning = false;
        }

        public void Start()
        {
            if (thread != null) throw new InvalidOperationException("Already Started");

            thread = new Thread(Run);

            keepRunning = true;
            thread.Start();
        }

        public void ExpectClose()
        {
            this.keepRunning = false;
        }

        private void Run()
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
                Boolean keepRunningCache = keepRunning;

                this.keepRunning = false;
                thread = null;

                if (keepRunningCache) socketTunnel.ThreadClosed(this);
            }
        }
    }


    public class SocketTunnel
    {
        public const Int32 DefaultBufferSize = 1024;

        private readonly ConnectionMessageLogger connectionMessageLogger;
        private readonly IConnectionDataLogger connectionDataLogger;
        private readonly Socket socketA, socketB;

        //public readonly String messageLoggerName;
        private readonly SocketTunnelThread tunnelThreadA, tunnelThreadB;

        private Boolean tunnelClosed;

        public SocketTunnel(ConnectionMessageLogger connectionMessageLogger, IConnectionDataLogger connectionDataLogger,
            Socket socketA, Socket socketB, Int32 readBufferSize)
        {
            if (connectionMessageLogger == null) throw new ArgumentNullException("connectionMessageLogger");
            if (connectionDataLogger == null) throw new ArgumentNullException("connectionDataLogger");

            if (socketA == null) throw new ArgumentNullException("socketA");
            if (socketB == null) throw new ArgumentNullException("socketB");

            this.connectionMessageLogger = connectionMessageLogger;
            this.connectionDataLogger = connectionDataLogger;

            this.socketA = socketA;
            this.socketB = socketB;

            //this.messageLoggerName = (connectionMessageLogger. == null) ? String.Empty : messageLogger.name;

            this.tunnelThreadA = new SocketTunnelThread(this, connectionMessageLogger.AToBMessageLogger,
                connectionDataLogger.AToBDataLogger, socketA, socketB, readBufferSize);
            this.tunnelThreadB = new SocketTunnelThread(this, connectionMessageLogger.BToAMessageLogger,
                connectionDataLogger.BToADataLogger, socketB, socketA, readBufferSize);
            this.tunnelClosed = false;
        }

        public void Start()
        {
            tunnelThreadA.Start();
            tunnelThreadB.Start();
        }

        private void ThreadClosed(SocketTunnelThread tunnelThread)
        {
            tunnelThreadA.ExpectClose();
            tunnelThreadB.ExpectClose();

            if(!tunnelClosed)
            {
                lock (this)
                {
                    if (tunnelClosed)
                    {
                        return;
                    }
                    else
                    {
                        tunnelClosed = true;
                    }
                }

                try
                {

                    if (socketA.Connected) try {
                        connectionMessageLogger.LogAToB("socketA.Shutdown");
                        socketA.Shutdown(SocketShutdown.Both);
                    }
                        catch (SocketException se) { connectionMessageLogger.Log("SocketException in socketA.Shutdown: {0}", se.Message); }
                        catch (ObjectDisposedException ode) { connectionMessageLogger.Log("ObjectDisposedException in socketA.Shutdown: {0}", ode.Message); }

                    if (socketB.Connected) try {
                        connectionMessageLogger.LogBToA("socketB.Shutdown");
                        socketB.Shutdown(SocketShutdown.Both);
                    }
                        catch (SocketException se) { connectionMessageLogger.Log("SocketException in socketB.Shutdown: {0}", se.Message); }
                        catch (ObjectDisposedException ode) { connectionMessageLogger.Log("ObjectDisposedException in socketB.Shutdown: {0}", ode.Message); }                    
                }
                finally
                {
                    socketA.Close();
                    socketB.Close();
                }
            }
        }



    }
    */
}