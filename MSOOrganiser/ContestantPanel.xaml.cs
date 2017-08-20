using JuliaHayward.Common.Environment;
using JuliaHayward.Common.Logging;
using MSOCore;
using MSOCore.Calculators;
using MSOCore.Models;
using MSOOrganiser.Dialogs;
using MSOOrganiser.Events;
using MSOOrganiser.Reports;
using MSOOrganiser.UIUtilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MSOOrganiser
{
    /// <summary>
    /// Interaction logic for EventPanel.xaml
    /// </summary>
    public partial class ContestantPanel : UserControl
    {
        public ContestantPanel()
        {
            InitializeComponent();
            DataContext = new ContestantPanelVm();

            if (!JuliaEnvironment.CurrentEnvironment.IsDebug())
            {
                dataGrid.Columns[0].Visibility = Visibility.Collapsed;
                paymentDataGrid.Columns[0].Visibility = Visibility.Collapsed;
            }
        }

        public ContestantPanelVm ViewModel
        {
            get { return (ContestantPanelVm)DataContext;  }
        }

        // A delegate type for hooking up change notifications.
        public delegate void EventEventHandler(object sender, EventEventArgs e);

        public event EventEventHandler EventSelected;


        public void Populate()
        {
            ViewModel.PopulateDropdown();
        }

        public void Populate(int contestantId)
        {
            ViewModel.PopulateDropdown(contestantId);
        }

        public void search_Click(object sender, RoutedEventArgs e)
        {
            using (new SpinnyCursor())
            {
                ViewModel.PopulateDropdown();
            }
        }

        public void clearSearch_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.FilterFirstName = "";
            ViewModel.FilterLastName = "";
            ViewModel.PopulateDropdown();
        }

        private void contestantCombo_Changed(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.PopulateContestant();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PopulateContestant();
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
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

        private void olympiadCombo_Changed(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.PopulateEvents();
            ViewModel.PopulatePayments();
        }

        private void event_Click(object sender, RoutedEventArgs e)
        {
            var entrant = ((FrameworkElement)sender).DataContext as ContestantPanelVm.EventVm;
            if (EventSelected != null)
            {
                var args = new EventEventArgs() { EventCode = entrant.EventCode, 
                    OlympiadId = ViewModel.CurrentOlympiadId };
                EventSelected(this, args);
            }
        }

        private void editFee_Click(object sender, RoutedEventArgs e)
        {
            var entrant = ((FrameworkElement)sender).DataContext as ContestantPanelVm.EventVm;
            var dialog = new SelectFeeDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult.Value)
            {
                entrant.Fee = dialog.Fee;
            }
            ViewModel.UpdateTotals();
            ViewModel.IsDirty = true;
        }

        private void addEvent_Click(object sender, RoutedEventArgs e)
        {
            var selectedEvents = ViewModel.Events.Select(x => x.EventCode);
            var nonEditableEvents = ViewModel.Events.Where(x => x.Rank != 0).Select(x => x.EventCode);
            var dialog = new AddEventsToContestantWindow(ViewModel.CurrentOlympiadId, 
                ViewModel.IsJuniorForOlympiad, selectedEvents, nonEditableEvents);
            dialog.ShowDialog();

            if (dialog.DialogResult.Value)
            {
                foreach (var evt in dialog.SelectedEvents)
                {
                    if (evt.IsSelected)
                    {
                        var existingEvent = ViewModel.Events.FirstOrDefault(x => x.EventCode == evt.Code);
                        if (existingEvent == null)
                        {
                            ViewModel.AddEvent(new ContestantPanelVm.EventVm() { EventCode = evt.Code, EventName = evt.Name, EventId = evt.Id, Fee = evt.Fee, StandardFee = evt.Fee, IncludedInMaxFee = evt.IsIncludedInMaxFee, IsEvent = (evt.Event.Number > 0) });
                            ViewModel.IsDirty = true;
                        }
                    }
                    else
                    {
                        var eventToDelete = ViewModel.Events.FirstOrDefault(x => x.EventCode == evt.Code);
                        if (eventToDelete != null)
                        {
                            ViewModel.RemoveEvent(eventToDelete);
                            ViewModel.IsDirty = true;
                        }
                    }
                }
                ViewModel.ApportionCosts();
                ViewModel.IsDirty = true;
            }
        }

        private void addPayment_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddPaymentToContestantDialog();
            if (dialog.ShowDialog().Value)
            {
                ViewModel.AddPayment(new ContestantPanelVm.PaymentVm()
                {
                    Amount = dialog.Payment.Amount,
                    Method = dialog.Payment.PaymentMethod,
                    Banked = dialog.Payment.Banked
                });
                ViewModel.IsDirty = true;
            }
        }

        private void addNew_Click(object sender, RoutedEventArgs e)
        {
            this.TabControl.SelectedIndex = 0;
            ViewModel.Contestants.Clear();
            ViewModel.Contestants.Insert(0, new ContestantPanelVm.ContestantVm { Text = "New Contestant", Value = "0" });
            ViewModel.ContestantId = "0";
            ViewModel.PopulateContestant();
            // Put the cursor in the right place - doesn't work if tab has changed
            this.FirstName.Focus();
        }

        private void print_Click(object sender, RoutedEventArgs e)
        {
            var documentPrinter = new FlowDocumentPrinter();
            var printer = new ContestantResultsPrinter();
            documentPrinter.PrintFlowDocument(() => printer.Print(ViewModel));
        }
    }

    public class ContestantPanelVm : VmBase
    {

    public class TitleVm
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }

        public class ContestantVm
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }

        public class NationalityVm
        {
            public string Text { get; set; }
            public string Value { get; set; }

            public NationalityVm(string n)
            {
                Text = n;
                Value = n;
            }
        }

        public class OlympiadVm
        {
            public string Text { get; set; }
            public int Value { get; set; }
        }

        public class EventVm  : VmBase
        {
            public int EventId { get; set; }
            public string EventCode { get; set; }
            public string EventName { get; set; }
            public DateTime? Date { get; set; }
            public string FormattedDate { get {
                return (Date.HasValue) ? Date.Value.ToString("ddd dd, HH:mm") : "";
            } }
            public bool Receipt { get; set; }
            public decimal StandardFee { get; set; }    // Before we apply the max-fee apportioning
            private decimal _fee;
            public decimal Fee { get { return _fee; }
                set { if (value != _fee) { _fee = value; OnPropertyChanged("Fee"); } }
            }
            public bool IncludedInMaxFee { get; set; }
            public bool IsEvent { get; set; }
            public Visibility ResultsButtonVisibility
            {
                get { return (IsEvent) ? Visibility.Visible : Visibility.Collapsed; }
            }
            public Visibility EditFeeButtonVisibility
            {
                get { return (!IsEvent || GlobalSettings.LoggedInUserIsAdmin) 
                    ? Visibility.Visible : Visibility.Collapsed; }
            }
            public string Partner { get; set; }
            public string Medal { get; set; }
            public string JuniorMedal { get; set; }
            public int Rank { get; set; }
            public string Penta { get; set; }
            public string TieBreak { get; set; }
            public bool Absent { get; set; }
        }

        public class PaymentVm
        {
            public int PaymentId { get; set; }
            public decimal Amount { get; set; }
            public string Method { get; set; }
            public bool Banked { get; set; }
        }

        #region bindable properties
        public ObservableCollection<TitleVm> Titles { get; set; }
        public ObservableCollection<EventVm> Events { get; set; }
        public ObservableCollection<PaymentVm> Payments { get; set; }
        public static ObservableCollection<NationalityVm> Nationalities { get; set; }

        public ObservableCollection<ContestantVm> Contestants { get; set; }
        public CollectionView FilteredContestants { get; set; }
        public bool IsJuniorForOlympiad { get; set; }
        
        private string _contestantId;
        public string ContestantId
        {
            get
            {
                return _contestantId;
            }
            set
            {
                if (_contestantId != value)
                {
                    _contestantId = value;
                    OnPropertyChanged("ContestantId");
                }
            }
        }

        public ObservableCollection<OlympiadVm> Olympiads { get; set; }

        private int _currentOlympiadId;
        public int CurrentOlympiadId
        {
            get
            {
                return _currentOlympiadId;
            }
            set
            {
                if (_currentOlympiadId != value)
                {
                    _currentOlympiadId = value;
                    OnPropertyChanged("CurrentOlympiadId");
                }
            }
        }

        public bool EditingThePast { get; set; }

        private string _filterFirstName = "";
        public string FilterFirstName
        {
            get
            {
                return _filterFirstName;
            }
            set
            {
                if (_filterFirstName != value)
                {
                    _filterFirstName = value;
                    OnPropertyChanged("FilterFirstName");
                }
            }
        }

        private string _filterLastName = "";
        public string FilterLastName
        {
            get
            {
                return _filterLastName;
            }
            set
            {
                if (_filterLastName != value)
                {
                    _filterLastName = value;
                    OnPropertyChanged("FilterLastName");
                }
            }
        }
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
                    OnPropertyChanged("IsNotDirty");
                }
            }
        }

        public bool IsNotDirty { get { return !IsDirty; } }

        private string _Title;
        public string Title 
        { 
            get 
            { 
                return _Title; 
            }
            set
            {
                if (_Title != value)
                {
                    _Title = value;
                    IsDirty = true;
                    OnPropertyChanged("Title");
                }
                SetGenderForHonorific(_Title);
            }
        }

        private void SetGenderForHonorific(string title)
        {
            var maleHonorifics = new[] { "Mr", "Master", "Sir" };
            if (maleHonorifics.Contains(title))
                IsMale = true;

            var femaleHonorifics = new[] { "Mrs", "Ms", "Miss", "Lady" };
            if (femaleHonorifics.Contains(title)) 
                IsMale = false;
        }


        private string _FirstName;
        public string FirstName
        {
            get
            {
                return _FirstName;
            }
            set
            {
                if (_FirstName != value)
                {
                    _FirstName = value;
                    IsDirty = true;
                    OnPropertyChanged("FirstName");
                    OnPropertyChanged("Name");
                }
            }
        }
        private string _LastName;
        public string LastName
        {
            get
            {
                return _LastName;
            }
            set
            {
                if (_LastName != value)
                {
                    _LastName = value;
                    IsDirty = true;
                    OnPropertyChanged("LastName");
                    OnPropertyChanged("Name");
                }
            }
        }
        private string _Initials;
        public string Initials
        {
            get
            {
                return _Initials;
            }
            set
            {
                if (_Initials != value)
                {
                    _Initials = value;
                    IsDirty = true;
                    OnPropertyChanged("Initials");
                    OnPropertyChanged("Name");
                }
            }
        }

        public string Name { get { return FirstName + " " + Initials + " " + LastName;  } }

        private bool _IsMale;
        public bool IsMale
        {
            get
            {
                return _IsMale;
            }
            set
            {
                if (_IsMale != value)
                {
                    _IsMale = value;
                    IsDirty = true;
                    OnPropertyChanged("IsMale");
                }
            }
        }

        private string _Nationality;
        public string Nationality
        {
            get
            {
                return _Nationality;
            }
            set
            {
                if (_Nationality != value)
                {
                    _Nationality = value;
                    IsDirty = true;
                    OnPropertyChanged("Nationality");
                }
            }
        }

        private string _DayPhone;
        public string DayPhone
        {
            get
            {
                return _DayPhone;
            }
            set
            {
                if (_DayPhone != value)
                {
                    _DayPhone = value;
                    IsDirty = true;
                    OnPropertyChanged("DayPhone");
                }
            }
        }

        private string _EvePhone;
        public string EvePhone
        {
            get
            {
                return _EvePhone;
            }
            set
            {
                if (_EvePhone != value)
                {
                    _EvePhone = value;
                    IsDirty = true;
                    OnPropertyChanged("EvePhone");
                }
            }
        }

        private string _Fax;
        public string Fax
        {
            get
            {
                return _Fax;
            }
            set
            {
                if (_Fax != value)
                {
                    _Fax = value;
                    IsDirty = true;
                    OnPropertyChanged("Fax");
                }
            }
        }
