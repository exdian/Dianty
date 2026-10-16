using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.EventArguments;
using Dianty.Services;
using DungeonToolkit;
using DungeonToolkit.Coyote;
using System;

namespace Dianty.ViewModels;

public sealed partial class HomePageViewModel : ObservableObject, IDisposable
{
    private readonly IDataSource<IDeviceInfo> _coyotes;
    private readonly IDeviceConnectionStatusMonitor _deviceConnectionStatusMonitor;
    private readonly IWaveformPlayNotification _channelA;
    private readonly IWaveformPlayNotification _channelB;
    private readonly INameRegistryReader<WaveformId> _waveformNameTable;
    private readonly IWaveformPusherStatus _waveformPusherStatus;
    private readonly IStrengthCalculatorStatus _gameIntegrationStrength;
    private readonly IQueueService _queueService;
    private readonly ILocalizationService _localizationService;
    private bool _isDisposed;

    public HomePageViewModel(
        IDataSource<IDeviceInfo> coyotes,
        IDeviceConnectionStatusMonitor deviceConnectionStatusMonitor,
        IWaveformPlayNotification channelA,
        IWaveformPlayNotification channelB,
        INameRegistryReader<WaveformId> waveformNameTable,
        IWaveformPusherStatus waveformPusherStatus,
        IStrengthCalculatorStatus gameIntegrationStrength,
        IQueueService queueService,
        ILocalizationService localizationService)
    {
        _coyotes = coyotes;
        _deviceConnectionStatusMonitor = deviceConnectionStatusMonitor;
        _channelA = channelA;
        _channelB = channelB;
        _waveformNameTable = waveformNameTable;
        _waveformPusherStatus = waveformPusherStatus;
        _gameIntegrationStrength = gameIntegrationStrength;
        _queueService = queueService;
        _localizationService = localizationService;

        FormattedConnectedCount = string.Format(ConnectedCountFormat, 0);
        FormattedAddedCount = string.Format(AddedCountFormat, _coyotes.GetSnapshot().Count);
        FormattedWaveformNameA = string.Format(AChannelFormat, GetWaveformName(_channelA.CurrentWaveformId) ?? NoneLabel);
        FormattedWaveformNameB = string.Format(BChannelFormat, GetWaveformName(_channelB.CurrentWaveformId) ?? NoneLabel);
        FormattedCurrentStrength = string.Format(CurrentStrengthFormat, _waveformPusherStatus.IsPushing
            ? _gameIntegrationStrength.OutputStrength : NoneLabel);

        _coyotes.Changed += OnCoyoteCollectionChanged;
        _deviceConnectionStatusMonitor.DeviceConnectionStatusChanged += OnDeviceConnectionStatusChanged;
        _channelA.CurrentWaveformChanged += OnChannelACurrentWaveformChanged;
        _channelB.CurrentWaveformChanged += OnChannelBCurrentWaveformChanged;
        _waveformNameTable.RegisteredNameChanged += OnWaveformNameTableChanged;
        _waveformPusherStatus.PushStatusChanged += OnWaveformPusherStatusChanged;
        _gameIntegrationStrength.OutputStrengthChanged += OnGameIntegrationManagerOutputStrengthChanged;
        _localizationService.CurrentLanguageChanged += OnCurrentLanguageChanged;
    }

    private string ConnectedCountFormat => _localizationService.AppText.MainWindowText.MainViewText.HomePageText.ConnectedCountFormat;
    private string AddedCountFormat => _localizationService.AppText.MainWindowText.MainViewText.HomePageText.AddedCountFormat;
    private string AChannelFormat => _localizationService.AppText.MainWindowText.MainViewText.HomePageText.ChannelAFormat;
    private string BChannelFormat => _localizationService.AppText.MainWindowText.MainViewText.HomePageText.ChannelBFormat;
    private string CurrentStrengthFormat => _localizationService.AppText.MainWindowText.MainViewText.HomePageText.CurrentStrengthFormat;
    private string NoneLabel => _localizationService.AppText.MainWindowText.MainViewText.HomePageText.NoneLabel;

