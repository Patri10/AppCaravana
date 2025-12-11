using System;
using System.Globalization;
using System.Windows.Data;

namespace AppCaravana.Convertidores
{
    /// <summary>
    /// Convierte un booleano a "Sí" o "No".
    /// </summary>
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Sí" : "No";
            }
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                return strValue.Equals("Sí", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}
