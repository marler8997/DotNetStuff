using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Marler.Hmd
{
    [TestClass]
    public class HmdSingleIDTokenizerTest
    {
        [TestMethod]
        public void SimpleTest()
        {
            String testString = "ABlockID { AValueID: value; AnotherValue: value2; } OtherValue: v; ";

            HmdSingleIDTokenizer tokenizer = new HmdSingleIDTokenizer(new StringReader(testString));
            HmdBlockID root = new HmdBlockID(String.Empty, null);
            HmdFileParser.Parse(root, tokenizer, null, null);


            Assert.AreEqual(1, root.ChildCount);
            Assert.IsTrue(root.GetChild(0).isBlock);

            Assert.AreEqual("ABlockID", root.GetChild(0).idOriginalCase);

            HmdBlockID childBlock = root.GetChild(0).CastAsBlockID;

            Assert.AreEqual(2, childBlock.ChildCount);

            Assert.IsFalse(childBlock.GetChild(0).CastAsValueID.isBlock);
            Assert.AreEqual("AValueID", childBlock.GetChild(0).idOriginalCase);
            Assert.AreEqual(" value", childBlock.GetChild(0).CastAsValueID.value);

            Assert.IsFalse(childBlock.GetChild(1).CastAsValueID.isBlock);
            Assert.AreEqual("AnotherValue", childBlock.GetChild(1).idOriginalCase);
            Assert.AreEqual(" value2", childBlock.GetChild(1).CastAsValueID.value);


            root = new HmdBlockID(String.Empty, null);
            tokenizer.Reset();
            HmdFileParser.Parse(root, tokenizer, null, null);

            Assert.AreEqual(1, root.ChildCount);
            Assert.IsFalse(root.GetChild(0).isBlock);

            Assert.AreEqual("OtherValue", root.GetChild(0).idOriginalCase);
            Assert.AreEqual(" v", root.GetChild(0).CastAsValueID.value);
        }
    }
}
