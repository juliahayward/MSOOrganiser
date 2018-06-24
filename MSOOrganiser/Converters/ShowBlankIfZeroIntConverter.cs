using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MSOOrganiser.Converters
{
    public class ShowBlankIfZeroIntConverter: IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) 
        {
            if ((int)value == 0)
                return "";
            else
                return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || (string)value == "")
                return 0;

            int output;
            if (int.TryParse((string)value, out output))
                return output;
            else
                return 0;
        }
    }
}
