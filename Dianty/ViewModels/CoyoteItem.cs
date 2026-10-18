using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.EventArguments;
using Dianty.Services;
using Dianty.Utils;
using DungeonToolkit;
using DungeonToolkit.Coyote;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dianty.ViewModels;

public abstract partial class CoyoteItemBase : ObservableObject, IDisposable
{
    protected readonly IDeviceInfo _deviceInfo;
    protected readonly IQueueService _queueService;
    protected readonly ILocalizationService _localizationService;
    private bool _isDisposed;

    protected CoyoteItemBase(IDeviceInfo deviceInfo, IQueueService queueService, ILocalizationService localizationService)
    {
        _deviceInfo = deviceInfo;
        _queueService = queueService;
        _localizationService = localizationService;

        UpdateConnectionKindText();
        UpdateConnectionState(_deviceInfo.ConnectionStatus);
        UpdateConnectionMessage();
        IsEnabled = _deviceInfo.IsEnabled;

        _deviceInfo.ConnectionStatusChanged += OnConnectionStatusChanged;
        _localizationService.CurrentLanguageChanged += OnCurrentLanguageChanged;
    }

    public DeviceId Id => _deviceInfo.Id;

    public ConnectionKind ConnectionKind => _deviceInfo.ConnectionKind;

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConnectionKindDescription { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string ConnectionKindNote { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string ConnectionMessage { get; protected set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _deviceInfo.ConnectionStatusChanged -= OnConnectionStatusChanged;
                _localizationService.CurrentLanguageChanged -= OnCurrentLanguageChanged;
            }

            _isDisposed = true;
        }
    }

    protected abstract void UpdateConnectionState(ConnectionStatus status);

    protected abstract void UpdateConnectionMessage();

    protected virtual void OnConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        UpdateConnectionState(e.ConnectionStatus);
        UpdateConnectionMessage();
    }

    protected virtual void OnCurrentLanguageChanged(object? sender, CurrentLanguageChangedEventArgs e)
    {
        UpdateConnectionKindText();
        UpdateConnectionMessage();
    }

    private void UpdateConnectionKindText()
    {
        _queueService.TryEnqueue(() =>
        {
            switch (ConnectionKind)
            {
                default:
                case ConnectionKind.Unknown:
                    ConnectionKindDescription = string.Empty;
                    ConnectionKindNote = string.Empty;
                    break;
                case ConnectionKind.BLE:
                    ConnectionKindDescription = _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.BluetoothLabel;
                    ConnectionKindNote = _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionStatusCardBluetoothDescription;
                    break;
                case ConnectionKind.WebSocket:
                    ConnectionKindDescription = _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.SocketLabel;
                    ConnectionKindNote = _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionStatusCardWebSocketDescription;
                    break;
            }
        });
    }

    partial void OnIsEnabledChanged(bool value)
    {
        _deviceInfo.IsEnabled = value;
    }

    protected enum ConnectionState
    {
        Disconnected, Searching, Connecting, Connected, ConnectionFailed, ConnectionSuccessful, ConnectionCanceled
    }
}

public sealed partial class CoyoteBleItem : CoyoteItemBase
{
    private readonly IStrengthInfo _strengthInfo;
    private readonly IOutputParameterConfig _outputParameterConfig;
    private readonly IBatteryInfo _batteryInfo;
    private readonly ILightsColorConfig _lightsColorConfig;
    private readonly IDeviceConnectionService _deviceConnectionService;
    private readonly Timer _outputParameterConfigTimer;
    private readonly int _parameterConfigCooldownTime = Timeout.Infinite;
    private readonly bool _isConstructed;
    private ConnectionState _connectionState;
    private int _connectionSwitchCount;
    private bool _isDisposed;

