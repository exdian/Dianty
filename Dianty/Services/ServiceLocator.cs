using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Dianty.Services;

public class ServiceLocator
{
    private readonly Dictionary<Type, object> _services = [];
    private Action? _register;

    public static ServiceLocator? Instance { get; private set; }

    public static void Init(Action? register)
    {
        var instance = new ServiceLocator { _register = register };
        Instance = instance;
    }

    public void RegisterDefault()
    {
        if (_register is null)
            return;

        var register = _register;
        _register = null;
        register.Invoke();
    }

    public void Register<T>(T service)
    {
        Debug.Assert(service is not null);
        _services[typeof(T)] = service;
    }

    public void Register<T>(Func<T> getService)
    {
        Debug.Assert(getService is not null);
        _services[typeof(T)] = getService;
    }

    public T GetService<T>()
    {
        var service = _services[typeof(T)];
        if (service is Func<T> func)
            return func();
        else
            return (T)service;
    }

    public void RegisterViewModel(Type pageType, object viewModel)
    {
        Debug.Assert(pageType is not null);
        Debug.Assert(viewModel is not null);
        _services[pageType] = viewModel;
    }

    public object? GetViewModel(Type pageType)
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
