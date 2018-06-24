using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;

namespace MSOOrganiser.DocumentExtensions
{
    public class BorderedTableCell : TableCell
    {
        public BorderedTableCell()
            : base()
        {
            BorderBrush = Brushes.Black;
            BorderThickness = new System.Windows.Thickness(1);
        }

        public BorderedTableCell(Block blockItem)
            : base(blockItem)
        {
            BorderBrush = Brushes.Black;
            BorderThickness = new System.Windows.Thickness(1);
        }
    }
}
