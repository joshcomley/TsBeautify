namespace TsBeautify
{
    public class TsBeautifyOptions
    {
        public bool OpenBlockOnNewLine { get; set; }
        public int? IndentSize { get; set; }
        public char? IndentChar { get; set; }
        public int? IndentLevel { get; set; }
        public bool? PreserveNewlines { get; set; } = false;
    }
}