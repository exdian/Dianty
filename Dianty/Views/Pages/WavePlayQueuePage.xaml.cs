using Dianty.Services;
using Dianty.ViewModels;
using Dianty.Views.Controls;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Dianty.Views.Pages;

public sealed partial class WavePlayQueuePage : Page
{
    public WavePlayQueuePage()
    {
        InitializeComponent();
    }

    private IQueueService? _queueService;

    private WavesPageViewModel? ViewModel { get; set; }
    private PathBar.RequiredParameter? PathBarParameter { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is RequiredParameter parameter)
        {
            ViewModel = parameter.ViewModel;
            PathBarParameter = parameter.PathBarParameter;
            _queueService = parameter.QueueService;
        }
    }

    private void OnPageContentBorderLoaded(object sender, RoutedEventArgs e)
    {
        if (_queueService is null || ViewModel is null)
            return;

        _queueService.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            _pageContent.Child = new WavePlayQueueControl(ViewModel);
        });
    }

    public record class RequiredParameter(WavesPageViewModel ViewModel, PathBar.RequiredParameter PathBarParameter,
        IQueueService QueueService);
}
