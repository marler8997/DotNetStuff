using System;
using System.Collections.Generic;

namespace Marler.NetworkTools
{
    public static class Mount
    {
        public const UInt32 ProgramNumber = 100005;
    }

    public static class Mount1
    {
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
                    programHeader = new RpcProgramHeader(RpcVersion.Two, Mount.ProgramNumber, 1);
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
                        VoidReflectorSerializer.Instance, VoidReflectorSerializer.Instance);
                    procedures[MNT   ] = new RpcProcedure("Mount"    , MNT   ,
                        Mount1Procedure.MountCall.objectSerializer, Mount1Procedure.MountReply.objectSerializer);
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
        //public const Int32 MaxNameLength = 255;
        //public const Int32 FileHandleSize = 32;
    }
}
namespace Marler.NetworkTools.Mount1Procedure
{
    public class Null : RpcProcedure
    {
        public Null()
            : base("Null", Mount1.NULL, VoidSerializableData.Instance, VoidSerializableData.Instance)
        {
        }
    }

    //
    // Unmount Procedure
    //
    public class Unmount : RpcProcedure
    {
        public Unmount(UnmountCall call)
            : base("UMNT", Mount1.UMNT, call, VoidSerializableData.Instance)
        {
        }
    }
    public class UnmountCall : ObjectReflectorSerializer
    {
        public static readonly ISerializableReflector[] memberSerializers = new ISerializableReflector[] {
            new XdrStringReflector(typeof(UnmountCall), "directory", Mount1.MaxPathLength),
        };

        public String directory;

        public UnmountCall()
            : base(memberSerializers)
        {
        }
        public UnmountCall(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
        public UnmountCall(String directory)
            : base(memberSerializers)
        {
            this.directory = directory;
        }
    }
}
