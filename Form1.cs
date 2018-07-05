using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Flurl.Http;
using Mtg;
using WebEye.Controls.WinForms.WebCameraControl;

using Newtonsoft.Json;
using OpenCvSharp;

// Cards are: 63 x 88 mm
// 63/88 = 1.4 aspect ratio
//
// Card:    320 x 228 = 1.4 aspect ratio
// camera:  1280  x 720 = 1.8 aspect ratio

namespace MtgLib
{
    public partial class Form1 : Form
    {
        // you will need to have one of these yourself. It's just a text file that
        // contains your private Api key to GoogleVision.
        const string ApiKeyFileName = "GoogleVisonAPIKey.txt";

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

            LoadGoogleVisionApiKey();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _cards.Save();
        }

        private void LoadGoogleVisionApiKey()
        {
            _visionApiKey = System.IO.File.ReadAllText(ApiKeyFileName);
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

        private void openImageToolStripMenuItem_ClickAsync(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog();
            if (result != DialogResult.OK)
                return;

            ProcessFile(openFileDialog1.FileName);
        }

        private async void ProcessFile(string fileName)
        {
            Console.WriteLine($"Processing file {fileName}");
            var src = new Mat(fileName);//, ImreadModes.GrayScale);
            var width = (float)src.Width;
            var height = (float)src.Height;
            var aspect = width / height;
            width = 200;
            height = width/aspect;
            var dest = src.Resize(new Size(width, height));
            var bytes = dest.ToBytes(ext: ".jpg");
            Console.WriteLine($"Converted to {width}x{height}, {bytes.Length} bytes.jpg");
            var result = await Post(System.Convert.ToBase64String(bytes));
            var res = JsonConvert.DeserializeObject<VisionResponse>(result);
            ProcessResponse(res);
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

            var response = await baseUrl.PostJsonAsync(JsonConvert.DeserializeObject(text));
            return await response.Content.ReadAsStringAsync();
        }

        private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void writeToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void batchConvertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessFiles(Directory.GetFiles(@"C:\Users\christian\Pictures\MTG"));

            //using (var fbd = new FolderBrowserDialog())
            //{
            //    fbd.SelectedPath = _imageDir;
            //    var result = fbd.ShowDialog();
            //    if (result != DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath))
            //        return;

            //    ProcessFiles(Directory.GetFiles(fbd.SelectedPath));
            //}
        }

        void ProcessFiles(IEnumerable<string> fileNames)
        {
            foreach (var file in fileNames)
            {
                ProcessFile(file);
            }
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _cards.Clear();
        }

        private void allToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var card in _cards.Cards)
            {
                Console.WriteLine($"Title={card.Title}, scanned={card.ScannedTitle}, text={card.Text}");
            }
        }

        private readonly CardLibrary _cards = new CardLibrary();
        private string _visionApiKey;

        private void exportToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                return;
            _cards.Export(saveFileDialog1.FileName);
        }
    }
}
