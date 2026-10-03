using CommunityToolkit.WinUI.Controls;
using Dianty.Utils;
using Dianty.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;

namespace Dianty.Views.Controls;

public sealed partial class CoyoteBleDetailCards : UserControl
{
    public CoyoteBleDetailCards(CoyoteBleItem coyoteBleItem, ICommand deleteCommand)
    {
        InitializeComponent();

        ViewModel = coyoteBleItem;
        DeleteCommand = deleteCommand;
    }

    private CoyoteBleItem ViewModel { get; }
    public ICommand DeleteCommand { get; }

    private void OnSettingsCardLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is SettingsCard settingsCard)
            SettingsCardHelper.StretchContent(settingsCard);
    }
}
