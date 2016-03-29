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
    class PokerStandingsPrinter
    {
        public void Print()
        {
            var rg = new PokerStandingsGenerator();
            var results = rg.GetStandings();

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
                cell.Blocks.Add(new Paragraph(new Run(results.OlympiadTitle)) { Margin = new Thickness(10), FontSize = 24, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                cell.Blocks.Add(new Paragraph(new Run("Poker WC Standings")) { Margin = new Thickness(2), FontSize = 48, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                trow.Cells.Add(cell);
                headerTable.RowGroups[0].Rows.Add(trow);

                doc.Blocks.Add(headerTable);

                /************ Main body *************/

                Table bodyTable = new Table() { CellSpacing = 0 };
                bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(20) });
                bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(20) });
                bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(200) });
                bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(75) });
                bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(90) });
                bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(90) });
                bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(90) });
                bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(90) });
                bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(90) });
                bodyTable.RowGroups.Add(new TableRowGroup());

                var row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run("")) { Margin = new Thickness(2), FontSize = 12 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run("")) { Margin = new Thickness(2), FontSize = 12 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run("Name")) { Margin = new Thickness(2), FontSize = 12 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run("Total")) { Margin = new Thickness(2), FontSize = 12 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run("")) { Margin = new Thickness(2), FontSize = 12 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run("")) { Margin = new Thickness(2), FontSize = 12 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run("")) { Margin = new Thickness(2), FontSize = 12 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run("")) { Margin = new Thickness(2), FontSize = 12 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run("")) { Margin = new Thickness(2), FontSize = 12 }));
                bodyTable.RowGroups[0].Rows.Add(row);

                var rank = 0;
                foreach (var s in results.Standings.Take(40))
                {
                    rank++;
                    row = new TableRow();
                    row.Cells.Add(new TableCell(new Paragraph(new Run(rank.ToString())) { Margin = new Thickness(2), FontSize = 12, }));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(s.FemaleFlag)) { Margin = new Thickness(2), FontSize = 12, }));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(s.Name)) { Margin = new Thickness(2), FontSize = 12 }));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(s.TotalScoreStr)) { Margin = new Thickness(2), FontSize = 12 }));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(s.ScoreStr(0))) { Margin = new Thickness(2), FontSize = 10 }));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(s.ScoreStr(1))) { Margin = new Thickness(2), FontSize = 10 }));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(s.ScoreStr(2))) { Margin = new Thickness(2), FontSize = 10 }));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(s.ScoreStr(3))) { Margin = new Thickness(2), FontSize = 10 }));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(s.ScoreStr(4))) { Margin = new Thickness(2), FontSize = 10 }));
                    bodyTable.RowGroups[0].Rows.Add(row);
                }

                row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run("Top 40 listed.")) { Margin = new Thickness(2), FontSize = 12, TextAlignment = TextAlignment.Center }) { ColumnSpan = 8 });
                bodyTable.RowGroups[0].Rows.Add(row);

                doc.Blocks.Add(bodyTable);

                DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                dlg.PrintDocument(paginator, "Poker World Championship Standings");
            }
        }
    }
}
