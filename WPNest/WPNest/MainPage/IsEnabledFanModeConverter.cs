using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WPNest
{
    public class IsEnabledFanModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FanMode)
            {
                var fanMode = (FanMode)value;
                if (fanMode == FanMode.Auto || fanMode == FanMode.On)
                    return true;
                else
                {
                    return false;
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
