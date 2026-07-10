using DungeonToolkit.Coyote;
using System;
using System.Collections.Generic;

namespace Dianty.Models;

public class GameManager : AutomaticStrength
{
    public GameManager(CoyoteManager coyoteManager)
    {
        _coyoteManager = coyoteManager;
    }

    ~GameManager()
    {
        for (int i = 0; i < _gameRules.Count; i++)
        {
            _gameRules[i].OutputStrengthChanged -= GameRule_OutputStrengthChanged;
        }
    }

    private readonly List<GameRule> _gameRules = [];
    private readonly CoyoteManager _coyoteManager;

    public required GtaVcGameRule GtaVcGameRule
    {
        get;
        init
        {
            field = value;
            field.OutputStrengthChanged += GameRule_OutputStrengthChanged;
            _gameRules.Add(field);
        }
    }

    private void GameRule_OutputStrengthChanged(object? sender, EventArgs e)
    {
        ComputeOutputStrength();
    }

    protected override int GetMaxStrength()
    {
        int result = 0;
        for (int i = 0; i < _gameRules.Count; i++)
        {
            var gameRule = _gameRules[i];
            if (gameRule.IsEnabled)
            {
                var outputStrength = gameRule.OutputStrength;
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
        for (int i = 0; i < _gameRules.Count; i++)
        {
            var gameRule = _gameRules[i];
            if (gameRule.IsEnabled)
            {
                var outputStrength = gameRule.OutputStrength;
                if (outputStrength > result)
                {
                    result = result + outputStrength;
                }
            }
        }
        return result;
    }

    protected override void OnOutputStrengthChanged(int strength)
    {
        _coyoteManager.StrengthA = strength;
        _coyoteManager.StrengthB = strength;
        base.OnOutputStrengthChanged(strength);
    }

    public abstract class GameRule : AutomaticStrength
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public virtual bool IsEnabled { get; set; }
    }
}
