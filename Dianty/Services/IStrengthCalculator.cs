using Dianty.EventArguments;
using System;

namespace Dianty.Services;

public interface IStrengthCalculatorStatus
{
    int OutputStrength { get; }

    event EventHandler<OutputStrengthChangedEventArgs>? OutputStrengthChanged;
}

public interface IStrengthCalculatorConfig
{
    StrengthCalculationMode StrengthMode { get; set; }
}

public enum StrengthCalculationMode
{
    Max, Sum
}
