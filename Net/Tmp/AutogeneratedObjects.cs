using System;

using More;

namespace More.Net
{
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
                    reflectors[0] = new ByteArrayReflector(typeof(ServerInfo), "Name", 1);
                    reflectors[1] = new BigEndianUInt16Reflector(typeof(ServerInfo), "SecondsPerHeartbeat");
                    reflectors[2] = new BigEndianUInt16Reflector(typeof(ServerInfo), "SecondsPerReconnect");
                    reflector = new Reflectors(reflectors);
                }
                return reflector;
            }
        }

        public static Int32 SerializationLength(ServerInfo obj)
        {
            return Reflector.SerializationLength(obj);
        }
        public static Int32 DynamicLengthSerialize(Byte[] array, Int32 offset, ServerInfo instance)
        {
            return Reflector.Serialize(instance, array, offset);
        }
        public static Int32 DynamicLengthDeserialize(Byte[] array, Int32 offset, Int32 offsetLimit, out ServerInfo outInstance)
        {
            Int32 newOffset;
            outInstance = new ServerInfo(array, offset, offsetLimit, out newOffset);
            return newOffset;
        }

        public Byte[] Name;
        public UInt16 SecondsPerHeartbeat;
        public UInt16 SecondsPerReconnect;

        // Deserialization constructor
        public ServerInfo(Byte[] array, Int32 offset, Int32 offsetLimit)
        {
            Int32 newOffset = Reflector.Deserialize(this, array, offset, offsetLimit);
            if(newOffset != offsetLimit) throw new FormatException(String.Format(
                "Expected packet 'ServerInfo' to be {1} bytes but was {2} bytes", offsetLimit - offset, newOffset - offset));
        }
        public ServerInfo(Byte[] array, Int32 offset, Int32 offsetLimit, out Int32 newOffset)
        {
            newOffset = Reflector.Deserialize(this, array, offset, offsetLimit);
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
                    reflectors[0] = new BigEndianUnsignedEnumReflector<TunnelOptions>(typeof(OpenAccessorTunnelRequest), "Options", 1);
                    reflectors[1] = new ByteArrayReflector(typeof(OpenAccessorTunnelRequest), "TargetHost", 1);
                    reflectors[2] = new BigEndianUInt16Reflector(typeof(OpenAccessorTunnelRequest), "TargetPort");
                    reflectors[3] = new ByteArrayReflector(typeof(OpenAccessorTunnelRequest), "TunnelKey", 1);
                    reflector = new Reflectors(reflectors);
                }
                return reflector;
            }
        }

        public static Int32 SerializationLength(OpenAccessorTunnelRequest obj)
        {
            return Reflector.SerializationLength(obj);
        }
        public static Int32 DynamicLengthSerialize(Byte[] array, Int32 offset, OpenAccessorTunnelRequest instance)
        {
            return Reflector.Serialize(instance, array, offset);
        }
        public static Int32 DynamicLengthDeserialize(Byte[] array, Int32 offset, Int32 offsetLimit, out OpenAccessorTunnelRequest outInstance)
        {
            Int32 newOffset;
            outInstance = new OpenAccessorTunnelRequest(array, offset, offsetLimit, out newOffset);
            return newOffset;
        }

        public TunnelOptions Options;
        public Byte[] TargetHost;
        public UInt16 TargetPort;
        public Byte[] TunnelKey;

        // Deserialization constructor
        public OpenAccessorTunnelRequest(Byte[] array, Int32 offset, Int32 offsetLimit)
        {
            Int32 newOffset = Reflector.Deserialize(this, array, offset, offsetLimit);
            if(newOffset != offsetLimit) throw new FormatException(String.Format(
                "Expected packet 'OpenAccessorTunnelRequest' to be {1} bytes but was {2} bytes", offsetLimit - offset, newOffset - offset));
        }
        public OpenAccessorTunnelRequest(Byte[] array, Int32 offset, Int32 offsetLimit, out Int32 newOffset)
        {
            newOffset = Reflector.Deserialize(this, array, offset, offsetLimit);
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
                    reflectors[0] = new BigEndianUnsignedEnumReflector<TunnelOptions>(typeof(OpenTunnelRequest), "Options", 1);
                    reflectors[1] = new ByteArrayReflector(typeof(OpenTunnelRequest), "TargetHost", 1);
                    reflectors[2] = new BigEndianUInt16Reflector(typeof(OpenTunnelRequest), "TargetPort");
                    reflectors[3] = new ByteArrayReflector(typeof(OpenTunnelRequest), "OtherTargetHost", 1);
                    reflectors[4] = new BigEndianUInt16Reflector(typeof(OpenTunnelRequest), "OtherTargetPort");
                    reflector = new Reflectors(reflectors);
                }
                return reflector;
            }
        }

        public static Int32 SerializationLength(OpenTunnelRequest obj)
        {
            return Reflector.SerializationLength(obj);
        }
        public static Int32 DynamicLengthSerialize(Byte[] array, Int32 offset, OpenTunnelRequest instance)
        {
            return Reflector.Serialize(instance, array, offset);
        }
        public static Int32 DynamicLengthDeserialize(Byte[] array, Int32 offset, Int32 offsetLimit, out OpenTunnelRequest outInstance)
        {
            Int32 newOffset;
            outInstance = new OpenTunnelRequest(array, offset, offsetLimit, out newOffset);
            return newOffset;
        }

        public TunnelOptions Options;
        public Byte[] TargetHost;
        public UInt16 TargetPort;
        public Byte[] OtherTargetHost;
        public UInt16 OtherTargetPort;

        // Deserialization constructor
        public OpenTunnelRequest(Byte[] array, Int32 offset, Int32 offsetLimit)
        {
            Int32 newOffset = Reflector.Deserialize(this, array, offset, offsetLimit);
            if(newOffset != offsetLimit) throw new FormatException(String.Format(
                "Expected packet 'OpenTunnelRequest' to be {1} bytes but was {2} bytes", offsetLimit - offset, newOffset - offset));
        }
        public OpenTunnelRequest(Byte[] array, Int32 offset, Int32 offsetLimit, out Int32 newOffset)
        {
            newOffset = Reflector.Deserialize(this, array, offset, offsetLimit);
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
}
