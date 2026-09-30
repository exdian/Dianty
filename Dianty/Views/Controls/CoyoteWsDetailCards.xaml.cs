using Dianty.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Dianty.Views.Controls;

public sealed partial class CoyoteWsDetailCards : UserControl
{
    public CoyoteWsDetailCards(CoyoteWsItem coyoteWsItem)
    {
        InitializeComponent();

        ViewModel = coyoteWsItem;
    }

    private CoyoteWsItem ViewModel { get; }
}
