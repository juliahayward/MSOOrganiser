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
using MSOCore.Extensions;


namespace MSOOrganiser.Reports
{
    // Analogous to SingleEventResultsPrinter
    public class TodaysEventsResultsPrinter : FlowDocumentGeneratorBase
    {
        public FlowDocument Print(DateTime date)
        {
            var context = DataEntitiesProvider.Provide();
            var evts = context.Events.Where(x => x.Event_Sess.Any(s => s.Date == date))
                .OrderBy(x => x.Code).ToList();
            var currentOlympiad = evts.First().Olympiad_Info;
            var juniorDob = currentOlympiad.AgeDate.Value.AddYears(-currentOlympiad.JnrAge.Value - 1);

            FlowDocument doc = this.StandardTwoColumnDocument();

            // TODO this just puts one on each page
            var isFirst = true;
            foreach (var evt in evts)
            {
                AddResults(doc, evt, isFirst, evt.Olympiad_Info);
                isFirst = false;
            }
            return doc;
        }

        private void AddResults(FlowDocument doc, Event evt, bool isFirst, Olympiad_Info currentOlympiad)
        {
            /* ********** Header *********** */

            Table headerTable = new Table() { CellSpacing = 0, BorderThickness = new Thickness(1), BorderBrush = Brushes.Black };
            headerTable.Columns.Add(new TableColumn() { Width = new GridLength(100) });
            headerTable.Columns.Add(new TableColumn() { Width = new GridLength(276) });
            headerTable.RowGroups.Add(new TableRowGroup());

            if (!isFirst) headerTable.BreakPageBefore = true;

            var trow = new TableRow();
            trow.Cells.Add(new TableCell(new Paragraph(new Run(evt.Code)) { Margin = new Thickness(4), FontSize = 24, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center }));
            trow.Cells.Add(new TableCell(new Paragraph(new Run(evt.Mind_Sport)) { Margin = new Thickness(2), FontSize = 16, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center }));
            headerTable.RowGroups[0].Rows.Add(trow);

            doc.Blocks.Add(headerTable);

            /************ Main body *************/

            Table bodyTable = new Table() { CellSpacing = 0 };
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(40) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(113) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(50) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(50) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(85) });
            bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(40) });
            bodyTable.RowGroups.Add(new TableRowGroup());

            var lastRankOrMedal = "G";
            TableRow row;
            foreach (var entrant in evt.Entrants)
            {
                row = new TableRow();
                Thickness thickness;
                var rankOrMedal = (entrant.Medal != null) ? entrant.Medal.Substring(0, 1) : entrant.Rank.ToString();
                if (rankOrMedal != lastRankOrMedal)
                {
                    // add a bit of spacing
                    lastRankOrMedal = rankOrMedal;
                    bodyTable.RowGroups[0].Rows.Add(new TableRow() { });
                    thickness = new Thickness(2, 8, 2, 2);
                }
                else
                    thickness = new Thickness(2);

                row.Cells.Add(new TableCell(new Paragraph(new Run(rankOrMedal)) { Margin = thickness, FontSize = 10 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(entrant.Name.FullName())) { Margin = thickness, FontSize = 10 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(entrant.Score)) { Margin = thickness, FontSize = 10 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(entrant.Tie_break)) { Margin = thickness, FontSize = 10 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(entrant.Name.Nationality)) { Margin = thickness, FontSize = 10 }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(entrant.Name.JuniorFlagForOlympiad(currentOlympiad))) { Margin = thickness, FontSize = 10 }));
                bodyTable.RowGroups[0].Rows.Add(row);
            }
            doc.Blocks.Add(bodyTable);

            row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run("Players: " + evt.Entrants.Count())) { Margin = new Thickness(2), FontSize = 12, FontWeight = FontWeights.Bold }) { ColumnSpan = 6, TextAlignment = TextAlignment.Right });
            bodyTable.RowGroups[0].Rows.Add(row);
        }
    }
}
