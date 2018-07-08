using System.Drawing;
using System.Windows.Forms;

namespace Mtg
{
    public static class Console
    {
        public static ListView ListView;

        public static void Write(string text, Color color)
        {
            var item = new ListViewItem
            {
                ForeColor = color,
                Text = text
            };
            ListView.Items.Add(item);
            ListView.Items[ListView.Items.Count -1].EnsureVisible();

            System.Console.WriteLine($"{color}: {text}");
        }

        public static void Log(string text)
        {
            Write(text, Color.Black);
        }

        public static void Warn(string text)
        {
            Write(text, Color.Magenta);
        }

        public static void Error(string text)
        {
            Write(text, Color.Red);
        }
    }
}
