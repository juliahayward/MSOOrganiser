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
    public class PrintEventEntriesReportPrinter
    {
        public void Print(DateTime startDate, DateTime endDate)
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
            var events = context.Events.Where(x => x.OlympiadId == currentOlympiad.Id 
                && x.Event_Sess.Any(sess => sess.Date >= startDate && sess.Date <= endDate));

            Print(events.Select(x => x.Code));
        }

        public void Print(IEnumerable<string> EventCodes)
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
            var juniorDate = DateTime.Now.AddYears(-currentOlympiad.JnrAge.Value - 1);

            PrintDialog dlg = new PrintDialog();
            if ((bool)dlg.ShowDialog().GetValueOrDefault())
            {
                FlowDocument doc = new FlowDocument();
                doc.ColumnWidth = 200; // 96ths of an inch
                doc.FontFamily = new FontFamily("Verdana");
                bool isFirst = true;

                foreach (var EventCode in EventCodes)
                {
                    var evt = context.Events.First(x => x.OlympiadId == currentOlympiad.Id && x.Code == EventCode);

                    var entrants = context.Entrants.Where(x => x.OlympiadId == currentOlympiad.Id && x.Game_Code == EventCode)
                        .OrderBy(x => x.Name.Lastname)
                        .ThenBy(x => x.Name.Firstname);

                    AddEventToDoc(EventCode, currentOlympiad, evt, entrants, juniorDate, doc, isFirst);
                    isFirst = false;
                }

                DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                dlg.PrintDocument(paginator, "Event Entries");
            }

        }

        public void Print(string EventCode)
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
            var juniorDate = DateTime.Now.AddYears(- currentOlympiad.JnrAge.Value - 1);

            var evt = context.Events.First(x => x.OlympiadId == currentOlympiad.Id && x.Code == EventCode);
            
            var entrants = context.Entrants.Where(x => x.OlympiadId == currentOlympiad.Id && x.Game_Code == EventCode)
                .OrderBy(x => x.Name.Lastname)
                .ThenBy(x => x.Name.Firstname);


            PrintDialog dlg = new PrintDialog();
            if ((bool)dlg.ShowDialog().GetValueOrDefault())
            {
                FlowDocument doc = new FlowDocument();
                doc.ColumnWidth = 200; // 96ths of an inch
                doc.FontFamily = new FontFamily("Verdana");

                AddEventToDoc(EventCode, currentOlympiad, evt, entrants, juniorDate, doc, true);

                DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                dlg.PrintDocument(paginator, "Event Entries " + EventCode);
            }
        }

        private void AddEventToDoc(string EventCode, Olympiad_Info currentOlympiad, 
            Event evt, IEnumerable<Entrant> entrants, DateTime juniorDate, FlowDocument doc, 
            bool isFirst)
        {
            Section topSection = new Section();
            topSection.BorderBrush = Brushes.Black;
            topSection.BorderThickness = new Thickness(1);
            if (!isFirst) topSection.BreakPageBefore = true;

            Paragraph para = new Paragraph();
            para.TextAlignment = TextAlignment.Left;
            para.FontSize = 8;
            para.Margin = new Thickness(4);
            para.Inlines.Add(new Run(EventCode));
            para.Inlines.Add(new Run("                                                                 "));
            para.Inlines.Add(new Run(currentOlympiad.YearOf.Value.ToString()));
            topSection.Blocks.Add(para);

            para = new Paragraph();
            para.Margin = new Thickness(0);
            para.TextAlignment = TextAlignment.Center;
            para.FontWeight = FontWeights.Bold;
            para.Inlines.Add(new Run(evt.Mind_Sport));
            topSection.Blocks.Add(para);

            para = new Paragraph();
            para.TextAlignment = TextAlignment.Center;
            para.FontSize = 8;
            para.Margin = new Thickness(4);
            para.Inlines.Add(new Run("Entries as of " + DateTime.Now.ToString()));
            topSection.Blocks.Add(para);

            doc.Blocks.Add(topSection);
            Section middleSection = new Section();
            middleSection.BorderBrush = Brushes.Black;
            middleSection.BorderThickness = new Thickness(1);

            Table table = new Table() { CellSpacing = 0 };
            table.Columns.Add(new TableColumn() { Width = new GridLength(90) });
            table.Columns.Add(new TableColumn() { Width = new GridLength(90) });
            table.Columns.Add(new TableColumn() { Width = new GridLength(30) });
            table.RowGroups.Add(new TableRowGroup());

            foreach (var e in entrants)
            {
                // Should not happen but occasionally it does
                if (e.Name == null) continue;
                
                var jnr = (e.Name.DateofBirth.HasValue && (e.Name.DateofBirth.Value > juniorDate))
                    ? "JNR" : "";

                var row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(e.Name.Firstname)) { Margin = new Thickness(2), FontSize = 10, TextAlignment = TextAlignment.Right }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(e.Name.Lastname)) { Margin = new Thickness(2), FontSize = 10 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(jnr)) { Margin = new Thickness(2), FontSize = 10 }));
                table.RowGroups[0].Rows.Add(row);
            }

            middleSection.Blocks.Add(table);

            para = new Paragraph();
            para.Margin = new Thickness(4);
            para.TextAlignment = TextAlignment.Right;
            para.FontWeight = FontWeights.Bold;
            para.Inlines.Add(new Run("No. players: " + entrants.Count()));
            middleSection.Blocks.Add(para);
            doc.Blocks.Add(middleSection);

            para = new Paragraph();
            para.Margin = new Thickness(0);
            para.FontSize = 8;
            para.TextAlignment = TextAlignment.Center;
            para.Inlines.Add(new Run("Any players who turn up and are not above, please list below and return this sheet to registration within 30 mins of starting your event"));
            doc.Blocks.Add(para);
        }
    }
}
