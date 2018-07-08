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

        private WebCameraId _webCamera;

        public Form1()
        {
            InitializeComponent();

            Text = "MtG Card Library";

            openFileDialog1.InitialDirectory = _imageDir;

            _webCamera = new List<WebCameraId>(webCameraControl1.GetVideoCaptureDevices())[0];

            tabControl1.Selected += TabControl1OnSelected;

            RefreshLibraryView();
        }

        private void RefreshLibraryView()
        {
            LoadCards();
            SortByPrice();
            SelectTop();
        }

        private void SelectTop()
        {
            var highest = 0.0f;
            var items = listViewLibrary.Items;
            foreach (ListViewItem item in items)
            {
                var cost = float.Parse(_cards.Get((Guid) item.Tag).ScryfallCard.usd);
                if (cost > highest)
                {
                    item.Selected = true;
                    highest = cost;
                }
            }
        }

        private void SortByPrice()
        {
            // quick hack to sort by card cost
            listViewLibrary_ColumnClick(this, new ColumnClickEventArgs(1));
            listViewLibrary_ColumnClick(this, new ColumnClickEventArgs(1));
        }

        // invoked when tab control changes current tab.
        private void TabControl1OnSelected(object sender, TabControlEventArgs tabControlEventArgs)
        {
            Log($"Selected {tabControlEventArgs.TabPageIndex}");
            if (webCameraControl1.IsCapturing)
                webCameraControl1.StopCapture();
            switch (tabControlEventArgs.TabPageIndex)
            {
                // my library
                case 0:
                    RefreshLibraryView();
                    break;
                // my decks
                case 1:
                    break;
                // webcam
                case 2:
                    webCameraControl1.StartCapture(_webCamera);
                    break;
                // console
                case 3:
                    break;
                // all cards
                case 4:
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
            listViewLibrary.Items.Clear();
            Log($"Read {num} cards from library");

            foreach (var card in _cards.Cards)
            {
                // Log($"Adding {card.Title}");
                var item = new ListViewItem(card.Title);
                item.SubItems.Add(card.ScryfallCard.AudText);
                item.SubItems.Add(card.ScryfallCard.oracle_text);
                item.Tag = card.TypeId;

                listViewLibrary.Items.Add(item);
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
                if (item?.Tag == null)
                {
                    Log($"Bad entry {item}");
                    return null;
                }
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
            if (card == null)
                return;
            textBoxCardInfoName.Text = card.Title;
            textBoxCardText.Text = card.ScryfallCard.oracle_text;
            // TODO: Not hard-code USD -> AUD conversion rate
            //textBoxCardCost.Text = "$" + (float.Parse(card.ScryfallCard.usd) * 1.35f).ToString("F1");

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

        private void listViewLibrary_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            var list = listViewLibrary;
            if (e.Column != _sortColumn)
            {
                _sortColumn = e.Column;
                list.Sorting = SortOrder.Ascending;
            }
            else
            {
                list.Sorting = list.Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }

            list.Sort();
            list.ListViewItemSorter = new ListViewUtil.Comparer(e.Column, list.Sorting);
        }

        private void listViewLibrary_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            var b = e.Bounds;
            b.Height = (int) (b.Height * 0.6f);  // doing things because otherwise the title columns are too tall??!
            var color = Brushes.LightGray;//column == e.ColumnIndex ? Brushes.LightGray : Brushes.Gray;
            e.Graphics.FillRectangle(color, b);
            e.DrawText();
        }

        private void listViewLibrary_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private readonly CardLibrary _cards = new CardLibrary();
        private int _sortColumn = -1;

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox1().Show();
        }

        // take a snapshot of card from webcam. only really need to see the title.
        private async void button3_Click(object sender, EventArgs e)
        {
            var tmp = Path.GetTempFileName();
            webCameraControl1.GetCurrentImage().Save(tmp);
            await _cards.ProcessFileVision(tmp);
            File.Delete(tmp);
        }
    }
}
