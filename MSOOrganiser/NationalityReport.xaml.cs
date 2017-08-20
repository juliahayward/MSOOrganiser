using MSOCore.Reports;
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
        public NationalityReport()
        {
            InitializeComponent();
            Populate();

            dataGrid.DataContext = Items;
        }

        ObservableCollection<NationalityReportGenerator.NationalityVm> Items { get; set; }

        public void Populate()
        {
            var generator = new NationalityReportGenerator();

            Items = new ObservableCollection<NationalityReportGenerator.NationalityVm>();
            foreach (var item in generator.GetItemsForLatest())
                Items.Add(item);

            Items.Add(new NationalityReportGenerator.NationalityVm() {
                Nationality = "Total",
                NumberOfFemales = Items.Sum(x => x.NumberOfFemales),
                NumberOfMales = Items.Sum(x => x.NumberOfMales),
                Total = (Items.Sum(x => x.NumberOfFemales) + Items.Sum(x => x.NumberOfMales)).ToString()
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
