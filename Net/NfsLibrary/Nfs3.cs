//
// http://www.ietf.org/rfc/rfc1813.txt
//

using System;
using System.Runtime.Serialization;

using More;

using FileID   = System.UInt64;
using Uid      = System.UInt32;
using Gid      = System.UInt32;
using Size     = System.UInt64;
using Offset   = System.UInt64;
using Count    = System.UInt32;
using Cookie   = System.UInt64;
using FileName = System.String;

namespace More.Net
{

    public enum Nfs3Command {
        NULL        = 0,
        GETATTR     = 1,
        SETATTR     = 2,
        LOOKUP      = 3,
        ACCESS      = 4,
        READLINK    = 5,
        READ        = 6,
        WRITE       = 7,
        CREATE      = 8,
        MKDIR       = 9,
        SYMLINK     = 10,
        MKNOD       = 11,
        REMOVE      = 12,
        RMDIR       = 13,
        RENAME      = 14,
        LINK        = 15,
        READDIR     = 16,
        READDIRPLUS = 17,
        FSSTAT      = 18,
        FSINFO      = 19,
        PATHCONF    = 20,
        COMMIT      = 21,
    }

    public static class Nfs3
    {
        public const UInt32 ProgramNumber = 100003;

        /*
        public const UInt32 NULL                 = 0;
        public const UInt32 GETATTR              = 1;
        public const UInt32 SETATTR              = 2;
        public const UInt32 LOOKUP               = 3;
        public const UInt32 ACCESS               = 4;
        public const UInt32 READLINK             = 5;
        public const UInt32 READ                 = 6;
        public const UInt32 WRITE                = 7;
        public const UInt32 CREATE               = 8;
        public const UInt32 MKDIR                = 9;
        public const UInt32 SYMLINK              = 10;
        public const UInt32 MKNOD                = 11;
        public const UInt32 REMOVE               = 12;
        public const UInt32 RMDIR                = 13;
        public const UInt32 RENAME               = 14;
        public const UInt32 LINK                 = 15;
        public const UInt32 READDIR              = 16;
        public const UInt32 READDIRPLUS          = 17;
        public const UInt32 FSSTAT               = 18;
        public const UInt32 FSINFO               = 19;
        public const UInt32 PATHCONF             = 20;
        public const UInt32 COMMIT               = 21;
        public const UInt32 ProcedureNumberLimit = 22;
        */




        private static RpcProgramHeader programHeader = null;
        public static RpcProgramHeader ProgramHeader
        {
            get
            {
                if (programHeader == null)
                {
                    programHeader = new RpcProgramHeader(RpcVersion.Two, ProgramNumber, 3);
                }
                return programHeader;
            }
        }
        /*
        private static IRpcProcedureMap procedureMap = null;
        public static IRpcProcedureMap ProcedureMap
        {
            get
            {
                if(procedureMap == null)
                {
                    RpcProcedure[] procedures = new RpcProcedure[ProcedureNumberLimit];

                    procedures[NULL  ] = new RpcProcedure("Null"              , NULL     ,
                        VoidReflector.Instance, VoidReflector.Instance);
                    procedures[FSINFO] = new RpcProcedure("FileSystemInfo"    , FSINFO   ,
                        Nfs3Procedure.FsInfoCall.objectSerializer, Nfs3Procedure.FsInfoReply.objectSerializer);

                    procedureMap = new RpcProcedureArrayMap(procedures);
                }
                return procedureMap;
            }
        }
        */