    public CoyoteBleItem(
        IDeviceInfo deviceInfo,
        IStrengthInfo strengthInfo,
        IOutputParameterConfig outputParameterConfig,
        IBatteryInfo batteryInfo,
        ILightsColorConfig lightsColorConfig,
        IDeviceConnectionService deviceConnectionService,
        IQueueService queueService,
        ILocalizationService localizationService)
        : base(deviceInfo, queueService, localizationService)
    {
        _strengthInfo = strengthInfo;
        _outputParameterConfig = outputParameterConfig;
        _batteryInfo = batteryInfo;
        _lightsColorConfig = lightsColorConfig;
        _deviceConnectionService = deviceConnectionService;

        _outputParameterConfigTimer = new Timer(SetOutputParameter, null, Timeout.Infinite, Timeout.Infinite);
        LightsColor = _lightsColorConfig.CurrentColor;
        IsGentle = _outputParameterConfig.IsGentle;
        StrengthLimitA = _strengthInfo.StrengthLimitA;
        StrengthLimitB = _strengthInfo.StrengthLimitB;
        FrequencyBalanceA = _outputParameterConfig.CurrentParameter.FrequencyBalanceA;
        FrequencyBalanceB = _outputParameterConfig.CurrentParameter.FrequencyBalanceB;
        IntensityBalanceA = _outputParameterConfig.CurrentParameter.IntensityBalanceA;
        IntensityBalanceB = _outputParameterConfig.CurrentParameter.IntensityBalanceB;
        BatteryLevel = _batteryInfo.BatteryLevel;
        CurrentStrengthA = _strengthInfo.CurrentStrengthA;
        CurrentStrengthB = _strengthInfo.CurrentStrengthB;
        IsConnecting = _deviceInfo.ConnectionStatus is ConnectionStatus.Searching or ConnectionStatus.Connecting;
        ConnectionSwitchValue = _deviceInfo.ConnectionStatus != ConnectionStatus.Disconnected;

        _strengthInfo.StrengthChanged += OnCurrentStrengthChanged;
        _batteryInfo.BatteryLevelChanged += OnBatteryLevelChanged;

        _parameterConfigCooldownTime = 1500;
        _isConstructed = true;
    }

    private string DisconnectedStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.DisconnectedStateText;
    private string BluetoothSearchingStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.BluetoothSearchingStateText;
    private string BluetoothConnectingStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.BluetoothConnectingStateText;
    private string ConnectedStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectedStateText;
    private string ConnectionFailedStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionFailedStateText;
    private string ConnectionSuccessfulStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionSuccessfulStateText;
    private string ConnectionCanceledStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionCanceledStateText;

    public static LightsColor[] LightsColors => Enum.GetValues<LightsColor>();

    [ObservableProperty]
    public partial LightsColor LightsColor { get; set; }

    [ObservableProperty]
    public partial bool IsGentle { get; set; }

    [ObservableProperty]
    public partial double StrengthLimitA { get; set; }

    [ObservableProperty]
    public partial double StrengthLimitB { get; set; }

    [ObservableProperty]
    public partial double FrequencyBalanceA { get; set; }

    [ObservableProperty]
    public partial double FrequencyBalanceB { get; set; }

    [ObservableProperty]
    public partial double IntensityBalanceA { get; set; }

    [ObservableProperty]
    public partial double IntensityBalanceB { get; set; }

    [ObservableProperty]
    public partial int BatteryLevel { get; private set; }

    [ObservableProperty]
    public partial int CurrentStrengthA { get; private set; }

    [ObservableProperty]
    public partial int CurrentStrengthB { get; private set; }

    [ObservableProperty]
    public partial bool IsConnecting { get; private set; }

    [ObservableProperty]
    public partial bool ConnectionSwitchValue { get; set; }

    private void SetOutputParameter(object? state)
    {
        var parameter = new OutputParameter(
            (byte)StrengthLimitA, (byte)StrengthLimitB,
            (byte)FrequencyBalanceA, (byte)FrequencyBalanceB,
            (byte)IntensityBalanceA, (byte)IntensityBalanceB);
        _outputParameterConfig.SetOutputParameterAsync(parameter);
    }

    protected override void UpdateConnectionState(ConnectionStatus status)
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

