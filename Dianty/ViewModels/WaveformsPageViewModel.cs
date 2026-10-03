using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dianty.Services;
using DungeonToolkit;
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

public sealed partial class WaveformsPageViewModel : ObservableObject, IDisposable
{
    private readonly IWaveformFactory _waveformFactory;
    private readonly IIdPool<WaveformId> _waveformIdPool;
    private readonly IRegistry<WaveformId, IWaveform> _waveformTable;
    private readonly IWaveformItemFactory _waveformItemFactory;
    private readonly IDataSource<IWaveformInfo> _waveformDataSource;
    private readonly INameRegistry<WaveformId> _waveformNameTable;
    private readonly IWaveformPlaylist _channelA;
    private readonly IWaveformPlaylist _channelB;
    private readonly IDataSource<IWaveformDecoderInfo> _channelADataSource;
    private readonly IDataSource<IWaveformDecoderInfo> _channelBDataSource;
    private readonly IOrderStore<WaveformDecoderId> _channelAOrder;
    private readonly IOrderStore<WaveformDecoderId> _channelBOrder;
    private readonly IWindowContentService _windowContentService;
    private readonly IQueueService _queueService;
    private readonly ILocalizationService _localizationService;
    private bool _isDisposed;

    public WaveformsPageViewModel(
        IWaveformFactory waveformFactory,
        IIdPool<WaveformId> waveformIdPool,
        IRegistry<WaveformId, IWaveform> waveformTable,
        IWaveformItemFactory waveformItemFactory,
        IDataSource<IWaveformInfo> waveformDataSource,
        INameRegistry<WaveformId> WaveformNameTable,
        IWaveformPlaylist channelA,
        IWaveformPlaylist channelB,
        IDataSource<IWaveformDecoderInfo> channelADataSource,
        IDataSource<IWaveformDecoderInfo> channelBDataSource,
        IOrderStore<WaveformDecoderId> channelAOrder,
        IOrderStore<WaveformDecoderId> channelBOrder,
        IWindowContentService windowContentService,
        IQueueService queueService,
        ILocalizationService localizationService)
    {
        _waveformFactory = waveformFactory;
        _waveformIdPool = waveformIdPool;
        _waveformTable = waveformTable;
        _waveformItemFactory = waveformItemFactory;
        _waveformDataSource = waveformDataSource;
        _waveformNameTable = WaveformNameTable;
        _channelA = channelA;
        _channelB = channelB;
        _channelADataSource = channelADataSource;
        _channelBDataSource = channelBDataSource;
        _channelAOrder = channelAOrder;
        _channelBOrder = channelBOrder;
        _windowContentService = windowContentService;
        _queueService = queueService;
        _localizationService = localizationService;

        WaveformPlaybackModeSelectionItems =
            [new WaveformPlaybackModeSelectionItem(WaveformPlaybackMode.RepeatAll, "\uE8EE"),
            new WaveformPlaybackModeSelectionItem(WaveformPlaybackMode.Shuffle, "\uE8B1"),
            new WaveformPlaybackModeSelectionItem(WaveformPlaybackMode.RepeatOne, "\uE8ED"),];
        SelectedWaveformPlaybackModeA = WaveformPlaybackModeSelectionItems[0];
        SelectedWaveformPlaybackModeB = WaveformPlaybackModeSelectionItems[0];

        WaveformSwitchIntervalItems = [5, 10, 20, 30, 50, 100, 200, 300, 600, 1200, 3000];
        SelectedWaveformIntervalA = 50;
        SelectedWaveformIntervalB = 50;

        AddClassicWaveform();

        WaveformPlayingItemsA.CollectionChanged += OnWaveformPlayingItemsACollectionChanged;
        WaveformPlayingItemsB.CollectionChanged += OnWaveformPlayingItemsBCollectionChanged;

        _waveformDataSource.Changed += OnWaveformDataSourceChanged;
        _channelADataSource.Changed += OnChannelADataSourceChanged;
        _channelBDataSource.Changed += OnChannelBDataSourceChanged;
    }

