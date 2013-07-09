using System;

using More;

    [Flags]
    public enum AccessorConnectionInfo {
        TmpRequiresTls = 1,
    }
    [Flags]
    public enum ServerConnectionInfo {
        IsTunnel = 1,
        RequireTls = 2,
    }
    [Flags]
    public enum TunnelOptions {
        RequireTls = 1,
    }
    public class ServerInfo
    {
        static IReflector reflector = null;
        public static IReflector Reflector
        {
            get
            {
                if(reflector == null)
                {
                    IReflector[] reflectors = new IReflector[3];
                    reflectors[0] = new PdlByteLengthByteArrayReflector(typeof(ServerInfo), "Name");
                    reflectors[1] = new PdlUInt16Reflector(typeof(ServerInfo), "SecondsPerHeartbeat");
                    reflectors[2] = new PdlUInt16Reflector(typeof(ServerInfo), "SecondsPerReconnect");
                    reflector = new IReflectors(reflectors);
                }
                return reflector;
            }
        }
        public Byte[] Name;
        public UInt16 SecondsPerHeartbeat;
        public UInt16 SecondsPerReconnect;

        // Deserialization constructor
        public ServerInfo(Byte[] array, Int32 offset, Int32 maxOffset)
        {
            int finalOffset = Reflector.Deserialize(this, array, offset, maxOffset);
            if(finalOffset != maxOffset) throw new FormatException(String.Format(
                "Expected packet 'ServerInfo' to be {1} bytes but was {2} bytes", maxOffset - offset, finalOffset - offset));
        }
        public ServerInfo(Byte[] Name, UInt16 SecondsPerHeartbeat, UInt16 SecondsPerReconnect)
        {
            this.Name = Name;
            this.SecondsPerHeartbeat = SecondsPerHeartbeat;
            this.SecondsPerReconnect = SecondsPerReconnect;
        }
    }
    public class OpenAccessorTunnelRequest
    {
        static IReflector reflector = null;
        public static IReflector Reflector
        {
            get
            {
                if(reflector == null)
                {
                    IReflector[] reflectors = new IReflector[4];
                    reflectors[0] = new PdlByteEnumReflector<TunnelOptions>(typeof(OpenAccessorTunnelRequest), "Options");
                    reflectors[1] = new PdlByteLengthByteArrayReflector(typeof(OpenAccessorTunnelRequest), "TargetHost");
                    reflectors[2] = new PdlUInt16Reflector(typeof(OpenAccessorTunnelRequest), "TargetPort");
                    reflectors[3] = new PdlByteLengthByteArrayReflector(typeof(OpenAccessorTunnelRequest), "TunnelKey");
                    reflector = new IReflectors(reflectors);
                }
                return reflector;
            }
        }
        public TunnelOptions Options;
        public Byte[] TargetHost;
        public UInt16 TargetPort;
        public Byte[] TunnelKey;

        // Deserialization constructor
        public OpenAccessorTunnelRequest(Byte[] array, Int32 offset, Int32 maxOffset)
        {
            int finalOffset = Reflector.Deserialize(this, array, offset, maxOffset);
            if(finalOffset != maxOffset) throw new FormatException(String.Format(
                "Expected packet 'OpenAccessorTunnelRequest' to be {1} bytes but was {2} bytes", maxOffset - offset, finalOffset - offset));
        }
        public OpenAccessorTunnelRequest(TunnelOptions Options, Byte[] TargetHost, UInt16 TargetPort, Byte[] TunnelKey)
        {
            this.Options = Options;
            this.TargetHost = TargetHost;
            this.TargetPort = TargetPort;
            this.TunnelKey = TunnelKey;
        }
    }
    public class OpenTunnelRequest
    {
        static IReflector reflector = null;
        public static IReflector Reflector
        {
            get
            {
                if(reflector == null)
                {
                    IReflector[] reflectors = new IReflector[5];
                    reflectors[0] = new PdlByteEnumReflector<TunnelOptions>(typeof(OpenTunnelRequest), "Options");
                    reflectors[1] = new PdlByteLengthByteArrayReflector(typeof(OpenTunnelRequest), "TargetHost");
                    reflectors[2] = new PdlUInt16Reflector(typeof(OpenTunnelRequest), "TargetPort");
                    reflectors[3] = new PdlByteLengthByteArrayReflector(typeof(OpenTunnelRequest), "OtherTargetHost");
                    reflectors[4] = new PdlUInt16Reflector(typeof(OpenTunnelRequest), "OtherTargetPort");
                    reflector = new IReflectors(reflectors);
                }
                return reflector;
            }
        }
        public TunnelOptions Options;
        public Byte[] TargetHost;
        public UInt16 TargetPort;
        public Byte[] OtherTargetHost;
        public UInt16 OtherTargetPort;

        // Deserialization constructor
        public OpenTunnelRequest(Byte[] array, Int32 offset, Int32 maxOffset)
        {
            int finalOffset = Reflector.Deserialize(this, array, offset, maxOffset);
            if(finalOffset != maxOffset) throw new FormatException(String.Format(
                "Expected packet 'OpenTunnelRequest' to be {1} bytes but was {2} bytes", maxOffset - offset, finalOffset - offset));
        }
        public OpenTunnelRequest(TunnelOptions Options, Byte[] TargetHost, UInt16 TargetPort, Byte[] OtherTargetHost, UInt16 OtherTargetPort)
        {
            this.Options = Options;
            this.TargetHost = TargetHost;
            this.TargetPort = TargetPort;
            this.OtherTargetHost = OtherTargetHost;
            this.OtherTargetPort = OtherTargetPort;
        }
    }
