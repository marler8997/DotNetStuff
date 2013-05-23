//
// http://www.ietf.org/rfc/rfc1813.txt
//

using System;

using Marler.Common;

using FileID   = System.UInt64;
using Uid      = System.UInt32;
using Gid      = System.UInt32;
using Size     = System.UInt64;
using Offset   = System.UInt64;
using Count    = System.UInt32;
using Cookie   = System.UInt64;
using FileName = System.String;

namespace Marler.Net
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

        public const Int32 FileHandleMaxSize  = 64;
        public const Int32 CookieVerifierSize =  8;
        public const Int32 CreateVerifierSize =  8;
        public const Int32 WriteVerifierSize  =  8;
    }
}    
namespace Marler.Net.Nfs3Procedure
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
    public class Time : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[]{
            new XdrUInt32Reflector(typeof(Time), "seconds"),
            new XdrUInt32Reflector(typeof(Time), "nanoseconds"),
        };

        public UInt32 seconds, nanoseconds;

        public Time()
            : base(memberSerializers)
        {
        }
        public Time(UInt32 seconds, UInt32 nanoseconds)
            : base(memberSerializers)
        {
            this.seconds = seconds;
            this.nanoseconds = nanoseconds;
        }
    }
    public class SizeAndTimes : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrUInt64Reflector           (typeof(SizeAndTimes), "fileSize"),
            new XdrStructFieldReflector<Time>(typeof(SizeAndTimes), "lastModifyTime", Time.memberSerializers),
            new XdrStructFieldReflector<Time>(typeof(SizeAndTimes), "lastAttributeModifyTime", Time.memberSerializers),
        };

        public Size fileSize;
        public Time lastModifyTime;
        public Time lastAttributeModifyTime;

        public SizeAndTimes()
            : base(memberSerializers)
        {
        }
        public SizeAndTimes(FileAttributes fileAttributes)
        {
            this.fileSize = fileAttributes.fileSize;
            this.lastModifyTime = new Time(fileAttributes.lastModifyTime.seconds, fileAttributes.lastModifyTime.nanoseconds);
            this.lastAttributeModifyTime = new Time(fileAttributes.lastAttributeModifyTime.seconds, fileAttributes.lastAttributeModifyTime.nanoseconds);
        }
    }
    public class FileAttributes : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrEnumReflector  (typeof(FileAttributes), "fileType", typeof(FileType)),
            new XdrEnumReflector  (typeof(FileAttributes), "protectionMode", typeof(ModeFlags)),
            new XdrUInt32Reflector(typeof(FileAttributes), "hardLinks"),
            new XdrUInt32Reflector(typeof(FileAttributes), "ownerUid"),
            new XdrUInt32Reflector(typeof(FileAttributes), "gid"),
            new XdrUInt64Reflector(typeof(FileAttributes), "fileSize"),
            new XdrUInt64Reflector(typeof(FileAttributes), "diskSize"),
            new XdrUInt32Reflector(typeof(FileAttributes), "specialData1"),
            new XdrUInt32Reflector(typeof(FileAttributes), "specialData2"),
            new XdrUInt64Reflector(typeof(FileAttributes), "fileSystemID"),
            new XdrUInt64Reflector(typeof(FileAttributes), "fileID"),
            new XdrStructFieldReflector<Time>(typeof(FileAttributes), "lastAccessTime", Time.memberSerializers),
            new XdrStructFieldReflector<Time>(typeof(FileAttributes), "lastModifyTime", Time.memberSerializers),
            new XdrStructFieldReflector<Time>(typeof(FileAttributes), "lastAttributeModifyTime", Time.memberSerializers),
        };

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
            : base(memberSerializers)
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
            : base(memberSerializers)
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
    public class BeforeAndAfterAttributes : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector                    (typeof(BeforeAndAfterAttributes), "beforeIncluded"),
                new IReflector[] {new XdrStructFieldReflector<SizeAndTimes>  (typeof(BeforeAndAfterAttributes), "before", SizeAndTimes.memberSerializers)},
                VoidReflector.ArrayInstance),
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector                    (typeof(BeforeAndAfterAttributes), "afterIncluded"),
                new IReflector[] {new XdrStructFieldReflector<FileAttributes>(typeof(BeforeAndAfterAttributes), "after", FileAttributes.memberSerializers)},
                VoidReflector.ArrayInstance),            
        };

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

        public BeforeAndAfterAttributes()
            : base(memberSerializers)
        {
        }
        public BeforeAndAfterAttributes(SizeAndTimes before, FileAttributes after)
            : base(memberSerializers)
        {
            this.beforeIncluded = (before == null) ? false : true;
            this.before = before;

            this.afterIncluded = (after == null) ? false : true;
            this.after = after;
        }
    }
    public class OptionalFileAttributes : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector                    (typeof(OptionalFileAttributes), "fileAttributesIncluded"),
                new IReflector[] {new XdrStructFieldReflector<FileAttributes>(typeof(OptionalFileAttributes), "fileAttributes", FileAttributes.memberSerializers)},
                VoidReflector.ArrayInstance
            )
        };

        public static OptionalFileAttributes None = new OptionalFileAttributes();

        public Boolean fileAttributesIncluded;
        public FileAttributes fileAttributes;

        public OptionalFileAttributes()
            : base(memberSerializers)
        {
        }
        public OptionalFileAttributes(FileAttributes fileAttributes)
            : base(memberSerializers)
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
    public class OptionalFileHandle : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector(typeof(OptionalFileHandle), "fileHandleIncluded"),
                new IReflector[] {new XdrOpaqueVarLengthReflector(typeof(OptionalFileHandle), "fileHandle", Nfs3.FileHandleMaxSize)},
                VoidReflector.ArrayInstance
            )
        };

        public static OptionalFileHandle None = new OptionalFileHandle();

        public Boolean fileHandleIncluded;
        public Byte[] fileHandle;

        public OptionalFileHandle()
            : base(memberSerializers)
        {
        }
        public OptionalFileHandle(Byte[] fileHandle)
            : base(memberSerializers)
        {
            if(fileHandle == null) throw new ArgumentNullException("fileHandle");

            this.fileHandleIncluded = true;
            this.fileHandle = fileHandle;
        }
    }
    public class SetAttributesStruct : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector          (typeof(SetAttributesStruct), "setMode"),
                new IReflector[] {new XdrEnumReflector             (typeof(SetAttributesStruct), "mode", typeof(ModeFlags))},
                VoidReflector.ArrayInstance
            ),
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector          (typeof(SetAttributesStruct), "setUid"),
                new IReflector[] {new XdrUInt32Reflector           (typeof(SetAttributesStruct), "uid")},
                VoidReflector.ArrayInstance
            ),
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector          (typeof(SetAttributesStruct), "setGid"),
                new IReflector[] {new XdrUInt32Reflector           (typeof(SetAttributesStruct), "gid")},
                VoidReflector.ArrayInstance
            ),
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector          (typeof(SetAttributesStruct), "setSize"),
                new IReflector[] {new XdrUInt64Reflector           (typeof(SetAttributesStruct), "size")},
                VoidReflector.ArrayInstance
            ),
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector          (typeof(SetAttributesStruct), "setLastAccessTime"),
                new IReflector[] {new XdrStructFieldReflector<Time>(typeof(SetAttributesStruct), "lastAccessTime", Time.memberSerializers)},
                VoidReflector.ArrayInstance
            ),
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector          (typeof(SetAttributesStruct), "setLastModifyTime"),
                new IReflector[] {new XdrStructFieldReflector<Time>(typeof(SetAttributesStruct), "lastModifyTime", Time.memberSerializers)},
                VoidReflector.ArrayInstance
            ),
        };

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
            : base(memberSerializers)
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
    public class GetFileAttributesCall : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(GetFileAttributesCall), "fileHandle", Nfs3.FileHandleMaxSize),
        };
        public Byte[] fileHandle;

        public GetFileAttributesCall()
            : base(memberSerializers)
        {
        }
        public GetFileAttributesCall(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public GetFileAttributesCall(Byte[] fileHandle)
            : base(memberSerializers)
        {
            this.fileHandle = fileHandle;
        }
    }
    public class GetFileAttributesReply : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector(typeof(GetFileAttributesReply), "status", typeof(Status)),
                VoidReflector.ArrayInstance,
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, new IReflector[] {
                    new XdrStructFieldReflector<FileAttributes>(typeof(GetFileAttributesReply), "fileAttributes", FileAttributes.memberSerializers)})
            ),
        };

        public Status status;
        public FileAttributes fileAttributes;

        public GetFileAttributesReply()
            : base(memberSerializers)
        {
        }
        public GetFileAttributesReply(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public GetFileAttributesReply(FileAttributes fileAttributes)
            : base(memberSerializers)
        {
            this.status = Status.Ok;
            this.fileAttributes = fileAttributes;
        }
        public GetFileAttributesReply(Status status)
            : base(memberSerializers)
        {
            if(status == Status.Ok)
                throw new InvalidOperationException("WrongConstructor: This constructor is meant for enum values other than Ok");
            this.status = status;
        }
    }

    //
    // SetFileAttributes Procedure
    //
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
    public class SetFileAttributesCall : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrOpaqueVarLengthReflector                 (typeof(SetFileAttributesCall), "fileHandle", Nfs3.FileHandleMaxSize),
            new XdrStructFieldReflector<SetAttributesStruct>(typeof(SetFileAttributesCall), "setAttributes"),
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector(typeof(SetFileAttributesCall), "checkGuardTime"),
                new IReflector[] {new XdrStructFieldReflector<Time>(typeof(SetFileAttributesCall), "guardTime", Time.memberSerializers)},
                VoidReflector.ArrayInstance),
        };

        public Byte[] fileHandle;
        public SetAttributesStruct setAttributes;

        public Boolean checkGuardTime;
        public Time guardTime;

        public SetFileAttributesCall()
            : base(memberSerializers)
        {
        }
        public SetFileAttributesCall(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public SetFileAttributesCall(Byte[] fileHandle, SetAttributesStruct setAttributes, Time guardTime)
            : base(memberSerializers)
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
    public class SetFileAttributesReply : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrEnumReflector(typeof(SetFileAttributesReply), "status", typeof(Status)),
            new XdrStructFieldReflector<BeforeAndAfterAttributes>(typeof(SetFileAttributesReply), "beforeAndAfter", BeforeAndAfterAttributes.memberSerializers),
        };

        public Status status;
        public BeforeAndAfterAttributes beforeAndAfter;

        public SetFileAttributesReply()
            : base(memberSerializers)
        {
        }
        public SetFileAttributesReply(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public SetFileAttributesReply(Status status, BeforeAndAfterAttributes beforeAndAfter)
            : base(memberSerializers)
        {
            this.status = status;
            this.beforeAndAfter = beforeAndAfter;
        }
    }

    //
    // Lookup Procedure
    //
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
    public class LookupCall : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(LookupCall), "directoryHandle", Nfs3.FileHandleMaxSize),
            new XdrStringReflector         (typeof(LookupCall), "fileName", -1),
        };
        public Byte[] directoryHandle;
        public String fileName;

        public LookupCall()
            : base(memberSerializers)
        {
        }
        public LookupCall(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public LookupCall(Byte[] directoryHandle, String fileName)
            : base(memberSerializers)
        {
            this.directoryHandle = directoryHandle;
            this.fileName = fileName;
        }
    }
    public class LookupReply : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector(typeof(LookupReply), "status", typeof(Status)),
                new IReflector[] {new XdrStructFieldReflector<OptionalFileAttributes>(typeof(LookupReply), "optionalFailAttributes", OptionalFileAttributes.memberSerializers)},
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, new IReflector[] {
                    new XdrOpaqueVarLengthReflector(typeof(LookupReply), "fileHandle", Nfs3.FileHandleMaxSize),
                    new XdrStructFieldReflector<OptionalFileAttributes>(typeof(LookupReply), "optionalFileAttributes", OptionalFileAttributes.memberSerializers),
                    new XdrStructFieldReflector<OptionalFileAttributes>(typeof(LookupReply), "optionalDirectoryAttributes", OptionalFileAttributes.memberSerializers),
                })           
            ),
        };

        public Status status;
        public OptionalFileAttributes optionalFailAttributes;

        public Byte[] fileHandle;
        public OptionalFileAttributes optionalDirectoryAttributes, optionalFileAttributes;

        public LookupReply()
            : base(memberSerializers)
        {
        }
        public LookupReply(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public LookupReply(Status status, OptionalFileAttributes optionalFailAttributes)
            : base(memberSerializers)
        {
            if (status == Status.Ok)
                throw new InvalidOperationException("WrongConstructor: This constructor is meant for enum values other than Ok");

            this.status = status;
            this.optionalFailAttributes = optionalFailAttributes;
        }
        public LookupReply(Byte[] fileHandle, OptionalFileAttributes optionalDirectoryAttributes, OptionalFileAttributes optionalFileAttributes)
            : base(memberSerializers)
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
    public class AccessCall : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(AccessCall), "fileHandle", Nfs3.FileHandleMaxSize),
            new XdrEnumReflector           (typeof(AccessCall), "accessFlags", typeof(AccessFlags)),
        };

        public Byte[] fileHandle;
        public AccessFlags accessFlags;

        public AccessCall()
            : base(memberSerializers)
        {
        }
        public AccessCall(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public AccessCall(Byte[] fileHandle, AccessFlags accessFlags)
            : base(memberSerializers)
        {
            this.fileHandle = fileHandle;
            this.accessFlags = accessFlags;
        }
    }
    public class AccessReply : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector(typeof(AccessReply), "status", typeof(Status)),
                new IReflector[] {new XdrStructFieldReflector<OptionalFileAttributes>(typeof(AccessReply), "optionalFileAttributes", OptionalFileAttributes.memberSerializers)},
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, new IReflector[] {
                    new XdrStructFieldReflector<OptionalFileAttributes>(typeof(AccessReply), "optionalFileAttributes", OptionalFileAttributes.memberSerializers),
                    new XdrEnumReflector(typeof(AccessReply), "accessFlags", typeof(AccessFlags)),
                })
            ),
        };

        public Status status;
        public OptionalFileAttributes optionalFileAttributes;
        public AccessFlags accessFlags;

        public AccessReply()
            : base(memberSerializers)
        {
        }
        public AccessReply(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public AccessReply(OptionalFileAttributes optionalFileAttributes, AccessFlags accessFlags)
            : base(memberSerializers)
        {
            this.status = Status.Ok;
            this.optionalFileAttributes = optionalFileAttributes;
            this.accessFlags = accessFlags;
        }
        public AccessReply(Status status, OptionalFileAttributes optionalFileAttributes)
            : base(memberSerializers)
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
    public class ReadCall : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(ReadCall), "fileHandle", Nfs3.FileHandleMaxSize),
            new XdrUInt64Reflector         (typeof(ReadCall), "offset"),
            new XdrUInt32Reflector         (typeof(ReadCall), "count"),
        };

        public Byte[] fileHandle;
        public Offset offset;
        public Count count;

        public ReadCall()
            : base(memberSerializers)
        {
        }
        public ReadCall(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public ReadCall(Byte[] fileHandle, Offset offset, Count count)
            : base(memberSerializers)
        {
            this.fileHandle = fileHandle;
            this.offset = offset;
            this.count = count;
        }
    }
    public class ReadReply : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector(typeof(ReadReply), "status", typeof(Status)),
                new IReflector[] {new XdrStructFieldReflector<OptionalFileAttributes>(typeof(ReadReply), "optionalFileAttributes", OptionalFileAttributes.memberSerializers)},
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, new IReflector[] {
                    new XdrStructFieldReflector<OptionalFileAttributes>(typeof(ReadReply), "optionalFileAttributes", OptionalFileAttributes.memberSerializers),
                    new XdrUInt32Reflector                             (typeof(ReadReply), "count"),
                    new XdrBooleanReflector                            (typeof(ReadReply), "endOfFile"),
                    new XdrOpaqueVarLengthReflector<PartialByteArraySerializer>(typeof(ReadReply), "fileData", -1),
                })
            ),
        };

        public Status status;
        public OptionalFileAttributes optionalFileAttributes;
        public Count count;
        public Boolean endOfFile;
        public PartialByteArraySerializer fileData;

        public ReadReply()
            : base(memberSerializers)
        {
        }
        public ReadReply(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public ReadReply(OptionalFileAttributes optionalFileAttributes, Count count, Boolean endOfFile, PartialByteArraySerializer fileData)
            : base(memberSerializers)
        {
            this.status = Status.Ok;
            this.optionalFileAttributes = optionalFileAttributes;
            this.count = count;
            this.endOfFile = endOfFile;
            this.fileData = fileData;
        }
        public ReadReply(Status status, OptionalFileAttributes optionalFileAttributes)
            : base(memberSerializers)
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
    public class WriteCall : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(WriteCall), "fileHandle", Nfs3.FileHandleMaxSize),
            new XdrUInt64Reflector         (typeof(WriteCall), "offset"),
            new XdrUInt32Reflector         (typeof(WriteCall), "count"),
            new XdrEnumReflector           (typeof(WriteCall), "stableHow", typeof(StableHowEnum)),
            new XdrOpaqueVarLengthReflector(typeof(WriteCall), "data", -1),
        };

        public Byte[] fileHandle;
        public Offset offset;
        public Count count;
        public StableHowEnum stableHow;
        public Byte[] data;

        public WriteCall()
            : base(memberSerializers)
        {
        }
        public WriteCall(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public WriteCall(Byte[] fileHandle, StableHowEnum stableHow, Byte[] data, Offset offset, Count count)
            : base(memberSerializers)
        {
            this.fileHandle = fileHandle;
            this.data = data;
            this.offset = offset;
            this.count = count;
            this.stableHow = stableHow;
        }
    }
    public class WriteReply : ClassSerializer
    {
        private static readonly XdrStructFieldReflector<BeforeAndAfterAttributes> fileAttributesSerializer =
            new XdrStructFieldReflector<BeforeAndAfterAttributes>(typeof(WriteReply), "fileAttributes", BeforeAndAfterAttributes.memberSerializers);

        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector                             (typeof(WriteReply), "status", typeof(Status)),
                new IReflector[] {fileAttributesSerializer},
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, new IReflector[] {
                    fileAttributesSerializer,
                    new XdrUInt32Reflector                       (typeof(WriteReply), "count"),
                    new XdrEnumReflector                         (typeof(WriteReply), "stableHow", typeof(StableHowEnum)),
                    new XdrOpaqueFixedLengthReflector            (typeof(WriteReply), "writeVerifier", Nfs3.WriteVerifierSize),
                })
            )
        };

        public Status status;
        public BeforeAndAfterAttributes fileAttributes;
        public Count count;
        public StableHowEnum stableHow;
        public Byte[] writeVerifier;

        public WriteReply()
            : base(memberSerializers)
        {
        }
        public WriteReply(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public WriteReply(BeforeAndAfterAttributes fileAttributes, Count count, StableHowEnum stableHow, Byte[] writeVerifier)
            : base(memberSerializers)
        {
            this.status = Status.Ok;
            this.fileAttributes = fileAttributes;
            this.count = count;
            this.stableHow = stableHow;
            this.writeVerifier = writeVerifier;
        }
        public WriteReply(Status status, BeforeAndAfterAttributes fileAttributes)
            : base(memberSerializers)
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
    public class CreateCall : ClassSerializer
    {
        public static XdrStructFieldReflector<SetAttributesStruct> setAttributesSerializer =
            new XdrStructFieldReflector<SetAttributesStruct>(typeof(CreateCall), "setAttributes", SetAttributesStruct.memberSerializers);

        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(CreateCall), "directoryHandle", Nfs3.FileHandleMaxSize),
            new XdrStringReflector         (typeof(CreateCall), "newFileName", -1),
            new XdrDescriminatedUnionReflector<CreateModeEnum>(
                new XdrEnumReflector       (typeof(CreateCall), "mode", typeof(CreateModeEnum)),
                null,
                new XdrDescriminatedUnionReflector<CreateModeEnum>.KeyAndSerializer(CreateModeEnum.Unchecked, new IReflector[] {setAttributesSerializer}),
                new XdrDescriminatedUnionReflector<CreateModeEnum>.KeyAndSerializer(CreateModeEnum.Guarded  , new IReflector[] {setAttributesSerializer}),
                new XdrDescriminatedUnionReflector<CreateModeEnum>.KeyAndSerializer(CreateModeEnum.Exclusive, new IReflector[] {
                    new XdrOpaqueFixedLengthReflector(typeof(CreateCall), "createVerifier", Nfs3.CreateVerifierSize)})
            ),
        };

        public Byte[] directoryHandle;
        public String newFileName;

        public CreateModeEnum mode;
        public SetAttributesStruct setAttributes;
        public Byte[] createVerifier;

        public CreateCall()
            : base(memberSerializers)
        {
        }
        public CreateCall(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public CreateCall(Byte[] directoryHandle, String newFileName, CreateModeEnum mode, SetAttributesStruct setAttributes)
            : base(memberSerializers)
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
    public class CreateReply : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector                                   (typeof(CreateReply), "status", typeof(Status)),
                VoidReflector.ArrayInstance,
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, new IReflector[] {
                    new XdrStructFieldReflector<OptionalFileHandle>    (typeof(CreateReply), "optionalFileHandle", OptionalFileHandle.memberSerializers),
                    new XdrStructFieldReflector<OptionalFileAttributes>(typeof(CreateReply), "optionalFileAttributes", OptionalFileAttributes.memberSerializers),
                })
            ),
            new XdrStructFieldReflector<BeforeAndAfterAttributes>      (typeof(CreateReply), "directoryAttributes", BeforeAndAfterAttributes.memberSerializers)
        };

        public Status status;
        public OptionalFileHandle optionalFileHandle;
        public OptionalFileAttributes optionalFileAttributes;
        public BeforeAndAfterAttributes directoryAttributes;

        public CreateReply()
            : base(memberSerializers)
        {
        }
        public CreateReply(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public CreateReply(OptionalFileHandle optionalFileHandle, OptionalFileAttributes optionalFileAttributes, BeforeAndAfterAttributes directoryAttributes)
            : base(memberSerializers)
        {
            this.status = Status.Ok;
            this.optionalFileHandle = optionalFileHandle;
            this.optionalFileAttributes = optionalFileAttributes;
            this.directoryAttributes = directoryAttributes;

        }
        public CreateReply(Status status, BeforeAndAfterAttributes directoryAttributes)
            : base(memberSerializers)
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
    public class MkdirCall : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(MkdirCall), "directoryHandle", Nfs3.FileHandleMaxSize),
            new XdrStringReflector         (typeof(MkdirCall), "newDirectoryName", -1),
            new XdrStructFieldReflector<SetAttributesStruct>(typeof(MkdirCall), "setAttributes", SetAttributesStruct.memberSerializers),
        };

        public Byte[] directoryHandle;
        public String newDirectoryName;

        public SetAttributesStruct setAttributes;

        public MkdirCall()
            : base(memberSerializers)
        {
        }
        public MkdirCall(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public MkdirCall(Byte[] directoryHandle, String newDirectoryName, SetAttributesStruct setAttributes)
            : base(memberSerializers)
        {
            this.directoryHandle = directoryHandle;
            this.newDirectoryName = newDirectoryName;
            this.setAttributes = setAttributes;
        }
    }
    public class MkdirReply : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector                                   (typeof(MkdirReply), "status", typeof(Status)),
                VoidReflector.ArrayInstance,
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, new IReflector[] {
                    new XdrStructFieldReflector<OptionalFileHandle>    (typeof(MkdirReply), "optionalFileHandle", OptionalFileHandle.memberSerializers),
                    new XdrStructFieldReflector<OptionalFileAttributes>(typeof(MkdirReply), "optionalAttributes", OptionalFileAttributes.memberSerializers),
                })
            ),
            new XdrStructFieldReflector<BeforeAndAfterAttributes>      (typeof(MkdirReply), "parentDirectoryAttributes", BeforeAndAfterAttributes.memberSerializers)
        };

        public Status status;
        public OptionalFileHandle optionalFileHandle;
        public OptionalFileAttributes optionalAttributes;
        public BeforeAndAfterAttributes parentDirectoryAttributes;

        public MkdirReply()
            : base(memberSerializers)
        {
        }
        public MkdirReply(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public MkdirReply(OptionalFileHandle optionalFileHandle, OptionalFileAttributes optionalAttributes, BeforeAndAfterAttributes parentDirectoryAttributes)
            : base(memberSerializers)
        {
            this.status = Status.Ok;
            this.optionalFileHandle = optionalFileHandle;
            this.optionalAttributes = optionalAttributes;
            this.parentDirectoryAttributes = parentDirectoryAttributes;

        }
        public MkdirReply(Status status, BeforeAndAfterAttributes parentDirectoryAttributes)
            : base(memberSerializers)
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
    public class SymLinkCall : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(SymLinkCall), "linkToHandle", Nfs3.FileHandleMaxSize),
            new XdrStringReflector         (typeof(SymLinkCall), "linkName", -1),
            new XdrStructFieldReflector<SetAttributesStruct>(typeof(SymLinkCall), "attributes", SetAttributesStruct.memberSerializers),
            new XdrStringReflector         (typeof(SymLinkCall), "data", -1),
        };

        public Byte[] linkToHandle;
        public String linkName;

        public SetAttributesStruct attributes;
        public String data;

        public SymLinkCall()
            : base(memberSerializers)
        {
        }
        public SymLinkCall(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public SymLinkCall(Byte[] linkToHandle, String linkName, SetAttributesStruct attributes, String data)
            : base(memberSerializers)
        {
            this.linkToHandle = linkToHandle;
            this.linkName = linkName;
            this.attributes = attributes;
            this.data = data;
        }
    }
    public class SymLinkReply : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector                                   (typeof(SymLinkReply), "status", typeof(Status)),
                VoidReflector.ArrayInstance,
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, new IReflector[] {
                    new XdrStructFieldReflector<OptionalFileHandle>    (typeof(SymLinkReply), "optionalFileHandle", OptionalFileHandle.memberSerializers),
                    new XdrStructFieldReflector<OptionalFileAttributes>(typeof(SymLinkReply), "optionalAttributes", OptionalFileAttributes.memberSerializers),
                })
            ),
            new XdrStructFieldReflector<BeforeAndAfterAttributes>      (typeof(SymLinkReply), "attributes", BeforeAndAfterAttributes.memberSerializers)
        };

        public Status status;
        public OptionalFileHandle optionalFileHandle;
        public OptionalFileAttributes optionalAttributes;
        public BeforeAndAfterAttributes attributes;

        public SymLinkReply()
            : base(memberSerializers)
        {
        }
        public SymLinkReply(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public SymLinkReply(OptionalFileHandle optionalFileHandle, OptionalFileAttributes optionalAttributes, BeforeAndAfterAttributes attributes)
            : base(memberSerializers)
        {
            this.status = Status.Ok;
            this.optionalFileHandle = optionalFileHandle;
            this.optionalAttributes = optionalAttributes;
            this.attributes = attributes;

        }
        public SymLinkReply(Status status, BeforeAndAfterAttributes attributes)
            : base(memberSerializers)
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
    public class RemoveCall : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(RemoveCall), "directoryHandle", Nfs3.FileHandleMaxSize),
            new XdrStringReflector         (typeof(RemoveCall), "fileName", -1),
        };

        public Byte[] directoryHandle;
        public String fileName;

        public RemoveCall()
            : base(memberSerializers)
        {
        }
        public RemoveCall(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public RemoveCall(Byte[] directoryHandle, String fileName)
            : base(memberSerializers)
        {
            this.directoryHandle = directoryHandle;
            this.fileName = fileName;
        }
    }
    public class RemoveReply : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrEnumReflector                                 (typeof(RemoveReply), "status", typeof(Status)),
            new XdrStructFieldReflector<BeforeAndAfterAttributes>(typeof(RemoveReply), "directoryAttributes", BeforeAndAfterAttributes.memberSerializers)
        };

        public Status status;
        public BeforeAndAfterAttributes directoryAttributes;

        public RemoveReply()
            : base(memberSerializers)
        {
        }
        public RemoveReply(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public RemoveReply(Status status, BeforeAndAfterAttributes directoryAttributes)
            : base(memberSerializers)
        {
            this.status = status;
            this.directoryAttributes = directoryAttributes;
        }
    }


    //
    // Rmdir Procedure
    //
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
    //RmdirCall and RmdirReply are same as Remove


    //
    // Rename Procedure
    //
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
    public class RenameCall : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(RenameCall), "oldDirectoryHandle", Nfs3.FileHandleMaxSize),
            new XdrStringReflector         (typeof(RenameCall), "oldName", -1),
            new XdrOpaqueVarLengthReflector(typeof(RenameCall), "newDirectoryHandle", Nfs3.FileHandleMaxSize),
            new XdrStringReflector         (typeof(RenameCall), "newName", -1),
        };

        public Byte[] oldDirectoryHandle;
        public String oldName;

        public Byte[] newDirectoryHandle;
        public String newName;

        public RenameCall()
            : base(memberSerializers)
        {
        }
        public RenameCall(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public RenameCall(Byte[] oldDirectoryHandle, String oldName, Byte[] newDirectoryHandle, String newName)
            : base(memberSerializers)
        {
            this.oldDirectoryHandle = oldDirectoryHandle;
            this.oldName = oldName;
            this.newDirectoryHandle = newDirectoryHandle;
            this.newName = newName;
        }
    }
    public class RenameReply : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrEnumReflector                                 (typeof(RenameReply), "status", typeof(Status)),
            new XdrStructFieldReflector<BeforeAndAfterAttributes>(typeof(RenameReply), "oldDirectoryAttributes", BeforeAndAfterAttributes.memberSerializers),
            new XdrStructFieldReflector<BeforeAndAfterAttributes>(typeof(RenameReply), "newDirectoryAttributes", BeforeAndAfterAttributes.memberSerializers),
        };

        public Status status;
        public BeforeAndAfterAttributes oldDirectoryAttributes;
        public BeforeAndAfterAttributes newDirectoryAttributes;

        public RenameReply()
            : base(memberSerializers)
        {
        }
        public RenameReply(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public RenameReply(Status status, BeforeAndAfterAttributes oldDirectoryAttributes, BeforeAndAfterAttributes newDirectoryAttributes)
            : base(memberSerializers)
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
            : base("ReadDirPlus", (UInt32)Nfs3Command.READDIRPLUS, call)
        {
            this.reply = new ReadDirPlusReply();
            this.responseSerializer = this.reply;
        }
    }
    public class ReadDirPlusCall : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrOpaqueVarLengthReflector  (typeof(ReadDirPlusCall), "directoryHandle", Nfs3.FileHandleMaxSize),
            new XdrUInt64Reflector           (typeof(ReadDirPlusCall), "cookie"),
            new XdrOpaqueFixedLengthReflector(typeof(ReadDirPlusCall), "cookieVerifier", Nfs3.CookieVerifierSize),
            new XdrUInt32Reflector           (typeof(ReadDirPlusCall), "maxDirectoryBytes"),
            new XdrUInt32Reflector(typeof(ReadDirPlusCall), "maxRpcReplyMessageBytes"),
        };

        public Byte[] directoryHandle;
        public Cookie cookie;
        public Byte[] cookieVerifier;
        public Count maxDirectoryBytes;
        public Count maxRpcReplyMessageBytes;

        public ReadDirPlusCall()
            : base(memberSerializers)
        {
        }
        public ReadDirPlusCall(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public ReadDirPlusCall(Byte[] directoryHandle, Cookie cookie, Byte[] cookieVerifier, Count maxDirectoryBytes, Count maxRpcReplyMessageBytes)
            : base(memberSerializers)
        {
            this.directoryHandle = directoryHandle;
            this.cookie = cookie;
            this.cookieVerifier = cookieVerifier;
            this.maxDirectoryBytes = maxDirectoryBytes;
            this.maxRpcReplyMessageBytes = maxRpcReplyMessageBytes;
        }
    }
    public class EntryPlus : ClassSerializer
    {
        //
        // These serializers have to be created weirdly because there are some circular references in it
        //
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrUInt64Reflector                             (typeof(EntryPlus), "fileID"),
            new XdrStringReflector                             (typeof(EntryPlus), "fileName", -1),
            new XdrUInt64Reflector                             (typeof(EntryPlus), "cookie"),
            new XdrStructFieldReflector<OptionalFileAttributes>(typeof(EntryPlus), "optionalAttributes", OptionalFileAttributes.memberSerializers),
            new XdrStructFieldReflector<OptionalFileHandle>    (typeof(EntryPlus), "optionalHandle", OptionalFileHandle.memberSerializers),
            null // Placeholder
        };
        private static readonly XdrStructFieldReflector<EntryPlus> nextEntryReflector =
            new XdrStructFieldReflector<EntryPlus>(typeof(EntryPlus), "nextEntry", memberSerializers);
        static EntryPlus()
        {
            memberSerializers[5] = new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector(typeof(EntryPlus), "nextEntryIncluded"),
                new IReflector[] {nextEntryReflector}, VoidReflector.ArrayInstance);
        }

        public FileID fileID;
        public String fileName;
        public Cookie cookie;
        public OptionalFileAttributes optionalAttributes;
        public OptionalFileHandle optionalHandle;
        public Boolean nextEntryIncluded;
        public EntryPlus nextEntry;

        public EntryPlus()
            : base(memberSerializers)
        {
        }
        public EntryPlus(
            FileID fileID,
            String fileName,
            Cookie cookie,
            OptionalFileAttributes optionalAttributes,
            OptionalFileHandle optionalHandle,
            EntryPlus nextEntry)
            : base(memberSerializers)
        {
            this.fileID = fileID;
            this.fileName = fileName;
            this.cookie = cookie;
            this.optionalAttributes = optionalAttributes;
            this.optionalHandle = optionalHandle;
            this.nextEntryIncluded = (nextEntry != null) ? true : false;
            this.nextEntry = nextEntry;
        }
    }
    public class ReadDirPlusReply : ClassSerializer
    {
        public static readonly IReflector[] okSerializers = new IReflector[] {
            new XdrStructFieldReflector<OptionalFileAttributes>(typeof(ReadDirPlusReply), "optionalDirectoryAttributes", OptionalFileAttributes.memberSerializers),
            new XdrOpaqueFixedLengthReflector                  (typeof(ReadDirPlusReply), "cookieVerifier", Nfs3.CookieVerifierSize),
            new XdrBooleanDescriminateReflector(
                new XdrBooleanReflector(typeof(ReadDirPlusReply), "entriesIncluded"),
                new IReflector[] {
                    new XdrStructFieldReflector<EntryPlus>(typeof(ReadDirPlusReply), "entry", EntryPlus.memberSerializers),
                    new XdrBooleanReflector               (typeof(ReadDirPlusReply), "endOfFile")},
                VoidReflector.ArrayInstance),
        };

        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector(typeof(ReadDirPlusReply), "status", typeof(Status)),
                new IReflector[] {new XdrStructFieldReflector<OptionalFileAttributes>(typeof(ReadDirPlusReply), "optionalDirectoryAttributes", OptionalFileAttributes.memberSerializers)}, 
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, okSerializers)
            ),
        };

        public Status status;
        public OptionalFileAttributes optionalDirectoryAttributes;
        public Byte[] cookieVerifier;
        public Boolean entriesIncluded;
        public EntryPlus entry;
        public Boolean endOfFile;

        public ReadDirPlusReply()
            : base(memberSerializers)
        {
        }
        public ReadDirPlusReply(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public ReadDirPlusReply(OptionalFileAttributes optionalDirectoryAttributes, Byte[] cookieVerifier, EntryPlus entry, Boolean endOfFile)
            : base(memberSerializers)
        {
            this.status = Status.Ok;
            this.optionalDirectoryAttributes = optionalDirectoryAttributes;
            this.cookieVerifier = cookieVerifier;
            this.entriesIncluded = (entry == null) ? false : true;
            this.entry = entry;
            this.endOfFile = endOfFile;
        }
        public ReadDirPlusReply(Status status, OptionalFileAttributes optionalDirectoryAttributes)
            : base(memberSerializers)
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
    public class FileSystemStatusCall : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(FileSystemStatusCall), "fileSystemRoot", Nfs3.FileHandleMaxSize),
        };
        public Byte[] fileSystemRoot;

        public FileSystemStatusCall()
            : base(memberSerializers)
        {
        }
        public FileSystemStatusCall(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public FileSystemStatusCall(Byte[] fileSystemRoot)
            : base(memberSerializers)
        {
            this.fileSystemRoot = fileSystemRoot;
        }
    }
    public class FileSystemStatusReply : ClassSerializer
    {
        public static readonly IReflector[] okSerializers = new IReflector[] {
            new XdrStructFieldReflector<OptionalFileAttributes>(typeof(FileSystemStatusReply), "optionalFileAttributes", OptionalFileAttributes.memberSerializers),
            new XdrUInt64Reflector(typeof(FileSystemStatusReply), "totalBytes"),
            new XdrUInt64Reflector(typeof(FileSystemStatusReply), "freeBytes"),
            new XdrUInt64Reflector(typeof(FileSystemStatusReply), "availableBytes"),
            new XdrUInt64Reflector(typeof(FileSystemStatusReply), "totalFileSlots"),
            new XdrUInt64Reflector(typeof(FileSystemStatusReply), "freeFileSlots"),
            new XdrUInt64Reflector(typeof(FileSystemStatusReply), "availableFileSlots"),
            new XdrUInt32Reflector(typeof(FileSystemStatusReply), "fileSystemVolatility"),
        };

        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector(typeof(FileSystemStatusReply), "status", typeof(Status)), new IReflector[] {
                new XdrStructFieldReflector<OptionalFileAttributes>(typeof(FileSystemStatusReply), "optionalFileAttributes", OptionalFileAttributes.memberSerializers)},
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, okSerializers)
            ),
        };

        public Status status;
        public OptionalFileAttributes optionalFileAttributes;

        UInt64 totalBytes, freeBytes, availableBytes;
        UInt64 totalFileSlots, freeFileSlots, availableFileSlots;
        UInt32 fileSystemVolatility;

        public FileSystemStatusReply()
            : base(memberSerializers)
        {
        }
        public FileSystemStatusReply(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public FileSystemStatusReply(Status status, OptionalFileAttributes optionalFileAttributes)
            : base(memberSerializers)
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
            : base(memberSerializers)
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
    public class FileSystemInfo : RpcProcedure
    {
        public readonly FsInfoReply reply;

        public FileSystemInfo(FsInfoCall call)
            : base("FileSystemInfo", (UInt32)Nfs3Command.FSINFO, call)
        {
            this.reply = new FsInfoReply();
            this.responseSerializer = this.reply;
        }
    }
    public class FsInfoCall : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(FsInfoCall), "fileHandle", Nfs3.FileHandleMaxSize),
        };
        public Byte[] fileHandle;

        public FsInfoCall()
            : base(memberSerializers)
        {
        }
        public FsInfoCall(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public FsInfoCall(Byte[] fileHandle)
            : base(memberSerializers)
        {
            this.fileHandle = fileHandle;
        }
    }
    public class FsInfoReply : ClassSerializer
    {
        public static readonly IReflector[] okSerializers = new IReflector[] {
            new XdrStructFieldReflector<OptionalFileAttributes>(typeof(FsInfoReply), "optionalFileAttributes", OptionalFileAttributes.memberSerializers),
            new XdrUInt32Reflector(typeof(FsInfoReply), "readSizeMax"),
            new XdrUInt32Reflector(typeof(FsInfoReply), "readSizePreference"),
            new XdrUInt32Reflector(typeof(FsInfoReply), "readSizeMultiple"),
            new XdrUInt32Reflector(typeof(FsInfoReply), "writeSizeMax"),
            new XdrUInt32Reflector(typeof(FsInfoReply), "writeSizePreference"),
            new XdrUInt32Reflector(typeof(FsInfoReply), "writeSizeMultiple"),
            new XdrUInt32Reflector(typeof(FsInfoReply), "readDirSizePreference"),
            new XdrUInt64Reflector(typeof(FsInfoReply), "maxFileSize"),
            new XdrUInt32Reflector(typeof(FsInfoReply), "serverTimeGranularitySeconds"),
            new XdrUInt32Reflector(typeof(FsInfoReply), "serverTimeGranularityNanoSeconds"),
            new XdrEnumReflector  (typeof(FsInfoReply), "fileProperties", typeof(FileProperties)),
        };

        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector(typeof(FsInfoReply), "status", typeof(Status)), new IReflector[] {
                new XdrStructFieldReflector<OptionalFileAttributes>(typeof(FsInfoReply), "optionalFileAttributes", OptionalFileAttributes.memberSerializers)},
                new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, okSerializers)
            ),
        };

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

        public FsInfoReply()
            : base(memberSerializers)
        {
        }
        public FsInfoReply(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public FsInfoReply(Status status, OptionalFileAttributes optionalFileAttributes)
            : base(memberSerializers)
        {
            if (status == Status.Ok)
                throw new InvalidOperationException("Wrong Constructor: this constructor is not meant for an Ok status");
            this.status = status;
            this.optionalFileAttributes = optionalFileAttributes;
        }
        public FsInfoReply(
            OptionalFileAttributes optionalFileAttributes, 
            UInt32 readSizeMax, UInt32 readSizePreference, UInt32 readSizeMultiple,
            UInt32 writeSizeMax, UInt32 writeSizePreference, UInt32 writeSizeMultiple,
            UInt32 readDirSizePreference, UInt64 maxFileSize,
            UInt32 serverTimeGranularitySeconds, UInt32 serverTimeGranularityNanoSeconds,
            FileProperties fileProperties)
            : base(memberSerializers)
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
    public class CommitCall : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(CommitCall), "fileHandle", Nfs3.FileHandleMaxSize),
            new XdrUInt64Reflector         (typeof(CommitCall), "offset"),
            new XdrUInt32Reflector         (typeof(CommitCall), "count"),
        };

        public Byte[] fileHandle;
        public UInt64 offset;
        public UInt32 count;

        public CommitCall()
            : base(memberSerializers)
        {
        }
        public CommitCall(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public CommitCall(Byte[] fileHandle, UInt64 offset, UInt32 count)
            : base(memberSerializers)
        {
            this.fileHandle = fileHandle;
            this.offset = offset;
            this.count = count;
        }
    }
    public class CommitReply : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrDescriminatedUnionReflector<Status>(
                new XdrEnumReflector(typeof(CommitReply), "status", typeof(Status)), new IReflector[] {
                    new XdrStructFieldReflector<BeforeAndAfterAttributes>(typeof(CommitReply), "beforeAndAfter", BeforeAndAfterAttributes.memberSerializers),
                }, new XdrDescriminatedUnionReflector<Status>.KeyAndSerializer(Status.Ok, new IReflector[] {
                    new XdrStructFieldReflector<BeforeAndAfterAttributes>(typeof(CommitReply), "beforeAndAfter", BeforeAndAfterAttributes.memberSerializers),
                    new XdrOpaqueFixedLengthReflector                    (typeof(CommitReply), "writeVerifier", Nfs3.WriteVerifierSize),
                }))
        };

        Status status;
        BeforeAndAfterAttributes beforeAndAfter;
        byte[] writeVerifier;

        public CommitReply()
            : base(memberSerializers)
        {
        }
        public CommitReply(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public CommitReply(Status status, BeforeAndAfterAttributes beforeAndAfter)
            : base(memberSerializers)
        {
            if (status == Status.Ok)
                throw new InvalidOperationException("Wrong Constructor: this constructor is not meant for an Ok status");
            this.status = status;
            this.beforeAndAfter = beforeAndAfter;
        }
        public CommitReply(BeforeAndAfterAttributes beforeAndAfter, Byte[] writeVerifier)
            : base(memberSerializers)
        {
            this.status = Status.Ok;
            this.beforeAndAfter = beforeAndAfter;
            this.writeVerifier = writeVerifier;
        }
    }
}
