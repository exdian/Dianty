using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.Services;
using DungeonToolkit.Coyote;

namespace Dianty.ViewModels;

public partial class WaveItem(Wave wave) : ObservableObject
{
    public Wave Wave { get; } = wave;

    [ObservableProperty]
    public partial string Name { get; set; } = wave.Name;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool? IsEnabledA { get; set; } = false;

    [ObservableProperty]
    public partial bool? IsEnabledB { get; set; } = false;

    partial void OnNameChanged(string value)
    {
        Wave.Name = value;
    }
}

public partial class WavePlayingItem : ObservableObject
{
    public WavePlayingItem(WaveItem waveItem, IQueueService queueService)
    {
        WaveItem = waveItem;
        _queueService = queueService;
        WavePlayer = new WavePlayer(waveItem.Wave);
        Speed = waveItem.Wave.Speed;
        IsPlaying = WavePlayer.IsPlaying;
        WavePlayer.PlayingStatusChanged += OnPlayingStatusChanged;
    }

    private readonly IQueueService _queueService;

    public WaveItem WaveItem { get; }

    public WavePlayer WavePlayer { get; }

    [ObservableProperty]
    public partial int Speed { get; set; }

    [ObservableProperty]
    public partial bool IsPlaying { get; private set; }

    private void OnPlayingStatusChanged(object? sender, WavePlayer.PlayingStatusChangedEventArgs e)
    {
        _queueService.TryEnqueue(() => IsPlaying = e.IsPlaying);
    }

    partial void OnSpeedChanged(int value)
    {
        WavePlayer.Speed = value;
    }
}
