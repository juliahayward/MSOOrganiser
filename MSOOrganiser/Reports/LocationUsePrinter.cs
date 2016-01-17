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

namespace MSOOrganiser.Reports
{
    public class LocationUsePrinter
    {
        public void Print()
        {
            var rg = new LocationUseReportGenerator();
            var results = rg.GetItemsForLatest();

            PrintDialog dlg = new PrintDialog();
            if ((bool)dlg.ShowDialog().GetValueOrDefault())
            {
                FlowDocument doc = new FlowDocument();
                
                doc.ColumnWidth = 300; // 96ths of an inch
                doc.FontFamily = new FontFamily("Verdana");

                foreach (var location in results.Locations)
                {
                    if (!results.Events
                        .Any(x => x.Location == location.Name))
                        continue;

                Paragraph topPara = new Paragraph();
                topPara.TextAlignment = TextAlignment.Left;
                topPara.FontSize = 12;
                topPara.FontWeight = FontWeights.Bold;
                topPara.Margin = new Thickness(4);
                topPara.Inlines.Add(new Run(results.OlympiadName + " Location Use"));
                topPara.BreakPageBefore = true;
                doc.Blocks.Add(topPara);
                    
                    topPara = new Paragraph();
                    topPara.TextAlignment = TextAlignment.Left;
                    topPara.FontSize = 12;
                    topPara.FontWeight = FontWeights.Bold;
                    topPara.Margin = new Thickness(4);
                    topPara.Inlines.Add(new Run(location.Name));
                    doc.Blocks.Add(topPara);

                    for (var date = results.StartDate; date <= results.EndDate; date = date.AddDays(1))
                    {
                        if (!results.Events
                                .Any(x => x.Location == location.Name && x.Date == date))
                            continue;

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
                        innerpara.Inlines.Add(new Run(date.ToString("dddd dd MMM yyyy")));
                        figure.Blocks.Add(innerpara);

                        foreach (var session in results.Sessions)
                        {
                            if (!results.Events
                                .Any(x => x.Location == location.Name && 
                                    x.Date == date && x.SessionCodes.Contains(session.Code)))
                                continue;

                            innerpara = new Paragraph();
                            innerpara.TextAlignment = TextAlignment.Left;
                            innerpara.Margin = new Thickness(4);
                            innerpara.FontWeight = FontWeights.Bold;
                            innerpara.FontSize = 12;
                            innerpara.Inlines.Add(new Run(session.Text));
                            figure.Blocks.Add(innerpara);

                            Table table = new Table() { CellSpacing = 0 };
                            table.Columns.Add(new TableColumn() { Width = new GridLength(40) });
                            table.Columns.Add(new TableColumn() { Width = new GridLength(220) });
                            table.Columns.Add(new TableColumn() { Width = new GridLength(40) });
                            table.RowGroups.Add(new TableRowGroup());

                            foreach (var evt in results.Events
                                .Where(x => x.Location == location.Name && 
                                    x.Date == date && x.SessionCodes.Contains(session.Code))
                                .OrderBy(x => x.SequenceNumber))
                            {
                                var row = new TableRow();
                                row.Cells.Add(new TableCell(new Paragraph(new Run(evt.Code)) { Margin = new Thickness(2), FontSize = 10 }));
                                row.Cells.Add(new TableCell(new Paragraph(new Run(evt.Name)) { Margin = new Thickness(2), FontSize = 10 }));
                                row.Cells.Add(new TableCell(new Paragraph(new Run(evt.NumParticipants.ToString())) { Margin = new Thickness(2), FontSize = 10, TextAlignment = TextAlignment.Right }));
                                table.RowGroups[0].Rows.Add(row);
                            }

                            figure.Blocks.Add(table);
                        }
                        para.Inlines.Add(figure);
                        topSection.Blocks.Add(para);

                        doc.Blocks.Add(topSection);
                    }
                }

                DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                dlg.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
                dlg.PrintDocument(paginator, "Location Usage");
            }
        }
    }
}
