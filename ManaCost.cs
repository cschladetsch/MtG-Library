using System.Collections.Generic;

namespace Mtg
{
    public class ManaCost
    {
        public int GetCost(EManaType type)
        {
            return !_costs.TryGetValue(type, out var mana) ? 0 : mana;
        }

        private Dictionary<EManaType, int> _costs = new Dictionary<EManaType, int>();
    }
}
