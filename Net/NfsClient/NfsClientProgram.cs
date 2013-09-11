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
            Boolean forceMountToUsePrivelegedSourcePort = true;
            Boolean forceNfsToUsePrivelegedSourcePort = true;

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
                ByteBuffer buffer = new ByteBuffer(1024, 1024);

                //
                // Make connection to portmap service
                //
                portmapConnection = new RpcTcpClientConnection(new TcpSocket(AddressFamily.InterNetwork),
                    PortMap2.ProgramHeader, portmapCredentials, RpcVerifier.None);
                //IPEndPoint portmapEndPoint = new IPEndPoint(serverHost, 111);
                portmapConnection.socket.Connect(serverHost, 111);


                /*
                //
                // Get NFS Port
                //
                PortMap2Procedure.GetPortCall getNfsPort = new PortMap2Procedure.GetPortCall(
                        Nfs3.ProgramHeader.program,
                        Nfs3.ProgramHeader.programVersion,
                        6                 , // TCP
                        0                   // Port 0
                        );
                portmapConnection.CallBlockingTcp(getNfsPort, buffer);

                Console.WriteLine();
                Console.WriteLine(ISerializerString.DataString(getNfsPort.reply));


                //
                // Connect to NFS Service
                //
                nfsConnection = new RpcTcpClientConnection(new TcpSocket(AddressFamily.InterNetwork),
                    Nfs3.ProgramHeader, nfsCredentials, RpcVerifier.None);
                if (forceNfsToUsePrivelegedSourcePort)
                {
                    nfsConnection.BindToPrivelegedPort();
                }
                nfsConnection.socket.Connect(serverHost, (Int32)getNfsPort.reply.port);
        
                //
                // Test NFS Service
                //
                Nfs3Procedure.Null nullNfsCall = new Nfs3Procedure.Null();
                nfsConnection.CallBlockingTcp(nullNfsCall, buffer);

                //
                // Connect to Mount service
                //
                PortMap2Procedure.GetPort getMountPort = new PortMap2Procedure.GetPort(new PortMap2Procedure.GetPortCall(
                        Mount3.ProgramHeader.program       , // Mount
                        Mount3.ProgramHeader.programVersion,
                        6,                                   // Tcp
                        0                                    // Port 0
                        ));
                portmapConnection.CallBlockingTcp(getMountPort, buffer);

                Console.WriteLine();
                Console.WriteLine(ISerializerString.DataString(getMountPort.reply));

                //
                // Connect to Mount Service
                //
                mountConnection = new RpcTcpClientConnection(new TcpSocket(AddressFamily.InterNetwork),
                    Mount3.ProgramHeader, mountCredentials, RpcVerifier.None);
                if (forceMountToUsePrivelegedSourcePort)
                {
                    mountConnection.BindToPrivelegedPort();
                }
                mountConnection.socket.Connect(serverHost, (Int32)getMountPort.reply.port);

                //
                // Test Mount Service
                //
                Mount3Procedure.Null nullMountCall = new Mount3Procedure.Null();
                mountConnection.CallBlockingTcp(nullNfsCall, buffer);

                //
                // Mount the remote direcory
                //
                Mount3Procedure.Mount mount = new Mount3Procedure.Mount(new Mount3Procedure.MountCall(remoteMountDirectory));
                mountConnection.CallBlockingTcp(mount, buffer);

                Console.WriteLine();
                Console.WriteLine(ISerializerString.DataString(mount.reply));
                AssertOK(mount.reply.status);

                Byte[] rootDirectoryHandle = mount.reply.fileHandle;


                ReadDirPlus readDirPlus = new ReadDirPlus(new ReadDirPlusCall(rootDirectoryHandle, 0, null, 512, UInt32.MaxValue));
                nfsConnection.CallBlockingTcp(readDirPlus, buffer);

                Console.WriteLine();
                Console.WriteLine(ISerializerString.DataString(readDirPlus.reply.CreateSerializer()));
                



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
