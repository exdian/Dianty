using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dianty.Services;
using Dianty.Utils;
using DungeonToolkit.Coyote;
using Microsoft.UI.Dispatching;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static Dianty.Services.ILocalizationService;

namespace Dianty.ViewModels;

public partial class CoyoteItem : ObservableObject, IDisposable
{
    public CoyoteItem(CoyoteBLE coyote, IQueueService queueService, ICoyoteListService coyoteListService, ILocalizationService localizationService)
    {
        _coyoteBLE = coyote;
        _queueService = queueService;
        _coyoteListService = coyoteListService;
        _localizationService = localizationService;
        IsBleConnection = true;
        Name = coyote.DeviceName;
        IsEnabled = coyote.IsEnabled;
        ConnectionTypeDescription = _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionStatusCardBluetoothDescription;
        UpdateConnectionMessage(coyote.IsConnected);
        _localizationService.CurrentLanguageFileNameChanged += OnCurrentLanguageFileNameChanged;
    }

    public CoyoteItem(CoyoteWS coyote, IQueueService queueService, ICoyoteListService coyoteListService, ILocalizationService localizationService)
    {
        _coyoteWS = coyote;
        _queueService = queueService;
        _coyoteListService = coyoteListService;
        _localizationService = localizationService;
        Name = coyote.DeviceName;
        IsEnabled = coyote.IsEnabled;
        ConnectionTypeDescription = _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionStatusCardSocketDescription;
        UpdateConnectionMessage(coyote.IsBound);
        _localizationService.CurrentLanguageFileNameChanged += OnCurrentLanguageFileNameChanged;
    }

    protected readonly CoyoteBLE? _coyoteBLE;
    protected readonly CoyoteWS? _coyoteWS;
    protected readonly IQueueService _queueService;
    protected readonly ICoyoteListService _coyoteListService;
    protected readonly ILocalizationService _localizationService;
    private bool _isDisposed;

