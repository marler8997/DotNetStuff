using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using More;
using More.Net;

namespace Nfs.Test
{
    [TestClass]
    public class NfsPathTest
    {
        /*
        [TestMethod]
        public void PerformanceTest()
        {
            Byte[] buffer = new Byte[FileAttributes.FixedSerializationLength];

            FileAttributes attributes = new FileAttributes();
            attributes.lastAccessTime = new Time(0, 0);
            attributes.lastAttributeModifyTime = new Time(0, 0);
            attributes.lastModifyTime = new Time(0, 0);
            More.Net.Nfs3Procedure.FileAttributes attributes2 = new More.Net.Nfs3Procedure.FileAttributes();
            attributes2.lastAccessTime = new More.Net.Nfs3Procedure.Time(0, 0);
            attributes2.lastAttributeModifyTime = new More.Net.Nfs3Procedure.Time(0, 0);
            attributes2.lastModifyTime = new More.Net.Nfs3Procedure.Time(0, 0);

            long before;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                FileAttributes.Serializer.FixedLengthSerialize(buffer, 0, attributes);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));


            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                More.Net.Nfs3Procedure.FileAttributes.memberSerializers.Serialize(attributes2, buffer, 0);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                FileAttributes.Serializer.FixedLengthSerialize(buffer, 0, attributes);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));


            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                More.Net.Nfs3Procedure.FileAttributes.memberSerializers.Serialize(attributes2, buffer, 0);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
        }
        */



        [TestMethod]
        public void TestLeafName()
        {
            Assert.AreEqual(null, NfsPath.LeafName(null));
            Assert.AreEqual(null, NfsPath.LeafName(""));
            Assert.AreEqual(null, NfsPath.LeafName("/"));
            Assert.AreEqual("a" , NfsPath.LeafName( "a"));
            Assert.AreEqual("a" , NfsPath.LeafName("/a"));
            Assert.AreEqual("a" , NfsPath.LeafName( "a/"));
            Assert.AreEqual("a" , NfsPath.LeafName("/a/"));

            Assert.AreEqual("leafname", NfsPath.LeafName("leafname"));
            Assert.AreEqual("leafname", NfsPath.LeafName("/leafname"));
            Assert.AreEqual("leafname", NfsPath.LeafName("leafname/"));
            Assert.AreEqual("leafname", NfsPath.LeafName("/leafname/"));

            Assert.AreEqual("a", NfsPath.LeafName("leafname/a"));
            Assert.AreEqual("a", NfsPath.LeafName("leafname/a/"));
            Assert.AreEqual("a", NfsPath.LeafName("/leafname/a"));
            Assert.AreEqual("a", NfsPath.LeafName("/leafname/a/"));
        }
        [TestMethod]
        public void TestSplitShareNameAndSubPath()
        {
            String subPath;

            Assert.AreEqual(null, NfsPath.SplitShareNameAndSubPath(null, out subPath));
            Assert.AreEqual(null, subPath);

            Assert.AreEqual(null, NfsPath.SplitShareNameAndSubPath("", out subPath));
            Assert.AreEqual(null, subPath);

            Assert.AreEqual(null, NfsPath.SplitShareNameAndSubPath("/", out subPath));
            Assert.AreEqual(null, subPath);

            Assert.AreEqual("a", NfsPath.SplitShareNameAndSubPath("a", out subPath));
            Assert.AreEqual(null, subPath);

            Assert.AreEqual("a", NfsPath.SplitShareNameAndSubPath("/a", out subPath));
            Assert.AreEqual(null, subPath);

            Assert.AreEqual("a", NfsPath.SplitShareNameAndSubPath("a/", out subPath));
            Assert.AreEqual(null, subPath);

            Assert.AreEqual("a", NfsPath.SplitShareNameAndSubPath("/a/", out subPath));
            Assert.AreEqual(null, subPath);

            Assert.AreEqual("sharename", NfsPath.SplitShareNameAndSubPath("sharename", out subPath));
            Assert.AreEqual(null, subPath);

            Assert.AreEqual("sharename", NfsPath.SplitShareNameAndSubPath("/sharename", out subPath));
            Assert.AreEqual(null, subPath);

            Assert.AreEqual("sharename", NfsPath.SplitShareNameAndSubPath("sharename/", out subPath));
            Assert.AreEqual(null, subPath);

            Assert.AreEqual("sharename", NfsPath.SplitShareNameAndSubPath("/sharename/", out subPath));
            Assert.AreEqual(null, subPath);

            Assert.AreEqual("sharename", NfsPath.SplitShareNameAndSubPath("sharename/a", out subPath));
            Assert.AreEqual("a", subPath);

            Assert.AreEqual("sharename", NfsPath.SplitShareNameAndSubPath("sharename/a/", out subPath));
            Assert.AreEqual("a", subPath);

            Assert.AreEqual("sharename", NfsPath.SplitShareNameAndSubPath("sharename/a", out subPath));
            Assert.AreEqual("a", subPath);

            Assert.AreEqual("sharename", NfsPath.SplitShareNameAndSubPath("/sharename/a/", out subPath));
            Assert.AreEqual("a", subPath);

            Assert.AreEqual("sharename", NfsPath.SplitShareNameAndSubPath("/sharename/a/b", out subPath));
            Assert.AreEqual("a/b", subPath);

            Assert.AreEqual("sharename", NfsPath.SplitShareNameAndSubPath("/sharename/a/b/", out subPath));
            Assert.AreEqual("a/b", subPath);
        }
    }
}
