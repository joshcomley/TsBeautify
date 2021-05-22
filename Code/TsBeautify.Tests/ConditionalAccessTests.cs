using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TsBeautify.Tests
{
    [TestClass]
    public class ConditionalAccessTests : TestBase
    {
        [TestMethod]
        public void TestConditionalAccess()
        {
            Assert.AreEqual(@"o?.x;", Beautify(@"o?.x;"));
        }
    }
}