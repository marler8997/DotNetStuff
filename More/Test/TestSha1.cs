using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    /// <summary>
    /// Summary description for TestSha1
    /// </summary>
    [TestClass]
    public class TestSha1
    {
        [TestMethod]
        public void TestSha1HashApi()
        {
            UInt32[] expectedHash = new UInt32[] {
                0x12DADA1F, 0xFF4D4787, 0xADE33331, 0x47202C3B, 0x443E376F
            };


            Console.WriteLine("EXPCTD:" + Sha1.HashString(expectedHash));
            Console.WriteLine("----------------------------------------------------------");


            Byte[] data = new Byte[] { 1, 2, 3, 4 };
            String diff;

            //
            Sha1 fourCallSha = new Sha1();

            fourCallSha.Add(data, 0, 1);
            fourCallSha.Add(data, 1, 1);
            fourCallSha.Add(data, 2, 1);
            fourCallSha.Add(data, 3, 1);

            UInt32[] fourCallFinalHash = fourCallSha.Finish();
            Console.WriteLine("FINAL1 :" + Sha1.HashString(fourCallFinalHash));

            diff = expectedHash.Diff(fourCallFinalHash);
            Assert.IsNull(diff);

            Console.WriteLine("----------------------------------------------------------");
            //
            Sha1 twoCallSha = new Sha1();

            twoCallSha.Add(data, 0, 2);

            twoCallSha.Add(data, 2, 2);

            UInt32[] twoCallFinahHash = twoCallSha.Finish();
            Console.WriteLine("FINAL2 :" + Sha1.HashString(twoCallFinahHash));

            diff = expectedHash.Diff(twoCallFinahHash);
            Assert.IsNull(diff);

            Console.WriteLine("----------------------------------------------------------");
            //
            Sha1 oneCallSha = new Sha1();

            oneCallSha.Add(data, 0, 4);

            UInt32[] oneCallFinalHash = oneCallSha.Finish();
            Console.WriteLine("FINAL3 :" + Sha1.HashString(oneCallFinalHash));

            diff = expectedHash.Diff(oneCallFinalHash);
            Assert.IsNull(diff);
        }

        class TestClass
        {
            public readonly String contentString;
            public readonly Byte[] contentBytes;
            
            public readonly UInt32[] expectedHash;
            public TestClass(String contentString, params UInt32[] expectedHash)
            {
                this.contentString = contentString;
                this.contentBytes = Encoding.ASCII.GetBytes(contentString);
                this.expectedHash = expectedHash;
            }
        }

        [TestMethod]
        public void TestKnownHashes()
        {
            TestClass[] tests = new TestClass[] {
                new TestClass("abc", 0xA9993E36, 0x4706816A, 0xBA3E2571, 0x7850C26C, 0x9Cd0d89D),
                new TestClass("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq",
                    0x84983E44, 0x1C3BD26E, 0xBAAE4AA1, 0xF95129E5, 0xE54670F1),
                new TestClass("12345678901234567890123456789012345678901234567890123456789012345678901234567890",
                    0x50ABF570, 0x6A150990, 0xA08B2C5E, 0xA40FA0E5, 0x85554732),
                new TestClass("jafdnznjkl89fn4q3poiunqn8vrnaru8apr8umpau8rfpnu312--1-0-139-110un45paiouwepiourpoqiwrud0-ur238901unmxd-0r1u-rdu0-12u3rm-u-uqfoprufquwioperupauwperuq2cfurq2urduq;w3uirmparuw390peuaf;wuir;oui;avuwao; aro aruawrv au;ru ;aweuriafuwer23f0quprmpuqpuqurq[0q5tau=53una54fion[5cnuq30m5uq903uqncf4",
                    0xEEC53E5E, 0x78191154, 0x0A073AE1, 0x39743E68, 0x8A6CD077),
            };

            for(int i = 0; i < tests.Length; i++)
            {
                TestClass test = tests[i];
                
                //
                // Test using 1 call
                //
                Sha1 sha = new Sha1();
                sha.Add(test.contentBytes, 0, test.contentBytes.Length);

                UInt32[] finished = sha.Finish();

                Console.WriteLine("Content '{0}'", test.contentString);
                Console.WriteLine("    Expected {0}", Sha1.HashString(test.expectedHash));
                Console.WriteLine("    Actual   {0}", Sha1.HashString(finished));

                String sosDiff = Sos.Diff(test.expectedHash, finished);
                Assert.IsNull(sosDiff, sosDiff);

                //
                // Test using multiple calls
                //
                for (int addLength = 1; addLength < test.contentBytes.Length; addLength++)
                {
                    Console.WriteLine("Test AddLength {0}", addLength);
                    sha = new Sha1();

                    // Add the bytes
                    Int32 bytesToWrite = test.contentBytes.Length;
                    Int32 contentBytesOffset = 0;
                    while (bytesToWrite > 0)
                    {
                        Int32 writeLength = Math.Min(bytesToWrite, addLength);
                        sha.Add(test.contentBytes, contentBytesOffset, writeLength);
                        contentBytesOffset += writeLength;
                        bytesToWrite -= writeLength;
                    }

                    finished = sha.Finish();


                    sosDiff = Sos.Diff(test.expectedHash, finished);
                    if (sosDiff != null)
                    {
                        Console.WriteLine("Content '{0}'", test.contentString);
                        Console.WriteLine("    Expected {0}", Sha1.HashString(test.expectedHash));
                        Console.WriteLine("    Actual   {0}", Sha1.HashString(finished));
                        Assert.Fail();
                    }
                }
            }
        }
    }
}
