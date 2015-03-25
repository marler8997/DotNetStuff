using System;
using System.Collections.Generic;
using System.Net.Sockets;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using More;
using More.Net;

[TestClass]
public class RemoteDataTests
{
    class RecordHandler
    {
        public void HandleRecord(String clientString, Socket socket, Byte[] bytes, UInt32 offset, UInt32 length)
        {
        }
    }


    void SendRecord(Socket socket, Byte[] bytes, UInt32 offsetLimit)
    {
        bytes.BigEndianSetUInt32(0, offsetLimit - 4);
        bytes[0] = (Byte)(bytes[0] | 0x80);
        socket.Send(bytes, 0, (Int32)offsetLimit, SocketFlags.None);
    }

    UInt32 ReadRecord(Socket socket, Byte[] bytes)
    {
        socket.ReadFullSize(bytes, 0, 4);
        UInt32 size = bytes.BigEndianReadUInt32(0);
        socket.ReadFullSize(bytes, 0, (Int32)size);
        return size;
    }
    /*
    [TestMethod]
    public void TestMethod1()
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(EndPoints.EndPointFromIPOrHost("localhost", RemoteData.DefaultPort));
        
        Byte[] bytes = new Byte[1024];
        UInt32 offset;

        offset = 4;
        bytes[offset++] = RemoteData.List;
        offset = bytes.SetVarUInt32(offset, 0); // 0 is the root directory object id
        SendRecord(socket, bytes, offset);

        UInt32 recordLength = ReadRecord(socket, bytes);

        ObjectEntry[] entries;
        offset = ObjectEntries.DeserializeObjects(bytes, 0, out entries);
        Assert.AreEqual(recordLength, offset);

        for (int i = 0; i < entries.Length; i++)
        {
            Console.WriteLine(entries[i]);
        }
    }
    */
}
