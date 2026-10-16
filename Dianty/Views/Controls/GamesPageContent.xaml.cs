using CommunityToolkit.Mvvm.Messaging;
using Dianty.Services;
using Dianty.Services.Messages;
using Dianty.ViewModels;
using Dianty.Views.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace Dianty.Views.Controls;

public sealed partial class GamesPageContent : UserControl
{
    public GamesPageContent(GamesPage.RequiredParameter requiredParameter)
    {
        InitializeComponent();
        ViewModel = requiredParameter.GamesPageViewModel;
        GtaVcGameIntegrationViewModel = requiredParameter.GtaVcGameIntegrationViewModel;
        _pageHeaderSelectorFactory = requiredParameter.PageHeaderSelectorFactory;
        _pageContentFactory = requiredParameter.PageContentFactory;
        _queueService = requiredParameter.QueueService;
        _localizationService = requiredParameter.LocalizationService;
    }

    private readonly Func<string, Func<ILocalizationService, string>> _pageHeaderSelectorFactory;
    private readonly Func<string, FrameworkElement?> _pageContentFactory;
    private readonly IQueueService _queueService;
    private readonly ILocalizationService _localizationService;

    private GamesPageViewModel ViewModel { get; }
    private GtaVcGameIntegrationViewModel GtaVcGameIntegrationViewModel { get; }

#pragma warning disable CA1822 // 将成员标记为 static
    private Type GtaVcRuleCards => typeof(GtaVcRuleCards);
#pragma warning restore CA1822 // 将成员标记为 static

    private void OnGameIntegrationCardClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element || element.Tag is not Type contentType)
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
            [s => s.AppText.MainWindowText.MainViewText.MenuGames,
            _pageHeaderSelectorFactory.Invoke(contentType.Name)];
        var pathBarParameter = new PathBar.RequiredParameter(_queueService, _localizationService, pathGetters);
        var requiredParameter = new GameIntegrationDetailPage.RequiredParameter(pathBarParameter, contentType.Name, _pageContentFactory, _queueService);
        WeakReferenceMessenger.Default.Send(new NavigationRequest(typeof(GameIntegrationDetailPage), requiredParameter, options));
    }
}
