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
    public class PrizeFormsPrinter
    {
        public class PrizeVm
        {
            public DateTime Date { get; set; }
            public string Prize { get; set; }
            public string EventName { get; set; }
        }


        public FlowDocument Print()
        {
            var context = DataEntitiesProvider.Provide();
            var olympiad = context.Olympiad_Infoes.First(x => x.Current);
            var prizes = new PrizeVm[] { 
                new PrizeVm { Date = new DateTime(2017, 8, 20), EventName = "Ferret racing", Prize = "100" },
                new PrizeVm { Date = new DateTime(2017, 8, 21), EventName = "Welly throwing", Prize = "20.00" }
            };

            FlowDocument doc = new FlowDocument();

            doc.ColumnWidth = 750; // 96ths of an inch
            doc.FontFamily = new FontFamily("Verdana");

            foreach (var prize in prizes)
            {
                Table headerTable = new Table() { CellSpacing = 0, BreakPageBefore = true };
                headerTable.Columns.Add(new TableColumn() { Width = new GridLength(220) });
                headerTable.Columns.Add(new TableColumn() { Width = new GridLength(550) });
                headerTable.RowGroups.Add(new TableRowGroup());

                Image image = new Image();
                image.Source = new BitmapImage(new Uri(@"pack://application:,,,/MSOOrganiser;component/Resources/Logo.png", UriKind.Absolute));

                var trow = new TableRow();
                trow.Cells.Add(new TableCell(new Paragraph(new InlineUIContainer(image)) { Margin = new Thickness(10), FontSize = 10, FontWeight = FontWeights.Bold }));
                var cell = new TableCell();
                cell.Blocks.Add(new Paragraph(new Run(olympiad.FullTitle())) { Margin = new Thickness(2, 12, 2, 2), FontSize = 36, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                trow.Cells.Add(cell);
                headerTable.RowGroups[0].Rows.Add(trow);

                trow = new TableRow();
                cell = new TableCell() { ColumnSpan = 2};
                cell.Blocks.Add(new Paragraph(new Run("www.msoworld.com")) { Margin = new Thickness(2), FontSize = 18, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                trow.Cells.Add(cell);
                headerTable.RowGroups[0].Rows.Add(trow);

                trow = new TableRow();
                cell = new TableCell() { ColumnSpan = 2 };
                cell.Blocks.Add(new Paragraph(new Run("Reply to Director: A. Corfe, 51 Borough Way, Potters Bar, Herts. EN6 3HA" + Environment.NewLine + "Tel: 01707 659080  Fax: 01707 661160")) { Margin = new Thickness(2, 16, 2, 2), FontSize = 18, TextAlignment = TextAlignment.Center });
                trow.Cells.Add(cell);
                headerTable.RowGroups[0].Rows.Add(trow);

                trow = new TableRow();
                cell = new TableCell() { ColumnSpan = 2 };
                cell.Blocks.Add(new Paragraph(new Run(prize.Date.ToString("dd MMM yyyy"))) { Margin = new Thickness(2, 36, 2, 2), FontSize = 24, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Left });
                trow.Cells.Add(cell);
                headerTable.RowGroups[0].Rows.Add(trow);

                trow = new TableRow();
                cell = new TableCell() { ColumnSpan = 2 };
                cell.Blocks.Add(new Paragraph(new Run("Name: ____________________")) { Margin = new Thickness(2, 36, 2, 2), FontSize = 24, TextAlignment = TextAlignment.Left });
                trow.Cells.Add(cell);
                headerTable.RowGroups[0].Rows.Add(trow);

                trow = new TableRow();
                cell = new TableCell() { ColumnSpan = 2 };
                cell.Blocks.Add(new Paragraph(new Run("Received: £" + prize.Prize)) { Margin = new Thickness(2, 36, 2, 2), FontSize = 24, TextAlignment = TextAlignment.Left });
                trow.Cells.Add(cell);
                headerTable.RowGroups[0].Rows.Add(trow);

                trow = new TableRow();
                cell = new TableCell() { ColumnSpan = 2 };
                cell.Blocks.Add(new Paragraph(new Run("As a prize in the " + prize.EventName + " event.")) { Margin = new Thickness(2, 36, 2, 2), FontSize = 24, TextAlignment = TextAlignment.Left });
                trow.Cells.Add(cell);
                headerTable.RowGroups[0].Rows.Add(trow);

                trow = new TableRow();
                cell = new TableCell() { ColumnSpan = 2 };
                cell.Blocks.Add(new Paragraph(new Run("Signed: ____________________")) { Margin = new Thickness(2, 108, 2, 2), FontSize = 24, TextAlignment = TextAlignment.Left });
                trow.Cells.Add(cell);
                headerTable.RowGroups[0].Rows.Add(trow);

                trow = new TableRow();
                cell = new TableCell() { ColumnSpan = 2 };
                cell.Blocks.Add(new Paragraph(new Run("Cheque Number:__________ or CASH")) { Margin = new Thickness(2, 36, 2, 2), FontSize = 24, TextAlignment = TextAlignment.Left });
                trow.Cells.Add(cell);
                headerTable.RowGroups[0].Rows.Add(trow);

                doc.Blocks.Add(headerTable);
            }

            return doc;
        }
    }
}
