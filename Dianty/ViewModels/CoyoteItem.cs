#define Debug_QrCode
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dianty.Services;
using Dianty.Utils;
using DungeonToolkit.Coyote;
using QRCoder;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Dianty.ViewModels;

public partial class CoyoteItem : ObservableObject
{
    public CoyoteItem(CoyoteBLE coyote, IQueueService queueService)
    {
        _coyoteBLE = coyote;
        _queueService = queueService;
        Name = coyote.DeviceName;
        IsEnabled = coyote.IsEnabled;
        UpdateConnectionMessage(coyote.IsConnected);
        IsBleConnection = true;
        coyote.ConnectionStatusChanged += OnConnectionStatusChanged;
    }

    public CoyoteItem(CoyoteWS coyote, IQueueService queueService)
    {
        _coyoteWS = coyote;
        _queueService = queueService;
        Name = coyote.DeviceName;
        IsEnabled = coyote.IsEnabled;
        UpdateConnectionMessage(coyote.IsBound);
        coyote.ConnectionStatusChanged += OnConnectionStatusChanged;
    }

    protected readonly CoyoteBLE? _coyoteBLE;
    protected readonly CoyoteWS? _coyoteWS;
    protected readonly IQueueService _queueService;

    [ObservableProperty]
    public partial string Name { get; set; }

    public bool IsBleConnection { get; }

    [ObservableProperty]
    public partial string ConnectionMessage { get; protected set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    protected void UpdateConnectionMessage(bool isConnected)
    {
        ConnectionMessage = isConnected ? "已连接" : "已断开连接";
    }

    private void OnConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        _queueService.TryEnqueue(() => UpdateConnectionMessage(e.IsConnected));
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
    public CoyoteBleItem(CoyoteBLE coyote, IQueueService queueService, ICoyoteBleDetector coyoteBleDetector)
        : base(coyote, queueService)
    {
        _coyoteBleDetector = coyoteBleDetector;
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
        coyote.ConnectionStatusChanged += OnConnectionStatusChanged;
        coyote.StrengthChanged += OnStrengthChanged;
        coyote.BatteryLevelChanged += OnBatteryLevelChanged;
    }

    private readonly ICoyoteBleDetector _coyoteBleDetector;
    private CancellationTokenSource? _cts;
    private int _reconnectingCount;

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

    [RelayCommand(CanExecute = nameof(CanReconnecting), AllowConcurrentExecutions = true)]
    private async Task Reconnecting()
    {
        Debug.Assert(_coyoteBLE is not null);
        if (IsConnecting)
        {
            _cts?.Cancel();
            return;
        }
        IsConnecting = true;

        _reconnectingCount++;
        var count = _reconnectingCount;
        ConnectionMessage = "正在连接";
        string connectionMessage = "连接失败";
        _cts = new CancellationTokenSource();
        try
        {
            if (await _coyoteBLE.ReconnectingAsync(_coyoteBleDetector, _cts.Token))
                connectionMessage = "连接成功";
        }
        catch (OperationCanceledException)
        {
            connectionMessage = "已取消连接";
        }
        catch { }
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
                _queueService.TryEnqueue(() => UpdateConnectionMessage(_coyoteBLE.IsConnected));
        }
    }

    private bool CanReconnecting()
    {
        Debug.Assert(_coyoteBLE is not null);
        return !_coyoteBLE.IsConnected;
    }

    private void OnConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        _queueService.TryEnqueue(ReconnectingCommand.NotifyCanExecuteChanged);
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

    partial void OnMaxStrengthAChanged(double value)
    {
        Debug.Assert(_coyoteBLE is not null);
        _coyoteBLE.MaxStrengthA = (byte)value;
    }

    partial void OnMaxStrengthBChanged(double value)
    {
        Debug.Assert(_coyoteBLE is not null);
        _coyoteBLE.MaxStrengthB = (byte)value;
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
    }

    partial void OnBfFrequencyParamBChanged(double value)
    {
        Debug.Assert(_coyoteBLE is not null);
        _coyoteBLE.BfFrequencyParamB = (byte)value;
    }

    partial void OnBfStrengthParamAChanged(double value)
    {
        Debug.Assert(_coyoteBLE is not null);
        _coyoteBLE.BfStrengthParamA = (byte)value;
    }

