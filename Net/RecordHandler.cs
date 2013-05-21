using System;
using System.Collections.Generic;
using System.Text;

using Marler.Common;

namespace Marler.Net
{
    public class RecordHandler
    {
        ByteBuffer buffer;
        private int bytesOffset;

        private int currentRecordSize;
        private int bytesReturnedInLastRecord;

        public RecordHandler(ByteBuffer buffer)
        {
            this.buffer = buffer;
            this.buffer.EnsureCapacity(5);
            this.bytesOffset = 0;
            this.currentRecordSize = 0;
            this.bytesReturnedInLastRecord = 0;
        }
        public void Reset()
        {
            this.bytesOffset = 0;
            this.currentRecordSize = 0;
            this.bytesReturnedInLastRecord = 0;
        }
        public void AddBytes(byte[] newBytes, int offset, int size)
        {
            if (size <= 0) return;

            Byte[] bufferBytes;
            
            if (currentRecordSize <= 0)
            {
                bufferBytes = buffer.array;
                if (bytesOffset < 1)
                {
                    if (size <= 1)
                    {
                        bufferBytes[bytesOffset++] = newBytes[0];
                        return;
                    }

                    this.currentRecordSize = ((0xFF00 & (newBytes[offset] << 8)) |
                                             (0x00FF & (newBytes[offset + 1])))
                                                        + 1;
                    offset += 2;
                    size -= 2;
                }
                else
                {
                    // bytesOffset == 1
                    this.currentRecordSize = ((0xFF00 & (bufferBytes[0] << 8)) |
                                             (0x00FF & (newBytes[offset])))
                                                        + 1;
                    offset++;
                    size--;
                    bytesOffset = 0;
                }
            }

            //
            // At this point, bytesOffset will represent how many of the current record bytes are in the buffer
            //

            // Make the bytes bigger if necessary
            buffer.EnsureCapacity(bytesOffset + size);
            bufferBytes = buffer.array;
            
            // Put the new bytes in
            for (int i = 0; i < size; i++)
            {
                bufferBytes[bytesOffset++] = newBytes[offset + i];
            }
        }
        public Int32 GetRecord()
        {
            Byte[] bufferBytes = buffer.array;

            //
            // Copy any bytes left over from the next record if there are any
            //
            if (bytesReturnedInLastRecord > 0)
            {
                int extraBytes = bytesOffset - bytesReturnedInLastRecord;
                if (extraBytes < 2) return 0; // wait until more bytes are added

                currentRecordSize = ((0xFF00 & (bufferBytes[bytesReturnedInLastRecord] << 8)) |
                                    (0x00FF & (bufferBytes[bytesReturnedInLastRecord + 1])))
                                            + 1;

                for (int i = 0; bytesReturnedInLastRecord + 2 + i < bytesOffset; i++)
                {
                    bufferBytes[i] = bufferBytes[bytesReturnedInLastRecord + 2 + i];
                }
                bytesOffset -= (bytesReturnedInLastRecord + 2);
                bytesReturnedInLastRecord = 0;
            }

            if (currentRecordSize <= 0) return 0;


            if (currentRecordSize == bytesOffset)
            {
                int size = currentRecordSize;
                bytesOffset = 0;
                currentRecordSize = 0;
                return size;
            }

            if (currentRecordSize < bytesOffset)
            {
                bytesReturnedInLastRecord = currentRecordSize;

                currentRecordSize = 0;
                return bytesReturnedInLastRecord;
            }

            return 0;
        }
    }
}
