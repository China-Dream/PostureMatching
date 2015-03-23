// -----------------------------------------------------------------------
// Converters.cs
// -----------------------------------------------------------------------

namespace PostureMatching
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// This enum is used for generic enabled/disabled bindings.
    /// </summary>
    public enum ToggleState
    {
        /// <summary>
        /// The feature is enabled (true).
        /// </summary>
        Enabled,

        /// <summary>
        /// The feature is disabled (false).
        /// </summary>
        Disabled
    }

    /// <summary>
    /// Converts a true bool value to a ToggleState.Enabled.
    /// </summary>
    public class BoolToToggleStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isTrue = ((value is bool) && (bool)value) ^ (null != parameter);
            return isTrue ? ToggleState.Enabled : ToggleState.Disabled;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ToggleState state = (ToggleState)value;
            return state == ToggleState.Enabled;
        }
    }

    /// <summary>
    /// Converts a true bool value to Visiblity.Visibility and everything else to Visibility.Collapsed
    /// This is negated if a non-null parameter is passed.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = ((value is bool) && (bool)value) ^ (null != parameter);
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}