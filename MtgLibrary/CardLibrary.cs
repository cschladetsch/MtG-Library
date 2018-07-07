using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Flurl;
using Flurl.Http;

using Newtonsoft.Json;
using OpenCvSharp;

namespace Mtg
{
    /// <summary>
    /// A collection of cards
    /// </summary>
    public class CardLibrary
    {
        /// <summary>
        /// The API Key to use to acccess Google Vision API.
        ///
        /// You will need to have one of these yourself.
        /// ApiKeyFileName is just the name of a text file that
        /// contains your private Api key to GoogleVision.
        /// </summary>
        private const string ApiKeyFileName = "GoogleVisonAPIKey.txt";

        private string Endpoint = "https://api.scryfall.com";

        /// <summary>
        /// The width in pixels to scale to before sending to Google Vision.
        /// Aspect ratio is reserved from original.
        ///
        /// A bit of trial and error shows that this number is a good compromise
        /// between bandwidth use and OCR precision for MtG cards.
        /// </summary>
        private const int SentImageWidth = 350;

        public const string FileName = "Cards.json";
        public const string AllCardsFileName = "AllCards.json";

        public IEnumerable<Card> Cards
        {
            get
            {
                foreach (var kv in _library.Counts)
                {
                    for (var n = 0; n < kv.Value; ++n)
                    {
                        yield return _library.Cards[kv.Key];
                    }
                }
            }
        }

        public CardLibrary()
        {
            _visionApiKey = File.ReadAllText(ApiKeyFileName);
        }

        public void Clear()
        {
            _library.Clear();
        }

        public Card Get(Guid id)
        {
            return _library.Find(id);
        }

        public async Task ProcessFileVision(string fileName)
        {
            if (fileName.Contains("-short"))
                return;

            Console.WriteLine($"Processing file {fileName}");
            var src = new Mat(fileName);
            var width = (float)src.Width;
            var height = (float)src.Height;
            var aspect = width / height;
            width = SentImageWidth;
            height = width/aspect;
            var dest = src.Resize(new Size(width, height));
            var bytes = dest.ToBytes(ext: ".jpg");
            Console.WriteLine($"Converted to {width}x{height}, {bytes.Length} bytes.jpg");
            dest.ImWrite(fileName + "-short.jpg");  // just for comparison with original
            var result = await Post(Convert.ToBase64String(bytes));
            ProcessResponse(JsonConvert.DeserializeObject<VisionResponse>(result));
        }

        private void ProcessResponse(VisionResponse res)
        {
            if (res == null)
            {
                //MessageBox.Show("Process Failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (res.responses.Count == 0)
            {
                //MessageBox.Show("No text found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var fullText = res.responses[0].FullTextAnnotation;
            if (fullText == null)
            {
                Console.WriteLine("WARN: Empty vision response");
                return;
            }

            //var text = res.responses[0].FullTextAnnotation.text;
            //var split = text.Split('\n');
            //_cardTitleText.Text = TrimMana(split[0]);

            Add(res);
        }

        async Task<string> Post(string base64Content)
        {
            // hacky, but it works
            var text = @"
            {
             ""requests"": [
              {
                ""image"": {
                  ""content"": ""$CONTENT""
                },
                ""features"": [
                {
                  ""type"": ""TEXT_DETECTION""
                }
               ]
              }
             ]
            }
            ".Replace("$CONTENT", base64Content);
            var baseUrl  = "https://vision.googleapis.com/v1/images:annotate?fields=responses&key=" + _visionApiKey;

            // give a large timeout cos we want to add like, 60 cards at a time
            var response = await baseUrl.WithTimeout(TimeSpan.FromMinutes(5))
                .PostJsonAsync(JsonConvert.DeserializeObject(text));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<int> Load(string fileName = FileName)
        {
            if (!File.Exists(fileName))
            {
                Console.WriteLine("Starting new library");
                _library = new PersistentLibrary();
            }

            if (!File.Exists(AllCardsFileName))
            {
                Console.WriteLine("Do not have list of all cards; fetching");
                await GetAllCardNames();
            }

            _allCardNames = JsonConvert.DeserializeObject<AllCardNames>(File.ReadAllText(AllCardsFileName));
            _library = JsonConvert.DeserializeObject<PersistentLibrary>(File.ReadAllText(fileName));
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
            var input = TrimMana(split[0]);
            var title = ClosestStringMatch.Find(input, _allCardNames.data);

            Console.WriteLine($"Found {title} as best match for {input}");

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

        class AllCardNames
        {
            public string @object;
            public string uri;
            public int total_values;
            public List<string> data;
        }

        public IEnumerable<string> AllExisitingCardnames => _allCardNames?.data;

        public async Task<bool> GetAllCardNames()
        {
            try
            {
                _allCardNames = await Endpoint.AppendPathSegment("catalog/card-names")
                    .SetQueryParam("format", "json")
                    .GetJsonAsync<AllCardNames>();
                File.WriteAllText(AllCardsFileName, JsonConvert.SerializeObject(_allCardNames));
                Console.WriteLine($"Fetched {_allCardNames.data.Count} card names");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                return false;
            }
        }

        public async Task<bool> PullInfo()
        {
            const int minIntervalMillis = 150;
            foreach (var card in _library.Cards.Values)
            {
                var now = DateTime.Now;
                var delta = now - _lastQuery;
                if (delta.TotalMilliseconds < minIntervalMillis)
                    await Task.Delay(TimeSpan.FromMilliseconds(minIntervalMillis));
                _lastQuery = now;

                if (!await card.PullInfo())
                    Console.WriteLine($"Couldn't find info on {card.Title}");
            }

            return true;
        }

        //private Dictionary<Guid, System.Drawing.
        private PersistentLibrary _library;
        private List<Deck> _decks;
        private AllCardNames _allCardNames;
        private DateTime _lastQuery;
        private readonly string _visionApiKey;
    }
}
