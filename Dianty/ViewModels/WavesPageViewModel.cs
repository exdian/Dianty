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
    public WavesPageViewModel(CoyoteManager coyoteManager, IQueueService queueService, IWindowService windowService, ILocalizationService localizationService)
    {
        _coyoteManager = coyoteManager;
        _queueService = queueService;
        _windowService = windowService;
        _localizationService = localizationService;

        var waves = new List<Wave>
        {
            Wave.FromUtf8Data("Dungeonlab+pulse:35,1,8=0,20,0,1,1/0.00-1,20.00-0,40.00-0,60.00-0,80.00-0,100.00-1,100.00-1,100.00-1"u8.ToArray(),
                _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.ClassicWavesNameLabel.Breathing),
            Wave.FromUtf8Data("Dungeonlab+pulse:5,1,8=0,32,19,2,1/0.00-1,16.65-0,33.30-0,50.00-0,66.65-0,83.30-0,100.00-1,92.00-0,84.00-0,76.00-0,68.00-1"u8.ToArray(),
                _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.ClassicWavesNameLabel.Tide),
            Wave.FromUtf8Data("Dungeonlab+pulse:0,1,8=0,34,19,1,1/100.00-1,0.00-1,100.00-1,66.65-0,33.30-0,0.00-1,0.00-0,0.00-1"u8.ToArray(),
                _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.ClassicWavesNameLabel.Pulsating),
            Wave.FromUtf8Data("Dungeonlab+pulse:16,1,8=0,29,43,1,1/0.00-1,100.00-1"u8.ToArray(),
                _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.ClassicWavesNameLabel.QuickRub),
            Wave.FromUtf8Data("Dungeonlab+pulse:5,1,8=0,20,19,1,1/0.00-1,28.55-1,0.00-1,52.50-1,0.00-1,73.40-1,0.00-1,87.25-1,0.00-1,100.00-1,0.00-1"u8.ToArray(),
                _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.ClassicWavesNameLabel.GradualRub),
            Wave.FromUtf8Data("Dungeonlab+pulse:5,1,16=65,20,5,1,1/100.00-1,100.00-1+section+0,20,19,1,1/0.00-1,0.00-0,0.00-0,0.00-0,0.00-1,75.00-1,83.30-0,91.65-0,100.00-1,0.00-1,0.00-0,0.00-0,0.00-0,0.00-1"u8.ToArray(),
                _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.ClassicWavesNameLabel.Heartbeat),
            Wave.FromUtf8Data("Dungeonlab+pulse:0,1,16=52,16,0,2,1/100.00-1,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-1+section+0,20,0,1,1/100.00-1,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-1"u8.ToArray(),
                _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.ClassicWavesNameLabel.Compress),
            Wave.FromUtf8Data("Dungeonlab+pulse:5,1,8=0,20,19,1,1/0.00-1,20.00-0,40.00-0,60.00-0,80.00-0,100.00-1,0.00-1,25.00-0,50.00-0,75.00-0,100.00-1,0.00-1,33.30-0,66.65-0,100.00-1,0.00-1,50.00-0,100.00-1,0.00-1,100.00-1,0.00-1,100.00-1,0.00-1,100.00-1,0.00-1,100.00-1"u8.ToArray(),
                _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.ClassicWavesNameLabel.Rhythmic),
            Wave.FromUtf8Data("Dungeonlab+pulse:0,1,8=0,38,24,2,1/100.00-1,100.00-0,100.00-1,0.00-1"u8.ToArray(),
                _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.ClassicWavesNameLabel.Grainy),
            Wave.FromUtf8Data("Dungeonlab+pulse:20,1,16=0,30,44,2,1/0.00-1,33.30-0,66.65-0,100.00-1"u8.ToArray(),
                _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.ClassicWavesNameLabel.Bouncy),
            Wave.FromUtf8Data("Dungeonlab+pulse:5,1,16=0,60,51,4,1/0.00-1,50.00-0,100.00-1,73.35-1"u8.ToArray(),
                _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.ClassicWavesNameLabel.Ripple),
            Wave.FromUtf8Data("Dungeonlab+pulse:25,1,8=4,0,38,1,1/33.50-1,66.75-0,100.00-1+section+44,54,34,1,1/100.00-1,100.00-1"u8.ToArray(),
                _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.ClassicWavesNameLabel.Rainfall),
            Wave.FromUtf8Data("Dungeonlab+pulse:15,1,8=14,20,40,1,1/100.00-1,100.00-0,100.00-1,0.00-1,0.00-0,0.00-0,0.00-1+section+65,20,39,1,1/100.00-1,100.00-0,100.00-0,100.00-1"u8.ToArray(),
                _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.ClassicWavesNameLabel.TempoTap),
            Wave.FromUtf8Data("Dungeonlab+pulse:0,1,8=78,64,19,1,1/100.00-1,100.00-0,100.00-0,100.00-1+section+0,20,19,3,1/0.00-1,33.30-0,66.65-0,100.00-1"u8.ToArray(),
                _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.ClassicWavesNameLabel.Signal),
            Wave.FromUtf8Data("Dungeonlab+pulse:5,1,8=0,20,35,3,1/0.00-1,25.00-0,50.00-0,75.00-0,100.00-1,100.00-1,100.00-1,0.00-1,0.00-0,0.00-1+section+0,20,21,1,1/0.00-1,100.00-1"u8.ToArray(),
                _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.ClassicWavesNameLabel.Tease1),
            Wave.FromUtf8Data("Dungeonlab+pulse:18,1,8=27,7,32,3,1/0.00-1,11.10-0,22.20-0,33.30-0,44.40-0,55.50-0,66.60-0,77.70-0,88.80-0,100.00-1+section+0,20,39,2,1/0.00-1,100.00-1"u8.ToArray(),
                _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.ClassicWavesNameLabel.Tease2),
        };
        foreach (var wave in waves)
        {
            var waveItem = new WaveItem(wave);
            waveItem.ChannelEnabledChanged += OnWaveItemChannelEnabledChanged;
            WaveItems.Add(waveItem);
        }
        WavePlayingItemsA.CollectionChanged += OnWavePlayingItemsACollectionChanged;
        WavePlayingItemsB.CollectionChanged += OnWavePlayingItemsBCollectionChanged;

        WavePlaybackModeItems =
            [new WavePlaybackModeSelectionItem(WavePlaybackMode.RepeatAll, "\uE8EE"),
            new WavePlaybackModeSelectionItem(WavePlaybackMode.Shuffle, "\uE8B1"),
            new WavePlaybackModeSelectionItem(WavePlaybackMode.RepeatOne, "\uE8ED"),];
        WavePlaybackModeA = WavePlaybackModeItems[0];
        WavePlaybackModeB = WavePlaybackModeItems[0];

        WaveIntervalItems = [5, 10, 20, 30, 50, 100, 200, 300, 600, 1200, 3000];
        WaveIntervalA = 50;
        WaveIntervalB = 50;
    }

    private readonly CoyoteManager _coyoteManager;
    private readonly IQueueService _queueService;
    private readonly IWindowService _windowService;
    private readonly ILocalizationService _localizationService;

    private string WaveFilePickerTitle =>
        _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.WaveFilePickerTitle;
    private string WaveFilesLabel =>
        _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.WaveFilesLabel;
    private string AllFilesLabel =>
        _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.AllFilesLabel;
    private string DefaultWaveNameLabel =>
        _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.DefaultWaveNameLabel;

    public ObservableCollection<WaveItem> WaveItems { get; } = [];
    public ObservableCollection<WavePlayingItem> WavePlayingItemsA { get; } = [];
    public ObservableCollection<WavePlayingItem> WavePlayingItemsB { get; } = [];
    public WavePlaybackModeSelectionItem[] WavePlaybackModeItems { get; }
    public int[] WaveIntervalItems { get; }

    [ObservableProperty]
    public partial bool IsChannelEnabledA { get; set; }

    [ObservableProperty]
    public partial bool IsChannelEnabledB { get; set; }

    [ObservableProperty]
    public partial WavePlaybackModeSelectionItem WavePlaybackModeA { get; set; }

    [ObservableProperty]
    public partial WavePlaybackModeSelectionItem WavePlaybackModeB { get; set; }

    [ObservableProperty]
    public partial int WaveIntervalA { get; set; }

    [ObservableProperty]
    public partial int WaveIntervalB { get; set; }

    [RelayCommand]
    private async Task ImportWaveAsync(WindowId windowId)
    {
        var fileOpenPicker = new FileOpenPicker(windowId) { Title = WaveFilePickerTitle };
#pragma warning disable IDE0028 // 此处使用集合表达式会影响 AOT 编译
        fileOpenPicker.FileTypeChoices[$"{WaveFilesLabel} (*.pulse)"] = new List<string> { ".pulse" };
        fileOpenPicker.FileTypeChoices[WaveFilesLabel] = new List<string> { ".pulse", ".txt", ".bin" };
        fileOpenPicker.FileTypeChoices[$"{AllFilesLabel} (*.*)"] = new List<string> { "*" };
#pragma warning restore IDE0028

        var files = await fileOpenPicker.PickMultipleFilesAsync();
        if (files.Count > 0)
        {
            Dictionary<string, List<string>> exceptionMessage = [];
            var result = new List<WaveItem>();
            foreach (var path in files.Select(f => f.Path))
            {
                try
                {
                    var wave = await Wave.FromFileNameAsync(path, CancellationToken.None);
                    var name = Path.GetFileNameWithoutExtension(path);
                    wave.Name = string.IsNullOrEmpty(name) ? DefaultWaveNameLabel : name;
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
                var dialogText = _localizationService.AppText.MainWindowText.MainViewText.WavesPageText.ErrorProcessingFileDialogText;
                var messageBuffer = new StringBuilder();
                messageBuffer.AppendLine(dialogText.ContentFirstLineText);
                foreach (var message in exceptionMessage)
                {
                    var fileNames = message.Value;
                    messageBuffer.Append(message.Key);
                    messageBuffer.AppendLine(string.Format(dialogText.FailureCountFormat, fileNames.Count));
                    for (int i = 0; i < fileNames.Count; i++)
                    {
                        messageBuffer.AppendLine(fileNames[i]);
                    }
                }
                _queueService.TryEnqueue(async () =>
                {
                    var dialog = _windowService.CreateContentDialog();
                    if (dialog == null)
                        return;
                    dialog.Title = result.Count > 0 ? dialogText.PartiallyFailedTitle : dialogText.AllFailedTitle;
                    dialog.CloseButtonText = dialogText.CloseButtonText;
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
        _coyoteManager.ChannelA.NextWave();
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
        _coyoteManager.ChannelB.NextWave();
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

    partial void OnWavePlaybackModeAChanged(WavePlaybackModeSelectionItem value)
    {
        _coyoteManager.ChannelA.PlaybackMode = value.Mode;
    }

    partial void OnWavePlaybackModeBChanged(WavePlaybackModeSelectionItem value)
    {
        _coyoteManager.ChannelB.PlaybackMode = value.Mode;
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

public readonly record struct WavePlaybackModeSelectionItem(WavePlaybackMode Mode, string Description);
