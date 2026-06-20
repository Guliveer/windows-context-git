using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GitContextMenu.UI.Converters;

[ValueConversion(typeof(bool), typeof(Brush))]
public sealed class IsErrorToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true
            ? new SolidColorBrush(Color.FromRgb(0xF3, 0x8B, 0xA8)) // red
            : new SolidColorBrush(Color.FromRgb(0xCD, 0xD6, 0xF4)); // text

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
