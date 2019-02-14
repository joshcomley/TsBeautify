using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TsBeautify.Extensions;

namespace TsBeautify
{
    internal class TsBeautifierInstance
    {
        private const string WhitespaceChars = "\n\r\t ";
        private const string WordChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_$";
        private const string Chars = "0123456789";
        private const string GenericsChars = "<>";
        private readonly Dictionary<string, bool> _digits;
        private readonly Dictionary<char, bool> _digitsChars;
        private readonly Dictionary<string, bool> _generics;
        private readonly Dictionary<string, bool> _genericsBrackets;
        private readonly Dictionary<char, bool> _genericsBracketsChars;
        private readonly Dictionary<char, bool> _genericsChars;
        private readonly string _indentString;
        private readonly string _input;
        private readonly Dictionary<string, bool> _lineStarters;

        private readonly Stack<TsMode> _modes;
        private readonly TsBeautifyOptions _options;
        private readonly bool _optPreserveNewlines;
        private readonly StringBuilder _output;
        private readonly Dictionary<string, bool> _punctuation;
        private readonly Dictionary<string, bool> _whitespace;
        private readonly Dictionary<char, bool> _whitespaceChars;
        private readonly Dictionary<string, bool> _wordchar;
        private readonly Dictionary<char, bool> _wordcharChars;

        private TsMode _currentMode;
        private bool _doBlockJustClosed;
        private int _genericsDepth;
        private bool _ifLineFlag;
        private int _indentLevel;
        private bool _isImportBlock;
        private char _secondToLastChar;
        private int _secondToLastCharIndex = -1;
        private char _currentInputChar;
        private int _currentInputCharIndex = -1;
        private string _lastText;
        private TokenType _lastType;
        private int _parserPos;

        private Token _token;
        private string _tokenText;

        public TsBeautifierInstance(string jsSourceText, TsBeautifyOptions options = null, bool interpolation = false)
        {
            _token = new Token(null, TokenType.BlockComment);
            Interpolation = interpolation;
            options = options ?? new TsBeautifyOptions();
            _options = options;
            var optIndentSize = options.IndentSize ?? 4;
            var optIndentChar = options.IndentChar ?? ' ';
            var optIndentLevel = options.IndentLevel ?? 0;
            _optPreserveNewlines = options.PreserveNewlines ?? true;
            _output = new StringBuilder();
            _modes = new Stack<TsMode>();

            _indentString = "";

            while (optIndentSize > 0)
            {
                _indentString += optIndentChar;
                optIndentSize -= 1;
            }

            _indentLevel = optIndentLevel;


            _input = jsSourceText;

            _lastType = TokenType.StartExpression; // last token type
            _lastText = ""; // last token text

            _doBlockJustClosed = false;

            _whitespace = ToLookup(WhitespaceChars);
            _whitespaceChars = ToCharLookup(_whitespace);
            _wordchar = ToLookup(WordChars);
            _digits = ToLookup(Chars);
            _genericsBrackets = ToLookup(GenericsChars);
            _generics = ToLookup(WhitespaceChars + WordChars + "," + GenericsChars);
            _digitsChars = ToCharLookup(_digits);
            _wordcharChars = ToCharLookup(_wordchar);
            _genericsChars = ToCharLookup(_generics);
            _genericsBracketsChars = ToCharLookup(_genericsBrackets);
            // <!-- is a special case (ok, it's a minor hack actually)
            _punctuation =
                ArrayToLookup(
                    "=> + - * / % & ++ -- = += -= *= /= %= == === != !== > < >= <= >> << >>> >>>= >>= <<= && &= | || ?? ! !! , : ? ^ ^= |= ::"
                        .Split(' '));

            // words which should always start on new line.
            _lineStarters =
                ArrayToLookup(
                    "@test,import,let,continue,try,throw,return,var,if,switch,case,default,for,while,break,function"
                        .Split(','));

            // states showing if we are currently in expression (i.e. "if" case) - 'EXPRESSION', or in usual block (like, procedure), 'BLOCK'.
            // some formatting depends on that.
            _currentMode = TsMode.Block;
            _modes.Push(_currentMode);

            _parserPos = 0;
        }