    partial void OnBfStrengthParamBChanged(double value)
    {
        Debug.Assert(_coyoteBLE is not null);
        _coyoteBLE.BfStrengthParamB = (byte)value;
    }
}

public partial class CoyoteWsItem : CoyoteItem
{
    public CoyoteWsItem(CoyoteWS coyote, IQueueService queueService) : base(coyote, queueService)
    {
        CurrentStrengthA = coyote.CurrentStrengthA;
        CurrentStrengthB = coyote.CurrentStrengthB;
        StrengthCapA = coyote.StrengthCapA;
        StrengthCapB = coyote.StrengthCapB;
        coyote.ConnectionStatusChanged += OnConnectionStatusChanged;
        coyote.ClientIdChanged += OnClientIdChanged;
        coyote.BindingSucceed += OnBindingSucceed;
        coyote.StrengthChanged += OnStrengthChanged;
    }

    private TaskCompletionSource? _getClientIdTcs;
    private TaskCompletionSource? _bindingTcs;
    private CancellationTokenSource? _cts;
    private int _reconnectingCount;

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
    public partial bool IsVisibleQrCode { get; set; }

    [RelayCommand(CanExecute = nameof(CanReconnecting), AllowConcurrentExecutions = true)]
    private async Task Reconnecting()
    {
        Debug.Assert(_coyoteWS is not null);
        if (IsConnecting)
        {
            _cts?.Cancel();
            return;
        }
        IsConnecting = true;

        _reconnectingCount++;
        var count = _reconnectingCount;
        ConnectionMessage = "正在连接服务器获取二维码";
        string connectionMessage = "连接失败";
        _cts = new CancellationTokenSource();
        try
        {
#if !Debug_QrCode
            _getClientIdTcs = new TaskCompletionSource();
            if (!await _coyoteWS.ConnectAsync(_cts.Token))
                return;
            await _getClientIdTcs.Task.WaitAsync(_cts.Token);
#endif
            _bindingTcs = new TaskCompletionSource();
            var qrCodeSvgPath = await Task.Run(CreateQrCodeSvgPathString);
            _queueService.TryEnqueue(() =>
            {
                QrCodeSvgPath = qrCodeSvgPath;
                CanShowQrCode = true;
                IsVisibleQrCode = true;
                ConnectionMessage = "已获取二维码";
            });
            await _bindingTcs.Task.WaitAsync(_cts.Token);
            connectionMessage = "连接成功";
        }
        catch (OperationCanceledException)
        {
            connectionMessage = "已取消连接";
            if (_coyoteWS.IsConnected)
                await _coyoteWS.DisconnectAsync();
        }
        catch { }
        finally
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
            _getClientIdTcs?.TrySetResult();
            _getClientIdTcs = null;
            _bindingTcs?.TrySetResult();
            _bindingTcs = null;
            _queueService.TryEnqueue(() =>
            {
                CanShowQrCode = false;
                IsVisibleQrCode = false;
                ConnectionMessage = connectionMessage;
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

    private bool CanReconnecting()
    {
        Debug.Assert(_coyoteWS is not null);
        return !_coyoteWS.IsBound;
    }

    [RelayCommand]
    private void ShowQrcode()
    {
        IsVisibleQrCode = !IsVisibleQrCode;
    }

    private void OnConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        if (!e.IsConnected)
        {
            _getClientIdTcs?.TrySetException(new Exception());
            _bindingTcs?.TrySetException(new Exception());
        }
        _queueService.TryEnqueue(ReconnectingCommand.NotifyCanExecuteChanged);
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

    private string CreateQrCodeSvgPathString()
    {
        Debug.Assert(_coyoteWS is not null);
        var clientId = _coyoteWS.ClientId;
        using QRCodeGenerator qrGenerator = new();
        using QRCodeData qrCodeData = qrGenerator.CreateQrCode(
            "https://www.dungeon-lab.com/app-download.php#DGLAB-SOCKET#" +
            $"wss://ws.dungeon-lab.cn/{clientId}", QRCodeGenerator.ECCLevel.L);
        using SvgQRCode svgQrCode = new(qrCodeData);
        return svgQrCode.GetSvgPath(needMargin: true);
    }
}
