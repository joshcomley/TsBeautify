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
        private readonly bool _addScriptTags;
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


            _input = jsSourceText.Replace("<script type=\"text/javascript\">", "").Replace("</script>", "");
            if (_input.Length != jsSourceText.Length)
            {
                _output.AppendLine("<script type=\"text/javascript\">");
                _addScriptTags = true;
            }

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

            while (true)
            {
                var t = GetNextToken(ref _parserPos);
                _tokenText = t[0];
                var tokenType = t[1];
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
                            PrintNewLine(null);
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
                        _output.Append(t[0]);
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
                                PrintNewLine(null);
                            }
                            else
                            {
                                PrintSpace();
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
                            PrintNewLine(null);
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
                                PrintNewLine(null);
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
                            PrintNewLine(null);
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
                                        PrintNewLine(null);
                                    }
                                }
                            }
                            else
                            {
                                if (lineStarters.Contains(_tokenText) && _lastText != ")")
                                {
                                    PrintNewLine(null);
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
                            PrintNewLine(null);
                        }
                        else if (_lastType == "TK_WORD")
                        {
                            PrintSpace();
                        }

                        PrintToken();
                        break;
                    case "TK_GENERICS":
                        _output.Append(t[0]);
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
                            PrintNewLine(null);
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
                                    PrintNewLine(null);
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
                                PrintNewLine(null);
                            }
                            else
                            {
                                if (_currentMode == "BLOCK" && !_isImportBlock)
                                {
                                    PrintToken();
                                    PrintNewLine(null);
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
                                    PrintNewLine(null);
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
                                    PrintNewLine(null);
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

                        PrintNewLine(null);
                        PrintToken();
                        PrintNewLine(null);
                        break;

                    case "TK_COMMENT":

                        // print_newline();
                        PrintSpace();
                        PrintToken();
                        PrintNewLine(null);
                        break;

                    case "TK_UNKNOWN":
                        PrintToken();
                        break;
                }

                _lastType = tokenType;
                _lastText = _tokenText;
            }
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

        private void PrintNewLine(bool? ignoreRepeated)
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

        private string[] GetNextToken(ref int parserPos)
        {
            var nNewlines = 0;

            if (parserPos >= _input.Length)
            {
                return new[] { "", "TK_EOF" };
            }

            var c = _input[parserPos].ToString();
            parserPos++;

            while (_whitespace.Contains(c))
            {
                if (parserPos >= _input.Length)
                {
                    return new[] { "", "TK_EOF" };
                }

                if (c == "\n")
                {
                    nNewlines++;
                }

                c = _input[parserPos].ToString();
                parserPos++;
            }

            var wantedNewline = false;

            if (_optPreserveNewlines)
            {
                if (nNewlines > 1)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        PrintNewLine(i == 0);
                    }
                }

                wantedNewline = nNewlines == 1;
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
                return new[] { c, "TK_GENERICS" };
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
                    c += sign + t[0];
                    return new[] { c, "TK_WORD" };
                }

                if (c == "in")
                {
                    return new[] { c, "TK_OPERATOR" };
                }

                if (wantedNewline && _lastType != "TK_OPERATOR" && !_ifLineFlag)
                {
                    PrintNewLine(null);
                }

                return new[] { c, "TK_WORD" };
            }

            if (c == "(" || c == "[")
            {
                return new[] { c, "TK_START_EXPR" };
            }

            if (c == ")" || c == "]")
            {
                return new[] { c, "TK_END_EXPR" };
            }

            if (c == "{")
            {
                if (_lastText == "import")
                {
                    _isImportBlock = true;
                    return new[] { c, "TK_START_IMPORT" };
                }
                return new[] { c, "TK_START_BLOCK" };
            }

            if (c == "}")
            {
                if (_isImportBlock)
                {
                    _isImportBlock = false;
                    return new[] { c, "TK_END_IMPORT" };
                }
                return new[] { c, "TK_END_BLOCK" };
            }

            if (c == ";")
            {
                return new[] { c, "TK_SEMICOLON" };
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
                    return new[] { "/*" + comment + "*/", "TK_BLOCK_COMMENT" };
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
                        PrintNewLine(null);
                    }

                    return new[] { comment, "TK_COMMENT" };
                }
            }

            if (c == "'" || c == "\"" || c == "/" || c == "`"
                && (_lastType == "TK_WORD" && _lastText == "return" || _lastType == "TK_START_EXPR" ||
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
                                return new[] { resultingString, "TK_STRING" };
                            }
                        }
                    }
                    else
                    {
                        var interpolationAllowed = c == "`";
                        while (esc || _input[parserPos].ToString() != sep)
                        {
                            if (interpolationAllowed && _input.StartsWithAt("${", parserPos))
                            {
                                var jsSourceText = _input.Substring(parserPos + 1);
                                var sub = new TsBeautifierInstance(jsSourceText, _options, true);
                                var interpolated = sub.Beautify().TrimStart('{').Trim();
                                resultingString += $"${{{interpolated}}}";
                                parserPos += sub._parserPos + 1;
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
                                return new[] { resultingString, "TK_STRING" };
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

                return new[] { resultingString, "TK_STRING" };
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
                        return new[] { sharp, "TK_WORD" };
                    }

                    return new[] { sharp, "TK_OPERATOR" };
                }
            }


            if (c == "<" && _input.Substring(parserPos - 1, 3) == "<!--")
            {
                parserPos += 3;
                return new[] { "<!--", "TK_COMMENT" };
            }

            if (c == "-" && _input.Substring(parserPos - 1, 2) == "-->")
            {
                parserPos += 2;
                if (wantedNewline)
                {
                    PrintNewLine(null);
                }

                return new[] { "-->", "TK_COMMENT" };
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

                return new[] { c, "TK_OPERATOR" };
            }

            return new[] { c, "TK_UNKNOWN" };
        }

        public string Beautify()
        {
            if (_addScriptTags)
            {
                _output.AppendLine().AppendLine("</script>");
            }

            return _output.ToString();
        }
    }
}