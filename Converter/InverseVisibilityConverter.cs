using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PhotoUploader.Converter
{

    [ValueConversion(typeof(Visibility), typeof(Visibility))]
    public class InverseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if ((Visibility)value == Visibility.Visible)
                return Visibility.Collapsed;

            return Visibility.Visible;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            bool booleanvalue = (bool)value;

            if (booleanvalue)
                return Visibility.Collapsed;

            return Visibility.Visible;


        }
    }
}
