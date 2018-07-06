using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using WebEye.Controls.WinForms.WebCameraControl;

// Cards are: 63 x 88 mm
// 63/88 = 1.4 aspect ratio
//
// Card:        320 x 228 = 1.4 aspect ratio
// phone cam:   1280 x 720 = 1.8 aspect ratio
// webcam:      640 x 480 = 1.3

namespace Mtg
{
    public partial class Form1 : Form
    {
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

            openFileDialog1.InitialDirectory = _imageDir;

            var cameras = new List<WebCameraId>(webCameraControl1.GetVideoCaptureDevices());
            //webCameraControl1.StartCapture(cameras[0]);

            tabControl1.Selected += TabControl1OnSelected;

            LoadCards();
        }

        private void TabControl1OnSelected(object sender, TabControlEventArgs tabControlEventArgs)
        {
            Console.WriteLine($"Selected {tabControlEventArgs.TabPageIndex}");
            switch (tabControlEventArgs.TabPageIndex)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
            }
        }

        private async Task LoadCards()
        {
            var num = await _cards.Load();
            Console.WriteLine($"Read {num} cards from library");
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _cards.Save();
        }

        private static string TrimMana(string title)
        {
            return title.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ');
        }

        private async void openImageToolStripMenuItem_ClickAsync(object sender, EventArgs e)
        {
            //var result = openFileDialog1.ShowDialog();
            //if (result != DialogResult.OK)
            //    return;

            //await ProcessFile(openFileDialog1.FileName);
        }

        private void Log(string text)
        {
            Console.WriteLine(text);
        }

        private async void batchConvertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log("Batch Convert Started");
            await ProcessFiles(Directory.GetFiles(@"C:\Users\christian\Pictures\MTG\BlueWhiteDeck"));
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
            await Task.WhenAll(fileNames.Select(_cards.ProcessFileVision).ToArray());

            _cards.Save(@"c:\users\christian\desktop\latest.json");
            _cards.Export(@"c:\users\christian\desktop\latest.tappedout");

            Console.WriteLine($"Batch completed, total of {_cards.Cards.Count()} cards");

            //await _cards.GetAllCardInfos();
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

        private void inventoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private async void getLatestCardListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (await _cards.GetAllCardNames())
                MessageBox.Show($"Retrieved {_cards.AllExisitingCardnames.Count()} card names");
            else
                MessageBox.Show("Failed to get card names", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private async void updateCardDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await _cards.PullInfo();
        }

        private readonly Mtg.CardLibrary _cards = new CardLibrary();
    }
}
