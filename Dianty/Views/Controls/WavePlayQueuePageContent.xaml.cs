using Dianty.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Dianty.Views.Controls;

public sealed partial class WavePlayQueuePageContent : UserControl
{
    public WavePlayQueuePageContent(WavesPageViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
    }

    private WavesPageViewModel ViewModel { get; }
}
