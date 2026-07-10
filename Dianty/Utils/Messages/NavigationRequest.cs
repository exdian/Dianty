using Microsoft.UI.Xaml.Navigation;
using System;

namespace Dianty.Utils.Messages;

public class NavigationRequest
{
    public NavigationRequest(int goBackLevel, FrameNavigationOptions navigationOptions)
    {
        GoBackLevel = goBackLevel;
        NavigationOptions = navigationOptions;
    }

    public NavigationRequest(Type pageType, object? parameter, FrameNavigationOptions navigationOptions)
    {
        PageType = pageType;
        Parameter = parameter;
        NavigationOptions = navigationOptions;
    }

    public int GoBackLevel { get; }
    public Type? PageType { get; }
    public object? Parameter { get; }
    public FrameNavigationOptions NavigationOptions { get; }
}
