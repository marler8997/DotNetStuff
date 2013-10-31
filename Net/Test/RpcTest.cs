using System;
using System.Collections.Generic;
using System.Net.Sockets;


using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace More.Net
{

    public struct Data
    {
        public readonly Byte[] bytes;
        public readonly UInt32 offset;
        public readonly UInt32 length;
        public Data(Byte[] bytes, UInt32 offset, UInt32 length)
        {
            this.bytes = bytes;
            this.offset = offset;
            this.length = length;
        }
    }

    public class RecordChecker
    {
        public readonly Queue<Data> expectedDataQueue = new Queue<Data>();

        public void Add(Data data)
        {
            this.expectedDataQueue.Enqueue(data);
        }
        public void Add(Byte[] bytes)
        {
            this.expectedDataQueue.Enqueue(new Data(bytes, 0, (UInt32)bytes.Length));
        }
        public void Add(Byte[] bytes, UInt32 offset, UInt32 length)
        {
            this.expectedDataQueue.Enqueue(new Data(bytes, offset, length));
        }
        public void HandleRecord(String clientString, Socket socket, Byte[] bytes, UInt32 offset, UInt32 length)
        {
            if (expectedDataQueue.Count <= 0) Assert.Fail("Got record but was not expecting any");

            Data expectedData = expectedDataQueue.Dequeue();

            Assert.AreEqual(expectedData.length, length, "Record length did not match expected length");

            for(int i = 0; i < length; i++)
            {
                Assert.AreEqual(expectedData.bytes[expectedData.offset + i], bytes[offset + i],
                    String.Format("Expected {0} but got {1}", expectedData.SerializeObject(), bytes.SerializeObject()));
            }
        }
        public void Finish()
        {
            if (expectedDataQueue.Count > 0) Assert.Fail(String.Format("Still expected {0} records", expectedDataQueue.Count));
        }
    }

    [TestClass]
    public class RpcTest
    {
        [TestMethod]
        public void TestRpcRecordParser()
        {
            RecordChecker checker = new RecordChecker();
            RecordBuilder recordBuilder = new RecordBuilder(checker.HandleRecord);
            Byte[] record;

            //
            // Empty Record
            //
            checker.Add(new Byte[0], 0, 0);
            recordBuilder.HandleData(null, null, new Byte[] { 0x80, 0, 0, 0 }, 0, 4);
            checker.Finish();

            //
            // One Byte Record
            //
            record = new Byte[] { 0x80, 0, 0, 1, 0xF3 };
            checker.Add(new Byte[] { 0xF3 });
            recordBuilder.HandleData(null, null, record, 0, 5);

            record = new Byte[] { 0x80, 0, 0, 1, 0xA7 };
            checker.Add(new Byte[] { 0xA7 });
            recordBuilder.HandleData(null, null, record, 0, 4);
            recordBuilder.HandleData(null, null, record, 4, 5);
            checker.Finish();

            //
            // Two Byte Record
            //
            record = new Byte[] { 0x80, 0, 0, 2, 0x58, 0x28 };
            checker.Add(new Byte[] { 0x58, 0x28 });
            recordBuilder.HandleData(null, null, record, 0, 6);

            record = new Byte[] { 0x80, 0, 0, 2, 0x1C, 0x68 };
            checker.Add(new Byte[] { 0x1C, 0x68 });
            recordBuilder.HandleData(null, null, record, 0, 4);
            recordBuilder.HandleData(null, null, record, 4, 6);

            record = new Byte[] { 0x80, 0, 0, 2, 0xE2, 0x55 };
            checker.Add(new Byte[] { 0xE2, 0x55 });
            recordBuilder.HandleData(null, null, record, 0, 4);
            recordBuilder.HandleData(null, null, record, 4, 5);
            recordBuilder.HandleData(null, null, record, 5, 6);
            checker.Finish();

            //
            // 2 Records at a time
            //
            record = new Byte[] { 0x80, 0, 0, 2, 0x12, 0x34, 
                                  0x80, 0, 0, 4, 0x56, 0x78, 0x89, 0xAB };
            checker.Add(new Byte[] { 0x12, 0x34 });
            checker.Add(new Byte[] { 0x56, 0x78, 0x89, 0xAB });
            recordBuilder.HandleData(null, null, record, 0, 14);
            checker.Finish();

            //
            // Handle Partial Length
            //
            TestRecordBuilderWithAllLengths(checker, recordBuilder, new Byte[] {
                 0x80, 0, 0, 2, 0x12, 0x34, 
                 0x80, 0, 0, 4, 0x56, 0x78, 0x89, 0xAB 
            }, new Byte[] { 0x12, 0x34 }, new Byte[] { 0x56, 0x78, 0x89, 0xAB });

            TestRecordBuilderWithAllLengths(checker, recordBuilder, new Byte[] {
                 0x80, 0, 0, 10, 0xF1, 0x49, 0xEB, 0x11, 0xA1, 0xB4, 0xAF, 0x0F, 0x41, 0x04,
                 0x80, 0, 0, 0,
                 0x80, 0, 0, 3, 0xAA, 0xBB, 0xCC, 
            }, new Byte[] { 0xF1, 0x49, 0xEB, 0x11, 0xA1, 0xB4, 0xAF, 0x0F, 0x41, 0x04 }, new Byte[0], new Byte[] { 0xAA, 0xBB, 0xCC });

            checker.Finish();
        }
        void TestRecordBuilderWithAllLengths(RecordChecker checker, RecordBuilder recordBuilder, params Byte[][] dataAndExpectedRecords)
        {
            Byte[] data = dataAndExpectedRecords[0];

            for (UInt32 handleLength = 1; handleLength <= (UInt32)data.Length; handleLength++)
            {
                Console.WriteLine("HandleLength: {0}", handleLength);
                for (int i = 1; i < dataAndExpectedRecords.Length; i++)
                {
                    checker.Add(dataAndExpectedRecords[i]);
                }

                UInt32 totalDataBytesHandled = 0;
                while (true)
                {
                    Console.WriteLine("    Handling Offset: {0}", totalDataBytesHandled);
                    recordBuilder.HandleData(null, null, data, totalDataBytesHandled, totalDataBytesHandled + handleLength);
                    totalDataBytesHandled += handleLength;

                    if (totalDataBytesHandled + handleLength > data.Length) break;
                }

                UInt32 bytesLeft = (UInt32)data.Length - totalDataBytesHandled;
                if (bytesLeft > 0)
                {
                    Console.WriteLine("    Handling Offset: {0} Length: {1}", totalDataBytesHandled, bytesLeft);
                    recordBuilder.HandleData(null, null, data, totalDataBytesHandled, totalDataBytesHandled + bytesLeft);
                }

                checker.Finish();
            }
        }

    }
}
