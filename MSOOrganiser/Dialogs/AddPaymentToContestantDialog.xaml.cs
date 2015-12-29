using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace MSOOrganiser.Dialogs
{
    /// <summary>
    /// Interaction logic for AddPaymentToContestantDialog.xaml
    /// </summary>
    public partial class AddPaymentToContestantDialog : Window
    {
        public AddPaymentToContestantDialog()
        {
            InitializeComponent();
            DataContext = new AddPaymentToContestantVm();
        }

        public AddPaymentToContestantVm ViewModel
        {
            get { return (AddPaymentToContestantVm)DataContext; }
        }

        public AddPaymentToContestantVm Payment { get { return ViewModel; } }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void addEvent_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }

    public class AddPaymentToContestantVm
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public bool Banked { get; set; }
    }
}
