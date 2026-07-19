using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dianty.Services;
using Dianty.Utils;
using DungeonToolkit.Coyote;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dianty.ViewModels;

public partial class WavesPageViewModel : ObservableObject
{
    public WavesPageViewModel(CoyoteManager coyoteManager, IQueueService queueService, IWindowService windowService)
    {
        _coyoteManager = coyoteManager;
        _queueService = queueService;
        _windowService = windowService;

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
            var waveItem = new WaveItem(wave);
            waveItem.ChannelEnabledChanged += OnWaveItemChannelEnabledChanged;
            WaveItems.Add(waveItem);
        }
        WavePlayingItemsA.CollectionChanged += OnWavePlayingItemsACollectionChanged;
        WavePlayingItemsB.CollectionChanged += OnWavePlayingItemsBCollectionChanged;

        WavePlayingModeItems =
            [new WavePlayingModeSelectionItem(WavePlayMode.RepeatAll, "\uE8EE"),
            new WavePlayingModeSelectionItem(WavePlayMode.Shuffle, "\uE8B1"),
            new WavePlayingModeSelectionItem(WavePlayMode.RepeatOne, "\uE8ED"),];
        WavePlayingModeA = WavePlayingModeItems[0];
        WavePlayingModeB = WavePlayingModeItems[0];

