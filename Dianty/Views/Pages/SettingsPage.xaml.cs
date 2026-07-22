using CommunityToolkit.WinUI.Controls;
using Dianty.Utils;
using Dianty.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Dianty.Views.Pages;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private SettingsPageViewModel? ViewModel { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is SettingsPageViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }

    private void OnSettingsCardLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is SettingsCard settingsCard)
            SettingsCardHelper.StretchContent(settingsCard);
    }

    private void OnTreeViewItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is TreeViewNode node && node.HasChildren)
            node.IsExpanded = !node.IsExpanded;
    }
}