private string _Email;
        public string Email
        {
            get
            {
                return _Email;
            }
            set
            {
                if (_Email != value)
                {
                    _Email = value;
                    IsDirty = true;
                    OnPropertyChanged("Email");
                }
            }
        }
private bool _WantsNoNews;
        public bool WantsNoNews
        {
            get
            {
                return _WantsNoNews;
            }
            set
            {
                if (_WantsNoNews != value)
                {
                    _WantsNoNews = value;
                    IsDirty = true;
                    OnPropertyChanged("WantsNoNews");
                }
            }
        }

        private DateTime? _DateOfBirth;
        public DateTime? DateOfBirth
        {
            get
            {
                return _DateOfBirth;
            }
            set
            {
                if (_DateOfBirth != value)
                {
                    _DateOfBirth = value;
                    IsDirty = true;
                    OnPropertyChanged("DateOfBirth");
                }
            }
        }

private bool _IsConcessional;
        public bool IsConcessional
        {
            get
            {
                return _IsConcessional;
            }
            set
            {
                if (_IsConcessional != value)
                {
                    _IsConcessional = value;
                    IsDirty = true;
                    OnPropertyChanged("IsConcessional");
                }
            }
        }
private string _Address1;
        public string Address1
        {
            get
            {
                return _Address1;
            }
            set
            {
                if (_Address1 != value)
                {
                    _Address1 = value;
                    IsDirty = true;
                    OnPropertyChanged("Address1");
                }
            }
        }
