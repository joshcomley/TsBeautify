using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TsBeautify.Extensions;

namespace TsBeautify
{
    internal class StateManager
    {
        private readonly State _state;

        public StateManager(State state, bool interpolation = false)
        {
            Interpolation = interpolation;
            _state = state;
        }

        public bool Interpolation { get; }

        public char SecondToLastChar
        {
            get
            {
                if (_state.SecondToLastCharIndex != _state.Output.Length)
                {
                    _state.SecondToLastCharIndex = _state.Output.Length;
                    _state.SecondToLastChar = _state.Output[_state.Output.Length - 1];
                }

                return _state.SecondToLastChar;
            }
        }

        private bool SkipWhiteSpace(out char retrievedChar, out int newLineCount)
        {
            retrievedChar = _state.CurrentInputChar;
            newLineCount = 0;
            if (_state.ParserPos >= _state.StaticState.Input.Length)
            {
                SetToken("", TokenType.EndOfFile, newLineCount);
                return true;
            }

            while (_state.StaticState.WhitespaceChars.ContainsKey(retrievedChar))
            {
                if (_state.ParserPos >= _state.StaticState.Input.Length)
                {
                    SetToken("", TokenType.EndOfFile, newLineCount);
                    return true;
                }

                if (retrievedChar == '\n')
                {
                    newLineCount++;
                }

                retrievedChar = _state.CurrentInputChar;
                _state.ParserPos++;
            }

            return false;
        }

