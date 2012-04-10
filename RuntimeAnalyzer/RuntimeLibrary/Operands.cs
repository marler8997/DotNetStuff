using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.RuntimeAnalyzer
{
    public static class Operands
    {
        //
        // Memory Type:
        //
        // 0000 0000 (  0) - 1110 1111 (239) : Quick representation of a variable
        // 1111 0000 (240) - 1111 0111 (247) : Indirect representation of a variable
		// 1111 1000 (248) - 1111 1111 (255) : Long representation of a variable (size is obtained by masking the byte with 0000 0111 and adding 1)
        //
        public const Byte WritableOpTypeFrameOffsetShortMax  = 223; // 1101 1111

        //
        // Value Type:
        //
        // 0000 0000 (  0) - 1110 0111 (231) : Quick representation of a variable
        // 1110 1000 (232) - 1110 1111 (239) : Long Representation of a literal (size is obtained by masking the byte with 0000 0111 and adding 1)
        // 1111 0000 (240) - 1111 0111 (247) : Indirect representation of a variable
        // 1111 1000 (248) - 1111 1111 (255) : Long Representation of Variable (size is obtained by masking the byte with 0000 0111 and adding 1)
        //
        public const Byte ReadableOpTypeFrameOffsetShortMax  = 215; // 1101 0111 : Used to check if an Info byte represents a quick variable reference
        public const Byte ReadableOpTypeLiteralMax           = 223; // 1101 1111 : Used to check if an Info byte represents a literal


        public const Byte OpTypeAddressMax                = 231;// 1110 0111 : Used to check if an Info byte represents an address
        public const Byte OpTypeFrameOffsetMax            = 239;// 1110 1111 : Used to check if an Info byte represents a frame offset
        public const Byte OpTypeAddressDereferenceMax     = 247;// 1111 0111 : Used to check if an Info byte represents a dereferenced address
        



        public const Byte InfoSizeMask                    = 0x07; // 0000 0111         : Used to mask the extra bytes for an operand from the first byte

        public const Byte LiteralInfoMask                 = 0xD8; // 1101 1000 = 216 : Used to create the info byte for a Literal operand
        public const Byte AddressInfoMask                 = 0xE0; // 1110 0000 = 224 : Used to create the info byte for an Address operand
        public const Byte FrameOffsetInfoMask             = 0xE8; // 1110 1000 = 232 : Used to create the info byte for a Frame Offset operand
        public const Byte AddressDereferenceInfoMask      = 0xF0; // 1111 0000 = 240 : Used to create the info byte for a Dereferenced Address operand
        public const Byte FrameOffsetDereferenceInfoMask  = 0xF8; // 1111 1000 = 248 : Used to create the info byte for a Dereferenced Frame Offset operand







        //
        // Jump Relative Address Type:
        //
        /*
        public const Byte JumpRelativeAddressOpMaxShort = 0xFD; // 1111 1101
        public const Byte JumpRelativeAddressOp4Bytes   = 0xFE; // 1111 1110
        public const Byte JumpRelativeAddressOp8Bytes   = 0xFF; // 1111 1111
        */
    }
}
