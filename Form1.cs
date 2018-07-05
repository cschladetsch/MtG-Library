using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Flurl.Http;
using WebEye.Controls.WinForms.WebCameraControl;

using Newtonsoft.Json;
using OpenCvSharp;

// Cards are: 63 x 88 mm
// 63/88 = 1.4 aspect ratio
//
// Card:    320 x 228 = 1.4 aspect ratio
// camera:  1280  x 720 = 1.8 aspect ratio

namespace Mtg
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// The API Key to use to acccess Google Vision API.
        ///
        /// You will need to have one of these yourself.
        /// ApiKeyFileName is just the name of a text file that
        /// contains your private Api key to GoogleVision.
        /// </summary>
        const string ApiKeyFileName = "GoogleVisonAPIKey.txt";

        /// <summary>
        /// The width in pixels to scale to before sending to Google Vision.
        /// Aspect ratio is reserved from original.
        ///
        /// A bit of trial and error shows that this number is a good compromise
        /// between bandwidth use and OCR precision for MtG cards.
        /// </summary>
        private const int SentImageWidth = 350;

        /// <summary>
        /// Where to look for recently scanned images for cards.
        ///
        /// Note that by default* these images will be deleted after they are processed.
        ///
        /// *This doesn't happen yet.
        /// </summary>
        private readonly string _imageDir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "MTG";

        public Form1()
        {
            InitializeComponent();
            Text = "MtG Card Library";

            var cameras = new List<WebCameraId>(webCameraControl1.GetVideoCaptureDevices());
            webCameraControl1.StartCapture(cameras[0]);

            openFileDialog1.InitialDirectory = _imageDir;

            var num = _cards.Load();
            Console.WriteLine($"Read {num} cards from library");

            _visionApiKey = File.ReadAllText(ApiKeyFileName);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _cards.Save();
        }

        private void ProcessResponse(VisionResponse res)
        {
            if (res == null)
            {
                MessageBox.Show("Process Failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (res.responses.Count == 0)
            {
                MessageBox.Show("No text found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var fullText = res.responses[0].FullTextAnnotation;
            if (fullText == null)
            {
                Console.WriteLine("WARN: Empty vision response");
                return;
            }

            var text = res.responses[0].FullTextAnnotation.text;
            var split = text.Split('\n');
            _cardTitleText.Text = TrimMana(split[0]);

            _cards.Add(res);
            // TODO: fill other text fields with date from response
        }

        private static string TrimMana(string title)
        {
            return title.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ');
        }

        private async void openImageToolStripMenuItem_ClickAsync(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog();
            if (result != DialogResult.OK)
                return;

            await ProcessFile(openFileDialog1.FileName);
        }

        private async Task ProcessFile(string fileName)
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
            var response = await baseUrl.WithTimeout(TimeSpan.FromMinutes(5)).PostJsonAsync(JsonConvert.DeserializeObject(text));
            return await response.Content.ReadAsStringAsync();
        }

        private void Log(string text)
        {
            Console.WriteLine(text);
        }

        private async void batchConvertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log("Batch Convert Started");
            //Task.Run(() => ProcessFiles(Directory.GetFiles(@"C:\Users\christian\Pictures\MTG")));
            await ProcessFiles(Directory.GetFiles(@"C:\Users\christian\Pictures\MTG"));
            Log("Batch Convert End");

            //using (var fbd = new FolderBrowserDialog())
            //{
            //    fbd.SelectedPath = _imageDir;
            //    var result = fbd.ShowDialog();
            //    if (result != DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath))
            //        return;

            //    ProcessFiles(Directory.GetFiles(fbd.SelectedPath));
            //}
        }

        private async Task ProcessFiles(IEnumerable<string> fileNames)
        {
            await Task.WhenAll(fileNames.Select(ProcessFile).ToArray());

            _cards.Save(@"c:\users\christian\desktop\latest.json");
            _cards.Export(@"c:\users\christian\desktop\latest.tappedout");

            Console.WriteLine($"Batch completed, total of {_cards.Cards.Count()} cards");
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _cards.Clear();
        }

        private void allToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var card in _cards.Cards)
            {
                Console.WriteLine(card);
            }
        }

        private void exportToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                return;
            _cards.Export(saveFileDialog1.FileName);
        }

        private readonly CardLibrary _cards = new CardLibrary();
        private string _visionApiKey;
    }
}
