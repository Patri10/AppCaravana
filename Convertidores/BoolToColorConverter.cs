using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AppCaravana.Convertidores
{
    /// <summary>
    /// Convierte un booleano (StockDescargado) a color de estado.
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDownloaded)
            {
                // Si está descargado (true), usa verde (éxito).
                if (isDownloaded)
                {
                    return new SolidColorBrush(Color.FromRgb(22, 163, 74)); // green-600
                }
                // Si está pendiente (false), usa rojo (alerta).
                else
                {
                    return new SolidColorBrush(Color.FromRgb(220, 38, 38)); // red-600
                }
            }
            return new SolidColorBrush(Color.FromRgb(107, 114, 128)); // gray-500
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}