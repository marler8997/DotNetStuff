using System;
using System.Net;
using System.Collections.Generic;
using System.IO;

namespace Marler.Net
{
    class Program
    {
        static void Usage()
        {
            Console.WriteLine("Usage: NfsServer.exe <share-directory> <share-name> <listen-ip>");
            Console.WriteLine("    <listen-ip>  (Use '0.0.0.0' for any ip)");
        }
        static void Main(String[] args)
        {
            if (args.Length != 3)
            {
                Usage();
                return;
            }


            String shareDirectory = args[0];
            String shareName = args[1];
            String listenIPString = args[2];


            //
            // Options
            //
            Int32 mountListenPort = 59733;
            Int32 backlog = 4;

            UInt32 readSizeMax = 65536;
            UInt32 suggestedReadSizeMultiple = 4096;

            //
            // Listen IP Address
            //
            IPAddress listenIPAddress = IPAddress.Parse(listenIPString);

            //
            // Control Server
            //
            Int32 controlServerPort = 1234;
            IPEndPoint controlServerEndPoint = new IPEndPoint(listenIPAddress, controlServerPort);//null;

            //
            // Logging Options
            //
            NfsServerLog.storePerformance                   = true;
            NfsServerLog.sharedFileSystemLogger             = Console.Out;
            NfsServerLog.rpcCallLogger                      = Console.Out;
            NfsServerLog.warningLogger                      = Console.Out;

            RpcPerformanceLog.rpcMessageSerializationLogger = Console.Out;

            TextWriter selectServerEventLog                 = null; //Console.Out;

            //
            // Permissions
            //
            Nfs3Procedure.ModeFlags defaultDirectoryPermissions =
                Nfs3Procedure.ModeFlags.OtherExecute | Nfs3Procedure.ModeFlags.OtherWrite | Nfs3Procedure.ModeFlags.OtherRead |
                Nfs3Procedure.ModeFlags.GroupExecute | Nfs3Procedure.ModeFlags.GroupWrite | Nfs3Procedure.ModeFlags.GroupRead |
                Nfs3Procedure.ModeFlags.OwnerExecute | Nfs3Procedure.ModeFlags.OwnerWrite | Nfs3Procedure.ModeFlags.OwnerRead;
            /*Nfs3Procedure.ModeFlags.SaveSwappedText | Nfs3Procedure.ModeFlags.SetUidOnExec | Nfs3Procedure.ModeFlags.SetGidOnExec;*/
            Nfs3Procedure.ModeFlags defaultFilePermissions =
                Nfs3Procedure.ModeFlags.OtherExecute | Nfs3Procedure.ModeFlags.OtherWrite | Nfs3Procedure.ModeFlags.OtherRead |
                Nfs3Procedure.ModeFlags.GroupExecute | Nfs3Procedure.ModeFlags.GroupWrite | Nfs3Procedure.ModeFlags.GroupRead |
                Nfs3Procedure.ModeFlags.OwnerExecute | Nfs3Procedure.ModeFlags.OwnerWrite | Nfs3Procedure.ModeFlags.OwnerRead;
            /*Nfs3Procedure.ModeFlags.SaveSwappedText | Nfs3Procedure.ModeFlags.SetUidOnExec | Nfs3Procedure.ModeFlags.SetGidOnExec;*/
            IPermissions permissions = new DumbPermissions(defaultDirectoryPermissions, defaultFilePermissions);

            ShareDirectory[] shareDirectories = new ShareDirectory[] {
                new ShareDirectory(shareDirectory, shareName),
            };

            IFileIDsAndHandlesDictionary fileIDDictionary = new FreeStackFileIDDictionary(512, 512, 4096, 1024);

            SharedFileSystem sharedFileSystem = new SharedFileSystem(fileIDDictionary, permissions, shareDirectories);

            try
            {
                new RpcServicesManager().Run(
                    selectServerEventLog,
                    controlServerEndPoint,
                    listenIPAddress,
                    backlog, sharedFileSystem,
                    Ports.PortMap, mountListenPort, Ports.Nfs,
                    readSizeMax, suggestedReadSizeMultiple);
            }
            finally
            {
                if (NfsServerLog.storePerformance == true) NfsServerLog.PrintNfsCalls(Console.Out);
            }
        }
    }
}


