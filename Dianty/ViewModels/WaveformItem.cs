using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dianty.EventArguments;
using Dianty.Services;
using DungeonToolkit.Coyote;
using System;

namespace Dianty.ViewModels;

public abstract partial class WaveformItemBase : ObservableObject
{
    private readonly INameRegistryWriter<WaveformId> _nameTable;
    private readonly IWaveformPlaylist _channelA;
    private readonly IWaveformPlaylist _channelB;

    protected WaveformItemBase(
        WaveformId id,
        INameRegistryWriter<WaveformId> nameTable,
        IWaveformPlaylist channelA,
        IWaveformPlaylist channelB)
    {
        Id = id;
        _nameTable = nameTable;
        _channelA = channelA;
        _channelB = channelB;
    }

    public WaveformId Id { get; }

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsEnabledA { get; set; }

    [ObservableProperty]
    public partial bool IsEnabledB { get; set; }

    partial void OnNameChanged(string value)
    {
        _nameTable.Rename(Id, value);
    }

    partial void OnIsEnabledAChanged(bool value)
    {
        if (value)
            _channelA.Add(Id);
        else
            _channelA.Remove(Id);
    }

    partial void OnIsEnabledBChanged(bool value)
    {
        if (value)
            _channelB.Add(Id);
        else
            _channelB.Remove(Id);
    }
}

public sealed partial class ClassicWaveformItem : WaveformItemBase, IDisposable
{
    private readonly Func<ILocalizationService, string> _defaultNameGetter;
    private readonly IQueueService _queueService;
    private readonly ILocalizationService _localizationService;
    private string _defaultName;
    private bool _isDisposed;

    public ClassicWaveformItem(
        WaveformId id,
        INameRegistryWriter<WaveformId> nameTable,
        IWaveformPlaylist channelA,
        IWaveformPlaylist channelB,
        Func<ILocalizationService, string> defaultNameGetter,
        IQueueService queueService,
        ILocalizationService localizationService)
        : base(id, nameTable, channelA, channelB)
    {
        _defaultNameGetter = defaultNameGetter;
        _queueService = queueService;
        _localizationService = localizationService;

        _defaultName = _defaultNameGetter.Invoke(_localizationService);
        Name = _defaultName;

        _localizationService.CurrentLanguageChanged += OnCurrentLanguageChanged;
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
            _localizationService.CurrentLanguageChanged -= OnCurrentLanguageChanged;
        }

        _isDisposed = true;
    }

    private void OnCurrentLanguageChanged(object? sender, CurrentLanguageChangedEventArgs e)
    {
        var defaultName = _defaultNameGetter.Invoke(_localizationService);
        _queueService.TryEnqueue(() =>
        {
            if (Name == _defaultName)
                Name = defaultName;
            _defaultName = defaultName;
        });
    }
}

public sealed partial class CustomWaveformItem(
    WaveformId id,
    INameRegistryWriter<WaveformId> nameTable,
    IWaveformPlaylist channelA,
    IWaveformPlaylist channelB)
    : WaveformItemBase(id, nameTable, channelA, channelB);

public sealed partial class WaveformDecoderItem : ObservableObject, IDisposable
{
    private readonly IWaveformDecoderInfo _waveformDecoderInfo;
    private readonly INameRegistryReader<WaveformId> _waveformNameTable;
    private readonly IWaveformDecoderConfig _waveformDecoderConfig;
    private readonly IWaveformPlayNotification _playNotification;
    private readonly IQueueService _queueService;
    private bool _isDisposed;

    public WaveformDecoderItem(
        IWaveformDecoderInfo waveformDecoderInfo,
        INameRegistryReader<WaveformId> WaveformNameTable,
        IWaveformDecoderConfig waveformDecoderConfig,
        IWaveformPlayNotification playNotification,
        IQueueService queueService)
    {
        _waveformDecoderInfo = waveformDecoderInfo;
        _waveformNameTable = WaveformNameTable;
        _waveformDecoderConfig = waveformDecoderConfig;
        _playNotification = playNotification;
        _queueService = queueService;

        Name = _waveformNameTable.GetName(WaveformId);
        Speed = _waveformDecoderInfo.Speed;

        _waveformNameTable.RegisteredNameChanged += OnRegisteredNameChanged;
        _playNotification.CurrentWaveformChanged += OnPlayingWaveformChanged;
    }

    public WaveformId WaveformId => _waveformDecoderInfo.WaveformId;

    public WaveformDecoderId WaveformDecoderId => _waveformDecoderInfo.WaveformDecoderId;

    [ObservableProperty]
    public partial string Name { get; private set; }

    [ObservableProperty]
    public partial int Speed { get; private set; }

    [ObservableProperty]
    public partial bool IsPlaying { get; private set; }

    [RelayCommand]
    private void SwitchWaveformSpeed()
    {
        var speed = Speed == 4 ? 1 : Speed << 1;
        Speed = _waveformDecoderConfig.SetSpeed(speed);
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
            _waveformNameTable.RegisteredNameChanged -= OnRegisteredNameChanged;
            _playNotification.CurrentWaveformChanged -= OnPlayingWaveformChanged;
        }

        _isDisposed = true;
    }

    private void OnRegisteredNameChanged(object? sender, RegisteredNameChangedEventArgs<WaveformId> e)
    {
        _queueService.TryEnqueue(() =>
        {
            if (e.Key == WaveformId)
                Name = e.Name;
        });
    }

    private void OnPlayingWaveformChanged(object? sender, EventArgs e)
    {
        _queueService.TryEnqueue(() => IsPlaying = _playNotification.CurrentWaveformId == _waveformDecoderInfo.WaveformId);
    }
}
