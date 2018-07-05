using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mtg
{
    class Card
    {
        public Guid TypeId;
        public List<string> ScannedTitle = new List<string>();

        public string Title
        {
            get => _title;
            set => _title = UpperCaseWords(value);
        }
        public ManaCost ManaCost;
        public ECardType Type;
        public string Name;
        public string Text;
        public ERelease Release;

        //private string ScannedToString()
        //{
        //    var sb = new StringBuilder();
        //    var sep = "";
        //    foreach (var s in ScannedTitle)
        //    {
        //        sb.Append($"'{s}{sep}");
        //        sep = ", ";
        //    }
        //    return sb.ToString();
        //}

        public override string ToString()
        {
            return ($"Title={Title}, Scanned={ScannedTitle.Aggregate("", (a, b) => a + ", " + b)}, Text={Text}");
        }

        public static string UpperCaseWords(string n)
        {
            var split = n.Split(' ');
            var result = "";
            foreach (var word in split)
            {
                var c = word[0];
                if (char.IsWhiteSpace(c))
                {
                    result += c;
                    continue;
                }
                var f = char.ToUpper(c);
                result += f + word.Substring(1) + ' ';
            }
            return result.Trim();
        }

        private string _title;
    }
}
