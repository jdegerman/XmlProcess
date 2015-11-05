using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace XmlProcess
{
    public static class Extensions
    {
        #region - String methods -
        public static bool IsMatch(this string s, string pattern)
        {
            return Regex.IsMatch(s, pattern);
        }
        public static string Replace(this string s, string pattern, Func<Match, string> replaceEvaluator)
        {
            return Regex.Replace(s, pattern, new MatchEvaluator(replaceEvaluator));
        }
        public static Func<char> ToCharStream(this string input)
        {
            var index = 0;
            return () => index < input.Length ? input[index++] : '\0';
        }
        public static IEnumerable<char> ToCharIterator(this string input)
        {
            var index = 0;
            while (index < input.Length)
                yield return input[index++];
            yield return '\0';
        }
        #endregion
        #region - Array methods -
        public static T[] Tail<T>(this T[] arr)
        {
            if (arr == null || arr.Length < 2)
                return new T[0];
            var result = new T[arr.Length - 1];
            Array.Copy(arr, 1, result, 0, arr.Length - 1);
            return result;
        }
        public static bool TryGet<T>(this T[] array, int index, out T value)
        {
            value = default(T);
            if (array == null || index < 0 || index >= array.Length)
                return false;
            value = array[index];
            return true;
        }
        #endregion
        #region - Dictionary methods -
        public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            if (!dict.ContainsKey(key))
                throw new Exception("Value '" + key + "' not specified");
            return dict[key];
        }
        #endregion
        #region - Token queue methods -
        public static void EnqueueTokenIf(this Queue<Token> q, ref string item, int row)
        {
            if (string.IsNullOrEmpty(item))
                return;
            q.Enqueue(new Token(item, row));
            item = "";
        }
        public static void EnqueueToken(this Queue<Token> q, char c, int row)
        {
            q.Enqueue(new Token(c, row));
        }
        public static void EnqueueToken(this Queue<Token> q, string s, int row)
        {
            q.Enqueue(new Token(s, row));
        }
        public static bool Match(this Queue<Token> q, string pattern, out Token result)
        {
            result = null;
            return q.Count > 0 && (result = q.Dequeue()).Value.IsMatch(pattern);
        }
        public static bool MatchPeek(this Queue<Token> q, string pattern, out Token result)
        {
            result = null;
            return q.Count > 0 && (result = q.Peek()).Value.IsMatch(pattern);
        }
        public static Token Require(this Queue<Token> q, string pattern, string error)
        {
            Token t;
            if (q.Match(pattern, out t))
                return t;
            else
                throw new Exception(error + " at row " + (t == null ? "UNKNOWN" : t.Row.ToString()) + ", found " + (t == null ? "UNKNOWN" : t.Value.ToString()));
        }
        public static Token RequirePeek(this Queue<Token> q, string pattern, string error)
        {
            Token t;
            if (q.MatchPeek(pattern, out t))
                return t;
            else
                throw new Exception(error + " at row " + (t == null ? "UNKNOWN" : t.Row.ToString()) + ", found " + (t == null ? "UNKNOWN" : t.Value.ToString()));
        }
        #endregion
    }
}
