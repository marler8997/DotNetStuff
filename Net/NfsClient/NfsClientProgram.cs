using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

using More;
using More.Net.Nfs3Procedure;

namespace More.Net
{
    class Program
    {
        static void Usage()
        {
            Console.WriteLine("Usage: NfsClient.exe host directory");
        }

        static void AssertOK(Nfs3Procedure.Status status)
        {
            if (status != Nfs3Procedure.Status.Ok) throw new InvalidOperationException(String.Format("Nfs Command Failed (status={0})", status));
        }


        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Error: Expected 2 command line arguments but got {0}", args.Length);
                return;
            }

            //
            // Options
            //
            Boolean forceMountToUsePrivelegedSourcePort = false;
            Boolean forceNfsToUsePrivelegedSourcePort = false;

            String serverHost = args[0];
            String remoteMountDirectory = args[1];

            //
            //
            //
            RpcTcpClientConnection portmapConnection = null;
            RpcTcpClientConnection mountConnection = null;
            RpcTcpClientConnection nfsConnection = null;
    
            DateTime epoch = new DateTime(1970, 1, 1);
            UInt32 stamp = (UInt32)DateTime.Now.Subtract(epoch).TotalSeconds;

            RpcCredentials portmapCredentials = RpcCredentials.None;
            RpcCredentials mountCredentials = RpcCredentials.CreateUnixCredentials(new RpcUnixCredentials(stamp, //0x509d4ff4U,
                "hplx0274.boi.hp.com", 0, 0, new UInt32[] { 0, 1, 1, 2, 3, 4, 6, 10, 101, 10000, 10070, 10091, 39747 }));
            RpcCredentials nfsCredentials = mountCredentials;

