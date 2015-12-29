using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MSOOrganiser.Converters
{
    public class ShowCurrencyConverter: IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) 
        {
            if ((decimal)value == 0)
                return "";
            else
                return ((decimal)value).ToString("C");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((string)value == "")
                return 0;
            else
                return decimal.Parse(((string)value).Replace("£", ""));
        }
    }
}
