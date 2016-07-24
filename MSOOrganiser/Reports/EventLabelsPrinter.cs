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
    public class EventLabelsPrinter
    {
        public FlowDocument Print()
        {
            var rg = new MedalFormsGenerator();
            var results = rg.GetItemsForLatest();
            
            FlowDocument doc = new FlowDocument();

            doc.ColumnWidth = 225; // 96ths of an inch
            doc.FontFamily = new FontFamily("Verdana");
            int number = 0;

            foreach (var evt in results.Events.OrderBy(x => x.SequenceNumber))
            {
                var breakPageBefore = (number > 0 && number % 18 == 0);
                var breakColumnBefore = (number > 0 && number % 18 != 0 && number % 6 == 0);
                var isInLeftColumn = (number % 18 < 6);
                var leftMargin = (isInLeftColumn) ? 16 : 6;
                number++;

                Table headerTable = new Table() { CellSpacing = 0, BreakPageBefore = breakPageBefore,
                    BreakColumnBefore = breakColumnBefore, BorderBrush = Brushes.Black, BorderThickness = 
                new Thickness(1), Margin = new Thickness(leftMargin, 30, 6, 30) }; 
                headerTable.Columns.Add(new TableColumn() { Width = new GridLength(125) });
                headerTable.Columns.Add(new TableColumn() { Width = new GridLength(100) });
                headerTable.RowGroups.Add(new TableRowGroup());

                var titleRow = new TableRow();
                var cell = new TableCell() { ColumnSpan = 2 };
                cell.Blocks.Add(new Paragraph(new Run(results.OlympiadTitle)) { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                titleRow.Cells.Add(cell);
                headerTable.RowGroups[0].Rows.Add(titleRow);

                var bodyRow = new TableRow();
                cell = new TableCell();
                Image image = new Image();
                image.Source = new BitmapImage(new Uri(@"pack://application:,,,/MSOOrganiser;component/Resources/Logo.png", UriKind.Absolute));
                cell.Blocks.Add(new Paragraph(new InlineUIContainer(image)) { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                cell.Blocks.Add(new Paragraph(new Run(evt.Title)) { Margin = new Thickness(0), FontSize = 8, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                bodyRow.Cells.Add(cell);
                cell = new TableCell();
                cell.Blocks.Add(new Paragraph(new Run(evt.Code)) { Margin = new Thickness(2), FontSize = 20, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                cell.Blocks.Add(new Paragraph(new Run(evt.SequenceNumber.ToString())) { Margin = new Thickness(2, 2, 2, 18), FontSize = 64, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                bodyRow.Cells.Add(cell);
                headerTable.RowGroups[0].Rows.Add(bodyRow);

                doc.Blocks.Add(headerTable);
            }

            return doc;
        }
    }
}
