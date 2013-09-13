using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using More;
using More.Net.PortMap2Procedure;

namespace More.Net
{
#if !WindowsCE
    [NpcInterface]
#endif
    public interface IPortMap2Handler
    {
        GetPortReply GETPORT(GetPortCall getPortCall);
        DumpReply DUMP();
    }
    class PortMap2Server : RpcServerHandler, IPortMap2Handler
    {
        private readonly RpcServicesManager servicesManager;
        readonly NamedMapping[] namedMappings;

        public PortMap2Server(RpcServicesManager servicesManager, NamedMapping[] namedMappings, ByteBuffer sendBuffer)
            : base("PortMap2", sendBuffer)
        {
            this.servicesManager = servicesManager;
            this.namedMappings = namedMappings;
        }
        public override Boolean ProgramHeaderSupported(RpcProgramHeader programHeader)
        {
            if (programHeader.program != PortMap.ProgramNumber || programHeader.programVersion != PortMap2.ProgramVersion)
            {
                if (NfsServerLog.warningLogger != null) NfsServerLog.warningLogger.WriteLine(
                     "[{0}] [WARNING] Received RPC call for PortMap program {1} version {2}, but only program {3} version {4} are supported",
                     serviceName, programHeader.program, programHeader.programVersion, PortMap2.ProgramHeader, PortMap2.ProgramVersion);
                return false;
            }
            return true;
        }
        public override RpcReply Call(String clientString, RpcCall call, Byte[] callParameters, UInt32 callOffset, UInt32 callMaxOffset, out ISerializer replyParameters)
        {
            ISerializer callData;
            replyParameters = VoidSerializer.Instance;

            switch (call.procedure)
            {
                case PortMap2.NULL:
                    callData = VoidSerializer.Instance;
                    break;
                case PortMap2.GETPORT:

                    GetPortCall getPortCall = new GetPortCall(callParameters, callOffset, callMaxOffset);
                    callData = getPortCall.CreateSerializer();

                    replyParameters = GETPORT(getPortCall).CreateSerializer();

                    break;
                case PortMap2.DUMP:

                    callData = VoidSerializer.Instance;
                    replyParameters = DUMP().CreateSerializer();

                    break;
                case PortMap2.CALLIT:

                    // Not yet implemented
                    return null;

                default:
                    if (NfsServerLog.warningLogger != null)
                        NfsServerLog.warningLogger.WriteLine("[{0}] [Warning] client '{1}' sent unknown procedure number {2}", serviceName, clientString, call.procedure);
                    return new RpcReply(RpcVerifier.None, RpcAcceptStatus.ProcedureUnavailable);
            }

            if (NfsServerLog.rpcCallLogger != null)
                NfsServerLog.rpcCallLogger.WriteLine("[{0}] Rpc {1} => {2}", serviceName, ISerializerString.DataSmallString(callData), ISerializerString.DataSmallString(replyParameters));
            return new RpcReply(RpcVerifier.None);
        }

        public GetPortReply GETPORT(GetPortCall getPortCall)
        {
            GetPortReply getPortReply = new GetPortReply(getPortCall.program, getPortCall.programVersion, getPortCall.transportProtocol, 0);

            String matchingProgramName = null;
            Boolean foundMatchingProgramVersion = false;

            for (int i = 0; i < namedMappings.Length; i++)
            {
                NamedMapping namedMapping = namedMappings[i];
                Mapping mapping = namedMapping.mapping;
                if(getPortCall.program == mapping.program)
                {
                    matchingProgramName = namedMapping.programName;

                    if(getPortCall.programVersion == mapping.version)
                    {
                        foundMatchingProgramVersion = true;

                        if (getPortCall.transportProtocol == mapping.protocol)
                        {
                            getPortReply.port = mapping.port;
                        }
                    }
                }
            }

            if (getPortReply.port == 0 && NfsServerLog.warningLogger != null)
            {
                if (matchingProgramName == null)
                {
                    NfsServerLog.warningLogger.WriteLine("[{0}] [Warning] Client requested port for Program {1} but it was not found",
                        serviceName, getPortCall.program);
                }
                else if (!foundMatchingProgramVersion)
                {
                    NfsServerLog.warningLogger.WriteLine("[{0}] [Warning] Client requested port for Program '{1}' ({2}) but version {3} was not found in the mapping list",
                        serviceName, matchingProgramName, getPortCall.program, getPortCall.programVersion);
                }
                else
                {
                    NfsServerLog.warningLogger.WriteLine("[{0}] [Warning] Client requested port for Program '{1}' ({2}) Version {3}, but ip protocol {4} is not supported",
                        serviceName, matchingProgramName, getPortCall.program, getPortCall.programVersion, getPortCall.transportProtocol);
                }
            }

            return getPortReply;
        }
        public DumpReply DUMP()
        {
            MappingEntry previousEntry = null;
            for(int i = 0; i < namedMappings.Length; i++)
            {
                NamedMapping namedMapping = namedMappings[i];
                Mapping mapping = namedMapping.mapping;
                
                MappingEntry entry = new MappingEntry(mapping);
                entry.SetNextMapping(previousEntry);
                previousEntry = entry;
            }

            return new DumpReply(previousEntry);
        }
    }
}
