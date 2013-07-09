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
    public static class FrameAndHeartbeatProtocol
    {
        public const Byte Heartbeat = 0xFF;
        public static readonly Byte[] HeartBeatPacket = new Byte[] { Heartbeat };

        public static void InsertLength(Byte[] buffer, Int32 offset, Int32 length)
        {
            if (length > 0xEFFFFF) throw new InvalidOperationException(String.Format(
                "Max length is {0} but you provided {1}", 0xEFFFFF, length));
            buffer[offset    ] = (Byte)(length >> 16);
            buffer[offset + 1] = (Byte)(length >>  8);
            buffer[offset + 2] = (Byte)(length      );
        }
        public static Byte[] AllocateFrame(Int32 offset, Int32 length)
        {
            Byte[] frame = new Byte[offset + length + 3];
            InsertLength(frame, offset, length);
            return frame;
        }
    }

    public class FrameAndHeartbeatDataSender : IDataHandler
    {
        readonly IDataHandler passDataTo;
        public FrameAndHeartbeatDataSender(IDataHandler passDataTo)
        {
            this.passDataTo = passDataTo;
        }
        public void HandleData(byte[] data, int offset, int length)
        {
            Byte[] newData = new Byte[length + 3];
            FrameAndHeartbeatProtocol.InsertLength(newData, 0, length);
            Array.Copy(data, offset, newData, 3, length);
            passDataTo.HandleData(newData, 0, newData.Length);
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
        public void HandleData(byte[] data, int offset, int length)
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
        Int32 currentBufferLength;

        public FrameAndHeartbeatReceiverFilter()
        {
            this.buffer = new ByteBuffer();
            this.currentBufferLength = 0;
        }
        public void FilterTo(DataHandler handler, byte[] data, int offset, int length)
        {
            FilterTo(handler, null, data, offset, length);
        }
        public void FilterTo(DataHandler handler, Action heartbeatCallback,
            byte[] data, int offset, int length)
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

            CommonHandleData(handler, heartbeatCallback, processArray, processOffset, processLength);
        }
        void CommonHandleData(DataHandler handler, Action heartbeatCallback,
            Byte[] processArray, Int32 processOffset, Int32 processLength)
        {
            //
            // Process the array
            //
            while (true)
            {
                //
                // Check for heartbeats
                //
                while (processArray[processOffset] == FrameAndHeartbeatProtocol.Heartbeat)
                {
                    if (heartbeatCallback != null) heartbeatCallback();

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
}
