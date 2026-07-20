using CommunityToolkit.Mvvm.Messaging;
using Dianty.Services;
using Dianty.Utils.Messages;
using Dianty.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;

namespace Dianty.Views.Pages;

public sealed partial class WaveSettingsPage : Page
{
    public WaveSettingsPage()
    {
        InitializeComponent();
    }

    private IQueueService? _queueService;

    private WaveItem? ViewModel { get; set; }

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

        // 文本框输入状态下点击导航，文本框绑定的属性不能及时更新，因此需要将导航放到低优先级队列
        if (_queueService is null)
        {
            WeakReferenceMessenger.Default.Send(navigationRequest);
        }
        else
        {
            _queueService.TryEnqueue(DispatcherQueuePriority.Low,
                () => WeakReferenceMessenger.Default.Send(navigationRequest));
        }
    }

    public record class RequiredParameter(WaveItem ViewModel, string[] Paths, IQueueService QueueService);
}
