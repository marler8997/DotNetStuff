using System;
using System.Collections.Generic;
using System.Text;


using ObjectID = System.UInt32;
//using ObjectID = System.UInt64;


//
// The RemoteData Protocol
//
// Data Structures
//
// Permissions (the content of this is determined by client/server negotiation)
//
// DataObject (is an Object)
//     Examples: file, database entry
//
//     String name
//     Varint id
//     Permissions permissions
//     Directory[] parents
//
//     ID Creation: ID created after the first Read/Write
//
// StreamObject (is an Object)
//     Examples: tcp stream, udp connection, driver stream
//
//     String name
//     Varint id
//     Permissions permissions
//     Directory[] parents
//
//     ID Creation: ID created after the first Read/Write
//
// LinkObject(is an Object)
//     Examples: file link
//
//     String name
//     Directory[] parents
//     ObjectReference linkedObjectReference
//
// Directory (is an Object)
//     String name
//     Varint id
//     Directory[] parents
//     ObjectReference children
//
//     ID Creation: ID created after its contents are listed
//
//
// '/'       = The root directory
// '/MyDir'  = The MyDir Object underneath the root directory
// '/MyDir/' = Invalid path, a path cannot end with a '/'
//
//
//
// How to pass objects to the server
// ObjectID {
//   Enum Type {
//     ID
//     IDAndPath
//   }
//   Type type
//   switch(type) {
//     case ID {
//       Varint id
//     } case IDAndPath {
//       Varint directoryID
//       String path
//     }
//   }
// }
//
// 1. ObjectID
//    varint objectID  // Every object has a unique id
//
//    Note: if the client does not use the ObjectID, the server's response will include the ObjectID of the object
//
// 2. Directory ObjectID and PathName
//    varint parentID
//    String pathAndName
//
//    The server response will include the ID of the object
//
// 
//
// ObjectHandle varint // A handle to an object
// 
//
// Object ID {
//  
// }
//
// Operations:
//
//
// 1. List Objects
//    Example: "List Objects under Tree Path '/home'
//    Example: "List Objects with 'txt' extension
//    Example: "List Objects in the 'opengl' group
//
// 2. Open
//    Request: Object ID, OpenType
//    OpenType: Read, Write_NoLock, Write_LockOtherWrites, Append_NoLock, Append_LockOtherWrites
//
// 2. Read
//    Request: Object ID, DataOffset, (Optional DataLength)
//
// 3. Write
//    Request: Object ID, DataOffset, Data, Lock
//
// 4. Unlock
//    Request: Object ID
//
// ListDirectoryRequest {
//   ObjectID objectID
// }
// ListDirectoryResponse {
//     
// }
//
// ListShares
// 
//
//    



namespace More
{
    public static class RemoteData
    {
        public const UInt16 DefaultPort = 924;


        public const Byte List = 0;


    }


    public abstract class Permissions
    {

    }


    public enum ObjectType
    {
        Data = 0,
        Stream = 1,
        Link = 2,
        Directory = 3,
    }


    public abstract class ObjectEntry
    {
        public readonly ObjectType type;
        public readonly String name;
        protected ObjectEntry(ObjectType type, String name)
        {
            this.name = name;
        }
        public abstract UInt32 Serialize(ByteBuffer sendBuffer, UInt32 offset);
    }
    public class DataObjectEntry : ObjectEntry
    {
        public readonly Permissions permissions;
        public readonly ObjectID id;
        public DataObjectEntry(String name, Permissions permissions, ObjectID id)
            : base(ObjectType.Data, name)
        {
            this.permissions = permissions;
            this.id = id;
        }
        public override UInt32 Serialize(ByteBuffer sendBuffer, UInt32 offset)
        {
            /*
            Byte flags = (Byte)type;
            if (id != 0)
            {
                flags |= 0x80;
            }
            sendBuffer.EnsureCapacity(
            */
            throw new NotImplementedException();
        }
    }
    public class StreamObjectEntry : ObjectEntry
    {
        public readonly Permissions permissions;
        public readonly ObjectID id;
        public StreamObjectEntry(String name, Permissions permissions, ObjectID id)
            : base(ObjectType.Stream, name)
        {
            this.permissions = permissions;
            this.id = id;
        }
        public override UInt32 Serialize(ByteBuffer sendBuffer, UInt32 offset)
        {
            throw new NotImplementedException();
        }
    }
    public class LinkObjectEntry : ObjectEntry
    {
        public readonly Permissions permissions;
        public LinkObjectEntry(String name, Permissions permissions)
            : base(ObjectType.Link, name)
        {
            this.permissions = permissions;
        }
        public override UInt32 Serialize(ByteBuffer sendBuffer, UInt32 offset)
        {
            throw new NotImplementedException();
        }
    }
    public class DirectoryObjectEntry : ObjectEntry
    {
        public readonly Permissions permissions;
        public readonly ObjectID id;
        public DirectoryObjectEntry(String name, Permissions permissions, ObjectID id)
            : base(ObjectType.Directory, name)
        {
            this.permissions = permissions;
            this.id = id;
        }
        public override UInt32 Serialize(ByteBuffer sendBuffer, UInt32 offset)
        {
            throw new NotImplementedException();
        }
    }





