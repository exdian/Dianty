using Dianty.ViewModels;
using Dianty.Views.Controls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Dianty.Views.Pages;

public sealed partial class WavePlayQueuePage : Page
{
    public WavePlayQueuePage()
    {
        InitializeComponent();
    }

    private WavesPageViewModel? ViewModel { get; set; }
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

    public record class RequiredParameter(WavesPageViewModel ViewModel, PathBar.RequiredParameter PathBarParameter);
}
