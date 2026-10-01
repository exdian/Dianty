using Dianty.ViewModels;
using Dianty.Views.Controls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Dianty.Views.Pages;

public sealed partial class WaveSettingsPage : Page
{
    public WaveSettingsPage()
    {
        InitializeComponent();
    }

    private PathBar.RequiredParameter? PathBarParameter { get; set; }
    private WaveItem? ViewModel { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is RequiredParameter parameter)
        {
            PathBarParameter = parameter.PathBarParameter;
            ViewModel = parameter.ViewModel;
        }
    }

    public record class RequiredParameter(PathBar.RequiredParameter PathBarParameter, WaveItem ViewModel);
}
