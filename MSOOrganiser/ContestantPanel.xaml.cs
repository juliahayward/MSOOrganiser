using MSOCore.Models;
using MSOOrganiser.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            ViewModel.PopulateDropdown();
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
            ViewModel.Save();
        }

        private void olympiadCombo_Changed(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.PopulateEvents();
        }

        private void event_Click(object sender, RoutedEventArgs e)
        {
            var entrant = ((FrameworkElement)sender).DataContext as ContestantPanelVm.EventVm;
            if (EventSelected != null)
            {
                var args = new EventEventArgs() { EventCode = entrant.EventCode };
                EventSelected(this, args);
            }
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

        public class OlympiadVm
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }

        public class EventVm
        {
            public int EventId { get; set; }
            public string EventCode { get; set; }
            public string EventName { get; set; }
            public bool Receipt { get; set; }
            public decimal Fee { get; set; }
            public string Partner { get; set; }
            public string Medal { get; set; }
            public int Rank { get; set; }
            public string Penta { get; set; }
            public string TieBreak { get; set; }
            public bool Absent { get; set; }
        }
        

        #region bindable properties
        public ObservableCollection<TitleVm> Titles { get; set; }
        public ObservableCollection<EventVm> Events { get; set; }

        public ObservableCollection<ContestantVm> Contestants { get; set; }
        public CollectionView FilteredContestants { get; set; }
        
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

        private string _currentOlympiadId;
        public string CurrentOlympiadId
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

        private string _filterFirstName;
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

        private string _filterLastName;
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
                }
            }
        }

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
            }
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
                    _IsDirty = true;
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
                    _IsDirty = true;
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
                    _IsDirty = true;
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
                    _IsDirty = true;
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
                    _IsDirty = true;
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
                    _IsDirty = true;
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
                    _IsDirty = true;
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
                    _IsDirty = true;
                    OnPropertyChanged("WantsNoNews");
                }
            }
        }
