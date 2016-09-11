using MSOCore;
using MSOCore.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
    /// Interaction logic for SummaryPanel.xaml
    /// </summary>
    public partial class SummaryPanel : UserControl
    {
        public SummaryPanel()
        {
            InitializeComponent();

            DataContext = new SummaryPanelVm();

            ViewModel.Populate();
        }

        public SummaryPanelVm ViewModel
        {
            get { return DataContext as SummaryPanelVm; }
        }
    }

    public class SummaryPanelVm : VmBase
    {
        public class EventVm
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Dates { get; set; }
            public string NumCompetitors { get; set; }
            public string DateAndCompetitors
            {
                get
                {
                    return Dates + "     " + NumCompetitors + " competitors";
                }
            }
            public double FractionDone { get; set; }
            public string Status { get; set; }
        }

        public double ProgressValue { get; set; }
        public double ProgressMaximum { get; set; }
        public ObservableCollection<EventVm> Events { get; set; }

        public void Populate()
        {
            Events = new ObservableCollection<EventVm>();

            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.First(x => x.Current);
            var events = context.Events.Where(x => x.OlympiadId == currentOlympiad.Id && x.Number > 0);
            foreach (var evt in events.OrderBy(x => x.Number).ToList())
            {
                Events.Add(new EventVm() 
                { 
                    Name = evt.ShortName(), 
                    Id = evt.Number,
                    Dates = (evt.Start.HasValue) ? evt.Start.Value.ToString("ddd dd MMM HH:mm") + " - " + 
                        evt.End.Value.ToString("HH:mm")
                        : "Dates unknown",
                    NumCompetitors = evt.Entrants.Count(x => !x.Absent).ToString(),
                    FractionDone = evt.FractionDone(),
                    Status = evt.Status()
                });
            }

            ProgressMaximum = Events.Count();
            ProgressValue = Events.Sum(x => x.FractionDone); 
        }
    }
}
