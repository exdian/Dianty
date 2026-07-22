using CommunityToolkit.Mvvm.Messaging;
using Dianty.Services;
using Dianty.Utils.Messages;
using Dianty.ViewModels;
using Dianty.Views.Controls;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;

namespace Dianty.Views.Pages;

public sealed partial class CoyoteDetailPage : Page
{
    public CoyoteDetailPage()
    {
        InitializeComponent();
    }

    private IQueueService? _queueService;

    private CoyoteItem? ViewModel { get; set; }

    private ObservableCollection<string>? Paths { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is RequiredParameter parameter)
        {
            ViewModel = parameter.ViewModel;
            Paths = [.. parameter.Paths, "详细信息"];
            _queueService = parameter.QueueService;
        }
    }

    private void OnBreadcrumbBarItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (Paths is null)
            return;

        var options = new FrameNavigationOptions
        {
            IsNavigationStackEnabled = true,
            TransitionInfoOverride = new SlideNavigationTransitionInfo
            {
                Effect = SlideNavigationTransitionEffect.FromLeft
            }
        };
        var navigationRequest = new NavigationRequest(Paths.Count - 1 - args.Index, options);
        for (int i = Paths.Count - 1; i > args.Index; i--)
        {
            Paths.RemoveAt(i);
        }

        WeakReferenceMessenger.Default.Send(navigationRequest);
    }

    private void OnScrollViewerLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_queueService is null)
            return;

        _queueService.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            if (ViewModel is CoyoteBleItem coyoteBleItem)
                _pageContent.Content = new CoyoteBleDetailCard(coyoteBleItem);
            else if (ViewModel is CoyoteWsItem coyoteWsItem)
                _pageContent.Content = new CoyoteWsDetailCard(coyoteWsItem);
        });
    }

    public record class RequiredParameter(CoyoteItem ViewModel, string[] Paths, IQueueService QueueService);
}
