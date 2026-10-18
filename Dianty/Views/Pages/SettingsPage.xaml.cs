using CommunityToolkit.WinUI.Controls;
using Dianty.Utils;
using Dianty.ViewModels;
using Dianty.Views.Controls;
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

    private void OnAppInfoExpanderLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not SettingsExpander settingsExpander)
            return;
        if (VisualTreeHelperExtension.FindChildByName(settingsExpander, "PART_HeaderIconPresenterHolder") is not Viewbox viewbox)
            return;
        if (VisualTreeHelperExtension.FindChildByName(viewbox, "PART_HeaderIconPresenter") is not ContentPresenter contentPresenter)
            return;

        contentPresenter.Content = new AppIcon();
        viewbox.Visibility = Visibility.Visible;
    }
}