    public int ConnectedCount
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                FormattedConnectedCount = string.Format(ConnectedCountFormat, value);
            }
        }
    }

    public int AddedCount
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                FormattedAddedCount = string.Format(AddedCountFormat, value);
            }
        }
    }

    [ObservableProperty]
    public partial string FormattedConnectedCount { get; private set; }

    [ObservableProperty]
    public partial string FormattedAddedCount { get; private set; }

    [ObservableProperty]
    public partial string FormattedWaveformNameA { get; private set; }

    [ObservableProperty]
    public partial string FormattedWaveformNameB { get; private set; }

    [ObservableProperty]
    public partial string FormattedCurrentStrength { get; private set; }

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
            _coyotes.Changed -= OnCoyoteCollectionChanged;
            _deviceConnectionStatusMonitor.DeviceConnectionStatusChanged -= OnDeviceConnectionStatusChanged;
            _channelA.CurrentWaveformChanged -= OnChannelACurrentWaveformChanged;
            _channelB.CurrentWaveformChanged -= OnChannelBCurrentWaveformChanged;
            _waveformNameTable.RegisteredNameChanged -= OnWaveformNameTableChanged;
            _waveformPusherStatus.PushStatusChanged -= OnWaveformPusherStatusChanged;
            _gameIntegrationStrength.OutputStrengthChanged -= OnGameIntegrationManagerOutputStrengthChanged;
            _localizationService.CurrentLanguageChanged -= OnCurrentLanguageChanged;
        }

        _isDisposed = true;
    }

    private string? GetWaveformName(WaveformId? id)
    {
        if (id is null)
            return null;

        var result = _waveformNameTable.GetName(id.Value);
        if (result == string.Empty)
            return null;

        return result;
    }

    private void OnCoyoteCollectionChanged(object? sender, EventArgs e)
    {
        var connectedCount = 0;
        var coyotes = _coyotes.GetSnapshot();
        for (int i = 0; i < coyotes.Count; i++)
        {
            if (coyotes[i].ConnectionStatus == ConnectionStatus.Connected)
                connectedCount++;
        }

        _queueService.TryEnqueue(() =>
        {
            AddedCount = coyotes.Count;
            ConnectedCount = connectedCount;
        });
    }

    private void OnDeviceConnectionStatusChanged(object? sender, DeviceConnectionStatusChangedEventArgs e)
    {
        var status = e.ConnectionStatus;
        if (status == ConnectionStatus.Disconnected)
            _queueService.TryEnqueue(() => ConnectedCount--);
        else if (status == ConnectionStatus.Connected)
            _queueService.TryEnqueue(() => ConnectedCount++);
    }

    private void OnChannelACurrentWaveformChanged(object? sender, EventArgs e)
    {
        _queueService.TryEnqueue(() => FormattedWaveformNameA = string.Format(AChannelFormat, GetWaveformName(_channelA.CurrentWaveformId) ?? NoneLabel));
    }

    private void OnChannelBCurrentWaveformChanged(object? sender, EventArgs e)
    {
        _queueService.TryEnqueue(() => FormattedWaveformNameB = string.Format(BChannelFormat, GetWaveformName(_channelB.CurrentWaveformId) ?? NoneLabel));
    }

    private void OnWaveformNameTableChanged(object? sender, RegisteredNameChangedEventArgs<WaveformId> e)
    {
        _queueService.TryEnqueue(() =>
        {
            var key = e.Key;
            if (key == _channelA.CurrentWaveformId)
                FormattedWaveformNameA = string.Format(AChannelFormat, GetWaveformName(_channelA.CurrentWaveformId) ?? NoneLabel);
            if (key == _channelB.CurrentWaveformId)
                FormattedWaveformNameB = string.Format(BChannelFormat, GetWaveformName(_channelB.CurrentWaveformId) ?? NoneLabel);
        });
    }

    private void OnWaveformPusherStatusChanged(object? sender, PushStatusChangedEventArgs e)
    {
        _queueService.TryEnqueue(() => FormattedCurrentStrength = string.Format(CurrentStrengthFormat,
            _waveformPusherStatus.IsPushing ? _gameIntegrationStrength.OutputStrength : NoneLabel));
    }

    private void OnGameIntegrationManagerOutputStrengthChanged(object? sender, OutputStrengthChangedEventArgs e)
    {
        if (_waveformPusherStatus.IsPushing)
            _queueService.TryEnqueue(() => FormattedCurrentStrength = string.Format(CurrentStrengthFormat, e.Strength));
    }

    private void OnCurrentLanguageChanged(object? sender, CurrentLanguageChangedEventArgs e)
    {
        _queueService.TryEnqueue(() =>
        {
            FormattedConnectedCount = string.Format(ConnectedCountFormat, ConnectedCount);
            FormattedAddedCount = string.Format(AddedCountFormat, AddedCount);
            FormattedWaveformNameA = string.Format(AChannelFormat, GetWaveformName(_channelA.CurrentWaveformId) ?? NoneLabel);
            FormattedWaveformNameB = string.Format(BChannelFormat, GetWaveformName(_channelB.CurrentWaveformId) ?? NoneLabel);
            FormattedCurrentStrength = string.Format(CurrentStrengthFormat, _waveformPusherStatus.IsPushing
                ? _gameIntegrationStrength.OutputStrength : NoneLabel);
        });
    }
}
