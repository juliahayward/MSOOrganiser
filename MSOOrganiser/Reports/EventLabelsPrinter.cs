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
            var events = results.Events.OrderBy(x => x.SequenceNumber).ToList();
            
            FlowDocument doc = new FlowDocument();

            doc.ColumnWidth = 750; // 96ths of an inch
            doc.FontFamily = new FontFamily("Verdana");
            int number = 0;

            for (int i = 0; i <= 1 + (events.Count() - 1) / 15; i++)
            {
                var blankPara = new Paragraph(new Run() { }) { LineHeight = 8, BreakPageBefore = (i > 0) };
                doc.Blocks.Add(blankPara);
                Table outerTable = new Table()
                {
                    CellSpacing = 0,
                    BreakPageBefore = false,
                    BreakColumnBefore = false,
                    Margin = new Thickness(19, 0, 0, 0)
                };
                outerTable.Columns.Add(new TableColumn() { Width = new GridLength(247) });
                outerTable.Columns.Add(new TableColumn() { Width = new GridLength(247) });
                outerTable.Columns.Add(new TableColumn() { Width = new GridLength(247) });
                outerTable.RowGroups.Add(new TableRowGroup());
                for (int r = 0; r < 5; r++)
                {
                    var row = new TableRow();
                    for (int c = 0; c < 3; c++)
                    {
                        if (events.Any())
                        {
                            var cell = new TableCell() { LineHeight = 174.5 };
                            cell.Blocks.Add(GetEventTable(results.OlympiadTitle, events.First()));
                            events.RemoveAt(0);
                            row.Cells.Add(cell);
                        }
                    }
                    outerTable.RowGroups[0].Rows.Add(row);
                }
                doc.Blocks.Add(outerTable);
            }

            //foreach (var evt in results.Events.OrderBy(x => x.SequenceNumber))
            //{
            //    var breakPageBefore = (number > 0 && number % 18 == 0);
            //    var breakColumnBefore = (number > 0 && number % 18 != 0 && number % 6 == 0);
            //    var columnNumber = ((number / 6) % 3);
            //    var rowNumber = (number % 6);
            //    var leftMargin = leftMargins[columnNumber];
            //    var topMargin = rowMargins[rowNumber];
            //    number++;

            //    Table headerTable = new Table() { CellSpacing = 0, BreakPageBefore = breakPageBefore,
            //        BreakColumnBefore = breakColumnBefore, BorderBrush = Brushes.Black, BorderThickness = 
            //    new Thickness(1), Margin = new Thickness(leftMargin, topMargin, 6, 30) }; 
            //    headerTable.Columns.Add(new TableColumn() { Width = new GridLength(125) });
            //    headerTable.Columns.Add(new TableColumn() { Width = new GridLength(100) });
            //    headerTable.RowGroups.Add(new TableRowGroup());

            //    var titleRow = new TableRow();
            //    var cell = new TableCell() { ColumnSpan = 2 };
            //    cell.Blocks.Add(new Paragraph(new Run(results.OlympiadTitle)) { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
            //    titleRow.Cells.Add(cell);
            //    headerTable.RowGroups[0].Rows.Add(titleRow);

            //    var bodyRow = new TableRow();
            //    cell = new TableCell();
            //    Image image = new Image();
            //    image.Source = new BitmapImage(new Uri(@"pack://application:,,,/MSOOrganiser;component/Resources/Logo.png", UriKind.Absolute));
            //    cell.Blocks.Add(new Paragraph(new InlineUIContainer(image)) { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
            //    cell.Blocks.Add(new Paragraph(new Run(evt.Title)) { Margin = new Thickness(0), FontSize = 8, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
            //    bodyRow.Cells.Add(cell);
            //    cell = new TableCell();
            //    cell.Blocks.Add(new Paragraph(new Run(evt.Code)) { Margin = new Thickness(2), FontSize = 20, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
            //    cell.Blocks.Add(new Paragraph(new Run(evt.SequenceNumber.ToString())) { Margin = new Thickness(2, 2, 2, 18), FontSize = 64, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
            //    bodyRow.Cells.Add(cell);
            //    headerTable.RowGroups[0].Rows.Add(bodyRow);

            //    doc.Blocks.Add(headerTable);
            //}

            return doc;
        }

        private Table GetEventTable(string olympiadTitle, MSOCore.Reports.MedalFormsGenerator.MedalFormsVm.EventVm evt)
        {
            Table headerTable = new Table()
            {
                CellSpacing = 0,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(10,10,0,23),
                LineHeight = Double.NaN             // override any parent
            };
            headerTable.Columns.Add(new TableColumn() { Width = new GridLength(125) });
            headerTable.Columns.Add(new TableColumn() { Width = new GridLength(100) });
            headerTable.RowGroups.Add(new TableRowGroup());

            var titleRow = new TableRow();
            var cell = new TableCell() { ColumnSpan = 2 };
            cell.Blocks.Add(new Paragraph(new Run(olympiadTitle)) { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
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

            return headerTable;
        }
    }
}
