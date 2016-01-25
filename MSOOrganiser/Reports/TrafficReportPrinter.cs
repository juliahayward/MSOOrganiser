using MSOCore.Reports;
using MSOOrganiser.UIUtilities;
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
    public class TrafficReportPrinter
    {
        public void Print()
        {
            PrintDialog dlg = new PrintDialog();
            if ((bool)dlg.ShowDialog().GetValueOrDefault())
            {
                using (new SpinnyCursor())
                {
                    var rg = new TrafficReportGenerator();
                    var results = rg.GetItemsForLatest();


                    FlowDocument doc = new FlowDocument();

                    doc.ColumnWidth = 300; // 96ths of an inch
                    doc.FontFamily = new FontFamily("Verdana");

                    for (var date = results.StartDate; date <= results.EndDate; date = date.AddDays(1))
                    {
                        Paragraph topPara = new Paragraph();
                        topPara.TextAlignment = TextAlignment.Left;
                        topPara.FontSize = 12;
                        topPara.FontWeight = FontWeights.Bold;
                        topPara.Margin = new Thickness(4);
                        topPara.Inlines.Add(new Run(results.OlympiadName + " Traffic Report"));
                        topPara.BreakPageBefore = true;
                        doc.Blocks.Add(topPara);

                        topPara = new Paragraph();
                        topPara.TextAlignment = TextAlignment.Left;
                        topPara.FontSize = 12;
                        topPara.FontWeight = FontWeights.Bold;
                        topPara.Margin = new Thickness(4);
                        topPara.Inlines.Add(new Run(date.ToString("dd MMM yyyy")));
                        doc.Blocks.Add(topPara);

                        var grandTotal = 0;

                        foreach (var session in results.Sessions)
                        {
                            if (!results.Events
                                    .Any(x => x.Session == session.Code && x.Date == date))
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
                            innerpara.Inlines.Add(new Run(session.Text));
                            figure.Blocks.Add(innerpara);

                            Table table = new Table() { CellSpacing = 0 };
                            table.Columns.Add(new TableColumn() { Width = new GridLength(260) });
                            table.Columns.Add(new TableColumn() { Width = new GridLength(40) });
                            table.RowGroups.Add(new TableRowGroup());

                            var total = 0;

                            foreach (var location in results.Locations)
                            {
                                if (!results.Events
                                    .Any(x => x.Location == location.Name &&
                                        x.Date == date && x.Session == session.Code))
                                    continue;

                                var eventsInLocation = results.Events
                                    .Where(x => x.Location == location.Name &&
                                        x.Date == date && x.Session == session.Code);
                                var peopleInLocation = eventsInLocation.Sum(x => x.NumParticipants);

                                var row = new TableRow();
                                row.Cells.Add(new TableCell(new Paragraph(new Run(location.Name)) { Margin = new Thickness(2), FontSize = 10 }));
                                row.Cells.Add(new TableCell(new Paragraph(new Run(peopleInLocation.ToString())) { Margin = new Thickness(2), FontSize = 10, TextAlignment = TextAlignment.Right }));
                                table.RowGroups[0].Rows.Add(row);
                                total += peopleInLocation;
                            }

                            var trow = new TableRow();
                            trow.Cells.Add(new TableCell(new Paragraph(new Run("Total")) { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold }));
                            trow.Cells.Add(new TableCell(new Paragraph(new Run(total.ToString())) { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Right }));
                            table.RowGroups[0].Rows.Add(trow);

                            grandTotal += total;

                            figure.Blocks.Add(table);
                            para.Inlines.Add(figure);

                            topSection.Blocks.Add(para);
                            doc.Blocks.Add(topSection);
                        }

                        Paragraph tpara = new Paragraph();
                        tpara.TextAlignment = TextAlignment.Left;
                        tpara.Margin = new Thickness(0);
                        tpara.FontWeight = FontWeights.Normal;
                        tpara.FontSize = 10;

                        var tfigure = new Figure();
                        tfigure.CanDelayPlacement = false;
                        tfigure.BorderBrush = Brushes.Black;
                        tfigure.BorderThickness = new Thickness(1);

                        var ttable = new Table() { CellSpacing = 0 };
                        ttable.Columns.Add(new TableColumn() { Width = new GridLength(260) });
                        ttable.Columns.Add(new TableColumn() { Width = new GridLength(40) });
                        ttable.RowGroups.Add(new TableRowGroup());

                        var ttrow = new TableRow();
                        ttrow.Cells.Add(new TableCell(new Paragraph(new Run("Grand Total")) { Margin = new Thickness(2), FontSize = 12, FontWeight = FontWeights.Bold }));
                        ttrow.Cells.Add(new TableCell(new Paragraph(new Run(grandTotal.ToString())) { Margin = new Thickness(2), FontSize = 12, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Right }));
                        ttable.RowGroups[0].Rows.Add(ttrow);

                        tfigure.Blocks.Add(ttable);
                        tpara.Inlines.Add(tfigure);

                        doc.Blocks.Add(tpara);
                    }

                    DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                    dlg.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
                    dlg.PrintDocument(paginator, "Location Usage");
                }
            }
        }
    

        public void PrintEventsPerSession()
        {
            PrintDialog dlg = new PrintDialog();
            if ((bool)dlg.ShowDialog().GetValueOrDefault())
            {
                using (new SpinnyCursor())
                {
                    var rg = new TrafficReportGenerator();
                    var results = rg.GetItemsForLatest();


                    FlowDocument doc = new FlowDocument();

                    doc.ColumnWidth = 300; // 96ths of an inch
                    doc.FontFamily = new FontFamily("Verdana");

                    for (var date = results.StartDate; date <= results.EndDate; date = date.AddDays(1))
                    {
                        Paragraph topPara = new Paragraph();
                        topPara.TextAlignment = TextAlignment.Left;
                        topPara.FontSize = 12;
                        topPara.FontWeight = FontWeights.Bold;
                        topPara.Margin = new Thickness(4);
                        topPara.Inlines.Add(new Run(results.OlympiadName + " Events per Session"));
                        topPara.BreakPageBefore = true;
                        doc.Blocks.Add(topPara);

                        topPara = new Paragraph();
                        topPara.TextAlignment = TextAlignment.Left;
                        topPara.FontSize = 12;
                        topPara.FontWeight = FontWeights.Bold;
                        topPara.Margin = new Thickness(4);
                        topPara.Inlines.Add(new Run(date.ToString("dd MMM yyyy")));
                        doc.Blocks.Add(topPara);

                        var grandTotal = 0;

                        foreach (var session in results.Sessions)
                        {
                            if (!results.Events
                                    .Any(x => x.Session == session.Code && x.Date == date))
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
                            innerpara.Inlines.Add(new Run(session.Text));
                            figure.Blocks.Add(innerpara);

                            Table table = new Table() { CellSpacing = 0 };
                            table.Columns.Add(new TableColumn() { Width = new GridLength(260) });
                            table.Columns.Add(new TableColumn() { Width = new GridLength(40) });
                            table.RowGroups.Add(new TableRowGroup());

                            var total = 0;

                            foreach (var location in results.Locations)
                            {
                                if (!results.Events
                                    .Any(x => x.Location == location.Name &&
                                        x.Date == date && x.Session == session.Code))
                                    continue;

                                var eventsInLocation = results.Events
                                    .Where(x => x.Location == location.Name &&
                                        x.Date == date && x.Session == session.Code);

                                var row = new TableRow();
                                row.Cells.Add(new TableCell(new Paragraph(new Run(location.Name)) { Margin = new Thickness(2), FontSize = 10 }));
                                row.Cells.Add(new TableCell(new Paragraph(new Run("")) { Margin = new Thickness(2), FontSize = 10, TextAlignment = TextAlignment.Right }));
                                table.RowGroups[0].Rows.Add(row);

                                foreach (var evt in eventsInLocation.OrderBy(x => x.Name))
                                {
                                    row = new TableRow();
                                    row.Cells.Add(new TableCell(new Paragraph(new Run(evt.Name)) { Margin = new Thickness(2), FontSize = 10 }));
                                    row.Cells.Add(new TableCell(new Paragraph(new Run(evt.NumParticipants.ToString())) { Margin = new Thickness(2), FontSize = 10, TextAlignment = TextAlignment.Right }));
                                    table.RowGroups[0].Rows.Add(row);
                                    total += evt.NumParticipants;
                                }
                            }

                            var trow = new TableRow();
                            trow.Cells.Add(new TableCell(new Paragraph(new Run("Total")) { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold }));
                            trow.Cells.Add(new TableCell(new Paragraph(new Run(total.ToString())) { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Right }));
                            table.RowGroups[0].Rows.Add(trow);

                            grandTotal += total;

                            figure.Blocks.Add(table);
                            para.Inlines.Add(figure);

                            topSection.Blocks.Add(para);
                            doc.Blocks.Add(topSection);
                        }

                        Paragraph tpara = new Paragraph();
                        tpara.TextAlignment = TextAlignment.Left;
                        tpara.Margin = new Thickness(0);
                        tpara.FontWeight = FontWeights.Normal;
                        tpara.FontSize = 10;

                        var tfigure = new Figure();
                        tfigure.CanDelayPlacement = false;
                        tfigure.BorderBrush = Brushes.Black;
                        tfigure.BorderThickness = new Thickness(1);

                        var ttable = new Table() { CellSpacing = 0 };
                        ttable.Columns.Add(new TableColumn() { Width = new GridLength(260) });
                        ttable.Columns.Add(new TableColumn() { Width = new GridLength(40) });
                        ttable.RowGroups.Add(new TableRowGroup());

                        var ttrow = new TableRow();
                        ttrow.Cells.Add(new TableCell(new Paragraph(new Run("Grand Total")) { Margin = new Thickness(2), FontSize = 12, FontWeight = FontWeights.Bold }));
                        ttrow.Cells.Add(new TableCell(new Paragraph(new Run(grandTotal.ToString())) { Margin = new Thickness(2), FontSize = 12, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Right }));
                        ttable.RowGroups[0].Rows.Add(ttrow);

                        tfigure.Blocks.Add(ttable);
                        tpara.Inlines.Add(tfigure);

                        doc.Blocks.Add(tpara);
                    }

                    DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                    dlg.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
                    dlg.PrintDocument(paginator, "Location Usage");
                }
            }
        }
    }
}
