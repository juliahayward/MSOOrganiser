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
    public partial class AddSessionToEventDialog : Window
    {
        public IEnumerable<AddSessionToEventVm.SessionVm> SelectedSessions
        {
            get
            {
                return ViewModel.Sessions.ToList();
            }
        }
        public DateTime SelectedDate { get { return ViewModel.SelectedDate.Date; } }

        public AddSessionToEventDialog(IEnumerable<string> selectedSessions)
        {
            InitializeComponent();
            DataContext = new AddSessionToEventVm(selectedSessions);
        }

        public AddSessionToEventVm ViewModel
        {
            get { return (AddSessionToEventVm)DataContext; }
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

    public class AddSessionToEventVm : VmBase
    {
        public ObservableCollection<SessionVm> Sessions { get; set; }
        public ObservableCollection<DateVm> Dates { get; set; }

        public DateTime SelectedDate { get; set; }
        

        public AddSessionToEventVm(IEnumerable<string> selectedSessions)
        {
            Sessions = new ObservableCollection<SessionVm>();
            Dates = new ObservableCollection<DateVm>();

            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();

            for (var date = currentOlympiad.StartDate.Value; date <= currentOlympiad.FinishDate.Value; date = date.AddDays(1))
            {
                Dates.Add(new DateVm() { Date = date });
            }
            SelectedDate = Dates.First().Date;

            foreach (var s in context.Sessions
                .Where(x => x.IsActive)
                .Select(x => new SessionVm() { Code = x.Session1, Start = x.StartTime.Value, End = x.FinishTime.Value,
                 Worth = (int)x.Worth.Value })
                .ToList()
                .OrderBy(x => x.Start))
            {
                s.IsSelected = (selectedSessions.Contains(s.Code));
                Sessions.Add(s);
            }
        }

        public class SessionVm
        {
            public string Code { get; set; }
            public TimeSpan Start { get; set; }
            public TimeSpan End { get; set; }
            public int Worth { get; set; }
            public string Text { get { return Start.ToString(@"hh\:mm") + " - " + End.ToString(@"hh\:mm"); } }
            public bool IsSelected { get; set; }
        }

        public class DateVm
        {
            public DateTime Date { get; set; }
            public string Text { get { return Date.ToString("dd MMM yyyy"); } }
        }
    }
}
