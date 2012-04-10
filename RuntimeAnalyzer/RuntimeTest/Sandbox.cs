using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Timers;
using System.Diagnostics;
using System.Globalization;

namespace Marler.RuntimeAnalyzer
{
    [TestClass]
    public class Sandbox
    {
        public UInt64 One(byte[] bytes, Int32 offset)
        {
            return (((UInt64)(
            (((UInt32)bytes[offset++]) << 24) |
            (((UInt32)bytes[offset++]) << 16) |
            (((UInt32)bytes[offset++]) << 8) |
            (((UInt32)bytes[offset++]))
            )) << 8)
            |
            (((UInt64)bytes[offset++]));
        }

        public UInt64 Two(byte[] bytes, Int32 offset)
        {
            return
            (((UInt64)bytes[offset++]) << 32) |
            (((UInt64)bytes[offset++]) << 24) |
            (((UInt64)bytes[offset++]) << 16) |
            (((UInt64)bytes[offset++]) << 8) |
            (((UInt64)bytes[offset++]));
        }

        public UInt64 Three(byte[] bytes, Int32 offset)
        {
            return (((UInt64)(
            (((UInt32)bytes[offset++]) << 24) |
            (((UInt32)bytes[offset++]) << 16) |
            (((UInt32)bytes[offset++]) << 8) |
            (((UInt32)bytes[offset++]))
            )) << 8)
            |
            (((UInt64)bytes[offset++]));
        }

        [TestMethod]
        public void TestMethod1()
        {

            UInt64 zero = 0U;
            UInt64 ten = 10U;
            UInt64 uintMax = UInt64.MaxValue;

            Console.WriteLine("{0}", (Int64)ten + (Int64)uintMax);
            Console.WriteLine("0x{0:X}", uintMax);
            Console.WriteLine("0x{0:X}", (UInt64)((Int64)uintMax +(Int64)uintMax));



            Memory p = new Memory(100);

            String message = "Hello World\n";
            for (int i = 0; i < message.Length; i++)
            {
                //p.AssignLiteral(0, message[i]);
            }


            String [] s;
            
            s = new String[] {
                "0",
                "1",
                "0fFa1",
                "123"
            };

            NumberStyles parseNumberStyle = NumberStyles.HexNumber;

            for (int i = 0; i < s.Length; i++)
            {
                Int32 num = Int32.Parse(s[i], parseNumberStyle);
                Console.WriteLine("'{0}' = {1} 0x{1:X}", s[i], num);
            }



        }

    }
}
