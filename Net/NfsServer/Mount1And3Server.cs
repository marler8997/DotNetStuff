using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using More;

namespace More.Net
{

#if !WindowsCE
    [NpcInterface]
#endif
    public interface IMount1And3Handler
    {
        Mount3Reply MNT(String directory);
    }
    public class Mount1And3Server : RpcServerHandler, IMount1And3Handler
    {
        private readonly RpcServicesManager servicesManager;
        private readonly SharedFileSystem sharedFileSystem;

        public Mount1And3Server(RpcServicesManager servicesManager, SharedFileSystem sharedFileSystem, ByteBuffer sendBuffer)
            : base("Mount3", sendBuffer)
        {
            this.servicesManager = servicesManager;
            this.sharedFileSystem = sharedFileSystem;
        }
        public override Boolean ProgramHeaderSupported(RpcProgramHeader programHeader)
        {
            return programHeader.program == Mount.ProgramNumber &&
                (programHeader.programVersion == 1 || programHeader.programVersion == 3);
        }
        public override RpcReply Call(String clientString, RpcCall call, Byte[] callParameters, UInt32 callOffset, UInt32 callMaxOffset, out ISerializer replyParameters)
        {
            String methodName;

            ISerializer callData;
            replyParameters = VoidSerializer.Instance;

            switch(call.procedure)
            {
                case Mount.NULL:
                    methodName = "NULL";
                    callData = VoidSerializer.Instance;
                    break;
                case Mount.MNT:
                    methodName = "MNT";

                    MountCall mountCall = new MountCall(callParameters, callOffset, callMaxOffset);
                    callData = mountCall.CreateSerializer();

                    if(call.programHeader.programVersion == 1)
                    {
                        if (NfsServerLog.warningLogger != null)
                            NfsServerLog.warningLogger.WriteLine("[{0}] [Warning] client '{1}' called MNT('{2}') on for MOUNT version 1 but this is not supported",
                                serviceName, clientString, mountCall.directory);
                        return new RpcReply(RpcVerifier.None, RpcAcceptStatus.ProcedureUnavailable);
                    }
                    else
                    {
                        replyParameters = MNT(mountCall.directory).CreateSerializer();
                    }
                    break;
                case Mount.UMNT:
                    methodName = "UMNT";

                    UnmountCall unmountCall = new UnmountCall(callParameters, callOffset, callMaxOffset);
                    callData = unmountCall.CreateSerializer();
                    break;
                    
                default:
                    if (NfsServerLog.warningLogger != null)
                        NfsServerLog.warningLogger.WriteLine("[{0}] [Warning] client '{1}' sent unknown procedure number {2} (mount version {3})", serviceName, clientString, call.procedure, call.programHeader.programVersion);
                    return new RpcReply(RpcVerifier.None, RpcAcceptStatus.ProcedureUnavailable);
            }

            if (NfsServerLog.warningLogger != null)
                NfsServerLog.warningLogger.WriteLine("[{0}] Rpc {1} => {2}", serviceName, ISerializerString.DataSmallString(callData), ISerializerString.DataSmallString(replyParameters));
            return new RpcReply(RpcVerifier.None);
        }

        public Mount3Reply MNT(String directory)
        {
            ShareObject shareObject;
            Nfs3Procedure.Status status = sharedFileSystem.TryGetSharedDirectory(directory, out shareObject);
            if (status != Nfs3Procedure.Status.Ok) return new Mount3Reply(status);

            if (shareObject == null) return new Mount3Reply(Nfs3Procedure.Status.ErrorNoSuchFileOrDirectory);

            status = shareObject.CheckStatus();
            if (status != Nfs3Procedure.Status.Ok) return new Mount3Reply(status);

            return new Mount3Reply(shareObject.fileHandleBytes, null);
        }
    }
}
