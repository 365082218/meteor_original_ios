//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using NUnit.Framework;
using A = NUnit.Framework.Assert;

namespace Outfit7.Util {

    public class StringUtilsTest {

        [Test]
        public void TestUnQuote() {
            A.AreEqual("Joe \"faccia di polpo\"", StringUtils.UnQuote("\"Joe \"faccia di polpo\"\""));
            A.AreEqual("Joe \"faccia di polpo\"", StringUtils.UnQuote("    \"Joe \"faccia di polpo\"\""));
            A.AreEqual("Joe \"faccia di polpo\"", StringUtils.UnQuote("\"Joe \"faccia di polpo\"\"    "));
            A.AreEqual("Joe \"faccia di polpo\"", StringUtils.UnQuote("     \"Joe \"faccia di polpo\"\"    "));
            A.AreEqual("Joe \"faccia di polpo\"", StringUtils.UnQuote("\"Joe \"\"faccia di polpo\"\"\""));
            A.AreEqual("Joe \"faccia di polpo\"", StringUtils.UnQuote("Joe \"faccia di polpo\""));
            A.AreEqual("\"Joe \"faccia di polpo\"\"", StringUtils.UnQuote("\"\"Joe \"\"faccia di polpo\"\"\"\""));
            A.AreEqual("Joe", StringUtils.UnQuote("\"Joe\""));
            A.AreEqual("\"\"", StringUtils.UnQuote("\"\"\"\"\"\""));
            A.AreEqual("\"", StringUtils.UnQuote("\"\"\"\""));
            A.AreEqual("", StringUtils.UnQuote("\"\""));
            A.AreEqual("", StringUtils.UnQuote("\""));
            A.AreEqual("", StringUtils.UnQuote(""));
            A.AreEqual("", StringUtils.UnQuote("    "));
            A.AreEqual("Joe \"faccia di\npolpo\"", StringUtils.UnQuote("\"Joe \"faccia di\npolpo\"\""));
            A.AreEqual("Joe \"faccia di\npolpo\"", StringUtils.UnQuote("\"Joe \"faccia di\\npolpo\"\""));
        }
    }
}
