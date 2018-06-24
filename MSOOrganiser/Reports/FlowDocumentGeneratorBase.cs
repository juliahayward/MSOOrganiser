using System.Windows.Documents;
using System.Windows.Media;

namespace MSOOrganiser.Reports
{
    public class FlowDocumentGeneratorBase
    {
        public const int StandardOneColumnDocumentWidth = 770;
        public const int StandardTwoColumnDocumentWidth = 378;

        public FlowDocument StandardOneColumnDocument()
        {
            var doc = new FlowDocument();
            // 96ths of an inch - deliberately half width. This is the largest
            // we can go while keeping two columns, as there is some allowance for
            // column margins.
            doc.ColumnWidth = StandardOneColumnDocumentWidth;
            doc.FontFamily = new FontFamily("Verdana");
            return doc;
        }

        public FlowDocument StandardTwoColumnDocument()
        {
            var doc = new FlowDocument();
            // 96ths of an inch - deliberately half width. This is the largest
            // we can go while keeping two columns, as there is some allowance for
            // column margins.
            doc.ColumnWidth = 378; 
            doc.FontFamily = new FontFamily("Verdana");
            return doc;
        }
    }
}
