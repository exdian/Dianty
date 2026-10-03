using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.EventArguments;
using Dianty.Services;
using GameMonitor;
using System;
using System.Linq;

namespace Dianty.ViewModels;

public sealed partial class GtaVcGameIntegrationViewModel : ObservableObject, IDisposable
{
    private readonly IGtaVcGameIntegrationConfig _gameIntegration;
    private readonly IMonitorState _monitorState;
    private readonly IQueueService _queueService;
    private readonly ILocalizationService _localizationService;
    private bool _isDisposed;

    public GtaVcGameIntegrationViewModel(
        IGtaVcGameIntegrationConfig gameIntegration,
        IMonitorState monitorState,
        IQueueService queueService,
        ILocalizationService localizationService)
    {
        _gameIntegration = gameIntegration;
        _monitorState = monitorState;
        _queueService = queueService;
        _localizationService = localizationService;

        StrengthModeSelectionItems = StrengthModeSelectionItem.GetSelectionItems(_localizationService);
        SelectedStrengthMode = StrengthModeSelectionItems[0];
        UpdateStateMessage(_monitorState.State);

        _monitorState.StateChanged += OnMonitorStateChanged;
        _localizationService.CurrentLanguageChanged += OnCurrentLanguageChanged;
    }

    [ObservableProperty]
    public partial StrengthModeSelectionItem[] StrengthModeSelectionItems { get; private set; }

    [ObservableProperty]
    public partial bool IsAccessing { get; private set; }

    [ObservableProperty]
    public partial string StateMessage { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    [ObservableProperty]
    public partial StrengthModeSelectionItem? SelectedStrengthMode { get; set; }

    [ObservableProperty]
    public partial bool DamageRuleEnabled { get; set; }

    [ObservableProperty]
    public partial double DamageRuleThreshold { get; set; }

    [ObservableProperty]
    public partial double DamageRuleStrength { get; set; }

    [ObservableProperty]
    public partial double DamageRuleDuration { get; set; }

    [ObservableProperty]
    public partial bool BustedRuleEnabled { get; set; }

    [ObservableProperty]
    public partial double BustedRuleStrength { get; set; }

    [ObservableProperty]
    public partial double BustedRuleDuration { get; set; }

    [ObservableProperty]
    public partial bool WastedRuleEnabled { get; set; }

    [ObservableProperty]
    public partial double WastedRuleStrength { get; set; }

    [ObservableProperty]
    public partial double WastedRuleDuration { get; set; }

    [ObservableProperty]
    public partial bool MiTangRuleEnabled { get; set; }

    [ObservableProperty]
    public partial double MiTangRuleStrength { get; set; }

    [ObservableProperty]
    public partial double MiTangRuleDuration { get; set; }

    [ObservableProperty]
    public partial bool WantedLevelRuleEnabled { get; set; }

    [ObservableProperty]
    public partial double WantedLevelRuleStrength { get; set; }

    [ObservableProperty]
    public partial double WantedLevelRuleDuration { get; set; }

    [ObservableProperty]
    public partial bool FellOffBikeRuleEnabled { get; set; }

    [ObservableProperty]
    public partial double FellOffBikeRuleStrength { get; set; }

    [ObservableProperty]
    public partial double FellOffBikeRuleDuration { get; set; }

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
            _monitorState.StateChanged -= OnMonitorStateChanged;
            _localizationService.CurrentLanguageChanged -= OnCurrentLanguageChanged;
        }

        _isDisposed = true;
    }

    private void OnMonitorStateChanged(object? sender, MonitorStateChangedEventArgs e)
    {
        UpdateStateMessage(e.State);
    }

    private void OnCurrentLanguageChanged(object? sender, CurrentLanguageChangedEventArgs e)
    {
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
        UpdateStateMessage(_monitorState.State);
    }

    private void UpdateStateMessage(MonitoringState state)
    {
        _queueService.TryEnqueue(() =>
        {
            var text = _localizationService.AppText.MainWindowText.MainViewText.GamesPageText;
            switch (state)
            {
                default:
                case MonitoringState.Stopped:
                    IsAccessing = false;
                    StateMessage = text.GameIntegrationDisabledStateText;
                    break;
                case MonitoringState.FindTargetProcess:
                    IsAccessing = true;
                    StateMessage = text.FindTargetProcessStateText;
                    break;
                case MonitoringState.AccessedTargetProcess:
                    IsAccessing = false;
                    StateMessage = text.GameIntegrationEnabledStateText;
                    break;
            }
        });
    }

    partial void OnIsEnabledChanged(bool value)
    {
        _gameIntegration.IsEnabled = value;
    }

    partial void OnSelectedStrengthModeChanged(StrengthModeSelectionItem? value)
    {
        if (value is null)
            return;
        _gameIntegration.StrengthMode = value.Mode;
    }

    partial void OnDamageRuleEnabledChanged(bool value)
    {
        _gameIntegration.DamageRuleEnabled = value;
    }

    partial void OnDamageRuleThresholdChanged(double value)
    {
        _gameIntegration.DamageRuleThreshold = (int)value;
    }

    partial void OnDamageRuleStrengthChanged(double value)
    {
        _gameIntegration.DamageRuleStrength = (int)value;
    }

    partial void OnDamageRuleDurationChanged(double value)
    {
        _gameIntegration.DamageRuleDuration = (int)value;
    }

    partial void OnBustedRuleEnabledChanged(bool value)
    {
        _gameIntegration.BustedRuleEnabled = value;
    }

    partial void OnBustedRuleStrengthChanged(double value)
    {
        _gameIntegration.BustedRuleStrength = (int)value;
    }

    partial void OnBustedRuleDurationChanged(double value)
    {
        _gameIntegration.BustedRuleDuration = (int)value;
    }

    partial void OnWastedRuleEnabledChanged(bool value)
    {
        _gameIntegration.WastedRuleEnabled = value;
    }

    partial void OnWastedRuleStrengthChanged(double value)
    {
        _gameIntegration.WastedRuleStrength = (int)value;
    }

    partial void OnWastedRuleDurationChanged(double value)
    {
        _gameIntegration.WastedRuleDuration = (int)value;
    }

    partial void OnMiTangRuleEnabledChanged(bool value)
    {
        _gameIntegration.MiTangRuleEnabled = value;
    }

    partial void OnMiTangRuleStrengthChanged(double value)
    {
        _gameIntegration.MiTangRuleStrength = (int)value;
    }

    partial void OnMiTangRuleDurationChanged(double value)
    {
        _gameIntegration.MiTangRuleDuration = (int)value;
    }

    partial void OnWantedLevelRuleEnabledChanged(bool value)
    {
        _gameIntegration.WantedLevelRuleEnabled = value;
    }

    partial void OnWantedLevelRuleStrengthChanged(double value)
    {
        _gameIntegration.WantedLevelRuleStrength = (int)value;
    }

    partial void OnWantedLevelRuleDurationChanged(double value)
    {
        _gameIntegration.WantedLevelRuleDuration = (int)value;
    }

    partial void OnFellOffBikeRuleEnabledChanged(bool value)
    {
        _gameIntegration.FellOffBikeRuleEnabled = value;
    }

    partial void OnFellOffBikeRuleStrengthChanged(double value)
    {
        _gameIntegration.FellOffBikeRuleStrength = (int)value;
    }

    partial void OnFellOffBikeRuleDurationChanged(double value)
    {
        _gameIntegration.FellOffBikeRuleDuration = (int)value;
    }
}
