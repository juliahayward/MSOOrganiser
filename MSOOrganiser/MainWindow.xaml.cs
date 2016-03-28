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
using MSOOrganiser.Reports;
using MSOOrganiser.Dialogs;
using MSOOrganiser.UIUtilities;
using System.Diagnostics;

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
        }

        private void window_Loaded(object sender, EventArgs e)
        {
            var loginBox = new LoginWindow();
            loginBox.ShowDialog();
            if (loginBox.UserId == 0)
            {
                MessageBox.Show("Invalid user/password");
                this.Close();
            }
            LoggedInUserId = loginBox.UserId;
            UserLoginId = loginBox.UserLoginId;
            this.Title += " --- logged in as " +loginBox.UserName;

            if (dockPanel.Children.Count > 2) dockPanel.Children.RemoveAt(2);
            var panel = new StartupPanel();
            dockPanel.Children.Add(panel);
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var context = new DataEntities();
            var user = context.Users.Find(LoggedInUserId);
            var login = user.UserLogins.Where(x => x.Id == UserLoginId && x.LogOutDate == null)
                .OrderByDescending(x => x.LogInDate).FirstOrDefault();
            if (login != null)
            {
                login.LogOutDate = DateTime.UtcNow;
                context.SaveChanges();
            }
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
            panel.EventSelected += panel_EventSelected;
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
            var panel = new EventPanel();
            panel.Populate(e.EventCode, e.OlympiadId);
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
            var panel = new EventPanel();
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

        private void printEventEntriesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EventEntriesReportPicker();
            if (dialog.ShowDialog().Value)
            {
                var printer = new PrintEventEntriesReportPrinter();
                if (dialog.UseEvent)
                    printer.Print(dialog.EventCode);
                else
                    printer.Print(dialog.StartDate, dialog.EndDate);
            }
        }

        private void parkingListsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This does not appear to work in the Access version - please contact Julia");
        }

        private void entrySummaryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var printer = new PrintEventEntriesSummaryReportPrinter();
            printer.Print();
        }

        private void contactsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var printer = new PrintContactsReportPrinter();
            printer.Print();
        }

        private void maxFeePentaCards_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This does not appear to work in the Access version - please contact Julia");
        }

        private void medalTable_Click(object sender, RoutedEventArgs e)
        {
            var printer = new MedalTablePrinter();
            printer.Print();
        }

        private void donationsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var printer = new DonationPrinter();
            printer.Print();
        }

        private void medalFormsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EventEntriesReportPicker();
            if (dialog.ShowDialog().Value)
            {
                var printer = new MedalFormsPrinter();
                if (dialog.UseEvent)
                    printer.Print(dialog.EventCode);
                else
                    printer.Print(dialog.StartDate, dialog.EndDate);
            }
        }

        private void about_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AboutDialog();
            dialog.ShowDialog();
        }

        private void pentamindStandings_Click(object sender, RoutedEventArgs e)
        {
            var printer = new PentamindStandingsPrinter();
            printer.Print();
        }

        private void eventIncomeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var printer = new EventIncomeReportPrinter();
            printer.Print(true);
        }

        private void nonEventIncomeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var printer = new EventIncomeReportPrinter();
            printer.Print(false);
        }

        private void gamePlanMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var printer = new GamePlanPrinter();
            printer.Print();
        }

        private void locationUse_Click(object sender, RoutedEventArgs e)
        {
            var printer = new LocationUsePrinter();
            printer.Print();
        }

        private void printTodaysEventsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SelectDateDialog();
            if (dlg.ShowDialog().Value)
            {
                var printer = new TodaysEventsPrinter();
                printer.Print(dlg.SelectedDate);
            }
        }

        private void daysReportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // This is a combination of existing reports
            //  1. Events per Session (for the right day; not normally independent)
            //  2. Todays Events (for the right day)
            //  3.
            //  4.
            //  5.  Traffic report (for the right day)
            //  6.  Income summary
            //  7.  Entry Summary
            //  8.  Medal table
            //  9.  Event Entries (for the right day)
        }

        private void trafficReport_Click(object sender, RoutedEventArgs e)
        {
            var printer = new TrafficReportPrinter();
            printer.Print();
        }

        private void printEventsPerSessionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var printer = new TrafficReportPrinter();
            printer.PrintEventsPerSession();
        }

        private void contestantList_Click(object sender, RoutedEventArgs e)
        {
            var exporter = new ContestantListCsvExporter();
            exporter.ExportThisYearsContestants();
        }

        private void help_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.juliahayward.com/MSO/Help.html");
        }
    }
}
