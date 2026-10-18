using DungeonToolkit.Coyote;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Net.WebSockets;

namespace Dianty.Utils.Converters;

public sealed partial class VisibleIfBluetoothConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is ConnectionKind.BLE ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
