using JuliaHayward.Common.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
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
    /// Interaction logic for ErrorDialog.xaml
    /// </summary>
    public partial class ErrorDialog : Window
    {
        public ErrorDialog(Exception ex) : this()
        {
            this.textBox.Text = "";
            var thisEx = ex;
            while (thisEx != null)
            {
                this.textBox.Text = thisEx.Message + Environment.NewLine + thisEx.StackTrace + Environment.NewLine;
                thisEx = thisEx.InnerException;
            }
            var trelloKey = ConfigurationManager.AppSettings["TrelloKey"];
            var trelloAuthKey = ConfigurationManager.AppSettings["TrelloAuthKey"];

            var logger = new TrelloLogger(trelloKey, trelloAuthKey);
            logger.Error("MSOWeb", ex.Message, ex.StackTrace);
        }

        public ErrorDialog()
        {
            InitializeComponent();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void shutdown_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