    protected override void UpdateConnectionMessage()
    {
        _queueService.TryEnqueue(() =>
        {
            ConnectionMessage = _connectionState switch
            {
                ConnectionState.Disconnected => DisconnectedStateText,
                ConnectionState.Searching => BluetoothSearchingStateText,
                ConnectionState.Connecting => BluetoothConnectingStateText,
                ConnectionState.Connected => ConnectedStateText,
                ConnectionState.ConnectionFailed => ConnectionFailedStateText,
                ConnectionState.ConnectionSuccessful => ConnectionSuccessfulStateText,
                ConnectionState.ConnectionCanceled => ConnectionCanceledStateText,
                _ => string.Empty,
            };
        });
    }

    protected override void OnConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        base.OnConnectionStatusChanged(sender, e);
        _queueService.TryEnqueue(() => ConnectionSwitchValue = e.ConnectionStatus != ConnectionStatus.Disconnected);
    }

    private void OnCurrentStrengthChanged(object? sender, StrengthChangedEventArgs e)
    {
        _queueService.TryEnqueue(() =>
        {
            CurrentStrengthA = e.CurrentStrengthA;
            CurrentStrengthB = e.CurrentStrengthB;
        });
    }

    private void OnBatteryLevelChanged(object? sender, BatteryLevelChangedEventArgs e)
    {
        _queueService.TryEnqueue(() => BatteryLevel = e.BatteryLevel);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (_isDisposed)
            return;

        if (disposing)
        {
            _strengthInfo.StrengthChanged -= OnCurrentStrengthChanged;
            _batteryInfo.BatteryLevelChanged -= OnBatteryLevelChanged;
            _outputParameterConfigTimer.Dispose();
        }

        _isDisposed = true;
    }

    async partial void OnConnectionSwitchValueChanged(bool value)
    {
        if (!_isConstructed)
            return;

        if (value)
        {
            _connectionSwitchCount++;
            var count = _connectionSwitchCount;
            IsConnecting = true;
            try
            {
                if (!await _deviceConnectionService.ConnectAsync(Id, CancellationToken.None))
                    throw new Exception();
            }
            catch (OperationCanceledException)
            {
                _connectionState = ConnectionState.ConnectionCanceled;
            }
            catch
            {
                _connectionState = ConnectionState.ConnectionFailed;
            }
            finally
            {
                UpdateConnectionMessage();
                IsConnecting = false;

                await Task.Delay(3000);
                if (count == _connectionSwitchCount)
                {
                    UpdateConnectionState(_deviceInfo.ConnectionStatus);
                    UpdateConnectionMessage();
                }
            }
        }
        else
        {
            await _deviceConnectionService.DisconnectAsync(Id, CancellationToken.None).ConfigureAwait(false);
        }
    }

    partial void OnLightsColorChanged(LightsColor value)
    {
        if (_isConstructed)
            _lightsColorConfig.SetLightsColorAsync(value);
    }

    partial void OnStrengthLimitAChanged(double value)
    {
        _outputParameterConfigTimer.Change(_parameterConfigCooldownTime, Timeout.Infinite);
    }

    partial void OnStrengthLimitBChanged(double value)
    {
        _outputParameterConfigTimer.Change(_parameterConfigCooldownTime, Timeout.Infinite);
    }

    partial void OnIsGentleChanged(bool value)
    {
        _outputParameterConfig.IsGentle = value;
    }

    partial void OnFrequencyBalanceAChanged(double value)
    {
        _outputParameterConfigTimer.Change(_parameterConfigCooldownTime, Timeout.Infinite);
    }

    partial void OnFrequencyBalanceBChanged(double value)
    {
        _outputParameterConfigTimer.Change(_parameterConfigCooldownTime, Timeout.Infinite);
    }

    partial void OnIntensityBalanceAChanged(double value)
    {
        _outputParameterConfigTimer.Change(_parameterConfigCooldownTime, Timeout.Infinite);
    }

    partial void OnIntensityBalanceBChanged(double value)
    {
        _outputParameterConfigTimer.Change(_parameterConfigCooldownTime, Timeout.Infinite);
    }
}