            try
            {
                StringBuilder builder = new StringBuilder();
                Buf buffer = new Buf(1024, 1024);

                //
                // Make connection to portmap service
                //
                portmapConnection = new RpcTcpClientConnection(new TcpSocket(AddressFamily.InterNetwork),
                    PortMap2.ProgramHeader, portmapCredentials, RpcVerifier.None);
                //IPEndPoint portmapEndPoint = new IPEndPoint(serverHost, 111);
                portmapConnection.socket.Connect(serverHost, 111);


                //
                // Dump
                //
                PortMap2Procedure.DumpReply dumpReply = new PortMap2Procedure.DumpReply();
                ISerializer dumpReplySerializer = dumpReply.CreateSerializer();
                portmapConnection.CallBlockingTcp(PortMap2.DUMP, VoidSerializer.Instance,
                    dumpReply.CreateSerializer(), buffer);

                builder.Length = 0;
                dumpReplySerializer.DataString(builder);
                Console.WriteLine(builder.ToString());

                //
                // Get Nfs Port
                //
                Mapping getNfsPortMapping = new Mapping(
                        Nfs3.ProgramHeader.program,
                        Nfs3.ProgramHeader.programVersion,
                        PortMap.IPProtocolTcp,
                        0                   // Port 0
                        );
                GetPortReply getNfsPortReply = portmapConnection.CallBlockingTcp(PortMap2.GETPORT,
                    getNfsPortMapping.CreateSerializerAdapater(), GetPortReply.Serializer, buffer);

                Console.WriteLine("Nfs Port: {0}", getNfsPortReply.port);

                //
                // Connect to NFS Service
                //
                nfsConnection = new RpcTcpClientConnection(new TcpSocket(AddressFamily.InterNetwork),
                    Nfs3.ProgramHeader, nfsCredentials, RpcVerifier.None);
                if (forceNfsToUsePrivelegedSourcePort)
                {
                    nfsConnection.BindToPrivelegedPort();
                }
                nfsConnection.socket.Connect(serverHost, (Int32)getNfsPortReply.port);
        
                //
                // Test NFS Service
                //
                nfsConnection.CallBlockingTcp((UInt32)Nfs3Command.NULL, VoidSerializer.Instance, VoidSerializer.Instance, buffer);

                //
                // Get Mount Port
                //
                Mapping getMountPort = new Mapping(
                        Mount3.ProgramHeader.program,
                        Mount3.ProgramHeader.programVersion,
                        PortMap.IPProtocolTcp,
                        0                   // Port 0
                        );
                GetPortReply getMountPortReply = portmapConnection.CallBlockingTcp(PortMap2.GETPORT,
                    getMountPort.CreateSerializerAdapater(), GetPortReply.Serializer, buffer);

                Console.WriteLine("Mount Port: {0}", getMountPortReply.port);

                //
                // Connect to Mount Service
                //
                mountConnection = new RpcTcpClientConnection(new TcpSocket(AddressFamily.InterNetwork),
                    Mount3.ProgramHeader, mountCredentials, RpcVerifier.None);
                if (forceMountToUsePrivelegedSourcePort)
                {
                    mountConnection.BindToPrivelegedPort();
                }
                mountConnection.socket.Connect(serverHost, (Int32)getMountPortReply.port);

                //
                // Test Mount Service
                //
                mountConnection.CallBlockingTcp<Object>(Mount.NULL, VoidSerializer.Instance, VoidInstanceSerializer.Instance, buffer);

                //
                // Mount the remote direcory
                //
                Mount3Reply mountReply = new Mount3Reply(Status.ErrorInvalidArgument);
                ISerializer mountReplySerializer = mountReply.CreateSerializer();
                MountCall mountCall = new MountCall(remoteMountDirectory);
                mountConnection.CallBlockingTcp(Mount.MNT, mountCall.CreateSerializer(),
                    mountReplySerializer, buffer);

                Console.WriteLine();
                Console.WriteLine(DataStringBuilder.DataString(mountReplySerializer, builder));
                AssertOK(mountReply.status);

                Byte[] rootDirectoryHandle = mountReply.fileHandle;

                ReadDirPlusCall readDirPlusCall = new ReadDirPlusCall(rootDirectoryHandle, 0, null, 512, UInt32.MaxValue);
                ReadDirPlusReply readDirPlusReply = new ReadDirPlusReply(Status.ErrorInvalidArgument, null);
                nfsConnection.CallBlockingTcp((UInt32)Nfs3Command.READDIRPLUS, readDirPlusCall.CreateSerializer(),
                    readDirPlusReply.CreateSerializer(), buffer);
                for (int i = 0; i < readDirPlusReply.entries.Length; i++)
                {
                    EntryPlus entry = readDirPlusReply.entries[i];
                    Console.WriteLine("Entry[{0}] = {1}", i, entry.fileName);
                }

                //Console.WriteLine();
                //Console.WriteLine(ISerializerString.DataString(readDirPlus.reply.CreateSerializer()));
                
                /*
                
                //
                // Get FileSystemInfo
                //
                Nfs3Procedure.FSInfoCall fileSystemInfo = new Nfs3Procedure.FSInfoCall(rootDirectoryHandle);
                nfsConnection.CallBlockingTcp(fileSystemInfo.CreateSerializer(), buffer);
                Console.WriteLine();
                //Console.WriteLine(fileSystemInfo.reply.ToNiceString());

                //AssertOK(fileSystemInfo.reply.status);

                //
                // Get File Attributes
                //
                Nfs3Procedure.GetFileAttributes getFileAttributes = new Nfs3Procedure.GetFileAttributes(new Nfs3Procedure.GetFileAttributesCall(
                    rootDirectoryHandle
                    ));
                nfsConnection.CallBlockingTcp(getFileAttributes, buffer);
                Console.WriteLine();
                Console.WriteLine(getFileAttributes.reply.ToNiceString());

                AssertOK(getFileAttributes.reply.status);

                //
                // Access the file
                //
                Nfs3Procedure.Access access = new Nfs3Procedure.Access(new Nfs3Procedure.AccessCall(
                    rootDirectoryHandle,
                    Nfs3Procedure.AccessFlags.Delete | Nfs3Procedure.AccessFlags.Execute | Nfs3Procedure.AccessFlags.Extend |
                    Nfs3Procedure.AccessFlags.Lookup | Nfs3Procedure.AccessFlags.Modify | Nfs3Procedure.AccessFlags.Read
                ));
                nfsConnection.CallBlockingTcp(access, buffer);
                Console.WriteLine();
                Console.WriteLine(access.reply.ToNiceString());

                AssertOK(access.reply.status);

                //
                //
                //

                Nfs3Procedure.ReadDirPlus readDirPlus = new Nfs3Procedure.ReadDirPlus(new Nfs3Procedure.ReadDirPlusCall(
                    rootDirectoryHandle, 0, null, Int32.MaxValue, Int32.MaxValue));
                nfsConnection.CallBlockingTcp(readDirPlus, buffer);

                AssertOK(readDirPlus.reply.status);

                if (readDirPlus.reply.entriesIncluded)
                {
                    Nfs3Procedure.EntryPlus entryPlus = readDirPlus.reply.entry;

                    while(true)
                    {
                        Console.WriteLine();
                        Console.WriteLine("ID '{0}' Name '{1}' Cookie '{2}'", entryPlus.fileID, entryPlus.fileName,
                            entryPlus.cookie);

                        Nfs3Procedure.Lookup lookup = new Nfs3Procedure.Lookup(new Nfs3Procedure.LookupCall(
                            rootDirectoryHandle, entryPlus.fileName));
                        nfsConnection.CallBlockingTcp(lookup, buffer);

                        AssertOK(lookup.reply.status);

                        Console.WriteLine("   FileHandle: {0}", BitConverter.ToString(lookup.reply.fileHandle));

                        Nfs3Procedure.Read read = new Nfs3Procedure.Read(new Nfs3Procedure.ReadCall(
                            lookup.reply.fileHandle, 0, fileSystemInfo.reply.readSizeMax));
                        nfsConnection.CallBlockingTcp(read, buffer);

                        if(read.reply.status == Nfs3Procedure.Status.Ok)
                        {
                            if(read.reply.count <= 0)
                            {
                                Console.WriteLine("   FileLength: {0}", read.reply.count);
                            }
                            else
                            {
                                Console.WriteLine("   FileLength: {0} FirstPart: '{1}'", read.reply.count,
                                    Encoding.UTF8.GetString(read.reply.fileData.bytes, 0, (read.reply.count > 10U)? 10 : (Int32)read.reply.count));
                            }
                        }

                        if(!entryPlus.nextEntryIncluded) break;
                        entryPlus = entryPlus.nextEntry;
                    }
                }


                //
                //
                //

                String temporaryTestDirectory = "TemporaryTestDir";
                String temporaryRenameDirectory = "TemporaryTestDirRenamed";
                String temporaryTestFileName = "TemporaryTestFile";

                //
                // check if test directory exists and remove it if it does
                //
                Lookup lookupTestDirectory = new Lookup(new LookupCall(rootDirectoryHandle, temporaryTestDirectory));
                nfsConnection.CallBlockingTcp(lookupTestDirectory, buffer);

                if (lookupTestDirectory.reply.status == Status.Ok)
                {
                    Rmdir quickFixRmdir = new Rmdir(new RemoveCall(rootDirectoryHandle, temporaryTestDirectory));
                    nfsConnection.CallBlockingTcp(quickFixRmdir, buffer);

                    AssertOK(quickFixRmdir.reply.status);
                }
                else
                {
                    if (lookupTestDirectory.reply.status != Status.ErrorNoSuchFileOrDirectory)
                        throw new InvalidOperationException(String.Format("Expected OK or NoSuchFileOrDirectory but got '{0}'", lookupTestDirectory.reply.status));
                }




                //
                // Test MkDir
                //
                Mkdir mkdir = new Mkdir(new MkdirCall(rootDirectoryHandle, temporaryTestDirectory, new SetAttributesStruct(
                    false, 0, false, 0, false, 0, false, 0, null, null)));
                nfsConnection.CallBlockingTcp(mkdir, buffer);
                Console.WriteLine();
                Console.WriteLine(mkdir.reply.ToNiceString());

                AssertOK(mkdir.reply.status);
                if (mkdir.reply.optionalFileHandle.fileHandleIncluded == false) throw new Exception("no file handle returned from mkdir");

                Byte[] newDirectoryHandle = mkdir.reply.optionalFileHandle.fileHandle;


                //
                // Test Create
                //

                Create create = new Create(new CreateCall(newDirectoryHandle, temporaryTestFileName, CreateModeEnum.Unchecked,
                    new SetAttributesStruct(
                        false, 0,
                        false, 0,
                        false, 0,
                        false, 0,
                        null,
                        null)));
                nfsConnection.CallBlockingTcp(create, buffer);
                Console.WriteLine();
                Console.WriteLine(create.reply.ToNiceString());

                AssertOK(create.reply.status);
                if (create.reply.optionalFileHandle.fileHandleIncluded == false) throw new Exception("no file handle returned from Create");

                Byte[] newFileHandle = create.reply.optionalFileHandle.fileHandle;

                //
                // Test Write
                //
                Byte[] testWriteData = Encoding.UTF8.GetBytes("test data\r\nthis is a test write over nfs\r\n");
                Write write = new Write(new WriteCall(newFileHandle, StableHowEnum.Unstable,
                    testWriteData, 0, (UInt32)testWriteData.Length));
                nfsConnection.CallBlockingTcp(write, buffer);

                AssertOK(write.reply.status);

                //
                // Test SetAttr
                //
                SetFileAttributes setAttributes = new SetFileAttributes(new SetFileAttributesCall(
                    newFileHandle,
                    new SetAttributesStruct(false, 0, false, 0, false, 0, false, 0, null, null),
                    null));
                nfsConnection.CallBlockingTcp(setAttributes, buffer);
                Console.WriteLine();
                Console.WriteLine(setAttributes.reply.ToNiceString());

                AssertOK(setAttributes.reply.status);

                //
                // Test Remove
                //
                Remove remove = new Remove(new RemoveCall(newDirectoryHandle, temporaryTestFileName));
                nfsConnection.CallBlockingTcp(remove, buffer);

                Console.WriteLine();
                Console.WriteLine(remove.reply.ToNiceString());

                AssertOK(remove.reply.status);

                // remove parent directory
                remove = new Remove(new RemoveCall(rootDirectoryHandle, temporaryTestDirectory));
                nfsConnection.CallBlockingTcp(remove, buffer);

                Console.WriteLine();
                Console.WriteLine(remove.reply.ToNiceString());

                AssertOK(remove.reply.status);

                // make parent directory again
                nfsConnection.CallBlockingTcp(mkdir, buffer);

                Console.WriteLine();
                Console.WriteLine(mkdir.reply.ToNiceString());

                AssertOK(mkdir.reply.status);
                if (mkdir.reply.optionalFileHandle.fileHandleIncluded == false) throw new Exception("no file handle returned from mkdir");
                newDirectoryHandle = mkdir.reply.optionalFileHandle.fileHandle;


                Rename rename = new Rename(new RenameCall(rootDirectoryHandle, temporaryTestDirectory,
                    rootDirectoryHandle, temporaryRenameDirectory));
                nfsConnection.CallBlockingTcp(rename, buffer);

                Console.WriteLine();
                Console.WriteLine(rename.reply.ToNiceString());

                AssertOK(rename.reply.status);


                // remove parent directory again using rmdir instead of remove
                Rmdir rmdir = new Rmdir(new RemoveCall(rootDirectoryHandle, temporaryRenameDirectory));
                nfsConnection.CallBlockingTcp(rmdir, buffer);

                Console.WriteLine();
                Console.WriteLine(rmdir.reply.ToNiceString());

                AssertOK(rmdir.reply.status);
                */
            }
            finally
            {
                if (portmapConnection != null)
                {
                    portmapConnection.Dispose();
                }
                if (mountConnection != null)
                {
                    mountConnection.Dispose();
                }
                if (nfsConnection != null)
                {
                    nfsConnection.Dispose();
                }
            }
        }
    }
}
