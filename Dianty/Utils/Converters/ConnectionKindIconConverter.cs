using DungeonToolkit.Coyote;
using Microsoft.UI.Xaml.Data;
using System;

namespace Dianty.Utils.Converters;

public sealed partial class ConnectionKindIconConverter : IValueConverter
{
    private const string Unknown = "\uE9CE";
    private const string BLE = "\uE702";
    private const string WebSocket = "\uE753";

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            ConnectionKind.BLE => BLE,
            ConnectionKind.WebSocket => WebSocket,
            _ => Unknown,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