        public bool Interpolation { get; }

        private bool InGenerics => _genericsDepth > 0;

        public char SecondToLastChar
        {
            get
            {
                if (_secondToLastCharIndex != _output.Length)
                {
                    _secondToLastCharIndex = _output.Length;
                    _secondToLastChar = _output[_output.Length - 1];
                }

                return _secondToLastChar;
            }
        }

        public char CurrentInputChar
        {
            get
            {
                if (_currentInputCharIndex != _parserPos)
                {
                    _currentInputCharIndex = _parserPos;
                    _currentInputChar = _input[_parserPos];
                }

                return _currentInputChar;
            }
        }

        public static Dictionary<string, bool> ToLookup(string str)
        {
            var chars = str.ToCharArray();
            var dic = new Dictionary<string, bool>();
            for (var i = 0; i < chars.Length; i++)
            {
                dic.Add(chars[i].ToString(), true);
            }

            return dic;
        }

        public static Dictionary<string, bool> ArrayToLookup(IEnumerable<string> str)
        {
            var strings = str.ToArray();
            var dic = new Dictionary<string, bool>();
            for (var i = 0; i < strings.Length; i++)
            {
                dic.Add(strings[i], true);
            }

            return dic;
        }

        public static Dictionary<char, bool> ToCharLookup(Dictionary<string, bool> lookup)
        {
            var dic = new Dictionary<char, bool>();
            foreach (var kvp in lookup)
            {
                dic.Add(kvp.Key[0], true);
            }

            return dic;
        }