    public bool IsBleConnection { get; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string ConnectionTypeDescription { get; private set; }

    [ObservableProperty]
    public partial string ConnectionMessage { get; protected set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    [RelayCommand]
    private void DeleteThis()
    {
        _coyoteListService.Remove(this);
        Dispose();
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        if (disposing)
        {
            _localizationService.CurrentLanguageFileNameChanged -= OnCurrentLanguageFileNameChanged;
            _coyoteBLE?.Dispose();
            _coyoteWS?.Dispose();
        }
    }

    protected virtual void OnCurrentLanguageFileNameChanged(object? sender, CurrentLanguageFileNameChangedEventArgs e)
    {
        _queueService.TryEnqueue(() =>
        {
            ConnectionTypeDescription = IsBleConnection
                ? _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionStatusCardBluetoothDescription
                : _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionStatusCardSocketDescription;
        });
    }

    protected void UpdateConnectionMessage(bool isConnected)
    {
        ConnectionMessage = isConnected
            ? _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectedStateText
            : _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.DisconnectedStateText;
    }

    partial void OnNameChanged(string value)
    {
        _coyoteBLE?.DeviceName = value;
        _coyoteWS?.DeviceName = value;
    }

    partial void OnIsEnabledChanged(bool value)
    {
        _coyoteBLE?.IsEnabled = value;
        _coyoteWS?.IsEnabled = value;
    }
}

public partial class CoyoteBleItem : CoyoteItem
{
    public CoyoteBleItem(CoyoteBLE coyote, IQueueService queueService, ICoyoteListService coyoteListService,
        ICoyoteBleDetector coyoteBleDetector, ILocalizationService localizationService)
        : base(coyote, queueService, coyoteListService, localizationService)
    {
        _coyoteBleDetector = coyoteBleDetector;
        _bfCommandTimer = new Timer(SendBfCommand, null, Timeout.Infinite, Timeout.Infinite);
        MaxStrengthA = coyote.MaxStrengthA;
        MaxStrengthB = coyote.MaxStrengthB;
        IsGentle = coyote.IsGentle;
        BfFrequencyParamA = coyote.BfFrequencyParamA;
        BfFrequencyParamB = coyote.BfFrequencyParamB;
        BfStrengthParamA = coyote.BfStrengthParamA;
        BfStrengthParamB = coyote.BfStrengthParamB;
        BatteryLevel = coyote.BatteryLevel;
        CurrentStrengthA = coyote.CurrentStrengthA;
        CurrentStrengthB = coyote.CurrentStrengthB;
        IsConnectingOrConnected = coyote.IsConnected;
        coyote.ConnectionStatusChanged += OnConnectionStatusChanged;
        coyote.StrengthChanged += OnStrengthChanged;
        coyote.BatteryLevelChanged += OnBatteryLevelChanged;
    }

    private readonly ICoyoteBleDetector _coyoteBleDetector;
    private readonly Timer _bfCommandTimer;
    private const int BfCommandCooldownTime = 1500;
    private CancellationTokenSource? _cts;
    private int _reconnectingCount;

    private string ConnectingStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectingStateText;
    private string ConnectionFailedStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionFailedStateText;
    private string ConnectionSuccessfulStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionSuccessfulStateText;
    private string ConnectionCanceledStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionCanceledStateText;

    public CoyoteBLE Coyote => _coyoteBLE!;

    [ObservableProperty]
    public partial double MaxStrengthA { get; set; }

    [ObservableProperty]
    public partial double MaxStrengthB { get; set; }

    [ObservableProperty]
    public partial bool IsGentle { get; set; }

    [ObservableProperty]
    public partial double BfFrequencyParamA { get; set; }

    [ObservableProperty]
    public partial double BfFrequencyParamB { get; set; }

    [ObservableProperty]
    public partial double BfStrengthParamA { get; set; }

    [ObservableProperty]
    public partial double BfStrengthParamB { get; set; }

    [ObservableProperty]
    public partial int BatteryLevel { get; private set; }

    [ObservableProperty]
    public partial int CurrentStrengthA { get; private set; }

    [ObservableProperty]
    public partial int CurrentStrengthB { get; private set; }

    [ObservableProperty]
    public partial bool IsConnecting { get; private set; }

    [ObservableProperty]
    public partial bool IsConnectingOrConnected { get; set; }

    private async void Reconnecting()
    {
        Debug.Assert(_coyoteBLE is not null);
        if (IsConnecting || _coyoteBLE.IsConnected)
            return;
        IsConnecting = true;

        _reconnectingCount++;
        var count = _reconnectingCount;
        ConnectionMessage = ConnectingStateText;
        string? connectionMessage = null;
        _cts = new CancellationTokenSource();
        try
        {
            if (await _coyoteBLE.ReconnectingAsync(_coyoteBleDetector, _cts.Token))
                connectionMessage = ConnectionSuccessfulStateText;
        }
        catch (OperationCanceledException)
        {
            connectionMessage = ConnectionCanceledStateText;
        }
        catch { }
        finally
        {
            if (!_coyoteBLE.IsConnected)
                _queueService.TryEnqueue(() => IsConnectingOrConnected = false);
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
                _queueService.TryEnqueue(() => UpdateConnectionMessage(_coyoteBLE.IsConnected));
        }
    }

    private void SendBfCommand(object? state)
    {
        Debug.Assert(_coyoteBLE is not null);
        if (_coyoteBLE.IsConnected)
            _ = _coyoteBLE.SendBfCommandAsync();
    }

    private void OnConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        _queueService.TryEnqueue(() =>
        {
            IsConnectingOrConnected = e.IsConnected;
            if (!e.IsConnected)
                UpdateConnectionMessage(false);
        });
    }

    private void OnStrengthChanged(object? sender, CoyoteBLE.StrengthChangedEventArgs e)
    {
        _queueService.TryEnqueue(() =>
        {
            CurrentStrengthA = e.CurrentStrengthA;
            CurrentStrengthB = e.CurrentStrengthB;
        });
    }

    private void OnBatteryLevelChanged(object? sender, CoyoteBLE.BatteryLevelChangedEventArgs e)
    {
        _queueService.TryEnqueue(() => BatteryLevel = e.BatteryLevel);
    }

    partial void OnIsConnectingOrConnectedChanged(bool value)
    {
        Debug.Assert(_coyoteBLE is not null);
        if (value)
        {
            Reconnecting();
        }
        else
        {
            _cts?.Cancel();
            if (_coyoteBLE.IsConnected)
                _coyoteBLE.Disconnect();
        }
    }

    partial void OnMaxStrengthAChanged(double value)
    {
        Debug.Assert(_coyoteBLE is not null);
        _coyoteBLE.MaxStrengthA = (byte)value;
        _bfCommandTimer.Change(BfCommandCooldownTime, Timeout.Infinite);
    }

    partial void OnMaxStrengthBChanged(double value)
    {
        Debug.Assert(_coyoteBLE is not null);
        _coyoteBLE.MaxStrengthB = (byte)value;
        _bfCommandTimer.Change(BfCommandCooldownTime, Timeout.Infinite);
    }

    partial void OnIsGentleChanged(bool value)
    {
        Debug.Assert(_coyoteBLE is not null);
        _coyoteBLE.IsGentle = value;
    }

    partial void OnBfFrequencyParamAChanged(double value)
    {
        Debug.Assert(_coyoteBLE is not null);
        _coyoteBLE.BfFrequencyParamA = (byte)value;
        _bfCommandTimer.Change(BfCommandCooldownTime, Timeout.Infinite);
    }

    partial void OnBfFrequencyParamBChanged(double value)
    {
        Debug.Assert(_coyoteBLE is not null);
        _coyoteBLE.BfFrequencyParamB = (byte)value;
        _bfCommandTimer.Change(BfCommandCooldownTime, Timeout.Infinite);
    }

    partial void OnBfStrengthParamAChanged(double value)
    {
        Debug.Assert(_coyoteBLE is not null);
        _coyoteBLE.BfStrengthParamA = (byte)value;
        _bfCommandTimer.Change(BfCommandCooldownTime, Timeout.Infinite);
    }

    partial void OnBfStrengthParamBChanged(double value)
    {
        Debug.Assert(_coyoteBLE is not null);
        _coyoteBLE.BfStrengthParamB = (byte)value;
        _bfCommandTimer.Change(BfCommandCooldownTime, Timeout.Infinite);
    }

    protected override void Dispose(bool disposing)
    {
        Debug.Assert(_coyoteBLE is not null);
        base.Dispose(disposing);
        if (disposing)
        {
            _coyoteBLE.ConnectionStatusChanged -= OnConnectionStatusChanged;
            _coyoteBLE.StrengthChanged -= OnStrengthChanged;
            _coyoteBLE.BatteryLevelChanged -= OnBatteryLevelChanged;
            _bfCommandTimer.Dispose();
        }
    }

    protected override void OnCurrentLanguageFileNameChanged(object? sender, CurrentLanguageFileNameChangedEventArgs e)
    {
        Debug.Assert(_coyoteBLE is not null);
        base.OnCurrentLanguageFileNameChanged(sender, e);
        _queueService.TryEnqueue(() =>
        {
            if (!IsConnecting)
            {
                UpdateConnectionMessage(_coyoteBLE.IsConnected);
            }
        });
    }
}

public partial class CoyoteWsItem : CoyoteItem
{
    public CoyoteWsItem(CoyoteWS coyote, IQueueService queueService, ICoyoteListService coyoteListService,
        ILocalizationService localizationService)
        : base(coyote, queueService, coyoteListService, localizationService)
    {
        CurrentStrengthA = coyote.CurrentStrengthA;
        CurrentStrengthB = coyote.CurrentStrengthB;
        StrengthCapA = coyote.StrengthCapA;
        StrengthCapB = coyote.StrengthCapB;
        IsConnectingOrConnected = coyote.IsBound;
        coyote.ConnectionStatusChanged += OnConnectionStatusChanged;
        coyote.ClientIdChanged += OnClientIdChanged;
        coyote.BindingStatusChanged += OnBindingStatusChanged;
        coyote.StrengthChanged += OnStrengthChanged;
    }

