using CommunityToolkit.Mvvm.Messaging;
using Dianty.Services;
using Dianty.Services.Messages;
using Dianty.ViewModels;
using Dianty.Views.Controls;
using DungeonToolkit;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace Dianty.Views.Pages;

public sealed partial class DevicesPage : Page
{
    public DevicesPage()
    {
        InitializeComponent();
    }

    private IQueueService? _queueService;
    private ILocalizationService? _localizationService;

    private DevicesPageViewModel? ViewModel { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is RequiredParameter parameter)
        {
            ViewModel = parameter.ViewModel;
            _queueService = parameter.QueueService;
            _localizationService = parameter.LocalizationService;
        }
    }

    private void OnDeviceItemCardClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element
            || element.DataContext is not CoyoteItemBase coyoteItem
            || ViewModel is null
            || _queueService is null
            || _localizationService is null)
        {
            return;
        }

        var options = new FrameNavigationOptions
        {
            IsNavigationStackEnabled = true,
            TransitionInfoOverride = new SlideNavigationTransitionInfo
            {
                Effect = SlideNavigationTransitionEffect.FromRight
            }
        };
        Func<ILocalizationService, string>[] pathGetters =
            [s => s.AppText.MainWindowText.MainViewText.MenuDevices,
            s => s.AppText.MainWindowText.MainViewText.DevicesPageText.DetailsMenuPath];
        var pathBarParameter = new PathBar.RequiredParameter(_queueService, _localizationService, pathGetters);
        var requiredParameter = new CoyoteDetailPage.RequiredParameter(pathBarParameter, ViewModel, coyoteItem, _queueService);
        WeakReferenceMessenger.Default.Send(new NavigationRequest(typeof(CoyoteDetailPage), requiredParameter, options));
    }

    private void OnSortButtonUnloaded(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton toggleButton || !(toggleButton.IsChecked ?? false))
            return;

        var command = toggleButton.Command;
        var commandParameter = toggleButton.CommandParameter;
        if (command is not null && command.CanExecute(commandParameter))
        {
            if (_queueService is null)
                command.Execute(commandParameter);
            else
                _queueService.TryEnqueue(DispatcherQueuePriority.Low, () => command.Execute(commandParameter));
        }
    }

    private void OnColorSplitButtonClick(SplitButton sender, SplitButtonClickEventArgs args)
    {
        sender.Flyout.ShowAt(sender);
    }

    private void OnLightsColorSelectionItemClick(object sender, ItemClickEventArgs e)
    {
        if (sender is not FrameworkElement element)
            return;

        if (e.ClickedItem is LightsColor lightsColor && element.DataContext is CoyoteItemBase coyoteItem)
        {
            coyoteItem.LightsColor = lightsColor;
            if (coyoteItem is CoyoteBleItem coyoteBleItem)
            {
                var command = coyoteBleItem.SetLightsColorCommand;
                if (command is not null && command.CanExecute(lightsColor))
                    command.Execute(lightsColor);
            }
        }

        var frameworkElement = element.Parent as FrameworkElement;
        while (frameworkElement is not null)
        {
            if (frameworkElement is Popup popup)
            {
                popup.IsOpen = false;
                break;
            }
            frameworkElement = frameworkElement?.Parent as FrameworkElement;
        }
    }

    public sealed record class RequiredParameter(
        DevicesPageViewModel ViewModel, IQueueService QueueService, ILocalizationService LocalizationService);
}
