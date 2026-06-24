using Dianty.Services;
using Dianty.Utils;
using Dianty.ViewModels;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Dianty;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window, ITitleBarService, IWindowService
{
    public MainWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1200, 800));
        TitleBarHelper.ApplySystemThemeToCaptionButtons(this, _rootElement.ActualTheme);
        _rootElement.ActualThemeChanged += (_, _) => TitleBarHelper.ApplySystemThemeToCaptionButtons(this, _rootElement.ActualTheme);
    }

    public MainViewModel? ViewModel { get; set; }

    public InputNonClientPointerSource GetInputNonClientPointerSource()
    {
        return InputNonClientPointerSource.GetForWindowId(AppWindow.Id);
    }

#if false
    // 以下代码需至少 Windows11 21H2(22000) 起作用
    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute, out int @return);

    private void SetBorderColor(int color)
    {
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        DwmSetWindowAttribute(hWnd, 34, ref color, sizeof(int), out int hr);
#if DEBUG
        Marshal.ThrowExceptionForHR(hr);
#endif
    }
#endif
}
