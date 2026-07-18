using System;

namespace Dianty.Views;

internal static class TopPageLocator
{
    private static Func<string, Type?>? _match;

    public static void Init(Func<string, Type?>? match)
    {
        _match = match;
    }

    public static Type? GetTopPage(string pageName)
    {
        return _match?.Invoke(pageName);
    }
}
