using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Marler.NetworkTools
{
    public class StreamSocketTunnelCallback : ITunnelCallback
    {
        private readonly Object sync = new Object();
        private readonly ManualResetEvent resetEvent = new ManualResetEvent(false);

        private readonly Socket socket;
        private readonly TextReader reader;
        private readonly TextWriter writer;

        private Boolean closed;

        public StreamSocketTunnelCallback(Socket socket, TextReader reader, TextWriter writer)
        {
            this.socket = socket;
            this.reader = reader;
            this.writer = writer;

            this.closed = false;
        }

        public void BlockTillClosed()
        {
            if (!closed)
            {
                resetEvent.WaitOne();
            }
        }

        private void CloseEverything()
        {
            if (socket.Connected)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception) { }
            }
            /*
            try
            {
                Console.WriteLine("INFO: closing socket...");
                socket.Close();
            }
            catch (Exception e) { Console.WriteLine("INFO: Failed to close socket: {0}", e); }
            try
            {
                Console.WriteLine("INFO: closing reader...");
                reader.Dispose();
            }
            catch (Exception e) { Console.WriteLine("INFO: Failed to close reader: {0}", e); }
            try
            {
                Console.WriteLine("INFO: closing writer...");
                writer.Dispose();
            }
            catch (Exception e) { Console.WriteLine("INFO: Failed to close writer: {0}", e); }
            */
            this.closed = true;
            resetEvent.Set();
        }

        public void ClosedAfterRead()
        {
            lock (sync)
            {
                if (!closed)
                {
                    Console.WriteLine("Connection Closed");
                    CloseEverything();
                }
            }
        }

        public void CannotWrite(byte[] leftoverData, int index, int count)
        {
            lock (sync)
            {
                if (!closed)
                {
                    Console.WriteLine("Connection Closed");
                    CloseEverything();
                }
            }
        }

        public void Exception(Exception e)
        {
            lock (sync)
            {
                if (!closed)
                {
                    Console.WriteLine(e.ToString());
                    CloseEverything();
                }
            }
        }

        public void FinallyBlock()
        {
            lock (sync)
            {
                if (!closed)
                {
                    CloseEverything();
                }
            }
        }
    }
}
