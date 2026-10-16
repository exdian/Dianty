using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using System;

namespace Dianty.Utils.Converters;

public sealed partial class SvgPathConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string svgPath)
        {
            try
            {
                return XamlBindingHelper.ConvertValue(typeof(Geometry), svgPath);
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
