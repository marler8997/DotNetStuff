using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Marler.Hmd
{
    [TestClass]
    public class TokenizerTest
    {

        public void ValidateNextGlobalToken(String input, HmdGlobalTokenType type, UInt32 lines, String expectedText)
        {
            HmdTokenizer tokenizer = new HmdTokenizer(new StringReader(input), 0);

            HmdGlobalToken token = tokenizer.NextGlobalToken();

            Assert.AreEqual(lines, token.line);
            Assert.IsTrue(expectedText.Equals(token.text, StringComparison.CurrentCultureIgnoreCase));
        }

        /*
        private void ValidateTokens(String hmdData, params Token[] expectedTokens)
        {



        }


        [TestMethod]
        public void TestMethod1()
        {
            ValidateTokens("id:value;", new Token(TokenType.ID, 0, "id"), new Token(TokenType.Value, 0, "value"));
        }
        */
    }
}
