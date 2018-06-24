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
    public class MedalTablePrinter : FlowDocumentGeneratorBase
    {
        public FlowDocument GenerateDocument()
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();

            var rg = new MedalTableReportGenerator();
            var results = rg.GetItemsForLatest();

            FlowDocument doc = this.StandardOneColumnDocument();

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
            cell.Blocks.Add(new Paragraph(new Run("Medal Table")) { Margin = new Thickness(2), FontSize = 48, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
            trow.Cells.Add(cell);
            headerTable.RowGroups[0].Rows.Add(trow);

            doc.Blocks.Add(headerTable);

            /************ Main body *************/

            Table bodyTable = new Table() { CellSpacing = 0 };
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(360) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(70) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(50) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(70) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(50) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(70) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(50) });
            bodyTable.RowGroups.Add(new TableRowGroup());

            var row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run("")) { Margin = new Thickness(2), FontSize = 10 }));
            row.Cells.Add(new TableCell(new Paragraph(new Run("Gold")) { Margin = new Thickness(2), FontSize = 12 }));
            row.Cells.Add(new TableCell(new Paragraph(new Run("")) { Margin = new Thickness(2), FontSize = 10 }));
            row.Cells.Add(new TableCell(new Paragraph(new Run("Silver")) { Margin = new Thickness(2), FontSize = 12 }));
            row.Cells.Add(new TableCell(new Paragraph(new Run("")) { Margin = new Thickness(2), FontSize = 10 }));
            row.Cells.Add(new TableCell(new Paragraph(new Run("Bronze")) { Margin = new Thickness(2), FontSize = 12 }));
            row.Cells.Add(new TableCell(new Paragraph(new Run("")) { Margin = new Thickness(2), FontSize = 10 }));
            bodyTable.RowGroups[0].Rows.Add(row);

            foreach (var country in results)
            {
                row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(country.Nationality)) { Margin = new Thickness(2), FontSize = 24, FontWeight = FontWeights.Bold }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(country.GoldsStr)) { Margin = new Thickness(2), FontSize = 24 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(country.JnrGoldsStr)) { Margin = new Thickness(8), FontSize = 12 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(country.SilversStr)) { Margin = new Thickness(2), FontSize = 24 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(country.JnrSilversStr)) { Margin = new Thickness(8), FontSize = 12 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(country.BronzesStr)) { Margin = new Thickness(2), FontSize = 24 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(country.JnrBronzesStr)) { Margin = new Thickness(8), FontSize = 12 }));
                bodyTable.RowGroups[0].Rows.Add(row);
            }
            doc.Blocks.Add(bodyTable);

            return doc;
        }
    }
}