public sealed partial class CoyoteWsItem : CoyoteItemBase
{
    private readonly IStrengthInfo _strengthInfo;
    private readonly IClientInfo _clientInfo;
    private readonly IDeviceConnectionService _deviceConnectionService;
    private readonly bool _isConstructed;
    private ConnectionState _connectionState;
    private int _connectionSwitchCount;
    private int _feedbackCount;
    private bool _isDisposed;

    public CoyoteWsItem(
        IDeviceInfo deviceInfo,
        IStrengthInfo strengthInfo,
        IClientInfo clientInfo,
        IDeviceConnectionService deviceConnectionService,
        IQueueService queueService,
        ILocalizationService localizationService)
        : base(deviceInfo, queueService, localizationService)
    {
        _strengthInfo = strengthInfo;
        _clientInfo = clientInfo;
        _deviceConnectionService = deviceConnectionService;

        CurrentStrengthA = _strengthInfo.CurrentStrengthA;
        CurrentStrengthB = _strengthInfo.CurrentStrengthB;
        StrengthLimitA = _strengthInfo.StrengthLimitA;
        StrengthLimitB = _strengthInfo.StrengthLimitB;
        IsConnecting = _deviceInfo.ConnectionStatus is ConnectionStatus.Searching or ConnectionStatus.Connecting;
        QrCodeSvgPath = _deviceInfo.ConnectionStatus == ConnectionStatus.Connecting ? _clientInfo.ClientId : string.Empty;
        CanShowQrCode = _deviceInfo.ConnectionStatus == ConnectionStatus.Connecting;
        ConnectionSwitchValue = _deviceInfo.ConnectionStatus != ConnectionStatus.Disconnected;

        _strengthInfo.StrengthChanged += OnStrengthInfoChanged;
        _clientInfo.FeedbackReceived += OnClientFeedbackReceived;

        _isConstructed = true;
    }

    private string DisconnectedStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.DisconnectedStateText;
    private string WebSocketSearchingStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.WebSocketSearchingStateText;
    private string WebSocketConnectingStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.WebSocketConnectingStateText;
    private string ConnectedStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectedStateText;
    private string ConnectionFailedStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionFailedStateText;
    private string ConnectionSuccessfulStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionSuccessfulStateText;
    private string ConnectionCanceledStateText =>
        _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText.ConnectionCanceledStateText;

    [ObservableProperty]
    public partial int CurrentStrengthA { get; private set; }

    [ObservableProperty]
    public partial int CurrentStrengthB { get; private set; }

    [ObservableProperty]
    public partial int StrengthLimitA { get; private set; }

    [ObservableProperty]
    public partial int StrengthLimitB { get; private set; }

    [ObservableProperty]
    public partial int FeedbackCode { get; private set; } = -1;

