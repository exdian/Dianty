using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.Models;
using System;

namespace Dianty.ViewModels;

public partial class GtaVcRuleCardViewModel : ObservableObject, IDisposable
{
    public GtaVcRuleCardViewModel(GtaVcGameRule gameRule)
    {
        _gameRule = gameRule;

        StrengthModeSelectionItems = StrengthModeSelectionItem.StrengthModeSelectionItems;
        StrengthMode = StrengthModeSelectionItems[0];
    }

    private bool _isDisposed;
    private readonly GtaVcGameRule _gameRule;

    public StrengthModeSelectionItem[] StrengthModeSelectionItems { get; }

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
            _gameRule.Dispose();
        }
    }

    partial void OnIsEnabledChanged(bool value)
    {
        _gameRule.IsEnabled = value;
    }

    partial void OnStrengthModeChanged(StrengthModeSelectionItem value)
    {
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
