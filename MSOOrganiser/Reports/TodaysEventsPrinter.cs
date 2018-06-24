using MSOCore.Reports;
using MSOCore.Extensions;
using MSOOrganiser.UIUtilities;
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
    public class TodaysEventsPrinter : FlowDocumentGeneratorBase
    {
        public FlowDocument Print(DateTime date)
        {
            var rg = new TodaysEventsGenerator();
            var results = rg.GetEvents(date);

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
            cell.Blocks.Add(new Paragraph(new Run(results.OlympiadName)) { Margin = new Thickness(10), FontSize = 24, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
            cell.Blocks.Add(new Paragraph(new Run(date.DayOfWeek.ToString() + "'s Events")) { Margin = new Thickness(2), FontSize = 48, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
            trow.Cells.Add(cell);
            headerTable.RowGroups[0].Rows.Add(trow);

            doc.Blocks.Add(headerTable);

            /************ Main body *************/

            Table bodyTable = new Table() { CellSpacing = 0 };
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(60) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(560) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(100) });
            bodyTable.RowGroups.Add(new TableRowGroup());

            foreach (var initial in results.Sessions.Keys.OrderBy(x => x))
            {

                var row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(initial)) { Margin = new Thickness(8), FontSize = 36, FontWeight = FontWeights.Bold }));
                var bodyCell1 = new TableCell();
                var bodyCell2 = new TableCell();

                var lastName = "";
                foreach (var session in results.Sessions[initial])
                {
                    if (session.EventName != lastName)
                    {
                        bodyCell1.Blocks.Add(new Paragraph(new Run(session.EventName)) { Margin = new Thickness(2), FontSize = 18, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Left });
                        bodyCell2.Blocks.Add(new Paragraph(new Run(" ")) { Margin = new Thickness(2), FontSize = 18 });
                        lastName = session.EventName;
                    }
                    bodyCell1.Blocks.Add(new Paragraph(new Run("        -> " + session.Location)) { Margin = new Thickness(2), FontSize = 12 });
                    bodyCell2.Blocks.Add(new Paragraph(new Run(session.Start.ToStandardString() + " - " + session.End.ToStandardString())) { Margin = new Thickness(2), FontSize = 12 });
                }
                row.Cells.Add(bodyCell1);
                row.Cells.Add(bodyCell2);

                bodyTable.RowGroups[0].Rows.Add(row);

            }

            doc.Blocks.Add(bodyTable);

            return doc;
        }
    }
}
