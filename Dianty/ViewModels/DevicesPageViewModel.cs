using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dianty.EventArguments;
using Dianty.Services;
using Dianty.Utils;
using DungeonToolkit;
using DungeonToolkit.Coyote;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dianty.ViewModels;

public sealed partial class DevicesPageViewModel : ObservableObject, IDisposable
{
    private readonly IDeviceManager _deviceManager;
    private readonly IDataSource<IDeviceInfo> _deviceDataSource;
    private readonly IDeviceConnectionService _deviceConnectionService;
    private readonly IRegistryReader<DeviceId, IClientInfo> _webSocketClientInfoTable;
    private readonly ICoyoteItemBaseFactory _coyoteItemBaseFactory;
    private readonly IQueueService _queueService;
    private readonly ILocalizationService _localizationService;
    private IDeviceInfo? _connectingCoyote;
    private CancellationTokenSource? _cts;
    private TaskCompletionSource? _tcs;
    private int _connectionSwitchCount;
    private bool _isDisposed;

    public DevicesPageViewModel(
        ObservableCollection<CoyoteItemBase> coyoteCollection,
        IDeviceManager deviceManager,
        IDataSource<IDeviceInfo> deviceDataSource,
        IDeviceConnectionService deviceConnectionService,
        IRegistryReader<DeviceId, IClientInfo> webSocketClientInfoTable,
        ICoyoteItemBaseFactory coyoteItemBaseFactory,
        IQueueService queueService,
        ILocalizationService localizationService)
    {
        CoyoteItems = coyoteCollection;
        _deviceManager = deviceManager;
        _deviceDataSource = deviceDataSource;
        _deviceConnectionService = deviceConnectionService;
        _webSocketClientInfoTable = webSocketClientInfoTable;
        _coyoteItemBaseFactory = coyoteItemBaseFactory;
        _queueService = queueService;
        _localizationService = localizationService;

        _deviceDataSource.Changed += OnDeviceDataSourceChanged;
        _localizationService.CurrentLanguageChanged += OnCurrentLanguageChanged;

#if DEBUG
        IReadOnlyList<IDeviceInfo> debugDevices =
            [_deviceManager.Create(ConnectionKind.BLE),
            _deviceManager.Create(ConnectionKind.WebSocket),
            _deviceManager.Create(ConnectionKind.WebSocket)];

        _deviceManager.Register(debugDevices, d => d.Id);

        _queueService.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
        {
            CoyoteItems[0].Name = "debug";
            CoyoteItems[1].Name = "debug0";
            CoyoteItems[2].Name = "debug1";
        });
#endif
    }

    private string BluetoothSearchingStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.BluetoothSearchingStateText;
    private string BluetoothConnectingStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.BluetoothConnectingStateText;
    private string WebSocketSearchingStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.WebSocketSearchingStateText;
    private string WebSocketConnectingStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.WebSocketConnectingStateText;
    private string ConnectionFailedStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionFailedStateText;
    private string ConnectionSuccessfulStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionSuccessfulStateText;
    private string ConnectionCanceledStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionCanceledStateText;
    private string DefaultDeviceNameLabel =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.DefaultDeviceNameLabel;

    public ObservableCollection<CoyoteItemBase> CoyoteItems { get; }

    [ObservableProperty]
    public partial string ConnectionMessage { get; private set; } = string.Empty;

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

        _connectionSwitchCount++;
        var count = _connectionSwitchCount;
        var coyote = _deviceManager.Create(ConnectionKind.BLE);
        _connectingCoyote = coyote;
        coyote.ConnectionStatusChanged += OnBluetoothConnectionStatusChanged;
        _cts = new CancellationTokenSource();
        try
        {
            if (!await _deviceConnectionService.ConnectAsync(coyote.Id, _cts.Token))
                throw new Exception();

            if (!_deviceManager.Register(coyote.Id))
                throw new Exception();

            ConnectionMessage = ConnectionSuccessfulStateText;
        }
        catch (OperationCanceledException)
        {
            ConnectionMessage = ConnectionCanceledStateText;
            _deviceManager.Destroy(coyote.Id);
        }
        catch
        {
            ConnectionMessage = ConnectionFailedStateText;
            _deviceManager.Destroy(coyote.Id);
        }
        finally
        {
            coyote.ConnectionStatusChanged -= OnBluetoothConnectionStatusChanged;
            _connectingCoyote = null;
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
            IsConnecting = false;

            await Task.Delay(3000);
            if (count == _connectionSwitchCount)
                ConnectionMessage = string.Empty;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task ConnectWsAsync()
    {
        if (IsConnecting)
            return;
        IsConnecting = true;

        _connectionSwitchCount++;
        var count = _connectionSwitchCount;
        var coyote = _deviceManager.Create(ConnectionKind.WebSocket);
        _connectingCoyote = coyote;
        coyote.ConnectionStatusChanged += OnWebSocketConnectionStatusChanged;
        _cts = new CancellationTokenSource();
        _tcs = new TaskCompletionSource();
        try
        {
            var task = _deviceConnectionService.ConnectAsync(coyote.Id, _cts.Token);

            await _tcs.Task.WaitAsync(_cts.Token);

            if (!_webSocketClientInfoTable.TryGetValue(coyote.Id, out var clientInfo))
                throw new Exception();

            var qrCodeSvgPath = await Task.Run(() => CoyoteHelper.CreateQrCodeSvgPathString(clientInfo)).WaitAsync(_cts.Token);
            QrCodeSvgPath = qrCodeSvgPath;
            CanShowQrCode = true;
            IsQrCodeDisplayed = true;

            if (!await task)
                throw new Exception();

            if (!_deviceManager.Register(coyote.Id))
                throw new Exception();

            ConnectionMessage = ConnectionSuccessfulStateText;
        }
        catch (OperationCanceledException)
        {
            ConnectionMessage = ConnectionCanceledStateText;
            _deviceManager.Destroy(coyote.Id);
        }
        catch
        {
            ConnectionMessage = ConnectionFailedStateText;
            _deviceManager.Destroy(coyote.Id);
        }
        finally
        {
            coyote.ConnectionStatusChanged -= OnWebSocketConnectionStatusChanged;
            _connectingCoyote = null;
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
            _tcs?.TrySetResult();
            _tcs = null;
            IsQrCodeDisplayed = false;
            CanShowQrCode = false;
            IsConnecting = false;

            await Task.Delay(3000);
            if (count == _connectionSwitchCount)
            {
                QrCodeSvgPath = string.Empty;
                ConnectionMessage = string.Empty;
            }
        }
    }

    [RelayCommand]
    private void CancelConnect()
    {
        _cts?.Cancel();
    }

    [RelayCommand]
    private void DeleteCoyote(CoyoteItemBase coyoteItem)
    {
        _deviceManager.Destroy(coyoteItem.Id);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            _deviceDataSource.Changed -= OnDeviceDataSourceChanged;
            _localizationService.CurrentLanguageChanged -= OnCurrentLanguageChanged;
        }

        _isDisposed = true;
    }

    private void OnDeviceDataSourceChanged(object? sender, EventArgs e)
    {
        var snapshot = _deviceDataSource.GetSnapshot();
        var snapshotById = snapshot.Select(w => w.Id).ToHashSet();
        var existingIds = CoyoteItems.Select(d => d.Id).ToHashSet();
        var newItems = snapshot.Where(x => !existingIds.Contains(x.Id));

        _queueService.TryEnqueue(() =>
        {
            for (int i = CoyoteItems.Count - 1; i >= 0; i--)
            {
                var item = CoyoteItems[i];
                if (!snapshotById.Contains(item.Id))
                {
                    CoyoteItems.RemoveAt(i);
                    item.Dispose();
                }
            }

            int index = 0;
            foreach (var item in newItems)
            {
                CoyoteItemBase coyoteItem;
                try
                {
                    coyoteItem = _coyoteItemBaseFactory.Create(item);
                }
                catch
                {
                    continue;
                }
                coyoteItem.Name = DefaultDeviceNameLabel;
                CoyoteItems.Insert(index, coyoteItem);
                index++;
            }
        });
    }

    private void UpdateBluetoothConnectionMessage(ConnectionStatus status)
    {
        _queueService.TryEnqueue(() =>
        {
            switch (status)
            {
                case ConnectionStatus.Disconnected:
                    break;
                case ConnectionStatus.Searching:
                    ConnectionMessage = BluetoothSearchingStateText;
                    break;
                case ConnectionStatus.Connecting:
                    ConnectionMessage = BluetoothConnectingStateText;
                    break;
                case ConnectionStatus.Connected:
                    ConnectionMessage = ConnectionSuccessfulStateText;
                    break;
                default:
                    ConnectionMessage = string.Empty;
                    break;
            }
        });
    }

    private void OnBluetoothConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        UpdateBluetoothConnectionMessage(e.ConnectionStatus);
    }

    private void UpdateWebSocketConnectionMessage(ConnectionStatus status)
    {
        _queueService.TryEnqueue(() =>
        {
            switch (status)
            {
                case ConnectionStatus.Disconnected:
                    break;
                case ConnectionStatus.Searching:
                    ConnectionMessage = WebSocketSearchingStateText;
                    break;
                case ConnectionStatus.Connecting:
                    ConnectionMessage = WebSocketConnectingStateText;
                    _tcs?.TrySetResult();
                    break;
                case ConnectionStatus.Connected:
                    ConnectionMessage = ConnectionSuccessfulStateText;
                    break;
                default:
                    ConnectionMessage = string.Empty;
                    break;
            }
        });
    }

    private void OnWebSocketConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        UpdateWebSocketConnectionMessage(e.ConnectionStatus);
    }

    private void OnCurrentLanguageChanged(object? sender, CurrentLanguageChangedEventArgs e)
    {
        var coyote = _connectingCoyote;
        if (coyote is not null)
        {
            switch (coyote.ConnectionKind)
            {
                default:
                case ConnectionKind.Unknown:
                    break;
                case ConnectionKind.BLE:
                    UpdateBluetoothConnectionMessage(coyote.ConnectionStatus);
                    break;
                case ConnectionKind.WebSocket:
                    UpdateWebSocketConnectionMessage(coyote.ConnectionStatus);
                    break;
            }
        }
    }
}
