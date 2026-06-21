using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dianty.Utils;

internal partial class LoadViewTemplateSelector : DataTemplateSelector
{
    public DataTemplate? LoadTemplate { get; set; }
    public DataTemplate? MainTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        // ContentControl.Content 被赋值为 null 时，不会执行此处代码，因此只能使用字符串
        if (item is string)
        {
            return LoadTemplate!;
        }
        else
        {
            return MainTemplate!;
        }
    }
}
