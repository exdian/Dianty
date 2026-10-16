using Dianty.EventArguments;
using System;

namespace Dianty.Services;

public interface IGameIntegrationManager : IStrengthCalculatorStatus, IStrengthCalculatorConfig
{
    event EventHandler<EnabledGameIntegrationCountChangedEventArgs>? EnabledGameIntegrationCountChanged;
}
