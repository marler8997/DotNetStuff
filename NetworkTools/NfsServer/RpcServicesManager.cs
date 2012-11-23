using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace Marler.NetworkTools
{
    public class RpcServicesManager
    {
        Int64 serverStartTimeStopwatchTicks;
        MultipleListenersSelectServer selectServer;

        public RpcServicesManager()
        {
        }

        public void Run(TextWriter selectServerEventLog, IPEndPoint controlServerEndPoint, IPAddress listenIPAddress, Int32 backlog, SharedFileSystem sharedFileSystem,
            Int32 portmapPort, Int32 mountPort, Int32 nfsPort, UInt32 readSizeMax, UInt32 suggestedReadSizeMultiple)
        {
            ByteBuffer sendBuffer = new ByteBuffer(4096, 1024);

            //
            // Start Servers
            //
            PortMap2Server portMapServer = new PortMap2Server(this, sendBuffer, mountPort, nfsPort);
            Mount1And3Server mountServer = new Mount1And3Server(this, sharedFileSystem, sendBuffer);
            Nfs3Server nfsServer = new Nfs3Server(this, sharedFileSystem, sendBuffer, readSizeMax, suggestedReadSizeMultiple);

            if (listenIPAddress == null) listenIPAddress = IPAddress.Any;
            IPEndPoint portMapEndPoint = new IPEndPoint(listenIPAddress, portmapPort);

            selectServer = new MultipleListenersSelectServer();
            selectServer.PrepareToRun();

            this.serverStartTimeStopwatchTicks = Stopwatch.GetTimestamp();


            List<TcpSelectListener> tcpListeners = new List<TcpSelectListener>();
            tcpListeners.Add(new TcpSelectListener(portMapEndPoint                         , backlog, portMapServer));
            tcpListeners.Add(new TcpSelectListener(new IPEndPoint(listenIPAddress, mountPort), backlog, mountServer));
            tcpListeners.Add(new TcpSelectListener(new IPEndPoint(listenIPAddress, nfsPort)  , backlog, nfsServer));

            if(controlServerEndPoint != null)
            {
                tcpListeners.Add(new TcpSelectListener(controlServerEndPoint, 0, new ControlServer()));
            }

            selectServer.Run(selectServerEventLog, new byte[1024], tcpListeners.ToArray(),
                new UdpSelectListener[]{
                    new UdpSelectListener(portMapEndPoint, portMapServer)
                }
            );
        }

        public void PrintPerformance()
        {
            Int64 totalStopwatchTicks = Stopwatch.GetTimestamp() - serverStartTimeStopwatchTicks;

            /*
            if (RpcPerformanceLog.rpcCallTimeLogger != null)
            {
                RpcPerformanceLog.PrintPerformance(totalTimeMilliseconds);
            }
            */
            /*
            if (selectServer.totalSelectBlockTimeMicroseconds > 0)
            {
                UInt64 totalSelectBlockMilliseconds = selectServer.totalSelectBlockTimeMicroseconds / 1000;
                Console.WriteLine("[Performance] SelectBlockPercentage {0:0.00}% TotalSelectBlockTime {1} milliseconds TotalTime {2} milliseconds",
                (Double)totalSelectBlockMilliseconds / (Double)totalTimeMilliseconds, totalSelectBlockMilliseconds, totalTimeMilliseconds);
                
            }
            */
        }
    }

}
