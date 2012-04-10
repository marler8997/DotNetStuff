using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.RuntimeAnalyzer
{
    public class RuntimeLibrary
    {
        public readonly UInt64 stdIOAddress;

        public RuntimeLibrary(UInt64 stdIOAddress)
        {
            this.stdIOAddress = stdIOAddress;
        }

        public UInt64 EmitLibrary(Byte byteCode, UInt64 offset)
        {
            //
            // Emit the Print Message Function
            //

            //
            // When I implement logic blocks...the logic block for putc, getc and so on should
            // be included in the byte code:)
            //


            //
            // puts
            //
            // This function takes in an address and prints each address to the console until it sees 0
            //
            



            return offset;
        }


    }
}
