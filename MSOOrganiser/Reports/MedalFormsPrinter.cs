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

namespace MSOOrganiser.Reports
{
    public class MedalFormsPrinter
    {
        public void Print(string eventCode)
        {
            var rg = new MedalFormsGenerator();
            var results = rg.GetItemsForLatest(eventCode);
            Print(results);
        }

        public void Print(DateTime fromDate, DateTime toDate)
        {
            DateTime dt = DateTime.Now;


            var rg = new MedalFormsGenerator();
            var results = rg.GetItemsForLatest(fromDate, toDate);


            DateTime dt2 = DateTime.Now;
            System.Windows.MessageBox.Show(dt2.Subtract(dt).TotalMilliseconds.ToString());


            Print(results);
        }

        public void Print(MedalFormsGenerator.MedalFormsVm results)
        {
            PrintDialog dlg = new PrintDialog();
            if ((bool)dlg.ShowDialog().GetValueOrDefault())
            {
                FlowDocument doc = new FlowDocument();

                doc.ColumnWidth = 770; // 96ths of an inch
                doc.FontFamily = new FontFamily("Verdana");

                bool isFirst = true;
                foreach (var evt in results.Events)
                {

                    /* ********** Header *********** */

                    Table headerTable = new Table() { CellSpacing = 0, BreakPageBefore = !isFirst };
                    headerTable.Columns.Add(new TableColumn() { Width = new GridLength(170) });
                    headerTable.Columns.Add(new TableColumn() { Width = new GridLength(480) });
                    headerTable.Columns.Add(new TableColumn() { Width = new GridLength(120) });
                    headerTable.RowGroups.Add(new TableRowGroup());

                    Image image = new Image();
                    image.Source = new BitmapImage(new Uri(@"pack://application:,,,/MSOOrganiser;component/Resources/Logo.png", UriKind.Absolute));

                    var trow = new TableRow();
                    trow.Cells.Add(new TableCell(new Paragraph(new InlineUIContainer(image)) { Margin = new Thickness(10), FontSize = 10, FontWeight = FontWeights.Bold }));
                    var cell = new TableCell();
                    cell.Blocks.Add(new Paragraph(new Run(results.OlympiadTitle)) { Margin = new Thickness(10), FontSize = 18, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                    cell.Blocks.Add(new Paragraph(new Run(evt.Title)) { Margin = new Thickness(2), FontSize = 32, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                    trow.Cells.Add(cell);
                    cell = new BorderedTableCell();
                    cell.Blocks.Add(new Paragraph(new Run(evt.Code)) { Margin = new Thickness(10), FontSize = 12, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                    cell.Blocks.Add(new Paragraph(new Run(evt.SequenceNumber)) { Margin = new Thickness(2), FontSize = 64, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                    trow.Cells.Add(cell);

                    headerTable.RowGroups[0].Rows.Add(trow);

                    doc.Blocks.Add(headerTable);

                    /************ Event metadata ********/

                    Table metaTable = new Table() { CellSpacing = 0 };
                    metaTable.Columns.Add(new TableColumn() { Width = new GridLength(100) });
                    metaTable.Columns.Add(new TableColumn() { Width = new GridLength(240) });
                    metaTable.Columns.Add(new TableColumn() { Width = new GridLength(240) });
                    metaTable.RowGroups.Add(new TableRowGroup());

                    trow = new TableRow();
                    trow.Cells.Add(new TableCell());
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("Start: " + evt.StartDate)) { Margin = new Thickness(2), FontSize = 10 }));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("Location: " + evt.Location)) { Margin = new Thickness(2), FontSize = 10 }));
                    metaTable.RowGroups[0].Rows.Add(trow);

                    trow = new TableRow();
                    trow.Cells.Add(new TableCell());
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("End: " + evt.EndDate)) { Margin = new Thickness(2), FontSize = 10 }));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("Prize Giving: " + evt.PrizeGiving)) { Margin = new Thickness(2), FontSize = 10 }));
                    metaTable.RowGroups[0].Rows.Add(trow);

                    doc.Blocks.Add(metaTable);

                    /* top 3 */

                    Table top3Table = new Table() { CellSpacing = 0 };
                    top3Table.Columns.Add(new TableColumn() { Width = new GridLength(256) });
                    top3Table.Columns.Add(new TableColumn() { Width = new GridLength(256) });
                    top3Table.Columns.Add(new TableColumn() { Width = new GridLength(256) });
                    top3Table.RowGroups.Add(new TableRowGroup());

