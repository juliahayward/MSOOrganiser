using MSOCore.Reports;
using MSOOrganiser.DocumentExtensions;
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
using System.Windows.Shapes;

namespace MSOOrganiser.Reports
{
    public class GamePlanPrinter
    {
        public void Print()
        {
            var rg = new GamePlanReportGenerator();
            var results = rg.GetItemsForLatest();
            var todaysDate = DateTime.Now.ToString("dd MMM yyyy");

            PrintDialog dlg = new PrintDialog();
            if ((bool)dlg.ShowDialog().GetValueOrDefault())
            {
                FlowDocument doc = new FlowDocument();

                doc.ColumnWidth = 770; // 96ths of an inch
                doc.FontFamily = new FontFamily("Verdana");

                // Top page

                Table headerTable = new Table() { CellSpacing = 0 };
                headerTable.Columns.Add(new TableColumn() { Width = new GridLength(120) });
                headerTable.Columns.Add(new TableColumn() { Width = new GridLength(650) });
                headerTable.RowGroups.Add(new TableRowGroup());

                var trow = new TableRow();
                trow.Cells.Add(new TableCell(new Paragraph(new Run(todaysDate) { FontSize = 10, Foreground = Brushes.Gray })));
                trow.Cells.Add(new TableCell(new Paragraph(new Run("Page 1 of 60") { FontSize = 10, Foreground = Brushes.Gray }) { TextAlignment = TextAlignment.Right }));
                headerTable.RowGroups[0].Rows.Add(trow);
                doc.Blocks.Add(headerTable);

                Image image = new Image();
                image.Source = new BitmapImage(new Uri(@"pack://application:,,,/MSOOrganiser;component/Resources/Logo.png", UriKind.Absolute));

                doc.Blocks.Add(new Paragraph(new InlineUIContainer(image)) { Margin = new Thickness(160), FontSize = 10, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                doc.Blocks.Add(new Paragraph(new Run(results.OlympiadName)) { Margin = new Thickness(10), FontSize = 24, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                doc.Blocks.Add(new Paragraph(new Run("Game Plan")) { Margin = new Thickness(10), FontSize = 36, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                // Results page

                foreach (var game in results.Games.OrderBy(x => x.Value.Name))
                {
                    // a bit of a fiddle
                    doc.Blocks.Add(new Paragraph(new Run("")) { FontSize = 1, BreakPageBefore = true });

                    headerTable = new Table() { CellSpacing = 0, BreakPageBefore = true };
                    headerTable.Columns.Add(new TableColumn() { Width = new GridLength(120) });
                    headerTable.Columns.Add(new TableColumn() { Width = new GridLength(530) });
                    headerTable.Columns.Add(new TableColumn() { Width = new GridLength(120) });
                    headerTable.RowGroups.Add(new TableRowGroup());

                    trow = new TableRow();
                    trow.Cells.Add(new TableCell(new Paragraph(new Run(todaysDate) { FontSize = 10, Foreground = Brushes.Gray })));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run(results.OlympiadName) { FontSize = 10, Foreground = Brushes.Gray }) { TextAlignment = TextAlignment.Center }));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("Page 2 of 60") { FontSize = 10, Foreground = Brushes.Gray }) { TextAlignment = TextAlignment.Right }));
                    headerTable.RowGroups[0].Rows.Add(trow);
                    doc.Blocks.Add(headerTable);

                    doc.Blocks.Add(HorizontalRule());
                    doc.Blocks.Add(new Paragraph(new Run(game.Value.Name) { FontSize = 24, FontWeight = FontWeights.Bold }) { TextAlignment = TextAlignment.Center });
                    doc.Blocks.Add(HorizontalRule());

                    var detailsTable = new Table() { CellSpacing = 0 };
                    detailsTable.Columns.Add(new TableColumn() { Width = new GridLength(770) });
                    detailsTable.RowGroups.Add(new TableRowGroup());

                    trow = new TableRow();
                    var paragraph = new Paragraph();
                    paragraph.Inlines.Add(new Run("Useful Contacts:") { FontSize = 12, FontWeight = FontWeights.Bold });
                    paragraph.Inlines.Add(new Run(Environment.NewLine + game.Value.Contacts) { FontSize = 12 });
                    trow.Cells.Add(new TableCell(paragraph));
                    detailsTable.RowGroups[0].Rows.Add(trow);
                    doc.Blocks.Add(detailsTable);

                    var detailsTable2 = new Table() { CellSpacing = 0, BreakPageBefore = true };
                    detailsTable2.Columns.Add(new TableColumn() { Width = new GridLength(375) });
                    detailsTable2.Columns.Add(new TableColumn() { Width = new GridLength(375) });
                    detailsTable2.RowGroups.Add(new TableRowGroup());

                    trow = new TableRow();
                    paragraph = new Paragraph();
                    paragraph.Inlines.Add(new Run("Equipment:") { FontSize = 12, FontWeight = FontWeights.Bold });
                    paragraph.Inlines.Add(new Run(Environment.NewLine + game.Value.Equipment) { FontSize = 12 });
                    trow.Cells.Add(new TableCell(paragraph));
                    paragraph = new Paragraph();
                    paragraph.Inlines.Add(new Run("Rules Required:") { FontSize = 12, FontWeight = FontWeights.Bold });
                    paragraph.Inlines.Add(new Run(Environment.NewLine + game.Value.Rules) { FontSize = 12 });
                    trow.Cells.Add(new TableCell(paragraph));
                    detailsTable2.RowGroups[0].Rows.Add(trow);
                    doc.Blocks.Add(detailsTable2);

                    doc.Blocks.Add(MinorHorizontalRule());

                    headerTable = new Table() { CellSpacing = 0, BreakPageBefore = true };
                    headerTable.Columns.Add(new TableColumn() { Width = new GridLength(120) });
                    headerTable.Columns.Add(new TableColumn() { Width = new GridLength(530) });
                    headerTable.Columns.Add(new TableColumn() { Width = new GridLength(120) });
                    headerTable.RowGroups.Add(new TableRowGroup());

                    trow = new TableRow();
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("42") { FontSize = 18 })));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("Gackgammon Olympiad Chapionship") { FontSize = 18 }) { TextAlignment = TextAlignment.Center }));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("GGOC") { FontSize = 18 }) { TextAlignment = TextAlignment.Right }));
                    headerTable.RowGroups[0].Rows.Add(trow);
                    doc.Blocks.Add(headerTable);

                    doc.Blocks.Add(MinorHorizontalRule());

                    headerTable = new Table() { CellSpacing = 0 };
                    headerTable.Columns.Add(new TableColumn() { Width = new GridLength(100) });
                    headerTable.Columns.Add(new TableColumn() { Width = new GridLength(285) });
                    headerTable.Columns.Add(new TableColumn() { Width = new GridLength(100) });
                    headerTable.Columns.Add(new TableColumn() { Width = new GridLength(285) });
                    headerTable.RowGroups.Add(new TableRowGroup());

                    trow = new TableRow();
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("Dates:") { FontSize = 12, FontWeight = FontWeights.Bold })));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("Fri 22/8 14:00-17:45") { FontSize = 12 })));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("In Pentamind:") { FontSize = 12, FontWeight = FontWeights.Bold })));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("Yes") { FontSize = 12 })));
                    headerTable.RowGroups[0].Rows.Add(trow);
                    trow = new TableRow();
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("Entry Fee:") { FontSize = 12, FontWeight = FontWeights.Bold })));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("H - £10 (£9)") { FontSize = 12 })));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("# in Team:") { FontSize = 12, FontWeight = FontWeights.Bold })));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("1") { FontSize = 12 })));
                    headerTable.RowGroups[0].Rows.Add(trow);
                    trow = new TableRow();
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("Location:") { FontSize = 12, FontWeight = FontWeights.Bold })));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("Upstairs Room 1") { FontSize = 12 })));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("Expected:") { FontSize = 12, FontWeight = FontWeights.Bold })));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("10") { FontSize = 12 })));
                    headerTable.RowGroups[0].Rows.Add(trow);
                    trow = new TableRow();
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("# Sessions:") { FontSize = 12, FontWeight = FontWeights.Bold })));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("1") { FontSize = 12 })));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("Prize Fund:") { FontSize = 12, FontWeight = FontWeights.Bold })));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("£100") { FontSize = 12 })));
                    headerTable.RowGroups[0].Rows.Add(trow);
                    trow = new TableRow();
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("Arbiter(s):") { FontSize = 12, FontWeight = FontWeights.Bold })));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("Tarquin Emo Death") { FontSize = 12 })));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("Prize Giving:") { FontSize = 12, FontWeight = FontWeights.Bold })));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("£") { FontSize = 12 })));
                    headerTable.RowGroups[0].Rows.Add(trow);
                    trow = new TableRow();
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("Notes:") { FontSize = 12, FontWeight = FontWeights.Bold })));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("Flibble") { FontSize = 12 })) { ColumnSpan = 3 });
                    headerTable.RowGroups[0].Rows.Add(trow);
                    doc.Blocks.Add(headerTable);

                }

                DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                dlg.PrintDocument(paginator, "GamePlan");
            }
        }

        private Block HorizontalRule()
        {
            var separator = new Rectangle();
            separator.Stroke = new SolidColorBrush(Colors.Blue);
            separator.StrokeThickness = 3;
            separator.Height = 3;
            separator.Width = double.NaN;

            var lineBlock = new BlockUIContainer(separator);
            return lineBlock;
        }

        private Block MinorHorizontalRule()
        {
            var separator = new Rectangle();
            separator.Stroke = new SolidColorBrush(Colors.Gray);
            separator.StrokeThickness = 1;
            separator.Height = 1;
            separator.Width = double.NaN;

            var lineBlock = new BlockUIContainer(separator);
            return lineBlock;
        }
    }
}
