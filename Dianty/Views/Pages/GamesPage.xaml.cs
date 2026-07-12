using Dianty.Services;
using Dianty.ViewModels;
using Dianty.Views.Controls;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Dianty.Views.Pages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class GamesPage : Page
{
    public GamesPage()
    {
        InitializeComponent();
    }

    private IQueueService? _queueService;

    private GamesPageViewModel? ViewModel { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is RequiredParameter requiredParameter)
        {
            ViewModel = requiredParameter.ViewModel;
            _queueService = requiredParameter.QueueService;
        }
    }

    private void StackPanel_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // 实测该元素更晚触发 Loaded 事件
        if (_queueService is null)
            return;

        _queueService.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            var rulesCard = new GtaVcRuleCard(ViewModel);
            var collection = _gamesPageStackPanel.Children;
            collection.Insert(collection.Count - 1, rulesCard);
        });
    }

    public record class RequiredParameter(GamesPageViewModel ViewModel, IQueueService QueueService);
}
