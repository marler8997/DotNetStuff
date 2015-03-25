using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

using More.Net;

namespace More
{
    public enum LogLevel
    {
        None,
        Warning,
        Info,
        All,
    }

    public class RemoteDataServerProgramOptions : CLParser
    {
        public CLGenericArgument<IPAddress> listenIPAddress;
        public CLGenericArgument<UInt16> npcListenPort;

        public CLStringArgument logFile;

        public CLEnumArgument<LogLevel> logLevel;

        public RemoteDataServerProgramOptions()
        {
            listenIPAddress = new CLGenericArgument<IPAddress>(IPAddress.Parse, 'l', "Listen IP Address");
            listenIPAddress.SetDefault(IPAddress.Parse("0.0.0.0"));
            Add(listenIPAddress);

            npcListenPort = new CLGenericArgument<UInt16>(UInt16.Parse, 'n', "NpcListenPort", "The TCP port that the NPC server will be listening to (If no port is specified, the NPC server will not be running)");
            Add(npcListenPort);

            logFile = new CLStringArgument('f', "LogFile", "Log file (logs to stdout if not specified)");
            Add(logFile);

            logLevel = new CLEnumArgument<LogLevel>('v', "LogLevel", "Level of statements to log");
            logLevel.SetDefault(LogLevel.None);
            Add(logLevel);
        }
        public override void PrintUsageHeader()
        {
            Console.WriteLine("Usage: RemoteDataServer.exe [options] (<local-share-path1> <remote-share-name1>)...");
        }
    }

