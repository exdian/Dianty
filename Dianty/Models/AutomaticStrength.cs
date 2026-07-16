using System;

namespace Dianty.Models;

public abstract class AutomaticStrength
{
    public Mode StrengthMode { get; set; }

    public int OutputStrength
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                OnOutputStrengthChanged(value);
            }
        }
    }

    public event EventHandler<OutputStrengthChangedEventArgs>? OutputStrengthChanged;

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

    protected virtual void OnOutputStrengthChanged(int strength)
    {
        var args = new OutputStrengthChangedEventArgs(strength);
        OutputStrengthChanged?.Invoke(this, args);
    }

    public class OutputStrengthChangedEventArgs(int strength) : EventArgs
    {
        public int Strength { get; } = strength;
    }

    public enum Mode
    {
        Max, Sum
    }
}
