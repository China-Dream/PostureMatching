// -----------------------------------------------------------------------
// Converters.cs
// -----------------------------------------------------------------------

namespace WpfViewers
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Multiplies a double by a double.
    /// </summary>
    public class DoubleScalerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double valAsDouble = ObjectToDouble(value, 0);
            double factor = ObjectToDouble(parameter, 0);

            return valAsDouble * factor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        internal static double ObjectToDouble(object value, double fallbackValue)
        {
            double retVal = fallbackValue;

            // We need to convert here because the parameter isn't strongly typed.
            // This means that values (int, double, string literals in XAML) won't
            // be auto-converted by the compiler.
            if (value is int)
            {
                retVal = (double)(int)value;
            }
            else if (value is double)
            {
                retVal = (double)value;
            }
            else if (value is string)
            {
                retVal = (double)TypeDescriptor.GetConverter(typeof(double)).ConvertFrom(null, System.Globalization.CultureInfo.InvariantCulture, value);
            }

            return retVal;
        }
    }
}
