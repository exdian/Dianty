using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Dianty.Models;
using Dianty.Services;
using Dianty.Utils.Messages;
using System;
using System.Linq;
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

        OutputModeSelectionItems = OutputModeSelectionItem.GetSelectionItems(_localizationService);
        SelectedOutputMode = OutputModeSelectionItems[0];

        StrengthModeSelectionItems = StrengthModeSelectionItem.GetSelectionItems(_localizationService);
        SelectedStrengthMode = StrengthModeSelectionItems[0];

        _gameManager.EnabledGameCountChanged += OnGameManagerEnabledGameCountChanged;
        _gameManager.OutputStrengthChanged += GameManager_OutputStrengthChanged;
        _gameManager.CoyoteManager.OutputStatusChanged += OnCoyoteManagerOutputStatusChanged;
        _localizationService.CurrentLanguageFileNameChanged += OnCurrentLanguageFileNameChanged;
    }

    private readonly GameManager _gameManager;
    private readonly IQueueService _queueService;
    private readonly ILocalizationService _localizationService;
    private bool _isDisposed;

    public required GtaVcRuleCardViewModel GtaVcRuleCardViewModel { get; init; }

    [ObservableProperty]
    public partial OutputModeSelectionItem[] OutputModeSelectionItems { get; private set; }

    [ObservableProperty]
    public partial StrengthModeSelectionItem[] StrengthModeSelectionItems { get; private set; }

    [ObservableProperty]
    public partial int EnabledGameCount { get; private set; }

    [ObservableProperty]
    public partial int OutputStrength { get; private set; }

    [ObservableProperty]
    public partial OutputModeSelectionItem? SelectedOutputMode { get; set; }

    [ObservableProperty]
    public partial bool IsAutoStartStop { get; private set; }

    [ObservableProperty]
    public partial bool IsOutputting { get; set; }

    [ObservableProperty]
    public partial StrengthModeSelectionItem? SelectedStrengthMode { get; set; }

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
        var outputModeSelectionItems = OutputModeSelectionItem.GetSelectionItems(_localizationService);
        var selectedOutputMode = SelectedOutputMode;
        if (selectedOutputMode is not null)
        {
            var isAutoStartStop = selectedOutputMode.IsAutoStartStop;
            selectedOutputMode = outputModeSelectionItems.FirstOrDefault(i => i.IsAutoStartStop == isAutoStartStop);
        }
        _queueService.TryEnqueue(() =>
        {
            OutputModeSelectionItems = outputModeSelectionItems;
            SelectedOutputMode = selectedOutputMode;
        });

        var strengthModeSelectionItems = StrengthModeSelectionItem.GetSelectionItems(_localizationService);
        var selectedStrengthMode = SelectedStrengthMode;
        if (selectedStrengthMode is not null)
        {
            var mode = selectedStrengthMode.Mode;
            selectedStrengthMode = strengthModeSelectionItems.FirstOrDefault(i => i.Mode == mode);
        }
        _queueService.TryEnqueue(() =>
        {
            StrengthModeSelectionItems = strengthModeSelectionItems;
            SelectedStrengthMode = selectedStrengthMode;
        });
    }

    partial void OnSelectedOutputModeChanged(OutputModeSelectionItem? value)
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

    partial void OnSelectedStrengthModeChanged(StrengthModeSelectionItem? value)
    {
        if (value is null)
            return;
        _gameManager.StrengthMode = value.Mode;
    }
}

public record class OutputModeSelectionItem(bool IsAutoStartStop, string Description)
{
    private static OutputModeSelectionItem[]? s_SelectionItems;

    public static OutputModeSelectionItem[] GetSelectionItems(ILocalizationService service)
    {
        var text = service.AppText.MainWindowText.MainViewText.GamesPageText;
        if (s_SelectionItems is null
            || s_SelectionItems.Length != 2
            || !ReferenceEquals(s_SelectionItems[0].Description, text.AutoModeItemText)
            || !ReferenceEquals(s_SelectionItems[1].Description, text.ManualModeItemText))
        {
            s_SelectionItems =
                [new OutputModeSelectionItem(true, text.AutoModeItemText),
                new OutputModeSelectionItem(false, text.ManualModeItemText)];
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
