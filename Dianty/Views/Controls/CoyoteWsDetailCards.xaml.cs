using Dianty.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;

namespace Dianty.Views.Controls;

public sealed partial class CoyoteWsDetailCards : UserControl
{
    public CoyoteWsDetailCards(CoyoteWsItem coyoteWsItem, ICommand deleteCommand)
    {
        InitializeComponent();

        ViewModel = coyoteWsItem;
        DeleteCommand = deleteCommand;
    }

    private CoyoteWsItem ViewModel { get; }
    public ICommand DeleteCommand { get; }
}
