using System;
using System.Collections.Generic;

using More;

namespace More.Net
{
    public static class Mount3
    {
        public const UInt32 ProgramNumber = 100005;

        public const UInt32 NULL   = 0;
        public const UInt32 MNT    = 1;
        public const UInt32 DUMP   = 2;
        public const UInt32 UMNT   = 3;
        public const UInt32 UMTALL = 4;
        public const UInt32 EXPORT = 5;
        public const UInt32 ProcedureNumberLimit = 6;

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

                    procedures[NULL  ] = new RpcProcedure("Null"     , NULL  ,
                        VoidReflector.Instance, VoidReflector.Instance);
                    procedures[MNT   ] = new RpcProcedure("Mount"    , MNT   ,
                        Mount3Procedure.MountCall.objectSerializer, Mount3Procedure.MountReply.objectSerializer);
                    procedures[DUMP  ] = new RpcProcedure("Dump"     , DUMP  ,
                        ReflectorSerializerNotImplemented.Instance, ReflectorSerializerNotImplemented.Instance);
                    procedures[UMNT  ] = new RpcProcedure("Unmount"  , UMNT  ,
                        ReflectorSerializerNotImplemented.Instance, ReflectorSerializerNotImplemented.Instance);
                    procedures[UMTALL] = new RpcProcedure("UmountAll", UMTALL,
                        ReflectorSerializerNotImplemented.Instance, ReflectorSerializerNotImplemented.Instance);
                    procedures[EXPORT] = new RpcProcedure("Export"   , EXPORT,
                        ReflectorSerializerNotImplemented.Instance, ReflectorSerializerNotImplemented.Instance);

                    procedureMap = new RpcProcedureArrayMap(procedures);
                }
                return procedureMap;
            }
        }
        */

        public const Int32 MaxPathLength = 1024;
        public const Int32 MaxNameLength = 255;
        public const Int32 FileHandleSize = 32;
    }
}
namespace More.Net.Mount3Procedure
{
    public class Null : RpcProcedure
    {
        public Null()
            : base("Null", Mount3.NULL, VoidSerializer.Instance, VoidSerializer.Instance)
        {
        }
    }

    //
    // Mount Procedure
    //
    public class Mount : RpcProcedure
    {
        public readonly MountReply reply;

        public Mount(MountCall call)
            : base("MNT", Mount3.MNT, call)
        {
            this.reply = new MountReply();
            this.responseSerializer = this.reply;
        }
    }
    public class MountCall : SubclassSerializer
    {
        public static readonly IReflectors memberSerializers = new IReflectors(new IReflector[] {
            new XdrStringReflector(typeof(MountCall), "directory", Mount3.MaxPathLength),
        });

        public String directory;

        public MountCall()
            : base(memberSerializers)
        {
        }
        public MountCall(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public MountCall(String directory)
            : base(memberSerializers)
        {
            this.directory = directory;
        }
    }
    public class MountReply : SubclassSerializer
    {
        public static readonly IReflector[] mountOkSerializers = new IReflector[] {
            new XdrOpaqueVarLengthReflector(typeof(MountReply), "fileHandle", Mount3.FileHandleSize),
            new XdrVarLengthArray<XdrEnum<RpcAuthenticationFlavor>>(typeof(MountReply), "authenticationFlavors", -1),
        };

        public static readonly IReflectors memberSerializers = new IReflectors(new IReflector[] {
            new XdrDescriminatedUnionReflector<Nfs3Procedure.Status>(
                new XdrEnumReflector(typeof(MountReply), "status", typeof(Nfs3Procedure.Status)),
                VoidReflector.ReflectorsArray,
                new XdrDescriminatedUnionReflector<Nfs3Procedure.Status>.KeyAndSerializer(Nfs3Procedure.Status.Ok, mountOkSerializers)
            ),
        });

        public Nfs3Procedure.Status status;
        public Byte[] fileHandle;
        public XdrEnum<RpcAuthenticationFlavor>[] authenticationFlavors;

        public MountReply()
            : base(memberSerializers)
        {
        }
        public MountReply(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        
        public MountReply(Byte[] fileHandle, XdrEnum<RpcAuthenticationFlavor>[] authenticationFlavors)
            : base(memberSerializers)
        {
            this.status = Nfs3Procedure.Status.Ok;
            this.fileHandle = fileHandle;
            this.authenticationFlavors = authenticationFlavors;
        }
        public MountReply(Nfs3Procedure.Status status)
            : base(memberSerializers)
        {
            if (status == Nfs3Procedure.Status.Ok)
                throw new InvalidOperationException("Wrong Constructor: The MountStatus for this constructor can not be Ok");
            this.status = status;
        }
    }
}
