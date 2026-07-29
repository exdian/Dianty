using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Dianty.Models;
using Dianty.Services;
using Dianty.Utils.Messages;
using System;
using static Dianty.Models.AutomaticStrength;
using static DungeonToolkit.Coyote.CoyoteManager;

namespace Dianty.ViewModels;

public partial class GamesPageViewModel : ObservableObject, IDisposable
{
    public GamesPageViewModel(GameManager gameManager, IQueueService queueService)
    {
        _gameManager = gameManager;
        _queueService = queueService;

        _gameManager.EnabledGameCountChanged += OnGameManagerEnabledGameCountChanged;
        _gameManager.OutputStrengthChanged += GameManager_OutputStrengthChanged;
        _gameManager.CoyoteManager.OutputStatusChanged += OnCoyoteManagerOutputStatusChanged;

        AutoStartStopModeSelectionItems = AutoStartStopModeSelectionItem.AutoStartStopModeSelectionItems;
        IsAutoStartStopMode = AutoStartStopModeSelectionItems[0];

        StrengthModeSelectionItems = StrengthModeSelectionItem.StrengthModeSelectionItems;
        GamesStrengthMode = StrengthModeSelectionItems[0];
    }

    private readonly GameManager _gameManager;
    private readonly IQueueService _queueService;
    private bool _isDisposed;

    public AutoStartStopModeSelectionItem[] AutoStartStopModeSelectionItems { get; }
    public StrengthModeSelectionItem[] StrengthModeSelectionItems { get; }
    public required GtaVcRuleCardViewModel GtaVcRuleCardViewModel { get; init; }

    // 概况
    [ObservableProperty]
    public partial int EnabledGameCount { get; private set; }

    [ObservableProperty]
    public partial int OutputStrength { get; private set; }

    [ObservableProperty]
    public partial AutoStartStopModeSelectionItem IsAutoStartStopMode { get; set; }

    [ObservableProperty]
    public partial bool IsAutoStartStop { get; private set; }

    [ObservableProperty]
    public partial bool IsOutputting { get; set; }

    [ObservableProperty]
    public partial StrengthModeSelectionItem GamesStrengthMode { get; set; }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        if (disposing)
        {
            _gameManager.EnabledGameCountChanged -= OnGameManagerEnabledGameCountChanged;
            _gameManager.OutputStrengthChanged -= GameManager_OutputStrengthChanged;
            _gameManager.CoyoteManager.OutputStatusChanged -= OnCoyoteManagerOutputStatusChanged;
            _gameManager.Dispose();
        }
    }

    private void OnGameManagerEnabledGameCountChanged(object? sender, GameManager.EnabledGameCountChangedEventArgs e)
    {
        _queueService.TryEnqueue(() => EnabledGameCount = e.Count);
    }

    private void GameManager_OutputStrengthChanged(object? sender, EventArgs e)
    {
        var outputStrength = _gameManager.OutputStrength;
        _queueService.TryEnqueue(() => OutputStrength = outputStrength);
        WeakReferenceMessenger.Default.Send(new Log($"总强度发生变化：{outputStrength}"));
    }

    private void OnCoyoteManagerOutputStatusChanged(object? sender, OutputStatusChangedEventArgs e)
    {
        if (IsAutoStartStop)
            _queueService.TryEnqueue(() => IsOutputting = e.IsOutputting);
    }

    partial void OnIsAutoStartStopModeChanged(AutoStartStopModeSelectionItem value)
    {
        var isAutoStartStop = value.IsAutoStartStop;
        IsAutoStartStop = isAutoStartStop;
        _gameManager.CoyoteManager.IsAutoStartStop = isAutoStartStop;
        if (!isAutoStartStop)
            _gameManager.CoyoteManager.StopOutput();
    }

    partial void OnIsOutputtingChanged(bool value)
    {
        if (IsAutoStartStop)
            return;
        if (value)
            _gameManager.CoyoteManager.StartOutput();
        else
            _gameManager.CoyoteManager.StopOutput();
    }

    partial void OnGamesStrengthModeChanged(StrengthModeSelectionItem value)
    {
        _gameManager.StrengthMode = value.Mode;
    }
}

public readonly record struct AutoStartStopModeSelectionItem(bool IsAutoStartStop, string Description)
{
    public static AutoStartStopModeSelectionItem[] AutoStartStopModeSelectionItems
    {
        get
        {
            field ??=
                [new AutoStartStopModeSelectionItem(true, "自动"),
                new AutoStartStopModeSelectionItem(false, "手动")];
            return field;
        }
        set;
    }
}

public readonly record struct StrengthModeSelectionItem(Mode Mode, string Description)
{
    public static StrengthModeSelectionItem[] StrengthModeSelectionItems
    {
        get
        {
            field ??=
                [new StrengthModeSelectionItem(Mode.Max, "取最大值"),
                new StrengthModeSelectionItem(Mode.Sum, "叠加强度")];
            return field;
        }
        set;
    }
}