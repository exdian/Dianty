using DungeonToolkit.Coyote;
using System;
using System.Collections.Generic;

namespace Dianty.Models;

public partial class GameManager(CoyoteManager coyoteManager) : AutomaticStrength, IDisposable
{
    private readonly List<GameRule> _gameRules = [];
    private bool _isDisposed;

    public CoyoteManager CoyoteManager { get; } = coyoteManager;

    public required GtaVcGameRule GtaVcGameRule
    {
        get;
        init
        {
            field = value;
            field.IsEnabledChanged += OnGameRuleIsEnabledChanged;
            field.OutputStrengthChanged += OnGameRuleOutputStrengthChanged;
            _gameRules.Add(field);
        }
    }

    public int EnabledGameCount
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                OnEnabledGameCountChanged(value);
            }
        }
    }

    public event EventHandler<EnabledGameCountChangedEventArgs>? EnabledGameCountChanged;

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
                rule.IsEnabledChanged -= OnGameRuleIsEnabledChanged;
                rule.OutputStrengthChanged -= OnGameRuleOutputStrengthChanged;
                if (rule is IDisposable gameRule)
                    gameRule.Dispose();
            }
            CoyoteManager.Dispose();
        }
    }

    private void OnGameRuleIsEnabledChanged(object? sender, GameRule.IsEnabledChangedEventArgs e)
    {
        if (e.IsEnabled)
            EnabledGameCount++;
        else
            EnabledGameCount--;
    }

    private void OnGameRuleOutputStrengthChanged(object? sender, EventArgs e)
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
        CoyoteManager.StrengthA = strength;
        CoyoteManager.StrengthB = strength;
        base.OnOutputStrengthChanged(strength);
    }

    private void OnEnabledGameCountChanged(int count)
    {
        var args = new EnabledGameCountChangedEventArgs(count);
        EnabledGameCountChanged?.Invoke(this, args);
    }

    public abstract class GameRule : AutomaticStrength
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsEnabled
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnIsEnabledChanged(value);
                    ComputeOutputStrength();
                }
            }
        }

        public event EventHandler<IsEnabledChangedEventArgs>? IsEnabledChanged;

        protected virtual void OnIsEnabledChanged(bool isEnabled)
        {
            var args = new IsEnabledChangedEventArgs(isEnabled);
            IsEnabledChanged?.Invoke(this, args);
        }

        public class IsEnabledChangedEventArgs(bool isEnabled) : EventArgs
        {
            public bool IsEnabled { get; } = isEnabled;
        }
    }

    public class EnabledGameCountChangedEventArgs(int count) : EventArgs
    {
        public int Count { get; } = count;
    }
}