private string _Address2;
        public string Address2
        {
            get
            {
                return _Address2;
            }
            set
            {
                if (_Address2 != value)
                {
                    _Address2 = value;
                    IsDirty = true;
                    OnPropertyChanged("Address2");
                }
            }
        }
private string _City;
        public string City
        {
            get
            {
                return _City;
            }
            set
            {
                if (_City != value)
                {
                    _City = value;
                    IsDirty = true;
                    OnPropertyChanged("City");
                }
            }
        }
private string _County;
        public string County
        {
            get
            {
                return _County;
            }
            set
            {
                if (_County != value)
                {
                    _County = value;
                    IsDirty = true;
                    OnPropertyChanged("County");
                }
            }
        }
private string _Postcode;
        public string Postcode
        {
            get
            {
                return _Postcode;
            }
            set
            {
                if (_Postcode != value)
                {
                    _Postcode = value;
                    IsDirty = true;
                    OnPropertyChanged("Postcode");
                }
            }
        }
private string _Country;
        public string Country
        {
            get
            {
                return _Country;
            }
            set
            {
                if (_Country != value)
                {
                    _Country = value;
                    IsDirty = true;
                    OnPropertyChanged("Country");
                }
            }
        }
private string _BCFCode;
        public string BCFCode
        {
            get
            {
                return _BCFCode;
            }
            set
            {
                if (_BCFCode != value)
                {
                    _BCFCode = value;
                    IsDirty = true;
                    OnPropertyChanged("BCFCode");
                }
            }
        }
