using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MSOOrganiser.Converters
{
    public class Show3DecPlacesConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) 
        {
            if ((float)value == 0)
                return "";
            else
                return ((float)value).ToString("0.000");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((string)value == "")
                return 0;
            else
                return float.Parse((string)value);
        }
    }
}
