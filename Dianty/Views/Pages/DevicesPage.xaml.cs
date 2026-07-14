using CommunityToolkit.Mvvm.Messaging;
using Dianty.Services;
using Dianty.Utils.Messages;
using Dianty.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Threading.Tasks;

namespace Dianty.Views.Pages;

public sealed partial class DevicesPage : Page
{
    public DevicesPage()
    {
        InitializeComponent();

        Loaded += OnDevicesPageLoaded;
        Unloaded += OnDevicesPageUnloaded;
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
        if (sender is not FrameworkElement element || _queueService is null)
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
        if (element.DataContext is CoyoteItem coyoteItem)
        {
            var requiredParameter = new CoyoteDetailPage.RequiredParameter(coyoteItem, traces, _queueService);
            WeakReferenceMessenger.Default.Send(new NavigationRequest(typeof(CoyoteDetailPage), requiredParameter, options));
        }
    }

    private void OnQrCodeButtonClick(object sender, RoutedEventArgs e)
    {
        _qrCodeTip.IsOpen = !_qrCodeTip.IsOpen;
    }

    private void ShowQrCode(object recipient, WebSocketQrCodeVisibility message)
    {
        if (_queueService is null)
            return;
        _queueService.TryEnqueue(async () =>
        {
            // TeachingTip 在打开和关闭动画途中不能响应开关
            int i = 0;
            while (i < 10 && IsLoaded && _qrCodeTip.IsOpen != message.IsVisible)
            {
                i++;
                _qrCodeTip.IsOpen = message.IsVisible;
                await Task.Delay(100);
            }
        });
    }

    private void OnDevicesPageLoaded(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Register<WebSocketQrCodeVisibility>(this, ShowQrCode);
    }

    private void OnDevicesPageUnloaded(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Unregister<WebSocketQrCodeVisibility>(this);
    }

    public record class RequiredParameter(DevicesPageViewModel ViewModel, IQueueService QueueService);
}
