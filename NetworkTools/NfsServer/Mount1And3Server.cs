using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Marler.NetworkTools
{
    public class Mount1And3Server : RpcServerHandler
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

        public override RpcReply Call(String clientString, RpcCall call, byte[] callParameters, int callOffset, int callMaxOffset, out ISerializableData replyParameters)
        {
            ISerializableData callData;
            replyParameters = VoidSerializableData.Instance;

            if (call.programHeader.programVersion == 1)
            {
                switch (call.procedure)
                {
                    case Mount1.NULL:
                        callData = VoidSerializableData.Instance;
                        break;
                    case Mount1.UMNT:

                        callData = new Mount1Procedure.UnmountCall(callParameters, callOffset, callMaxOffset);

                        break;
                    default:
                        if (NfsServerLog.warningLogger != null)
                            NfsServerLog.warningLogger.WriteLine("[{0}] [Warning] client '{1}' sent unknown procedure number {2}", serviceName, clientString, call.procedure);
                        return new RpcReply(RpcVerifier.None, RpcAcceptStatus.ProcedureUnavailable);
                }
            }
            else if (call.programHeader.programVersion == 3)
            {
                switch (call.procedure)
                {
                    case Mount3.NULL:
                        callData = VoidSerializableData.Instance;
                        break;
                    case Mount3.MNT:

                        Mount3Procedure.MountCall mountCall = new Mount3Procedure.MountCall(callParameters, callOffset, callMaxOffset);
                        replyParameters = Handle(mountCall);
                        callData = mountCall;

                        break;
                    default:
                        if (NfsServerLog.warningLogger != null)
                            NfsServerLog.warningLogger.WriteLine("[{0}] [Warning] client '{1}' sent unknown procedure number {2}", serviceName, clientString, call.procedure);
                        return new RpcReply(RpcVerifier.None, RpcAcceptStatus.ProcedureUnavailable);
                }
            }
            else
            {
                if (NfsServerLog.warningLogger != null)
                    NfsServerLog.warningLogger.WriteLine("[{0}] [Warning] client '{1}' sent unsupported version {2}", serviceName, clientString, call.programHeader.programVersion);
                return new RpcReply(new RpcMismatchInfo(1, 2));
            }
            if (NfsServerLog.warningLogger != null)
                NfsServerLog.warningLogger.WriteLine("[{0}] Rpc {1} => {2}", serviceName, callData.ToNiceSmallString(), replyParameters.ToNiceSmallString());
            return new RpcReply(RpcVerifier.None);
        }

        private Mount3Procedure.MountReply Handle(Mount3Procedure.MountCall mountCall)
        {
            String directoryKey = mountCall.directory;
            ShareObject shareObject;
            Nfs3Procedure.Status status = sharedFileSystem.TryGetSharedDirectory(mountCall.directory, out shareObject);
            if (status != Nfs3Procedure.Status.Ok) return new Mount3Procedure.MountReply(status);

            if (shareObject == null) return new Mount3Procedure.MountReply(Nfs3Procedure.Status.ErrorNoSuchFileOrDirectory);

            status = shareObject.CheckStatus();
            if (status != Nfs3Procedure.Status.Ok) return new Mount3Procedure.MountReply(status);

            return new Mount3Procedure.MountReply(shareObject.fileHandleBytes, null);
        }
    }
}
