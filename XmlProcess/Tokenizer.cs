using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlProcess
{
    static class Tokenizer
    {
        [ThreadStatic]
        static Queue<Token> tokens = new Queue<Token>();

        public static void TokenizeInput(TextReader input)
        {
            tokens.Clear();
            string row;
            int rowNo = 0;
            while ((row = input.ReadLine()) != null)
            {
                rowNo++;
                TokenizeRow(row, rowNo);
            }
        }
        public static bool Match(string pattern, out Token result)
        {
            return tokens.Match(pattern, out result);
        }
        public static bool MatchPeek(string pattern, out Token result)
        {
            return tokens.MatchPeek(pattern, out result);
        }
        public static Token Require(string pattern, string error)
        {
            return tokens.Require(pattern, error);
        }
        public static Token RequirePeek(string pattern, string error)
        {
            return tokens.RequirePeek(pattern, error);
        }
        private static void TokenizeRow(string row, int rowNo)
        {
            var stream = row.ToCharStream();
            char c;
            var buffer = string.Empty;
            while ((c = stream()) != '\0')
            {
                switch (c)
                {
                    case '{':
                    case '}':
                    case '(':
                    case ')':
                    case ';':
                        tokens.EnqueueTokenIf(ref buffer, rowNo);
                        tokens.EnqueueToken(c, rowNo);
                        break;
                    case ' ':
                    case '\t':
                    case '\n':
                    case '\r':
                        tokens.EnqueueTokenIf(ref buffer, rowNo);
                        break;
                    case '\'':
                    case '"':
                        var endchar = c;
                        tokens.EnqueueTokenIf(ref buffer, rowNo);
                        while ((c = stream()) != endchar)
                        {
                            if (c == '\0')
                                throw new Exception("Unexpected end-of-line in string on row " + rowNo);
                            buffer += c.ToString();
                        }
                        tokens.EnqueueToken("\"" + buffer + "\"", rowNo);
                        buffer = "";
                        break;
                    default:
                        buffer += c.ToString();
                        break;
                }
            }
        }
    }
}
