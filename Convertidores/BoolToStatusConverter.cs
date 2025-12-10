using System;
using System.Globalization;
using System.Windows.Data;

namespace AppCaravana.Convertidores
{
    /// <summary>
    /// Convierte un booleano (StockDescargado) a texto de estado.
    /// </summary>
    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDownloaded)
            {
                return isDownloaded ? "OK" : "PENDIENTE";
            }
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}