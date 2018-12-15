using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TsBeautify.Data;

namespace TsBeautify.Tests
{
    [TestClass]
    public class BeautifierTests
    {
        [TestMethod]
        public void TestBug1()
        {
            var jsBeautifyOptions = new TsBeautifyOptions();
            jsBeautifyOptions.PreserveNewlines = false;
            var typescript = Bug1.TypeScript;
            typescript = new TsBeautifier(jsBeautifyOptions).Beautify(typescript);
        }

        [TestMethod]
        public void TestCSharpCoalesceOperator()
        {
            var result = Beautify(@"var callbackMatch = abc ?? def;");
            Assert.AreEqual(@"var callbackMatch = abc ?? def;", result);
        }

        [TestMethod]
        public void TestAtEscapedString()
        {
            var result = Beautify(@"var callbackMatch = Regex.Match(line, @""typedef [A-Za-z0-9_]+ [(]\*(?<Name>[A-Za-z0-9_]+)[)][(](?<Arguments>.*)[)];"");");
            Assert.AreEqual(@"var callbackMatch = Regex.Match(line, @""typedef [A-Za-z0-9_]+ [(]\*(?<Name>[A-Za-z0-9_]+)[)][(](?<Arguments>.*)[)];"");", result);
        }

        [TestMethod]
        public void TestCSharpInterpolatedString()
        {
            var result = Beautify(@"var callbackMatch = Regex.Match(line, $@""typedef {something} [A-Za-z0-9_]+ {somethingElse} [(]\*(?<Name>[A-Za-z0-9_]+)[)][(](?<Arguments>.*)[)];"");");
            Assert.AreEqual(@"var callbackMatch = Regex.Match(line, $@""typedef {something} [A-Za-z0-9_]+ {somethingElse} [(]\*(?<Name>[A-Za-z0-9_]+)[)][(](?<Arguments>.*)[)];"");", result);
        }

        [TestMethod]
        public void TestCSharpEscapedInterpolatedString()
        {
            var result = Beautify(@"var callbackMatch = Regex.Match(line, $@""typedef {{something}} [A-Za-z0-9_]+ {somethingElse} [(]\*(?<Name>[A-Za-z0-9_]+)[)][(](?<Arguments>.*)[)];"");");
            Assert.AreEqual(@"var callbackMatch = Regex.Match(line, $@""typedef {{something}} [A-Za-z0-9_]+ {somethingElse} [(]\*(?<Name>[A-Za-z0-9_]+)[)][(](?<Arguments>.*)[)];"");", result);
        }

        private static string Beautify(string code)
        {
            var beautifier = new TsBeautifier();
            var result = beautifier.Beautify(code);
            return result;
        }

        [TestMethod]
        public void TestKeepBracesOnNewLines()
        {
            var code = @"if (true)
                {
                    var x = 1;
                }";
            var beautifier = new TsBeautifier().Configure(x => x.OpenBlockOnNewLine = true);
            var result = beautifier.Beautify(code);
            Assert.AreEqual(@"if (true)
{
    var x = 1;
}", result);
        }

        [TestMethod]
        public void ClassNameWithGenericsShouldHaveSpace()
        {
            var code = @"class MyClass<T> extends MyOtherClass {
    SomeMethod(): string {
        let x = <string><any>"""";
        return <string><any>"""";
    }
}";
            var beautifier = new TsBeautifier().Configure(x => x.OpenBlockOnNewLine = true);
            var result = beautifier.Beautify(code);
            Assert.AreEqual(
                @"class MyClass<T> extends MyOtherClass
{
    SomeMethod() : string
    {
        let x = <string><any>"""";
        return <string><any>"""";
    }
}",
                result);
        }

