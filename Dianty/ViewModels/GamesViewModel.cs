using CommunityToolkit.Mvvm.ComponentModel;
using static Dianty.Models.GameManager;

namespace Dianty.ViewModels;

internal partial class GamesViewModel : ObservableObject
{
    public GamesViewModel()
    {
        StrengthModes =
            [new StrengthModeItem { Value = Mode.Max, DisplayName = "取最大值" },
            new StrengthModeItem { Value = Mode.Add, DisplayName = "叠加强度" }];

        CurrentStrengthMode = StrengthModes[0];
    }

    public StrengthModeItem[] StrengthModes { get; }

    [ObservableProperty]
    public partial StrengthModeItem CurrentStrengthMode { get; set; }

    [ObservableProperty]
    public partial int OutputStrength { get; private set; }

    [ObservableProperty]
    public partial int EnableGameCount { get; private set; }

    public class StrengthModeItem
    {
        public Mode Value { get; set; }
        public string? DisplayName { get; set; }
    }
}
