using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.Hmd
{
    [TestClass]
    public class PropertyParserTest
    {

        [TestMethod]
        public void TestGeneralProperties()
        {
            HmdValueIDProperties valueIDProperties;
            ICountProperty defaultCountProperty = UnrestrictedCount.Instance;
            HmdType defaultHmdType = HmdType.String;

            //valueIDProperties = HmdParser.ParseValueProperties("test", "0-* enum(hey what)");
            //TestUtil.VerifyValue(TestU, defaultCountProperty, IDProperties.rootParentReference, HmdType.Enumeration, "heywhat");

        }
    }
}
