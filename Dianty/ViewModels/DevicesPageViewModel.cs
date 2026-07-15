using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dianty.Services;
using Dianty.Utils;
using Dianty.Utils.Messages;
using DungeonToolkit.Coyote;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Dianty.ViewModels;

public partial class DevicesPageViewModel : ObservableObject
{
    public DevicesPageViewModel(CoyoteCollection coyoteCollection, IQueueService queueService, ICoyoteBleDetector coyoteBleDetector)
    {
        _coyoteCollection = coyoteCollection;
        _queueService = queueService;
        _coyoteBleDetector = coyoteBleDetector;

#if DEBUG
        CoyoteItems.Add(new CoyoteBleItem(new CoyoteBLE { DeviceName = "调试" }, _queueService, _coyoteBleDetector, _coyoteCollection));
        CoyoteItems.Add(new CoyoteWsItem(new CoyoteWS { DeviceName = "调试0" }, _queueService, _coyoteCollection));
        CoyoteItems.Add(new CoyoteWsItem(new CoyoteWS { DeviceName = "调试1" }, _queueService, _coyoteCollection));
#endif
    }

    private readonly CoyoteCollection _coyoteCollection;
    private readonly IQueueService _queueService;
    private readonly ICoyoteBleDetector _coyoteBleDetector;
    private TaskCompletionSource? _getClientIdTcs;
    private TaskCompletionSource? _bindingTcs;
    private CancellationTokenSource? _cts;
    private int _reconnectingCount;

    public ObservableCollection<CoyoteItem> CoyoteItems => _coyoteCollection;

    [ObservableProperty]
    public partial string ConnectionMessage { get; protected set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsConnecting { get; private set; }

    [ObservableProperty]
    public partial string QrCodeSvgPath { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial bool CanShowQrCode { get; private set; }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task ConnectBleAsync()
    {
        if (IsConnecting)
            return;
        IsConnecting = true;

        _reconnectingCount++;
        var count = _reconnectingCount;
        ConnectionMessage = "正在搜索并连接";
        string connectionMessage = "连接失败";
        var coyote = new CoyoteBLE();
        _cts = new CancellationTokenSource();
        try
        {
            if (await coyote.ConnectNewAsync(_coyoteBleDetector, _cts.Token))
            {
                connectionMessage = "连接成功";
                var coyoteBleItem = new CoyoteBleItem(coyote, _queueService, _coyoteBleDetector, _coyoteCollection);
                _queueService.TryEnqueue(() => CoyoteItems.Add(coyoteBleItem));
            }
        }
        catch (OperationCanceledException)
        {
            connectionMessage = "已取消连接";
            coyote.Dispose();
        }
        catch
        {
            coyote.Dispose();
        }
        finally
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
            _queueService.TryEnqueue(() =>
            {
                ConnectionMessage = connectionMessage;
                IsConnecting = false;
            });

            await Task.Delay(3000);
            if (count == _reconnectingCount)
                _queueService.TryEnqueue(() => ConnectionMessage = string.Empty);
        }
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task ConnectWsAsync()
    {
        if (IsConnecting)
            return;
        IsConnecting = true;

        _reconnectingCount++;
        var count = _reconnectingCount;
        ConnectionMessage = "正在连接服务器获取二维码";
        string connectionMessage = "连接失败";
        var coyote = new CoyoteWS();
        coyote.ConnectionStatusChanged += OnConnectionStatusChanged;
        coyote.ClientIdChanged += OnClientIdChanged;
        coyote.BindingSucceed += OnBindingSucceed;
        _cts = new CancellationTokenSource();
        try
        {
            _getClientIdTcs = new TaskCompletionSource();
            if (!await coyote.ConnectAsync(_cts.Token))
                return;
            await _getClientIdTcs.Task.WaitAsync(_cts.Token);
            _bindingTcs = new TaskCompletionSource();
            var qrCodeSvgPath = await Task.Run(() => CoyoteHelper.CreateQrCodeSvgPathString(coyote)).WithCancellation(_cts.Token);
            _queueService.TryEnqueue(() =>
            {
                QrCodeSvgPath = qrCodeSvgPath;
                CanShowQrCode = true;
                ConnectionMessage = "已获取二维码";
                WeakReferenceMessenger.Default.Send(new WebSocketQrCodeVisibility(isVisible: true));
            });
            await _bindingTcs.Task.WaitAsync(_cts.Token);
            connectionMessage = "连接成功";
            var coyoteWsItem = new CoyoteWsItem(coyote, _queueService, _coyoteCollection);
            _queueService.TryEnqueue(() => CoyoteItems.Add(coyoteWsItem));
        }
        catch (OperationCanceledException)
        {
            connectionMessage = "已取消连接";
            coyote.Dispose();
        }
        catch
        {
            coyote.Dispose();
        }
        finally
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
            _getClientIdTcs?.TrySetResult();
            _getClientIdTcs = null;
            _bindingTcs?.TrySetResult();
            _bindingTcs = null;
            coyote.ConnectionStatusChanged -= OnConnectionStatusChanged;
            coyote.ClientIdChanged -= OnClientIdChanged;
            coyote.BindingSucceed -= OnBindingSucceed;
            _queueService.TryEnqueue(DispatcherQueuePriority.Low, () =>
            {
                WeakReferenceMessenger.Default.Send(new WebSocketQrCodeVisibility(isVisible: false));
                CanShowQrCode = false;
                ConnectionMessage = connectionMessage;
                IsConnecting = false;
            });

            await Task.Delay(3000);
            if (count == _reconnectingCount)
            {
                _queueService.TryEnqueue(() =>
                {
                    QrCodeSvgPath = string.Empty;
                    ConnectionMessage = string.Empty;
                });
            }
        }
    }

    [RelayCommand]
    private void CancelConnect()
    {
        _cts?.Cancel();
    }

    private void OnConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        if (!e.IsConnected)
        {
            _getClientIdTcs?.TrySetException(new Exception());
            _bindingTcs?.TrySetException(new Exception());
        }
    }

    private void OnClientIdChanged(object? sender, CoyoteWS.ClientIdChangedEventArgs e)
    {
        if (e.ClientId.Length != 0)
            _getClientIdTcs?.TrySetResult();
    }

    private void OnBindingSucceed(object? sender, EventArgs e)
    {
        _bindingTcs?.TrySetResult();
    }
}
