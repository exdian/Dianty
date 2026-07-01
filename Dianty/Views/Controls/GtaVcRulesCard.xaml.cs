using Dianty.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Dianty.Views.Controls;

public sealed partial class GtaVcRulesCard : UserControl
{
    public GtaVcRulesCard(GamesPageViewModel? viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
    }

    private GamesPageViewModel? ViewModel { get; }
}