    private string WaveformFilePickerTitle =>
        _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.WaveformFilePickerTitle;
    private string WaveformFilesLabel =>
        _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.WaveformFilesLabel;
    private string AllFilesLabel =>
        _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.AllFilesLabel;
    private string DefaultWaveformNameLabel =>
        _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.DefaultWaveformNameLabel;

    public ObservableCollection<WaveformItemBase> WaveformItems { get; } = [];
    public ObservableCollection<WaveformDecoderItem> WaveformPlayingItemsA { get; } = [];
    public ObservableCollection<WaveformDecoderItem> WaveformPlayingItemsB { get; } = [];
    public WaveformPlaybackModeSelectionItem[] WaveformPlaybackModeSelectionItems { get; }
    public int[] WaveformSwitchIntervalItems { get; }

    [ObservableProperty]
    public partial bool IsChannelEnabledA { get; set; }

    [ObservableProperty]
    public partial bool IsChannelEnabledB { get; set; }

    [ObservableProperty]
    public partial WaveformPlaybackModeSelectionItem SelectedWaveformPlaybackModeA { get; set; }

    [ObservableProperty]
    public partial WaveformPlaybackModeSelectionItem SelectedWaveformPlaybackModeB { get; set; }

    [ObservableProperty]
    public partial int SelectedWaveformIntervalA { get; set; }

    [ObservableProperty]
    public partial int SelectedWaveformIntervalB { get; set; }

