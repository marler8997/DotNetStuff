using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.OpenTK.Common
{
    public struct BinaryFraction
    {
        public Int32 numerator;
        public UInt16 denominatorThatIsAPowerOf2;
        public BinaryFraction(Int32 numerator, Byte denominatorThatIsAPowerOf2)
        {
            this.numerator = numerator;
            this.denominatorThatIsAPowerOf2 = denominatorThatIsAPowerOf2;
        }
    }
}
