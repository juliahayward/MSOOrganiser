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
using MSOCore;

namespace MSOOrganiser
{
    /// <summary>
    /// Interaction logic for DisplayResultsPanel.xaml
    /// </summary>
    public partial class DisplayResultsPanel : UserControl
    {
        public DisplayResultsPanel()
        {
            InitializeComponent();
            DataContext = new DisplayResultsPanelVm();
        }
    }

    public class DisplayResultsPanelVm : VmBase
    {
        public class EventVm
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }

        public class EntrantVm
        {
            public int EntrantId { get; set; }
            public int ContestantId { get; set; }
            public string Medal { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int Rank { get; set; }
            public string Score { get; set; }
            public bool Absent { get; set; }
            public string TieBreak { get; set; }
            public string PentaScore { get; set; }
            public string TeamOrPair { get; set; }
            public string PIN { get; set; } // need this?
        }

        public DisplayResultsPanelVm()
        {
            Events = new ObservableCollection<EventVm>();
            Entrants = new ObservableCollection<EntrantVm>();
            EventId = "";

            PopulateDropdown();
            PopulateEntrants();
            _timer.Elapsed += timer_Elapsed;
            _timer.Interval = 20000;
            _timer.Enabled = true;
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                RotateEvent();
            });
        }

        private void RotateEvent()
        {
            var nextevent = Events.Where(x => x.Value.CompareTo(EventId) > 0).FirstOrDefault();
            if (nextevent != null)
            {
                EventId = nextevent.Value;
                EventName = nextevent.Text;
            }
            else
            {
                EventId = Events.First().Value;
                EventName = Events.First().Text;
            }
            PopulateEntrants();
        }

        #region bindable properties
        private Timer _timer = new Timer();
        public ObservableCollection<EventVm> Events { get; set; }
        public ObservableCollection<EntrantVm> Entrants { get; set; }
        public int CurrentOlympiadId { get; set; }
        public string EventId { get; set; }

        private string _eventName;
        public string EventName
        {
            get
            {
                return _eventName;
            }
            set
            {
                if (_eventName != value)
                {
                    _eventName = value;
                    OnPropertyChanged("EventName");
                }
            }
        }
        #endregion

        public void PopulateDropdown()
        {
            Events.Clear();
            var context = DataEntitiesProvider.Provide();
            CurrentOlympiadId = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First().Id;
            foreach (var e in context.Events.Where(x => !x.Code.StartsWith("ZZ") && x.OlympiadId == CurrentOlympiadId)
                .OrderBy(x => x.Code))
                Events.Add(new EventVm { Text = e.Code + " " + e.Mind_Sport, Value = e.Code });

            EventId = Events.First().Value;
            EventName = Events.First().Text;
        }

        public void PopulateEntrants()
        {
            Entrants.Clear();
            if (EventId == null) return;
            if (CurrentOlympiadId == null) return;

            var context = DataEntitiesProvider.Provide();
            var olympiadId = CurrentOlympiadId;
            var entrants = context.Entrants
                .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e = e, c = c })
                .Where(x => x.e.OlympiadId == olympiadId && x.e.Game_Code == EventId)
                .ToList()
                .OrderBy(x => MedalRank(x.e.Medal)).ThenBy(x => x.e.Rank)
                .ThenByDescending(x => x.e.Score).ThenByDescending(x => x.e.Tie_break).ToList();
            foreach (var e in entrants)
            {
                Entrants.Add(new EntrantVm()
                {
                    EntrantId = e.e.EntryNumber,
                    ContestantId = e.c.Mind_Sport_ID,
                    Medal = e.e.Medal ?? "",
                    FirstName = e.c.Firstname,
                    LastName = e.c.Lastname,
                    Rank = e.e.Rank.HasValue ? e.e.Rank.Value : 0,
                    Score = e.e.Score,
                    Absent = e.e.Absent,
                    TieBreak = e.e.Tie_break,
                    PentaScore = e.e.Penta_Score.HasValue ? e.e.Penta_Score.ToString() : "",
                    TeamOrPair = e.e.Partner,
                    PIN = e.e.PIN.HasValue ? e.e.PIN.Value.ToString() : ""
                });
            }

        }

        public int MedalRank(string medal)
        {
            if (medal == "Gold")
                return 1;
            if (medal == "Silver")
                return 2;
            if (medal == "Bronze")
                return 3;
            return 10;
        }
    }
}
