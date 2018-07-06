using System.Collections.Generic;

namespace Mtg
{
    public class ManaCost
    {
        public int GetCost(EManaType type)
        {
            return !_costs.TryGetValue(type, out var mana) ? 0 : mana;
        }

        public static ManaCost FromString(string text)
        {
            var cost = new ManaCost();
            return cost;
        }

        private Dictionary<EManaType, int> _costs = new Dictionary<EManaType, int>();
    }
}
