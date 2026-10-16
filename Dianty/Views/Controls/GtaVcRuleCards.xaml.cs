using Dianty.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Dianty.Views.Controls;

public sealed partial class GtaVcRuleCards : UserControl
{
    public GtaVcRuleCards(GtaVcGameIntegrationViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
    }

    private GtaVcGameIntegrationViewModel? ViewModel { get; }
}
