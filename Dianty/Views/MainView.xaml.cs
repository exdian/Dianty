using CommunityToolkit.Mvvm.Messaging;
using Dianty.Services;
using Dianty.Utils;
using Dianty.Utils.Messages;
using Dianty.Views.Pages;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.Graphics;
using static Dianty.Services.ILocalizationService;

namespace Dianty.Views;

public sealed partial class MainView : UserControl
{
    public MainView(ITitleBarService titleBarService, IWindowService windowService, IQueueService queueService, ILocalizationService localizationService)
    {
        InitializeComponent();
        _titleBarService = titleBarService;
        _windowService = windowService;
        _queueService = queueService;
        _localizationService = localizationService;

        // 导航按钮不居中，需要手动刷新一下
        _navView.IsPaneOpen = false;
        _navView.IsPaneOpen = true;

#if !DEBUG
        _debugMenuItem.Visibility = Visibility.Collapsed;
#endif

        _keySequenceTrigger.AddKeySequence("debug", ToggleDebugMenuItem);
        Loaded += MainView_Loaded;
        Unloaded += MainView_Unloaded;
    }

    private readonly ITitleBarService _titleBarService;
    private readonly IWindowService _windowService;
    private readonly IQueueService _queueService;
    private readonly ILocalizationService _localizationService;
    private readonly List<FrameworkElement> _interactableElements = [];
    private readonly KeySequenceTrigger _keySequenceTrigger = new();
    private RectInt32[] _previousPassthroughRects = [];
    private Button? _backButton;
    private Button? _closePaneButton;
    private Button? _togglePaneButton;

#pragma warning disable CA1822 // 将成员标记为 static
    private Type HomePage => typeof(HomePage);
    private Type DevicesPage => typeof(DevicesPage);
    private Type WavesPage => typeof(WavesPage);
    private Type GamesPage => typeof(GamesPage);
    private Type SafetyPage => typeof(SafetyPage);
    private Type DebugPage => typeof(DebugPage);
    private Type SettingsPage => typeof(SettingsPage);
#pragma warning restore CA1822 // 将成员标记为 static

    private void MainView_Loaded(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Register<KeyDownMessage>(this, ProcessKey);
        WeakReferenceMessenger.Default.Register<NavigationRequest>(this, ProcessNavigationRequest);
        _localizationService.CurrentLanguageFileNameChanged += OnLocalizationServiceCurrentLanguageFileNameChanged;
    }

    private void MainView_Unloaded(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Unregister<KeyDownMessage>(this);
        WeakReferenceMessenger.Default.Unregister<NavigationRequest>(this);
        _localizationService.CurrentLanguageFileNameChanged -= OnLocalizationServiceCurrentLanguageFileNameChanged;
    }

    private void ProcessKey(object recipient, KeyDownMessage message)
    {
        _keySequenceTrigger.ProcessKey(message.Key);
    }

