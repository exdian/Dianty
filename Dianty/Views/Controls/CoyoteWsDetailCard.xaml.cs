using CommunityToolkit.Mvvm.Messaging;
using Dianty.Services;
using Dianty.Utils.Messages;
using Dianty.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

namespace Dianty.Views.Controls;

public sealed partial class CoyoteWsDetailCard : UserControl
{
    public CoyoteWsDetailCard(CoyoteWsItem? coyoteWsItem, IQueueService queueService)
    {
        InitializeComponent();

        ViewModel = coyoteWsItem;
        _queueService = queueService;
        Loaded += OnDevicesPageLoaded;
        Unloaded += OnDevicesPageUnloaded;
    }

    private readonly IQueueService _queueService;

    private CoyoteWsItem? ViewModel { get; set; }

    private void OnQrCodeButtonClick(object sender, RoutedEventArgs e)
    {
        _qrCodeTip.IsOpen = !_qrCodeTip.IsOpen;
    }

    private void ShowQrCode(object recipient, WebSocketQrCodeVisibility message)
    {
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
}
