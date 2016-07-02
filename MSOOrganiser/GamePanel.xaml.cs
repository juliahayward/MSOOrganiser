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
using MSOOrganiser.UIUtilities;
using System.Configuration;
using JuliaHayward.Common.Logging;
using System.Data.Entity.Validation;

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

        public GamePanelVm ViewModel
        {
            get { return DataContext as GamePanelVm; }
        }


        public void Populate()
        {
            ViewModel.PopulateDropdown();
        }

        private void gameCombo_Changed(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.PopulateGame();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PopulateGame();
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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
    }

    public class GamePanelVm : VmBase
    {
        public class GameVm
        {
            public string Text { get; set; }
            public int Value { get; set; }
        }

        #region bindable properties

        public ObservableCollection<GameVm> Games { get; set; }
        public ObservableCollection<GameCategoryVm> Categories { get; set; }

        private int _gameId;
        public int GameId
        {
            get
            {
                return _gameId;
            }
            set
            {
                if (_gameId != value)
                {
                    _gameId = value;
                    OnPropertyChanged("GameId");
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
        private int _CategoryId;
        public int CategoryId
        {
            get
            {
                return _CategoryId;
            }
            set
            {
                if (_CategoryId != value)
                {
                    _CategoryId = value;
                    IsDirty = true;
                    OnPropertyChanged("CategoryId");
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

        public class GameCategoryVm
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public GamePanelVm()
        {
            Games = new ObservableCollection<GameVm>();
            GameId = 0;

            Categories = new ObservableCollection<GameCategoryVm>();
        }

        public void PopulateDropdown()
        {
            Games.Clear();
            var context = DataEntitiesProvider.Provide();

            Categories.Clear();
            Categories.Add(new GameCategoryVm() { Id = 0, Name = "(No category)" });
            foreach (var c in context.GameCategories.Select(x => new GameCategoryVm { Id = x.Id, Name = x.NAME }))
                Categories.Add(c);

            Games.Add(new GameVm { Text = "New Game", Value = 0 });
            foreach (var game in context.Games.OrderBy(x => x.Code))
                Games.Add(new GameVm { Text = game.Code + " : " + game.Mind_Sport, Value = game.Id });
        }

        public void PopulateGame()
        {
            var id = GameId;
            if (id == 0)
            {
                Name = "";
                Code = "";
                Contacts = "";
                Equipment = "";
                Rules = "";
                CategoryId = 0;
            }
            else
            {
                var context = DataEntitiesProvider.Provide();
                var dbGame = context.Games.FirstOrDefault(x => x.Id == id);
                Name = dbGame.Mind_Sport;
                Code = dbGame.Code;
                Contacts = dbGame.Contacts;
                Equipment = dbGame.Equipment;
                Rules = dbGame.Rules;
                CategoryId = dbGame.CategoryId ?? 0;
            }
            IsDirty = false;
        }

        public List<string> Validate()
        {
            var context = DataEntitiesProvider.Provide();
            
            var errors = new List<string>();
            if (GameId == 0 && context.Games.Any(x => x.Code == Code))
                errors.Add("Code " + Code + " is already being used");
            return errors;
        }

        public void Save()
        {
            bool dropdownRefreshNeeded = false;
            var context = DataEntitiesProvider.Provide();
            var id = GameId;
            Game dbGame;
            if (id == 0)
            {
                dbGame = new Game()
                {
                    Mind_Sport = this.Name,
                    Code = this.Code,
                    Contacts = this.Contacts,
                    Equipment = this.Equipment,
                    Rules = this.Rules,
                    CategoryId = (this.CategoryId == 0) ? (int?)null : this.CategoryId
                };
                context.Games.Add(dbGame);
                dropdownRefreshNeeded = true;
            }
            else
            {
                dbGame = context.Games.FirstOrDefault(x => x.Id == id);
                dbGame.Mind_Sport = Name;
                dbGame.Code = Code;
                dbGame.Contacts = (Contacts == "") ? null : Contacts;   // TODO zero length contraints
                dbGame.Equipment = (Equipment == "") ? null : Equipment;
                dbGame.Rules = (Rules == "") ? null : Rules;
                dbGame.CategoryId = (this.CategoryId == 0) ? (int?)null : this.CategoryId;
            }
            context.SaveChanges();
            IsDirty = false;

            // So we don't have to do a full refresh of the combo
            if (dropdownRefreshNeeded)
                Games.Add(new GameVm { Text = dbGame.Code + " : " + dbGame.Mind_Sport, Value = dbGame.Id });

            GameId = dbGame.Id;
        }
    }
}
