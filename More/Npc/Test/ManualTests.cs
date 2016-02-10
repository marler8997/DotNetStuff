using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using More;

[NpcInterface]
interface INpcServerControl
{
    void Stop();
}
class NpcServerControl : INpcServerControl
{
    public IDisposable server;
    public NpcServerControl()
    {
    }
    public void Stop()
    {
        server.Dispose();
    }
}
namespace More.Npc.Test
{
    [TestClass]
    public class ManualTests
    {
        //[TestMethod]
        public void RunSingleThreadServer()
        {
            NpcServerControl npcServerControl = new NpcServerControl();
            NpcReflector reflector = new NpcReflector(
                new TestRemoteDevice(),
                npcServerControl,
                new NpcExecutionObject(new TestRemoteDevice(), "Other", null, null),
                new NpcExecutionObject(new NpcMethodsForTest(), "Test", null, null),
                new InheritBothRoots());

            NpcServerSingleThreaded server = new NpcServerSingleThreaded(
                new NpcServerLoggerCallback(Console.Out),
                reflector,
                new DefaultNpcHtmlGenerator("Test", reflector),
                1234);

            npcServerControl.server = server;

            server.Run();
        }
        //[TestMethod]
        public void RunMultiThreadedServer()
        {
            NpcServerControl npcServerControl = new NpcServerControl();
            NpcReflector reflector = new NpcReflector(
                new TestRemoteDevice(),
                npcServerControl,
                new NpcExecutionObject(new TestRemoteDevice(), "Other", null, null),
                new NpcExecutionObject(new NpcMethodsForTest(), "Test", null, null),
                new InheritBothRoots());

            NpcServerMultiThreaded server = new NpcServerMultiThreaded(
                new NpcServerLoggerCallback(Console.Out),
                reflector,
                new DefaultNpcHtmlGenerator("Test", reflector),
                1234);

            npcServerControl.server = server;

            server.Run();
        }
        /*
        [TestMethod]
        public void ConvertFavIconToByteArrayCode()
        {
            Byte[] bytes = System.IO.File.ReadAllBytes("C:\\temp\\favicon.ico");
            int offset = 0;
            while (offset < bytes.Length)
            {
                for (int i = 0; i < 16; i++)
                {
                    if (offset >= bytes.Length)
                        break;

                    if (i > 0)
                    {
                        Console.Write(", ");
                    }

                    Console.Write("0x{0:X2}", bytes[offset]);
                    offset++;
                }
                Console.WriteLine(",");
            }
        }
        */
    }
}