        public void Parse()
        {
            var lastWord = "";
            var varLine = false;
            var varLineTainted = false;
            var inCase = false;
            var genericsDepth = 0;
            var interpolation = Interpolation;
            while (true)
            {
                GetNextToken();
                _tokenText = _token.Value;
                var tokenType = _token.TokenType;
                if (tokenType == TokenType.EndBlock && interpolation && _indentLevel == 1)
                {
                    return;
                }

                if (tokenType == TokenType.EndOfFile)
                {
                    break;
                }

                switch (tokenType)
                {
                    case TokenType.StartExpression:
                        varLine = false;
                        SetMode(TsMode.Expression);
                        if (_lastText == ";" || _lastType == TokenType.StartBlock)
                        {
                            PrintNewLine();
                        }
                        else if (_lastType == TokenType.EndExpression || _lastType == TokenType.StartExpression)
                        {
                            // do nothing on (( and )( and ][ and ]( ..
                        }
                        else if (_lastType != TokenType.Word && _lastType != TokenType.Operator &&
                                 _lastType != TokenType.Generics)
                        {
                            PrintSpace();
                        }
                        else if (_lineStarters.ContainsKey(lastWord))
                        {
                            PrintSpace();
                        }

                        PrintToken();
                        break;

                    case TokenType.EndExpression:
                        PrintToken();
                        RestoreMode();
                        break;

                    case TokenType.StartImport:
                    case TokenType.EndImport:
                        PrintSpace();
                        _output.Append(_token.Value);
                        PrintSpace();
                        break;
                    case TokenType.StartBlock:

                        if (lastWord == "do")
                        {
                            SetMode(TsMode.DoBlock);
                        }
                        else
                        {
                            SetMode(TsMode.Block);
                        }

                        if (_lastType != TokenType.Operator && _lastType != TokenType.StartExpression)
                        {
                            if (_lastType == TokenType.StartBlock)
                            {
                                PrintNewLine();
                            }
                            else
                            {
                                if (_options.OpenBlockOnNewLine)
                                {
                                    PrintNewLine();
                                }
                                else
                                {
                                    PrintSpace();
                                }
                            }
                        }

                        PrintToken();
                        Indent();
                        break;

                    case TokenType.EndBlock:
                        if (_lastType == TokenType.StartBlock)
                        {
                            // nothing
                            TrimOutput();
                            Unindent();
                        }
                        else
                        {
                            Unindent();
                            PrintNewLine();
                        }

                        PrintToken();
                        RestoreMode();
                        break;

                    case TokenType.Word:

                        if (_doBlockJustClosed)
                        {
                            // do {} ## while ()
                            PrintSpace();
                            PrintToken();
                            PrintSpace();
                            _doBlockJustClosed = false;
                            break;
                        }

                        if ((_tokenText == "extends" || _tokenText == ":") && _lastText == ">")
                        {
                            PrintSpace();
                        }

                        if (_tokenText == "case" || _tokenText == "default")
                        {
                            if (_lastText == ":")
                            {
                                // switch cases following one another
                                RemoveIndent();
                            }
                            else
                            {
                                // case statement starts in the same line where switch
                                Unindent();
                                PrintNewLine();
                                Indent();
                            }

                            PrintToken();
                            inCase = true;
                            break;
                        }

                        var prefix = "NONE";

                        if (_lastType == TokenType.EndBlock)
                        {
                            if (!new[] {"else", "catch", "finally"}.Contains(_tokenText.ToLower()))
                            {
                                prefix = "NEWLINE";
                            }
                            else
                            {
                                prefix = "SPACE";
                                PrintSpace();
                            }
                        }
                        else if (_lastType == TokenType.SemiColon &&
                                 (_currentMode == TsMode.Block || _currentMode == TsMode.DoBlock))
                        {
                            prefix = "NEWLINE";
                        }
                        else if (_lastType == TokenType.SemiColon && _currentMode == TsMode.Expression)
                        {
                            prefix = "SPACE";
                        }
                        else if (_lastType == TokenType.String)
                        {
                            prefix = "NEWLINE";
                        }
                        else if (_lastType == TokenType.Word)
                        {
                            prefix = "SPACE";
                        }
                        else if (_lastType == TokenType.StartBlock)
                        {
                            prefix = "NEWLINE";
                        }
                        else if (_lastType == TokenType.EndExpression)
                        {
                            PrintSpace();
                            prefix = "NEWLINE";
                        }

                        if (_lastType != TokenType.EndBlock &&
                            new[] {"else", "catch", "finally"}.Contains(_tokenText.ToLower()))
                        {
                            PrintNewLine();
                        }
                        else if (_lineStarters.ContainsKey(_tokenText) || prefix == "NEWLINE")
                        {
                            if (_lastText == "else")
                            {
                                // no need to force newline on else break
                                PrintSpace();
                            }
                            else if ((_lastType == TokenType.StartExpression || _lastText == "=" || _lastText == ",") &&
                                     _tokenText == "function")
                            {
                                // no need to force newline on "function": (function
                                // DONOTHING
                            }
                            else if (_lastType == TokenType.Word && (_lastText == "return" || _lastText == "throw"))
                            {
                                // no newline between "return nnn"
                                PrintSpace();
                            }
                            else if (_lastType != TokenType.EndExpression)
                            {
                                if ((_lastType != TokenType.StartExpression || _tokenText != "var") && _lastText != ":")
                                {
                                    if (_tokenText == "if" && _lastType == TokenType.Word && lastWord == "else")
                                    {
                                        PrintSpace();
                                    }
                                    else
                                    {
                                        PrintNewLine();
                                    }
                                }
                            }
                            else
                            {
                                if (_lineStarters.ContainsKey(_tokenText) && _lastText != ")")
                                {
                                    PrintNewLine();
                                }
                            }
                        }
                        else if (prefix == "SPACE")
                        {
                            PrintSpace();
                        }

                        PrintToken();
                        lastWord = _tokenText;

                        if (_tokenText == "var")
                        {
                            varLine = true;
                            varLineTainted = false;
                        }

                        if (_tokenText == "if" || _tokenText == "else")
                        {
                            _ifLineFlag = true;
                        }

                        break;

                    case TokenType.SemiColon:

                        PrintToken();
                        varLine = false;
                        break;

                    case TokenType.String:

                        if (_lastType == TokenType.StartBlock || _lastType == TokenType.EndBlock ||
                            _lastType == TokenType.SemiColon)
                        {
                            PrintNewLine();
                        }
                        else if (_lastType == TokenType.Word && _lastText != "$")
                        {
                            PrintSpace();
                        }

                        PrintToken();
                        break;
                    case TokenType.Generics:
                        if (_token.Value == "<")
                        {
                            if (genericsDepth == 0)
                            {
                                if (_lastText == "}")
                                {
                                    PrintNewLine();
                                }
                                if (_lastType == TokenType.Word &&
                                    _lastText == "return")
                                {
                                    PrintSpace();
                                }
                            }

                            _output.Append(_token.Value);
                            genericsDepth++;
                        }
                        else
                        {
                            _output.Append(_token.Value);
                            genericsDepth--;
                        }

                        break;
                    case TokenType.Operator:

                        var startDelim = true;
                        var endDelim = true;
                        if (varLine && _tokenText != ",")
                        {
                            varLineTainted = true;
                            if (_tokenText == ":")
                            {
                                varLine = false;
                            }
                        }

                        if (varLine && _tokenText == "," && _currentMode == TsMode.Expression)
                        {
                            varLineTainted = false;
                        }

                        if (_tokenText == ":" && inCase)
                        {
                            PrintToken(); // colon really asks for separate treatment
                            PrintNewLine();
                            inCase = false;
                            break;
                        }

                        if (_tokenText == "::")
                        {
                            // no spaces around exotic namespacing syntax operator
                            PrintToken();
                            break;
                        }

                        if (_tokenText == ",")
                        {
                            if (varLine)
                            {
                                if (varLineTainted)
                                {
                                    PrintToken();
                                    PrintNewLine();
                                    varLineTainted = false;
                                }
                                else
                                {
                                    PrintToken();
                                    PrintSpace();
                                }
                            }
                            else if (_lastType == TokenType.EndBlock)
                            {
                                PrintToken();
                                PrintNewLine();
                            }
                            else
                            {
                                if (_currentMode == TsMode.Block && !_isImportBlock)
                                {
                                    PrintToken();
                                    if (genericsDepth > 0)
                                    {
                                        PrintSpace();
                                    }
                                    else
                                    {
                                        PrintNewLine();
                                    }
                                }
                                else
                                {
                                    // EXPR od DO_BLOCK
                                    PrintToken();
                                    PrintSpace();
                                }
                            }

                            break;
                        }
                        else if (_tokenText == "--" || _tokenText == "++")
                        {
                            // unary operators special case
                            if (_lastText == ";")
                            {
                                if (_currentMode == TsMode.Block)
                                {
                                    // { foo; --i }
                                    PrintNewLine();
                                    startDelim = true;
                                    endDelim = false;
                                }
                                else
                                {
                                    // space for (;; ++i)
                                    startDelim = true;
                                    endDelim = false;
                                }
                            }
                            else
                            {
                                if (_lastText == "{")
                                {
                                    PrintNewLine();
                                }

                                startDelim = false;
                                endDelim = false;
                            }
                        }
                        else if ((_tokenText == "!" || _tokenText == "+" || _tokenText == "-") &&
                                 (_lastText == "return" || _lastText == "case"))
                        {
                            startDelim = true;
                            endDelim = false;
                        }
                        else if ((_tokenText == "!" || _tokenText == "+" || _tokenText == "-") &&
                                 _lastType == TokenType.StartExpression)
                        {
                            // special case handling: if (!a)
                            startDelim = false;
                            endDelim = false;
                        }
                        else if (_lastType == TokenType.Operator)
                        {
                            startDelim = false;
                            endDelim = false;
                        }
                        else if (_lastType == TokenType.EndExpression)
                        {
                            startDelim = true;
                            endDelim = true;
                        }
                        else if (_tokenText == ".")
                        {
                            // decimal digits or object.property
                            startDelim = false;
                            endDelim = false;
                        }
                        else if (_tokenText == ":")
                        {
                            if (IsTernaryOperation())
                            {
                                startDelim = true;
                            }
                            else
                            {
                                startDelim = false;
                            }
                        }

                        if (startDelim)
                        {
                            PrintSpace();
                        }

                        PrintToken();
                        if (endDelim)
                        {
                            PrintSpace();
                        }

                        break;

                    case TokenType.BlockComment:
                        PrintNewLineOrSpace(_token);
                        PrintToken();
                        PrintNewLineOrSpace(_token);
                        break;

                    case TokenType.Comment:

                        // print_newline();
                        if (_lastType == TokenType.StartBlock)
                        {
                            PrintNewLine();
                        }
                        else
                        {
                            PrintNewLineOrSpace(_token);
                        }

                        PrintToken();
                        PrintNewLine();
                        break;

                    case TokenType.Unknown:
                        PrintToken();
                        break;
                }

                _lastType = tokenType;
                _lastText = _tokenText;
            }
        }

