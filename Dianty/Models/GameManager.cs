using GameMonitor;

namespace Dianty.Models;

public class GameManager : AutomaticStrength
{
    public GameManager(IMemoryService memoryService)
    {
        GtaVcGameRule = new GtaVcGameRule(memoryService);
        GtaVcGameRule.OutputStrengthChanged += GtaVcGameRule_OutputStrengthChanged;

        _gameRules = [GtaVcGameRule];
    }

    private readonly GameRule[] _gameRules;

    public GtaVcGameRule GtaVcGameRule { get; }

    private void GtaVcGameRule_OutputStrengthChanged(object? sender, OutputStrengthChangedEventArgs e)
    {
        ComputeOutputStrength();
    }

    protected override int GetMaxStrength()
    {
        int result = 0;
        for (int i = 0; i < _gameRules.Length; i++)
        {
            var gameRulerule = _gameRules[i];
            if (gameRulerule.IsEnable && gameRulerule.OutputStrength > result)
            {
                result = gameRulerule.OutputStrength;
            }
        }
        return result;
    }

    protected override int SumStrength()
    {
        int result = 0;
        for (int i = 0; i < _gameRules.Length; i++)
        {
            var gameRulerule = _gameRules[i];
            if (gameRulerule.IsEnable && gameRulerule.OutputStrength > 0)
            {
                result = result + gameRulerule.OutputStrength;
            }
        }
        return result;
    }

    public abstract class GameRule : AutomaticStrength
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public virtual bool IsEnable { get; set; }
    }
}
