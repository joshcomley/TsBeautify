using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TsBeautify.Tests
{
    [TestClass]
    public class FullTest : TestBase
    {
        [TestMethod]
        public void ReturnShouldNeverBeFollowedByANewLine()
        {
            var result = Beautify(@"export  class    MyClass{
public doSomething(obj: SomeObj): void {
const x = obj?.Name;
}
 }");
            Assert.AreEqual(@"export class MyClass {
    public doSomething(obj: SomeObj) : void {
        const x = obj?.Name;
    }
}", result);
        }
    }
}