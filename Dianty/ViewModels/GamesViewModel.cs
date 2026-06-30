using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.Models;
using GameMonitor;
using System.Collections.Generic;
using System.Linq;
using static Dianty.Models.AutomaticStrength;

namespace Dianty.ViewModels;

public partial class GamesViewModel : ObservableObject
{
    public GamesViewModel(IMemoryService memoryService)
    {
        _gameManager = new GameManager(memoryService);

        StrengthModes = new Dictionary<Mode, string>
        {
            [Mode.Max] = "取最大值",
            [Mode.Sum] = "叠加强度"
        }.ToArray();

        GamesStrengthMode = StrengthModes.First();
        GtaVcStrengthMode = StrengthModes.First();
    }

    private readonly GameManager _gameManager;

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

    partial void OnGamesStrengthModeChanged(KeyValuePair<Mode, string> value)
    {
        _gameManager.StrengthMode = value.Key;
    }

    partial void OnGtaVcEnableChanged(bool value)
    {
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
}
