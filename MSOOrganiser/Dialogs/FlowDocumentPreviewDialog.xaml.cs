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
        Action<FlowDocument> _print;
        Func<FlowDocument> _generate;

        public FlowDocumentPreviewDialog(Func<FlowDocument> generateDocument, Action<FlowDocument> printDocument)
        {
            InitializeComponent();
            _generate = generateDocument;
            _print = printDocument;
            // We do it this was as we can't print the FlowDocument once it's been
            // assigned to the docViewer - we need a fresh one
            docViewer.Document = _generate();
        }

        // For designer
        public FlowDocumentPreviewDialog()
        {
            InitializeComponent();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            _print(_generate());
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
