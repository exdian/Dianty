using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.Models;
using Dianty.Services;
using Dianty.Utils;
using System;
using static Dianty.Services.ILocalizationService;
using static GameMonitor.GtaVcMonitor;

namespace Dianty.ViewModels;

public partial class GtaVcRuleCardViewModel : ObservableObject, IDisposable
{
    public GtaVcRuleCardViewModel(GtaVcGameRule gameRule, IQueueService queueService, ILocalizationService localizationService)
    {
        _gameRule = gameRule;
        _queueService = queueService;
        _localizationService = localizationService;

        StrengthModeSelectionItems = StrengthModeSelectionItem.GetSelectionItems(_localizationService);
        StrengthMode = StrengthModeSelectionItems[0];
        UpdateStateMessage(_gameRule.Monitor.State);

        _gameRule.Monitor.StateChanged += OnMonitorStateChanged;
        _localizationService.CurrentLanguageFileNameChanged += OnCurrentLanguageFileNameChanged;
    }

    private bool _isDisposed;
    private readonly GtaVcGameRule _gameRule;
    private readonly IQueueService _queueService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    public partial StrengthModeSelectionItem[] StrengthModeSelectionItems { get; private set; }

    [ObservableProperty]
    public partial bool IsAccessing { get; private set; }

    [ObservableProperty]
    public partial string StateMessage { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    [ObservableProperty]
    public partial StrengthModeSelectionItem StrengthMode { get; set; }

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

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        if (disposing)
        {
            _gameRule.Monitor.StateChanged -= OnMonitorStateChanged;
            _localizationService.CurrentLanguageFileNameChanged -= OnCurrentLanguageFileNameChanged;
            _gameRule.Dispose();
        }
    }

    private void OnMonitorStateChanged(object? sender, StateChangedEventArgs e)
    {
        UpdateStateMessage(e.State);
    }

    private void OnCurrentLanguageFileNameChanged(object? sender, CurrentLanguageFileNameChangedEventArgs e)
    {
        var strengthModeSelectionItems = StrengthModeSelectionItem.GetSelectionItems(_localizationService);
        var gamesStrengthMode = StrengthMode.Mode;
        var index = strengthModeSelectionItems.FirstIndex(i => i.Mode == gamesStrengthMode);
        _queueService.TryEnqueue(() =>
        {
            StrengthModeSelectionItems = strengthModeSelectionItems;
            StrengthMode = StrengthModeSelectionItems[index];
        });
        UpdateStateMessage(_gameRule.Monitor.State);
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
                    StateMessage = text.GameDisabledStateText;
                    break;
                case MonitoringState.FindTargetProcess:
                    IsAccessing = true;
                    StateMessage = text.FindTargetProcessStateText;
                    break;
                case MonitoringState.AccessedTargetProcess:
                    IsAccessing = false;
                    StateMessage = text.GameEnabledStateText;
                    break;
            }
        });
    }

    partial void OnIsEnabledChanged(bool value)
    {
        _gameRule.IsEnabled = value;
    }

    partial void OnStrengthModeChanged(StrengthModeSelectionItem value)
    {
        if (value is null)
            return;
        _gameRule.StrengthMode = value.Mode;
    }

    partial void OnDamageRuleEnabledChanged(bool value)
    {
        _gameRule.DamageRuleEnabled = value;
    }

    partial void OnDamageRuleThresholdChanged(double value)
    {
        _gameRule.DamageRuleThreshold = (int)value;
    }

    partial void OnDamageRuleStrengthChanged(double value)
    {
        _gameRule.DamageRuleStrength = (int)value;
    }

    partial void OnDamageRuleDurationChanged(double value)
    {
        _gameRule.DamageRuleDuration = (int)value;
    }

    partial void OnBustedRuleEnabledChanged(bool value)
    {
        _gameRule.BustedRuleEnabled = value;
    }

    partial void OnBustedRuleStrengthChanged(double value)
    {
        _gameRule.BustedRuleStrength = (int)value;
    }

    partial void OnBustedRuleDurationChanged(double value)
    {
        _gameRule.BustedRuleDuration = (int)value;
    }

    partial void OnWastedRuleEnabledChanged(bool value)
    {
        _gameRule.WastedRuleEnabled = value;
    }

    partial void OnWastedRuleStrengthChanged(double value)
    {
        _gameRule.WastedRuleStrength = (int)value;
    }

    partial void OnWastedRuleDurationChanged(double value)
    {
        _gameRule.WastedRuleDuration = (int)value;
    }

    partial void OnMiTangRuleEnabledChanged(bool value)
    {
        _gameRule.MiTangRuleEnabled = value;
    }

    partial void OnMiTangRuleStrengthChanged(double value)
    {
        _gameRule.MiTangRuleStrength = (int)value;
    }

    partial void OnMiTangRuleDurationChanged(double value)
    {
        _gameRule.MiTangRuleDuration = (int)value;
    }

    partial void OnWantedLevelRuleEnabledChanged(bool value)
    {
        _gameRule.WantedLevelRuleEnabled = value;
    }

    partial void OnWantedLevelRuleStrengthChanged(double value)
    {
        _gameRule.WantedLevelRuleStrength = (int)value;
    }

    partial void OnWantedLevelRuleDurationChanged(double value)
    {
        _gameRule.WantedLevelRuleDuration = (int)value;
    }

    partial void OnFellOffBikeRuleEnabledChanged(bool value)
    {
        _gameRule.FellOffBikeRuleEnabled = value;
    }

    partial void OnFellOffBikeRuleStrengthChanged(double value)
    {
        _gameRule.FellOffBikeRuleStrength = (int)value;
    }

    partial void OnFellOffBikeRuleDurationChanged(double value)
    {
        _gameRule.FellOffBikeRuleDuration = (int)value;
    }
}
