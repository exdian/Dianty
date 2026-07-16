using Microsoft.UI.Xaml.Data;
using System;

namespace Dianty.Utils;

public partial class QrCodeSvgPathConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string svgPath)
        {
            try
            {
                return SvgPathConverter.Parse(svgPath.AsMemory());
            }
            catch { }
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