        public void GetNextToken()
        {
            if (SkipWhiteSpace2(true, out var newLineCount, out var retrievedChar))
            {
                return;
            }

            //_parserPos++;
            //if (SkipWhiteSpace(out var retrievedChar, out var newLineCount))
            //{
            //    return;
            //}
            var currentString = retrievedChar.ToString();
            if (retrievedChar == '@')
            {
                SkipWhiteSpace2(false, out var newLineCount2, out var nextNonWhiteSpaceChar);
                newLineCount += newLineCount2;
            }

            var wantedNewline = false;

            if (_state.StaticState.PreserveNewlines)
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
            if (retrievedChar == '<')
            {
                if (_state.InGenerics)
                {
                    _state.GenericsDepth++;
                    isGenerics = true;
                }
                else
                {
                    // Check for the start of generics
                    for (var i = _state.ParserPos + 1; i < _state.StaticState.Input.Length; i++)
                    {
                        var cSub = _state.StaticState.Input[i];
                        if (cSub == '>')
                        {
                            isGenerics = true;
                            break;
                        }

                        if (!_state.StaticState.GenericsChars.ContainsKey(cSub))
                        {
                            break;
                        }
                    }

                    if (isGenerics)
                    {
                        _state.GenericsDepth++;
                    }
                }
            }
            else if (retrievedChar == '>' && _state.GenericsDepth > 0)
            {
                isGenerics = true;
                _state.GenericsDepth--;
            }

            if (isGenerics && _state.StaticState.GenericsBracketsChars.ContainsKey(retrievedChar))
            {
                SetToken(currentString, TokenType.Generics, newLineCount);
                return;
            }

            if (_state.StaticState.WordCharChars.ContainsKey(retrievedChar) || retrievedChar == '@' &&
                _state.StaticState.WordCharChars.ContainsKey(_state.CurrentInputChar))
            {
                if (_state.ParserPos < _state.StaticState.Input.Length)
                {
                    while (_state.StaticState.WordCharChars.ContainsKey(_state.CurrentInputChar))
                    {
                        currentString += _state.CurrentInputChar;
                        _state.ParserPos++;
                        if (_state.ParserPos == _state.StaticState.Input.Length)
                        {
                            break;
                        }
                    }
                }


                if ((_state.CurrentInputChar == '-' || _state.CurrentInputChar == '+') &&
                    _state.ParserPos != _state.StaticState.Input.Length && Regex.IsMatch(currentString, "^[0-9]+[Ee]$"))
                {
                    var sign = _state.CurrentInputChar;
                    _state.ParserPos++;

                    GetNextToken();
                    currentString += sign + _state.Token.Value;
                    SetToken(currentString, TokenType.Word, newLineCount);
                    return;
                }

                if (currentString == "in")
                {
                    SetToken(currentString, TokenType.Operator, newLineCount);
                    return;
                }

                if (wantedNewline && _state.LastType != TokenType.Operator && !_state.IfLineFlag)
                {
                    PrintNewLine();
                }

                SetToken(currentString, TokenType.Word, newLineCount);
                return;
            }

            if (retrievedChar == '(' || retrievedChar == '[')
            {
                SetToken(currentString, TokenType.StartExpression, newLineCount);
                return;
            }

            if (retrievedChar == ')' || retrievedChar == ']')
            {
                SetToken(currentString, TokenType.EndExpression, newLineCount);
                return;
            }

            if (retrievedChar == '{')
            {
                if (_state.LastText == "import")
                {
                    _state.IsImportBlock = true;
                    SetToken(currentString, TokenType.StartImport, newLineCount);
                    return;
                }

                SetToken(currentString, TokenType.StartBlock, newLineCount);
                return;
            }

            if (retrievedChar == '}')
            {
                if (_state.IsImportBlock)
                {
                    _state.IsImportBlock = false;
                    SetToken(currentString, TokenType.EndImport, newLineCount);
                    return;
                }

                SetToken(currentString, TokenType.EndBlock, newLineCount);
                return;
            }

            if (retrievedChar == ';')
            {
                SetToken(currentString, TokenType.SemiColon, newLineCount);
                return;
            }

            if (retrievedChar == '/')
            {
                var comment = "";
                if (_state.CurrentInputChar == '*')
                {
                    _state.ParserPos++;
                    if (_state.ParserPos < _state.StaticState.Input.Length)
                    {
                        while (!(_state.CurrentInputChar == '*' &&
                                 _state.StaticState.Input[_state.ParserPos + 1] > '\0' &&
                                 _state.StaticState.Input[_state.ParserPos + 1] == '/' &&
                                 _state.ParserPos < _state.StaticState.Input.Length))
                        {
                            comment += _state.CurrentInputChar;
                            _state.ParserPos++;
                            if (_state.ParserPos >= _state.StaticState.Input.Length)
                            {
                                break;
                            }
                        }
                    }

                    _state.ParserPos += 2;
                    SetToken("/*" + comment + "*/", TokenType.BlockComment, newLineCount);
                    return;
                }

                if (_state.CurrentInputChar == '/')
                {
                    comment = currentString;
                    while (_state.CurrentInputChar != '\x0d' && _state.CurrentInputChar != '\x0a')
                    {
                        comment += _state.CurrentInputChar;
                        _state.ParserPos++;
                        if (_state.ParserPos >= _state.StaticState.Input.Length)
                        {
                            break;
                        }
                    }

                    _state.ParserPos++;
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
            if (retrievedChar == '$' || _state.LastText == "$")
            {
                var lookahead = retrievedChar == '$' ? 1 : 0;
                var next = At(_state.ParserPos + lookahead);
                if (next == '@' || next == '"')
                {
                    interpolationAllowed = StringInterpolationKind.CSharp;
                    if (lookahead == 1)
                    {
                        _state.Output.Append(retrievedChar);
                        retrievedChar = _state.CurrentInputChar;
                        currentString = retrievedChar.ToString();
                        _state.ParserPos++;
                    }
                }
            }

            if (retrievedChar == '@' && _state.CurrentInputChar == '"')
            {
                _state.Output.Append(retrievedChar);
                retrievedChar = _state.CurrentInputChar;
                currentString = retrievedChar.ToString();
                _state.ParserPos++;
            }

            var isQuote = retrievedChar == '\'' || retrievedChar == '\\' || retrievedChar == '`' ||
                          retrievedChar == '"';
            if ((isQuote || retrievedChar == '/')
                && (_state.LastType == TokenType.Word &&
                    (_state.LastText == "$" || _state.LastText == "return" || _state.LastText == "from" ||
                     _state.LastText == "case") ||
                    isQuote && _state.LastType == TokenType.Word ||
                    _state.LastType == TokenType.StartExpression ||
                    _state.LastType == TokenType.BlockComment ||
                    _state.LastType == TokenType.Comment ||
                    _state.LastType == TokenType.StartBlock ||
                    _state.LastType == TokenType.EndBlock ||
                    _state.LastType == TokenType.Operator ||
                    _state.LastType == TokenType.Generics ||
                    _state.LastType == TokenType.EndOfFile ||
                    _state.LastType == TokenType.SemiColon)
            )
            {
                var sep = retrievedChar;
                var esc = false;
                var resultingString = new StringBuilder(currentString);

                if (_state.ParserPos < _state.StaticState.Input.Length)
                {
                    if (sep == '/')
                    {
                        var inCharClass = false;
                        while (esc || inCharClass || _state.CurrentInputChar != sep)
                        {
                            resultingString.Append(_state.CurrentInputChar);
                            if (!esc)
                            {
                                esc = _state.CurrentInputChar == '\\';
                                if (_state.CurrentInputChar == '[')
                                {
                                    inCharClass = true;
                                }
                                else if (_state.CurrentInputChar == ']')
                                {
                                    inCharClass = false;
                                }
                            }
                            else
                            {
                                esc = false;
                            }

                            _state.ParserPos++;
                            if (_state.ParserPos >= _state.StaticState.Input.Length)
                            {
                                SetToken(resultingString.ToString(), TokenType.String, newLineCount);
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (retrievedChar == '`')
                        {
                            interpolationAllowed = StringInterpolationKind.TypeScript;
                        }

                        while (esc || _state.CurrentInputChar != sep)
                        {
                            var interpolationActioned = false;
                            switch (interpolationAllowed)
                            {
                                case StringInterpolationKind.CSharp:
                                    if (_state.StaticState.Input.StartsWithAt("{", _state.ParserPos) &&
                                        !_state.StaticState.Input.StartsWithAt("{{", _state.ParserPos))
                                    {
                                        interpolationActioned = true;
                                    }

                                    break;
                                case StringInterpolationKind.TypeScript:
                                    if (_state.StaticState.Input.StartsWithAt("${", _state.ParserPos))
                                    {
                                        var escapeCount = 0;
                                        for (var i = _state.ParserPos - 1; i > 0; i--)
                                        {
                                            if (_state.StaticState.Input[i] == '\\')
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
                                        var jsSourceText = _state.StaticState.Input.Substring(_state.ParserPos);
                                        var sub = new StateManager(
                                            new State(new StaticState(jsSourceText, _state.StaticState.Options)),
                                            true);
                                        sub.Parse();
                                        var interpolated = sub._state.Output.ToString().TrimStart('{').Trim();
                                        resultingString.Append($"{{{interpolated}}}");
                                        _state.ParserPos += sub._state.ParserPos;
                                    }
                                        break;
                                    case StringInterpolationKind.TypeScript:
                                    {
                                        var jsSourceText = _state.StaticState.Input.Substring(_state.ParserPos + 1);
                                        var sub = new StateManager(
                                            new State(new StaticState(jsSourceText, _state.StaticState.Options)),
                                            true);
                                        sub.Parse();
                                        var interpolated = sub._state.Output.ToString().TrimStart('{').Trim();
                                        resultingString.Append($"${{{interpolated}}}");
                                        _state.ParserPos += sub._state.ParserPos + 1;
                                    }
                                        break;
                                }
                            }
                            else
                            {
                                resultingString.Append(_state.CurrentInputChar);
                                if (!esc)
                                {
                                    esc = _state.CurrentInputChar == '\\';
                                }
                                else
                                {
                                    esc = false;
                                }

                                _state.ParserPos++;
                            }

                            if (_state.ParserPos >= _state.StaticState.Input.Length)
                            {
                                SetToken(resultingString.ToString(), TokenType.String, newLineCount);
                                return;
                            }
                        }
                    }
                }

                _state.ParserPos += 1;

                resultingString.Append(sep);

                if (sep == '/')
                {
                    while (_state.ParserPos < _state.StaticState.Input.Length &&
                           _state.StaticState.WordCharChars.ContainsKey(_state.CurrentInputChar))
                    {
                        resultingString.Append(_state.CurrentInputChar);
                        _state.ParserPos += 1;
                    }
                }

                SetToken(resultingString.ToString(), TokenType.String, newLineCount);
                return;
            }

            if (retrievedChar == '#')
            {
                var sharp = "#";
                if (_state.ParserPos < _state.StaticState.Input.Length &&
                    _state.StaticState.DigitsChars.ContainsKey(_state.CurrentInputChar))
                {
                    do
                    {
                        retrievedChar = _state.CurrentInputChar;
                        sharp += retrievedChar;
                        _state.ParserPos += 1;
                    } while (_state.ParserPos < _state.StaticState.Input.Length && retrievedChar != '#' &&
                             retrievedChar != '=');

                    if (retrievedChar == '#')
                    {
                        SetToken(sharp, TokenType.Word, newLineCount);
                        return;
                    }

                    SetToken(sharp, TokenType.Operator, newLineCount);
                    return;
                }
            }


            if (retrievedChar == '<' && _state.StaticState.Input.Substring(_state.ParserPos - 1, 3) == "<!--")
            {
                _state.ParserPos += 3;
                SetToken("<!--", TokenType.Comment, newLineCount);
                return;
            }

            if (retrievedChar == '-' && _state.StaticState.Input.Substring(_state.ParserPos - 1, 2) == "-->")
            {
                _state.ParserPos += 2;
                if (wantedNewline)
                {
                    PrintNewLine();
                }

                SetToken("-->", TokenType.Comment, newLineCount);
                return;
            }

            if (_state.StaticState.Punctuation.ContainsKey(currentString))
            {
                while (_state.ParserPos < _state.StaticState.Input.Length &&
                       _state.StaticState.Punctuation.ContainsKey(currentString + _state.CurrentInputChar))
                {
                    currentString += _state.CurrentInputChar;
                    _state.ParserPos += 1;
                    if (_state.ParserPos >= _state.StaticState.Input.Length)
                    {
                        break;
                    }
                }

                SetToken(currentString, TokenType.Operator, newLineCount);
                return;
            }

            SetToken(currentString, TokenType.Unknown, newLineCount);
        }

        private bool SkipWhiteSpace2(bool skipFirst, out int newLineCount, out char retrievedChar)
        {
            newLineCount = 0;

            if (_state.ParserPos >= _state.StaticState.Input.Length)
            {
                SetToken("", TokenType.EndOfFile, newLineCount);
                retrievedChar = '\n';
                return true;
            }

            retrievedChar = _state.CurrentInputChar;

            if (skipFirst)
            {
                _state.ParserPos++;
            }

            while (_state.StaticState.WhitespaceChars.ContainsKey(retrievedChar))
            {
                if (_state.ParserPos >= _state.StaticState.Input.Length)
                {
                    SetToken("", TokenType.EndOfFile, newLineCount);
                    return true;
                }

                if (retrievedChar == '\n')
                {
                    newLineCount++;
                }

                retrievedChar = _state.CurrentInputChar;
                _state.ParserPos++;
            }

            return false;
        }

        private void SetToken(string token, TokenType tokenType, int newLineCount)
        {
            _state.Token.NewLineCount = newLineCount;
            _state.Token.Value = token;
            _state.Token.TokenType = tokenType;
        }

        private char? At(int position)
        {
            if (position > _state.StaticState.Input.Length - 1)
            {
                return null;
            }

            return _state.StaticState.Input[position];
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
                _state.TokenText = _state.Token.Value;
                var tokenType = _state.Token.TokenType;
                if (tokenType == TokenType.Word && _state.StaticState.LineStarters.ContainsKey(_state.Token.Value))
                {
                    PrintNewLine();
                }

                if (tokenType == TokenType.EndBlock && interpolation && _state.IndentLevel == 1)
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
                        if (_state.LastText == ";" || _state.LastType == TokenType.StartBlock)
                        {
                            PrintNewLine();
                        }
                        else if (_state.LastType == TokenType.EndExpression ||
                                 _state.LastType == TokenType.StartExpression)
                        {
                            // do nothing on (( and )( and ][ and ]( ..
                        }
                        else if (_state.LastType != TokenType.Word && _state.LastType != TokenType.Operator &&
                                 _state.LastType != TokenType.Generics)
                        {
                            PrintSpace();
                        }
                        else if (_state.StaticState.LineStarters.ContainsKey(lastWord))
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
                        _state.Output.Append(_state.Token.Value);
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

                        if (_state.LastType != TokenType.Operator && _state.LastType != TokenType.StartExpression)
                        {
                            if (_state.LastType == TokenType.StartBlock)
                            {
                                PrintNewLine();
                            }
                            else
                            {
                                if (_state.StaticState.Options.OpenBlockOnNewLine)
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
                        if (_state.LastType == TokenType.StartBlock)
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

                        if (_state.DoBlockJustClosed)
                        {
                            // do {} ## while ()
                            PrintSpace();
                            PrintToken();
                            PrintSpace();
                            _state.DoBlockJustClosed = false;
                            break;
                        }

                        if ((_state.TokenText == "extends" || _state.TokenText == ":") && _state.LastText == ">")
                        {
                            PrintSpace();
                        }

                        if (_state.TokenText == "case" || _state.TokenText == "default")
                        {
                            if (_state.LastText == ":")
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

                        if (_state.LastType == TokenType.EndBlock)
                        {
                            if (!new[] {"else", "catch", "finally"}.Contains(_state.TokenText.ToLower()))
                            {
                                prefix = "NEWLINE";
                            }
                            else
                            {
                                prefix = "SPACE";
                                PrintSpace();
                            }
                        }
                        else if (_state.LastType == TokenType.SemiColon &&
                                 (_state.CurrentMode == TsMode.Block || _state.CurrentMode == TsMode.DoBlock))
                        {
                            prefix = "NEWLINE";
                        }
                        else if (_state.LastType == TokenType.SemiColon && _state.CurrentMode == TsMode.Expression)
                        {
                            prefix = "SPACE";
                        }
                        else if (_state.LastType == TokenType.String)
                        {
                            prefix = "NEWLINE";
                        }
                        else if (_state.LastType == TokenType.Word)
                        {
                            prefix = "SPACE";
                        }
                        else if (_state.LastType == TokenType.StartBlock)
                        {
                            prefix = "NEWLINE";
                        }
                        else if (_state.LastType == TokenType.EndExpression)
                        {
                            PrintSpace();
                            prefix = "NEWLINE";
                        }

                        if (_state.LastType != TokenType.EndBlock &&
                            new[] {"else", "catch", "finally"}.Contains(_state.TokenText.ToLower()))
                        {
                            PrintNewLine();
                        }
                        else if (_state.StaticState.LineStarters.ContainsKey(_state.TokenText) || prefix == "NEWLINE")
                        {
                            if (_state.LastText == "else")
                            {
                                // no need to force newline on else break
                                PrintSpace();
                            }
                            else if ((_state.LastType == TokenType.StartExpression || _state.LastText == "=" ||
                                      _state.LastText == ",") &&
                                     _state.TokenText == "function")
                            {
                                // no need to force newline on "function": (function
                                // DONOTHING
                            }
                            else if (_state.LastType == TokenType.Word &&
                                     (_state.LastText == "return" || _state.LastText == "throw"))
                            {
                                // no newline between "return nnn"
                                PrintSpace();
                            }
                            else if (_state.LastType != TokenType.EndExpression)
                            {
                                if ((_state.LastType != TokenType.StartExpression || _state.TokenText != "var") &&
                                    _state.LastText != ":")
                                {
                                    if (_state.TokenText == "if" && _state.LastType == TokenType.Word &&
                                        lastWord == "else")
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
                                if (_state.StaticState.LineStarters.ContainsKey(_state.TokenText) &&
                                    _state.LastText != ")")
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
                        lastWord = _state.TokenText;

                        if (_state.TokenText == "var")
                        {
                            varLine = true;
                            varLineTainted = false;
                        }

                        if (_state.TokenText == "if" || _state.TokenText == "else")
                        {
                            _state.IfLineFlag = true;
                        }

                        break;

                    case TokenType.SemiColon:

                        PrintToken();
                        varLine = false;
                        break;

                    case TokenType.String:

                        if (_state.LastType == TokenType.StartBlock || _state.LastType == TokenType.EndBlock ||
                            _state.LastType == TokenType.SemiColon)
                        {
                            PrintNewLine();
                        }
                        else if (_state.LastType == TokenType.Word && _state.LastText != "$")
                        {
                            PrintSpace();
                        }

                        PrintToken();
                        break;
                    case TokenType.Generics:
                        if (_state.Token.Value == "<")
                        {
                            if (genericsDepth == 0)
                            {
                                if (_state.LastText == "}")
                                {
                                    PrintNewLine();
                                }

                                if (_state.LastType == TokenType.Word &&
                                    _state.LastText == "return")
                                {
                                    PrintSpace();
                                }
                            }

                            _state.Output.Append(_state.Token.Value);
                            genericsDepth++;
                        }
                        else
                        {
                            _state.Output.Append(_state.Token.Value);
                            genericsDepth--;
                        }

                        break;
                    case TokenType.Operator:

                        var startDelim = true;
                        var endDelim = true;
                        if (varLine && _state.TokenText != ",")
                        {
                            varLineTainted = true;
                            if (_state.TokenText == ":")
                            {
                                varLine = false;
                            }
                        }

                        if (varLine && _state.TokenText == "," && _state.CurrentMode == TsMode.Expression)
                        {
                            varLineTainted = false;
                        }

                        if (_state.TokenText == ":" && inCase)
                        {
                            PrintToken(); // colon really asks for separate treatment
                            PrintNewLine();
                            inCase = false;
                            break;
                        }

                        if (_state.TokenText == "::")
                        {
                            // no spaces around exotic namespacing syntax operator
                            PrintToken();
                            break;
                        }

                        if (_state.TokenText == ",")
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
                            else if (_state.LastType == TokenType.EndBlock)
                            {
                                PrintToken();
                                PrintNewLine();
                            }
                            else
                            {
                                if (_state.CurrentMode == TsMode.Block && !_state.IsImportBlock)
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
                        else if (_state.TokenText == "--" || _state.TokenText == "++")
                        {
                            // unary operators special case
                            if (_state.LastText == ";")
                            {
                                if (_state.CurrentMode == TsMode.Block)
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
                                if (_state.LastText == "{")
                                {
                                    PrintNewLine();
                                }

                                startDelim = false;
                                endDelim = false;
                            }
                        }
                        else if ((_state.TokenText == "!" || _state.TokenText == "+" || _state.TokenText == "-") &&
                                 (_state.LastText == "return" || _state.LastText == "case"))
                        {
                            startDelim = true;
                            endDelim = false;
                        }
                        else if ((_state.TokenText == "!" || _state.TokenText == "+" || _state.TokenText == "-") &&
                                 _state.LastType == TokenType.StartExpression)
                        {
                            // special case handling: if (!a)
                            startDelim = false;
                            endDelim = false;
                        }
                        else if (_state.LastType == TokenType.Operator)
                        {
                            startDelim = false;
                            endDelim = false;
                        }
                        else if (_state.LastType == TokenType.EndExpression)
                        {
                            startDelim = true;
                            endDelim = true;
                        }
                        else if (_state.TokenText == ".")
                        {
                            // decimal digits or object.property
                            startDelim = false;
                            endDelim = false;
                        }
                        else if (_state.TokenText == ":")
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

                        var isConditionalAccess = PeekNextChar() == '.';
                        var isMethodReturnType = IsMethodReturnType();
                        if (startDelim && !isConditionalAccess)
                        {
                            PrintSpace();
                        }

                        PrintToken();
                        if (endDelim && !isConditionalAccess)
                        {
                            PrintSpace();
                        }

                        break;

                    case TokenType.BlockComment:
                        PrintNewLineOrSpace(_state.Token);
                        PrintToken();
                        PrintNewLineOrSpace(_state.Token);
                        break;

                    case TokenType.Comment:

                        // print_newline();
                        if (_state.LastType == TokenType.StartBlock)
                        {
                            PrintNewLine();
                        }
                        else
                        {
                            PrintNewLineOrSpace(_state.Token);
                        }

                        PrintToken();
                        PrintNewLine();
                        break;

                    case TokenType.Unknown:
                        PrintToken();
                        break;
                }

                _state.LastType = tokenType;
                _state.LastText = _state.TokenText;
            }
        }

        private bool IsMethodReturnType()
        {
            // GetNextToken();
            return false;
        }

        private char? PeekNextChar()
        {
            var parserPos = _state.ParserPos;
            while (true)
            {
                if (_state.StaticState.Input.Length - 1 <= parserPos)
                {
                    return null;
                }

                var ch = _state.StaticState.Input[parserPos];
                if (ch.IsWhiteSpace())
                {
                    parserPos++;
                    continue;
                }

                return ch;
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
            while (_state.Output.Length > 0 && (SecondToLastChar == ' ' ||
                                                SecondToLastChar == _state.StaticState.IndentString[0]))
            {
                _state.Output.Remove(_state.Output.Length - 1, 1);
            }
        }

        private void PrintNewLine(bool? ignoreRepeated = null)
        {
            if (_state.LastText == "return" || _state.LastText == "!")
            {
                return;
            }

            ignoreRepeated = ignoreRepeated ?? true;

            _state.IfLineFlag = false;
            TrimOutput();

            if (_state.Output.Length == 0)
            {
                return;
            }

            if (SecondToLastChar != '\n' || !ignoreRepeated.Value)
            {
                _state.Output.Append(Environment.NewLine);
            }

            for (var i = 0; i < _state.IndentLevel; i++)
            {
                _state.Output.Append(_state.StaticState.IndentString);
            }
        }

        private void PrintSpace()
        {
            if (SecondToLastChar != ' ' && SecondToLastChar != '\n' &&
                SecondToLastChar != _state.StaticState.IndentString[0])
            {
                _state.Output.Append(' ');
            }
        }


        private void PrintToken()
        {
            _state.Output.Append(_state.TokenText);
        }

        private void Indent()
        {
            _state.IndentLevel++;
        }

        private void Unindent()
        {
            if (_state.IndentLevel > 0)
            {
                _state.IndentLevel--;
            }
        }

        private void RemoveIndent()
        {
            if (_state.Output.Length > 0 && SecondToLastChar == _state.StaticState.IndentString[0])
            {
                _state.Output.Remove(_state.Output.Length - 1, 1);
            }
        }

        private void SetMode(TsMode mode)
        {
            _state.Modes.Push(_state.CurrentMode);
            _state.CurrentMode = mode;
        }

        private void RestoreMode()
        {
            _state.DoBlockJustClosed = _state.CurrentMode == TsMode.DoBlock;
            _state.CurrentMode = _state.Modes.Pop();
        }

        private bool IsTernaryOperation()
        {
            var level = 0;
            var colonCount = 0;
            for (var i = _state.Output.Length - 1; i >= 0; i--)
            {
                switch (_state.Output[i])
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
    }
}