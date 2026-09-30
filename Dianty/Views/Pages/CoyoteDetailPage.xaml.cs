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

    private IQueueService? _queueService;

    private CoyoteItem? ViewModel { get; set; }
    private PathBar.RequiredParameter? PathBarParameter { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is RequiredParameter parameter)
        {
            _queueService = parameter.QueueService;
            ViewModel = parameter.ViewModel;
            PathBarParameter = parameter.PathBarParameter;
        }
    }

    private void OnPageContentScrollViewerLoaded(object sender, RoutedEventArgs e)
    {
        if (_queueService is null)
            return;

        _queueService.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            if (ViewModel is CoyoteBleItem coyoteBleItem)
                _pageContent.Content = new CoyoteBleDetailCards(coyoteBleItem);
            else if (ViewModel is CoyoteWsItem coyoteWsItem)
                _pageContent.Content = new CoyoteWsDetailCards(coyoteWsItem);
        });
    }

    public record class RequiredParameter(
        CoyoteItem ViewModel, IQueueService QueueService, PathBar.RequiredParameter PathBarParameter);
}
