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

    private WavesPageViewModel? _viewModel;
    private IQueueService? _queueService;

    private PathBar.RequiredParameter? PathBarParameter { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is RequiredParameter parameter)
        {
            PathBarParameter = parameter.PathBarParameter;
            _viewModel = parameter.ViewModel;
            _queueService = parameter.QueueService;
        }
    }

    private void OnRootGridLoaded(object sender, RoutedEventArgs e)
    {
        if (_queueService is null || _viewModel is null)
            return;

        _queueService.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            var content = new WavePlayQueuePageContent(_viewModel);
            Grid.SetRow(content, 1);
            _rootGrid.Children.Add(content);
        });
    }

    public record class RequiredParameter(
        PathBar.RequiredParameter PathBarParameter, WavesPageViewModel ViewModel, IQueueService QueueService);
}
