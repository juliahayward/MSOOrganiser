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
using MSOCore.Extensions;

namespace MSOOrganiser.Reports
{
    public class ContestantResultsPrinter : FlowDocumentGeneratorBase
    {
        public FlowDocument Print(ContestantPanelVm contestant)
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();

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
            cell.Blocks.Add(new Paragraph(new Run("Results for " + contestant.Name)) { Margin = new Thickness(2), FontSize = 32, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
            trow.Cells.Add(cell);
            headerTable.RowGroups[0].Rows.Add(trow);

            doc.Blocks.Add(headerTable);

            /************ Main body *************/

            Table bodyTable = new Table() { CellSpacing = 0 };
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(350) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(120) }); 
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(80) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(100) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(100) });
            bodyTable.RowGroups.Add(new TableRowGroup());

            var row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run("Name")) { Margin = new Thickness(2), FontSize = 10 }));
            row.Cells.Add(new TableCell(new Paragraph(new Run("Date")) { Margin = new Thickness(2), FontSize = 10 }));
            row.Cells.Add(new TableCell(new Paragraph(new Run("Rank")) { Margin = new Thickness(2), FontSize = 12 }));
            row.Cells.Add(new TableCell(new Paragraph(new Run("Medal")) { Margin = new Thickness(2), FontSize = 10 }));
            row.Cells.Add(new TableCell(new Paragraph(new Run("Pentamind Pts")) { Margin = new Thickness(2), FontSize = 12 }));
            bodyTable.RowGroups[0].Rows.Add(row);

            foreach (var evt in contestant.Events.OrderBy(x => x.Date))
            {
                row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(evt.EventName)) { Margin = new Thickness(2), FontSize = 12, TextAlignment = TextAlignment.Left }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(evt.FormattedDate)) { Margin = new Thickness(2), FontSize = 12 }));
                var rankString = (evt.Rank > 0) ? evt.Rank.ToString().Ordinal() : "";
                row.Cells.Add(new TableCell(new Paragraph(new Run(rankString)) { Margin = new Thickness(2), FontSize = 12 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(evt.Medal + " " + evt.JuniorMedal)) { Margin = new Thickness(2), FontSize = 12 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(evt.Penta)) { Margin = new Thickness(2), FontSize = 12 }));
                bodyTable.RowGroups[0].Rows.Add(row);
            }
            doc.Blocks.Add(bodyTable);

            return doc;
        }
    }
}