        public const UInt32 FileHandleMaxSize  = 64;
        public const UInt32 CookieVerifierSize =  8;
        public const UInt32 CreateVerifierSize =  8;
        public const UInt32 WriteVerifierSize  =  8;
    }
}    
namespace More.Net.Nfs3Procedure
{
    public enum FileType
    {
        Regular         = 1,
        Directory       = 2,
        BlockDevice     = 3,
        CharacterDevice = 4,
        SymbolicLink    = 5,
        Socket          = 6,
        NamedPipe       = 7
    }
    public enum Status
    {
        Ok                           =     0,
        ErrorPermission              =     1,
        ErrorNoSuchFileOrDirectory   =     2,
        ErrorIOHard                  =     5,
        ErrorIONoSuchDeviceOrAddress =     6,
        ErrorAccess                  =    13,
        ErrorAlreadyExists           =    17,
        ErrorCrossLinkDevice         =    18,
        ErrorNoSuchDevice            =    19,
        ErrorNotDirectory            =    20,
        ErrorIsDirectory             =    21,
        ErrorInvalidArgument         =    22,
        ErrorFileTooBig              =    27,
        ErrorNoSpaceLeftOnDevice     =    28,
        ErrorReadOnlyFileSystem      =    30,
        ErrorToManyHardLinks         =    31,
        ErrorNameTooLong             =    63,
        ErrorDirectoryNotEmpty       =    66,
        ErrorUserQuotaExceeded       =    69,
        ErrorStaleFileHandle         =    70,
        ErrorTooManyRemoteLevels     =    71,
        ErrorBadHandle               = 10001,
        ErrorNotSynchronized         = 10002,
        ErrorBadCookie               = 10003,
        ErrorNotSupported            = 10004,
        ErrorTooSmall                = 10005,
        ErrorServerFault             = 10006,
        ErrorBadType                 = 10007,
        ErrorJukeBox                 = 10008,
    }
    [Flags]
    public enum ModeFlags
    {
        OtherExecute    = 0x0001,
        OtherWrite      = 0x0002,
        OtherRead       = 0x0004,
        GroupExecute    = 0x0008,
        GroupWrite      = 0x0010,
        GroupRead       = 0x0020,
        OwnerExecute    = 0x0040,
        OwnerWrite      = 0x0080,
        OwnerRead       = 0x0100,
        SaveSwappedText = 0x0200,
        SetGidOnExec    = 0x0400,
        SetUidOnExec    = 0x0800,
        UnknownFlag1    = 0x1000,
        UnknownFlag2    = 0x2000,
        UnknownFlag3    = 0x3000,
        UnknownFlag4    = 0x4000,
    }
    [Flags]
    public enum FileProperties
    {
        Fsf3Link = 0x001,
        Fsf3SymLink = 0x002,
        Fsf3Homogeneous = 0x008,
        Fsf3CanSetTime = 0x010,
    }
    public class Time// : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new BigEndianUInt32Reflector(typeof(Time), "seconds"),
            new BigEndianUInt32Reflector(typeof(Time), "nanoseconds"),
        });

        public UInt32 seconds, nanoseconds;

        public Time()
        {
        }
        public Time(UInt32 seconds, UInt32 nanoseconds)
        {
            this.seconds = seconds;
            this.nanoseconds = nanoseconds;
        }
    }
    public class SizeAndTimes
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new BigEndianUInt64Reflector        (typeof(SizeAndTimes), "fileSize"),
            new ClassFieldReflectors<Time>(typeof(SizeAndTimes), "lastModifyTime", Time.memberSerializers),
            new ClassFieldReflectors<Time>(typeof(SizeAndTimes), "lastAttributeModifyTime", Time.memberSerializers),
        });

        public Size fileSize;
        public Time lastModifyTime;
        public Time lastAttributeModifyTime;

        public SizeAndTimes(FileAttributes fileAttributes)
        {
            this.fileSize = fileAttributes.fileSize;
            this.lastModifyTime = new Time(fileAttributes.lastModifyTime.seconds, fileAttributes.lastModifyTime.nanoseconds);
            this.lastAttributeModifyTime = new Time(fileAttributes.lastAttributeModifyTime.seconds, fileAttributes.lastAttributeModifyTime.nanoseconds);
        }
    }
    public class FileAttributes
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrEnumReflector  (typeof(FileAttributes), "fileType", typeof(FileType)),
            new XdrEnumReflector  (typeof(FileAttributes), "protectionMode", typeof(ModeFlags)),
            new BigEndianUInt32Reflector(typeof(FileAttributes), "hardLinks"),
            new BigEndianUInt32Reflector(typeof(FileAttributes), "ownerUid"),
            new BigEndianUInt32Reflector(typeof(FileAttributes), "gid"),
            new BigEndianUInt64Reflector(typeof(FileAttributes), "fileSize"),
            new BigEndianUInt64Reflector(typeof(FileAttributes), "diskSize"),
            new BigEndianUInt32Reflector(typeof(FileAttributes), "specialData1"),
            new BigEndianUInt32Reflector(typeof(FileAttributes), "specialData2"),
            new BigEndianUInt64Reflector(typeof(FileAttributes), "fileSystemID"),
            new BigEndianUInt64Reflector(typeof(FileAttributes), "fileID"),
            new ClassFieldReflectors<Time>(typeof(FileAttributes), "lastAccessTime", Time.memberSerializers),
            new ClassFieldReflectors<Time>(typeof(FileAttributes), "lastModifyTime", Time.memberSerializers),
            new ClassFieldReflectors<Time>(typeof(FileAttributes), "lastAttributeModifyTime", Time.memberSerializers),
        });

        public FileType fileType;
        public ModeFlags protectionMode;
        public UInt32 hardLinks;
        public Uid ownerUid;
        public Gid gid;
        public Size fileSize;
        public Size diskSize;
        public UInt32 specialData1, specialData2;
        public UInt64 fileSystemID;
        public FileID fileID;
        public Time lastAccessTime;
        public Time lastModifyTime;
        public Time lastAttributeModifyTime;

        public FileAttributes()
        {
        }
        public FileAttributes(
            FileType fileType,
            ModeFlags protectionMode,
            UInt32 hardLinks,
            Uid ownerUid,
            Gid gid,
            Size fileSize,
            Size diskSize,
            UInt32 specialData1,
            UInt32 specialData2,
            UInt64 fileSystemID,
            FileID fileID,
            Time lastAccessTime,
            Time lastModifyTime,
            Time lastAttributeModifyTime)
        {
            this.fileType = fileType;
            this.protectionMode = protectionMode;
            this.hardLinks = hardLinks;
            this.ownerUid = ownerUid;
            this.gid = gid;
            this.fileSize = fileSize;
            this.diskSize = diskSize;
            this.specialData1 = specialData1;
            this.specialData2 = specialData2;
            this.fileSystemID = fileSystemID;
            this.fileID = fileID;
            this.lastAccessTime = lastAccessTime;
            this.lastModifyTime = lastModifyTime;
            this.lastAttributeModifyTime = lastAttributeModifyTime;
        }
    }
    public class BeforeAndAfterAttributes
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector                    (typeof(BeforeAndAfterAttributes), "beforeIncluded"),
                new Reflectors(new ClassFieldReflectors<SizeAndTimes>  (typeof(BeforeAndAfterAttributes), "before", SizeAndTimes.memberSerializers)),
                VoidReflector.Reflectors),
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector                    (typeof(BeforeAndAfterAttributes), "afterIncluded"),
                new Reflectors(new ClassFieldReflectors<FileAttributes>(typeof(BeforeAndAfterAttributes), "after", FileAttributes.memberSerializers)),
                VoidReflector.Reflectors),            
        });

        private static BeforeAndAfterAttributes none = null;
        public static BeforeAndAfterAttributes None
        {
            get
            {
                if (none == null) none = new BeforeAndAfterAttributes(null, null);
                return none;
            }
        }

        public Boolean beforeIncluded;
        public SizeAndTimes before;

        public Boolean afterIncluded;
        public FileAttributes after;

        public BeforeAndAfterAttributes(SizeAndTimes before, FileAttributes after)
        {
            this.beforeIncluded = (before == null) ? false : true;
            this.before = before;

            this.afterIncluded = (after == null) ? false : true;
            this.after = after;
        }
    }
    public class OptionalFileAttributes
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector                    (typeof(OptionalFileAttributes), "fileAttributesIncluded"),
                new Reflectors(new ClassFieldReflectors<FileAttributes>(typeof(OptionalFileAttributes), "fileAttributes", FileAttributes.memberSerializers)),
                VoidReflector.Reflectors
            )
        });

        public static OptionalFileAttributes None = new OptionalFileAttributes();

        public Boolean fileAttributesIncluded;
        public FileAttributes fileAttributes;

        private OptionalFileAttributes()
        {
            this.fileAttributesIncluded = false;
            this.fileAttributes = null;
        }
        public OptionalFileAttributes(FileAttributes fileAttributes)
        {
            if (fileAttributes == null)
            {
                this.fileAttributesIncluded = false;
            }
            else
            {
                this.fileAttributesIncluded = true;
                this.fileAttributes = fileAttributes;
            }
        }
    }
    public class OptionalFileHandle
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector(typeof(OptionalFileHandle), "fileHandleIncluded"),
                new Reflectors(new XdrOpaqueVarLengthReflector(typeof(OptionalFileHandle), "fileHandle", Nfs3.FileHandleMaxSize)),
                VoidReflector.Reflectors
            )
        });

        public static OptionalFileHandle None = new OptionalFileHandle();

        public Boolean fileHandleIncluded;
        public Byte[] fileHandle;

        private OptionalFileHandle()
        {
            this.fileHandleIncluded = false;
            this.fileHandle = null;
        }
        public OptionalFileHandle(Byte[] fileHandle)
        {
            if(fileHandle == null) throw new ArgumentNullException("fileHandle");

            this.fileHandleIncluded = true;
            this.fileHandle = fileHandle;
        }
    }
    public class SetAttributesStruct
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector          (typeof(SetAttributesStruct), "setMode"),
                new Reflectors(new XdrEnumReflector             (typeof(SetAttributesStruct), "mode", typeof(ModeFlags))),
                VoidReflector.Reflectors
            ),
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector          (typeof(SetAttributesStruct), "setUid"),
                new Reflectors(new BigEndianUInt32Reflector           (typeof(SetAttributesStruct), "uid")),
                VoidReflector.Reflectors
            ),
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector          (typeof(SetAttributesStruct), "setGid"),
                new Reflectors(new BigEndianUInt32Reflector           (typeof(SetAttributesStruct), "gid")),
                VoidReflector.Reflectors
            ),
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector          (typeof(SetAttributesStruct), "setSize"),
                new Reflectors(new BigEndianUInt64Reflector           (typeof(SetAttributesStruct), "size")),
                VoidReflector.Reflectors
            ),
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector          (typeof(SetAttributesStruct), "setLastAccessTime"),
                new Reflectors(new ClassFieldReflectors<Time>(typeof(SetAttributesStruct), "lastAccessTime", Time.memberSerializers)),
                VoidReflector.Reflectors
            ),
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector          (typeof(SetAttributesStruct), "setLastModifyTime"),
                new Reflectors(new ClassFieldReflectors<Time>(typeof(SetAttributesStruct), "lastModifyTime", Time.memberSerializers)),
                VoidReflector.Reflectors
            ),
        });

        public Boolean setMode;
        public ModeFlags mode;

        public Boolean setUid;
        public Uid uid;

        public Boolean setGid;
        public Gid gid;

        public Boolean setSize;
        public Size size;

        public Boolean setLastAccessTime;
        public Time lastAccessTime;

        public Boolean setLastModifyTime;
        public Time lastModifyTime;

        public SetAttributesStruct()
        {
        }
        public SetAttributesStruct(
            Boolean setMode, ModeFlags mode,
            Boolean setUid, Uid uid,
            Boolean setGid, Gid gid,
            Boolean setSize, Size size,
            Time lastAccessTime,
            Time lastModifyTime
            )
        {
            this.setMode = setMode; this.mode = mode;
            this.setUid = setUid; this.uid = uid;
            this.setGid = setGid; this.gid = gid;
            this.setSize = setSize; this.size = size;

            this.setLastAccessTime = (lastAccessTime != null);
            this.lastAccessTime = lastAccessTime;

            this.setLastModifyTime = (lastModifyTime != null);
            this.lastModifyTime = lastModifyTime;
        }
    }

    //
    // Null Procedure
    //
    public class Null : RpcProcedure
    {
        public Null()
            : base("Null", (UInt32)Nfs3Command.NULL, VoidSerializer.Instance, VoidSerializer.Instance)
        {
        }
    }

    //
    // GetFileAttributes Procedure
    //
    /*
    public class GetFileAttributes : RpcProcedure
    {
        public readonly GetFileAttributesReply reply;

        public GetFileAttributes(GetFileAttributesCall call)
            : base("GetFileAttributes", (UInt32)Nfs3Command.GETATTR, call)
        {
            this.reply = new GetFileAttributesReply();
            this.responseSerializer = this.reply;
        }
    }
    */
    public class GetFileAttributesCall : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(GetFileAttributesCall), "handle", Nfs3.FileHandleMaxSize),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Byte[] handle;
        
        public GetFileAttributesCall(Byte[] data, UInt32 offset, UInt32 offsetLimit)
        {
            memberSerializers.Deserialize(this, data, offset, offsetLimit);
        }
        public GetFileAttributesCall(Byte[] handle)
        {
            this.handle = handle;
        }
    }
    public class GetFileAttributesReply : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector(typeof(GetFileAttributesReply), "status", typeof(Status)),
                VoidReflector.ReflectorsArray,
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, new IReflector[] {
                    new ClassFieldReflectors<FileAttributes>(typeof(GetFileAttributesReply), "fileAttributes", FileAttributes.memberSerializers)})
            ),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Status status;
        public FileAttributes fileAttributes;
        public GetFileAttributesReply(FileAttributes fileAttributes)
        {
            this.status = Status.Ok;
            this.fileAttributes = fileAttributes;
        }
        public GetFileAttributesReply(Status status)
        {
            if(status == Status.Ok)
                throw new InvalidOperationException("WrongConstructor: This constructor is meant for enum values other than Ok");
            this.status = status;
        }
    }

    //
    // SetFileAttributes Procedure
    //
    /*
    public class SetFileAttributes : RpcProcedure
    {
        public readonly SetFileAttributesReply reply;

        public SetFileAttributes(SetFileAttributesCall call)
            : base("SetFileAttributes", (UInt32)Nfs3Command.SETATTR, call)
        {
            this.reply = new SetFileAttributesReply();
            this.responseSerializer = this.reply;
        }
    }
    */
    public class SetFileAttributesCall : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrOpaqueVarLengthReflector                 (typeof(SetFileAttributesCall), "fileHandle", Nfs3.FileHandleMaxSize),
            new ClassFieldReflectors<SetAttributesStruct>(typeof(SetFileAttributesCall), "setAttributes", SetAttributesStruct.memberSerializers),
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector(typeof(SetFileAttributesCall), "checkGuardTime"),
                new Reflectors(new ClassFieldReflectors<Time>(typeof(SetFileAttributesCall), "guardTime", Time.memberSerializers)),
                VoidReflector.Reflectors),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Byte[] fileHandle;
        public SetAttributesStruct setAttributes;

        public Boolean checkGuardTime;
        public Time guardTime;

        public SetFileAttributesCall(Byte[] data, UInt32 offset, UInt32 offsetLimit)
        {
            memberSerializers.Deserialize(this, data, offset, offsetLimit);
        }
        public SetFileAttributesCall(Byte[] fileHandle, SetAttributesStruct setAttributes, Time guardTime)
        {
            this.fileHandle = fileHandle;
            this.setAttributes = setAttributes;
            if (guardTime == null)
            {
                this.checkGuardTime = false;
            }
            else
            {
                this.checkGuardTime = false;
                this.guardTime = guardTime;
            }
        }
    }
    public class SetFileAttributesReply : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrEnumReflector(typeof(SetFileAttributesReply), "status", typeof(Status)),
            new ClassFieldReflectors<BeforeAndAfterAttributes>(typeof(SetFileAttributesReply), "beforeAndAfter", BeforeAndAfterAttributes.memberSerializers),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Status status;
        public BeforeAndAfterAttributes beforeAndAfter;

        public SetFileAttributesReply(Status status, BeforeAndAfterAttributes beforeAndAfter)
        {
            this.status = status;
            this.beforeAndAfter = beforeAndAfter;
        }
    }

    //
    // Lookup Procedure
    //
    /*
    public class Lookup : RpcProcedure
    {
        public readonly LookupReply reply;

        public Lookup(LookupCall call)
            : base("Lookup", (UInt32)Nfs3Command.LOOKUP, call)
        {
            this.reply = new LookupReply();
            this.responseSerializer = this.reply;
        }
    }
    */
    public class LookupCall : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(LookupCall), "directoryHandle", Nfs3.FileHandleMaxSize),
            new XdrStringReflector         (typeof(LookupCall), "fileName", UInt32.MaxValue),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Byte[] directoryHandle;
        public String fileName;

        public LookupCall(Byte[] data, UInt32 offset, UInt32 offsetLimit)
        {
            memberSerializers.Deserialize(this, data, offset, offsetLimit);
        }
        public LookupCall(Byte[] directoryHandle, String fileName)
        {
            this.directoryHandle = directoryHandle;
            this.fileName = fileName;
        }
    }
    public class LookupReply : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector(typeof(LookupReply), "status", typeof(Status)),
                new IReflector[] {new ClassFieldReflectors<OptionalFileAttributes>(typeof(LookupReply), "optionalFailAttributes", OptionalFileAttributes.memberSerializers)},
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, new IReflector[] {
                    new XdrOpaqueVarLengthReflector(typeof(LookupReply), "fileHandle", Nfs3.FileHandleMaxSize),
                    new ClassFieldReflectors<OptionalFileAttributes>(typeof(LookupReply), "optionalFileAttributes", OptionalFileAttributes.memberSerializers),
                    new ClassFieldReflectors<OptionalFileAttributes>(typeof(LookupReply), "optionalDirectoryAttributes", OptionalFileAttributes.memberSerializers),
                })           
            ),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Status status;
        public OptionalFileAttributes optionalFailAttributes;

        public Byte[] fileHandle;
        public OptionalFileAttributes optionalDirectoryAttributes, optionalFileAttributes;

        public LookupReply(Status status, OptionalFileAttributes optionalFailAttributes)
        {
            if (status == Status.Ok)
                throw new InvalidOperationException("WrongConstructor: This constructor is meant for enum values other than Ok");

            this.status = status;
            this.optionalFailAttributes = optionalFailAttributes;
        }
        public LookupReply(Byte[] fileHandle, OptionalFileAttributes optionalDirectoryAttributes, OptionalFileAttributes optionalFileAttributes)
        {
            this.fileHandle = fileHandle;
            this.optionalDirectoryAttributes = optionalDirectoryAttributes;
            this.optionalFileAttributes = optionalFileAttributes;
        }
    }

    //
    // Access Procedure
    //
    [Flags]
    public enum AccessFlags
    {
        Read    = 0x0001,
        Lookup  = 0x0002,
        Modify  = 0x0004,
        Extend  = 0x0008,
        Delete  = 0x0010,
        Execute = 0x0020,
    }
    /*
    public class Access : RpcProcedure
    {
        public readonly AccessReply reply;

        public Access(AccessCall call)
            : base("Access", (UInt32)Nfs3Command.ACCESS, call)
        {
            this.reply = new AccessReply();
            this.responseSerializer = this.reply;
        }
    }
    */
    public class AccessCall : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(AccessCall), "fileHandle", Nfs3.FileHandleMaxSize),
            new XdrEnumReflector           (typeof(AccessCall), "accessFlags", typeof(AccessFlags)),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Byte[] fileHandle;
        public AccessFlags accessFlags;
        
        public AccessCall(Byte[] data, UInt32 offset, UInt32 offsetLimit)
        {
            memberSerializers.Deserialize(this, data, offset, offsetLimit);
        }
        public AccessCall(Byte[] fileHandle, AccessFlags accessFlags)
        {
            this.fileHandle = fileHandle;
            this.accessFlags = accessFlags;
        }
    }
    public class AccessReply : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector(typeof(AccessReply), "status", typeof(Status)),
                new IReflector[] {new ClassFieldReflectors<OptionalFileAttributes>(typeof(AccessReply), "optionalFileAttributes", OptionalFileAttributes.memberSerializers)},
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, new IReflector[] {
                    new ClassFieldReflectors<OptionalFileAttributes>(typeof(AccessReply), "optionalFileAttributes", OptionalFileAttributes.memberSerializers),
                    new XdrEnumReflector(typeof(AccessReply), "accessFlags", typeof(AccessFlags)),
                })
            ),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Status status;
        public OptionalFileAttributes optionalFileAttributes;
        public AccessFlags accessFlags;

        public AccessReply(OptionalFileAttributes optionalFileAttributes, AccessFlags accessFlags)
        {
            this.status = Status.Ok;
            this.optionalFileAttributes = optionalFileAttributes;
            this.accessFlags = accessFlags;
        }
        public AccessReply(Status status, OptionalFileAttributes optionalFileAttributes)
        {
            if (status == Status.Ok)
                throw new InvalidOperationException("WrongConstructor: This constructor is meant for enum values other than Ok");
            this.status = status;
            this.optionalFileAttributes = optionalFileAttributes;
        }
    }

    //
    // Read Procedure
    //
    /*
    public class Read : RpcProcedure
    {
        public readonly ReadReply reply;

        public Read(ReadCall call)
            : base("Read", (UInt32)Nfs3Command.READ, call)
        {
            this.reply = new ReadReply();
            this.responseSerializer = this.reply;
        }
    }
    */
    public class ReadCall : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(ReadCall), "fileHandle", Nfs3.FileHandleMaxSize),
            new BigEndianUInt64Reflector         (typeof(ReadCall), "offset"),
            new BigEndianUInt32Reflector         (typeof(ReadCall), "count"),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Byte[] fileHandle;
        public Offset offset;
        public Count count;
        
        public ReadCall(Byte[] data, UInt32 offset, UInt32 offsetLimit)
        {
            memberSerializers.Deserialize(this, data, offset, offsetLimit);
        }
        public ReadCall(Byte[] fileHandle, Offset offset, Count count)
        {
            this.fileHandle = fileHandle;
            this.offset = offset;
            this.count = count;
        }
    }
    public class ReadReply : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector(typeof(ReadReply), "status", typeof(Status)),
                new IReflector[] {new ClassFieldReflectors<OptionalFileAttributes>(typeof(ReadReply), "optionalFileAttributes", OptionalFileAttributes.memberSerializers)},
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, new IReflector[] {
                    new ClassFieldReflectors<OptionalFileAttributes>(typeof(ReadReply), "optionalFileAttributes", OptionalFileAttributes.memberSerializers),
                    new BigEndianUInt32Reflector                             (typeof(ReadReply), "count"),
                    new XdrBooleanReflector                            (typeof(ReadReply), "endOfFile"),
                    new XdrOpaqueVarLengthReflector<PartialByteArraySerializer>(typeof(ReadReply), "fileData", -1),
                })
            ),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Status status;
        public OptionalFileAttributes optionalFileAttributes;
        public Count count;
        public Boolean endOfFile;
        public PartialByteArraySerializer fileData;

        public ReadReply(OptionalFileAttributes optionalFileAttributes, Count count, Boolean endOfFile, PartialByteArraySerializer fileData)
        {
            this.status = Status.Ok;
            this.optionalFileAttributes = optionalFileAttributes;
            this.count = count;
            this.endOfFile = endOfFile;
            this.fileData = fileData;
        }
        public ReadReply(Status status, OptionalFileAttributes optionalFileAttributes)
        {
            if (status == Status.Ok)
                throw new InvalidOperationException("WrongConstructor: This constructor is meant for enum values other than Ok");
            this.status = status;
            this.optionalFileAttributes = optionalFileAttributes;
        }
    }

    //
    // Write Procedure
    //
    public enum StableHowEnum
    {
        Unstable = 0,
        DataSync = 1,
        FileSync = 2,
    }
    /*
    public class Write : RpcProcedure
    {
        public readonly WriteReply reply;

        public Write(WriteCall call)
            : base("Write", (UInt32)Nfs3Command.WRITE, call)
        {
            this.reply = new WriteReply();
            this.responseSerializer = this.reply;
        }
    }
    */
    public class WriteCall : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(WriteCall), "fileHandle", Nfs3.FileHandleMaxSize),
            new BigEndianUInt64Reflector         (typeof(WriteCall), "offset"),
            new BigEndianUInt32Reflector         (typeof(WriteCall), "count"),
            new XdrEnumReflector           (typeof(WriteCall), "stableHow", typeof(StableHowEnum)),
            new XdrOpaqueVarLengthReflector(typeof(WriteCall), "data", UInt32.MaxValue),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Byte[] fileHandle;
        public Offset offset;
        public Count count;
        public StableHowEnum stableHow;
        public Byte[] data;
        
        public WriteCall(Byte[] data, UInt32 offset, UInt32 offsetLimit)
        {
            memberSerializers.Deserialize(this, data, offset, offsetLimit);
        }
        public WriteCall(Byte[] fileHandle, StableHowEnum stableHow, Byte[] data, Offset offset, Count count)
        {
            this.fileHandle = fileHandle;
            this.data = data;
            this.offset = offset;
            this.count = count;
            this.stableHow = stableHow;
        }
    }
    public class WriteReply : ISerializerCreator
    {
        private static readonly ClassFieldReflectors<BeforeAndAfterAttributes> fileAttributesSerializer =
            new ClassFieldReflectors<BeforeAndAfterAttributes>(typeof(WriteReply), "fileAttributes", BeforeAndAfterAttributes.memberSerializers);

        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector                             (typeof(WriteReply), "status", typeof(Status)),
                new IReflector[] {fileAttributesSerializer},
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, new IReflector[] {
                    fileAttributesSerializer,
                    new BigEndianUInt32Reflector                       (typeof(WriteReply), "count"),
                    new XdrEnumReflector                         (typeof(WriteReply), "stableHow", typeof(StableHowEnum)),
                    new XdrOpaqueFixedLengthReflector            (typeof(WriteReply), "writeVerifier", Nfs3.WriteVerifierSize),
                })
            )
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Status status;
        public BeforeAndAfterAttributes fileAttributes;
        public Count count;
        public StableHowEnum stableHow;
        public Byte[] writeVerifier;

        public WriteReply(BeforeAndAfterAttributes fileAttributes, Count count, StableHowEnum stableHow, Byte[] writeVerifier)
        {
            this.status = Status.Ok;
            this.fileAttributes = fileAttributes;
            this.count = count;
            this.stableHow = stableHow;
            this.writeVerifier = writeVerifier;
        }
        public WriteReply(Status status, BeforeAndAfterAttributes fileAttributes)
        {
            if (status == Status.Ok)
                throw new InvalidOperationException("WrongConstructor: This constructor is meant for enum values other than Ok");
            this.status = status;
            this.fileAttributes = fileAttributes;
        }
    }

    //
    // Create Procedure
    //
    public enum CreateModeEnum
    {
        Unchecked = 0,
        Guarded   = 1,
        Exclusive = 2,
    }
    /*
    public class Create : RpcProcedure
    {
        public readonly CreateReply reply;

        public Create(CreateCall call)
            : base("Create", (UInt32)Nfs3Command.CREATE, call)
        {
            this.reply = new CreateReply();
            this.responseSerializer = this.reply;
        }
    }
    */
    public class CreateCall : ISerializerCreator
    {
        public static ClassFieldReflectors<SetAttributesStruct> setAttributesSerializer =
            new ClassFieldReflectors<SetAttributesStruct>(typeof(CreateCall), "setAttributes", SetAttributesStruct.memberSerializers);

        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(CreateCall), "directoryHandle", Nfs3.FileHandleMaxSize),
            new XdrStringReflector         (typeof(CreateCall), "newFileName", UInt32.MaxValue),
            new XdrDescriminatedUnionReflector<CreateModeEnum>(
                new XdrEnumReflector       (typeof(CreateCall), "mode", typeof(CreateModeEnum)),
                null,
                new XdrDescriminatedUnionReflector<CreateModeEnum>.KeyAndSerializer(CreateModeEnum.Unchecked, new IReflector[] {setAttributesSerializer}),
                new XdrDescriminatedUnionReflector<CreateModeEnum>.KeyAndSerializer(CreateModeEnum.Guarded  , new IReflector[] {setAttributesSerializer}),
                new XdrDescriminatedUnionReflector<CreateModeEnum>.KeyAndSerializer(CreateModeEnum.Exclusive, new IReflector[] {
                    new XdrOpaqueFixedLengthReflector(typeof(CreateCall), "createVerifier", Nfs3.CreateVerifierSize)})
            ),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Byte[] directoryHandle;
        public String newFileName;

        public CreateModeEnum mode;
        public SetAttributesStruct setAttributes;
        public Byte[] createVerifier;
        
        public CreateCall(Byte[] data, UInt32 offset, UInt32 offsetLimit)
        {
            memberSerializers.Deserialize(this, data, offset, offsetLimit);
        }
        public CreateCall(Byte[] directoryHandle, String newFileName, CreateModeEnum mode, SetAttributesStruct setAttributes)
        {
            if(mode != CreateModeEnum.Unchecked && mode != CreateModeEnum.Guarded)
                throw new InvalidOperationException(String.Format(
                    "Wrong Constructor: This constructor is only meant for 'Unchecked' or 'Guarded' mode but you passed in '{0}'",
                    mode));
            this.directoryHandle = directoryHandle;
            this.newFileName = newFileName;
            this.mode = mode;
            this.setAttributes = setAttributes;
        }
    }
    public class CreateReply : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector                                   (typeof(CreateReply), "status", typeof(Status)),
                VoidReflector.ReflectorsArray,
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, new IReflector[] {
                    new ClassFieldReflectors<OptionalFileHandle>    (typeof(CreateReply), "optionalFileHandle", OptionalFileHandle.memberSerializers),
                    new ClassFieldReflectors<OptionalFileAttributes>(typeof(CreateReply), "optionalFileAttributes", OptionalFileAttributes.memberSerializers),
                })
            ),
            new ClassFieldReflectors<BeforeAndAfterAttributes>      (typeof(CreateReply), "directoryAttributes", BeforeAndAfterAttributes.memberSerializers)
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Status status;
        public OptionalFileHandle optionalFileHandle;
        public OptionalFileAttributes optionalFileAttributes;
        public BeforeAndAfterAttributes directoryAttributes;

        public CreateReply(OptionalFileHandle optionalFileHandle, OptionalFileAttributes optionalFileAttributes, BeforeAndAfterAttributes directoryAttributes)
        {
            this.status = Status.Ok;
            this.optionalFileHandle = optionalFileHandle;
            this.optionalFileAttributes = optionalFileAttributes;
            this.directoryAttributes = directoryAttributes;

        }
        public CreateReply(Status status, BeforeAndAfterAttributes directoryAttributes)

        {
            if (status == Status.Ok)
                throw new InvalidOperationException("WrongConstructor: This constructor is meant for enum values other than Ok");
            this.status = status;
            this.directoryAttributes = directoryAttributes;
        }
    }


    //
    // Mkdir Procedure
    //
    /*
    public class Mkdir : RpcProcedure
    {
        public readonly MkdirReply reply;

        public Mkdir(MkdirCall call)
            : base("Mkdir", (UInt32)Nfs3Command.MKDIR, call)
        {
            this.reply = new MkdirReply();
            this.responseSerializer = this.reply;
        }
    }
    */
    public class MkdirCall : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(MkdirCall), "directoryHandle", Nfs3.FileHandleMaxSize),
            new XdrStringReflector         (typeof(MkdirCall), "newDirectoryName", UInt32.MaxValue),
            new ClassFieldReflectors<SetAttributesStruct>(typeof(MkdirCall), "setAttributes", SetAttributesStruct.memberSerializers),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Byte[] directoryHandle;
        public String newDirectoryName;

        public SetAttributesStruct setAttributes;
        
        public MkdirCall(Byte[] data, UInt32 offset, UInt32 offsetLimit)
        {
            memberSerializers.Deserialize(this, data, offset, offsetLimit);
        }
        public MkdirCall(Byte[] directoryHandle, String newDirectoryName, SetAttributesStruct setAttributes)
        {
            this.directoryHandle = directoryHandle;
            this.newDirectoryName = newDirectoryName;
            this.setAttributes = setAttributes;
        }
    }
    public class MkdirReply : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector                                   (typeof(MkdirReply), "status", typeof(Status)),
                VoidReflector.ReflectorsArray,
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, new IReflector[] {
                    new ClassFieldReflectors<OptionalFileHandle>    (typeof(MkdirReply), "optionalFileHandle", OptionalFileHandle.memberSerializers),
                    new ClassFieldReflectors<OptionalFileAttributes>(typeof(MkdirReply), "optionalAttributes", OptionalFileAttributes.memberSerializers),
                })
            ),
            new ClassFieldReflectors<BeforeAndAfterAttributes>      (typeof(MkdirReply), "parentDirectoryAttributes", BeforeAndAfterAttributes.memberSerializers)
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Status status;
        public OptionalFileHandle optionalFileHandle;
        public OptionalFileAttributes optionalAttributes;
        public BeforeAndAfterAttributes parentDirectoryAttributes;

        public MkdirReply(OptionalFileHandle optionalFileHandle, OptionalFileAttributes optionalAttributes, BeforeAndAfterAttributes parentDirectoryAttributes)
        {
            this.status = Status.Ok;
            this.optionalFileHandle = optionalFileHandle;
            this.optionalAttributes = optionalAttributes;
            this.parentDirectoryAttributes = parentDirectoryAttributes;

        }
        public MkdirReply(Status status, BeforeAndAfterAttributes parentDirectoryAttributes)
        {
            if (status == Status.Ok)
                throw new InvalidOperationException("WrongConstructor: This constructor is meant for enum values other than Ok");
            this.status = status;
            this.parentDirectoryAttributes = parentDirectoryAttributes;
        }
    }


    //
    // SymLink Procedure
    //
    /*
    public class SymLink : RpcProcedure
    {
        public readonly SymLinkReply reply;

        public SymLink(SymLinkCall call)
            : base("SymLink", (UInt32)Nfs3Command.SYMLINK, call)
        {
            this.reply = new SymLinkReply();
            this.responseSerializer = this.reply;
        }
    }
    */
    public class SymLinkCall : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(SymLinkCall), "linkToHandle", Nfs3.FileHandleMaxSize),
            new XdrStringReflector         (typeof(SymLinkCall), "linkName", UInt32.MaxValue),
            new ClassFieldReflectors<SetAttributesStruct>(typeof(SymLinkCall), "attributes", SetAttributesStruct.memberSerializers),
            new XdrStringReflector         (typeof(SymLinkCall), "data", UInt32.MaxValue),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Byte[] linkToHandle;
        public String linkName;

        public SetAttributesStruct attributes;
        public String data;
        
        public SymLinkCall(Byte[] data, UInt32 offset, UInt32 offsetLimit)
        {
            memberSerializers.Deserialize(this, data, offset, offsetLimit);
        }
        public SymLinkCall(Byte[] linkToHandle, String linkName, SetAttributesStruct attributes, String data)
        {
            this.linkToHandle = linkToHandle;
            this.linkName = linkName;
            this.attributes = attributes;
            this.data = data;
        }
    }
    public class SymLinkReply : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector                                   (typeof(SymLinkReply), "status", typeof(Status)),
                VoidReflector.ReflectorsArray,
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, new IReflector[] {
                    new ClassFieldReflectors<OptionalFileHandle>    (typeof(SymLinkReply), "optionalFileHandle", OptionalFileHandle.memberSerializers),
                    new ClassFieldReflectors<OptionalFileAttributes>(typeof(SymLinkReply), "optionalAttributes", OptionalFileAttributes.memberSerializers),
                })
            ),
            new ClassFieldReflectors<BeforeAndAfterAttributes>      (typeof(SymLinkReply), "attributes", BeforeAndAfterAttributes.memberSerializers)
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Status status;
        public OptionalFileHandle optionalFileHandle;
        public OptionalFileAttributes optionalAttributes;
        public BeforeAndAfterAttributes attributes;

        public SymLinkReply(OptionalFileHandle optionalFileHandle, OptionalFileAttributes optionalAttributes, BeforeAndAfterAttributes attributes)
        {
            this.status = Status.Ok;
            this.optionalFileHandle = optionalFileHandle;
            this.optionalAttributes = optionalAttributes;
            this.attributes = attributes;

        }
        public SymLinkReply(Status status, BeforeAndAfterAttributes attributes)
        {
            if (status == Status.Ok)
                throw new InvalidOperationException("WrongConstructor: This constructor is meant for enum values other than Ok");
            this.status = status;
            this.attributes = attributes;
        }
    }
    //
    // Remove Procedure
    //
    /*
    public class Remove : RpcProcedure
    {
        public readonly RemoveReply reply;

        public Remove(RemoveCall call)
            : base("Remove", (UInt32)Nfs3Command.REMOVE, call)
        {
            this.reply = new RemoveReply();
            this.responseSerializer = this.reply;
        }
    }
    */
    public class RemoveCall : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(RemoveCall), "directoryHandle", Nfs3.FileHandleMaxSize),
            new XdrStringReflector         (typeof(RemoveCall), "fileName", UInt32.MaxValue),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Byte[] directoryHandle;
        public String fileName;
        
        public RemoveCall(Byte[] data, UInt32 offset, UInt32 offsetLimit)
        {
            memberSerializers.Deserialize(this, data, offset, offsetLimit);
        }
        public RemoveCall(Byte[] directoryHandle, String fileName)
        {
            this.directoryHandle = directoryHandle;
            this.fileName = fileName;
        }
    }
    public class RemoveReply : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrEnumReflector                                 (typeof(RemoveReply), "status", typeof(Status)),
            new ClassFieldReflectors<BeforeAndAfterAttributes>(typeof(RemoveReply), "directoryAttributes", BeforeAndAfterAttributes.memberSerializers)
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Status status;
        public BeforeAndAfterAttributes directoryAttributes;

        public RemoveReply(Status status, BeforeAndAfterAttributes directoryAttributes)
        {
            this.status = status;
            this.directoryAttributes = directoryAttributes;
        }
    }


    //
    // Rmdir Procedure
    //
    /*
    public class Rmdir : RpcProcedure
    {
        public readonly RemoveReply reply;

        public Rmdir(RemoveCall call)
            : base("Rmdir", (UInt32)Nfs3Command.RMDIR, call)
        {
            this.reply = new RemoveReply();
            this.responseSerializer = this.reply;
        }
    }
    */
    //RmdirCall and RmdirReply are same as Remove


    //
    // Rename Procedure
    //
    /*
    public class Rename : RpcProcedure
    {
        public readonly RenameReply reply;

        public Rename(RenameCall call)
            : base("Rename", (UInt32)Nfs3Command.RENAME, call)
        {
            this.reply = new RenameReply();
            this.responseSerializer = this.reply;
        }
    }
    */
    public class RenameCall : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(RenameCall), "oldDirectoryHandle", Nfs3.FileHandleMaxSize),
            new XdrStringReflector         (typeof(RenameCall), "oldName", UInt32.MaxValue),
            new XdrOpaqueVarLengthReflector(typeof(RenameCall), "newDirectoryHandle", Nfs3.FileHandleMaxSize),
            new XdrStringReflector         (typeof(RenameCall), "newName", UInt32.MaxValue),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Byte[] oldDirectoryHandle;
        public String oldName;

        public Byte[] newDirectoryHandle;
        public String newName;
        
        public RenameCall(Byte[] data, UInt32 offset, UInt32 offsetLimit)
        {
            memberSerializers.Deserialize(this, data, offset, offsetLimit);
        }
        public RenameCall(Byte[] oldDirectoryHandle, String oldName, Byte[] newDirectoryHandle, String newName)
        {
            this.oldDirectoryHandle = oldDirectoryHandle;
            this.oldName = oldName;
            this.newDirectoryHandle = newDirectoryHandle;
            this.newName = newName;
        }
    }
    public class RenameReply : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrEnumReflector                                 (typeof(RenameReply), "status", typeof(Status)),
            new ClassFieldReflectors<BeforeAndAfterAttributes>(typeof(RenameReply), "oldDirectoryAttributes", BeforeAndAfterAttributes.memberSerializers),
            new ClassFieldReflectors<BeforeAndAfterAttributes>(typeof(RenameReply), "newDirectoryAttributes", BeforeAndAfterAttributes.memberSerializers),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Status status;
        public BeforeAndAfterAttributes oldDirectoryAttributes;
        public BeforeAndAfterAttributes newDirectoryAttributes;

        public RenameReply(Status status, BeforeAndAfterAttributes oldDirectoryAttributes, BeforeAndAfterAttributes newDirectoryAttributes)
        {
            this.status = status;
            this.oldDirectoryAttributes = oldDirectoryAttributes;
            this.newDirectoryAttributes = newDirectoryAttributes;
        }
    }
    //
    // ReadDirPlus Procedure
    //
    public class ReadDirPlus : RpcProcedure
    {
        public readonly ReadDirPlusReply reply;

        public ReadDirPlus(ReadDirPlusCall call)
            : base("ReadDirPlus", (UInt32)Nfs3Command.READDIRPLUS, call.CreateSerializer())
        {
            this.reply = (ReadDirPlusReply)FormatterServices.GetUninitializedObject(typeof(ReadDirPlusReply));
            this.responseSerializer = this.reply.CreateSerializer();
        }
    }
    public class ReadDirPlusCall : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrOpaqueVarLengthReflector  (typeof(ReadDirPlusCall), "directoryHandle", Nfs3.FileHandleMaxSize),
            new BigEndianUInt64Reflector           (typeof(ReadDirPlusCall), "cookie"),
            new XdrOpaqueFixedLengthReflector(typeof(ReadDirPlusCall), "cookieVerifier", Nfs3.CookieVerifierSize),
            new BigEndianUInt32Reflector           (typeof(ReadDirPlusCall), "maxDirectoryBytes"),
            new BigEndianUInt32Reflector(typeof(ReadDirPlusCall), "maxRpcReplyMessageBytes"),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Byte[] directoryHandle;
        public Cookie cookie;
        public Byte[] cookieVerifier;
        public Count maxDirectoryBytes;
        public Count maxRpcReplyMessageBytes;
        
        public ReadDirPlusCall(Byte[] data, UInt32 offset, UInt32 offsetLimit)
        {
            memberSerializers.Deserialize(this, data, offset, offsetLimit);
        }
        public ReadDirPlusCall(Byte[] directoryHandle, Cookie cookie, Byte[] cookieVerifier, Count maxDirectoryBytes, Count maxRpcReplyMessageBytes)
        {
            this.directoryHandle = directoryHandle;
            this.cookie = cookie;
            this.cookieVerifier = cookieVerifier;
            this.maxDirectoryBytes = maxDirectoryBytes;
            this.maxRpcReplyMessageBytes = maxRpcReplyMessageBytes;
        }
    }
    public class EntryPlus : ISerializerCreator
    {
        static IReflector NextEntryReflectorCreator(Reflectors reflectorsReference)
        {
            return new XdrBooleanDescriminateReflector(

                new XdrBooleanReflector(typeof(EntryPlus), "nextEntryIncluded"),

                new Reflectors(new ClassFieldReflectors<EntryPlus>(typeof(EntryPlus), "nextEntry", reflectorsReference)),

                VoidReflector.Reflectors);
        }

        //
        // These serializers have to be created weirdly because there are some circular references in it
        //
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new BigEndianUInt64Reflector                    (typeof(EntryPlus), "fileID"),
            new XdrStringReflector                          (typeof(EntryPlus), "fileName", UInt32.MaxValue),
            new BigEndianUInt64Reflector                    (typeof(EntryPlus), "cookie"),
            new ClassFieldReflectors<OptionalFileAttributes>(typeof(EntryPlus), "optionalAttributes", OptionalFileAttributes.memberSerializers),
            new ClassFieldReflectors<OptionalFileHandle>    (typeof(EntryPlus), "optionalHandle", OptionalFileHandle.memberSerializers),
            null // Placeholder
        },
            NextEntryReflectorCreator);

        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public FileID fileID;
        public String fileName;
        public Cookie cookie;
        public OptionalFileAttributes optionalAttributes;
        public OptionalFileHandle optionalHandle;
        Boolean nextEntryIncluded;
        EntryPlus nextEntry;

        //public EntryPlus() { }
        public EntryPlus(Byte[] data, UInt32 offset, UInt32 offsetLimit)
        {
            memberSerializers.Deserialize(this, data, offset, offsetLimit);
        }
        public EntryPlus(
            FileID fileID,
            String fileName,
            Cookie cookie,
            OptionalFileAttributes optionalAttributes,
            OptionalFileHandle optionalHandle)
        {
            this.fileID = fileID;
            this.fileName = fileName;
            this.cookie = cookie;
            this.optionalAttributes = optionalAttributes;
            this.optionalHandle = optionalHandle;
            this.nextEntryIncluded = false;
        }
        public EntryPlus NextEntry { get { return nextEntry; } }
        public void SetNextEntry(EntryPlus nextEntry)
        {
            if (nextEntry == null)
            {
                this.nextEntry = null;
                this.nextEntryIncluded = false;
            }
            else
            {
                this.nextEntry = nextEntry;
                this.nextEntryIncluded = true;
            }
        }
    }
    public class ReadDirPlusReply : ISerializerCreator
    {
        static readonly IReflector[] okSerializers = new IReflector[] {
            new ClassFieldReflectors<OptionalFileAttributes>(typeof(ReadDirPlusReply), "optionalDirectoryAttributes", OptionalFileAttributes.memberSerializers),
            new XdrOpaqueFixedLengthReflector                  (typeof(ReadDirPlusReply), "cookieVerifier", Nfs3.CookieVerifierSize),
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector(typeof(ReadDirPlusReply), "entriesIncluded"),
                new Reflectors(new IReflector[] {
                    new ClassFieldReflectors<EntryPlus>(typeof(ReadDirPlusReply), "entry", EntryPlus.memberSerializers),
                    new XdrBooleanReflector               (typeof(ReadDirPlusReply), "endOfEntries")}),
                VoidReflector.Reflectors),
        };

        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector(typeof(ReadDirPlusReply), "status", typeof(Status)),
                new IReflector[] {new ClassFieldReflectors<OptionalFileAttributes>(typeof(ReadDirPlusReply), "optionalDirectoryAttributes", OptionalFileAttributes.memberSerializers)}, 
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, okSerializers)
            ),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Status status;
        public OptionalFileAttributes optionalDirectoryAttributes;
        public Byte[] cookieVerifier;
        public Boolean entriesIncluded;
        public EntryPlus entry;
        public Boolean endOfEntries;

        public ReadDirPlusReply(OptionalFileAttributes optionalDirectoryAttributes, Byte[] cookieVerifier, EntryPlus entry, Boolean endOfEntries)
        {
            this.status = Status.Ok;
            this.optionalDirectoryAttributes = optionalDirectoryAttributes;
            this.cookieVerifier = cookieVerifier;
            this.entriesIncluded = (entry == null) ? false : true;
            this.entry = entry;
            this.endOfEntries = endOfEntries;
        }
        public ReadDirPlusReply(Status status, OptionalFileAttributes optionalDirectoryAttributes)
        {
            if (status == Status.Ok)
                throw new InvalidOperationException("WrongConstructor: This constructor is meant for enum values other than Ok");
            this.status = status;
            this.optionalDirectoryAttributes = optionalDirectoryAttributes;
        }
    }


    //
    // FileSystemStatus
    //
    /*
    public class FileSystemStatus : RpcProcedure
    {
        public readonly FileSystemStatusReply reply;

        public FileSystemStatus(FileSystemStatusCall call)
            : base("FileSystemStatus", (UInt32)Nfs3Command.FSSTAT, call)
        {
            this.reply = new FileSystemStatusReply();
            this.responseSerializer = this.reply;
        }
    }
    */
    public class FileSystemStatusCall : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(FileSystemStatusCall), "fileSystemRoot", Nfs3.FileHandleMaxSize),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Byte[] fileSystemRoot;

        public FileSystemStatusCall(Byte[] data, UInt32 offset, UInt32 offsetLimit)
        {
            memberSerializers.Deserialize(this, data, offset, offsetLimit);
        }
        public FileSystemStatusCall(Byte[] fileSystemRoot)
        {
            this.fileSystemRoot = fileSystemRoot;
        }
    }
    public class FileSystemStatusReply : ISerializerCreator
    {
        public static readonly IReflector[] okSerializers = new IReflector[] {
            new ClassFieldReflectors<OptionalFileAttributes>(typeof(FileSystemStatusReply), "optionalFileAttributes", OptionalFileAttributes.memberSerializers),
            new BigEndianUInt64Reflector(typeof(FileSystemStatusReply), "totalBytes"),
            new BigEndianUInt64Reflector(typeof(FileSystemStatusReply), "freeBytes"),
            new BigEndianUInt64Reflector(typeof(FileSystemStatusReply), "availableBytes"),
            new BigEndianUInt64Reflector(typeof(FileSystemStatusReply), "totalFileSlots"),
            new BigEndianUInt64Reflector(typeof(FileSystemStatusReply), "freeFileSlots"),
            new BigEndianUInt64Reflector(typeof(FileSystemStatusReply), "availableFileSlots"),
            new BigEndianUInt32Reflector(typeof(FileSystemStatusReply), "fileSystemVolatility"),
        };

        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector(typeof(FileSystemStatusReply), "status", typeof(Status)), new IReflector[] {
                new ClassFieldReflectors<OptionalFileAttributes>(typeof(FileSystemStatusReply), "optionalFileAttributes", OptionalFileAttributes.memberSerializers)},
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, okSerializers)
            ),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Status status;
        public OptionalFileAttributes optionalFileAttributes;

        UInt64 totalBytes, freeBytes, availableBytes;
        UInt64 totalFileSlots, freeFileSlots, availableFileSlots;
        UInt32 fileSystemVolatility;

        public FileSystemStatusReply(Status status, OptionalFileAttributes optionalFileAttributes)
        {
            if (status == Status.Ok)
                throw new InvalidOperationException("Wrong Constructor: this constructor is not meant for an Ok status");
            this.status = status;
            this.optionalFileAttributes = optionalFileAttributes;
        }
        public FileSystemStatusReply(
            OptionalFileAttributes optionalFileAttributes,
            UInt64 totalBytes, UInt64 freeBytes, UInt64 availableBytes,
            UInt64 totalFileSlots, UInt64 freeFileSlots, UInt64 availableFileSlots,
            UInt32 fileSystemVolatility)
        {
            this.status = Status.Ok;
            this.optionalFileAttributes = optionalFileAttributes;

            this.totalBytes = totalBytes;
            this.freeBytes = freeBytes;
            this.availableBytes = availableBytes;

            this.totalFileSlots = totalFileSlots;
            this.freeFileSlots = freeFileSlots;
            this.availableFileSlots = availableFileSlots;

            this.fileSystemVolatility = fileSystemVolatility;
        }
    }


    //
    // FileSystemInfo Procedure
    //
    /*
    public class FSInfo : RpcProcedure
    {
        //public readonly FsInfoReply reply;

        public FileSystemInfo(FsInfoCall call)
            : base("FileSystemInfo", (UInt32)Nfs3Command.FSINFO, call)
        {
            //this.reply = new FsInfoReply();
            this.responseSerializer = null; //FsInfoReply.classSerializer;
        }
    }
    */
    public class FSInfoCall : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(FSInfoCall), "handle", Nfs3.FileHandleMaxSize),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Byte[] handle;

        public FSInfoCall(Byte[] data, UInt32 offset, UInt32 offsetLimit)
        {
            memberSerializers.Deserialize(this, data, offset, offsetLimit);
        }
        public FSInfoCall(Byte[] handle)
        {
            this.handle = handle;
        }
    }
    public class FSInfoReply : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector(typeof(FSInfoReply), "status", typeof(Status)), new IReflector[] {
                new ClassFieldReflectors<OptionalFileAttributes>(typeof(FSInfoReply), "optionalFileAttributes", OptionalFileAttributes.memberSerializers)},
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, new IReflector[] {
                    new ClassFieldReflectors<OptionalFileAttributes>(typeof(FSInfoReply), "optionalFileAttributes", OptionalFileAttributes.memberSerializers),
                    new BigEndianUInt32Reflector(typeof(FSInfoReply), "readSizeMax"),
                    new BigEndianUInt32Reflector(typeof(FSInfoReply), "readSizePreference"),
                    new BigEndianUInt32Reflector(typeof(FSInfoReply), "readSizeMultiple"),
                    new BigEndianUInt32Reflector(typeof(FSInfoReply), "writeSizeMax"),
                    new BigEndianUInt32Reflector(typeof(FSInfoReply), "writeSizePreference"),
                    new BigEndianUInt32Reflector(typeof(FSInfoReply), "writeSizeMultiple"),
                    new BigEndianUInt32Reflector(typeof(FSInfoReply), "readDirSizePreference"),
                    new BigEndianUInt64Reflector(typeof(FSInfoReply), "maxFileSize"),
                    new BigEndianUInt32Reflector(typeof(FSInfoReply), "serverTimeGranularitySeconds"),
                    new BigEndianUInt32Reflector(typeof(FSInfoReply), "serverTimeGranularityNanoSeconds"),
                    new XdrEnumReflector  (typeof(FSInfoReply), "fileProperties", typeof(FileProperties)),
                })
            ),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Status status;
        public OptionalFileAttributes optionalFileAttributes;

        public UInt32 readSizeMax;
        public UInt32 readSizePreference;
        public UInt32 readSizeMultiple;

        public UInt32 writeSizeMax;
        public UInt32 writeSizePreference;
        public UInt32 writeSizeMultiple;

        public UInt32 readDirSizePreference;

        public UInt64 maxFileSize;

        public UInt32 serverTimeGranularitySeconds;
        public UInt32 serverTimeGranularityNanoSeconds;

        public FileProperties fileProperties;

        public FSInfoReply(Status status, OptionalFileAttributes optionalFileAttributes)
        {
            if (status == Status.Ok)
                throw new InvalidOperationException("Wrong Constructor: this constructor is not meant for an Ok status");
            this.status = status;
            this.optionalFileAttributes = optionalFileAttributes;
        }
        public FSInfoReply(
            OptionalFileAttributes optionalFileAttributes, 
            UInt32 readSizeMax, UInt32 readSizePreference, UInt32 readSizeMultiple,
            UInt32 writeSizeMax, UInt32 writeSizePreference, UInt32 writeSizeMultiple,
            UInt32 readDirSizePreference, UInt64 maxFileSize,
            UInt32 serverTimeGranularitySeconds, UInt32 serverTimeGranularityNanoSeconds,
            FileProperties fileProperties)
        {
            this.status = Status.Ok;
            this.optionalFileAttributes = optionalFileAttributes;
            this.readSizeMax = readSizeMax;
            this.readSizePreference = readSizePreference;
            this.readSizeMultiple = readSizeMultiple;

            this.writeSizeMax = writeSizeMax;
            this.writeSizePreference = writeSizePreference;
            this.writeSizeMultiple = writeSizeMultiple;

            this.readDirSizePreference = readDirSizePreference;

            this.maxFileSize = maxFileSize;

            this.serverTimeGranularitySeconds = serverTimeGranularitySeconds;
            this.serverTimeGranularityNanoSeconds = serverTimeGranularityNanoSeconds;

            this.fileProperties = fileProperties;
        }
    }

    //
    // Commit Procedure
    //
    /*
    public class Commit : RpcProcedure
    {
        public readonly CommitReply reply;

        public Commit(CommitCall call)
            : base("Commit", (UInt32)Nfs3Command.COMMIT, call)
        {
            this.reply = new CommitReply();
            this.responseSerializer = this.reply;
        }
    }
    */
    public class CommitCall : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(CommitCall), "fileHandle", Nfs3.FileHandleMaxSize),
            new BigEndianUInt64Reflector         (typeof(CommitCall), "offset"),
            new BigEndianUInt32Reflector         (typeof(CommitCall), "count"),
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        public Byte[] fileHandle;
        public UInt64 offset;
        public UInt32 count;

        public CommitCall(Byte[] data, UInt32 offset, UInt32 offsetLimit)
        {
            memberSerializers.Deserialize(this, data, offset, offsetLimit);
        }
        public CommitCall(Byte[] fileHandle, UInt64 offset, UInt32 count)
        {
            this.fileHandle = fileHandle;
            this.offset = offset;
            this.count = count;
        }
    }
    public class CommitReply : ISerializerCreator
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector(typeof(CommitReply), "status", typeof(Status)), new IReflector[] {
                    new ClassFieldReflectors<BeforeAndAfterAttributes>(typeof(CommitReply), "beforeAndAfter", BeforeAndAfterAttributes.memberSerializers),
                }, new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, new IReflector[] {
                    new ClassFieldReflectors<BeforeAndAfterAttributes>(typeof(CommitReply), "beforeAndAfter", BeforeAndAfterAttributes.memberSerializers),
                    new XdrOpaqueFixedLengthReflector                    (typeof(CommitReply), "writeVerifier", Nfs3.WriteVerifierSize),
                }))
        });
        public ISerializer CreateSerializer() { return new SerializerFromObjectAndReflectors(this, memberSerializers); }

        Status status;
        BeforeAndAfterAttributes beforeAndAfter;
        byte[] writeVerifier;

        public CommitReply(Status status, BeforeAndAfterAttributes beforeAndAfter)
        {
            if (status == Status.Ok)
                throw new InvalidOperationException("Wrong Constructor: this constructor is not meant for an Ok status");
            this.status = status;
            this.beforeAndAfter = beforeAndAfter;
        }
        public CommitReply(BeforeAndAfterAttributes beforeAndAfter, Byte[] writeVerifier)
        {
            this.status = Status.Ok;
            this.beforeAndAfter = beforeAndAfter;
            this.writeVerifier = writeVerifier;
        }
    }
}
