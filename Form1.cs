// This file, like many WinForms-based apps, has become a bit of a mess.
// I did have a stab at using custom User Controls for the different TabPages,
// but it became too unwieldly.

// At least, all the non-UI library is in the separate 'MtgLibrary.dll' assembly.

// Use hard-coded path to images for faster development iteration
//#define HARD_CODED_IMAGE_PATH

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebEye.Controls.WinForms.WebCameraControl;

namespace Mtg
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Where to look for recently scanned images for cards.
        /// </summary>
        private readonly string _imageDir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "MTG";

        public Form1()
        {
            InitializeComponent();

            Text = "MtG Card Library";

            openFileDialog1.InitialDirectory = _imageDir;

            var cameras = new List<WebCameraId>(webCameraControl1.GetVideoCaptureDevices());
            webCameraControl1.StartCapture(cameras[0]);

            tabControl1.Selected += TabControl1OnSelected;

            LoadCards();
        }

        // invoked when tab control changes current tab.
        // TODO: refresh data
        private void TabControl1OnSelected(object sender, TabControlEventArgs tabControlEventArgs)
        {
            //Log($"Selected {tabControlEventArgs.TabPageIndex}");
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

        async void LoadCards()
        {
            await LoadCardsAsync();
        }

        private async Task LoadCardsAsync()
        {
            var num = await _cards.Load();
            Log($"Read {num} cards from library");

            foreach (var card in _cards.Cards)
            {
                // Log($"Adding {card.Title}");
                listViewLibrary.Items.Add(new ListViewItem
                {
                    Text = card.Title,
                    Tag = card.TypeId,
                    Name = card.Title
                });
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _cards.Save();
        }

        private async void openImageToolStripMenuItem_ClickAsync(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog();
            if (result != DialogResult.OK)
                return;

            await _cards.ProcessFileVision(openFileDialog1.FileName);
        }

        private async void batchConvertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log("Batch Convert Started");
#if HARD_CODED_IMAGE_PATH
            await ProcessFiles(Directory.GetFiles(@"C:\Users\christian\Pictures\MTG\BlueWhiteDeck"));
#else
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.SelectedPath = _imageDir;
                var result = fbd.ShowDialog();
                if (result != DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    return;

                await ProcessFiles(Directory.GetFiles(fbd.SelectedPath));
            }
#endif
            await _cards.PullInfo();
            Log("Batch Convert End");
        }

        private async Task ProcessFiles(IEnumerable<string> fileNames)
        {
            await Task.WhenAll(fileNames.Select(_cards.ProcessFileVision).ToArray());

            // obviously TODO: not hard-code these
            _cards.Save(@"c:\users\christian\desktop\latest.json");
            _cards.Export(@"c:\users\christian\desktop\latest.tappedout");

            Console.WriteLine($"Batch completed, total of {_cards.Cards.Count()} cards");
            var pulled = await _cards.PullInfo();
            Console.WriteLine($"Pulled info={pulled}");
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _cards.Clear();
            listViewLibrary.Items.Clear();
        }

        private void allToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var card in _cards.Cards)
                Log(card.ToString());
        }

        private void exportToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                return;
            _cards.Export(saveFileDialog1.FileName);
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

        // Get the currently selected card from the Library tab, if any
        private Card SelectedCard
        {
            get
            {
                var items = listViewLibrary.SelectedIndices;
                if (items.Count != 1)
                    return null;
                var item = listViewLibrary.Items[items[0]];
                var id = (Guid) item.Tag;
                if (id != Guid.Empty)
                    return _cards.Get(id);
                Log($"No id for card {item.Name}");
                return null;
            }
        }

        private void listViewLibrary_SelectedIndexChanged(object sender, EventArgs e)
        {
            var card = SelectedCard;
            textBoxCardInfoName.Text = card.Title;
            textBoxCardText.Text = card.ScryfallCard.oracle_text;
            // TODO: Not hard-code USD -> AUD conversion rate
            textBoxCardCost.Text = "$" + (float.Parse(card.ScryfallCard.usd) * 1.35f).ToString("F1");

            if (!string.IsNullOrEmpty(card.ImageFilename))
                cardPicture.Image = Image.FromFile(card.ImageFilename);
        }

        private void cardPicture_DoubleClick(object sender, EventArgs e)
        {
            // TODO: Show all info on card with high-res image
            Log("Double click on ${SelectedCard?.Title}");
        }

        private void Log(string text)
        {
            Console.WriteLine(text);
        }

        private readonly CardLibrary _cards = new CardLibrary();
    }
}