    private TaskCompletionSource? _getClientIdTcs;
    private TaskCompletionSource? _bindingTcs;
    private CancellationTokenSource? _cts;
    private int _reconnectingCount;

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

    public CoyoteWS Coyote => _coyoteWS!;

    [ObservableProperty]
    public partial int CurrentStrengthA { get; private set; }

    [ObservableProperty]
    public partial int CurrentStrengthB { get; private set; }

    [ObservableProperty]
    public partial int StrengthCapA { get; private set; }

    [ObservableProperty]
    public partial int StrengthCapB { get; private set; }

    [ObservableProperty]
    public partial bool IsConnecting { get; private set; }

    [ObservableProperty]
    public partial string QrCodeSvgPath { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial bool CanShowQrCode { get; private set; }

    [ObservableProperty]
    public partial bool IsQrCodeDisplayed { get; private set; }

    [ObservableProperty]
    public partial bool IsConnectingOrConnected { get; set; }

    private async void Reconnecting()
    {
        Debug.Assert(_coyoteWS is not null);
        if (IsConnecting || _coyoteWS.IsConnected)
            return;
        IsConnecting = true;

        _reconnectingCount++;
        var count = _reconnectingCount;
        ConnectionMessage = WaitingSocketServerStateText;
        string? connectionMessage = null;
        _cts = new CancellationTokenSource();
        try
        {
            _getClientIdTcs = new TaskCompletionSource();
            if (!await _coyoteWS.ConnectAsync(_cts.Token))
                return;
            await _getClientIdTcs.Task.WaitAsync(_cts.Token);
            _bindingTcs = new TaskCompletionSource();
            var qrCodeSvgPath = await Task.Run(() => CoyoteHelper.CreateQrCodeSvgPathString(_coyoteWS)).WaitAsync(_cts.Token);
            _queueService.TryEnqueue(() =>
            {
                ConnectionMessage = WaitingScanQrCodeStateText;
                QrCodeSvgPath = qrCodeSvgPath;
                CanShowQrCode = true;
                IsQrCodeDisplayed = true;
            });
            await _bindingTcs.Task.WaitAsync(_cts.Token);
            connectionMessage = ConnectionSuccessfulStateText;
        }
        catch (OperationCanceledException)
        {
            connectionMessage = ConnectionCanceledStateText;
        }
        catch { }
        finally
        {
            if (!_coyoteWS.IsBound)
            {
                await _coyoteWS.DisconnectAsync();
                _queueService.TryEnqueue(() => IsConnectingOrConnected = false);
            }
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
            _getClientIdTcs?.TrySetResult();
            _getClientIdTcs = null;
            _bindingTcs?.TrySetResult();
            _bindingTcs = null;
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
                    UpdateConnectionMessage(_coyoteWS.IsBound);
                });
            }
        }
    }

    partial void OnIsConnectingOrConnectedChanged(bool value)
    {
        Debug.Assert(_coyoteWS is not null);
        if (value)
        {
            Reconnecting();
        }
        else
        {
            _cts?.Cancel();
            if (_coyoteWS.IsConnected)
                _ = _coyoteWS.DisconnectAsync();
        }
    }

    private void OnConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        if (!e.IsConnected)
        {
            _getClientIdTcs?.TrySetException(new Exception());
            _bindingTcs?.TrySetException(new Exception());
        }
        _queueService.TryEnqueue(() => IsConnectingOrConnected = e.IsConnected);
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
        else
            _queueService.TryEnqueue(() => UpdateConnectionMessage(false));
    }

    private void OnStrengthChanged(object? sender, CoyoteWS.StrengthChangedEventArgs e)
    {
        _queueService.TryEnqueue(() =>
        {
            CurrentStrengthA = e.CurrentStrengthA;
            CurrentStrengthB = e.CurrentStrengthB;
            StrengthCapA = e.StrengthCapA;
            StrengthCapB = e.StrengthCapB;
        });
    }

    protected override void Dispose(bool disposing)
    {
        Debug.Assert(_coyoteWS is not null);
        base.Dispose(disposing);
        if (disposing)
        {
            _coyoteWS.ConnectionStatusChanged -= OnConnectionStatusChanged;
            _coyoteWS.ClientIdChanged -= OnClientIdChanged;
            _coyoteWS.BindingStatusChanged -= OnBindingStatusChanged;
            _coyoteWS.StrengthChanged -= OnStrengthChanged;
        }
    }

    protected override void OnCurrentLanguageFileNameChanged(object? sender, CurrentLanguageFileNameChangedEventArgs e)
    {
        Debug.Assert(_coyoteWS is not null);
        base.OnCurrentLanguageFileNameChanged(sender, e);
        _queueService.TryEnqueue(() =>
        {
            if (!IsConnecting)
            {
                UpdateConnectionMessage(_coyoteWS.IsBound);
            }
        });
    }
}
