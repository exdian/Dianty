using CommunityToolkit.Mvvm.Messaging;
using Dianty.Utils.Messages;
using Dianty.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace Dianty.Views.Pages;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
    }

#pragma warning disable CA1822 // 将成员标记为 static
    private Type DevicesPage => typeof(DevicesPage);
    private Type WavesPage => typeof(WavesPage);
    private Type GamesPage => typeof(GamesPage);
#pragma warning restore CA1822 // 将成员标记为 static

    private HomePageViewModel? ViewModel { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is HomePageViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }

    private void OnMenuCardClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element || element.Tag is not Type pageType)
            return;

        var options = new FrameNavigationOptions
        {
            IsNavigationStackEnabled = true,
            TransitionInfoOverride = new DrillInNavigationTransitionInfo()
        };
        WeakReferenceMessenger.Default.Send(new NavigationRequest(pageType, options, true));
    }
}
