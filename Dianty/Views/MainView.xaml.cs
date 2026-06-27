using Dianty.Services;
using Dianty.Utils;
using Dianty.Views.Pages;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Graphics;

namespace Dianty.Views;

public sealed partial class MainView : UserControl
{
    public MainView(ITitleBarService titleBarService, IWindowService windowService)
    {
        InitializeComponent();
        _titleBarService = titleBarService;
        _windowService = windowService;
        // 导航按钮不居中，需要手动刷新一下
        _navView.IsPaneOpen = false;
        _navView.IsPaneOpen = true;
    }

    private readonly ITitleBarService _titleBarService;
    private readonly IWindowService _windowService;
    private readonly List<FrameworkElement> _interactableElements = [];
    private RectInt32[] _previousPassthroughRects = [];
    private Button? _backButton;
    private Button? _closePaneButton;
    private Button? _togglePaneButton;

    private Type HomePage => typeof(HomePage);
    private Type DevicesPage => typeof(DevicesPage);
    private Type WavesPage => typeof(WavesPage);
    private Type GamesPage => typeof(GamesPage);
    private Type SafetyPage => typeof(SafetyPage);
    private static Type SettingsPage => typeof(SettingsPage);

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        _navView.SelectedItem = _navView.MenuItems[0];

        var splitView = VisualTreeHelperExtension.FindChildByName(_navView, "RootSplitView") as SplitView;
        if (splitView is not null)
        {
            _titleBarService.SetTitleBar(splitView);

            string[] interactableElementNames =
                ["NavigationViewBackButton", "TogglePaneButton", "MenuItemsScrollViewer", "FooterItemsScrollViewer"];
            for (int i = 0; i < interactableElementNames.Length; i++)
            {
                var element = VisualTreeHelperExtension
                    .FindChildByName(_navView, interactableElementNames[i]) as FrameworkElement;
                if (element is not null)
                {
                    _interactableElements.Add(element);
                    if (element is ScrollViewer scrollViewer)
                    {
                        scrollViewer.VerticalAlignment = VerticalAlignment.Top;
                    }
                }
            }

            UpdateDragRegion();
            UpdateIconRegion();

            // 图标区域在窗口发生交互时很可能会被重置，因此需要重新设置
            var nonClientPointerSource = _windowService.GetInputNonClientPointerSource();
            if (nonClientPointerSource is not null)
            {
                nonClientPointerSource.ExitedMoveSize += MainView_ExitedMoveSize;
                _appIcon.Unloaded += (_, _) => nonClientPointerSource.ExitedMoveSize -= MainView_ExitedMoveSize;
            }

            var activationListener = _windowService.GetInputActivationListener();
            if (activationListener is not null)
            {
                activationListener.InputActivationChanged += ActivationListener_InputActivationChanged;
                _appIcon.Unloaded += (_, _) => activationListener.InputActivationChanged -= ActivationListener_InputActivationChanged;
            }

            // 获取所有可能位于标题栏区域的元素
            _backButton = VisualTreeHelperExtension.FindChildByName(_navView, "NavigationViewBackButton") as Button;
            _closePaneButton = VisualTreeHelperExtension.FindChildByName(_navView, "NavigationViewCloseButton") as Button;
            _togglePaneButton = VisualTreeHelperExtension.FindChildByName(_navView, "TogglePaneButton") as Button;
        }
    }

    private void UpdateDragRegion()
    {
        var rects = new List<RectInt32>(_interactableElements.Count + 1);

        var rect = FrameworkElementHelper.GetBounds(_contentFrame);
        if (_navView.DisplayMode != NavigationViewDisplayMode.Expanded && _navView.IsPaneOpen)
        {
            // 面板以浮动状态打开时，会遮住部分内容，因此需要裁剪此穿透区域
            double scale = _contentFrame.XamlRoot.RasterizationScale;
            var leftBorder = (int)(_navView.OpenPaneLength * scale);
            if (rect.X < leftBorder)
            {
                int newX = leftBorder;
                int newWidth = rect.Width - (newX - rect.X);
                rect.X = newX;
                rect.Width = newWidth < 0 ? 0 : newWidth;
            }
        }
        rects.Add(rect);

        foreach (var element in _interactableElements)
        {
            rect = FrameworkElementHelper.GetBounds(element);
            if (rect.X >= 0 || rect.Y >= 0)
            {
                rects.Add(rect);
            }
        }

        var passthroughRects = rects.ToArray();

        if (passthroughRects.AsSpan().SequenceEqual(_previousPassthroughRects.AsSpan()))
            return;

        _previousPassthroughRects = passthroughRects;
        var nonClientPointerSource = _windowService.GetInputNonClientPointerSource();
        if (passthroughRects.Length > 0)
        {
            nonClientPointerSource?.SetRegionRects(NonClientRegionKind.Passthrough, passthroughRects);
        }
        else
        {
            nonClientPointerSource?.ClearRegionRects(NonClientRegionKind.Passthrough);
        }
    }

    private void UpdateIconRegion()
    {
        var nonClientPointerSource = _windowService.GetInputNonClientPointerSource();
        if (nonClientPointerSource is null)
            return;

        var rect = FrameworkElementHelper.GetBounds(_appIcon);
        if ((rect.X < 0 && rect.Y < 0)
            || (_navView.DisplayMode != NavigationViewDisplayMode.Expanded && _navView.IsPaneOpen))
        {
            // 面板以浮动状态打开时，会遮住标题栏图标
            nonClientPointerSource?.ClearRegionRects(NonClientRegionKind.Icon);
        }
        else
        {
            nonClientPointerSource?.SetRegionRects(NonClientRegionKind.Icon, [rect]);
        }
    }

    private void MainView_ExitedMoveSize(InputNonClientPointerSource sender, ExitedMoveSizeEventArgs args)
    {
        // 改变窗口位置时
        UpdateIconRegion();
    }

    private void ActivationListener_InputActivationChanged(InputActivationListener sender, InputActivationListenerActivationChangedEventArgs args)
    {
        bool isDeactivated = sender.State == InputActivationState.Deactivated;
        VisualStateManager.GoToState(this, isDeactivated ? "Deactivated" : "Activated", false);

        // 当窗口处于非活动状态时，所有标题栏元素都应为半透明
        if (_navView.DisplayMode == NavigationViewDisplayMode.Minimal)
        {
            _backButton?.Opacity = isDeactivated ? 0.5 : 1;
            _closePaneButton?.Opacity = isDeactivated ? 0.5 : 1;
            _togglePaneButton?.Opacity = isDeactivated ? 0.5 : 1;
        }
        else
        {
            _backButton?.Opacity = isDeactivated ? 0.5 : 1;
            _closePaneButton?.Opacity = isDeactivated ? 0.5 : 1;
        }
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            var viewModel = ServiceLocator.GetViewModel(SettingsPage);
            _contentFrame.Navigate(SettingsPage, viewModel, args.RecommendedNavigationTransitionInfo);
        }
        else if (args.SelectedItemContainer is not null)
        {
            Type navPageType = args.SelectedItemContainer.Tag as Type ?? HomePage;
            var viewModel = ServiceLocator.GetViewModel(navPageType);
            _contentFrame.Navigate(navPageType, viewModel, args.RecommendedNavigationTransitionInfo);
        }
    }

    private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (!_contentFrame.CanGoBack)
            return;
        if (_navView.IsPaneOpen
            && (_navView.DisplayMode == NavigationViewDisplayMode.Compact
            || _navView.DisplayMode == NavigationViewDisplayMode.Minimal))
        {
            return;
        }

        _contentFrame.GoBack();
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        if (_contentFrame.SourcePageType is null)
            return;

        if (_contentFrame.SourcePageType == SettingsPage)
        {
            var selectedItem = (NavigationViewItem)_navView.SettingsItem;
            if (!ReferenceEquals(selectedItem, _navView.SelectedItem))
                _navView.SelectedItem = selectedItem;
        }
        else
        {
            var selectedItem = _navView.MenuItems
                .OfType<NavigationViewItem>()
                .First(i => i.Tag.Equals(_contentFrame.SourcePageType));
            if (!ReferenceEquals(selectedItem, _navView.SelectedItem))
                _navView.SelectedItem = selectedItem;
        }
    }

    private void NavView_LayoutUpdated(object sender, object e)
    {
        UpdateDragRegion();
        UpdateIconRegion();
    }

    private void NavView_PaneChanged(NavigationView sender, object args)
    {
        UpdateDragRegion();
        UpdateIconRegion();
    }
}