    [ObservableProperty]
    public partial string FeedbackText { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsConnecting { get; private set; }

    [ObservableProperty]
    public partial string QrCodeSvgPath { get; private set; }

    [ObservableProperty]
    public partial bool CanShowQrCode { get; private set; }

    [ObservableProperty]
    public partial bool IsQrCodeDisplayed { get; private set; }

    [ObservableProperty]
    public partial bool ConnectionSwitchValue { get; set; }

    async partial void OnConnectionSwitchValueChanged(bool value)
    {
        if (!_isConstructed)
            return;

        if (value)
        {
            _connectionSwitchCount++;
            var count = _connectionSwitchCount;
            IsConnecting = true;
            try
            {
                if (!await _deviceConnectionService.ConnectAsync(Id, CancellationToken.None))
                    throw new Exception();
            }
            catch (OperationCanceledException)
            {
                _connectionState = ConnectionState.ConnectionCanceled;
            }
            catch
            {
                _connectionState = ConnectionState.ConnectionFailed;
            }
            finally
            {
                UpdateConnectionMessage();
                IsQrCodeDisplayed = false;
                CanShowQrCode = false;
                IsConnecting = false;

                await Task.Delay(3000);
                if (count == _connectionSwitchCount)
                {
                    QrCodeSvgPath = string.Empty;
                    UpdateConnectionState(_deviceInfo.ConnectionStatus);
                    UpdateConnectionMessage();
                }
            }
        }
        else
        {
            await _deviceConnectionService.DisconnectAsync(Id, CancellationToken.None).ConfigureAwait(false);
        }
    }

    protected override void UpdateConnectionState(ConnectionStatus status)
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
                _queueService.TryEnqueue(async () =>
                {
                    var qrCodeSvgPath = await Task.Run(() => CoyoteHelper.CreateQrCodeSvgPathString(_clientInfo));
                    if (_deviceInfo.ConnectionStatus == ConnectionStatus.Connecting)
                    {
                        ConnectionMessage = WebSocketConnectingStateText;
                        QrCodeSvgPath = qrCodeSvgPath;
                        CanShowQrCode = true;
                        IsQrCodeDisplayed = true;
                    }
                });
                break;
            case ConnectionStatus.Connected:
                _connectionState = IsConnecting ? ConnectionState.ConnectionSuccessful : ConnectionState.Connected;
                break;
            default:
                break;
        }
    }

    protected override void UpdateConnectionMessage()
    {
        _queueService.TryEnqueue(() =>
        {
            ConnectionMessage = _connectionState switch
            {
                ConnectionState.Disconnected => DisconnectedStateText,
                ConnectionState.Searching => WebSocketSearchingStateText,
                ConnectionState.Connecting => WebSocketConnectingStateText,
                ConnectionState.Connected => ConnectedStateText,
                ConnectionState.ConnectionFailed => ConnectionFailedStateText,
                ConnectionState.ConnectionSuccessful => ConnectionSuccessfulStateText,
                ConnectionState.ConnectionCanceled => ConnectionCanceledStateText,
                _ => string.Empty,
            };
        });
    }

    protected override void OnConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        base.OnConnectionStatusChanged(sender, e);
        _queueService.TryEnqueue(() => ConnectionSwitchValue = e.ConnectionStatus != ConnectionStatus.Disconnected);
    }

    private void UpdateFeedback()
    {
        _queueService.TryEnqueue(async () =>
        {
            _feedbackCount++;
            var feedbackCount = _feedbackCount;

            var text = _localizationService.AppText.MainWindowText.MainViewText.DevicesPageText;
            FeedbackText = FeedbackCode switch
            {
                0 => text.WebSocketFeedbackCircleA,
                1 => text.WebSocketFeedbackTriangleA,
                2 => text.WebSocketFeedbackSquareA,
                3 => text.WebSocketFeedbackPentagramA,
                4 => text.WebSocketFeedbackHexagonA,
                5 => text.WebSocketFeedbackCircleB,
                6 => text.WebSocketFeedbackTriangleB,
                7 => text.WebSocketFeedbackSquareB,
                8 => text.WebSocketFeedbackPentagramB,
                9 => text.WebSocketFeedbackHexagonB,
                _ => string.Empty,
            };

            await Task.Delay(5000);
            if (feedbackCount == _feedbackCount)
                FeedbackText = string.Empty;
        });
    }

    private void OnStrengthInfoChanged(object? sender, StrengthChangedEventArgs e)
    {
        _queueService.TryEnqueue(() =>
        {
            CurrentStrengthA = e.CurrentStrengthA;
            CurrentStrengthB = e.CurrentStrengthB;
            StrengthLimitA = e.StrengthLimitA;
            StrengthLimitB = e.StrengthLimitB;
        });
    }

    private void OnClientFeedbackReceived(object? sender, FeedbackReceivedEventArgs e)
    {
        FeedbackCode = e.Feedback;
        UpdateFeedback();
    }

    protected override void OnCurrentLanguageChanged(object? sender, CurrentLanguageChangedEventArgs e)
    {
        base.OnCurrentLanguageChanged(sender, e);
        if (FeedbackText != string.Empty)
            UpdateFeedback();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (_isDisposed)
            return;

        if (disposing)
        {
            _strengthInfo.StrengthChanged -= OnStrengthInfoChanged;
            _clientInfo.FeedbackReceived -= OnClientFeedbackReceived;
        }

        _isDisposed = true;
    }
}
