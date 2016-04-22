using MSOCore;
using MSOCore.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MSOOrganiser.Reports
{
    public class DonationPrinter
    {
        public void Print()
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();

            var rg = new DonationReportGenerator();
            var results = rg.GetItemsForLatest();

            PrintDialog dlg = new PrintDialog();
            if ((bool)dlg.ShowDialog().GetValueOrDefault())
            {
                FlowDocument doc = new FlowDocument();

                doc.ColumnWidth = 770; // 96ths of an inch
                doc.FontFamily = new FontFamily("Verdana");

                /* ********** Header *********** */

                Table headerTable = new Table() { CellSpacing = 0 };
                headerTable.Columns.Add(new TableColumn() { Width = new GridLength(220) });
                headerTable.Columns.Add(new TableColumn() { Width = new GridLength(550) });
                headerTable.RowGroups.Add(new TableRowGroup());

                Image image = new Image();
                image.Source = new BitmapImage(new Uri(@"pack://application:,,,/MSOOrganiser;component/Resources/Logo.png", UriKind.Absolute));

                var trow = new TableRow();
                trow.Cells.Add(new TableCell(new Paragraph(new InlineUIContainer(image)) { Margin = new Thickness(10), FontSize = 10, FontWeight = FontWeights.Bold }));
                var cell = new TableCell();
                cell.Blocks.Add(new Paragraph(new Run(currentOlympiad.FullTitle())) { Margin = new Thickness(10), FontSize = 24, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                cell.Blocks.Add(new Paragraph(new Run("Donations to the Olympiad")) { Margin = new Thickness(2), FontSize = 48, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                trow.Cells.Add(cell);
                headerTable.RowGroups[0].Rows.Add(trow);

                doc.Blocks.Add(headerTable);

                /************ Main body *************/

                Table bodyTable = new Table() { CellSpacing = 0 };
                bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(360) });
                bodyTable.RowGroups.Add(new TableRowGroup());

                var row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run("Received with Thanks:")) { Margin = new Thickness(2), FontSize = 24 }));
                bodyTable.RowGroups[0].Rows.Add(row);

                foreach (var donor in results)
                {
                    row = new TableRow();
                    row.Cells.Add(new TableCell(new Paragraph(new Run(donor.FullName)) { Margin = new Thickness(2), FontSize = 24, FontWeight = FontWeights.Bold }));
                    bodyTable.RowGroups[0].Rows.Add(row);
                }
                doc.Blocks.Add(bodyTable);


                DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                dlg.PrintDocument(paginator, "Donations");
            }
        }
    }
}
