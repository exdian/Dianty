using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dianty.Utils;
using DungeonToolkit.Coyote;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Dianty.ViewModels;

public partial class WavesPageViewModel : ObservableObject
{
    public WavesPageViewModel(CoyoteManager coyoteManager)
    {
        _coyoteManager = coyoteManager;
        var waves = new List<Wave>
        {
            Wave.FromUtf8Data("Dungeonlab+pulse:35,1,8=0,20,0,1,1/0.00-1,20.00-0,40.00-0,60.00-0,80.00-0,100.00-1,100.00-1,100.00-1"u8.ToArray(), "呼吸"),
            Wave.FromUtf8Data("Dungeonlab+pulse:5,1,8=0,32,19,2,1/0.00-1,16.65-0,33.30-0,50.00-0,66.65-0,83.30-0,100.00-1,92.00-0,84.00-0,76.00-0,68.00-1"u8.ToArray(), "潮汐"),
            Wave.FromUtf8Data("Dungeonlab+pulse:0,1,8=0,34,19,1,1/100.00-1,0.00-1,100.00-1,66.65-0,33.30-0,0.00-1,0.00-0,0.00-1"u8.ToArray(), "连击"),
            Wave.FromUtf8Data("Dungeonlab+pulse:16,1,8=0,29,43,1,1/0.00-1,100.00-1"u8.ToArray(), "快速按捏"),
            Wave.FromUtf8Data("Dungeonlab+pulse:5,1,8=0,20,19,1,1/0.00-1,28.55-1,0.00-1,52.50-1,0.00-1,73.40-1,0.00-1,87.25-1,0.00-1,100.00-1,0.00-1"u8.ToArray(), "按捏渐强"),
            Wave.FromUtf8Data("Dungeonlab+pulse:5,1,16=65,20,5,1,1/100.00-1,100.00-1+section+0,20,19,1,1/0.00-1,0.00-0,0.00-0,0.00-0,0.00-1,75.00-1,83.30-0,91.65-0,100.00-1,0.00-1,0.00-0,0.00-0,0.00-0,0.00-1"u8.ToArray(), "心跳节奏"),
            Wave.FromUtf8Data("Dungeonlab+pulse:0,1,16=52,16,0,2,1/100.00-1,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-1+section+0,20,0,1,1/100.00-1,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-1"u8.ToArray(), "压缩"),
            Wave.FromUtf8Data("Dungeonlab+pulse:5,1,8=0,20,19,1,1/0.00-1,20.00-0,40.00-0,60.00-0,80.00-0,100.00-1,0.00-1,25.00-0,50.00-0,75.00-0,100.00-1,0.00-1,33.30-0,66.65-0,100.00-1,0.00-1,50.00-0,100.00-1,0.00-1,100.00-1,0.00-1,100.00-1,0.00-1,100.00-1,0.00-1,100.00-1"u8.ToArray(), "节奏步伐"),
            Wave.FromUtf8Data("Dungeonlab+pulse:0,1,8=0,38,24,2,1/100.00-1,100.00-0,100.00-1,0.00-1"u8.ToArray(), "颗粒摩擦"),
            Wave.FromUtf8Data("Dungeonlab+pulse:20,1,16=0,30,44,2,1/0.00-1,33.30-0,66.65-0,100.00-1"u8.ToArray(), "渐变弹跳"),
            Wave.FromUtf8Data("Dungeonlab+pulse:5,1,16=0,60,51,4,1/0.00-1,50.00-0,100.00-1,73.35-1"u8.ToArray(), "波浪涟漪"),
            Wave.FromUtf8Data("Dungeonlab+pulse:25,1,8=4,0,38,1,1/33.50-1,66.75-0,100.00-1+section+44,54,34,1,1/100.00-1,100.00-1"u8.ToArray(), "雨水冲刷"),
            Wave.FromUtf8Data("Dungeonlab+pulse:15,1,8=14,20,40,1,1/100.00-1,100.00-0,100.00-1,0.00-1,0.00-0,0.00-0,0.00-1+section+65,20,39,1,1/100.00-1,100.00-0,100.00-0,100.00-1"u8.ToArray(), "变速敲击"),
            Wave.FromUtf8Data("Dungeonlab+pulse:0,1,8=78,64,19,1,1/100.00-1,100.00-0,100.00-0,100.00-1+section+0,20,19,3,1/0.00-1,33.30-0,66.65-0,100.00-1"u8.ToArray(), "信号灯"),
            Wave.FromUtf8Data("Dungeonlab+pulse:5,1,8=0,20,35,3,1/0.00-1,25.00-0,50.00-0,75.00-0,100.00-1,100.00-1,100.00-1,0.00-1,0.00-0,0.00-1+section+0,20,21,1,1/0.00-1,100.00-1"u8.ToArray(), "挑逗1"),
            Wave.FromUtf8Data("Dungeonlab+pulse:18,1,8=27,7,32,3,1/0.00-1,11.10-0,22.20-0,33.30-0,44.40-0,55.50-0,66.60-0,77.70-0,88.80-0,100.00-1+section+0,20,39,2,1/0.00-1,100.00-1"u8.ToArray(), "挑逗2"),
        };
        foreach (var wave in waves)
        {
            WaveItems.Add(new WaveItem(wave));
        }
    }

    private readonly CoyoteManager _coyoteManager;

    public ObservableCollection<WaveItem> WaveItems { get; } = [];
    public ObservableCollection<WavePlayingItem> WavePlayingItemsA { get; } = [];
    public ObservableCollection<WavePlayingItem> WavePlayingItemsB { get; } = [];

    [RelayCommand]
    private void SwitchWaveA(WaveItem waveItem)
    {
        if (waveItem.IsEnabledA ?? false)
        {
            _coyoteManager.ChannelA.Add(waveItem.Wave);
            WavePlayingItemsA.Add(new WavePlayingItem(waveItem));
        }
        else
        {
            _coyoteManager.ChannelA.Remove(waveItem.Wave);
            var index = WavePlayingItemsA.FirstIndex(w => w.WaveItem == waveItem);
            Debug.Assert(index >= 0);
            if (index >= 0)
                WavePlayingItemsA.RemoveAt(index);
        }
    }

    [RelayCommand]
    private void SwitchWaveB(WaveItem waveItem)
    {
        if (waveItem.IsEnabledB ?? false)
        {
            _coyoteManager.ChannelB.Add(waveItem.Wave);
            WavePlayingItemsB.Add(new WavePlayingItem(waveItem));
        }
        else
        {
            _coyoteManager.ChannelB.Remove(waveItem.Wave);
            var index = WavePlayingItemsB.FirstIndex(w => w.WaveItem == waveItem);
            Debug.Assert(index >= 0);
            if (index >= 0)
                WavePlayingItemsB.RemoveAt(index);
        }
    }
}
