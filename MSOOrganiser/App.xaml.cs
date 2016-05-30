using MSOOrganiser.Dialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MSOOrganiser
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        /// <summary>
        /// Error handler of last resort
        /// </summary>
        void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            Exception e = args.Exception;
            var dlg = new ErrorDialog(e);
            dlg.ShowDialog();

            var wantsToContinue = (bool)dlg.DialogResult;
            if (!wantsToContinue)
                this.Shutdown();

            args.Handled = true;
        }
    }
}
