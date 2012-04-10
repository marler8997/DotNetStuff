using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Marler.RuntimeAnalyzer
{
    public class Runtime
    {
        public readonly String raProgram;
        public readonly Memory processStack;

        public Runtime(String raProgram, Memory processStack)
        {
            this.raProgram = raProgram;
            this.processStack = processStack;
        }

        public void Run()
        {
            UInt64 startOffset;
            byte[] byteCode = null;

            using (FileStream fileStream = new FileStream(raProgram, FileMode.Open))
            {
                UInt64 fileStreamLength = (UInt64)fileStream.Length;
                //Console.WriteLine("fileStreamLength = {0}", fileStreamLength);

                //
                // 1. Read Meta Data
                //
                UInt64 bytesRead = 0;
                //Console.WriteLine("bytesRead: {0}", bytesRead);
                startOffset = Util.ReadAddressValue(fileStream, ref bytesRead);
                //Console.WriteLine("bytesRead: {0} startOffset = {1}", bytesRead, startOffset);

                //
                // 2. Read the byte code
                //
                Int32 length = (Int32)(fileStreamLength - bytesRead);
                //Console.WriteLine();
                //Console.WriteLine("Reading Byte Code ({0} bytes)...", length);
                byteCode = new Byte[fileStreamLength - bytesRead];

                Int32 lastBytesRead;
                Int32 offset = 0;

                do
                {
                    lastBytesRead = fileStream.Read(byteCode, offset, length);
                    length -= lastBytesRead;
                    if (length <= 0)
                    {
                        break;
                    }
                    offset += lastBytesRead;
                } while (lastBytesRead > 0);

                if(length > 0) throw new EndOfStreamException(String.Format("Still needed {0} bytes", length));
                //Console.WriteLine("Done Reading Byte Code");
            }


            if (byteCode != null)
            {
                InstructionProcessor.Execute(byteCode, startOffset, processStack);
            }

        }

       



    }


    



}
