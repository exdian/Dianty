using CommunityToolkit.Mvvm.Messaging;
using Dianty.Services;
using Dianty.Utils;
using Dianty.Utils.Messages;
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
        GtaVcRuleCardViewModel = requiredParameter.GtaVcRuleCardViewModel;
        _queueService = requiredParameter.QueueService;
        _localizationService = requiredParameter.LocalizationService;
    }

    private readonly IQueueService _queueService;
    private readonly ILocalizationService _localizationService;

    private GamesPageViewModel ViewModel { get; }
    private GtaVcRuleCardViewModel GtaVcRuleCardViewModel { get; }

#pragma warning disable CA1822 // 将成员标记为 static
    private Type GtaVcRuleCards => typeof(GtaVcRuleCards);
#pragma warning restore CA1822 // 将成员标记为 static

    private void OnGameplayCardClick(object sender, RoutedEventArgs e)
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
            PageHelper.GetPagePageHeaderGetter(contentType.Name)];
        var pathBarParameter = new PathBar.RequiredParameter(_queueService, _localizationService, pathGetters);
        var requiredParameter = new GameplayDetailPage.RequiredParameter(pathBarParameter, contentType.Name, _queueService);
        WeakReferenceMessenger.Default.Send(new NavigationRequest(typeof(GameplayDetailPage), requiredParameter, options));
    }
}
