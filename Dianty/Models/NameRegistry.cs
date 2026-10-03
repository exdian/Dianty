using Dianty.EventArguments;
using Dianty.Services;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Dianty.Models;

public sealed class NameRegistry<T> : INameRegistry<T> where T : notnull
{
    private readonly Dictionary<T, string> _registry = [];
    private readonly Lock _lock = new();
    private Dictionary<T, string> _snapshot = [];

    public event EventHandler<RegisteredNameChangedEventArgs<T>>? RegisteredNameChanged;

    public string GetName(T key)
    {
        if (_snapshot.TryGetValue(key, out var name))
            return name;

        return string.Empty;
    }

    public void Rename(T key, string name)
    {
        lock (_lock)
        {
            if (name is null || name.Length == 0)
                _registry.Remove(key);
            else
                _registry[key] = name;

            _snapshot = new Dictionary<T, string>(_registry);
        }

        OnRegisteredNameChanged(key, name ?? string.Empty);
    }

    private void OnRegisteredNameChanged(T key, string name)
    {
        var args = new RegisteredNameChangedEventArgs<T>(key, name);
        RegisteredNameChanged?.Invoke(this, args);
    }
}
