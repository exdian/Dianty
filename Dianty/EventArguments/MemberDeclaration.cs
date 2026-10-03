using System;
using Windows.System;

namespace Dianty.EventArguments;

public sealed class LetterKeyDownWithoutTextInputEventArgs(VirtualKey key) : EventArgs
{
    public VirtualKey Key { get; } = key;
}

public sealed class EnableStatusChangedEventArgs(bool isEnabled) : EventArgs
{
    public bool IsEnabled { get; } = isEnabled;
}

public sealed class EnabledGameIntegrationCountChangedEventArgs(int count) : EventArgs
{
    public int Count { get; } = count;
}

public sealed class OutputStrengthChangedEventArgs(int strength) : EventArgs
{
    public int Strength { get; } = strength;
}

public sealed class CurrentLanguageChangedEventArgs(string currentLanguageFileName) : EventArgs
{
    public string CurrentLanguageFileName { get; } = currentLanguageFileName;
}

public sealed class RegisteredNameChangedEventArgs<T>(T key, string name) : EventArgs
{
    public T Key { get; } = key;
    public string Name { get; } = name;
}
