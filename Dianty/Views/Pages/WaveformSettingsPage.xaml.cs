using Dianty.ViewModels;
using Dianty.Views.Controls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Dianty.Views.Pages;

public sealed partial class WaveformSettingsPage : Page
{
    public WaveformSettingsPage()
    {
        InitializeComponent();
    }

    private PathBar.RequiredParameter? PathBarParameter { get; set; }
    private WaveformItemBase? WaveformItem { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is RequiredParameter parameter)
        {
            PathBarParameter = parameter.PathBarParameter;
            WaveformItem = parameter.WaveformItem;
        }
    }

    public sealed record class RequiredParameter(
        PathBar.RequiredParameter PathBarParameter, WaveformItemBase WaveformItem);
}
