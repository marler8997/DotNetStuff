using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using More.Net;

namespace Nfs.Test
{
    [TestClass]
    public class NfsPathTest
    {
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
