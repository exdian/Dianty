using CommunityToolkit.WinUI.Controls;
using Dianty.Utils;
using Dianty.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dianty.Views.Controls;

public sealed partial class CoyoteBleDetailCards : UserControl
{
    public CoyoteBleDetailCards(CoyoteBleItem coyoteBleItem)
    {
        InitializeComponent();

        ViewModel = coyoteBleItem;
    }

    private CoyoteBleItem ViewModel { get; }

    private void OnSettingsCardLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is SettingsCard settingsCard)
            SettingsCardHelper.StretchContent(settingsCard);
    }
}
