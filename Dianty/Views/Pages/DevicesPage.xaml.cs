using CommunityToolkit.Mvvm.Messaging;
using Dianty.Services;
using Dianty.Utils.Messages;
using Dianty.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace Dianty.Views.Pages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class DevicesPage : Page
{
    public DevicesPage()
    {
        InitializeComponent();
    }

    private IQueueService? _queueService;

    private DevicesPageViewModel? ViewModel { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is RequiredParameter parameter)
        {
            ViewModel = parameter.ViewModel;
            _queueService = parameter.QueueService;
        }
    }

    private void OnSettingsCardClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
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
        if (element.DataContext is CoyoteBleItem bleItem)
        {
            var requiredParameter = new CoyoteBleDetailPage.RequiredParameter(bleItem, traces, _queueService);
            WeakReferenceMessenger.Default.Send(new NavigationRequest(typeof(CoyoteBleDetailPage), requiredParameter, options));
        }
        else if (element.DataContext is CoyoteWsItem wsItem)
        {

        }
    }

    public record class RequiredParameter(DevicesPageViewModel ViewModel, IQueueService QueueService);
}
