using Dianty.Services;
using Dianty.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dianty.Views.Controls;

public sealed partial class GamesPageContent : UserControl
{
    public GamesPageContent(GamesPageViewModel viewModel, IQueueService queueService)
    {
        InitializeComponent();
        ViewModel = viewModel;
        _queueService = queueService;
    }

    private readonly IQueueService _queueService;

    private GamesPageViewModel ViewModel { get; }

    private void OnContentStackPanelLoaded(object sender, RoutedEventArgs e)
    {
        _queueService.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            var rulesCard = new GtaVcRuleCard(ViewModel.GtaVcRuleCardViewModel);
            var collection = _contentStackPanel.Children;
            collection.Insert(collection.IndexOf(_emptyRuleCard), rulesCard);
        });
    }
}
