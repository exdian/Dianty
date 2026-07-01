using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Dianty.Services;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = [];
    private static Action? _register;

    public static void Init(Action? register)
    {
        _services.Clear();
        _register = register;
    }

    public static void RegisterDefault()
    {
        if (_register is null)
            return;

        var register = _register;
        _register = null;
        register.Invoke();
    }

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

    public static void RegisterViewModel(Type pageType, object viewModel)
    {
        Debug.Assert(pageType is not null);
        Debug.Assert(viewModel is not null);
        _services[pageType] = viewModel;
    }

    public static object? GetViewModel(Type pageType)
    {
        Debug.Assert(pageType is not null);
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
