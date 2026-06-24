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

/// <plan>
/// 将 RootSplitView 设置为标题栏
/// 穿透 NavigationViewBackButton 区域
/// 无需穿透 NavigationViewCloseButton 区域，因为与 NavigationViewBackButton 重叠
/// 穿透 TogglePaneButton 区域
/// 穿透 MenuItemsScrollViewer 区域
/// 穿透 FooterItemsScrollViewer 区域
/// 无需穿透未使用区域：PaneTitlePresenter、AutoSuggestArea、PaneCustomContentBorder、FooterContentBorder
/// 自行实现 TitleBar 图标以优化效果
/// </plan>
public sealed partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        // 导航按钮不居中，需要手动刷新一下
        _navView.IsPaneOpen = false;
        _navView.IsPaneOpen = true;

        _windowService = ServiceLocator.GetService<IWindowService>();
    }

    private readonly IWindowService _windowService;
    private readonly List<FrameworkElement> _interactableElements = [];
    private RectInt32[] _previousPassthroughRects = [];

    private Type HomePage => typeof(HomePage);
    private Type DevicesPage => typeof(DevicesPage);
    private Type WavesPage => typeof(WavesPage);
    private Type GamesPage => typeof(GamesPage);
    private Type SafetyPage => typeof(SafetyPage);

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        _navView.SelectedItem = _navView.MenuItems[0];

        var splitView = VisualTreeHelperExtension.FindChildByName(_navView, "RootSplitView") as SplitView;
        if (splitView is not null)
        {
            var service = ServiceLocator.GetService<ITitleBarService>();
            service.SetTitleBar(splitView);

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
            _interactableElements.Add(_contentFrame);

            UpdateDragRegion();
        }
    }

    private void UpdateDragRegion()
    {
        var passthroughRects = (from element in _interactableElements
                                let rect = FrameworkElementHelper.GetBounds(element)
                                where rect.X >= 0 || rect.Y >= 0
                                select rect).ToArray();

        if (passthroughRects.AsSpan().SequenceEqual(_previousPassthroughRects.AsSpan()))
            return;

        _previousPassthroughRects = passthroughRects;
        var nonClientPointerSource = _windowService.GetInputNonClientPointerSource();
        if (passthroughRects.Length > 0)
        {
            nonClientPointerSource.SetRegionRects(NonClientRegionKind.Passthrough, passthroughRects);
        }
        else
        {
            nonClientPointerSource.ClearRegionRects(NonClientRegionKind.Passthrough);
        }
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            _contentFrame.Navigate(typeof(SettingsPage), null, args.RecommendedNavigationTransitionInfo);
        }
        else if (args.SelectedItemContainer is not null)
        {
            Type navPageType = args.SelectedItemContainer.Tag as Type ?? HomePage;
            _contentFrame.Navigate(navPageType, null, args.RecommendedNavigationTransitionInfo);
        }
    }

    private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (!_contentFrame.CanGoBack)
            return;
        if (_navView.IsPaneOpen
            && (_navView.DisplayMode == NavigationViewDisplayMode.Compact
            || _navView.DisplayMode == NavigationViewDisplayMode.Minimal))
            return;
        _contentFrame.GoBack();
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        if (_contentFrame.SourcePageType is null)
            return;

        if (_contentFrame.SourcePageType == typeof(SettingsPage))
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

    private void UpdateViewLayout()
    {
        if (!_titleBar.IsLoaded)
            return;

        if (_navView.DisplayMode == NavigationViewDisplayMode.Minimal)
        {
            _titleBar.Margin = new Thickness(84, 0, 0, 0);
        }
        else
        {
            _titleBar.Margin = new Thickness(44, 0, 0, 0);
        }
    }

    private void TitleBar_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateViewLayout();
    }

    private void NavView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        UpdateViewLayout();
    }

    private void NavView_LayoutUpdated(object sender, object e)
    {
        if (_navView.IsLoaded)
        {
            UpdateDragRegion();
        }
    }
}
