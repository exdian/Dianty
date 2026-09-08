using CommunityToolkit.Mvvm.Messaging;
using Dianty.Services;
using Dianty.Utils.Messages;
using Dianty.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace Dianty.Views.Pages;

public sealed partial class WavesPage : Page
{
    public WavesPage()
    {
        InitializeComponent();
    }

    private IQueueService? _queueService;
    private ILocalizationService? _localizationService;

    private WavesPageViewModel? ViewModel { get; set; }

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

    private void OnQueueButtonClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is null || _queueService is null || _localizationService is null)
            return;

        var options = new FrameNavigationOptions
        {
            IsNavigationStackEnabled = true,
            TransitionInfoOverride = new SlideNavigationTransitionInfo
            {
                Effect = SlideNavigationTransitionEffect.FromRight
            }
        };
        var requiredParameter = new WavePlayingQueuePage.RequiredParameter(ViewModel,
            () => [_localizationService.AppText.MainWindowText.MainViewText.MenuWaves], _queueService, _localizationService);
        WeakReferenceMessenger.Default.Send(new NavigationRequest(typeof(WavePlayingQueuePage), requiredParameter, options));
    }

    private void OnWaveItemCardClick(object sender, RoutedEventArgs e)
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
        if (element.DataContext is WaveItem waveItem)
        {
            var requiredParameter = new WaveSettingsPage.RequiredParameter(waveItem,
                () => [_localizationService.AppText.MainWindowText.MainViewText.MenuWaves], _queueService, _localizationService);
            WeakReferenceMessenger.Default.Send(new NavigationRequest(typeof(WaveSettingsPage), requiredParameter, options));
        }
    }

    private void OnSortButtonClick(object sender, RoutedEventArgs e)
    {
        _deletionButton.IsChecked = false;
        if (_sortButton.IsChecked ?? false)
            _pageContent.ItemTemplate = (DataTemplate)Resources["SortWaveItemTemplate"];
        else
            _pageContent.ItemTemplate = (DataTemplate)Resources["NormalWaveItemTemplate"];
    }

    private void OnDeletionButtonClick(object sender, RoutedEventArgs e)
    {
        _sortButton.IsChecked = false;
        if (_deletionButton.IsChecked ?? false)
            _pageContent.ItemTemplate = (DataTemplate)Resources["DeletionWaveItemTemplate"];
        else
            _pageContent.ItemTemplate = (DataTemplate)Resources["NormalWaveItemTemplate"];
    }

    private void OnWaveItemDeletionButtonClickA(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not null && sender is ButtonBase button)
        {
            var command = ViewModel.RemoveWaveCommand;
            var commandParameter = button.CommandParameter;
            if (command.CanExecute(commandParameter))
                command.Execute(commandParameter);
        }
    }

    public record class RequiredParameter(WavesPageViewModel ViewModel, IQueueService QueueService,
        ILocalizationService LocalizationService);
}
