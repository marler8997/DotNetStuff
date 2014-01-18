using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace More.Net
{
    //
    // These classes implement the FrameAndHeartbeat Protocol
    //
    // Uses: This protocol can be used to both implement heartbeats and reestablish data frame
    //       boundaries after it has been written to a stream interface (such as TCP).
    //
    // Heartbeat Packet:
    //   [0xFF]
    // Frame Packet:
    //   [FrameSizeByte1, FrameSizeByte2, FrameSizeByte3, FrameData...]
    //   i.e. [0,0,0] (an empty frame)
    //   i.e. [0,0,6,1,2,3,4,5,6] (a frame of the numbers 1 through 6)
    //   Note: FrameSizeByte1 cannot be 0xFF, otherwise that byte will be interpreted
    //         as a heartbeat packet.
    //
    // Restrictions:
    // 1 It is important that when someone writes to a FrameAndHeartbeat stream, they must
    //   make sure that they either write each frame all at once, or they lock the stream
    //   when they are making multiple writes for a single frame.  Otherwise, frame data and
    //   heartbeats can become intermixed and corrupt the data.
    // 2 Since FrameSizeByte1 cannot be 0xFF, the maximum frame size is 0xEFFFFF (15,728,639) bytes
    //
    public static class FrameProtocol
    {
        public static readonly Byte[] HeartbeatSendPacket = new Byte[] { 0, 0, 0, 0 };
        public static void InsertLength(Byte[] buffer, UInt32 offset, UInt32 length)
        {
            buffer[offset    ] = (Byte)(length >> 24);
            buffer[offset + 1] = (Byte)(length >> 16);
            buffer[offset + 2] = (Byte)(length >>  8);
            buffer[offset + 3] = (Byte)(length      );
        }
        public static Byte[] AllocateFrame(UInt32 offset, UInt32 length)
        {
            Byte[] frame = new Byte[offset + length + 4];
            InsertLength(frame, offset, length);
            return frame;
        }
        public static UInt32 SetupFrame(ByteBuffer buffer, UInt32 frameOffset, UInt32 contentLength)
        {
            buffer.EnsureCapacityCopyData(frameOffset + 4 + contentLength);
            InsertLength(buffer.array, frameOffset, contentLength);
            return frameOffset + 4;
        }
    }
    /*
    public class FrameAndHeartbeatDataSender : IDataHandler
    {
        readonly IDataHandler passDataTo;
        public FrameAndHeartbeatDataSender(IDataHandler passDataTo)
        {
            this.passDataTo = passDataTo;
        }
        public void HandleData(Byte[] data, UInt32 offset, UInt32 length)
        {
            Byte[] newData = new Byte[length + 4];
            FrameAndHeartbeat.InsertLength(newData, 0, length);
            Array.Copy(data, offset, newData, 4, length);
            passDataTo.HandleData(newData, 0, (UInt32)newData.Length);
        }
        public void Dispose()
        {
            passDataTo.Dispose();
        }
    }
    */
    public class FrameProtocolReceiverHandler : IDataHandler
    {
        readonly DataHandler dataHandler;
        readonly Action disposeHandler;
        readonly FrameProtocolFilter filter;

        public FrameProtocolReceiverHandler(DataHandler dataHandler, Action disposeHandler)
        {
            this.dataHandler = dataHandler;
            this.disposeHandler = disposeHandler;
            this.filter = new FrameProtocolFilter();
        }
        public void HandleData(Byte[] data, UInt32 offset, UInt32 length)
        {
            filter.FilterTo(dataHandler, data, offset, length);
        }
        public void Dispose()
        {
            if (disposeHandler != null) disposeHandler();
        }
    }
    public class FrameProtocolFilter : IDataFilter
    {
        readonly ByteBuffer buffer;
        UInt32 currentBufferLength;

        public FrameProtocolFilter()
        {
            this.buffer = new ByteBuffer();
            this.currentBufferLength = 0;
        }
        public void FilterTo(DataHandler handler, Byte[] data, UInt32 offset, UInt32 length)
        {
            if (length <= 0) return;

            //
            // Choose which array to work with
            //
            if (currentBufferLength <= 0)
            {
                CommonHandleData(handler, data, offset, length);
            }
            else
            {
                UInt32 processLength = currentBufferLength + length;
                buffer.EnsureCapacityCopyData(processLength);
                Array.Copy(data, offset, buffer.array, currentBufferLength, length);
                currentBufferLength = processLength;
                CommonHandleData(handler, buffer.array, 0, processLength);
            }
        }
        void CommonHandleData(DataHandler handler,
            Byte[] processArray, UInt32 processOffset, UInt32 processLength)
        {
            //
            // Process the array
            //
            while (true)
            {
                //
                // Process the command
                //
                if (processLength < 4)
                {
                    //
                    // Copy left over bytes
                    //
                    if (processOffset > 0 || processArray != buffer.array)
                    {
                        buffer.EnsureCapacityCopyData(4);
                        Array.Copy(processArray, processOffset, buffer.array, 0, processLength);
                        currentBufferLength = processLength;
                    }
                    return;
                }

                UInt32 frameLength = 4U +
                    ((0xFF000000 & (UInt32)(processArray[processOffset    ] << 24)) |
                     (0x00FF0000 & (UInt32)(processArray[processOffset + 1] << 16)) |
                     (0x0000FF00 & (UInt32)(processArray[processOffset + 2] <<  8)) |
                     (0x000000FF & (UInt32)(processArray[processOffset + 3]      )) );

                if (frameLength > processLength)
                {
                    //
                    // Copy left over bytes
                    //
                    if (processOffset > 0 || processArray != buffer.array)
                    {
                        buffer.EnsureCapacityCopyData(frameLength);
                        Array.Copy(processArray, processOffset, buffer.array, 0, processLength);
                        currentBufferLength = processLength;
                    }
                    return;
                }

                handler(processArray, processOffset + 4, frameLength - 4);
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
    /*
    public static class FrameAndHeartbeat
    {
        public const UInt32 MaxLength = 0xEFFFFFFF;

        public const Byte Heartbeat = 0xFF;
        public static readonly Byte[] HeartBeatPacket = new Byte[] { Heartbeat };

        public static void InsertLength(Byte[] buffer, UInt32 offset, UInt32 length)
        {
            if (length > MaxLength) throw new InvalidOperationException(String.Format(
                "Max length is {0} but you provided {1}", MaxLength, length));
            buffer[offset    ] = (Byte)(length >> 16);
            buffer[offset + 1] = (Byte)(length >>  8);
            buffer[offset + 2] = (Byte)(length      );
        }
        public static Byte[] AllocateFrame(UInt32 offset, UInt32 length)
        {
            Byte[] frame = new Byte[offset + length + 3];
            InsertLength(frame, offset, length);
            return frame;
        }
        public static UInt32 SetupFrame(ByteBuffer buffer, UInt32 frameOffset, UInt32 contentLength)
        {
            buffer.EnsureCapacityCopyData(frameOffset + 3 + contentLength);
            InsertLength(buffer.array, frameOffset, contentLength);
            return frameOffset + 3;
        }
    }

    public class FrameAndHeartbeatDataSender : IDataHandler
    {
        readonly IDataHandler passDataTo;
        public FrameAndHeartbeatDataSender(IDataHandler passDataTo)
        {
            this.passDataTo = passDataTo;
        }
        public void HandleData(Byte[] data, UInt32 offset, UInt32 length)
        {
            Byte[] newData = new Byte[length + 3];
            FrameAndHeartbeat.InsertLength(newData, 0, length);
            Array.Copy(data, offset, newData, 3, length);
            passDataTo.HandleData(newData, 0, (UInt32)newData.Length);
        }
        public void  Dispose()
        {
            passDataTo.Dispose();
        }
    }
    public class FrameAndHeartbeatReceiverHandler : IDataHandler
    {
        readonly DataHandler dataHandler;
        readonly Action heartbeatHandler;
        readonly Action disposeHandler;
        readonly FrameAndHeartbeatReceiverFilter filter;

        public FrameAndHeartbeatReceiverHandler(DataHandler dataHandler, Action heartbeatHandler, Action disposeHandler)
        {
            this.dataHandler = dataHandler;
            this.heartbeatHandler = heartbeatHandler;
            this.disposeHandler = disposeHandler;
            this.filter = new FrameAndHeartbeatReceiverFilter();
        }
        public void HandleData(Byte[] data, UInt32 offset, UInt32 length)
        {
            filter.FilterTo(dataHandler, heartbeatHandler, data, offset, length);
        }
        public void Dispose()
        {
            if (disposeHandler != null) disposeHandler();
        }
    }
    public class FrameAndHeartbeatReceiverFilter : IDataFilter
    {
        readonly ByteBuffer buffer;
        UInt32 currentBufferLength;

        public FrameAndHeartbeatReceiverFilter()
        {
            this.buffer = new ByteBuffer();
            this.currentBufferLength = 0;
        }
        public void FilterTo(DataHandler handler, Byte[] data, UInt32 offset, UInt32 length)
        {
            FilterTo(handler, null, data, offset, length);
        }
        public void FilterTo(DataHandler handler, Action heartbeatCallback,
            Byte[] data, UInt32 offset, UInt32 length)
        {
            if (length <= 0) return;

            //
            // Choose which array to work with
            //
            if (currentBufferLength <= 0)
            {
                CommonHandleData(handler, heartbeatCallback, data, offset, length);
            }
            else
            {
                UInt32 processLength = currentBufferLength + length;
                buffer.EnsureCapacityCopyData(processLength);
                Array.Copy(data, offset, buffer.array, currentBufferLength, length);
                currentBufferLength = processLength;
                CommonHandleData(handler, heartbeatCallback, buffer.array, 0, processLength);
            }

        }
        void CommonHandleData(DataHandler handler, Action heartbeatCallback,
            Byte[] processArray, UInt32 processOffset, UInt32 processLength)
        {
            //
            // Process the array
            //
            while (true)
            {
                //
                // Check for heartbeats
                //
                while (processArray[processOffset] == FrameAndHeartbeat.Heartbeat)
                {
                    if (heartbeatCallback != null) heartbeatCallback();

                    processOffset++;
                    processLength--;

                    if (processLength == 0)
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
                        buffer.EnsureCapacityCopyData(3);
                        Array.Copy(processArray, processOffset, buffer.array, 0, processLength);
                        currentBufferLength = processLength;
                    }
                    return;
                }

                UInt32 frameLength = 3 +
                    ((0xFF0000 & (UInt32)(processArray[processOffset    ] << 16)) |
                     (0x00FF00 & (UInt32)(processArray[processOffset + 1] <<  8)) |
                     (0x0000FF & (UInt32)(processArray[processOffset + 2]      )) );

                if (frameLength > processLength)
                {
                    //
                    // Copy left over bytes
                    //
                    if (processOffset > 0 || processArray != buffer.array)
                    {
                        buffer.EnsureCapacityCopyData(frameLength);
                        Array.Copy(processArray, processOffset, buffer.array, 0, processLength);
                        currentBufferLength = processLength;
                    }
                    return;
                }

                handler(processArray, processOffset + 3, frameLength - 3);
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
    */
}
