using CommunityToolkit.Mvvm.Messaging;
using Dianty.Services;
using Dianty.Utils.Messages;
using Dianty.ViewModels;
using Dianty.Views.Controls;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static Dianty.Services.ILocalizationService;

namespace Dianty.Views.Pages;

public sealed partial class CoyoteDetailPage : Page
{
    public CoyoteDetailPage()
    {
        InitializeComponent();
        Unloaded += OnCoyoteDetailPageUnloaded;
    }

    private IQueueService? _queueService;
    private ILocalizationService? _localizationService;
    private IEnumerable<Func<ILocalizationService, string>>? _pathGetters;

    private CoyoteItem? ViewModel { get; set; }

    private ObservableCollection<string>? Paths { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is RequiredParameter parameter)
        {
            _queueService = parameter.QueueService;
            _localizationService = parameter.LocalizationService;
            _pathGetters = parameter.PathGetters;
            ViewModel = parameter.ViewModel;

            Paths = [.. _pathGetters.Select(f => f.Invoke(_localizationService)),
                _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.DetailsMenuPath];
            _localizationService.CurrentLanguageFileNameChanged += OnCurrentLanguageFileNameChanged;
        }
    }

    private void OnCoyoteDetailPageUnloaded(object sender, RoutedEventArgs e)
    {
        _localizationService?.CurrentLanguageFileNameChanged -= OnCurrentLanguageFileNameChanged;
    }

    private void OnCurrentLanguageFileNameChanged(object? sender, CurrentLanguageFileNameChangedEventArgs e)
    {
        if (Paths is null || _queueService is null || _localizationService is null || _pathGetters is null)
            return;
        string[] paths = [.. _pathGetters.Select(f => f.Invoke(_localizationService)),
            _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.DetailsMenuPath];
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

    private void OnScrollViewerLoaded(object sender, RoutedEventArgs e)
    {
        if (_queueService is null)
            return;

        _queueService.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            if (ViewModel is CoyoteBleItem coyoteBleItem)
                _pageContent.Content = new CoyoteBleDetailCard(coyoteBleItem);
            else if (ViewModel is CoyoteWsItem coyoteWsItem)
                _pageContent.Content = new CoyoteWsDetailCard(coyoteWsItem);
        });
    }

    public record class RequiredParameter(
        CoyoteItem ViewModel, IEnumerable<Func<ILocalizationService, string>> PathGetters, IQueueService QueueService,
        ILocalizationService LocalizationService);
}
