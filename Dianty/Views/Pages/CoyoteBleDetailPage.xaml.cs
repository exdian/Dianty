using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Controls;
using Dianty.Services;
using Dianty.Utils;
using Dianty.Utils.Messages;
using Dianty.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;

namespace Dianty.Views.Pages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class CoyoteBleDetailPage : Page
{
    public CoyoteBleDetailPage()
    {
        InitializeComponent();
    }

    private IQueueService? _queueService;

    private CoyoteBleItem? ViewModel { get; set; }

    private ObservableCollection<string>? Traces { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is RequiredParameter parameter)
        {
            ViewModel = parameter.ViewModel;
            Traces = new ObservableCollection<string>(parameter.Traces)
            {
                "详细信息"
            };
            _queueService = parameter.QueueService;
        }
    }

    private void OnBreadcrumbBarItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (Traces is not null)
        {
            for (int i = Traces.Count - 1; i > args.Index; i--)
            {
                Traces.RemoveAt(i);
            }
        }

        // 当文本框修改时点击导航，文本框绑定的属性不能及时更新，因此需要将导航放到低优先级队列
        if (_queueService is null)
        {
            WeakReferenceMessenger.Default.Send(new NavigationRequest(isBackward: true));
        }
        else
        {
            _queueService.TryEnqueue(DispatcherQueuePriority.Low,
                () => WeakReferenceMessenger.Default.Send(new NavigationRequest(isBackward: true)));
        }
    }

    private void OnSettingsCardLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is SettingsCard settingsCard)
            AdjustSettingsCardLayout(settingsCard);
    }

    private static void AdjustSettingsCardLayout(SettingsCard settingsCard)
    {
        // 使内容拉伸铺满
        if (VisualTreeHelperExtension.FindChildByName(settingsCard, "PART_RootGrid") is not Grid grid)
            return;
        if (VisualTreeHelperExtension.FindChildByName(grid, "PART_ContentPresenter") is not ContentPresenter contentPresenter)
            return;
        settingsCard.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        grid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Auto);
        grid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
        contentPresenter.HorizontalAlignment = HorizontalAlignment.Stretch;
        Grid.SetColumnSpan(contentPresenter, 2);
        var visualStateGroups = VisualStateManager.GetVisualStateGroups(grid);
        foreach (var visualStateGroup in visualStateGroups)
        {
            if (visualStateGroup.Name == "ContentAlignmentStates")
            {
                foreach (var state in visualStateGroup.States)
                {
                    if (state.Name == "RightWrapped")
                    {
                        state.Setters.RemoveAt(3);
                    }
                    else if (state.Name == "RightWrappedNoIcon")
                    {
                        state.Setters.RemoveAt(4);
                    }
                }
                break;
            }
        }
    }

    public record class RequiredParameter(CoyoteBleItem ViewModel, string[] Traces, IQueueService? QueueService);
}
