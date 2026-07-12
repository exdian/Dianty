using CommunityToolkit.WinUI.Controls;
using Dianty.Utils;
using Dianty.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dianty.Views.Controls;

public sealed partial class CoyoteBleDetailCard : UserControl
{
    public CoyoteBleDetailCard(CoyoteBleItem? coyoteBleItem)
    {
        InitializeComponent();

        ViewModel = coyoteBleItem;
    }

    private CoyoteBleItem? ViewModel { get; set; }

    private void OnSettingsCardLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is SettingsCard settingsCard)
            SettingsCardHelper.StretchContent(settingsCard);
    }
}
