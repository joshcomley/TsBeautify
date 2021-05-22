using System.Collections.Generic;
using System.Text;

namespace TsBeautify
{
    internal class State
    {
        public StringBuilder Output { get; set; }
        public string LastText { get; set; }
        public TokenType LastType { get; set; }
        public Token Token { get; set; }
        public string TokenText { get; set; }
        public TsMode CurrentMode { get; set; }
        public bool DoBlockJustClosed { get; set; }
        public int GenericsDepth { get; set; }
        public bool IfLineFlag { get; set; }
        public bool IsImportBlock { get; set; }
        public char SecondToLastChar { get; set; }
        public int SecondToLastCharIndex { get; set; } = -1;
        public int CurrentInputCharIndex { get; set; } = -1;
        public int ParserPos { get; set; }
        public int IndentLevel { get; set; }
        public Stack<TsMode> Modes { get; } = new Stack<TsMode>();
        public StaticState StaticState { get; set; }
        private char _currentInputChar; 
        public char CurrentInputChar
        {
            get
            {
                if (CurrentInputCharIndex != ParserPos)
                {
                    CurrentInputCharIndex = ParserPos;
                    _currentInputChar = StaticState.Input[ParserPos];
                }

                return _currentInputChar;
            }
        }

        public bool InGenerics => GenericsDepth > 0;

        public State(StaticState staticState)
        {
            StaticState = staticState;
            Modes.Push(CurrentMode);
            Output = new StringBuilder();
            Token = new Token(null, TokenType.BlockComment);
            LastType = TokenType.StartExpression; // last token type
            LastText = ""; // last token text
            // states showing if we are currently in expression (i.e. "if" case) - 'EXPRESSION', or in usual block (like, procedure), 'BLOCK'.
            // some formatting depends on that.
            CurrentMode = TsMode.Block;
            ParserPos = 0;
            DoBlockJustClosed = false;
            IndentLevel = StaticState.Options.IndentLevel ?? 0;
        }

        public State Clone()
        {
            return new State(StaticState)
            {
                LastText = LastText,
                LastType = LastType,
                Token = Token,
                TokenText = TokenText,
                CurrentMode = CurrentMode,
                DoBlockJustClosed = DoBlockJustClosed,
                GenericsDepth = GenericsDepth,
                IfLineFlag = IfLineFlag,
                IsImportBlock = IsImportBlock,
                SecondToLastChar = SecondToLastChar,
                SecondToLastCharIndex = SecondToLastCharIndex,
                CurrentInputCharIndex = CurrentInputCharIndex,
                ParserPos = ParserPos,
                StaticState = StaticState,
                IndentLevel= IndentLevel,
                Output = new StringBuilder(Output.ToString())
            };
        }
    }
}