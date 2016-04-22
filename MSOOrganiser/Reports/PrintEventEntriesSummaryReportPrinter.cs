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
    public class PrintEventEntriesSummaryReportPrinter
    {
        public void Print()
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();

            var events = context.Events.Where(x => x.OlympiadId == currentOlympiad.Id)
                .ToDictionary(e => e.Code, e => e.Mind_Sport);

            var entrants = context.Entrants.Where(x => x.OlympiadId == currentOlympiad.Id)
                .GroupBy(x => x.Game_Code)
                .ToDictionary(gp => gp.Key, gp => gp.Count());

            var games = context.Games.Where(x => !x.Code.StartsWith("ZZ"))
                .ToDictionary(g => g.Code, g => g.Mind_Sport);

            PrintDialog dlg = new PrintDialog();
            if ((bool)dlg.ShowDialog().GetValueOrDefault())
            {
                FlowDocument doc = new FlowDocument();
                
                doc.ColumnWidth = 200; // 96ths of an inch
                doc.FontFamily = new FontFamily("Verdana");

                Paragraph topPara = new Paragraph();
                topPara.TextAlignment = TextAlignment.Left;
                topPara.FontSize = 12;
                topPara.FontWeight = FontWeights.Bold;
                topPara.Margin = new Thickness(4);
                topPara.Inlines.Add(new Run("Numbers in Events"));
                doc.Blocks.Add(topPara);

                var grandTotal = 0;

                foreach (var gameCode in games.Keys.OrderBy(x => x))
                {
                    if (!entrants.Keys.Any(x => x.StartsWith(gameCode))) continue;

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
                    innerpara.Inlines.Add(new Run(games[gameCode]));
                    figure.Blocks.Add(innerpara);

                    Table table = new Table() { CellSpacing = 0 };
                    table.Columns.Add(new TableColumn() { Width = new GridLength(40) });
                    table.Columns.Add(new TableColumn() { Width = new GridLength(120) }); 
                    table.Columns.Add(new TableColumn() { Width = new GridLength(40) });
                    table.RowGroups.Add(new TableRowGroup());

                    var subtotal = 0;

                    foreach (var eventCode in entrants.Keys.Where(x => x.StartsWith(gameCode)))
                    {
                        var eventName = events[eventCode];

                        var row = new TableRow();
                        row.Cells.Add(new TableCell(new Paragraph(new Run(eventCode)) 
                        { Margin = new Thickness(2), FontSize = 10 }));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(eventName)) 
                        { Margin = new Thickness(2), FontSize = 10 }));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(entrants[eventCode].ToString())) 
                        { Margin = new Thickness(2), FontSize = 10 }));
                        table.RowGroups[0].Rows.Add(row);
                        subtotal += entrants[eventCode];
                        grandTotal += entrants[eventCode];
                    }

                    var trow = new TableRow();
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("")) 
                    { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold }));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run("Total")) 
                    { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold }));
                    trow.Cells.Add(new TableCell(new Paragraph(new Run(subtotal.ToString())) 
                    { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold }));
                    table.RowGroups[0].Rows.Add(trow);

                    figure.Blocks.Add(table);
                    para.Inlines.Add(figure);
                    topSection.Blocks.Add(para);

                    doc.Blocks.Add(topSection);
                }

                Paragraph gtpara = new Paragraph();
                gtpara.TextAlignment = TextAlignment.Left;
                gtpara.Margin = new Thickness(0);
                gtpara.FontWeight = FontWeights.Normal;
                gtpara.FontSize = 10;

                Figure gtfigure = new Figure();
                gtfigure.CanDelayPlacement = false;
                gtfigure.BorderBrush = Brushes.Black;
                gtfigure.BorderThickness = new Thickness(1);

                Table gttable = new Table() { CellSpacing = 0 };
                gttable.Columns.Add(new TableColumn() { Width = new GridLength(40) });
                gttable.Columns.Add(new TableColumn() { Width = new GridLength(120) });
                gttable.Columns.Add(new TableColumn() { Width = new GridLength(40) });
                gttable.RowGroups.Add(new TableRowGroup());

                var gtrow = new TableRow();
                gtrow.Cells.Add(new TableCell(new Paragraph(new Run("")) 
                { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold }));
                gtrow.Cells.Add(new TableCell(new Paragraph(new Run("Grand Total")) 
                { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold }));
                gtrow.Cells.Add(new TableCell(new Paragraph(new Run(grandTotal.ToString())) 
                { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold }));
                gttable.RowGroups[0].Rows.Add(gtrow);

                gtfigure.Blocks.Add(gttable);
                gtpara.Inlines.Add(gtfigure);
                doc.Blocks.Add(gtpara);

                DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                dlg.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
                dlg.PrintDocument(paginator, "Event Entries Summary");
            }

        }
    }
}
