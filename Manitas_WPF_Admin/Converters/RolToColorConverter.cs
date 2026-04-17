using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Manitas_WPF_Admin.Converters
{
    public class RolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string rol = value?.ToString().ToLower() ?? "";
            if (rol.Contains("manita")) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEDD5"));
            if (rol.Contains("cliente")) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0F2FE"));
            return new SolidColorBrush(Colors.Transparent);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}