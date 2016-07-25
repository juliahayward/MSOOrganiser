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
    public class ArbitersBadgesPrinter
    {
        public FlowDocument Print()
        {
            var context = DataEntitiesProvider.Provide();
            var olympiad = context.Olympiad_Infoes.First(x => x.Current);
            var arbiters = olympiad.Events.SelectMany(x => x.Arbiters);
            var names = arbiters.Select(x => x.Name).Distinct().ToList()
                .Select(n => n.FullName())
                .OrderBy(x => x).ToList();
            // Fill up the page
            var blanks = 8 - (names.Count() % 8);
            for (int i=0; i<blanks % 8; i++)
                names.Add("");

            FlowDocument doc = new FlowDocument();

            doc.ColumnWidth = 375; // 96ths of an inch
            doc.FontFamily = new FontFamily("Verdana");
            int number = 0;

            foreach (var name in names)
            {
                var breakPageBefore = (number > 0 && number % 8 == 0);
                var breakColumnBefore = (number > 0 && number % 8 != 0 && number % 2 == 0);
                var isInLeftColumn = (number % 8 < 4);
                var leftMargin = (isInLeftColumn) ? 16 : 6;
                number++;

                Table headerTable = new Table() { CellSpacing = 0, BreakPageBefore = breakPageBefore,
                    BreakColumnBefore = breakColumnBefore, BorderBrush = Brushes.Black, BorderThickness = 
                new Thickness(1), Margin = new Thickness(leftMargin, 20, 6, 20) }; 
                headerTable.Columns.Add(new TableColumn() { Width = new GridLength(350) });
                headerTable.Columns.Add(new TableColumn() { Width = new GridLength(350) });
                headerTable.RowGroups.Add(new TableRowGroup());

                var titleRow = new TableRow();
                var cell = new TableCell();
                cell.Blocks.Add(new Paragraph(new Run(olympiad.FullTitle())) { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                titleRow.Cells.Add(cell);
                headerTable.RowGroups[0].Rows.Add(titleRow);

                var bodyRow = new TableRow();
                cell = new TableCell();
                Image image = new Image();
                image.Source = new BitmapImage(new Uri(@"pack://application:,,,/MSOOrganiser;component/Resources/Logo.png", UriKind.Absolute));
                cell.Blocks.Add(new Paragraph(new InlineUIContainer(image)) { Margin = new Thickness(70, 2, 70, 2), FontSize = 10, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                bodyRow.Cells.Add(cell);
                headerTable.RowGroups[0].Rows.Add(bodyRow);

                bodyRow = new TableRow();
                cell = new TableCell();
                cell.Blocks.Add(new Paragraph(new Run(name)) { Margin = new Thickness(2, 2, 2, 0), FontSize = 28, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                bodyRow.Cells.Add(cell);
                headerTable.RowGroups[0].Rows.Add(bodyRow);

                bodyRow = new TableRow();
                cell = new TableCell();
                cell.Blocks.Add(new Paragraph(new Run("ARBITER")) { Margin = new Thickness(2, 0, 2, 2), FontSize = 18, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                bodyRow.Cells.Add(cell);
                headerTable.RowGroups[0].Rows.Add(bodyRow);

                doc.Blocks.Add(headerTable);
            }

            return doc;
        }
    }
}
