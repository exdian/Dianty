using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Dianty.ViewModels;

public partial class SafetyPageViewModel(CoyoteCollection coyoteCollection) : ObservableObject
{
    private readonly CoyoteCollection _coyoteCollection = coyoteCollection;

    [RelayCommand]
    private void Disable()
    {
        foreach (var item in _coyoteCollection)
        {
            item.IsEnabled = false;
        }
    }

    [RelayCommand]
    private void Disconnect()
    {
        foreach (var item in _coyoteCollection)
        {
            if (item is CoyoteBleItem coyoteBleItem)
                coyoteBleItem.IsConnectingOrConnected = false;
            else if (item is CoyoteWsItem coyoteWsItem)
                coyoteWsItem.IsConnectingOrConnected = false;

        }
    }
}
