using Dianty.Services;
using Dianty.Utils;
using Dianty.ViewModels;
using Dianty.Views;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using Windows.Graphics;
using WinRT.Interop;

namespace Dianty;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window, ITitleBarService, IWindowService, IQueueService, ITemplateContent
{
    public MainWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        SetWindowSize();
    }

    private ITemplateContent TemplateContent => this;

    public MainViewModel? ViewModel { get; set; }

    public InputNonClientPointerSource? GetInputNonClientPointerSource()
    {
        if (AppWindow is null)
            return null;
        return InputNonClientPointerSource.GetForWindowId(AppWindow.Id);
    }

    public InputActivationListener? GetInputActivationListener()
    {
        if (AppWindow is null)
            return null;
        return InputActivationListener.GetForWindowId(AppWindow.Id);
    }

    public bool TryEnqueue(DispatcherQueueHandler callback)
    {
        return DispatcherQueue.TryEnqueue(callback);
    }

    object? ITemplateContent.CreateContent(object? item)
    {
        if (item is bool value && value)
        {
            return new MainView(this, this);
        }
        else
        {
            return new LoadView();
        }
    }

    [LibraryImport("user32.dll")]
    private static partial uint GetDpiForWindow(nint hWnd);
    private void SetWindowSize()
    {
        var hWnd = WindowNative.GetWindowHandle(this);
        var dpi = GetDpiForWindow(hWnd);
        if (dpi == 0)
            return;

        var scale = dpi / 96.0d;
        var windowWidth = 800 * scale;
        var windowHeight = 520 * scale;
        AppWindow.Resize(new SizeInt32((int)windowWidth, (int)windowHeight));
    }

    private void RootElement_Loaded(object sender, RoutedEventArgs e)
    {
        WindowHelper.SetWindowMinSize(this, 285, 56);
        WindowHelper.SetWindowSize(this, 800, 520);
        TitleBarHelper.ApplySystemThemeToCaptionButtons(this, _rootElement.ActualTheme);
        _rootElement.ActualThemeChanged += (_, _) => TitleBarHelper.ApplySystemThemeToCaptionButtons(this, _rootElement.ActualTheme);
    }

    [LibraryImport("user32.dll", EntryPoint = "PostMessageA")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    public const uint WM_NCMOUSEMOVE = 0x00A0;
    public const int HTCAPTION = 2;
    private void RootElement_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        // 当鼠标指针从标题栏按钮移到非客户区的穿透区域时，标题栏按钮仍会处于指针悬停状态
        // 因此需要发送消息提醒窗口鼠标指针已离开标题栏按钮
        // 实测 WM_NCMOUSELEAVE 消息不能解决此问题，可能是因为整个窗口都是非客户区
        var hWnd = WindowNative.GetWindowHandle(this);
        PostMessage(hWnd, WM_NCMOUSEMOVE, HTCAPTION, nint.Zero);
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
