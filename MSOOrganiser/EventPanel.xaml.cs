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
using MSOCore;
using MSOCore.Calculators;
using MSOCore.Models;

namespace MSOOrganiser
{
    /// <summary>
    /// Interaction logic for ResultsPanel.xaml
    /// </summary>
    public partial class EventPanel : UserControl
    {
        public EventPanel()
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
            ((ResultsPanelVm)DataContext).Populate();
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
            ((ResultsPanelVm)DataContext).Populate();
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

        private void calculatePenta_Click(object sender, RoutedEventArgs e)
        {
            var vm = (ResultsPanelVm)DataContext;
            var calculator = new Penta2015Calculator();
            try
            {
                calculator.Calculate(vm.NumberInTeam, vm.Entrants);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void checkRanks_Click(object sender, RoutedEventArgs e)
        {
            var vm = (ResultsPanelVm)DataContext;
            var checker = new RankChecker();
            try
            {
                checker.Check(vm.NumberInTeam, vm.Entrants);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void calculatePenta10_Click(object sender, RoutedEventArgs e)
        {
            var vm = (ResultsPanelVm)DataContext;
            var calculator = new Penta2010Calculator();
            try
            {
                calculator.Calculate(vm.NumberInTeam, vm.Entrants);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }


    public class ResultsPanelVm : VmBase
    {
        // For dropdown
        public class EventVm
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }

        public class TypeVm
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }

        public class EntryFeeVm
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }

        // for table
        public class SessionVm : VmBase
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public string SessionCode { get; set; }
        }

        // for table
        public class EntrantVm : VmBase, IPentaCalculable
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
            private string _pentaScore;
            public string PentaScore
            {
                get { return _pentaScore; }
                set
                {
                    if (_pentaScore != value)
                    {
                        _pentaScore = value;
                        OnPropertyChanged("PentaScore");
                    }
                }
            }
            public string TeamOrPair { get; set; }
            public string PIN { get; set; } // need this?
        }

        public ResultsPanelVm()
        {
            Events = new ObservableCollection<EventVm>();
            Fees = new ObservableCollection<EntryFeeVm>();
            Entrants = new ObservableCollection<EntrantVm>();
            Sessions = new ObservableCollection<SessionVm>();
            Types = new ObservableCollection<TypeVm>();
            EventCode = "";

            PopulateDropdown();
            Populate();
        }

        #region bindable properties
        public ObservableCollection<TypeVm> Types { get; set; }
        public ObservableCollection<EntryFeeVm> Fees { get; set; }
        public ObservableCollection<EventVm> Events { get; set; }
        public ObservableCollection<EntrantVm> Entrants { get; set; }
        public ObservableCollection<SessionVm> Sessions { get; set; }
        public int CurrentOlympiadId { get; set; }
        public string EventCode { get; set; }
        public int EventId { get; set; }        // EIN in database

        private bool _IsDirty;
        public bool IsDirty
        {
            get
            {
                return _IsDirty;
            }
            set
            {
                if (_IsDirty != value)
                {
                    _IsDirty = value;
                    OnPropertyChanged("IsDirty");
                }
            }
        }

        private string _arbiter;
        public string Arbiter
        {
            get
            {
                return _arbiter;
            }
            set
            {
                if (_arbiter != value)
                {
                    _arbiter = value;
                    _IsDirty = true;
                    OnPropertyChanged("Arbiter");
                }
            }
        }

        private string _entryFee;
        public string EntryFee
        {
            get
            {
                return _entryFee;
            }
            set
            {
                if (_entryFee != value)
                {
                    _entryFee = value;
                    _IsDirty = true;
                    OnPropertyChanged("EntryFee");
                }
            }
        }

        private string _location;
        public string Location
        {
            get
            {
                return _location;
            }
            set
            {
                if (_location != value)
                {
                    _location = value;
                    _IsDirty = true;
                    OnPropertyChanged("Location");
                }
            }
        }

        private decimal _prizeFund;
        public decimal PrizeFund
        {
            get
            {
                return _prizeFund;
            }
            set
            {
                if (_prizeFund != value)
                {
                    _prizeFund = value;
                    _IsDirty = true;
                    OnPropertyChanged("PrizeFund");
                }
            }
        }

        private decimal _prize1;
        public decimal Prize1
        {
            get
            {
                return _prize1;
            }
            set
            {
                if (_prize1 != value)
                {
                    _prize1 = value;
                    _IsDirty = true;
                    OnPropertyChanged("Prize1");
                }
            }
        }

        private decimal _prize2;
        public decimal Prize2
        {
            get
            {
                return _prize2;
            }
            set
            {
                if (_prize2 != value)
                {
                    _prize2 = value;
                    _IsDirty = true;
                    OnPropertyChanged("Prize2");
                }
            }
        }

        private decimal _prize3;
        public decimal Prize3
        {
            get
            {
                return _prize3;
            }
            set
            {
                if (_prize3 != value)
                {
                    _prize3 = value;
                    _IsDirty = true;
                    OnPropertyChanged("Prize3");
                }
            }
        }

        private decimal _juniorPrize1;
        public decimal JuniorPrize1
        {
            get
            {
                return _juniorPrize1;
            }
            set
            {
                if (_juniorPrize1 != value)
                {
                    _juniorPrize1 = value;
                    _IsDirty = true;
                    OnPropertyChanged("JuniorPrize1");
                }
            }
        }

        private decimal _juniorPrize2;
        public decimal JuniorPrize2
        {
            get
            {
                return _juniorPrize2;
            }
            set
            {
                if (_juniorPrize2 != value)
                {
                    _juniorPrize2 = value;
                    _IsDirty = true;
                    OnPropertyChanged("JuniorPrize2");
                }
            }
        }

        private string _otherPrizes;
        public string OtherPrizes
        {
            get
            {
                return _otherPrizes;
            }
            set
            {
                if (_otherPrizes != value)
                {
                    _otherPrizes = value;
                    _IsDirty = true;
                    OnPropertyChanged("OtherPrizes");
                }
            }
        }

        private string _juniorOtherPrizes;
        public string JuniorOtherPrizes
        {
            get
            {
                return _juniorOtherPrizes;
            }
            set
            {
                if (_juniorOtherPrizes != value)
                {
                    _juniorOtherPrizes = value;
                    _IsDirty = true;
                    OnPropertyChanged("JuniorOtherPrizes");
                }
            }
        }

        private decimal _juniorPrize3;
        public decimal JuniorPrize3
        {
            get
            {
                return _juniorPrize3;
            }
            set
            {
                if (_juniorPrize3 != value)
                {
                    _juniorPrize3 = value;
                    _IsDirty = true;
                    OnPropertyChanged("JuniorPrize3");
                }
            }
        }

        private int _numberInTeam;
        public int NumberInTeam
        {
            get
            {
                return _numberInTeam;
            }
            set
            {
                if (_numberInTeam != value)
                {
                    _numberInTeam = value;
                    _IsDirty = true;
                    OnPropertyChanged("NumberInTeam");
                }
            }
        }

        private bool _pentamind;
        public bool Pentamind
        {
            get
            {
                return _pentamind;
            }
            set
            {
                if (_pentamind != value)
                {
                    _pentamind = value;
                    _IsDirty = true;
                    OnPropertyChanged("Pentamind");
                }
            }
        }

        private bool _includedInMaxFee;
        public bool IncludedInMaxFee
        {
            get
            {
                return _includedInMaxFee;
            }
            set
            {
                if (_includedInMaxFee != value)
                {
                    _includedInMaxFee = value;
                    _IsDirty = true;
                    OnPropertyChanged("IncludedInMaxFee");
                }
            }
        }

        private bool _juniorMedals;
        public bool JuniorMedals
        {
            get
            {
                return _juniorMedals;
            }
            set
            {
                if (_juniorMedals != value)
                {
                    _juniorMedals = value;
                    _IsDirty = true;
                    OnPropertyChanged("JuniorMedals");
                }
            }
        }

        private string _type;
        public string Type
        {
            get
            {
                return _type;
            }
            set
            {
                if (_type != value)
                {
                    _type = value;
                    _IsDirty = true;
                    OnPropertyChanged("Type");
                }
            }
        }

        private int _expectedNumber;
        public int ExpectedNumber
        {
            get
            {
                return _expectedNumber;
            }
            set
            {
                if (_expectedNumber != value)
                {
                    _expectedNumber = value;
                    _IsDirty = true;
                    OnPropertyChanged("ExpectedNumber");
                }
            }
        }

        private string _notes;
        public string Notes
        {
            get
            {
                return _notes;
            }
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    _IsDirty = true;
                    OnPropertyChanged("Notes");
                }
            }
        }

