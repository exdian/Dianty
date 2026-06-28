using Dianty.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Dianty.Views.Controls;

public sealed partial class GtaVcRulesCard : UserControl
{
    public GtaVcRulesCard(GamesViewModel? viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
    }

    private GamesViewModel? ViewModel { get; }
}
