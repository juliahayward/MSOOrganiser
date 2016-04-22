using MSOCore;
using MSOCore.Models;
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
using System.Windows.Shapes;

namespace MSOOrganiser.Dialogs
{
    /// <summary>
    /// Interaction logic for AddSessionToEventDialog.xaml
    /// </summary>
    public partial class SelectDateDialog : Window
    {
        public DateTime SelectedDate { get { return ViewModel.SelectedDate.Date; } }

        public SelectDateDialog()
        {
            InitializeComponent();
            DataContext = new SelectDateVm();
        }

        public SelectDateVm ViewModel
        {
            get { return (SelectDateVm)DataContext; }
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }

    public class SelectDateVm : VmBase
    {
        public ObservableCollection<DateVm> Dates { get; set; }

        public DateTime SelectedDate { get; set; }

        public SelectDateVm()
        {
            Dates = new ObservableCollection<DateVm>();

            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();

            for (var date = currentOlympiad.StartDate.Value; date <= currentOlympiad.FinishDate.Value; date = date.AddDays(1))
            {
                Dates.Add(new DateVm() { Date = date });
            }
            SelectedDate = Dates.First().Date;
        }

        public class DateVm
        {
            public DateTime Date { get; set; }
            public string Text { get { return Date.ToString("dd MMM yyyy"); } }
        }
    }
}
