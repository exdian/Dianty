using Dianty.EventArguments;
using Dianty.Services;
using System;

namespace Dianty.Models;

public abstract class GameIntegration : StrengthCalculator, IGameIntegrationConfig, IGameIntegrationStatus
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
                OnEnableStatusChanged(value);
                ComputeOutputStrength();
            }
        }
    }

    public event EventHandler<EnableStatusChangedEventArgs>? EnableStatusChanged;

    protected virtual void OnEnableStatusChanged(bool isEnabled)
    {
        var args = new EnableStatusChangedEventArgs(isEnabled);
        EnableStatusChanged?.Invoke(this, args);
    }
}
