using Dianty.EventArguments;
using Dianty.Services;
using DungeonToolkit.Coyote;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dianty.Models;

public sealed partial class GameIntegrationManager : StrengthCalculator, IDisposable, IGameIntegrationManager
{
    private readonly List<IGameIntegrationStatus> _gameIntegrationStatuses = [];
    private readonly IWaveformPusherConfig _waveformPusherConfig;
    private bool _isDisposed;

    public GameIntegrationManager(
        IEnumerable<IGameIntegrationStatus> gameIntegrationStatuses,
        IWaveformPusherConfig waveformPusherConfig)
    {
        AddGameIntegration(gameIntegrationStatuses);
        _waveformPusherConfig = waveformPusherConfig;
    }

    public int EnabledGameIntegrationCount
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                OnEnabledGameIntegrationCountChanged(value);
            }
        }
    }

    public event EventHandler<EnabledGameIntegrationCountChangedEventArgs>? EnabledGameIntegrationCountChanged;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            for (int i = 0; i < _gameIntegrationStatuses.Count; i++)
            {
                var gameIntegration = _gameIntegrationStatuses[i];
                gameIntegration.EnableStatusChanged -= OnGameIntegrationEnableStatusChanged;
                gameIntegration.OutputStrengthChanged -= OnGameIntegrationOutputStrengthChanged;
            }
            _gameIntegrationStatuses.Clear();
        }

        _isDisposed = true;
    }

    private void AddGameIntegration(IEnumerable<IGameIntegrationStatus> gameIntegrationStatuses)
    {
        foreach (var gameIntegration in gameIntegrationStatuses.ToArray())
        {
            gameIntegration.EnableStatusChanged += OnGameIntegrationEnableStatusChanged;
            gameIntegration.OutputStrengthChanged += OnGameIntegrationOutputStrengthChanged;
            _gameIntegrationStatuses.Add(gameIntegration);
        }
    }

    private void OnGameIntegrationEnableStatusChanged(object? sender, EnableStatusChangedEventArgs e)
    {
        if (e.IsEnabled)
            EnabledGameIntegrationCount++;
        else
            EnabledGameIntegrationCount--;
    }

    private void OnGameIntegrationOutputStrengthChanged(object? sender, OutputStrengthChangedEventArgs e)
    {
        ComputeOutputStrength();
    }

    protected override int GetMaxStrength()
    {
        int result = 0;
        for (int i = 0; i < _gameIntegrationStatuses.Count; i++)
        {
            var gameIntegration = _gameIntegrationStatuses[i];
            if (gameIntegration.IsEnabled)
            {
                var outputStrength = gameIntegration.OutputStrength;
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
        for (int i = 0; i < _gameIntegrationStatuses.Count; i++)
        {
            var gameIntegration = _gameIntegrationStatuses[i];
            if (gameIntegration.IsEnabled)
            {
                var outputStrength = gameIntegration.OutputStrength;
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
        _waveformPusherConfig.StrengthA = strength;
        _waveformPusherConfig.StrengthB = strength;
        base.OnOutputStrengthChanged(strength);
    }

    private void OnEnabledGameIntegrationCountChanged(int count)
    {
        var args = new EnabledGameIntegrationCountChangedEventArgs(count);
        EnabledGameIntegrationCountChanged?.Invoke(this, args);
    }
}
