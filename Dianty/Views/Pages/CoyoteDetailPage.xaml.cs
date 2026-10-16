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

    private DevicesPageViewModel? _viewModel;
    private CoyoteItemBase? _coyoteItem;
    private IQueueService? _queueService;

    private PathBar.RequiredParameter? PathBarParameter { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is RequiredParameter parameter)
        {
            PathBarParameter = parameter.PathBarParameter;
            _viewModel = parameter.ViewModel;
            _coyoteItem = parameter.CoyoteItem;
            _queueService = parameter.QueueService;
        }
    }

    private void OnPageScrollViewerLoaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel is null || _queueService is null)
            return;

        _queueService.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            if (_coyoteItem is CoyoteBleItem coyoteBleItem)
                _pageContent.Content = new CoyoteBleDetailCards(coyoteBleItem, _viewModel.DeleteCoyoteCommand);
            else if (_coyoteItem is CoyoteWsItem coyoteWsItem)
                _pageContent.Content = new CoyoteWsDetailCards(coyoteWsItem, _viewModel.DeleteCoyoteCommand);
        });
    }

    public sealed record class RequiredParameter(
        PathBar.RequiredParameter PathBarParameter, DevicesPageViewModel ViewModel, CoyoteItemBase CoyoteItem, IQueueService QueueService);
}
