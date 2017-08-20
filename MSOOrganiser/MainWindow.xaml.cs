using AutoUpdaterForWPF;
using MSOCore;
using MSOCore.Calculators;
using MSOCore.Models;
using MSOOrganiser.Data;
using MSOOrganiser.Dialogs;
using MSOOrganiser.Reports;
using MSOOrganiser.UIUtilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using MSOCore.Reports;
using System.Reflection;

namespace MSOOrganiser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _updateCheckDone = false;
        private DispatcherTimer _databaseCheckTimer = new DispatcherTimer();

        private int LoggedInUserId { get; set; }
        private int UserLoginId { get; set; }

        public delegate void UserLoggedInEventHandler(object sender, EventArgs e);
        public static event UserLoggedInEventHandler UserLoggedIn; 

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowVm(new PaymentProcessor());

            _databaseCheckTimer.Interval = new TimeSpan(0, 0, 20);
            _databaseCheckTimer.Tick += _databaseCheckTimer_Tick;
        }

        public MainWindowVm ViewModel
        {
            get { return DataContext as MainWindowVm;  }
        }

        void _databaseCheckTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var context = DataEntitiesProvider.Provide();
                var thisOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
                ViewModel.DbStatus = "Connected: " + DateTime.Now.ToString("HH:mm:ss");

                ViewModel.CheckForEntries();
            }
            catch (Exception ex)
            {
                ViewModel.DbStatus = "Connection failed: " + DateTime.Now.ToString("HH:mm:ss");
            }
        }

        private void ReplaceMainPanelWith(UIElement panel)
        {
            if (dockPanel.Children.Count > 3) dockPanel.Children.RemoveAt(3);
            dockPanel.Children.Add(panel);
        }

        private void window_Loaded(object sender, EventArgs e)
        {
            CheckForUpdates();
            ConnectionStringUpdater.Update();

            ReplaceMainPanelWith(new StartupPanel());
        }

        private void WriteLoggedInUserToRegistry(string userId)
        {
            var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\JuliaHayward\MSOOrganiser");
            key.SetValue("LastLoginId", userId);
            key.Close();
        }

        private string GetLastLoggedInUser()
        {
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\JuliaHayward\MSOOrganiser");
            if (key == null) return "";

            var value = key.GetValue("LastLoginId");
            key.Close();
            return (string)value ?? "";
        }

        private void CheckForUpdates()
        {
            try
            {
                var autoUpdater = new AutoUpdater();
                var result = autoUpdater.DoUpdate("http://apps.juliahayward.com/MSOOrganiser");
                if (result == AutoUpdateResult.UpdateInitiated)
                {
                    this.Close();
                    Environment.Exit(0);
                }
            }
            catch (Exception) { /* don't bother logging */ }
            _updateCheckDone = true;
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_updateCheckDone)
                return;
            if (LoggedInUserId == 0)
                return;

            logOut_Click(null, null);
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
            using (new SpinnyCursor())
            {
                var panel = new OlympiadPanel();
                panel.Populate();
                panel.EventSelected += panel_EventSelected;
                ReplaceMainPanelWith(panel);
            }
        }

        private void competitorsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            using (new SpinnyCursor())
            {
                var panel = new ContestantPanel();
                panel.Populate();
                panel.EventSelected += panel_EventSelected;
                ReplaceMainPanelWith(panel);
            }
        }

        void panel_EventSelected(object sender, Events.EventEventArgs e)
        {
            using (new SpinnyCursor())
            {
                var panel = new EventPanel();
                panel.Populate(e.EventCode, e.OlympiadId);
                panel.ContestantSelected += panel_ContestantSelected;
                ReplaceMainPanelWith(panel);
            }
        }


        private void gamesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            using (new SpinnyCursor())
            {
                if (dockPanel.Children.Count > 3) dockPanel.Children.RemoveAt(3);
                var panel = new GamePanel();
                panel.Populate();
                dockPanel.Children.Add(panel);
            }
        }

        private void nationalitiesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            var panel = new NationalityReport();
           // panel.Populate();
            ReplaceMainPanelWith(panel);

            Cursor = Cursors.Arrow;
        }

        private void resultsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var panel = new EventPanel();
            panel.Populate();
            panel.ContestantSelected += panel_ContestantSelected;
            ReplaceMainPanelWith(panel);
        }

        void panel_ContestantSelected(object sender, Events.ContestantEventArgs e)
        {
            using (new SpinnyCursor())
            {
                var panel = new ContestantPanel();
                panel.Populate(e.ContestantId);
                panel.EventSelected += panel_EventSelected;
                ReplaceMainPanelWith(panel);
            }
        }

        private void displayResultsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var panel = new DisplayResultsPanel();
            ReplaceMainPanelWith(panel);
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
            var document = printer.GenerateDocument();
            Action action = (() =>
            {
                var docPrinter = new FlowDocumentPrinter();
                docPrinter.PrintFlowDocument(() => printer.GenerateDocument());
            });
            var previewer = new FlowDocumentPreviewDialog(document, action);
            previewer.ShowDialog();
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
            var document = printer.GenerateDocument();
            Action action = (() =>
            {
                var docPrinter = new FlowDocumentPrinter();
                docPrinter.PrintFlowDocument(() => printer.GenerateDocument());
            });
            var previewer = new FlowDocumentPreviewDialog(document, action);
            previewer.ShowDialog();
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
                FlowDocument doc;
                if (dialog.UseEvent)
                    doc = printer.Print(dialog.EventCode);
                else
                    doc = printer.Print(dialog.StartDate, dialog.EndDate);

                var flowDocumentPrinter = new FlowDocumentPrinter();
                flowDocumentPrinter.PrintFlowDocument(() => doc, includeFooter: false);
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

        private void juniorPentamindStandings_Click(object sender, RoutedEventArgs e)
        {
            var printer = new PentamindStandingsPrinter();
            printer.PrintJunior();
        }

        private void seniorPentamindStandings_Click(object sender, RoutedEventArgs e)
        {
            var printer = new PentamindStandingsPrinter();
            printer.PrintSenior();
        }

        private void pokerStandings_Click(object sender, RoutedEventArgs e)
        {
            var printer = new PokerStandingsPrinter();
            printer.Print();
        }

        private void eurogamesStandings_Click(object sender, RoutedEventArgs e)
        {
            var printer = new PentamindStandingsPrinter();
            printer.PrintEuro();
        }

        private void eventIncomeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var docPrinter = new FlowDocumentPrinter();
            var printer = new EventIncomeReportPrinter();
            docPrinter.PrintFlowDocument(() => printer.Print(true));
        }

        private void nonEventIncomeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var docPrinter = new FlowDocumentPrinter();
            var printer = new EventIncomeReportPrinter();
            docPrinter.PrintFlowDocument(() => printer.Print(false));
        }

        private void totalIncomeByMethodMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var docPrinter = new FlowDocumentPrinter();
            var printer = new TotalIncomeByMethodReportPrinter();
            docPrinter.PrintFlowDocument(() => printer.Print(true));
        }

        private void peopleOwingMoneyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var docPrinter = new FlowDocumentPrinter();
            var printer = new PeopleOwingMoneyReportPrinter();
            docPrinter.PrintFlowDocument(() => printer.Print(true));
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

        private void arbiterSchedule_Click(object sender, RoutedEventArgs e)
        {
            var printer = new ArbiterSchedulePrinter();
            printer.Print();
        }


        private void printTodaysEventsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // We mess up the document when we put it in the previewer so need to get it again; 
            // would be nice to cache a copy
            var dlg = new SelectDateDialog();
            if (dlg.ShowDialog().Value)
            {
                var printer = new TodaysEventsPrinter();    
                var document = printer.Print(dlg.SelectedDate);
                Action action = (() =>
                {
                    var docPrinter = new FlowDocumentPrinter();
                    docPrinter.PrintFlowDocument(() => printer.Print(dlg.SelectedDate));
                });
                var previewer = new FlowDocumentPreviewDialog(document, action);
                previewer.ShowDialog();
            }
        }

        private void daysReportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SelectDateDialog();
            if (dlg.ShowDialog().Value)
            {
                var docPrinter = new FlowDocumentPrinter();
                docPrinter.PrintFlowDocuments(() => FlowDocumentsForDay(dlg.SelectedDate));
            }
        }

        private IEnumerable<FlowDocument> FlowDocumentsForDay(DateTime selectedDate)
        {
            //  1. Events per Session (for the right day; not normally independent)
            var printer1 = new TrafficReportPrinter();
            yield return printer1.PrintEventsPerSession(selectedDate);

            //  2. Todays Events (for the right day)
            var printer2 = new TodaysEventsPrinter();
            yield return printer2.Print(selectedDate);

            // TODO TODO TODO 
            // This is a combination of existing reports (1,2,3,5 are for today, rest are as at now
            //  3.

            //  4.  Event Results for today, concatenated
            var printer4 = new TodaysEventsResultsPrinter();
            yield return printer4.Print(selectedDate);

            //  5.  Traffic report (for the right day)
            yield return printer1.Print(selectedDate);

            //  6.  Income summary
            var printer6 = new EventIncomeReportPrinter();
            yield return printer6.Print(true);

            //  7.  Entry Summary
            var printer7 = new PrintEventEntriesSummaryReportPrinter();
            yield return printer7.GenerateDocument();
            
            //  8.  Medal table
            var printer8 = new MedalTablePrinter();
            yield return printer8.GenerateDocument();

            //  9.  Event Entries (for the right day TODO TODO doesn't work in Access??)
        }

        private void trafficReport_Click(object sender, RoutedEventArgs e)
        {
            var docPrinter = new FlowDocumentPrinter();
            var printer = new TrafficReportPrinter();
            docPrinter.PrintFlowDocument(() => printer.Print());
        }

        private void printEventsPerSessionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var docPrinter = new FlowDocumentPrinter();
            var printer = new TrafficReportPrinter();
            docPrinter.PrintFlowDocument(() => printer.PrintEventsPerSession());
        }

        private void eventLabelsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please ensure that Avery 3x6 labels are in the printer");

            var docPrinter = new FlowDocumentPrinter();
            var printer = new EventLabelsPrinter();
            docPrinter.PrintFlowDocument(() => printer.Print(), includeFooter: false);

            MessageBox.Show("Please take the labels out of the printer now");
        }

        private void arbitersBadgesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please ensure that YELLOW paper is in the printer");

            var docPrinter = new FlowDocumentPrinter();
            var printer = new ArbitersBadgesPrinter();
            docPrinter.PrintFlowDocument(() => printer.Print());

            MessageBox.Show("Please take the yellow paper out of the printer now");
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

        private void copyEventsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var context = DataEntitiesProvider.Provide();
            var thisOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();

            if (thisOlympiad.Events.Any())
            {
                MessageBox.Show("You cannot do this if the current Olympiad already has events.");
                return;
            }

            var previousOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).Skip(1).First();

            this.Status.Text = "Copying from " + previousOlympiad.YearOf + " to " + thisOlympiad.YearOf;
            var dateShift = thisOlympiad.StartDate.Value.Subtract(previousOlympiad.StartDate.Value);

            foreach (var evt in previousOlympiad.Events)
            {
                var newE = evt.CopyTo(thisOlympiad);
                // This is a copy, so move the dates along by the same amount
                foreach (var es in newE.Event_Sess)
                {
                    es.Date = es.Date.Value.Add(dateShift);
                }
                thisOlympiad.Events.Add(newE);
            }

            context.SaveChanges();
            this.Status.Text = "";
        }

        private void printResults_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Event results can be printed from the individual event panel");
        }

        private void prepayments_Click(object sender, RoutedEventArgs e)
        {
            PaymentProcessorResult result;
            using (new SpinnyCursor())
            {
                var processor = new PaymentProcessor();
                result = processor.ProcessAll();
            }
            MessageBox.Show("Loaded " + result.SingleEventOrders + " single-event orders and "
                + Environment.NewLine + result.MaxFeeOrders + " all-you-can-play orders");
        }

        private void unspentFee_Click(object sender, RoutedEventArgs e)
        {
            var context = DataEntitiesProvider.Provide();
            var olympiadId = context.Olympiad_Infoes.First(x => x.Current).Id;

            var payments = context.Payments.Where(p => p.OlympiadId == olympiadId)
                .GroupBy(x => x.MindSportsID)
                .ToDictionary(x => x.Key, x => x.Sum(p => p.Payment1));
            var fees = context.Entrants.Where(p => p.OlympiadId == olympiadId)
                .GroupBy(x => x.Mind_Sport_ID)
                .ToDictionary(x => x.Key, x => x.Sum(p => p.Fee));
            var contestants = payments.Where(x => !fees.Keys.Contains(x.Key) || x.Value > fees[x.Key])
                .Select(x => x.Key);

            // Warning - can't do comparison inside SQL as Sum() can be NULL
            var names = context.Contestants.Where(x => contestants.Contains(x.Mind_Sport_ID))
                .ToList()
                .Select(x => x.FullName());


            if (names.Any())
            {
                MessageBox.Show("The following contestants have unspent fees:"
                    + Environment.NewLine + string.Join(Environment.NewLine, names)); 
            }
            else
            {
                MessageBox.Show("There are no contestants with unspent fees");
            }
        }

        private void unpaidFee_Click(object sender, RoutedEventArgs e)
        {
            var context = DataEntitiesProvider.Provide();
            var olympiadId = context.Olympiad_Infoes.First(x => x.Current).Id;

            var payments = context.Payments.Where(p => p.OlympiadId == olympiadId)
                .GroupBy(x => x.MindSportsID)
                .ToDictionary(x => x.Key, x => x.Sum(p => p.Payment1));
            var fees = context.Entrants.Where(p => p.OlympiadId == olympiadId)
                .GroupBy(x => x.Mind_Sport_ID)
                .ToDictionary(x => x.Key, x => x.Sum(p => p.Fee));
            var contestants = fees.Where(x => x.Value > 0 && // They have some fees - not complimentary
                    (!payments.Keys.Contains(x.Key) || x.Value > payments[x.Key]))
                .Select(x => x.Key);

            // Warning - can't do comparison inside SQL as Sum() can be NULL
            var names = context.Contestants.Where(x => contestants.Contains(x.Mind_Sport_ID))
                .ToList()
                .Select(x => x.FullName());

            if (names.Any())
            {
                MessageBox.Show("The following contestants have unpaid fees:"
                    + Environment.NewLine + string.Join(Environment.NewLine, names));
            }
            else
            {
                MessageBox.Show("There are no contestants with unpaid fees");
            }
        }

        private void freezeMetaGames_Click(object sender, RoutedEventArgs e)
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.First(x => x.Current);

            // First of all, for each pentamind qualifier make a PEWC entry
            var pentamindStandingsGenerator = new PentamindStandingsGenerator();
            var standings = pentamindStandingsGenerator.GetStandings(null);
            int rank = 1;
            foreach (var standing in standings.Standings)
            {
                if (!standing.IsValid) continue;

                var contestant = context.Contestants.FirstOrDefault(x => x.Mind_Sport_ID == standing.ContestantId);
                var evt = context.Events.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id && x.Code == "PEWC");
                var entry = context.Entrants.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id 
                                        && x.Game_Code == "PEWC" && x.Mind_Sport_ID == standing.ContestantId);
                if (entry == null)
                {
                    entry = Entrant.NewEntrant(evt.EIN, "PEWC", currentOlympiad.Id, contestant, 0m);
                    context.Entrants.Add(entry);
                }
                entry.Score = standing.TotalScoreStr;
                entry.Rank = rank;
                rank++;
                context.SaveChanges();
            }

            // Next a Eurogames one
            var eurostandings = pentamindStandingsGenerator.GetEuroStandings(null);
            rank = 1;
            foreach (var standing in eurostandings.Standings)
            {
                if (!standing.IsValid) continue;

                var contestant = context.Contestants.FirstOrDefault(x => x.Mind_Sport_ID == standing.ContestantId);
                var evt = context.Events.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id && x.Code == "EGWC");
                var entry = context.Entrants.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id
                                        && x.Game_Code == "EGWC" && x.Mind_Sport_ID == standing.ContestantId);
                if (entry == null)
                {
                    entry = Entrant.NewEntrant(evt.EIN, "EGWC", currentOlympiad.Id, contestant, 0m);
                    context.Entrants.Add(entry);
                }
                entry.Score = standing.TotalScoreStr;
                entry.Rank = rank;
                rank++;
                context.SaveChanges();
            }

            // Next a Poker one
            var pokerStandingsGenerator = new PokerStandingsGenerator();
            var pokerstandings = pokerStandingsGenerator.GetStandings();
            rank = 1;
            foreach (var standing in pokerstandings.Standings)
            {
                if (!standing.IsValid) continue;

                var contestant = context.Contestants.FirstOrDefault(x => x.Mind_Sport_ID == standing.ContestantId);
                var evt = context.Events.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id && x.Code == "POAC");
                var entry = context.Entrants.FirstOrDefault(x => x.OlympiadId == currentOlympiad.Id
                                        && x.Game_Code == "POAC" && x.Mind_Sport_ID == standing.ContestantId);
                if (entry == null)
                {
                    entry = Entrant.NewEntrant(evt.EIN, "POAC", currentOlympiad.Id, contestant, 0m);
                    context.Entrants.Add(entry);
                }
                entry.Score = standing.TotalScoreStr;
                entry.Rank = rank;
                rank++;
                context.SaveChanges();
            }
        }

        private void releaseNotes_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://apps.juliahayward.com/msoorganiser/1.0.2/ReleaseNotes.html");
        }

        private void logIn_Click(object sender, RoutedEventArgs e)
        {
            var context = DataEntitiesProvider.Provide();
            var minVersionStr = context.GlobalSettings.FirstOrDefault(x => x.Name == "MinimumSupportedVersion");
            if (minVersionStr != null)
            {
                var minVersion = Version.Parse(minVersionStr.Value);
                var thisVersion = Assembly.GetExecutingAssembly().GetName().Version;
                if (thisVersion < minVersion)
                {
                    MessageBox.Show("This version is not supported any more because of database changes. Please get an update");
                    return;
                }
            }


            var lastLoggedIn = GetLastLoggedInUser();

            var loginBox = new LoginWindow(lastLoggedIn);
            loginBox.ShowDialog();
            if (loginBox.UserId == 0)
            {
                MessageBox.Show("Invalid user/password");
                return;
            }

            WriteLoggedInUserToRegistry(loginBox.UserName);

            _databaseCheckTimer.Start();

            GlobalSettings.LoggedInUser = loginBox.UserName;
            LoggedInUserId = loginBox.UserId;
            UserLoginId = loginBox.UserLoginId;
            this.Title += " --- logged in as " + loginBox.UserName
                + " --- " + DataEntitiesProvider.Description();

            ViewModel.IsLoggedIn = true;
            UserLoggedIn(this, new EventArgs());
        }

        private void logOut_Click(object sender, RoutedEventArgs e)
        {
            var context = DataEntitiesProvider.Provide();
            var user = context.Users.Find(LoggedInUserId);
            if (user != null)
            {
                var login = user.UserLogins.Where(x => x.Id == UserLoginId && x.LogOutDate == null)
                    .OrderByDescending(x => x.LogInDate).FirstOrDefault();
                if (login != null)
                {
                    login.LogOutDate = DateTime.UtcNow;
                    context.SaveChanges();
                }
            }

            var panel = new StartupPanel();
            ReplaceMainPanelWith(panel);

            ViewModel.IsLoggedIn = false;
        }

        private void allocateKoreanPayment_Click(object sender, RoutedEventArgs e)
        {
/*            var contestantIds = new List<int>() { 10976, 11234, 10972, 11236, 10979, 11233, 10985, 10975, 11235 };

            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.First(x => x.Current);
            var entries = context.Entrants.Where(x => contestantIds.Contains(x.Name.Mind_Sport_ID) 
                && x.OlympiadId == currentOlympiad.Id).ToList();

            var costapportioner = new CostApportioner<Entrant>(x => x.Fee, (x, f) => x.Fee = f, x => true);
            costapportioner.ApportionCost(entries, 212.80m);
            context.SaveChanges();

            foreach (var contestantId in contestantIds)
            {
                var contestant = context.Contestants.First(x => x.Mind_Sport_ID == contestantId);
                var owed = entries.Where(x => x.Name.Mind_Sport_ID == contestantId).Sum(x => x.Fee);
                var payment = new Payment()
                {
                    Banked = 2016,
                    MindSportsID = contestantId,
                    Name = contestant,
                    OlympiadId = currentOlympiad.Id,
                    Payment_Method = "Group cheque (Korea)",
                    Year = 2016,
                    Payment1 = owed,
 * Received = DateTime.Now
                };
                context.Payments.Add(payment);
            }

            context.SaveChanges();
 * */
        }

        private void allocateSpanishPayment_Click(object sender, RoutedEventArgs e)
        {
     /*       var contestantIds = new List<int>() { 9486,
11092,
11187,
11188,
11189,
11190,
11191,
11192,
11193,
11194,
11195,
11196,
11203,
11204 };

            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.First(x => x.Current);
            var entries = context.Entrants.Where(x => contestantIds.Contains(x.Name.Mind_Sport_ID)
                && x.OlympiadId == currentOlympiad.Id
                && x.Fee > 0
                && x.EntryNumber != 32180   // already paid for separately
                && x.EntryNumber != 32186).ToList();

            var costapportioner = new CostApportioner<Entrant>(x => x.Fee, (x, f) => x.Fee = f, x => true);
            costapportioner.ApportionCost(entries, 799.60m);
            context.SaveChanges();

            foreach (var contestantId in contestantIds)
            {
                var contestant = context.Contestants.First(x => x.Mind_Sport_ID == contestantId);
                var owed = entries.Where(x => x.Name.Mind_Sport_ID == contestantId).Sum(x => x.Fee);
                var payment = new Payment()
                {
                    Banked = 2016,
                    MindSportsID = contestantId,
                    Name = contestant,
                    OlympiadId = currentOlympiad.Id,
                    Payment_Method = "Group cheque (Paco)",
                    Year = 2016,
                    Payment1 = owed,
      * Received = DateTime.Now
                };
                context.Payments.Add(payment);
            }

            context.SaveChanges();
       */ }

        private void calculateSeedings_Click(object sender, RoutedEventArgs e)
        {
            using (new SpinnyCursor())
            {
                var calculator = new SeedingScoreCalculator();
                calculator.CalculateSeedings();
            }
        }
    }


    public class MainWindowVm : VmBase
    {
        private readonly PaymentProcessor _paymentProcessor;

        public MainWindowVm(PaymentProcessor paymentProcessor)
        {
            _paymentProcessor = paymentProcessor;
        }

        private bool _isLoggedIn = false;
        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set
            {
                if (_isLoggedIn != value)
                {
                    _isLoggedIn = value;
                    OnPropertyChanged("IsLoggedIn");
                }
            }
        }

        private string _dbStatus = "";

        public string DbStatus
        {
            get { return _dbStatus; }
            set
            {
                if (_dbStatus != value)
                {
                    _dbStatus = value;
                    OnPropertyChanged("DbStatus");
                }
            }
        }

        public void CheckForEntries()
        {
            var context = DataEntitiesProvider.Provide();
            var entries = context.EntryJsons.Where(x => x.ProcessedDate == null && x.Notes == null);
            if (entries.Count() > 0)
            {
                if (GlobalSettings.AutomaticallyLoadEntries)
                {
                    var result = _paymentProcessor.ProcessAll();
                    DbStatus = "Loaded " + result.SingleEventOrders + " orders";
                }
                else
                {
                    DbStatus = "There are online entries ready to load";
                }
            }
        }
    }
}

