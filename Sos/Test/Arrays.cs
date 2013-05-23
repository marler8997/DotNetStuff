using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestClasses;
using System.Threading;

namespace Marler.Net
{
    /// <summary>
    /// Summary description for Arrays
    /// </summary>
    [TestClass]
    public class Arrays
    {
        [TestMethod]
        public void TestArrays()
        {
            StringBuilder builder = new StringBuilder();

            Array[] arrays = new Array[] {
                new Boolean[0],
                new Boolean[]{true},
                new Boolean[]{false},
                new Boolean[]{true,true,false,false,false,true},

                new SByte[0],
                new SByte[] {-128,0,1,2,3},
                new SByte[] {-128,-100,0,1,2,-23},

                new Byte[0],
                new Byte[] {0,1,2,3},

                new Double[0],
                new Double[] {Double.NaN, Double.MinValue, Double.MaxValue, Double.NegativeInfinity, Double.PositiveInfinity, Double.Epsilon},

                new ClassWithPrimitiveTypes[] {new ClassWithPrimitiveTypes(), new ClassWithPrimitiveTypes()},
            };
            for (int i = 0; i < arrays.Length; i++)
            {
                Util.TestSerializer(builder, arrays[i]);
            }
        }
    }
}
