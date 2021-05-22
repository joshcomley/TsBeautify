namespace TsBeautify
{
    internal enum TokenType
    {
        Comment,
        Operator,
        Unknown,
        EndBlock,
        EndOfFile,
        StartExpression,
        StartBlock,
        EndExpression,
        Word,
        StartImport,
        EndImport,
        SemiColon,
        String,
        Generics,
        BlockComment
    }
}