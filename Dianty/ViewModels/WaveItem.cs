using CommunityToolkit.Mvvm.ComponentModel;
using DungeonToolkit.Coyote;

namespace Dianty.ViewModels;

public partial class WaveItem(Wave wave) : ObservableObject
{
    public Wave Wave { get; } = wave;

    [ObservableProperty]
    public partial string Name { get; set; } = wave.Name;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool? IsEnabledA { get; set; } = false;

    [ObservableProperty]
    public partial bool? IsEnabledB { get; set; } = false;

    partial void OnNameChanged(string value)
    {
        Wave.Name = value;
    }
}

public partial class WavePlayingItem(WaveItem waveItem) : ObservableObject
{
    public WaveItem WaveItem { get; } = waveItem;

    [ObservableProperty]
    public partial int Speed { get; set; } = waveItem.Wave.Speed;
}
