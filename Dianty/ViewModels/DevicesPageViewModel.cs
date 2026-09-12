using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dianty.Services;
using Dianty.Utils;
using DungeonToolkit.Coyote;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Dianty.ViewModels;

public partial class DevicesPageViewModel : ObservableObject
{
    public DevicesPageViewModel(
        CoyoteCollection coyoteCollection, IQueueService queueService, ICoyoteBleDetector coyoteBleDetector,
        ILocalizationService localizationService)
    {
        _coyoteCollection = coyoteCollection;
        _queueService = queueService;
        _coyoteBleDetector = coyoteBleDetector;
        _localizationService = localizationService;

#if DEBUG
        CoyoteItems.Add(new CoyoteBleItem(new CoyoteBLE { DeviceName = "debug" },
            _queueService, _coyoteCollection, _coyoteBleDetector, _localizationService));
        CoyoteItems.Add(new CoyoteWsItem(new CoyoteWS { DeviceName = "debug0" },
            _queueService, _coyoteCollection, _localizationService));
        CoyoteItems.Add(new CoyoteWsItem(new CoyoteWS { DeviceName = "debug1" },
            _queueService, _coyoteCollection, _localizationService));
#endif
    }

    private readonly CoyoteCollection _coyoteCollection;
    private readonly IQueueService _queueService;
    private readonly ICoyoteBleDetector _coyoteBleDetector;
    private readonly ILocalizationService _localizationService;
    private TaskCompletionSource? _getClientIdTcs;
    private TaskCompletionSource? _bindingTcs;
    private CancellationTokenSource? _cts;
    private int _reconnectingCount;

    private string WaitingBluetoothConnectionStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.WaitingBluetoothConnectionStateText;
    private string WaitingSocketServerStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.WaitingSocketServerStateText;
    private string WaitingScanQrCodeStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.WaitingScanQrCodeStateText;
    private string ConnectionFailedStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionFailedStateText;
    private string ConnectionSuccessfulStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionSuccessfulStateText;
    private string ConnectionCanceledStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionCanceledStateText;
    private string DefaultDeviceNameLabel =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.DefaultDeviceNameLabel;

    public ObservableCollection<CoyoteItem> CoyoteItems => _coyoteCollection;

    [ObservableProperty]
    public partial string ConnectionMessage { get; protected set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsConnecting { get; private set; }

    [ObservableProperty]
    public partial string QrCodeSvgPath { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial bool CanShowQrCode { get; private set; }

    [ObservableProperty]
    public partial bool IsQrCodeDisplayed { get; private set; }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task ConnectBleAsync()
    {
        if (IsConnecting)
            return;
        IsConnecting = true;

        _reconnectingCount++;
        var count = _reconnectingCount;
        ConnectionMessage = WaitingBluetoothConnectionStateText;
        string? connectionMessage = null;
        var coyote = new CoyoteBLE { DeviceName = DefaultDeviceNameLabel };
        _cts = new CancellationTokenSource();
        try
        {
            if (await coyote.ConnectNewAsync(_coyoteBleDetector, _cts.Token))
            {
                connectionMessage = ConnectionSuccessfulStateText;
                var coyoteBleItem = new CoyoteBleItem(
                    coyote, _queueService, _coyoteCollection, _coyoteBleDetector, _localizationService);
                _queueService.TryEnqueue(() => CoyoteItems.Add(coyoteBleItem));
            }
        }
        catch (OperationCanceledException)
        {
            connectionMessage = ConnectionCanceledStateText;
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
                ConnectionMessage = connectionMessage ?? ConnectionFailedStateText;
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
        ConnectionMessage = WaitingSocketServerStateText;
        string? connectionMessage = null;
        var coyote = new CoyoteWS { DeviceName = DefaultDeviceNameLabel };
        coyote.ConnectionStatusChanged += OnConnectionStatusChanged;
        coyote.ClientIdChanged += OnClientIdChanged;
        coyote.BindingStatusChanged += OnBindingStatusChanged;
        _cts = new CancellationTokenSource();
        try
        {
            _getClientIdTcs = new TaskCompletionSource();
            if (!await coyote.ConnectAsync(_cts.Token))
                return;
            await _getClientIdTcs.Task.WaitAsync(_cts.Token);
            _bindingTcs = new TaskCompletionSource();
            var qrCodeSvgPath = await Task.Run(() => CoyoteHelper.CreateQrCodeSvgPathString(coyote)).WaitAsync(_cts.Token);
            _queueService.TryEnqueue(() =>
            {
                ConnectionMessage = WaitingScanQrCodeStateText;
                QrCodeSvgPath = qrCodeSvgPath;
                CanShowQrCode = true;
                IsQrCodeDisplayed = true;
            });
            await _bindingTcs.Task.WaitAsync(_cts.Token);
            connectionMessage = ConnectionSuccessfulStateText;
            var coyoteWsItem = new CoyoteWsItem(coyote, _queueService, _coyoteCollection, _localizationService);
            _queueService.TryEnqueue(() => CoyoteItems.Add(coyoteWsItem));
        }
        catch (OperationCanceledException)
        {
            connectionMessage = ConnectionCanceledStateText;
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
            coyote.BindingStatusChanged -= OnBindingStatusChanged;
            _queueService.TryEnqueue(DispatcherQueuePriority.Low, () =>
            {
                IsQrCodeDisplayed = false;
                CanShowQrCode = false;
                ConnectionMessage = connectionMessage ?? ConnectionFailedStateText;
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

    private void OnBindingStatusChanged(object? sender, CoyoteWS.BindingStatusChangedEventArgs e)
    {
        if (e.IsBound)
            _bindingTcs?.TrySetResult();
    }
}
