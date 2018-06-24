using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace MSOOrganiser.DocumentExtensions
{
    public class StdTableCell : TableCell
    {
        public StdTableCell(string text) : base()
        {
            this.Blocks.Add(new Paragraph(new Run(text)) { Margin = new Thickness(2), FontSize = 10 });                             
        }
    }

    public class StdRightTableCell : TableCell
    {
        public StdRightTableCell(string text)
            : base()
        {
            this.Blocks.Add(new Paragraph(new Run(text)) { Margin = new Thickness(2), FontSize = 10, TextAlignment = TextAlignment.Right });
        }
    }

    public class BoldTableCell : TableCell
    {
        public BoldTableCell(string text)
            : base()
        {
            this.Blocks.Add(new Paragraph(new Run(text)) { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold });
        }
    }

    public class BoldRightTableCell : TableCell
    {
        public BoldRightTableCell(string text)
            : base()
        {
            this.Blocks.Add(new Paragraph(new Run(text)) { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Right });
        }
    }
}
