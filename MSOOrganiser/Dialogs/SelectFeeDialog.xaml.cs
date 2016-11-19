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
    /// Interaction logic for AddLocationDialog.xaml
    /// </summary>
    public partial class SelectFeeDialog : Window
    {
        public SelectFeeDialog()
        {
            InitializeComponent();
            FeeTextBox.Focus();

            DataContext = new SelectFeeDialogVm();
        }

        public SelectFeeDialogVm ViewModel
        {
            get { return (SelectFeeDialogVm)DataContext; }
        }

        public decimal Fee { get { return ViewModel.Fee; } }


        private void ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }

    public class SelectFeeDialogVm
    {
        public decimal Fee { get; set; }
    }
}
