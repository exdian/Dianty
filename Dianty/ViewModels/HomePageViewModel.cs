using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.Localization;
using Dianty.Models;
using Dianty.Services;
using DungeonToolkit.Coyote;
using System.Collections;
using System.Collections.Specialized;

namespace Dianty.ViewModels;

public partial class HomePageViewModel : ObservableObject
{
    public HomePageViewModel(CoyoteCollection coyoteItems, GameManager gameManager, IQueueService queueService)
    {
        _coyoteItems = coyoteItems;
        _gameManager = gameManager;
        _queueService = queueService;

        _coyoteItems.CollectionChanged += OnCoyoteCollectionChanged;
        _gameManager.CoyoteManager.ChannelA.PlayingWaveChanged += OnChannelAPlayingWaveChanged;
        _gameManager.CoyoteManager.ChannelB.PlayingWaveChanged += OnChannelBPlayingWaveChanged;
        _gameManager.CoyoteManager.OutputStatusChanged += OnCoyoteManagerOutputStatusChanged;
        _gameManager.OutputStrengthChanged += OnGameManagerOutputStrengthChanged;

        Localizer.Instance.CurrentLanguageFileNameChanged += OnCurrentLanguageFileNameChanged;
    }

    private readonly CoyoteCollection _coyoteItems;
    private readonly GameManager _gameManager;
    private readonly IQueueService _queueService;

    private static string StringNone => Localizer.Instance.AppText.MainWindowText.MainViewText.HomePageText.None;

    [ObservableProperty]
    public partial int ConnectedCount { get; private set; }

    [ObservableProperty]
    public partial int AddedCount { get; private set; }

    [ObservableProperty]
    public partial string WaveNameA { get; private set; } = StringNone;

    [ObservableProperty]
    public partial string WaveNameB { get; private set; } = StringNone;

    [ObservableProperty]
    public partial string CurrentStrength { get; private set; } = StringNone;

    private void OnCoyoteCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var newItems = e.NewItems;
        var oldItems = e.OldItems;
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (newItems is not null)
                    OnAddItems(newItems);
                break;
            case NotifyCollectionChangedAction.Remove:
                if (oldItems is not null)
                    OnRemoveItems(oldItems);
                break;
            case NotifyCollectionChangedAction.Replace:
                if (oldItems is not null)
                    OnRemoveItems(oldItems);
                if (newItems is not null)
                    OnAddItems(newItems);
                break;
            case NotifyCollectionChangedAction.Move:
                break;
            case NotifyCollectionChangedAction.Reset:
                ConnectedCount = 0;
                AddedCount = 0;
                // 此处未能取消事件订阅，如果在移除条目后继续对已移除的条目进行操作，会导致计数错误
                break;
            default:
                break;
        }
    }

    private void OnAddItems(IList list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            if (item is CoyoteBleItem coyoteBleItem)
            {
                AddedCount++;
                if (coyoteBleItem.Coyote.IsConnected)
                    ConnectedCount++;
                coyoteBleItem.Coyote.ConnectionStatusChanged += OnCoyoteConnectionStatusChanged;
            }
            else if (item is CoyoteWsItem coyoteWsItem)
            {
                AddedCount++;
                if (coyoteWsItem.Coyote.IsBound)
                    ConnectedCount++;
                coyoteWsItem.Coyote.BindingStatusChanged += OnCoyoteBindingStatusChanged;
            }
        }
    }

    private void OnRemoveItems(IList list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            if (item is CoyoteBleItem coyoteBleItem)
            {
                coyoteBleItem.Coyote.ConnectionStatusChanged -= OnCoyoteConnectionStatusChanged;
                if (coyoteBleItem.Coyote.IsConnected)
                    ConnectedCount--;
                AddedCount--;
            }
            else if (item is CoyoteWsItem coyoteWsItem)
            {
                coyoteWsItem.Coyote.BindingStatusChanged -= OnCoyoteBindingStatusChanged;
                if (coyoteWsItem.Coyote.IsBound)
                    ConnectedCount--;
                AddedCount--;
            }
        }
    }

    private void OnCoyoteConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        if (e.IsConnected)
            _queueService.TryEnqueue(() => ConnectedCount++);
        else
            _queueService.TryEnqueue(() => ConnectedCount--);
    }

    private void OnCoyoteBindingStatusChanged(object? sender, CoyoteWS.BindingStatusChangedEventArgs e)
    {
        if (e.IsBound)
            _queueService.TryEnqueue(() => ConnectedCount++);
        else
            _queueService.TryEnqueue(() => ConnectedCount--);
    }

    private void OnChannelAPlayingWaveChanged(object? sender, WaveQueue.PlayingWaveChangedEventArgs e)
    {
        _queueService.TryEnqueue(() => WaveNameA = e.WavePlayer?.Wave.Name ?? StringNone);
    }

    private void OnChannelBPlayingWaveChanged(object? sender, WaveQueue.PlayingWaveChangedEventArgs e)
    {
        _queueService.TryEnqueue(() => WaveNameB = e.WavePlayer?.Wave.Name ?? StringNone);
    }

    private void OnCoyoteManagerOutputStatusChanged(object? sender, CoyoteManager.OutputStatusChangedEventArgs e)
    {
        if (e.IsOutputting)
            _queueService.TryEnqueue(() => CurrentStrength = _gameManager.OutputStrength.ToString());
        else
            _queueService.TryEnqueue(() => CurrentStrength = StringNone);
    }

    private void OnGameManagerOutputStrengthChanged(object? sender, AutomaticStrength.OutputStrengthChangedEventArgs e)
    {
        if (_gameManager.CoyoteManager.IsOutputting)
            _queueService.TryEnqueue(() => CurrentStrength = e.Strength.ToString());
    }

    private void OnCurrentLanguageFileNameChanged(object? sender, ILocalizationService.CurrentLanguageFileNameChangedEventArgs e)
    {
        _queueService.TryEnqueue(() =>
        {
            if (_gameManager.CoyoteManager.ChannelA.PlayingWave is null)
                WaveNameA = StringNone;
            if (_gameManager.CoyoteManager.ChannelB.PlayingWave is null)
                WaveNameB = StringNone;
            if (_gameManager.CoyoteManager.IsOutputting == false)
                CurrentStrength = StringNone;
        });
    }
}
