using DungeonToolkit;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace Dianty.Utils.Converters;

public sealed partial class DeviceLightsColorBrushConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            LightsColor.None => new SolidColorBrush(Colors.Black),
            LightsColor.Yellow => new SolidColorBrush(Colors.Yellow),
            LightsColor.Red => new SolidColorBrush(Colors.Red),
            LightsColor.Purple => new SolidColorBrush(Colors.Purple),
            LightsColor.Blue => new SolidColorBrush(Colors.Blue),
            LightsColor.Cyan => new SolidColorBrush(Colors.Cyan),
            LightsColor.Green => new SolidColorBrush(Colors.Green),
            LightsColor.White => new SolidColorBrush(Colors.White),
            _ => null,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
