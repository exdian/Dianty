using CommunityToolkit.Mvvm.Messaging;
using Dianty.Services;
using Dianty.Utils.Messages;
using Dianty.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using static Dianty.Services.ILocalizationService;

namespace Dianty.Views.Pages;

public sealed partial class WavePlayingQueuePage : Page
{
    public WavePlayingQueuePage()
    {
        InitializeComponent();
        Unloaded += OnWavePlayingQueuePageUnloaded;
    }

    private IQueueService? _queueService;
    private ILocalizationService? _localizationService;
    private Func<string[]>? _pathsGetter;

    private WavesPageViewModel? ViewModel { get; set; }
    private ObservableCollection<string>? Paths { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is RequiredParameter parameter)
        {
            _queueService = parameter.QueueService;
            _localizationService = parameter.LocalizationService;
            _pathsGetter = parameter.PathsGetter;
            ViewModel = parameter.ViewModel;

            Paths = [.. _pathsGetter.Invoke(),
                _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.QueueMenuPath];
            _localizationService.CurrentLanguageFileNameChanged += OnCurrentLanguageFileNameChanged;
        }
    }

    private void OnWavePlayingQueuePageUnloaded(object sender, RoutedEventArgs e)
    {
        _localizationService?.CurrentLanguageFileNameChanged -= OnCurrentLanguageFileNameChanged;
    }

    private void OnCurrentLanguageFileNameChanged(object? sender, CurrentLanguageFileNameChangedEventArgs e)
    {
        if (Paths is null || _queueService is null || _localizationService is null || _pathsGetter is null)
            return;
        string[] paths = [.. _pathsGetter.Invoke(),
            _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.QueueMenuPath];
        _queueService.TryEnqueue(() =>
        {
            Paths.Clear();
            foreach (string path in paths)
            {
                Paths.Add(path);
            }
        });
    }

    private void OnBreadcrumbBarItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (Paths is null)
            return;

        var options = new FrameNavigationOptions
        {
            IsNavigationStackEnabled = true,
            TransitionInfoOverride = new SlideNavigationTransitionInfo
            {
                Effect = SlideNavigationTransitionEffect.FromLeft
            }
        };
        var navigationRequest = new NavigationRequest(Paths.Count - 1 - args.Index, options);
        for (int i = Paths.Count - 1; i > args.Index; i--)
        {
            Paths.RemoveAt(i);
        }

        WeakReferenceMessenger.Default.Send(navigationRequest);
    }

    public record class RequiredParameter(WavesPageViewModel ViewModel, Func<string[]> PathsGetter,
        IQueueService QueueService, ILocalizationService LocalizationService);
}
