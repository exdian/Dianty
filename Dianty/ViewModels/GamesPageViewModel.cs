using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Dianty.Models;
using Dianty.Services;
using Dianty.Utils;
using GameMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using static Dianty.Models.AutomaticStrength;

namespace Dianty.ViewModels;

public partial class GamesPageViewModel : ObservableObject
{
    public GamesPageViewModel(IMemoryService memoryService, IQueueService queueService)
    {
        _gameManager = new GameManager(memoryService);
        _gameManager.OutputStrengthChanged += GameManager_OutputStrengthChanged;
        _queueService = queueService;

        StrengthModes = new Dictionary<Mode, string>
        {
            [Mode.Max] = "取最大值",
            [Mode.Sum] = "叠加强度"
        }.ToArray();

        GamesStrengthMode = StrengthModes.First();
        GtaVcStrengthMode = StrengthModes.First();
    }

    ~GamesPageViewModel()
    {
        _gameManager.OutputStrengthChanged -= GameManager_OutputStrengthChanged;
    }

    private readonly GameManager _gameManager;
    private readonly IQueueService _queueService;

    public KeyValuePair<Mode, string>[] StrengthModes { get; }

    // 概况
    [ObservableProperty]
    public partial int EnableGameCount { get; private set; }

    [ObservableProperty]
    public partial int OutputStrength { get; private set; }

    [ObservableProperty]
    public partial KeyValuePair<Mode, string> GamesStrengthMode { get; set; }

    // 规则
    // 罪恶都市
    [ObservableProperty]
    public partial bool GtaVcEnable { get; set; }

    [ObservableProperty]
    public partial KeyValuePair<Mode, string> GtaVcStrengthMode { get; set; }

    [ObservableProperty]
    public partial bool GtaVcDamageRuleEnable { get; set; }

    [ObservableProperty]
    public partial double GtaVcDamageRuleThreshold { get; set; }

    [ObservableProperty]
    public partial double GtaVcDamageRuleStrength { get; set; }

    [ObservableProperty]
    public partial double GtaVcDamageRuleDuration { get; set; }

    [ObservableProperty]
    public partial bool GtaVcBustedRuleEnable { get; set; }

    [ObservableProperty]
    public partial double GtaVcBustedRuleStrength { get; set; }

    [ObservableProperty]
    public partial double GtaVcBustedRuleDuration { get; set; }

    private void GameManager_OutputStrengthChanged(object? sender, EventArgs e)
    {
        var outputStrength = _gameManager.OutputStrength;
        _queueService.TryEnqueue(() =>
        {
            OutputStrength = outputStrength;
        });
        WeakReferenceMessenger.Default.Send(new Log($"总强度发生变化：{outputStrength}"));
    }

    partial void OnGamesStrengthModeChanged(KeyValuePair<Mode, string> value)
    {
        _gameManager.StrengthMode = value.Key;
    }

    partial void OnGtaVcEnableChanged(bool value)
    {
        if (value)
            EnableGameCount++;
        else
            EnableGameCount--;
        _gameManager.GtaVcGameRule.IsEnable = value;
    }

    partial void OnGtaVcStrengthModeChanged(KeyValuePair<Mode, string> value)
    {
        _gameManager.GtaVcGameRule.StrengthMode = value.Key;
    }

    partial void OnGtaVcDamageRuleEnableChanged(bool value)
    {
        _gameManager.GtaVcGameRule.DamageRuleEnable = value;
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

    partial void OnGtaVcBustedRuleEnableChanged(bool value)
    {
        _gameManager.GtaVcGameRule.BustedRuleEnable = value;
    }

    partial void OnGtaVcBustedRuleStrengthChanged(double value)
    {
        _gameManager.GtaVcGameRule.BustedRuleStrength = (int)value;
    }

    partial void OnGtaVcBustedRuleDurationChanged(double value)
    {
        _gameManager.GtaVcGameRule.BustedRuleDuration = (int)value;
    }
}
