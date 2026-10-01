using Dianty.Services;
using Dianty.ViewModels;
using Dianty.Views.Controls;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Dianty.Views.Pages;

public sealed partial class CoyoteDetailPage : Page
{
    public CoyoteDetailPage()
    {
        InitializeComponent();
    }

    private CoyoteItem? _viewModel;
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

    private void OnPageScrollViewerLoaded(object sender, RoutedEventArgs e)
    {
        if (_queueService is null)
            return;

        _queueService.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            if (_viewModel is CoyoteBleItem coyoteBleItem)
                _pageContent.Content = new CoyoteBleDetailCards(coyoteBleItem);
            else if (_viewModel is CoyoteWsItem coyoteWsItem)
                _pageContent.Content = new CoyoteWsDetailCards(coyoteWsItem);
        });
    }

    public record class RequiredParameter(
        PathBar.RequiredParameter PathBarParameter, CoyoteItem ViewModel, IQueueService QueueService);
}
