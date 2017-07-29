using MSOCore;
using MSOCore.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
    /// Interaction logic for SummaryPanel.xaml
    /// </summary>
    public partial class SummaryPanel : UserControl
    {
        private Timer _timer = new Timer();

        public SummaryPanel()
        {
            InitializeComponent();

            DataContext = new SummaryPanelVm();

            _timer.Interval = 1000 * 60 * 5;
            _timer.Elapsed += _timer_Elapsed;

            MainWindow.UserLoggedIn += MainWindow_UserLoggedIn;
        }

        public void _timer_Elapsed(object sender, EventArgs e)
        {
            // Needs to be in the UI thread
            Dispatcher.Invoke(() => ViewModel.Populate());
        }

        void MainWindow_UserLoggedIn(object sender, EventArgs e)
        {
            ViewModel.Populate();
            _timer.Enabled = true;
        }

        public SummaryPanelVm ViewModel
        {
            get { return DataContext as SummaryPanelVm; }
        }
    }

    public class SummaryPanelVm : VmBase
    {
        public class EventVm
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Dates { get; set; }
            public string NumCompetitors { get; set; }
            public string DateAndCompetitors
            {
                get
                {
                    return Dates + "     " + NumCompetitors + " entries";
                }
            }
            public double FractionDone { get; set; }
            public string Status { get; set; }
        }

        private double _progressValue = 0.0;
        public double ProgressValue
        {
            get
            { return _progressValue; }
            set
            {
                if (value != _progressValue)
                {
                    _progressValue = value;
                    OnPropertyChanged("ProgressValue");
                }
            }
        }

        private double _progressMaximum = 1.0;
        public double ProgressMaximum
        {
            get { return _progressMaximum; }
            set
            {
                if (value != _progressMaximum)
                {
                    _progressMaximum = value;
                    OnPropertyChanged("ProgressMaximum");
                }
            }
        }
        public ObservableCollection<EventVm> Events { get; set; }

        public SummaryPanelVm()
        {
            Events = new ObservableCollection<EventVm>();
        }

        public void Populate()
        {
            Events.Clear();

            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.First(x => x.Current);
            var events = context.Events.Where(x => x.OlympiadId == currentOlympiad.Id && x.Number > 0);
            foreach (var evt in events.OrderBy(x => x.Number).ToList())
            {
                Events.Add(new EventVm() 
                { 
                    Name = evt.ShortName(), 
                    Id = evt.Number,
                    Dates = (evt.Start.HasValue) ? evt.Start.Value.ToString("ddd dd MMM HH:mm") + " - " + 
                        evt.End.Value.ToString("HH:mm")
                        : "Dates unknown",
                    NumCompetitors = evt.Entrants.Count(x => !x.Absent).ToString(),
                    FractionDone = evt.FractionDone(),
                    Status = evt.Status()
                });
            }

            ProgressMaximum = Events.Count();
            ProgressValue = Events.Sum(x => x.FractionDone); 
        }
    }
}
