using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using More;
using More.Net.PortMap2Procedure;

namespace More.Net
{
    [NpcInterface]
    public interface IPortMap2Handler
    {
        GetPortReply Handle(GetPortCall getPortCall);
    }

    class PortMap2Server : RpcServerHandler, IPortMap2Handler
    {
        private readonly RpcServicesManager servicesManager;
        public readonly Int32 mountPort;
        public readonly Int32 nfsPort;

        public PortMap2Server(RpcServicesManager servicesManager, ByteBuffer sendBuffer, Int32 mountPort, Int32 nfsPort)
            : base("PortMap2", sendBuffer)
        {
            this.servicesManager = servicesManager;
            this.mountPort = mountPort;
            this.nfsPort = nfsPort;
        }
        public override Boolean ProgramHeaderSupported(RpcProgramHeader programHeader)
        {
            return programHeader.program == PortMap2.ProgramNumber && programHeader.programVersion == 2;
        }
        public override RpcReply Call(String clientString, RpcCall call, Byte[] callParameters, Int32 callOffset, Int32 callMaxOffset, out ISerializer replyParameters)
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
                    replyParameters = Handle(getPortCall);
                    callData = getPortCall;

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
                NfsServerLog.rpcCallLogger.WriteLine("[{0}] Rpc {1} => {2}", serviceName, callData.DataSmallString(), replyParameters.DataSmallString());
            return new RpcReply(RpcVerifier.None);
        }

        public GetPortReply Handle(GetPortCall getPortCall)
        {
            GetPortReply getPortReply = new GetPortReply(getPortCall.program, getPortCall.programVersion, getPortCall.transportProtocol, 0);

            // For now only support tcp
            if (getPortCall.transportProtocol == 6)
            {

                switch (getPortCall.program)
                {
                    case Mount3.ProgramNumber:

                        if (getPortCall.programVersion != 3 && getPortCall.programVersion != 1)
                        {
                            if (NfsServerLog.warningLogger != null)
                                NfsServerLog.warningLogger.WriteLine("[{0}] [Warning] Client requested port for MOUNT version {0} but only version 1 and 3 are supported",
                                getPortCall.programVersion);
                            break;
                        }

                        getPortReply.port = (UInt32)mountPort;
                        break;
                    case Nfs3.ProgramNumber:

                        if (getPortCall.programVersion != 3)
                        {
                            if (NfsServerLog.warningLogger != null)
                                NfsServerLog.warningLogger.WriteLine("[{0}] [Warning] Client requested port for MOUNT version {0} but only version {1} is supported",
                                getPortCall.programVersion, 3);
                            break;
                        }

                        getPortReply.port = (UInt32)nfsPort;
                        break;
                    default:
                        if (NfsServerLog.warningLogger != null)
                            NfsServerLog.warningLogger.WriteLine("[{0}] [Warning] Client requested port for unknown program {0}", getPortCall.program);
                        break;
                }

            }


            return getPortReply;
        }
    }
}
