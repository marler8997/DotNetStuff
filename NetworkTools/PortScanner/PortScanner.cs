using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Marler.NetworkTools
{
    public class PortScanner
    {
        public readonly IPAddress hostIP;

        private readonly UInt16 absoluteMinPort, absoluteMaxPort;
        private readonly UInt16 maxThreads;

        private readonly TimeSpan timeout;
        private readonly UInt32 sleepTime;

        public PortScanner(IPAddress hostIP, UInt16 minPort, UInt16 maxPort, UInt16 maxThreads, TimeSpan timeout, UInt32 sleepTime)
        {
            if (minPort <= 0)
            {
                throw new ArgumentOutOfRangeException("minPort");
            }
            if (maxPort < minPort)
            {
                throw new ArgumentOutOfRangeException("maxPort",
                    String.Format("maxPort ({0}) must be >= to minPort ({1})", maxPort, minPort));
            }
            if (maxThreads <= 0) throw new ArgumentOutOfRangeException("maxRange");

            this.hostIP = hostIP;
            this.absoluteMinPort = minPort;
            this.absoluteMaxPort = maxPort;
            this.maxThreads = maxThreads;
            this.timeout = timeout;
            this.sleepTime = sleepTime;
        }


        public void Scan()
        {
            Console.WriteLine("Scanning from port {0} to {1}, maxThreads = {2}, timeout = {3}",
                absoluteMinPort, absoluteMaxPort, maxThreads, timeout);

            PortScannerManager portScannerManager =
                new PortScannerManager(hostIP, absoluteMinPort, absoluteMaxPort, maxThreads, timeout, sleepTime);
            portScannerManager.Scan();
            portScannerManager.PrintStatus();
        }

        public class PortScannerManager
        {
            public readonly IPAddress hostIP;
            public readonly UInt16 minPort, maxPort, maxThreads;
            public readonly TimeSpan timeout;
            public readonly UInt32 sleepTime;

            public readonly ResourceTracker threadTracker;
            private readonly Boolean[] portStatus;            
            
            private UInt16 scannersNotComplete;
            private EventWaitHandle scannersDoneWaitHandle;

            public PortScannerManager(IPAddress hostIP, UInt16 minPort, UInt16 maxPort,
                UInt16 maxThreads, TimeSpan timeout, UInt32 sleepTime)
            {
                this.hostIP = hostIP;
                this.minPort = minPort;
                this.maxPort = maxPort;
                this.maxThreads = maxThreads;
                this.timeout = timeout;
                this.sleepTime = sleepTime;

                this.threadTracker = new ResourceTracker(maxThreads);

                this.portStatus = new Boolean[maxPort - minPort + 1];
            }

            public void ScannerComplete(UInt16 port, Boolean connected)
            {
                this.portStatus[port - minPort] = connected;
                if (connected)
                {
                    Console.WriteLine("Port {0} is open", port);
                }
                lock (threadTracker)
                {
                    scannersNotComplete--;
                    if (scannersNotComplete <= 0) scannersDoneWaitHandle.Set();
                }
                this.threadTracker.Free();
            }

            public void Scan()
            {
                this.scannersNotComplete = (UInt16)(maxPort - minPort + 1);
                this.scannersDoneWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

                for (UInt16 port = minPort; port <= maxPort; port++)
                {
                    if (port <= 0) break; // use this if maxPort == 65535

                    Boolean threadsNotAvailable = !threadTracker.Available;
                    if (threadsNotAvailable)
                    {
                        Console.WriteLine("Reached maximum threads ({0}) at port {1}", maxThreads, port);
                        if (sleepTime > 0)
                        {
                            Console.WriteLine("Sleeping for {0} milliseconds...", sleepTime);
                            Thread.Sleep((Int32)sleepTime);
                        }
                    }
                    threadTracker.Reserve();
                    if (threadsNotAvailable)
                    {
                        Console.WriteLine("Continuing Thread Creation", port);
                    }
                    new Thread(new PortConnectionThread(this, port).Run).Start();
                }

                Console.WriteLine("Waiting for remaining threads to finish...");
                this.scannersDoneWaitHandle.WaitOne();
            }

            public void PrintStatus()
            {
                List<Int32> openPorts = new List<int>();
                for (int i = 0; i < portStatus.Length; i++)
                {
                    if (portStatus[i])
                    {
                        openPorts.Add(minPort + i);
                    }
                }

                Console.WriteLine("==========================================================");
                Console.WriteLine("Summary");
                Console.WriteLine("==========================================================");
                Console.WriteLine("There are {0} open ports between {1} and {2}",
                    openPorts.Count, minPort, minPort + portStatus.Length - 1);
                if (openPorts.Count > 0)
                {
                    Console.Write("Open Ports:");
                    for (int i = 0; i < openPorts.Count; i++)
                    {
                        Console.Write(" {0}", openPorts[i]);
                    }
                    Console.WriteLine();
                }
                Console.WriteLine("==========================================================");
            }

        }

        public class PortConnectionThread
        {
            private readonly PortScannerManager portScannerManager;
            private readonly UInt16 port;

            public PortConnectionThread(PortScannerManager portScannerManager, UInt16 port)
            {
                this.portScannerManager = portScannerManager;
                this.port = port;
            }

            public void Run()
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Boolean connected = false;
                try
                {
                    if (portScannerManager.timeout == TimeSpan.Zero)
                    {
                        socket.Connect(portScannerManager.hostIP, port);
                        connected = true;
                    }
                    else
                    {
                        if (socket.ConnectWithTimeout(new IPEndPoint(portScannerManager.hostIP, port), portScannerManager.timeout))
                        {
                            connected = true;
                        }
                    }
                }
                catch (SocketException se)
                {
                }
                finally
                {
                    if (socket != null)
                    {
                        if (socket.Connected) try { socket.Shutdown(SocketShutdown.Both); }
                            catch (SocketException) { }
                            catch (ObjectDisposedException) { };
                        socket.Close();
                    }
                    portScannerManager.ScannerComplete(port, connected);
                }
            }
        }

    }

}
