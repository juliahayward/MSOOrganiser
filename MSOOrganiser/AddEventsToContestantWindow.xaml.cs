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
                .Select(x => new EventVm() { Id = x.EIN, Code = x.Code, Name = x.Mind_Sport, FeeCode = x.Entry_Fee })
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

        public class EventVm
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Text { get { return Code + " " + Name; } }
            public string FeeCode { get; set; }
            public decimal Fee { get; set; }
            public bool IsSelected { get; set; }
            public bool IsEnabled { get; set; }
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
