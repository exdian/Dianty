using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Dianty.Utils;

internal class VisualTreeHelperExtension
{
    public static DependencyObject? FindChildByName(DependencyObject parent, string name)
    {
        // 检查当前节点
        if (parent is FrameworkElement fe && fe.Name == name)
        {
            return parent;
        }

        // 遍历所有子节点
        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            var result = FindChildByName(child, name);
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }
}
