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
    /// Interaction logic for AddEventToOlympiadWindow.xaml
    /// </summary>
    public partial class AddEventToOlympiadWindow : Window
    {
        public AddEventToOlympiadWindowVm.EventVm SelectedEvent { get; private set; }

        public AddEventToOlympiadWindow()
        {
            InitializeComponent();
            DataContext = new AddEventToOlympiadWindowVm();
        }

        public AddEventToOlympiadWindowVm ViewModel
        {
            get { return (AddEventToOlympiadWindowVm)DataContext; }
        }

        private void addEvent_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.UsePastEvent)
            {
                if (ViewModel.SelectedPastEventId != 0)
                    SelectedEvent = ViewModel.PastEvents.First(x => x.Id == ViewModel.SelectedPastEventId);
                else
                {
                    MessageBox.Show("Please select a past event");
                    return;
                }
            }
            else
            {
                this.SelectedEvent = new AddEventToOlympiadWindowVm.EventVm() {  Id = 0, Code = ViewModel.NewCode, Name = ViewModel.NewName};
            }
            this.DialogResult = true;
            this.Close();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedEvent = null;
            this.DialogResult = false;
            this.Close();
        }
    }

    public class AddEventToOlympiadWindowVm : VmBase
    {
        private bool _UsePastEvent;
        public bool UsePastEvent
        {
            get
            {
                return _UsePastEvent;
            }
            set
            {
                if (_UsePastEvent != value)
                {
                    _UsePastEvent = value;
                    OnPropertyChanged("UsePastEvent");
                    OnPropertyChanged("UseNewEvent");
                }
            }
        }

        public bool UseNewEvent
        {
            get
            {
                return !_UsePastEvent;
            }
            set
            {
                if ((!_UsePastEvent) != value)
                {
                    _UsePastEvent = !value;
                    OnPropertyChanged("UsePastEvent");
                    OnPropertyChanged("UseNewEvent");
                }
            }
        }

        public ObservableCollection<EventVm> PastEvents { get; set; }
        public int SelectedPastEventId { get; set; }

        public string NewCode { get; set; }
        public string NewName { get; set; }

        public AddEventToOlympiadWindowVm()
        {
            UsePastEvent = true;
            PastEvents = new ObservableCollection<EventVm>();
            var context = DataEntitiesProvider.Provide();
            foreach (var evt in context.Events
                .Where(x => x.Code != null && x.Mind_Sport != null)
                .Select(x => new EventVm() { Id = x.EIN, Code = x.Code, Name = x.Mind_Sport })
                .ToList()
                .Distinct(new EventVmCodeOnlyComparer())
                .OrderBy(x => x.Code))
            {
                PastEvents.Add(evt);
            }
        }

        public class EventVm
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Text { get { return Code + " " + Name; } }
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
