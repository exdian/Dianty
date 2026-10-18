using Dianty.ViewModels;
using DungeonToolkit;
using DungeonToolkit.Coyote;
using System;

namespace Dianty.Services;

public sealed class ViewModelItemFactory : ICoyoteItemBaseFactory, IWaveformItemFactory
{
    private readonly IRegistryReader<DeviceId, IStrengthInfo> _strengthInfoRegistryReader;
    private readonly IRegistryReader<DeviceId, IOutputParameterConfig> _outputParameterConfigRegistryReader;
    private readonly IRegistryReader<DeviceId, IBatteryInfo> _batteryInfoRegistryReader;
    private readonly IRegistryReader<DeviceId, ILightsColorConfig> _lightsColorConfigRegistryReader;
    private readonly IRegistryReader<DeviceId, IClientInfo> _clientInfoRegistryReader;
    private readonly IDeviceConnectionService _deviceConnectionService;
    private readonly IQueueService _queueService;
    private readonly ILocalizationService _localizationService;
    private readonly INameRegistry<WaveformId> _waveformNameTable;
    private readonly IWaveformPlaylist _waveformPlaylistA;
    private readonly IWaveformPlaylist _waveformPlaylistB;
    private readonly IRegistryReader<WaveformDecoderId, IWaveformDecoderConfig> _waveformDecoderConfigRegistryReaderA;
    private readonly IRegistryReader<WaveformDecoderId, IWaveformDecoderConfig> _waveformDecoderConfigRegistryReaderB;
    private readonly IWaveformPlayNotification _waveformPlayNotificationA;
    private readonly IWaveformPlayNotification _waveformPlayNotificationB;

    public ViewModelItemFactory(
        IRegistryReader<DeviceId, IStrengthInfo> strengthInfoRegistryReader,
        IRegistryReader<DeviceId, IOutputParameterConfig> outputParameterConfigRegistryReader,
        IRegistryReader<DeviceId, IBatteryInfo> batteryInfoRegistryReader,
        IRegistryReader<DeviceId, ILightsColorConfig> lightsColorConfigRegistryReader,
        IRegistryReader<DeviceId, IClientInfo> clientInfoRegistryReader,
        IDeviceConnectionService deviceConnectionService,
        IQueueService queueService,
        ILocalizationService localizationService,
        INameRegistry<WaveformId> waveformNameTable,
        IWaveformPlaylist waveformPlaylistA,
        IWaveformPlaylist waveformPlaylistB,
        IRegistryReader<WaveformDecoderId, IWaveformDecoderConfig> waveformDecoderConfigRegistryReaderA,
        IRegistryReader<WaveformDecoderId, IWaveformDecoderConfig> waveformDecoderConfigRegistryReaderB,
        IWaveformPlayNotification waveformPlayNotificationA,
        IWaveformPlayNotification waveformPlayNotificationB)
    {
        _strengthInfoRegistryReader = strengthInfoRegistryReader;
        _outputParameterConfigRegistryReader = outputParameterConfigRegistryReader;
        _batteryInfoRegistryReader = batteryInfoRegistryReader;
        _lightsColorConfigRegistryReader = lightsColorConfigRegistryReader;
        _clientInfoRegistryReader = clientInfoRegistryReader;
        _deviceConnectionService = deviceConnectionService;
        _queueService = queueService;
        _localizationService = localizationService;
        _waveformNameTable = waveformNameTable;
        _waveformPlaylistA = waveformPlaylistA;
        _waveformPlaylistB = waveformPlaylistB;
        _waveformDecoderConfigRegistryReaderA = waveformDecoderConfigRegistryReaderA;
        _waveformDecoderConfigRegistryReaderB = waveformDecoderConfigRegistryReaderB;
        _waveformPlayNotificationA = waveformPlayNotificationA;
        _waveformPlayNotificationB = waveformPlayNotificationB;
    }

    CoyoteItemBase ICoyoteItemBaseFactory.Create(IDeviceInfo deviceInfo)
    {
        switch (deviceInfo.ConnectionKind)
        {
            default:
            case ConnectionKind.Unknown:
                throw new ArgumentException(null, nameof(deviceInfo));
            case ConnectionKind.BLE:
                var id = deviceInfo.Id;
                if (_strengthInfoRegistryReader.TryGetValue(id, out var strengthInfo)
                    && _outputParameterConfigRegistryReader.TryGetValue(id, out var outputParameterConfig)
                    && _batteryInfoRegistryReader.TryGetValue(id, out var batteryInfo)
                    && _lightsColorConfigRegistryReader.TryGetValue(id, out var lightsColorConfig))
                {
                    return new CoyoteBleItem(deviceInfo, strengthInfo, outputParameterConfig, batteryInfo, lightsColorConfig, _deviceConnectionService, _queueService, _localizationService);
                }
                break;
            case ConnectionKind.WebSocket:
                id = deviceInfo.Id;
                if (_strengthInfoRegistryReader.TryGetValue(id, out strengthInfo)
                    && _clientInfoRegistryReader.TryGetValue(id, out var clientInfo))
                {
                    return new CoyoteWsItem(deviceInfo, strengthInfo, clientInfo, _deviceConnectionService, _queueService, _localizationService);
                }
                break;
        }

        throw new Exception();
    }

    public WaveformItemBase CreateWaveformItem(IWaveformInfo waveformInfo, Func<ILocalizationService, string>? defaultNameGetter)
    {
        if (defaultNameGetter is null)
            return new CustomWaveformItem(waveformInfo.Id, _waveformNameTable, _waveformPlaylistA, _waveformPlaylistB);
        else
            return new ClassicWaveformItem(waveformInfo.Id, _waveformNameTable, _waveformPlaylistA, _waveformPlaylistB, defaultNameGetter, _queueService, _localizationService);
    }

    public WaveformDecoderItem CreateWaveformDecoderItem(IWaveformDecoderInfo waveformDecoderInfo, Channel channel)
    {
        switch (channel)
        {
            default:
            case Channel.None:
                throw new ArgumentException(null, nameof(channel));
            case Channel.A:
                if (_waveformDecoderConfigRegistryReaderA.TryGetValue(waveformDecoderInfo.WaveformDecoderId, out var waveformDecoderConfig))
                    return new WaveformDecoderItem(waveformDecoderInfo, _waveformNameTable, waveformDecoderConfig, _waveformPlayNotificationA, _queueService);
                break;
            case Channel.B:
                if (_waveformDecoderConfigRegistryReaderB.TryGetValue(waveformDecoderInfo.WaveformDecoderId, out waveformDecoderConfig))
                    return new WaveformDecoderItem(waveformDecoderInfo, _waveformNameTable, waveformDecoderConfig, _waveformPlayNotificationB, _queueService);
                break;
        }

        throw new ArgumentException(null, nameof(waveformDecoderInfo));
    }
}
