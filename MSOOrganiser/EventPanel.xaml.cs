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
using MSOOrganiser.Dialogs;
using MSOCore.Extensions;
using MSOOrganiser.UIUtilities;
using JuliaHayward.Common.Environment;
using System.Configuration;
using JuliaHayward.Common.Logging;
using System.Data.Entity.Validation;
using MSOOrganiser.Reports;

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

            if (!JuliaEnvironment.CurrentEnvironment.IsDebug())
            {
                dataGrid2.Columns[0].Visibility = Visibility.Collapsed;
                dataGrid.Columns[0].Visibility = Visibility.Collapsed;
                dataGrid.Columns[1].Visibility = Visibility.Collapsed;
            }
        }

        public ResultsPanelVm ViewModel
        {
            get { return DataContext as ResultsPanelVm; }
        }

        // A delegate type for hooking up change notifications.
        public delegate void ContestantEventHandler(object sender, ContestantEventArgs e);

        public event ContestantEventHandler ContestantSelected;

        public void Populate()
        {
            ViewModel.PopulateDropdown();
        }

        public void Populate(string eventCode, int olympiadId)
        {
            ViewModel.PopulateDropdown(eventCode, olympiadId);
            ViewModel.Populate();
            SetPairsColumnVisibility();
        }
          
        private void SetPairsColumnVisibility()
        {
            if (ViewModel.NumberInTeam == 1)
            {
                dataGrid.Columns[12].Visibility = Visibility.Collapsed; // text box
                dataGrid.Columns[13].Visibility = Visibility.Collapsed; // dropdown
            }
            else if (ViewModel.NumberInTeam == 2)
            {
                dataGrid.Columns[12].Visibility = Visibility.Collapsed;
                dataGrid.Columns[13].Visibility = Visibility.Visible;
            }
            else
            {
                dataGrid.Columns[12].Visibility = Visibility.Visible;
                dataGrid.Columns[13].Visibility = Visibility.Collapsed;
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Populate();
            SetPairsColumnVisibility();
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            // TODO DI the relevant bits into vm and call methods there
            calculateRanks_Click(sender, e);
            calculatePenta_Click(sender, e);

            try
            {
                if (ViewModel.EditingThePast)
                {
                    if (MessageBox.Show("You are editing data for a past Olympiad. Are you sure this is right?",
                        "MSOOrganiser", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No)
                        == MessageBoxResult.No) return;
                }
                var errors = ViewModel.Validate();
                if (!errors.Any())
                {
                    using (new SpinnyCursor())
                    {
                        ViewModel.Save();
                    }
                }
                else
                {
                    errors.Insert(0, "Could not save:");
                    MessageBox.Show(string.Join(Environment.NewLine, errors));
                }
            }
            catch (Exception ex)
            {
                string message = (ex is DbEntityValidationException)
                    ? ((DbEntityValidationException)ex).EntityValidationErrors.First().ValidationErrors.First().ErrorMessage
                    : ex.Message;

                MessageBox.Show("Something went wrong  - data not saved (" + message + ")");

                var trelloKey = ConfigurationManager.AppSettings["TrelloKey"];
                var trelloAuthKey = ConfigurationManager.AppSettings["TrelloAuthKey"];
                var logger = new TrelloLogger(trelloKey, trelloAuthKey);
                logger.Error("MSOWeb", message, ex.StackTrace);
            }
        }

        public void print_Click(object sender, EventArgs e)
        {
            var printer = new SingleEventResultsPrinter(ViewModel.CurrentOlympiadId, ViewModel.EventCode);
            printer.Print();
        }

        private void eventCombo_Changed(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.Populate();
            SetPairsColumnVisibility();
        }

        private void olympiadCombo_Changed(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.Populate();
            SetPairsColumnVisibility();
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
            var calculator = new Penta2015Calculator();
            try
            {
                calculator.Calculate(ViewModel.NumberInTeam, ViewModel.Entrants, ViewModel.Pentamind);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void calculateRanks_Click(object sender, RoutedEventArgs args)
        {
            var checker = new RankCalculator();
            try
            {
                checker.Calculate(ViewModel.NumberInTeam, ViewModel.HighScoresAreBest, ViewModel.Entrants);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void checkRanks_Click(object sender, RoutedEventArgs e)
        {
            var checker = new RankChecker();
            try
            {
                checker.Check(ViewModel.NumberInTeam, ViewModel.Entrants);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void addSession_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddSessionToEventDialog(ViewModel.Sessions.Select(x => x.SessionCode));
            if (dialog.ShowDialog().Value)
            {
                foreach (var s in dialog.SelectedSessions)
                {
                    if (s.IsSelected)
                    {
                        var existingSession = ViewModel.Sessions.FirstOrDefault(x => x.SessionCode == s.Code);
                        if (existingSession == null)
                        {
                            ViewModel.Sessions.Add(new ResultsPanelVm.SessionVm() 
                                { SessionCode = s.Code, Date = dialog.SelectedDate, Id = 0, 
                                    Start = s.Start.ToStandardString(), End = s.End.ToStandardString(),
                                Worth = s.Worth});
                            ViewModel.IsDirty = true;
                        }
                    }
                    else
                    {
                        var sessionToDelete = ViewModel.Sessions.FirstOrDefault(x => x.SessionCode == s.Code);
                        if (sessionToDelete != null)
                        {
                            ViewModel.Sessions.Remove(sessionToDelete);
                            ViewModel.IsDirty = true;
                        }
                    }
                }
                ViewModel.NumSessions = ViewModel.Sessions.Sum(x => x.Worth);
            }
        }

        private void deleteSession_Click(object sender, RoutedEventArgs e)
        {
            var sessionToDelete = ((FrameworkElement)sender).DataContext as ResultsPanelVm.SessionVm;
            ViewModel.Sessions.Remove(sessionToDelete);
            ViewModel.IsDirty = true;
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

        public class OlympiadVm
        {
            public string Text { get; set; }
            public int Id { get; set; }
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

        public class LocationVm
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }

        public class ScoreModeVm
        {
            public string Text { get; set; }
            public bool Value { get; set; }
        }

        public class PartnerVm
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
            public string Start { get; set; }
            public string End { get; set; }
            public int Worth { get; set; }
        }

        // for table
        public class EntrantVm : VmBase, IPentaCalculable
        {
            private ResultsPanelVm _parent;

            public EntrantVm(ResultsPanelVm parent)
            {
                _parent = parent;
            }

            public int EntrantId { get; set; }
            public int ContestantId { get; set; }
            public string Medal { get; set; }
            public int MedalSortValue { get { return (string.IsNullOrEmpty(Medal)) ? 100 : (int)Enum.Parse(typeof(Medals), Medal); } }
            public string JuniorMedal { get; set; }
            public int JuniorMedalSortValue { get { return (string.IsNullOrEmpty(JuniorMedal)) ? 100 : (int)Enum.Parse(typeof(Medals), JuniorMedal); } }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FullName { get { return FirstName + " " + LastName; } }
            public bool IsJunior { get; set; }
            public string Junior { get { return IsJunior ? "JNR" : "";  } }
            private int _rank;
            public int Rank 
            {
                get { return _rank; }
                set
                {
                    if (_rank != value)
                    {
                        _rank = value;
                        OnPropertyChanged("Rank");
                    }
                }
            }
            public string Score { get; set; }
            public bool Absent { get; set; }
            public string TieBreak { get; set; }
            private float _pentaScore;
            public float PentaScore
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

            public ObservableCollection<string> Medals { get { return _parent.Medals; } }
            public ObservableCollection<string> JuniorMedals { get { return _parent.JuniorMedals1; } }
            public ObservableCollection<string> Partners { get { return _parent.Partners; } }
        }

        public ResultsPanelVm()
        {
            Events = new ObservableCollection<EventVm>();
            Fees = new ObservableCollection<EntryFeeVm>();
            Entrants = new ObservableCollection<EntrantVm>();
            Sessions = new ObservableCollection<SessionVm>();
            Types = new ObservableCollection<TypeVm>();
            Locations = new ObservableCollection<LocationVm>();
            Olympiads = new ObservableCollection<OlympiadVm>();
            Partners = new ObservableCollection<string>();
            ScoreModes = new ObservableCollection<ScoreModeVm>();

            Medals = StandardMedalsList();
            JuniorMedals1 = JuniorMedalsList();
            EventCode = "";

            PopulateScoreModes();
            PopulateDropdown();
            Populate();
        }

        #region bindable properties
        public ObservableCollection<ScoreModeVm> ScoreModes { get; set; }
        public ObservableCollection<string> Medals { get; set; }
        public ObservableCollection<string> JuniorMedals1 { get; set; } 
        // TODO Other JuniorMedals should be HasJuniorMedals
        public ObservableCollection<TypeVm> Types { get; set; }
        public ObservableCollection<EntryFeeVm> Fees { get; set; }
        public ObservableCollection<EventVm> Events { get; set; }
        public ObservableCollection<EntrantVm> Entrants { get; set; }
        public ObservableCollection<SessionVm> Sessions { get; set; }
        public ObservableCollection<LocationVm> Locations { get; set; }
        public ObservableCollection<OlympiadVm> Olympiads { get; set; }
        public ObservableCollection<string> Partners { get; set; }
        public int CurrentOlympiadId { get; set; }
        public bool EditingThePast { get; set; }
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

        private bool _highScoresAreBest = true;
        public bool HighScoresAreBest
        {
            get
            {
                return _highScoresAreBest;
            }
            set
            {
                if (_highScoresAreBest != value)
                {
                    _highScoresAreBest = value;
                    _IsDirty = true;
                    OnPropertyChanged("HighScoresAreBest");
                }
            }
        }

        private bool _highTieBreaksAreBest = true;
        public bool HighTieBreaksAreBest
        {
            get
            {
                return _highTieBreaksAreBest;
            }
            set
            {
                if (_highTieBreaksAreBest != value)
                {
                    _highTieBreaksAreBest = value;
                    _IsDirty = true;
                    OnPropertyChanged("HighTieBreaksAreBest");
                }
            }
        }

        #endregion

        private void PopulateScoreModes()
        {
            ScoreModes.Clear();
            ScoreModes.Add(new ScoreModeVm() { Text = "Highest", Value = true });
            ScoreModes.Add(new ScoreModeVm() { Text = "Lowest", Value = false });
        }

        private ObservableCollection<string> StandardMedalsList()
        {
            var list = new ObservableCollection<string>();
            list.Add("");
            list.Add("Gold");
            list.Add("Silver");
            list.Add("Bronze");
            return list;
        }

        private ObservableCollection<string> JuniorMedalsList()
        {
            var list = new ObservableCollection<string>();
            list.Add("");
            list.Add("Gold JNR");
            list.Add("Silver JNR");
            list.Add("Bronze JNR");
            return list;
        }



        public void PopulateDropdown(string eventCode = null, int olympiadId = -1)
        {
            Events.Clear();
            var context = DataEntitiesProvider.Provide();

            Olympiad_Info currentOlympiad;
            if (olympiadId < 1)
            {
                currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
            }
            else
            {
                currentOlympiad = context.Olympiad_Infoes.First(x => x.Id == olympiadId);
            }
            CurrentOlympiadId = currentOlympiad.Id;

            foreach (var e in currentOlympiad.Events.Where(x => !x.Code.StartsWith("ZZ"))
                .OrderBy(x => x.Code))
                Events.Add(new EventVm { Text = e.Code + " " + e.Mind_Sport, Value = e.Code });

            if (eventCode == null)
                EventCode = (Events.Any()) ? Events.First().Value : null;
            else
                EventCode = eventCode;

            Types.Clear();
            Types.Add(new TypeVm() { Value = null, Text = "(normal)" });
            Types.Add(new TypeVm() { Value = "Beginners'", Text = "Beginners'" });

            Fees.Clear();
            Fees.Add(new EntryFeeVm() { Value = null, Text = "(none)" });
            foreach (var f in context.Fees.OrderBy(x => x.Code))
                Fees.Add(new EntryFeeVm() { Value = f.Code, Text = f.DropdownText });

            Locations.Clear();
            Locations.Add(new LocationVm() { Value = null, Text = "(no location)" });
            foreach (var l in currentOlympiad.Locations.OrderBy(x => x.Location1))
                Locations.Add(new LocationVm() { Value = l.Location1, Text = l.Location1 });

            Olympiads.Clear();
            foreach (var o in context.Olympiad_Infoes.OrderByDescending(x => x.StartDate))
                Olympiads.Add(new OlympiadVm { Text = o.FullTitle(), Id = o.Id });
        }

        public void Populate()
        {
            Entrants.Clear();
            if (EventCode == null) return;
            
            var context = DataEntitiesProvider.Provide();
            
            var olympiadId = CurrentOlympiadId;
            var currentOlympiad = context.Olympiad_Infoes.First(x => x.Id == CurrentOlympiadId);
            EditingThePast = !currentOlympiad.Current;
            var evt = context.Events.FirstOrDefault(x => x.OlympiadId == olympiadId && x.Code == EventCode);
            if (evt == null)
            {
                MessageBox.Show("Event " + EventCode + " didn't occur in " + currentOlympiad.YearOf);
                return;
            }

            EventId = evt.EIN;
            Arbiter = string.Join(", ", evt.Arbiters.Select(a => a.Name.FullName()));
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
            // evt.Display; // appears not to be used
            var h = evt.Event_Sess; // collection
            // evt.MAX_Number // not used since 2006
            var t = evt.Number; // within that year, 0 = combined event or not a game
            // evt.Prize_Giving; - has never been used, always NULL

            Sessions.Clear();
            foreach (var s in evt.Event_Sess)
                Sessions.Add(new SessionVm() { Id = s.INDEX, Date = s.Date.Value, SessionCode = s.Session,
                    Start = s.Session1.StartTime.Value.ToString(@"hh\:mm"),
                    End = s.Session1.FinishTime.Value.ToString(@"hh\:mm"),
                    // Todo purge the nulls out of the database (needs a whole rethink anyway)
                Worth = (s.Session1.Worth.HasValue) ? (int)s.Session1.Worth.Value : 0});

            var entrants = context.Entrants
                .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e = e, c = c })
                .Where(x => x.e.OlympiadId == olympiadId && x.e.Game_Code == EventCode)
                .OrderBy(x => x.e.Rank)
                .ThenBy(x => x.c.Lastname).ToList();

            Partners.Clear();
            foreach (var ec in entrants.OrderBy(ec => ec.c.FullName()))
                Partners.Add(ec.c.FullName());


            var juniorDate = currentOlympiad.AgeDate.Value.AddYears(-currentOlympiad.JnrAge.Value - 1);

            foreach (var e in entrants)
            {
                Entrants.Add(new EntrantVm(this)
                {
                    EntrantId = e.e.EntryNumber,
                    ContestantId = e.c.Mind_Sport_ID,
                    Medal = e.e.Medal ?? "",
                    JuniorMedal = e.e.JuniorMedal ?? "",
                    FirstName = e.c.Firstname,
                    LastName = e.c.Lastname,
                    IsJunior = e.c.DateofBirth.HasValue && e.c.DateofBirth > juniorDate, 
                    Rank = e.e.Rank.HasValue ? e.e.Rank.Value : 0,
                    Score = e.e.Score,
                    Absent = e.e.Absent,
                    TieBreak = e.e.Tie_break,
                    PentaScore = e.e.Penta_Score.HasValue ? e.e.Penta_Score.Value : 0,
                    TeamOrPair = e.e.Partner,
                    PIN = e.e.PIN.HasValue ? e.e.PIN.Value.ToString() : ""
                });
            }
        }

        public List<string> Validate()
        {
            var errors = new List<string>();

            ValidatePartners(errors);

            return errors;
        }

        private void ValidatePartners(List<string> errors)
        {
            if (NumberInTeam == 2)
            {
                foreach (var e1 in Entrants.Where(x => !x.Absent && x.Rank != 0))
                {
                    var e2 = Entrants.FirstOrDefault(x => x.FullName == e1.TeamOrPair);
                    if (e2 == null)
                        errors.Add(e1.FullName + " does not have a partner");
                    else if (e2.TeamOrPair != e1.FullName)
                        errors.Add(e1.FullName + " and " + e2.FullName + " have mismatched partners");
                    else if (e2.Rank != e1.Rank)
                        errors.Add("Partners " + e1.FullName + " and " + e2.FullName + " should have the same rank");
                }
            }
        }

        public void Save()
        {
            var context = DataEntitiesProvider.Provide();
            var evt = context.Events.First(x => x.OlympiadId == CurrentOlympiadId && x.Code == EventCode);

            // There's no "create" here
            if (evt == null)
                throw new ArgumentException("No such event " + EventCode + " in olympiad " + CurrentOlympiadId);

            evt.EIN = EventId;
            evt.Arbiters.Clear();
            var arbiter = context.Contestants.ToList().FirstOrDefault(a => a.FullName() == Arbiter);
            if (arbiter != null)
            {
                evt.Arbiters.Add(new Arbiter() { EIN = EventId, Name = arbiter });
            }
            evt.Location = Location;
            evt.Entry_Fee = EntryFee;
            evt.Number_in_Team = NumberInTeam;
            evt.Prize_fund = PrizeFund;
            evt.C1st_Prize = Prize1.ToString();
            evt.C2nd_Prize = Prize2.ToString();
            evt.C3rd_Prize = Prize3.ToString();
            evt.JNR_1st_Prize = JuniorPrize1.ToString();
            evt.JNR_2nd_Prize = JuniorPrize2.ToString();
            evt.JNR_3rd_Prize = JuniorPrize3.ToString();
            evt.Other_Prizes = OtherPrizes;
            evt.JNR_Other_Prizes = JuniorOtherPrizes;
            evt.Pentamind = Pentamind;
            evt.incMaxFee = IncludedInMaxFee;
            evt.JNR_Medals = JuniorMedals;
            evt.Type = Type;
            evt.X_Num = ExpectedNumber;
            evt.Notes = Notes;
            evt.No_Sessions = NumSessions;

            evt.Event_Sess.Clear();
            foreach (var s in Sessions)
            {
                var session = new Event_Sess() { EIN = EventId, Date = s.Date, Session = s.SessionCode };
                evt.Event_Sess.Add(session);
            }
       
            // Entrants are *not* added here but their ranks etc are
            foreach (var e in Entrants)
            {
                var entrant = context.Entrants.First(x => x.EntryNumber == e.EntrantId);
                entrant.Absent = e.Absent;
                entrant.Medal = (string.IsNullOrEmpty(e.Medal)) ? null : e.Medal;
                entrant.JuniorMedal = (string.IsNullOrEmpty(e.JuniorMedal)) ? null : e.JuniorMedal;
                entrant.Rank = e.Rank;
                entrant.Score = e.Score;
                if (e.Rank > 0) entrant.Penta_Score = e.PentaScore; else entrant.Penta_Score = null;
                entrant.Tie_break = e.TieBreak;
                entrant.Partner = e.TeamOrPair;
            }

            context.SaveChanges();

            // Now set the Number. This has to be done in conjunction with other events.
            var indexer = new EventIndexer();
            indexer.IndexEvents(CurrentOlympiadId);
        }
    }
}
