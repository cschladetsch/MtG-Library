﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Flurl;
using Flurl.Http;
using static Mtg.Console;

namespace Mtg
{
    /// <summary>
    /// Represents a card in your library, or a card you're interested in.
    /// </summary>
    public class Card
    {
        public Guid TypeId;
        public Guid ScryfallId;

        public string Title;
        public ManaCost ManaCost;
        public ECardType Type;
        public string Name;
        public string Text => ScryfallCard?.oracle_text;
        public ERelease Release;
        public ScryfallCard ScryfallCard;
        public string ImageFilename;

        public override string ToString()
        {
            return ($"{Title}, {Text}");
        }

        public async Task<bool> PullInfo()
        {
            try
            {
                var result = await Endpoint.AppendPathSegment("cards/named")
                    .SetQueryParam("fuzzy", Title)
                    .GetJsonAsync<ScryfallCard>();
                if (result == null)
                {
                    Log($"Couldn't find infor for {Title}");
                    return false;
                }
                Log($"Info for {Title}: {result.oracle_text}");
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
                    Log($"Wrote image for {result.name} to {ImageFilename}");
                }

                return true;
            }
            catch (Exception e)
            {
                Error($"Error getting info on {Title}: {e.Message}");
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

        private const string Endpoint = "https://api.scryfall.com";
    }
}
