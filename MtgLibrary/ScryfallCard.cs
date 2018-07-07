using System;
using System.Collections.Generic;
// ReSharper disable All

#pragma warning disable 649

namespace Mtg
{
    /// <summary>
    /// Information about a card pulled from https://scryfall.com
    /// </summary>
    public class ScryfallCard
    {
        public string @object;
        public Guid id;
        public Guid oracle_id;
        public List<int> multiverse_ids;
        public int mtgo_id;
        public int mtgo_foil_id;
        public string name;
        public string lang;
        public string uri;
        public string scryfall_uri;
        public string layout;
        public bool highres_image;
        public Dictionary<string, string> image_uris;
        public string mana_cost;
        public float cmc;
        public string type_line;
        public string oracle_text;
        public List<string> colors;
        public List<string> color_identity;
        public Dictionary<string, string> legalities;
        public bool reserved;
        public bool foil;
        public bool nonfoil;
        public bool oversized;
        public bool reprint;
        public string set;
        public string set_name;
        public string set_search_uri;
        public string scryfall_set_uri;
        public string rulings_uri;
        public string prints_search_uri;
        public string collector_number;
        public bool digital;
        public string rarity;
        public Guid illustration_id;
        public string artist;
        public string frame;
        public bool full_art;
        public string border_color;
        public bool timeshifted;
        public bool colorshifted;
        public bool futureshifted;
        public int edhrec_rank;
        public string usd;  // cost
        public string tix;
        public string eur;
        public Dictionary<string, string> related_uris;
        public Dictionary<string, string> purchase_uris;

        public string AudText => (float.Parse(usd) * 1.33).ToString("C");

        public override string ToString()
        {
            return $"{name}";
        }
    }
}
