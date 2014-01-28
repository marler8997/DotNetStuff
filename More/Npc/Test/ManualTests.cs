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
    public NpcServerSingleThreaded server;
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
        public void ManualTest()
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
    }
}
