using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mtg
{
    public static class ClosestStringMatch
    {
        public static string Find(string input, IList<string> options)
        {
            if (options == null || options.Count == 0)
                return input;
            if (string.IsNullOrEmpty(input))
                return null;

            // calculate distance between input and all available options
            var matches = options.Select(opt => LevenshteinDistance(input.ToLower(), opt.ToLower())).ToList();

            // find match with smallest distance
            var best = int.MaxValue;
            var index = 0;
            var n = 0;
            foreach (var match in matches)
            {
                if (match < best)
                {
                    best = match;
                    index = n;
                }
                ++n;
            }
            return options[index];
        }

        public static int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];
            if (n == 0)
                return m;
            if (m == 0)
                return n;
            for (var i = 0; i <= n; d[i, 0] = i++)
                ;
            for (var j = 0; j <= m; d[0, j] = j++)
                ;
            for (var i = 1; i <= n; i++)
            {
                for (var j = 1; j <= m; j++)
                {
                    var cost = t[j - 1] == s[i - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }
    }
}
