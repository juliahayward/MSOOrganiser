using System;
using System.Collections.Generic;
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
using System.Collections.ObjectModel;
using MSOCore;
using MSOCore.Models;

namespace MSOOrganiser
{
    /// <summary>
    /// Interaction logic for AddEventsToContestantWindow.xaml
    /// </summary>
    public partial class AddEventsToContestantWindow : Window
    {
        public IEnumerable<AddEventsToContestantWindowVm.EventVm> SelectedEvents { get; private set; }

        public AddEventsToContestantWindow(int olympiadId, bool isConcession, IEnumerable<string> selectedCodes, IEnumerable<string> nonEditableCodes)
        {
            InitializeComponent();
            DataContext = new AddEventsToContestantWindowVm(olympiadId, isConcession, selectedCodes, nonEditableCodes);
        }

        public AddEventsToContestantWindowVm ViewModel
        {
            get { return (AddEventsToContestantWindowVm)DataContext; }
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            var errors = ViewModel.Validate();
            if (errors.Any())
            {
                MessageBox.Show(string.Join(Environment.NewLine, errors));
               // return;
            }
            SelectedEvents = ViewModel.Events.ToList();
            this.DialogResult = true;
            this.Close();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedEvents = null;
            this.DialogResult = false;
            this.Close();
        }
    }

    public class AddEventsToContestantWindowVm : VmBase
    {
        public ObservableCollection<EventVm> Events { get; set; }

        private string _filterText = "";
        public string FilterText
        {
            get { return _filterText; }
            set
            {
                if (value != _filterText)
                {
                    _filterText = value;
                    FilterList();
                    OnPropertyChanged("FilterText");
                }
            }
        }

        public AddEventsToContestantWindowVm(int olympiadId, bool isConcession, IEnumerable<string> selectedEvents, IEnumerable<string> nonEditableCodes)
        {
            var context = DataEntitiesProvider.Provide();
            var allFees = context.Fees.ToList();
            var fees = (isConcession)
                ? allFees.ToDictionary(x => x.Code, x => x.Concession)
                : allFees.ToDictionary(x => x.Code, x => x.Adult);

            Events = new ObservableCollection<EventVm>();
            foreach (var evt in context.Events
                .Where(x => x.Code != null && x.Mind_Sport != null && x.OlympiadId == olympiadId)
                .Select(x => new EventVm() { Id = x.EIN, Code = x.Code, Name = x.Mind_Sport, FeeCode = x.Entry_Fee, 
                    IsIncludedInMaxFee = (x.incMaxFee.HasValue && x.incMaxFee.Value),
                    Event = x
                    })
                .ToList()
                .Distinct(new EventVmCodeOnlyComparer())
                .OrderBy(x => x.Code))
            {
                evt.IsSelected = (selectedEvents.Contains(evt.Code));
                evt.IsEnabled = !(nonEditableCodes.Contains(evt.Code));
                evt.Fee = (evt.FeeCode == null) ? 0.0m : (fees[evt.FeeCode] ?? 0.0m);
                Events.Add(evt);
            }
        }

        public IEnumerable<string> Validate()
        {
            var events = Events.Where(x => x.IsSelected).OrderBy(x => x.Start).ToArray();
            for (int i = 0; i < events.Count() - 1; i++)
            {
                if (events[i].End > events[i + 1].Start)
                    yield return string.Format("Events {0} and {1} clash on {2}", events[i].Code, events[i + 1].Code,
                        events[i].Start.ToString("ddd dd MMM"));
            }
        }

        private void FilterList()
        {
            foreach (var evt in Events)
            {
                evt.Visibility = (evt.Name.ToLower().Contains(_filterText.ToLower()))
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public class EventVm : VmBase
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Text { get { return Code + " " + Name; } }
            public string FeeCode { get; set; }
            public decimal Fee { get; set; }
            public bool IsIncludedInMaxFee { get; set; }
            public bool IsSelected { get; set; }
            public bool IsEnabled { get; set; }
            public Event Event { get; set; }
            public DateTime Start
            {
                get
                {
                    return Event.Event_Sess.First().Date.Value + Event.Event_Sess.First().Session1.StartTime.Value;
                }
            }
            public DateTime End
            {
                get
                {
                    return Event.Event_Sess.Last().Date.Value + Event.Event_Sess.Last().Session1.FinishTime.Value;
                }
            }
            private Visibility _Visibility = Visibility.Visible;
            public Visibility Visibility
            {
                get { return _Visibility; }
                set { if (value != _Visibility) { _Visibility = value; OnPropertyChanged("Visibility"); } }
            }
            public string ToolTip { get { if (IsEnabled) return null; else return "Events that you already have a score for cannot be deselected"; } }
        }

        public class EventVmCodeOnlyComparer : IEqualityComparer<EventVm>
        {
            public bool Equals(EventVm x, EventVm y)
            {
                return x.Code == y.Code && x.Name == y.Name;
            }

            public int GetHashCode(EventVm obj)
            {
                return obj.Text.GetHashCode();
            }
        }
    }

}