        private bool PrintNewLineOrSpace(Token t)
        {
            if (t.NewLineCount > 0)
            {
                PrintNewLine();
                return true;
            }

            PrintSpace();
            return false;
        }

        private void TrimOutput()
        {
            while (_output.Length > 0 && (SecondToLastChar == ' ' ||
                                          SecondToLastChar == _indentString[0]))
            {
                _output.Remove(_output.Length - 1, 1);
            }
        }

        private void PrintNewLine(bool? ignoreRepeated = null)
        {
            ignoreRepeated = ignoreRepeated ?? true;

            _ifLineFlag = false;
            TrimOutput();

            if (_output.Length == 0)
            {
                return;
            }

            if (SecondToLastChar != '\n' || !ignoreRepeated.Value)
            {
                _output.Append(Environment.NewLine);
            }

            for (var i = 0; i < _indentLevel; i++)
            {
                _output.Append(_indentString);
            }
        }

        private void PrintSpace()
        {
            if (SecondToLastChar != ' ' && SecondToLastChar != '\n' && SecondToLastChar != _indentString[0])
            {
                _output.Append(' ');
            }
        }


        private void PrintToken()
        {
            _output.Append(_tokenText);
        }

        private void Indent()
        {
            _indentLevel++;
        }

        private void Unindent()
        {
            if (_indentLevel > 0)
            {
                _indentLevel--;
            }
        }

