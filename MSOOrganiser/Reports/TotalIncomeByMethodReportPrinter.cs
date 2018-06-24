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
    public class TotalIncomeByMethodReportPrinter
    {
        /// <summary>
        /// "actualEvents" are games; the rest are things like T-shirts
        /// </summary>
        /// <param name="actualEvents"></param>
        public FlowDocument Print(bool actualEvents)
        {
            var rg = new TotalIncomeByMethodReportGenerator();
            var results = rg.GetItemsForLatest();

                FlowDocument doc = new FlowDocument();
                
                doc.ColumnWidth = 300; // 96ths of an inch
                doc.FontFamily = new FontFamily("Verdana");

                Paragraph topPara = new Paragraph();
                topPara.TextAlignment = TextAlignment.Left;
                topPara.FontSize = 12;
                topPara.FontWeight = FontWeights.Bold;
                topPara.Margin = new Thickness(4);
                topPara.Inlines.Add(new Run(results.OlympiadName));
                topPara.Inlines.Add(new Run("Total Income by method"));
                doc.Blocks.Add(topPara);

                var grandTotal = 0m;

                foreach (var fee in results.Fees)
                {
                    // Inspired by EventIncome, maybe will expand to show individua lpayments
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
                    innerpara.Inlines.Add(new Run(fee.Method));
                    figure.Blocks.Add(innerpara);

                    Table table = new Table() { CellSpacing = 0 };
                    table.Columns.Add(new TableColumn() { Width = new GridLength(190) }); 
                    table.Columns.Add(new TableColumn() { Width = new GridLength(40) });
                    table.Columns.Add(new TableColumn() { Width = new GridLength(70) });
                    table.RowGroups.Add(new TableRowGroup());

                    var subtotal = 0m;

                        var row = new TableRow();
                        row.Cells.Add(new TableCell(new Paragraph(new Run(fee.Method ?? "Unknown")) 
                        { Margin = new Thickness(2), FontSize = 10 }));
                        row.Cells.Add(new TableCell(new Paragraph(new Run("")) 
                        { Margin = new Thickness(2), FontSize = 10 }));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(fee.TotalFees.ToString("C"))) 
                        { Margin = new Thickness(2), FontSize = 10, TextAlignment = TextAlignment.Right }));
                        table.RowGroups[0].Rows.Add(row);
                        grandTotal += fee.TotalFees;

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
                gttable.Columns.Add(new TableColumn() { Width = new GridLength(150) });
                gttable.Columns.Add(new TableColumn() { Width = new GridLength(40) });
                gttable.Columns.Add(new TableColumn() { Width = new GridLength(70) });
                gttable.RowGroups.Add(new TableRowGroup());

                var gtrow = new TableRow();
                gtrow.Cells.Add(new TableCell(new Paragraph(new Run("")) 
                { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold }));
                gtrow.Cells.Add(new TableCell(new Paragraph(new Run("Grand Total")) 
                { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold }));
                gtrow.Cells.Add(new TableCell(new Paragraph(new Run("")) 
                { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold }));
                gtrow.Cells.Add(new TableCell(new Paragraph(new Run(grandTotal.ToString("C"))) 
                { Margin = new Thickness(2), FontSize = 10, FontWeight = FontWeights.Bold }));
                gttable.RowGroups[0].Rows.Add(gtrow);

                gtfigure.Blocks.Add(gttable);
                gtpara.Inlines.Add(gtfigure);
                doc.Blocks.Add(gtpara);

                return doc;
        }
    }
}
