using CommunityToolkit.Mvvm.Messaging;
using Dianty.Services;
using Dianty.Utils.Messages;
using Dianty.ViewModels;
using Dianty.Views.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
        if (sender is not FrameworkElement element || _queueService is null || _localizationService is null)
            return;

        var options = new FrameNavigationOptions
        {
            IsNavigationStackEnabled = true,
            TransitionInfoOverride = new SlideNavigationTransitionInfo
            {
                Effect = SlideNavigationTransitionEffect.FromRight
            }
        };
        if (element.DataContext is CoyoteItem coyoteItem)
        {
            Func<ILocalizationService, string>[] pathGetters =
                [s => s.AppText.MainWindowText.MainViewText.MenuDevices,
                s => s.AppText.MainWindowText.MainViewText.DevicesPageText.DetailsMenuPath];
            var pathBarParameter = new PathBar.RequiredParameter(_queueService, _localizationService, pathGetters);
            var requiredParameter = new CoyoteDetailPage.RequiredParameter(pathBarParameter, coyoteItem, _queueService);
            WeakReferenceMessenger.Default.Send(new NavigationRequest(typeof(CoyoteDetailPage), requiredParameter, options));
        }
    }

    private void OnSortButtonClick(object sender, RoutedEventArgs e)
    {
        if (_sortButton.IsChecked ?? false)
            _pageContent.ItemTemplate = (DataTemplate)Resources["SortCoyoteItemTemplate"];
        else
            _pageContent.ItemTemplate = (DataTemplate)Resources["NormalCoyoteItemTemplate"];
    }

    public record class RequiredParameter(DevicesPageViewModel ViewModel, IQueueService QueueService, ILocalizationService LocalizationService);
}
