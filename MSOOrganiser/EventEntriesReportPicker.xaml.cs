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

namespace MSOOrganiser
{
    /// <summary>
    /// Interaction logic for EventEntriesReportPicker.xaml
    /// </summary>
    public partial class EventEntriesReportPicker : Window
    {
        public EventEntriesReportPicker()
        {
            InitializeComponent();
            DataContext = new EventEntriesReportPickerVm();
        }

        public EventEntriesReportPickerVm ViewModel
        {
            get { return (EventEntriesReportPickerVm)DataContext; }
        }

        public bool UseEvent { get { return ViewModel.UseEvent; } }
        public string EventCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.UseEvent)
            {
                EventCode = ViewModel.SelectedEventCode;
            }
            else
            {
                StartDate = ViewModel.SelectedFromDate;
                EndDate = ViewModel.SelectedToDate;
            }
            this.DialogResult = true;
            this.Close();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }

    public class EventEntriesReportPickerVm : VmBase
    {
        private bool _UseEvent;
        public bool UseEvent
        {
            get
            {
                return _UseEvent;
            }
            set
            {
                if (_UseEvent != value)
                {
                    _UseEvent = value;
                    OnPropertyChanged("UseEvent");
                    OnPropertyChanged("UseDates");
                }
            }
        }

        public bool UseDates
        {
            get
            {
                return !_UseEvent;
            }
            set
            {
                if ((!_UseEvent) != value)
                {
                    _UseEvent = !value;
                    OnPropertyChanged("UseEvent");
                    OnPropertyChanged("UseDates");
                }
            }
        }

        public ObservableCollection<EventVm> Events { get; set; }
        public ObservableCollection<DateVm> Dates { get; set; }

        public string SelectedEventCode { get; set; }
        public DateTime SelectedFromDate { get; set; }
        public DateTime SelectedToDate { get; set; }

        public EventEntriesReportPickerVm()
        {
            UseEvent = true;
            Events = new ObservableCollection<EventVm>();
            Dates = new ObservableCollection<DateVm>();

            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
            foreach (var evt in context.Events
                .Where(x => x.Code != null && x.Mind_Sport != null && x.OlympiadId == currentOlympiad.Id)
                .Select(x => new EventVm() { Id = x.EIN, Code = x.Code, Name = x.Mind_Sport })
                .ToList()
                .OrderBy(x => x.Code))
            {
                Events.Add(evt);
            }
            for (var date = currentOlympiad.StartDate.Value; date <= currentOlympiad.FinishDate.Value; date = date.AddDays(1))
            {
                Dates.Add(new DateVm() { Date = date });
            }

            SelectedEventCode = Events.First().Code;
            SelectedFromDate = Dates.First().Date;
            SelectedToDate = Dates.First().Date;
        }

        public class EventVm
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Text { get { return Code + " " + Name; } }
        }

        public class DateVm
        {
            public DateTime Date { get; set; }
            public string Text { get { return Date.ToString("dd MMM yyyy"); } }
        }
    }
}
