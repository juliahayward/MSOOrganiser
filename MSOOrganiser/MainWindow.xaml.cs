using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
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
using System.Data.OleDb;
using System.Data;
using MSOCore;

namespace MSOOrganiser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int LoggedInUserId { get; set; }
        private int UserLoginId { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            var loginBox = new LoginWindow();
            loginBox.ShowDialog();
            if (loginBox.UserId == 0)
            {
                MessageBox.Show("Invalid user/password");
                this.Close();
            }
            LoggedInUserId = loginBox.UserId;
            UserLoginId = loginBox.UserLoginId;

            this.Title += " --- logged in as " + loginBox.UserName;

            if (dockPanel.Children.Count > 2) dockPanel.Children.RemoveAt(2);
            var panel = new StartupPanel();
            dockPanel.Children.Add(panel);
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var context = new DataEntities();
            var user = context.Users.Find(LoggedInUserId);
            var login = user.UserLogins.First(x => x.Id == UserLoginId);
            login.LogOutDate = DateTime.UtcNow;
            context.SaveChanges();
        }


        private void changePasswordMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var passwordDialog = new PasswordChangeWindow() { UserId = LoggedInUserId };
            passwordDialog.ShowDialog();
        }

        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void olympiadsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (dockPanel.Children.Count > 2) dockPanel.Children.RemoveAt(2);
            var panel = new OlympiadPanel();
            panel.Populate();
            dockPanel.Children.Add(panel);
        }

        private void competitorsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (dockPanel.Children.Count > 2) dockPanel.Children.RemoveAt(2);
            var panel = new ContestantPanel();
            panel.Populate();
            panel.EventSelected += panel_EventSelected;
            dockPanel.Children.Add(panel);
        }

        void panel_EventSelected(object sender, Events.EventEventArgs e)
        {
            if (dockPanel.Children.Count > 2) dockPanel.Children.RemoveAt(2);
            var panel = new ResultsPanel();
            panel.Populate(e.EventCode);
            panel.ContestantSelected += panel_ContestantSelected;
            dockPanel.Children.Add(panel);
        }


        private void gamesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (dockPanel.Children.Count > 2) dockPanel.Children.RemoveAt(2);
            var panel = new GamePanel();
            panel.Populate();
            dockPanel.Children.Add(panel);
        }

        private void nationalitiesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            if (dockPanel.Children.Count > 2) dockPanel.Children.RemoveAt(2);
            var panel = new NationalityReport();
           // panel.Populate();
            dockPanel.Children.Add(panel);

            Cursor = Cursors.Arrow;
        }

        private void resultsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (dockPanel.Children.Count > 2) dockPanel.Children.RemoveAt(2);
            var panel = new ResultsPanel();
            panel.Populate();
            panel.ContestantSelected += panel_ContestantSelected;
            dockPanel.Children.Add(panel);
        }

        void panel_ContestantSelected(object sender, Events.ContestantEventArgs e)
        {
            if (dockPanel.Children.Count > 2) dockPanel.Children.RemoveAt(2);
            var panel = new ContestantPanel();
            panel.Populate(e.ContestantId);
            panel.EventSelected += panel_EventSelected;
            dockPanel.Children.Add(panel);
        }

        private void displayResultsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (dockPanel.Children.Count > 2) dockPanel.Children.RemoveAt(2);
            var panel = new DisplayResultsPanel();
            dockPanel.Children.Add(panel);
        }

        
        

    }
}