        private void RemoveIndent()
        {
            if (_output.Length > 0 && SecondToLastChar == _indentString[0])
            {
                _output.Remove(_output.Length - 1, 1);
            }
        }

        private void SetMode(TsMode mode)
        {
            _modes.Push(_currentMode);
            _currentMode = mode;
        }

        private void RestoreMode()
        {
            _doBlockJustClosed = _currentMode == TsMode.DoBlock;
            _currentMode = _modes.Pop();
        }

        private bool IsTernaryOperation()
        {
            var level = 0;
            var colonCount = 0;
            for (var i = _output.Length - 1; i >= 0; i--)
            {
                switch (_output[i])
                {
                    case ':':
                        if (level == 0)
                        {
                            colonCount++;
                        }

                        break;
                    case '?':
                        if (level == 0)
                        {
                            if (colonCount == 0)
                            {
                                return true;
                            }

                            colonCount--;
                        }

                        break;
                    case '{':
                        if (level == 0)
                        {
                            return false;
                        }

                        level--;
                        break;
                    case '(':
                    case '[':
                        level--;
                        break;
                    case ')':
                    case ']':
                    case '}':
                        level++;
                        break;
                }
            }

            return false;
        }

        private void GetNextToken()
        {
            var newLineCount = 0;

            if (_parserPos >= _input.Length)
            {
                SetToken("", TokenType.EndOfFile, newLineCount);
                return;
            }

            var currentChar = CurrentInputChar;
            _parserPos++;

            while (_whitespaceChars.ContainsKey(currentChar))
            {
                if (_parserPos >= _input.Length)
                {
                    SetToken("", TokenType.EndOfFile, newLineCount);
                    return;
                }

                if (currentChar == '\n')
                {
                    newLineCount++;
                }

                currentChar = CurrentInputChar;
                _parserPos++;
            }

            var currentString = currentChar.ToString();
            var wantedNewline = false;

            if (_optPreserveNewlines)
            {
                if (newLineCount > 1)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        PrintNewLine(i == 0);
                    }
                }

