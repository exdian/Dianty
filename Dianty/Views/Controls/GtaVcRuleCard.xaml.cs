using Dianty.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Dianty.Views.Controls;

public sealed partial class GtaVcRuleCard : UserControl
{
    public GtaVcRuleCard(GamesPageViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
    }

    private GamesPageViewModel ViewModel { get; }
}
