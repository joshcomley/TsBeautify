using System.Collections.Generic;
using System.Linq;

namespace TsBeautify
{
    internal class StaticState
    {
        public readonly Dictionary<char, bool> DigitsChars;
        public readonly Dictionary<char, bool> GenericsBracketsChars;
        public readonly Dictionary<char, bool> GenericsChars;

        public readonly string IndentString;
        public readonly string Input;
        public readonly Dictionary<string, bool> LineStarters;
        public readonly TsBeautifyOptions Options;
        public readonly bool PreserveNewlines;
        public readonly Dictionary<string, bool> Punctuation;
        public readonly Dictionary<char, bool> WhitespaceChars;
        public readonly Dictionary<char, bool> WordCharChars;

        public StaticState(string input, TsBeautifyOptions options)
        {
            Input = input;
            var indentString = "";
            var optIndentSize = options.IndentSize ?? 4;
            var optIndentChar = options.IndentChar ?? ' ';

            while (optIndentSize > 0)
            {
                indentString += optIndentChar;
                optIndentSize -= 1;
            }

            IndentString = indentString;
            var whitespace = ToLookup(Constants.WhitespaceChars);
            WhitespaceChars = ToCharLookup(whitespace);
            var wordChars = ToLookup(Constants.WordChars);
            var digits = ToLookup(Constants.Chars);
            var genericsBrackets = ToLookup(Constants.GenericsChars);
            var generics = ToLookup($"{Constants.WhitespaceChars}{Constants.WordChars},{Constants.GenericsChars}");
            DigitsChars = ToCharLookup(digits);
            WordCharChars = ToCharLookup(wordChars);
            GenericsChars = ToCharLookup(generics);
            GenericsBracketsChars = ToCharLookup(genericsBrackets);
            // <!-- is a special case (ok, it's a minor hack actually)
            Punctuation = ArrayToLookup(
                "=> + - * / % & ++ -- = += -= *= /= %= == === != !== > < >= <= >> << >>> >>>= >>= <<= && &= | || ?? ! !! , : ? ^ ^= |= ::"
                    .Split(' '));

            // words which should always start on new line.
            LineStarters = ArrayToLookup(
                "@test,import,let,continue,try,throw,return,var,if,switch,case,default,for,while,break,function"
                    .Split(','));
            Options = options;
            PreserveNewlines = options.PreserveNewlines ?? true;
        }

        private static Dictionary<string, bool> ToLookup(string str)
        {
            var chars = str.ToCharArray();
            var dic = new Dictionary<string, bool>();
            for (var i = 0; i < chars.Length; i++)
            {
                dic.Add(chars[i].ToString(), true);
            }

            return dic;
        }

        private static Dictionary<string, bool> ArrayToLookup(IEnumerable<string> str)
        {
            var strings = str.ToArray();
            var dic = new Dictionary<string, bool>();
            for (var i = 0; i < strings.Length; i++)
            {
                dic.Add(strings[i], true);
            }

            return dic;
        }

        private static Dictionary<char, bool> ToCharLookup(Dictionary<string, bool> lookup)
        {
            var dic = new Dictionary<char, bool>();
            foreach (var kvp in lookup)
            {
                dic.Add(kvp.Key[0], true);
            }

            return dic;
        }
    }
}