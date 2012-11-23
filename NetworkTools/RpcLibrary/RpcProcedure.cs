using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace Marler.NetworkTools
{
    public class RpcProcedure
    {
        public readonly String procedureName;
        public readonly UInt32 procedureNumber;

        public readonly ISerializableData requestSerializer;
        public ISerializableData responseSerializer;

        public RpcProcedure(String procedureName, UInt32 procedureNumber,
            ISerializableData requestSerializer, ISerializableData responseSerializer)
        {
            this.procedureName = procedureName;
            this.procedureNumber = procedureNumber;
            this.requestSerializer = requestSerializer;
            this.responseSerializer = responseSerializer;
        }
        protected RpcProcedure(String procedureName, UInt32 procedureNumber,
            ISerializableData requestSerializer)
        {
            this.procedureName = procedureName;
            this.procedureNumber = procedureNumber;
            this.requestSerializer = requestSerializer;
        }
    }
}
