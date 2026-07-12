using Dianty.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Dianty.Views.Controls;

public sealed partial class CoyoteWsDetailCard : UserControl
{
    public CoyoteWsDetailCard(CoyoteWsItem? coyoteWsItem)
    {
        InitializeComponent();

        ViewModel = coyoteWsItem;
    }

    private CoyoteWsItem? ViewModel { get; set; }
}
