using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;

namespace MSOOrganiser.DocumentExtensions
{
    public class BorderedParagraph : Paragraph
    {
        public BorderedParagraph()
            : base()
        {
            BorderBrush = Brushes.Black;
            BorderThickness = new System.Windows.Thickness(1);
        }

        public BorderedParagraph(Inline inlineItem)
            : base(inlineItem)
        {
            BorderBrush = Brushes.Black;
            BorderThickness = new System.Windows.Thickness(1);
        }
    }
}