private string _FIDECode;
        public string FIDECode
        {
            get
            {
                return _FIDECode;
            }
            set
            {
                if (_FIDECode != value)
                {
                    _FIDECode = value;
                    IsDirty = true;
                    OnPropertyChanged("FIDECode");
                }
            }
        }
private string _Notes;
        public string Notes
        {
            get
            {
                return _Notes;
            }
            set
            {
                if (_Notes != value)
                {
                    _Notes = value;
                    IsDirty = true;
                    OnPropertyChanged("Notes");
                }
            }
        }

        private string _competedIn;
        public string CompetedIn
        {
            get
            {
                return _competedIn;
            }
            set
            {
                if (_competedIn != value)
                {
                    _competedIn = value;
                    IsDirty = true;
                    OnPropertyChanged("CompetedIn");
                }
            }
        }

        public string Totals
        {
            get
            {
                var totalFees = Events.Sum(x => x.Fee);
                var totalPayment = Payments.Sum(x => x.Amount);
                var paymentString = "Total fees: £" + totalFees.ToString("F")
                    + "      Total payments: £" + totalPayment.ToString("F")
                    + ((totalFees <= totalPayment) ? "" : "     Owing: £" + (totalFees - totalPayment).ToString("F"));

                var eventsString = "        Total events: " + Events.Count();
                return paymentString + eventsString;
            }
        }

        public DateTime UpdatedAt { get; set; }

        #endregion

        public ContestantPanelVm()
        {
            Contestants = new ObservableCollection<ContestantVm>();
            Olympiads = new ObservableCollection<OlympiadVm>();
            Events = new ObservableCollection<EventVm>();
            Payments = new ObservableCollection<PaymentVm>();
            ContestantId = "0";
            FilterFirstName = "";
            FilterLastName = "";

            Titles = new ObservableCollection<TitleVm>();
            Titles.Add(new TitleVm() { Text = "Mr", Value = "Mr" });
            Titles.Add(new TitleVm() { Text = "Mrs", Value = "Mrs" });
            Titles.Add(new TitleVm() { Text = "Ms", Value = "Ms" });
            Titles.Add(new TitleVm() { Text = "Miss", Value = "Miss" });
            Titles.Add(new TitleVm() { Text = "Master", Value = "Master" });
            Titles.Add(new TitleVm() { Text = "Dr", Value = "Dr" });
            Titles.Add(new TitleVm() { Text = "Prof", Value = "Prof" });
            Titles.Add(new TitleVm() { Text = "Sir", Value = "Sir" });
            Titles.Add(new TitleVm() { Text = "Lady", Value = "Lady" });

            if (Nationalities == null)
            {
                Nationalities = new ObservableCollection<NationalityVm>();
                var context = DataEntitiesProvider.Provide();
                context.Nationalities.Select(x => x.Name).ToList()
                    .ForEach(n => Nationalities.Add(new NationalityVm(n)));
            }
            
            PopulateOlympiadDropdown();
            PopulateEvents();
            PopulatePayments();
        }

        public void AddPayment(PaymentVm item)
        {
            Payments.Add(item);
            OnPropertyChanged("Totals");
        }

        public void AddEvent(EventVm item)
        {
            Events.Add(item);
            OnPropertyChanged("Totals");
        }

        public void ApportionCosts()
        {
            var context = DataEntitiesProvider.Provide();
            var olympiad = context.Olympiad_Infoes.First(x => x.Id == CurrentOlympiadId);

            var canApportion = (!IsJuniorForOlympiad) ? olympiad.MaxFee.HasValue : olympiad.MaxCon.HasValue;
            if (!canApportion) return;

            var maxFee = (!IsJuniorForOlympiad) ?
                olympiad.MaxFee.Value : olympiad.MaxCon.Value;

            var apportioner = new CostApportioner<EventVm>(
                x => x.StandardFee, (e, f) => e.Fee = f, x => x.IncludedInMaxFee);
            apportioner.ApportionCost(Events, maxFee);

            OnPropertyChanged("Totals");
        }

        public void UpdateTotals()
        {
            OnPropertyChanged("Totals");
        }

        public void RemoveEvent(EventVm item)
        {
            Events.Remove(item);
            OnPropertyChanged("Totals");
        }


        public void PopulateDropdown()
        {
            Contestants.Clear();

            // Don't search if neither name provided
            if (FilterFirstName == "" && FilterLastName == "")
            {
                Contestants.Add(new ContestantVm { Text = "New Competitor", Value = "0" });
            }
            else
            {
                var context = DataEntitiesProvider.Provide();
                var searched = context.Contestants.Where(SearchPredicate).ToList();
                if (!searched.Any())
                {
                    MessageBox.Show("No matches found");
                    Contestants.Add(new ContestantVm { Text = "New Competitor", Value = "0" });
                }
                else
                {
                    if (searched.Count() > 1)
                        MessageBox.Show(string.Format("{0} matches found", searched.Count()));

                    foreach (var c in searched.OrderBy(x => x.Lastname).ThenBy(x => x.Firstname))
                        Contestants.Add(new ContestantVm { Text = c.FullNameWithInitials(), Value = c.Mind_Sport_ID.ToString() });
                }
            }

            ContestantId = Contestants.First().Value;
        }

        public void PopulateDropdown(int contestantId)
        {
            Contestants.Clear();
            var context = DataEntitiesProvider.Provide();
            foreach (var c in context.Contestants
                .Where(x => x.Mind_Sport_ID == contestantId))
                Contestants.Add(new ContestantVm { Text = c.FullNameWithInitials(), Value = c.Mind_Sport_ID.ToString() });

            ContestantId = Contestants.First().Value;
        }

        public void PopulateOlympiadDropdown()
        {
            Olympiads.Clear();
            var context = DataEntitiesProvider.Provide();
            foreach (var c in context.Olympiad_Infoes
                .OrderByDescending(x => x.StartDate))
                Olympiads.Add(new OlympiadVm { Text = c.FullTitle(), Value = c.Id });
            CurrentOlympiadId = Olympiads.First().Value;
        }

        public void PopulateEvents()
        {
            Events.Clear();
            if (ContestantId == null) return;

            EditingThePast = (CurrentOlympiadId != Olympiads.First().Value);

            var context = DataEntitiesProvider.Provide();
            var olympiadId = CurrentOlympiadId;
            var olympiad = context.Olympiad_Infoes.First(x => x.Id == CurrentOlympiadId);
            IsJuniorForOlympiad = Contestant.IsJuniorForOlympiad(DateOfBirth, olympiad);
            var contestantId = int.Parse(ContestantId);
            var entrants = context.Entrants
                .Join(context.Events, e => e.Game_Code, g => g.Code, (e, g) => new { e = e, g = g })
                .Where(x => x.e.OlympiadId == olympiadId && x.g.OlympiadId == olympiadId && x.e.Mind_Sport_ID == contestantId)
                .OrderBy(x => x.e.Game_Code).ToList();
     
            var allFees = context.Fees.ToList();
            var fees = (IsJuniorForOlympiad)
                ? allFees.ToDictionary(x => x.Code, x => x.Concession)
                : allFees.ToDictionary(x => x.Code, x => x.Adult);

            foreach (var e in entrants)
            {
                // TODO: this is really an EntrantVm not an EventVm
                Events.Add(new EventVm()
                {
                    EventId = e.e.EventId.Value,
                    Absent = e.e.Absent,
                    EventCode = e.e.Game_Code,
                    EventName = e.g.Mind_Sport,
                    Fee = e.e.Fee,
                    StandardFee = (e.g.Entry_Fee != null) ? fees[e.g.Entry_Fee].Value : 0,
                    IncludedInMaxFee = (e.g.incMaxFee.HasValue && e.g.incMaxFee.Value),
                    IsEvent = (e.g.Number > 0),
                    Medal = e.e.Medal ?? "",
                    JuniorMedal = e.e.JuniorMedal ?? "",
                    Partner = e.e.Partner ?? "",
                    Penta = e.e.Penta_Score.HasValue ? e.e.Penta_Score.Value.ToString() : "",
                    Rank = e.e.Rank.HasValue ? e.e.Rank.Value : 0,
                    Receipt = e.e.Receipt.Value,
                    TieBreak = e.e.Tie_break ?? "",
                    Date = e.e.Event.Start
                });
            }
        }

        public void PopulatePayments()
        {
            Payments.Clear();
            if (ContestantId == null) return;
            if (CurrentOlympiadId == null) return;

            var context = DataEntitiesProvider.Provide();
            var olympiadId = CurrentOlympiadId;
            var contestantId = int.Parse(ContestantId);
            var payments = context.Payments.Where(x => x.OlympiadId == olympiadId && x.MindSportsID == contestantId)
                .OrderBy(x => x.PaymentNumber).ToList();
            foreach (var p in payments)
            {
                Payments.Add(new PaymentVm()
                {
                    Amount = p.Payment1.Value,
                    Banked = (p.Banked.HasValue && p.Banked.Value == 1),
                    Method = p.Payment_Method,
                    PaymentId = p.PaymentNumber
                });
            }
        }

        public Func<Contestant, bool> SearchPredicate
        {
            get
            {
                Func<Contestant, bool> func1;
                if (string.IsNullOrEmpty(FilterLastName)) 
                    func1 = (x => true);
                else
                    func1 = (x => x.Lastname != null && x.Lastname.ToLower().Contains(FilterLastName.ToLower()));
                Func<Contestant, bool> func2;
                if (string.IsNullOrEmpty(FilterFirstName)) 
                    func2 = (x => true);
                else
                    func2 = (x => x.Firstname != null && x.Firstname.ToLower().Contains(FilterFirstName.ToLower()));
                return x => (func1(x) && func2(x));
            }
        }

        public void PopulateContestant()
        {
            if (ContestantId == null) return; // happens on Contestants.Clear()

            var id = int.Parse(ContestantId);
            if (id == 0)
            {
                FirstName = "";
                LastName = "";
                Initials = "";
                IsMale = true;
                Nationality = "";
                DayPhone = "";
                EvePhone = "";
                Fax = "";
                WantsNoNews = false;
                DateOfBirth = null;
                IsConcessional = false;
                Address1 = "";
                Address2 = "";
                City = "";
                County = "";
                Postcode = "";
                Country = "";
                BCFCode = "";
                FIDECode = "";
                Notes = "";
                UpdatedAt = DateTime.Now;
                CompetedIn = "";
            }
            else
            {
                var context = DataEntitiesProvider.Provide();
                var dbCon = context.Contestants.FirstOrDefault(x => x.Mind_Sport_ID == id);
                Title = dbCon.Title;
                FirstName = dbCon.Firstname;
                LastName = dbCon.Lastname;
                Initials = dbCon.Initials;
                IsMale = dbCon.Male;
                Nationality = dbCon.Nationality;
                DayPhone = dbCon.DayPhone;
                EvePhone = dbCon.EvePhone;
                Fax = dbCon.Fax;
                WantsNoNews = dbCon.No_News.HasValue ? dbCon.No_News.Value : true;
                DateOfBirth = dbCon.DateofBirth;
                IsConcessional = dbCon.Concessional.HasValue ? dbCon.Concessional.Value : true;
                Address1 = dbCon.Address1;
                Address2 = dbCon.Address2;
                City = dbCon.City;
                County = dbCon.County;
                Postcode = dbCon.PostCode;
                Country = dbCon.Country;
                BCFCode = dbCon.BCFCode;
                FIDECode = dbCon.FIDECode;
                Notes = dbCon.Notes;

                var years = context.Entrants
                    // Shouldn't be necessary but dipping into historic data is error-prone
                    .Where(e => e.Mind_Sport_ID == id && e.Event != null && e.Event.Olympiad_Info != null)
                    .Select(e => e.Event.Olympiad_Info.YearOf.Value)
                    .Distinct()
                    .OrderBy(y => y);
                CompetedIn = NumberListContractor.Contract(years);
            }

            IsDirty = false;
            PopulateEvents();
            PopulatePayments();
            OnPropertyChanged("Totals");
        }

        public List<string> Validate()
        {
            var errors = new List<string>();

            if (!(string.IsNullOrEmpty(Email) || new EmailAddressAttribute().IsValid(Email)))
                errors.Add(Email + " is not a valid email address");

            return errors;
        }

        public void Save()
        {
            // Just to make sure we have picked up any changes that have come through online
            ApportionCosts();

            var context = DataEntitiesProvider.Provide();
            var id = int.Parse(ContestantId);
            if (id == 0)
            {
                var dbCon = new Contestant()
                {
                    Title = this.Title,
                    Firstname = this.FirstName,
                    Lastname = this.LastName,
                    Initials = this.Initials,
                    Male = this.IsMale,
                    Nationality = this.Nationality,
                    DayPhone = this.DayPhone,
                    EvePhone = this.EvePhone,
                    Fax = this.Fax,
                    No_News = this.WantsNoNews,
                    DateofBirth = this.DateOfBirth,
                    Concessional = this.IsConcessional,
                    Address1 = this.Address1,
                    Address2 = this.Address2,
                    City = this.City,
                    County = this.County,
                    PostCode = this.Postcode,
                    Country = this.Country,
                    BCFCode = this.BCFCode,
                    FIDECode = this.FIDECode,
                    Notes = this.Notes
                };
                context.Contestants.Add(dbCon);
                id = dbCon.Mind_Sport_ID;
                dbCon.Entrants = this.Events
                    .Select(x => Entrant.NewEntrant(x.EventId, x.EventCode, CurrentOlympiadId, dbCon, x.Fee))
                    .ToList();
                dbCon.Payments = this.Payments
                    .Select(x => new Payment() { Payment1 = x.Amount, Payment_Method = x.Method, PaymentNumber = 0, Banked = x.Banked ? 1 : 0, MindSportsID = dbCon.Mind_Sport_ID, OlympiadId = CurrentOlympiadId, Name = dbCon, Received = DateTime.Now })
                    .ToList();
            }
            else
            {
                var dbCon = context.Contestants.FirstOrDefault(x => x.Mind_Sport_ID == id);

                // TODO if dbCon is greater than this UpdatedDate, someone else has edited

                dbCon.Title = Title;
                dbCon.Firstname = FirstName;
                dbCon.Lastname = LastName;
                dbCon.Initials = Initials;
                dbCon.Male = IsMale;
                dbCon.Nationality = Nationality;
                dbCon.DayPhone = DayPhone;
                dbCon.EvePhone = EvePhone;
                dbCon.Fax = Fax;
                dbCon.No_News = WantsNoNews;
                dbCon.DateofBirth = DateOfBirth;
                dbCon.Concessional = IsConcessional;
                dbCon.Address1 = Address1;
                dbCon.Address2 = Address2;
                dbCon.City = City;
                dbCon.County = County;
                dbCon.PostCode = Postcode;
                dbCon.Country = Country;
                dbCon.BCFCode = BCFCode;
                dbCon.FIDECode = FIDECode;
                dbCon.Notes = Notes;

                // Add new events that aren't already in this olympiad for this person
                foreach (var e in this.Events.Where(x => !dbCon.Entrants.Any(ee => ee.OlympiadId == CurrentOlympiadId && ee.Game_Code == x.EventCode)))
                {
                    dbCon.Entrants.Add(Entrant.NewEntrant(e.EventId, e.EventCode, CurrentOlympiadId, dbCon, e.Fee));
                }
                // Remove unwanted events from this olympiad
                foreach (var de in dbCon.Entrants.Where(x => x.OlympiadId == CurrentOlympiadId && !this.Events.Any(ee => ee.EventCode == x.Game_Code)).ToList())
                {
                    dbCon.Entrants.Remove(de);
                    context.Entrants.Remove(de);
                }
                // Update changed events from this olympiad
                foreach (var de in dbCon.Entrants.Where(x => x.OlympiadId == CurrentOlympiadId && this.Events.Any(ee => ee.EventCode == x.Game_Code)).ToList())
                {
                    // Fee may change because we've reached maximum fee and are apportioning
                    de.Fee = this.Events.First(ee => ee.EventCode == de.Game_Code).Fee;
                }
                // Add new payments that aren't already in this olympiad for this person
                foreach (var p in this.Payments.Where(x => !dbCon.Payments.Any(pp => pp.PaymentNumber == x.PaymentId)))
                {
                    dbCon.Payments.Add(new Payment() { Payment1 = p.Amount, Payment_Method = p.Method, PaymentNumber = 0, Banked = p.Banked ? 1 : 0, MindSportsID = dbCon.Mind_Sport_ID, OlympiadId = CurrentOlympiadId, Name = dbCon, Received = DateTime.Now });
                }
                // At the moment we can't delete payments (deliberate)
            }
            context.SaveChanges();
            IsDirty = false;
            PopulateDropdown();
            ContestantId = id.ToString();
        }
    }
}
