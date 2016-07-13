using MSOOrganiser.UIUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MSOOrganiser.Reports
{
    public class FlowDocumentPrinter
    {
        public void PrintFlowDocument(Func<FlowDocument> documentProvider)
        {
            PrintDialog dlg = new PrintDialog();
            if ((bool)dlg.ShowDialog().GetValueOrDefault())
            {
                using (new SpinnyCursor())
                {
                    var doc = documentProvider();

                    DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                    dlg.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
                    dlg.PrintDocument(paginator, "MSO Report");
                }
            }
        }

        public void PrintFlowDocuments(Func<IEnumerable<FlowDocument>> documentProvider)
        {
            PrintDialog dlg = new PrintDialog();
            if ((bool)dlg.ShowDialog().GetValueOrDefault())
            {
                using (new SpinnyCursor())
                {
                    foreach (var doc in documentProvider())
                    {
                        DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                        dlg.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
                        dlg.PrintDocument(paginator, "MSO Report");
                    }
                }
            }
        }
    }
}
