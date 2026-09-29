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

    private WaveItem? ViewModel { get; set; }
    private PathBar.RequiredParameter? PathBarParameter { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is RequiredParameter parameter)
        {
            ViewModel = parameter.ViewModel;
            PathBarParameter = parameter.PathBarParameter;
        }
    }

    public record class RequiredParameter(WaveItem ViewModel, PathBar.RequiredParameter PathBarParameter);
}
