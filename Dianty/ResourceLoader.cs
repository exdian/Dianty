using System;

namespace Dianty;

internal static class ResourceLoader
{
    private static Action? _load;

    public static void Init(Action? load)
    {
        _load = load;
    }

    public static void Load()
    {
        if (_load is null)
            return;

        var load = _load;
        _load = null;
        load.Invoke();
    }
}
