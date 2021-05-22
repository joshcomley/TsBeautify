namespace TsBeautify.Tests
{
    public abstract class TestBase
    {
        protected static string Beautify(string typescript)
        {
            var beautifier = new TsBeautifier();
            var result = beautifier.Beautify(typescript);
            return result;
        }
    }
}