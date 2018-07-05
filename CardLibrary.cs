using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Mtg
{
    /// <summary>
    /// A collection of cards
    /// </summary>
    class CardLibrary
    {
        public const string FileName = "Cards.json";

        /// <summary>
        /// Internal representation of the library.
        /// There is a set of unique Cards, then a Count for each instance of
        /// a card the library contains.
        /// </summary>
        class Library
        {
            public Dictionary<Guid, Card> Cards = new Dictionary<Guid, Card>();
            public Dictionary<Guid, int> Counts = new Dictionary<Guid, int>();

            public void Clear()
            {
                Cards.Clear();
                Counts.Clear();
            }

            public void AddInstance(Guid cardTypeId)
            {
                if (!Counts.ContainsKey(cardTypeId))
                    Counts.Add(cardTypeId, 0);
                Counts[cardTypeId]++;
            }

            public void AddType(Card card)
            {
                Cards.Add(card.TypeId, card);
            }

            public Card Find(string title)
            {
                // TODO: slightly fuzzy find?
                return Cards.Values.FirstOrDefault(c => c.ScannedTitle.Contains(title) || c.Title == title);
            }
        }

        public IEnumerable<Card> Cards
        {
            get
            {
                foreach (var kv in _library.Counts)
                {
                    for (int n = 0; n < kv.Value; ++n)
                    {
                        yield return _library.Cards[kv.Key];
                    }
                }
            }
        }

        public void Clear()
        {
            _library.Clear();
        }

        public int Load(string fileName = FileName)
        {
            if (!File.Exists(fileName))
            {
                Console.WriteLine("Starting new library");
                _library = new Library();
                return 0;
            }

            var text = File.ReadAllText(fileName);
            _library = JsonConvert.DeserializeObject<Library>(text);
            return _library.Counts.Values.Aggregate(0, (a, b) => a + b);
        }

        public void Save(string fileName = FileName)
        {
            File.WriteAllText(fileName, JsonConvert.SerializeObject(_library));
        }

        public void Add(VisionResponse res)
            //, Func<string, List<string>, bool> validateExisting
            //, Func<string, Image, bool> valdiateNew)
        {
            var text = res.responses[0].FullTextAnnotation.text;
            var split = text.Split('\n');
            var title = TrimMana(split[0]);

            var card = new Card()
            {
                Title = title,
            };

            var existing = _library.Find(title);
            if (existing != null)
            {
                Console.WriteLine($"Duplicate card {card.Title}");
                card.TypeId = existing.TypeId;
            }
            else
            {
                Console.WriteLine($"New card {card.Title}");
                card.TypeId = Guid.NewGuid();
                _library.AddType(card);
            }

            _library.AddInstance(card.TypeId);
        }

        private static string TrimMana(string title)
        {
            var trim1 = title.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ');
            return trim1;
        }

        public void Export(string fileName)
        {
            var ext = Path.GetExtension(fileName);
            switch (ext)
            {
                case ".tappedout":
                    ExportTappedOut(fileName);
                    break;
            }
        }

        private void ExportTappedOut(string fileName)
        {
            var sb = new StringBuilder();
            foreach (var c in _library.Counts)
            {
                var entry = $"{c.Value}x {_library.Cards[c.Key].Title}";
                sb.AppendLine(entry);
            }

            File.WriteAllText(fileName, sb.ToString());
        }

        private Library _library;

    }
}
