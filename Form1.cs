using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using WebEye.Controls.WinForms.WebCameraControl;

using Newtonsoft.Json;

// Cards are: 63 x 88 mm
// 63/88 = 1.4 aspect ratio
//
// Card:    320 x 228 = 1.4 aspect ratio
// camera:  1280  x 720 = 1.8 aspect ratio

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        // you will need to have one of these yourself. It's just a text file that
        // contains your private Api key to GoogleVision.
        const string ApiKeyFileName = "GoogleVisonAPIKey.txt";

        public Form1()
        {
            InitializeComponent();
            Text = "MtG Card Library";

            List<WebCameraId> cameras = new List<WebCameraId>(webCameraControl1.GetVideoCaptureDevices());
            webCameraControl1.StartCapture(cameras[0]);

            openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "MTG";

            LoadGoogleVisionApiKey();
        }

        private void LoadGoogleVisionApiKey()
        {
            _visionApiKey = System.IO.File.ReadAllText(ApiKeyFileName);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        class VisionResponse
        {
            public class FullTextAnnotation
            {
                public List<object> pages;
                public string text;
            }

            public class Response
            {
                public List<object> textAnnotations;
                public FullTextAnnotation FullTextAnnotation;
            }

            public List<Response> responses;
        }

        private async Task AnalizeFileAsync(string fileName)
        {
            var bytes = System.IO.File.ReadAllBytes(fileName);
            var result = await PostAsync(System.Convert.ToBase64String(bytes));
            var res = JsonConvert.DeserializeObject<VisionResponse>(result);
            ProcessResponse(res);
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
        }

        string TrimMana(string title)
        {
            return title.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ');
        }

        async Task<string> PostAsync(string base64Content)
        {
            var json = @"
            {
             ""requests"": [
              {
                ""image"": {
                    ""source"": {
                    },
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

            using (var client = new HttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(baseUrl, content);
                Console.WriteLine($"Sending {content}");
                return await response.Content.ReadAsStringAsync();
            }
        }

        private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void openImageToolStripMenuItem_ClickAsync(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog();
            if (result != DialogResult.OK)
                return;

            ProcessFileAsync(openFileDialog1.FileName);
        }

        private async void ProcessFileAsync(string fileName)
        {
            await AnalizeFileAsync(fileName);
        }

        private void writeToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        // you will need to have one of these yourself
        private string _visionApiKey;
    }
}