    class Program
    {
        static void Main(String[] args)
        {
            //NfsServerLog.stopwatchTicksBase = Stopwatch.GetTimestamp();

            RemoteDataServerProgramOptions options = new RemoteDataServerProgramOptions();
            List<String> nonOptionArguments = options.Parse(args);

            if (nonOptionArguments.Count < 2)
            {
                options.ErrorAndUsage("Expected at least 2 non-option arguments but got '{0}'", nonOptionArguments.Count);
                return;
            }
            if (nonOptionArguments.Count % 2 == 1)
            {
                options.ErrorAndUsage("Expected an even number of non-option arguments but got {0}", nonOptionArguments.Count);
            }

            UniqueIndexObjectDictionary<ShareObject> shareObjects = new UniqueIndexObjectDictionary<ShareObject>(
                512, 512, 4096, 1024);

            RootShareDirectory[] rootShareDirectories = new RootShareDirectory[nonOptionArguments.Count / 2];
            for (int i = 0; i < rootShareDirectories.Length; i++)
            {
                String localPathAndName = nonOptionArguments[2 * i];
                String remoteShareName = nonOptionArguments[2 * i + 1];

                DirectoryShareObject directoryShareObject = new DirectoryShareObject(localPathAndName, remoteShareName, null);
                UInt32 id = shareObjects.Add(directoryShareObject);
                directoryShareObject.id = id;

                rootShareDirectories[i] = new RootShareDirectory(directoryShareObject);
            }

            //
            // Options not exposed via command line yet
            //
            Int32 mountListenPort = 59733;
            Int32 backlog = 4;

            UInt32 readSizeMax = 65536;
            UInt32 suggestedReadSizeMultiple = 4096;

            //
            // Listen IP Address
            //
            IPAddress listenIPAddress = options.listenIPAddress.ArgValue;

            //
            // Npc Server
            //
            IPEndPoint npcServerEndPoint = !options.npcListenPort.set ? null :
                new IPEndPoint(listenIPAddress, options.npcListenPort.ArgValue);

            //
            // Logging Options
            //
            /*
            TextWriter selectServerEventsLog = null;
            if (options.logLevel.ArgValue != LogLevel.None)
            {
                TextWriter logWriter;
                if (options.logFile.set)
                {
                    logWriter = new StreamWriter(new FileStream(options.logFile.ArgValue, FileMode.Create, FileAccess.Write, FileShare.Read));
                }
                else
                {
                    logWriter = Console.Out;
                }

                NfsServerLog.sharedFileSystemLogger = (options.logLevel.ArgValue >= LogLevel.Info) ? logWriter : null;
                NfsServerLog.rpcCallLogger = (options.logLevel.ArgValue >= LogLevel.Info) ? logWriter : null;
                NfsServerLog.warningLogger = (options.logLevel.ArgValue >= LogLevel.Warning) ? logWriter : null;
                NfsServerLog.npcEventsLogger = (options.logLevel.ArgValue >= LogLevel.Info) ? logWriter : null;

                RpcPerformanceLog.rpcMessageSerializationLogger = (options.logLevel.ArgValue >= LogLevel.Info) ? logWriter : null;

                selectServerEventsLog = (options.logLevel.ArgValue >= LogLevel.All) ? logWriter : null;
            }
            */

            //
            // Permissions
            //
            //ModeFlags defaultDirectoryPermissions =
            //    ModeFlags.OtherExecute | ModeFlags.OtherWrite | ModeFlags.OtherRead |
            //    ModeFlags.GroupExecute | ModeFlags.GroupWrite | ModeFlags.GroupRead |
            //    ModeFlags.OwnerExecute | ModeFlags.OwnerWrite | ModeFlags.OwnerRead;
            /*ModeFlags.SaveSwappedText | ModeFlags.SetUidOnExec | ModeFlags.SetGidOnExec;*/
            //ModeFlags defaultFilePermissions =
            //    ModeFlags.OtherExecute | ModeFlags.OtherWrite | ModeFlags.OtherRead |
            //    ModeFlags.GroupExecute | ModeFlags.GroupWrite | ModeFlags.GroupRead |
            //    ModeFlags.OwnerExecute | ModeFlags.OwnerWrite | ModeFlags.OwnerRead;
            /*ModeFlags.SaveSwappedText | ModeFlags.SetUidOnExec | ModeFlags.SetGidOnExec;*/
            //IPermissions permissions = new ConstantPermissions(defaultDirectoryPermissions, defaultFilePermissions);



            SharedFileSystem sharedFileSystem = new SharedFileSystem(shareObjects, /*permissions, */rootShareDirectories);

            Buf sendBuffer = new Buf(4096, 1024);

            RemoteDataServer remoteDataServer = new RemoteDataServer(sharedFileSystem, sendBuffer);

            //
            // Create Endpoints
            //
            if (listenIPAddress == null) listenIPAddress = IPAddress.Any;
            IPEndPoint remoteDataEndPoint = new IPEndPoint(listenIPAddress, RemoteData.DefaultPort);


            MultipleListenersSelectServer selectServer = new MultipleListenersSelectServer();

            //this.serverStartTimeStopwatchTicks = Stopwatch.GetTimestamp();

            List<TcpSelectListener> tcpListeners = new List<TcpSelectListener>();
            tcpListeners.Add(new TcpSelectListener(remoteDataEndPoint, backlog, remoteDataServer));

            /*
            if (npcServerEndPoint != null)
            {
#if !WindowsCE
                Nfs3Server.NfsServerManager nfsServerManager = new Nfs3Server.NfsServerManager(nfsServer);
                NpcReflector reflector = new NpcReflector(
                    new NpcExecutionObject(nfsServerManager, "Nfs3ServerManager", null, null),
                    new NpcExecutionObject(nfsServer, "Nfs3Server", null, null),
                    new NpcExecutionObject(portMapServer, "Portmap2Server", null, null),
                    new NpcExecutionObject(mountServer, "Mount1And3Server", null, null)
                    );
                tcpListeners.Add(new TcpSelectListener(npcServerEndPoint, 32, new NpcStreamSelectServerCallback(
                    NpcCallback.Instance, reflector, new DefaultNpcHtmlGenerator("NfsServer", reflector))));
#endif
            }
            */

            selectServer.PrepareToRun();
            selectServer.Run(null, new Byte[1024], tcpListeners.ToArray(),
                new UdpSelectListener[]{
                    new UdpSelectListener(remoteDataEndPoint, remoteDataServer),
                }
            );         
        }
    }
}


