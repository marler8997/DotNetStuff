using System;

using Marler.Common;

namespace Marler.Net
{
    public class RpcCall : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new XdrStructFieldReflector<RpcProgramHeader>(typeof(RpcCall), "programHeader", RpcProgramHeader.memberSerializers),
            new XdrUInt32Reflector                       (typeof(RpcCall), "procedure"),
            new XdrStructFieldReflector<RpcCredentials>  (typeof(RpcCall), "credentials", RpcCredentials.memberSerializers),
            new XdrStructFieldReflector<RpcVerifier>     (typeof(RpcCall), "verifier"   , RpcVerifier.memberSerializers),
        };

        public readonly RpcProgramHeader programHeader;
        public readonly UInt32 procedure;
        public readonly RpcCredentials credentials;
        public readonly RpcVerifier verifier;

        public RpcCall()
            : base(memberSerializers)
        {
        }
        public RpcCall(RpcProgramHeader programHeader, UInt32 procedure,
            RpcCredentials credentials, RpcVerifier verifier)
            : base(memberSerializers)
        {
            this.programHeader = programHeader;
            this.procedure = procedure;
            this.credentials = credentials;
            this.verifier = verifier;
        }

        public RpcCall(Byte[] data, Int32 offset, Int32 maxOffset, out Int32 newOffset)
            : base(memberSerializers)
        {
            newOffset = Deserialize(data, offset, maxOffset);
        }
    }
}
