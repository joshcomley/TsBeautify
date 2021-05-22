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

        [TestMethod]
        public void TestConditionalAccess2()
        {
            Assert.AreEqual(@"private Test(obj: SomeObject | null): void {
    let x = obj?.Name;
}", Beautify(@"private Test(obj: SomeObject | null) : void {
    let x = obj ? .Name;
}"));
        }
    }
}