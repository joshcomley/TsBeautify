using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TsBeautify.Data;

namespace TsBeautify.Tests
{
    [TestClass]
    public class BeautifierTests
    {
        [TestMethod]
        public void TestDivide()
        {
            var typescript = @"
                if (pageSize > 0)
                {
                    page = skippedSoFar / pageSize;
                }

                var pageCount = 0;
                var i = totalCount;
                while (i > 0)
                {
                    pageCount++;
                    i -= pageSize;
                }

                dbList.PagingInfo = new PagingInfo(skippedSoFar, totalCount, pageSize, page, pageCount);";
            var beautifier = new TsBeautifier();
            var result = beautifier.Beautify(typescript);
            Assert.AreEqual(@"if (pageSize > 0) {
    page = skippedSoFar / pageSize;
}
var pageCount = 0;
var i = totalCount;
while (i > 0) {
    pageCount++;
    i -= pageSize;
}
dbList.PagingInfo = new PagingInfo(skippedSoFar, totalCount, pageSize, page, pageCount);", result);
        }

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

        [TestMethod]
        public void TestLargeFile()
        {
            var typescript = LargeFile.Inputxx;
            var beautifier = new TsBeautifier();
            var result = beautifier.Beautify(typescript);
            Assert.AreEqual(LargeFile.Beautified, result);
        }

        [TestMethod]
        public void TestKeyWordInLiteral()
        {
            var typescript = @"        switch (type) {
            case ""number"":
                return """" + key;
            case ""string"":
                return <string><any>key;
            case ""function"": {
                let name = key[""ClassName""] || key[""Name""] || key[""name""];//TypeInfo.NameOf(key);
                if (!name) {
                    name = TypeInfo.NameOf(key);
                }
                if (name) {
                    return name;
                }
            }
                break;
}";
            var beautifier = new TsBeautifier();
            var result = beautifier.Beautify(typescript);
            Assert.AreEqual(@"switch (type) {
case ""number"":
    return """" + key;
case ""string"":
    return<string><any>key;
case ""function"":
    {
        let name = key[""ClassName""] || key[""Name""] || key[""name""]; //TypeInfo.NameOf(key);
        if (!name) {
            name = TypeInfo.NameOf(key);
        }
        if (name) {
            return name;
        }
    }
    break;
}", result);
        }
    }
}
