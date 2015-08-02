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
using MSOCore;

namespace MSOOrganiser
{
    /// <summary>
    /// Interaction logic for EventPanel.xaml
    /// </summary>
    public partial class GamePanel : UserControl
    {
  

        public GamePanel()
        {
            InitializeComponent();
            DataContext = new GamePanelVm();
        }


        public void Populate()
        {
            ((GamePanelVm)DataContext).PopulateDropdown();
        }

        private void gameCombo_Changed(object sender, SelectionChangedEventArgs e)
        {
            ((GamePanelVm)DataContext).PopulateGame();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            ((GamePanelVm)DataContext).PopulateGame();
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            ((GamePanelVm)DataContext).Save();
        }
    }

    public class GamePanelVm : VmBase
    {
        public class GameVm
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }

        #region bindable properties
        public ObservableCollection<GameVm> Games { get; set; }
        public string GameId { get; set; }

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

        private string _Name;
        public string Name 
        { 
            get 
            { 
                return _Name; 
            }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    IsDirty = true;
                    OnPropertyChanged("Name");
                }
            }
        }
        private string _Code;
        public string Code
        {
            get
            {
                return _Code;
            }
            set
            {
                if (_Code != value)
                {
                    _Code = value;
                    IsDirty = true;
                    OnPropertyChanged("Code");
                }
            }
        }
        private string _Contacts;
        public string Contacts
        {
            get
            {
                return _Contacts;
            }
            set
            {
                if (_Contacts != value)
                {
                    _Contacts = value;
                    IsDirty = true;
                    OnPropertyChanged("Contacts");
                }
            }
        }
        private string _Equipment;
        public string Equipment
        {
            get
            {
                return _Equipment;
            }
            set
            {
                if (_Equipment != value)
                {
                    _Equipment = value;
                    _IsDirty = true;
                    OnPropertyChanged("Equipment");
                }
            }
        }
        private string _Rules;
        public string Rules
        {
            get
            {
                return _Rules;
            }
            set
            {
                if (_Rules != value)
                {
                    _Rules = value;
                    _IsDirty = true;
                    OnPropertyChanged("Rules");
                }
            }
        }
        #endregion

        public GamePanelVm()
        {
            Games = new ObservableCollection<GameVm>();
            GameId = "0";
        }

        public void PopulateDropdown()
        {
            Games.Clear();
            var context = new DataEntities();
            Games.Add(new GameVm { Text = "New Game", Value = "0" });
            foreach (var game in context.Games.OrderBy(x => x.Code))
                Games.Add(new GameVm { Text = game.Code + " : " + game.Mind_Sport, Value = game.Id.ToString() });
        }

        public void PopulateGame()
        {
            var id = int.Parse(GameId);
            if (id == 0)
            {
                Name = "";
                Code = "";
                Contacts = "";
                Equipment = "";
                Rules = "";
            }
            else
            {
                var context = new DataEntities();
                var dbGame = context.Games.FirstOrDefault(x => x.Id == id);
                Name = dbGame.Mind_Sport;
                Code = dbGame.Code;
                Contacts = dbGame.Contacts;
                Equipment = dbGame.Equipment;
                Rules = dbGame.Rules;
            }
            IsDirty = false;
        }

        public void Save()
        {
            var context = new DataEntities();
            var id = int.Parse(GameId);
            if (id == 0)
            {
                //var dbGame = new Game()
                //{
                //    Name = this.Name,
                //    Code = this.Code,
                //    Contacts = this.Contacts,
                //    Equipment = this.Equipment,
                //    Rules = this.Rules
                //};
                //context.Games.Add(dbGame);
                //id = dbGame.Id;
            }
            else
            {
                //var dbGame = context.Games.FirstOrDefault(x => x.Id == id);
                //dbGame.Name = Name;
                //dbGame.Code = Code;
                //dbGame.Contacts = Contacts;
                //dbGame.Equipment = Equipment;
                //dbGame.Rules = Rules;
            }
            context.SaveChanges();
            IsDirty = false;
            PopulateDropdown();
            GameId = id.ToString();
        }
    }
}
