using Dianty.EventArguments;
using System;

namespace Dianty.Services;

public interface IGameIntegrationConfig : IStrengthCalculatorConfig
{
    bool IsEnabled { get; set; }
}

public interface IGameIntegrationStatus : IStrengthCalculatorStatus
{
    bool IsEnabled { get; }

    event EventHandler<EnableStatusChangedEventArgs>? EnableStatusChanged;
}
