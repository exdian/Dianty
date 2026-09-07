using CommunityToolkit.Mvvm.Messaging;
using Dianty.Localization;
using Dianty.Services;
using Dianty.Utils.Messages;
using Dianty.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

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

    private void OnSettingsCardClick(object sender, RoutedEventArgs e)
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
            var requiredParameter = new CoyoteDetailPage.RequiredParameter(coyoteItem,
                () => [_localizationService.AppText.MainWindowText.MainViewText.MenuDevices], _queueService, _localizationService);
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
