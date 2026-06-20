using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using GitContextMenu.Core.Git.Models;

namespace GitContextMenu.UI.Converters;

[ValueConversion(typeof(FileState), typeof(Brush))]
public sealed class FileStateColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is FileState state)
        {
            return state switch
            {
                FileState.Added     => new SolidColorBrush(Color.FromRgb(0xA6, 0xE3, 0xA1)), // green
                FileState.Modified  => new SolidColorBrush(Color.FromRgb(0xF9, 0xE2, 0xAF)), // yellow
                FileState.Deleted   => new SolidColorBrush(Color.FromRgb(0xF3, 0x8B, 0xA8)), // red
                FileState.Renamed   => new SolidColorBrush(Color.FromRgb(0x89, 0xB4, 0xFA)), // blue
                FileState.Copied    => new SolidColorBrush(Color.FromRgb(0x89, 0xDC, 0xEB)), // sky
                FileState.Conflict  => new SolidColorBrush(Color.FromRgb(0xFA, 0xB3, 0x87)), // peach
                FileState.Untracked => new SolidColorBrush(Color.FromRgb(0x6C, 0x70, 0x86)), // overlay
                _                   => Brushes.White,
            };
        }
        return Brushes.White;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
