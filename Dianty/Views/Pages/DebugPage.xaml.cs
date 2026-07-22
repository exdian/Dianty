using CommunityToolkit.Mvvm.Messaging;
using Dianty.Utils.Messages;
using Dianty.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Dianty.Views.Pages;

public sealed partial class DebugPage : Page
{
    public DebugPage()
    {
        InitializeComponent();

        Loaded += DebugPage_Loaded;
        Unloaded += DebugPage_Unloaded;
    }

    private DebugPageViewModel? ViewModel { get; set; }

    public static NavigationViewItem GetNavigationViewItem()
    {
        return new NavigationViewItem
        {
            Icon = new SymbolIcon(Symbol.Repair),
            Content = "调试",
            Tag = typeof(DebugPage)
        };
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is DebugPageViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }

    private void DebugPage_Loaded(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Register<LogAppendedMessage>(this, (_, _) =>
        {
            if (ViewModel is not null && ViewModel.Logs.Count > 0)
            {
                // ListView 滚动到最后一项
                _logListView.ScrollIntoView(ViewModel.Logs[^1]);
            }
        });
    }

    private void DebugPage_Unloaded(object sender, RoutedEventArgs e)
    {
        // 取消注册，避免内存泄漏
        WeakReferenceMessenger.Default.Unregister<LogAppendedMessage>(this);
    }
}
