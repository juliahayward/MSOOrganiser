using MSOCore;
using MSOCore.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for SummaryPanel.xaml
    /// </summary>
    public partial class SummaryPanel : UserControl
    {
        public SummaryPanel()
        {
            DataContext = new SummaryPanelVm(); 
            
            InitializeComponent();
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
        }


        public ObservableCollection<EventVm> Events { get; set; }

        public SummaryPanelVm()
        {
            Events = new ObservableCollection<EventVm>();

            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.First(x => x.Current);
            var events = context.Events.Where(x => x.OlympiadId == currentOlympiad.Id && x.Number > 0);
            foreach (var evt in events.OrderBy(x => x.Number))
            {
                Events.Add(new EventVm() { Name = evt.ShortName(), Id = evt.Number });
            }
        }
    }
}
