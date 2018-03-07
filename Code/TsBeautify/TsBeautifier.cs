namespace TsBeautify
{
    public class TsBeautifier
    {
        public TsBeautifyOptions Options { get; set; }
        public TsBeautifier(TsBeautifyOptions options = null)
        {
            Options = options ?? new TsBeautifyOptions();
        }

        public string Beautify(string typescript)
        {
            try
            {
                var parser = new TsBeautifierInstance(typescript, Options);
                return parser.Beautify();
            }
            catch
            {
                return typescript;
            }
        }
    }
}