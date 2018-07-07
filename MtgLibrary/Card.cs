using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Flurl;
using Flurl.Http;

namespace Mtg
{
    /// <summary>
    /// Represents a card in your library, or a card you're interested in.
    /// </summary>
    public class Card
    {
        private const string Endpoint = "https://api.scryfall.com";

        public Guid TypeId;
        public Guid ScryfallId;
        public string Title;
        public ManaCost ManaCost;
        public ECardType Type;
        public string Name;
        public string Text;
        public ERelease Release;
        public ScryfallCard ScryfallCard;
        public string ImageFilename;

        public override string ToString()
        {
            return ($"Title={Title}, Text={ScryfallCard?.oracle_text}");
        }

        public async Task<bool> PullInfo()
        {
            try
            {
                var result = await Endpoint.AppendPathSegment("cards/named")
                    .SetQueryParam("fuzzy", Title)
                    .GetJsonAsync<ScryfallCard>();
                Console.WriteLine($"Info for {Title}: {result}");
                ScryfallId = result.id;
                Title = result.name;
                ScryfallCard = result;

                ScryfallCard.oracle_text = ExpandText(ScryfallCard.oracle_text);

                var imageUri = result.image_uris["small"];
                var imagePath = result.id.ToString() + "-small.jpg";
                ImageFilename = imagePath;
                if (!File.Exists(imagePath))
                {
                    var bytes = await imageUri.GetBytesAsync();
                    File.WriteAllBytes(imagePath, bytes);
                    Console.WriteLine($"Wrote image for {result.name} to {ImageFilename}");
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error getting info on {Title}: {e.Message}");
                return false;
            }
        }

        string ExpandText(string text)
        {
            var replacements = new Dictionary<string, string>()
            {
                ["{T}"] = "Tap",
                ["{W}"] = "Plains",
                ["{B}"] = "Swamp",
                ["{R}"] = "Mountain",
                ["{U}"] = "Island",
                ["{G}"] = "Forest",
            };
            return replacements.Aggregate(text, (current, kv) => current.Replace(kv.Key, kv.Value));
        }
    }
}
