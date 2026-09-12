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

#pragma warning disable CA1822 // 将成员标记为 static
    // 此处属于 VS 误分析，实际上不能标记为 static
    // 因为事件是在 xaml 里订阅的，自动生成的代码只会通过 this 来访问此方法，不会因为方法被标记为 static 而使用类名来访问
    private void OnTreeViewItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
#pragma warning restore CA1822 // 将成员标记为 static
    {
        if (args.InvokedItem is TreeViewNode node && node.HasChildren)
            node.IsExpanded = !node.IsExpanded;
    }
}
