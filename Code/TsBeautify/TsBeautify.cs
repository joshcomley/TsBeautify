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
        private readonly string _digits;
        private readonly string _genericsBrackets;
        private readonly string _indentString;
        private readonly string _input;
        private readonly string _lastText;
        private readonly string _lastType;

        private readonly Stack<string> _modes;
        private readonly bool _optPreserveNewlines;
        private readonly StringBuilder _output;
        private readonly int _parserPos;
        private readonly string[] _punct;
        private readonly string _tokenText;

        private readonly string _whitespace;
        private readonly string _wordchar;
        private string _currentMode;
        private bool _doBlockJustClosed;
        private readonly string _generics;
        private int _genericsDepth;
        private bool _ifLineFlag;
        private int _indentLevel;
        private readonly TsBeautifyOptions _options;
        private bool _isImportBlock;

        public TsBeautifierInstance(string jsSourceText, TsBeautifyOptions options = null, bool interpolation = false)
        {
            options = options ?? new TsBeautifyOptions();
            _options = options;
            var optIndentSize = options.IndentSize ?? 4;
            var optIndentChar = options.IndentChar ?? ' ';
            var optIndentLevel = options.IndentLevel ?? 0;
            _optPreserveNewlines = options.PreserveNewlines ?? true;
            _output = new StringBuilder();
            _modes = new Stack<string>();

            _indentString = "";

            while (optIndentSize > 0)
            {
                _indentString += optIndentChar;
                optIndentSize -= 1;
            }

            _indentLevel = optIndentLevel;


            _input = jsSourceText;

            var lastWord = "";
            _lastType = "TK_START_EXPR"; // last token type
            _lastText = ""; // last token text

            _doBlockJustClosed = false;
            var varLine = false;
            var varLineTainted = false;

            _whitespace = "\n\r\t ";
            _wordchar = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_$";
            _digits = "0123456789";
            _genericsBrackets = "<>";
            _generics = _whitespace + _wordchar + _digits + "," + _genericsBrackets;
            // <!-- is a special case (ok, it's a minor hack actually)
            _punct =
                "=> + - * / % & ++ -- = += -= *= /= %= == === != !== > < >= <= >> << >>> >>>= >>= <<= && &= | || ! !! , : ? ^ ^= |= ::"
                    .Split(' ');

            // words which should always start on new line.
            var lineStarters = "@test,import,let,continue,try,throw,return,var,if,switch,case,default,for,while,break,function".Split(',');

            // states showing if we are currently in expression (i.e. "if" case) - 'EXPRESSION', or in usual block (like, procedure), 'BLOCK'.
            // some formatting depends on that.
            _currentMode = "BLOCK";
            _modes.Push(_currentMode);

            _parserPos = 0;
            var inCase = false;
            var genericsDepth = 0;

            while (true)
            {
                var t = GetNextToken(ref _parserPos);
                _tokenText = t.Value;
                var tokenType = t.TokenType;
                if (tokenType == "TK_END_BLOCK" && interpolation && _indentLevel == 1)
                {
                    return;
                }

                if (tokenType == "TK_EOF")
                {
                    break;
                }

                switch (tokenType)
                {
                    case "TK_START_EXPR":
                        varLine = false;
                        SetMode("EXPRESSION");
                        if (_lastText == ";" || _lastType == "TK_START_BLOCK")
                        {
                            PrintNewLine();
                        }
                        else if (_lastType == "TK_END_EXPR" || _lastType == "TK_START_EXPR")
                        {
                            // do nothing on (( and )( and ][ and ]( ..
                        }
                        else if (_lastType != "TK_WORD" && _lastType != "TK_OPERATOR" && _lastType != "TK_GENERICS")
                        {
                            PrintSpace();
                        }
                        else if (lineStarters.Contains(lastWord))
                        {
                            PrintSpace();
                        }

                        PrintToken();
                        break;

                    case "TK_END_EXPR":
                        PrintToken();
                        RestoreMode();
                        break;

                    case "TK_START_IMPORT":
                    case "TK_END_IMPORT":
                        PrintSpace();
                        _output.Append(t.Value);
                        PrintSpace();
                        break;
                    case "TK_START_BLOCK":

                        if (lastWord == "do")
                        {
                            SetMode("DO_BLOCK");
                        }
                        else
                        {
                            SetMode("BLOCK");
                        }

                        if (_lastType != "TK_OPERATOR" && _lastType != "TK_START_EXPR")
                        {
                            if (_lastType == "TK_START_BLOCK")
                            {
                                PrintNewLine();
                            }
                            else
                            {
                                if (options.OpenBlockOnNewLine)
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

                    case "TK_END_BLOCK":
                        if (_lastType == "TK_START_BLOCK")
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

                    case "TK_WORD":

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

                        if (_lastType == "TK_END_BLOCK")
                        {
                            if (!new[] { "else", "catch", "finally" }.Contains(_tokenText.ToLower()))
                            {
                                prefix = "NEWLINE";
                            }
                            else
                            {
                                prefix = "SPACE";
                                PrintSpace();
                            }
                        }
                        else if (_lastType == "TK_SEMICOLON" && (_currentMode == "BLOCK" || _currentMode == "DO_BLOCK"))
                        {
                            prefix = "NEWLINE";
                        }
                        else if (_lastType == "TK_SEMICOLON" && _currentMode == "EXPRESSION")
                        {
                            prefix = "SPACE";
                        }
                        else if (_lastType == "TK_STRING")
                        {
                            prefix = "NEWLINE";
                        }
                        else if (_lastType == "TK_WORD")
                        {
                            prefix = "SPACE";
                        }
                        else if (_lastType == "TK_START_BLOCK")
                        {
                            prefix = "NEWLINE";
                        }
                        else if (_lastType == "TK_END_EXPR")
                        {
                            PrintSpace();
                            prefix = "NEWLINE";
                        }

                        if (_lastType != "TK_END_BLOCK" &&
                            new[] { "else", "catch", "finally" }.Contains(_tokenText.ToLower()))
                        {
                            PrintNewLine();
                        }
                        else if (lineStarters.Contains(_tokenText) || prefix == "NEWLINE")
                        {
                            if (_lastText == "else")
                            {
                                // no need to force newline on else break
                                PrintSpace();
                            }
                            else if ((_lastType == "TK_START_EXPR" || _lastText == "=" || _lastText == ",") &&
                                     _tokenText == "function")
                            {
                                // no need to force newline on "function": (function
                                // DONOTHING
                            }
                            else if (_lastType == "TK_WORD" && (_lastText == "return" || _lastText == "throw"))
                            {
                                // no newline between "return nnn"
                                PrintSpace();
                            }
                            else if (_lastType != "TK_END_EXPR")
                            {
                                if ((_lastType != "TK_START_EXPR" || _tokenText != "var") && _lastText != ":")
                                {
                                    if (_tokenText == "if" && _lastType == "TK_WORD" && lastWord == "else")
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
                                if (lineStarters.Contains(_tokenText) && _lastText != ")")
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

                    case "TK_SEMICOLON":

                        PrintToken();
                        varLine = false;
                        break;

                    case "TK_STRING":

                        if (_lastType == "TK_START_BLOCK" || _lastType == "TK_END_BLOCK" || _lastType == "TK_SEMICOLON")
                        {
                            PrintNewLine();
                        }
                        else if (_lastType == "TK_WORD" && _lastText != "$")
                        {
                            PrintSpace();
                        }

                        PrintToken();
                        break;
                    case "TK_GENERICS":
                        if (t.Value == "<")
                        {
                            if (genericsDepth == 0)
                            {
                                if (_lastType == "TK_WORD" &&
                                    (_lastText == "return"))
                                {
                                    PrintSpace();
                                }
                            }
                            _output.Append(t.Value);
                            genericsDepth++;
                        }
                        else
                        {
                            _output.Append(t.Value);
                            genericsDepth--;
                        }
                        break;
                    case "TK_OPERATOR":

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

                        if (varLine && _tokenText == "," && _currentMode == "EXPRESSION")
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
                            else if (_lastType == "TK_END_BLOCK")
                            {
                                PrintToken();
                                PrintNewLine();
                            }
                            else
                            {
                                if (_currentMode == "BLOCK" && !_isImportBlock)
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
                                if (_currentMode == "BLOCK")
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
                                 _lastType == "TK_START_EXPR")
                        {
                            // special case handling: if (!a)
                            startDelim = false;
                            endDelim = false;
                        }
                        else if (_lastType == "TK_OPERATOR")
                        {
                            startDelim = false;
                            endDelim = false;
                        }
                        else if (_lastType == "TK_END_EXPR")
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

                    case "TK_BLOCK_COMMENT":
                        PrintNewLineOrSpace(t);
                        PrintToken();
                        PrintNewLineOrSpace(t);
                        break;

                    case "TK_COMMENT":

                        // print_newline();
                        if (_lastType == "TK_START_BLOCK")
                        {
                            PrintNewLine();
                        }
                        else
                        {
                            PrintNewLineOrSpace(t);
                        }
                        PrintToken();
                        PrintNewLine();
                        break;

                    case "TK_UNKNOWN":
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
            else
            {
                PrintSpace();
            }
            return false;
        }

        private bool InGenerics => _genericsDepth > 0;


        private void TrimOutput()
        {
            while (_output.Length > 0 && (_output[_output.Length - 1] == ' ' ||
                                          _output[_output.Length - 1].ToString() == _indentString))
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

            if (_output[_output.Length - 1] != '\n' || !ignoreRepeated.Value)
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
            var lastOutput = " ";
            if (_output.Length > 0)
            {
                lastOutput = _output[_output.Length - 1].ToString();
            }

            if (lastOutput != " " && lastOutput != "\n" && lastOutput != _indentString)
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
            if (_output.Length > 0 && _output[_output.Length - 1].ToString() == _indentString)
            {
                _output.Remove(_output.Length - 1, 1);
            }
        }

        private void SetMode(string mode)
        {
            _modes.Push(_currentMode);
            _currentMode = mode;
        }

        private void RestoreMode()
        {
            _doBlockJustClosed = _currentMode == "DO_BLOCK";
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

        private Token GetNextToken(ref int parserPos)
        {
            var newLineCount = 0;

            if (parserPos >= _input.Length)
            {
                return new Token("", "TK_EOF", newLineCount);
            }

            var c = _input[parserPos].ToString();
            parserPos++;

            while (_whitespace.Contains(c))
            {
                if (parserPos >= _input.Length)
                {
                    return new Token("", "TK_EOF", newLineCount);
                }

                if (c == "\n")
                {
                    newLineCount++;
                }

                c = _input[parserPos].ToString();
                parserPos++;
            }

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
            if (c == "<")
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

                        if (!_generics.Contains(cSub))
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
            else if (c == ">" && _genericsDepth > 0)
            {
                isGenerics = true;
                _genericsDepth--;
            }

            if (isGenerics && _genericsBrackets.Contains(c))
            {
                return new Token(c, "TK_GENERICS", newLineCount);
            }


            if (_wordchar.Contains(c))
            {
                if (parserPos < _input.Length)
                {
                    while (_wordchar.Contains(_input[parserPos]))
                    {
                        c += _input[parserPos];
                        parserPos++;
                        if (parserPos == _input.Length)
                        {
                            break;
                        }
                    }
                }


                if (parserPos != _input.Length && Regex.IsMatch(c, "^[0-9]+[Ee]$") &&
                    (_input[parserPos] == '-' || _input[parserPos] == '+'))
                {
                    var sign = _input[parserPos];
                    parserPos++;

                    var t = GetNextToken(ref parserPos);
                    c += sign + t.Value;
                    return new Token(c, "TK_WORD", newLineCount);
                }

                if (c == "in")
                {
                    return new Token(c, "TK_OPERATOR", newLineCount);
                }

                if (wantedNewline && _lastType != "TK_OPERATOR" && !_ifLineFlag)
                {
                    PrintNewLine();
                }

                return new Token(c, "TK_WORD", newLineCount);
            }

            if (c == "(" || c == "[")
            {
                return new Token(c, "TK_START_EXPR", newLineCount);
            }

            if (c == ")" || c == "]")
            {
                return new Token(c, "TK_END_EXPR", newLineCount);
            }

            if (c == "{")
            {
                if (_lastText == "import")
                {
                    _isImportBlock = true;
                    return new Token(c, "TK_START_IMPORT", newLineCount);
                }
                return new Token(c, "TK_START_BLOCK", newLineCount);
            }

            if (c == "}")
            {
                if (_isImportBlock)
                {
                    _isImportBlock = false;
                    return new Token(c, "TK_END_IMPORT", newLineCount);
                }
                return new Token(c, "TK_END_BLOCK", newLineCount);
            }

            if (c == ";")
            {
                return new Token(c, "TK_SEMICOLON", newLineCount);
            }

            if (c == "/")
            {
                var comment = "";
                if (_input[parserPos] == '*')
                {
                    parserPos++;
                    if (parserPos < _input.Length)
                    {
                        while (!(_input[parserPos] == '*' && _input[parserPos + 1] > '\0' &&
                                 _input[parserPos + 1] == '/' && parserPos < _input.Length))
                        {
                            comment += _input[parserPos];
                            parserPos++;
                            if (parserPos >= _input.Length)
                            {
                                break;
                            }
                        }
                    }

                    parserPos += 2;
                    return new Token("/*" + comment + "*/", "TK_BLOCK_COMMENT", newLineCount);
                }

                if (_input[parserPos] == '/')
                {
                    comment = c;
                    while (_input[parserPos] != '\x0d' && _input[parserPos] != '\x0a')
                    {
                        comment += _input[parserPos];
                        parserPos++;
                        if (parserPos >= _input.Length)
                        {
                            break;
                        }
                    }

                    parserPos++;
                    if (wantedNewline)
                    {
                        PrintNewLine();
                    }

                    return new Token(comment, "TK_COMMENT", newLineCount);
                }
            }

            StringInterpolationKind interpolationAllowed = StringInterpolationKind.None;
            // Allow C# interpolated strings
            if (c == "$" || _lastText == "$")
            {
                var lookahead = c == "$" ? 1 : 0;
                var next = At(parserPos + lookahead);
                if (next == "@" || next == "\"")
                {
                    interpolationAllowed = StringInterpolationKind.CSharp;
                    if (lookahead == 1)
                    {
                        _output.Append(c);
                        c = _input[parserPos].ToString();
                        parserPos++;
                    }
                }
            }
            if (c == "@" && _input[parserPos].ToString() == "\"")
            {
                _output.Append(c);
                c = _input[parserPos].ToString();
                parserPos++;
            }
            if ((c == "'" || c == "\"" || c == "/" || c == "`")
                && ((_lastType == "TK_WORD" && (_lastText == "$" || _lastText == "return" || _lastText == "from" || _lastText == "case")) || _lastType == "TK_START_EXPR" ||
                    _lastType == "TK_START_BLOCK" || _lastType == "TK_END_BLOCK" || _lastType == "TK_OPERATOR" ||
                    _lastType == "TK_EOF" || _lastType == "TK_SEMICOLON")
            )
            {
                var sep = c;
                var esc = false;
                var resultingString = c;

                if (parserPos < _input.Length)
                {
                    if (sep == "/")
                    {
                        var inCharClass = false;
                        while (esc || inCharClass || _input[parserPos].ToString() != sep)
                        {
                            resultingString += _input[parserPos];
                            if (!esc)
                            {
                                esc = _input[parserPos] == '\\';
                                if (_input[parserPos] == '[')
                                {
                                    inCharClass = true;
                                }
                                else if (_input[parserPos] == ']')
                                {
                                    inCharClass = false;
                                }
                            }
                            else
                            {
                                esc = false;
                            }

                            parserPos++;
                            if (parserPos >= _input.Length)
                            {
                                return new Token(resultingString, "TK_STRING", newLineCount);
                            }
                        }
                    }
                    else
                    {
                        if (c == "`")
                        {
                            interpolationAllowed = StringInterpolationKind.TypeScript;
                        }
                        while (esc || _input[parserPos].ToString() != sep)
                        {
                            var interpolationActioned = false;
                            switch (interpolationAllowed)
                            {
                                case StringInterpolationKind.CSharp:
                                    if (_input.StartsWithAt("{", parserPos) && !_input.StartsWithAt("{{", parserPos))
                                    {
                                        interpolationActioned = true;
                                    }
                                    break;
                                case StringInterpolationKind.TypeScript:
                                    if (_input.StartsWithAt("${", parserPos))
                                    {
                                        var escapeCount = 0;
                                        for (var i = parserPos - 1; i > 0; i--)
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
                                            var jsSourceText = _input.Substring(parserPos);
                                            var sub = new TsBeautifierInstance(jsSourceText, _options, true);
                                            var interpolated = sub.Beautify().TrimStart('{').Trim();
                                            resultingString += $"{{{interpolated}}}";
                                            parserPos += sub._parserPos;
                                        }
                                        break;
                                    case StringInterpolationKind.TypeScript:
                                        {
                                            var jsSourceText = _input.Substring(parserPos + 1);
                                            var sub = new TsBeautifierInstance(jsSourceText, _options, true);
                                            var interpolated = sub.Beautify().TrimStart('{').Trim();
                                            resultingString += $"${{{interpolated}}}";
                                            parserPos += sub._parserPos + 1;
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                resultingString += _input[parserPos];
                                if (!esc)
                                {
                                    esc = _input[parserPos] == '\\';
                                }
                                else
                                {
                                    esc = false;
                                }

                                parserPos++;
                            }

                            if (parserPos >= _input.Length)
                            {
                                return new Token(resultingString, "TK_STRING", newLineCount);
                            }
                        }
                    }
                }

                parserPos += 1;

                resultingString += sep;

                if (sep == "/")
                {
                    while (parserPos < _input.Length && _wordchar.Contains(_input[parserPos]))
                    {
                        resultingString += _input[parserPos];
                        parserPos += 1;
                    }
                }

                return new Token(resultingString, "TK_STRING", newLineCount);
            }

            if (c == "#")
            {
                var sharp = "#";
                if (parserPos < _input.Length && _digits.Contains(_input[parserPos]))
                {
                    do
                    {
                        c = _input[parserPos].ToString();
                        sharp += c;
                        parserPos += 1;
                    } while (parserPos < _input.Length && c != "#" && c != "=");

                    if (c == "#")
                    {
                        return new Token(sharp, "TK_WORD", newLineCount);
                    }

                    return new Token(sharp, "TK_OPERATOR", newLineCount);
                }
            }


            if (c == "<" && _input.Substring(parserPos - 1, 3) == "<!--")
            {
                parserPos += 3;
                return new Token("<!--", "TK_COMMENT", newLineCount);
            }

            if (c == "-" && _input.Substring(parserPos - 1, 2) == "-->")
            {
                parserPos += 2;
                if (wantedNewline)
                {
                    PrintNewLine();
                }

                return new Token("-->", "TK_COMMENT", newLineCount);
            }

            if (_punct.Contains(c))
            {
                while (parserPos < _input.Length && _punct.Contains(c + _input[parserPos]))
                {
                    c += _input[parserPos];
                    parserPos += 1;
                    if (parserPos >= _input.Length)
                    {
                        break;
                    }
                }

                return new Token(c, "TK_OPERATOR", newLineCount);
            }

            return new Token(c, "TK_UNKNOWN", newLineCount);
        }

        private string At(int position)
        {
            if (position > _input.Length - 1)
            {
                return null;
            }

            return _input[position].ToString();
        }

        public string Beautify()
        {
            return _output.ToString();
        }

        enum StringInterpolationKind
        {
            None = 1,
            CSharp = 2,
            TypeScript = 3
        };
    }

    internal class Token
    {
        public string Value { get; set; }
        public string TokenType { get; set; }
        public int NewLineCount { get; set; }

        public Token(string token, string tokenType, int newLineCount = 0)
        {
            Value = token;
            TokenType = tokenType;
            NewLineCount = newLineCount;
        }
    }
}