                wantedNewline = newLineCount == 1;
            }

            var isGenerics = false;
            if (currentChar == '<')
            {
                if (InGenerics)
                {
                    _genericsDepth++;
                    isGenerics = true;
                }
                else
                {
                    // Check for the start of generics
                    for (var i = _parserPos + 1; i < _input.Length; i++)
                    {
                        var cSub = _input[i];
                        if (cSub == '>')
                        {
                            isGenerics = true;
                            break;
                        }

                        if (!_genericsChars.ContainsKey(cSub))
                        {
                            break;
                        }
                    }

                    if (isGenerics)
                    {
                        _genericsDepth++;
                    }
                }
            }
            else if (currentChar == '>' && _genericsDepth > 0)
            {
                isGenerics = true;
                _genericsDepth--;
            }

            if (isGenerics && _genericsBracketsChars.ContainsKey(currentChar))
            {
                SetToken(currentString, TokenType.Generics, newLineCount);
                return;
            }


            if (_wordcharChars.ContainsKey(currentChar))
            {
                if (_parserPos < _input.Length)
                {
                    while (_wordcharChars.ContainsKey(CurrentInputChar))
                    {
                        currentString += CurrentInputChar;
                        _parserPos++;
                        if (_parserPos == _input.Length)
                        {
                            break;
                        }
                    }
                }


                if ((CurrentInputChar == '-' || CurrentInputChar == '+') &&
                    _parserPos != _input.Length && Regex.IsMatch(currentString, "^[0-9]+[Ee]$"))
                {
                    var sign = CurrentInputChar;
                    _parserPos++;

                    GetNextToken();
                    currentString += sign + _token.Value;
                    SetToken(currentString, TokenType.Word, newLineCount);
                    return;
                }

                if (currentString == "in")
                {
                    SetToken(currentString, TokenType.Operator, newLineCount);
                    return;
                }

                if (wantedNewline && _lastType != TokenType.Operator && !_ifLineFlag)
                {
                    PrintNewLine();
                }

                SetToken(currentString, TokenType.Word, newLineCount);
                return;
            }

            if (currentChar == '(' || currentChar == '[')
            {
                SetToken(currentString, TokenType.StartExpression, newLineCount);
                return;
            }

            if (currentChar == ')' || currentChar == ']')
            {
                SetToken(currentString, TokenType.EndExpression, newLineCount);
                return;
            }

            if (currentChar == '{')
            {
                if (_lastText == "import")
                {
                    _isImportBlock = true;
                    SetToken(currentString, TokenType.StartImport, newLineCount);
                    return;
                }

                SetToken(currentString, TokenType.StartBlock, newLineCount);
                return;
            }

            if (currentChar == '}')
            {
                if (_isImportBlock)
                {
                    _isImportBlock = false;
                    SetToken(currentString, TokenType.EndImport, newLineCount);
                    return;
                }

                SetToken(currentString, TokenType.EndBlock, newLineCount);
                return;
            }

            if (currentChar == ';')
            {
                SetToken(currentString, TokenType.SemiColon, newLineCount);
                return;
            }

