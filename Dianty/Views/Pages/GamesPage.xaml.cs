using Dianty.Services;
using Dianty.ViewModels;
using Dianty.Views.Controls;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Dianty.Views.Pages;

public sealed partial class GamesPage : Page
{
    public GamesPage()
    {
        InitializeComponent();
    }

    private IQueueService? _queueService;

    private GamesPageViewModel? ViewModel { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is RequiredParameter requiredParameter)
        {
            ViewModel = requiredParameter.ViewModel;
            _queueService = requiredParameter.QueueService;
        }
    }

    private void OnRootGridLoaded(object sender, RoutedEventArgs e)
    {
        if (_queueService is null || ViewModel is null)
            return;

        _queueService.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            var content = new GamesPageContent(ViewModel, _queueService);
            Grid.SetRow(content, 1);
            _rootGrid.Children.Add(content);
        });
    }

    public record class RequiredParameter(GamesPageViewModel ViewModel, IQueueService QueueService);
}
