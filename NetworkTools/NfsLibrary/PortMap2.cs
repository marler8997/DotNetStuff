using System;

namespace Marler.NetworkTools
{
    public static class PortMap2
    {
        public const UInt32 ProgramNumber = 100000;

        public const UInt32 NULL    = 0;
        public const UInt32 GETPORT = 3;
        public const UInt32 ProcedureNumberLimit = 4;

        private static RpcProgramHeader programHeader = null;
        public static RpcProgramHeader ProgramHeader
        {
            get
            {
                if (programHeader == null)
                {
                    programHeader = new RpcProgramHeader(RpcVersion.Two, ProgramNumber, 2);
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
                if (procedureMap == null)
                {
                    RpcProcedure[] procedures = new RpcProcedure[ProcedureNumberLimit];

                    procedures[NULL] = new RpcProcedure("Null", NULL,
                        VoidReflectorSerializer.Instance, VoidReflectorSerializer.Instance);
                    procedures[GETPORT] = new RpcProcedure("GetPort", GETPORT,
                        PortMap2Procedure.GetPortCall.memberSerializers, PortMap2Procedure.GetPortReply.memberSerializers);

                    procedureMap = new RpcProcedureArrayMap(procedures);
                }
                return procedureMap;
            }
        }
        */
    }
}
namespace Marler.NetworkTools.PortMap2Procedure
{
    public class Null : RpcProcedure
    {
        public Null()
            : base("Null", PortMap2.NULL, VoidSerializableData.Instance, VoidSerializableData.Instance)
        {
        }
    }
    public class GetPort : RpcProcedure
    {
        public readonly GetPortReply reply;

        public GetPort(GetPortCall call)
            : base("GetPort", PortMap2.GETPORT, call)
        {
            this.reply = new GetPortReply();
            this.responseSerializer = this.reply;
        }
    }
    public class GetPortCall : ObjectReflectorSerializer
    {
        public static readonly ISerializableReflector[] memberSerializers = new ISerializableReflector[] {
            new XdrUInt32Reflector(typeof(GetPortCall), "program"),
            new XdrUInt32Reflector(typeof(GetPortCall), "programVersion"),
            new XdrUInt32Reflector(typeof(GetPortCall), "transportProtocol"),
            new XdrUInt32Reflector(typeof(GetPortCall), "port"),
        };

        public UInt32 program;
        public UInt32 programVersion;
        public UInt32 transportProtocol;
        public UInt32 port;
        public GetPortCall(UInt32 program, UInt32 programVersion, UInt32 transportProtocol, UInt32 port)
            : base(memberSerializers)
        {
            this.program = program;
            this.programVersion = programVersion;
            this.transportProtocol = transportProtocol;
            this.port = port;
        }
        public GetPortCall(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
    }
    public class GetPortReply : ObjectReflectorSerializer
    {
        public static readonly ISerializableReflector[] memberSerializers = new ISerializableReflector[] {
            new XdrUInt32Reflector(typeof(GetPortReply), "port"),
        };

        public UInt32 port;
        public GetPortReply()
            : base(memberSerializers)
        {
        }
        public GetPortReply(UInt32 program, UInt32 programVersion, UInt32 transportProtocol, UInt32 port)
            : base(memberSerializers)
        {
            this.port = port;
        }
        public GetPortReply(Byte[] data, Int32 offset, Int32 maxOffset)
            : base(memberSerializers)
        {
            Deserialize(data, offset, maxOffset);
        }
    }
}