            if (currentChar == '/')
            {
                var comment = "";
                if (CurrentInputChar == '*')
                {
                    _parserPos++;
                    if (_parserPos < _input.Length)
                    {
                        while (!(CurrentInputChar == '*' && _input[_parserPos + 1] > '\0' &&
                                 _input[_parserPos + 1] == '/' && _parserPos < _input.Length))
                        {
                            comment += CurrentInputChar;
                            _parserPos++;
                            if (_parserPos >= _input.Length)
                            {
                                break;
                            }
                        }
                    }

                    _parserPos += 2;
                    SetToken("/*" + comment + "*/", TokenType.BlockComment, newLineCount);
                    return;
                }

                if (CurrentInputChar == '/')
                {
                    comment = currentString;
                    while (CurrentInputChar != '\x0d' && CurrentInputChar != '\x0a')
                    {
                        comment += CurrentInputChar;
                        _parserPos++;
                        if (_parserPos >= _input.Length)
                        {
                            break;
                        }
                    }

                    _parserPos++;
                    if (wantedNewline)
                    {
                        PrintNewLine();
                    }

                    SetToken(comment, TokenType.Comment, newLineCount);
                    return;
                }
            }

            var interpolationAllowed = StringInterpolationKind.None;
            // Allow C# interpolated strings
            if (currentChar == '$' || _lastText == "$")
            {
                var lookahead = currentChar == '$' ? 1 : 0;
                var next = At(_parserPos + lookahead);
                if (next == '@' || next == '"')
                {
                    interpolationAllowed = StringInterpolationKind.CSharp;
                    if (lookahead == 1)
                    {
                        _output.Append(currentChar);
                        currentChar = CurrentInputChar;
                        currentString = currentChar.ToString();
                        _parserPos++;
                    }
                }
            }

            if (currentChar == '@' && CurrentInputChar == '"')
            {
                _output.Append(currentChar);
                currentChar = CurrentInputChar;
                currentString = currentChar.ToString();
                _parserPos++;
            }

