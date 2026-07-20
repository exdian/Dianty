using Dianty.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Dianty.Views.Pages;

public sealed partial class SafetyPage : Page
{
    public SafetyPage()
    {
        InitializeComponent();
    }

    private SafetyPageViewModel? ViewModel { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is SafetyPageViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }
}
