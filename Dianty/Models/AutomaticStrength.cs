using System;

namespace Dianty.Models;

public abstract class AutomaticStrength
{
    public Mode StrengthMode { get; set; }

    public int OutputStrength
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnOutputStrengthChanged();
            }
        }
    }

    public event EventHandler<OutputStrengthChangedEventArgs>? OutputStrengthChanged;

    protected void OnOutputStrengthChanged()
    {
        var args = new OutputStrengthChangedEventArgs(OutputStrength);
        OutputStrengthChanged?.Invoke(this, args);
    }

    protected void ComputeOutputStrength()
    {
        if (StrengthMode == Mode.Max)
        {
            OutputStrength = GetMaxStrength();
        }
        else
        {
            OutputStrength = SumStrength();
        }
    }

    protected abstract int GetMaxStrength();

    protected abstract int SumStrength();

    public enum Mode
    {
        Max, Sum
    }

    public class OutputStrengthChangedEventArgs(int strength) : EventArgs
    {
        public int Strength { get; } = strength;
    }
}
