using Microsoft.UI.Xaml.Data;
using System;

namespace Dianty.Utils;

public partial class IntToBatteryPercentageConverter : IValueConverter
{
    private const string Unknown = "未知";

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not int batteryLevel || batteryLevel < 0 || batteryLevel > 100)
            return Unknown;
        return $"{batteryLevel}%";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