    public static class ObjectEntries
    {
        // ObjectListEntry {
        //   Flags {
        //     Fxxxxxxx ObjectIDIncluded
        //     xxxxxxFF ObjectType (Data,Stream,Link,Directory)
        //   }
        //   String name
        //   // Permissions permissions
        //   switch(ObjectType) {
        //     case Data {
        //       if(ObjectIDIncluded) {
        //         Varint id
        //       }
        //     } case Stream {
        //       if(ObjectIDIncluded) {
        //         Varint id
        //       }
        //     } case Link {
        //     } case Directory {
        //       if(ObjectIDIncluded) {
        //         Varint id
        //       }
        //     }
        //   }
        // }


        public static UInt32 Serialize(this ObjectEntry[] entries, ByteBuffer sendBuffer, UInt32 offset)
        {
            UInt32 sizeOffset = offset;
            sendBuffer.EnsureCapacityCopyData(offset + 4);

            offset += 4;
            for (int i = 0; i < entries.Length; i++)
            {
                ObjectEntry entry = entries[i];
                offset = entry.Serialize(sendBuffer, offset);
            }

            UInt32 length = offset - sizeOffset - 4;

            sendBuffer.array.BigEndianSetUInt32(sizeOffset, length);
            return offset;
        }

        public static UInt32 DeserializeObjects(Byte[] data, UInt32 offset, out ObjectEntry[] entries)
        {
            UInt32 entriesLength = data.BigEndianReadUInt32(offset);
            offset += 4;

            entries = new ObjectEntry[entriesLength];
            for (UInt32 i = 0; i < entriesLength; i++)
            {
                offset = DeserializeObject(data, offset, out entries[i]);
            }
            return offset;
        }
        private static UInt32 DeserializeObject(Byte[] data, UInt32 offset, out ObjectEntry objectEntry)
        {
            Byte flags = data[offset];

            ObjectType objectType = (ObjectType)(flags & 0x7);

            UInt32 nameLength;
            offset = data.ReadVarUInt32(offset, out nameLength);
            String name = Encoding.ASCII.GetString(data, (Int32)offset, (Int32)nameLength);
            offset += nameLength;

            //UInt32 permissionsLength;
            //offset = data.ReadVarUInt32(offset, out permissionsLength);    

            Boolean idIncluded;
            ObjectID id = 0;

            switch (objectType)
            {
                case ObjectType.Data:
                    idIncluded = (flags & 0x80) == 0x80;
                    if (idIncluded)
                    {
                        offset = data.ReadVarUInt32(offset, out id);
                    }

                    objectEntry = new DataObjectEntry(name, null, id);
                    return offset;
                case ObjectType.Stream:
                    idIncluded = (flags & 0x80) == 0x80;
                    if (idIncluded)
                    {
                        offset = data.ReadVarUInt32(offset, out id);
                    }

                    objectEntry = new StreamObjectEntry(name, null, id);
                    return offset;
                case ObjectType.Link:

                    objectEntry = new LinkObjectEntry(name, null);
                    return offset;
                case ObjectType.Directory:
                    idIncluded = (flags & 0x80) == 0x80;
                    if (idIncluded)
                    {
                        offset = data.ReadVarUInt32(offset, out id);
                    }

                    objectEntry = new DirectoryObjectEntry(name, null, id);
                    return offset;
            }

            throw new FormatException(String.Format("Unknown ObjectType {0} ({1})", objectType, (Int32)objectType));
        }
    }


}
