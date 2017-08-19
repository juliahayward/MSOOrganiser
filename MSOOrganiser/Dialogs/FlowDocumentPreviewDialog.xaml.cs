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
    /// Interaction logic for FlowDocumentPreviewDialog.xaml
    /// </summary>
    public partial class FlowDocumentPreviewDialog : Window
    {
        Action _print;

        public FlowDocumentPreviewDialog(FlowDocument document, Action print)
        {
            InitializeComponent();
            docViewer.Document = document;
            _print = print;
        }

        // For designer
        public FlowDocumentPreviewDialog()
        {
            InitializeComponent();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            _print();
            this.DialogResult = true;
            this.Close();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