        [TestMethod]
        public void TestSingleLineCommentShouldBeOnNewLine()
        {
            var code = @"if (true)
                {
                    // I am a comment
                    var x = 1; // An am an end-of-line comment
                    var y = /* I am an in-line comment */ 2;
                    // I am another comment
                }";
            var beautifier = new TsBeautifier().Configure(x => x.OpenBlockOnNewLine = true);
            var result = beautifier.Beautify(code);
            Assert.AreEqual(@"if (true)
{
    // I am a comment
    var x = 1; // An am an end-of-line comment
    var y = /* I am an in-line comment */ 2;
    // I am another comment
}", result);
        }

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
        public void TestMultipleGenerics()
        {
            var code =
                @"    private _propertiesMap: Dictionary<string, IProperty> = new Dictionary<string, IProperty>();";
            var beautifier = new TsBeautifier();
            var result = beautifier.Beautify(code);
            Assert.AreEqual(@"private _propertiesMap: Dictionary<string, IProperty> = new Dictionary<string, IProperty>();", result);
        }

        [TestMethod]
        public void TestTest()
        {
            var code =
                @"@test public async""Test1"" () {
}@test public async""Test2"" () {
}";
            var beautifier = new TsBeautifier();
            var result = beautifier.Beautify(code);
            Assert.AreEqual(@"@test public async ""Test1"" () {}
@test public async ""Test2"" () {}", result);
        }

        [TestMethod]
        public void TestLargeFile1()
        {
            var typescript = LargeFile.Input1;
            var beautifier = new TsBeautifier();
            var result = beautifier.Beautify(typescript);
            Assert.AreEqual(LargeFile.Input1Beautified, result);
        }

        [TestMethod]
        public void TestLargeFile2()
        {
            var typescript = LargeFile.Input2;
            var beautifier = new TsBeautifier();
            var result = beautifier.Beautify(typescript);
            Assert.AreEqual(LargeFile.Input2Beautified, result);
        }

        [TestMethod]
        public void Test456()
        {
            var typescript =
                @"let set = (<ITrackingSet><any>MethodInfoExtensions.InvokeGeneric(Enumerable.First(TypeInfo.GetRuntimeMethods(TrackingSetCollection )

, m => m.Name == <any>`TrackingSet`)

, this, new Array<Object>(), type));";
            var beautifier = new TsBeautifier();
            var result = beautifier.Beautify(typescript);
            Assert.AreEqual(@"let set = (<ITrackingSet><any>MethodInfoExtensions.InvokeGeneric(Enumerable.First(TypeInfo.GetRuntimeMethods(TrackingSetCollection), m => m.Name == <any>`TrackingSet`), this, new Array<Object>(), type));", result);
        }

        [TestMethod]
        public void TestImportModule()
        {
            var typescript = @"import ""module-alias/register"";
import { suite, test, slow, timeout } from ""mocha-typescript"";
import { assert } from ""chai"";import { __Iql_Tests_Types_typeLoaded, __Iql_Tests_Types_defer } from ""../../Iql.Tests.Types.Defer"";
import { TestsBase } from ""../TestsBase"";
";
            var beautifier = new TsBeautifier();
            var result = beautifier.Beautify(typescript);
            Assert.AreEqual(@"import ""module-alias/register"";
import { suite, test, slow, timeout } from ""mocha-typescript"";
import { assert } from ""chai"";
import { __Iql_Tests_Types_typeLoaded, __Iql_Tests_Types_defer } from ""../../Iql.Tests.Types.Defer"";
import { TestsBase } from ""../TestsBase"";", result);
        }

        [TestMethod]
        public void Test123()
        {
            var typescript =
                @"r=//
`^$`;";
            var beautifier = new TsBeautifier();
            var result = beautifier.Beautify(typescript);
            Assert.AreEqual(@"r = //
`^$`;", result);
        }

        [TestMethod]
        public void TestHugeConversion()
        {
            var typescript = TsBeautifyTs.Code;
            for (var i = 0; i < 1000; i++)
            {
                var beautifier = new TsBeautifier();
                var result = beautifier.Beautify(typescript);
            }
        }

        [TestMethod]
        public void TestHugeStringConversion()
        {
            var typescript = HugeString.TypeScript;
            var beautifier = new TsBeautifier();
            var result = beautifier.Beautify(typescript);
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
    return <string><any>key;
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
