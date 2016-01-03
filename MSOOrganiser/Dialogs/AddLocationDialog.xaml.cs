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
    public partial class AddLocationDialog : Window
    {
        public AddLocationDialog()
        {
            InitializeComponent();
            DataContext = new AddLocationDialogVm();
        }

        public AddLocationDialogVm ViewModel
        {
            get { return (AddLocationDialogVm)DataContext; }
        }

        public string Name { get { return ViewModel.Name; } }


        private void add_Click(object sender, RoutedEventArgs e)
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

    public class AddLocationDialogVm
    {
        public string Name { get; set; }
    }
}
