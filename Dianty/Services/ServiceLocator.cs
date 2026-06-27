using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Dianty.Services;

internal class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = [];

    public static void Register<T>(T service)
    {
        Debug.Assert(service is not null);
        _services[typeof(T)] = service;
    }

    public static void Register<T>(Func<T> getService)
    {
        Debug.Assert(getService is not null);
        _services[typeof(T)] = getService;
    }

    public static T GetService<T>()
    {
        var service = _services[typeof(T)];
        if (service is Func<T> func)
            return func();
        else
            return (T)service;
    }

    public static void RegisterViewModels()
    {

    }

    public static object? GetViewModel(Type pageType)
    {
        if (_services.TryGetValue(pageType, out var viewModel))
        {
            if (viewModel is Func<object> func)
                return func();
            else
                return viewModel;
        }
        else
        {
            return null;
        }
    }
}
