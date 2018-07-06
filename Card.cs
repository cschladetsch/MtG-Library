using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Flurl;
using Flurl.Http;

namespace Mtg
{
    /// <summary>
    /// Represents a card in your library, or a card you're interested in.
    /// </summary>
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

        private const string Endpoint = "https://api.scryfall.com";

        public async Task<bool> PullInfo(Guid id)
        {
            return false;
        }

        public async Task<bool> PullInfo()
        {
            return false;
        }
        private string _title;
    }
}
