using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using More;

class AnObjectType
{
    public DayOfWeek day;
    public String name;
    public Int32[] integers;
    public AnObjectType()
    {
    }
    public AnObjectType(DayOfWeek day, String name, Int32[] integers)
    {
        this.day = day;
        this.name = name;
        this.integers = integers;
    }
}
[NpcInterface]
interface INpcMethodsForTest
{
    void EmptyCall();

    void SetDay(DayOfWeek day);
    DayOfWeek GetDay();

    void SetIntegers(Int32[] integers);
    Int32[] GetIntegers();

    void SetCustomObjects(AnObjectType[] objects);
    AnObjectType[] GetCustomObjects();
}
class NpcMethodsForTest : INpcMethodsForTest
{
    public void EmptyCall()
    {
    }
    public DayOfWeek day;
    public void SetDay(DayOfWeek day)
    {
        this.day = day;
    }
    public DayOfWeek GetDay()
    {
        return day;
    }
    public Int32[] integers;
    public void SetIntegers(Int32[] integers)
    {
        this.integers = integers;
    }
    public Int32[] GetIntegers()
    {
        return integers;
    }
    public AnObjectType[] objects;
    public void SetCustomObjects(AnObjectType[] objects)
    {
        this.objects = objects;
    }
    public AnObjectType[] GetCustomObjects()
    {
        return objects;
    }
}
namespace More
{

    [TestClass]
    public class HighLevelRegressionTests
    {


        const Int32 TestTcpPort = 65508;

        [TestMethod]
        public void SystemTest1()
        {
            NpcMethodsForTest npcMethodsForTest = new NpcMethodsForTest();
            NpcReflector reflector = new NpcReflector(npcMethodsForTest);

            NpcServerSingleThreaded server = new NpcServerSingleThreaded(NpcServerConsoleLoggerCallback.Instance,
                    reflector, new DefaultNpcHtmlGenerator("Npc", reflector), TestTcpPort);

            Thread serverThread = new Thread(server.Run);
            serverThread.Start();
            Thread.Sleep(100);

            NpcClient client = new NpcClient(new IPEndPoint(IPAddress.Loopback, TestTcpPort), false);

            client.Call("NpcMethodsForTest.EmptyCall");
            client.Call("NpcMethodsForTest.EmptyCall");
            client.Call("NpcMethodsForTest.EmptyCall");
            client.Call("NpcMethodsForTest.EmptyCall");

            client.Call("NpcMethodsForTest.SetDay", DayOfWeek.Tuesday);
            Assert.AreEqual(DayOfWeek.Tuesday, client.Call("NpcMethodsForTest.GetDay"));

            Int32[] integers = new Int32[] { 1, 2, 3, 1, 2, 0404, 8281, 3020, -1883, 0211 };
            client.Call("NpcMethodsForTest.SetIntegers", integers);
            Assert.IsNull(integers.Diff(client.Call("NpcMethodsForTest.GetIntegers")));

            AnObjectType[] customObjects = new AnObjectType[] {
                new AnObjectType(DayOfWeek.Friday, "A random name", new Int32[]{Int32.MinValue, Int32.MaxValue, 0}),
            };

            client.Call("NpcMethodsForTest.SetCustomObjects", new Object[] {customObjects});
            Assert.IsNull(customObjects.Diff(client.Call("NpcMethodsForTest.GetCustomObjects")));

            server.Dispose();
            serverThread.Join();
        }

        [TestMethod]
        public void ServerRestartTest()
        {
            NpcMethodsForTest npcMethodsForTest = new NpcMethodsForTest();
            NpcReflector reflector = new NpcReflector(npcMethodsForTest);

            NpcServerSingleThreaded server = new NpcServerSingleThreaded(NpcServerConsoleLoggerCallback.Instance,
                    reflector, new DefaultNpcHtmlGenerator("Npc", reflector), TestTcpPort);
            Thread serverThread = new Thread(server.Run);
            serverThread.Start();

            Thread.Sleep(100);

            NpcClient client = new NpcClient(new IPEndPoint(IPAddress.Loopback, TestTcpPort), false);

            client.Call("NpcMethodsForTest.EmptyCall");
            client.Call("NpcMethodsForTest.EmptyCall");
            client.Call("NpcMethodsForTest.EmptyCall");
            client.Call("NpcMethodsForTest.EmptyCall");

            client.Call("NpcMethodsForTest.SetDay", DayOfWeek.Tuesday);
            Assert.AreEqual(DayOfWeek.Tuesday, client.Call("NpcMethodsForTest.GetDay"));

            Int32[] integers = new Int32[] { 1, 2, 3, 1, 2, 0404, 8281, 3020, -1883, 0211 };
            client.Call("NpcMethodsForTest.SetIntegers", integers);
            Assert.IsNull(integers.Diff(client.Call("NpcMethodsForTest.GetIntegers")));

            AnObjectType[] customObjects = new AnObjectType[] {
                new AnObjectType(DayOfWeek.Friday, "A random name", new Int32[]{Int32.MinValue, Int32.MaxValue, 0}),
            };

            client.Call("NpcMethodsForTest.SetCustomObjects", new Object[] { customObjects });
            Assert.IsNull(customObjects.Diff(client.Call("NpcMethodsForTest.GetCustomObjects")));

            //
            // Restart the Server
            //
            Console.WriteLine("Resetting server");
            server.Dispose();
            serverThread.Join();
            
            server = new NpcServerSingleThreaded(NpcServerConsoleLoggerCallback.Instance,
                reflector, new DefaultNpcHtmlGenerator("Npc", reflector), TestTcpPort);
            serverThread = new Thread(server.Run);
            serverThread.Start();

            Thread.Sleep(100);

            client.Call("NpcMethodsForTest.EmptyCall");
            client.Call("NpcMethodsForTest.EmptyCall");

            server.Dispose();
            serverThread.Join();
        }
    }
}