    [RelayCommand]
    private async Task ImportWaveformAsync(WindowId windowId)
    {
        var fileOpenPicker = new FileOpenPicker(windowId) { Title = WaveformFilePickerTitle };
#pragma warning disable IDE0028 // 此处使用集合表达式会影响 AOT 编译
        fileOpenPicker.FileTypeChoices[$"{WaveformFilesLabel} (*.pulse)"] = new List<string> { ".pulse" };
        fileOpenPicker.FileTypeChoices[WaveformFilesLabel] = new List<string> { ".pulse", ".txt", ".bin" };
        fileOpenPicker.FileTypeChoices[$"{AllFilesLabel} (*.*)"] = new List<string> { "*" };
#pragma warning restore IDE0028

        var files = await fileOpenPicker.PickMultipleFilesAsync();
        if (files.Count > 0)
        {
            Dictionary<string, List<string>> exceptionMessage = [];
            var result = new List<IWaveform>(files.Count);
            foreach (var path in files.Select(f => f.Path))
            {
                try
                {
                    var waveformData = await File.ReadAllTextAsync(path, CancellationToken.None).ConfigureAwait(false);
                    var name = Path.GetFileNameWithoutExtension(path);
                    name = string.IsNullOrEmpty(name) ? DefaultWaveformNameLabel : name;

                    var waveformId = _waveformIdPool.Rent();
                    IWaveform? waveform;
                    try
                    {
                        waveform = _waveformFactory.Create(waveformId, waveformData);
                    }
                    catch
                    {
                        _waveformIdPool.Return(waveformId);
                        throw;
                    }
                    result.Add(waveform);
                    _waveformNameTable.Rename(waveformId, name);
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

            _waveformTable.TryAdd(result, w => w.Id);

            if (exceptionMessage.Count > 0)
            {
                var dialogText = _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.ErrorProcessingFileDialogText;
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
                    var dialog = _windowContentService.CreateContentDialog();
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
    private void RemoveWaveform(WaveformItemBase waveformItem)
    {
        var id = waveformItem.Id;
        _waveformTable.Remove(id);
        _channelA.Remove(id);
        _channelB.Remove(id);
        _waveformIdPool.Return(id);
    }

    [RelayCommand]
    private void PlayWaveformA(WaveformDecoderItem waveformPlayingItem)
    {
        _channelA.Play(waveformPlayingItem.WaveformDecoderId);
    }

    [RelayCommand]
    private void PlayWaveformB(WaveformDecoderItem waveformPlayingItem)
    {
        _channelB.Play(waveformPlayingItem.WaveformDecoderId);
    }

    [RelayCommand]
    private void RemovePlaylistWaveformItemA(WaveformDecoderItem waveformPlayingItem)
    {
        _channelA.Remove(waveformPlayingItem.WaveformDecoderId);
    }

    [RelayCommand]
    private void RemovePlaylistWaveformItemB(WaveformDecoderItem waveformPlayingItem)
    {
        _channelB.Remove(waveformPlayingItem.WaveformDecoderId);
    }

    [RelayCommand]
    private void ClearPlayingItemsA()
    {
        _channelA.Clear();
    }

    [RelayCommand]
    private void ClearPlayingItemsB()
    {
        _channelB.Clear();
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
            _waveformDataSource.Changed -= OnWaveformDataSourceChanged;
            _channelADataSource.Changed -= OnChannelADataSourceChanged;
            _channelBDataSource.Changed -= OnChannelBDataSourceChanged;
            for (int i = 0; i < WaveformItems.Count; i++)
            {
                var item = WaveformItems[i];
                if (item is IDisposable disposable)
                    disposable.Dispose();
            }
            for (int i = 0; i < WaveformPlayingItemsA.Count; i++)
            {
                WaveformPlayingItemsA[i].Dispose();
            }
            for (int i = 0; i < WaveformPlayingItemsB.Count; i++)
            {
                WaveformPlayingItemsB[i].Dispose();
            }
        }

        _isDisposed = true;
    }

    private void AddClassicWaveform()
    {
        var waveformData = new Dictionary<string, Func<ILocalizationService, string>>
        {
            ["Dungeonlab+pulse:35,1,8=0,20,0,1,1/0.00-1,20.00-0,40.00-0,60.00-0,80.00-0,100.00-1,100.00-1,100.00-1"] = s => _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.ClassicWaveformsNameLabel.Breathing,
            ["Dungeonlab+pulse:5,1,8=0,32,19,2,1/0.00-1,16.65-0,33.30-0,50.00-0,66.65-0,83.30-0,100.00-1,92.00-0,84.00-0,76.00-0,68.00-1"] = s => _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.ClassicWaveformsNameLabel.Tide,
            ["Dungeonlab+pulse:0,1,8=0,34,19,1,1/100.00-1,0.00-1,100.00-1,66.65-0,33.30-0,0.00-1,0.00-0,0.00-1"] = s => _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.ClassicWaveformsNameLabel.Pulsating,
            ["Dungeonlab+pulse:16,1,8=0,29,43,1,1/0.00-1,100.00-1"] = s => _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.ClassicWaveformsNameLabel.QuickRub,
            ["Dungeonlab+pulse:5,1,8=0,20,19,1,1/0.00-1,28.55-1,0.00-1,52.50-1,0.00-1,73.40-1,0.00-1,87.25-1,0.00-1,100.00-1,0.00-1"] = s => _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.ClassicWaveformsNameLabel.GradualRub,
            ["Dungeonlab+pulse:5,1,16=65,20,5,1,1/100.00-1,100.00-1+section+0,20,19,1,1/0.00-1,0.00-0,0.00-0,0.00-0,0.00-1,75.00-1,83.30-0,91.65-0,100.00-1,0.00-1,0.00-0,0.00-0,0.00-0,0.00-1"] = s => _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.ClassicWaveformsNameLabel.Heartbeat,
            ["Dungeonlab+pulse:0,1,16=52,16,0,2,1/100.00-1,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-1+section+0,20,0,1,1/100.00-1,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-0,100.00-1"] = s => _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.ClassicWaveformsNameLabel.Compress,
            ["Dungeonlab+pulse:5,1,8=0,20,19,1,1/0.00-1,20.00-0,40.00-0,60.00-0,80.00-0,100.00-1,0.00-1,25.00-0,50.00-0,75.00-0,100.00-1,0.00-1,33.30-0,66.65-0,100.00-1,0.00-1,50.00-0,100.00-1,0.00-1,100.00-1,0.00-1,100.00-1,0.00-1,100.00-1,0.00-1,100.00-1"] = s => _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.ClassicWaveformsNameLabel.Rhythmic,
            ["Dungeonlab+pulse:0,1,8=0,38,24,2,1/100.00-1,100.00-0,100.00-1,0.00-1"] = s => _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.ClassicWaveformsNameLabel.Grainy,
            ["Dungeonlab+pulse:20,1,16=0,30,44,2,1/0.00-1,33.30-0,66.65-0,100.00-1"] = s => _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.ClassicWaveformsNameLabel.Bouncy,
            ["Dungeonlab+pulse:5,1,16=0,60,51,4,1/0.00-1,50.00-0,100.00-1,73.35-1"] = s => _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.ClassicWaveformsNameLabel.Ripple,
            ["Dungeonlab+pulse:25,1,8=4,0,38,1,1/33.50-1,66.75-0,100.00-1+section+44,54,34,1,1/100.00-1,100.00-1"] = s => _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.ClassicWaveformsNameLabel.Rainfall,
            ["Dungeonlab+pulse:15,1,8=14,20,40,1,1/100.00-1,100.00-0,100.00-1,0.00-1,0.00-0,0.00-0,0.00-1+section+65,20,39,1,1/100.00-1,100.00-0,100.00-0,100.00-1"] = s => _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.ClassicWaveformsNameLabel.TempoTap,
            ["Dungeonlab+pulse:0,1,8=78,64,19,1,1/100.00-1,100.00-0,100.00-0,100.00-1+section+0,20,19,3,1/0.00-1,33.30-0,66.65-0,100.00-1"] = s => _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.ClassicWaveformsNameLabel.Signal,
            ["Dungeonlab+pulse:5,1,8=0,20,35,3,1/0.00-1,25.00-0,50.00-0,75.00-0,100.00-1,100.00-1,100.00-1,0.00-1,0.00-0,0.00-1+section+0,20,21,1,1/0.00-1,100.00-1"] = s => _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.ClassicWaveformsNameLabel.Tease1,
            ["Dungeonlab+pulse:18,1,8=27,7,32,3,1/0.00-1,11.10-0,22.20-0,33.30-0,44.40-0,55.50-0,66.60-0,77.70-0,88.80-0,100.00-1+section+0,20,39,2,1/0.00-1,100.00-1"] = s => _localizationService.AppText.MainWindowText.MainViewText.WaveformsPageText.ClassicWaveformsNameLabel.Tease2,
        };

        var waveforms = new List<IWaveform>(waveformData.Count);
        foreach (var item in waveformData)
        {
            var waveformId = _waveformIdPool.Rent();
            IWaveform? waveform;
            WaveformItemBase? waveformItem;
            try
            {
                waveform = _waveformFactory.Create(waveformId, item.Key);
                waveformItem = _waveformItemFactory.CreateWaveformItem(waveform, item.Value);
            }
            catch
            {
                _waveformIdPool.Return(waveformId);
                continue;
            }
            waveforms.Add(waveform);
            WaveformItems.Add(waveformItem);
        }

        _waveformTable.TryAdd(waveforms, w => w.Id);
    }

    private void OnWaveformPlayingItemsACollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _channelAOrder.Save(WaveformPlayingItemsA.Select(d => d.WaveformDecoderId));
    }

    private void OnWaveformPlayingItemsBCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _channelBOrder.Save(WaveformPlayingItemsB.Select(d => d.WaveformDecoderId));
    }

    private void OnWaveformDataSourceChanged(object? sender, EventArgs e)
    {
        var snapshot = _waveformDataSource.GetSnapshot();
        var snapshotById = snapshot.Select(w => w.Id).ToHashSet();
        var existingIds = WaveformItems.Select(w => w.Id).ToHashSet();
        var newItems = snapshot.Where(w => !existingIds.Contains(w.Id));

        _queueService.TryEnqueue(() =>
        {
            for (int i = WaveformItems.Count - 1; i >= 0; i--)
            {
                var item = WaveformItems[i];
                if (!snapshotById.Contains(item.Id))
                {
                    WaveformItems.RemoveAt(i);
                    if (item is IDisposable disposable)
                        disposable.Dispose();
                }
            }

            int index = 0;
            foreach (var item in newItems)
            {
                var waveformItem = _waveformItemFactory.CreateWaveformItem(item, null);
                waveformItem.Name = _waveformNameTable.GetName(item.Id);
                WaveformItems.Insert(index, waveformItem);
                index++;
            }
        });
    }

    private void OnChannelADataSourceChanged(object? sender, EventArgs e)
    {
        UpdateWaveformPlayingItems(_channelADataSource, WaveformPlayingItemsA, Channel.A);
    }

    private void OnChannelBDataSourceChanged(object? sender, EventArgs e)
    {
        UpdateWaveformPlayingItems(_channelBDataSource, WaveformPlayingItemsB, Channel.B);
    }

    private void UpdateWaveformPlayingItems(
        IDataSource<IWaveformDecoderInfo> dataSource,
        ObservableCollection<WaveformDecoderItem> items,
        Channel channel)
    {
        var snapshot = dataSource.GetSnapshot();
        var snapshotByWaveformDecoderId = snapshot.Select(w => w.WaveformDecoderId).ToHashSet();
        var existingIds = items.Select(d => d.WaveformDecoderId).ToHashSet();
        var newItems = snapshot.Where(d => !existingIds.Contains(d.WaveformDecoderId));

        _queueService.TryEnqueue(() =>
        {
            var removedItemWaveformIds = new HashSet<WaveformId>();

            for (int i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                if (!snapshotByWaveformDecoderId.Contains(item.WaveformDecoderId))
                {
                    items.RemoveAt(i);
                    removedItemWaveformIds.Add(item.WaveformId);
                    item.Dispose();
                }
            }

            if (removedItemWaveformIds.Count > 0)
            {
                Action<WaveformItemBase> setProperty = channel switch
                {
                    Channel.A => w => w.IsEnabledA = false,
                    Channel.B => w => w.IsEnabledB = false,
                    _ => _ => { }
                };
                for (int i = 0; i < WaveformItems.Count; i++)
                {
                    var waveformItem = WaveformItems[i];
                    if (removedItemWaveformIds.Contains(waveformItem.Id))
                    {
                        setProperty.Invoke(waveformItem);
                    }
                }
            }

            foreach (var item in newItems)
            {
                WaveformDecoderItem waveformDecoderItem;
                try
                {
                    waveformDecoderItem = _waveformItemFactory.CreateWaveformDecoderItem(item, channel);
                }
                catch
                {
                    continue;
                }
                items.Add(waveformDecoderItem);
            }
        });
    }

    partial void OnIsChannelEnabledAChanged(bool value)
    {
        _channelA.IsEnabled = value;
    }

    partial void OnIsChannelEnabledBChanged(bool value)
    {
        _channelB.IsEnabled = value;
    }

    partial void OnSelectedWaveformPlaybackModeAChanged(WaveformPlaybackModeSelectionItem value)
    {
        _channelA.PlaybackMode = value.Mode;
    }

    partial void OnSelectedWaveformPlaybackModeBChanged(WaveformPlaybackModeSelectionItem value)
    {
        _channelB.PlaybackMode = value.Mode;
    }

    partial void OnSelectedWaveformIntervalAChanged(int value)
    {
        _channelA.SwitchInterval = value;
    }

    partial void OnSelectedWaveformIntervalBChanged(int value)
    {
        _channelB.SwitchInterval = value;
    }
}

public readonly record struct WaveformPlaybackModeSelectionItem(WaveformPlaybackMode Mode, string Description);
