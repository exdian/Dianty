using System;

namespace Dianty.Models;

public abstract class AutomaticStrength
{
    public Mode StrengthMode { get; set; }

    public event EventHandler? OutputStrengthChanged;

    public int ComputeOutputStrength()
    {
        if (StrengthMode == Mode.Max)
        {
            return GetMaxStrength();
        }
        else
        {
            return SumStrength();
        }
    }

    protected abstract int GetMaxStrength();

    protected abstract int SumStrength();

    protected void OnOutputStrengthChanged()
    {
        OutputStrengthChanged?.Invoke(this, EventArgs.Empty);
    }

    public enum Mode
    {
        Max, Sum
    }
}
