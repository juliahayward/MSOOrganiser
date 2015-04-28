using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MSOOrganiser
{
    /// <summary>
    /// Interaction logic for NationalityReport.xaml
    /// </summary>
    public partial class NationalityReport : UserControl
    {
        public class NationalityVm 
        { 
            public string Nationality { get; set; } 
            public string Males { get; set; }
            public string Females { get; set; }
            public string Total { get; set; }
        }

        public NationalityReport()
        {
            InitializeComponent();
            Populate();

            dataGrid.DataContext = Items;
        }

        ObservableCollection<NationalityVm> Items { get; set; }

        public void Populate()
        {
            Items = new ObservableCollection<NationalityVm>();
            
            var context = new DataEntities();
            var contIds = context.Entrants.Where(x => x.Year == 2014) // TODO current olympiad
                .Select(x => x.Mind_Sport_ID).Distinct().ToList();
            var conts = context.Contestants.Where(x => contIds.Contains(x.Mind_Sport_ID))
                .ToList();

            var totals = conts.GroupBy(x => x.Nationality ?? "Other")
                .ToDictionary(x => x.Key, x => x.Count());
            var males = conts.Where(x => !x.Male.HasValue || x.Male.Value).GroupBy(x => x.Nationality ?? "Other")
                .ToDictionary(x => x.Key, x => x.Count());
            var females = conts.Where(x => x.Male.HasValue && !x.Male.Value).GroupBy(x => x.Nationality ?? "Other")
                .ToDictionary(x => x.Key, x => x.Count());
            foreach (var key in totals.Keys.OrderBy(x => x))
                Items.Add(new NationalityVm() 
                    { 
                        Nationality = key, 
                        Males = males.ContainsKey(key) ? males[key].ToString() : "",
                        Females = females.ContainsKey(key) ? females[key].ToString() : "",
                        Total = totals[key].ToString()
                    });

        }

        private void print_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog dlg = new PrintDialog();
            if ((bool)dlg.ShowDialog().GetValueOrDefault())
            {
                FlowDocument doc = new FlowDocument();
                doc.ColumnWidth = 700; // 96ths of an inch
                doc.FontFamily = new FontFamily("Verdana");

                Paragraph para = new Paragraph();
                para.Margin = new Thickness(0);
                para.TextAlignment = TextAlignment.Center;
                para.Inlines.Add(new Run("Nationalities"));
                doc.Blocks.Add(para);

                Table table = new Table() { CellSpacing = 0, BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) };
                table.Columns.Add(new TableColumn() { Width = new GridLength(250) });
                table.Columns.Add(new TableColumn() { Width = new GridLength(50) });
                table.Columns.Add(new TableColumn() { Width = new GridLength(50) });
                table.Columns.Add(new TableColumn() { Width = new GridLength(50) });
                table.RowGroups.Add(new TableRowGroup());

                var trow = new TableRow();
                trow.Cells.Add(new TableCell(new Paragraph(new Run("Nationality")) { Margin = new Thickness(4), FontSize = 8, FontWeight = FontWeights.Bold }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                trow.Cells.Add(new TableCell(new Paragraph(new Run("Male")) { Margin = new Thickness(4), FontSize = 8, FontWeight = FontWeights.Bold }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                trow.Cells.Add(new TableCell(new Paragraph(new Run("Female")) { Margin = new Thickness(4), FontSize = 8, FontWeight = FontWeights.Bold }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                trow.Cells.Add(new TableCell(new Paragraph(new Run("Total")) { Margin = new Thickness(4), FontSize = 8, FontWeight = FontWeights.Bold }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                table.RowGroups[0].Rows.Add(trow);

                foreach (var item in Items)
                {
                    var row = new TableRow();
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.Nationality)) { Margin = new Thickness(4), FontSize = 8 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.Males)) { Margin = new Thickness(4), FontSize = 8 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.Females)) { Margin = new Thickness(4), FontSize = 8 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.Total)) { Margin = new Thickness(4), FontSize = 8 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                    table.RowGroups[0].Rows.Add(row);
                }

                doc.Blocks.Add(table);

                DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                dlg.PrintDocument(paginator, "Nationalities");
            }

        }
    }
}
