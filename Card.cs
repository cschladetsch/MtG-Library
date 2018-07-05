using System;

namespace Mtg
{
    class Card
    {
        public Guid Id;
        public Guid TypeId;
        public string ScannedTitle;
        public string Title;
        public ManaCost ManaCost;
        public ECardType Type;
        public string Name;
        public string Text;
        public ERelease Release;
    }
}
