using GameMonitor;
using System;

namespace Dianty.Models;

public class GameManager : AutomaticStrength
{
    public GameManager(IMemoryService memoryService)
    {
        GtaVcGameRule = new GtaVcGameRule(memoryService);
        GtaVcGameRule.OutputStrengthChanged += GtaVcGameRule_OutputStrengthChanged;

        _gameRules = [GtaVcGameRule];
    }

    ~GameManager()
    {
        GtaVcGameRule.OutputStrengthChanged -= GtaVcGameRule_OutputStrengthChanged;
    }

    private readonly GameRule[] _gameRules;

    public GtaVcGameRule GtaVcGameRule { get; }

    private void GtaVcGameRule_OutputStrengthChanged(object? sender, EventArgs e)
    {
        OnOutputStrengthChanged();
    }

    protected override int GetMaxStrength()
    {
        int result = 0;
        for (int i = 0; i < _gameRules.Length; i++)
        {
            var gameRule = _gameRules[i];
            if (gameRule.IsEnable)
            {
                var outputStrength = gameRule.ComputeOutputStrength();
                if (outputStrength > result)
                {
                    result = outputStrength;
                }
            }
        }
        return result;
    }

    protected override int SumStrength()
    {
        int result = 0;
        for (int i = 0; i < _gameRules.Length; i++)
        {
            var gameRule = _gameRules[i];
            if (gameRule.IsEnable)
            {
                var outputStrength = gameRule.ComputeOutputStrength();
                if (outputStrength > result)
                {
                    result = result + outputStrength;
                }
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
