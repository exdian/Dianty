using Dianty.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Dianty.Views.Controls;

public sealed partial class GtaVcRuleCards : UserControl
{
    public GtaVcRuleCards(GtaVcRuleCardViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
    }

    private GtaVcRuleCardViewModel? ViewModel { get; }
}
