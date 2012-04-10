using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.Hmd
{
    /// <summary>
    /// Summary description for HmdIDPropertiesTest
    /// </summary>
    [TestClass]
    public class HmdIDPropertiesTest
    {
        [TestMethod]
        public void HmdIDPropertiesIDTest()
        {
            HmdBlockIDProperties testRoot = new HmdBlockIDProperties("The_Test_Root", new StaticCount(1), null);

            Assert.AreEqual(null, testRoot.definitionContext);
            Assert.AreEqual("the_test_root", testRoot.idLowerCase);
            Assert.AreEqual("the_test_root", testRoot.idWithContext);

            HmdBlockIDProperties level0Block = new HmdBlockIDProperties("Level0Block", UnrestrictedCount.Instance, testRoot);

            Assert.AreEqual(String.Empty, level0Block.definitionContext);
            Assert.AreEqual("level0block", level0Block.idLowerCase);
            Assert.AreEqual("level0block", level0Block.idWithContext);

            HmdBlockIDProperties level1Block = new HmdBlockIDProperties("Level1Block", UnrestrictedCount.Instance, level0Block);

            Assert.AreEqual("level0block", level1Block.definitionContext);
            Assert.AreEqual("level1block", level1Block.idLowerCase);
            Assert.AreEqual("level0block.level1block", level1Block.idWithContext);

            HmdBlockIDProperties level2Block = new HmdBlockIDProperties("Level2Block", UnrestrictedCount.Instance, level1Block);

            Assert.AreEqual("level0block.level1block", level2Block.definitionContext);
            Assert.AreEqual("level2block", level2Block.idLowerCase);
            Assert.AreEqual("level0block.level1block.level2block", level2Block.idWithContext);


        }
    }
}
