using CommunityToolkit.Mvvm.Messaging;
using Dianty.Services;
using Dianty.Utils;
using Dianty.Utils.Messages;
using Dianty.ViewModels;
using Dianty.Views;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Graphics;
using WinRT.Interop;

namespace Dianty;

public sealed partial class MainWindow : Window, ITitleBarService, IWindowService, IQueueService, ITemplateContent
{
    public MainWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        SetWindowSize();
    }

    private readonly Dictionary<Windows.System.VirtualKey, bool> _keyStatus = [];

    private ITemplateContent TemplateContent => this;

    public MainViewModel? ViewModel { get; set; }

    public ElementTheme ColorTheme
    {
        get
        {
            if (Content is FrameworkElement rootElement)
                return rootElement.RequestedTheme;
            else
                return ElementTheme.Default;
        }

        set
        {
            if (Content is FrameworkElement rootElement && rootElement.RequestedTheme != value)
                rootElement.RequestedTheme = value;
        }
    }

    public InputNonClientPointerSource? GetInputNonClientPointerSource()
    {
        if (AppWindow == null)
            return null;
        return InputNonClientPointerSource.GetForWindowId(AppWindow.Id);
    }

    public InputActivationListener? GetInputActivationListener()
    {
        if (AppWindow == null)
            return null;
        return InputActivationListener.GetForWindowId(AppWindow.Id);
    }

    public ContentDialog? CreateContentDialog()
    {
        if (Content == null || Content.XamlRoot == null)
            return null;
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            RequestedTheme = ColorTheme
        };
        return dialog;
    }

    void IQueueService.TryEnqueue(DispatcherQueueHandler callback)
    {
        DispatcherQueue?.TryEnqueue(callback);
    }

    void IQueueService.TryEnqueue(DispatcherQueuePriority priority, DispatcherQueueHandler callback)
    {
        DispatcherQueue?.TryEnqueue(priority, callback);
    }

    object? ITemplateContent.CreateContent(object? item)
    {
        if (item is bool value && value)
        {
            return new MainView(this, this, this, ServiceLocator.GetService<ILocalizationService>());
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
        _rootElement.ActualThemeChanged += OnRootElementActualThemeChanged;
    }

    private void RootElement_Unloaded(object sender, RoutedEventArgs e)
    {
        _rootElement.ActualThemeChanged -= OnRootElementActualThemeChanged;
    }

    private void OnRootElementActualThemeChanged(FrameworkElement sender, object args)
    {
        TitleBarHelper.ApplySystemThemeToCaptionButtons(this, _rootElement.ActualTheme);
    }

    [LibraryImport("user32.dll", EntryPoint = "PostMessageA")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    private const uint WM_NCMOUSEMOVE = 0x00A0;
    private const int HTCAPTION = 2;
    private void RootElement_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        // 当鼠标指针从标题栏按钮移到非客户区的穿透区域时，标题栏按钮仍会处于指针悬停状态
        // 因此需要发送消息提醒窗口鼠标指针已离开标题栏按钮
        // 实测 WM_NCMOUSELEAVE 消息不能解决此问题，可能是因为整个窗口都是非客户区
        // 并且 InputNonClientPointerSource 的 PointerEntered 和 PointerExited 事件参数不会出现 Passthrough 的区域类型，只能以这种方法实现了
        var hWnd = WindowNative.GetWindowHandle(this);
        PostMessage(hWnd, WM_NCMOUSEMOVE, HTCAPTION, nint.Zero);
    }

    private void RootElement_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (FocusManager.GetFocusedElement(Content.XamlRoot) is TextBox)
        {
            _rootElement.Focus(FocusState.Programmatic);
        }
    }

    private void RootElement_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        var key = e.Key;
        if ((int)key < 65 || (int)key > 90 || FocusManager.GetFocusedElement(Content.XamlRoot) is TextBox)
            return;

        if (_keyStatus.TryGetValue(key, out var wasKeyDown))
        {
            if (wasKeyDown)
            {
                return;
            }
        }
        _keyStatus[key] = true;
        WeakReferenceMessenger.Default.Send(new KeyDownMessage(key));
    }

    private void RootElement_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        var key = e.Key;
        if (_keyStatus.ContainsKey(key))
        {
            _keyStatus[key] = false;
        }
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
