using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using Windows.Graphics;

namespace Dianty.Utils;

public partial class WindowHelper
{
    public static void SetWindowSize(Window window, double width, double height)
    {
        if (window.Content == null || window.Content.XamlRoot == null)
        {
            Debug.WriteLine("窗口 XamlRoot 不存在");
            return;
        }

        var scale = window.Content.XamlRoot.RasterizationScale;
        var windowWidth = width * scale;
        var windowHeight = height * scale;
        window.AppWindow.Resize(new SizeInt32((int)windowWidth, (int)windowHeight));
    }

    // 此部分代码来自 https://github.com/microsoft/WinUI-Gallery
    public static void SetWindowMinSize(Window window, double width, double height)
    {
        if (window.Content is not FrameworkElement windowContent)
        {
            Debug.WriteLine("Window content is not a FrameworkElement.");
            return;
        }

        if (windowContent.XamlRoot == null)
        {
            Debug.WriteLine("Window content's XamlRoot is null.");
            return;
        }

        if (window.AppWindow.Presenter is not OverlappedPresenter presenter)
        {
            Debug.WriteLine("Window's AppWindow.Presenter is not an OverlappedPresenter.");
            return;
        }

        var scale = windowContent.XamlRoot.RasterizationScale;
        var minWidth = width * scale;
        var minHeight = height * scale;
        presenter.PreferredMinimumWidth = (int)minWidth;
        presenter.PreferredMinimumHeight = (int)minHeight;
    }
}