        WaveIntervalItems = [5, 10, 20, 30, 50, 100, 200, 300, 600, 1200, 3000];
        WaveIntervalA = 50;
        WaveIntervalB = 50;
    }

    private readonly CoyoteManager _coyoteManager;
    private readonly IQueueService _queueService;
    private readonly IWindowService _windowService;

    public ObservableCollection<WaveItem> WaveItems { get; } = [];
    public ObservableCollection<WavePlayingItem> WavePlayingItemsA { get; } = [];
    public ObservableCollection<WavePlayingItem> WavePlayingItemsB { get; } = [];
    public WavePlayingModeSelectionItem[] WavePlayingModeItems { get; }
    public int[] WaveIntervalItems { get; }

    [ObservableProperty]
    public partial bool IsChannelEnabledA { get; set; }

    [ObservableProperty]
    public partial bool IsChannelEnabledB { get; set; }

    [ObservableProperty]
    public partial WavePlayingModeSelectionItem WavePlayingModeA { get; set; }

    [ObservableProperty]
    public partial WavePlayingModeSelectionItem WavePlayingModeB { get; set; }

    [ObservableProperty]
    public partial int WaveIntervalA { get; set; }

    [ObservableProperty]
    public partial int WaveIntervalB { get; set; }

    [RelayCommand]
    private async Task ImportWaveAsync(WindowId windowId)
    {
        var fileOpenPicker = new FileOpenPicker(windowId);
        fileOpenPicker.FileTypeChoices["波形文件 (*.pulse)"] = [".pulse"];
        fileOpenPicker.FileTypeChoices["波形文件"] = [".pulse", ".txt", ".bin",];
        fileOpenPicker.FileTypeChoices["所有文件 (*.*)"] = ["*"];

        var files = await fileOpenPicker.PickMultipleFilesAsync();
        if (files.Count > 0)
        {
            Dictionary<string, List<string>> exceptionMessage = [];
            var result = new List<WaveItem>();
            foreach (var path in files.Select(f => f.Path))
            {
                try
                {
                    var fileInfo = new FileInfo(path);
                    if (fileInfo.Length > 1000000)
                        throw new WaveException("文件过大");
                    var wave = await Wave.FromFileNameAsync(path, CancellationToken.None);
                    wave.Name = Path.GetFileNameWithoutExtension(path) ?? "新波形";
                    result.Add(new WaveItem(wave));
                }
                catch (Exception ex)
                {
                    if (!exceptionMessage.TryGetValue(ex.Message, out var messageList))
                    {
                        messageList = [];
                        exceptionMessage[ex.Message] = messageList;
                    }
                    messageList.Add(path);
                }
            }
            if (result.Count > 0)
            {
                _queueService.TryEnqueue(() =>
                {
                    for (int i = result.Count - 1, j = 0; i >= 0; i--, j++)
                    {
                        var waveItem = result[i];
                        waveItem.ChannelEnabledChanged += OnWaveItemChannelEnabledChanged;
                        WaveItems.Insert(j, waveItem);
                    }
                });
            }
            if (exceptionMessage.Count > 0)
            {
                var messageBuffer = new StringBuilder();
                messageBuffer.AppendLine("读取文件时出错:");
                foreach (var message in exceptionMessage)
                {
                    var fileNames = message.Value;
                    messageBuffer.Append(message.Key);
                    messageBuffer.AppendLine($"({fileNames.Count}个) :");
                    for (int i = 0; i < fileNames.Count; i++)
                    {
                        messageBuffer.AppendLine(fileNames[i]);
                    }
                }
                _queueService.TryEnqueue(async () =>
                {
                    var dialog = _windowService.CreateContentDialog();
                    if (dialog is null)
                        return;
                    dialog.Title = result.Count > 0 ? "处理部分文件时发生错误" : "发生错误";
                    dialog.CloseButtonText = "关闭";
                    dialog.IsPrimaryButtonEnabled = false;
                    dialog.IsSecondaryButtonEnabled = false;
                    dialog.DefaultButton = ContentDialogButton.Close;
                    dialog.Content = messageBuffer.ToString();
                    await dialog.ShowAsync();
                });
            }
        }
    }

    [RelayCommand]
    private void RemoveWave(WaveItem waveItem)
    {
        WaveItems.Remove(waveItem);
        RemoveWavePlayingItem(waveItem, WavePlayingItemsA, _coyoteManager.ChannelA);
        RemoveWavePlayingItem(waveItem, WavePlayingItemsB, _coyoteManager.ChannelB);
    }

    [RelayCommand]
    private void PlayWaveA(WavePlayingItem wavePlayingItem)
    {
        _coyoteManager.ChannelA.Play(wavePlayingItem.WavePlayer);
    }

    [RelayCommand]
    private void PlayWaveB(WavePlayingItem wavePlayingItem)
    {
        _coyoteManager.ChannelB.Play(wavePlayingItem.WavePlayer);
    }

    [RelayCommand]
    private static void RemovePlayingItemA(WavePlayingItem wavePlayingItem)
    {
        wavePlayingItem.WaveItem.IsEnabledA = false;
    }

    [RelayCommand]
    private static void RemovePlayingItemB(WavePlayingItem wavePlayingItem)
    {
        wavePlayingItem.WaveItem.IsEnabledB = false;
    }

    [RelayCommand]
    private void ClearPlayingItemsA()
    {
        // 单个删除可以保留删除动画，视觉效果更好
        for (int i = WavePlayingItemsA.Count - 1; i >= 0; i--)
        {
            WavePlayingItemsA.RemoveAt(i);
        }

        // 先清除 WavePlayingItems 再赋值 false 可以避免多次遍历 WavePlayingItems
        foreach (var item in WaveItems)
        {
            item.IsEnabledA = false;
        }
    }

    [RelayCommand]
    private void ClearPlayingItemsB()
    {
        for (int i = WavePlayingItemsB.Count - 1; i >= 0; i--)
        {
            WavePlayingItemsB.RemoveAt(i);
        }
        foreach (var item in WaveItems)
        {
            item.IsEnabledB = false;
        }
    }

    private void OnWaveItemChannelEnabledChanged(WaveItem sender, WaveItem.ChannelEnabledChangedEventArgs e)
    {
        switch (e.Channel)
        {
            default:
            case Channel.None:
                break;
            case Channel.A:
                if (sender.IsEnabledA)
                    WavePlayingItemsA.Add(new WavePlayingItem(sender, _queueService));
                else
                    RemoveWavePlayingItem(sender, WavePlayingItemsA, _coyoteManager.ChannelA);
                break;
            case Channel.B:
                if (sender.IsEnabledB)
                    WavePlayingItemsB.Add(new WavePlayingItem(sender, _queueService));
                else
                    RemoveWavePlayingItem(sender, WavePlayingItemsB, _coyoteManager.ChannelB);
                break;
        }
    }

    private void OnWavePlayingItemsACollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _coyoteManager.ChannelA.Replace(GetWavePlayers(WavePlayingItemsA));
    }

    private void OnWavePlayingItemsBCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _coyoteManager.ChannelB.Replace(GetWavePlayers(WavePlayingItemsB));
    }

    private static IEnumerable<WavePlayer> GetWavePlayers(IEnumerable<WavePlayingItem> wavePlayingItems)
    {
        foreach (var wavePlayingItem in wavePlayingItems)
        {
            yield return wavePlayingItem.WavePlayer;
        }
    }

    private static void RemoveWavePlayingItem(WaveItem waveItem, Collection<WavePlayingItem> wavePlayingItems, WaveQueue channel)
    {
        var index = wavePlayingItems.FirstIndex(w => w.WaveItem == waveItem);
        if (index >= 0)
        {
            if (channel.PlayingWave == wavePlayingItems[index].WavePlayer)
                channel.NextWave();
            wavePlayingItems.RemoveAt(index);
        }
    }

    partial void OnIsChannelEnabledAChanged(bool value)
    {
        _coyoteManager.ChannelA.IsEnabled = value;
    }

    partial void OnIsChannelEnabledBChanged(bool value)
    {
        _coyoteManager.ChannelB.IsEnabled = value;
    }

    partial void OnWavePlayingModeAChanged(WavePlayingModeSelectionItem value)
    {
        _coyoteManager.ChannelA.PlayMode = value.Mode;
    }

    partial void OnWavePlayingModeBChanged(WavePlayingModeSelectionItem value)
    {
        _coyoteManager.ChannelB.PlayMode = value.Mode;
    }

    partial void OnWaveIntervalAChanged(int value)
    {
        _coyoteManager.ChannelA.NextWaveInterval = value;
    }

    partial void OnWaveIntervalBChanged(int value)
    {
        _coyoteManager.ChannelB.NextWaveInterval = value;
    }
}

public readonly record struct WavePlayingModeSelectionItem(WavePlayMode Mode, string Description);
