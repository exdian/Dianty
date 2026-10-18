using CommunityToolkit.Mvvm.Messaging;
using Dianty.Services;
using Dianty.Services.Messages;
using Dianty.ViewModels;
using Dianty.Views.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace Dianty.Views.Pages;

public sealed partial class WaveformsPage : Page
{
    public WaveformsPage()
    {
        InitializeComponent();
    }

    private IQueueService? _queueService;
    private ILocalizationService? _localizationService;

    private WaveformsPageViewModel? ViewModel { get; set; }

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

    private void OnPlaylistButtonClick(object sender, RoutedEventArgs e)
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
        Func<ILocalizationService, string>[] pathGetters =
            [s => s.AppText.MainWindowText.MainViewText.MenuWaveforms,
            s => s.AppText.MainWindowText.MainViewText.WaveformsPageText.PlaylistMenuPath];
        var pathBarParameter = new PathBar.RequiredParameter(_queueService, _localizationService, pathGetters);
        var requiredParameter = new WaveformPlaylistPage.RequiredParameter(pathBarParameter, ViewModel, _queueService);
        WeakReferenceMessenger.Default.Send(new NavigationRequest(typeof(WaveformPlaylistPage), requiredParameter, options));
    }

    private void OnWaveformItemCardClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element
            || element.DataContext is not WaveformItemBase waveformItem
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
            [s => s.AppText.MainWindowText.MainViewText.MenuWaveforms,
            s => s.AppText.MainWindowText.MainViewText.WaveformsPageText.DetailsMenuPath];
        var pathBarParameter = new PathBar.RequiredParameter(_queueService, _localizationService, pathGetters);
        var requiredParameter = new WaveformSettingsPage.RequiredParameter(pathBarParameter, waveformItem);
        WeakReferenceMessenger.Default.Send(new NavigationRequest(typeof(WaveformSettingsPage), requiredParameter, options));
    }

    private void OnSortButtonClick(object sender, RoutedEventArgs e)
    {
        _deletionButton.IsChecked = false;
        if (_sortButton.IsChecked ?? false)
            _pageContent.ItemTemplate = (DataTemplate)Resources["SortWaveformItemTemplate"];
        else
            _pageContent.ItemTemplate = (DataTemplate)Resources["NormalWaveformItemTemplate"];
    }

    private void OnDeletionButtonClick(object sender, RoutedEventArgs e)
    {
        _sortButton.IsChecked = false;
        if (_deletionButton.IsChecked ?? false)
            _pageContent.ItemTemplate = (DataTemplate)Resources["DeletionWaveformItemTemplate"];
        else
            _pageContent.ItemTemplate = (DataTemplate)Resources["NormalWaveformItemTemplate"];
    }

    private void OnWaveformItemDeletionButtonClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not null && sender is ButtonBase button)
        {
            var command = ViewModel.RemoveWaveformCommand;
            var commandParameter = button.CommandParameter;
            if (command.CanExecute(commandParameter))
                command.Execute(commandParameter);
        }
    }

    public sealed record class RequiredParameter(WaveformsPageViewModel ViewModel, IQueueService QueueService,
        ILocalizationService LocalizationService);
}
