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
    private ConnectionState _connectionState = ConnectionState.Unknown;
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
        }
        catch (OperationCanceledException)
        {
            _connectionState = ConnectionState.ConnectionCanceled;
            _deviceManager.Destroy(coyote.Id);
        }
        catch
        {
            _connectionState = ConnectionState.ConnectionFailed;
            _deviceManager.Destroy(coyote.Id);
        }
        finally
        {
            coyote.ConnectionStatusChanged -= OnBluetoothConnectionStatusChanged;
            _connectingCoyote = null;
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
            UpdateBluetoothConnectionMessage();
            IsConnecting = false;

            await Task.Delay(3000);
            if (count == _connectionSwitchCount)
            {
                _connectionState = ConnectionState.Unknown;
                ConnectionMessage = string.Empty;
            }
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
        }
        catch (OperationCanceledException)
        {
            _connectionState = ConnectionState.ConnectionCanceled;
            _deviceManager.Destroy(coyote.Id);
        }
        catch
        {
            _connectionState = ConnectionState.ConnectionFailed;
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
            UpdateBluetoothConnectionMessage();
            IsConnecting = false;

            await Task.Delay(3000);
            if (count == _connectionSwitchCount)
            {
                _connectionState = ConnectionState.Unknown;
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

    private void UpdateConnectionState(ConnectionStatus status)
    {
        switch (status)
        {
            case ConnectionStatus.Disconnected:
                if (!IsConnecting)
                    _connectionState = ConnectionState.Disconnected;
                break;
            case ConnectionStatus.Searching:
                _connectionState = ConnectionState.Searching;
                break;
            case ConnectionStatus.Connecting:
                _connectionState = ConnectionState.Connecting;
                break;
            case ConnectionStatus.Connected:
                _connectionState = IsConnecting ? ConnectionState.ConnectionSuccessful : ConnectionState.Connected;
                break;
            default:
                break;
        }
    }

    private void UpdateBluetoothConnectionMessage()
    {
        _queueService.TryEnqueue(() =>
        {
            ConnectionMessage = _connectionState switch
            {
                ConnectionState.Searching => BluetoothSearchingStateText,
                ConnectionState.Connecting => BluetoothConnectingStateText,
                ConnectionState.ConnectionFailed => ConnectionFailedStateText,
                ConnectionState.ConnectionSuccessful => ConnectionSuccessfulStateText,
                ConnectionState.ConnectionCanceled => ConnectionCanceledStateText,
                _ or ConnectionState.Unknown or ConnectionState.Disconnected or ConnectionState.Connected => string.Empty,
            };
        });
    }

    private void OnBluetoothConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        UpdateConnectionState(e.ConnectionStatus);
        UpdateBluetoothConnectionMessage();
    }

    private void UpdateWebSocketConnectionMessage()
    {
        _queueService.TryEnqueue(() =>
        {
            ConnectionMessage = _connectionState switch
            {
                ConnectionState.Searching => WebSocketSearchingStateText,
                ConnectionState.Connecting => WebSocketConnectingStateText,
                ConnectionState.ConnectionFailed => ConnectionFailedStateText,
                ConnectionState.ConnectionSuccessful => ConnectionSuccessfulStateText,
                ConnectionState.ConnectionCanceled => ConnectionCanceledStateText,
                _ or ConnectionState.Unknown or ConnectionState.Disconnected or ConnectionState.Connected => string.Empty,
            };
        });
    }

    private void OnWebSocketConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        UpdateConnectionState(e.ConnectionStatus);
        UpdateWebSocketConnectionMessage();
        if (e.ConnectionStatus == ConnectionStatus.Connecting)
            _tcs?.TrySetResult();
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
                    UpdateBluetoothConnectionMessage();
                    break;
                case ConnectionKind.WebSocket:
                    UpdateWebSocketConnectionMessage();
                    break;
            }
        }
        else
        {
            var connectionMessage = _connectionState switch
            {
                ConnectionState.ConnectionFailed => ConnectionFailedStateText,
                ConnectionState.ConnectionSuccessful => ConnectionSuccessfulStateText,
                ConnectionState.ConnectionCanceled => ConnectionCanceledStateText,
                _ => string.Empty,
            };

            if (connectionMessage != string.Empty)
                _queueService.TryEnqueue(() => ConnectionMessage = connectionMessage);
        }
    }

    private enum ConnectionState
    {
        Unknown = -1, Disconnected = 0, Searching, Connecting, Connected, ConnectionFailed, ConnectionSuccessful, ConnectionCanceled
    }
}
