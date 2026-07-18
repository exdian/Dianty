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

    private WavesPageViewModel? ViewModel { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is RequiredParameter parameter)
        {
            ViewModel = parameter.ViewModel;
            _queueService = parameter.QueueService;
        }
    }

    private void OnToggleButtonClickA(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not null && sender is ButtonBase button)
        {
            var command = ViewModel.SwitchWaveACommand;
            var commandParameter = button.CommandParameter;
            if (command.CanExecute(commandParameter))
                command.Execute(commandParameter);
        }
    }

    private void OnToggleButtonClickB(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not null && sender is ButtonBase button)
        {
            var command = ViewModel.SwitchWaveBCommand;
            var commandParameter = button.CommandParameter;
            if (command.CanExecute(commandParameter))
                command.Execute(commandParameter);
        }
    }

    private void OnQueueButtonClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
            return;

        string[] traces = [_pageHeader.Text];
        var options = new FrameNavigationOptions
        {
            IsNavigationStackEnabled = true,
            TransitionInfoOverride = new SlideNavigationTransitionInfo
            {
                Effect = SlideNavigationTransitionEffect.FromRight
            }
        };
        var requiredParameter = new WavePlayingQueuePage.RequiredParameter(ViewModel, traces);
        WeakReferenceMessenger.Default.Send(new NavigationRequest(typeof(WavePlayingQueuePage), requiredParameter, options));
    }

    public record class RequiredParameter(WavesPageViewModel ViewModel, IQueueService QueueService);
}
