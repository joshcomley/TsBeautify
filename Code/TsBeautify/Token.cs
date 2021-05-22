namespace TsBeautify
{
    internal class Token
    {
        public string Value { get; set; }
        public TokenType TokenType { get; set; }
        public int NewLineCount { get; set; }

        public Token(string token, TokenType tokenType, int newLineCount = 0)
        {
            Value = token;
            TokenType = tokenType;
            NewLineCount = newLineCount;
        }
    }
}