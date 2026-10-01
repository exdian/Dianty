using Dianty.Services;
using Dianty.ViewModels;
using Dianty.Views.Controls;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Dianty.Views.Pages;

public sealed partial class GamesPage : Page
{
    public GamesPage()
    {
        InitializeComponent();
    }

    private RequiredParameter? _parameter;

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _parameter = e.Parameter as RequiredParameter;
    }

    private void OnRootGridLoaded(object sender, RoutedEventArgs e)
    {
        _parameter?.QueueService.TryEnqueue(DispatcherQueuePriority.Low, LoadContent);
    }

    private void LoadContent()
    {
        var content = new GamesPageContent(_parameter!);
        Grid.SetRow(content, 1);
        _rootGrid.Children.Add(content);
    }

    public record class RequiredParameter(
        GamesPageViewModel GamesPageViewModel, GtaVcRuleCardViewModel GtaVcRuleCardViewModel,
        IQueueService QueueService, ILocalizationService LocalizationService);
}