    private void ToggleDebugMenuItem()
    {
        _queueService.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            if (_debugMenuItem.Visibility == Visibility.Visible)
                _debugMenuItem.Visibility = Visibility.Collapsed;
            else
                _debugMenuItem.Visibility = Visibility.Visible;
        });
    }

    private void ProcessNavigationRequest(object recipient, NavigationRequest message)
    {
        _queueService.TryEnqueue(() =>
        {
            var isNavigationStackEnabled = message.NavigationOptions.IsNavigationStackEnabled;
            if (message.GoBackLevel > 0)
            {
                if (message.GoBackLevel <= _contentFrame.BackStack.Count)
                {
                    var pageStackEntry = _contentFrame.BackStack[^message.GoBackLevel];
                    _contentFrame.Navigate(pageStackEntry.SourcePageType, pageStackEntry.Parameter, message.NavigationOptions.TransitionInfoOverride);
                    if (!isNavigationStackEnabled)
                        _contentFrame.BackStack.RemoveAt(_contentFrame.BackStack.Count - 1);
                }
            }
            else if (message.PageType is not null)
            {
                var viewModel = message.NeedParameter ? ServiceLocator.GetViewModel(message.PageType) : message.Parameter;
                _contentFrame.Navigate(message.PageType, viewModel, message.NavigationOptions.TransitionInfoOverride);
                if (!isNavigationStackEnabled && _contentFrame.BackStack.Count > 0)
                    _contentFrame.BackStack.RemoveAt(_contentFrame.BackStack.Count - 1);
            }
        });
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        _contentFrame.Navigate(HomePage, ServiceLocator.GetViewModel(HomePage));

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
            nonClientPointerSource?.ExitedMoveSize += MainView_ExitedMoveSize;

            var activationListener = _windowService.GetInputActivationListener();
            activationListener?.InputActivationChanged += ActivationListener_InputActivationChanged;

            // 获取所有可能位于标题栏区域的元素
            _backButton = VisualTreeHelperExtension.FindChildByName(_navView, "NavigationViewBackButton") as Button;
            _closePaneButton = VisualTreeHelperExtension.FindChildByName(_navView, "NavigationViewCloseButton") as Button;
            _togglePaneButton = VisualTreeHelperExtension.FindChildByName(_navView, "TogglePaneButton") as Button;

            // 面板浮动打开时的透明矩形
            var rectangle = VisualTreeHelperExtension.FindChildByName(splitView, "LightDismissLayer") as Rectangle;
            if (rectangle is not null)
            {
                var clip = new RectangleGeometry
                {
                    Rect = new Rect(0, 48, float.MaxValue, float.MaxValue)
                };
                rectangle.Clip = clip;
            }
        }
    }

    private void NavView_Unloaded(object sender, RoutedEventArgs e)
    {
        var nonClientPointerSource = _windowService.GetInputNonClientPointerSource();
        nonClientPointerSource?.ExitedMoveSize -= MainView_ExitedMoveSize;

        var activationListener = _windowService.GetInputActivationListener();
        activationListener?.InputActivationChanged -= ActivationListener_InputActivationChanged;
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
            _togglePaneButton?.Opacity = 1;
        }
    }

    private void Navigate(Type pageType, NavigationTransitionInfo transitionInfo)
    {
        Type targetPageType = _contentFrame.CurrentSourcePageType;
        if (pageType is not null && !pageType.Equals(targetPageType))
        {
            var viewModel = ServiceLocator.GetViewModel(pageType);
            _contentFrame.Navigate(pageType, viewModel, transitionInfo);
        }
    }

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
        {
            Navigate(SettingsPage, args.RecommendedNavigationTransitionInfo);
        }
        else if (args.InvokedItemContainer is not null)
        {
            Type navPageType = args.InvokedItemContainer.Tag as Type ?? HomePage;
            Navigate(navPageType, args.RecommendedNavigationTransitionInfo);
        }
    }

    private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (!_contentFrame.CanGoBack)
            return;

        if (_navView.IsPaneOpen && (_navView.DisplayMode is NavigationViewDisplayMode.Compact or NavigationViewDisplayMode.Minimal))
            _navView.IsPaneOpen = false;
        else
            _contentFrame.GoBack();
    }

    private void OnFrameNavigated(object sender, NavigationEventArgs e)
    {
        var topPageType = TopPageLocator.GetTopPage(_contentFrame.SourcePageType.Name);
        if (topPageType is null)
        {
            _navView.SelectedItem = null;
            return;
        }

        if (topPageType == SettingsPage)
        {
            if (!ReferenceEquals(_navView.SelectedItem, _navView.SettingsItem))
                _navView.SelectedItem = _navView.SettingsItem;
        }
        else
        {
            SelectNavigationItem(topPageType);
        }
    }

    private void SelectNavigationItem(Type pageType)
    {
        var selectedItem = _navView.MenuItems.OfType<NavigationViewItem>()
            .FirstOrDefault(i => i.Tag.Equals(pageType));
        if (selectedItem is not null && !ReferenceEquals(selectedItem, _navView.SelectedItem))
            _navView.SelectedItem = selectedItem;
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

    private void RootElement_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        UpdateIconRegion();
    }

    private void TitleBar_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (_navView.IsPaneOpen && (_navView.DisplayMode is NavigationViewDisplayMode.Compact or NavigationViewDisplayMode.Minimal))
        {
            _navView.IsPaneOpen = false;
        }
    }

    private void OnLocalizationServiceCurrentLanguageFileNameChanged(object? sender, CurrentLanguageFileNameChangedEventArgs e)
    {
        _queueService.TryEnqueue(() =>
        {
            if (_navView.SettingsItem is ContentControl contentControl && contentControl.Content is string)
            {
                contentControl.Content = _localizationService.AppText.MainWindowText.MainViewText.MenuSettings;
            }
        });
    }
}
