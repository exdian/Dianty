using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Dianty.EventArguments;
using Dianty.Services;
using Dianty.Services.Messages;
using DungeonToolkit.Coyote;
using System;
using System.Linq;

namespace Dianty.ViewModels;

public sealed partial class GamesPageViewModel : ObservableObject, IDisposable
{
    private readonly IGameIntegrationManager _gameIntegrationManager;
    private readonly IWaveformPusherControl _waveformPusherControl;
    private readonly IWaveformPusherStatus _waveformPusherStatus;
    private readonly IQueueService _queueService;
    private readonly ILocalizationService _localizationService;
    private bool _isDisposed;

    public GamesPageViewModel(
        IGameIntegrationManager gameIntegrationManager,
        IWaveformPusherControl waveformPusherControl,
        IWaveformPusherStatus waveformPusherStatus,
        IQueueService queueService,
        ILocalizationService localizationService)
    {
        _gameIntegrationManager = gameIntegrationManager;
        _waveformPusherControl = waveformPusherControl;
        _waveformPusherStatus = waveformPusherStatus;
        _queueService = queueService;
        _localizationService = localizationService;

        OutputModeSelectionItems = OutputModeSelectionItem.GetSelectionItems(_localizationService);
        SelectedOutputMode = OutputModeSelectionItems[0];

        StrengthModeSelectionItems = StrengthModeSelectionItem.GetSelectionItems(_localizationService);
        SelectedStrengthMode = StrengthModeSelectionItems[0];

        _gameIntegrationManager.EnabledGameIntegrationCountChanged += OnGameIntegrationManagerEnabledGameIntegrationCountChanged;
        _gameIntegrationManager.OutputStrengthChanged += OnGameIntegrationManagerOutputStrengthChanged;
        _waveformPusherStatus.PushStatusChanged += OnWaveformPushStatusChanged;
        _localizationService.CurrentLanguageChanged += OnCurrentLanguageChanged;
    }

    [ObservableProperty]
    public partial OutputModeSelectionItem[] OutputModeSelectionItems { get; private set; }

    [ObservableProperty]
    public partial StrengthModeSelectionItem[] StrengthModeSelectionItems { get; private set; }

    [ObservableProperty]
    public partial int EnabledGameIntegrationCount { get; private set; }

    [ObservableProperty]
    public partial int OutputStrength { get; private set; }

    [ObservableProperty]
    public partial OutputModeSelectionItem? SelectedOutputMode { get; set; }

    [ObservableProperty]
    public partial bool IsAutomaticStartStop { get; private set; }

    [ObservableProperty]
    public partial bool IsOutputting { get; set; }

    [ObservableProperty]
    public partial StrengthModeSelectionItem? SelectedStrengthMode { get; set; }

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
            _gameIntegrationManager.EnabledGameIntegrationCountChanged -= OnGameIntegrationManagerEnabledGameIntegrationCountChanged;
            _gameIntegrationManager.OutputStrengthChanged -= OnGameIntegrationManagerOutputStrengthChanged;
            _waveformPusherStatus.PushStatusChanged -= OnWaveformPushStatusChanged;
            _localizationService.CurrentLanguageChanged -= OnCurrentLanguageChanged;
        }

        _isDisposed = true;
    }

    private void OnGameIntegrationManagerEnabledGameIntegrationCountChanged(object? sender, EnabledGameIntegrationCountChangedEventArgs e)
    {
        _queueService.TryEnqueue(() => EnabledGameIntegrationCount = e.Count);
    }

    private void OnGameIntegrationManagerOutputStrengthChanged(object? sender, OutputStrengthChangedEventArgs e)
    {
        var outputStrength = e.Strength;
        _queueService.TryEnqueue(() => OutputStrength = outputStrength);
        WeakReferenceMessenger.Default.Send(new Log($"总强度发生变化：{outputStrength}"));
    }

    private void OnWaveformPushStatusChanged(object? sender, PushStatusChangedEventArgs e)
    {
        if (IsAutomaticStartStop)
            _queueService.TryEnqueue(() => IsOutputting = e.IsPushing);
    }

    private void OnCurrentLanguageChanged(object? sender, CurrentLanguageChangedEventArgs e)
    {
        var outputModeSelectionItems = OutputModeSelectionItem.GetSelectionItems(_localizationService);
        var selectedOutputMode = SelectedOutputMode;
        if (selectedOutputMode is not null)
        {
            var isAutomaticStartStop = selectedOutputMode.IsAutomaticStartStop;
            selectedOutputMode = outputModeSelectionItems.FirstOrDefault(i => i.IsAutomaticStartStop == isAutomaticStartStop);
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

    partial void OnSelectedOutputModeChanged(OutputModeSelectionItem? oldValue, OutputModeSelectionItem? newValue)
    {
        if (newValue is null)
            return;
        if (oldValue is not null && oldValue.IsAutomaticStartStop == newValue.IsAutomaticStartStop)
            return;

        var isAutomaticStartStop = newValue.IsAutomaticStartStop;
        IsOutputting = false;
        IsAutomaticStartStop = isAutomaticStartStop;
        _waveformPusherControl.IsAutomaticStartStop = isAutomaticStartStop;
        if (!isAutomaticStartStop)
            _waveformPusherControl.Stop();
    }

    partial void OnIsOutputtingChanged(bool value)
    {
        if (IsAutomaticStartStop)
            return;
        if (value)
            _waveformPusherControl.Start();
        else
            _waveformPusherControl.Stop();
    }

    partial void OnSelectedStrengthModeChanged(StrengthModeSelectionItem? value)
    {
        if (value is null)
            return;
        _gameIntegrationManager.StrengthMode = value.Mode;
    }
}

public sealed record class OutputModeSelectionItem(bool IsAutomaticStartStop, string Description)
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

public sealed record class StrengthModeSelectionItem(StrengthCalculationMode Mode, string Description)
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
                [new StrengthModeSelectionItem(StrengthCalculationMode.Max, text.MaximumModeItemText),
                new StrengthModeSelectionItem(StrengthCalculationMode.Sum, text.SumModeItemText)];
        }
        return s_SelectionItems;
    }
}
