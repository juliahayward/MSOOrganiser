using MSOOrganiser.Events;
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
    /// Interaction logic for ResultsPanel.xaml
    /// </summary>
    public partial class ResultsPanel : UserControl
    {
        public ResultsPanel()
        {
            InitializeComponent();
            DataContext = new ResultsPanelVm();
        }

        // A delegate type for hooking up change notifications.
        public delegate void ContestantEventHandler(object sender, ContestantEventArgs e);

        public event ContestantEventHandler ContestantSelected;

        public void Populate()
        {
            ((ResultsPanelVm)DataContext).PopulateDropdown();
        }

        public void Populate(string eventCode)
        {
            ((ResultsPanelVm)DataContext).PopulateDropdown(eventCode);
            ((ResultsPanelVm)DataContext).PopulateEntrants();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException("foo");
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException("foo");
        }

        private void eventCombo_Changed(object sender, SelectionChangedEventArgs e)
        {
            ((ResultsPanelVm)DataContext).PopulateEntrants();
        }

        private void person_Click(object sender, RoutedEventArgs e)
        {
            var entrant = ((FrameworkElement)sender).DataContext as ResultsPanelVm.EntrantVm;
            if (ContestantSelected != null)
            {
                var args = new ContestantEventArgs() { ContestantId = entrant.ContestantId };
                ContestantSelected(this, args);
            }
        }
    }


    public class ResultsPanelVm
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

        public ResultsPanelVm()
        {
            Events = new ObservableCollection<EventVm>();
            Entrants = new ObservableCollection<EntrantVm>();
            EventId = "";

            PopulateDropdown();
            PopulateEntrants();
        }

        #region bindable properties
        public ObservableCollection<EventVm> Events { get; set; }
        public ObservableCollection<EntrantVm> Entrants { get; set; }
        public int CurrentOlympiadId { get; set; }
        public string EventId { get; set; }

        #endregion

        public void PopulateDropdown(string eventCode = null)
        {
            Events.Clear();
            var context = new DataEntities();
            CurrentOlympiadId = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First().Id;
            foreach (var e in context.Events.Where(x => !x.Code.StartsWith("ZZ") && x.OlympiadId == CurrentOlympiadId)
                .OrderBy(x => x.Code))
                Events.Add(new EventVm { Text = e.Code + " " + e.Mind_Sport, Value = e.Code });

            if (eventCode == null)
                EventId = Events.First().Value;
            else
                EventId = eventCode;
        }

        public void PopulateEntrants()
        {
            Entrants.Clear();
            if (EventId == null) return;
            if (CurrentOlympiadId == null) return;

            var context = new DataEntities();
            var olympiadId = CurrentOlympiadId;
            var entrants = context.Entrants
                .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e = e, c = c })
                .Where(x => x.e.OlympiadId == olympiadId && x.e.Game_Code == EventId)
                .OrderBy(x => x.e.Rank)
                .ThenBy(x => x.c.Lastname).ToList();
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
                    Absent = e.e.Absent.HasValue ? e.e.Absent.Value : false,
                    TieBreak = e.e.Tie_break,
                    PentaScore = e.e.Penta_Score.HasValue ? e.e.Penta_Score.ToString() : "",
                    TeamOrPair = e.e.Partner,
                    PIN = e.e.PIN.HasValue ? e.e.PIN.Value.ToString() : ""
                });
            }

        }
    }
}