            if (currentChar == '`')
            {
                int a = 0;
            }
            if ((currentChar == '\'' || currentChar == '\\' || currentChar == '/' || currentChar == '`' || currentChar == '"')
                && (_lastType == TokenType.Word &&
                    (_lastText == "$" || _lastText == "return" || _lastText == "from" || _lastText == "case") ||
                    _lastType == TokenType.StartExpression ||
                    _lastType == TokenType.StartBlock || _lastType == TokenType.EndBlock ||
                    _lastType == TokenType.Operator ||
                    _lastType == TokenType.EndOfFile || _lastType == TokenType.SemiColon)
            )
            {
                var sep = currentChar;
                var esc = false;
                var resultingString = new StringBuilder(currentString);

                if (_parserPos < _input.Length)
                {
                    if (sep == '/')
                    {
                        var inCharClass = false;
                        while (esc || inCharClass || CurrentInputChar != sep)
                        {
                            resultingString.Append(CurrentInputChar);
                            if (!esc)
                            {
                                esc = CurrentInputChar == '\\';
                                if (CurrentInputChar == '[')
                                {
                                    inCharClass = true;
                                }
                                else if (CurrentInputChar == ']')
                                {
                                    inCharClass = false;
                                }
                            }
                            else
                            {
                                esc = false;
                            }

                            _parserPos++;
                            if (_parserPos >= _input.Length)
                            {
                                SetToken(resultingString.ToString(), TokenType.String, newLineCount);
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (currentChar == '`')
                        {
                            interpolationAllowed = StringInterpolationKind.TypeScript;
                        }

                        while (esc || CurrentInputChar != sep)
                        {
                            var interpolationActioned = false;
                            switch (interpolationAllowed)
                            {
                                case StringInterpolationKind.CSharp:
                                    if (_input.StartsWithAt("{", _parserPos) && !_input.StartsWithAt("{{", _parserPos))
                                    {
                                        interpolationActioned = true;
                                    }

                                    break;
                                case StringInterpolationKind.TypeScript:
                                    if (_input.StartsWithAt("${", _parserPos))
                                    {
                                        var escapeCount = 0;
                                        for (var i = _parserPos - 1; i > 0; i--)
                                        {
                                            if (_input[i] == '\\')
                                            {
                                                escapeCount++;
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }

                                        if (escapeCount % 2 == 0)
                                        {
                                            // Escaped
                                            interpolationActioned = true;
                                        }
                                    }

                                    break;
                            }

                            if (interpolationActioned)
                            {
                                switch (interpolationAllowed)
                                {
                                    case StringInterpolationKind.CSharp:
                                    {
                                        var jsSourceText = _input.Substring(_parserPos);
                                        var sub = new TsBeautifierInstance(jsSourceText, _options, true);
                                        var interpolated = sub.Beautify().TrimStart('{').Trim();
                                        resultingString.Append($"{{{interpolated}}}");
                                        _parserPos += sub._parserPos;
                                    }
                                        break;
                                    case StringInterpolationKind.TypeScript:
                                    {
                                        var jsSourceText = _input.Substring(_parserPos + 1);
                                        var sub = new TsBeautifierInstance(jsSourceText, _options, true);
                                        var interpolated = sub.Beautify().TrimStart('{').Trim();
                                        resultingString.Append($"${{{interpolated}}}");
                                        _parserPos += sub._parserPos + 1;
                                    }
                                        break;
                                }
                            }
                            else
                            {
                                resultingString.Append(CurrentInputChar);
                                if (!esc)
                                {
                                    esc = CurrentInputChar == '\\';
                                }
                                else
                                {
                                    esc = false;
                                }

                                _parserPos++;
                            }

                            if (_parserPos >= _input.Length)
                            {
                                SetToken(resultingString.ToString(), TokenType.String, newLineCount);
                                return;
                            }
                        }
                    }
                }

                _parserPos += 1;

                resultingString.Append(sep);

                if (sep == '/')
                {
                    while (_parserPos < _input.Length && _wordcharChars.ContainsKey(CurrentInputChar))
                    {
                        resultingString.Append(CurrentInputChar);
                        _parserPos += 1;
                    }
                }

                SetToken(resultingString.ToString(), TokenType.String, newLineCount);
                return;
            }

            if (currentChar == '#')
            {
                var sharp = "#";
                if (_parserPos < _input.Length && _digitsChars.ContainsKey(CurrentInputChar))
                {
                    do
                    {
                        currentChar = CurrentInputChar;
                        sharp += currentChar;
                        _parserPos += 1;
                    } while (_parserPos < _input.Length && currentChar != '#' && currentChar != '=');

                    if (currentChar == '#')
                    {
                        SetToken(sharp, TokenType.Word, newLineCount);
                        return;
                    }

                    SetToken(sharp, TokenType.Operator, newLineCount);
                    return;
                }
            }


            if (currentChar == '<' && _input.Substring(_parserPos - 1, 3) == "<!--")
            {
                _parserPos += 3;
                SetToken("<!--", TokenType.Comment, newLineCount);
                return;
            }

            if (currentChar == '-' && _input.Substring(_parserPos - 1, 2) == "-->")
            {
                _parserPos += 2;
                if (wantedNewline)
                {
                    PrintNewLine();
                }

                SetToken("-->", TokenType.Comment, newLineCount);
                return;
            }

            if (_punctuation.ContainsKey(currentString))
            {
                while (_parserPos < _input.Length && _punctuation.ContainsKey(currentString + CurrentInputChar))
                {
                    currentString += CurrentInputChar;
                    _parserPos += 1;
                    if (_parserPos >= _input.Length)
                    {
                        break;
                    }
                }

                SetToken(currentString, TokenType.Operator, newLineCount);
                return;
            }

            SetToken(currentString, TokenType.Unknown, newLineCount);
        }

        private void SetToken(string token, TokenType tokenType, int newLineCount)
        {
            _token.NewLineCount = newLineCount;
            _token.Value = token;
            _token.TokenType = tokenType;
        }

        private char? At(int position)
        {
            if (position > _input.Length - 1)
            {
                return null;
            }

            return _input[position];
        }

        public string Beautify()
        {
            Parse();
            return _output.ToString();
        }
    }

    internal enum StringInterpolationKind
    {
        None = 1,
        CSharp = 2,
        TypeScript = 3
    }

    internal enum TsMode
    {
        Block,
        Expression,
        DoBlock
    }

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

    internal struct Token
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