using CommunityToolkit.Mvvm.ComponentModel;
using static Dianty.Models.GameManager;

namespace Dianty.ViewModels;

public partial class GamesViewModel : ObservableObject
{
    public GamesViewModel()
    {
        StrengthModes =
            [new StrengthModeItem { Value = Mode.Max, DisplayName = "取最大值" },
            new StrengthModeItem { Value = Mode.Add, DisplayName = "叠加强度" }];

        CurrentGamesStrengthMode = StrengthModes[0];
        CurrentGtaVcStrengthMode = StrengthModes[0];
    }

    public StrengthModeItem[] StrengthModes { get; }

    [ObservableProperty]
    public partial StrengthModeItem CurrentGamesStrengthMode { get; set; }

    [ObservableProperty]
    public partial int OutputStrength { get; private set; }

    [ObservableProperty]
    public partial int EnableGameCount { get; private set; }

    [ObservableProperty]
    public partial StrengthModeItem CurrentGtaVcStrengthMode { get; set; }

    public readonly record struct StrengthModeItem(Mode Value, string DisplayName);
}
