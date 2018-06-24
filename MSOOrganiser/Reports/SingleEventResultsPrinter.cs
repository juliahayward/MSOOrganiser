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
    public class SingleEventResultsPrinter
    {
        private readonly int _olympiadId;
        private readonly string _eventCode;

        public SingleEventResultsPrinter(int olympiadId, string eventCode)
        {
            _olympiadId = olympiadId;
            _eventCode = eventCode;
        }

        public FlowDocument Print()
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.Single(x => x.Id == _olympiadId);
            var evt = context.Events.SingleOrDefault(x => x.OlympiadId == _olympiadId && x.Code == _eventCode);
            var juniorDob = currentOlympiad.AgeDate.Value.AddYears(-currentOlympiad.JnrAge.Value - 1);

            var entrants = evt.Entrants.Where(x => x.Rank.HasValue && !x.Absent)
                .OrderBy(x => x.Rank).ThenBy(x => x.Medal.MedalRank());

                FlowDocument doc = new FlowDocument();

                doc.ColumnWidth = 385; // 96ths of an inch - deliberately half width
                doc.FontFamily = new FontFamily("Verdana");

                /* ********** Header *********** */

                Table headerTable = new Table() { CellSpacing = 0, BorderThickness = new Thickness(1), BorderBrush = Brushes.Black };
                headerTable.Columns.Add(new TableColumn() { Width = new GridLength(110) });
                headerTable.Columns.Add(new TableColumn() { Width = new GridLength(275) });
                headerTable.RowGroups.Add(new TableRowGroup());

                var trow = new TableRow();
                trow.Cells.Add(new TableCell(new Paragraph(new Run(_eventCode)) { Margin = new Thickness(4), FontSize = 24, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center }));
                trow.Cells.Add(new TableCell(new Paragraph(new Run(evt.Mind_Sport + " " + currentOlympiad.YearOf)) { Margin = new Thickness(2), FontSize = 16, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center }));
                headerTable.RowGroups[0].Rows.Add(trow);

                doc.Blocks.Add(headerTable);

                /************ Main body *************/

                Table bodyTable = new Table() { CellSpacing = 0 };
                bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(40) });
                bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(120) });
                bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(50) });
                bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(50) });
                bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(85) });
                bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(40) });
                bodyTable.RowGroups.Add(new TableRowGroup());

                var lastRankOrMedal = "G";
                TableRow row;
                foreach (var entrant in entrants)
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
                row.Cells.Add(new TableCell(new Paragraph(new Run("Players: " + entrants.Count())) { Margin = new Thickness(2), FontSize = 12, FontWeight = FontWeights.Bold }) { ColumnSpan = 6, TextAlignment = TextAlignment.Right });
                bodyTable.RowGroups[0].Rows.Add(row);

                return doc;
        }
    }
}