private string _DateOfBirth;
        public string DateOfBirth
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
                    _IsDirty = true;
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
                    _IsDirty = true;
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
                    _IsDirty = true;
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
                    _IsDirty = true;
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
                    _IsDirty = true;
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
                    _IsDirty = true;
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
                    _IsDirty = true;
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
                    _IsDirty = true;
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
                    _IsDirty = true;
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
                    _IsDirty = true;
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
                    _IsDirty = true;
                    OnPropertyChanged("Notes");
                }
            }
        }

        public DateTime UpdatedAt { get; set; }

        #endregion

        public ContestantPanelVm()
        {
            Contestants = new ObservableCollection<ContestantVm>();
            Olympiads = new ObservableCollection<OlympiadVm>();
            Events = new ObservableCollection<EventVm>();
            ContestantId = "0";
            FilterFirstName = "";
            FilterLastName = "";

            Titles = new ObservableCollection<TitleVm>();
            Titles.Add(new TitleVm() { Text = "Mr", Value = "Mr" });
            Titles.Add(new TitleVm() { Text = "Mrs", Value = "Mrs" });
            Titles.Add(new TitleVm() { Text = "Ms", Value = "Ms" });
            Titles.Add(new TitleVm() { Text = "Miss", Value = "Miss" });
            Titles.Add(new TitleVm() { Text = "Dr", Value = "Dr" });
            
            PopulateOlympiadDropdown();
            PopulateEvents();
        }

        public void PopulateDropdown()
        {
            Contestants.Clear();
            var context = new DataEntities();
    
            if (FilterFirstName == "" && FilterLastName == "")
                Contestants.Add(new ContestantVm { Text = "New Competitor", Value = "0" });
            
            foreach (var c in context.Contestants
                .Where(SearchPredicate)
                .OrderBy(x => x.Lastname).ThenBy(x => x.Firstname))
                Contestants.Add(new ContestantVm { Text = c.Firstname + " " + c.Lastname, Value = c.Mind_Sport_ID.ToString() });

            ContestantId = Contestants.First().Value;
        }

        public void PopulateDropdown(int contestantId)
        {
            Contestants.Clear();
            var context = new DataEntities();
            foreach (var c in context.Contestants
                .Where(x => x.Mind_Sport_ID == contestantId))
                Contestants.Add(new ContestantVm { Text = c.Firstname + " " + c.Lastname, Value = c.Mind_Sport_ID.ToString() });

            ContestantId = Contestants.First().Value;
        }

        public void PopulateOlympiadDropdown()
        {
            Olympiads.Clear();
            var context = new DataEntities();
            foreach (var c in context.Olympiad_Infoes
                .OrderByDescending(x => x.StartDate))
                Olympiads.Add(new OlympiadVm { Text = c.FullTitle(), Value = c.Id.ToString() });
            CurrentOlympiadId = Olympiads.First().Value;
        }

        public void PopulateEvents()
        {
            Events.Clear();
            if (ContestantId == null) return;
            if (CurrentOlympiadId == null) return;

            var context = new DataEntities();
            var olympiadId = int.Parse(CurrentOlympiadId);
            var contestantId = int.Parse(ContestantId);
            var entrants = context.Entrants
                .Join(context.Events, e => e.Game_Code, g => g.Code, (e, g) => new { e = e, g = g })
                .Where(x => x.e.OlympiadId == olympiadId && x.g.OlympiadId == olympiadId && x.e.Mind_Sport_ID == contestantId)
                .OrderBy(x => x.e.Game_Code).ToList();       
            foreach (var e in entrants)
            {
                Events.Add(new EventVm()
                {
                    EventId = e.e.EntryNumber,
                    Absent = false,
                    EventCode = e.e.Game_Code,
                    EventName = e.g.Mind_Sport,
                    Fee = e.e.Fee.HasValue ? e.e.Fee.Value : 0,
                    Medal = e.e.Medal ?? "",
                    Partner = e.e.Partner ?? "",
                    Penta = e.e.Penta_Score.HasValue ? e.e.Penta_Score.Value.ToString() : "",
                    Rank = e.e.Rank.HasValue ? e.e.Rank.Value : 0,
                    Receipt = e.e.Receipt.Value,
                    TieBreak = e.e.Tie_break ?? ""
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
                DateOfBirth = "";
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
            }
            else
            {
                var context = new DataEntities();
                var dbCon = context.Contestants.FirstOrDefault(x => x.Mind_Sport_ID == id);
                Title = dbCon.Title;
                FirstName = dbCon.Firstname;
                LastName = dbCon.Lastname;
                Initials = dbCon.Initials;
                IsMale = dbCon.Male.HasValue ? dbCon.Male.Value : true;
                Nationality = dbCon.Nationality;
                DayPhone = dbCon.DayPhone;
                EvePhone = dbCon.EvePhone;
                Fax = dbCon.Fax;
                WantsNoNews = dbCon.No_News.HasValue ? dbCon.No_News.Value : true;
                DateOfBirth = (dbCon.DateofBirth.HasValue) ? dbCon.DateofBirth.Value.ToString("dd/mm/yyyy") : "";
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
            }
            IsDirty = false;
            PopulateEvents();
        }

        public void Save()
        {
            var context = new DataEntities();
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
                    DateofBirth = (string.IsNullOrEmpty(this.DateOfBirth)) ? (DateTime?)null : DateTime.Parse(this.DateOfBirth),
                    Concessional = this.IsConcessional,
                    Address1 = this.Address1,
                    Address2 = this.Address2,
                    City = this.City,
                    County = this.County,
                    PostCode = this.Postcode,
                    Country = this.Country,
                    BCFCode = this.BCFCode,
                    FIDECode = this.FIDECode,
                    Notes = this.Notes,
                };
                context.Contestants.Add(dbCon);
                id = dbCon.Mind_Sport_ID;
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
                DateTime dt;
                var success = DateTime.TryParse(DateOfBirth, out dt);
                if (success) dbCon.DateofBirth = dt;
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
            }
            context.SaveChanges();
            IsDirty = false;
            PopulateDropdown();
            ContestantId = id.ToString();
        }
    }
}
