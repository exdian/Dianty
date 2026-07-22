using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dianty.Services;
using DungeonToolkit.Coyote;
using System;

namespace Dianty.ViewModels;

public partial class WaveItem(Wave wave) : ObservableObject
{
    public Wave Wave { get; } = wave;

    [ObservableProperty]
    public partial string Name { get; set; } = wave.Name;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsEnabledA { get; set; }

    [ObservableProperty]
    public partial bool IsEnabledB { get; set; }

    public event EventHandler<WaveItem, ChannelEnabledChangedEventArgs>? ChannelEnabledChanged;

    private void OnChannelEnabledChanged(Channel channel)
    {
        var args = new ChannelEnabledChangedEventArgs(channel);
        ChannelEnabledChanged?.Invoke(this, args);
    }

    partial void OnNameChanged(string value)
    {
        Wave.Name = value;
    }

    partial void OnIsEnabledAChanged(bool value)
    {
        OnChannelEnabledChanged(Channel.A);
    }

    partial void OnIsEnabledBChanged(bool value)
    {
        OnChannelEnabledChanged(Channel.B);
    }

    public class ChannelEnabledChangedEventArgs(Channel channel) : EventArgs
    {
        public Channel Channel { get; } = channel;
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

    [RelayCommand]
    private static void SwitchWaveSpeed(WavePlayingItem wavePlayingItem)
    {
        var speed = wavePlayingItem.Speed;
        if (speed == 1 || speed == 2)
            wavePlayingItem.Speed = speed << 1;
        else
            wavePlayingItem.Speed = 1;
    }

    private void OnPlayingStatusChanged(object? sender, WavePlayer.PlayingStatusChangedEventArgs e)
    {
        _queueService.TryEnqueue(() => IsPlaying = e.IsPlaying);
    }

    partial void OnSpeedChanged(int value)
    {
        WavePlayer.Speed = value;
    }
}
