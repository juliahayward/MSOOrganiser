using AutoUpdaterForWPF;
using Microsoft.Win32;
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
using System.IO;

namespace MSOOrganiser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;

        private bool _updateCheckDone = false;
        private DispatcherTimer _databaseCheckTimer = new DispatcherTimer();

        private int LoggedInUserId { get; set; }
        private int UserLoginId { get; set; }

        public delegate void UserLoggedInEventHandler(object sender, EventArgs e);
        public static event UserLoggedInEventHandler UserLoggedIn; 

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

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
            SwitchToEventPanel(e.EventCode, e.OlympiadId);
        }

        public void SwitchToEventPanel(string EventCode, int olympiadId = 0)
        {
            if (olympiadId == 0)
            {
                var context = DataEntitiesProvider.Provide();
                olympiadId = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First().Id;
            }
            using (new SpinnyCursor())
            {
                var panel = new EventPanel();
                panel.Populate(EventCode, olympiadId);
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
            MessageBox.Show("Now on the website");
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

        private void entrySummaryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Func<FlowDocument> generate = () =>
            {
                var printer = new PrintEventEntriesSummaryReportPrinter();
                return printer.GenerateDocument();
            };
            
            Action<FlowDocument> print = doc =>
            {
                var docPrinter = new FlowDocumentPrinter();
                docPrinter.PrintFlowDocument(doc);
            };

            var previewer = new FlowDocumentPreviewDialog(generate, print);
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
            Func<FlowDocument> generate = () =>
            {
                var printer = new MedalTablePrinter();
                return printer.GenerateDocument();
            };
            
            Action<FlowDocument> print = doc =>
            {
                var docPrinter = new FlowDocumentPrinter();
                docPrinter.PrintFlowDocument(doc);
            };

            var previewer = new FlowDocumentPreviewDialog(generate, print);
            previewer.ShowDialog();
        }

        private void eventsWithPrizes_Click(object sender, RoutedEventArgs e)
        {
            Func<FlowDocument> generate = () =>
                {
                    var printer = new EventsWithPrizesPrinter();
                    return printer.GenerateDocument();
                };
            Action<FlowDocument> print = doc =>
            {
                var docPrinter = new FlowDocumentPrinter();
                docPrinter.PrintFlowDocument(doc);
            };

            var previewer = new FlowDocumentPreviewDialog(generate, print);
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

        private void pentamindStandings_Click(object sender, RoutedEventArgs e)
        {
            Func<FlowDocument> generate = () =>
            {
                var printer = new PentamindStandingsPrinter();
                return printer.Print();
            };
            Action<FlowDocument> print = doc =>
                {
                    var flowDocumentPrinter = new FlowDocumentPrinter();
                    flowDocumentPrinter.PrintFlowDocument(doc, includeFooter: false);
                };
            FlowDocumentPreviewDialog dialog = new FlowDocumentPreviewDialog(generate, print);
            dialog.ShowDialog();
        }

        private void juniorPentamindStandings_Click(object sender, RoutedEventArgs e)
        {
            Func<FlowDocument> generate = () =>
            {
                var printer = new PentamindStandingsPrinter();
                return printer.PrintJunior();
            };
            Action<FlowDocument> print = doc =>
            {
                var flowDocumentPrinter = new FlowDocumentPrinter();
                flowDocumentPrinter.PrintFlowDocument(doc, includeFooter: false);
            };
            FlowDocumentPreviewDialog dialog = new FlowDocumentPreviewDialog(generate, print);
            dialog.ShowDialog();
        }

        private void seniorPentamindStandings_Click(object sender, RoutedEventArgs e)
        {
            Func<FlowDocument> generate = () =>
            {
                var printer = new PentamindStandingsPrinter();
                return printer.PrintSenior();
            };
            Action<FlowDocument> print = doc =>
            {
                var flowDocumentPrinter = new FlowDocumentPrinter();
                flowDocumentPrinter.PrintFlowDocument(doc, includeFooter: false);
            };
            FlowDocumentPreviewDialog dialog = new FlowDocumentPreviewDialog(generate, print);
            dialog.ShowDialog();
        }

        private void pokerStandings_Click(object sender, RoutedEventArgs e)
        {
            Func<FlowDocument> generate = () =>
            {
                var printer = new PokerStandingsPrinter();
                return printer.Print();
            };
            Action<FlowDocument> print = doc =>
            {
                var flowDocumentPrinter = new FlowDocumentPrinter();
                flowDocumentPrinter.PrintFlowDocument(doc, includeFooter: false);
            };
            FlowDocumentPreviewDialog dialog = new FlowDocumentPreviewDialog(generate, print);
            dialog.ShowDialog();
        }

        private void eurogamesStandings_Click(object sender, RoutedEventArgs e)
        {
            Func<FlowDocument> generate = () =>
            {
                var printer = new PentamindStandingsPrinter();
                return printer.PrintEuro();
            };
            Action<FlowDocument> print = doc =>
            {
                var flowDocumentPrinter = new FlowDocumentPrinter();
                flowDocumentPrinter.PrintFlowDocument(doc, includeFooter: false);
            };
            FlowDocumentPreviewDialog dialog = new FlowDocumentPreviewDialog(generate, print);
            dialog.ShowDialog();
        }

        private void modernAbstractStandings_Click(object sender, RoutedEventArgs e)
        {
            Func<FlowDocument> generate = () =>
            {
                var printer = new PentamindStandingsPrinter();
                return printer.PrintModernAbstract();
            };
            Action<FlowDocument> print = doc =>
            {
                var flowDocumentPrinter = new FlowDocumentPrinter();
                flowDocumentPrinter.PrintFlowDocument(doc, includeFooter: false);
            };
            FlowDocumentPreviewDialog dialog = new FlowDocumentPreviewDialog(generate, print);
            dialog.ShowDialog();
        }

        private void eventIncomeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Func<FlowDocument> generate = () =>
            {
                    var printer = new EventIncomeReportPrinter();
                    return printer.Print(true);
                };
            Action<FlowDocument> print = doc =>
            {
                var flowDocumentPrinter = new FlowDocumentPrinter();
                flowDocumentPrinter.PrintFlowDocument(doc);
            };
            FlowDocumentPreviewDialog dialog = new FlowDocumentPreviewDialog(generate, print);
            dialog.ShowDialog();
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
            Func<FlowDocument> generate = () =>
            {
                var printer = new PeopleOwingMoneyReportPrinter();
                return printer.Print(true);
            };
            Action<FlowDocument> print = doc =>
            {
                var flowDocumentPrinter = new FlowDocumentPrinter();
                flowDocumentPrinter.PrintFlowDocument(doc);
            };
            FlowDocumentPreviewDialog dialog = new FlowDocumentPreviewDialog(generate, print);
            dialog.ShowDialog();
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
                Func<FlowDocument> generate = () =>
                    {
                        var printer = new TodaysEventsPrinter();
                        return printer.Print(dlg.SelectedDate);
                    };
                Action<FlowDocument> print = doc =>
                {
                    var docPrinter = new FlowDocumentPrinter();
                    docPrinter.PrintFlowDocument(doc);
                };

                var previewer = new FlowDocumentPreviewDialog(generate, print);
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

        private void prizeFormsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Func<FlowDocument> generate = () =>
            {
                var printer = new PrizeFormsPrinter();
                return printer.Print();
            };
            Action<FlowDocument> print = doc =>
            {
                MessageBox.Show("Please ensure that YELLOW paper is in the printer");
                var docPrinter = new FlowDocumentPrinter();
                docPrinter.PrintFlowDocument(doc, includeFooter: false);
                MessageBox.Show("Please take the yellow paper out of the printer now");
            };

            var dlg = new FlowDocumentPreviewDialog(generate, print);
            dlg.ShowDialog();          
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

        private void prepayments_Click(object sender, RoutedEventArgs e)
        {
            // This was for 2017 and prior, where stuff was written to the DB
            PaymentProcessorResult result;
            using (new SpinnyCursor())
            {
                var processor = new PaymentProcessor();
                result = processor.ProcessAll();
            }
            MessageBox.Show("Loaded " + result.SingleEventOrders + " single-event orders and "
                + Environment.NewLine + result.MaxFeeOrders + " all-you-can-play orders");
        }

        private void prepayments2018_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("You can do this via the web now!");

            var dlg = new OpenFileDialog();
            dlg.FileName = "Data File";
            dlg.DefaultExt = ".json";
            dlg.Filter = "JSON documents |*.json";

            var result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                int ppResult;
                using (new SpinnyCursor())
                {
                    var processor = new PaymentProcessor2018();
                    var orders = processor.ParseJsonFile(filename);
                    processor.ProcessAll(orders);

                    MessageBox.Show("Loaded " + orders.Count() + " orders ");
                }
            }
        }

        private void extractEmails_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.FileName = "Data File";
            dlg.DefaultExt = ".json";
            dlg.Filter = "JSON documents |*.json";

            var result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                int ppResult;
                using (new SpinnyCursor())
                {
                    var processor = new PaymentProcessor2018();
                    var orders = processor.ParseJsonFile(filename);
                    var dumpFilename = filename.Substring(0, filename.LastIndexOf(@"\") + 1) + "emails.csv";
                    processor.ExtractEmails(orders, dumpFilename);

                    MessageBox.Show("Extracted emails");
                }
            }
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

        private void birthday_Click(object sender, RoutedEventArgs e)
        {
            var context = DataEntitiesProvider.Provide();
            var olympiadId = context.Olympiad_Infoes.First(x => x.Current).Id;
            var today = DateTime.Now.Date;

            var names = context.Entrants.Where(x => x.OlympiadId == olympiadId)
                .Select(x => x.Name).Distinct().ToList()
                .Where(c => c.DateofBirth.HasValue && c.DateofBirth.Value.Month == today.Month && c.DateofBirth.Value.Day == today.Day)
                .Select(c => c.FullName() + " (" + (today.Year - c.DateofBirth.Value.Year) + ")");

            if (names.Any())
            {
                MessageBox.Show("The following contestants have birthdays today:"
                    + Environment.NewLine + string.Join(Environment.NewLine, names));
            }
            else
            {
                MessageBox.Show("There are no contestants with birthdays today");
            }
        }

        private void freezeMetaGames_Click(object sender, RoutedEventArgs e)
        {
            var metaEventFreezer = new MetaEventFreezer();
            metaEventFreezer.FreezeMetaEvents();
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
            loginBox.Owner = this;
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
            var contestantIds = new List<int>() { 10975
,10979
,11386
,11387
,11388
,11389
,11390
,11391
,11392
,11393
,11394
,11395
,11397 };

            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.First(x => x.Current);
            var entries = context.Entrants.Where(x => contestantIds.Contains(x.Name.Mind_Sport_ID) 
                && x.OlympiadId == currentOlympiad.Id).ToList();

            var costapportioner = new CostApportioner<Entrant>(x => x.Fee, (x, f) => x.Fee = f, x => true);
            costapportioner.ApportionCost(entries, 869.89m);
            context.SaveChanges();

            foreach (var contestantId in contestantIds)
            {
                var contestant = context.Contestants.First(x => x.Mind_Sport_ID == contestantId);
                var owed = entries.Where(x => x.Name.Mind_Sport_ID == contestantId).Sum(x => x.Fee);
                var payment = new Payment()
                {
                    Banked = 2017,
                    MindSportsID = contestantId,
                    Name = contestant,
                    OlympiadId = currentOlympiad.Id,
                    Payment_Method = "Group cheque (Korea)",
                    Year = 2017,
                    Payment1 = owed,
                    Received = DateTime.Now
                };
                context.Payments.Add(payment);
            }

            context.SaveChanges();
        }

        private void allocateSpanishPayment_Click(object sender, RoutedEventArgs e)
        {
            var contestantIds = new List<int>() { 
            11431,11433,11434,11435,11436,11437,11438
            };

            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.First(x => x.Current);
            var entries = context.Entrants.Where(x => contestantIds.Contains(x.Name.Mind_Sport_ID)
                && x.OlympiadId == currentOlympiad.Id
                && x.Fee > 0).ToList();

            var costapportioner = new CostApportioner<Entrant>(x => x.Fee, (x, f) => x.Fee = f, x => true);
            costapportioner.ApportionCost(entries, 586.80m);
            context.SaveChanges();

            foreach (var contestantId in contestantIds)
            {
                var contestant = context.Contestants.First(x => x.Mind_Sport_ID == contestantId);
                var owed = entries.Where(x => x.Name.Mind_Sport_ID == contestantId).Sum(x => x.Fee);
                var payment = new Payment()
                {
                    Banked = 2017,
                    MindSportsID = contestantId,
                    Name = contestant,
                    OlympiadId = currentOlympiad.Id,
                    Payment_Method = "Group cheque (Paco)",
                    Year = 2017,
                    Payment1 = owed,
                    Received = DateTime.Now
                };
                context.Payments.Add(payment);
            }

            context.SaveChanges();
        }

        private void calculateSeedings_Click(object sender, RoutedEventArgs e)
        {
            // dead
        }

        private void calculateElos_Click(object sender, RoutedEventArgs e)
        {
            using (new SpinnyCursor())
            {
                var calculator = new SeedingScoreCalculator();
                calculator.CalculateRatings();
            }
        }

        private void importDR_Click(object sender, RoutedEventArgs e)
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.First(x => x.Current);
            var evt = context.Events.First(x => x.EIN == 2527);

            using (var fr = new StreamReader(new FileStream("C:\\users\\julia.hayward\\desktop\\drrj.txt", FileMode.Open, FileAccess.Read)))
            {
                while (!fr.EndOfStream)
                {
                    var line = fr.ReadLine();
                    var pieces = line.Split('\t');
                    var rank = pieces[0].Trim();
                    string firstname, lastname;
                    if (pieces[1].Contains(";"))
                    {
                        lastname = pieces[1].Substring(0, pieces[1].IndexOf(";")).Trim();
                        firstname = pieces[1].Substring(pieces[1].IndexOf(";") + 1).Trim();
                    }
                    else if (pieces[1].Trim().Contains(" "))
                    {
                        lastname = pieces[1].Substring(0, pieces[1].Trim().IndexOf(" ")).Trim();
                        firstname = pieces[1].Substring(pieces[1].Trim().IndexOf(" ") + 1).Trim();
                    }
                    else
                    {
                        lastname = pieces[1].Trim();
                        firstname = "";
                    }
                    var nationality = pieces[3].Trim();
                    var yob = pieces[2].Trim();
                    DateTime? dob = (yob.Length > 1) ? new DateTime(int.Parse(yob), 1, 1) : (DateTime?)null;
                    var username = pieces[5].Trim();
                    var score = pieces[4].Trim();
                    bool female = (pieces[6].Contains("f"));

                    Contestant c = context.Contestants.FirstOrDefault(x => x.Lastname == lastname && x.Firstname == firstname);

                    if (c == null)
                    {
                        c = new Contestant()
                        {
                            Firstname = firstname,
                            Lastname = lastname,
                            Male = !female,
                            DateofBirth = dob,
                            Nationality = nationality,
                            OnlineNicknames = "playelephant: " + username
                        };
                        context.Contestants.Add(c);
                    }

                    var ent = new Entrant()
                    {
                        Event = evt,
                        Score = score,
                        Tie_break = (200 - int.Parse(rank)).ToString(),
                        Game_Code="DRRJ",
                        OlympiadId=25
                    };

                    c.Entrants.Add(ent);
                    
                    context.SaveChanges();
                }
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

