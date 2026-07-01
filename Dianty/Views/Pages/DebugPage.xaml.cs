using CommunityToolkit.Mvvm.Messaging;
using Dianty.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Dianty.Views.Pages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class DebugPage : Page
{
    public DebugPage()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<LogAppendedMessage>(this, (_, _) =>
        {
            if (ViewModel is not null && ViewModel.Logs.Count > 0)
            {
                // ListView 滚动到最后一项
                _logListView.ScrollIntoView(ViewModel.Logs[^1]);
            }
        });

        Loaded += (s, e) =>
        {
            ViewModel?.AppendLog("调试页面已加载");
        };

        Unloaded += (s, e) =>
        {
            // 取消注册，避免内存泄漏
            WeakReferenceMessenger.Default.Unregister<LogAppendedMessage>(this);
        };
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
}
