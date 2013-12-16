using System;
using System.Collections.Generic;
using System.Net.Sockets;

using More.Net;

namespace More
{
    public class RemoteDataServer : RecordServerHandler
    {
        readonly SharedFileSystem sharedFileSystem;

        public RemoteDataServer(SharedFileSystem sharedFileSystem, ByteBuffer sendBuffer)
            : base("RemoteData", sendBuffer)
        {
            this.sharedFileSystem = sharedFileSystem;
        }
        
        public override UInt32 HandleRecord(String clientString, Byte[] record, UInt32 offset, UInt32 offsetLimit,
            ByteBuffer sendBuffer, UInt32 sendOffset)
        {
            Byte command = record[offset];
            offset++;

            switch (command)
            {
                case RemoteData.List:
                    UInt32 objectID;
                    offset = record.ReadVarUInt32(offset, out objectID);

                    Console.WriteLine("List(ObjectID={0})", objectID);




                    DirectoryObjectEntry shareEntry;
                    for(int i = 0; i < sharedFileSystem.rootShareDirectories.Length; i++)
                    {
                        RootShareDirectory shareDirectory = sharedFileSystem.rootShareDirectories[i];
                    }

                    sharedFileSystem.CreateArrayOfShareObjects();

                    throw new NotImplementedException();








                    break;
                default:
                    Console.WriteLine("Unknown Command '{0}'", command);
                    return 0;
            }
        }
    }
}
