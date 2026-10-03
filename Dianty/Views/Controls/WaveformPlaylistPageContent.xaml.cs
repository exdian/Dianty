using Dianty.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Dianty.Views.Controls;

public sealed partial class WaveformPlaylistPageContent : UserControl
{
    public WaveformPlaylistPageContent(WaveformsPageViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
    }

    private WaveformsPageViewModel ViewModel { get; }
}
