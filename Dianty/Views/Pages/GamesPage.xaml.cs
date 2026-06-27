using Dianty.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Dianty.Views.Pages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class GamesPage : Page
{
    public GamesPage()
    {
        InitializeComponent();
    }

    private GamesViewModel? ViewModel;

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is GamesViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }
}
