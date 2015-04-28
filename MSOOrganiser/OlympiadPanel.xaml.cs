using MSOCore.Models;
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
    public partial class OlympiadPanel : UserControl
    {
  

        public OlympiadPanel()
        {
            InitializeComponent();
            DataContext = new OlympiadPanelVm();
        }


        public void Populate()
        {
            ((OlympiadPanelVm)DataContext).PopulateDropdown();
        }

        private void olympiadCombo_Changed(object sender, SelectionChangedEventArgs e)
        {
            ((OlympiadPanelVm)DataContext).PopulateGame();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            ((OlympiadPanelVm)DataContext).PopulateGame();
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            ((OlympiadPanelVm)DataContext).Save();
        }
    }

    public class OlympiadPanelVm : VmBase
    {

        public class OlympiadVm
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }

        #region bindable properties
        public ObservableCollection<OlympiadVm> Olympiads { get; set; }
        public string OlympiadId { get; set; }

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

        private string _YearOf;
        public string YearOf 
        { 
            get 
            {
                return _YearOf; 
            }
            set
            {
                if (_YearOf != value)
                {
                    _YearOf = value;
                    IsDirty = true;
                    OnPropertyChanged("YearOf");
                }
            }
        }
        private string _Number;
        public string Number
        {
            get
            {
                return _Number;
            }
            set
            {
                if (_Number != value)
                {
                    _Number = value;
                    IsDirty = true;
                    OnPropertyChanged("Number");
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
        private string _Venue;
        public string Venue
        {
            get
            {
                return _Venue;
            }
            set
            {
                if (_Venue != value)
                {
                    _Venue = value;
                    _IsDirty = true;
                    OnPropertyChanged("Venue");
                }
            }
        }
        private string _StartDate;
        public string StartDate
        {
            get
            {
                return _StartDate;
            }
            set
            {
                if (_StartDate != value)
                {
                    _StartDate = value;
                    _IsDirty = true;
                    OnPropertyChanged("StartDate");
                }
            }
        }
        private string _FinishDate;
        public string FinishDate
        {
            get
            {
                return _FinishDate;
            }
            set
            {
                if (_FinishDate != value)
                {
                    _FinishDate = value;
                    _IsDirty = true;
                    OnPropertyChanged("FinishDate");
                }
            }
        }
        private string _MaxFee;
        public string MaxFee
        {
            get
            {
                return _MaxFee;
            }
            set
            {
                if (_MaxFee != value)
                {
                    _MaxFee = value;
                    _IsDirty = true;
                    OnPropertyChanged("MaxFee");
                }
            }
        }
        private string _MaxCon;
        public string MaxCon
        {
            get
            {
                return _MaxCon;
            }
            set
            {
                if (_MaxCon != value)
                {
                    _MaxCon = value;
                    _IsDirty = true;
                    OnPropertyChanged("MaxCon");
                }
            }
        }
        private string _AgeDate;
        public string AgeDate
        {
            get
            {
                return _AgeDate;
            }
            set
            {
                if (_AgeDate != value)
                {
                    _AgeDate = value;
                    _IsDirty = true;
                    OnPropertyChanged("AgeDate");
                }
            }
        }
        private string _JnrAge;
        public string JnrAge
        {
            get
            {
                return _JnrAge;
            }
            set
            {
                if (_JnrAge != value)
                {
                    _JnrAge = value;
                    _IsDirty = true;
                    OnPropertyChanged("JnrAge");
                }
            }
        }
        private string _SnrAge;
        public string SnrAge
        {
            get
            {
                return _SnrAge;
            }
            set
            {
                if (_SnrAge != value)
                {
                    _SnrAge = value;
                    _IsDirty = true;
                    OnPropertyChanged("SnrAge");
                }
            }
        }
         private string _PentaLong;
        public string PentaLong
        {
            get
            {
                return _PentaLong;
            }
            set
            {
                if (_PentaLong != value)
                {
                    _PentaLong = value;
                    _IsDirty = true;
                    OnPropertyChanged("PentaLong");
                }
            }
        }
            private string _PentaTotal;
        public string PentaTotal
        {
            get
            {
                return _PentaTotal;
            }
            set
            {
                if (_PentaTotal != value)
                {
                    _PentaTotal = value;
                    _IsDirty = true;
                    OnPropertyChanged("PentaTotal");
                }
            }
        }
        #endregion

        public OlympiadPanelVm()
        {
            Olympiads = new ObservableCollection<OlympiadVm>();
            OlympiadId = "0";
        }

        public void PopulateDropdown()
        {
            Olympiads.Clear();
            var context = new DataEntities();
            Olympiads.Add(new OlympiadVm { Text = "New Olympiad", Value = "0" });
            foreach (var o in context.Olympiad_Infoes.OrderByDescending(x => x.StartDate))
                Olympiads.Add(new OlympiadVm { Text = o.FullTitle(), Value = o.Id.ToString() });
        }

        public void PopulateGame()
        {
            var id = int.Parse(OlympiadId);
            if (id == 0)
            {
                YearOf = "";
                Number = "";
                Title = ""; Venue = ""; StartDate = ""; FinishDate = ""; MaxFee = ""; MaxCon = ""; AgeDate= "";
                JnrAge = ""; SnrAge = "";
                PentaLong = ""; PentaTotal = "";
            }
            else
            {
                var context = new DataEntities();
                var o = context.Olympiad_Infoes.FirstOrDefault(x => x.Id == id);
                YearOf = o.YearOf.ToString();
                Number = o.Number;
                Title = o.Title;
                Venue = o.Venue;
                StartDate = o.StartDate.Value.ToString("dd/MM/yyyy");
                FinishDate = o.FinishDate.Value.ToString("dd/MM/yyyy");
                MaxFee = o.MaxFee.Value.ToString("F2");
                MaxCon = o.MaxCon.Value.ToString("F2");
                AgeDate = o.AgeDate.ToString();
                JnrAge = o.JnrAge.ToString();
                SnrAge = o.SnrAge.ToString();
                PentaLong = o.PentaLong.ToString();
                PentaTotal = o.PentaTotal.ToString();
            }
            IsDirty = false;
        }

        public void Save()
        {
            var context = new DataEntities();
            var id = int.Parse(OlympiadId);
            if (id == 0)
            {
                var o = new Olympiad_Info()
                {
                    YearOf = int.Parse(this.YearOf),
                    Number = this.Number,
                    Title = this.Title,
                    Venue = this.Venue,
                    StartDate = DateTime.Parse(this.StartDate),
                    FinishDate = DateTime.Parse(this.FinishDate),
                    MaxFee = decimal.Parse(this.MaxFee),
                    MaxCon = decimal.Parse(this.MaxCon),
                    AgeDate = DateTime.Parse(this.AgeDate),
                    JnrAge = int.Parse(this.JnrAge),
                    SnrAge = int.Parse(this.SnrAge),
                    PentaLong = int.Parse(this.PentaLong),
                    PentaTotal = int.Parse(this.PentaTotal)
                };
                context.Olympiad_Infoes.Add(o);
                id = o.Id;
            }
            else
            {
                var o = context.Olympiad_Infoes.FirstOrDefault(x => x.Id == id);
                o.YearOf = int.Parse(this.YearOf);
                o.Number = this.Number;
                o.Title = this.Title;
                o.Venue = this.Venue;
                o.StartDate = DateTime.Parse(this.StartDate);
                o.FinishDate = DateTime.Parse(this.FinishDate);
                o.MaxFee = decimal.Parse(this.MaxFee);
                o.MaxCon = decimal.Parse(this.MaxCon);
                o.AgeDate = DateTime.Parse(this.AgeDate);
                o.JnrAge = int.Parse(this.JnrAge);
                o.SnrAge = int.Parse(this.SnrAge);
                o.PentaLong = int.Parse(this.PentaLong);
                o.PentaTotal = int.Parse(this.PentaTotal);
            }
            context.SaveChanges();
            IsDirty = false;
            PopulateDropdown();
            OlympiadId = id.ToString();
        }
    }
}
