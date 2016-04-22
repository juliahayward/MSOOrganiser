using MSOCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MSOOrganiser.Reports
{
    public class PrintContactsReportPrinter
    {
        public void Print()
        {
            var context = DataEntitiesProvider.Provide();

            var games = context.Games.Where(x => !x.Code.StartsWith("ZZ"))
                .OrderBy(x => x.Code)
                .ToList();

            PrintDialog dlg = new PrintDialog();
            if ((bool)dlg.ShowDialog().GetValueOrDefault())
            {
                FlowDocument doc = new FlowDocument();

                doc.ColumnWidth = 320; // 96ths of an inch
                doc.FontFamily = new FontFamily("Verdana");

                Paragraph topPara = new Paragraph();
                topPara.TextAlignment = TextAlignment.Left;
                topPara.FontWeight = FontWeights.Bold;
                topPara.FontSize = 12;
                topPara.Margin = new Thickness(4);
                topPara.Inlines.Add(new Run("Contacts"));
                doc.Blocks.Add(topPara);


                foreach (var game in games)
                {
                    Section topSection = new Section();

                    // http://stackoverflow.com/questions/12397089/how-can-i-keep-a-table-together-in-a-flowdocument

                    Paragraph para = new Paragraph();
                    para.TextAlignment = TextAlignment.Left;
                    para.Margin = new Thickness(0);
                    para.FontWeight = FontWeights.Normal;
                    para.FontSize = 10;

                    Figure figure = new Figure();
                    figure.CanDelayPlacement = false;
                    figure.BorderBrush = Brushes.Black;
                    figure.BorderThickness = new Thickness(1);

                    Paragraph innerpara = new Paragraph();
                    innerpara.TextAlignment = TextAlignment.Left;
                    innerpara.Margin = new Thickness(4);
                    innerpara.FontWeight = FontWeights.Bold;
                    innerpara.FontSize = 12;
                    innerpara.Inlines.Add(new Run(game.Mind_Sport));
                    figure.Blocks.Add(innerpara);

                    Table table = new Table() { CellSpacing = 0 };
                    table.Columns.Add(new TableColumn() { Width = new GridLength(56) });
                    table.Columns.Add(new TableColumn() { Width = new GridLength(264) });
                    table.RowGroups.Add(new TableRowGroup());


                    var row = new TableRow();
                    row.Cells.Add(new TableCell(new Paragraph(new Run("Code")) { Margin = new Thickness(2), FontSize = 10 }));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(game.Code)) { Margin = new Thickness(2), FontSize = 10 }));
                    table.RowGroups[0].Rows.Add(row);


                    var trow = new TableRow();
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("Contacts")) { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold }));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run(game.Contacts)) { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold }));
                    table.RowGroups[0].Rows.Add(trow);

                    figure.Blocks.Add(table);
                    para.Inlines.Add(figure);
                    topSection.Blocks.Add(para);

                    doc.Blocks.Add(topSection);
                }

                DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                dlg.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
                dlg.PrintDocument(paginator, "Event Entries Summary");
            }
        }
    }
}
