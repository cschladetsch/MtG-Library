using System;
using System.Collections.Generic;
using System.Linq;

namespace Mtg
{
    /// <summary>
    /// Internal representation of the library.
    /// There is a set of unique Cards, then a Count for each instance of
    /// a card the library contains.
    /// </summary>
    internal class PersistentLibrary
    {
        public readonly Dictionary<Guid, Card> Cards = new Dictionary<Guid, Card>();
        public readonly Dictionary<Guid, int> Counts = new Dictionary<Guid, int>();

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

        public Card Find(Guid id)
        {
            return Cards[id];
        }

        public Card Find(string title)
        {
            return Cards.Values.FirstOrDefault(c => c.Title.ToLower() == title.ToLower());
        }
    }
}
