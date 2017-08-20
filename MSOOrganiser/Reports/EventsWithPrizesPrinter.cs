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
    public class EventsWithPrizesPrinter : FlowDocumentGeneratorBase
    {
        public FlowDocument GenerateDocument()
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();

            var rg = new EventsWithPrizesReportGenerator();
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
            cell.Blocks.Add(new Paragraph(new Run("Events with Prizes")) { Margin = new Thickness(2), FontSize = 48, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
            trow.Cells.Add(cell);
            headerTable.RowGroups[0].Rows.Add(trow);

            doc.Blocks.Add(headerTable);

            /************ Main body *************/

            Table bodyTable = new Table() { CellSpacing = 0 };
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(80) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(270) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(70) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(70) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(70) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(70) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(70) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(70) });
            bodyTable.RowGroups.Add(new TableRowGroup());

            var row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run("")) { Margin = new Thickness(2), FontSize = 10 }));
            row.Cells.Add(new TableCell(new Paragraph(new Run("")) { Margin = new Thickness(2), FontSize = 10 }));
            row.Cells.Add(new TableCell(new Paragraph(new Run("1st")) { Margin = new Thickness(2), FontSize = 12 }));
            row.Cells.Add(new TableCell(new Paragraph(new Run("1st JNR")) { Margin = new Thickness(2), FontSize = 10 }));
            row.Cells.Add(new TableCell(new Paragraph(new Run("2nd")) { Margin = new Thickness(2), FontSize = 12 }));
            row.Cells.Add(new TableCell(new Paragraph(new Run("2nd JNR")) { Margin = new Thickness(2), FontSize = 10 }));
            row.Cells.Add(new TableCell(new Paragraph(new Run("3rd")) { Margin = new Thickness(2), FontSize = 12 }));
            row.Cells.Add(new TableCell(new Paragraph(new Run("3rd JNR")) { Margin = new Thickness(2), FontSize = 10 }));
            bodyTable.RowGroups[0].Rows.Add(row);

            foreach (var evt in results)
            {
                row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(evt.Code)) { Margin = new Thickness(2), FontSize = 16, FontWeight = FontWeights.Bold }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(evt.Name)) { Margin = new Thickness(2), FontSize = 16 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(evt.Gold)) { Margin = new Thickness(2), FontSize = 16 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(evt.JnrGold)) { Margin = new Thickness(2), FontSize = 16 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(evt.Silver)) { Margin = new Thickness(2), FontSize = 16 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(evt.JnrSilver)) { Margin = new Thickness(2), FontSize = 16 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(evt.Bronze)) { Margin = new Thickness(2), FontSize = 16 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(evt.JnrBronze)) { Margin = new Thickness(2), FontSize = 16 }));
                bodyTable.RowGroups[0].Rows.Add(row);
            }
            doc.Blocks.Add(bodyTable);

            return doc;
        }
    }
}
