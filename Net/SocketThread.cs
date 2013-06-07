using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net.Sockets;

namespace More.Net
{
#if !WindowsCE
    public delegate void SocketThreadClosedEvent(SocketThread streamThread);
    public class SocketThread
    {
        public const Int32 DefaultBufferSize = 1024;

        public readonly String messageLoggerName;
        private readonly SocketThreadClosedEvent closedEvent;
        private readonly IDataLogger dataLogger;
        private readonly MessageLogger messageLogger;
        private readonly Socket inputSocket, outputSocket;
        public readonly Int32 bufferSize;

        private Boolean keepRunning;
        private Thread thread;

        public SocketThread(SocketThreadClosedEvent closedEvent, IDataLogger dataLogger,
            MessageLogger messageLogger, Socket inputSocket, Socket outputSocket, Int32 bufferSize)
        {
            if (inputSocket == null) throw new ArgumentNullException("inputStream");
            if (outputSocket == null) throw new ArgumentNullException("outputSocket");

            this.messageLoggerName = (messageLogger == null) ? String.Empty : messageLogger.name;
            this.closedEvent = closedEvent;

            this.dataLogger = dataLogger;
            this.messageLogger = messageLogger;

            this.inputSocket = inputSocket;
            this.outputSocket = outputSocket;

            this.bufferSize = bufferSize;

            this.keepRunning = false;
            this.thread = null;
        }

        public void Start()
        {
            if (this.thread != null) throw new InvalidOperationException("Thread already started");

            this.thread = new Thread(Run);
            this.thread.Name = String.Format("{0} Thread", messageLogger.name);

            this.keepRunning = true;
            this.thread.Start();
        }

        public void Interrupt()
        {
            this.keepRunning = false;


            if (inputSocket.Connected) try { inputSocket.Shutdown(SocketShutdown.Both); }
                    catch (SocketException) { }
                    catch (ObjectDisposedException) { };
            inputSocket.Close();

            if (outputSocket.Connected) try { outputSocket.Shutdown(SocketShutdown.Both); }
                    catch (SocketException) { }
                    catch (ObjectDisposedException) { };
            outputSocket.Close();

            Thread threadCache = thread;
            thread = null;
            if (threadCache != null)
            {
                if (threadCache.IsAlive) threadCache.Interrupt();
            }
        }

        private void Run()
        {
            try
            {
                byte[] buffer = new byte[bufferSize];

                while (keepRunning)
                {
                    if(messageLogger != null) messageLogger.Log("Reading");

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
                thread = null;

                if (inputSocket.Connected) try { Console.WriteLine("{0}: closing input socket", messageLoggerName); inputSocket.Shutdown(SocketShutdown.Both); }
                    catch (SocketException) { }
                    catch (ObjectDisposedException) { };
                if (outputSocket.Connected) try { Console.WriteLine("{0}: closing output socket", messageLoggerName); outputSocket.Shutdown(SocketShutdown.Both); }
                    catch (SocketException) { }
                    catch (ObjectDisposedException) { };

                inputSocket.Close();
                outputSocket.Close();

                if (closedEvent != null) closedEvent(this);
            }
        }

    }
#endif
}