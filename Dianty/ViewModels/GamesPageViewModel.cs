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

        _gameManager.OutputStrengthChanged += GameManager_OutputStrengthChanged;
        _gameManager.CoyoteManager.OutputStatusChanged += OnCoyoteManagerOutputStatusChanged;

        AutoStartStopModeSelectionItems =
            [new AutoStartStopModeSelectionItem(true, "自动"),
            new AutoStartStopModeSelectionItem(false, "手动")];
        IsAutoStartStopMode = AutoStartStopModeSelectionItems[0];

        StrengthModeSelectionItems =
            [new StrengthModeSelectionItem(Mode.Max, "取最大值"),
            new StrengthModeSelectionItem(Mode.Sum, "叠加强度")];
        GamesStrengthMode = StrengthModeSelectionItems[0];
        GtaVcStrengthMode = StrengthModeSelectionItems[0];
    }

    private readonly GameManager _gameManager;
    private readonly IQueueService _queueService;
    private bool _isDisposed;

    public AutoStartStopModeSelectionItem[] AutoStartStopModeSelectionItems { get; }
    public StrengthModeSelectionItem[] StrengthModeSelectionItems { get; }

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

    // 规则
    // 罪恶都市
    [ObservableProperty]
    public partial bool GtaVcEnabled { get; set; }

    [ObservableProperty]
    public partial StrengthModeSelectionItem GtaVcStrengthMode { get; set; }

    [ObservableProperty]
    public partial bool GtaVcDamageRuleEnabled { get; set; }

    [ObservableProperty]
    public partial double GtaVcDamageRuleThreshold { get; set; }

    [ObservableProperty]
    public partial double GtaVcDamageRuleStrength { get; set; }

    [ObservableProperty]
    public partial double GtaVcDamageRuleDuration { get; set; }

    [ObservableProperty]
    public partial bool GtaVcBustedRuleEnabled { get; set; }

    [ObservableProperty]
    public partial double GtaVcBustedRuleStrength { get; set; }

    [ObservableProperty]
    public partial double GtaVcBustedRuleDuration { get; set; }

    [ObservableProperty]
    public partial bool GtaVcWastedRuleEnabled { get; set; }

    [ObservableProperty]
    public partial double GtaVcWastedRuleStrength { get; set; }

    [ObservableProperty]
    public partial double GtaVcWastedRuleDuration { get; set; }

    [ObservableProperty]
    public partial bool GtaVcMiTangRuleEnabled { get; set; }

    [ObservableProperty]
    public partial double GtaVcMiTangRuleStrength { get; set; }

    [ObservableProperty]
    public partial double GtaVcMiTangRuleDuration { get; set; }

    [ObservableProperty]
    public partial bool GtaVcWantedLevelRuleEnabled { get; set; }

    [ObservableProperty]
    public partial double GtaVcWantedLevelRuleStrength { get; set; }

    [ObservableProperty]
    public partial double GtaVcWantedLevelRuleDuration { get; set; }

    [ObservableProperty]
    public partial bool GtaVcFellOffBikeRuleEnabled { get; set; }

    [ObservableProperty]
    public partial double GtaVcFellOffBikeRuleStrength { get; set; }

    [ObservableProperty]
    public partial double GtaVcFellOffBikeRuleDuration { get; set; }

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
            _gameManager.OutputStrengthChanged -= GameManager_OutputStrengthChanged;
            _gameManager.CoyoteManager.OutputStatusChanged -= OnCoyoteManagerOutputStatusChanged;
            _gameManager.Dispose();
        }
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

    partial void OnGtaVcEnabledChanged(bool value)
    {
        if (value)
            EnabledGameCount++;
        else
            EnabledGameCount--;
        _gameManager.GtaVcGameRule.IsEnabled = value;
    }

    partial void OnGtaVcStrengthModeChanged(StrengthModeSelectionItem value)
    {
        _gameManager.GtaVcGameRule.StrengthMode = value.Mode;
    }

    partial void OnGtaVcDamageRuleEnabledChanged(bool value)
    {
        _gameManager.GtaVcGameRule.DamageRuleEnabled = value;
    }

    partial void OnGtaVcDamageRuleThresholdChanged(double value)
    {
        _gameManager.GtaVcGameRule.DamageRuleThreshold = (int)value;
    }

    partial void OnGtaVcDamageRuleStrengthChanged(double value)
    {
        _gameManager.GtaVcGameRule.DamageRuleStrength = (int)value;
    }

    partial void OnGtaVcDamageRuleDurationChanged(double value)
    {
        _gameManager.GtaVcGameRule.DamageRuleDuration = (int)value;
    }

    partial void OnGtaVcBustedRuleEnabledChanged(bool value)
    {
        _gameManager.GtaVcGameRule.BustedRuleEnabled = value;
    }

    partial void OnGtaVcBustedRuleStrengthChanged(double value)
    {
        _gameManager.GtaVcGameRule.BustedRuleStrength = (int)value;
    }

    partial void OnGtaVcBustedRuleDurationChanged(double value)
    {
        _gameManager.GtaVcGameRule.BustedRuleDuration = (int)value;
    }

    partial void OnGtaVcWastedRuleEnabledChanged(bool value)
    {
        _gameManager.GtaVcGameRule.WastedRuleEnabled = value;
    }

    partial void OnGtaVcWastedRuleStrengthChanged(double value)
    {
        _gameManager.GtaVcGameRule.WastedRuleStrength = (int)value;
    }

    partial void OnGtaVcWastedRuleDurationChanged(double value)
    {
        _gameManager.GtaVcGameRule.WastedRuleDuration = (int)value;
    }

    partial void OnGtaVcMiTangRuleEnabledChanged(bool value)
    {
        _gameManager.GtaVcGameRule.MiTangRuleEnabled = value;
    }

    partial void OnGtaVcMiTangRuleStrengthChanged(double value)
    {
        _gameManager.GtaVcGameRule.MiTangRuleStrength = (int)value;
    }

    partial void OnGtaVcMiTangRuleDurationChanged(double value)
    {
        _gameManager.GtaVcGameRule.MiTangRuleDuration = (int)value;
    }

    partial void OnGtaVcWantedLevelRuleEnabledChanged(bool value)
    {
        _gameManager.GtaVcGameRule.WantedLevelRuleEnabled = value;
    }

    partial void OnGtaVcWantedLevelRuleStrengthChanged(double value)
    {
        _gameManager.GtaVcGameRule.WantedLevelRuleStrength = (int)value;
    }

    partial void OnGtaVcWantedLevelRuleDurationChanged(double value)
    {
        _gameManager.GtaVcGameRule.WantedLevelRuleDuration = (int)value;
    }

    partial void OnGtaVcFellOffBikeRuleEnabledChanged(bool value)
    {
        _gameManager.GtaVcGameRule.FellOffBikeRuleEnabled = value;
    }

    partial void OnGtaVcFellOffBikeRuleStrengthChanged(double value)
    {
        _gameManager.GtaVcGameRule.FellOffBikeRuleStrength = (int)value;
    }

    partial void OnGtaVcFellOffBikeRuleDurationChanged(double value)
    {
        _gameManager.GtaVcGameRule.FellOffBikeRuleDuration = (int)value;
    }
}

public readonly record struct AutoStartStopModeSelectionItem(bool IsAutoStartStop, string Description);

public readonly record struct StrengthModeSelectionItem(Mode Mode, string Description);
