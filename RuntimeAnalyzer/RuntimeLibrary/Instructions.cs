using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.RuntimeAnalyzer
{
    public static class Instructions
    {
        public const Byte Move                = 0;
        public const Byte ON                  = 1;
        public const Byte Off                 = 2;
        
        //
        // Self Modifying Operators
        //
        public const Byte Increment           = 3;
        public const Byte Decrement           = 4;

        //
        // Unary Operators
        //
        public const Byte Negate              = 5;
        public const Byte NegateTo            = 6;

        public const Byte Compliment          = 7;
        public const Byte ComplimentTo        = 8;

        public const Byte LeadingZeros        = 9;
        public const Byte LeadingZerosTo      = 10;

        public const Byte LeadingOnes         = 11;
        public const Byte LeadingOnesTo       = 12;

        //
        // Binary Operators
        //
        public const Byte And                 = 13;
        public const Byte AndTo               = 14;

        public const Byte Or                  = 15;
        public const Byte OrTo                = 16;

        public const Byte XOr                 = 17;
        public const Byte XOrTo               = 18;

        public const Byte ShiftLeft           = 19;
        public const Byte ShiftLeftTo         = 20;
        
        public const Byte ShiftRight          = 21;
        public const Byte ShiftRightTo        = 22;

        public const Byte RotateLeft          = 23;
        public const Byte RotateLeftTo        = 24;

        public const Byte RotateRight         = 25;
        public const Byte RotateRightTo       = 26;

        public const Byte Add                 = 27;
        public const Byte AddTo               = 28;

        public const Byte Mult                = 29;
        public const Byte MultTo              = 30;

        public const Byte Sub                 = 31;
        public const Byte SubTo               = 32;
        public const Byte SubReverse          = 33;

        public const Byte Div                 = 34;
        public const Byte DivTo               = 35;
        public const Byte DivReverse          = 36;

        //
        // Jumps
        //
        public const Byte JumpB                = 37;
        public const Byte JumpF                = 38;

        //
        // Conditional Jumps
        //
        public const Byte JumpBIfZero          = 39;
        public const Byte JumpFIfZero          = 40;
        public const Byte JumpBIfNotZero       = 41;
        public const Byte JumpFIfNotZero       = 42;
        public const Byte JumpBIfPositive      = 43;
        public const Byte JumpFIfPositive      = 44;
        public const Byte JumpBIfNotNegative   = 45;
        public const Byte JumpFIfNotNegative   = 46;
        public const Byte JumpBIfNegative      = 47;
        public const Byte JumpFIfNegative      = 48;
        public const Byte JumpBIfNotPositive   = 49;
        public const Byte JumpFIfNotPositive   = 50;

        //
        // Binary Conditional Jumps
        //
        public const Byte JumpBIfEQ            = 51;
        public const Byte JumpFIfEQ            = 52;
        public const Byte JumpBIfNEQ           = 53;
        public const Byte JumpFIfNEQ           = 54;
        public const Byte JumpBIfGT            = 55;
        public const Byte JumpFIfGT            = 56;
        public const Byte JumpBIfGTE           = 57;
        public const Byte JumpFIfGTE           = 58;
        public const Byte JumpBIfLT            = 59;
        public const Byte JumpFIfLT            = 60;
        public const Byte JumpBIfLTE           = 61;
        public const Byte JumpFIfLTE           = 62;

        public const Byte Halt                 = 63;
        public const Byte InstructionCount     = 64;


        public static String[] InstructionName = new String[InstructionCount];
        static Instructions()
        {
            InstructionName[Move]                = "Move";
            InstructionName[ON]                  = "ON";
            InstructionName[Off]                 = "Off";
            
            //
            // Self Modifying Operators
            //
            InstructionName[Increment]           = "Increment";
            InstructionName[Decrement]           = "Decrement";

            //
            // Unary Operators
            //
            InstructionName[Negate]              = "Negate";
            InstructionName[NegateTo]            = "NegateTo";

            InstructionName[Compliment]          = "Compliment";
            InstructionName[ComplimentTo]        = "ComplimentTo";

            InstructionName[LeadingZeros]        = "LeadingZeros";
            InstructionName[LeadingZerosTo]      = "LeadingZerosTo";

            InstructionName[LeadingOnes]         = "LeadingOnes";
            InstructionName[LeadingOnesTo]       = "LeadingOnesTo";

            //
            // Binary Operators
            //
            InstructionName[And]                 = "And";
            InstructionName[AndTo]               = "AndTo";

            InstructionName[Or]                  = "Or";
            InstructionName[OrTo]                = "OrTo";

            InstructionName[XOr]                 = "XOr";
            InstructionName[XOrTo]               = "XOrTo";

            InstructionName[ShiftLeft]           = "ShiftLeft";
            InstructionName[ShiftLeftTo]         = "ShiftLeftTo";
            
            InstructionName[ShiftRight]          = "ShiftRight";
            InstructionName[ShiftRightTo]        = "ShiftRightTo";

            InstructionName[RotateLeft]          = "RotateLeft";
            InstructionName[RotateLeftTo]        = "RotateLeftTo";

            InstructionName[RotateRight]         = "RotateRight";
            InstructionName[RotateRightTo]       = "RotateRightTo";

            InstructionName[Add]                 = "Add";
            InstructionName[AddTo]               = "AddTo";

            InstructionName[Mult]                = "Mult";
            InstructionName[MultTo]              = "MutlTo";

            InstructionName[Sub]                 = "Sub";
            InstructionName[SubTo]               = "SubTo";
            InstructionName[SubReverse]          = "SubReverse";

            InstructionName[Div]                 = "Div";
            InstructionName[DivTo]               = "DivTo";
            InstructionName[DivReverse]          = "DivReverse";

            //
            // Jumps
            //
            InstructionName[JumpB]                = "JumpB";
            InstructionName[JumpF]                = "JumpF";

            //
            // Conditional Jumps
            //
            InstructionName[JumpBIfZero]          = "JumpBIfZero";
            InstructionName[JumpFIfZero]          = "JumpFIfZero";
            InstructionName[JumpBIfNotZero]       = "JumpBIfNotZero";
            InstructionName[JumpFIfNotZero]       = "JumpFIfNotZero";
            InstructionName[JumpBIfPositive]      = "JumpBIfPositive";
            InstructionName[JumpFIfPositive]      = "JumpFIfPositive";
            InstructionName[JumpBIfNotNegative]   = "JumpBIfNotNegative";
            InstructionName[JumpFIfNotNegative]   = "JumpFIfNotNegative";
            InstructionName[JumpBIfNegative]      = "JumpBIfNegative";
            InstructionName[JumpFIfNegative]      = "JumpFIfNegative";
            InstructionName[JumpBIfNotPositive]   = "JumpBIfNotPositive";
            InstructionName[JumpFIfNotPositive]   = "JumpFIfNotPositive";

            //
            // Binary Conditional Jumps
            //
            InstructionName[JumpBIfEQ]            = "JumpBIfEQ";
            InstructionName[JumpFIfEQ]            = "JumpFIfEQ";
            InstructionName[JumpBIfNEQ]           = "JumpBIfNEQ";
            InstructionName[JumpFIfNEQ]           = "JumpFIfNEQ";
            InstructionName[JumpBIfGT]            = "JumpBIfGT";
            InstructionName[JumpFIfGT]            = "JumpFIfGT";
            InstructionName[JumpBIfGTE]           = "JumpBIfGTE";
            InstructionName[JumpFIfGTE]           = "JumpFIfGTE";
            InstructionName[JumpBIfLT]            = "JumpBIfLT";
            InstructionName[JumpFIfLT]            = "JumpFIfLT";
            InstructionName[JumpBIfLTE]           = "JumpBIfLTE";
            InstructionName[JumpFIfLTE]           = "JumpFIfLTE";

            InstructionName[Halt]                 = "Halt";
        }


        public static String ToString(Byte instruction)
        {
            if (instruction < InstructionCount)
                return InstructionName[instruction];

            return "???";
        }
    }
}
