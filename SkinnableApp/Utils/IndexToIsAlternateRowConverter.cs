using System;
using System.Windows.Data;

namespace Common.Converters
{
    public sealed class IndexToIsAlternateRowConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int index = (int)value;
            return (index % 2 == 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Convert back is not used in the binding
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        #endregion
    }
}
