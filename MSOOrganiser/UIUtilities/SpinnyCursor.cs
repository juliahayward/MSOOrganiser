using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MSOOrganiser.UIUtilities
{
    public class SpinnyCursor : IDisposable
    {
        public SpinnyCursor()
        {
            Mouse.OverrideCursor = Cursors.Wait;
        }

        public void Dispose()
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }
    }
}
