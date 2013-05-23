﻿using System;
using System.Net.Sockets;

namespace Marler.Net
{
    public class IncomingConnection
    {
        public readonly String endPointName;
        public readonly Socket socket;
        public readonly UInt16 acceptedOnPort;

        public IncomingConnection(Socket socket, UInt16 acceptedOnPort)
        {
            this.endPointName = socket.RemoteEndPoint.ToString();
            this.socket = socket;
            this.acceptedOnPort = acceptedOnPort;
        }

        public override string ToString()
        {
            return endPointName;
        }
    }
}