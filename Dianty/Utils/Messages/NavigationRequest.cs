using Microsoft.UI.Xaml.Navigation;
using System;

namespace Dianty.Utils.Messages;

public class NavigationRequest
{
    public NavigationRequest(bool isBackward)
    {
        IsBackward = isBackward;
    }

    public NavigationRequest(Type pageType, object? parameter, FrameNavigationOptions navigationOptions)
    {
        PageType = pageType;
        Parameter = parameter;
        NavigationOptions = navigationOptions;
    }

    public bool IsBackward { get; }
    public Type? PageType { get; }
    public object? Parameter { get; }
    public FrameNavigationOptions? NavigationOptions { get; }
}