        private int _numSessions;
        public int NumSessions
        {
            get
            {
                return _numSessions;
            }
            set
            {
                if (_numSessions != value)
                {
                    _numSessions = value;
                    _IsDirty = true;
                    OnPropertyChanged("NumSessions");
                }
            }
        }

        private string _sessions2001;
        public string Sessions2001
        {
            get
            {
                return _sessions2001;
            }
            set
            {
                if (_sessions2001 != value)
                {
                    _sessions2001 = value;
                    _IsDirty = true;
                    OnPropertyChanged("Sessions2001");
                }
            }
        }

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
                EventCode = Events.First().Value;
            else
                EventCode = eventCode;

            Types.Clear();
            Types.Add(new TypeVm() { Value = null, Text = "(normal)" });
            Types.Add(new TypeVm() { Value = "Beginners'", Text = "Beginners'" });

            Fees.Clear();
            Fees.Add(new EntryFeeVm() { Value = null, Text = "(none)" });
            foreach (var f in context.Fees.OrderBy(x => x.Code))
                Fees.Add(new EntryFeeVm() { Value = f.Code, Text = f.DropdownText });
        }

        public void Populate()
        {
            Entrants.Clear();
            if (EventCode == null) return;
            if (CurrentOlympiadId == null) return;

            var context = new DataEntities();
            
            var olympiadId = CurrentOlympiadId;
            var evt = context.Events.First(x => x.OlympiadId == olympiadId && x.Code == EventCode);

            EventId = evt.EIN;
            Arbiter = evt.Arbiter;
            Location = evt.Location;
            EntryFee = evt.Entry_Fee; // nulls ok
            NumberInTeam = evt.Number_in_Team.HasValue ? evt.Number_in_Team.Value : 1;
            PrizeFund = (evt.Prize_fund.HasValue) ? evt.Prize_fund.Value : 0;
            Prize1 = (evt.C1st_Prize != null) ? decimal.Parse(evt.C1st_Prize) : 0;
            Prize2 = (evt.C2nd_Prize != null) ? decimal.Parse(evt.C2nd_Prize) : 0;
            Prize3 = (evt.C3rd_Prize != null) ? decimal.Parse(evt.C3rd_Prize) : 0;
            JuniorPrize1 = (evt.JNR_1st_Prize != null) ? decimal.Parse(evt.JNR_1st_Prize) : 0;
            JuniorPrize2 = (evt.JNR_2nd_Prize != null) ? decimal.Parse(evt.JNR_2nd_Prize) : 0;
            JuniorPrize3 = (evt.JNR_3rd_Prize != null) ? decimal.Parse(evt.JNR_3rd_Prize) : 0;
            OtherPrizes = evt.Other_Prizes;
            JuniorOtherPrizes = evt.JNR_Other_Prizes;
            Pentamind = (evt.Pentamind.HasValue) ? evt.Pentamind.Value : false;
            IncludedInMaxFee = (evt.incMaxFee.HasValue) ? evt.incMaxFee.Value : false;
            JuniorMedals = (evt.JNR_Medals.HasValue) ? evt.JNR_Medals.Value : false;
            Type = evt.Type;    // NULLs OK
            ExpectedNumber = (evt.X_Num.HasValue) ? evt.X_Num.Value : 10;
            Notes = evt.Notes;
            NumSessions = (evt.No_Sessions.HasValue) ? (int)evt.No_Sessions : 0;
            Sessions2001 = evt.Sessions;    // 2001 format - dates embedded in string
            // evt.Display; // appears not to be used
            var h = evt.Event_Sess; // collection
            // evt.MAX_Number // not used since 2006
            var t = evt.Number; // within that year, 0 = combined event or not a game
            // evt.Prize_Giving; - has never been used, always NULL

            Sessions.Clear();
            foreach (var s in evt.Event_Sess)
                Sessions.Add(new SessionVm() { Id = s.INDEX, Date = s.Date.Value, SessionCode = s.Session });

            var entrants = context.Entrants
                .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e = e, c = c })
                .Where(x => x.e.OlympiadId == olympiadId && x.e.Game_Code == EventCode)
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
