using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Dianty.Models;
using Dianty.Services;
using Dianty.Utils;
using Dianty.Utils.Messages;
using System;
using static Dianty.Models.AutomaticStrength;
using static Dianty.Services.ILocalizationService;
using static DungeonToolkit.Coyote.CoyoteManager;

namespace Dianty.ViewModels;

public partial class GamesPageViewModel : ObservableObject, IDisposable
{
    public GamesPageViewModel(GameManager gameManager, IQueueService queueService, ILocalizationService localizationService)
    {
        _gameManager = gameManager;
        _queueService = queueService;
        _localizationService = localizationService;

        _gameManager.EnabledGameCountChanged += OnGameManagerEnabledGameCountChanged;
        _gameManager.OutputStrengthChanged += GameManager_OutputStrengthChanged;
        _gameManager.CoyoteManager.OutputStatusChanged += OnCoyoteManagerOutputStatusChanged;
        _localizationService.CurrentLanguageFileNameChanged += OnCurrentLanguageFileNameChanged;

        AutoStartStopModeSelectionItems = AutoStartStopModeSelectionItem.GetSelectionItems(_localizationService);
        IsAutoStartStopMode = AutoStartStopModeSelectionItems[0];

        StrengthModeSelectionItems = StrengthModeSelectionItem.GetSelectionItems(_localizationService);
        GamesStrengthMode = StrengthModeSelectionItems[0];
    }

    private readonly GameManager _gameManager;
    private readonly IQueueService _queueService;
    private readonly ILocalizationService _localizationService;
    private bool _isDisposed;

    public required GtaVcRuleCardViewModel GtaVcRuleCardViewModel { get; init; }

    [ObservableProperty]
    public partial AutoStartStopModeSelectionItem[] AutoStartStopModeSelectionItems { get; private set; }

    [ObservableProperty]
    public partial StrengthModeSelectionItem[] StrengthModeSelectionItems { get; private set; }

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
            _localizationService.CurrentLanguageFileNameChanged -= OnCurrentLanguageFileNameChanged;
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

    private void OnCurrentLanguageFileNameChanged(object? sender, CurrentLanguageFileNameChangedEventArgs e)
    {
        var autoStartStopModeSelectionItems = AutoStartStopModeSelectionItem.GetSelectionItems(_localizationService);
        var isAutoStartStop = IsAutoStartStopMode.IsAutoStartStop;
        var index = autoStartStopModeSelectionItems.FirstIndex(i => i.IsAutoStartStop == isAutoStartStop);
        _queueService.TryEnqueue(() =>
        {
            AutoStartStopModeSelectionItems = autoStartStopModeSelectionItems;
            IsAutoStartStopMode = AutoStartStopModeSelectionItems[index];
        });

        var strengthModeSelectionItems = StrengthModeSelectionItem.GetSelectionItems(_localizationService);
        var gamesStrengthMode = GamesStrengthMode.Mode;
        index = strengthModeSelectionItems.FirstIndex(i => i.Mode == gamesStrengthMode);
        _queueService.TryEnqueue(() =>
        {
            StrengthModeSelectionItems = strengthModeSelectionItems;
            GamesStrengthMode = StrengthModeSelectionItems[index];
        });
    }

    partial void OnIsAutoStartStopModeChanged(AutoStartStopModeSelectionItem value)
    {
        if (value is null)
            return;
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
        if (value is null)
            return;
        _gameManager.StrengthMode = value.Mode;
    }
}

public record class AutoStartStopModeSelectionItem(bool IsAutoStartStop, string Description)
{
    private static AutoStartStopModeSelectionItem[]? s_SelectionItems;

    public static AutoStartStopModeSelectionItem[] GetSelectionItems(ILocalizationService service)
    {
        var text = service.AppText.MainWindowText.MainViewText.GamesPageText;
        if (s_SelectionItems is null
            || s_SelectionItems.Length != 2
            || !ReferenceEquals(s_SelectionItems[0].Description, text.AutoModeItemText)
            || !ReferenceEquals(s_SelectionItems[1].Description, text.ManualModeItemText))
        {
            s_SelectionItems =
                [new AutoStartStopModeSelectionItem(true, text.AutoModeItemText),
                new AutoStartStopModeSelectionItem(false, text.ManualModeItemText)];
        }
        return s_SelectionItems;
    }
}

public record class StrengthModeSelectionItem(Mode Mode, string Description)
{
    private static StrengthModeSelectionItem[]? s_SelectionItems;

    public static StrengthModeSelectionItem[] GetSelectionItems(ILocalizationService service)
    {
        var text = service.AppText.MainWindowText.MainViewText.GamesPageText;
        if (s_SelectionItems is null
            || s_SelectionItems.Length != 2
            || !ReferenceEquals(s_SelectionItems[0].Description, text.MaximumModeItemText)
            || !ReferenceEquals(s_SelectionItems[1].Description, text.SumModeItemText))
        {
            s_SelectionItems =
                [new StrengthModeSelectionItem(Mode.Max, text.MaximumModeItemText),
                new StrengthModeSelectionItem(Mode.Sum, text.SumModeItemText)];
        }
        return s_SelectionItems;
    }
}
