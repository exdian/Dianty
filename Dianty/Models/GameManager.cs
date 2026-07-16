using DungeonToolkit.Coyote;
using System;
using System.Collections.Generic;

namespace Dianty.Models;

public partial class GameManager : AutomaticStrength, IDisposable
{
    public GameManager(CoyoteManager coyoteManager)
    {
        _coyoteManager = coyoteManager;
    }

    private readonly List<GameRule> _gameRules = [];
    private readonly CoyoteManager _coyoteManager;
    private bool _isDisposed;

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
            for (int i = 0; i < _gameRules.Count; i++)
            {
                var rule = _gameRules[i];
                rule.OutputStrengthChanged -= GameRule_OutputStrengthChanged;
                if (rule is IDisposable gameRule)
                    gameRule.Dispose();
            }
            _coyoteManager.Dispose();
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
