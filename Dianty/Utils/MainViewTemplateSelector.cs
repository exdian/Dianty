using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dianty.Utils;

internal partial class LoadViewTemplateSelector : DataTemplateSelector
{
    public DataTemplate? LoadTemplate { get; set; }
    public DataTemplate? MainTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        if (item is bool isLoaded && isLoaded)
        {
            return MainTemplate!;
        }
        else
        {
            return LoadTemplate!;
        }
    }
}
