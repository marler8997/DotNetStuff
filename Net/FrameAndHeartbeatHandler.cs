using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace More.Net
{
    //
    // These classes are used in order to allow two clients to send heartbeats
    // and data to eachother using the same tcp connection
    //
    // Every heartbeat packet is a 1 byte value 0x80
    // Every frame packet is 3 size bytes (in network host order)
    // followed by that amount of data.
    // i.e. byte[] {0, 0, 5, 1, 2, 3, 4, 5}
    //      this would induce GotFrame( byte[]{0,0,5,1,2,3,4,5}, 3, 5);
    //
    public interface IDataAndHeartbeatHandler : IDataHandler
    {
        void HandleHeartbeat();
    }
    public class DefaultHeartbeatAndDataHandler : IDataAndHeartbeatHandler
    {
        Int64 lastHeartbeat;
        public Int64 LastHeartbeat { get { return lastHeartbeat; } }

        readonly IDataHandler passTo;

        public DefaultHeartbeatAndDataHandler(IDataHandler passTo)
        {
            lastHeartbeat = 0;
            this.passTo = passTo;
        }
        public void HandleHeartbeat()
        {
            lastHeartbeat = Stopwatch.GetTimestamp();
        }
        public void HandleData(Byte[] data, Int32 offset, Int32 length)
        {
            passTo.HandleData(data, offset, length);
        }
        public void Dispose()
        {
            passTo.Dispose();
        }
    }
    public class SocketHeartbeatAndDataHandler : IDataAndHeartbeatHandler
    {
        readonly Socket socket;
        public SocketHeartbeatAndDataHandler(Socket socket)
        {

        }
        public void HandleHeartbeat()
        {
        }
        public void HandleData(byte[] data, int offset, int length)
        {
            socket.Send(data, offset, length, SocketFlags.None);
        }
        public void Dispose()
        {
            socket.ShutdownAndDispose();
        }
    }

    /*
    public class FrameAndHeartbeatDataHandlerFactory : IDataHandlerChainFactory
    {
        static FrameAndHeartbeatDataHandlerFactory instance;
        public static FrameAndHeartbeatDataHandlerFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FrameAndHeartbeatDataHandlerFactory();
                }
                return instance;
            }
        }
        private FrameAndHeartbeatDataHandlerFactory()
        {
        }
        public IDataHandler CreateDataHandlerChain(IDataHandler passTo)
        {
            return new FrameAndHeartbeatDataHandler(new DefaultHeartbeatAndDataHandler(passTo));
        }
    }
    */

    public static class FrameAndHeartbeatData
    {
        public const Byte Heartbeat = 0x80;
        public static readonly Byte[] HeartBeatPacket = new Byte[] { Heartbeat };
    }

    public class FrameAndHeartbeatDataSender : IDataHandler
    {
        public static void InsertLength(Byte[] buffer, Int32 offset, Int32 length)
        {
            if (length > 0x7FFFFF) throw new InvalidOperationException(String.Format("Max length is {0} but you provided {1}",
                 0x7FFFFF, length));
            buffer[offset    ] = (Byte)(length >> 16);
            buffer[offset + 1] = (Byte)(length >>  8);
            buffer[offset + 2] = (Byte)(length      );
        }

        readonly IDataHandler passDataTo;
        public FrameAndHeartbeatDataSender(IDataHandler passDataTo)
        {
            this.passDataTo = passDataTo;
        }
        public void HandleData(byte[] data, int offset, int length)
        {
            Byte[] newData = new Byte[length + 3];
            InsertLength(newData, 0, length);
            Array.Copy(data, offset, newData, 3, length);
            passDataTo.HandleData(newData, 0, newData.Length);
        }
        public void  Dispose()
        {
            passDataTo.Dispose();
        }
    }

    public class FrameAndHeartbeatDataReceiver : IDataHandler
    {
        readonly IDataAndHeartbeatHandler handler;
        readonly ByteBuffer buffer;
        Int32 currentBufferLength;

        public FrameAndHeartbeatDataReceiver(IDataAndHeartbeatHandler handler)
        {
            this.handler = handler;
            this.buffer = new ByteBuffer();
            this.currentBufferLength = 0;
        }
        public void Dispose()
        {
            handler.Dispose();
        }
        public void HandleData(byte[] data, int offset, int length)
        {
            if (length <= 0) return;

            //
            // Choose which array to work with
            //
            Int32 processLength;
            Int32 processOffset;
            Byte[] processArray;

            if (currentBufferLength <= 0)
            {
                processLength = length;
                processOffset = offset;
                processArray = data;
            }
            else
            {
                processLength = currentBufferLength + length;
                buffer.EnsureCapacity(processLength);
                Array.Copy(data, offset, buffer.array, currentBufferLength, length);
                currentBufferLength = processLength;
                processOffset = 0;
                processArray = buffer.array;
            }

            CommonHandleData(processArray, processOffset, processLength);
        }
        void CommonHandleData(Byte[] processArray, Int32 processOffset, Int32 processLength)
        {
            //
            // Process the array
            //
            while (true)
            {
                //
                // Check for heartbeats
                //
                while (processArray[processOffset] == FrameAndHeartbeatData.Heartbeat)
                {
                    handler.HandleHeartbeat();

                    processOffset++;
                    processLength--;

                    if (processLength <= 0)
                    {
                        currentBufferLength = 0;
                        return;
                    }
                }

                //
                // Process the command
                //
                if (processLength < 3)
                {
                    //
                    // Copy left over bytes
                    //
                    if (processOffset > 0 || processArray != buffer.array)
                    {
                        buffer.EnsureCapacity(3);
                        Array.Copy(processArray, processOffset, buffer.array, 0, processLength);
                        currentBufferLength = processLength;
                    }
                    return;
                }

                Int32 frameLength =
                    ((0xFF0000 & (processArray[processOffset] << 16)) |
                     (0x00FF00 & (processArray[processOffset + 1] << 8)) |
                     (0x0000FF & (processArray[processOffset + 2]))) + 3;

                if (frameLength > processLength)
                {
                    //
                    // Copy left over bytes
                    //
                    if (processOffset > 0 || processArray != buffer.array)
                    {
                        buffer.EnsureCapacity(frameLength);
                        Array.Copy(processArray, processOffset, buffer.array, 0, processLength);
                        currentBufferLength = processLength;
                    }
                    return;
                }

                handler.HandleData(processArray, processOffset + 3, frameLength - 3);
                processOffset += frameLength;
                processLength -= frameLength;

                if (processLength <= 0)
                {
                    currentBufferLength = 0;
                    break;
                }
            }
        }
    }
}
