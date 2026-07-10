using Microsoft.UI.Xaml.Data;
using System;

namespace Dianty.Utils;

public partial class IntToBatteryIconConverter : IValueConverter
{
    private const string Unknown = "\uF608";
    private const string Level_0_19 = "\uECB9";
    private const string Level_20_39 = "\uECBA";
    private const string Level_40_51 = "\uECBB";
    private const string Level_52_63 = "\uECBC";
    private const string Level_64_75 = "\uECBD";
    private const string Level_76_87 = "\uECBE";
    private const string Level_88_100 = "\uECBF";

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not int batteryLevel)
            return Unknown;
        return batteryLevel switch
        {
            >= 0 and <= 19 => Level_0_19,
            >= 20 and <= 39 => Level_20_39,
            >= 40 and <= 51 => Level_40_51,
            >= 52 and <= 63 => Level_52_63,
            >= 64 and <= 75 => Level_64_75,
            >= 76 and <= 87 => Level_76_87,
            >= 88 and <= 100 => Level_88_100,
            _ => Unknown,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
