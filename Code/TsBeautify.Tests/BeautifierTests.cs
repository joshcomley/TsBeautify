using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TsBeautify.Tests
{
    [TestClass]
    public class BeautifierTests
    {
        [TestMethod]
        public void TestSingleLineWhitespaceCleaning()
        {
            var typescript = "let   x: string    = `something` ;";
            var beautifier = new TsBeautifier();
            var result = beautifier.Beautify(typescript);
            Assert.AreEqual(@"let x: string = `something`;", result);
        }

        [TestMethod]
        public void TestStringInterpolation()
        {
            var typescript = @"let x = `${myVar}`;
let y = 7;";
            //typescript = "let x = `a`;";
            var beautifier = new TsBeautifier();
            var result = beautifier.Beautify(typescript);
            Assert.AreEqual(typescript, result);
        }

        [TestMethod]
        public void TestFunctionShortcut()
        {
            var typescript = @"f(p => { });";
            //typescript = "let x = `a`;";
            var beautifier = new TsBeautifier();
            var result = beautifier.Beautify(typescript);
            Assert.AreEqual(@"f(p => {});", result);
        }

        [TestMethod]
        public void TestGenerics()
        {
            var typescript = @"f<Array<string>>(p => { });";
            var beautifier = new TsBeautifier();
            var result = beautifier.Beautify(typescript);
            Assert.AreEqual(@"f<Array<string>>(p => {});", result);
        }

        [TestMethod]
        public void TestImports()
        {
            var typescript = @"import   { PropertyInfo ,   Type, Interface, TypeInfo} from ""@brandless/tsutility"";";
            var beautifier = new TsBeautifier();
            var result = beautifier.Beautify(typescript);
            Assert.AreEqual(@"import { PropertyInfo, Type, Interface, TypeInfo } from ""@brandless/tsutility"";", result);
        }
    }
}
