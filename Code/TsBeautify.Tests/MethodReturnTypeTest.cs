using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TsBeautify.Tests
{
    [TestClass]
    public class MethodReturnTypeTest : TestBase
    {
        [TestMethod]
        public void TestMethodReturnType()
        {
            Assert.AreEqual(@"x(): y {}", Beautify(@"x()  : y {}"));
        }
        
        [TestMethod]
        public void TestMethodComplexReturnType()
        {
            Assert.AreEqual(@"x(): X<Y<Z>> {}", Beautify(@"x()  : X<Y<Z>> {}"));
        }
    }
}