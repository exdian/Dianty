using Dianty.Services;
using Microsoft.UI.Xaml;
using System;

namespace Dianty.Utils;

public static class PageHelper
{
    private static Func<string, Type?>? _topPageLocator;
    private static Func<string, Func<ILocalizationService, string>?>? _pagePageHeaderGetterFactory;
    private static Func<string, FrameworkElement?>? _pageContentFactory;

    public static void Init(
        Func<string, Type?>? topPageLocator,
        Func<string, Func<ILocalizationService, string>?>? pagePageHeaderGetterFactory,
        Func<string, FrameworkElement?>? pageContentFactory)
    {
        _topPageLocator = topPageLocator;
        _pagePageHeaderGetterFactory = pagePageHeaderGetterFactory;
        _pageContentFactory = pageContentFactory;
    }

    public static Type? GetTopPageType(string pageName)
    {
        return _topPageLocator?.Invoke(pageName);
    }

    public static Func<ILocalizationService, string> GetPagePageHeaderGetter(string contentName)
    {
        return _pagePageHeaderGetterFactory?.Invoke(contentName) ?? (s => "null");
    }

    public static FrameworkElement? GetPageContent(string contentName)
    {
        return _pageContentFactory?.Invoke(contentName);
    }
}
