using Microsoft.UI.Xaml.Data;
using System;

namespace Dianty.Utils;

public partial class WaveIntervalConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not int interval)
            return string.Empty;
        return $" {interval / 10f}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
