namespace TsBeautify.Extensions
{
    public static class StringExtensions
    {
        public static bool IsWhiteSpace(this char ch)
        {
            return ch == ' ' || ch == ' ';
        }
        
        public static bool StartsWithAt(this string str, string toLookFor, int atPosititon)
        {
            var pos = atPosititon;
            for (var i = 0; i < toLookFor.Length; i++)
            {
                var ch = toLookFor[i];
                if (str[pos] != ch)
                {
                    return false;
                }

                pos++;
            }
            return true;
        }
    }
}