using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.Models;
using Dianty.Services;
using DungeonToolkit.Coyote;
using System.Collections;
using System.Collections.Specialized;

namespace Dianty.ViewModels;

public partial class HomePageViewModel : ObservableObject
{
    public HomePageViewModel(
        CoyoteCollection coyoteItems, GameManager gameManager, IQueueService queueService,
        ILocalizationService localizationService)
    {
        _coyoteItems = coyoteItems;
        _gameManager = gameManager;
        _queueService = queueService;
        _localizationService = localizationService;
        _coyoteItems.CollectionChanged += OnCoyoteCollectionChanged;
        _gameManager.CoyoteManager.ChannelA.PlayingWaveChanged += OnChannelAPlayingWaveChanged;
        _gameManager.CoyoteManager.ChannelB.PlayingWaveChanged += OnChannelBPlayingWaveChanged;
        _gameManager.CoyoteManager.OutputStatusChanged += OnCoyoteManagerOutputStatusChanged;
        _gameManager.OutputStrengthChanged += OnGameManagerOutputStrengthChanged;
        _localizationService.CurrentLanguageFileNameChanged += OnCurrentLanguageFileNameChanged;

        FormattedConnectedCount = string.Format(ConnectedCountFormat, 0);
        FormattedAddedCount = string.Format(AddedCountFormat, _coyoteItems.Count);
        FormattedWaveNameA = string.Format(AChannelFormat, _gameManager.CoyoteManager.ChannelA.PlayingWave?.Wave.Name ?? NoneLabel);
        FormattedWaveNameB = string.Format(BChannelFormat, _gameManager.CoyoteManager.ChannelB.PlayingWave?.Wave.Name ?? NoneLabel);
        FormattedCurrentStrength = string.Format(CurrentStrengthFormat, _gameManager.CoyoteManager.IsOutputting
            ? _gameManager.OutputStrength : NoneLabel);
    }

    private readonly CoyoteCollection _coyoteItems;
    private readonly GameManager _gameManager;
    private readonly IQueueService _queueService;
    private readonly ILocalizationService _localizationService;

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
    public partial string FormattedWaveNameA { get; private set; }

    [ObservableProperty]
    public partial string FormattedWaveNameB { get; private set; }

    [ObservableProperty]
    public partial string FormattedCurrentStrength { get; private set; }

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
                // 此处未能取消事件订阅，如果在移除条目后继续对已移除的条目进行操作，会导致计数错误
                break;
            default:
                break;
        }
        AddedCount = _coyoteItems.Count;
    }

    private void OnAddItems(IList list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            if (item is CoyoteBleItem coyoteBleItem)
            {
                if (coyoteBleItem.Coyote.IsConnected)
                    ConnectedCount++;
                coyoteBleItem.Coyote.ConnectionStatusChanged += OnCoyoteConnectionStatusChanged;
            }
            else if (item is CoyoteWsItem coyoteWsItem)
            {
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
            }
            else if (item is CoyoteWsItem coyoteWsItem)
            {
                coyoteWsItem.Coyote.BindingStatusChanged -= OnCoyoteBindingStatusChanged;
                if (coyoteWsItem.Coyote.IsBound)
                    ConnectedCount--;
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
        _queueService.TryEnqueue(() => FormattedWaveNameA = string.Format(AChannelFormat, e.WavePlayer?.Wave.Name ?? NoneLabel));
    }

    private void OnChannelBPlayingWaveChanged(object? sender, WaveQueue.PlayingWaveChangedEventArgs e)
    {
        _queueService.TryEnqueue(() => FormattedWaveNameB = string.Format(BChannelFormat, e.WavePlayer?.Wave.Name ?? NoneLabel));
    }

    private void OnCoyoteManagerOutputStatusChanged(object? sender, CoyoteManager.OutputStatusChangedEventArgs e)
    {
        _queueService.TryEnqueue(() =>
        {
            FormattedCurrentStrength = string.Format(CurrentStrengthFormat, e.IsOutputting ? _gameManager.OutputStrength : NoneLabel);
        });
    }

    private void OnGameManagerOutputStrengthChanged(object? sender, AutomaticStrength.OutputStrengthChangedEventArgs e)
    {
        if (_gameManager.CoyoteManager.IsOutputting)
            _queueService.TryEnqueue(() => FormattedCurrentStrength = string.Format(CurrentStrengthFormat, e.Strength));
    }

    private void OnCurrentLanguageFileNameChanged(object? sender, ILocalizationService.CurrentLanguageFileNameChangedEventArgs e)
    {
        _queueService.TryEnqueue(() =>
        {
            FormattedConnectedCount = string.Format(ConnectedCountFormat, ConnectedCount);
            FormattedAddedCount = string.Format(AddedCountFormat, AddedCount);
            FormattedWaveNameA = string.Format(AChannelFormat, _gameManager.CoyoteManager.ChannelA.PlayingWave?.Wave.Name ?? NoneLabel);
            FormattedWaveNameB = string.Format(BChannelFormat, _gameManager.CoyoteManager.ChannelB.PlayingWave?.Wave.Name ?? NoneLabel);
            FormattedCurrentStrength = string.Format(CurrentStrengthFormat, _gameManager.CoyoteManager.IsOutputting
                ? _gameManager.OutputStrength : NoneLabel);
        });
    }
}
