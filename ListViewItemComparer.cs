using System;
using System.Collections;
using System.Windows.Forms;

namespace Mtg
{
    public static class ListViewUtil
    {
        public class Comparer : IComparer
        {
            public Comparer()
            {
                _column = 0;
                _order = SortOrder.Ascending;
            }

            public Comparer(int column, SortOrder order)
            {
                _column = column;
                _order = order;
            }

            public int Compare(object x, object y)
            {
                var returnVal = string.CompareOrdinal(
                    ((ListViewItem)x)?.SubItems[_column].Text,
                    ((ListViewItem)y)?.SubItems[_column].Text);

                if (_order == SortOrder.Descending)
                    returnVal *= -1;

                return returnVal;
            }

            private readonly int _column;
            private readonly SortOrder _order;
        }
    }
}
