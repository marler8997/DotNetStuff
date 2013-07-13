using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Diagnostics;

using More;

namespace More.Net
{
    public class RpcServicesManager
    {
        Int64 serverStartTimeStopwatchTicks;
        MultipleListenersSelectServer selectServer;

        public RpcServicesManager()
        {
        }

        public void Run(TextWriter selectServerEventLog, IPEndPoint debugServerEndPoint, IPEndPoint npcServerEndPoint,
            IPAddress listenIPAddress, Int32 backlog, SharedFileSystem sharedFileSystem,
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

            if (debugServerEndPoint != null)
            {
                tcpListeners.Add(new TcpSelectListener(debugServerEndPoint, 0, new ControlServer()));
            }
            if (npcServerEndPoint != null)
            {
                Nfs3Server.NfsServerManager nfsServerManager = new Nfs3Server.NfsServerManager(nfsServer);
                NpcReflector reflector = new NpcReflector(
                    new NpcExecutionObject(nfsServerManager, "Nfs3ServerManager", null, null),
                    new NpcExecutionObject(nfsServer, "Nfs3Server", null, null)
                    );
                tcpListeners.Add(new TcpSelectListener(npcServerEndPoint, 32, new NpcStreamSelectServerCallback(
                    NpcCallback.Instance, reflector, new DefaultNpcHtmlGenerator("NfsServer", reflector))));
            }

            selectServer.Run(selectServerEventLog, new byte[1024], tcpListeners.ToArray(),
                new UdpSelectListener[]{
                    new UdpSelectListener(portMapEndPoint, portMapServer)
                }
            );
        }

        /*
        public void PrintPerformance()
        {
            Int64 totalStopwatchTicks = Stopwatch.GetTimestamp() - serverStartTimeStopwatchTicks;

            if (RpcPerformanceLog.rpcCallTimeLogger != null)
            {
                RpcPerformanceLog.PrintPerformance(totalTimeMilliseconds);
            }
            if (selectServer.totalSelectBlockTimeMicroseconds > 0)
            {
                UInt64 totalSelectBlockMilliseconds = selectServer.totalSelectBlockTimeMicroseconds / 1000;
                Console.WriteLine("[Performance] SelectBlockPercentage {0:0.00}% TotalSelectBlockTime {1} milliseconds TotalTime {2} milliseconds",
                (Double)totalSelectBlockMilliseconds / (Double)totalTimeMilliseconds, totalSelectBlockMilliseconds, totalTimeMilliseconds);
                
            }
        }
        */
    }
    class NpcCallback : INpcServerCallback
    {
        private static NpcCallback instance = null;
        public static NpcCallback Instance
        {
            get
            {
                if (instance == null) instance = new NpcCallback();
                return instance;
            }
        }
        private NpcCallback() { }

        public void ServerListening(Socket listenSocket)
        {
            if (NfsServerLog.npcEventsLogger != null)
            {
                NfsServerLog.npcEventsLogger.WriteLine("[Npc] Server is listening");
            }
        }
        public void FunctionCall(string clientString, string methodName)
        {
            if (NfsServerLog.npcEventsLogger != null)
            {
                NfsServerLog.npcEventsLogger.WriteLine("[Npc] Client '{0}': Function Call '{1}'",
                    clientString, methodName);
            }
        }
        public void FunctionCallThrewException(string clientString, string methodName, Exception e)
        {
            if (NfsServerLog.npcEventsLogger != null)
            {
                NfsServerLog.npcEventsLogger.WriteLine("[Npc] Client '{0}': Function Call '{1}' threw exception: {2}",
                    clientString, methodName, e);
            }
        }
        public void GotInvalidData(string clientString, string message)
        {
            if (NfsServerLog.npcEventsLogger != null)
            {
                NfsServerLog.npcEventsLogger.WriteLine("[Npc] Client '{0}': Got invalid data: {1}", clientString, message);
            }
        }
        public void ExceptionDuringExecution(string clientString, string methodName, Exception e)
        {
            if (NfsServerLog.npcEventsLogger != null)
            {
                NfsServerLog.npcEventsLogger.WriteLine("[Npc] Client '{0}': Exception: {1}", clientString, e);
            }
        }
        public void ExceptionWhileGeneratingHtml(string clientString, Exception e)
        {
            if (NfsServerLog.npcEventsLogger != null)
            {
                NfsServerLog.npcEventsLogger.WriteLine("[Npc] Client '{0}': Exception: {1}", clientString, e);
            }
        }
        public void UnhandledException(string clientString, Exception e)
        {
            if (NfsServerLog.npcEventsLogger != null)
            {
                NfsServerLog.npcEventsLogger.WriteLine("[Npc] Client '{0}': Exception: {1}", clientString, e);
            }
        }
    }
}
