// (C) 2022 christian@schadetsch.com

using System;
using System.Windows.Forms;

namespace Mtg
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
