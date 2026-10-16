namespace Dianty.Services;

public interface IGtaVcGameIntegrationConfig : IGameIntegrationConfig
{
    bool DamageRuleEnabled { get; set; }
    int DamageRuleThreshold { get; set; }
    int DamageRuleStrength { get; set; }
    int DamageRuleDuration { get; set; }
    bool BustedRuleEnabled { get; set; }
    int BustedRuleStrength { get; set; }
    int BustedRuleDuration { get; set; }
    bool WastedRuleEnabled { get; set; }
    int WastedRuleStrength { get; set; }
    int WastedRuleDuration { get; set; }
    bool MiTangRuleEnabled { get; set; }
    int MiTangRuleStrength { get; set; }
    int MiTangRuleDuration { get; set; }
    bool WantedLevelRuleEnabled { get; set; }
    int WantedLevelRuleStrength { get; set; }
    int WantedLevelRuleDuration { get; set; }
    bool FellOffBikeRuleEnabled { get; set; }
    int FellOffBikeRuleStrength { get; set; }
    int FellOffBikeRuleDuration { get; set; }
}
