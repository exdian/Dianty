using CommunityToolkit.Mvvm.Messaging;
using Dianty.Utils.Messages;
using Dianty.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace Dianty.Views.Pages;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
    }

    private HomePageViewModel? ViewModel { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is HomePageViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }

    private void OnDeviceCardClick(object sender, RoutedEventArgs e)
    {
        var options = new FrameNavigationOptions
        {
            IsNavigationStackEnabled = true,
            TransitionInfoOverride = new DrillInNavigationTransitionInfo()
        };
        WeakReferenceMessenger.Default.Send(new NavigationRequest(typeof(DevicesPage), options, true));
    }

    private void OnWaveCardClick(object sender, RoutedEventArgs e)
    {
        var options = new FrameNavigationOptions
        {
            IsNavigationStackEnabled = true,
            TransitionInfoOverride = new DrillInNavigationTransitionInfo()
        };
        WeakReferenceMessenger.Default.Send(new NavigationRequest(typeof(WavesPage), options, true));
    }

    private void OnGameCardClick(object sender, RoutedEventArgs e)
    {
        var options = new FrameNavigationOptions
        {
            IsNavigationStackEnabled = true,
            TransitionInfoOverride = new DrillInNavigationTransitionInfo()
        };
        WeakReferenceMessenger.Default.Send(new NavigationRequest(typeof(GamesPage), options, true));
    }
}