                    trow = new TableRow();
                    cell = new TableCell();
                    cell.Blocks.Add(new Paragraph(new Run("1st Place")) { Margin = new Thickness(2), FontSize = 24, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                    cell.Blocks.Add(new Paragraph(new Run("Gold Medal")) { Margin = new Thickness(2), FontSize = 12, TextAlignment = TextAlignment.Center });
                    cell.Blocks.Add(new Paragraph(new Run(evt.Prize1)) { Margin = new Thickness(2), FontSize = 12, TextAlignment = TextAlignment.Center });
                    trow.Cells.Add(cell);
                    cell = new TableCell();
                    cell.Blocks.Add(new Paragraph(new Run("2nd Place")) { Margin = new Thickness(2), FontSize = 24, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                    cell.Blocks.Add(new Paragraph(new Run("Silver Medal")) { Margin = new Thickness(2), FontSize = 12, TextAlignment = TextAlignment.Center });
                    cell.Blocks.Add(new Paragraph(new Run(evt.Prize2)) { Margin = new Thickness(2), FontSize = 12, TextAlignment = TextAlignment.Center });
                    trow.Cells.Add(cell);
                    cell = new TableCell();
                    cell.Blocks.Add(new Paragraph(new Run("3rd Place")) { Margin = new Thickness(2), FontSize = 24, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                    cell.Blocks.Add(new Paragraph(new Run("Bronze Medal")) { Margin = new Thickness(2), FontSize = 12, TextAlignment = TextAlignment.Center });
                    cell.Blocks.Add(new Paragraph(new Run(evt.Prize3)) { Margin = new Thickness(2), FontSize = 12, TextAlignment = TextAlignment.Center });
                    trow.Cells.Add(cell);
                    top3Table.RowGroups[0].Rows.Add(trow);

                    trow = new TableRow();
                    cell = new TableCell() { Padding = new Thickness(40), ColumnSpan = 3 };
                    trow.Cells.Add(cell);
                    top3Table.RowGroups[0].Rows.Add(trow);

                    trow = new TableRow();
                    cell = new TableCell() { Padding = new Thickness(16), ColumnSpan = 3, BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) };
                    cell.Blocks.Add(new Paragraph(new Run("Junior Prizes: " + evt.JuniorPrizes)));
                    trow.Cells.Add(cell);
                    top3Table.RowGroups[0].Rows.Add(trow);

                    trow = new TableRow();
                    cell = new TableCell() { Padding = new Thickness(16), ColumnSpan = 3, BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) };
                    cell.Blocks.Add(new Paragraph(new Run("Other Prizes: " + evt.OtherPrizes)));
                    trow.Cells.Add(cell);
                    top3Table.RowGroups[0].Rows.Add(trow);

                    trow = new TableRow();
                    cell = new TableCell() { ColumnSpan = 3 };
                    cell.Blocks.Add(new Paragraph(new Run("ARBITER: Please print in BLOCK CAPITALS")) { FontSize = 12 });
                    trow.Cells.Add(cell);
                    top3Table.RowGroups[0].Rows.Add(trow);

                    doc.Blocks.Add(top3Table);

                    /************ Main body *************/

                    Table mainTable = new Table() { CellSpacing = 0 };
                    mainTable.Columns.Add(new TableColumn() { Width = new GridLength(60) });
                    mainTable.Columns.Add(new TableColumn() { Width = new GridLength(155) });
                    mainTable.Columns.Add(new TableColumn() { Width = new GridLength(155) });
                    mainTable.Columns.Add(new TableColumn() { Width = new GridLength(100) });
                    mainTable.Columns.Add(new TableColumn() { Width = new GridLength(100) });
                    mainTable.Columns.Add(new TableColumn() { Width = new GridLength(200) });
                    mainTable.RowGroups.Add(new TableRowGroup());

                    trow = new TableRow();
                    cell = new BorderedTableCell(new Paragraph(new Run("Position")) { FontSize = 12 });
                    trow.Cells.Add(cell);
                    cell = new BorderedTableCell(new Paragraph(new Run("First Name:")) { FontSize = 12 });
                    trow.Cells.Add(cell);
                    cell = new BorderedTableCell(new Paragraph(new Run("Surname:")) { FontSize = 12 });
                    trow.Cells.Add(cell);
                    cell = new BorderedTableCell(new Paragraph(new Run("Nationality:")) { FontSize = 12 });
                    trow.Cells.Add(cell);
                    cell = new BorderedTableCell(new Paragraph(new Run("Score:")) { FontSize = 12 });
                    trow.Cells.Add(cell);
                    cell = new BorderedTableCell(new Paragraph(new Run("Prize:")) { FontSize = 12 });
                    trow.Cells.Add(cell);
                    mainTable.RowGroups[0].Rows.Add(trow);

                    trow = new TableRow();
                    cell = new BorderedTableCell() { Padding = new Thickness(210) };
                    trow.Cells.Add(cell);
                    cell = new BorderedTableCell() { Padding = new Thickness(210) };
                    trow.Cells.Add(cell);
                    cell = new BorderedTableCell() { Padding = new Thickness(210) };
                    trow.Cells.Add(cell);
                    cell = new BorderedTableCell() { Padding = new Thickness(210) };
                    trow.Cells.Add(cell);
                    cell = new BorderedTableCell() { Padding = new Thickness(210) };
                    trow.Cells.Add(cell);
                    cell = new BorderedTableCell() { Padding = new Thickness(210) };
                    trow.Cells.Add(cell);
                    mainTable.RowGroups[0].Rows.Add(trow);

                    doc.Blocks.Add(mainTable);

                    /************ Footer *************/

                    Table footerTable = new Table() { CellSpacing = 0 };
                    footerTable.Columns.Add(new TableColumn() { Width = new GridLength(170) });
                    footerTable.Columns.Add(new TableColumn() { Width = new GridLength(270) });
                    footerTable.Columns.Add(new TableColumn() { Width = new GridLength(330) });
                    footerTable.RowGroups.Add(new TableRowGroup());

                    trow = new TableRow();
                    cell = new BorderedTableCell(new Paragraph(new Run("Attach full list of results")) { FontSize = 12 });
                    trow.Cells.Add(cell);
                    cell = new BorderedTableCell(new Paragraph(new Run("Arbiter:")) { FontSize = 12 });
                    trow.Cells.Add(cell);
                    cell = new BorderedTableCell(new Paragraph(new Run("Verified by D Levy / T Corfe:")) { FontSize = 12 });
                    trow.Cells.Add(cell);
                    footerTable.RowGroups[0].Rows.Add(trow);

                    doc.Blocks.Add(footerTable);

                    /*********** Page 2 ****************/

                    doc.Blocks.Add(new Paragraph(new Run(evt.Title)) { BreakPageBefore = true, Margin = new Thickness(2), FontSize = 16, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                    var Block = new Paragraph(new Run(Page2HeaderText)) { FontSize = 12, FontWeight = FontWeights.Bold };
                    doc.Blocks.Add(Block);

                    Table page2Outer = new Table() { CellSpacing = 0 };
                    page2Outer.Columns.Add(new TableColumn() { Width = new GridLength(440) });
                    page2Outer.Columns.Add(new TableColumn() { Width = new GridLength(330) });
                    page2Outer.RowGroups.Add(new TableRowGroup());

                    trow = new TableRow();
                    var p2OuterLeftCell = new TableCell();
                    trow.Cells.Add(p2OuterLeftCell);
                    var p2OuterRightCell = new TableCell();
                    trow.Cells.Add(p2OuterRightCell);
                    page2Outer.RowGroups[0].Rows.Add(trow);

                    doc.Blocks.Add(page2Outer);

                    /*********** Page 2 left **************/

                    Table page2Left = new Table() { CellSpacing = 0, BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) };
                    page2Left.Columns.Add(new TableColumn() { Width = new GridLength(160) });
                    page2Left.Columns.Add(new TableColumn() { Width = new GridLength(65) });
                    page2Left.Columns.Add(new TableColumn() { Width = new GridLength(65) });
                    page2Left.Columns.Add(new TableColumn() { Width = new GridLength(100) });
                    page2Left.Columns.Add(new TableColumn() { Width = new GridLength(40) });
                    page2Left.RowGroups.Add(new TableRowGroup());

                    trow = new TableRow();
                    trow.Cells.Add(new BorderedTableCell(new Paragraph(new Run("Name")) { FontSize = 12 }));
                    trow.Cells.Add(new BorderedTableCell(new Paragraph(new Run("Rank")) { FontSize = 12 }));
                    trow.Cells.Add(new BorderedTableCell(new Paragraph(new Run("Score")) { FontSize = 12 }));
                    trow.Cells.Add(new BorderedTableCell(new Paragraph(new Run("Nationality")) { FontSize = 12 }));
                    trow.Cells.Add(new BorderedTableCell(new Paragraph(new Run("JNR")) { FontSize = 12 }));
                    page2Left.RowGroups[0].Rows.Add(trow);

                    foreach (var entrant in evt.Entrants)
                    {

                        trow = new TableRow();
                        trow.Cells.Add(new BorderedTableCell(new Paragraph(new Run(entrant.Name)) { FontSize = 12, Padding = new Thickness(4) }));
                        trow.Cells.Add(new BorderedTableCell(new Paragraph(new Run("")) { FontSize = 12 }));
                        trow.Cells.Add(new BorderedTableCell(new Paragraph(new Run("")) { FontSize = 12 }));
                        trow.Cells.Add(new BorderedTableCell(new Paragraph(new Run(entrant.Nationality)) { FontSize = 12, Padding = new Thickness(4) }));
                        trow.Cells.Add(new BorderedTableCell(new Paragraph(new Run(entrant.Junior)) { FontSize = 12, Padding = new Thickness(4) }));
                        page2Left.RowGroups[0].Rows.Add(trow);

                    }

                    trow = new TableRow();
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("No. of Players: " + evt.Entrants.Count())) { FontSize = 16, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center }) { ColumnSpan = 5 });
                    page2Left.RowGroups[0].Rows.Add(trow);

                    p2OuterLeftCell.Blocks.Add(page2Left);

                    /**** Page 2 right *****/

                    p2OuterRightCell.Blocks.Add(new Paragraph(new Run(Page2RightHeaderText)) { Padding = new Thickness(8), FontSize = 12, FontWeight = FontWeights.Bold });
                    p2OuterRightCell.Blocks.Add(new Paragraph(new Run(Page2RightBlurb1)) { FontSize = 12 });
                    p2OuterRightCell.Blocks.Add(new BorderedParagraph() { Padding = new Thickness(90) });
                    p2OuterRightCell.Blocks.Add(new Paragraph(new Run(Page2RightBlurb2)) { FontSize = 12 });
                    p2OuterRightCell.Blocks.Add(new BorderedParagraph() { Padding = new Thickness(90) });
                    p2OuterRightCell.Blocks.Add(new Paragraph(new Run(Page2RightBlurb3)) { FontSize = 12 });

                    isFirst = false;
                }

                DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                dlg.PrintDocument(paginator, "MedalForms");
            }
        }

        private const string Page2HeaderText = "NOTE: The rank below is used for the pentamind and should not be split by a tie break";
        private const string Page2RightHeaderText = "Dear Tournament Director,";
        private static string Page2RightBlurb1 = "We’d really love it if you could spare the time to give us some information on your tournament for the “Olympiad News” printed bulletin and the MSO web site. The more you can tell us, the more coverage your game will get!"
            + Environment.NewLine + Environment.NewLine
            + "What were the main stories of the tournament? Were there any dramatic last-round turnarounds, catastrophic failures, heroic comebacks, fallen favourites, new stars, amazing games, bizarre one-offs, controversial incidents, incredible novelties, outrageous swindles, beautiful plays or anything else exciting? Please write 50-100 words, or go into as much detail as you want to if you like.";
        private const string Page2RightBlurb2 = "Can you give us any information on the winners or other main players? Are there any that you think would be interesting for us to interview? ";
        private static string Page2RightBlurb3 = "We strongly encourage you to provide the scores of any games that would be worthy of publication – if you can provide commentary on them, so much the better. If you can supply any particularly interesting quotes, embarrassing bloopers or entertaining anecdotes, that would be wonderful. If there’s anything else which you think would be good for the web site, please let us know; we’d love to feature whatever articles or reports that you want to provide. However you supply them, we’ll do our best to make good use of them."
            + Environment.NewLine + Environment.NewLine 
            + "Very many thanks in advance for any help that you supply!"
            + Environment.NewLine 
            + "Chris M. Dickson" 
            + Environment.NewLine   
            + "MSO Olympiad News bulletin editor and webmaster, www.msoworld.com";
    }
}
