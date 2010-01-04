using System;
using System.Windows.Data;

namespace Common.Converters
{
    public sealed class ImageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                //TODO: Implement fix for IE8 breaking images, see http://channel9.msdn.com/ShowPost.aspx?PostID=388896
                return value;
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Convert back is not used in the binding
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        #endregion
    }
}
